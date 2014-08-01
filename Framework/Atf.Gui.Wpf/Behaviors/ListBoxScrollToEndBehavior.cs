//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Behavior to auto scroll newly added list box items into view
    /// </summary>
    public class ListBoxScrollToEndBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            Bind();
            DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListBox))
                .AddValueChanged(AssociatedObject, ItemsSourceChanged);

        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            Unbind();
            DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListBox))
                .RemoveValueChanged(AssociatedObject, ItemsSourceChanged);
        }

        private void ItemsSourceChanged(object sender, EventArgs e)
        {
            Bind();
        }

        private void Unbind()
        {
            if (m_itemsSource != null)
            {
                m_itemsSource.CollectionChanged -= ItemsSource_CollectionChanged;
                m_itemsSource = null;
            }
        }

        private void Bind()
        {
            Unbind();
            m_itemsSource = AssociatedObject.ItemsSource as INotifyCollectionChanged;
            if (m_itemsSource != null)
            {
                m_itemsSource.CollectionChanged += ItemsSource_CollectionChanged;
            }

            if (AssociatedObject.Items.Count > 0)
            {
                SelectAndScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);
            }
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var ic = AssociatedObject.Items;

            object selectedItem = null;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Move: 
                    selectedItem = e.NewItems[e.NewItems.Count - 1]; 
                    break;
                case NotifyCollectionChangedAction.Remove: 
                    if (ic.Count < e.OldStartingIndex)
                    {
                        selectedItem = ic[e.OldStartingIndex - 1];
                    } 
                    else if (ic.Count > 0)
                    {
                        selectedItem = ic[0]; 
                    }
                    break;
                case NotifyCollectionChangedAction.Reset: 
                    if (ic.Count > 0)
                    {
                        selectedItem = ic[0];
                    }
                    break;
            }

            SelectAndScrollIntoView(selectedItem);
        }

        private void SelectAndScrollIntoView(object selectedItem)
        {
            if (selectedItem != null)
            {
                AssociatedObject.Items.MoveCurrentTo(selectedItem);
                AssociatedObject.ScrollIntoView(selectedItem);
            }
        }

        private INotifyCollectionChanged m_itemsSource;
    }
}
