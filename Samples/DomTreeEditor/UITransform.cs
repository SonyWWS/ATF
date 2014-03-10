//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Adapts DomNode to a UI transform, with 3d translation, rotation, and scale vectors</summary>
    public class UITransform : DomNodeAdapter
    {
        /// <summary>
        /// Gets or sets the rotation</summary>
        public Vec3F Rotate
        {
            get { return GetAttribute<Vec3F>(UISchema.UITransformType.RotateAttribute); }
            set { SetAttribute(UISchema.UITransformType.RotateAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the scale</summary>
        public Vec3F Scale
        {
            get { return GetAttribute<Vec3F>(UISchema.UITransformType.ScaleAttribute); }
            set { SetAttribute(UISchema.UITransformType.ScaleAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the translation</summary>
        public Vec3F Translation
        {
            get { return GetAttribute<Vec3F>(UISchema.UITransformType.TranslateAttribute); }
            set { SetAttribute(UISchema.UITransformType.TranslateAttribute, value); }
        }
    }
}
