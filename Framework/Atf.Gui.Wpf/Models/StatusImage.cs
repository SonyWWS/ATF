//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Image status item in the status bar</summary>
    public class StatusImage : StatusItem
    {
        /// <summary>
        /// Gets or sets image resource</summary>
        public object ImageSourceKey
        {
            get { return m_imageKey; }
            set
            {
                m_imageKey = value;
                OnPropertyChanged(s_imageKeyArgs);
            }
        }
        private object m_imageKey;

        private static readonly PropertyChangedEventArgs s_imageKeyArgs
            = ObservableUtil.CreateArgs<StatusImage>(x => x.ImageSourceKey);
    }
}
