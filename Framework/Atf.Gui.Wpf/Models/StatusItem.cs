//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Base class for status bar items</summary>
    public class StatusItem : NotifyPropertyChangedBase, IStatusItem
    {
        /// <summary>
        /// Gets or sets whether item docked on the left in the status bar</summary>
        public bool IsLeftDock
        {
            get { return m_isLeftDock; }
            set
            {
                m_isLeftDock = value;
                OnPropertyChanged(s_isLeftDockArgs);
            }
        }
        private bool m_isLeftDock;

        /// <summary>
        /// Gets or sets status item's tool tip string</summary>
        public string ToolTip
        {
            get { return m_toolTip; }
            set
            {
                m_toolTip = value;
                OnPropertyChanged(s_toolTipArgs);
            }
        }
        private string m_toolTip;

        private static readonly PropertyChangedEventArgs s_isLeftDockArgs
            = ObservableUtil.CreateArgs<StatusItem>(x => x.IsLeftDock);
        private static readonly PropertyChangedEventArgs s_toolTipArgs
            = ObservableUtil.CreateArgs<StatusItem>(x => x.ToolTip);
    }
}
