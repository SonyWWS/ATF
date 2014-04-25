//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using System;
using System.Collections.Generic;


namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts a DomNode to a template folder, which holds a hierarchy containing references to subgraphs</summary>
    public class TemplateFolder : Sce.Atf.Dom.TemplateFolder
    {
        /// <summary>
        /// Gets and sets the template folder name</summary>
        public override string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.templateFolderType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.templateFolderType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets templates of this folder</summary>
        public override IList<Sce.Atf.Dom.Template> Templates
        {
            get { return GetChildList<Sce.Atf.Dom.Template>(Schema.templateFolderType.templateChild); }
        }

        /// <summary>
        /// Gets sub-folders of this folder</summary>
        public override IList<Sce.Atf.Dom.TemplateFolder> Folders
        {
            get { return GetChildList<Sce.Atf.Dom.TemplateFolder>(Schema.templateFolderType.templateFolderChild); }
        }

        /// <summary>
        /// Get an absolute or relative URL to an external file that defines the template library</summary>
        /// <remarks>Default null for inline templates</remarks>
        public override Uri Url
        {
            get { return GetAttribute<Uri>(Schema.templateFolderType.referenceFileAttribute); }
            set { SetAttribute(Schema.templateFolderType.referenceFileAttribute, value); }
        }
    }
}
