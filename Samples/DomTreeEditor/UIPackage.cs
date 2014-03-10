//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Adapts DomNode to a UI package, a collection of resources and forms</summary>
    public class UIPackage : UIObject
    {
        /// <summary>
        /// Gets a list of all UIForms in the package</summary>
        public IList<UIForm> Forms
        {
            get { return GetChildList<UIForm>(UISchema.UIPackageType.FormChild); }
        }

        /// <summary>
        /// Gets a list of all UIShaders in the package</summary>
        public IList<UIShader> Shaders
        {
            get { return GetChildList<UIShader>(UISchema.UIPackageType.ShaderChild); }
        }

        /// <summary>
        /// Gets a list of all UITextures in the package</summary>
        public IList<UITexture> Textures
        {
            get { return GetChildList<UITexture>(UISchema.UIPackageType.TextureChild); }
        }

        /// <summary>
        /// Gets a list of all UIFonts in the package</summary>
        public IList<UIFont> Fonts
        {
            get { return GetChildList<UIFont>(UISchema.UIPackageType.FontChild); }
        }
    }
}
