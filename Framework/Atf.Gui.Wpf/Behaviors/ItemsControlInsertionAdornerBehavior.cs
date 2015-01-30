//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Behavior to draw insertion adorners on items controls.
    /// For list based controls, this draws an insertion line while dragging
    /// Items over the control.
    /// For TreeViews, this highlights the drop target treeview item and optionally
    /// adds insertion lines.</summary>
    /// <typeparam name="T">Type of DependencyObject</typeparam>
    public abstract class ItemsControlInsertionAdornerBehavior<T> : Behavior<T>
       where T : ItemsControl
    {
        /// <summary>
        /// Handle DragEnter event</summary>
        /// <param name="e">Drag event arguments</param>
        protected virtual void OnDragEnter(DragEventArgs e) { }

        /// <summary>
        /// Handle DragOver event</summary>
        /// <param name="e">Drag event arguments</param>
        protected virtual void OnDragOver(DragEventArgs e) { }

        /// <summary>
        /// Handle Drop event</summary>
        /// <param name="e">Drag event arguments</param>
        protected virtual void OnDrop(DragEventArgs e) { }

        /// <summary>
        /// Handle DragLeave event</summary>
        /// <param name="e">Drag event arguments</param>
        protected virtual void OnDragLeave(DragEventArgs e) { }

        #region Overrides

        /// <summary>
        /// Handle Attached event</summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AllowDrop = true;
            AssociatedObject.Drop += OnDrop;
            AssociatedObject.DragEnter += OnDragEnter;
            AssociatedObject.DragLeave += OnDragLeave;
            AssociatedObject.DragOver += OnDragOver;
        }

        /// <summary>
        /// Handle Detaching event</summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Drop -= OnDrop;
            AssociatedObject.DragEnter -= OnDragEnter;
            AssociatedObject.DragLeave -= OnDragLeave;
            AssociatedObject.DragOver -= OnDragOver;
        }

        #endregion

        /// <summary>
        /// Handle DragEnter event</summary>
        /// <param name="sender"></param>
        /// <param name="e">Drag event arguments</param>
        void OnDragEnter(object sender, DragEventArgs e)
        {
        }

        /// <summary>
        /// Handle DragLeave event</summary>
        /// <param name="sender"></param>
        /// <param name="e">Drag event arguments</param>
        void OnDragLeave(object sender, DragEventArgs e)
        {
        }

        /// <summary>
        /// Handle DragOver event</summary>
        /// <param name="sender"></param>
        /// <param name="e">Drag event arguments</param>
        void OnDragOver(object sender, DragEventArgs e)
        {
        }

        /// <summary>
        /// Handle Drop event</summary>
        /// <param name="sender"></param>
        /// <param name="e">Drag event arguments</param>
        void OnDrop(object sender, DragEventArgs e)
        {
        }
    }
}
