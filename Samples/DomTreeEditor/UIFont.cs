//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace DomTreeEditorSample
{
    /// <summary>
    /// Adapts DomNode to font resources</summary>
    public class UIFont : UIObject
    {
        /// <summary>
        /// Gets and sets the font file path</summary>
        public string FontFile
        {
            get { return GetAttribute<string>(UISchema.UIFontType.FontFileAttribute); }
            set { SetAttribute(UISchema.UIFontType.FontFileAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the font size</summary>
        public float FontSize
        {
            get { return GetAttribute<float>(UISchema.UITextureType.TextureArrayAttribute); }
            set { SetAttribute(UISchema.UITextureType.TextureArrayAttribute, value); }
        }
    }
}
