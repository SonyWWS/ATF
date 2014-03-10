//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapts DomNode to prototype folders</summary>
    public abstract class PrototypeFolder : DomNodeAdapter
    {

        protected abstract AttributeInfo NameAttribute { get; }

        // required  child info
        protected abstract ChildInfo PrototypeChildInfo { get; }
        protected abstract ChildInfo PrototypeFolderChildInfo { get; }

        /// <summary>
        /// Gets or sets the prototype folder name</summary>
        public string Name
        {
            get { return (string)DomNode.GetAttribute(NameAttribute); }
            set { DomNode.SetAttribute(NameAttribute, value); }
        }

        /// <summary>
        /// Gets a list of the prototype folders in the prototype folder</summary>
        public IList<PrototypeFolder> Folders
        {
            get { return GetChildList<PrototypeFolder>(PrototypeFolderChildInfo); }
        }

        /// <summary>
        /// Gets a list of prototypes in the prototype folder</summary>
        public IList<Prototype> Prototypes
        {
            get { return GetChildList<Prototype>(PrototypeChildInfo); }
        }

    }
}
