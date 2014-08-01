//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Drop target behavior for items control to accept drag drop operations
    /// Drop target is selected using this order of preference:
    /// 1. If drop is over an item from the control, this is used as the target
    /// 2. If not then the RootTarget is used
    /// 3. If RootTarget is not set then the DataContext of the items control is used
    /// </summary>
    public class ItemsControlDropTargetBehavior : DropTargetBehavior<ItemsControl>
    {
        public static readonly DependencyProperty RootTargetProperty =
            DependencyProperty.Register("RootTarget", typeof(object), typeof(ItemsControlDropTargetBehavior), new PropertyMetadata(default(object)));

        public object RootTarget
        {
            get { return (object)GetValue(RootTargetProperty); }
            set { SetValue(RootTargetProperty, value); }
        }

        protected virtual object GetDropTarget(DragEventArgs e)
        {
            var pos = e.GetPosition(AssociatedObject);
            object parent = AssociatedObject.GetItemAtPoint(pos);

            if (parent == null)
            {
                parent = RootTarget ?? AssociatedObject.DataContext;
            }
            return parent;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            object target = GetDropTarget(e);

            if (ApplicationUtil.CanInsert(AssociatedObject.DataContext, target, e.Data))
            {
                if ((e.AllowedEffects & DragDropEffects.Copy) > 0 && 
                    (e.KeyStates & DragDropKeyStates.ControlKey) > 0)
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.Move;
                }
            }
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        /// <summary>
        /// Method called on Drop events</summary>
        /// <param name="e">DragEventArgs containing event information</param>
        protected override void OnDrop(DragEventArgs e)
        {
            object target = GetDropTarget(e);

            if (!ApplicationUtil.CanInsert(AssociatedObject.DataContext, target, e.Data))
                return;

            IDataObject dataObject = e.Data;

            if ((e.AllowedEffects & DragDropEffects.Copy) > 0 &&
                    (e.KeyStates & DragDropKeyStates.ControlKey) > 0)
            {
                var registry = Composer.Current.Container.GetExportedValueOrDefault<IContextRegistry>();
                if(registry != null)
                {
                    // Attempt to get context which was the source of this drag operation
                    var ctx = registry.GetActiveContext<IInstancingContext>();
                    if(ctx != null && ctx.CanCopy())
                    {
                        object rawObject = ctx.Copy();
                        dataObject = rawObject as IDataObject ?? new DataObject(rawObject);
                    }
                }
            }

            var statusService = Composer.Current.Container.GetExportedValueOrDefault<IStatusService>();
            ApplicationUtil.Insert(AssociatedObject.DataContext, target, dataObject, "Drag Drop".Localize(), statusService);

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

    }

    /// <summary>
    /// ItemsControl indexed drop target behavior</summary>
    public class ItemsControlIndexedDropTargetBehavior : DropTargetBehavior<ItemsControl>
    {
        /// <summary>
        /// Raises behavior Attached event and performs custom actions</summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            m_hasVerticalOrientation = false;// HasVerticalOrientation(AssociatedObject);
        }

        /// <summary>
        /// Method called on DragEnter events</summary>
        /// <param name="e">DragEventArgs containing event information</param>
        protected override void OnDragEnter(DragEventArgs e)
        {
            //CreateInsertionAdorner();
        }

        private void CreateInsertionAdorner()
        {
           // var adornerLayer = AdornerLayer.GetAdornerLayer(this.targetItemContainer);
           // this.insertionAdorner = new InsertionAdorner(this.hasVerticalOrientation, this.isInFirstHalf, this.targetItemContainer, adornerLayer);

        }

        /// <summary>
        /// Method called on DragOver events</summary>
        /// <param name="e">DragEventArgs containing event information</param>
        protected override void OnDragOver(DragEventArgs e)
        {
            int insertionIndex = AssociatedObject.Items.Count;
            bool isInFirstHalf = false;

            var source = (DependencyObject)e.OriginalSource;

            FrameworkElement container = null;

            var tv = AssociatedObject as TreeView;
            if (tv != null)
            {
                container = source.FindAncestor<TreeViewItem>();
            }
            else
            {
                container = AssociatedObject.ContainerFromElement(source) as FrameworkElement;
            }

            if (container != null)
            {
                Point positionRelativeToItemContainer = e.GetPosition(container);
                isInFirstHalf = IsInFirstHalf(container, positionRelativeToItemContainer, m_hasVerticalOrientation);

                ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
                insertionIndex = itemsControl.ItemContainerGenerator.IndexFromContainer(container);

                if (!isInFirstHalf)
                    insertionIndex++;
            }

            object parentData = container != null ? container.DataContext : AssociatedObject.DataContext;

            if (ApplicationUtil.CanInsert(AssociatedObject.DataContext, parentData, e.Data))
            {
                if ((e.AllowedEffects & DragDropEffects.Copy) > 0 &&
                   (e.KeyStates & DragDropKeyStates.ControlKey) > 0)
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.Move;
                }
                e.Handled = true;
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            var pos = e.GetPosition(AssociatedObject);
            var parent = AssociatedObject.GetItemAtPoint(pos);

            if (parent == null)
                parent = AssociatedObject.DataContext;

            if (ApplicationUtil.CanInsert(AssociatedObject.DataContext, parent, e.Data))
            {
                var statusService = Composer.Current.Container.GetExportedValueOrDefault<IStatusService>();
                ApplicationUtil.Insert(AssociatedObject.DataContext, parent, e.Data, "Drag Drop".Localize(), statusService);
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private static bool HasVerticalOrientation(FrameworkElement itemContainer)
        {
            bool hasVerticalOrientation = true;
            if (itemContainer != null)
            {
                Panel panel = VisualTreeHelper.GetParent(itemContainer) as Panel;
                StackPanel stackPanel;
                WrapPanel wrapPanel;

                if ((stackPanel = panel as StackPanel) != null)
                {
                    hasVerticalOrientation = (stackPanel.Orientation == Orientation.Vertical);
                }
                else if ((wrapPanel = panel as WrapPanel) != null)
                {
                    hasVerticalOrientation = (wrapPanel.Orientation == Orientation.Vertical);
                }
                // You can add support for more panel types here.
                else
                {
                    throw new NotSupportedException("Only items controls with StackPanel or WrapPanel currently supported by ItemsControlIndexedDropTargetBehavior");
                }
            }
            return hasVerticalOrientation;
        }

        private static bool IsInFirstHalf(FrameworkElement container, Point clickedPoint, bool hasVerticalOrientation)
        {
            if (hasVerticalOrientation)
            {
                return clickedPoint.Y < container.ActualHeight / 2;
            }
            return clickedPoint.X < container.ActualWidth / 2;
        }

        private bool m_hasVerticalOrientation;
    }

    /// <summary>
    /// Adorner class for object being dragged over</summary>
    public class InsertionAdorner : Adorner
    {
        private bool isSeparatorHorizontal;
        /// <summary>
        /// Gets or sets whether dragged object in first half of object dragged over</summary>
        public bool IsInFirstHalf { get; set; }
        private AdornerLayer adornerLayer;
        private static Pen pen;
        private static PathGeometry triangle;

        /// <summary>
        /// Static constructor that creates the pen and triangle and freezes them to improve performance</summary>
        static InsertionAdorner()
        {
            pen = new Pen { Brush = Brushes.Gray, Thickness = 2 };
            pen.Freeze();

            LineSegment firstLine = new LineSegment(new Point(0, -5), false);
            firstLine.Freeze();
            LineSegment secondLine = new LineSegment(new Point(0, 5), false);
            secondLine.Freeze();

            PathFigure figure = new PathFigure { StartPoint = new Point(5, 0) };
            figure.Segments.Add(firstLine);
            figure.Segments.Add(secondLine);
            figure.Freeze();

            triangle = new PathGeometry();
            triangle.Figures.Add(figure);
            triangle.Freeze();
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="isSeparatorHorizontal">Whether separator between halves of object dragged over is horizontal</param>
        /// <param name="isInFirstHalf">Whether dragged object in first half of object dragged over</param>
        /// <param name="adornedElement">Adorned element being dragged over</param>
        /// <param name="adornerLayer">Adorner layer</param>
        public InsertionAdorner(bool isSeparatorHorizontal, bool isInFirstHalf, UIElement adornedElement, AdornerLayer adornerLayer)
            : base(adornedElement)
        {
            this.isSeparatorHorizontal = isSeparatorHorizontal;
            IsInFirstHalf = isInFirstHalf;
            this.adornerLayer = adornerLayer;
            IsHitTestVisible = false;

            this.adornerLayer.Add(this);
        }

        /// <summary>
        /// Renders adorner. This draws one line and two triangles at each end of the line.</summary>
        /// <param name="drawingContext">Drawing context for rendering</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            Point startPoint;
            Point endPoint;

            CalculateStartAndEndPoint(out startPoint, out endPoint);
            drawingContext.DrawLine(pen, startPoint, endPoint);

            if (isSeparatorHorizontal)
            {
                DrawTriangle(drawingContext, startPoint, 0);
                DrawTriangle(drawingContext, endPoint, 180);
            }
            else
            {
                DrawTriangle(drawingContext, startPoint, 90);
                DrawTriangle(drawingContext, endPoint, -90);
            }
        }

        private void DrawTriangle(DrawingContext drawingContext, Point origin, double angle)
        {
            drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
            drawingContext.PushTransform(new RotateTransform(angle));

            drawingContext.DrawGeometry(pen.Brush, null, triangle);

            drawingContext.Pop();
            drawingContext.Pop();
        }

        private void CalculateStartAndEndPoint(out Point startPoint, out Point endPoint)
        {
            startPoint = new Point();
            endPoint = new Point();

            double width = AdornedElement.RenderSize.Width;
            double height = AdornedElement.RenderSize.Height;

            if (isSeparatorHorizontal)
            {
                endPoint.X = width;
                if (!IsInFirstHalf)
                {
                    startPoint.Y = height;
                    endPoint.Y = height;
                }
            }
            else
            {
                endPoint.Y = height;
                if (!IsInFirstHalf)
                {
                    startPoint.X = width;
                    endPoint.X = width;
                }
            }
        }

        /// <summary>
        /// Removes this object from the adorner layer</summary>
        public void Detach()
        {
            adornerLayer.Remove(this);
        }

    }
}
