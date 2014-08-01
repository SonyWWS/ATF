//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using NUnit.Framework;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestTransactionReporter : TestValidator
    {
        public TestTransactionReporter()
        {
            RootType.Define(new ExtensionInfo<TransactionReporter>());
            RootType.Define(new ExtensionInfo<TransactionContext>());
        }

        [SetUp]
        public void SetupBeforeEachTest()
        {
            m_events = new List<EventArgs>();
            m_root = CreateTree();
            m_transactionContext = m_root.Cast<TransactionContext>();
            m_reporter = m_root.Cast<TransactionReporter>();
            m_reporter.TransactionFinishedAttributeChanged += ReporterOnTransactionFinishedAttributeChanged;
            m_reporter.TransactionFinishedChildInserted += ReporterOnTransactionFinishedChildInserted;
            m_reporter.TransactionFinishedChildRemoved += ReporterOnTransactionFinishedChildRemoved;
        }

        [Test]
        public void TestAttributeChangesNoTransaction()
        {
            DomNode child = m_root.GetChild(ChildInfo);
            DomNode grandchild = child.GetChild(ChildInfo);
            grandchild.SetAttribute(StringAttrInfo, "foo1");
            grandchild.SetAttribute(StringAttrInfo, "foo2");
            grandchild.SetAttribute(StringAttrInfo, "foo3");

            // Because there's no transaction, each DOM change event should yield a TransactionFinishedXxx event.
            Assert.IsTrue(m_events.Count == 3);
            CheckAttributeEvent(m_events[0], grandchild, "", "foo1");
            CheckAttributeEvent(m_events[1], grandchild, "foo1", "foo2");
            CheckAttributeEvent(m_events[2], grandchild, "foo2", "foo3");
        }

        [Test]
        public void TestDomChangesNoTransaction()
        {
            DomNode child = m_root.GetChild(ChildInfo);
            DomNode grandchild = child.GetChild(ChildInfo);
            grandchild.SetAttribute(StringAttrInfo, "foo1");
            grandchild.SetAttribute(StringAttrInfo, "foo2");
            grandchild.SetAttribute(StringAttrInfo, "foo3");
            child.RemoveFromParent();
            m_root.SetChild(ChildInfo, child);
            grandchild.SetAttribute(StringAttrInfo, "foo4");

            // Because there's no transaction, each DOM change event should yield a TransactionFinishedXxx event.
            Assert.IsTrue(m_events.Count == 6);
            CheckAttributeEvent(m_events[0], grandchild, "", "foo1");
            CheckAttributeEvent(m_events[1], grandchild, "foo1", "foo2");
            CheckAttributeEvent(m_events[2], grandchild, "foo2", "foo3");
            CheckChildRemovedEvent(m_events[3], m_root, child);
            CheckChildInsertedEvent(m_events[4], m_root, child);
            CheckAttributeEvent(m_events[5], grandchild, "foo3", "foo4");
        }

        [Test]
        public void TestAttributeChangesInTransaction()
        {
            DomNode child = m_root.GetChild(ChildInfo);
            DomNode grandchild = child.GetChild(ChildInfo);

            // Combine 3 attribute change events into one.
            m_transactionContext.DoTransaction(() =>
            {
                grandchild.SetAttribute(StringAttrInfo, "foo1");
                grandchild.SetAttribute(StringAttrInfo, "foo2");
                grandchild.SetAttribute(StringAttrInfo, "foo3");
            }, "test transaction");

            Assert.IsTrue(m_events.Count == 1);
            CheckAttributeEvent(m_events[0], grandchild, "", "foo3");

            // Make sure that TransactionReporter clears its internal state correctly.
            // Test that multiple attribute change events that leave the original value
            //  equal to the final value do not raise a final attribute change event.
            grandchild.SetAttribute(StringAttrInfo, "foo1");
            m_events.Clear();
            m_transactionContext.DoTransaction(() =>
            {
                grandchild.SetAttribute(StringAttrInfo, "foo2");
                grandchild.SetAttribute(StringAttrInfo, "foo1");
            }, "test transaction 2");

            Assert.IsTrue(m_events.Count == 0);
        }

        [Test]
        public void TestHierarchicalAttributeChangesInTransaction()
        {
            // Test that attribute changes are reported correctly whether the AttributeInfo
            //  is used from the base DomNodeType or from a derived DomNodeType.
            AttributeInfo baseInfo = m_root.Type.BaseType.GetAttributeInfo("string");
            AttributeInfo derivedInfo = m_root.Type.GetAttributeInfo("string");

            // Make sure the test code is working!
            Assert.IsTrue(derivedInfo.OwningType != baseInfo.OwningType);
            Assert.IsTrue(derivedInfo.DefiningType == baseInfo.DefiningType);

            // Combine 3 attribute change events into one.
            m_transactionContext.DoTransaction(() =>
            {
                m_root.SetAttribute(baseInfo, "foo1");
                m_root.SetAttribute(derivedInfo, "foo2");
                m_root.SetAttribute(derivedInfo, "foo3");
            }, "test transaction");

            Assert.IsTrue(m_events.Count == 1);
            CheckAttributeEvent(m_events[0], m_root, "", "foo3");

            // Make sure that TransactionReporter clears its internal state correctly.
            // Test that multiple attribute change events that leave the original value
            //  equal to the final value do not raise a final attribute change event.
            m_root.SetAttribute(derivedInfo, "foo1");
            m_events.Clear();
            m_transactionContext.DoTransaction(() =>
            {
                m_root.SetAttribute(derivedInfo, "foo2");
                m_root.SetAttribute(baseInfo, "foo1");
            }, "test transaction 2");

            Assert.IsTrue(m_events.Count == 0);
        }

        [Test]
        public void TestDomChangesInTransaction()
        {
            DomNode child = m_root.GetChild(ChildInfo);
            DomNode grandchild = child.GetChild(ChildInfo);

            // Remove and then add the parent of the DomNode whose attributes get changed.
            m_transactionContext.DoTransaction(() =>
            {
                grandchild.SetAttribute(StringAttrInfo, "foo1");
                grandchild.SetAttribute(StringAttrInfo, "foo2");
                grandchild.SetAttribute(StringAttrInfo, "foo3");
                child.RemoveFromParent();
                m_root.SetChild(ChildInfo, child);
                grandchild.SetAttribute(StringAttrInfo, "foo4");
            }, "test transaction");

            Assert.IsTrue(m_events.Count == 2);
            CheckChildRemovedEvent(m_events[0], m_root, child);
            CheckChildInsertedEvent(m_events[1], m_root, child);
        }

        [Test]
        public void TestAddChildAndModifyInTransaction()
        {
            DomNode child = m_root.GetChild(ChildInfo);
            DomNode grandchild = child.GetChild(ChildInfo);
            var greatGrandchild = new DomNode(ChildType, ChildInfo);

            // Add a great-grandchild and then modify it. The attribute changed events should be ignored.
            m_transactionContext.DoTransaction(() =>
            {
                grandchild.SetChild(ChildInfo, greatGrandchild);
                greatGrandchild.SetAttribute(StringAttrInfo, "foo1");
            }, "test transaction 1");

            Assert.IsTrue(m_events.Count == 1);
            CheckChildInsertedEvent(m_events[0], grandchild, greatGrandchild);

            // Make sure the TransactionReporter's state gets cleared for the next transaction.
            m_events.Clear();
            m_transactionContext.DoTransaction(() =>
            {
                grandchild.SetChild(ChildInfo, null);//remove great-grandchild
                greatGrandchild.SetChild(ChildInfo, new DomNode(ChildType));
                grandchild.SetChild(ChildInfo, greatGrandchild);//insert great-grandchild (and its child)
                greatGrandchild.SetAttribute(StringAttrInfo, "foo2");
                greatGrandchild.GetChild(ChildInfo).SetAttribute(StringAttrInfo, "child foo2");
            }, "test transaction 2");
            Assert.IsTrue(m_events.Count == 2);
            CheckChildRemovedEvent(m_events[0], grandchild, greatGrandchild);
            CheckChildInsertedEvent(m_events[1], grandchild, greatGrandchild);

            // This time, make sure that removing the child of a newly inserted tree doesn't generate new events.
            greatGrandchild.RemoveFromParent();
            var great2Grandchild = new DomNode(ChildType);
            greatGrandchild.SetChild(ChildInfo, great2Grandchild);
            m_events.Clear();
            m_transactionContext.DoTransaction(() =>
            {
                grandchild.SetChild(ChildInfo, greatGrandchild);//insert great-grandchild and great2Grandchild
                greatGrandchild.SetAttribute(StringAttrInfo, "foo3");
                great2Grandchild.SetAttribute(StringAttrInfo, "foo3");
                great2Grandchild.RemoveFromParent();
            }, "test transaction 3");
            Assert.IsTrue(m_events.Count == 1);
            CheckChildInsertedEvent(m_events[0], grandchild, greatGrandchild);

            // This time, make sure that removing two children of a newly inserted tree doesn't generate new events.
            grandchild.SetChild(ChildInfo, null);
            greatGrandchild.SetChild(ChildInfo, great2Grandchild);
            var great3Grandchild = new DomNode(ChildType);
            great2Grandchild.SetChild(ChildInfo, great3Grandchild);
            m_events.Clear();
            m_transactionContext.DoTransaction(() =>
            {
                grandchild.SetChild(ChildInfo, greatGrandchild);
                greatGrandchild.SetAttribute(StringAttrInfo, "foo4");
                great2Grandchild.SetAttribute(StringAttrInfo, "foo4");
                great3Grandchild.SetAttribute(StringAttrInfo, "foo4");
                great3Grandchild.RemoveFromParent();
                great2Grandchild.RemoveFromParent();
            }, "test transaction 4");
            Assert.IsTrue(m_events.Count == 1);
            CheckChildInsertedEvent(m_events[0], grandchild, greatGrandchild);

            // Check that removing all the inserted children generates no events.
            grandchild.SetChild(ChildInfo, null);
            greatGrandchild.SetChild(ChildInfo, great2Grandchild);
            great2Grandchild.SetChild(ChildInfo, great3Grandchild);
            m_events.Clear();
            m_transactionContext.DoTransaction(() =>
            {
                grandchild.SetChild(ChildInfo, greatGrandchild);
                greatGrandchild.SetAttribute(StringAttrInfo, "foo5");
                great2Grandchild.SetAttribute(StringAttrInfo, "foo5");
                great3Grandchild.SetAttribute(StringAttrInfo, "foo5");
                great3Grandchild.RemoveFromParent();
                great2Grandchild.RemoveFromParent();
                greatGrandchild.RemoveFromParent();
            }, "test transaction 5");
            Assert.IsTrue(m_events.Count == 0);
        }

        [Test]
        public void TestReparentInTransaction()
        {
            DomNode a = m_root.GetChild(ChildInfo);
            DomNode b = a.GetChild(ChildInfo);
            b.RemoveFromParent();

            // root -- a  ==>
            // root -- b -- a
            m_events.Clear();
            m_transactionContext.DoTransaction(() =>
            {
                a.SetAttribute(StringAttrInfo, "foo1");
                a.RemoveFromParent();
                m_root.SetChild(ChildInfo, b);
                b.SetChild(ChildInfo, a);
                b.SetAttribute(StringAttrInfo, "foo1");
            }, "test transaction 1");

            Assert.IsTrue(m_events.Count == 2);
            CheckChildRemovedEvent(m_events[0], m_root, a);
            CheckChildInsertedEvent(m_events[1], m_root, b);
        }

        private void CheckAttributeEvent(EventArgs eventArgs, DomNode domNode, string oldValue, string newValue)
        {
            var attrArgs = eventArgs as AttributeEventArgs;
            Assert.NotNull(attrArgs);
            Assert.AreSame(attrArgs.DomNode, domNode);
            Assert.AreSame(attrArgs.OldValue, oldValue);
            Assert.AreSame(attrArgs.NewValue, newValue);
        }

        private void CheckChildInsertedEvent(EventArgs eventArgs, DomNode parent, DomNode child)
        {
            var childArgs = eventArgs as ChildInsertedEventArgs;
            Assert.NotNull(childArgs);
            Assert.AreSame(childArgs.Parent, parent);
            Assert.AreSame(childArgs.Child, child);
        }

        private void CheckChildRemovedEvent(EventArgs eventArgs, DomNode parent, DomNode child)
        {
            var childArgs = eventArgs as ChildRemovedEventArgs;
            Assert.NotNull(childArgs);
            Assert.AreSame(childArgs.Parent, parent);
            Assert.AreSame(childArgs.Child, child);
        }

        private void ReporterOnTransactionFinishedAttributeChanged(object sender, AttributeEventArgs attributeEventArgs)
        {
            Assert.AreSame(sender, m_reporter);
            m_events.Add(attributeEventArgs);
        }

        private void ReporterOnTransactionFinishedChildInserted(object sender, ChildEventArgs childEventArgs)
        {
            Assert.AreSame(sender, m_reporter);
            m_events.Add(new ChildInsertedEventArgs(childEventArgs));
        }

        private void ReporterOnTransactionFinishedChildRemoved(object sender, ChildEventArgs childEventArgs)
        {
            Assert.AreSame(sender, m_reporter);
            m_events.Add(new ChildRemovedEventArgs(childEventArgs));
        }

        private class ChildInsertedEventArgs : ChildEventArgs
        {
            public ChildInsertedEventArgs(ChildEventArgs e) : base(e.Parent, e.ChildInfo, e.Child, e.Index)
            {
            }
        }

        private class ChildRemovedEventArgs : ChildEventArgs
        {
            public ChildRemovedEventArgs(ChildEventArgs e)
                : base(e.Parent, e.ChildInfo, e.Child, e.Index)
            {
            }
        }

        private List<EventArgs> m_events;
        private DomNode m_root;
        private TransactionReporter m_reporter;
        private TransactionContext m_transactionContext;
    }
}
