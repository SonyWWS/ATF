//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Selection behaviors</summary>
    public static class SelectionBehaviors
    {
        #region SelectionContext Attached Property

        /// <summary>
        /// SelectionContext attached property.
        /// This is used by several other behaviors.</summary>
        public static readonly DependencyProperty SelectionContextProperty =
            DependencyProperty.RegisterAttached("SelectionContext", typeof(ISelectionContext), typeof(SelectionBehaviors), new PropertyMetadata(SelectionContextPropertyChanged));

        /// <summary>
        /// Gets SelectionContext attached property</summary>
        /// <param name="obj">Dependency object to obtain property for</param>
        /// <returns>SelectionContext attached property</returns>
        public static ISelectionContext GetSelectionContext(DependencyObject obj)
        {
            return (ISelectionContext)obj.GetValue(SelectionContextProperty);
        }

        /// <summary>
        /// Sets SelectionContext attached property</summary>
        /// <param name="obj">Dependency object to set property for</param>
        /// <param name="value">SelectionContext attached property</param>
        public static void SetSelectionContext(DependencyObject obj, ISelectionContext value)
        {
            obj.SetValue(SelectionContextProperty, value);
        }

        #endregion

        private static void SelectionContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }

    /// <summary>
    /// Base class for selection behaviors.
    /// Can be used on Selectors or TreeViews.</summary>
    /// <typeparam name="T">Selector or TreeView types</typeparam>
    public abstract class SelectionBehaviorBase<T> : Behavior<T>
        where T : System.Windows.FrameworkElement
    {
        /// <summary>
        /// Gets SelectionContext</summary>
        public ISelectionContext SelectionContext
        {
            get 
            { 
                var selection = (ISelectionContext)AssociatedObject.GetValue(SelectionBehaviors.SelectionContextProperty);
                if (selection == null)
                    selection = AssociatedObject.DataContext.As<ISelectionContext>();
                return selection;
            }
        }

        private static void SelectionContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as SelectionBehaviorBase<T>;
            behavior.ChangeContext(e.OldValue as ISelectionContext);
        }

        #region Overrides
        
        /// <summary>
        /// Static constructor</summary>
        static SelectionBehaviorBase()
        {
            SelectionBehaviors.SelectionContextProperty.OverrideMetadata(typeof(Behavior<T>), new PropertyMetadata(SelectionContextPropertyChanged));
        }

        /// <summary>
        /// Raises behavior Attached event and performs custom actions</summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is Selector)
            {
                (AssociatedObject as Selector).SelectionChanged 
                    += (s, e) => OnAssociatedObjectSelectionChanged(e.AddedItems, e.RemovedItems);
            }
            else if (AssociatedObject is TreeView)
            {
                (AssociatedObject as TreeView).SelectedItemChanged
                    += (s, e) => OnAssociatedObjectSelectionChanged(new object[] { e.NewValue }, new object[] { e.OldValue });
            }
            else
            {
                throw new InvalidOperationException("SelectionBehaviorBase does not support this object type");
            }

            ChangeContext(null);
        }

        #endregion

        #region Virtuals

        /// <summary>
        /// Performs custom actions on SelectionContextChanged events</summary>
        protected virtual void OnSelectionContextChanged()
        {
        }

        /// <summary>
        /// Performs custom actions on SelectionContextSelectionChanging events</summary>
        protected virtual void OnSelectionContextSelectionChanging()
        {
        }

        /// <summary>
        /// Performs custom actions on SelectionContextSelectionChanged events</summary>
        protected virtual void OnSelectionContextSelectionChanged()
        {
        }

        /// <summary>
        /// Performs custom actions on AssociatedObjectSelectionChanged events</summary>
        protected virtual void OnAssociatedObjectSelectionChanged(IList addeditems, IList removedItems)
        {
        }

        #endregion

        private void ChangeContext(ISelectionContext oldSelectionContext)
        {
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

        void SelectionContext_SelectionChanged(object sender, EventArgs e)
        {
            OnSelectionContextSelectionChanged();
        }

        void SelectionContext_SelectionChanging(object sender, EventArgs e)
        {
            OnSelectionContextSelectionChanging();
        }

    }

    /// <summary>
    /// Behavior to bind the selection of items in a selector to an ISelectionContext.
    /// Note that binding is only one way, i.e., selection in the selector is mirrored
    /// into the ISelectionContext, but not the other way.</summary>
    public class ToSourceSelectionBehavior : SelectionBehaviorBase<Selector>
    {
        #region Overrides

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
    public class TwoWaySelectionBehavior : SelectionBehaviorBase<Selector>
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

            m_associatedItemsControl = AssociatedObject as ItemsControl;
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
                        if (removedItems.Count > 0)
                        {
                            var converted = ConvertToSelectionContext(removedItems);
                            SelectionContext.RemoveRange(converted);
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
            if (m_associatedItemsControl != null)
            {
                if (!m_associatedItemsControl.Items.Contains(item))
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
            foreach (var item in m_associatedObjectSelection)
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

            // Add any new items in the ISelectionContext to the selection 
            // in the AssociatedObject
            foreach (var item in newSelection)
            {
                object converted = ConvertFromSelectionContextAndCheckExistence(item);
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
            foreach (var item in newSelection)
            {
                object converted = ConvertFromSelectionContextAndCheckExistence(item);
                if (converted != null)
                {
                    AssociatedObject.SelectedItem = converted;
                    break;
                }
            }
        }

        private object ConvertFromSelectionContextAndCheckExistence(object item)
        {
            object result = ConvertFromSelectionContext(item);

            // Check that this item exists in the ItemsSource of the AssociatedObject
            if (result != null && m_associatedItemsControl != null)
            {
                if (!m_associatedItemsControl.Items.Contains(item))
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
        private ItemsControl m_associatedItemsControl;

        #endregion
    }
}
