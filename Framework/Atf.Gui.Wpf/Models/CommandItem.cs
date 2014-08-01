//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Sce.Atf.Applications;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Interop;

namespace Sce.Atf.Wpf.Models
{
    [Export(typeof(IMenuItem))]
    [Export(typeof(IToolBarItem))]
    internal class CommandItem : NotifyPropertyChangedBase, ICommandItem, IToolBarItem
    {
        public CommandItem(CommandInfo info,
            Func<ICommandItem, bool> canExecuteMethod,
            Action<ICommandItem> executeMethod)
        {
            m_info = info;
            m_text = info.DisplayedMenuText;
            MenuPath = ParseMenuPath(info.MenuText);
            m_description = info.Description;
            m_imageSourceKey = FixUpLegacyImage(info.ImageKey);
            info.ShortcutsChanged += Info_ShortcutsChanged;

            // Special hack here - if no menu tag is set then this command is only available
            // in context menus.  Ideally should add CommandVisibility.ContextMenu option
            // for now just use CommandVisibility.None
            m_visibility = info.MenuTag != null ? info.Visibility : CommandVisibility.None;
            m_canExecuteMethod = canExecuteMethod;
            m_executeMethod = executeMethod;

            RebuildBinding();
        }

        public ComposablePart ComposablePart { get; set; }

        #region ICommandItem Members

        public object CommandTag { get { return m_info.CommandTag; } }

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

        public IEnumerable<Keys> Shortcuts
        {
            get { return m_info.Shortcuts; }
            set { m_info.Shortcuts = value; }
        }

        public CommandVisibility Visibility { get { return m_visibility; } }

        public int Index
        {
            get { return m_info.Index; }
        }

        #endregion

        #region IMenuItem Members

        public string Text
        {
            get { return m_text; }
            set { m_text = value; OnPropertyChanged(s_textArgs); }
        }

        public string Description
        {
            get { return m_description; }
            set { m_description = value; OnPropertyChanged(s_descriptionArgs); }
        }

        public bool IsVisible
        {
            get { return this.IsVisible(CommandVisibility.Menu); }
        }

        public object MenuTag { get { return m_info.MenuTag; } }

        public object GroupTag { get { return m_info.GroupTag; } }

        public IEnumerable<string> MenuPath { get; private set; }

        #endregion

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return m_canExecuteMethod == null || m_canExecuteMethod(this);
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

        #region IToolBarItem Members

        object IToolBarItem.Tag
        {
            get { return CommandTag; }
        }

        object IToolBarItem.ToolBarTag
        {
            get { return MenuTag; }
        }

        bool IToolBarItem.IsVisible
        {
            get { return this.IsVisible(CommandVisibility.Toolbar); }
        }

        #endregion

        private void Info_ShortcutsChanged(object sender, EventArgs e)
        {
            RebuildBinding();
            OnPropertyChanged(s_shortcutArgs);
        }

        private IEnumerable<string> ParseMenuPath(string menuText)
        {
            if (!string.IsNullOrEmpty(menuText))
            {
                string[] segments;
                if (menuText[0] == '@')
                    segments = new[] { menuText.Substring(1, menuText.Length - 1) };
                else
                    segments = menuText.Split(s_pathDelimiters, 8);

                if(segments.Length > 1)
                {
                    string[] result = new string[segments.Length - 1];
                    Array.Copy(segments, result, segments.Length - 1);
                    return result;
                }
            }
            return EmptyArray<string>.Instance;

        }

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
            foreach (var shortcut in Shortcuts)
            {
                if (shortcut != Keys.None)
                {
                    var keyGesture = ToWpfKeyGesture(shortcut);
                    var binding = new InputBinding(this, keyGesture);
                    m_inputBindings.Add(binding);
                    window.InputBindings.Add(binding);
                }
            }
        }

        private static object FixUpLegacyImage(object imageKey)
        {
            // Default to just return the image key unchanged
            object imageResourceKey = imageKey;

            string imageName = imageKey as string;
            if (!string.IsNullOrEmpty(imageName))
            {
                // If the key is a string then check if the image resource exists
                // using the string directly as the key
                var existingWpfImageResource = Application.Current.TryFindResource(imageName);
                if (existingWpfImageResource == null)
                {
                    // If not then this may be a legacy embedded image string
                    var embeddedImage = Sce.Atf.ResourceUtil.GetImage(imageName);
                    if (embeddedImage == null)
                        throw new InvalidOperationException("Could not find embedded image: " + imageName);

                    // Add a resource to app resources keyed by the embedded image
                    // and set the resource key
                    Util.GetOrCreateResourceForEmbeddedImage(embeddedImage);
                    imageResourceKey = embeddedImage;
                }
            }

            return imageResourceKey;
        }

        private static KeyGesture ToWpfKeyGesture(Keys atfKeys)
        {
            ModifierKeys modifiers = Sce.Atf.Wpf.Interop.KeysInterop.ToWpfModifiers(atfKeys);
            Key key = Sce.Atf.Wpf.Interop.KeysInterop.ToWpf(atfKeys);
            return new KeyGesture(key, modifiers);
        }

        private bool m_isChecked;
        private object m_imageSourceKey;
        private readonly CommandVisibility m_visibility;
        private List<InputBinding> m_inputBindings = new List<InputBinding>();
        private readonly CommandInfo m_info;
        private readonly Func<ICommandItem, bool> m_canExecuteMethod;
        private readonly Action<ICommandItem> m_executeMethod;
        private string m_text;
        private string m_description;
        private static readonly char[] s_pathDelimiters = new[] { '/', '\\' };

        // Property changed event args
        private static readonly PropertyChangedEventArgs s_textArgs
            = ObservableUtil.CreateArgs<CommandItem>(x => x.Text);
        private static readonly PropertyChangedEventArgs s_descriptionArgs
            = ObservableUtil.CreateArgs<CommandItem>(x => x.Description);
        private static readonly PropertyChangedEventArgs s_shortcutArgs
            = ObservableUtil.CreateArgs<CommandItem>(x => x.Shortcuts);
        private static readonly PropertyChangedEventArgs s_imageSourceKeyArgs
            = ObservableUtil.CreateArgs<CommandItem>(x => x.ImageSourceKey);
        private static readonly PropertyChangedEventArgs s_isCheckedArgs
            = ObservableUtil.CreateArgs<CommandItem>(x => x.IsChecked);

    }
}
