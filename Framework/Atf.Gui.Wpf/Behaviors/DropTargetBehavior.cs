//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System.Windows;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Base drop target behavior</summary>
    /// <typeparam name="T">Owner type</typeparam>
    public abstract class DropTargetBehavior<T> : Behavior<T>
        where T : FrameworkElement
    {
        /// <summary>
        /// Method called on DragEnter events</summary>
        /// <param name="e">DragEventArgs containing event information</param>
        protected virtual void OnDragEnter(DragEventArgs e) { }

        /// <summary>
        /// Method called on DragOver events</summary>
        /// <param name="e">DragEventArgs containing event information</param>
        protected virtual void OnDragOver(DragEventArgs e) { }

        /// <summary>
        /// Method called on Drop events</summary>
        /// <param name="e">DragEventArgs containing event information</param>
        protected virtual void OnDrop(DragEventArgs e) { }

        /// <summary>
        /// Method called on DragLeave events</summary>
        /// <param name="e">DragEventArgs containing event information</param>
        protected virtual void OnDragLeave(DragEventArgs e) { }

        #region Overrides

        /// <summary>
        /// Raises behavior Attached event and performs custom actions</summary>
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

        /// <summary>
        /// Raises the DragEnter event and performs custom processing</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">DragEventArgs that contains the event data</param>
        void OnDragEnter(object sender, DragEventArgs e)
        {
            OnDragEnter(e);
        }

        /// <summary>
        /// Raises the DragLeave event and performs custom processing</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">DragEventArgs that contains the event data</param>
        void OnDragLeave(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            OnDragLeave(e);
        }

        /// <summary>
        /// Raises the DragOver event and performs custom processing</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">DragEventArgs that contains the event data</param>
        void OnDragOver(object sender, DragEventArgs e)
        {
            OnDragOver(e);
        }

        /// <summary>
        /// Raises the Drop event and performs custom processing</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">DragEventArgs that contains the event data</param>
        void OnDrop(object sender, DragEventArgs e)
        {
            OnDrop(e);
        }
    }

}
