//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    public class DomTest
    {
        public static bool Equals(AttributeEventArgs e1, AttributeEventArgs e2)
        {
            if (e1 == null || e2 == null)
                return (e1 == e2);
            return
                e1.DomNode == e2.DomNode &&
                e1.AttributeInfo == e2.AttributeInfo &&
                object.Equals(e1.OldValue, e2.OldValue) &&
                object.Equals(e1.NewValue, e2.NewValue);
        }

        public static bool Equals(ChildEventArgs e1, ChildEventArgs e2)
        {
            if (e1 == null || e2 == null)
                return (e1 == e2);
            return
                e1.Parent == e2.Parent &&
                e1.ChildInfo == e2.ChildInfo &&
                e1.Child == e2.Child &&
                e1.Index == e2.Index;
        }

        protected class DomNodeListener
        {
            public DomNodeListener(DomNode domNode)
            {
                DomNode = domNode;
                domNode.AttributeChanging += domNode_AttributeChanging;
                domNode.AttributeChanged += domNode_AttributeChanged;
                domNode.ChildInserting += domNode_ChildInserting;
                domNode.ChildInserted += domNode_ChildInserted;
                domNode.ChildRemoving += domNode_ChildRemoving;
                domNode.ChildRemoved += domNode_ChildRemoved;
            }

            public DomNode DomNode;
            public AttributeEventArgs AttributeChangingArgs;
            public AttributeEventArgs AttributeChangedArgs;
            public ChildEventArgs ChildInsertingArgs;
            public ChildEventArgs ChildInsertedArgs;
            public ChildEventArgs ChildRemovingArgs;
            public ChildEventArgs ChildRemovedArgs;

            private void domNode_AttributeChanging(object sender, AttributeEventArgs e)
            {
                AttributeChangingArgs = e;
            }

            private void domNode_AttributeChanged(object sender, AttributeEventArgs e)
            {
                AttributeChangedArgs = e;
            }

            private void domNode_ChildInserting(object sender, ChildEventArgs e)
            {
                ChildInsertingArgs = e;
            }

            private void domNode_ChildInserted(object sender, ChildEventArgs e)
            {
                ChildInsertedArgs = e;
            }

            private void domNode_ChildRemoving(object sender, ChildEventArgs e)
            {
                ChildRemovingArgs = e;
            }

            private void domNode_ChildRemoved(object sender, ChildEventArgs e)
            {
                ChildRemovedArgs = e;
            }
        }

        protected class SimpleAttributeRule : AttributeRule
        {
            public override bool Validate(object value, AttributeInfo info)
            {
                Validated = true;
                return true;
            }
            public bool Validated;
        }

        protected class SimpleChildRule : ChildRule
        {
            public override bool Validate(DomNode parent, DomNode child, ChildInfo childInfo)
            {
                Validated = true;
                return true;
            }
            public bool Validated;
        }

        protected AttributeInfo GetIntAttribute(string name)
        {
            return new AttributeInfo(name, AttributeType.IntType);
        }

        protected AttributeInfo GetStringAttribute(string name)
        {
            return new AttributeInfo(name, AttributeType.StringType);
        }

        protected AttributeInfo GetRefAttribute(string name)
        {
            return new AttributeInfo(name, new AttributeType("ref", typeof(DomNode)));
        }
    }
}
