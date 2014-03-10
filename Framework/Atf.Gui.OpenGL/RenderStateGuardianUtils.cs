//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.VectorMath;

using Tao.OpenGl;

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
                    Gl.glDisable(Gl.GL_TEXTURE_2D);
                    Gl.glDisable(Gl.GL_CULL_FACE);
                    Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);

                    Gl.glPolygonOffset(-1.0f, -1.0f);
                    Gl.glEnable(Gl.GL_POLYGON_OFFSET_LINE);

                    Util3D.RenderStats.RenderStateChanges++;
                    //Console.WriteLine("CommitWire");
                }

                Vec4F color = renderState.WireframeColor;
                Gl.glColor4f(color.X, color.Y, color.Z, color.W);

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
                    Gl.glLineWidth(renderState.LineThickness);
            }
        }

        private static void CommitSmooth(RenderState renderState, RenderState previousRenderState)
        {
            if ((renderState.RenderMode & RenderMode.Smooth) != 0)
            {
                if (RenderStatesDiffer(previousRenderState, renderState, RenderMode.Smooth))
                {
                    Gl.glShadeModel(Gl.GL_SMOOTH);
                    Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
                    Util3D.RenderStats.RenderStateChanges++;
                    //Console.WriteLine("CommitSmooth");
                }

                Vec4F color = renderState.SolidColor;
                Gl.glColor4f(color.X, color.Y, color.Z, color.W);
            }
        }

        private static void CommitAlpha(RenderState renderState, RenderState previousRenderState)
        {
            if (RenderStatesDiffer(previousRenderState, renderState, RenderMode.Alpha))
            {
                if ((renderState.RenderMode & RenderMode.Alpha) != 0)
                {
                    Gl.glEnable(Gl.GL_BLEND);
                    Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                    //Gl.glDepthMask(Gl.GL_FALSE);
                }
                else
                {
                    Gl.glDisable(Gl.GL_BLEND);
                    //Gl.glDepthMask(Gl.GL_TRUE);
                }

                Util3D.RenderStats.RenderStateChanges++;
                //Console.WriteLine("CommitAlpha");
            }
        }

        private static void CommitLighting(RenderState renderState, RenderState previousRenderState)
        {
            if (RenderStatesDiffer(previousRenderState, renderState, RenderMode.Lit))
            {
                if ((renderState.RenderMode & RenderMode.Lit) != 0)
                {
                    Gl.glEnable(Gl.GL_LIGHTING);
                    Gl.glEnable(Gl.GL_LIGHT0);
                    Gl.glEnable(Gl.GL_NORMALIZE);
                }
                else
                    Gl.glDisable(Gl.GL_LIGHTING);

                Util3D.RenderStats.RenderStateChanges++;
                //Console.WriteLine("CommitLighting");
            }

            if ((renderState.RenderMode & RenderMode.Lit) != 0)
            {
                {
                    Vec4F colorVec = renderState.EmissionColor;
                    float[] color = { colorVec.X, colorVec.Y, colorVec.Z, colorVec.W };

                    Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_EMISSION, color);
                }
                {
                    Vec4F colorVec = renderState.AmbientColor;
                    float[] color = { colorVec.X, colorVec.Y, colorVec.Z, colorVec.W };

                    Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_AMBIENT, color);
                }
                {
                    Vec4F colorVec = renderState.SolidColor;
                    float[] color = { colorVec.X, colorVec.Y, colorVec.Z, colorVec.W };

                    Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_DIFFUSE, color);
                }
                {
                    Vec4F specularcolorVec = renderState.SpecularColor;
                    float[] color = { specularcolorVec.X, specularcolorVec.Y, specularcolorVec.Z, specularcolorVec.W };

                    Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_SPECULAR, color);
                }
                {
                    float Shininess = renderState.Shininess;
                    Gl.glMaterialf(Gl.GL_FRONT_AND_BACK, Gl.GL_SHININESS, Shininess);
                }
            }
        }

        private static void CommitZBuffer(RenderState renderState, RenderState previousRenderState)
        {
            if (RenderStatesDiffer(previousRenderState, renderState, RenderMode.DisableZBuffer))
            {
                if ((renderState.RenderMode & RenderMode.DisableZBuffer) != 0)
                    Gl.glDisable(Gl.GL_DEPTH_TEST);
                else
                {
                    Gl.glEnable(Gl.GL_DEPTH_TEST);
                    Gl.glDepthFunc(Gl.GL_LEQUAL);
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
                    Gl.glDepthMask(Gl.GL_FALSE);
                else
                    Gl.glDepthMask(Gl.GL_TRUE);

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
                    Gl.glEnable(Gl.GL_TEXTURE_2D);
                    Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);
                }
                else
                {
                    Gl.glDisable(Gl.GL_TEXTURE_2D);
                }
                Util3D.RenderStats.RenderStateChanges++;
                //Console.WriteLine("CommitTextures -- enable/disable textures");
            }

            if ((renderState.RenderMode & RenderMode.Textured) != 0)
            {
                if (textureBitChange ||
                    previousRenderState.TextureName != renderState.TextureName)
                {
                    Gl.glBindTexture(Gl.GL_TEXTURE_2D, renderState.TextureName);
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
                    Gl.glDisable(Gl.GL_CULL_FACE);
                else
                {
                    Gl.glEnable(Gl.GL_CULL_FACE);
                    Gl.glCullFace(Gl.GL_BACK);
                }

                Util3D.RenderStats.RenderStateChanges++;
                //Console.WriteLine("CommitCullBackFace");
            }
        }
    }
}
