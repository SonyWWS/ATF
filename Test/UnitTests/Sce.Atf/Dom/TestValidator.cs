//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using NUnit.Framework;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestValidator : DomTest
    {
        private DomNodeType m_rootType;
        private DomNodeType m_childType;
        private AttributeInfo m_stringAttrInfo;
        private AttributeInfo m_refAttrInfo;
        private ChildInfo m_childInfo;

        public TestValidator()
        {
            // define a tree of validation contexts
            m_childType = new DomNodeType("test");
            m_stringAttrInfo = GetStringAttribute("string");
            m_childType.Define(m_stringAttrInfo);
            m_refAttrInfo = GetRefAttribute("ref");
            m_childType.Define(m_refAttrInfo);
            m_childInfo = new ChildInfo("child", m_childType);
            m_childType.Define(m_childInfo);
            m_childType.Define(new ExtensionInfo<ValidationContext>());

            // define a distinct root type with the validator
            m_rootType = new DomNodeType("root");
            m_rootType.BaseType = m_childType;
            m_rootType.Define(new ExtensionInfo<Validator>());

            IEnumerable<AttributeInfo> attributes = m_rootType.Attributes; // freezes the types
        }

        private DomNode CreateTree()
        {
            DomNode root = new DomNode(m_rootType);
            DomNode child = new DomNode(m_childType);
            DomNode grandchild = new DomNode(m_childType);
            child.SetChild(m_childInfo, grandchild);
            root.SetChild(m_childInfo, child);
            return root;
        }

        [Test]
        public void TestOnAttributeSet()
        {
            DomNode root = CreateTree();
            Validator validator = root.As<Validator>();
            DomNode grandchild = root.GetChild(m_childInfo).GetChild(m_childInfo);
            grandchild.SetAttribute(m_stringAttrInfo, "foo");
            Assert.AreSame(validator.Sender, root);
            AttributeEventArgs e = (AttributeEventArgs)validator.E;
            Assert.NotNull(e);
            Assert.AreSame(e.DomNode, grandchild);
        }

        [Test]
        public void TestOnChildInserted()
        {
            DomNode root = new DomNode(m_rootType);
            Validator validator = root.As<Validator>();
            DomNode child = new DomNode(m_childType);
            root.SetChild(m_childInfo, child);
            Assert.AreSame(validator.Sender, root);
            ChildEventArgs e = (ChildEventArgs)validator.E;
            Assert.NotNull(e);
            Assert.AreSame(e.Parent, root);
        }

        [Test]
        public void TestOnChildRemoved()
        {
            DomNode root = new DomNode(m_rootType);
            Validator validator = root.As<Validator>();
            DomNode child = new DomNode(m_childType);
            root.SetChild(m_childInfo, child);
            root.SetChild(m_childInfo, null);
            Assert.AreSame(validator.Sender, root);
            ChildEventArgs e = (ChildEventArgs)validator.E;
            Assert.NotNull(e);
            Assert.AreSame(e.Parent, root);
        }

        [Test]
        public void TestBeginning()
        {
            DomNode root = CreateTree();
            Validator validator = root.As<Validator>();
            ValidationContext context = root.As<ValidationContext>();
            context.RaiseBeginning();
            Assert.True(validator.IsValidating);
            Assert.AreSame(validator.Sender, context);
            Assert.AreSame(validator.E, EventArgs.Empty);
        }

        [Test]
        public void TestEnding()
        {
            DomNode root = CreateTree();
            Validator validator = root.As<Validator>();
            ValidationContext context = root.As<ValidationContext>();
            context.RaiseBeginning();
            Assert.True(validator.IsValidating);
            context.RaiseEnding();
            Assert.False(validator.IsValidating);
            Assert.AreSame(validator.Sender, context);
            Assert.AreSame(validator.E, EventArgs.Empty);
        }

        [Test]
        public void TestEnded()
        {
            DomNode root = CreateTree();
            Validator validator = root.As<Validator>();
            ValidationContext context = root.As<ValidationContext>();
            context.RaiseBeginning();
            context.RaiseEnding();
            context.RaiseEnded();
            Assert.False(validator.IsValidating);
            Assert.AreSame(validator.Sender, context);
            Assert.AreSame(validator.E, EventArgs.Empty);
        }

        [Test]
        public void TestCancelled()
        {
            DomNode root = CreateTree();
            Validator validator = root.As<Validator>();
            ValidationContext context = root.As<ValidationContext>();
            context.RaiseBeginning();
            context.RaiseCancelled();
            Assert.False(validator.IsValidating);
            Assert.AreSame(validator.Sender, context);
            Assert.AreSame(validator.E, EventArgs.Empty);
        }

        [Test]
        public void TestSubscribeAndUnsubscribe()
        {
            DomNode root = new DomNode(m_rootType);
            Validator validator = root.As<Validator>();

            DomNode child = new DomNode(m_childType);
            DomNode grandchild = new DomNode(m_childType);
            child.SetChild(m_childInfo, grandchild);
            root.SetChild(m_childInfo, child);

            ValidationContext context = grandchild.As<ValidationContext>();
            context.RaiseBeginning();
            Assert.True(validator.IsValidating);
            Assert.AreSame(validator.Sender, context);
            Assert.AreSame(validator.E, EventArgs.Empty);
            context.RaiseEnded();

            root.SetChild(m_childInfo, null);
            context.RaiseBeginning();
            Assert.False(validator.IsValidating);
        }
    }
}
