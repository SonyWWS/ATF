//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Linq;
using NUnit.Framework;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestDomNodeType : DomTest
    {
        [Test]
        public void TestBaseType()
        {
            DomNodeType test = new DomNodeType("test");
            Assert.AreEqual(test.BaseType, DomNodeType.BaseOfAllTypes);
            DomNodeType baseType = new DomNodeType("base");
            test.BaseType = baseType;
            Assert.AreEqual(test.BaseType, baseType);

            DomNode node = new DomNode(test);
            // base type is now frozen
            Assert.Throws<InvalidOperationException>(delegate { test.BaseType = new DomNodeType("newBase"); });

            test = new DomNodeType(
                "test",
                null,
                EmptyEnumerable<AttributeInfo>.Instance,
                EmptyEnumerable<ChildInfo>.Instance,
                EmptyEnumerable<ExtensionInfo>.Instance);

            Assert.AreEqual(test.BaseType, DomNodeType.BaseOfAllTypes);
            test.BaseType = baseType;
            Assert.AreEqual(test.BaseType, baseType);

            node = new DomNode(test);
            Assert.Throws<InvalidOperationException>(delegate { test.BaseType = new DomNodeType("newBase"); });
        }

        [Test]
        public void TestBaseOfAllTypes()
        {
            DomNodeType test = DomNodeType.BaseOfAllTypes;
            Assert.NotNull(test);
            CollectionAssert.IsEmpty(test.Attributes);
            CollectionAssert.IsEmpty(test.Children);
            CollectionAssert.IsEmpty(test.Extensions);
            Assert.Null(test.BaseType);

            // test that it's frozen
            Assert.Throws<InvalidOperationException>(delegate { test.BaseType = new DomNodeType("foo"); });
            Assert.Throws<InvalidOperationException>(delegate { test.Define(GetStringAttribute("foo")); });
            Assert.Throws<InvalidOperationException>(delegate { test.Define(new ChildInfo("foo", new DomNodeType("foo"))); });
            Assert.Throws<InvalidOperationException>(delegate { test.Define(new ExtensionInfo<int>()); });
        }

        [Test]
        public void TestLineage()
        {
            DomNodeType child = new DomNodeType("child");
            Utilities.TestSequenceEqual(child.Lineage, child, DomNodeType.BaseOfAllTypes);
            DomNodeType parent = new DomNodeType("parent");
            child.BaseType = parent;
            Utilities.TestSequenceEqual(child.Lineage, child, parent, DomNodeType.BaseOfAllTypes);
        }

        [Test]
        public void TestDefaultAttributeInfo()
        {
            DomNodeType test = new DomNodeType("foo");
            CollectionAssert.IsEmpty(test.Attributes);
        }

        [Test]
        public void TestCustomAttributeInfo()
        {
            AttributeInfo info = new AttributeInfo("foo", new AttributeType("foo", typeof(string)));
            DomNodeType test = new DomNodeType(
                "test",
                null,
                new AttributeInfo[] { info },
                EmptyEnumerable<ChildInfo>.Instance,
                EmptyEnumerable<ExtensionInfo>.Instance);

            Utilities.TestSequenceEqual(test.Attributes, info);
            Assert.True(test.IsValid(info));
            Assert.AreSame(test.GetAttributeInfo("foo"), info);
            // check that type is now frozen
            Assert.Throws<InvalidOperationException>(delegate { test.Define(GetStringAttribute("notFoo")); });

            Assert.AreEqual(info.OwningType, test);
            Assert.AreEqual(info.DefiningType, test);
            Assert.Null(test.GetAttributeInfo("bar"));
        }

        [Test]
        public void TestInheritedAttributeInfo()
        {
            AttributeInfo info = new AttributeInfo("foo", new AttributeType("foo", typeof(string)));
            DomNodeType test = new DomNodeType(
                "test",
                null,
                new AttributeInfo[] { info },
                EmptyEnumerable<ChildInfo>.Instance,
                EmptyEnumerable<ExtensionInfo>.Instance);

            DomNodeType child = new DomNodeType("child");
            child.BaseType = test;

            AttributeInfo inherited = child.GetAttributeInfo("foo");
            Assert.AreEqual(inherited.OwningType, test);
            Assert.AreEqual(inherited.DefiningType, test);

            Assert.True(inherited.Equivalent(info));
            Assert.True(info.Equivalent(inherited));
        }

        [Test]
        public void TestOverriddenAttributeInfo()
        {
            AttributeInfo info = new AttributeInfo("foo", new AttributeType("foo", typeof(string)));
            DomNodeType test = new DomNodeType(
                "test",
                null,
                new AttributeInfo[] { info },
                EmptyEnumerable<ChildInfo>.Instance,
                EmptyEnumerable<ExtensionInfo>.Instance);

            AttributeInfo overridden = new AttributeInfo("foo", new AttributeType("foo", typeof(string)));
            DomNodeType child = new DomNodeType("child", test,
                new AttributeInfo[] { overridden },
                EmptyEnumerable<ChildInfo>.Instance,
                EmptyEnumerable<ExtensionInfo>.Instance);

            Assert.AreSame(child.GetAttributeInfo("foo"), overridden);
            Assert.AreEqual(overridden.OwningType, child);
            Assert.AreEqual(overridden.DefiningType, test);

            Assert.True(info.Equivalent(overridden));
            Assert.True(overridden.Equivalent(info));

            Assert.True(test.IsValid(overridden));
            Assert.True(child.IsValid(info));
        }

        [Test]
        public void TestDefaultChildInfo()
        {
            DomNodeType test = new DomNodeType("foo");
            CollectionAssert.IsEmpty(test.Children);
        }

        [Test]
        public void TestCustomChildInfo()
        {
            DomNodeType type = new DomNodeType("foo");
            ChildInfo info = new ChildInfo("foo", type);
            DomNodeType test = new DomNodeType(
                "test",
                null,
                EmptyEnumerable<AttributeInfo>.Instance,
                new ChildInfo[] { info },
                EmptyEnumerable<ExtensionInfo>.Instance);

            Utilities.TestSequenceEqual(test.Children, info);
            Assert.True(test.IsValid(info));
            Assert.AreSame(test.GetChildInfo("foo"), info);
            // check that type is now frozen
            Assert.Throws<InvalidOperationException>(delegate { test.Define(new ChildInfo("notFoo", type)); });

            Assert.AreEqual(info.OwningType, test);
            Assert.AreEqual(info.DefiningType, test);
            Assert.Null(test.GetChildInfo("bar"));
        }

        [Test]
        public void TestInheritedChildInfo()
        {
            ChildInfo info = new ChildInfo("foo", new DomNodeType("foo"));
            DomNodeType test = new DomNodeType(
                "test",
                null,
                EmptyEnumerable<AttributeInfo>.Instance,
                new ChildInfo[] { info },
                EmptyEnumerable<ExtensionInfo>.Instance);

            DomNodeType child = new DomNodeType("child");
            child.BaseType = test;

            ChildInfo inherited = child.GetChildInfo("foo");
            Assert.AreEqual(inherited.OwningType, test);
            Assert.AreEqual(inherited.DefiningType, test);

            Assert.True(inherited.Equivalent(info));
            Assert.True(info.Equivalent(inherited));
        }

        [Test]
        public void TestOverriddenChildInfo()
        {
            DomNodeType childType = new DomNodeType("foo");
            ChildInfo info = new ChildInfo("foo", childType);
            DomNodeType test = new DomNodeType(
                "test",
                null,
                EmptyEnumerable<AttributeInfo>.Instance,
                new ChildInfo[] { info },
                EmptyEnumerable<ExtensionInfo>.Instance);

            ChildInfo overridden = new ChildInfo("foo", childType);
            DomNodeType child = new DomNodeType("child", test,
                EmptyEnumerable<AttributeInfo>.Instance,
                new ChildInfo[] { overridden },
                EmptyEnumerable<ExtensionInfo>.Instance);

            Assert.AreSame(child.GetChildInfo("foo"), overridden);
            Assert.AreEqual(overridden.OwningType, child);
            Assert.AreEqual(overridden.DefiningType, test);

            Assert.True(info.Equivalent(overridden));
            Assert.True(overridden.Equivalent(info));

            Assert.True(test.IsValid(overridden));
            Assert.True(child.IsValid(info));
        }

        [Test]
        public void TestDefaultExtensionInfo()
        {
            DomNodeType test = new DomNodeType("foo");
            CollectionAssert.IsEmpty(test.Extensions);
        }

        [Test]
        public void TestCustomExtensionInfo()
        {
            ExtensionInfo info = new ExtensionInfo<TestDomNodeType>("foo");
            DomNodeType test = new DomNodeType(
                "test",
                null,
                EmptyEnumerable<AttributeInfo>.Instance,
                EmptyEnumerable<ChildInfo>.Instance,
                new ExtensionInfo[] { info });

            Utilities.TestSequenceEqual(test.Extensions, info);
            Assert.True(test.IsValid(info));
            Assert.AreSame(test.GetExtensionInfo("foo"), info);
            // check that type is now frozen
            Assert.Throws<InvalidOperationException>(delegate { test.Define(new ExtensionInfo<TestDomNodeType>("foo")); });

            Assert.AreEqual(info.OwningType, test);
            Assert.AreEqual(info.DefiningType, test);
            Assert.Null(test.GetExtensionInfo("bar"));
        }

        [Test]
        public void TestInheritedExtensionInfo()
        {
            ExtensionInfo info = new ExtensionInfo<TestDomNodeType>("foo");
            DomNodeType test = new DomNodeType(
                "test",
                null,
                EmptyEnumerable<AttributeInfo>.Instance,
                EmptyEnumerable<ChildInfo>.Instance,
                new ExtensionInfo[] { info });

            DomNodeType child = new DomNodeType("child");
            child.BaseType = test;

            ExtensionInfo inherited = child.GetExtensionInfo("foo");
            Assert.AreEqual(inherited.OwningType, test);
            Assert.AreEqual(inherited.DefiningType, test);

            Assert.True(inherited.Equivalent(info));
            Assert.True(info.Equivalent(inherited));
        }

        [Test]
        public void TestOverriddenExtensionInfo()
        {
            ExtensionInfo info = new ExtensionInfo<TestDomNodeType>("foo");
            DomNodeType test = new DomNodeType(
                "test",
                null,
                EmptyEnumerable<AttributeInfo>.Instance,
                EmptyEnumerable<ChildInfo>.Instance,
                new ExtensionInfo[] { info });

            ExtensionInfo overridden = new ExtensionInfo<TestDomNodeType>("foo");
            DomNodeType child = new DomNodeType("child", test,
                EmptyEnumerable<AttributeInfo>.Instance,
                EmptyEnumerable<ChildInfo>.Instance,
                new ExtensionInfo[] { overridden });

            Assert.AreSame(child.GetExtensionInfo("foo"), overridden);
            Assert.AreEqual(overridden.OwningType, child);
            Assert.AreEqual(overridden.DefiningType, test);

            Assert.True(info.Equivalent(overridden));
            Assert.True(overridden.Equivalent(info));

            Assert.True(test.IsValid(overridden));
            Assert.True(child.IsValid(info));
        }

        private class TestAdapter1 : DomNodeAdapter
        {
        }

        private class TestAdapter2 : DomNodeAdapter
        {
        }

        [Test]
        public void TestDuplicateExtensionInfo()
        {
            var ext1 = new ExtensionInfo<TestAdapter1>("foo");
            var ext2 = new ExtensionInfo<TestAdapter2>("foo");

            var domType = new DomNodeType(
                "test",
                null,
                EmptyEnumerable<AttributeInfo>.Instance,
                EmptyEnumerable<ChildInfo>.Instance,
                new ExtensionInfo[] { ext1, ext2 });

            var domNode = new DomNode(domType);

            object resultExt1 = domNode.GetExtension(ext1);
            object resultExt2 = domNode.GetExtension(ext2);

            Assert.IsTrue(resultExt1 is TestAdapter1);
            Assert.IsTrue(resultExt2 is TestAdapter2);

            object[] decorators = domNode.GetDecorators(typeof(object)).ToArray();
            Assert.IsTrue(
                decorators.Length == 2 &&
                decorators[0] == resultExt1 &&
                decorators[1] == resultExt2);

            DomNodeAdapter[] extensions = domNode.AsAll<DomNodeAdapter>().ToArray();
            Assert.IsTrue(
                extensions.Length == 2 &&
                extensions[0] == resultExt1 &&
                extensions[1] == resultExt2);

            // Searching by name is a problem, though. The search is ambiguous.
            // See tracker item for discussion: http://tracker.ship.scea.com/jira/browse/WWSATF-522
            Assert.Throws<InvalidOperationException>(() => domType.GetExtensionInfo("foo"));
        }

        [Test]
        public void TestDuplicateAttributeInfo()
        {
            // This would be illegal in a schema file, but it seems to be supported OK in the DOM.
            var attr1 = new AttributeInfo("foo", new AttributeType("foo", typeof(string)));
            var attr2 = new AttributeInfo("foo", new AttributeType("foo", typeof(string)));
            var domType = new DomNodeType(
                "test",
                null,
                new AttributeInfo[] { attr1, attr2 },
                EmptyEnumerable<ChildInfo>.Instance,
                EmptyEnumerable<ExtensionInfo>.Instance);

            var domNode = new DomNode(domType);
            
            var originalAttr1 = "setting attr1";
            var originalAttr2 = "setting attr2";

            domNode.SetAttribute(attr1, originalAttr1);
            domNode.SetAttribute(attr2, originalAttr2);

            string resultAttr1 = (string)domNode.GetAttribute(attr1);
            string resultAttr2 = (string)domNode.GetAttribute(attr2);

            Assert.IsTrue(string.Equals(resultAttr1,originalAttr1));
            Assert.IsTrue(string.Equals(resultAttr2, originalAttr2));
        }

        [Test]
        public void TestIdAttribute()
        {
            AttributeInfo idAttr = new AttributeInfo("foo", new AttributeType("foo", typeof(string)));
            DomNodeType test = new DomNodeType(
                "test",
                null,
                new AttributeInfo[] { idAttr },
                EmptyEnumerable<ChildInfo>.Instance,
                EmptyEnumerable<ExtensionInfo>.Instance);

            Assert.Null(test.IdAttribute);

            test.SetIdAttribute("foo");
            Assert.AreSame(test.IdAttribute, idAttr);
        }

        [Test]
        public void TestBaseIdAttribute()
        {
            AttributeInfo idAttr = new AttributeInfo("foo", new AttributeType("foo", typeof(string)));
            DomNodeType parent = new DomNodeType(
                "test",
                null,
                new AttributeInfo[] { idAttr },
                EmptyEnumerable<ChildInfo>.Instance,
                EmptyEnumerable<ExtensionInfo>.Instance);

            parent.SetIdAttribute("foo");

            DomNodeType child = new DomNodeType("child");
            child.BaseType = parent;

            Assert.AreSame(child.IdAttribute, idAttr);
        }

        [Test]
        public void TestIsAbstract()
        {
            DomNodeType test = new DomNodeType("test");
            Assert.False(test.IsAbstract);
            test.IsAbstract = true;
            Assert.True(test.IsAbstract);

            Assert.Throws<InvalidOperationException>(delegate { DomNode node = new DomNode(test); });
        }

        [Test]
        public void TestIsAssignableFrom()
        {
            DomNodeType parent = new DomNodeType("parent");
            Assert.True(parent.IsAssignableFrom(parent));

            DomNodeType child = new DomNodeType("child");
            child.BaseType = parent;

            Assert.True(parent.IsAssignableFrom(child));
            Assert.False(child.IsAssignableFrom(parent));
        }

        [Test]
        public void TestGetDescendantInfo()
        {
            DomNodeType grandchildType = new DomNodeType("grandchild");
            ChildInfo grandChildInfo = new ChildInfo("grandChild", grandchildType);
            DomNodeType childType = new DomNodeType("child");
            childType.Define(grandChildInfo);

            Assert.Null(childType.GetDescendantInfo(string.Empty));
            Assert.Null(childType.GetDescendantInfo("foo"));
            Assert.AreSame(childType.GetDescendantInfo("grandChild"), grandChildInfo);

            ChildInfo childInfo = new ChildInfo("child", childType);
            DomNodeType parentType = new DomNodeType("parent");
            parentType.Define(childInfo);

            Assert.AreSame(parentType.GetDescendantInfo("child"), childInfo);
            Assert.AreSame(parentType.GetDescendantInfo("child:grandChild"), grandChildInfo);
        }
    }
}
