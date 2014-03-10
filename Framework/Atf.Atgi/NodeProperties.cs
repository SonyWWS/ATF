//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// DOM object interface to expose ATGI node properties</summary>
    public class NodeProperties : CustomTypeDescriptorNodeAdapter
    {
        /// <summary>
        /// Gets property descriptors for the underlying DOM object</summary>
        /// <returns>Property descriptors for the underlying DOM object</returns>
        protected override System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
        {
            Node node = DomNode.As<Node>();
            if (node != null)
            {
                return new System.ComponentModel.PropertyDescriptor[]
                {
                    new AttributePropertyDescriptor(
                        "Name",
                        Schema.nodeType.nameAttribute,
                        "Misc",
                        "Node name",
                        true),
                    new AttributePropertyDescriptor(
                        "Visible",
                        Schema.nodeType.visibilityAttribute,
                        "Misc",
                        "Node visibility",
                        true),
                    new AttributePropertyDescriptor(
                        "Transform",
                        Schema.nodeType.transformAttribute,
                        "Transform",
                        "Node transform",
                        true),
                    new AttributePropertyDescriptor(
                        "Translation",
                        Schema.nodeType.translateAttribute,
                        "Transform",
                        "Node translation",
                        true),
                    new ChildAttributePropertyDescriptor(
                        "Rotation",
                        Schema.rotationType.Attribute,
                        Schema.nodeType.rotEulChild,
                        "Transform",
                        "Node rotation",
                        true),
                    new AttributePropertyDescriptor(
                        "Scale", Schema.nodeType.scaleAttribute,
                        "Transform",
                        "Node scale",
                        true),
                    new AttributePropertyDescriptor(
                        "Rotation Pivot",
                        Schema.nodeType.rotatePivotAttribute,
                        "Transform",
                        "Node rotation pivot",
                        true),
                    new AttributePropertyDescriptor(
                        "Rotation Pivot Translation",
                        Schema.nodeType.rotatePivotTranslationAttribute,
                        "Transform",
                        "Node rotation pivot translation",
                        true),
                    new AttributePropertyDescriptor(
                        "Scale Pivot",
                        Schema.nodeType.scalePivotAttribute,
                        "Transform",
                        "Node scale pivot",
                        true),
                    new AttributePropertyDescriptor(
                        "Scale Pivot Translation",
                        Schema.nodeType.scalePivotTranslationAttribute,
                        "Transform",
                        "Node scale pivot translation",
                        true),
                };
            }

            return EmptyArray<System.ComponentModel.PropertyDescriptor>.Instance;
        }
    }
}
