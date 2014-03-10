//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;

using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapts the circuit to IDocument and synchronizes URI and dirty bit changes to the
    /// ControlInfo instance used to register the viewing control in the UI</summary>
    public abstract class CircuitDocument : DomDocument
    {
        // required  child info
        protected abstract ChildInfo SubCircuitChildInfo { get; }
      
        /// <summary>
        /// Performs initialization when the adapter is connected to the circuit's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            var documentClientInfo = DomNode.Type.GetTag<DocumentClientInfo>();
            if (documentClientInfo != null)
                SetEditorFileType(documentClientInfo.FileType);
            m_subCircuits = new DomNodeListAdapter<SubCircuit>(DomNode, SubCircuitChildInfo);

            base.OnNodeSet();
        }

        /// <summary>
        /// Gets or sets ControlInfo. Set by editor.</summary>
        public ControlInfo ControlInfo
        {
            get { return m_controlInfo; }
            set { m_controlInfo = value; }
        }

        /// <summary>
        /// Gets the circuit's sub-circuits</summary>
        public IList<SubCircuit> SubCircuits
        {
            get { return m_subCircuits; }
        }

        /// <summary>
        /// Gets the document client's user-readable file type name</summary>
        public override string Type
        {
            get { return m_editorFileType; }
        }

        /// <summary>
        /// Sets the user-readable (and thus, localizable) circuit document file type name,
        /// which can be read using the Type property.</summary>
        /// <param name="fileType"></param>
        public void SetEditorFileType(string fileType)
        {
            m_editorFileType = fileType;
        }


        /// <summary>
        /// Raises the DirtyChanged event and performs custom processing</summary>
        /// <param name="e">EventArgs containing event data</param>
        protected override void OnDirtyChanged(EventArgs e)
        {
            UpdateControlInfo();

            base.OnDirtyChanged(e);
        }

        /// <summary>
        /// Raises the UriChanged event and performs custom processing</summary>
        /// <param name="e">UriChangedEventArgs containing event data</param>
        protected override void OnUriChanged(UriChangedEventArgs e)
        {
            UpdateControlInfo();

            base.OnUriChanged(e);
        }

        private void UpdateControlInfo()
        {
            string filePath = Uri.LocalPath;
            string fileName = Path.GetFileName(filePath);
            if (Dirty)
                fileName += "*";

            if (m_controlInfo != null)
            {
                m_controlInfo.Name = fileName;
                m_controlInfo.Description = filePath;
            }
        }

        private string m_editorFileType;
 

        private DomNodeListAdapter<SubCircuit> m_subCircuits;
        private ControlInfo m_controlInfo;
    }
}
