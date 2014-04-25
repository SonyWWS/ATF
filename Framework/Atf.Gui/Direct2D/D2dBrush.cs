//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Defines an object that paints an area</summary>
    public abstract class D2dBrush : D2dResource
    {
        /// <summary>
        /// Gets and sets Opacity. Opacity ranges from 0.0 to 1.0.</summary>
        public float Opacity
        {
            get { return NativeBrush.Opacity; }
            set { NativeBrush.Opacity = value; }
        }

        /// <summary>
        /// Gets the D2dGraphics object that was used to create this D2dBrush</summary>
        public D2dGraphics Owner
        {
            get {return m_owner;}
        }

        /// <summary>
        /// Gets native brush resource</summary>
        /// <remarks>This SharpDX brush is for internal use and should not have been made available to client code.
        /// It may be removed in the future. Please don't use it.</remarks>
        protected internal Brush NativeBrush
        {
            get;
            internal set;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or 
        /// resetting unmanaged resources</summary>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                m_owner.RecreateResources -= RecreateResources;
                // NativeBrush has a finalizer and will dispose of its unmanaged resources.
                NativeBrush.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Create or recreate native resource. Is called when render-target
        /// needs to be recreated.</summary>
        internal virtual void Create()
        {

        }

        internal D2dBrush(D2dGraphics owner)
        {
            m_owner = owner;
            m_owner.RecreateResources += RecreateResources;
            m_rtNumber = owner.RenderTargetNumber;
        }

        private void RecreateResources(object sender, EventArgs e)
        {
            // validation before recreating resource
            if (IsDisposed)
            {// should not recreate disposed resource
                m_owner.RecreateResources -= RecreateResources;
                return;
            }

            if (m_rtNumber == m_owner.RenderTargetNumber)
            {// this resource does not need to be recreated
                return;
            }            

            Create();

            m_rtNumber = m_owner.RenderTargetNumber;
        }

        private readonly D2dGraphics m_owner;
        private uint m_rtNumber; // rt number of the owner at construction time
    }


    /// <summary>
    /// Specifies how a brush paints areas outside of its normal content area</summary>
    /// <remarks>
    /// For a D2dBitmapBrush, the brush's content is the brush's
    /// bitmap. For an D2dLinearGradientBrush, the brush's content
    /// area is the gradient axis. For a D2dRadialGradientBrush,
    /// the brush's content is the area within the gradient ellipse.</remarks>
    public enum D2dExtendMode
    {
        /// <summary>
        /// Repeat the edge pixels of the brush's content for all regions outside the
        /// normal content area.</summary>
        Clamp = 0,

        /// <summary>
        /// Repeat the brush's content.</summary>
        Wrap = 1,

        /// <summary>
        /// The same as Wrap, except that alternate tiles of the brush's
        /// content are flipped. (The brush's normal content is drawn untransformed.)</summary>
        Mirror = 2,
    }
}
