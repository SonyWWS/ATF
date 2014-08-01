//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Behavior to draw insertion adorners on items controls
    /// For list based controls this will draw an insertion line while dragging
    /// Items over the control.
    /// For TreeViews this will highlight the drop target treeview item and optionaly
    /// add insertion lines
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ItemsControlInsertionAdornerBehavior<T> : Behavior<T>
       where T : ItemsControl
    {
        protected virtual void OnDragEnter(DragEventArgs e) { }

        protected virtual void OnDragOver(DragEventArgs e) { }

        protected virtual void OnDrop(DragEventArgs e) { }

        protected virtual void OnDragLeave(DragEventArgs e) { }

        #region Overrides

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AllowDrop = true;
            AssociatedObject.Drop += OnDrop;
            AssociatedObject.DragEnter += OnDragEnter;
            AssociatedObject.DragLeave += OnDragLeave;
            AssociatedObject.DragOver += OnDragOver;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Drop -= OnDrop;
            AssociatedObject.DragEnter -= OnDragEnter;
            AssociatedObject.DragLeave -= OnDragLeave;
            AssociatedObject.DragOver -= OnDragOver;
        }

        #endregion

        void OnDragEnter(object sender, DragEventArgs e)
        {
        }

        void OnDragLeave(object sender, DragEventArgs e)
        {
        }

        void OnDragOver(object sender, DragEventArgs e)
        {
        }

        void OnDrop(object sender, DragEventArgs e)
        {
        }
    }
}
