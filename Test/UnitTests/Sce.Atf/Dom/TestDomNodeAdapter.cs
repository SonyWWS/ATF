//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using System.Collections;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestDomNodeAdapter : DomTest
    {
        public class SimpleAdapter : DomNodeAdapter
        {
        }

        public class DerivedAdapter : SimpleAdapter
        {
        }

        private class VisibleAdapter : DomNodeAdapter
        {
            protected override void OnNodeSet()
            {
                base.OnNodeSet();
                OnNodeSetCalled = true;
                DomNode.AttributeChanged += (sender, args) => LastAttributeChangedArgs = args;
            }
            public bool OnNodeSetCalled;
            public AttributeEventArgs LastAttributeChangedArgs;

            public override object GetAdapter(Type type)
            {
                GetAdapterType = type;
                return base.GetAdapter(type);
            }
            public Type GetAdapterType;

            public override IEnumerable<object> GetDecorators(Type type)
            {
                GetDecoratorsType = type;
                return base.GetDecorators(type);
            }
            public Type GetDecoratorsType;

            public new T GetAttribute<T>(AttributeInfo attributeInfo) { return base.GetAttribute<T>(attributeInfo); }

            public new void SetAttribute(AttributeInfo attributeInfo, object value) { base.SetAttribute(attributeInfo, value); }

            public new T GetReference<T>(AttributeInfo attributeInfo)
                where T : class { return base.GetReference<T>(attributeInfo); }

            public new void SetReference(AttributeInfo attributeInfo, IAdaptable value)
                { base.SetReference(attributeInfo, value); }

            public new T GetChild<T>(ChildInfo childInfo)
                where T : class { return base.GetChild<T>(childInfo); }

            public new void SetChild(ChildInfo childInfo, IAdaptable value)
                { base.SetChild(childInfo, value); }

            public new IList<T> GetChildList<T>(ChildInfo childInfo)
                where T : class { return base.GetChildList<T>(childInfo); }

            public new T GetExtension<T>(ExtensionInfo extensionInfo)
                where T : class { return base.GetExtension<T>(extensionInfo); }
        }

        [Test]
        public void TestOnNodeSet()
        {
            DomNodeType type = new DomNodeType("type");
            type.Define(new ExtensionInfo<VisibleAdapter>());
            DomNode node = new DomNode(type);

            VisibleAdapter test = node.GetAdapter(typeof(VisibleAdapter)) as VisibleAdapter;
            Assert.NotNull(test);
            Assert.AreSame(test.DomNode, node);
            Assert.AreSame(((IAdapter)test).Adaptee, node);
            Assert.True(test.OnNodeSetCalled);
            Assert.Null(test.GetAdapterType);
            Assert.Null(test.GetDecoratorsType);
        }

        [Test]
        public void TestGetSetAttribute()
        {
            DomNodeType type = new DomNodeType("type");
            AttributeInfo intInfo = GetIntAttribute("int");
            type.Define(intInfo);
            AttributeInfo stringInfo = GetStringAttribute("string");
            type.Define(stringInfo);
            type.Define(new ExtensionInfo<VisibleAdapter>());
            DomNode node = new DomNode(type);

            VisibleAdapter test = node.GetAdapter(typeof(VisibleAdapter)) as VisibleAdapter;
            test.SetAttribute(intInfo, 1);
            Assert.AreEqual(test.GetAttribute<int>(intInfo), 1);
            test.SetAttribute(stringInfo, "foo");
            Assert.AreEqual(test.GetAttribute<string>(stringInfo), "foo");
        }

        // Currently, non-null values can't be used as the default values for attributes
        //  that are reference types (or at least DomNode reference types). A solution exists,
        //  but it would complicate DomNode.
        //  http://tracker.ship.scea.com/jira/browse/WWSATF-1357
        //[Test]
        //public void TestNonNullDefaultReferenceAttribute()
        //{
        //    DomNodeType type = new DomNodeType("type");
        //    AttributeInfo objInfo = new AttributeInfo("obj", new AttributeType("objType", typeof(DomNode)));
        //    var defaultObjAttribute = new DomNode(DomNodeType.BaseOfAllTypes);
        //    objInfo.DefaultValue = defaultObjAttribute;
        //    type.Define(objInfo);
        //    type.Define(new ExtensionInfo<VisibleAdapter>());
        //    DomNode node = new DomNode(type);

        //    VisibleAdapter test = node.GetAdapter(typeof(VisibleAdapter)) as VisibleAdapter;

        //    // Set attribute to its default value. Make sure no attribute change event is received.
        //    Assert.AreEqual(test.GetAttribute<DomNode>(objInfo), objInfo.DefaultValue);
        //    Assert.AreEqual(test.GetAttribute<DomNode>(objInfo), defaultObjAttribute);
        //    test.LastAttributeChangedArgs = null;
        //    test.SetAttribute(objInfo, defaultObjAttribute);
        //    Assert.IsNull(test.LastAttributeChangedArgs);

        //    // Make sure we can set it to null, even though the default is not null.
        //    Assert.AreEqual(test.GetAttribute<DomNode>(objInfo), objInfo.DefaultValue);
        //    Assert.AreEqual(test.GetAttribute<DomNode>(objInfo), defaultObjAttribute);
        //    test.SetAttribute(objInfo, null);
        //    Assert.IsNotNull(test.LastAttributeChangedArgs);
        //    Assert.IsNull(test.GetAttribute<DomNode>(objInfo));

        //    // Set attribute to something else. Make sure old value is null and new value is correct.
        //    test.LastAttributeChangedArgs = null;
        //    var newAttributeValue = new DomNode(type);
        //    test.SetAttribute(objInfo, newAttributeValue);
        //    Assert.IsNotNull(test.LastAttributeChangedArgs);
        //    Assert.AreEqual(test.LastAttributeChangedArgs.OldValue, null);
        //    Assert.AreEqual(test.LastAttributeChangedArgs.NewValue, newAttributeValue);
        //    Assert.IsNotNull(test.GetAttribute<DomNode>(objInfo));
        //}

        [Test]
        public void TestGetSetReference()
        {
            DomNodeType type = new DomNodeType("type");
            AttributeInfo refInfo = GetRefAttribute("ref");
            type.Define(refInfo);
            type.Define(new ExtensionInfo<VisibleAdapter>());
            DomNode node = new DomNode(type);
            DomNode refNode = new DomNode(type);

            VisibleAdapter test = node.GetAdapter(typeof(VisibleAdapter)) as VisibleAdapter;
            VisibleAdapter adapter = refNode.GetAdapter(typeof(VisibleAdapter)) as VisibleAdapter;
            test.SetReference(refInfo, adapter);
            Assert.AreSame(test.GetReference<VisibleAdapter>(refInfo), adapter);
        }

        [Test]
        public void TestGetSetChild()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo info = new ChildInfo("child", type);
            type.Define(info);
            type.Define(new ExtensionInfo<VisibleAdapter>());
            DomNode node = new DomNode(type);
            DomNode child = new DomNode(type);

            VisibleAdapter test = node.GetAdapter(typeof(VisibleAdapter)) as VisibleAdapter;
            VisibleAdapter adapter = child.GetAdapter(typeof(VisibleAdapter)) as VisibleAdapter;
            test.SetChild(info, adapter);
            Assert.AreSame(test.GetChild<VisibleAdapter>(info), adapter);

            test.SetChild(info, null);
            Assert.Null(test.GetChild<VisibleAdapter>(info));
        }

        [Test]
        public void TestGetChildList()
        {
            DomNodeType type = new DomNodeType("type");
            ChildInfo info = new ChildInfo("child", type, true);
            type.Define(info);
            type.Define(new ExtensionInfo<VisibleAdapter>());
            DomNode node = new DomNode(type);
            DomNode child = new DomNode(type);

            VisibleAdapter test = node.GetAdapter(typeof(VisibleAdapter)) as VisibleAdapter;
            VisibleAdapter adapter = child.GetAdapter(typeof(VisibleAdapter)) as VisibleAdapter;

            IList<VisibleAdapter> list = test.GetChildList<VisibleAdapter>(info);
            Assert.NotNull(list);
        }

        [Test]
        public void TestGetExtension()
        {
            DomNodeType type = new DomNodeType("type");
            type.Define(new ExtensionInfo<VisibleAdapter>());
            ExtensionInfo info = new ExtensionInfo<TestDomNodeAdapter>();
            type.Define(info);
            DomNode node = new DomNode(type);

            VisibleAdapter test = node.GetAdapter(typeof(VisibleAdapter)) as VisibleAdapter;
            Assert.NotNull(test.GetExtension<TestDomNodeAdapter>(info));
        }

        [Test]
        public void TestGetAdapter()
        {
            DomNodeType type = new DomNodeType("type");
            type.Define(new ExtensionInfo<VisibleAdapter>());
            type.Define(new ExtensionInfo<SimpleAdapter>());
            DomNode node = new DomNode(type);

            VisibleAdapter test = node.GetAdapter(typeof(VisibleAdapter)) as VisibleAdapter;

            Assert.AreSame(test.GetAdapter(typeof(object)), node.GetAdapter(typeof(object)));
            Assert.AreEqual(test.GetAdapterType, typeof(object));
            Assert.AreSame(test.GetAdapter(typeof(VisibleAdapter)), node.GetAdapter(typeof(VisibleAdapter)));
            Assert.AreSame(test.GetAdapter(typeof(SimpleAdapter)), node.GetAdapter(typeof(SimpleAdapter)));
        }

        [Test]
        public void TestGetDecorators()
        {
            DomNodeType type = new DomNodeType("type");
            type.Define(new ExtensionInfo<VisibleAdapter>());
            type.Define(new ExtensionInfo<SimpleAdapter>());
            DomNode node = new DomNode(type);

            VisibleAdapter test = node.GetAdapter(typeof(VisibleAdapter)) as VisibleAdapter;

            Assert.True(test.GetDecorators(typeof(DomNodeAdapter)).SequenceEqual(node.GetDecorators(typeof(DomNodeAdapter))));
            Assert.AreEqual(test.GetDecoratorsType, typeof(DomNodeAdapter));

            Assert.True(test.GetDecorators(typeof(SimpleAdapter)).SequenceEqual(node.GetDecorators(typeof(SimpleAdapter))));
        }

        [Test]
        public void TestEqualsDomNodeAdapter()
        {
            DomNodeType type = new DomNodeType("type");
            type.Define(new ExtensionInfo<SimpleAdapter>());
            DomNode node = new DomNode(type);

            SimpleAdapter test = node.GetAdapter(typeof(SimpleAdapter)) as SimpleAdapter;
            Assert.True(test.Equals(node));
        }

        [Test]
        public void TestDerivedDomNodeAdapters()
        {
            var baseType = new DomNodeType("baseType");
            baseType.Define(new ExtensionInfo<SimpleAdapter>());

            var derivedType = new DomNodeType("derivedType", baseType);
            derivedType.Define(new ExtensionInfo<DerivedAdapter>());

            var derivedNode = new DomNode(derivedType);
            var derivedTypeAdapters = new List<DomNodeAdapter>(derivedNode.AsAll<DomNodeAdapter>());
            Assert.True(derivedTypeAdapters.Count == 2);
            Assert.True(ContainsExactType<SimpleAdapter>(derivedTypeAdapters));
            Assert.True(ContainsExactType<DerivedAdapter>(derivedTypeAdapters));
        }

        private static bool ContainsExactType<T>(IEnumerable list)
        {
            foreach(object obj in list)
                if (obj.GetType().Equals(typeof(T)))
                    return true;
            return false;
        }
    }
}
