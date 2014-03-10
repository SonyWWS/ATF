//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Applications;

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

        public Size MarginSize { get; set; }

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
            return IsBounded(items);
        }

        /// <summary>
        /// Frames the items in the current view</summary>
        /// <param name="items">Items to frame</param>
        public void Frame(IEnumerable<object> items)
        {
            var bounds = GetBounds(items);
            bounds.Inflate(MarginSize);
            m_transformAdapter.Frame(bounds);
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
            m_transformAdapter.EnsureVisible(GetBounds(items));
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
            bounds.Inflate(MarginSize);
            return bounds;
        }

        private readonly ITransformAdapter m_transformAdapter;        
    }
}
