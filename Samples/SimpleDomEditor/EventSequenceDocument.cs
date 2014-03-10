//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace SimpleDomEditorSample
{
    /// <summary>
    /// Document for managing event sequences</summary>
    public class EventSequenceDocument : DomDocument, ISearchableContext
    {
        /// <summary>
        /// Constructor</summary>
        public EventSequenceDocument()
        {
        }

        #region ISearchableContext Members

        /// <summary>
        /// Gets or sets the Control for displaying and choosing the search parameters to be used by the Search
        /// method</summary>
        public ISearchUI SearchUI 
        { 
            get { return m_searchUI; } 
            set 
            {
                // This might be bad form.  The 'set' for this property needs to be an ISearchUI, but I only
                // want 'get' to expose ISearchableContextUI, the base interface of ISearchUI.
                ISearchUI searchUI = (ISearchUI)value;
                if (searchUI == null)
                    throw new ArgumentException("SearchUI must only set to a class that implements ISearchUI");

                m_searchUI = searchUI; 
                m_searchUI.Bind(DomNode.As<DomNodeQueryable>()); 
            }
        }
        private ISearchUI m_searchUI;

        /// <summary>
        /// Gets or sets the Control for specifying replace parameters to be applied to the search results</summary>
        public IReplaceUI ReplaceUI 
        { 
            get { return m_replaceUI; }
            set
            {
                // This might be bad form.  The 'set' for this property needs to be an IReplaceUI, but I only
                // want 'get' to expose ISearchableContextUI, the base interface of IReplaceUI.
                IReplaceUI replaceUI = (IReplaceUI)value;
                if (replaceUI == null)
                    throw new ArgumentException("ReplaceUI must only set to a class that implements IReplaceUI");

                m_replaceUI = replaceUI;
                m_replaceUI.Bind(DomNode.As<DomNodeQueryable>());
            }
        }
        private IReplaceUI m_replaceUI;

        /// <summary>
        /// Gets or sets the Control for displaying and choosing the results of the search</summary>
        public IResultsUI ResultsUI 
        { 
            get { return m_resultsUI; } 
            set
            {
                // This might be bad form.  The 'set' for this property needs to be an IResultsUI, but I only
                // want 'get' to expose ISearchableContextUI, the base interface of IResultsUI.
                IResultsUI resultsUI = (IResultsUI)value;
                if (resultsUI == null)
                    throw new ArgumentException("ResultsUI must only set to a class that implements IResultsUI");

                m_resultsUI = resultsUI;
                m_resultsUI.Bind(DomNode.As<DomNodeQueryable>());
            }
        }
        private IResultsUI m_resultsUI;

        #endregion

        /// <summary>
        /// Gets a string identifying the type of the resource to the end-user</summary>
        public override string Type
        {
            get { return Editor.DocumentClientInfo.FileType; }
        }

        /// <summary>
        /// Raises the UriChanged event and performs custom processing</summary>
        /// <param name="e">Event args</param>
        protected override void OnUriChanged(UriChangedEventArgs e)
        {
            UpdateControlInfo();

            base.OnUriChanged(e);
        }

        /// <summary>
        /// Raises the DirtyChanged event and performs custom processing</summary>
        /// <param name="e">Event args</param>
        protected override void OnDirtyChanged(EventArgs e)
        {
            UpdateControlInfo();

            base.OnDirtyChanged(e);
        }

        private void UpdateControlInfo()
        {
            string filePath = Uri.LocalPath;
            string fileName = Path.GetFileName(filePath);
            if (Dirty)
                fileName += "*";

            EventSequenceContext context = this.As<EventSequenceContext>();
            context.ControlInfo.Name = fileName;
            context.ControlInfo.Description = filePath;
        }
    }
}
