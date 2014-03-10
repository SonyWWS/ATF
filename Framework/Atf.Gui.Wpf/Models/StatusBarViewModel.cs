//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using System.ComponentModel.Composition;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// View model used for status bar</summary>
    [ExportViewModel(Contracts.StatusBarViewModel)]
    public class StatusBarViewModel : NotifyPropertyChangedBase
    {
        #region StatusItems Property

        /// <summary>
        /// Gets or sets items in status bar</summary>
        [ImportMany(AllowRecomposition = true)]
        public IStatusItem[] StatusItems
        {
            get { return m_statusItems; }
            set
            {
                m_statusItems = value;
                OnPropertyChanged(s_statusItemsArgs);
            }
        }

        private IStatusItem[] m_statusItems;
        private static readonly PropertyChangedEventArgs s_statusItemsArgs
            = ObservableUtil.CreateArgs<StatusBarViewModel>(x => x.StatusItems);

        #endregion
    }
}
