//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA node</summary>
    public class Node : DomNodeAdapter,  ITransformable,ISceneGraphHierarchy, INameable
    {
       

        #region INameable Members

        /// <summary>
        /// Gets and sets DOM object's name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.node.nameAttribute); }
            set { throw new InvalidOperationException(); }
        }

        #endregion

        #region ITransformable Members

        /// <summary>
        /// Gets and sets the local transformation matrix. This is derived from the various
        /// components below. Setting Transform does not update the components. See
        /// transform constraints.</summary>
        public Matrix4F Transform
        {
            get { return new Matrix4F(m_transform); }
            set {m_transform = value.ToArray(); }
        }

        /// <summary>
        /// Gets and sets the node translation. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        public Vec3F Translation
        {
            get {  return m_translation; }
            set { m_translation = value; }
        }

        /// <summary>
        /// Gets and sets the node rotation. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        public Vec3F Rotation
        {
            get{ return m_rotation; }
            set { m_rotation = value; }
        }

        /// <summary>
        /// Gets and sets the node scale. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return new Vec3F{1,1,1}.</remarks>
        public Vec3F Scale
        {
            get{ return m_scale;}
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
        /// Performs initialization when the adapter's node is set.
        /// This method is called each time the adapter is connected to its underlying node.
        /// Typically overridden by creators of DOM adapters.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            // get trans, scale, and rot.
            foreach (DomNode domNode in this.DomNode.GetChildList(Schema.node.scaleChild))
            {
                m_scale = Tools.GetVector3(domNode, Schema.TargetableFloat3.Attribute);
                break;
            }

            foreach (DomNode domNode in this.DomNode.GetChildList(Schema.node.translateChild))
            {
                m_translation = Tools.GetVector3(domNode, Schema.TargetableFloat3.Attribute);
                break;
            }
                        
            const float PiOver180 = (float)(Math.PI / 180.0f);
            foreach (DomNode node in DomNode.GetChildList(Schema.node.rotateChild))
            {                
                double[] arr = (double[])node.GetAttribute(Schema.rotate.Attribute);
                float angle = (float)arr[3] * PiOver180;
                string sid = node.GetAttribute(Schema.rotate.sidAttribute) as string;
                if (string.IsNullOrEmpty(sid))
                    continue;
                if (sid == "rotateX")
                    m_rotation.X = angle;
                else if (sid == "rotateY")
                    m_rotation.Y = angle;
                else if (sid == "rotateZ")
                    m_rotation.Z = angle;
            }

            Matrix4F M = new Matrix4F();
            Matrix4F temp = new Matrix4F();

            temp.Scale(Scale);
            M.Mul(M, temp);

            if (m_rotation.X != 0)
            {
                temp.RotX(m_rotation.X);
                M.Mul(M, temp);
            }

            if (m_rotation.Y != 0)
            {
                temp.RotY(m_rotation.Y);
                M.Mul(M, temp);
            }

            if (m_rotation.Z != 0)
            {
                temp.RotZ(m_rotation.Z);
                M.Mul(M, temp);
            }

            temp.Set(Translation);
            M.Mul(M, temp);

            Transform = M;
            m_boundingBox = new Cached<Box>(CalculateBoundingBox);
            Visible = true;
        }

        private Box CalculateBoundingBox()
        {
            // compute box.
            var box = new Box();
            
            foreach (InstanceGeometry instGeom in GetChildList<InstanceGeometry>(Schema.node.instance_geometryChild))
            {                
                box.Extend(instGeom.Geometry.BoundingBox);
            }

            foreach (InstanceController instCtrl in GetChildList<InstanceController>(Schema.node.instance_controllerChild))
            {                
                box.Extend(instCtrl.Geometry.BoundingBox);
            }

            foreach (Node nd in GetChildList<Node>(Schema.node.nodeChild))
            {                
                box.Extend(nd.BoundingBox);
            }

            box.Transform(Transform);
            return box;
           
        }

        private Cached<Box> m_boundingBox;

        private Vec3F m_translation = Vec3F.ZeroVector;
        private Vec3F m_rotation = Vec3F.ZeroVector;
        private Vec3F m_scale = new Vec3F(1.0f, 1.0f, 1.0f);
        //private Matrix4F m_transform = Matrix4F.Identity;
        private float[] m_transform = new float[16];
        private TransformationTypes m_transformType =
            TransformationTypes.Translation |
            TransformationTypes.Scale |
            TransformationTypes.Rotation;
            
        #region ISceneGraphHierarchy Members

        /// <summary>
        /// Gets enumeration of the children of the object</summary>
        /// <returns>Enumeration of the children of the object</returns>
        public IEnumerable<object> GetChildren()
        {

            foreach (DomNode dom in DomNode.GetChildren(Schema.node.nodeChild))
            {
                yield return dom;
            }

            foreach (DomNode dom in DomNode.GetChildren(Schema.node.instance_geometryChild))
            {
                yield return dom;
            }

            foreach (DomNode dom in DomNode.GetChildren(Schema.node.instance_controllerChild))
            {
                yield return dom;
            }
        }

        #endregion
    }
}
