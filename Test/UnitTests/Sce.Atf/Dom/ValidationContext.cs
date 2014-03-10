//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf;
using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    /// <summary>
    /// Simple validation context to test validators
    /// </summary>
    public class ValidationContext : DomNodeAdapter, IValidationContext
    {
        public void RaiseBeginning()
        {
            Beginning.Raise(this, EventArgs.Empty);
        }

        public event EventHandler Beginning;

        public void RaiseCancelled()
        {
            Cancelled.Raise(this, EventArgs.Empty);
        }

        public event EventHandler Cancelled;

        public void RaiseEnding()
        {
            Ending.Raise(this, EventArgs.Empty);
        }

        public event EventHandler Ending;

        public void RaiseEnded()
        {
            Ended.Raise(this, EventArgs.Empty);
        }

        public event EventHandler Ended;
    }
}
