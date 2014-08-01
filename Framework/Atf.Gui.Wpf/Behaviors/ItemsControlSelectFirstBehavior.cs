//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// A useful behavior for items such as ComboBoxes which do not select the first item
    /// in the list when the ItemsSource changes and the control has previously been disabled.
    /// </summary>
    public class SelectorSelectFirstBehavior : Behavior<Selector>
    {
        #region Overrides

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
            AssociatedObject.IsEnabledChanged += OnIsEnabledChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
            AssociatedObject.IsEnabledChanged -= OnIsEnabledChanged;
            base.OnDetaching();
        }

        #endregion

        void OnSelectionChanged(object sender, EventArgs e)
        {
            if (AssociatedObject.Items.Count > 0 && AssociatedObject.SelectedItem == null)
            {
                AssociatedObject.SelectedIndex = 0;
            }
        }

        void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (AssociatedObject.Items.Count > 0 && AssociatedObject.SelectedItem == null)
            {
                AssociatedObject.SelectedIndex = 0;
            }
        }
    }
}