//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI Instance</summary>
    public class Instance : DomNodeAdapter, IBoundable
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
        /// Gets and sets the instance name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.instanceType.nameAttribute); }
            set { SetAttribute(Schema.instanceType.nameAttribute, value); }
        }

        #region IBoundable Members

        /// <summary>
        /// Calculates the bounding box for the instance</summary>
        /// <returns>Bounding box</returns>
        public Box CalculateBoundingBox()
        {
            Box box = new Box();

            foreach (IBoundable boundable in DomNode.Children.AsIEnumerable<IBoundable>())
                box.Extend(boundable.BoundingBox);

            return box;
        }

        /// <summary>
        /// Gets a bounding box in local space</summary>
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

        ///// <summary>
        ///// Gets instance's  target name</summary>
        //private string Target
        //{
        //    get
        //    {
        //        object targetUri = InternalObject.GetAttribute("target");
        //        if (targetUri != null)
        //        {
        //            DomUri uri = targetUri as DomUri;
        //            return uri.ToString();
        //        }
        //        else
        //            return String.Empty;
        //    }
        //}

        private Cached<Box> m_boundingBox;
        private bool m_visible = true;
    }
}

