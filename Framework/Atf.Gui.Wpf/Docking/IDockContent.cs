//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// Event arguments for Boolean value</summary>
    public class BooleanArgs : EventArgs
    {
        /// <summary>
        /// Get value</summary>
        public bool Value { get; private set; }

        /// <summary>
        /// Constructor with value</summary>
        /// <param name="value">Value to set</param>
        public BooleanArgs(bool value)
        {
            Value = value;
        }
    }

    ///<summary>
    /// IDockable content interface, every content that can be docked must implement this</summary>
    public interface IDockContent : INotifyPropertyChanged
    {
        /// <summary> 
        /// Get header text </summary>
        String Header { get; set; }

        /// <summary> 
        /// Get header icon</summary>
        object Icon { get; set; }

        /// <summary> 
        ///Get unique ID</summary>
        String UID { get; }

        ///<summary> 
        /// Get the visibility state of control, true if visible, false if hidden</summary>
        bool IsVisible { get; }

        /// <summary> 
        /// Get the focus state of the control</summary>
        bool IsFocused { get; }

        /// <summary>
        /// Event triggered when the IsVisible property changes</summary>
        event EventHandler<BooleanArgs> IsVisibleChanged;

        /// <summary>
        /// Event triggered when the IsFocused property changes</summary>
        event EventHandler<BooleanArgs> IsFocusedChanged;

        /// <summary>
        /// Event triggered when the docked panel is being closed</summary>
        event ContentClosedEvent Closing;

        /// <summary>
        /// Get the actual content being docked</summary>
        object Content { get; }
    }

    /// <summary>
    /// Content closed event</summary>
    /// <param name="sender">Sender of this message</param>
    /// <param name="args">Event arguments</param>
    public delegate void ContentClosedEvent(object sender, ContentClosedEventArgs args);

    /// <summary>
    /// Content closed event arguments class</summary>
    public class ContentClosedEventArgs : EventArgs
    {
        /// <summary>
        /// Content that should be closed, current or all</summary>
        public ContentToClose ContentToClose { get; private set; }
        /// <summary>
        /// Constructor with content to close</summary>
        /// <param name="contentToClose">Content to close, current or all</param>
        public ContentClosedEventArgs(ContentToClose contentToClose)
        {
            ContentToClose = contentToClose;
        }
    }

    /// <summary>
    /// Enum for content to close</summary>
    public enum ContentToClose
    {
        Current,
        All
    }
}
