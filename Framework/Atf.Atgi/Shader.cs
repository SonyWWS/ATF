//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI Shader</summary>
    public class Shader : DomNodeAdapter, IShader, IRenderStateCreator
    {
        /// <summary>
        /// Gets and sets the Shader name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.shaderType.nameAttribute); }
            set { SetAttribute(Schema.shaderType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets the list of shader bindings (names and types of data)</summary>
        public IList<IBinding> Bindings
        {
            get { return GetChildList<IBinding>(Schema.shaderType.bindingChild); }
        }

        /// <summary>
        /// Gets the list of custom attributes</summary>
        public IList<object> CustomAttributes
        {
            get
            {
                IList<object> attributes = null;

                DomNode customData = DomNode.GetChild(Schema.shaderType.customDataChild);
                if (customData != null)
                {
                    attributes = new DomNodeListAdapter<object>(customData, Schema.customDataType.attributeChild);
                }
                return attributes;
            }
        }

        #region IRenderStateCreator Members

        /// <summary>
        /// Creates the render state object for this shader</summary>
        /// <returns>The render state representing this shader</returns>
        public RenderState CreateRenderState()
        {
            RenderState rs = new RenderState();
            rs.SolidColor = new Vec4F(1, 1, 1, 1);
            rs.WireframeColor = new Vec4F(1, 1, 1, 1);
            rs.RenderMode = RenderMode.Smooth | RenderMode.Lit | RenderMode.SolidColor | RenderMode.WireframeColor;

            rs.InheritState = ~(RenderMode.Alpha | RenderMode.DisableZBuffer | RenderMode.SolidColor);

            ParseBindingsForTexture(rs);
            ParseCustomAttributesForLightingInfo(rs);

            return rs;
        }

        private void ParseCustomAttributesForLightingInfo(RenderState rs)
        {
            IEnumerable attributes = CustomAttributes;
            
            if (attributes != null)
            {
                float[] alpha = null;

                foreach (DomNode attribute in attributes)
                {
                    string name = attribute.GetAttribute(Schema.customDataAttributeType.nameAttribute) as string;
                    if (name == "color")
                    {
                        float[] diffuse = new float[3];
                        PopulateCustomAttributeValueArray(
                            attribute.GetAttribute(Schema.customDataAttributeType.valueAttribute) as string, diffuse);
                        rs.SolidColor = new Vec4F(diffuse[0], diffuse[1], diffuse[2], 1.0f);
                    }
                    else if (name == "specularColor")
                    {
                        float[] specular = new float[3];
                        PopulateCustomAttributeValueArray(
                            attribute.GetAttribute(Schema.customDataAttributeType.valueAttribute) as string, specular);
                    }
                    else if (name == "transparency")
                    {
                        alpha = new float[3];
                        PopulateCustomAttributeValueArray(
                            attribute.GetAttribute(Schema.customDataAttributeType.valueAttribute) as string, alpha);
                    }
                }

                if (alpha != null)
                {
                    Vec4F color = new Vec4F(rs.SolidColor);
                    color[3] = alpha[0];
                    rs.SolidColor = color;
                    rs.RenderMode |= RenderMode.Alpha;
                }
            }
        }

        private void PopulateCustomAttributeValueArray(string value, float[] array)
        {
            int i = 0;
            string[] values = value.Split(' ');
            foreach (string v in values)
            {
                array[i] = Convert.ToSingle(v);
                ++i;
            }
        }
        
        private void ParseBindingsForTexture(RenderState rs)
        {
            foreach (IBinding binding in Bindings)
            {
                if (binding.BindingType == "texture")
                {
                    DomNode textureObject = (DomNode)binding.Source;
                    ITexture texture = textureObject.As<ITexture>();

                    string texUri = texture.PathName;

                    int texName = Global<TextureManager>.Instance.GetTextureName(texUri);
                    
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

        #endregion
    }
}

