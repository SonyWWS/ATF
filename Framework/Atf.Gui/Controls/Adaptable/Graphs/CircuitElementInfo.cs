//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Additional options to be returned by ICircuitElement</summary>
    public class CircuitElementInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets whether unconnected pins (that means pins that have no wires attached)
        /// should be shown.</summary>
        public bool ShowUnconnectedPins
        {
            get { return m_showUnconnectedPins; }
            set
            {
                if (m_showUnconnectedPins != value)
                {
                    m_showUnconnectedPins = value;
                    OnPropertyChanged("ShowUnconnectedPins");
                }
            }
        }

        /// <summary>
        /// Gets/sets a value that indicates whether the element should be drawn as valid.
        /// If 'false', the D2dCircuitRenderer will use D2dDiagramTheme's ErrorBrush to
        /// draw this circuit element.</summary>
        public bool IsValid 
        {
            get { return m_valid; }
            set
            {
                if (m_valid != value)
                {
                    m_valid = value;
                    OnPropertyChanged("IsValid");
                }
            }       
        }


        #region INotifyPropertyChanged members

        /// <summary>
        /// Event that is raised if any of the properties on this object are changed</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Raises the PropertyChanged event</summary>
        /// <param name="propertyName">Name of the property whose value has changed</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool m_showUnconnectedPins = true;
        private bool m_valid = true;
    }
}
