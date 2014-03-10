//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// Implementation of INode. Corresponds to 'nodeType' in atgi.xsd.</summary>
    public class Node : DomNodeAdapter, INode, ITransformable
    {
        /// <summary>
        /// Performs custom actions on NodeSet events.
        /// Called after successfully attaching to internal DOM object.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            // Initialize scale to (1, 1, 1) if missing
            DomNode.SetAttributeIfDefault(Schema.nodeType.scaleAttribute, new Vec3F(1, 1, 1));
                
            m_rotation = DomNode.GetChild(Schema.nodeType.rotEulChild);
            m_rotationAxis = DomNode.GetChild(Schema.nodeType.rotAxisEulChild);
            Transform = TransformUtils.CalcTransform(this);

            Box boxValue = DomNodeUtil.GetBox(DomNode, Schema.nodeType.boundingBoxAttribute);
            if (boxValue.IsEmpty)
                m_boundingBox = new Cached<Box>(CalculateBoundingBox); // don't set value and force to compute
            else
                m_boundingBox = new Cached<Box>(CalculateBoundingBox, boxValue); // non-default value found, use it
        }

        /// <summary>
        /// Gets and sets node name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.nodeType.nameAttribute); }
            set { SetAttribute(Schema.nodeType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets the list of Meshes</summary>
        public IList<IMesh> Meshes
        {
            get { return GetChildList<IMesh>(Schema.nodeType.meshChild); }
        }

        /// <summary>
        /// Gets the list of child nodes</summary>
        public IList<INode> ChildNodes
        {
            get { return GetChildList<INode>(Schema.nodeType.nodeChild); }
        }

        /// <summary>
        /// Gets the list of child joints</summary>
        public IList<IJoint> ChildJoints
        {
            get { return GetChildList<IJoint>(Schema.nodeType.jointChild); }
        }

        #region IRenderableNode Members

        /// <summary>
        /// Gets and sets the local transformation matrix</summary>
        public Matrix4F Transform
        {
            get
            {
                return DomNodeUtil.GetMatrix(DomNode, Schema.nodeType.transformAttribute);
            }
            set
            {
                DomNodeUtil.SetMatrix(DomNode, Schema.nodeType.transformAttribute, value);
            }
        }

        /// <summary>
        /// Gets and sets the node translation</summary>
        public Vec3F Translation
        {
            get { return DomNodeUtil.GetVector(DomNode, Schema.nodeType.translateAttribute); }
            set { DomNodeUtil.SetVector(DomNode, Schema.nodeType.translateAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the node rotation</summary>
        public virtual Vec3F Rotation
        {
            get
            {
                Vec3F rot = GetVector(m_rotation, Schema.rotationType.Attribute);
                if (m_rotation != null)
                {
                    string strRotOrd = m_rotation.GetAttribute(Schema.rotationType.rotOrdAttribute) as string;
                    if (strRotOrd != null && strRotOrd != "xyz")
                    {
                        EulerAngleOrder rotOrd = (EulerAngleOrder)Enum.Parse(typeof(EulerAngleOrder), strRotOrd, true);
                        EulerAngles3F eulerAngles = new EulerAngles3F(rot, rotOrd);
                        rot = eulerAngles.CalculateOrderedAngles();
                    }
                }
                return rot;
            }
            set
            {
                SetVector(m_rotation, Schema.nodeType.rotEulChild, Schema.rotationType.Attribute, value);
            }
        }

        /// <summary>
        /// Gets the node rotation DomNode</summary>
        public DomNode RotationObject
        {
            get { return m_rotation; }
        }

        /// <summary>
        /// Gets and sets the node rotation axis</summary>
        public Vec3F RotationAxis
        {
            get { return GetVector(m_rotationAxis, Schema.rotationType.Attribute); }
            set { SetVector(m_rotationAxis, Schema.nodeType.rotAxisEulChild, Schema.rotationType.Attribute, value); }
        }

        /// <summary>
        /// Gets the node rotation axis DomNode</summary>
        public DomNode RotationAxisObject
        {
            get { return m_rotationAxis; }
        }

        /// <summary>
        /// Gets and sets the node scale</summary>
        public Vec3F Scale
        {
            get
            {
                Vec3F result;
                if (DomNodeUtil.GetVector(DomNode, Schema.nodeType.scaleAttribute, out result))
                    return result;
                return new Vec3F(1, 1, 1);
            }
            set
            {
                DomNodeUtil.SetVector(DomNode, Schema.nodeType.scaleAttribute, value);
            }
        }

        /// <summary>
        /// Gets the node scale DomNode</summary>
        public DomNode ScaleObject
        {
            get { return DomNode; }
        }

        /// <summary>
        /// Gets and sets the node scale pivot</summary>
        public Vec3F ScalePivot
        {
            get
            {
                Vec3F result;
                DomNodeUtil.GetVector(DomNode, Schema.nodeType.scalePivotAttribute, out result);
                return result;
            }
            set { DomNodeUtil.SetVector(DomNode, Schema.nodeType.scalePivotAttribute, value); }
        }

        /// <summary>
        /// Gets the node scale pivot DomNode</summary>
        public DomNode ScalePivotObject
        {
            get { return DomNode; }
        }

        /// <summary>
        /// Gets and sets the node pivot translation</summary>
        public Vec3F ScalePivotTranslation
        {
            get
            {
                Vec3F result;
                DomNodeUtil.GetVector(DomNode, Schema.nodeType.scalePivotTranslationAttribute, out result);
                return result;
            }
            set
            {
                DomNodeUtil.SetVector(DomNode, Schema.nodeType.scalePivotTranslationAttribute, value);
            }
        }

        /// <summary>
        /// Gets the node pivot translation DomNode</summary>
        public DomNode ScalePivotTranslationObject
        {
            get { return DomNode; }
        }

        /// <summary>
        /// Gets and sets the node rotate pivot</summary>
        public Vec3F RotatePivot
        {
            get
            {
                Vec3F result;
                DomNodeUtil.GetVector(DomNode, Schema.nodeType.rotatePivotAttribute, out result);
                return result;
            }
            set
            {
                DomNodeUtil.SetVector(DomNode, Schema.nodeType.rotatePivotAttribute, value);
            }
        }

        /// <summary>
        /// Gets the node rotate pivot DomNode</summary>
        public DomNode RotatePivotObject
        {
            get { return DomNode; }
        }

        /// <summary>
        /// Gets and sets the node pivot translation</summary>
        public Vec3F RotatePivotTranslation
        {
            get
            {
                Vec3F result;
                DomNodeUtil.GetVector(DomNode, Schema.nodeType.rotatePivotTranslationAttribute, out result);
                return result;
            }
            set
            {
                DomNodeUtil.SetVector(DomNode, Schema.nodeType.rotatePivotTranslationAttribute, value);
            }
        }

        /// <summary>
        /// Gets the node pivot translation DomNode</summary>
        public DomNode RotatePivotTranslationObject
        {
            get { return DomNode; }
        }

        /// <summary>
        /// Gets and sets what type of transformation this object can support.
        /// By default, all available transformations are supported.</summary>
        public TransformationTypes TransformationType
        {
            get { return m_transformType; }
            set { m_transformType = value; }
        }

        /// <summary>
        /// This is the mutable node visibility state for temporarily enabling or disabling
        /// this node. This is different than the ATGI "visibility" attribute, which is not
        /// mutable and therefore is persisted.</summary>
        public bool Visible
        {
            get { return m_visible; }
            set { m_visible = value; }
        }

        #endregion

        #region IBoundable
        /// <summary>
        /// Calculates the bounding box of all children, if necessary, and then uses those to
        /// calculate the node's bounding box.</summary>
        /// <returns>Bounding box</returns>
        public Box CalculateBoundingBox()
        {
            Box box = new Box();

            foreach (IBoundable boundable in DomNode.Children.AsIEnumerable<IBoundable>())
                box.Extend(boundable.BoundingBox);

            // Transform box
            box.Transform(Transform);

            DomNodeUtil.SetBox(DomNode, Schema.nodeType.boundingBoxAttribute, box);

            return box;
        }

        /// <summary>
        /// Gets and sets the geometry's bounding box. Creates an epsilon size box.</summary>
        public Box BoundingBox
        {
            get { return m_boundingBox.Value; }
        }

        #endregion

        private Vec3F GetVector(DomNode domNode, AttributeInfo attributeInfo)
        {
            Vec3F result;
            if (domNode != null)
                DomNodeUtil.GetVector(domNode, attributeInfo, out result);
            else
                result = new Vec3F();

            return result;
        }

        private void SetVector(DomNode domNode, ChildInfo metaElement, AttributeInfo attributeInfo, Vec3F v)
        {
            if (domNode == null)
                domNode = DomNode.GetChild(metaElement);

            DomNodeUtil.SetVector(domNode, attributeInfo, v);
        }

        private DomNode m_rotation;
        private DomNode m_rotationAxis;

        private TransformationTypes m_transformType =
            TransformationTypes.Translation |
            TransformationTypes.Scale |
            TransformationTypes.Rotation |
            TransformationTypes.ScalePivot |
            TransformationTypes.ScalePivotTranslation |
            TransformationTypes.RotatePivot |
            TransformationTypes.RotatePivotTranslation;

        private bool m_visible = true;
        private Cached<Box> m_boundingBox;
    }
}

