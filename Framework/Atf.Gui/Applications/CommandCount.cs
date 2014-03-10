//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// A class to maintain a command count</summary>
    public class CommandCount
    {
        /// <summary>
        /// Constructor</summary>
        public CommandCount()
        {
            Reset();
        }

        /// <summary>
        /// Resets count to initial state</summary>
        public void Reset()
        {
            bool oldState = Dirty;
            m_clean = m_current = 0;
            m_forceDirty = false;
            CheckDirtyChanged(oldState);
        }

        /// <summary>
        /// Gets the current command count</summary>
        public int Current
        {
            get { return m_current; }
        }

        /// <summary>
        /// Gets whether command count can be decremented</summary>
        public bool CanDecrement
        {
            get { return m_current > 0; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command count is at its "clean" point</summary>
        /// <remarks>Setting this property to false sets the "clean" point. Setting
        /// it to true forces a "dirty" state until the next time Dirty is set to false.</remarks>
        public bool Dirty
        {
            get
            {
                return
                      m_forceDirty ||
                      (m_clean != m_current);
            }
            set
            {
                bool oldState = Dirty;

                if (value == false)
                {
                    m_clean = m_current;
                    m_forceDirty = false;
                }
                else
                {
                    m_forceDirty = true;
                }

                CheckDirtyChanged(oldState);
            }
        }

        /// <summary>
        /// Event that is raised when the Dirty state changes</summary>
        public event EventHandler DirtyChanged;

        /// <summary>
        /// Increments the command count</summary>
        public void Increment()
        {
            bool oldState = Dirty;
            m_current++;
            CheckDirtyChanged(oldState);
        }

        /// <summary>
        /// Decrements the command count</summary>
        public void Decrement()
        {
            if (!CanDecrement)
                throw new InvalidOperationException("Can't decrement");

            bool oldState = Dirty;
            m_current--;
            CheckDirtyChanged(oldState);
        }

        /// <summary>
        /// Reverts the command count by undoing or redoing to the "clean" point</summary>
        /// <param name="history">Command history to undo or redo</param>
        /// <remarks>Use this method to revert in a multi-document, single command history scenario</remarks>
        public void Revert(CommandHistory history)
        {
            // need to undo?
            while (m_clean < m_current)
            {
                if (!history.CanUndo)
                    break;
                history.Undo();
            }

            // need to redo?
            while (m_clean > m_current)
            {
                if (!history.CanRedo)
                    break;
                history.Redo();
            }
        }

        private void CheckDirtyChanged(bool oldState)
        {
            if (oldState != Dirty)
            {
                DirtyChanged.Raise(this, EventArgs.Empty);
            }
        }

        private int m_current;
        private int m_clean;
        private bool m_forceDirty;
    }
}
