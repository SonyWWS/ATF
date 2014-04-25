//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Adapts a DomNode to a template folder, which holds a hierarchy containing references to subgraphs</summary>
    public abstract class TemplateFolder : DomNodeAdapter
    {
        /// <summary>
        /// Gets and sets the template folder name</summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Gets templates of this folder</summary>
        public abstract IList<Template> Templates { get; }

        /// <summary>
        /// Gets sub-folders of this folder</summary>
        public abstract IList<TemplateFolder> Folders { get; }

        /// <summary>
        /// Get an absolute or relative URL to an external file that defines the template library</summary>
        /// <remarks>Default null for inline templates</remarks>
        public abstract Uri Url { get; set; }
    }
}
