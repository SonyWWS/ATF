//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Sce.Atf.Applications;
using Sce.Atf.Direct2D;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Control adapter to add a layout adorner to complex states</summary>
    public class MouseLayoutManipulator : DraggingControlAdapter
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="transformAdapter">ITransformAdapter</param>
        public MouseLayoutManipulator(ITransformAdapter transformAdapter)
        {
            m_transformAdapter = transformAdapter;
        }

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();
            control.ContextChanged += control_ContextChanged;

            var d2dControl = control as D2dAdaptableControl;
            if (d2dControl != null)
                d2dControl.DrawingD2d += d2dControl_DrawingD2d;
            else
                control.Paint += control_Paint;
               
            base.Bind(control);
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.ContextChanged -= control_ContextChanged;

            var d2dControl = control as D2dAdaptableControl;
            if (d2dControl != null)
                d2dControl.DrawingD2d -= d2dControl_DrawingD2d;
            else
                control.Paint -= control_Paint;

            base.Unbind(control);
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            if (m_selectionContext != null)
                m_selectionContext.SelectionChanged -= selectionContext_SelectionChanged;

            m_layoutContext = AdaptedControl.ContextAs<ILayoutContext>();
            m_selectionContext = AdaptedControl.ContextAs<ISelectionContext>();
            if (m_selectionContext != null)
                m_selectionContext.SelectionChanged += selectionContext_SelectionChanged;
        }

        private void selectionContext_SelectionChanged(object sender, EventArgs e)
        {
            AdaptedControl.Invalidate();
        }

        private const int HandleSize = 4;

        private void control_Paint(object sender, PaintEventArgs e)
        {
            // if we're dragging, or could drag and no one else is dragging...
            if (Dragging() ||
                (DragPossible() && !AdaptedControl.Capture))
            {
                Rectangle frameRect = GetItemBounds();
                if (!frameRect.IsEmpty)
                {
                    Matrix transform = m_transformAdapter.Transform;
                    frameRect = GdiUtil.Transform(transform, frameRect);
                    frameRect.Inflate(-HandleSize, -HandleSize);

                    e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(frameRect, Direction.TopLeft));
                    e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(frameRect, Direction.Top));
                    e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(frameRect, Direction.TopRight));
                    e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(frameRect, Direction.Right));
                    e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(frameRect, Direction.BottomRight));
                    e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(frameRect, Direction.Bottom));
                    e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(frameRect, Direction.BottomLeft));
                    e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(frameRect, Direction.Left));
                }
            }
        }

        private void d2dControl_DrawingD2d(object sender, EventArgs e)
        {
            
            // if we're dragging, or could drag and no one else is dragging...
            if (Dragging() ||
                (DragPossible() && !AdaptedControl.Capture))
            {
                var d2dControl = this.AdaptedControl as D2dAdaptableControl;
                D2dGraphics gfx = d2dControl.D2dGraphics;

                Rectangle frameRect = GetItemBounds();
                if (!frameRect.IsEmpty)
                {
                    Matrix transform = m_transformAdapter.Transform;
                    frameRect = GdiUtil.Transform(transform, frameRect);
                    frameRect.Inflate(-HandleSize, -HandleSize);
                    gfx.Transform = Matrix3x2F.Identity;
                    gfx.FillRectangle(GetHandleRect(frameRect, Direction.TopLeft), Color.Gray);
                    gfx.FillRectangle(GetHandleRect(frameRect, Direction.Top), Color.Gray);
                    gfx.FillRectangle(GetHandleRect(frameRect, Direction.TopRight), Color.Gray);
                    gfx.FillRectangle(GetHandleRect(frameRect, Direction.Right), Color.Gray);
                    gfx.FillRectangle(GetHandleRect(frameRect, Direction.BottomRight), Color.Gray);
                    gfx.FillRectangle(GetHandleRect(frameRect, Direction.Bottom), Color.Gray);
                    gfx.FillRectangle(GetHandleRect(frameRect, Direction.BottomLeft), Color.Gray);
                    gfx.FillRectangle(GetHandleRect(frameRect, Direction.Left), Color.Gray);
                    gfx.Transform = transform;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"></see> event. Performs custom actions on MouseDown event.</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseDown(object sender, MouseEventArgs e)
        {
            base.OnMouseDown(sender, e);

            if (DragPossible())
            {
                if (e.Button == MouseButtons.Left &&
                    ((Control.ModifierKeys & Keys.Alt) == 0)
                    && !AdaptedControl.Capture )
                {
                    {
                        m_direction = GetHitDirection(FirstPoint);
                        if (m_direction != Direction.None)
                        {
                            // capture the mouse immediately, so objects under manipulator don't become selected
                            //  by the click, and cause the manipulator to disappear
                            SetCursor(m_direction);
                            m_startingBounds = GetItemBounds();
                            m_draggingItems = GetItems();
                            m_originalBounds = new Rectangle[m_draggingItems.Length];
                            for (int i = 0; i < m_draggingItems.Length; i++)
                                m_layoutContext.GetBounds(m_draggingItems[i], out m_originalBounds[i]);

                            if (m_autoTranslateAdapter != null)
                                m_autoTranslateAdapter.Enabled = true;

                            AdaptedControl.Capture = true;
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"></see> event. Performs custom actions on MouseMove event.</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(sender, e);

            if (DragPossible())
            {
                if (e.Button == MouseButtons.None &&
                    AdaptedControl.Focused &&
                    AdaptedControl.Cursor == Cursors.Default)
                {
                    
                    Direction direction = GetHitDirection(new Point(e.X, e.Y));
                    SetCursor(direction);
                }
            }
        }

        /// <summary>
        /// Performs custom actions when performing a mouse dragging operation</summary>
        /// <param name="e">Mouse event args</param>
        /// <remarks>If dragging, raises the DrawingD2d event.</remarks>
        protected override void OnDragging(MouseEventArgs e)
        {          
            if (Dragging())
            {
                UpdateBounds();
                var d2dControl = this.AdaptedControl as D2dAdaptableControl;
                d2dControl.DrawD2d();                
            } 
        }

        private void UpdateBounds()
        {
            Matrix transform = GetTransform();
            for (int i = 0; i < m_draggingItems.Length; i++)
            {
                Rectangle bounds = GdiUtil.Transform(transform, m_originalBounds[i]);
                m_layoutContext.SetBounds(m_draggingItems[i], bounds, BoundsSpecified.All);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"></see> event. Performs custom actions on MouseUp event.</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseUp(object sender, MouseEventArgs e)
        {
            base.OnMouseUp(sender, e);

            if (e.Button == MouseButtons.Left)
            {
                if (Dragging())
                {
                    // Reset the positions to be their original values, before doing the transaction.
                    // The transaction will then record the correct before-and-after changes for undo/redo.
                    for (int i = 0; i < m_draggingItems.Length; i++)
                        m_layoutContext.SetBounds(m_draggingItems[i], m_originalBounds[i], BoundsSpecified.All);

                    var transactionContext = AdaptedControl.ContextAs<ITransactionContext>();
                    transactionContext.DoTransaction(UpdateBounds, "Resize States".Localize());

                    AdaptedControl.Invalidate();
                }

                if (m_autoTranslateAdapter != null)
                    m_autoTranslateAdapter.Enabled = false;
            }

            m_draggingItems = null;
            m_direction = Direction.None;
        }

        private void SetCursor(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                case Direction.Right:
                    AdaptedControl.Cursor = Cursors.SizeWE;
                    break;
                case Direction.Top:
                case Direction.Bottom:
                    AdaptedControl.Cursor = Cursors.SizeNS;
                    break;
                case Direction.TopLeft:
                case Direction.BottomRight:
                    AdaptedControl.Cursor = Cursors.SizeNWSE;
                    break;
                case Direction.TopRight:
                case Direction.BottomLeft:
                    AdaptedControl.Cursor = Cursors.SizeNESW;
                    break;
            }
        }

        // Create a transform matrix from m_startingBounds to a new boundary rectangle,
        //  based on the current mouse drag.
        private Matrix GetTransform()
        {
            Matrix transform = m_transformAdapter.Transform;
            PointF delta = GdiUtil.InverseTransformVector(transform, Delta);

            // For the uniform scale, map the first and last mouse points on to the correct
            //  line that goes through the current center of the bounding square.
            if (m_transformAdapter.UniformScale)
            {
                PointF dragDir = new PointF();
                switch (m_direction)
                {
                    case Direction.Left:
                        dragDir = new PointF(1, 0);
                        break;
                    case Direction.Right:
                        dragDir = new PointF(-1, 0);
                        break;
                    case Direction.Top:
                        dragDir = new PointF(0, 1);
                        break;
                    case Direction.Bottom:
                        dragDir = new PointF(0, -1);
                        break;
                    case Direction.TopLeft:
                        dragDir = new PointF(.707f, .707f);
                        break;
                    case Direction.BottomRight:
                        dragDir = new PointF(-.707f, -.707f);
                        break;
                    case Direction.TopRight:
                        dragDir = new PointF(-.707f, .707f);
                        break;
                    case Direction.BottomLeft:
                        dragDir = new PointF(.707f, -.707f);
                        break;
                }
                float distanceAlongDir = delta.X * dragDir.X + delta.Y * dragDir.Y;
                delta = new PointF(dragDir.X * distanceAlongDir, dragDir.Y * distanceAlongDir);
            }

            // Now find the correct 'currentBounds'.
            RectangleF currentBounds = m_startingBounds;
            switch (m_direction)
            {
                case Direction.Left:
                    currentBounds.X += delta.X;
                    currentBounds.Width -= delta.X;
                    if (m_transformAdapter.UniformScale)
                    {
                        currentBounds.Y += delta.X * 0.5f;
                        currentBounds.Height -= delta.X;
                    }
                    break;
                case Direction.Right:
                    currentBounds.Width += delta.X;
                    if (m_transformAdapter.UniformScale)
                    {
                        currentBounds.Y -= delta.X * 0.5f;
                        currentBounds.Height += delta.X;
                    }
                    break;
                case Direction.Top:
                    currentBounds.Y += delta.Y;
                    currentBounds.Height -= delta.Y;
                    if (m_transformAdapter.UniformScale)
                    {
                        currentBounds.X += delta.Y * 0.5f;
                        currentBounds.Width -= delta.Y;
                    }
                    break;
                case Direction.Bottom:
                    currentBounds.Height += delta.Y;
                    if (m_transformAdapter.UniformScale)
                    {
                        currentBounds.X -= delta.Y * 0.5f;
                        currentBounds.Width += delta.Y;
                    }
                    break;
                case Direction.TopLeft:
                    currentBounds.X += delta.X;
                    currentBounds.Width -= delta.X;
                    currentBounds.Y += delta.Y;
                    currentBounds.Height -= delta.Y;
                    break;
                case Direction.BottomRight:
                    currentBounds.Width += delta.X;
                    currentBounds.Height += delta.Y;
                    break;
                case Direction.TopRight:
                    currentBounds.Width += delta.X;
                    currentBounds.Y += delta.Y;
                    currentBounds.Height -= delta.Y;
                    break;
                case Direction.BottomLeft:
                    currentBounds.X += delta.X;
                    currentBounds.Width -= delta.X;
                    currentBounds.Height += delta.Y;
                    break;
            }

            // constrain dragged rectangle to be non-degenerate
            currentBounds.X = Math.Min(currentBounds.Left, m_startingBounds.Right - 1);
            currentBounds.Y = Math.Min(currentBounds.Top, m_startingBounds.Bottom - 1);
            currentBounds.Width = Math.Max(currentBounds.Width, 1);
            currentBounds.Height = Math.Max(currentBounds.Height, 1);

            return new Matrix(
                m_startingBounds,
                new PointF[]
                {
                    currentBounds.Location,
                    new PointF(currentBounds.Right, currentBounds.Top),
                    new PointF(currentBounds.Left, currentBounds.Bottom)
                });
        }

        private Direction GetHitDirection(Point point)
        {
            Rectangle frameRect = GetItemBounds();
            if (!frameRect.IsEmpty)
            {
                Matrix transform = m_transformAdapter.Transform;
                frameRect = GdiUtil.Transform(transform, frameRect);
                frameRect.Inflate(-HandleSize, -HandleSize);

                if (GetHandleRect(frameRect, Direction.TopLeft).Contains(point))
                    return Direction.TopLeft;
                else if (GetHandleRect(frameRect, Direction.Top).Contains(point))
                    return Direction.Top;
                else if (GetHandleRect(frameRect, Direction.TopRight).Contains(point))
                    return Direction.TopRight;
                else if (GetHandleRect(frameRect, Direction.Right).Contains(point))
                    return Direction.Right;
                else if (GetHandleRect(frameRect, Direction.BottomRight).Contains(point))
                    return Direction.BottomRight;
                else if (GetHandleRect(frameRect, Direction.Bottom).Contains(point))
                    return Direction.Bottom;
                else if (GetHandleRect(frameRect, Direction.BottomLeft).Contains(point))
                    return Direction.BottomLeft;
                else if (GetHandleRect(frameRect, Direction.Left).Contains(point))
                    return Direction.Left;
            }

            return Direction.None;
        }

        private Rectangle GetItemBounds()
        {
            IEnumerable<object> items = Dragging() ? m_draggingItems : GetItems();
            Rectangle result;
            LayoutContexts.GetBounds(m_layoutContext, items, out result);
            return result;
        }

        private bool Dragging()
        {
            return (m_draggingItems != null);
        }

        private bool DragPossible()
        {
            return CanSetBounds() != BoundsSpecified.None;
        }

        private BoundsSpecified CanSetBounds()
        {
            BoundsSpecified result = BoundsSpecified.None;
            if (m_selectionContext != null &&
                m_layoutContext != null)
            {
                result = HorizontallySizeable | VerticallySizeable;

                foreach (object item in m_selectionContext.Selection)
                {
                    BoundsSpecified specified = m_layoutContext.CanSetBounds(item);
                    if (CanSetBounds(specified))
                        result &= specified;
                }
            }

            return result;
        }

        private bool CanSetBounds(BoundsSpecified specified)
        {
            return
                ((specified & HorizontallySizeable) == HorizontallySizeable) ||
                ((specified & VerticallySizeable) == VerticallySizeable);
        }

        private object[] GetItems()
        {
            List<object> items = new List<object>();
            if (m_selectionContext != null &&
                m_layoutContext != null)
            {
                foreach (object item in m_selectionContext.Selection)
                {
                    BoundsSpecified specified = m_layoutContext.CanSetBounds(item);
                    if (CanSetBounds(specified))
                        items.Add(item);
                }
            }
            return items.ToArray();
        }

        private Rectangle GetHandleRect(Rectangle bounds, Direction direction)
        {
            Rectangle result = new Rectangle(
                bounds.X + bounds.Width / 2 - HandleSize,
                bounds.Y + bounds.Height / 2 - HandleSize,
                HandleSize * 2,
                HandleSize * 2);

            if ((direction & Direction.Left) != 0)
                result.X = bounds.Left - HandleSize;
            else if ((direction & Direction.Right) != 0)
                result.X = bounds.Right - HandleSize;
            if ((direction & Direction.Top) != 0)
                result.Y = bounds.Top - HandleSize;
            else if ((direction & Direction.Bottom) != 0)
                result.Y = bounds.Bottom - HandleSize;

            return result;
        }

        private readonly ITransformAdapter m_transformAdapter;
        private IAutoTranslateAdapter m_autoTranslateAdapter;
        private ILayoutContext m_layoutContext;
        private ISelectionContext m_selectionContext;

        [Flags]
        enum Direction
        {
            None = 0,
            Left = 1,
            Top = 2,
            Right = 4,
            Bottom = 8,
            TopLeft = Left | Top,
            TopRight = Right | Top,
            BottomRight = Right | Bottom,
            BottomLeft = Left | Bottom,
        }

        private Direction m_direction = Direction.None;
        private object[] m_draggingItems;
        private Rectangle[] m_originalBounds;
        private Rectangle m_startingBounds;

        private const BoundsSpecified HorizontallySizeable = BoundsSpecified.X | BoundsSpecified.Width;
        private const BoundsSpecified VerticallySizeable = BoundsSpecified.Y | BoundsSpecified.Height;
    }
}
