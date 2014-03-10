//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI Scene</summary>
    public class Scene : DomNodeAdapter, IScene
    {
        /// <summary>
        /// Gets and sets the Scene name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.sceneType.nameAttribute); }
            set { SetAttribute(Schema.sceneType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets the list of scene nodes</summary>
        public IList<INode> Nodes
        {
            get { return GetChildList<INode>(Schema.sceneType.nodeChild); }
        }
    }
}

