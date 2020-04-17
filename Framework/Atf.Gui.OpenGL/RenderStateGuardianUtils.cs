//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.VectorMath;

//using Tao.OpenGl;
using OTK = OpenTK.Graphics;

namespace Sce.Atf.Rendering.OpenGL
{
    /// <summary>
    /// OpenGL-specific render state commit functions</summary>
    public static class RenderStateGuardianUtils
    {
        /// <summary>
        /// Registers all of the default render state handlers on the supplied RenderStateGuardian</summary>
        /// <param name="rsg">RenderStateGuardian to register default handlers on</param>
        public static void InitRenderStateGuardian(RenderStateGuardian rsg)
        {
            rsg.RegisterRenderStateHandler((int)RenderMode.Smooth, CommitSmooth);
            rsg.RegisterRenderStateHandler((int)RenderMode.Wireframe, CommitWire);
            rsg.RegisterRenderStateHandler((int)RenderMode.Alpha, CommitAlpha);
            rsg.RegisterRenderStateHandler((int)RenderMode.Lit, CommitLighting);
            rsg.RegisterRenderStateHandler((int)RenderMode.DisableZBuffer, CommitZBuffer);
            rsg.RegisterRenderStateHandler((int)RenderMode.Textured, CommitTextures);
            rsg.RegisterRenderStateHandler((int)RenderMode.CullBackFace, CommitCullBackFace);
            rsg.RegisterRenderStateHandler((int)RenderMode.DisableZBufferWrite, CommitDepthMask);
        }

        private static bool RenderStatesDiffer(RenderState oldRenderState, RenderState newRenderState, RenderMode flag)
        {
            bool oldRenderStateNull = (oldRenderState == null);
            bool newFlag = ((newRenderState.RenderMode & flag) != 0);

            if (oldRenderStateNull)
                return true;

            bool oldFlag = ((oldRenderState.RenderMode & flag) != 0);

            return oldFlag != newFlag;
        }

        private static void CommitWire(RenderState renderState, RenderState previousRenderState)
        {
            if ((renderState.RenderMode & RenderMode.Wireframe) != 0)
            {
                if (RenderStatesDiffer(previousRenderState, renderState, RenderMode.Wireframe))
                {
                    OTK.OpenGL.GL.Disable(OTK.OpenGL.EnableCap.Texture2D);
                    OTK.OpenGL.GL.Disable(OTK.OpenGL.EnableCap.CullFace);
                    OTK.OpenGL.GL.PolygonMode(OTK.OpenGL.MaterialFace.FrontAndBack, OTK.OpenGL.PolygonMode.Fill);

                    OTK.OpenGL.GL.PolygonOffset(-1.0f, -1.0f);
                    OTK.OpenGL.GL.Enable(OTK.OpenGL.EnableCap.PolygonOffsetLine);

                    Util3D.RenderStats.RenderStateChanges++;
                    //Console.WriteLine("CommitWire");
                }

                Vec4F color = renderState.WireframeColor;
                OTK.OpenGL.GL.Color4(color.X, color.Y, color.Z, color.W);

                bool makeGlCall = false;

                if (previousRenderState == null)
                    makeGlCall = true;
                else
                {
                    bool previousRsWasSmooth = ((previousRenderState.RenderMode & RenderMode.Smooth) != 0);
                    bool previousRsHadDifferentThickness = (previousRenderState.LineThickness != renderState.LineThickness);

                    makeGlCall = previousRsWasSmooth || previousRsHadDifferentThickness;
                }

                if (makeGlCall)
                    OTK.OpenGL.GL.LineWidth(renderState.LineThickness);
            }
        }

        private static void CommitSmooth(RenderState renderState, RenderState previousRenderState)
        {
            if ((renderState.RenderMode & RenderMode.Smooth) != 0)
            {
                if (RenderStatesDiffer(previousRenderState, renderState, RenderMode.Smooth))
                {
                    OTK.OpenGL.GL.ShadeModel(OTK.OpenGL.ShadingModel.Smooth);
                    OTK.OpenGL.GL.PolygonMode(OTK.OpenGL.MaterialFace.FrontAndBack, OTK.OpenGL.PolygonMode.Fill);
                    Util3D.RenderStats.RenderStateChanges++;
                    //Console.WriteLine("CommitSmooth");
                }

                Vec4F color = renderState.SolidColor;
                OTK.OpenGL.GL.Color4(color.X, color.Y, color.Z, color.W);
            }
        }

        private static void CommitAlpha(RenderState renderState, RenderState previousRenderState)
        {
            if (RenderStatesDiffer(previousRenderState, renderState, RenderMode.Alpha))
            {
                if ((renderState.RenderMode & RenderMode.Alpha) != 0)
                {
                    OTK.OpenGL.GL.Enable(OTK.OpenGL.EnableCap.Blend);
                    OTK.OpenGL.GL.BlendFunc(OTK.OpenGL.BlendingFactor.SrcAlpha, OTK.OpenGL.BlendingFactor.OneMinusSrcAlpha);
                    //Gl.glDepthMask(Gl.GL_FALSE);
                }
                else
                {
                    OTK.OpenGL.GL.Disable(OTK.OpenGL.EnableCap.Blend);
                }

                Util3D.RenderStats.RenderStateChanges++;
            }
        }

        private static void CommitLighting(RenderState renderState, RenderState previousRenderState)
        {
            if (RenderStatesDiffer(previousRenderState, renderState, RenderMode.Lit))
            {
                if ((renderState.RenderMode & RenderMode.Lit) != 0)
                {
                    OTK.OpenGL.GL.Enable(OTK.OpenGL.EnableCap.Lighting);
                    OTK.OpenGL.GL.Enable(OTK.OpenGL.EnableCap.Light0);
                    OTK.OpenGL.GL.Enable(OTK.OpenGL.EnableCap.Normalize);
                }
                else
                    OTK.OpenGL.GL.Disable(OTK.OpenGL.EnableCap.Lighting);

                Util3D.RenderStats.RenderStateChanges++;
                //Console.WriteLine("CommitLighting");
            }

            if ((renderState.RenderMode & RenderMode.Lit) != 0)
            {
                {
                    Vec4F colorVec = renderState.EmissionColor;
                    float[] color = { colorVec.X, colorVec.Y, colorVec.Z, colorVec.W };

                    OTK.OpenGL.GL.Material(OTK.OpenGL.MaterialFace.FrontAndBack, OTK.OpenGL.MaterialParameter.Emission, color);
                }
                {
                    Vec4F colorVec = renderState.AmbientColor;
                    float[] color = { colorVec.X, colorVec.Y, colorVec.Z, colorVec.W };

                    OTK.OpenGL.GL.Material(OTK.OpenGL.MaterialFace.FrontAndBack, OTK.OpenGL.MaterialParameter.Ambient, color);
                }
                {
                    Vec4F colorVec = renderState.SolidColor;
                    float[] color = { colorVec.X, colorVec.Y, colorVec.Z, colorVec.W };

                    OTK.OpenGL.GL.Material(OTK.OpenGL.MaterialFace.FrontAndBack, OTK.OpenGL.MaterialParameter.Diffuse, color);
                }
                {
                    Vec4F specularcolorVec = renderState.SpecularColor;
                    float[] color = { specularcolorVec.X, specularcolorVec.Y, specularcolorVec.Z, specularcolorVec.W };

                    OTK.OpenGL.GL.Material(OTK.OpenGL.MaterialFace.FrontAndBack, OTK.OpenGL.MaterialParameter.Specular, color);
                }
                {
                    float Shininess = renderState.Shininess;
                    OTK.OpenGL.GL.Material(OTK.OpenGL.MaterialFace.FrontAndBack, OTK.OpenGL.MaterialParameter.Shininess, Shininess);
                }
            }
        }

        private static void CommitZBuffer(RenderState renderState, RenderState previousRenderState)
        {
            if (RenderStatesDiffer(previousRenderState, renderState, RenderMode.DisableZBuffer))
            {
                if ((renderState.RenderMode & RenderMode.DisableZBuffer) != 0)
                    OTK.OpenGL.GL.Disable(OTK.OpenGL.EnableCap.DepthTest);
                else
                {
                    OTK.OpenGL.GL.Enable(OTK.OpenGL.EnableCap.DepthTest);
                    OTK.OpenGL.GL.DepthFunc(OTK.OpenGL.DepthFunction.Lequal);
                }

                Util3D.RenderStats.RenderStateChanges++;
                //Console.WriteLine("CommitZBuffer");
            }
        }

        private static void CommitDepthMask(RenderState renderState, RenderState previousRenderState)
        {
            if (RenderStatesDiffer(previousRenderState, renderState, RenderMode.DisableZBufferWrite))
            {
                if ((renderState.RenderMode & RenderMode.DisableZBufferWrite) != 0)
                    OTK.OpenGL.GL.DepthMask(false);
                else
                    OTK.OpenGL.GL.DepthMask(true);

                Util3D.RenderStats.RenderStateChanges++;
                //Console.WriteLine("CommitDepthMask");
            }
        }

        private static void CommitTextures(RenderState renderState, RenderState previousRenderState)
        {
            bool textureBitChange = RenderStatesDiffer(previousRenderState, renderState, RenderMode.Textured);
            if (textureBitChange)
            {
                if ((renderState.RenderMode & RenderMode.Textured) != 0)
                {
                    OTK.OpenGL.GL.Enable(OTK.OpenGL.EnableCap.Texture2D);
                    OTK.OpenGL.GL.TexEnv(OTK.OpenGL.TextureEnvTarget.TextureEnv, OTK.OpenGL.TextureEnvParameter.TextureEnvMode, (float)OTK.OpenGL.TextureEnvMode.Modulate);
                }
                else
                {
                    OTK.OpenGL.GL.Disable(OTK.OpenGL.EnableCap.Texture2D);
                }
                Util3D.RenderStats.RenderStateChanges++;
                //Console.WriteLine("CommitTextures -- enable/disable textures");
            }

            if ((renderState.RenderMode & RenderMode.Textured) != 0)
            {
                if (textureBitChange ||
                    previousRenderState.TextureName != renderState.TextureName)
                {
                    OTK.OpenGL.GL.BindTexture(OTK.OpenGL.TextureTarget.Texture2D, renderState.TextureName);
                    Util3D.RenderStats.RenderStateChanges++;
                    //Console.WriteLine("CommitTextures -- new texture {0}", renderState.TextureName);
                }
            }
        }

        private static void CommitCullBackFace(RenderState renderState, RenderState previousRenderState)
        {
            if (RenderStatesDiffer(previousRenderState, renderState, RenderMode.CullBackFace) ||
                RenderStatesDiffer(previousRenderState, renderState, RenderMode.Wireframe))
            {
                if ((renderState.RenderMode & RenderMode.CullBackFace) == 0)
                    OTK.OpenGL.GL.Disable(OTK.OpenGL.EnableCap.CullFace);
                else
                {
                    OTK.OpenGL.GL.Enable(OTK.OpenGL.EnableCap.CullFace);
                    OTK.OpenGL.GL.CullFace(OTK.OpenGL.CullFaceMode.Back);
                }

                Util3D.RenderStats.RenderStateChanges++;
                //Console.WriteLine("CommitCullBackFace");
            }
        }
    }
}
