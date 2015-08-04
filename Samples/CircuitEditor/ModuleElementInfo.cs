//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Controls.Adaptable.Graphs;

namespace CircuitEditorSample
{
    /// <summary>
    /// These are additional options per circuit element, that are returned by the
    /// ICircuitElement.ElementInfo property. The Circuit Editor sample app's Module
    /// class implements ICircuitElement.</summary>
    public class ModuleElementInfo : CircuitElementInfo
    {
        /// <summary>
        /// Gets or sets whether this circuit element should be displayed in an enabled state.
        /// The default is 'true'.</summary>
        public bool Enabled
        {
            get { return m_enabled; }
            set
            {
                if (m_enabled != value)
                {
                    m_enabled = value;
                    OnPropertyChanged("Enabled");
                }
            }
        }

        private bool m_enabled = true;
    }
}
