//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// ViewModel for a Target</summary>
    [Serializable]
    public class TargetViewModel : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="target">The ITarget whose information will be displayed</param>
        /// <param name="protocol">The target's protocol</param>
        public TargetViewModel(ITarget target, IProtocol protocol)
        {
            Requires.NotNull(target, "target");
            Requires.NotNull(protocol, "protocol");

            Target = target;
            m_protocol = protocol;
        }

        /// <summary>
        /// Gets the target</summary>
        public ITarget Target { get; private set; }

        /// <summary>
        /// Gets and sets the target's protocol</summary>
        public IProtocol Protocol 
        { 
            get { return m_protocol; }
            set 
            {
                if (Target.ProtocolId != value.Id)
                    throw new ArgumentException("Invalid protocol for this target");

                m_protocol = value; 
            }
        }

        /// <summary>
        /// Gets and sets whether the target is selected</summary>
        public bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                m_isSelected = value;
                OnPropertyChanged(s_isSelectedArgs);
            }
        }

        [NonSerialized]
        private IProtocol m_protocol;
        
        private bool m_isSelected;
        private static readonly PropertyChangedEventArgs s_isSelectedArgs
            = ObservableUtil.CreateArgs<TargetViewModel>(x => x.IsSelected);
    }
}
