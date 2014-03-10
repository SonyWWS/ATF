//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;
using System.Linq;

using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Obj
{
    /// <summary>
    /// Shader class</summary>
    public class Shader : DomNodeAdapter, IShader, IRenderStateCreator
    {

        #region IShader Members

        /// <summary>
        /// Gets the list of shader bindings</summary>
        public IList<IBinding> Bindings
        {
            get { return Enumerable.Empty<IBinding>() as IList<IBinding>; }
        }

        /// <summary>
        /// Gets the list of custom attributes</summary>
        public IList<object> CustomAttributes
        {
            get { return Enumerable.Empty<object>() as IList<object>; }
        }

        #endregion

        #region INameable Members

        /// <summary>
        /// Gets and sets DOM object's name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.shaderType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.shaderType.nameAttribute, value); }
        }

        #endregion

        #region IRenderStateCreator Members

        /// <summary>
        /// Creates RenderState for an object</summary>
        /// <returns>RenderState for object</returns>
        public RenderState CreateRenderState()
        {
            var rs = new RenderState
            {
                InheritState = ~(RenderMode.Alpha | RenderMode.DisableZBuffer | RenderMode.SolidColor),
                RenderMode = RenderMode.Smooth | RenderMode.Lit | RenderMode.SolidColor | RenderMode.WireframeColor,
                SolidColor = new Vec4F(1, 1, 1, 1),
                WireframeColor = new Vec4F(1, 1, 1, 1),

                AmbientColor = GetAttribute<Vec4F>(Schema.shaderType.ambientAttribute),
                DiffuseColor = GetAttribute<Vec4F>(Schema.shaderType.diffuseAttribute),
                Shininess = GetAttribute<float>(Schema.shaderType.shininessAttribute),
                SpecularColor = GetAttribute<Vec4F>(Schema.shaderType.specularAttribute)
            };

            ParseAttributesForTexture(rs);

            return rs;
        }

        #endregion

        private void ParseAttributesForTexture(RenderState rs)
        {
            string texturePath = Uri.UnescapeDataString(GetAttribute<string>(Schema.shaderType.textureAttribute));

            if (texturePath.Equals(""))
                return;

            int texName = Global<TextureManager>.Instance.GetTextureName(texturePath);
            if (texName != -1)
            {
                rs.RenderMode |= RenderMode.Textured;
                rs.TextureName = texName;

                TextureInfo info = Global<TextureManager>.Instance.GetTextureInfo(texName);
                if (info.Components == 4)
                    rs.RenderMode |= RenderMode.Alpha;
            }

            return;
        }
    }
}
