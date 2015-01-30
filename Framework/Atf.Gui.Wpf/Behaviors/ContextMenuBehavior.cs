//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

using Sce.Atf.Wpf.Interop;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Behavior to provide hook for ATF context menu service.
    /// Right clicking on the associated element queries all context menu
    /// providers for commands using the element's data context as the query target.
    /// If commands exist, a context menu is displayed.</summary>
    public class ContextMenuBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// Context dependency property</summary>
        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(object), typeof(ContextMenuBehavior), new PropertyMetadata(default(object)));

        /// <summary>
        /// Get or set context dependency property</summary>
        public object Context
        {
            get { return (object)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        /// <summary>
        /// Handle Attached event</summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseRightButtonUp += Element_MouseRightButtonUp;
        }

        /// <summary>
        /// Handle Detaching event</summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseRightButtonUp -= Element_MouseRightButtonUp;
        }

        /// <summary>
        /// Return context dependency property or sender data context if null</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Mouse button event arguments</param>
        /// <returns>Context dependency property or sender data context if null</returns>
        protected virtual object GetCommandContext(object sender, MouseButtonEventArgs e)
        {
            var senderFwe = (FrameworkElement)sender;
            object context = Context;
            if (context == null)
                context = senderFwe.DataContext;
            return context;
        }

        /// <summary>
        /// Return command target</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Mouse button event arguments</param>
        /// <returns>Command target</returns>
        protected virtual object GetCommandTarget(object sender, MouseButtonEventArgs e)
        {
            // Default behavior to find command target is to just take the data context 
            // of the clicked element
            object clickedData = null;

            var originalSource = e.OriginalSource as DependencyObject;
            if (originalSource != null)
            {
                var fwe = originalSource.FindAncestor<FrameworkElement>();
                if (fwe != null)
                    clickedData = fwe.DataContext;
            }

            return clickedData;
        }

        private void Element_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            object context = GetCommandContext(sender, e);
            object clickedData = GetCommandTarget(sender, e);

            if (context != null && clickedData != null)
            {
                var service = Composer.Current.Container.GetExportedValueOrDefault<IContextMenuService>();
                if (service != null)
                {
                    var providers = Composer.Current.Container.GetExportedValues<Atf.Applications.IContextMenuCommandProvider>();
                    
                    IEnumerable<object> commands =
                       Atf.Applications.ContextMenuCommandProvider.GetCommands(
                           providers,
                           context,
                           clickedData);

                    service.RunContextMenu(commands, (FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));

                    e.Handled = true;
                }
            }
        }
    }

    /// <summary>
    /// Behavior for ItemsControl that context menu is associated with</summary>
    public class ItemsControlContextMenuBehavior : ContextMenuBehavior
    {
        /// <summary>
        /// Handle Attached event</summary>
        /// <exception cref="InvalidOperationException"> if AssociatedObject not ItemsControl</exception>
        protected override void OnAttached()
        {
            if (!(AssociatedObject is ItemsControl))
                throw new InvalidOperationException("ItemsControlContextMenuBehavior can only be used on ItemsControl");

            base.OnAttached();
        }

        /// <summary>
        /// Get the command target for an ItemsControl item</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Mouse button event arguments</param>
        /// <returns>Command target for ItemsControl item</returns>
        protected override object GetCommandTarget(object sender, MouseButtonEventArgs e)
        {
            // Only return a command target if an ItemsControl item was clicked
            // if the click was in an empty area do not return anything
            var itemsControl = (ItemsControl)AssociatedObject;
            var hitItem = itemsControl.GetItemAtPoint(e.GetPosition(itemsControl));
            return hitItem;
        }
    }
}
