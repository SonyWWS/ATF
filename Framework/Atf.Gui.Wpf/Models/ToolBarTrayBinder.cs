//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// For some reason MS decided not to make ToolBarTray an ItemsControl, so can't do this with XAML
    /// binding.</summary>
    internal class ToolBarTrayBinder
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="toolBarTray">ToolBarTray</param>
        /// <param name="toolbarModels">Collection of menus</param>
        public ToolBarTrayBinder(ToolBarTray toolBarTray, ObservableCollection<IMenu> toolbarModels)
        {
            m_toolBarTray = toolBarTray;

            foreach (var toolbarModel in toolbarModels)
                AddToolBar(toolbarModel);

            toolbarModels.CollectionChanged += CollectionCollectionChanged;
        }

        private void AddToolBar(IMenu toolbarModel)
        {
            var toolBar = new ToolBar();
            toolBar.SetResourceReference(ToolBar.StyleProperty, Resources.ToolBarStyleKey);
            toolBar.DataContext = toolbarModel;
            m_toolBarTray.ToolBars.Add(toolBar);
        }

        private void CollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var toolbarModel in e.NewItems.Cast<Menu>())
                    {
                        AddToolBar(toolbarModel);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var toolbarModel in e.OldItems.Cast<Menu>())
                    {
                        var toolBar = m_toolBarTray.ToolBars.FirstOrDefault<ToolBar>(x => x.DataContext == toolbarModel);
                        m_toolBarTray.ToolBars.Remove(toolBar);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    m_toolBarTray.ToolBars.Clear();
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException();
            }
        }

        private ToolBarTray m_toolBarTray;

    }
}
