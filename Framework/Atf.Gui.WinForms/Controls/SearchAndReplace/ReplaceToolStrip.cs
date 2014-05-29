//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// ToolStrip GUI for specifying a replacement pattern on DomNode parameter value matches</summary>
    public abstract class ReplaceToolStrip : ToolStrip, IReplaceUI
    {
        /// <summary>
        /// Constructor</summary>
        public ReplaceToolStrip() { }

        /// <summary>
        /// Constructor with IQueryableReplaceContext</summary>
        /// <param name="replacableContext">IQueryableReplaceContext for replacement pattern</param>
        public ReplaceToolStrip(IQueryableReplaceContext replacableContext)
        {
            Bind(replacableContext);
        }

        #region IReplaceControl members
        /// <summary>
        /// Binds this replace Control to a data set (that is wrapped in a class implementing IQueryableReplaceContext)</summary>
        /// <param name="replaceableContext">The replaceable context object, or null.</param>
        public void Bind(IQueryableReplaceContext replaceableContext)
        {
            m_replaceableContext = replaceableContext;
            Enabled = (replaceableContext != null);
        }

        /// <summary>
        /// Gets actual client-defined GUI Control</summary>
        public Control Control { get { return this; } }

        /// <summary>
        /// Event raised by client when UI has graphically changed</summary>
        public abstract event EventHandler UIChanged;
        #endregion

        /// <summary>
        /// Returns an object that defines how search matches are modified in replacement</summary>
        /// <returns>Object that defines how search matches are modified in replacement</returns>
        public abstract object GetReplaceInfo();

        /// <summary>
        /// Performs the pattern replace, using the search and replace parameters from search Control and replace Control</summary>
        public virtual void DoReplace()
        {
            if (m_replaceableContext != null)
                m_replaceableContext.Replace(GetReplaceInfo());
        }

        private IQueryableReplaceContext m_replaceableContext;
    }
}

