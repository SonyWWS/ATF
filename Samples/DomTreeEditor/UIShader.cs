//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace DomTreeEditorSample
{
    /// <summary>
    /// Adapts DomNode to shader resources</summary>
    public class UIShader : UIObject
    {
        /// <summary>
        /// Gets and sets the FX file path</summary>
        public string FxFile
        {
            get { return GetAttribute<string>(UISchema.UIShaderType.FxFileAttribute); }
            set { SetAttribute(UISchema.UIShaderType.FxFileAttribute, value); }
        }

    }
}
