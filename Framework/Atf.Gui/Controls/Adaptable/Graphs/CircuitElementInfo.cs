//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using Sce.Atf.Controls.PropertyEditing;

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
            set { PropertyChanged.NotifyIfChanged(ref m_showUnconnectedPins, value, () => ShowUnconnectedPins); }
        }

        #region INotifyPropertyChanged members

        /// <summary>
        /// Event that is raised if any of the properties on this object are changed</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private bool m_showUnconnectedPins = true;
    }
}
