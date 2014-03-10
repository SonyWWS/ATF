//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Applications;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// A simple TextBox GUI for specifying a DomNode name search</summary>
    public class DomNodeNameSearchTextBox : SearchTextBox
    {
        /// <summary>
        /// Constructor</summary>
        public DomNodeNameSearchTextBox()
        {
            m_predicate = new DomNodeNamePredicate();

            // Suppress warnings about unused variable
            if (UIChanged == null) {}
        }

        /// <summary>
        /// Performs custom actions on TextChanged events</summary>
        /// <param name="e">Event args</param>
        protected override void OnTextChanged(EventArgs e)
        {
            DoSearch();
        }

        /// <summary>
        /// Event raised by client when UI has graphically changed</summary>
        public override event EventHandler UIChanged;

        /// <summary>
        /// Returns a DomNodeNamePredicate specifying the input text as the string to match</summary>
        public override IQueryPredicate GetPredicate()
        {
            m_predicate.StringToMatch = Text;
            return m_predicate;
        }

        private readonly DomNodeNamePredicate m_predicate;
    }
}

