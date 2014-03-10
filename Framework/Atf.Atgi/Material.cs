//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Xml;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI material, representing the material element in an ATGI XML file. See
    /// http://wiki.ship.scea.com/confluence/display/SCEEATGDOCS/Materials+in+.atgi. </summary>
    public class Material : DomNodeAdapter, IShader, IRenderStateCreator
    {
        /// <summary>
        /// Gets and sets the Material name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.materialType.nameAttribute); }
            set { SetAttribute(Schema.materialType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets the list of shader bindings (names and types of data) used by this Material</summary>
        public IList<IBinding> Bindings
        {
            get { return GetChildList<IBinding>(Schema.materialType.bindingChild); }
        }

        /// <summary>
        /// Gets the list of child elements of the 'customData' element that corresponds to
        /// this material</summary>
        public IList<object> CustomAttributes
        {
            get
            {
                IList<object> attributes = null;

                DomNode customData = DomNode.GetChild(Schema.materialType.customDataChild);
                if (customData != null)
                {
                    attributes = new DomNodeListAdapter<object>(customData, Schema.customDataType.attributeChild);
                }
                return attributes;
            }
        }

        /// <summary>
        /// Gets or sets the location of the *.material file that defines the shader implementation</summary>
        public Uri Url
        {
            get { return GetAttribute<Uri>(Schema.materialType.urlAttribute); }
            set { SetAttribute(Schema.materialType.urlAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the name of the AtgMaterialsExportDefinition element within the *.material file</summary>
        public Uri Mat
        {
            get { return GetAttribute<Uri>(Schema.materialType.matAttribute); }
            set { SetAttribute(Schema.materialType.matAttribute, value); }
        }

        #region IRenderStateCreator Members

        /// <summary>
        /// Creates the render state object for this material, including finding and loading the
        /// diffuse texture, if any</summary>
        /// <returns>The render state representing this material</returns>
        public virtual RenderState CreateRenderState()
        {
            RenderState rs = new RenderState();
            rs.SolidColor = new Vec4F(1, 1, 1, 1);
            rs.WireframeColor = new Vec4F(1, 1, 1, 1);
            rs.RenderMode = RenderMode.Smooth | RenderMode.Lit | RenderMode.SolidColor | RenderMode.WireframeColor;

            rs.InheritState = ~(RenderMode.Alpha | RenderMode.DisableZBuffer | RenderMode.SolidColor);

            ParseBindingsForTexture(rs);

            return rs;
        }

        #endregion

        /// <summary>
        /// The default tag that specifies the diffuse texture in a material element in
        /// a *.atgi file. Used by CreateRenderState().</summary>
        public static string DiffuseTextureTag = "diffuseTexture";

        /// <summary>
        /// Parses the bindings within this material to load the diffuse texture and set the
        /// RenderState. The diffuse texture is determined by the following preference:
        /// 1. A MaterialBinding whose type is "texture" and whose Tag is "diffuseTexture". See
        /// http://wiki.ship.scea.com/confluence/display/SCEEATGDOCS/Materials+in+.atgi.
        /// 2. The first binding whose type is "texture".</summary>
        /// <param name="rs">The RenderState object to be set for this material</param>
        private void ParseBindingsForTexture(RenderState rs)
        {
            IBinding bestMatch = null;
            foreach (IBinding binding in Bindings)
            {
                if (binding.BindingType == "texture")
                {
                    MaterialBinding matBinding = binding as MaterialBinding;
                    if (matBinding != null && matBinding.Tag == DiffuseTextureTag)
                    {
                        bestMatch = binding;
                        break;
                    }
                    else if (bestMatch == null)
                    {
                        bestMatch = binding;
                    }
                }
            }

            if (bestMatch != null)
            {
                DomNode textureObject = (DomNode)bestMatch.Source;
                ITexture texture = textureObject.As<ITexture>();

                int texId = Global<TextureManager>.Instance.GetTextureName(texture.PathName);

                if (texId != -1)
                {
                    rs.RenderMode |= RenderMode.Textured;
                    rs.TextureName = texId;

                    TextureInfo info = Global<TextureManager>.Instance.GetTextureInfo(texId);
                    if (info.Components == 4 &&
                        AlphaBlend)
                    {
                        rs.RenderMode |= RenderMode.Alpha;
                    }
                }
            }
        }

        private bool AlphaBlend
        {
            get
            {
                CheckMaterialCache();
                return m_cachedProperties.AlphaBlend;
            }
        }

        // for lazy loading and parsing of the material's XML file
        private void CheckMaterialCache()
        {
            if (m_cachedProperties != null)
                return;

            string fileName;
            if (Url.IsAbsoluteUri)
                fileName = Url.LocalPath;
            else
                fileName = Url.OriginalString;
            m_cachedProperties = new MaterialProperties(fileName);
        }

        private class MaterialProperties
        {
            public MaterialProperties(string filename)
            {
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(filename);
                    XmlElement root = doc.DocumentElement;

                    XmlElement guiGraph = GetChildByElementName(root, "GuiGraph");
                    if (guiGraph == null)
                        return;

                    XmlElement atgInterface = GetChildByElementName(guiGraph, "AtgInterface");
                    if (atgInterface == null)
                        return;

                    XmlElement guiPropertyBag = GetChildByElementName(atgInterface, "GuiPropertyBag");
                    if (guiPropertyBag == null)
                        return;

                    foreach (XmlElement property in guiPropertyBag.ChildNodes)
                    {
                        if (property.Attributes["Name"].Value == "Alpha Blend")
                        {
                            m_alphaBlend = (property.Attributes["Value"].Value == "Yes");
                        }
                    }
                }
                catch //file not found, bad uri, etc., are not really fatal errors. We may still have the diffuse texture, for example.
                {
                }
            }

            public bool AlphaBlend
            {
                get { return m_alphaBlend; }
            }

            private XmlElement GetChildByElementName(XmlElement element, string childName)
            {
                foreach (XmlElement child in element.ChildNodes)
                {
                    if (child.Name == childName)
                    {
                        return child;
                    }
                }
                return null;
            }

            // the default is 'true' so that if the material file is missing, the texture can decide
            private bool m_alphaBlend = true;
        }

        // Call CheckMaterialCache() before accessing, to ensure that it is not null.
        private MaterialProperties m_cachedProperties;
    }
}

