//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Displays the associated context menu on a button click.</summary>
    public class ButtonPopupBehavior : Behavior<Button>
    {
        /// <summary>
        /// Called after the Behavior is attached to an AssociatedObject.</summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += OnButtonClick;
        }

        /// <summary>
        /// Called after the Behavior is detached from an AssociatedObject.</summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Click -= OnButtonClick;
        }

        void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // open the context menu attached to the button
            var button = sender as Button;

            if (button != null && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = PlacementMode.Bottom;
                ContextMenuService.SetPlacement(button, PlacementMode.Bottom);
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}