//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private IToolBar[] m_toolBars;
        private bool m_toolBarsRequireRefresh;

        /// <summary>
        /// Get items for a given toolbar
        /// </summary>
        /// <param name="toolBar"></param>
        /// <returns></returns>
        public IEnumerable<IToolBarItem> GetToolBarItems(IToolBar toolBar)
        {
            if (toolBar == null || toolBar.Tag == null)
                return EmptyEnumerable<IToolBarItem>.Instance;
            var items = m_toolBarItems.Where(x => x.IsVisible
                && CommandComparer.TagsEqual(x.ToolBarTag, toolBar.Tag)).ToList();
            items.Sort(ToolBarItemComparer.Instance);
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
        private IToolBarItem[] m_toolBarItems;

        private class ToolBarComparer : IComparer<IToolBar>, IComparer
        {
            public static ToolBarComparer Instance
            {
                get
                {
                    if(s_instance == null)
                        s_instance = new ToolBarComparer();
                    return s_instance;
                }
            }
            private static ToolBarComparer s_instance;

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
        }

        private class ToolBarItemComparer : IComparer<IToolBarItem>
        {
            public static ToolBarItemComparer Instance
            {
                get
                {
                    if (s_instance == null)
                        s_instance = new ToolBarItemComparer();
                    return s_instance;
                }
            }
            private static ToolBarItemComparer s_instance;

            #region IComparer Members

            public int Compare(IToolBarItem x, IToolBarItem y)
            {
                if (x != null && y != null)
                {
                    return CommandComparer.CompareTags(x.Tag, y.Tag);
                }
                return 0;
            }

            #endregion
        }
    }
}
