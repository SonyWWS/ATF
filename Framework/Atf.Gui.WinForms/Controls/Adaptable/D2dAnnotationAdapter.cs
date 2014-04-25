//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.VectorMath;
using Sce.Atf.Direct2D;
using Sce.Atf.DirectWrite;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adapter that allows AdaptableControl to display and edit diagram
    /// annotations (comments)</summary>
    public class D2dAnnotationAdapter : DraggingControlAdapter,
        IPickingAdapter2,
        IItemDragAdapter,
       // IPrintingAdapter,        
        IDisposable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="theme">Diagram rendering theme</param>
        public D2dAnnotationAdapter(D2dDiagramTheme theme)
        {
            m_theme = theme;
            m_theme.Redraw += theme_Redraw;
        }

        /// <summary>
        /// Disposes of unmanaged resources</summary>
        public virtual void Dispose()
        {
            //Don't set m_theme to null because there can be timing issues where m_theme is needed
            //  even after Dispose is called.
                m_theme.Redraw -= theme_Redraw;
        }

        private void theme_Redraw(object sender, EventArgs e)
        {
            if (AdaptedControl != null)
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
            /// <param name="border">Specifies a hit on an item's border part</param>
            public AnnotationHitEventArgs(IAnnotation annotation, DiagramBorder border)
            {
                Item = annotation;
                Part = border;
            }

            /// <summary>
            /// Constructor, annotation hit</summary>
            /// <param name="annotation">Annotation item</param>
            /// <param name="scrollBar">Specifies a hit on an item's scroll bar part</param>
            public AnnotationHitEventArgs(IAnnotation annotation, DiagramScrollBar scrollBar)
            {
                Item = annotation;
                Part = scrollBar;
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
            /// Constructor, annotation hit</summary>
            /// <param name="annotation">Annotation item</param>
            /// <param name="titleBar">the title bar at the top of the annotation</param>
            public AnnotationHitEventArgs(IAnnotation annotation, DiagramTitleBar titleBar)
            {
                Item = annotation;
                Part = titleBar;
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

            /// <summary>
            /// Gets the border part, or null if not hit on edge</summary>
            public DiagramBorder Border
            {
                get { return Part as DiagramBorder; }
            }

            /// <summary>
            /// Hit position in Textlayout local space
            /// </summary>
            public PointF Position { get; set; }
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

                foreach (IAnnotation annotation in m_annotatedDiagram.Annotations.Reverse())
                {
                    Rectangle bounds = GetBounds(annotation);
                    if (bounds.IsEmpty)
                        continue;
                    var inflated = bounds;
                    int tolerance = m_theme.PickTolerance;
                    inflated.Inflate(tolerance, tolerance);
                    if (inflated.Contains(p) && m_annotationEditors.ContainsKey(annotation))
                    {
                        // check titlebar
                        var titlebarRect = new RectangleF(bounds.Left + 2 * tolerance, bounds.Y - tolerance,
                            bounds.Width -  4 * tolerance, Margin.Top + tolerance);
                        if (titlebarRect.Contains(p))
                        {                          
                            return new AnnotationHitEventArgs(annotation, new DiagramTitleBar(annotation));
                        }

                        var annotationData = m_annotationEditors[annotation];
                        // check scroll bar
                        if (annotationData.VerticalScrollBarVisibe)
                        {
                            var scrollbarRect = new RectangleF(bounds.Right - Margin.Right - ScrollBarWidth - 2 * ScrollBarMargin, bounds.Y,
                                ScrollBarWidth + 2 * ScrollBarMargin, bounds.Height);
                            if (scrollbarRect.Contains(p))
                                return new AnnotationHitEventArgs(annotation, new DiagramScrollBar(annotation, Orientation.Vertical));
                        }
                       

                        // check borders
                        // check corners first
                        var border = new RectangleF(bounds.Right - 2 * tolerance, bounds.Bottom - 2 * tolerance, 4 * tolerance, 4 * tolerance);
                        if (border.Contains(p))
                        {
                            var borderPart = new DiagramBorder(annotation)
                            {
                                Border = DiagramBorder.BorderType.LowerRightCorner
                            };
                            return new AnnotationHitEventArgs(annotation, borderPart);
                        }

                        border = new RectangleF(bounds.Left - 2 * tolerance, bounds.Top - 2 * m_theme.PickTolerance, 4 * tolerance, 4 * tolerance);
                        if (border.Contains(p))
                        {
                            var borderPart = new DiagramBorder(annotation)
                            {
                                Border = DiagramBorder.BorderType.UpperLeftCorner
                            };
                            return new AnnotationHitEventArgs(annotation, borderPart);
                        }

                        border = new RectangleF(bounds.Right - 2 * m_theme.PickTolerance, bounds.Top - 2 * m_theme.PickTolerance, 4 * tolerance, 4 * tolerance);
                        if (border.Contains(p))
                        {
                            var borderPart = new DiagramBorder(annotation)
                            {
                                Border = DiagramBorder.BorderType.UpperRightCorner
                            };
                            return new AnnotationHitEventArgs(annotation, borderPart);
                        }

                        border = new RectangleF(bounds.Left - 2 * m_theme.PickTolerance, bounds.Bottom - 2 * m_theme.PickTolerance, 4 * tolerance, 4 * tolerance);
                        if (border.Contains(p))
                        {
                            var borderPart = new DiagramBorder(annotation)
                            {
                                Border = DiagramBorder.BorderType.LowerLeftCorner
                            };
                            return new AnnotationHitEventArgs(annotation, borderPart);
                        }

                        border = new RectangleF(bounds.Left - m_theme.PickTolerance, bounds.Y, 2 * m_theme.PickTolerance, bounds.Height);
                        if (border.Contains(p))
                        {
                            var borderPart = new DiagramBorder(annotation)
                                {
                                    Border = DiagramBorder.BorderType.Left
                                };
                            return new AnnotationHitEventArgs(annotation, borderPart);
                        }
                        border.Offset(bounds.Width, 0);
                        if (border.Contains(p))
                        {
                            var borderPart = new DiagramBorder(annotation)
                                {
                                    Border = DiagramBorder.BorderType.Right
                                };
                            return new AnnotationHitEventArgs(annotation, borderPart);
                        }
                        border = new RectangleF(bounds.Left, bounds.Y - m_theme.PickTolerance, bounds.Width, 2 * m_theme.PickTolerance);
                        if (border.Contains(p))
                        {
                            var borderPart = new DiagramBorder(annotation)
                                {
                                    Border = DiagramBorder.BorderType.Top
                                };
                            return new AnnotationHitEventArgs(annotation, borderPart);
                        }
                        border.Offset(0, bounds.Height);
                        if (border.Contains(p))
                        {
                            var borderPart = new DiagramBorder(annotation)
                                {
                                    Border = DiagramBorder.BorderType.Bottom
                                };
                            return new AnnotationHitEventArgs(annotation, borderPart);
                        }

                        // check label
                        inflated.Inflate(tolerance, tolerance);
                        DiagramLabel label = null;
                        if (inflated.Contains(p))
                        {
                            var textBounds = new Rectangle(bounds.X + Margin.Left, bounds.Y + Margin.Top,
                                (int)annotationData.TextLayout.LayoutWidth + SystemInformation.VerticalScrollBarWidth, (int)annotationData.TextLayout.LayoutHeight);
                            label = new DiagramLabel(textBounds, TextFormatFlags.LeftAndRightPadding);

                            // set initial editing position
                            var contentBounds = new RectangleF(bounds.X + Margin.Left, bounds.Y + Margin.Top,
                                               bounds.Width - Margin.Size.Width, bounds.Height - Margin.Size.Height);
                            contentBounds.Width = Math.Max(contentBounds.Width, MinimumWidth);
                            contentBounds.Height = Math.Max(contentBounds.Height, MinimumHeight);

                            PointF origin = new PointF(contentBounds.Location.X, contentBounds.Location.Y - annotationData.TopLine * m_theme.TextFormat.FontHeight);
                            var result = new AnnotationHitEventArgs(annotation, label);
                            result.Position = new PointF(p.X - origin.X, p.Y - origin.Y);
                            return result;
                            
                        }
                                                
                        
                    }
                }
            }

            return new AnnotationHitEventArgs();
        }

        #region IPickingAdapter2 Members

        /// <summary>
        /// Performs hit test for a point, in client coordinates</summary>
        /// <param name="p">Pick point, in client coordinates</param>
        /// <returns>Hit record for a point, in client coordinates</returns>
        DiagramHitRecord IPickingAdapter2.Pick(Point p)
        {
            return Pick(p);
        }

        /// <summary>
        /// Performs hit testing for rectangle bounds, in client coordinates</summary>
        /// <param name="bounds">Pick rectangle, in client coordinates</param>
        /// <returns>Items that overlap with the rectangle, in client coordinates</returns>
        IEnumerable<object> IPickingAdapter2.Pick(Rectangle bounds)
        {
            if (m_annotatedDiagram == null)
                return EmptyEnumerable<object>.Instance;

            List<object> hit = new List<object>();
                        
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

        /// <summary>
        /// Gets a bounding rectangle for the items, in client coordinates</summary>
        /// <param name="items">Items</param>
        /// <returns>Bounding rectangle for the items, in client coordinates</returns>
        Rectangle IPickingAdapter2.GetBounds(IEnumerable<object> items)
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

        //void IPrintingAdapter.Print(PrintDocument printDocument, Graphics g)
        //{
        //    switch (printDocument.PrinterSettings.PrintRange)
        //    {
        //        case PrintRange.Selection:
        //            PrintSelection(g);
        //            break;

        //        default:
        //            PrintAll(g);
        //            break;
        //    }
        //}

        //private void PrintSelection(D2dGraphics g)
        //{
        //    if (m_selectionContext != null)
        //    {
        //        foreach (IAnnotation annotation in m_selectionContext.GetSelection<IAnnotation>())
        //            DrawAnnotation(annotation, DiagramDrawingStyle.Normal, g);
        //    }
        //}

        //private void PrintAll(D2dGraphics g)
        //{
        //    foreach (IAnnotation annotation in m_annotatedDiagram.Annotations)
        //        DrawAnnotation(annotation, DiagramDrawingStyle.Normal, g);
        //}

        #endregion

        #region IItemDragAdapter Members

        void IItemDragAdapter.BeginDrag(ControlAdapter initiator)
        {
            m_draggingAnnotations = m_selectionContext.GetSelection<IAnnotation>().ToArray();
            m_moving = true;

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

            if (m_autoTranslateAdapter != null)
                m_autoTranslateAdapter.Enabled = true;
        }

        void IItemDragAdapter.EndingDrag()
        {
            // Restore the old positions so that the change can be recorded in DoTransaction().
            for (int i = 0; i < m_draggingAnnotations.Length; i++)
            {
                IAnnotation annotation = m_draggingAnnotations[i];
                MoveAnnotation(annotation, m_oldPositions[i]);
            }
        }

        void IItemDragAdapter.EndDrag()
        {
            // OnDragging will update the positions and OnEndDrag() will create the transaction.
            for (int i = 0; i < m_draggingAnnotations.Length; i++)
            {
                IAnnotation annotation = m_draggingAnnotations[i];
                MoveAnnotation(annotation, m_newPositions[i]);
            }
            m_draggingAnnotations = null;
        }

        #endregion

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            m_transformAdapter = control.As<ITransformAdapter>();
            m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();

            var d2dControl = control as D2dAdaptableControl;
            d2dControl.ContextChanged += control_ContextChanged;
            d2dControl.DrawingD2d += control_Paint;
            d2dControl.KeyPress += control_KeyPress;
            d2dControl.PreviewKeyDown += control_PreviewKeyDown;
            d2dControl.GotFocus += control_GotFocus;
            d2dControl.LostFocus += control_LostFocus;
            d2dControl.DoubleClick += control_DoubleClick;
            base.Bind(control);
        }


        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            var d2dControl = control as D2dAdaptableControl;
            d2dControl.ContextChanged -= control_ContextChanged;
            d2dControl.DrawingD2d -= control_Paint;
            d2dControl.KeyPress -= control_KeyPress;
            d2dControl.PreviewKeyDown -= control_PreviewKeyDown;
            d2dControl.GotFocus -= control_GotFocus;
            d2dControl.LostFocus -= control_LostFocus;      
            m_transformAdapter = null;
            m_autoTranslateAdapter = null;
            base.Unbind(control);
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            m_annotationEditors.Clear();

            IAnnotatedDiagram annotatedContext = AdaptedControl.ContextAs<IAnnotatedDiagram>();
            m_coloringContext = AdaptedControl.ContextAs<IColoringContext>();
            if (m_annotatedDiagram != annotatedContext)
            {
                if (m_annotatedDiagram != null)
                {
                    if (m_observableContext != null)
                    {
                        //m_observableContext.ItemInserted -= context_ObjectInserted;
                        //m_observableContext.ItemRemoved -= context_ObjectRemoved;
                       // m_observableContext.ItemChanged -= context_ObjectChanged;
                        //m_observableContext.Reloaded -= context_Reloaded;
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
                       // m_observableContext.ItemInserted += context_ObjectInserted;
                       // m_observableContext.ItemRemoved += context_ObjectRemoved;
                       // m_observableContext.ItemChanged += context_ObjectChanged;
                       // m_observableContext.Reloaded += context_Reloaded;
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


        private void control_Paint(object sender, EventArgs e)
        {
            if (m_annotatedDiagram == null)
                return;

            var control = (D2dAdaptableControl)this.AdaptedControl;
            D2dGraphics gfx = control.D2dGraphics;
            Matrix3x2F invXform = gfx.Transform;
            m_scaleX = invXform.M11;  // get the scale before inverting.
            invXform.Invert();
                       
            RectangleF graphBound = D2dUtil.Transform(invXform, control.ClientRectangle);

            D2dParagraphAlignment paragraphAlign = m_theme.TextFormat.ParagraphAlignment;
            D2dTextAlignment textAlign = m_theme.TextFormat.TextAlignment;
            D2dDrawTextOptions drawTextOptions = m_theme.TextFormat.DrawTextOptions;

            m_theme.TextFormat.ParagraphAlignment = D2dParagraphAlignment.Near;
            m_theme.TextFormat.TextAlignment = D2dTextAlignment.Leading;
            m_theme.TextFormat.DrawTextOptions = D2dDrawTextOptions.Clip;

            bool drawText = (m_scaleX * m_theme.TextFormat.FontHeight) > 5.0f;
            // draw all annotations in their current position, ghosting those which are dragging
            foreach (IAnnotation annotation in m_annotatedDiagram.Annotations)
            {
                Rectangle bounds = annotation.Bounds;
                if (bounds.Size.IsEmpty)
                {
                    if (string.IsNullOrEmpty(annotation.Text))
                    {
                        annotation.SetTextSize(new Size(180,100));
                    }
                    else
                    {
                        // calculate size of text block
                        SizeF textSizeF = gfx.MeasureText(annotation.Text, m_theme.TextFormat);
                        Size textSize = new Size((int)Math.Ceiling(textSizeF.Width), (int)Math.Ceiling(textSizeF.Height));
                        textSize.Width += 2 * Margin.Size.Width;
                        textSize.Height += 2 * Margin.Size.Height;
                        bounds.Size = textSize;
                        annotation.SetTextSize(textSize);
                    }
                   
                }
                if (!graphBound.IntersectsWith(bounds))
                    continue;

                DiagramDrawingStyle style = DiagramDrawingStyle.Normal;
                if (m_selectionContext.SelectionContains(annotation))
                {
                    style = DiagramDrawingStyle.Selected;
                    if (m_selectionContext.GetLastSelected<IAnnotation>() == annotation)
                        style = DiagramDrawingStyle.LastSelected;
                }

                DrawAnnotation(annotation, style, gfx, drawText, graphBound);
            }
            
            m_theme.TextFormat.ParagraphAlignment = paragraphAlign;
            m_theme.TextFormat.TextAlignment = textAlign;
            m_theme.TextFormat.DrawTextOptions = drawTextOptions;
        }


        /// <summary>
        /// Performs custom actions on adaptable control MouseMove events. The base method should
        /// be called first.</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected override void OnMouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(sender, e);

            AnnotationHitEventArgs hitRecord = Pick(CurrentPoint);
            if (hitRecord.Annotation != null && AdaptedControl.Cursor == Cursors.Default)
            {
                if (hitRecord.Label != null)
                {
                    AdaptedControl.Cursor = Cursors.IBeam;
                }
                else if (hitRecord.Part.Is<DiagramTitleBar>())
                    AdaptedControl.Cursor = Cursors.SizeAll;
                else if (hitRecord.Part.Is<DiagramBorder>())
                {
                    var borderPart = hitRecord.Part.Cast<DiagramBorder>();

                    AdaptedControl.AutoResetCursor = false;
                    if (borderPart.Border == DiagramBorder.BorderType.Right || borderPart.Border == DiagramBorder.BorderType.Left)
                        AdaptedControl.Cursor = Cursors.SizeWE;
                    else if (borderPart.Border == DiagramBorder.BorderType.Bottom || borderPart.Border == DiagramBorder.BorderType.Top)
                        AdaptedControl.Cursor = Cursors.SizeNS;
                    else if (borderPart.Border == DiagramBorder.BorderType.LowerRightCorner)
                        AdaptedControl.Cursor = Cursors.SizeNWSE;
                    else if (borderPart.Border == DiagramBorder.BorderType.UpperLeftCorner)
                        AdaptedControl.Cursor = Cursors.SizeNWSE;
                    else if (borderPart.Border == DiagramBorder.BorderType.UpperRightCorner)
                        AdaptedControl.Cursor = Cursors.SizeNESW;
                    else if (borderPart.Border == DiagramBorder.BorderType.LowerLeftCorner)
                        AdaptedControl.Cursor = Cursors.SizeNESW;
                }
                else if (hitRecord.Part.Is<DiagramScrollBar>())
                {
                    //AdaptedControl.Cursor = Cursors.HSplit;
                }
                else
                    AdaptedControl.Cursor = Cursors.SizeAll;
                AdaptedControl.AutoResetCursor = false;
            }
            else
                AdaptedControl.AutoResetCursor = true;
            
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseDown events</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected override void OnMouseDown(object sender, MouseEventArgs e)
        {
            base.OnMouseDown(sender, e);
            var hitRecord = Pick(CurrentPoint);
            AdaptedControl.HasKeyboardFocus = false;
            bool editingText = m_editingText;
            m_editingText = false;

            // set caret position
            if (e.Button == MouseButtons.Left && ((Control.ModifierKeys & Keys.Alt) == 0))
            {
                if (hitRecord.Annotation != null && hitRecord.Label != null) // hit the text
                {

                    m_editingText = true;
                    AdaptedControl.HasKeyboardFocus = true;
                    if (m_annotationEditors.ContainsKey(hitRecord.Annotation))
                    {
                        var annotationEditor = m_annotationEditors[hitRecord.Annotation];
                        annotationEditor.SetSelectionFromPoint(hitRecord.Position.X, hitRecord.Position.Y, false);
                        AdaptedControl.Invalidate();
                    }
                    if (AdaptedControl.Cursor == Cursors.Default)
                        AdaptedControl.Cursor = Cursors.IBeam;
                }                
            }
            else if (e.Button == MouseButtons.Right)
            {
                m_rmbPressed = true;
                // activate standard editing commands on annotation nodes via on context menu when user clicks over the title bar  
                if (hitRecord.Annotation != null && hitRecord.Part.Is<DiagramTitleBar>())
                    AdaptedControl.HasKeyboardFocus = false;
            }

            if (editingText != m_editingText)
                AdaptedControl.Invalidate();
        }

        /// <summary>
        /// Handles mouse up event</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Mouse event argumeents</param>
        protected override void OnMouseUp(object sender, MouseEventArgs e)
        {
            m_rmbPressed = false;
        }


        /// <summary>
        /// Performs custom actions when beginning a mouse dragging operation</summary>
        /// <param name="e">Mouse event args</param>
        protected override void OnBeginDrag(MouseEventArgs e)
        {
            base.OnBeginDrag(e);

            if (m_layoutContext != null && e.Button == MouseButtons.Left &&
                   ((Control.ModifierKeys & Keys.Alt) == 0) && !AdaptedControl.Capture)
            {
                m_mousePick = Pick(FirstPoint);
                if (m_mousePick.Item != null)
                {
                    foreach (var itemDragAdapter in AdaptedControl.AsAll<IItemDragAdapter>())
                        if (itemDragAdapter != this)
                            itemDragAdapter.BeginDrag(this);

                    m_draggingAnnotations = m_selectionContext.GetSelection<IAnnotation>().ToArray();

                    if (m_mousePick.Item.Is<IAnnotation>())
                    {
                        // resizing
                        if (m_mousePick.Part.Is<DiagramBorder>())
                        {

                            m_startBounds = m_mousePick.Item.Cast<IAnnotation>().Bounds;
                            m_resizing = true;
                        }
                        else if (m_mousePick.Part.Is<DiagramTitleBar>())
                        {
                            m_moving = true;
                        }
                        else if (m_mousePick.Part.Is<DiagramLabel>())
                        {
                            m_selecting = true;
                        }
                        else if (m_mousePick.Part.Is<DiagramScrollBar>())
                        {
                            var annotationData = m_annotationEditors[m_mousePick.Item.Cast<IAnnotation>()];
                            m_startTopLine = annotationData.TopLine;
                            m_scrolling = true;                           
                        }
                    }

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
                   

                    AdaptedControl.Capture = true;

                    if (m_autoTranslateAdapter != null)
                        m_autoTranslateAdapter.Enabled = true;
                }                             
            }
        }

        /// <summary>
        /// Performs custom actions when performing a mouse dragging operation</summary>
        /// <param name="e">Mouse move event args</param>
        protected override void OnDragging(MouseEventArgs e)
        {
            var D2dControl = this.AdaptedControl as D2dAdaptableControl;

            if (m_resizing)
            {
                ResizeAnnotation(m_mousePick.Part.Cast<DiagramBorder>());            
                D2dControl.Invalidate();
                return;
            }
            if (m_scrolling)
            {
                ScrollAnnotation(m_mousePick.Part.Cast<DiagramScrollBar>());
                D2dControl.Invalidate();
                return;
            }

            if (m_draggingAnnotations != null )
            {
                if (m_moving)
                {
                    Matrix3x2F invXform = Matrix3x2F.Invert(D2dControl.D2dGraphics.Transform);
                    PointF deltaF = Matrix3x2F.TransformVector(invXform, Delta);
                    Point delta = new Point((int) deltaF.X, (int) deltaF.Y);

                    // set dragged nodes' positions, offsetting by drag delta and applying layout constraints
                    for (int i = 0; i < m_draggingAnnotations.Length; i++)
                    {
                        IAnnotation annotation = m_draggingAnnotations[i];
                        Rectangle bounds = GetBounds(annotation);
                        bounds.X = m_oldPositions[i].X + delta.X;
                        bounds.Y = m_oldPositions[i].Y + delta.Y;
                        m_newPositions[i] = bounds.Location;
                        m_layoutContext.SetBounds(annotation, bounds, BoundsSpecified.Location);
                    }
                }
                else if (m_selecting)
                {
                    AnnotationHitEventArgs hitRecord = Pick(CurrentPoint);
                    if (hitRecord.Annotation != null && hitRecord.Label != null)
                    {
                        if (m_annotationEditors.ContainsKey(hitRecord.Annotation))
                        {
                            // Drag current selection.
                           
                            var annotationEditor = m_annotationEditors[hitRecord.Annotation];
                            annotationEditor.SetSelectionFromPoint(hitRecord.Position.X, hitRecord.Position.Y, true);
                            // update selection range
                            //int startPosition = annotationData.CaretPosition;
                            //int currentPosition = annotationData.TextPosition; // Pick() updated TextPosition
                            //annotationData.SelectionStart = Math.Min(startPosition, currentPosition);
                            //annotationData.SelectionLength = Math.Abs(currentPosition - startPosition);
                            AdaptedControl.Invalidate();
                        }
                    }
                }

                D2dControl.Invalidate();
            }
        }

        /// <summary>
        /// Performs custom actions when ending a mouse dragging operation</summary>
        /// <param name="e">Mouse event args</param>
        protected override void OnEndDrag(MouseEventArgs e)
        {
            base.OnEndDrag(e);
            if (m_draggingAnnotations != null)
            {
                var transactionContext = AdaptedControl.ContextAs<ITransactionContext>();
                if (m_moving)
                {
                    foreach (IItemDragAdapter itemDragAdapter in AdaptedControl.AsAll<IItemDragAdapter>())
                        itemDragAdapter.EndingDrag(); //call ourselves, too
                   
                    transactionContext.DoTransaction(
                        () =>
                            {
                                foreach (IItemDragAdapter itemDragAdapter in AdaptedControl.AsAll<IItemDragAdapter>())
                                    itemDragAdapter.EndDrag(); //call ourselves, too
                            }, "Drag Items".Localize());
                }
                else if (m_selecting)
                {
                    //// update caret position
                    //var annotation = m_mousePick.Item.Cast<IAnnotation>();
                    //if (m_annotationEditors.ContainsKey(annotation))
                    //{
                    //    var annotationData = m_annotationEditors[annotation];
                    //    if (annotationData.SelectionStart < annotationData.CaretPosition)
                    //        annotationData.CaretPosition = annotationData.SelectionStart;
                    //}

                }
                else if (m_resizing)
                {
                      transactionContext.DoTransaction( 
                          () => ResizeAnnotation(m_mousePick.Part.Cast<DiagramBorder>()),
                          "Resize Annotation".Localize());
                }

                if (m_autoTranslateAdapter != null)
                    m_autoTranslateAdapter.Enabled = false;

                m_draggingAnnotations = null;
                m_newPositions = null;
                m_oldPositions = null;
                AdaptedControl.Invalidate();
            }

            m_resizing = false;
            m_scrolling = false;
            m_moving = false;
            m_selecting = false;
        }

        private void DrawAnnotation(IAnnotation annotation, DiagramDrawingStyle style, D2dGraphics g, bool drawText, RectangleF graphBound)
        {
            // fill background
            Rectangle bounds = annotation.Bounds;

            Color backColor = m_coloringContext == null
                                  ? SystemColors.Info
                                  : (Color) m_coloringContext.GetColor(ColoringTypes.BackColor, annotation);
            // keep the width of border in 1 pixel after transformation to avoid D2d antialiasing away the line
            float borderThickness = 1.0f/m_scaleX;
            g.FillRectangle(bounds, backColor);
            g.DrawRectangle(bounds, ControlPaint.Dark(backColor), borderThickness, null);
       
            // draw titlebar
            if (style == DiagramDrawingStyle.LastSelected || style == DiagramDrawingStyle.Selected)
            {
                var titleBarRect = new RectangleF(bounds.X, bounds.Y, bounds.Width, Margin.Top - 1);
                g.FillRectangle(titleBarRect, ControlPaint.Dark(backColor));
            }
            // line seperate titlebar from text content     
            g.DrawLine(bounds.X, bounds.Y + Margin.Top-1, bounds.X + bounds.Width, bounds.Y + Margin.Top-1, ControlPaint.Dark(backColor), borderThickness);

            // draw content
            if (drawText)
            {
                var contentBounds = new RectangleF(bounds.X + Margin.Left, bounds.Y + Margin.Top,
                                               bounds.Width - Margin.Size.Width, bounds.Height - Margin.Size.Height);

                contentBounds.Width = Math.Max(contentBounds.Width, MinimumWidth);
                contentBounds.Height = Math.Max(contentBounds.Height, MinimumHeight);
                var textBounds = contentBounds;              
    
                int topLine = 0;
                D2dTextLayout textLayout = null;

                if (m_annotationEditors.ContainsKey(annotation))
                {
                                     
                    var annotationEditor = m_annotationEditors[annotation];
                    if (annotationEditor.TextLayout.Text != annotation.Text) // text content changed, for example, undo,redo
                    {
                        annotationEditor.ResetText(annotation.Text);
                    }
                    topLine = annotationEditor.TopLine;
                    textLayout = m_annotationEditors[annotation].TextLayout;
                    annotationEditor.VerticalScrollBarVisibe = textLayout.Height > textLayout.LayoutHeight;

                    if (m_annotationEditors[annotation].VerticalScrollBarVisibe)
                        textBounds.Width -= ScrollBarWidth + 2 * ScrollBarMargin;
                    textLayout.LayoutWidth = textBounds.Width;
                    textLayout.LayoutHeight = textBounds.Height;
                 
                }

                if (textLayout == null)
                {
                    // first assume no v-scroll bar needed
                    textLayout = D2dFactory.CreateTextLayout(annotation.Text, m_theme.TextFormat, contentBounds.Width, contentBounds.Height);
                    if (m_theme.TextFormat.Underlined)
                        textLayout.SetUnderline(true, 0, annotation.Text.Length);
                    if (m_theme.TextFormat.Strikeout)
                        textLayout.SetStrikethrough(true, 0, annotation.Text.Length);

                    if (textLayout.Height >  textLayout.LayoutHeight) // need v-scroll bar
                    {
                        textLayout.LayoutWidth = contentBounds.Width - ScrollBarWidth - 2*ScrollBarMargin;
                      
                    }
                    m_annotationEditors.Add(annotation, new TextEditor()
                        {
                            TextLayout = textLayout,
                            TextFormat = m_theme.TextFormat,
                            TopLine =  topLine,
                            VerticalScrollBarVisibe = textLayout.Height > textLayout.LayoutHeight
                        });
                }

                var annotationData = m_annotationEditors[annotation];
                PointF origin = new PointF(contentBounds.Location.X, contentBounds.Location.Y - topLine *  m_theme.TextFormat.FontHeight);

                g.PushAxisAlignedClip(contentBounds);

                // draw the selection range behind the text.
                if (annotationData.SelectionLength > 0)
                {
                    var hitTestMetrics = textLayout.HitTestTextRange(annotationData.SelectionStart, annotationData.SelectionLength, 0, 0);
                    for (int i = 0; i < hitTestMetrics.Length; ++i)
                    {

                        var highlightRect = new RectangleF(hitTestMetrics[i].Point.X, hitTestMetrics[i].Point.Y, hitTestMetrics[i].Width,
                                                           hitTestMetrics[i].Height);
                        highlightRect.Offset(origin);
                        g.FillRectangle(highlightRect, m_theme.TextHighlightBrush);
                    }               
                }

                // draw caret
                
                if ( style == DiagramDrawingStyle.Selected || style == DiagramDrawingStyle.LastSelected)
                {
                    textLayout = annotationData.TextLayout;
                    var caretRect = m_annotationEditors[annotation].GetCaretRect();
                    caretRect.Offset(origin);
                    //g.FillRectangle(caretRect, m_theme.HotBrush);

                    // set Windows caret position                  
                    if (m_editingText && contentBounds.IntersectsWith(caretRect) && graphBound.IntersectsWith(caretRect) && AdaptedControl.Focused)
                    {
                        var caretClientRect = GdiUtil.Transform(m_transformAdapter.Transform, caretRect);
                        float ratio = m_scaleX*m_theme.TextFormat.FontHeight/CaretHeight;                      
                        if (ratio > 1.1f || ratio < 0.9f) // adjust caret height
                        {
                            CaretHeight = (int)(m_scaleX*m_theme.TextFormat.FontHeight);
                            User32.DestroyCaret();
                            User32.CreateCaret(AdaptedControl.Handle, IntPtr.Zero, CaretWidth, CaretHeight);
                        }
                        // align bottom    
                        User32.SetCaretPos((int) caretClientRect.X, (int)(caretClientRect.Y + caretClientRect.Height - CaretHeight));
                        if (!m_rmbPressed)
                            AdaptedControl.HasKeyboardFocus = true;
                    }
                    else
                        HideCaret();
                }

                // draw text 
                g.DrawTextLayout(origin, textLayout, m_theme.TextBrush); 

                g.PopAxisAlignedClip();

                // draw v-scroll bar
                if (contentBounds.Height < textLayout.Height)
                {
                    float visibleLines = contentBounds.Height / m_theme.TextFormat.FontHeight;
                    float vMin = topLine * textLayout.LayoutHeight / textLayout.LineCount;
                    float vMax = (topLine + visibleLines-1) * textLayout.LayoutHeight / textLayout.LineCount;
                    if (m_scrolling)
                    {
                        var trackBounds = new RectangleF(contentBounds.Right - ScrollBarMargin - ScrollBarWidth, contentBounds.Y, ScrollBarWidth, contentBounds.Height);
                        g.FillRectangle(trackBounds, Color.Gainsboro);                    
                    }
                    var thumbBounds = new RectangleF(contentBounds.Right - ScrollBarMargin - ScrollBarWidth, contentBounds.Y + vMin, ScrollBarWidth, vMax - vMin);
                    g.FillRectangle(thumbBounds, Color.DimGray);
                }                
            }
            else
                HideCaret();
        }

        private void MoveAnnotation(IAnnotation annotation, Point location)
        {
            Rectangle bounds;
            m_layoutContext.GetBounds(annotation, out bounds);
            bounds.Location = location;
            m_layoutContext.SetBounds(annotation, bounds, BoundsSpecified.Location);
        }

        private void ResizeAnnotation(DiagramBorder diagramBorder)
        {
            Point currentPoint = GdiUtil.InverseTransform(m_transformAdapter.Transform, CurrentPoint);
            Point firstPoint = GdiUtil.InverseTransform(m_transformAdapter.Transform, FirstPoint);
            Point delta = new Point(currentPoint.X - firstPoint.X, currentPoint.Y - firstPoint.Y);
            Rectangle newBounds;
            switch (diagramBorder.Border)
            {

                case DiagramBorder.BorderType.LowerRightCorner:
                    newBounds = new Rectangle(m_startBounds.X, m_startBounds.Y, m_startBounds.Width + delta.X, m_startBounds.Height + delta.Y);
                    m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(newBounds), BoundsSpecified.All);
                    break;
                case DiagramBorder.BorderType.UpperLeftCorner:
                    newBounds = new Rectangle(currentPoint.X, currentPoint.Y, m_startBounds.Width - delta.X, m_startBounds.Height - delta.Y);
                    m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(newBounds), BoundsSpecified.All);
                    break;
                case DiagramBorder.BorderType.UpperRightCorner:
                    newBounds = new Rectangle(m_startBounds.X, currentPoint.Y, m_startBounds.Width + delta.X, m_startBounds.Height - delta.Y);
                    m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(newBounds), BoundsSpecified.All);
                    break;
                case DiagramBorder.BorderType.LowerLeftCorner:
                    newBounds = new Rectangle(currentPoint.X, m_startBounds.Y, m_startBounds.Width - delta.X, m_startBounds.Height + delta.Y);
                    m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(newBounds), BoundsSpecified.All);
                    break;

                case DiagramBorder.BorderType.Left:
                    newBounds = new Rectangle(currentPoint.X, m_startBounds.Y, m_startBounds.Width - delta.X, m_startBounds.Height);
                    m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(newBounds), BoundsSpecified.All);
                    break;
                case DiagramBorder.BorderType.Right:
                    newBounds = new Rectangle(m_startBounds.X, m_startBounds.Y, m_startBounds.Width + delta.X, m_startBounds.Height);
                    m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(newBounds), BoundsSpecified.Size);
                    break;
                case DiagramBorder.BorderType.Top:
                    newBounds = new Rectangle(m_startBounds.X, currentPoint.Y, m_startBounds.Width, m_startBounds.Height - delta.Y);
                    m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(newBounds), BoundsSpecified.All);
                    break;
                case DiagramBorder.BorderType.Bottom:
                    newBounds = new Rectangle(m_startBounds.X, m_startBounds.Y, m_startBounds.Width, m_startBounds.Height + delta.Y);
                    m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(newBounds), BoundsSpecified.Size);
                    break;

            }          
        }

        private Rectangle ConstrainBounds(Rectangle annotationBounds)
        {
           return new Rectangle(annotationBounds.X, annotationBounds.Y,
                (int)Math.Max(annotationBounds.Width, MinimumWidth),
                (int)Math.Max(annotationBounds.Height, MinimumHeight+ Margin.Top));
        }

        private void ScrollAnnotation(DiagramScrollBar scrollBar)
        {

            Point delta = Delta;
            var annotationData = m_annotationEditors[scrollBar.Item.Cast<IAnnotation>()];
            float lineHeight = annotationData.TextLayout.Height /annotationData.TextLayout.LineCount;
            float lines = delta.Y / lineHeight;
            int newTopLine = m_startTopLine + (int)lines;
            float visibleLines = annotationData.TextLayout.LayoutHeight / lineHeight;

            if (newTopLine < 0)
                newTopLine = 0;
            if (newTopLine + visibleLines - 1 > annotationData.TextLayout.LineCount)
                newTopLine = annotationData.TopLine;
            annotationData.TopLine =  newTopLine >=0?  newTopLine:0;
        }
        
        private Rectangle GetBounds(IAnnotation annotation)
        {
            Rectangle bounds = annotation.Bounds;
            if (bounds.Size.IsEmpty && m_theme != null)
            {
                D2dGraphics g = ((D2dAdaptableControl)this.AdaptedControl).D2dGraphics;

                // calculate size of text block
                SizeF textSizeF = g.MeasureText(annotation.Text, m_theme.TextFormat);
                bounds.Size = new Size((int)textSizeF.Width + 2 * Margin.Size.Width, (int)textSizeF.Height + 2 * Margin.Size.Height);

                // Don't update the IAnnotation. http://forums.ship.scea.com/jive/thread.jspa?threadID=10751
                //annotation.SetTextSize(textSize);
            }
            return bounds;
        }

       

        #region text editing

        private void control_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!AdaptedControl.HasKeyboardFocus)
                return;

            var annotation = m_selectionContext.GetSelection<IAnnotation>().FirstOrDefault();
            if (annotation == null)
                return;

            if (!m_annotationEditors.ContainsKey(annotation))
                return;
            var annotationEditor = m_annotationEditors[annotation];

            var textProperty = annotation.GetType().GetProperty("Text");
            if (!textProperty.CanWrite)
                return;
               
            var transactionContext = AdaptedControl.ContextAs<ITransactionContext>();
      
            switch (e.KeyChar)
            {
                case  '\r':                    
                    transactionContext.DoTransaction(() =>
                        {
                            DeleteTextSelection(annotation);
                            InsertText(annotation, "\r\n");

                        }, EditAnnotation);
                    annotationEditor.SetSelection(TextEditor.SelectionMode.AbsoluteLeading, annotationEditor.CaretAbsolutePosition + 2, false, false);
                    break;
                case '\b':
                    transactionContext.DoTransaction(() =>
                        {
                            if (annotationEditor.CaretAbsolutePosition != annotationEditor.CaretAnchorPosition)
                                DeleteTextSelection(annotation);
                            else if (annotationEditor.CaretAbsolutePosition > 0)
                            {
                                int count = 1;
                                // Need special case for surrogate pairs and CR/LF pair.
                                if (annotationEditor.CaretAbsolutePosition >= 2
                                    && annotationEditor.CaretAbsolutePosition <= annotation.Text.Length)
                                {
                                    char charBackOne = annotation.Text[annotationEditor.CaretAbsolutePosition - 1];
                                    char charBackTwo = annotation.Text[annotationEditor.CaretAbsolutePosition - 2];
                                    if ((char.IsLowSurrogate(charBackOne) && char.IsHighSurrogate(charBackTwo)) ||
                                        (charBackOne == '\n' && charBackTwo == '\r'))
                                    {
                                        count = 2;
                                    }
                                }
                                annotationEditor.SetSelection(TextEditor.SelectionMode.LeftChar, count, false, false);
                                string newText = annotationEditor.RemoveTextAt(annotation.Text, annotationEditor.CaretPosition, count);
                                textProperty.SetValue(annotation, newText, null);
                            }
                        }, EditAnnotation);
                    break;
                default:
                    if (e.KeyChar >= 0x20) // allow normal characters 
                    {
                        InsertChar(annotation, annotationEditor, e.KeyChar);
                        AdaptedControl.Invalidate();
                    }
                    break;
            }     
        }

        private void control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (!AdaptedControl.HasKeyboardFocus)
                return;

            var annotation = m_selectionContext.GetSelection<IAnnotation>().FirstOrDefault();
            if (annotation == null)
                return;

            if (!m_annotationEditors.ContainsKey(annotation))
                return;
            var annotationEditor = m_annotationEditors[annotation];

            var textProperty = annotation.GetType().GetProperty("Text");
            if (!textProperty.CanWrite)
                return;

            var transactionContext = AdaptedControl.ContextAs<ITransactionContext>();
      
            var key = e.KeyData & Keys.KeyCode;
            bool ctrlPressed = (e.KeyData & Keys.Control) == Keys.Control;
            bool shiftPressed = (e.KeyData & Keys.Shift) == Keys.Shift;
            //bool altPresssed = (e.KeyData & Keys.Alt) == Keys.Alt;

            switch (key)
            {
                case Keys.Delete:
                    if (ctrlPressed)
                    {
                        annotationEditor.SetSelection(TextEditor.SelectionMode.RightWord, 1, true, false);
                          transactionContext.DoTransaction(() =>
                              {
                                  DeleteTextSelection(annotation);
                              }, EditAnnotation);

                    }
                    else
                    {
                        transactionContext.DoTransaction(() =>
                            {
                                if (annotationEditor.CaretAbsolutePosition != annotationEditor.CaretAnchorPosition)
                                    DeleteTextSelection(annotation); // delete all the selected text.
                                else
                                {
                                    // get the size of the following cluster.
                                    var hitTestMetrics =
                                        annotationEditor.TextLayout.HitTestTextPosition(
                                            annotationEditor.CaretAbsolutePosition, false);
                                    string newText = annotationEditor.RemoveTextAt(annotation.Text,
                                                                                   hitTestMetrics.TextPosition,
                                                                                   hitTestMetrics.Length);
                                    textProperty.SetValue(annotation, newText, null);
                                    annotationEditor.SetSelection(TextEditor.SelectionMode.AbsoluteLeading,
                                                                  hitTestMetrics.TextPosition, false, false);
                                }
                            }, EditAnnotation);
                    }
                    AdaptedControl.Invalidate();
                    break;
                case Keys.Tab:
                    InsertChar(annotation, annotationEditor, '\t');
                    AdaptedControl.Invalidate();
                    break;
                // handle arrow keys
                case Keys.Left:
                    annotationEditor.SetSelection(ctrlPressed ? TextEditor.SelectionMode.LeftWord : TextEditor.SelectionMode.Left, 1, shiftPressed, false);
                    AdaptedControl.Invalidate();
                    break;
                case Keys.Right:
                    annotationEditor.SetSelection(ctrlPressed ? TextEditor.SelectionMode.RightWord : TextEditor.SelectionMode.Right, 1, shiftPressed, false);
                    AdaptedControl.Invalidate();
                    break;
                case Keys.Up: // up a line
                    annotationEditor.SetSelection(TextEditor.SelectionMode.Up, 1, shiftPressed, false);
                    AdaptedControl.Invalidate();
                    break;
                case Keys.Down: // down a line
                    annotationEditor.SetSelection(TextEditor.SelectionMode.Down, 1, shiftPressed, false);
                    AdaptedControl.Invalidate();
                    break;
                case Keys.Home: // beginning of line
                    annotationEditor.SetSelection(ctrlPressed ? TextEditor.SelectionMode.First : TextEditor.SelectionMode.Home, 0, shiftPressed, false);
                    AdaptedControl.Invalidate();
                    break;
                case Keys.End: // end of line
                    annotationEditor.SetSelection(ctrlPressed ? TextEditor.SelectionMode.Last : TextEditor.SelectionMode.End, 0, shiftPressed, false);
                    AdaptedControl.Invalidate();
                    break;
                case Keys.Insert:
                    if (ctrlPressed)
                        CopyToClipboard(annotation);
                    else
                        PasteFromClipboard(annotation);
                    break;
            }

            if (e.KeyCode == Keys.X && ctrlPressed)
            {
                CopyToClipboard(annotation);

                transactionContext.DoTransaction(() =>
                    {
                        DeleteTextSelection(annotation);
                    }, EditAnnotation);

            }
            else if (e.KeyCode == Keys.V && ctrlPressed)
            {              
                transactionContext.DoTransaction(() =>
                {
                    PasteFromClipboard(annotation);
                }, EditAnnotation);
                AdaptedControl.Invalidate();
            }
            else if (e.KeyCode == Keys.A && ctrlPressed)
            {
                annotationEditor.SetSelection(TextEditor.SelectionMode.All, 0, true, false);
                AdaptedControl.Invalidate();
            }
            else if (e.KeyCode == Keys.Z && ctrlPressed)
            {
                var historyContext = transactionContext.As<IHistoryContext>();
                if (historyContext != null && historyContext.CanUndo)
                    historyContext.Undo();
                AdaptedControl.Invalidate();
            }
            else if (e.KeyCode == Keys.Y && ctrlPressed)
            {
                var historyContext = transactionContext.As<IHistoryContext>();
                if (historyContext != null && historyContext.CanRedo)
                    historyContext.Redo();
                AdaptedControl.Invalidate();
            }
        }

        private void InsertChar(IAnnotation annotation, TextEditor annotationEditor, char charValue)
        {
            var transactionContext = AdaptedControl.ContextAs<ITransactionContext>();
            transactionContext.DoTransaction(() =>
                {
                    DeleteTextSelection(annotation);
                    InsertText(annotation, new string(charValue, 1));
                    annotationEditor.SetSelection(TextEditor.SelectionMode.Right, 1, false, false);
                }, EditAnnotation);
        }


        void control_LostFocus(object sender, EventArgs e)
        {
            User32.DestroyCaret();
            m_caretCreated = false;
            AdaptedControl.HasKeyboardFocus = false;
        }

        void control_GotFocus(object sender, EventArgs e)
        {
            CaretHeight = (int) (m_scaleX* m_theme.TextFormat.FontHeight);// AdaptedControl.Font.Height;
            User32.CreateCaret(AdaptedControl.Handle, IntPtr.Zero, CaretWidth, CaretHeight);
            User32.ShowCaret(AdaptedControl.Handle);
            m_caretCreated = true;
            if (m_selectionContext != null)
            {
                bool caretVisible = m_selectionContext.GetSelection<IAnnotation>().FirstOrDefault() != null;
                if (caretVisible)
                    AdaptedControl.Invalidate();
                else
                    HideCaret();
            }
            
        }

        void control_DoubleClick(object sender, EventArgs e)
        {
            Point clientPoint = AdaptedControl.PointToClient(Control.MousePosition);
            AnnotationHitEventArgs hitRecord = Pick(clientPoint);
            if (hitRecord.Annotation != null && hitRecord.Label != null)
            {
                //select the word
                if (m_annotationEditors.ContainsKey(hitRecord.Annotation))
                {
                    var annotationEditor = m_annotationEditors[hitRecord.Annotation];
                    annotationEditor.SetSelection(TextEditor.SelectionMode.SingleWord, 0, true, false);                
                    AdaptedControl.Invalidate();
                }
            }

        }

        /// <summary>
        /// Deletes any existing selection</summary>
        /// <param name="annotation">IAnnotation with selection</param>
        public void DeleteTextSelection(IAnnotation annotation)
        {
            if (m_annotationEditors.ContainsKey(annotation))
            {             
                var textProperty = annotation.GetType().GetProperty("Text");
                if (textProperty.CanWrite)
                {
                    var annotationEditor = m_annotationEditors[annotation];
                    annotationEditor.UpdateSelectionRange();
                    if (annotationEditor.SelectionLength > 0)
                    {                  
                        var newValue = annotationEditor.RemoveTextAt(annotation.Text, annotationEditor.SelectionStart,
                                                                     annotationEditor.SelectionLength);
                        textProperty.SetValue(annotation, newValue, null);
                        annotationEditor.SetSelection(TextEditor.SelectionMode.AbsoluteLeading, annotationEditor.SelectionStart, false, false);
                        
                    }
                }
             
            }
        }

        /// <summary>
        /// Pastes clipboard contents to annotation text</summary>
        /// <param name="annotation">IAnnotation text pasted to</param>
        public void PasteFromClipboard(IAnnotation annotation)
        {
            if (m_annotationEditors.ContainsKey(annotation))
            {
                var annotationEditor = m_annotationEditors[annotation];
                string text = Clipboard.GetText();
                DeleteTextSelection(annotation);
                InsertText(annotation, text);
                annotationEditor.SetSelection(TextEditor.SelectionMode.RightChar, text.Length, false, false);
            }
            
        }

        private void CopyToClipboard(IAnnotation annotation)
        {
            string textSelected = TextSelected(annotation);
            if (!string.IsNullOrEmpty(textSelected))
                Clipboard.SetText(textSelected);
        }

        // Insert text into annotation without adjusting caret position
        private void InsertText(IAnnotation annotation, string text)
        {
            if (m_annotationEditors.ContainsKey(annotation))
            {
                var textProperty = annotation.GetType().GetProperty("Text");
                if (textProperty.CanWrite)
                {
                 

                    //DeleteTextSelection(annotation);
                    var annotationEditor = m_annotationEditors[annotation];
                    var newValue = annotationEditor.InsertTextAt(annotation.Text, text);
                    textProperty.SetValue(annotation, newValue, null);                 

                }

            }
        }

        /// <summary>
        /// Indicates when the annotation is selected and the adapted control has keyboard focus</summary>
        /// <param name="annotation">IAnnotation to test</param>
        /// <returns>True iff the annotation is selected and the adapted control has keyboard focus</returns>
        public bool CanDeleteTextSelection(IAnnotation annotation)
        {           
            return m_selectionContext.GetSelection<IAnnotation>().Contains(annotation) &&
                AdaptedControl.HasKeyboardFocus;
        }


        /// <summary>
        /// Tests when the annotation is selected and the adapted control has keyboard focus</summary>
        /// <param name="annotation">IAnnotation to test</param>
        /// <returns>True iff the annotation is selected and the adapted control has keyboard focus</returns>
        public bool CanInsertText(IAnnotation annotation)
        {
            return m_selectionContext.GetSelection<IAnnotation>().Contains(annotation) &&
                AdaptedControl.HasKeyboardFocus;

        }

        /// <summary>
        /// Tests when the annotation is selected and the adapted control has keyboard focus</summary>
        /// <param name="annotation">IAnnotation to test</param>
        /// <returns>True iff the annotation is selected and the adapted control has keyboard focus</returns>
        public bool CanCopyText(IAnnotation annotation)
        {
            return m_selectionContext.GetSelection<IAnnotation>().Contains(annotation) &&
                AdaptedControl.HasKeyboardFocus;
        }

        /// <summary>
        /// Obtains selected text</summary>
        /// <param name="annotation">IAnnotation whose selected test is obtained</param>
        /// <returns>Annotation's selected text</returns>
        public string TextSelected(IAnnotation annotation)
        {
            if (m_annotationEditors.ContainsKey(annotation))
            {
                var annotationData = m_annotationEditors[annotation];
                return annotation.Text.Substring(annotationData.SelectionStart, annotationData.SelectionLength);
            }
            return string.Empty;
        }

        private void selection_Changed(object sender, EventArgs e)
        {
            if (m_caretCreated && m_selectionContext != null)
            {

                bool caretVisible = m_selectionContext.GetSelection<IAnnotation>().FirstOrDefault() != null;
                if (!caretVisible)
                    HideCaret();

            }
        }

        private void HideCaret()
        {
             User32.SetCaretPos(-10, 0);
             AdaptedControl.HasKeyboardFocus = false;
        }

        #endregion

        private static readonly Padding Margin = new Padding(5, 12, 3, 3);
        private static readonly string EditAnnotation = "Edit Annotation".Localize("the name of a command");
        private const int ScrollBarWidth =  5;
        private const int ScrollBarMargin = 2;
        private const int MinimumWidth = 50;
        private int MinimumHeight = 13;
        private const int CaretWidth =2;
        private int CaretHeight=12;
 
        private readonly Dictionary<IAnnotation, TextEditor> m_annotationEditors = new Dictionary<IAnnotation, TextEditor>(); 
        private D2dDiagramTheme m_theme;        
        private ITransformAdapter m_transformAdapter;
        private IAutoTranslateAdapter m_autoTranslateAdapter;

        private IAnnotatedDiagram m_annotatedDiagram;
        private IColoringContext m_coloringContext;
        private IObservableContext m_observableContext;
        private ISelectionContext m_selectionContext;
        private ILayoutContext m_layoutContext;

        private AnnotationHitEventArgs m_mousePick;
        private IAnnotation[] m_draggingAnnotations;        
        private Point[] m_newPositions;
        private Point[] m_oldPositions;

        private Rectangle m_startBounds;
        private float m_scaleX = 1.0f;
        private bool m_resizing;
        private bool m_scrolling;
        private bool m_moving;
        private bool m_selecting;    
        private int  m_startTopLine;
        private bool m_caretCreated;
        private bool m_editingText;
        private bool m_rmbPressed; //right mouse button pressed
    }
}
