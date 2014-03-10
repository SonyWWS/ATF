//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    internal class MenuItemBase : NotifyPropertyChangedBase, IMenuItem
    {
        public MenuItemBase(object menuTag, object groupTag, string text, string description)
        {
            m_menuTag = menuTag;
            m_groupTag = groupTag;
            m_text = text;
            m_description = description;
        }

        #region IMenuItem Members

        public string Text
        {
            get { return m_text; }
            set { m_text = value; OnPropertyChanged(s_textArgs); }
        }

        public string Description
        {
            get { return m_description; }
            set { m_description = value; OnPropertyChanged(s_descriptionArgs); }
        }

        public object MenuTag { get { return m_menuTag; } }

        public object GroupTag { get { return m_groupTag; } }

        #endregion

        private string m_text;
        private string m_description;
        private readonly object m_menuTag;
        private readonly object m_groupTag;

        // Property changed event args
        private static readonly PropertyChangedEventArgs s_textArgs
            = ObservableUtil.CreateArgs<MenuItemBase>(x => x.Text);
        private static readonly PropertyChangedEventArgs s_descriptionArgs
            = ObservableUtil.CreateArgs<MenuItemBase>(x => x.Description);
    }
}
