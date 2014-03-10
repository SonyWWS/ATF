//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI World</summary>
    public class World : DomNodeAdapter, IWorld
    {
        /// <summary>
        /// Gets and sets the world name</summary>
        public string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.worldType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.worldType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the Scene</summary>
        public IScene Scene
        {
            get
            {
                IList<DomNode> sceneNodes = DomNode.GetChildList(Schema.worldType.sceneChild);
                if (sceneNodes.Count > 0)
                    return sceneNodes[0].As<IScene>();
                return null;
            }
            set
            {
                IList<DomNode> sceneNodes = DomNode.GetChildList(Schema.worldType.sceneChild);
                sceneNodes[0] = value.Cast<DomNode>();
            }
        }
    }
}

