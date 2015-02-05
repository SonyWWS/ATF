//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Models
{
    /// <summary>
    /// View model class that can adapt an adaptee and provide a bindable As property</summary>
    public class AdapterViewModel : Adapter, INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor</summary>
        public AdapterViewModel()
        {
        }

        /// <summary>
        /// Constructor with adaptee</summary>
        /// <param name="adaptee">Object that is adapted</param>
        public AdapterViewModel(object adaptee)
            : base(adaptee)
        {
        }

        /// <summary>
        /// Get adapter object</summary>
        [Browsable(false)]
        public object As
        {
            get
            {
                if (m_adapterObject == null)
                {
                    var node = Adaptee.As<DomNode>();
                    if(node != null)
                    {
                        // Optimisation for Dom Nodes
                        m_adapterObject = new DomBindingAdapterObject(node, true);
                    }
                    else
                    {
                        m_adapterObject = new BindingAdapterObject(this);
                    }
                }
                return m_adapterObject;
            }
        }

        /// <summary>
        /// Raises AdapteeChanged event and performs custom actions after the adapted object has been set</summary>
        /// <param name="oldAdaptee">Previous adaptee reference</param>
        protected override void OnAdapteeChanged(object oldAdaptee)
        {
            base.OnAdapteeChanged(oldAdaptee);
            m_adapterObject = null;
            OnPropertyChanged(new PropertyChangedEventArgs("As"));
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Event that is raised after a property changed</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Raise PropertyChanged event</summary>
        /// <param name="propertyName">Name of changed property</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>Handle PropertyChanged event</summary>
        /// <param name="e">PropertyChangedEventArgs for event</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var h = PropertyChanged;
            if (h != null)
                h(this, e);
        }

        private object m_adapterObject;
    }
}
