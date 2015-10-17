//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Direct2D;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// DomNode adapter that provides viewing functions for a Circuit. It holds a reference to
    /// the AdaptableControl for viewing the Circuit. It implements ILayoutContext,
    /// IViewingContext, and updates the control's canvas bounds during validation.</summary>
    public class ViewingContext : Validator, IViewingContext, ILayoutContext
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the viewing context's DomNode.
        /// Raises the Observer NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            m_graph = DomNode.As<IGraph<IGraphNode, IGraphEdge<IGraphNode, IEdgeRoute>, IEdgeRoute>>();
            m_graphContainer = DomNode.As<ICircuitContainer>();
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
                m_moduleEditAdapter = null;

                if (m_control != null)
                {
                    m_layoutConstraints = m_control.AsAll<ILayoutConstraint>();
                    m_moduleEditAdapter = m_control.As<D2dGraphNodeEditAdapter<Element, Wire, ICircuitPin>>();
                    m_control.SizeChanged += control_SizeChanged;
                    m_control.VisibleChanged += control_VisibleChanged;

                }

                SetCanvasBounds();
            }
        }

        /// <summary>
        /// Make the viewing control accessible via DomNode IAdaptable</summary>
        /// <param name="type">Type for which to obtain adapter</param>
        /// <returns>Adapter for given type</returns>
        public override object GetAdapter(Type type)
        {
            if (type == typeof(AdaptableControl))
                return Control;
            return base.GetAdapter(type);
        }

        /// <summary>
        /// Gets the bounding rectangle of the circuit item in client coordinates</summary>
        /// <param name="item">Circuit item to find bounding rectangle for</param>
        /// <returns>Bounding rectangle of the circuit item in client coordinates</returns>
        public Rectangle GetBounds(object item)
        {
            return GetBounds(new object[] { item });
        }

        /// <summary>
        /// Gets the bounding rectangle of given circuit items in client coordinates</summary>
        /// <param name="items">Circuit items to find bounding rectangle for</param>
        /// <returns>Bounding rectangle of the circuit items in client coordinates</returns>
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
        /// Gets the bounding rectangle of all circuit items in client coordinates</summary>
        /// <returns>Bounding rectangle of all circuit items in client coordinates</returns>
        public Rectangle GetBounds()
        {
            var items = new List<object>();
            if (m_graphContainer != null)
            {
                items.AddRange(m_graphContainer.Elements.AsIEnumerable<object>());
                if (m_graphContainer.Annotations != null)
                    items.AddRange(m_graphContainer.Annotations.AsIEnumerable<object>());
            }
            else
            {
                if (m_graph != null)
                    items.AddRange(m_graph.Nodes.AsIEnumerable<IGraphNode>().AsIEnumerable<object>());
                //TODO: including Annotations 
            }

            Rectangle bounds = GetBounds(items);
            if (DomNode.Is<Group>()) // the view is associated with a group editor
            {
                // include group pins y range
                var group = DomNode.Cast<Group>();

                int yMin = int.MaxValue;
                int yMax = int.MinValue;

                foreach (var pin in group.InputGroupPins)
                {
                    var grpPin = pin.Cast<GroupPin>();
                    if (grpPin.Bounds.Location.Y < yMin)
                        yMin = grpPin.Bounds.Location.Y;
                    if (grpPin.Bounds.Location.Y > yMax)
                        yMax = grpPin.Bounds.Location.Y;
                }

                foreach (var pin in group.OutputGroupPins)
                {
                    var grpPin = pin.Cast<GroupPin>();
                    if (grpPin.Bounds.Location.Y < yMin)
                        yMin = grpPin.Bounds.Location.Y;
                    if (grpPin.Bounds.Location.Y > yMax)
                        yMax = grpPin.Bounds.Location.Y;
                }

                // transform y range to client space
                if (yMin != int.MaxValue && yMax != int.MinValue)
                {
                    var transformAdapter = m_control.Cast<ITransformAdapter>();
                    var yRange = D2dUtil.TransformVector(transformAdapter.Transform, new PointF(yMin, yMax));
                    yMin = (int)Math.Min(yRange.X, yRange.Y);
                    yMax = (int)Math.Max(yRange.X, yRange.Y);
                    int width = bounds.Width;
                    int height = yMax - yMin + 1;
                    bounds = Rectangle.Union(bounds, new Rectangle(bounds.Location.X, yMin, width, height));
                }

            }
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
        /// <returns><c>True</c> if the items can be framed in the current view</returns>
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
        /// <returns><c>True</c> if the items can be made visible in the current view</returns>
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
            var element = item.As<Element>();
            if (element != null)
            {
                bounds = GetBounds(element); // in client coordinates

                // transform to world coordinates
                var transformAdapter = m_control.Cast<ITransformAdapter>();
                bounds = GdiUtil.InverseTransform(transformAdapter.Transform, bounds);

                return BoundsSpecified.All;
            }

            var annotation = item.As<Annotation>();
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
            //if (item.Is<DomNode>())// debug disallowing dragging 
            //{
            //    var parent = item.Cast<DomNode>().Parent;
            //    if (parent.Is<Group>())
            //        return BoundsSpecified.None;
            //}

            // by default a group can be moved and resized
            if (item.Is<Group>()) 
                return BoundsSpecified.All;

            if (item.Is<Element>())
                return BoundsSpecified.Location;

            if (item.Is<Annotation>())
                return BoundsSpecified.All;

            return BoundsSpecified.None;
        }

        /// <summary>
        /// Sets the bounds of the item</summary>
        /// <param name="item">Item</param>
        /// <param name="bounds">New item bounds, in world coordinates</param>
        /// <param name="specified">Which parts of bounds to set</param>
        void ILayoutContext.SetBounds(object item, Rectangle bounds, BoundsSpecified specified)
        {
            var element = item.As<Element>();

            // Currently we have CanvasAdapter and D2dGridAdapter attached to the circuit adaptable control for layout constraints.  
            // When setting a dragging module’s bounds and the module belongs to a group, we are either moving around the module 
            // inside its owner, or moving out of its current owner. In either case, it seems more desirable to skip these 2 
            // layout constrains: the CanvasAdapter by default have (0,0) for Location which will clip away negative bounds, 
            // but negative location will be generated if we drag a sub-element to the left or above the group that owns it;  
            // D2dGridAdapter seems not needed to layout sub-elements inside its owner as the layout occurs at a lower level.
            bool applyConstraints = !IsDraggingSubNode(element);
            if (applyConstraints)
                bounds = ConstrainBounds(bounds, specified);

            if (element != null)
            {
                element.Bounds = WinFormsUtil.UpdateBounds(element.Bounds, bounds, specified);
            }
            else
            {
                var annotation = item.As<Annotation>();
                if (annotation != null)
                    annotation.Bounds = WinFormsUtil.UpdateBounds(annotation.Bounds, bounds, specified);
            }
        }

        private bool IsDraggingSubNode(Element element)
        {
            if (element != null && m_moduleEditAdapter != null && m_moduleEditAdapter.NodeDraggingPosition(element).HasValue)
            {
                if (element.DomNode.Parent.Is<Group>())
                    return true;
            }
            return false;
        }

        private Rectangle ConstrainBounds(Rectangle bounds, BoundsSpecified specified)
        {
            if (m_layoutConstraints != null)
            {
                foreach (ILayoutConstraint layoutConstraint in m_layoutConstraints)
                    if (layoutConstraint.Enabled)
                        bounds = layoutConstraint.Constrain(bounds, specified);

            }
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

        /// <summary>
        /// Updates the control CanvasAdapter's bounds</summary>
        protected virtual void SetCanvasBounds()
        {
            // update the control CanvasAdapter's bounds
            if (m_control != null &&
                m_control.Visible)
            {
                //get the bounds in client screen coordinates
                Rectangle bounds = GetBounds();

                //transform to world coordinates
                var transformAdapter = m_control.As<ITransformAdapter>();
                bounds = GdiUtil.InverseTransform(transformAdapter.Transform, bounds);

                // Make the canvas larger than it needs to be to give the user some room.
                // Use the client rectangle in world coordinates.
                Rectangle clientRect = GdiUtil.InverseTransform(transformAdapter.Transform, m_control.ClientRectangle);
                bounds.Width = Math.Max(bounds.Width * 2, clientRect.Width * 2);
                bounds.Height = Math.Max(bounds.Height * 2, clientRect.Height * 2);

                var canvasAdapter = m_control.As<ICanvasAdapter>();
                if (canvasAdapter != null)
                    canvasAdapter.Bounds = bounds;
            }
        }

        private IGraph<IGraphNode, IGraphEdge<IGraphNode, IEdgeRoute>, IEdgeRoute> m_graph;
        private ICircuitContainer m_graphContainer;
        private AdaptableControl m_control;
        private IEnumerable<ILayoutConstraint> m_layoutConstraints;
        private D2dGraphNodeEditAdapter<Element, Wire, ICircuitPin> m_moduleEditAdapter;
    }
}
