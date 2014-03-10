//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Dom;

namespace StatechartEditorSample
{
    /// <summary>
    /// Adapts the Statechart to IDocument and synchronizes URI and dirty bit changes to the
    /// ControlInfo instance used to register the viewing control in the UI.</summary>
    public class Document : DomDocument, IAnnotatedDiagram
    {
        /// <summary>
        /// Gets or sets ControlInfo. Set by editor.</summary>
        public ControlInfo ControlInfo
        {
            get { return m_controlInfo; }
            set { m_controlInfo = value; }
        }

        /// <summary>
        /// Gets a list of document Annotations</summary>
        public IList<IAnnotation> Annotations
        {
            get { return GetChildList<IAnnotation>(Schema.statechartDocumentType.annotationChild); }
        }

        #region IAnnotatedDiagram Members

        /// <summary>
        /// Gets the sequence of annotations in the context</summary>
        IEnumerable<IAnnotation> IAnnotatedDiagram.Annotations
        {
            get { return Annotations; }
        }

        #endregion

        /// <summary>
        /// Gets the document client's file type name</summary>
        public override string Type
        {
            get { return Editor.EditorInfo.FileType; }
        }

        /// <summary>
        /// Raises the UriChanged event and performs custom processing</summary>
        /// <param name="e">UriChangedEventArgs containing event data</param>
        protected override void OnUriChanged(UriChangedEventArgs e)
        {
            UpdateControlInfo();

            base.OnUriChanged(e);
        }

        /// <summary>
        /// Raises the DirtyChanged event and performs custom processing</summary>
        /// <param name="e">EventArgs containing event data</param>
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

            m_controlInfo.Name = fileName;
            m_controlInfo.Description = filePath;
        }

        private ControlInfo m_controlInfo;
    }
}
