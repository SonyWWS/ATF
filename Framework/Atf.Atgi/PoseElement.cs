//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI Pose element</summary>
    public class PoseElement : DomNodeAdapter, IPoseElement
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// This method is called each time the adapter is connected to its underlying node.
        /// Typically overridden by creators of DOM adapters.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            // Initialize scale to (1, 1, 1) if missing
            DomNode.SetAttributeIfDefault(Schema.poseType_element.scaleAttribute, new Vec3F(1, 1, 1));
        }
        
        /// <summary>
        /// Gets and sets the Pose target name</summary>
        public object Target
        {
            get { return DomNode.GetAttribute(Schema.poseType_element.targetAttribute); }
            set { DomNode.SetAttribute(Schema.poseType_element.targetAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the Pose translation</summary>
        public Vec3F Translation
        {
            get
            {
                return DomNodeUtil.GetVector(DomNode, Schema.poseType_element.translateAttribute);
            }
            set
            {
                DomNodeUtil.SetVector(DomNode, Schema.poseType_element.translateAttribute, value);
            }
        }

        /// <summary>
        /// Gets and sets the Pose rotation</summary>
        public EulerAngles3F Rotation
        {
            get
            {
                DomNode rotNode = DomNode.GetChild(Schema.poseType_element.rotEulChild);
                string rotOrdString = rotNode.GetAttribute(Schema.element_rotEul.rotOrdAttribute) as string;
                EulerAngleOrder rotOrd;
                EnumUtil.TryParse(rotOrdString, out rotOrd);
                Vec3F values = DomNodeUtil.GetVector(rotNode, Schema.element_rotEul.Attribute);
                return new EulerAngles3F(values, rotOrd);
            }
            set
            {
                DomNode rotNode = DomNode.GetChild(Schema.poseType_element.rotEulChild);
                rotNode.SetAttribute(Schema.element_rotEul.Attribute, value.Angles);
                rotNode.SetAttribute(Schema.element_rotEul.rotOrdAttribute, value.RotOrder.ToString());
            }
        }

        /// <summary>
        /// Gets and sets the Pose scale</summary>
        public Vec3F Scale
        {
            get { return DomNodeUtil.GetVector(DomNode, Schema.poseType_element.scaleAttribute); }
            set { DomNodeUtil.SetVector(DomNode, Schema.poseType_element.scaleAttribute, value); }
        }
    }
}
