//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Command that changes selection in a selection context</summary>
    public class SetSelectionCommand : Command
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="selectionContext">Selection context. See <see cref="ISelectionContext"/>.</param>
        /// <param name="nextSelection">Selection's next state</param>
        /// <remarks>The selection is assumed to be holding the previous state</remarks>
        public SetSelectionCommand(
            ISelectionContext selectionContext,
            IEnumerable<object> nextSelection)
            : this(selectionContext, selectionContext.Selection, nextSelection)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="selectionContext">Selection context. See <see cref="ISelectionContext"/>.</param>
        /// <param name="previous">Selection's previous state</param>
        /// <param name="next">Selection's next state</param>
        public SetSelectionCommand(
            ISelectionContext selectionContext,
            IEnumerable<object> previous,
            IEnumerable<object> next)
            : base("Set Selection".Localize())
        {
            if (selectionContext == null)
                throw new ArgumentNullException("selectionContext");

            m_selectionContext = selectionContext;

            m_previous = Snapshot(previous);
            // if previous is the same as next, then use the same array
            if (previous.SequenceEqual(next))
                m_next = m_previous;
            else
                m_next = Snapshot(next);
        }

        /// <summary>
        /// Does/Redoes the command</summary>
        public override void Do()
        {
            m_selectionContext.SetRange(m_next);
        }

        /// <summary>
        /// Undoes the command</summary>
        public override void Undo()
        {
            m_selectionContext.SetRange(m_previous);
        }

        private IEnumerable<object> Snapshot(IEnumerable<object> enumerable)
        {
            if (enumerable != null)
                return enumerable.ToArray();

            return EmptyEnumerable<object>.Instance;
        }

        private readonly ISelectionContext m_selectionContext;
        private readonly IEnumerable<object> m_previous;
        private readonly IEnumerable<object> m_next;
    }
}

