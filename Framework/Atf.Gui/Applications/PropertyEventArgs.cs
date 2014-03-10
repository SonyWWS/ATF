//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Arguments for "property value changed" event</summary>
    public class PropertyEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="owner">Property owner</param>
        /// <param name="descriptor">Property descriptor</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        public PropertyEventArgs(
            object owner,
            PropertyDescriptor descriptor,
            object oldValue,
            object newValue)
        {
            Owner = owner;
            Descriptor = descriptor;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Property owner</summary>
        public readonly object Owner;

        /// <summary>
        /// Property descriptor</summary>
        public readonly PropertyDescriptor Descriptor;

        /// <summary>
        /// Old value</summary>
        public readonly object OldValue;

        /// <summary>
        /// New value</summary>
        public readonly object NewValue;
    }
}
