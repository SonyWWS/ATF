﻿//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D
{    
    /// <summary>
    /// Renders to an intermediate bitmap created by the CreateCompatibleRenderTarget method</summary>
    public class D2dBitmapGraphics : D2dGraphics
    {
        /// <summary>
        /// Retrieves the bitmap for this D2dGraphics. The returned bitmap can be used
        /// for drawing operations.</summary>     
        /// <returns>D2dBitmap for this D2dGraphics</returns>
        public D2dBitmap GetBitmap()
        {
            var rt = (BitmapRenderTarget)D2dRenderTarget;

            // pass m_owner instead of "this"
            // To allow disposing D2dBitmap when render target recreated.
            return new D2dBitmap(m_owner, rt.Bitmap);
        }

        /// <summary>
        /// Recreates the render target, if necessary, by calling SetRenderTarget</summary>
        protected override void RecreateRenderTarget()
        {
            // Do not recreate D2dBitmapGraphics. Let the user do that 
            // by handling the RecreateResources event from the D2dGraphics 
            // that created this D2dBitmapGraphics.
        }

        internal D2dBitmapGraphics(D2dGraphics owner,BitmapRenderTarget renderTarget)
            : base(renderTarget)
        {
            m_owner = owner;
        }

        private readonly D2dGraphics m_owner;
    }

    /// <summary>
    /// Specifies additional features supportable by a compatible D2dGraphics when
    /// it is created. This enumeration allows a bitwise combination of its member values.</summary>
    /// <remarks>    
    /// The GdiCompatible option may only be requested if the parent D2dGraphics was created with the
    /// GdiCompatible option</remarks>
    public enum D2dCompatibleGraphicsOptions
    {        
        /// <summary>
        /// The D2dGraphics supports no additional features</summary>
        None = 0,
                
        /// <summary>
        /// The D2dGraphics supports interoperability with the Windows Graphics Device Interface (GDI)</summary>
        GdiCompatible = 1,
    }
}
