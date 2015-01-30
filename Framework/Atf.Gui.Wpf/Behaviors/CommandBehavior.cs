//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Attached command behavior to hook routed events to ICommands in the view model.
    /// Warning: uses reflection to hook events, so could be slow.</summary>
    public static class CommandBehavior
    {
        #region Command Attached Property

        /// <summary>
        /// The actual ICommand to run property</summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command",
                typeof(ICommand),
                typeof(CommandBehavior),
                new FrameworkPropertyMetadata((ICommand)null));

        /// <summary>
        /// Gets the CommandProperty (ICommand to run) property</summary>
        /// <param name="d">Dependency object to obtain property for</param>
        /// <returns>CommandProperty (ICommand to run) property</returns>
        public static ICommand GetCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(CommandProperty);
        }

        /// <summary>
        /// Sets the CommandProperty (ICommand to run) property</summary>
        /// <param name="d">Dependency object to set property for</param>
        /// <param name="value">CommandProperty (ICommand to run) property to set</param>
        public static void SetCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(CommandProperty, value);
        }

        #endregion

        #region CommandParameter Attached Property

        /// <summary>
        /// The ICommand parameter property</summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), 
            typeof(CommandBehavior));

        /// <summary>
        /// Gets the ICommand parameter property</summary>
        /// <param name="obj">Dependency object to obtain property for</param>
        /// <returns>ICommand parameter property</returns>
        public static object GetCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(CommandParameterProperty);
        }

        /// <summary>
        /// Sets the ICommand parameter property</summary>
        /// <param name="obj">Dependency object to set property for</param>
        /// <param name="value">ICommand parameter property to set</param>
        public static void SetCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(CommandParameterProperty, value);
        }

        #endregion

        #region Action Attached Property

        /// <summary>
        /// Action attached property. This can be set instead of CommandProperty in order
        /// to hook the routed event to an action.</summary>
        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.RegisterAttached("Action", typeof(Action), 
            typeof(CommandBehavior));

        /// <summary>
        /// Gets the Action attached property</summary>
        /// <param name="obj">Dependency object to obtain property for</param>
        /// <returns>Action attached property</returns>
        public static Action GetAction(DependencyObject obj)
        {
            return (Action)obj.GetValue(ActionProperty);
        }

        /// <summary>
        /// Sets the Action attached property</summary>
        /// <param name="obj">Dependency object to set property for</param>
        /// <param name="value">Action attached property to set</param>
        public static void SetAction(DependencyObject obj, Action value)
        {
            obj.SetValue(ActionProperty, value);
        }

        #endregion

        #region RoutedEventName Attached Property

        /// <summary>
        /// RoutedEventName property: the event that should actually execute the ICommand</summary>
        public static readonly DependencyProperty RoutedEventNameProperty =
            DependencyProperty.RegisterAttached("RoutedEventName", typeof(String),
            typeof(CommandBehavior),
                new FrameworkPropertyMetadata((String)String.Empty,
                    OnRoutedEventNameChanged));

        /// <summary>
        /// Gets the RoutedEventName property</summary>
        /// <param name="d">Dependency object to obtain property for</param>
        /// <returns>RoutedEventName property</returns>
        public static String GetRoutedEventName(DependencyObject d)
        {
            return (String)d.GetValue(RoutedEventNameProperty);
        }

        /// <summary>
        /// Sets the RoutedEventName property</summary>
        /// <param name="d">Dependency object to set property for</param>
        /// <param name="value">RoutedEventName property to set</param>
        public static void SetRoutedEventName(DependencyObject d, String value)
        {
            d.SetValue(RoutedEventNameProperty, value);
        }

        /// <summary>
        /// Hooks up a Dynamically created EventHandler (by using the 
        /// <see cref="EventHooker">EventHooker</see> class) that, when run,
        /// runs the associated ICommand</summary>
        private static void OnRoutedEventNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            String routedEvent = (String)e.NewValue;

            //If the RoutedEvent string is not null, create a new
            //dynamically created EventHandler that when run will execute
            //the actual bound ICommand instance (usually in the ViewModel)
            if (!String.IsNullOrEmpty(routedEvent))
            {
                EventHooker eventHooker = new EventHooker();
                eventHooker.ObjectWithAttachedCommand = d;

                EventInfo eventInfo = d.GetType().GetEvent(routedEvent,
                    BindingFlags.Public | BindingFlags.Instance);

                //Hook up Dynamically created event handler
                if (eventInfo != null)
                {
                    eventInfo.AddEventHandler(d,
                        eventHooker.GetNewEventHandlerToRunCommand(eventInfo));
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Contains the event that is hooked into the source RoutedEvent
    /// that was specified to run the ICommand</summary>
    internal sealed class EventHooker
    {
        #region Public Methods/Properties
        /// <summary>
        /// Gets or sets the DependencyObject that holds a binding to the actual
        /// ICommand to execute</summary>
        public DependencyObject ObjectWithAttachedCommand { get; set; }

        /// <summary>
        /// Creates a Dynamic EventHandler that runs the ICommand
        /// when the user specified RoutedEvent fires</summary>
        /// <param name="eventInfo">The specified RoutedEvent EventInfo</param>
        /// <returns>A delegate that points to a new EventHandler
        /// that runs the ICommand</returns>
        public Delegate GetNewEventHandlerToRunCommand(EventInfo eventInfo)
        {
            Delegate del = null;

            if (eventInfo == null)
                throw new ArgumentNullException("eventInfo");

            if (eventInfo.EventHandlerType == null)
                throw new ArgumentException("EventHandlerType is null");

            if (del == null)
                del = Delegate.CreateDelegate(eventInfo.EventHandlerType, this,
                      GetType().GetMethod("OnEventRaised",
                        BindingFlags.NonPublic |
                        BindingFlags.Instance));

            return del;
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Runs the ICommand when the requested RoutedEvent fires</summary>
        private void OnEventRaised(object sender, EventArgs e)
        {
            var dpo = (DependencyObject)sender;
            var command = (ICommand)dpo.GetValue(CommandBehavior.CommandProperty);

            if (command != null)
            {
                object commandParameter = dpo.GetValue(CommandBehavior.CommandParameterProperty);
                if (command.CanExecute(commandParameter))
                {
                    command.Execute(commandParameter);
                }
            }
            else
            {
                var action = (Action)dpo.GetValue(CommandBehavior.ActionProperty);
                if (action != null)
                {
                    action();
                }
            }
        }
        #endregion
    }
}
