//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using Sce.Atf.Wpf.Controls;
using PropertyDescriptor = System.ComponentModel.PropertyDescriptor;
using SettingsService = Sce.Atf.Wpf.Applications.SettingsService;

namespace Sce.Atf.Wpf.Models
{
    internal class SettingsDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="settingsService">Settings service that manages preferences</param>
        /// <param name="pathName">Path to the setting to show</param>
        public SettingsDialogViewModel(SettingsService settingsService, string pathName)
        {
            Title = "Preferences".Localize();

            m_settingsService = settingsService;
            m_originalState = m_settingsService.UserState; // for cancel

            m_treeViewAdapter = new TreeViewWithSelection(settingsService.UserSettingsInternal);
            m_treeViewAdapter.SelectionChanged += TreeViewAdapterSelectionChanged;
            TreeViewModel = new TreeViewModel { MultiSelectEnabled = false, ShowRoot = false, TreeView = m_treeViewAdapter };
            TreeViewModel.ExpandAll();

            Node node = pathName != null
                            ? TreeViewModel.Show(m_settingsService.GetSettingsPathInternal(pathName), true)
                            : TreeViewModel.ExpandToFirstLeaf();

            node.IsSelected = true;

            SetDefaultsCommand = new DelegateCommand(SetDefaults, CanSetDefaults, false);
        }

        /// <summary>
        /// Gets the ICommand that resets the default values</summary>
        public ICommand SetDefaultsCommand { get; private set; }

        /// <summary>
        /// Gets the image provider service</summary>
        public object ImageProvider { get; private set; }

        /// <summary>
        /// Gets the view model for the tree view</summary>
        public TreeViewModel TreeViewModel { get; private set; }

        /// <summary>
        /// Gets and sets the list of property descriptors for the settings. Set actually
        /// replaces the entire list.</summary>
        public IEnumerable<PropertyDescriptor> PropertyDescriptors
        {
            get { return m_properties; }
            set
            {
                m_properties = value;
                RaisePropertyChanged("PropertyDescriptors");
            }
        }

        /// <summary>
        /// Gets and sets the list of selected items. Set actually replaces the entire list.</summary>
        public IEnumerable Items
        {
            get { yield return m_item; }
            set
            {
                m_item = value;
                RaisePropertyChanged("Items");
            }
        }

        /// <summary>
        /// Always returns true</summary>
        /// <returns>True</returns>
        protected bool CanSetDefaults()
        {
            return true;
        }

        /// <summary>
        /// Event fired when the dialog is closing</summary>
        /// <param name="args">Event args</param>
        protected override void OnCloseDialog(CloseDialogEventArgs args)
        {
            if (args.DialogResult != true)
            {
                m_settingsService.UserState = m_originalState;
            }

            RaiseCloseDialog(args);
        }

        private void SetDefaults()
        {
            var result = WpfMessageBox.Show(
                "Reset all preferences to their default values?".Localize(),
                "Reset All Preferences".Localize(), System.Windows.MessageBoxButton.OKCancel);

            if (result == System.Windows.MessageBoxResult.OK)
            {
                m_settingsService.SetDefaults();
            }
        }

        private IEnumerable m_item;
        private IEnumerable<PropertyDescriptor> m_properties;
        private readonly TreeViewWithSelection m_treeViewAdapter;

        private void TreeViewAdapterSelectionChanged(object sender, EventArgs e)
        {
            var selected = m_treeViewAdapter.GetLastSelected<Tree<object>>();
            if (selected == null)
                return;

            Items = new[] { selected };
            var properties = m_settingsService.GetPropertiesInternal(selected);
            if (properties != null)
            {
                PropertyDescriptors = properties.ToArray();
            }
        }

        private readonly SettingsService m_settingsService;
        private readonly object m_originalState;
    }
}
