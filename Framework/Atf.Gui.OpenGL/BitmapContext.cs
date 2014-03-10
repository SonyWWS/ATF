//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Tao.OpenGl;

namespace Sce.Atf.Rendering.OpenGL
{
    /// <summary>
    /// A rendering context for OpenGL for creating bitmaps</summary>
    public class BitmapContext : Panel3D
    {
        /// <summary>
        /// Constructs an uncompressed RGB buffer of the specific size and bit depth</summary>
        /// <param name="width">Bitmap width. Must be a multiple of 4.</param>
        /// <param name="height">Bitmap height</param>
        public BitmapContext(int width, int height)
        {
            // The Bitmap constructor requires the stride to be a multiple of 4. Since the #
            //  of bytes per pixel is currently hard-coded to be 3, the stride is equal to
            //  3 * 'width', so 'width' must be a multiple of 4.
            if ((width & 0x3) != 0)
                throw new ArgumentException("the width must be a multiple of 4");

            // initialize this Control
            ClientSize = new System.Drawing.Size(width, height);
            m_width = width;
            m_height = height;
        }

        /// <summary>
        /// Provides a callback method to allow the caller to render the scene that is
        /// turned into a bitmap. The caller must not set a different OpenGl rendering
        /// context (e.g., by calling wglMakeCurrent).</summary>
        public delegate void RenderScene();

        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (m_form != null)
            {
                m_form.Dispose();
                m_form = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Renders a scene into an off-screen buffer and creates a finished bitmap of the results</summary>
        /// <param name="paintCallback">The delegate for rendering the scene. The correct
        /// OpenGl rendering context is set prior to executing this delegate.</param>
        /// <returns>A bitmap of the desired width and height that shows the rendered image</returns>
        public Bitmap CreateBitmap(RenderScene paintCallback)
        {
            if (OpenGlCore.FrameBufferObjectSupported)
            {
                // Set up a FBO with one renderbuffer attachment and one depth buffer attachment.
                Gl.glGenFramebuffersEXT(1, out m_framebuffer);
                Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, m_framebuffer);

                Gl.glGenRenderbuffersEXT(1, out m_renderbuffer);
                Gl.glBindRenderbufferEXT(Gl.GL_RENDERBUFFER_EXT, m_renderbuffer);
                Gl.glRenderbufferStorageEXT(Gl.GL_RENDERBUFFER_EXT, Gl.GL_RGBA8, m_width, m_height);
                Gl.glFramebufferRenderbufferEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_COLOR_ATTACHMENT0_EXT, Gl.GL_RENDERBUFFER_EXT, m_renderbuffer);

                Gl.glGenFramebuffersEXT(1, out m_depthBuffer);
                Gl.glBindRenderbufferEXT(Gl.GL_RENDERBUFFER_EXT, m_depthBuffer);
                Gl.glRenderbufferStorageEXT(Gl.GL_RENDERBUFFER_EXT, Gl.GL_DEPTH_COMPONENT24, m_width, m_height);
                Gl.glFramebufferRenderbufferEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_DEPTH_ATTACHMENT_EXT, Gl.GL_RENDERBUFFER_EXT, m_depthBuffer);

                int status = Gl.glCheckFramebufferStatusEXT(Gl.GL_FRAMEBUFFER_EXT);
                if (status != Gl.GL_FRAMEBUFFER_COMPLETE_EXT)
                    throw new InvalidOperationException("couldn't create frame buffer object");
            }
            else
            {
                m_form = new Form();
                m_form.ClientSize = new Size(Width, Height);
                m_form.Controls.Add(this);
            }
            
            
            Gl.glPushAttrib(Gl.GL_VIEWPORT_BIT);
            Gl.glViewport(0, 0, m_width, m_height);

            //Util3D.ReportErrors();
            
            base.BeginPaint();
            paintCallback();
            base.EndPaint();


            // Allocate a bitmap with its own memory.
            
            // Panel3D uses 32 bits for the frame buffer. We don't need the alpha channel.
            System.Drawing.Imaging.PixelFormat pixelFormat =
                System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            Bitmap bitmap = new Bitmap(m_width, m_height, pixelFormat);
            
            // Copy pixels
            BitmapData lockedPixelData = null;
            try
            {
                lockedPixelData = bitmap.LockBits(
                    new Rectangle(0, 0, m_width, m_height),
                    ImageLockMode.ReadWrite,
                    pixelFormat);
                
                // Read from the default OpenGl frame buffer into the Bitmap's pixels.
                // The given coordinates are from the lower-left corner and will need 
                // to be flipped vertically.
                IntPtr pixels = lockedPixelData.Scan0;
                Gl.glReadPixels(
                    0, 0,
                    m_width, m_height,
                    Gl.GL_BGR_EXT,
                    Gl.GL_UNSIGNED_BYTE,
                    pixels);
                //Util3D.ReportErrors();
            }
            finally
            {
                if (lockedPixelData != null)
                    bitmap.UnlockBits(lockedPixelData);
            }
            
            //Util3D.ReportErrors();
            Gl.glPopAttrib();

            if (OpenGlCore.FrameBufferObjectSupported)
            {
                Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, 0);
                
                // Clean-up
                Gl.glDeleteRenderbuffersEXT(1, ref m_depthBuffer);
                Gl.glDeleteRenderbuffersEXT(1, ref m_renderbuffer);
                Gl.glDeleteFramebuffersEXT(1, ref m_framebuffer);
                //Util3D.ReportErrors();
            }
            
            
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bitmap;
        }
     

        private Form m_form;
        private int m_framebuffer, m_renderbuffer, m_depthBuffer;
        readonly int m_width;
        readonly int m_height;
    }
}
