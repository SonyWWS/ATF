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
        protected DomNodeType ChildType;
        protected AttributeInfo StringAttrInfo;//defined on ChildType
        protected AttributeInfo RefAttrInfo;//defined on ChildType
        protected ChildInfo ChildInfo;//defined on ChildType
        
        protected DomNodeType RootType;//derives from ChildType

        public TestValidator()
        {
            // define a tree of validation contexts
            ChildType = new DomNodeType("test");
            StringAttrInfo = GetStringAttribute("string");
            ChildType.Define(StringAttrInfo);
            RefAttrInfo = GetRefAttribute("ref");
            ChildType.Define(RefAttrInfo);
            ChildInfo = new ChildInfo("child", ChildType);
            ChildType.Define(ChildInfo);
            ChildType.Define(new ExtensionInfo<ValidationContext>());

            // define a distinct root type with the validator
            RootType = new DomNodeType("root");
            RootType.BaseType = ChildType;
            RootType.Define(new ExtensionInfo<Validator>());
            AttributeInfo overriddenInfo = GetStringAttribute("string");
            RootType.Define(overriddenInfo);

            IEnumerable<AttributeInfo> attributes = RootType.Attributes; // freezes the types
        }

        protected DomNode CreateTree()
        {
            DomNode root = new DomNode(RootType);
            DomNode child = new DomNode(ChildType);
            DomNode grandchild = new DomNode(ChildType);
            child.SetChild(ChildInfo, grandchild);
            root.SetChild(ChildInfo, child);
            return root;
        }

        [Test]
        public void TestOnAttributeSet()
        {
            DomNode root = CreateTree();
            Validator validator = root.As<Validator>();
            DomNode grandchild = root.GetChild(ChildInfo).GetChild(ChildInfo);
            grandchild.SetAttribute(StringAttrInfo, "foo");
            Assert.AreSame(validator.Sender, root);
            AttributeEventArgs e = (AttributeEventArgs)validator.E;
            Assert.NotNull(e);
            Assert.AreSame(e.DomNode, grandchild);
        }

        [Test]
        public void TestOnChildInserted()
        {
            DomNode root = new DomNode(RootType);
            Validator validator = root.As<Validator>();
            DomNode child = new DomNode(ChildType);
            root.SetChild(ChildInfo, child);
            Assert.AreSame(validator.Sender, root);
            ChildEventArgs e = (ChildEventArgs)validator.E;
            Assert.NotNull(e);
            Assert.AreSame(e.Parent, root);
        }

        [Test]
        public void TestOnChildRemoved()
        {
            DomNode root = new DomNode(RootType);
            Validator validator = root.As<Validator>();
            DomNode child = new DomNode(ChildType);
            root.SetChild(ChildInfo, child);
            root.SetChild(ChildInfo, null);
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
            DomNode root = new DomNode(RootType);
            Validator validator = root.As<Validator>();

            DomNode child = new DomNode(ChildType);
            DomNode grandchild = new DomNode(ChildType);
            child.SetChild(ChildInfo, grandchild);
            root.SetChild(ChildInfo, child);

            ValidationContext context = grandchild.As<ValidationContext>();
            context.RaiseBeginning();
            Assert.True(validator.IsValidating);
            Assert.AreSame(validator.Sender, context);
            Assert.AreSame(validator.E, EventArgs.Empty);
            context.RaiseEnded();

            root.SetChild(ChildInfo, null);
            context.RaiseBeginning();
            Assert.False(validator.IsValidating);
        }
    }
}
