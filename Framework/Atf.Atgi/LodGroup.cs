//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI LodGroup (Level Of Detail Group)</summary>
    public class LodGroup : DomNodeAdapter, ILodGroup, IBoundable
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// This method is called each time the adapter is connected to its underlying node.
        /// Typically overridden by creators of DOM adapters.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            m_boundingBox = new Cached<Box>(CalculateBoundingBox);
        }

        /// <summary>
        /// Gets and sets the Scene name</summary>
        public string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.lodgroupType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.lodgroupType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets the list of Scene nodes</summary>
        public IList<INode> Nodes
        {
            get { return GetChildList<INode>(Schema.lodgroupType.nodeChild); }
        }

        #region ILodGroup Members

        /// <summary>
        /// Gets distance thresholds for the LODs</summary>
        public IList<float> Thresholds
        {
            get
            {
                return DomNode.GetChild(Schema.lodgroupType.thresholdsChild).
                    GetAttribute(Schema.lodgroupType_thresholds.Attribute) as float[];
            }
        }

        #endregion

        #region IBoundable Members

        /// <summary>
        /// Calculates the bounding box for the instance</summary>
        /// <returns>Bounding box</returns>
        public Box CalculateBoundingBox()
        {
            Box box = new Box();

            foreach (IBoundable boundable in DomNode.Children.AsIEnumerable<IBoundable>())
                box.Extend(boundable.BoundingBox);

            //m_box.Transform(Transform); //Look at the schema -- "lodgroupType" does not have a transformation
            return box;
        }

        /// <summary>
        /// Gets the bounding box in local space</summary>
        public Box BoundingBox
        {
            get { return m_boundingBox.Value; }
        }

        #endregion


        #region IVisible Members

        /// <summary>
        /// Gets or sets whether the object is visible</summary>
        public bool Visible
        {
            get { return m_visible; }
            set { m_visible = value; }
        }

        #endregion

        private Cached<Box> m_boundingBox;
        private bool m_visible = true;
    }
}

