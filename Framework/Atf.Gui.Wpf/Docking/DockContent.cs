//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// Class representing content to be docked in the docking framework</summary>
    public class DockContent : IDockContent
    {
        internal ContentSettings Settings { get; set; }

        /// <summary>
        /// Constructor</summary>
        /// <param name="content">The actual content being docked</param>
        /// <param name="uid">Unique ID for the item</param>
        public DockContent(object content, string uid)
        {
            Content = content;
            UID = uid;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="content">The actual content being docked</param>
        /// <param name="uid">Unique ID for the item</param>
        /// <param name="header">Header information to display in the docked tab or title bar</param>
        public DockContent(object content, string uid, string header)
            : this(content, uid)
        {
            Header = header;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="content">The actual content being docked</param>
        /// <param name="uid">Unique ID for the item</param>
        /// <param name="header">Header information to display in the docked tab or title bar</param>
        /// <param name="icon">Icon to display</param>
        public DockContent(object content, string uid, string header, object icon)
            : this(content, uid, header)
        {
            Icon = icon;
        }

        /// <summary>
        /// Fired when a property changes</summary>
        /// <param name="propertyName">The name of the property that changed</param>
        public void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Event to fire when a property is changed</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IDockContent Members

        /// <summary>
        /// Gets and sets the header information to display in the docked tab or title bar</summary>
        public string Header
        {
            get { return m_header; }
            set
            {
                if (m_header != value)
                {
                    m_header = (value != null) ? value : String.Empty;
                    NotifyPropertyChanged("Header");
                }
            }
        }

        /// <summary>
        /// Gets and sets the icon to display</summary>
        public object Icon
        {
            get { return m_icon; }
            set
            {
                if (m_icon != value)
                {
                    m_icon = value;
                    NotifyPropertyChanged("Icon");
                }
            }
        }

        /// <summary>
        /// Gets the unique ID for the item</summary>
        public string UID { get; private set; }

        /// <summary>
        /// Gets whether the item is visible</summary>
        public bool IsVisible
        {
            get { return m_isVisible; }
            internal set
            {
                if (m_isVisible != value)
                {
                    m_isVisible = value;
                    NotifyPropertyChanged("IsVisible");
                    if (IsVisibleChanged != null)
                    {
                        IsVisibleChanged(this, new BooleanArgs(m_isVisible));
                    }

                }
            }
        }

        /// <summary>
        /// Gets whether the item currently has focus</summary>
        public bool IsFocused 
        {
            get { return m_isFocused; }
            internal set
            {
                if (m_isFocused != value)
                {
                    m_isFocused = value;
                    NotifyPropertyChanged("IsFocused");
                    if (IsFocusedChanged != null)
                    {
                        IsFocusedChanged(this, new BooleanArgs(m_isFocused));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the actual content to be displayed in the docked panel</summary>
        public object Content
        {
            get;
            private set;
        }

        /// <summary>
        /// Event to fire when the IsVisible property changes</summary>
        public event EventHandler<BooleanArgs> IsVisibleChanged;

        /// <summary>
        /// Event to fire when the IsFocused property changes</summary>
        public event EventHandler<BooleanArgs> IsFocusedChanged;

        /// <summary>
        /// Event fired when the docked panel is being closed</summary>
        public event ContentClosedEvent Closing;

        /// <summary>
        /// Event called when the docked panel is being closed</summary>
        /// <param name="sender">The parent control</param>
        /// <param name="args">Event args indicating which content to close</param>
        public void OnClose(object sender, ContentClosedEventArgs args)
        {
            if (Closing != null)
            {
                Closing(this, args);
            }
        }

        #endregion

        private string m_header = String.Empty;
        private object m_icon;
        private bool m_isVisible;
        private bool m_isFocused;
    }
}
