//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// A simple selector double click behavior that calls a bound ViewModel command
    /// when the user double clicks the selector</summary>
    public class ItemsControlDoubleClickBehavior : Behavior<ItemsControl>
    {
        #region Overrides

        /// <summary>
        /// Raises behavior Attached event and performs custom actions</summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            
            AssociatedObject.MouseDoubleClick += AssociatedObject_MouseDoubleClick;
            
            EnableDisableElement();
        }

        /// <summary>
        /// Raises behavior Detaching event and performs custom actions</summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseDoubleClick -= AssociatedObject_MouseDoubleClick;
        }

        #endregion

        #region Private Methods

        private void AssociatedObject_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Get the ItemsControl and then get the item, and check there
            //is an actual item, as if we are using a ListView we may have clicked the
            //headers which are not items
            ItemsControl listView = sender as ItemsControl;
            DependencyObject originalSender = e.OriginalSource as DependencyObject;
            if (listView == null || originalSender == null)
                return;

            DependencyObject container =
                ItemsControl.ContainerFromElement
                (sender as ItemsControl, e.OriginalSource as DependencyObject);

            if (container == null ||
                container == DependencyProperty.UnsetValue) return;

            // found a container, now find the item.
            object activatedItem = listView.ItemContainerGenerator.ItemFromContainer(container);

            if (activatedItem != null)
            {
                Invoke(activatedItem, e);
            }
        }

        private static void OnCommandChanged(ItemsControlDoubleClickBehavior thisBehaviour, DependencyPropertyChangedEventArgs e)
        {
            if (thisBehaviour == null)
            {
                return;
            }

            if (e.OldValue != null)
            {
                ((ICommand)e.OldValue).CanExecuteChanged -= thisBehaviour.OnCommandCanExecuteChanged;
            }

            ICommand command = (ICommand)e.NewValue;

            if (command != null)
            {
                command.CanExecuteChanged += thisBehaviour.OnCommandCanExecuteChanged;
            }

            thisBehaviour.EnableDisableElement();
        }

        private bool IsAssociatedElementDisabled()
        {
            return AssociatedObject != null && !AssociatedObject.IsEnabled;
        }

        private void EnableDisableElement()
        {
            if (AssociatedObject == null || Command == null)
            {
                return;
            }

            if (AutoEnable)
            {
                AssociatedObject.IsEnabled = Command.CanExecute(CommandParameter);
            }
        }

        private void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            EnableDisableElement();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Performs the command the event is bound to for the clicked item</summary>
        /// <param name="clickedItem">Clicked item</param>
        /// <param name="parameter">MouseButtonEventArgs of the raised event</param>
        protected void Invoke(object clickedItem, MouseButtonEventArgs parameter)
        {
            if (IsAssociatedElementDisabled())
            {
                return;
            }

            ICommand command = Command;
            if (command != null && command.CanExecute(CommandParameter))
            {
                command.Execute(new EventToCommandArgs(clickedItem, command, CommandParameter, (EventArgs)parameter));
            }
        }

        #endregion

        #region Dependency Properties

        #region AutoEnable

        /// <summary>
        /// Identifies the AutoEnable dependency property</summary>
        public static readonly DependencyProperty AutoEnableProperty = DependencyProperty.Register(
            "AutoEnable", typeof(bool), typeof(ItemsControlDoubleClickBehavior),
            new PropertyMetadata(false,
                (s, e) => OnCommandChanged(s as ItemsControlDoubleClickBehavior, e)));

        /// <summary>
        /// Gets or sets the AutoEnable DependencyProperty</summary>
        public bool AutoEnable
        {
            get { return (bool)GetValue(AutoEnableProperty); }
            set { SetValue(AutoEnableProperty, value); }
        }

        #endregion

        #region CommandParameter

        /// <summary>
        /// Identifies the ICommand parameter dependency property</summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter", typeof(object), typeof(ItemsControlDoubleClickBehavior),
            new PropertyMetadata(null,
                (s, e) =>
                {
                    ItemsControlDoubleClickBehavior sender = s as ItemsControlDoubleClickBehavior;
                    if (sender == null)
                    {
                        return;
                    }

                    if (sender.AssociatedObject == null)
                    {
                        return;
                    }

                    sender.EnableDisableElement();
                }));


        /// <summary>
        /// Gets or sets an object used as a ICommand parameter passed to the ICommand
        /// attached to this trigger. This is a DependencyProperty.</summary>
        public object CommandParameter
        {
            get { return (Object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        #endregion

        #region Command

        /// <summary>
        /// Identifies the ICommand dependency property</summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(ItemsControlDoubleClickBehavior),
            new PropertyMetadata(null,
                (s, e) => OnCommandChanged(s as ItemsControlDoubleClickBehavior, e)));

        /// <summary>
        /// Gets or sets the ICommand that this trigger is bound to. This
        /// is a DependencyProperty.</summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        #endregion

        #endregion
    }
}
