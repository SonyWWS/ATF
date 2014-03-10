//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA rendering effects</summary>
    public class Effect : DomNodeAdapter, IRenderStateCreator, IShader
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// This method is called each time the adapter is connected to its underlying node.
        /// Typically overridden by creators of DOM adapters.</summary>
        protected override void OnNodeSet()
        {
            m_renderState =
                CreateRenderStateFromProfileCG() ??
                CreateRenderStateFromProfileCOMMON() ??
                CreateRenderStateDefault();
        }

        #region IRenderStateCreator Members
        private RenderState m_renderState;
        /// <summary>
        /// Creates RenderState for the object</summary>
        /// <returns>RenderState for the object</returns>
        public RenderState CreateRenderState() 
        {
            return m_renderState;
        }

        #endregion

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
            get { return GetAttribute<string>(Schema.effect.nameAttribute); }
            set {  }
        }

        #endregion

        #region ProfileCG

        private RenderState CreateRenderStateFromProfileCG()
        {
            RenderState rs = null;

            // Get profile
            DomNode profile_CG = GetChild<DomNode>(Schema.effect.profile_CGChild);
            if (profile_CG == null)
                return rs;

            // Get newParams
            IList<DomNode> newParams = profile_CG.GetChildList(Schema.profile_CG.newparamChild);

            // Get sampler
            DomNode samplerParam = null;
            foreach (DomNode newparam in newParams)
            {
                string semantic = newparam.GetAttribute(Schema.cg_newparam.semanticAttribute) as string;
                if (semantic == "DIFFUSETEXTURE")
                {
                    samplerParam = newparam;
                    break;
                }
            }

            if (samplerParam == null)
                return rs;

            // sampler2D
            DomNode sampler2D = samplerParam.GetChild(Schema.cg_newparam.sampler2DChild);
            if (sampler2D == null)
                return rs;

            // surface
            string source = sampler2D.GetAttribute(Schema.cg_sampler2D.sourceAttribute) as string;
            DomNode surfaceParam = null;
            foreach (DomNode newParam in newParams)
            {
                string sid = newParam.GetAttribute(Schema.cg_newparam.sidAttribute) as string;
                if (source == sid)
                {
                    surfaceParam = newParam;
                    break;
                }
            }
            if (surfaceParam == null)
                return rs;

            DomNode surface = surfaceParam.GetChild(Schema.cg_newparam.surfaceChild);
            if (surface == null)
                return rs;


            rs = CreateRenderStateDefault();

            // init_from            
            IList<DomNode> init_fromList = surface.GetChildList(Schema.cg_surface_type.init_fromChild);
            if (init_fromList.Count > 0)
            {
                DomNode init_from = init_fromList[0];
                DomNode image = (DomNode)init_from.GetAttribute(Schema.fx_surface_init_from_common.Attribute);

                Uri uri = (Uri)image.GetAttribute(Schema.image.init_fromAttribute);
                BindTexture(uri, rs);
            }
            
            return rs;
        }

        #endregion

        #region ProfileCommon

        private RenderState CreateRenderStateFromProfileCOMMON()
        {
            RenderState rs = null;
                                 
            DomNode profile_COMMON =  this.DomNode.GetChild(Schema.effect.profile_COMMONChild);
            if (profile_COMMON == null)
                return rs;

            // Get technique
            DomNode technique_COMMON = profile_COMMON.GetChild(Schema.profile_COMMON.techniqueChild);
            if (technique_COMMON == null )
                return rs;

            // Get shading profile
            DomNode shader =
                technique_COMMON.GetChild(Schema.profile_COMMON_technique.blinnChild) ??
                technique_COMMON.GetChild(Schema.profile_COMMON_technique.constantChild) ??
                technique_COMMON.GetChild(Schema.profile_COMMON_technique.lambertChild) ??
                technique_COMMON.GetChild(Schema.profile_COMMON_technique.phongChild);
            if (shader == null)
                return rs;

            rs = CreateRenderStateDefault();



            //<xs:sequence>
            //  <xs:element name="emission" type="common_color_or_texture_type" minOccurs="0"/>
            //  <xs:element name="ambient" type="common_color_or_texture_type" minOccurs="0"/>
            //  <xs:element name="diffuse" type="common_color_or_texture_type" minOccurs="0"/>
            //  <xs:element name="specular" type="common_color_or_texture_type" minOccurs="0"/>
            //  <xs:element name="shininess" type="common_float_or_param_type" minOccurs="0"/>
            //  <xs:element name="reflective" type="common_color_or_texture_type" minOccurs="0"/>
            //  <xs:element name="reflectivity" type="common_float_or_param_type" minOccurs="0"/>
            //  <xs:element name="transparent" type="common_transparent_type" minOccurs="0"/>
            //  <xs:element name="transparency" type="common_float_or_param_type" minOccurs="0"/>
            //  <xs:element name="index_of_refraction" type="common_float_or_param_type" minOccurs="0"/>
            //</xs:sequence>


                       
            DomNodeType shaderType = shader.Type;
            ChildInfo emission = shaderType.GetChildInfo("emission");
            ChildInfo ambient = shaderType.GetChildInfo("ambient");
            ChildInfo diffuse = shaderType.GetChildInfo("diffuse");
            ChildInfo specular = shaderType.GetChildInfo("specular");
            ChildInfo shininess = shaderType.GetChildInfo("shininess");
            ChildInfo reflective = shaderType.GetChildInfo("reflective");
            ChildInfo reflectivity = shaderType.GetChildInfo("reflectivity");
            ChildInfo transparent = shaderType.GetChildInfo("transparent");
            ChildInfo transparency = shaderType.GetChildInfo("transparency");
            ChildInfo indexOfRefraction = shaderType.GetChildInfo("index_of_refrac");


            if (emission != null)
            {
                float[] emissionColor = Tools.GetColor(shader.GetChild(emission));
                if (emissionColor != null)
                    rs.EmissionColor = new Vec4F(emissionColor);
            }

            if (ambient != null)
            {
                // Surface properties
                float[] ambientColor = Tools.GetColor(shader.GetChild(ambient));
                if (ambientColor != null)
                    rs.AmbientColor = new Vec4F(ambientColor);
            }

            float transparencyVal = -1.0f;
            if (transparency != null)
            {
                transparencyVal = Tools.GetFloat(shader.GetChild(transparency));
            }

            if (diffuse != null)
            {
                float[] diffuseColor = Tools.GetColor(shader.GetChild(diffuse));
                if (diffuseColor != null)
                {
                    if (transparencyVal >= 0.0f)
                    {
                        diffuseColor[3] = transparencyVal;
                        if(transparencyVal < 1.0f) rs.RenderMode |= RenderMode.Alpha;
                    }
                    rs.DiffuseColor = new Vec4F(diffuseColor);
                }
            }


            if (specular != null)
            {
                float[] specularColor = Tools.GetColor(shader.GetChild(specular));
                if (specularColor != null)
                    rs.SpecularColor = new Vec4F(specularColor);

                if (shininess != null)
                {
                    float shine = Tools.GetFloat(shader.GetChild(shininess));
                    rs.Shininess = shine;
                }
            }

            if (diffuse == null)
                return rs;

            DomNode diffuseChild = shader.GetChild(diffuse);
            DomNode textureChild =  diffuseChild.GetChild(Schema.common_color_or_texture_type.textureChild);
            if (textureChild == null)
                return rs;


            string strTexture = (string)textureChild.GetAttribute(Schema.common_color_or_texture_type_texture.textureAttribute);
            if (string.IsNullOrEmpty(strTexture))
                return rs;


            IList<DomNode> newParams = profile_COMMON.GetChildList(Schema.profile_COMMON.newparamChild);
            DomNode sampler = null;
            foreach (DomNode newParam in newParams)
            {
                string sid = newParam.GetAttribute(Schema.common_newparam_type.sidAttribute) as string;
                if (strTexture == sid)
                {
                    sampler = newParam;
                    break;
                }
            }

            if (sampler == null)
                return rs;

            // sampler2D
            DomNode sampler2D = sampler.GetChild(Schema.common_newparam_type.sampler2DChild);
            if (sampler2D == null)
                return rs;

            string source = sampler2D.GetAttribute(Schema.fx_sampler2D_common.sourceAttribute) as string;

            DomNode surfaceParam = null;
            foreach (DomNode newParam in newParams)
            {
                string sid = newParam.GetAttribute(Schema.common_newparam_type.sidAttribute) as string;
                if (source == sid)
                {
                    surfaceParam = newParam;
                    break;
                }
            }
            if (surfaceParam == null)
                return rs;

            DomNode surface = surfaceParam.GetChild(Schema.common_newparam_type.surfaceChild);
            if (surface == null)
                return rs;

            // init_from
            //string id = (string)surface.GetAttribute(Schema.fx_surface_common.init_fromAttribute);
            IList<DomNode> init_fromList = surface.GetChildList(Schema.fx_surface_common.init_fromChild);
            if (init_fromList.Count > 0)
            {
                DomNode init_from = init_fromList[0];
                DomNode image =(DomNode)init_from.GetAttribute(Schema.fx_surface_init_from_common.Attribute);

                Uri uri = (Uri)image.GetAttribute(Schema.image.init_fromAttribute);                
                BindTexture(uri, rs);
            }
                       
            return rs;
        }

        #endregion
       
        private RenderState CreateRenderStateDefault()
        {
            return new RenderState 
            {
                InheritState = ~(RenderMode.Alpha | RenderMode.DisableZBuffer | RenderMode.SolidColor),
                RenderMode = RenderMode.Smooth | RenderMode.Lit | RenderMode.SolidColor | RenderMode.WireframeColor,
                SolidColor = new Vec4F(1, 1, 1, 1),
                WireframeColor = new Vec4F(1, 1, 1, 1)
            };
        }

     
        private void BindTexture(Uri relativeUri, RenderState rs)
        {
            Collada root = this.DomNode.GetRoot().As<Collada>();
            string fullPath = root.GetAbsolutePath(relativeUri);

            if (!System.IO.File.Exists(fullPath))
            {
                Outputs.WriteLine(OutputMessageType.Error, "Texture not found  " + fullPath);
                return;
            }

            // Load texture and set RenderState
            int texName = Global<TextureManager>.Instance.GetTextureName(fullPath);
            if (texName != -1)
            {
                rs.RenderMode |= RenderMode.Textured;
                rs.TextureName = texName;                
            }
        }
    }
}
