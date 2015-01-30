//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Behavior for selection in a Selector</summary>
    public class SelectorSelectionBehavior : Behavior<Selector>
    {
        #region Overrides

        /// <summary>
        /// Handle Attached event</summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        /// <summary>
        /// Handle Detaching event</summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        #endregion

        #region Private Methods

        void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Invoke(sender, e);
        }

        void Invoke(object clickedItem, SelectionChangedEventArgs parameter)
        {
            ICommand command = Command;
            if (command != null && command.CanExecute(CommandParameter))
            {
                command.Execute(new EventToCommandArgs(clickedItem, command, CommandParameter, (EventArgs)parameter));
            }
        }

        static void OnCommandChanged(SelectorSelectionBehavior thisBehaviour, DependencyPropertyChangedEventArgs e)
        {
            if (thisBehaviour == null)
            {
                return;
            }

            ICommand oldCommand = (ICommand)e.OldValue;
            if (oldCommand != null)
            {
                oldCommand.CanExecuteChanged -= thisBehaviour.OnCommandCanExecuteChanged;
            }

            ICommand newCommand = (ICommand)e.NewValue;
            if (newCommand != null)
            {
                newCommand.CanExecuteChanged += thisBehaviour.OnCommandCanExecuteChanged;
            }
        }

        void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
        }

        #endregion

        #region CommandParameter

        /// <summary>
        /// Command parameter dependency property</summary>
        public static readonly DependencyProperty CommandParameterProperty = 
            DependencyProperty.Register(
                "CommandParameter", typeof(object), typeof(SelectorSelectionBehavior),
                new PropertyMetadata(null,
                (s, e) =>
                {
                    SelectorSelectionBehavior sender = s as SelectorSelectionBehavior;
                    if (sender == null)
                    {
                        return;
                    }

                    if (sender.AssociatedObject == null)
                    {
                        return;
                    }
                }));

        /// <summary>
        /// Get or set command parameter dependency property</summary>
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        #endregion

        #region Command

        /// <summary>
        /// Command dependency property</summary>
        public static readonly DependencyProperty CommandProperty = 
            DependencyProperty.Register(
                "Command", typeof(ICommand), typeof(SelectorSelectionBehavior),
                new PropertyMetadata(null,
                    (s, e) => OnCommandChanged(s as SelectorSelectionBehavior, e)));

        /// <summary>
        /// Get or set command dependency property</summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        #endregion
    }

    /// <summary>
    /// Behavior for last item hit in Selector</summary>
    public class SelectorLastHitAwareBehavior : Behavior<Selector>
    {
        /// <summary>
        /// Handle Attached event</summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += SetLastSelected;
        }

        /// <summary>
        /// Handle Detaching event</summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= SetLastSelected;
        }

        /// <summary>
        /// Set last hit item in Selector</summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Selection changed event arguments</param>
        void SetLastSelected(object sender, SelectionChangedEventArgs args)
        {
            var ctx = AssociatedObject.DataContext.As<ILastHitAware>();
            if (ctx != null)
            {
                ctx.LastHit = args.AddedItems.Count > 0 ? args.AddedItems[args.AddedItems.Count - 1] : null;
            }
        }
    }
}
