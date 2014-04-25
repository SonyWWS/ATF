//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Base class for all view models. Implements INotifyPropertyChanged for binding.  
    /// Includes debug checks on property names using reflection (slow!).</summary>
    [Serializable]
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        /// <summary>
        /// Event that is raised after a property changed</summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { m_propertyChanged += value; }
            remove { m_propertyChanged -= value; }
        }

        [NonSerialized]
        private PropertyChangedEventHandler m_propertyChanged;

        #endregion

        /// <summary>
        /// Raises PropertyChanged event and checks property name,
        /// raising an exception if name is invalid</summary>
        /// <param name="propertyName">Property name</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected void RaisePropertyChanged(string propertyName)
        {
            CheckPropertyName(propertyName);
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises PropertyChanged event and performs custom actions</summary>
        /// <param name="e">PropertyChangedEventArgs containing event data</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var h = m_propertyChanged;
            if (h != null) 
                h(this, e);
        }

        /// <summary>
        /// Checks property name and raises an exception if name is invalid</summary>
        /// <param name="propertyName">Property name</param>
        /// <exception cref="InvalidOperationException">Property with property name does not exist</exception>
        [Conditional("DEBUG")]
        private void CheckPropertyName(string propertyName)
        {
            var propertyDescriptor = TypeDescriptor.GetProperties(this)[propertyName];
            if (propertyDescriptor == null)
            {
                throw new InvalidOperationException(string.Format(null,
                    "The property with the propertyName '{0}' doesn't exist.", propertyName));
            }
        }
    }
}
