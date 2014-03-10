//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Packages event information for command arguments</summary>
    public class EventToCommandArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="commandRan">Command</param>
        /// <param name="commandParameter">Command parameter</param>
        /// <param name="eventArgs">EventArgs of event</param>
        public EventToCommandArgs(Object sender, ICommand commandRan, Object commandParameter, EventArgs eventArgs)
        {
            Sender = sender;
            CommandRan = commandRan;
            CommandParameter = commandParameter;
            EventArgs = eventArgs;
        }
        
        /// <summary>
        /// Gets or sets the event sender</summary>
        public Object Sender { get; private set; }
        /// <summary>
        /// Gets or sets the command executed</summary>
        public ICommand CommandRan { get; private set; }
        /// <summary>
        /// Gets or sets the command parameter</summary>
        public Object CommandParameter { get; private set; }
        /// <summary>
        /// Gets or sets the EventArgs of the related event</summary>
        public EventArgs EventArgs { get; private set; }
    }
}
