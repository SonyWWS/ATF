//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Wpf.Models;
using WinGuiCommon;

namespace WpfApp
{
    /// <summary>
    /// View model for the event viewer pane</summary>
    class EventViewModel : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Constructor</summary>
        public EventViewModel()
        {
        }

        /// <summary>
        /// Gets and sets the event that represents the data context for this view. On set, all properties are updated.</summary>
        public Event Event
        {
            get { return m_event; }
            set 
            { 
                m_event = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Event"));
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Name"));
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Duration"));
            }
        }

        /// <summary>
        /// Gets and sets the event's name</summary>
        public string Name 
        {
            get { return m_event != null ? m_event.Name : string.Empty; }
            set
            {
                if (m_event != null)
                {
                    m_event.Name = value;
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Name"));
                }
            }
        }

        /// <summary>
        /// Gets and sets the event's duration</summary>
        public int Duration
        {
            get { return m_event != null ? m_event.Duration : 0; }
            set
            {
                if (m_event != null)
                {
                    m_event.Duration = value;
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Duration"));
                }
            }
        }

        private Event m_event = null;
    }
}
