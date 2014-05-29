//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;
using System.ComponentModel;

namespace Sce.Atf.Wpf.Models
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
        /// Gets binding adapter object</summary>
        public object As
        {
            get
            {
                if (m_adapterObject == null)
                {
                    m_adapterObject = new BindingAdapterObject(this);
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
        /// Raises PropertyChanged event</summary>
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
