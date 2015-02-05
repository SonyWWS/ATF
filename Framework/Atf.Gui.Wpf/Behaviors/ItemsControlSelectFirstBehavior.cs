//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// A useful behavior for items such as ComboBoxes, which do not select the first item
    /// in the list when the ItemsSource changes and the control has previously been disabled</summary>
    public class SelectorSelectFirstBehavior : Behavior<Selector>
    {
        #region Overrides

        /// <summary>
        /// Handle Attached event</summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
            AssociatedObject.IsEnabledChanged += OnIsEnabledChanged;
        }

        /// <summary>
        /// Handle Detaching event</summary>
        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
            AssociatedObject.IsEnabledChanged -= OnIsEnabledChanged;
            base.OnDetaching();
        }

        #endregion

        /// <summary>
        /// Handle SelectionChanged event</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        void OnSelectionChanged(object sender, EventArgs e)
        {
            if (AssociatedObject.Items.Count > 0 && AssociatedObject.SelectedItem == null)
            {
                AssociatedObject.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Handle IsEnabledChanged event</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Dependency property changed event arguments</param>
        void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (AssociatedObject.Items.Count > 0 && AssociatedObject.SelectedItem == null)
            {
                AssociatedObject.SelectedIndex = 0;
            }
        }
    }
}