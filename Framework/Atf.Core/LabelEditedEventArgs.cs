//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Arguments for "label edited" event</summary>
    /// <typeparam name="T">Type of relabeled item</typeparam>
    /// <remarks>No old label field is necessary, since the model has that information</remarks>
    public class LabelEditedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="item">Relabeled item</param>
        /// <param name="label">New label</param>
        public LabelEditedEventArgs(T item, string label)
        {
            Item = item;
            Label = label;
        }

        /// <summary>
        /// Relabeled item</summary>
        public readonly T Item;

        /// <summary>
        /// New label</summary>
        public readonly string Label;
    }
}
