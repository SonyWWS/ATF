//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using SharpDX.Direct2D1;
using Sce.Atf.VectorMath;


namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Renders drawing instructions to a control</summary>
    public class D2dHwndGraphics : D2dGraphics
    {
        /// <summary>
        /// Changes the size of the D2dGraphics to the specified pixel size</summary>
        /// <param name="pixelSize">The new size of the D2dGraphics in device pixels</param>
        public void Resize(System.Drawing.Size pixelSize)
        {
            var rt = (WindowRenderTarget)D2dRenderTarget;
            rt.Resize(new SharpDX.DrawingSize(pixelSize.Width, pixelSize.Height));
        }

        /// <summary>
        /// Gets the HWND associated with this D2dGraphics</summary>
        public IntPtr Hwnd 
        {
            get 
            {
                var rt = (WindowRenderTarget)D2dRenderTarget;
                return rt.Hwnd;
            }
        }
       
        /// <summary>
        /// Indicates whether the HWND associated with this D2dGraphics is occluded.</summary>        
        /// <returns>D2dWindowState value that indicates whether the HWND associated with
        /// this D2dGraphics is occluded.</returns>
        /// <remarks>
        /// Note that if the window was occluded the last time EndDraw was called, the
        /// next time the D2dGraphics calls CheckWindowState, it returns D2dWindowState.Occluded
        /// regardless of the current window state. If you want to use CheckWindowState
        /// to determine the current window state, you should call CheckWindowState after
        /// every EndDraw call and ignore its return value. This ensures that your
        /// next call to CheckWindowState state returns the actual window state.</remarks>
        public D2dWindowState CheckWindowState()
        {
            var rt = (WindowRenderTarget)D2dRenderTarget;
            return (D2dWindowState) rt.CheckWindowState();
        }

        protected override void RecreateRenderTarget()
        {
            var curRT = (WindowRenderTarget)D2dRenderTarget;
            Matrix3x2F xform = Transform;
            var hwnProps = new HwndRenderTargetProperties();
            hwnProps.Hwnd = Hwnd;
            hwnProps.PixelSize = curRT.PixelSize;
            hwnProps.PresentOptions = PresentOptions.Immediately;
            RenderTarget rt = new WindowRenderTarget(D2dFactory.NativeFactory, D2dFactory.RenderTargetProperties, hwnProps);            
            SetRenderTarget(rt);
            Transform = xform;           
        }

        internal D2dHwndGraphics(WindowRenderTarget renderTarget)
            : base(renderTarget)
        {
            
        }
    }
 
    /// <summary>
    /// Describes whether a window is occluded.</summary>
    /// <remarks>
    /// If the window was occluded the last time EndDraw was called, the next
    /// time the D2dGraphics calls CheckWindowState, it returns Occluded
    /// regardless of the current window state. If you want to use CheckWindowState
    /// to determine the current window state, you should call CheckWindowState after
    /// every EndDraw call and ignore its return value. This ensures that your
    /// next call to CheckWindowState state returns the actual window state.</remarks>    
    public enum D2dWindowState
    {        
        /// <summary>
        /// The window is not occluded.</summary>
        None = 0,

        /// <summary>
        /// The window is occluded.</summary>
        Occluded = 1,
    }

}
