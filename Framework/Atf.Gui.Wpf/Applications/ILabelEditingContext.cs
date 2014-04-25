//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for contexts which support editing item labels</summary>
    public interface ILabelEditingContext
    {
        /// <summary>
        /// Checks whether the client can edit an item's label</summary>
        /// <param name="item">Item whose label to be edited</param>
        /// <returns>True iff client can edit the label</returns>
        bool CanEditLabel(object item);

        /// <summary>
        /// Edits an item's label</summary>
        /// <param name="item">Item whose label to be edited</param>
        void EditLabel(object item);

        /// <summary>
        /// Gets an item's label</summary>
        /// <param name="item">Item whose label is obtained</param>
        /// <returns>Item's label</returns>
        string GetLabel(object item);

        /// <summary>
        /// Sets an item's label</summary>
        /// <param name="item">Item whose label is set</param>
        /// <param name="value">Item's label</param>
        void SetLabel(object item, string value);

        /// <summary>
        /// Event that is raised when a label edit operation begins</summary>
        event EventHandler<BeginLabelEditEventArgs> BeginLabelEdit;
    }

    /// <summary>
    /// Event information for the BeginLabelEdit event</summary>
    public class BeginLabelEditEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="item">Item whose label has begun to be edited</param>
        public BeginLabelEditEventArgs(object item)
        {
            Item = item;
        }

        /// <summary>
        /// Item whose label has begun to be edited</summary>
        public readonly object Item;
    }

}
