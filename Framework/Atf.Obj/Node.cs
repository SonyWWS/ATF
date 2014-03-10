//Sony Computer Entertainment Confidential

using System.Collections.Generic;
using System.Linq;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Obj
{
    /// <summary>
    /// Adaptable nodes</summary>
    public class Node : DomNodeAdapter, INode, ITransformable
    {
        /// <summary>
        /// Calculates bounding box of node's children</summary>
        /// <returns>Bounding box of children</returns>
        public Box CalculateBoundingBox()
        {
            var box = new Box();
            foreach (IBoundable boundable in DomNode.Children.AsIEnumerable<IBoundable>())
                box.Extend(boundable.BoundingBox);

            box.Transform(Transform);
            DomNodeUtil.SetBox(DomNode, Schema.nodeType.boundingBoxAttribute, box);

            return box;
        }

        #region INode Members

        /// <summary>
        /// Gets the mesh list</summary>
        public IList<IMesh> Meshes
        {
            get { return GetChildList<IMesh>(Schema.nodeType.meshChild); }
        }

        /// <summary>
        /// Gets the child nodes list</summary>
        public IList<INode> ChildNodes
        {
            get { return Enumerable.Empty<INode>() as IList<INode>; } // The OBJ format does not support this functionality
        }

        /// <summary>
        /// Gets the child joints list</summary>
        public IList<IJoint> ChildJoints
        {
            get { return Enumerable.Empty<IJoint>() as IList<IJoint>; } // The OBJ format does not support this functionality
        }

        /// <summary>
        /// Gets and sets the transformation matrix</summary>
        public Matrix4F Transform
        {
            get { return m_transform; }
            set { m_transform = value; }
        }

        #endregion

        #region INameable Members

        /// <summary>
        /// Gets and sets DOM object's name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.nodeType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.nodeType.nameAttribute, value); }
        }

        #endregion

        #region ITransformable Members

        /// <summary>
        /// Gets and sets the node translation. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        public Vec3F Translation { get; set; }

        /// <summary>
        /// Gets and sets the node rotation. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        public Vec3F Rotation { get; set; }

        /// <summary>
        /// Gets and sets the node scale. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return new Vec3F{1,1,1}.</remarks>
        public Vec3F Scale
        {
            get { return m_scale; }
            set { m_scale = value; }
        }

        /// <summary>
        /// Gets and sets the translation to origin of scaling. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        public Vec3F ScalePivot { get; set; }

        /// <summary>
        /// Gets and sets the translation after scaling. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        public Vec3F ScalePivotTranslation { get; set; }

        /// <summary>
        /// Gets and sets the translation to origin of rotation. Setting the RotatePivot
        /// typically resets RotatePivotTranslation. See transform constraints.
        /// Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        public Vec3F RotatePivot { get; set; }

        /// <summary>
        /// Gets and sets the translation after rotation. Is used to keep local coordinates
        /// pinned to the same world coordinate when the RotatePivot is adjusted.
        /// Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        public Vec3F RotatePivotTranslation { get; set; }

        /// <summary>
        /// Gets or sets what type of transformation this object can support. All the transformation properties
        /// have a valid 'get', but check the appropriate flag before setting the property.</summary>
        public TransformationTypes TransformationType
        {
            get { return m_transformType; }
            set { m_transformType = value; }
        }

        #endregion

        #region IBoundable Members

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
        public bool Visible { get; set; }

        #endregion

        /// <summary>
        /// Raises the NodeSet event and performs custom processing</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            Box boxValue = DomNodeUtil.GetBox(DomNode, Schema.nodeType.boundingBoxAttribute);
            m_boundingBox = boxValue.IsEmpty ? new Cached<Box>(CalculateBoundingBox) : new Cached<Box>(CalculateBoundingBox, boxValue);

            Visible = true;
        }
        
        private Cached<Box> m_boundingBox;

        // The OBJ format does not support this functionality
        private Vec3F m_scale = new Vec3F(1.0f, 1.0f, 1.0f);
        private Matrix4F m_transform = Matrix4F.Identity;
        private TransformationTypes m_transformType =
            TransformationTypes.Translation |
            TransformationTypes.Scale |
            TransformationTypes.Rotation |
            TransformationTypes.ScalePivot |
            TransformationTypes.ScalePivotTranslation |
            TransformationTypes.RotatePivot |
            TransformationTypes.RotatePivotTranslation;
    }
}

