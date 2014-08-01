//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Input;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// This class facilitates associating a key binding in XAML markup to a command
    /// defined in a View Model by exposing a Command dependency property.
    /// The class derives from Freezable to work around a limitation in WPF when data-binding from XAML.</summary>
    public class CommandReference : Freezable, ICommand, IDisposable
    {
        /// <summary>
        /// Constructor</summary>
        public CommandReference()
        {
        }

        public static readonly DependencyProperty CommandProperty = 
            DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandReference), new PropertyMetadata(OnCommandChanged));

        /// <summary>
        /// Gets or sets the effective value of the Command DependencyProperty</summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        #region ICommand Members

        /// <summary>
        /// Indicates whether the command can execute in its current state</summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, 
        /// this object can be set to null</param>
        /// <returns>True iff this command can be executed</returns>
        public bool CanExecute(object parameter)
        {
            return Command != null && Command.CanExecute(parameter);
        }

        /// <summary>
        /// Executes the command</summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, 
        /// this object can be set to null</param>
        public void Execute(object parameter)
        {
            Command.Execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Raises the CanExecuteChanged event</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private void RaiseCanExecuteChanged(object sender, EventArgs e)
        {
            //CanExecuteChanged.Raise(sender, e);
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var commandReference = d as CommandReference;
            var oldCommand = e.OldValue as ICommand;
            var newCommand = e.NewValue as ICommand;

            if (oldCommand != null)
            {
                oldCommand.CanExecuteChanged -= (s, args) => commandReference.RaiseCanExecuteChanged(s, args);
            }
            if (newCommand != null)
            {
                newCommand.CanExecuteChanged += (s, args) => commandReference.RaiseCanExecuteChanged(s, args);
            }
        }

        #endregion

        #region Freezable

        /// <summary>
        /// Creates a new instance of the System.Windows.Freezable derived class</summary>
        /// <returns>New instance of the System.Windows.Freezable derived class</returns>
        protected override Freezable CreateInstanceCore()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources</summary>
        public void Dispose()
        {
            //test code
        }

        #endregion
    }
}
