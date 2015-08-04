//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// View model for an application's tool bars</summary>
    [ExportViewModel(Contracts.ToolBarViewModel)]
    public class ToolBarViewModel : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Get or set array of toolbar models</summary>
        [ImportMany(AllowRecomposition = true)]
        public IToolBar[] ToolBars
        {
            get
            {
                if (m_toolBars != null && m_toolBars.Length > 0 && m_toolBarsRequireRefresh)
                {
                    m_toolBarsRequireRefresh = false;

                    // Must access ListCollectionView on UI thread
                    Application.Current.Dispatcher.InvokeIfRequired(delegate
                    {
                        var cvs = (ListCollectionView)CollectionViewSource.GetDefaultView(m_toolBars);
                        cvs.CustomSort = ToolBarComparer.Instance;
                    });
                }
                return m_toolBars;
            }
            set
            {
                m_toolBars = value;
                m_toolBarsRequireRefresh = true;
                OnPropertyChanged(new PropertyChangedEventArgs("ToolBars"));
            }
        }

        /// <summary>
        /// Enumerate items for a given toolbar</summary>
        /// <param name="toolBar">Toolbar</param>
        /// <returns>Enumeration of IToolBarItem in toolbar</returns>
        public IEnumerable<IToolBarItem> GetToolBarItems(IToolBar toolBar)
        {
            if (toolBar == null || toolBar.Tag == null)
                return EmptyEnumerable<IToolBarItem>.Instance;
            var items = m_toolBarItems.Where(x => x.IsVisible
                && CommandComparer.TagsEqual(x.ToolBarTag, toolBar.Tag)).ToList();
            items.Sort(ToolBarItemComparer.Instance);

            // Maintain separator groups
            if (items.Any())
            {
                var groupedItems = new List<IToolBarItem>();

                var prevItem = items[0] as CommandItem;
                object prevTag = prevItem != null ? prevItem.GroupTag : null;

                groupedItems.Add(items[0]);

                for (int i = 1; i < items.Count; i++)
                {
                    var thisItem = items[i] as CommandItem;
                    if (thisItem == null)
                    {
                        groupedItems.Add(items[i]);
                        continue;
                    }

                    object groupTag = thisItem.GroupTag;

                    // add a separator if the new command is from a different group
                    if (groupTag != null
                        && prevTag != null
                        && !CommandComparer.TagsEqual(groupTag, prevTag))
                    {
                        groupedItems.Add(new ToolBarSeparator());
                    }

                    groupedItems.Add(thisItem);
                    prevTag = groupTag;
                }

                items = groupedItems;
            }

            return items;
        }

        [ImportMany(AllowRecomposition = true)]
        private IToolBarItem[] ToolBarItems
        {
            get { return m_toolBarItems; }
            set
            {
                m_toolBarItems = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ToolBars"));
            }
        }

        private class ToolBarComparer : IComparer<IToolBar>, IComparer
        {
            public static ToolBarComparer Instance
            {
                get { return s_instance ?? (s_instance = new ToolBarComparer()); }
            }

            #region IComparer<IToolBar> Members

            public int Compare(IToolBar x, IToolBar y)
            {
                if (x != null && y != null)
                {
                    return CommandComparer.CompareTags(x.Tag, y.Tag);
                }
                return 0;
            }

            #endregion

            #region IComparer Members

            public int Compare(object x, object y)
            {
                var tX = x as IToolBar;
                var tY = y as IToolBar;
                if (tX != null && tY != null)
                    return Compare(tX, tY);
                return 0;
            }

            #endregion

            private static ToolBarComparer s_instance;
        }

        private class ToolBarItemComparer : IComparer<IToolBarItem>
        {
            public static ToolBarItemComparer Instance
            {
                get { return s_instance ?? (s_instance = new ToolBarItemComparer()); }
            }

            #region IComparer Members

            public int Compare(IToolBarItem x, IToolBarItem y)
            {
                if (x != null && y != null)
                {
                    var xCommandItem = x as ICommandItem;
                    var yCommandItem = y as ICommandItem;
                    if (xCommandItem != null && yCommandItem != null)
                        return CommandComparer.CompareCommands(xCommandItem, yCommandItem);

                    return CommandComparer.CompareTags(x.Tag, y.Tag);
                }
                return 0;
            }

            #endregion

            private static ToolBarItemComparer s_instance;
        }

        private IToolBar[] m_toolBars;
        private bool m_toolBarsRequireRefresh;
        private IToolBarItem[] m_toolBarItems;
    }

    /// <summary>
    /// View model for a menu separator.</summary>
    public class ToolBarSeparator : NotifyPropertyChangedBase, IToolBarItem
    {
        /// <summary>
        /// Get tool bar separator tag object.</summary>
        public object Tag { get { return null; } }
        /// <summary>
        /// Get tool bar separator's tool bar tag object.</summary>
        public object ToolBarTag { get { return null; } }
        /// <summary>
        /// Get whether tool bar separator visible.</summary>
        public bool IsVisible { get { return true; } }
    }
}
