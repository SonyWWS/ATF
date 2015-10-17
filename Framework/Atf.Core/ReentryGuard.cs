//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// A utility for more safely and conveniently protecting against reentering code that
    /// is not designed to be reentrant. Allows the use of the 'using' statement to simplify
    /// code somewhat, as compared to using try-finally blocks.
    /// 
    /// Also allows for the situation where reentry is permissible, but different code paths must
    /// be taken. Use the EnterAndExitMultiple method for this situation.</summary>
    /// <example>
    /// void foo()
    /// {
    ///     if (!m_guard.CanEnter)
    ///         return;
    ///     using( m_guard.EnterAndExit() )
    ///     {
    ///         //do your stuff that may cause this function or other functions to be reentered.
    ///     }
    /// }
    /// void foo2()
    /// {
    ///     if (m_guard.HasEntered)
    ///     { //do some special stuff
    ///     }
    ///     using( m_guard.EnterAndExitMultiple() )
    ///     {
    ///         //reentry is allowed.
    ///     }
    /// }
    /// private ReentryGuard m_guard = new ReentryGuard();
    /// </example>
    public sealed class ReentryGuard
    {
        /// <summary>
        /// Gets a value indicating whether the function or program block can be safely entered. Test this before the 'using'
        /// block that surrounds the call to EnterAndExit.</summary>
        public bool CanEnter
        {
            get { return m_entryCount == 0; }
        }

        /// <summary>
        /// Gets a value indicating whether the function or program block has been safely entered. </summary>
        /// <returns><c>True</c> if the program's flow has entered the 'using' block.</returns>
        public bool HasEntered
        {
            get { return m_entryCount > 0; }
        }

        /// <summary>
        /// Until the 'using' block has finished, CanEnter is 'false'</summary>
        /// <returns>A temporary object to be used in a 'using' block</returns>
        public IDisposable EnterAndExit()
        {
            m_allowReentry = false;
            return new UsingBlock(this);
        }

        /// <summary>
        /// Until the 'using' block has finished, CanEnter is <c>false</c>. The number of times
        /// entry and exit has occurred is tracked. No exception is thrown if reentry occurs.</summary>
        /// <returns>A temporary object to be used in a 'using' block</returns>
        public IDisposable EnterAndExitMultiple()
        {
            m_allowReentry = true;
            return new UsingBlock(this);
        }

        private class UsingBlock : IDisposable
        {
            private readonly ReentryGuard m_owner;
            public UsingBlock(ReentryGuard guard)
            {
                m_owner = guard;
                if (m_owner.m_allowReentry==false && m_owner.m_entryCount > 0)
                    throw new InvalidOperationException(
                        "This function or statement block is not allowed to be reentered.\n" +
                        "Make sure that CanEnter is checked first and that EnterAndExit() is\n" +
                        "called within a 'using' block.");
                m_owner.m_entryCount++;
            }
            public void Dispose()
            {
                m_owner.m_entryCount--;
            }
        }

        private int m_entryCount;
        private bool m_allowReentry;
    }
}
