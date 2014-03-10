//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    /// <summary>
    /// Simple observable DOM validator</summary>
    public class Validator : Sce.Atf.Dom.Validator
    {
        public object Sender;
        public EventArgs E;

        public bool IsValidating
        {
            get { return Validating; }
        }

        protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
            Sender = sender;
            E = e;
            base.OnAttributeChanged(sender, e);
        }

        protected override void OnChildInserted(object sender, ChildEventArgs e)
        {
            Sender = sender;
            E = e;
            base.OnChildInserted(sender, e);
        }

        protected override void OnChildRemoved(object sender, ChildEventArgs e)
        {
            Sender = sender;
            E = e;
            base.OnChildRemoved(sender, e);
        }

        protected override void OnBeginning(object sender, EventArgs e)
        {
            Sender = sender;
            E = e;
            base.OnBeginning(sender, e);
        }

        protected override void OnEnding(object sender, EventArgs e)
        {
            Sender = sender;
            E = e;
            base.OnEnding(sender, e);
        }

        protected override void OnCancelled(object sender, EventArgs e)
        {
            Sender = sender;
            E = e;
            base.OnCancelled(sender, e);
        }

        protected override void OnEnded(object sender, EventArgs e)
        {
            Sender = sender;
            E = e;
            base.OnEnded(sender, e);
        }
    }
}
