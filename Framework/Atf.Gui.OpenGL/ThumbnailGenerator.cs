//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.IO;

using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Thumbnail generator, using a Scea.OpenGL.BitmapContext to render a scene into
    /// a bitmap</summary>
    public class ThumbnailGenerator
    {
        /// <summary>
        /// Creates the rendering context, in preparation for thumbnail rendering</summary>
        /// <param name="width">Thumbnail width, in pixels. Must be a multiple of 4.</param>
        /// <param name="height">Thumbnail height, in pixels</param>
        public void CreateRenderContext(int width, int height)
        {
            m_bitmapContext = new BitmapContext(width, height);
        }

        /// <summary>
        /// Destroys rendering context after thumbnails have been rendered</summary>
        public void DestroyRenderContext()
        {
            m_bitmapContext.Dispose();
            m_bitmapContext = null;
        }

        /// <summary>
        /// Generates a thumbnail image and saves it to the given path as a PNG-format file</summary>
        /// <param name="scene">Scene to render</param>
        /// <param name="action">Rendering action</param>
        /// <param name="path">Path of bitmap file, in the PNG format. The extension must be "png".</param>
        /// <param name="camera">Camera to use</param>
        public void Generate(Scene scene, IRenderAction action, Camera camera, string path)
        {
            if (Path.GetExtension(path) != ".png")
                throw new ArgumentException("the extension must be 'png'");

            m_scene = scene;
            m_action = action;
            m_camera = camera;

            // Paint the scene and save it.
            using (Bitmap bitmap = m_bitmapContext.CreateBitmap(Paint))
                bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);

            m_scene = null;
            m_action = null;
            m_camera = null;
        }

        /// <summary>
        /// The method that is passed to the BitmapContext for rendering the image using OpenGl</summary>
        private void Paint()
        {
            // Setup render state. Copied from RenderCommands.DoCommand()'s RenderSmooth.
            RenderState rs = new RenderState();
            rs.RenderMode =
                RenderMode.Smooth |
                RenderMode.SolidColor |
                RenderMode.Lit |
                RenderMode.CullBackFace |
                RenderMode.Textured;
            rs.SolidColor = new Vec4F(1, 1, 1, 1);
            m_scene.StateStack.Push(rs);

            // Change some settings related to printing information text. We don't want it.
            string originalTitle = m_action.Title;
            m_action.Title = string.Empty;
            bool originalStatsEnabled = Util3D.RenderStats.Enabled;
            Util3D.RenderStats.Enabled = false;

            // Dispatch the scene
            m_action.Dispatch(m_scene, m_camera);

            // Restore some settings.
            m_action.Title = originalTitle;
            Util3D.RenderStats.Enabled = originalStatsEnabled;
        }

        private Scene m_scene;
        private IRenderAction m_action;
        private Camera m_camera;

        private BitmapContext m_bitmapContext;
    }
}
