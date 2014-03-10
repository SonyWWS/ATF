//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Attribute to mark fields, for automatic loading of Cursors by ResourceUtil</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class CursorResourceAttribute : Attribute
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="cursorName">Cursor name</param>
        public CursorResourceAttribute(string cursorName)
        {
            m_cursorName = cursorName;
        }

        /// <summary>
        /// Gets the Cursor name</summary>
        public string CursorName
        {
            get { return m_cursorName; }
        }

        private readonly string m_cursorName;
    }
}
