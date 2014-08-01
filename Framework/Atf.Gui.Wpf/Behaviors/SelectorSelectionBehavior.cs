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
    public class SelectorSelectionBehavior : Behavior<Selector>
    {
        #region Overrides

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += new SelectionChangedEventHandler(AssociatedObject_SelectionChanged);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= new SelectionChangedEventHandler(AssociatedObject_SelectionChanged);
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


        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        #endregion

        #region Command

        public static readonly DependencyProperty CommandProperty = 
            DependencyProperty.Register(
                "Command", typeof(ICommand), typeof(SelectorSelectionBehavior),
                new PropertyMetadata(null,
                    (s, e) => OnCommandChanged(s as SelectorSelectionBehavior, e)));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        #endregion
    }

    public class SelectorLastHitAwareBehavior : Behavior<Selector>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += SetLastSelected;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= SetLastSelected;
        }

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
