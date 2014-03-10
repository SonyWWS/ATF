//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;

namespace FsmEditorSample
{
    /// <summary>
    /// Adapts DomNode to prototype folders</summary>
    public class PrototypeFolder : DomNodeAdapter
    {
        /// <summary>
        /// Gets and sets the prototype folder name</summary>
        public string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.prototypeFolderType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.prototypeFolderType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets a list of the prototype folders in the prototype folder</summary>
        public IList<PrototypeFolder> Folders
        {
            get { return GetChildList<PrototypeFolder>(Schema.prototypeFolderType.prototypeFolderChild); }
        }

        /// <summary>
        /// Gets a list of prototypes in the prototype folder</summary>
        public IList<Prototype> Prototypes
        {
            get { return GetChildList<Prototype>(Schema.prototypeFolderType.prototypeChild); }
        }
    }
}
