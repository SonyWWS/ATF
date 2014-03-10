//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA instance geometry</summary>
    public class InstanceGeometry : DomNodeAdapter, ISceneGraphHierarchy
    {

        

        #region IBoundable Members

        /// <summary>
        /// Gets a bounding box in local space</summary>
        public Box BoundingBox
        {
            get { return Geometry.BoundingBox; }
        }

        #endregion

        #region INameable Members

        /// <summary>
        /// Gets and sets DOM object's name</summary>
        public string Name
        {
            get { return Geometry.Name; }
            set { Geometry.Name = value; }
        }

        #endregion

        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// This method is called each time the adapter is connected to its underlying node.
        /// Typically overridden by creators of DOM adapters.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            Geometry = DomNode.GetAttribute(Schema.instance_geometry.urlAttribute).As<Geometry>();
            Geometry.Effects = Tools.CreateEffectDictionary(GetChild<DomNode>(Schema.instance_geometry.bind_materialChild));
        }

        /// <summary>
        /// Gets or sets Geometry</summary>
        public Geometry Geometry { get; set; }

        #region ISceneGraphHierarchy Members

        /// <summary>
        /// Gets enumeration of the children of the object</summary>
        /// <returns>Enumeration of the children of the object</returns>
        public IEnumerable<object> GetChildren()
        {
            yield return Geometry.DomNode;
        }

        #endregion
    }
}
