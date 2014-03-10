//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA visual scene</summary>
    public class VisualScene : DomNodeAdapter,ISceneGraphHierarchy, INameable
    {
     
        #region INameable Members

        /// <summary>
        /// Gets and sets DOM object's name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.visual_scene.nameAttribute); }
            set 
            { 
               // DomNode.SetAttribute(Schema.visual_scene.nameAttribute, value); 
            }
        }

        #endregion

        #region ISceneGraphHierarchy Members

        /// <summary>
        /// Gets enumeration of the children of the object</summary>
        /// <returns>Enumeration of the children of the object</returns>
        public IEnumerable<object> GetChildren()
        {
            return GetChildList<object>(Schema.visual_scene.nodeChild);
        }

        #endregion
    }
}
