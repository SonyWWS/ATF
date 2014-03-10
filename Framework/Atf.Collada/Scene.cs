//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA scene</summary>
    public class Scene : DomNodeAdapter, INameable, ISceneGraphHierarchy
    {

        #region INameable Members

        /// <summary>
        /// Gets and sets DOM object's name</summary>
        public string Name
        {
            get { return m_visualScene.Name; }
            set { m_visualScene.Name = value; }
        }

        #endregion

        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// This method is called each time the adapter is connected to its underlying node.
        /// Typically overridden by creators of DOM adapters.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            DomNode instanceVisualScene = GetChild<DomNode>(Schema.COLLADA_scene.instance_visual_sceneChild);
            m_visualScene = instanceVisualScene.GetAttribute(Schema.InstanceWithExtra.urlAttribute).As<VisualScene>();
        }

        private VisualScene m_visualScene;

        #region ISceneGraphHierarchy Members

        /// <summary>
        /// Gets enumeration of the children of the object</summary>
        /// <returns>Enumeration of the children of the object</returns>
        public IEnumerable<object> GetChildren()
        {
            yield return m_visualScene.DomNode;
        }

        #endregion
    }
}
