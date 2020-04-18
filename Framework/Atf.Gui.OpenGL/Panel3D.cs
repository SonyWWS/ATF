//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

using OpenTK;

namespace Sce.Atf.Rendering.OpenGL
{
    /// <summary>
    /// A control for using OpenGL to do the painting</summary>
    /// <remarks>This class's constructor initializes OpenGL so that other tools that use OpenGL, such as
    /// the Texture Manager, work even if BeginPaint has not been called. This allows
    /// Panel3D to work in a tabbed interface like the FAST Editor.</remarks>
    public class Panel3D : GLControl
    {
        /// <summary>
        /// Constructor</summary>
        /// <remarks>This constructor initializes OpenGL so that other tools that use OpenGL, such as
        /// the Texture Manager, work even if BeginPaint has not been called. This allows
        /// Panel3D to work in a tabbed interface like the FAST Editor.</remarks>
        public Panel3D()
        {
            RenderStateGuardianUtils.InitRenderStateGuardian(m_renderStateGuardian);
            this.MakeCurrent();
            StartGlIfNecessary();
        }

        /// <summary>
        /// Gets the render state guardian</summary>
        public RenderStateGuardian RenderStateGuardian
        {
            get { return m_renderStateGuardian; }
        }

        /// <summary>
        /// Disposes resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
             StopGl(disposing);
             base.Dispose(disposing);
        }

        /// <summary>
        /// Performs custom actions after the PaintBackground event occurs</summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions after the <see cref="E:System.Windows.Forms.Control.FontChanged"></see> event event occurs</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (m_isStarted)
                Invalidate();
        }

        /// <summary>
        /// Begins painting</summary>
        /// <exception cref="InvalidOperationException">Can't make this panel's GL context to be current</exception>
        protected virtual void BeginPaint()
        {
            StartGlIfNecessary();
            this.MakeCurrent();
            if (!this.Context.IsCurrent)
            {
                Util3D.ReportErrors();
                throw new InvalidOperationException("Can't make this panel's GL context to be current");
            }
        }

        /// <summary>
        /// Whether the OpenGl buffers should be swapped by the EndPaint method. Is always set to true when
        /// EndPaint finishes. Default is true.</summary>
        protected bool vSwapBuffers = true;

        /// <summary>
        /// Ends painting</summary>
        protected virtual void EndPaint()
        {
            if (vSwapBuffers)
                SwapBuffers();
            vSwapBuffers = true;
        }

        /// <summary>
        /// Makes this panel's OpenGL context current</summary>
        protected void SetCurrentContext()
        {
            this.MakeCurrent();
            if (!this.Context.IsCurrent)
            {
                Util3D.ReportErrors();
                throw new InvalidOperationException("Can't make this panel's GL context to be current");
            }
        }

        /// <summary>
        /// Client entry to initialize custom OpenGL resources</summary>
        protected virtual void Initialize() 
        {
            
        }

        /// <summary>
        /// Client entry to unload custom OpenGL resources</summary>
        protected virtual void Shutdown() 
        {

        }

        private void StartGlIfNecessary()
        {
            if (!m_isStarted)
            {
                try
                {
                    // Attempt To Get A Device Context
                    m_hdc = this.WindowInfo.Handle;
                    if (m_hdc == IntPtr.Zero)
                        throw new InvalidOperationException("Can't get device context");

                    OpenGlCore.InitOpenGl(this, out m_hglrc);
                    Initialize();
                    m_isStarted = true;
                }
                catch (Exception ex)
                {
                    StopGl(false);
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                    Outputs.WriteLine(OutputMessageType.Info, ex.StackTrace);
                }
            }
        }

        private void StopGl(bool disposing)
        {
            OpenGlCore.ShutdownOpenGl(this, ref m_hglrc);

            if (disposing && (m_hdc != IntPtr.Zero))
            {
                base.Dispose();
                m_hdc = IntPtr.Zero;
            }

            m_isStarted = false;
        }

        private IntPtr m_hdc;
        private IntPtr m_hglrc;
        private bool m_isStarted;
        private readonly RenderStateGuardian m_renderStateGuardian = new RenderStateGuardian();
    }
}
