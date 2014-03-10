//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Event arguments for property error events</summary>
    public class PropertyErrorEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyErrorEventArgs"/> class</summary>
        /// <param name="owner">Property owner</param>
        /// <param name="descriptor">Property descriptor</param>
        /// <param name="exception">Property editing exception that caused the event</param>
        public PropertyErrorEventArgs(
            object owner,
            PropertyDescriptor descriptor,
            Exception exception)
        {
            Owner = owner;
            Descriptor = descriptor;
            Exception = exception;
        }

        /// <summary>
        /// Property owner</summary>
        public object Owner;
        /// <summary>
        /// Property descriptor</summary>
        public PropertyDescriptor Descriptor;
        /// <summary>
        /// Property editing exception that caused the event</summary>
        public readonly Exception Exception;
    }
}
