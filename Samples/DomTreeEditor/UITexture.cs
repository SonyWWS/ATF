//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace DomTreeEditorSample
{
    /// <summary>
    /// Adapts DomNode to texture resources</summary>
    public class UITexture : UIObject
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
        /// Sets initial values for texture number array when it has none.</summary>
        protected override void OnNodeSet()
        {
            float[] val = GetAttribute<float[]>(UISchema.UITextureType.TextureArrayAttribute);
            if (val.GetLength(0) == 0)
                SetAttribute(UISchema.UITextureType.TextureArrayAttribute, new float[] { 11.8f, 2.3f, 4.0f, 7.5f });
        }

        /// <summary>
        /// Gets or sets the texture file path</summary>
        public string TextureFile
        {
            get { return GetAttribute<string>(UISchema.UITextureType.TextureFileAttribute); }
            set { SetAttribute(UISchema.UITextureType.TextureFileAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the texture array</summary>
        public float[] TextureArray
        {
            get { return GetAttribute<float[]>(UISchema.UITextureType.TextureArrayAttribute); }
            set { SetAttribute(UISchema.UITextureType.TextureArrayAttribute, value); }
        }
    }

}
