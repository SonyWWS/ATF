//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// GUI for specifying a search on any data set wrapped within a class that implements IQueryableReplaceContext</summary>
    public class ReplaceTextBox : TextBox, IReplaceUI
    {
        /// <summary>
        /// Constructor</summary>
        public ReplaceTextBox()
        {
            // Suppress "unused variable" warning
            if (UIChanged != null) {}

            // Execute replacement on press of return key
            KeyDown += textBox_KeyDown;
        }

        #region IReplaceControl members
        /// <summary>
        /// Binds this ReplaceControl to a data set (that is wrapped in a class implementing IQueryableReplaceContext)</summary>
        /// <param name="replaceableContext">The queryable context, or null</param>
        public void Bind(IQueryableReplaceContext replaceableContext)
        {
            ReplaceableContext = replaceableContext;
            Enabled = (replaceableContext != null);
        }

        /// <summary>
        /// Gets actual client-defined GUI Control</summary>
        public Control Control { get { return this; } }

        /// <summary>
        /// Event raised by client when UI has graphically changed</summary>
        public event EventHandler UIChanged;
        #endregion

        /// <summary>
        /// Performs custom actions on TextBox KeyDown events</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Key event args</param>
        protected virtual void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    DoReplace();
                    break;
            }
        }

        /// <summary>
        /// Returns an object that defines how search matches are modified in replacement</summary>
        /// <returns>Object that defines how search matches are modified in replacement</returns>
        public virtual object GetReplaceInfo()
        {
            return Text;
        }

        /// <summary>
        /// Performs the pattern replace, using the search and replace parameters from search Control and replace Control</summary>
        public void DoReplace()
        {
            if (ReplaceableContext != null)
                ReplaceableContext.Replace(GetReplaceInfo());
        }

        /// <summary>
        /// Gets the IQueryableReplaceContext</summary>
        protected IQueryableReplaceContext ReplaceableContext { get; private set; }
    }
}

