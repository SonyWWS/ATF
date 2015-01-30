//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestDomNode : DomTest
    {
        [Test]
        public void TestConstructor()
        {
            DomNodeType type = new DomNodeType("child");
            ChildInfo info = new ChildInfo("test", type);
            DomNode test = new DomNode(type, info);
            Assert.AreSame(test.Type, type);
            Assert.AreSame(test.ChildInfo, info);
        }

        [Test]
        public void TestRootParent()
        {
            DomNodeType type = new DomNodeType("type");
            DomNode test = new DomNode(type);
            Assert.Null(test.Parent);
        }

        [Test]
        public void TestChildParent()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);

            DomNode child = new DomNode(type);
            DomNode parent = new DomNode(type);
            parent.GetChildList(childInfo).Add(child);
            Assert.AreSame(child.Parent, parent);
        }

        [Test]
        public void TestRootGetRoot()
        {
            DomNodeType type = new DomNodeType("type");
            DomNode child = new DomNode(type);
            Assert.AreSame(child.GetRoot(), child);
        }

        [Test]
        public void TestDescendantGetRoot()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);

            DomNode child = new DomNode(type);
            DomNode parent = new DomNode(type);
            DomNode grandparent = new DomNode(type);
            parent.GetChildList(childInfo).Add(child);
            grandparent.GetChildList(childInfo).Add(parent);
            Assert.AreSame(child.GetRoot(), grandparent);
        }

        [Test]
        public void TestGetPath()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);

            DomNode child = new DomNode(type);
            DomNode parent = new DomNode(type);
            DomNode grandparent = new DomNode(type);
            parent.GetChildList(childInfo).Add(child);
            grandparent.GetChildList(childInfo).Add(parent);

            Utilities.TestSequenceEqual(child.GetPath(), grandparent, parent, child);
            Utilities.TestSequenceEqual(parent.GetPath(), grandparent, parent);
            Utilities.TestSequenceEqual(grandparent.GetPath(), grandparent);
        }

        [Test]
        public void TestRootLineage()
        {
            DomNodeType type = new DomNodeType("type");
            DomNode test = new DomNode(type);
            Utilities.TestSequenceEqual(test.Lineage, test);
        }

        [Test]
        public void TestDescendantLineage()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);

            DomNode child = new DomNode(type);
            DomNode parent = new DomNode(type);
            DomNode grandparent = new DomNode(type);
            parent.GetChildList(childInfo).Add(child);
            grandparent.GetChildList(childInfo).Add(parent);
            Utilities.TestSequenceEqual(child.Lineage, child, parent, grandparent);
        }

        [Test]
        public void TestGetRoots()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type);
            type.Define(childInfo);

            DomNode child = new DomNode(type);
            DomNode parent = new DomNode(type);
            DomNode grandparent = new DomNode(type);
            parent.SetChild(childInfo, child);
            grandparent.SetChild(childInfo, parent);
            DomNode child2 = new DomNode(type);
            Utilities.TestSequenceEqual(DomNode.GetRoots(new DomNode[] { grandparent, child, child2 }), grandparent, child2);
        }

        [Test]
        public void TestGetLowestCommonAncestor()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);

            DomNode parent = new DomNode(type);
            DomNode child1 = new DomNode(type);
            DomNode child2 = new DomNode(type);
            parent.GetChildList(childInfo).Add(child1);
            parent.GetChildList(childInfo).Add(child2);
            DomNode grandchild1 = new DomNode(type);
            child1.GetChildList(childInfo).Add(grandchild1);
            DomNode grandchild2 = new DomNode(type);
            child2.GetChildList(childInfo).Add(grandchild2);

            Assert.AreSame(DomNode.GetLowestCommonAncestor(child1, child2), parent);
            Assert.AreSame(DomNode.GetLowestCommonAncestor(grandchild1, grandchild2), parent);
            Assert.AreSame(DomNode.GetLowestCommonAncestor(child1, grandchild1), child1);

            Assert.AreSame(DomNode.GetLowestCommonAncestor(new DomNode[] { child1, child2, grandchild1 }), parent);
        }

        [Test]
        public void TestLeafChildren()
        {
            DomNodeType noChildren = new DomNodeType("child");
            DomNode test = new DomNode(noChildren);
            CollectionAssert.IsEmpty(test.Children);
        }

        [Test]
        public void TestParentChildren()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo1 = new ChildInfo("child1", type);
            type.Define(childInfo1);
            ChildInfo childInfo2 = new ChildInfo("child2", type, true);
            type.Define(childInfo2);

            DomNode child1 = new DomNode(type);
            DomNode child2 = new DomNode(type);
            DomNode parent = new DomNode(type);
            parent.SetChild(childInfo1, child1);
            parent.GetChildList(childInfo2).Add(child2);
            Utilities.TestSequenceEqual(parent.Children, child1, child2);
        }

        [Test]
        public void TestLeafSubtree()
        {
            DomNodeType type = new DomNodeType("type");
            DomNode test = new DomNode(type);
            Utilities.TestSequenceEqual(test.Subtree, test);
        }

        [Test]
        public void TestAncestorSubtree()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);

            DomNode grandparent = new DomNode(type);
            DomNode child1 = new DomNode(type);
            DomNode child2 = new DomNode(type);
            grandparent.GetChildList(childInfo).Add(child1);
            grandparent.GetChildList(childInfo).Add(child2);
            DomNode grandchild1 = new DomNode(type);
            DomNode grandchild2 = new DomNode(type);
            child1.GetChildList(childInfo).Add(grandchild1);
            child1.GetChildList(childInfo).Add(grandchild2);
            Utilities.TestSequenceEqual(grandparent.Subtree, grandparent, child1, grandchild1, grandchild2, child2);
        }

        [Test]
        public void TestFreezing()
        {
            DomNodeType type = new DomNodeType("child");
            DomNode test = new DomNode(type);

            Assert.Throws<InvalidOperationException>(delegate { type.Define(GetStringAttribute("foo")); });
            Assert.Throws<InvalidOperationException>(delegate { type.Define(new ChildInfo("foo", type)); });
            Assert.Throws<InvalidOperationException>(delegate { type.Define(new ExtensionInfo<TestDomNode>()); });
        }

        [Test]
        public void TestExtensionsAfterConstructing()
        {
            DomNodeType type = new DomNodeType("test");
            ExtensionInfo info1 = new ExtensionInfo<TestDomNode>();
            type.Define(info1);
            ExtensionInfo info2 = new ExtensionInfo<int>();
            type.Define(info2);

            DomNode test = new DomNode(type);
            Assert.True(test.GetExtension(info1) as TestDomNode != null);
            Assert.True(test.GetExtension(info2) is int);
        }

        private class SimpleAdapter : DomNodeAdapter
        {
        }

        private class DerivedAdapter : SimpleAdapter
        {
        }

        private class NonDomNodeAdapter : Adapter
        {
        }

        [Test]
        public void TestGetDomNodeAdapter()
        {
            DomNodeType type = new DomNodeType("type");
            type.Define(new ExtensionInfo<SimpleAdapter>());
            DomNode node = new DomNode(type);

            SimpleAdapter test = node.GetAdapter(typeof(SimpleAdapter)) as SimpleAdapter;
            Assert.NotNull(test);
        }

        [Test]
        public void TestGetNonDomNodeAdapter()
        {
            DomNodeType type = new DomNodeType("type");
            type.Define(new ExtensionInfo<NonDomNodeAdapter>());
            DomNode node = new DomNode(type);

            NonDomNodeAdapter test = node.GetAdapter(typeof(NonDomNodeAdapter)) as NonDomNodeAdapter;
            Assert.NotNull(test);
        }

        [Test]
        public void TestGetAdapterAsDomNode()
        {
            DomNodeType type = new DomNodeType("type");
            DomNode node = new DomNode(type);

            Assert.AreSame(node.GetAdapter(typeof(DomNode)), node);
        }

        [Test]
        public void TestGetAdapterInherited()
        {
            DomNodeType baseType = new DomNodeType("baseType");
            baseType.Define(new ExtensionInfo<DerivedAdapter>()); // derived adapter on base type!
            DomNodeType derivedType = new DomNodeType("derivedType");
            derivedType.BaseType = baseType;
            derivedType.Define(new ExtensionInfo<SimpleAdapter>());

            DomNode node = new DomNode(derivedType);
            object adapter = node.GetAdapter(typeof(SimpleAdapter));
            Assert.True(
                (adapter is SimpleAdapter) &&
                !(adapter is DerivedAdapter));

            adapter = node.GetAdapter(typeof(DerivedAdapter));
            Assert.True(adapter is DerivedAdapter);
        }

        [Test]
        public void TestGetDecorators()
        {
            DomNodeType type = new DomNodeType("type");
            type.Define(new ExtensionInfo<SimpleAdapter>());
            type.Define(new ExtensionInfo<DerivedAdapter>());
            DomNode node = new DomNode(type);

            object simple = node.GetAdapter(typeof(SimpleAdapter));
            object derived = node.GetAdapter(typeof(DerivedAdapter));

            // test that they're returned in order of definition on node
            Utilities.TestSequenceEqual(node.GetDecorators(typeof(SimpleAdapter)), simple, derived);
        }

        [Test]
        public void TestGetDecoratorsInherited()
        {
            DomNodeType baseType = new DomNodeType("baseType");
            baseType.Define(new ExtensionInfo<DerivedAdapter>()); // derived adapter on base type!
            DomNodeType derivedType = new DomNodeType("derivedType");
            derivedType.BaseType = baseType;
            derivedType.Define(new ExtensionInfo<SimpleAdapter>());

            DomNode node = new DomNode(derivedType);
            object simple = node.GetAdapter(typeof(SimpleAdapter));
            object derived = node.GetAdapter(typeof(DerivedAdapter));

            // test that they're returned in order of inheritance
            Utilities.TestSequenceEqual(node.GetDecorators(typeof(SimpleAdapter)), simple, derived);
        }

        [Test]
        public void TestEqualityDomNodeAdapter()
        {
            DomNodeType type = new DomNodeType("type");
            type.Define(new ExtensionInfo<SimpleAdapter>());
            DomNode node = new DomNode(type);

            SimpleAdapter test = node.GetAdapter(typeof(SimpleAdapter)) as SimpleAdapter;
            Assert.True(node.Equals(test));
        }

        [Test]
        public void TestEqualityNonDomNodeAdapter()
        {
            DomNodeType type = new DomNodeType("type");
            type.Define(new ExtensionInfo<NonDomNodeAdapter>());
            DomNode node = new DomNode(type);

            NonDomNodeAdapter test = node.GetAdapter(typeof(NonDomNodeAdapter)) as NonDomNodeAdapter;
            Assert.True(node.Equals(test));
        }

        [Test]
        public void TestGetAttribute()
        {
            DomNodeType type = new DomNodeType("child");
            AttributeInfo info = GetIntAttribute("int");
            type.Define(info);
            DomNode test = new DomNode(type);

            Assert.True(test.IsAttributeDefault(info));
            Assert.Null(test.GetLocalAttribute(info));
            Assert.False(test.IsAttributeSet(info));

            test.SetAttribute(info, 2);
            Assert.AreEqual(test.GetAttribute(info), 2);
            Assert.AreEqual(test.GetLocalAttribute(info), 2);
            Assert.False(test.IsAttributeDefault(info));
            Assert.True(test.IsAttributeSet(info));

            test.SetAttribute(info, null);
            Assert.True(test.IsAttributeDefault(info));
            Assert.Null(test.GetLocalAttribute(info));
            Assert.False(test.IsAttributeSet(info));

            test.SetAttribute(info, 0);
            Assert.AreEqual(test.GetAttribute(info), 0);
            Assert.True(test.IsAttributeDefault(info));
            Assert.AreEqual(test.GetLocalAttribute(info), 0);
            Assert.True(test.IsAttributeSet(info));
        }

        [Test]
        public void TestSetAttribute()
        {
            DomNodeType type = new DomNodeType("child");
            AttributeInfo info = GetIntAttribute("int");
            type.Define(info);
            DomNode test = new DomNode(type);

            Assert.False(test.IsAttributeSet(info));
            test.SetAttribute(info, 2);
            Assert.AreEqual(test.GetAttribute(info), 2);
            Assert.AreEqual(test.GetLocalAttribute(info), 2);
            Assert.True(test.IsAttributeSet(info));

            test.SetAttribute(info, null);
            Assert.True(test.IsAttributeDefault(info));
            Assert.Null(test.GetLocalAttribute(info));
            Assert.False(test.IsAttributeSet(info));
        }

        [Test]
        public void TestGetIdDefault()
        {
            DomNodeType testNoId = new DomNodeType("test");
            DomNode test = new DomNode(testNoId);
            Assert.Null(test.GetId());
        }

        [Test]
        public void TestGetId()
        {
            DomNodeType testId = new DomNodeType("test");
            AttributeInfo info = GetStringAttribute("string");
            testId.Define(info);
            testId.SetIdAttribute(info.Name);
            DomNode test = new DomNode(testId);
            Assert.Null(test.GetId());

            test.SetAttribute(info, "foo");
            Assert.AreEqual(test.GetId(), "foo");
        }

        [Test]
        public void TestGetChild()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type);
            type.Define(childInfo);

            DomNode parent = new DomNode(type);
            DomNode child = new DomNode(type);
            parent.SetChild(childInfo, child);
            Assert.AreSame(parent.GetChild(childInfo), child);
            Assert.Throws<InvalidOperationException>(delegate { parent.GetChildList(childInfo); });
        }

        [Test]
        public void TestGetChildList()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);

            DomNode parent = new DomNode(type);
            IList<DomNode> list = parent.GetChildList(childInfo);
            Assert.NotNull(list);
            Assert.Throws<InvalidOperationException>(delegate { parent.GetChild(childInfo); });
            CollectionAssert.IsEmpty(list);
        }

        [Test]
        public void TestChildListInsert()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);
            DomNode parent = new DomNode(type);
            IList<DomNode> list = parent.GetChildList(childInfo);
            DomNode child1 = new DomNode(type);
            list.Insert(0, child1);
            Utilities.TestSequenceEqual(list, child1);
            // insertion again will cause removal, then insertion
            list.Insert(0, child1);
            Utilities.TestSequenceEqual(list, child1);

            DomNode child2 = new DomNode(type);
            list.Insert(0, child2);
            Utilities.TestSequenceEqual(list, child2, child1);
        }

        [Test]
        public void TestChildListRemoveAt()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);
            DomNode parent = new DomNode(type);
            IList<DomNode> list = parent.GetChildList(childInfo);
            DomNode child1 = new DomNode(type);
            list.Add(child1);
            DomNode child2 = new DomNode(type);
            list.Add(child2);

            list.RemoveAt(1);
            Utilities.TestSequenceEqual(list, child1);
            list.RemoveAt(0);
            CollectionAssert.IsEmpty(list);
        }

        [Test]
        public void TestChildListIndexOf()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);
            DomNode parent = new DomNode(type);
            IList<DomNode> list = parent.GetChildList(childInfo);
            DomNode child1 = new DomNode(type);
            Assert.AreEqual(list.IndexOf(child1), -1);
            list.Insert(0, child1);
            Assert.AreEqual(list.IndexOf(child1), 0);
            DomNode child2 = new DomNode(type);
            list.Insert(0, child2);
            Assert.AreEqual(list.IndexOf(child2), 0);
            Assert.AreEqual(list.IndexOf(child1), 1);
        }

        [Test]
        public void TestChildListIndexer()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);
            DomNode parent = new DomNode(type);
            IList<DomNode> list = parent.GetChildList(childInfo);
            DomNode child1 = new DomNode(type);
            Assert.Throws<IndexOutOfRangeException>(delegate { list[0] = child1; });
            list.Add(child1);
            Assert.AreSame(list[0], child1);
            DomNode child2 = new DomNode(type);
            list.Add(child2);
            Assert.AreSame(list[1], child2);

            DomNode child3 = new DomNode(type);
            list[0] = child3;
            Utilities.TestSequenceEqual(list, child3, child2); // child1 gets removed by set
        }

        [Test]
        public void TestChildListAdd()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);
            DomNode parent = new DomNode(type);
            IList<DomNode> list = parent.GetChildList(childInfo);
            DomNode child1 = new DomNode(type);
            list.Add(child1);
            Utilities.TestSequenceEqual(list, child1);
            DomNode child2 = new DomNode(type);
            list.Add(child2);
            Utilities.TestSequenceEqual(list, child1, child2);
            // add node that's already in the list; should remove it from old location
            list.Add(child1);
            Utilities.TestSequenceEqual(list, child2, child1);
        }

        [Test]
        public void TestChildListRemove()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);
            DomNode parent = new DomNode(type);
            IList<DomNode> list = parent.GetChildList(childInfo);
            DomNode child1 = new DomNode(type);
            Assert.False(list.Remove(child1));
            list.Add(child1);
            DomNode child2 = new DomNode(type);
            list.Add(child2);

            Assert.True(list.Remove(child1));
            Utilities.TestSequenceEqual(list, child2);

            Assert.True(list.Remove(child2));
            CollectionAssert.IsEmpty(list);
        }

        [Test]
        public void TestChildListReadonly()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);
            DomNode parent = new DomNode(type);
            IList<DomNode> list = parent.GetChildList(childInfo);
            Assert.False(list.IsReadOnly);
        }

        [Test]
        public void TestChildListContains()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);
            DomNode parent = new DomNode(type);
            IList<DomNode> list = parent.GetChildList(childInfo);
            DomNode child1 = new DomNode(type);
            Assert.False(list.Contains(child1));
            list.Add(child1);
            DomNode child2 = new DomNode(type);
            list.Add(child2);
            Assert.True(list.Contains(child2));
        }

        [Test]
        public void TestChildListClear()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);
            DomNode parent = new DomNode(type);
            IList<DomNode> list = parent.GetChildList(childInfo);
            DomNode child1 = new DomNode(type);
            list.Add(child1);
            DomNode child2 = new DomNode(type);
            list.Add(child2);
            list.Clear();
            CollectionAssert.IsEmpty(list);
        }

        [Test]
        public void TestChildListCopyTo()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);
            DomNode parent = new DomNode(type);
            IList<DomNode> list = parent.GetChildList(childInfo);
            DomNode child1 = new DomNode(type);
            list.Add(child1);
            DomNode child2 = new DomNode(type);
            list.Add(child2);
            DomNode[] array = new DomNode[list.Count + 1];
            list.CopyTo(array, 1);
            Utilities.TestSequenceEqual(array, null, child1, child2);
        }

        [Test]
        public void TestRemoveFromParent()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type);
            type.Define(childInfo);
            DomNode parent = new DomNode(type);
            DomNode child = new DomNode(type);
            parent.SetChild(childInfo, child);
            child.RemoveFromParent();
            Assert.Null(parent.GetChild(childInfo));
            Assert.Null(child.Parent);

            // Make sure the removed child has a null Parent. http://tracker.ship.scea.com/jira/browse/WWSATF-1336
            parent.SetChild(childInfo, child);
            DomNode newChild = new DomNode(type);
            parent.SetChild(childInfo, newChild);
            Assert.Null(child.Parent);
            Assert.True(newChild.Parent == parent);
        }

        [Test]
        public void TestRemoveFromParentList()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo childInfo = new ChildInfo("child", type, true);
            type.Define(childInfo);
            DomNode parent = new DomNode(type);
            DomNode child = new DomNode(type);
            parent.GetChildList(childInfo).Add(child);
            child.RemoveFromParent();
            CollectionAssert.IsEmpty(parent.Children);
            Assert.Null(child.Parent);
        }

        [Test]
        public void TestAttributeChangedEvents()
        {
            DomNodeType type = new DomNodeType("type");
            AttributeInfo stringTypeInfo = GetStringAttribute("string");
            AttributeInfo intTypeInfo = GetIntAttribute("int");
            type.Define(stringTypeInfo);
            type.Define(intTypeInfo);
            DomNode test = new DomNode(type);
            test.AttributeChanging += test_AttributeChanging;
            test.AttributeChanged += test_AttributeChanged;
            AttributeEventArgs expected;

            // test for no value change if setting to the default value and attribute is already the default
            AttributeChangingArgs = null;
            AttributeChangedArgs = null;
            test.SetAttribute(stringTypeInfo, stringTypeInfo.DefaultValue);
            Assert.Null(AttributeChangingArgs);
            Assert.Null(AttributeChangedArgs);
            test.SetAttribute(intTypeInfo, intTypeInfo.DefaultValue);
            Assert.Null(AttributeChangingArgs);
            Assert.Null(AttributeChangedArgs);

            // test for value change, string type
            test = new DomNode(type);
            test.AttributeChanging += test_AttributeChanging;
            test.AttributeChanged += test_AttributeChanged;
            AttributeChangingArgs = null;
            AttributeChangedArgs = null;
            object oldValue = test.GetAttribute(stringTypeInfo);
            test.SetAttribute(stringTypeInfo, "foo");
            expected = new AttributeEventArgs(test, stringTypeInfo, oldValue, "foo");
            Assert.True(Equals(AttributeChangingArgs, expected));
            Assert.True(Equals(AttributeChangedArgs, expected));

            oldValue = test.GetAttribute(stringTypeInfo);
            test.SetAttribute(stringTypeInfo, "foobar");
            expected = new AttributeEventArgs(test, stringTypeInfo, oldValue, "foobar");
            Assert.True(Equals(AttributeChangingArgs, expected));
            Assert.True(Equals(AttributeChangedArgs, expected));

            // test for value change, int type
            AttributeChangingArgs = null;
            AttributeChangedArgs = null;
            oldValue = test.GetAttribute(intTypeInfo);
            test.SetAttribute(intTypeInfo, 5);
            expected = new AttributeEventArgs(test, intTypeInfo, oldValue, 5);
            Assert.True(Equals(AttributeChangingArgs, expected));
            Assert.True(Equals(AttributeChangedArgs, expected));

            oldValue = test.GetAttribute(intTypeInfo);
            test.SetAttribute(intTypeInfo, 7);
            expected = new AttributeEventArgs(test, intTypeInfo, oldValue, 7);
            Assert.True(Equals(AttributeChangingArgs, expected));
            Assert.True(Equals(AttributeChangedArgs, expected));

            // test for no value change
            test.SetAttribute(stringTypeInfo, "foo");
            AttributeChangingArgs = null;
            AttributeChangedArgs = null;
            test.SetAttribute(stringTypeInfo, "foo");
            Assert.Null(AttributeChangingArgs);
            Assert.Null(AttributeChangedArgs);

            test.SetAttribute(intTypeInfo, 9);
            AttributeChangingArgs = null;
            AttributeChangedArgs = null;
            test.SetAttribute(intTypeInfo, 9);
            Assert.Null(AttributeChangingArgs);
            Assert.Null(AttributeChangedArgs);
        }

        private AttributeEventArgs AttributeChangingArgs;
        void test_AttributeChanging(object sender, AttributeEventArgs e)
        {
            AttributeChangingArgs = e;
        }
        private AttributeEventArgs AttributeChangedArgs;
        void test_AttributeChanged(object sender, AttributeEventArgs e)
        {
            AttributeChangedArgs = e;
        }

        [Test]
        public void TestChildInsertEvents()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo info = new ChildInfo("child", type);
            ChildInfo infoList = new ChildInfo("childList", type, true);
            type.Define(info);
            type.Define(infoList);
            DomNode test = new DomNode(type);
            test.ChildInserting += test_ChildInserting;
            test.ChildInserted += test_ChildInserted;

            // test child
            ChildInsertingArgs = null;
            ChildInsertedArgs = null;
            DomNode child = new DomNode(type);
            test.SetChild(info, child);
            ChildEventArgs expected = new ChildEventArgs(test, info, child, 0);
            Assert.True(Equals(ChildInsertingArgs, expected));
            Assert.True(Equals(ChildInsertedArgs, expected));

            // test inserting a child when there is one there already
            ChildInsertingArgs = null;
            ChildInsertedArgs = null;
            DomNode newChild = new DomNode(type);
            test.SetChild(info, newChild);
            expected = new ChildEventArgs(test, info, newChild, 0);
            Assert.True(Equals(ChildInsertingArgs, expected));
            Assert.True(Equals(ChildInsertedArgs, expected));

            // test child list
            IList<DomNode> list = test.GetChildList(infoList);
            ChildInsertingArgs = null;
            ChildInsertedArgs = null;
            DomNode child2 = new DomNode(type);
            list.Add(child2);
            expected = new ChildEventArgs(test, infoList, child2, 0);
            Assert.True(Equals(ChildInsertingArgs, expected));
            Assert.True(Equals(ChildInsertedArgs, expected));
            ChildInsertingArgs = null;
            ChildInsertedArgs = null;
            DomNode child3 = new DomNode(type);
            list.Add(child3);
            expected = new ChildEventArgs(test, infoList, child3, 1);
            Assert.True(Equals(ChildInsertingArgs, expected));
            Assert.True(Equals(ChildInsertedArgs, expected));
        }

        private ChildEventArgs ChildInsertingArgs;
        void test_ChildInserting(object sender, ChildEventArgs e)
        {
            ChildInsertingArgs = e;
        }

        private ChildEventArgs ChildInsertedArgs;
        void test_ChildInserted(object sender, ChildEventArgs e)
        {
            ChildInsertedArgs = e;
        }

        [Test]
        public void TestChildRemoveEvents()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo info = new ChildInfo("child", type);
            ChildInfo infoList = new ChildInfo("childList", type, true);
            type.Define(info);
            type.Define(infoList);
            DomNode test = new DomNode(type);
            test.ChildRemoving += test_ChildRemoving;
            test.ChildRemoved += test_ChildRemoved;

            // test child
            DomNode child = new DomNode(type);
            test.SetChild(info, child);
            ChildRemovingArgs = null;
            ChildRemovedArgs = null;
            test.SetChild(info, null);
            ChildEventArgs expected = new ChildEventArgs(test, info, child, 0);
            Assert.True(Equals(ChildRemovingArgs, expected));
            Assert.True(Equals(ChildRemovedArgs, expected));

            // test inserting a child when there is one there already
            test.SetChild(info, child);
            DomNode newChild = new DomNode(type);
            ChildRemovingArgs = null;
            ChildRemovedArgs = null;
            test.SetChild(info, newChild);
            expected = new ChildEventArgs(test, info, child, 0);
            Assert.True(Equals(ChildRemovingArgs, expected));
            Assert.True(Equals(ChildRemovedArgs, expected));

            // test child list
            IList<DomNode> list = test.GetChildList(infoList);
            DomNode child2 = new DomNode(type);
            list.Add(child2);
            DomNode child3 = new DomNode(type);
            list.Add(child3);
            ChildRemovingArgs = null;
            ChildRemovedArgs = null;
            list.Remove(child3);
            expected = new ChildEventArgs(test, infoList, child3, 1);
            Assert.True(Equals(ChildRemovingArgs, expected));
            Assert.True(Equals(ChildRemovedArgs, expected));
            ChildRemovingArgs = null;
            ChildRemovedArgs = null;
            list.Remove(child2);
            expected = new ChildEventArgs(test, infoList, child2, 0);
            Assert.True(Equals(ChildRemovingArgs, expected));
            Assert.True(Equals(ChildRemovedArgs, expected));
        }

        private ChildEventArgs ChildRemovingArgs;
        void test_ChildRemoving(object sender, ChildEventArgs e)
        {
            ChildRemovingArgs = e;
        }

        private ChildEventArgs ChildRemovedArgs;
        void test_ChildRemoved(object sender, ChildEventArgs e)
        {
            ChildRemovedArgs = e;
        }

        [Test]
        public void TestCopy_SingleNode()
        {
            DomNodeType type = new DomNodeType("type");
            AttributeInfo info = GetStringAttribute("string");
            type.Define(info);
            DomNode test = new DomNode(type);
            test.SetAttribute(info, "foo");

            DomNode[] result = DomNode.Copy(new DomNode[] { test });
            Assert.True(Equals(result[0], test));

            DomNode singleResult = DomNode.Copy(test);
            Assert.True(Equals(singleResult, test));
        }

        [Test]
        public void TestCopy_MultipleNodes()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo info = new ChildInfo("child", type);
            ChildInfo infoList = new ChildInfo("childList", type, true);
            type.Define(info);
            type.Define(infoList);
            ChildInfo rootInfo = new ChildInfo("root", type, true);
            DomNode test = new DomNode(type, rootInfo);
            DomNode child1 = new DomNode(type);
            test.SetChild(info, child1);
            DomNode child2 = new DomNode(type);
            DomNode child3 = new DomNode(type);
            IList<DomNode> list = test.GetChildList(infoList);
            list.Add(child2);
            list.Add(child3);

            DomNode[] result = DomNode.Copy(new DomNode[] { test });
            Assert.AreEqual(result.Length, 1);
            Assert.True(Equals(result[0], test));

            DomNode singleResult = DomNode.Copy(test);
            Assert.True(Equals(singleResult, test));
        }

        private class DomTypes
        {
            static DomTypes()
            {
                // register extensions
                eventType.Type.Define(new ExtensionInfo<Event>());

                // Enable metadata driven property editing for events and resources
                AdapterCreator<CustomTypeDescriptorNodeAdapter> creator =
                    new AdapterCreator<CustomTypeDescriptorNodeAdapter>();
                eventType.Type.AddAdapterCreator(creator);

                // annotate types with display information for palette
                eventType.Type.SetTag(
                    new NodeTypePaletteItem(
                        eventType.Type,
                        "Event".Localize(),
                        "Event in a sequence".Localize(),
                        Resources.FactoryImage));

                // register property descriptors on state, transition, folder types
                eventType.Type.SetTag(
                    new System.ComponentModel.PropertyDescriptorCollection(
                        new Sce.Atf.Dom.PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Name".Localize(),
                                eventType.nameAttribute,
                                null,
                                "Event name".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Time".Localize(),
                                eventType.timeAttribute,
                                null,
                                "Event starting time".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Duration".Localize(),
                                eventType.durationAttribute,
                                null,
                                "Event duration".Localize(),
                                false),
                    }));
            }

            public static class eventType
            {
                static eventType()
                {
                    Type.Define(nameAttribute);
                    Type.Define(timeAttribute);
                    Type.Define(durationAttribute);
                    Type.Define(resourceChild);
                    resourceChild.AddRule(new ChildCountRule(1, int.MaxValue));
                }

                public readonly static DomNodeType Type = new DomNodeType("eventType");
                public readonly static AttributeInfo nameAttribute =
                    new AttributeInfo("name", new AttributeType(AttributeTypes.String.ToString(), typeof(string)));
                public readonly static AttributeInfo timeAttribute =
                    new AttributeInfo("time", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
                public readonly static AttributeInfo durationAttribute =
                    new AttributeInfo("duration", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
                public readonly static ChildInfo resourceChild = new ChildInfo("resource", Type, true);
            }
        }

        private class Event : DomNodeAdapter
        {
            public string Name
            {
                get { return GetAttribute<string>(DomTypes.eventType.nameAttribute); }
                set { SetAttribute(DomTypes.eventType.nameAttribute, value); }
            }

            public int Time
            {
                get { return GetAttribute<int>(DomTypes.eventType.timeAttribute); }
                set { SetAttribute(DomTypes.eventType.timeAttribute, value); }
            }

            public int Duration
            {
                get { return GetAttribute<int>(DomTypes.eventType.durationAttribute); }
                set { SetAttribute(DomTypes.eventType.durationAttribute, value); }
            }

            public IList<object> Resources
            {
                get { return GetChildList<object>(DomTypes.eventType.resourceChild); }
            }
        }

        private bool Equals(DomNode n1, DomNode n2)
        {
            if (n1 == null || n2 == null)
                return n1 == n2;
            if (n1.Type != n2.Type)
                return false;
            if (n1.ChildInfo != n2.ChildInfo)
                return false;

            foreach (AttributeInfo info in n1.Type.Attributes)
            {
                object val1 = n1.GetLocalAttribute(info);
                object val2 = n2.GetLocalAttribute(info);
                if (val1 == null || val2 == null)
                    if (val1 != val2)
                        return false;
            }

            foreach (ChildInfo info in n1.Type.Children)
            {
                if (info.IsList)
                {
                    IList<DomNode> children1 = n1.GetChildList(info);
                    IList<DomNode> children2 = n1.GetChildList(info);
                    if (children1.Count != children2.Count)
                        return false;
                    for (int i = 0; i < children1.Count; i++)
                        if (!Equals(children1[i], children2[i]))
                            return false;
                }
                else
                {
                    DomNode child1 = n1.GetChild(info);
                    DomNode child2 = n2.GetChild(info);
                    if (!Equals(child1, child2))
                        return false;
                }
            }

            return true;
        }
    }
}
