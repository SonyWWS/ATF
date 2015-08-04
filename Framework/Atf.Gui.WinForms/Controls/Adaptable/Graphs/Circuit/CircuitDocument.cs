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
        /// <summary>
        /// Performs initialization when the adapter is connected to the circuit's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            var documentClientInfo = DomNode.Type.GetTag<DocumentClientInfo>();
            if (documentClientInfo != null)
                SetEditorFileType(documentClientInfo.FileType);
            
            #pragma warning disable 618 //mastered sub-circuits are obsolete
            if (SubCircuitChildInfo != null)
                m_subCircuits = new DomNodeListAdapter<SubCircuit>(DomNode, SubCircuitChildInfo);
            #pragma warning restore 618

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
        /// Gets the optional AttributeInfo for the document version
        /// pins should be displayed.</summary>
        protected virtual AttributeInfo VersionAttribute 
        {
            get { return null; }
        }

        ///<summary>Gets the version of the tool that generated the document</summary>
        public Version Version
        {
            get
            {
                if (VersionAttribute == null)
                    return new Version(1, 0); // default version
                var versionValue = DomNode.GetAttribute(VersionAttribute) as string;
                return new Version(versionValue);
            }
        }

        /// <summary>
        /// Gets the document client's user-readable file type name</summary>
        public override string Type
        {
            get { return m_editorFileType; }
        }

        /// <summary>
        /// Sets the user-readable (and thus, localizable) circuit document file type name,
        /// which can be read using the Type property</summary>
        /// <param name="fileType">String representing circuit document file type</param>
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

        /// <summary>
        /// This property is obsolete as of ATF 3.9. It will be marked with the ObsoleteAttribute
        /// for ATF 3.10 and then later removed.</summary>
        //[Obsolete("Circuit groups and circuit templates have replaced mastered circuits")]
        protected virtual ChildInfo SubCircuitChildInfo { get { return null; } }

        /// <summary>
        /// Gets the circuit's sub-circuits</summary>
        [Obsolete("Circuit groups and circuit templates have replaced mastered circuits")]
        public IList<SubCircuit> SubCircuits
        {
            get { return m_subCircuits; }
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

        #pragma warning disable 618 //mastered sub-circuits are obsolete
        private IList<SubCircuit> m_subCircuits;
        #pragma warning restore 618

        private string m_editorFileType;
        private ControlInfo m_controlInfo;
    }
}
