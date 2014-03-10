//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// OBSOLETE. Please use D2dAnnotationAdapter instead.
    /// Adapter that allows AdaptableControl to display and edit diagram
    /// annotations (comments)</summary>
    public class AnnotationAdapter : DraggingControlAdapter,
        IPickingAdapter,
        IPrintingAdapter,
        IItemDragAdapter,
        IDisposable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="theme">Diagram rendering theme</param>
        public AnnotationAdapter(DiagramTheme theme)
        {
            m_theme = theme;
            m_theme.Redraw += theme_Redraw;
        }

        /// <summary>
        /// Disposes of unmanaged resources</summary>
        public virtual void Dispose()
        {
            if (m_theme != null)
            {
                m_theme.Redraw -= theme_Redraw;
                m_theme = null;
            }
        }

        private void theme_Redraw(object sender, EventArgs e)
        {
            AdaptedControl.Invalidate();
        }

        /// <summary>
        /// Class to hold picking results from diagram annotations</summary>
        public class AnnotationHitEventArgs : DiagramHitRecord
        {
            /// <summary>
            /// Constructor, nothing hit</summary>
            public AnnotationHitEventArgs()
            {
            }

            /// <summary>
            /// Constructor, annotation hit</summary>
            /// <param name="annotation">Annotation item</param>
            /// <param name="label">Editable text part, or null if hit on edge</param>
            public AnnotationHitEventArgs(IAnnotation annotation, DiagramLabel label)
            {
                Item = annotation;
                Part = label;
            }

            /// <summary>
            /// Gets the annotation item</summary>
            public IAnnotation Annotation
            {
                get { return Item as IAnnotation; }
            }

            /// <summary>
            /// Gets the editable text part, or null if hit on edge</summary>
            public DiagramLabel Label
            {
                get { return Part as DiagramLabel; }
            }
        }

        /// <summary>
        /// Performs a pick operation on the diagram annotations</summary>
        /// <param name="p">Picking point</param>
        /// <returns>Information about which annotation, if any, was hit by point</returns>
        public AnnotationHitEventArgs Pick(Point p)
        {
            if (m_annotatedDiagram != null)
            {
                if (m_transformAdapter != null)
                    p = GdiUtil.InverseTransform(m_transformAdapter.Transform, p);

                foreach (IAnnotation annotation in m_annotatedDiagram.Annotations)
                {
                    Rectangle bounds = GetBounds(annotation);
                    int tolerance = m_theme.PickTolerance;
                    bounds.Inflate(tolerance, tolerance);
                    if (bounds.Contains(p))
                    {
                        bounds.Inflate(tolerance * -2, tolerance * -2);
                        DiagramLabel label = null;
                        if (bounds.Contains(p))
                            label = new DiagramLabel(bounds, TextFormatFlags.Default);
                        
                        return new AnnotationHitEventArgs(annotation, label);
                    }
                }
            }

            return new AnnotationHitEventArgs();
        }

        #region IPickingAdapter Members

        DiagramHitRecord IPickingAdapter.Pick(Point p)
        {
            return Pick(p);
        }

        IEnumerable<object> IPickingAdapter.Pick(Region region)
        {
            if (m_annotatedDiagram == null)
                return EmptyEnumerable<object>.Instance;

            List<object> hit = new List<object>();
            RectangleF bounds;
            using (Graphics g = AdaptedControl.CreateGraphics())
                bounds = region.GetBounds(g);

            if (m_transformAdapter != null)
                bounds = GdiUtil.InverseTransform(m_transformAdapter.Transform, bounds);

            foreach (IAnnotation annotation in m_annotatedDiagram.Annotations)
            {
                Rectangle annotationBounds = GetBounds(annotation);
                if (bounds.IntersectsWith(annotationBounds))
                    hit.Add(annotation);
            }

            return hit;
        }

        Rectangle IPickingAdapter.GetBounds(IEnumerable<object> items)
        {
            Rectangle bounds = new Rectangle();
            foreach (IAnnotation annotation in items.AsIEnumerable<IAnnotation>())
            {
                Rectangle annotationBounds = GetBounds(annotation);
                bounds = bounds.IsEmpty ? annotationBounds : Rectangle.Union(bounds, annotationBounds);
            }

            if (!bounds.IsEmpty &&
                m_transformAdapter != null)
            {
                bounds = m_transformAdapter.TransformToClient(bounds);
            }

            return bounds;
        }

        #endregion

        #region IPrintingAdapter Members

        void IPrintingAdapter.Print(PrintDocument printDocument, Graphics g)
        {
            switch (printDocument.PrinterSettings.PrintRange)
            {
                case PrintRange.Selection:
                    PrintSelection(g);
                    break;

                default:
                    PrintAll(g);
                    break;
            }
        }

        private void PrintSelection(Graphics g)
        {
            if (m_selectionContext != null)
            {
                foreach (IAnnotation annotation in m_selectionContext.GetSelection<IAnnotation>())
                    DrawAnnotation(annotation, DiagramDrawingStyle.Normal, g);
            }
        }

        private void PrintAll(Graphics g)
        {
            foreach (IAnnotation annotation in m_annotatedDiagram.Annotations)
                DrawAnnotation(annotation, DiagramDrawingStyle.Normal, g);
        }

        #endregion

        #region IItemDragAdapter Members

        /// <summary>
        /// Begins dragging any selected items managed by the adapter. May be called
        /// by another adapter when it begins dragging.</summary>
        /// <param name="initiator">Control adapter that is initiating the drag</param>
        void IItemDragAdapter.BeginDrag(ControlAdapter initiator)
        {
            // drag all selected annotations
            m_annotationSet = new HashSet<IAnnotation>();
            List<IAnnotation> draggingAnnotations = new List<IAnnotation>();
            foreach (IAnnotation annotation in m_selectionContext.GetSelection<IAnnotation>())
            {
                draggingAnnotations.Add(annotation);
                m_annotationSet.Add(annotation);
            }

            m_draggingAnnotations = draggingAnnotations.ToArray();
            m_newPositions = new Point[m_draggingAnnotations.Length];
            m_oldPositions = new Point[m_draggingAnnotations.Length];
            for (int i = 0; i < m_draggingAnnotations.Length; i++)
            {
                // Initialize m_newPositions in case the mouse up event occurs before
                //  a paint event, which can happen during rapid clicking.
                Point currentLocation = m_draggingAnnotations[i].Bounds.Location;
                m_newPositions[i] = currentLocation;
                m_oldPositions[i] = currentLocation;
            }
        }

        void IItemDragAdapter.EndingDrag()
        {
        }

        /// <summary>
        /// Ends dragging any selected items managed by the adapter. May be called
        /// by another adapter when it ends dragging.</summary>
        void IItemDragAdapter.EndDrag()
        {
            int i = 0;
            foreach (IAnnotation annotation in m_draggingAnnotations)
            {
                MoveAnnotation(annotation, m_newPositions[i]);
                i++;
            }

            m_draggingAnnotations = null;
            m_annotationSet = null;
            m_newPositions = null;
            m_oldPositions = null;

            AdaptedControl.Invalidate();
        }

        #endregion

        /// <summary>
        /// Binds the adapter to the adaptable control; called in the order that the adapters
        /// were defined on the control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            m_transformAdapter = control.As<ITransformAdapter>();
            m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();

            control.ContextChanged += control_ContextChanged;
            control.Paint += control_Paint;

            base.Bind(control);
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.ContextChanged -= control_ContextChanged;
            control.Paint -= control_Paint;

            base.Unbind(control);
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            IAnnotatedDiagram annotatedContext = AdaptedControl.ContextAs<IAnnotatedDiagram>();
            if (m_annotatedDiagram != annotatedContext)
            {
                if (m_annotatedDiagram != null)
                {
                    if (m_observableContext != null)
                    {
                        m_observableContext.ItemInserted -= context_ObjectInserted;
                        m_observableContext.ItemRemoved -= context_ObjectRemoved;
                        m_observableContext.ItemChanged -= context_ObjectChanged;
                        m_observableContext.Reloaded -= context_Reloaded;
                        m_observableContext = null;
                    }
                    if (m_selectionContext != null)
                    {
                        m_selectionContext.SelectionChanged -= selection_Changed;
                        m_selectionContext = null;
                    }
                }

                m_annotatedDiagram = annotatedContext;

                if (m_annotatedDiagram != null)
                {
                    m_observableContext = AdaptedControl.ContextAs<IObservableContext>();
                    if (m_observableContext != null)
                    {
                        m_observableContext.ItemInserted += context_ObjectInserted;
                        m_observableContext.ItemRemoved += context_ObjectRemoved;
                        m_observableContext.ItemChanged += context_ObjectChanged;
                        m_observableContext.Reloaded += context_Reloaded;
                    }

                    m_selectionContext = AdaptedControl.ContextAs<ISelectionContext>();
                    if (m_selectionContext != null)
                    {
                        m_selectionContext.SelectionChanged += selection_Changed;
                    }

                    m_layoutContext = AdaptedControl.ContextAs<ILayoutContext>();
                }
            }
        }

        private void control_Paint(object sender, PaintEventArgs e)
        {
            if (m_annotatedDiagram == null)
                return;

            Graphics g = e.Graphics;
            Matrix oldTransform = null;
            PointF delta = Delta;
            if (m_transformAdapter != null)
            {
                // modify the transform so it is only a translation and uniform scale
                oldTransform = e.Graphics.Transform;
                float[] m = m_transformAdapter.Transform.Elements;
                float scale = Math.Min(m[0], m[3]);
                Matrix newTransform = new Matrix(scale, m[1], m[2], scale, m[4], m[5]);
                g.Transform = newTransform;

                delta = GdiUtil.InverseTransformVector(newTransform, delta);
            }

            // draw all annotations in their current position, ghosting those which are dragging
            foreach (IAnnotation annotation in m_annotatedDiagram.Annotations)
            {
                DiagramDrawingStyle style = DiagramDrawingStyle.Normal;
                if (m_annotationSet != null &&
                    m_annotationSet.Contains(annotation))
                {
                    style = DiagramDrawingStyle.Ghosted;
                }
                else if (m_selectionContext.SelectionContains(annotation))
                {
                    style = DiagramDrawingStyle.Selected;
                    if (m_selectionContext.GetLastSelected<IAnnotation>() == annotation)
                        style = DiagramDrawingStyle.LastSelected;
                }
                DrawAnnotation(annotation, style, g);
            }

            if (m_draggingAnnotations != null)
            {
                // set dragged nodes' positions, offsetting by drag delta and applying layout constraints
                for (int i = 0; i < m_draggingAnnotations.Length; i++)
                {
                    IAnnotation annotation = m_draggingAnnotations[i];
                    Rectangle oldBounds = GetBounds(annotation);
                    Rectangle newBounds = oldBounds;
                    newBounds.X += (int)delta.X;
                    newBounds.Y += (int)delta.Y;
                    m_newPositions[i] = newBounds.Location;
                    m_layoutContext.SetBounds(annotation, newBounds, BoundsSpecified.Location);

                    DrawAnnotation(annotation, DiagramDrawingStyle.Normal, g);

                    m_layoutContext.SetBounds(annotation, oldBounds, BoundsSpecified.Location);
                }
            }

            if (m_transformAdapter != null)
            {
                g.Transform = oldTransform;
            }
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseMove events; base method should
        /// be called first</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Mouse event args</param>
        protected override void OnMouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(sender, e);

            if (e.Button == MouseButtons.None)
            {
                AnnotationHitEventArgs hitRecord = Pick(CurrentPoint);
                if ((hitRecord.Annotation != null && hitRecord.Label == null) &&
                    AdaptedControl.Cursor == Cursors.Default)
                {
                    AdaptedControl.Cursor = Cursors.SizeAll;
                }
            }
        }

        /// <summary>
        /// Performs custom actions when performing a mouse dragging operation</summary>
        /// <param name="e">Mouse event args</param>
        protected override void OnDragging(MouseEventArgs e)
        {
            if (m_layoutContext != null &&
                m_draggingAnnotations == null)
            {
                if (e.Button == MouseButtons.Left &&
                    ((Control.ModifierKeys & Keys.Alt) == 0) &&
                    !AdaptedControl.Capture)
                {
                    {
                        m_mousePick = Pick(FirstPoint);
                        if (m_mousePick.Item != null)
                        {
                            m_initiated = true;

                            foreach (IItemDragAdapter itemDragAdapter in AdaptedControl.AsAll<IItemDragAdapter>())
                                itemDragAdapter.BeginDrag(this);

                            AdaptedControl.Capture = true;

                            if (m_autoTranslateAdapter != null)
                                m_autoTranslateAdapter.Enabled = true;
                        }
                    }
                }
            }

            if (m_draggingAnnotations != null)
            {
                AdaptedControl.Invalidate();
            }
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseUp events; base method should
        /// be called first</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Mouse event args</param>
        protected override void OnMouseUp(object sender, MouseEventArgs e)
        {
            base.OnMouseUp(sender, e);

            if (e.Button == MouseButtons.Left)
            {
                if (m_draggingAnnotations != null)
                {
                    if (m_initiated)
                    {
                        ITransactionContext transactionContext = AdaptedControl.ContextAs<ITransactionContext>();
                        transactionContext.DoTransaction(
                            delegate
                            {
                                foreach (IItemDragAdapter itemDragAdapter in AdaptedControl.AsAll<IItemDragAdapter>())
                                    itemDragAdapter.EndDrag();
                            }, "Drag Items".Localize());

                        m_initiated = false;
                    }
                }

                if (m_autoTranslateAdapter != null)
                    m_autoTranslateAdapter.Enabled = false;
            }
        }

        private void selection_Changed(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void context_ObjectInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            Invalidate();
        }

        private void context_ObjectRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            Invalidate();
        }

        private void context_ObjectChanged(object sender, ItemChangedEventArgs<object> e)
        {
            Invalidate();
        }

        private void context_Reloaded(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void Invalidate()
        {
            AdaptedControl.Invalidate();
        }

        private void DrawAnnotation(IAnnotation annotation, DiagramDrawingStyle style, Graphics g)
        {
            Rectangle bounds = annotation.Bounds;
            if (bounds.Size.IsEmpty)
            {
                // calculate size of text block
                SizeF textSizeF = g.MeasureString(annotation.Text, m_theme.Font);
                Size textSize = new Size((int)Math.Ceiling(textSizeF.Width), (int)Math.Ceiling(textSizeF.Height));
                bounds.Size = textSize;
                annotation.SetTextSize(textSize);
            }
            g.FillRectangle(SystemBrushes.Info, bounds);
            if ((style & DiagramDrawingStyle.Ghosted) == 0)
            {
                g.DrawString(annotation.Text, m_theme.Font, SystemBrushes.WindowText, bounds);

                Pen pen = null;
                if ((style & DiagramDrawingStyle.LastSelected) != 0)
                    pen = m_theme.LastHighlightPen;
                else if ((style & DiagramDrawingStyle.Selected) != 0)
                    pen = m_theme.HighlightPen;

                if (pen != null)
                    g.DrawRectangle(pen, bounds);
            }
        }

        private void MoveAnnotation(IAnnotation annotation, Point location)
        {
            Rectangle bounds;
            m_layoutContext.GetBounds(annotation, out bounds);
            bounds.Location = location;
            m_layoutContext.SetBounds(annotation, bounds, BoundsSpecified.Location);
        }

        // Gets a possibly imperfect bounding box around the annotation. Only DrawAnnotation, when
        //  using the Graphics object, can be perfect.
        private Rectangle GetBounds(IAnnotation annotation)
        {
            Rectangle bounds = annotation.Bounds;
            if (bounds.Size.IsEmpty)
            {
                // calculate size of text block
                SizeF textSizeF = TextRenderer.MeasureText(annotation.Text, m_theme.Font);
                Size textSize = new Size((int)textSizeF.Width, (int)textSizeF.Height);
                bounds.Size = textSize;

                // Don't update the IAnnotation. http://forums.ship.scea.com/jive/thread.jspa?threadID=10751
                //annotation.SetTextSize(textSize);
            }
            return bounds;
        }

        private DiagramTheme m_theme;
        private ITransformAdapter m_transformAdapter;
        private IAutoTranslateAdapter m_autoTranslateAdapter;

        private IAnnotatedDiagram m_annotatedDiagram;
        private IObservableContext m_observableContext;
        private ISelectionContext m_selectionContext;
        private ILayoutContext m_layoutContext;

        private AnnotationHitEventArgs m_mousePick;
        private IAnnotation[] m_draggingAnnotations;
        private HashSet<IAnnotation> m_annotationSet;
        private Point[] m_newPositions;
        private Point[] m_oldPositions;
        private bool m_initiated;
    }
}
