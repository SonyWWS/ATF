//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using Sce.Atf.Applications;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    internal class CommandItem : MenuItemBase, ICommandItem
    {
        public CommandItem(CommandDef def, 
            Func<object, bool> canExecuteMethod,
            Action<ICommandItem> executeMethod)
            : base(def.MenuTag, def.GroupTag, def.Text, def.Description)
        {
            m_commandTag = def.CommandTag;
            m_imageSourceKey = def.ImageSourceKey;
            m_inputGestures = def.InputGestures;
            m_menuPath = def.MenuPath;

            // Special hack here - if no menu tag is set then this command is only available
            // in context menus.  Ideally should add CommandVisibility.ContextMenu option
            // for now just use CommandVisibility.None
            m_visibility = def.MenuTag != null ? def.Visibility : CommandVisibility.None;
            m_canExecuteMethod = canExecuteMethod;
            m_executeMethod = executeMethod;

            RebuildBinding();
        }

        #region ICommandItem Members

        public object CommandTag { get { return m_commandTag; } }

        public bool IsChecked
        {
            get { return m_isChecked; }
            set { m_isChecked = value; OnPropertyChanged(s_isCheckedArgs); }
        }

        public object ImageSourceKey
        {
            get { return m_imageSourceKey; }
            set { m_imageSourceKey = value; OnPropertyChanged(s_imageSourceKeyArgs); }
        }

        public InputGesture[] InputGestures 
        { 
            get { return m_inputGestures; }
            set 
            { 
                m_inputGestures = value;
                RebuildBinding();
                OnPropertyChanged(s_inputGesturesArgs); 
            }
        }

        public CommandVisibility Visibility { get { return m_visibility; } }

        public IEnumerable<string> MenuPath { get { return m_menuPath; } }

        #endregion

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return m_canExecuteMethod != null ? m_canExecuteMethod(this) : true;
        }

        public void Execute(object parameter)
        {
            m_executeMethod(this);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (m_canExecuteMethod != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (m_canExecuteMethod != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        #endregion

        private void RebuildBinding()
        {
            // TODO: is this safe? perhaps keep a ref to the main window 
            var window = Application.Current.MainWindow;
            if (window == null)
                return;

            // Remove old bindings
            foreach (var binding in m_inputBindings)
            {
                window.InputBindings.Remove(binding);
            }
            
            // Add new bindings
            m_inputBindings = new List<InputBinding>();
            for (int i = 0; i < m_inputGestures.Length; i++)
            {
                var binding = new InputBinding(this, m_inputGestures[i]);
                m_inputBindings.Add(binding);
                window.InputBindings.Add(binding);
            }
        }

        private readonly object m_commandTag;
        private bool m_isChecked;
        private object m_imageSourceKey;
        private InputGesture[] m_inputGestures;
        private readonly string[] m_menuPath;
        private readonly CommandVisibility m_visibility;
        private List<InputBinding> m_inputBindings = new List<InputBinding>();
        private readonly Func<object, bool> m_canExecuteMethod;
        private readonly Action<ICommandItem> m_executeMethod;

        private static readonly PropertyChangedEventArgs s_inputGesturesArgs
            = ObservableUtil.CreateArgs<CommandItem>(x => x.InputGestures);
        private static readonly PropertyChangedEventArgs s_imageSourceKeyArgs
            = ObservableUtil.CreateArgs<CommandItem>(x => x.ImageSourceKey);
        private static readonly PropertyChangedEventArgs s_isCheckedArgs
            = ObservableUtil.CreateArgs<CommandItem>(x => x.IsChecked);

    }
}
