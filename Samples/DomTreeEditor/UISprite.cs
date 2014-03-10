//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace DomTreeEditorSample
{
    /// <summary>
    /// Adapts DomNode to a sprite, which is a UIControl that renders with a shader</summary>
    public class UISprite : UIControl
    {
        /// <summary>
        /// Gets or sets the sprite's shader</summary>
        public UIShader Shader
        {
            get 
            {
                UIRef uiRef = GetChild<UIRef>(UISchema.UISpriteType.ShaderChild);
                if (uiRef != null)
                    return uiRef.UIObject as UIShader;
                return null;
            }
            set
            {
                SetChild(UISchema.UISpriteType.ShaderChild, UIRef.New(value));
            }
        }
    }
}
