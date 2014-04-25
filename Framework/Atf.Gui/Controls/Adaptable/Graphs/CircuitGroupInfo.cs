//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Contains options for specifying the behavior or appearance of a ICircuitGroup.</summary>
    public class CircuitGroupInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor</summary>
        public CircuitGroupInfo()
        {
            m_showExpandedGroupPins = CircuitDefaultStyle.ShowExpandedGroupPins;
        }

        #region INotifyPropertyChanged members
        
        /// <summary>
        /// Event that is raised if any of the properties on this object are changed</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        #endregion

        /// <summary>
        /// Gets or sets the minimum size for the circuit group.</summary>
        public Size MinimumSize
        {
            get { return m_minSize; }
            set { PropertyChanged.NotifyIfChanged(ref m_minSize, value, () => MinimumSize); }
        }


        /// <summary>
        /// Offset to be added to draw all sub-elements when the group is expanded inline
        /// </summary>
        public Point Offset
        {
            get { return m_offset; }
            set { PropertyChanged.NotifyIfChanged(ref m_offset, value, () => Offset); }
        }

        /// <summary>
        /// Gets or sets whether to show the group pins when the group is expanded</summary>
        public bool ShowExpandedGroupPins
        {
            get { return m_showExpandedGroupPins; }
            set { PropertyChanged.NotifyIfChanged(ref m_showExpandedGroupPins, value, () => ShowExpandedGroupPins); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the group element is under editing.</summary>
        public bool IsEditing
        {
            get { return m_editing; }
            set { PropertyChanged.NotifyIfChanged(ref m_editing, value, () => IsEditing); }
        }

        /// <summary>
        /// Gets or sets the hidden input pins of the group element.</summary>
        public IEnumerable<ICircuitPin> HiddenInputPins
        {
            get { return m_hiddenInputPins; }
            set { PropertyChanged.NotifyIfChanged(ref m_hiddenInputPins, value, () => HiddenInputPins); }
        }

        /// <summary>
        /// Gets or sets the hidden output pins of the group element.</summary>
        public IEnumerable<ICircuitPin> HiddenOutputPins
        {
            get { return m_hiddenOutputPins; }
            set { PropertyChanged.NotifyIfChanged(ref m_hiddenOutputPins, value, () => HiddenOutputPins); }
        }

        /// <summary>
        /// Gets or sets picking priority</summary>
        public int PickingPriority
        {
            get { return m_pickingPriority; }
            set { m_pickingPriority = value; }
        }

        private Size m_minSize;
        private Point m_offset;
        private bool m_showExpandedGroupPins;
        private bool m_editing;
        private int m_pickingPriority;
        private IEnumerable<ICircuitPin> m_hiddenInputPins = EmptyEnumerable<ICircuitPin>.Instance;
        private IEnumerable<ICircuitPin> m_hiddenOutputPins = EmptyEnumerable<ICircuitPin>.Instance;

    }
}
