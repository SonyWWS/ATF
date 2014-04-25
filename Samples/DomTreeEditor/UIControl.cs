//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;

namespace DomTreeEditorSample
{
    /// <summary>
    /// UI Control, base class for UISprite and UITextItem. Adapts DomNode to a control.</summary>
    public class UIControl : UIObject
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the diagram annotation's DomNode,
        /// creating child DomNode for transform and setting its scale.</summary>
        protected override void OnNodeSet()
        {
            DomNode transform = DomNode.GetChild(UISchema.UIControlType.TransformChild);
            if (transform == null)
            {
                transform = new DomNode(UISchema.UITransformType.Type);
                transform.SetAttribute(UISchema.UITransformType.ScaleAttribute, new float[] { 1.0f, 1.0f, 1.0f });
                DomNode.SetChild(UISchema.UIControlType.TransformChild, transform);
            }
        }

        /// <summary>
        /// Gets the control's transform</summary>
        public UITransform Transform
        {
            get { return GetChild<UITransform>(UISchema.UIControlType.TransformChild); }
        }

        /// <summary>
        /// Gets the list of all child controls in the control</summary>
        public IList<UIControl> Controls
        {
            get { return GetChildList<UIControl>(UISchema.UIControlType.ControlChild); }
        }
    }
}
