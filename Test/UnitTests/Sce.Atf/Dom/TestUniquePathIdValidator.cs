//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestUniquePathIdValidator : DomTest
    {
        public static class ItemType
        {
            static ItemType()
            {
                Type.Define(NameAttribute);
                Type.SetIdAttribute(NameAttribute);
            }

            public readonly static DomNodeType Type = new DomNodeType("itemType");

            public readonly static AttributeInfo NameAttribute =
                new AttributeInfo("name", new AttributeType(AttributeTypes.String.ToString(), typeof(string)));
        }

        public static class FolderType
        {
            static FolderType()
            {
                Type.Define(NameAttribute);
                Type.Define(ItemChild);
                Type.SetIdAttribute(NameAttribute);
                ItemChild.AddRule(new ChildCountRule(0, int.MaxValue));
            }

            public readonly static DomNodeType Type = new DomNodeType("folderType");

            public readonly static AttributeInfo NameAttribute =
                new AttributeInfo("name", new AttributeType(AttributeTypes.String.ToString(), typeof(string)));

            public readonly static ChildInfo ItemChild = new ChildInfo("item", ItemType.Type, true);
        }

        public static class RootType
        {
            static RootType()
            {
                Type.Define(NameAttribute);
                Type.Define(FolderChild);
                Type.SetIdAttribute(NameAttribute);
                FolderChild.AddRule(new ChildCountRule(0, int.MaxValue));
            }

            public readonly static DomNodeType Type = new DomNodeType("rootType");

            public readonly static AttributeInfo NameAttribute =
                new AttributeInfo("name", new AttributeType(AttributeTypes.String.ToString(), typeof(string)));

            public readonly static ChildInfo FolderChild = new ChildInfo("folder", FolderType.Type, true);
        }

        public readonly static ChildInfo RootElement = new ChildInfo("root", null);

        public TestUniquePathIdValidator()
        {
            RootType.Type.Define(new ExtensionInfo<ValidationContext>());
            RootType.Type.Define(new ExtensionInfo<UniquePathIdValidator>());
        }

        void ValidateSubtree(DomNode folder)
        {
            var uniqueNamer = new UniqueNamer();
            foreach (DomNode node in folder.Subtree)
            {
                foreach (DomNode child in node.Children)
                {
                    if (child.Type.IdAttribute != null)
                    {
                        string id = child.GetId();
                        string uniqueId = uniqueNamer.Name(id);
                        if (id != uniqueId)
                            throw new InvalidTransactionException("id collision");
                    }
                }
                uniqueNamer.Clear();
            }
        }

        [Test]
        public void TestMoveDomNode()
        {
            var root = new DomNode(RootType.Type, RootElement);

            root.InitializeExtensions();

            var folderChild1 = new DomNode(FolderType.Type);
            var folderChild2 = new DomNode(FolderType.Type);
            var itemChild1 = new DomNode(ItemType.Type);
            var itemChild2 = new DomNode(ItemType.Type);

            var validationContext = root.As<ValidationContext>();
            
            // Set up the tree:
            // root
            //     folder
            //         item
            //     folder1
            //         item

            validationContext.RaiseBeginning();

            root.SetAttribute(RootType.NameAttribute, "root");
            itemChild1.SetAttribute(ItemType.NameAttribute, "item");
            itemChild2.SetAttribute(ItemType.NameAttribute, "item");
            folderChild1.SetAttribute(FolderType.NameAttribute, "folder");
            folderChild2.SetAttribute(FolderType.NameAttribute, "folder");

            folderChild1.GetChildList(FolderType.ItemChild).Add(itemChild1);
            folderChild2.GetChildList(FolderType.ItemChild).Add(itemChild2);

            root.GetChildList(RootType.FolderChild).Add(folderChild1);
            root.GetChildList(RootType.FolderChild).Add(folderChild2);

            // renames all folders and items with unique paths
            validationContext.RaiseEnding();
            validationContext.RaiseEnded(); 

            // Move item from first folder to second folder
            // root
            //     folder
            //     folder1
            //         item
            //         item1

            validationContext.RaiseBeginning();

            itemChild1.RemoveFromParent();
            folderChild2.GetChildList(FolderType.ItemChild).Add(itemChild1);

            validationContext.RaiseEnding();
            validationContext.RaiseEnded();
            Assert.DoesNotThrow(() => ValidateSubtree(folderChild2));
            // Make sure that the existing child wasn't renamed. Only the moved child should be renamed.
            Assert.True((string)itemChild2.GetAttribute(ItemType.NameAttribute) == "item");

            // Rename 'item_1' to 'item'.
            validationContext.RaiseBeginning();

            itemChild1.SetAttribute(ItemType.NameAttribute, "item");

            validationContext.RaiseEnding();
            validationContext.RaiseEnded();
            Assert.DoesNotThrow(() => ValidateSubtree(folderChild2));

            // Make sure that the existing child wasn't renamed. Only the moved child should be renamed.
            Assert.True((string)itemChild2.GetAttribute(ItemType.NameAttribute) == "item");

            // Rename the root.
            validationContext.RaiseBeginning();

            root.SetAttribute(RootType.NameAttribute, "new_root");

            validationContext.RaiseEnding();
            validationContext.RaiseEnded();
            Assert.DoesNotThrow(() => ValidateSubtree(root));
            Assert.True((string)root.GetAttribute(RootType.NameAttribute) == "new_root");
        }
    }
}