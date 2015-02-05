//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Base class for Selection Behaviors.
    /// Can be used on Selectors or TreeViews</summary>
    public abstract class SelectionBehaviorBase : Behavior<Selector>
    {
        /// <summary>
        /// Selection context dependency property</summary>
        public static readonly DependencyProperty SelectionContextProperty =
            DependencyProperty.Register("SelectionContext", 
                typeof(ISelectionContext), typeof(SelectionBehaviorBase), 
                    new PropertyMetadata(default(ISelectionContext), SelectionContextPropertyChanged));

        /// <summary>
        /// Get or set selection context dependency property</summary>
        public ISelectionContext SelectionContext
        {
            get { return (ISelectionContext)GetValue(SelectionContextProperty); }
            set { SetValue(SelectionContextProperty, value); }
        }

        private static void SelectionContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
           ((SelectionBehaviorBase)d).ChangedContext(e.OldValue as ISelectionContext);
        }

        /// <summary>
        /// Whether to enable selection dependency property</summary>
        public static readonly DependencyProperty EnableSelectionClearProperty =
            DependencyProperty.Register("EnableSelectionClear", typeof(bool), typeof(SelectionBehaviorBase));

        /// <summary>
        /// Get or set enable selection dependency property</summary>
        public bool EnableSelectionClear
        {
            get { return (bool)GetValue(EnableSelectionClearProperty); }
            set { SetValue(EnableSelectionClearProperty, value); }
        }

        private void ChangedContext(ISelectionContext oldSelectionContext)
        {
            if (oldSelectionContext == SelectionContext)
                return;

            if (oldSelectionContext != null)
            {
                oldSelectionContext.SelectionChanging -= SelectionContext_SelectionChanging;
                oldSelectionContext.SelectionChanged -= SelectionContext_SelectionChanged;
            }

            if (SelectionContext != null)
            {
                SelectionContext.SelectionChanging += SelectionContext_SelectionChanging;
                SelectionContext.SelectionChanged += SelectionContext_SelectionChanged;
            }

            OnSelectionContextChanged();
        }

        #region Overrides

        /// <summary>
        /// Handle Attached event</summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += SelectorSelectionChanged;
            AssociatedObject.PreviewMouseDown += SelectorMouseDown;
        }

        /// <summary>
        /// Handle Detaching event</summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= SelectorSelectionChanged;
            AssociatedObject.PreviewMouseDown -= SelectorMouseDown;
        }

        #endregion

        #region Virtuals

        /// <summary>
        /// Handle AssociatedObjectSelectionChanged event</summary>
        protected virtual void OnSelectionContextChanged()
        {
        }

        /// <summary>
        /// Handle SelectionContextSelectionChanging event</summary>
        protected virtual void OnSelectionContextSelectionChanging()
        {
        }

        /// <summary>
        /// Handle SelectionContextSelectionChanged event</summary>
        protected virtual void OnSelectionContextSelectionChanged()
        {
        }

        /// <summary>Handle AssociatedObjectSelectionChanged event</summary>
        /// <param name="addeditems">List of items added</param>
        /// <param name="removedItems">List of items removed</param>
        protected virtual void OnAssociatedObjectSelectionChanged(IList addeditems, IList removedItems)
        {
        }

        #endregion

        void SelectionContext_SelectionChanged(object sender, EventArgs e)
        {
            OnSelectionContextSelectionChanged();
        }

        void SelectionContext_SelectionChanging(object sender, EventArgs e)
        {
            OnSelectionContextSelectionChanging();
        }

        void SelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource == AssociatedObject)
                OnAssociatedObjectSelectionChanged(e.AddedItems, e.RemovedItems);
        }

        void SelectorMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!EnableSelectionClear)
                return;
            
            var current = e.OriginalSource as DependencyObject;
            while (current != null)
            {
                // If we get a ListBox before getting a ListBoxItem, then the mouse was not down on ListBoxItem.
                var result = current as ListBox;
                if (result != null)
                {
                    AssociatedObject.SelectedItem = null;
                    break;
                }

                var item = current as ListBoxItem;
                if (item != null)
                    break;

                current = current.GetVisualOrLogicalParent();
            }
        }
    }

    /// <summary>
    /// Behavior to bind the selection of items in a selector to an ISelectionContext.
    /// Note that binding is only one way, i.e., selection in the selector is mirrored
    /// into the ISelectionContext, but not the other way.</summary>
    public class ToSourceSelectionBehavior : SelectionBehaviorBase
    {
        #region Overrides

        /// <summary>
        /// Handle Attached event</summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            Composer.Current.Container.SatisfyImportsOnce(this);
        }

        /// <summary>
        /// Raises the AssociatedObjectSelectionChanged event and performs custom processing</summary>
        /// <param name="addeditems">Items added to selection</param>
        /// <param name="removedItems">Items removed to selection</param>
        protected override void OnAssociatedObjectSelectionChanged(IList addeditems, IList removedItems)
        {
            base.OnAssociatedObjectSelectionChanged(addeditems, removedItems);

            if (SelectionContext != null)
            {
                if (removedItems.Count > 0)
                    SelectionContext.RemoveRange(removedItems);

                if (addeditems.Count > 0)
                    SelectionContext.AddRange(addeditems);
            }
        }

        #endregion
    }

    /// <summary>
    /// Behavior to add two way selection binding support between an 
    /// ISelectionContext and a selector or multi-selector</summary>
    public class TwoWaySelectionBehavior : SelectionBehaviorBase
    {
        #region Overrides

        /// <summary>
        /// Raises behavior Attached event and performs custom actions</summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is MultiSelector)
            {
                var multiSelector = AssociatedObject as MultiSelector;
                m_associatedObjectSelection = multiSelector.SelectedItems;
            }
            else if (AssociatedObject is ListBox)
            {
                var listBox = AssociatedObject as ListBox;
                if(listBox.SelectionMode != SelectionMode.Single)
                    m_associatedObjectSelection = (AssociatedObject as ListBox).SelectedItems;
            }

            Composer.Current.Container.SatisfyImportsOnce(this);
        }

        /// <summary>
        /// Raises behavior Attached event and performs custom actions</summary>
        /// <param name="addeditems">Items added to selection</param>
        /// <param name="removedItems">Items removed to selection</param>
        protected override void OnAssociatedObjectSelectionChanged(IList addeditems, IList removedItems)
        {
            base.OnAssociatedObjectSelectionChanged(addeditems, removedItems);

            if (!m_synchronising)
            {
                try
                {
                    m_synchronising = true;

                    if (SelectionContext != null)
                    {
                        if (m_associatedObjectSelection != null)
                        {
                            // In muti select mode remove specific items
                            if (removedItems.Count > 0)
                            {
                                var converted = ConvertToSelectionContext(removedItems);
                                SelectionContext.RemoveRange(converted);
                            }
                        }
                        else
                        {
                            // In single select mode always clear selection
                            SelectionContext.Clear();
                        }

                        if (addeditems.Count > 0)
                        {
                            var converted = ConvertToSelectionContext(addeditems);
                            SelectionContext.AddRange(converted);
                        }
                    }
                }
                finally
                {
                    m_synchronising = false;
                }
            }
        }

        /// <summary>
        /// Raises SelectionContextSelectionChanged event and performs custom actions</summary>
        protected override void OnSelectionContextSelectionChanged()
        {
            base.OnSelectionContextSelectionChanged();

            if (!m_synchronising && SelectionContext != null)
            {
                try
                {
                    object[] newSelection = SelectionContext.Selection.ToArray();

                    if (m_associatedObjectSelection != null)
                    {
                        // Multi selection mode
                        SynchoniseMultiSelectionToAssociateObject(m_associatedObjectSelection, newSelection);
                    }
                    else
                    {
                        // Single selection mode
                        SynchoniseSingleSelectionToAssociateObject(newSelection);
                    }
                }
                finally
                {
                    m_synchronising = false;
                }
            }
        }

        #endregion

        #region Virtuals

        /// <summary>
        /// Converts from an item that can be selected in this selection context</summary>
        /// <param name="item">Item to be converted</param>
        /// <returns>Selection context</returns>
        protected virtual object ConvertFromSelectionContext(object item)
        {
            // Check that this item exists in the ItemsSource of the AssociatedObject
            if (AssociatedObject != null)
            {
                if (!AssociatedObject.Items.Contains(item))
                    return null;
            }

            return item;
        }

        /// <summary>
        /// Converts an item to one that can be selected in this selection context</summary>
        /// <param name="item">Item to be converted</param>
        /// <returns>Converted item</returns>
        protected virtual object ConvertToSelectionContext(object item)
        {
            return item;
        }

        #endregion

        #region Private Methods

        private void SynchoniseMultiSelectionToAssociateObject(IList associatedObjectSelection, object[] newSelection)
        {
            // Remove items from AssociatedObject selection which are no
            // longer in the ISelectionContext
            foreach (var item in m_associatedObjectSelection.Cast<object>().ToArray())
            {
                object converted = ConvertToSelectionContext(item);
                if (converted != null)
                {
                    if (!newSelection.Contains(converted))
                    {
                        m_associatedObjectSelection.Remove(item);
                    }
                }
            }

            // Add any new items in the ISlectionContext to the selection 
            // in the AssociatedObject
            foreach (var item in newSelection)
            {
                object converted = ConvertFromSelectionContextAndCheckExistance(item);
                if (converted != null)
                {
                    m_associatedObjectSelection.Add(converted);
                }
            }
        }

        private void SynchoniseSingleSelectionToAssociateObject(object[] newSelection)
        {
            // Find most recently selected item from ISelectionContext which exists in the ItemsSource
            // of the AssociatedObject
            object selected = null;

            foreach (var item in newSelection)
            {
                object converted = ConvertFromSelectionContextAndCheckExistance(item);
                if (converted != null)
                {
                    selected = converted;
                    break;
                }
            }

            AssociatedObject.SelectedItem = selected;
        }

        private object ConvertFromSelectionContextAndCheckExistance(object item)
        {
            object result = ConvertFromSelectionContext(item);

            // Check that this item exists in the ItemsSource of the AssociatedObject
            if (result != null && AssociatedObject != null)
            {
                if (!AssociatedObject.Items.Contains(item))
                    return null;
            }

            return result;
        }

        private IEnumerable ConvertToSelectionContext(IEnumerable items)
        {
            foreach (var item in items)
            {
                object converted = ConvertToSelectionContext(item);
                if (converted != null)
                    yield return converted;
            }
        }

        #endregion

        #region Private Fields

        private bool m_synchronising;
        private IList m_associatedObjectSelection;

        #endregion
    }

    /// <summary>
    /// Adapter for TwoWaySelectionBehavior</summary>
    public class AdaptingTwoWaySelectionBehavior : TwoWaySelectionBehavior
    {
        /// <summary>
        /// Get or set adapter Type</summary>
        public Type AdapterType { get; set; }

        /// <summary>
        /// Adapt item to AdapterType</summary>
        /// <exception cref="InvalidOperationException"> if AdapterType not set</exception>
        /// <param name="item">Item to adapt</param>
        /// <returns>Adapter for item of AdapterType</returns>
        protected override object ConvertFromSelectionContext(object item)
        {
            if (AdapterType == null)
                throw new InvalidOperationException("AdapterType must be set on AdaptingTwoWaySelectionBehavior");

            var adaptable = item as IAdaptable;
            return adaptable != null ? adaptable.As(AdapterType) : null;
        }
    }
}
