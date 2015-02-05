//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// This class allows delegating the commanding logic to methods passed as parameters,
    /// and enables a View to bind commands to objects that are not part of the element tree</summary>
    public class DelegateCommand : ICommand
    {
        #region Constructors

        /// <summary>
        /// Constructor with execute method</summary>
        /// <param name="executeMethod">Method to execute</param>
        public DelegateCommand(Action executeMethod)
            : this(executeMethod, null, false)
        {
        }

        /// <summary>
        /// Constructor with action and CanExecute function</summary>
        /// <param name="executeMethod">Method to execute</param>
        /// <param name="canExecuteMethod">CanExecute function</param>
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
            : this(executeMethod, canExecuteMethod, false)
        {
        }

        /// <summary>
        /// Constructor with action, CanExecute function and auto requery disabled flag</summary>
        /// <param name="executeMethod">Method to execute</param>
        /// <param name="canExecuteMethod">CanExecute function</param>
        /// <param name="isAutomaticRequeryDisabled">Whether disabling CommandManager's automatic requery on this command</param>
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod, bool isAutomaticRequeryDisabled)
        {
            Requires.NotNull(executeMethod, "executeMethod");

            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
            _isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines if the command can be executed</summary>
        /// <returns>True iff can execute command</returns>
        public bool CanExecute()
        {
            if (_canExecuteMethod != null)
            {
                return _canExecuteMethod();
            }
            return true;
        }

        /// <summary>
        /// Executes the command</summary>
        public void Execute()
        {
            if (_executeMethod != null)
            {
                _executeMethod();
            }
        }

        /// <summary>
        /// Gets or sets disabling CommandManager's automatic requery on this command</summary>
        public bool IsAutomaticRequeryDisabled
        {
            get
            {
                return _isAutomaticRequeryDisabled;
            }
            set
            {
                if (_isAutomaticRequeryDisabled != value)
                {
                    if (value)
                    {
                        CommandManagerHelper.RemoveHandlersFromRequerySuggested(_canExecuteChangedHandlers);
                    }
                    else
                    {
                        CommandManagerHelper.AddHandlersToRequerySuggested(_canExecuteChangedHandlers);
                    }
                    _isAutomaticRequeryDisabled = value;
                }
            }
        }

        /// <summary>
        /// Raises the CanExecuteChaged event</summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        /// <summary>
        /// Protected virtual method to raise CanExecuteChanged event</summary>
        protected virtual void OnCanExecuteChanged()
        {
            CommandManagerHelper.CallWeakReferenceHandlers(_canExecuteChangedHandlers);
        }

        #endregion

        #region ICommand Members

        /// <summary>
        /// Event that is raised when changes occur that affect whether or not the command should execute</summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (!_isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested += value;
                }
                CommandManagerHelper.AddWeakReferenceHandler(ref _canExecuteChangedHandlers, value, 2);
            }
            remove
            {
                if (!_isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested -= value;
                }
                CommandManagerHelper.RemoveWeakReferenceHandler(_canExecuteChangedHandlers, value);
            }
        }

        /// <summary>
        /// Indicates whether the command can execute in its current state</summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, 
        /// this object can be set to null.</param>
        /// <returns>True iff this command can be executed</returns>
        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        /// <summary>
        /// Executes the command</summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, 
        /// this object can be set to null</param>
        void ICommand.Execute(object parameter)
        {
            Execute();
        }

        #endregion

        #region Data

        private readonly Action _executeMethod = null;
        private readonly Func<bool> _canExecuteMethod = null;
        private bool _isAutomaticRequeryDisabled = false;
        private List<WeakReference> _canExecuteChangedHandlers;

        #endregion
    }

    /// <summary>
    /// This class allows delegating the commanding logic to methods passed as parameters,
    /// and enables a View to bind commands to objects that are not part of the element tree</summary>
    /// <typeparam name="T">Type of the parameter passed to delegates</typeparam>
    public class DelegateCommand<T> : ICommand
    {
        #region Constructors

        /// <summary>
        /// Constructor with execute method</summary>
        /// <param name="executeMethod">Method to execute</param>
        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, null, false)
        {
        }

        /// <summary>
        /// Constructor with action and CanExecute function</summary>
        /// <param name="executeMethod">Method to execute</param>
        /// <param name="canExecuteMethod">CanExecute function</param>
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
            : this(executeMethod, canExecuteMethod, false)
        {
        }

        /// <summary>
        /// Constructor with action, CanExecute function and auto requery disabled flag</summary>
        /// <param name="executeMethod">Method to execute</param>
        /// <param name="canExecuteMethod">CanExecute function</param>
        /// <param name="isAutomaticRequeryDisabled">Whether disabling CommandManager's automatic requery on this command</param>
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod, bool isAutomaticRequeryDisabled)
        {
            Requires.NotNull(executeMethod, "executeMethod");

            m_executeMethod = executeMethod;
            m_canExecuteMethod = canExecuteMethod;
            m_isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines if the command can be executed</summary>
        /// <param name="parameter">Type of the parameter passed to delegates</param>
        /// <returns>True iff can execute command</returns>
        public bool CanExecute(T parameter)
        {
            if (m_canExecuteMethod != null)
            {
                return m_canExecuteMethod(parameter);
            }
            return true;
        }

        /// <summary>
        /// Executes the command</summary>
        /// <param name="parameter">Type of the parameter passed to delegates</param>
        public void Execute(T parameter)
        {
            if (m_executeMethod != null)
            {
                m_executeMethod(parameter);
            }
        }

        /// <summary>
        /// Raises the CanExecuteChaged event</summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        /// <summary>
        /// Protected virtual method to raise CanExecuteChanged event</summary>
        protected virtual void OnCanExecuteChanged()
        {
            CommandManagerHelper.CallWeakReferenceHandlers(m_canExecuteChangedHandlers);
        }

        /// <summary>
        /// Gets or sets property to enable or disable the CommandManager's automatic requery on this command</summary>
        public bool IsAutomaticRequeryDisabled
        {
            get
            {
                return m_isAutomaticRequeryDisabled;
            }
            set
            {
                if (m_isAutomaticRequeryDisabled != value)
                {
                    if (value)
                    {
                        CommandManagerHelper.RemoveHandlersFromRequerySuggested(m_canExecuteChangedHandlers);
                    }
                    else
                    {
                        CommandManagerHelper.AddHandlersToRequerySuggested(m_canExecuteChangedHandlers);
                    }
                    m_isAutomaticRequeryDisabled = value;
                }
            }
        }

        #endregion

        #region ICommand Members

        /// <summary>
        /// Event that is raised when changes occur that affect whether or not the command should execute</summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (!m_isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested += value;
                }
                CommandManagerHelper.AddWeakReferenceHandler(ref m_canExecuteChangedHandlers, value, 2);
            }
            remove
            {
                if (!m_isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested -= value;
                }
                CommandManagerHelper.RemoveWeakReferenceHandler(m_canExecuteChangedHandlers, value);
            }
        }

        /// <summary>
        /// Indicates whether the command can execute in its current state</summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, 
        /// this object can be set to null.</param>
        /// <returns>True iff this command can be executed</returns>
        bool ICommand.CanExecute(object parameter)
        {
            // if T is of value type and the parameter is not
            // set yet, then return false if CanExecute delegate
            // exists, else return true
            if (parameter == null &&
                typeof(T).IsValueType)
            {
                return (m_canExecuteMethod == null);
            }
            return CanExecute((T)parameter);
        }

        /// <summary>
        /// Execute command</summary>
        /// <param name="parameter">Parameter passed to delegate</param>
        void ICommand.Execute(object parameter)
        {
            Execute((T)parameter);
        }

        #endregion

        #region Data

        private readonly Action<T> m_executeMethod = null;
        private readonly Func<T, bool> m_canExecuteMethod = null;
        private bool m_isAutomaticRequeryDisabled = false;
        private List<WeakReference> m_canExecuteChangedHandlers;

        #endregion
    }

    /// <summary>
    /// This class allows delegating the commanding logic to methods passed as parameters,
    /// and enables a View to bind commands to objects that are not part of the element tree.</summary>
    /// <typeparam name="T1">Type of the parameter passed to the delegates</typeparam>
    /// <typeparam name="T2">Type of parameter in method that is passed as parameter</typeparam>
    public class DelegateCommand<T1, T2> : ICommand
    {
        #region Constructors

        /// <summary>
        /// Constructor with method</summary>
        /// <param name="m_executeMethod">Method to execute</param>
        public DelegateCommand(Action<T2> m_executeMethod)
            : this(m_executeMethod, null, false)
        {
        }

        /// <summary>
        /// Constructor with execute and can execute methods</summary>
        /// <param name="m_executeMethod">Execute method</param>
        /// <param name="m_canExecuteMethod">Can execute method</param>
        public DelegateCommand(Action<T2> m_executeMethod, Func<T1, bool> m_canExecuteMethod)
            : this(m_executeMethod, m_canExecuteMethod, false)
        {
        }

        /// <summary>
        /// Constructor with execute and can execute methods and flag to enable or disable CommandManager's automatic requery</summary>
        /// <param name="executeMethod">Execute method</param>
        /// <param name="canExecuteMethod">Can execute method</param>
        /// <param name="isAutomaticRequeryDisabled">Flag for disabling automatic requery</param>
        public DelegateCommand(Action<T2> executeMethod, Func<T1, bool> canExecuteMethod, bool isAutomaticRequeryDisabled)
        {
            Requires.NotNull(executeMethod, "executeMethod");

            m_executeMethod = executeMethod;
            m_canExecuteMethod = canExecuteMethod;
            m_isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method to determine if the command can be executed</summary>
        /// <param name="parameter">Method's parameter</param>
        /// <returns>True iff this command can be executed</returns>
        public bool CanExecute(T1 parameter)
        {
            if (m_canExecuteMethod != null)
            {
                return m_canExecuteMethod(parameter);
            }
            return true;
        }

        /// <summary>
        /// Execution of the command</summary>
        /// <param name="parameter">Method's parameter</param>
        public void Execute(T2 parameter)
        {
            if (m_executeMethod != null)
            {
                m_executeMethod(parameter);
            }
        }

        /// <summary>
        ///     Raises the CanExecuteChaged event</summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        /// <summary>
        /// Protected virtual method to raise CanExecuteChanged event</summary>
        protected virtual void OnCanExecuteChanged()
        {
            CommandManagerHelper.CallWeakReferenceHandlers(m_canExecuteChangedHandlers);
        }

        /// <summary>
        /// Property to enable or disable CommandManager's automatic requery on this command</summary>
        public bool IsAutomaticRequeryDisabled
        {
            get
            {
                return m_isAutomaticRequeryDisabled;
            }
            set
            {
                if (m_isAutomaticRequeryDisabled != value)
                {
                    if (value)
                    {
                        CommandManagerHelper.RemoveHandlersFromRequerySuggested(m_canExecuteChangedHandlers);
                    }
                    else
                    {
                        CommandManagerHelper.AddHandlersToRequerySuggested(m_canExecuteChangedHandlers);
                    }
                    m_isAutomaticRequeryDisabled = value;
                }
            }
        }

        #endregion

        #region ICommand Members

        /// <summary>
        ///     ICommand.CanExecuteChanged implementation
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (!m_isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested += value;
                }
                CommandManagerHelper.AddWeakReferenceHandler(ref m_canExecuteChangedHandlers, value, 2);
            }
            remove
            {
                if (!m_isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested -= value;
                }
                CommandManagerHelper.RemoveWeakReferenceHandler(m_canExecuteChangedHandlers, value);
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            // if T is of value type and the parameter is not
            // set yet, then return false if CanExecute delegate
            // exists, else return true
            if (parameter == null &&
                typeof(T1).IsValueType)
            {
                return (m_canExecuteMethod == null);
            }

            return CanExecute((T1)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            Execute((T2)parameter);
        }

        #endregion

        #region Data

        private readonly Action<T2> m_executeMethod = null;
        private readonly Func<T1, bool> m_canExecuteMethod = null;
        private bool m_isAutomaticRequeryDisabled = false;
        private List<WeakReference> m_canExecuteChangedHandlers;

        #endregion
    }

    /// <summary>
    /// This class contains methods for the CommandManager that help avoid memory leaks
    /// by using weak references</summary>
    internal class CommandManagerHelper
    {
        internal static void CallWeakReferenceHandlers(List<WeakReference> handlers)
        {
            if (handlers != null)
            {
                // Take a snapshot of the handlers before we call out to them since the handlers
                // could cause the array to me modified while we are reading it.

                EventHandler[] callees = new EventHandler[handlers.Count];
                int count = 0;

                for (int i = handlers.Count - 1; i >= 0; i--)
                {
                    WeakReference reference = handlers[i];
                    EventHandler handler = reference.Target as EventHandler;
                    if (handler == null)
                    {
                        // Clean up old handlers that have been collected
                        handlers.RemoveAt(i);
                    }
                    else
                    {
                        callees[count] = handler;
                        count++;
                    }
                }

                // Call the handlers that we snapshotted
                for (int i = 0; i < count; i++)
                {
                    EventHandler handler = callees[i];
                    handler(null, EventArgs.Empty);
                }
            }
        }

        internal static void AddHandlersToRequerySuggested(List<WeakReference> handlers)
        {
            if (handlers != null)
            {
                foreach (WeakReference handlerRef in handlers)
                {
                    EventHandler handler = handlerRef.Target as EventHandler;
                    if (handler != null)
                    {
                        CommandManager.RequerySuggested += handler;
                    }
                }
            }
        }

        internal static void RemoveHandlersFromRequerySuggested(List<WeakReference> handlers)
        {
            if (handlers != null)
            {
                foreach (WeakReference handlerRef in handlers)
                {
                    EventHandler handler = handlerRef.Target as EventHandler;
                    if (handler != null)
                    {
                        CommandManager.RequerySuggested -= handler;
                    }
                }
            }
        }

        internal static void AddWeakReferenceHandler(ref List<WeakReference> handlers, EventHandler handler)
        {
            AddWeakReferenceHandler(ref handlers, handler, -1);
        }

        internal static void AddWeakReferenceHandler(ref List<WeakReference> handlers, EventHandler handler, int defaultListSize)
        {
            if (handlers == null)
            {
                handlers = (defaultListSize > 0 ? new List<WeakReference>(defaultListSize) : new List<WeakReference>());
            }

            handlers.Add(new WeakReference(handler));
        }

        internal static void RemoveWeakReferenceHandler(List<WeakReference> handlers, EventHandler handler)
        {
            if (handlers != null)
            {
                for (int i = handlers.Count - 1; i >= 0; i--)
                {
                    WeakReference reference = handlers[i];
                    EventHandler existingHandler = reference.Target as EventHandler;
                    if ((existingHandler == null) || (existingHandler == handler))
                    {
                        // Clean up old handlers that have been collected
                        // in addition to the handler that is to be removed.
                        handlers.RemoveAt(i);
                    }
                }
            }
        }
    }
}
