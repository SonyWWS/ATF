//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Applications;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adapter that defines a viewing context on the adapted control. Requires
    /// an ITransformAdapter.</summary>
    public class ViewingAdapter : ControlAdapter, IViewingContext
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="transformAdapter">Transform adapter</param>
        public ViewingAdapter(ITransformAdapter transformAdapter)
        {
            m_transformAdapter = transformAdapter;
        }

        /// <summary>
        /// Gets or sets bounding margin around objects</summary>
        public Size MarginSize { get; set; }


        /// <summary>
        /// Gets or sets whether toggle mode is enabled for frame operation.</summary>
        public bool ToggleFramingEnabled { get; set; }

        #region IViewingContext Members

        /// <summary>
        /// Returns whether the items can be framed in the current view</summary>
        /// <param name="items">Items to frame</param>
        /// <returns>True iff the items can be framed in the current view</returns>
        public bool CanFrame(IEnumerable<object> items)
        {
            // disallow framing if cursor is inside an annotation editing area   
            // this fixed the problem that 'A' or 'F' chars cannot be typed into a comment 
            // because these 2 chars happen to be shortcuts for ViewFrameSelection and ViewFrameAll commands.
            if (AdaptedControl.HasKeyboardFocus) 
                return false;
            return IsBounded(items) || (ToggleFramingEnabled && m_isUnframing);
        }


        // is framing or unframing, only used if ToggleFramingEnabled is true.
        private bool m_isUnframing;
        private Matrix3x2F m_unframeMtrx;
        /// <summary>
        /// Frames the items in the current view</summary>
        /// <param name="items">Items to frame</param>
        public void Frame(IEnumerable<object> items)
        {
            if (ToggleFramingEnabled)
            {
                if (m_isUnframing)
                {
                    m_isUnframing = false;
                    m_transformAdapter.SetTransform(m_unframeMtrx.M11, m_unframeMtrx.M22, m_unframeMtrx.DX, m_unframeMtrx.DY);
                    return;
                }
                m_isUnframing = true;
                m_unframeMtrx = m_transformAdapter.Transform;
            }



            var bounds = GetBounds(items);

            // transform bounds from client space to graph space.
            Matrix3x2F invXform = Matrix3x2F.Invert(m_transformAdapter.Transform);
            var gBounds = Matrix3x2F.Transform(invXform, bounds);            
            var crect = AdaptedControl.ClientRectangle;
            crect.Inflate(-MarginSize.Width, -MarginSize.Height);
            if (crect.Width < 1 || crect.Height < 1) return;

            float sx = MathUtil.Clamp(crect.Width / gBounds.Width, m_transformAdapter.MinScale.X, m_transformAdapter.MaxScale.X);
            float sy = MathUtil.Clamp(crect.Height / gBounds.Height, m_transformAdapter.MinScale.Y, m_transformAdapter.MaxScale.Y);
            float scale = Math.Min(sx, sy);
            crect.X +=(int) (crect.Width - gBounds.Width * scale) / 2;
            crect.Y += (int)(crect.Height - gBounds.Height * scale) / 2;
            float tx = crect.X - gBounds.X * scale;
            float ty = crect.Y - gBounds.Y * scale;
            m_transformAdapter.SetTransform(scale, scale, tx, ty);            
        }

        /// <summary>
        /// Returns whether the items can be made visible in the current view;
        /// they may not be centered as in the Frame method</summary>
        /// <param name="items">Items to show</param>
        /// <returns>True iff the items can be made visible in the current view</returns>
        public bool CanEnsureVisible(IEnumerable<object> items)
        {
            return IsBounded(items);
        }

        /// <summary>
        /// Ensures that the items are visible in the current view</summary>
        /// <param name="items">Items to show</param>
        public void EnsureVisible(IEnumerable<object> items)
        {
            var bounds = GetBounds(items);
            // check rectangle is already in the visible rect
            RectangleF clientRect = AdaptedControl.ClientRectangle;
            if (clientRect.Contains(bounds))
            {
                // already visible
                return;
            }
            Frame(items);            
        }

        #endregion

      
        private bool IsBounded(IEnumerable<object> items)
        {
            var oneItem = new object[1];

            foreach (IPickingAdapter pickingAdapter in AdaptedControl.AsAll<IPickingAdapter>())
            {
                foreach (object item in items)
                {
                    oneItem[0] = item;
                    Rectangle adapterBounds = pickingAdapter.GetBounds(oneItem);
                    if (!adapterBounds.IsEmpty)
                        return true;
                }
            }

            foreach (IPickingAdapter2 pickingAdapter in AdaptedControl.AsAll<IPickingAdapter2>())
            {
                foreach (object item in items)
                {
                    oneItem[0] = item;
                    Rectangle adapterBounds = pickingAdapter.GetBounds(oneItem);
                    if (!adapterBounds.IsEmpty)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Computes a rectangle bounding a collection of objects</summary>
        /// <param name="items">Enumeration of objects to find bounds for</param>
        /// <returns>Rectangle bounding a collection of objects</returns>
        protected Rectangle GetBounds(IEnumerable<object> items)
        {
            Rectangle bounds = new Rectangle();
            foreach (IPickingAdapter pickingAdapter in AdaptedControl.AsAll<IPickingAdapter>())
            {
                Rectangle adapterBounds = pickingAdapter.GetBounds(items);
                if (!adapterBounds.IsEmpty)
                {
                    bounds = bounds.IsEmpty ? adapterBounds : Rectangle.Union(bounds, adapterBounds);
                }
            }

            foreach (IPickingAdapter2 pickingAdapter in AdaptedControl.AsAll<IPickingAdapter2>())
            {
                Rectangle adapterBounds = pickingAdapter.GetBounds(items);
                if (!adapterBounds.IsEmpty)
                {
                    bounds = bounds.IsEmpty ? adapterBounds : Rectangle.Union(bounds, adapterBounds);
                }
            }            
            return bounds;
        }

        private readonly ITransformAdapter m_transformAdapter;        
    }
}
