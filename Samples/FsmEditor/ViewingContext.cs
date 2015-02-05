﻿//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Dom;

namespace FsmEditorSample
{
    /// <summary>
    /// Adapter that provides viewing functions for a finite state machine (FSM). It holds a reference to
    /// the AdaptableControl for viewing the FSM. It implements ILayoutContext,
    /// IViewingContext, and updates the control's canvas bounds during validation.</summary>
    public class ViewingContext : Validator, ILayoutContext, IViewingContext
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the viewing context's DomNode.
        /// Raises the Observer NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            m_fsm = DomNode.Cast<Fsm>();

            base.OnNodeSet();
        }

        /// <summary>
        /// Gets or sets the viewing Control. Set by the editor.</summary>
        public AdaptableControl Control
        {
            get { return m_control; }
            set
            {
                if (m_control != null)
                {
                    m_control.SizeChanged -= control_SizeChanged;
                    m_control.VisibleChanged -= control_VisibleChanged;
                }

                m_control = value;
                m_layoutConstraints = EmptyEnumerable<ILayoutConstraint>.Instance;

                if (m_control != null)
                {
                    m_layoutConstraints = m_control.AsAll<ILayoutConstraint>();
                    m_control.SizeChanged += control_SizeChanged;
                    m_control.VisibleChanged += control_VisibleChanged;
                }

                SetCanvasBounds();
            }
        }

        /// <summary>
        /// Gets the bounding rectangle of the FSM item in client coordinates</summary>
        /// <param name="item">FSM item to find bounding rectangle for</param>
        /// <returns>Bounding rectangle of the FSM item in client coordinates</returns>
        public Rectangle GetBounds(object item)
        {
            return GetBounds(new object[] { item });
        }

        /// <summary>
        /// Gets the bounding rectangle of the given FSM items in client coordinates</summary>
        /// <param name="items">FSM items to find bounding rectangle for</param>
        /// <returns>Bounding rectangle of the FSM items in client coordinates</returns>
        public Rectangle GetBounds(IEnumerable<object> items)
        {
            Rectangle bounds = new Rectangle();
            foreach (IPickingAdapter2 pickingAdapter in m_control.AsAll<IPickingAdapter2>())
            {
                Rectangle adapterBounds = pickingAdapter.GetBounds(items);
                if (!adapterBounds.IsEmpty)
                {
                    if (bounds.IsEmpty)
                        bounds = adapterBounds;
                    else
                        bounds = Rectangle.Union(bounds, adapterBounds);
                }
            }

            return bounds;
        }

        /// <summary>
        /// Gets the bounding rectangle of all FSM items in client coordinates</summary>
        /// <returns>bounding rectangle of all FSM items in client coordinates</returns>
        public Rectangle GetBounds()
        {
            List<object> items = new List<object>(m_fsm.States.AsIEnumerable<object>());
            items.AddRange(m_fsm.Annotations.AsIEnumerable<object>());
            Rectangle bounds = GetBounds(items);
            return bounds;
        }

        /// <summary>
        /// Gets an enumeration of the sequence of all items visible in the control</summary>
        /// <returns>An enumeration of all items visible in the control</returns>
        public IEnumerable<object> GetVisibleItems()
        {
            Rectangle windowBounds = m_control.As<ICanvasAdapter>().WindowBounds;            
            foreach (IPickingAdapter2 pickingAdapter in m_control.AsAll<IPickingAdapter2>())
            {
                foreach (object item in pickingAdapter.Pick(windowBounds))
                    yield return item;
            }
        }

        #region IViewingContext Members

        /// <summary>
        /// Returns whether the items can be framed in the current view</summary>
        /// <param name="items">Items to frame</param>
        /// <returns>True iff the items can be framed in the current view</returns>
        public bool CanFrame(IEnumerable<object> items)
        {
            return m_control.As<IViewingContext>().CanFrame(items);
        }

        /// <summary>
        /// Frames the items in the current view</summary>
        /// <param name="items">Items to frame</param>
        public void Frame(IEnumerable<object> items)
        {
            m_control.As<IViewingContext>().Frame(items);
        }

        /// <summary>
        /// Returns whether the items can be made visible in the current view;
        /// they may not be centered as in the Frame method</summary>
        /// <param name="items">Items to show</param>
        /// <returns>True iff the items can be made visible in the current view</returns>
        public bool CanEnsureVisible(IEnumerable<object> items)
        {
            return m_control.As<IViewingContext>().CanFrame(items);
        }

        /// <summary>
        /// Ensures that the items are visible in the current view</summary>
        /// <param name="items">Items to show</param>
        public void EnsureVisible(IEnumerable<object> items)
        {
            m_control.As<IViewingContext>().EnsureVisible(items);
        }

        #endregion

        #region ILayoutContext Members

        /// <summary>
        /// Returns the smallest rectangle that bounds the item</summary>
        /// <param name="item">Item</param>
        /// <param name="bounds">Bounding rectangle of item, in world coordinates</param>
        /// <returns>Value indicating which parts of bounding rectangle are meaningful</returns>
        BoundsSpecified ILayoutContext.GetBounds(object item, out Rectangle bounds)
        {
            State state = item.As<State>();
            if (state != null)
            {
                bounds = state.Bounds;
                return BoundsSpecified.All;
            }

            Annotation annotation = item.As<Annotation>();
            if (annotation != null)
            {
                bounds = annotation.Bounds;
                return BoundsSpecified.All;
            }

            bounds = new Rectangle();
            return BoundsSpecified.None;
        }

        /// <summary>
        /// Returns a value indicating which parts of the item's bounds can be set</summary>
        /// <param name="item">Item</param>
        /// <returns>Value indicating which parts of the item's bounds can be set</returns>
        BoundsSpecified ILayoutContext.CanSetBounds(object item)
        {
            if (item.Is<State>())
                return BoundsSpecified.All;

            if (item.Is<Annotation>())
                return BoundsSpecified.Location;

            return BoundsSpecified.None;
        }

        /// <summary>
        /// Sets the bounds of the item</summary>
        /// <param name="item">Item</param>
        /// <param name="bounds">New item bounds, in world coordinates</param>
        /// <param name="specified">Which parts of bounds to set</param>
        void ILayoutContext.SetBounds(object item, Rectangle bounds, BoundsSpecified specified)
        {
            bounds = ConstrainBounds(bounds, specified);

            var state = item.As<State>();
            if (state != null)
            {
                state.Bounds = WinFormsUtil.UpdateBounds(state.Bounds, bounds, specified);
            }
            else
            {
                var annotation = item.As<Annotation>();
                if (annotation != null)
                    annotation.Bounds = WinFormsUtil.UpdateBounds(annotation.Bounds, bounds, specified);
            }
        }

        private Rectangle ConstrainBounds(Rectangle bounds, BoundsSpecified specified)
        {
            foreach (ILayoutConstraint layoutConstraint in m_layoutConstraints)
                if (layoutConstraint.Enabled)
                    bounds = layoutConstraint.Constrain(bounds, specified);

            return bounds;
        }

        #endregion

        /// <summary>
        /// Raises the OnEnded event and performs custom processing</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">EventArgs containing event data</param>
        protected override void OnEnded(object sender, EventArgs e)
        {
            SetCanvasBounds();

            base.OnEnded(sender, e);
        }

        private void control_VisibleChanged(object sender, EventArgs e)
        {
            SetCanvasBounds();
        }

        private void control_SizeChanged(object sender, EventArgs e)
        {
            SetCanvasBounds();
        }

        private void SetCanvasBounds()
        {
            // update the control CanvasAdapter's bounds
            if (m_control != null &&
                m_control.Visible)
            {
                Rectangle bounds = GetBounds();

                ITransformAdapter transformAdapter = m_control.As<ITransformAdapter>();
                bounds = GdiUtil.InverseTransform(transformAdapter.Transform, bounds);

                // make canvas twice as large as it needs to be to give the user some room,
                //  or at least as large as the control's client area.
                Rectangle clientRect = m_control.ClientRectangle;
                bounds.Width = Math.Max(bounds.Width * 2, clientRect.Width);
                bounds.Height = Math.Max(bounds.Height * 2, clientRect.Height);

                ICanvasAdapter canvasAdapter = m_control.As<ICanvasAdapter>();
                canvasAdapter.Bounds = new Rectangle(0, 0, bounds.Right, bounds.Bottom);
            }
        }

        private Fsm m_fsm;
        private AdaptableControl m_control;
        private IEnumerable<ILayoutConstraint> m_layoutConstraints;
    }
}
