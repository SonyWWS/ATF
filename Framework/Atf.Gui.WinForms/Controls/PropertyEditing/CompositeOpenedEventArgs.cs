//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Event args for CompositeOpened event</summary>
    public class CompositeOpenedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="part">The part of the composite that was opened</param>
        /// <param name="descriptors">Property descriptors for the part that was opened</param>
        public CompositeOpenedEventArgs(object part, PropertyDescriptor[] descriptors)
        {
            Part = part;
            Descriptors = descriptors;
        }

        /// <summary>
        /// The part of the composite that was opened</summary>
        public readonly object Part;

        /// <summary>
        /// Property descriptors for the part that was opened</summary>
        public readonly PropertyDescriptor[] Descriptors;
    }
}
