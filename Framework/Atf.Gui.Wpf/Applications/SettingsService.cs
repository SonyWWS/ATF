//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.IO;

using Sce.Atf.Applications;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Service that manages user editable settings (preferences) and application settings persistence</summary>
    [Export(typeof(ISettingsService))]
    [Export(typeof(SettingsServiceBase))]
    [Export(typeof(SettingsService))]
    [Export(typeof(ISettingsPathsProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SettingsService : SettingsServiceBase, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Constructor</summary>
        public SettingsService()
        {
        }

        #region IPartImportsSatisfiedNotification Members

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            if (m_mainWindow == null)
                throw new InvalidOperationException("Can't get main window");

            m_mainWindow.Loading += mainWindow_Loaded;
            m_mainWindow.Closed += mainWindow_Closed;

            string settingsDirectory = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(settingsDirectory))
                Directory.CreateDirectory(settingsDirectory);
        }

        #endregion

        #region ISettingsService Members

        /// <summary>
        /// Presents the settings dialog to the user, with the tree control opened to
        /// the given path</summary>
        /// <param name="pathName">Path of settings to display initially, or null</param>
        public override void PresentUserSettings(string pathName)
        {
            var vm = new SettingsDialogViewModel(this, pathName);
            var settingsDialog = new SettingsDialog(vm);

            if (settingsDialog.ShowParentedDialog() == true)
            {
                SaveSettings();
            }
        }

        #endregion

        /// <summary>
        /// Presents the load/save settings dialog to the user</summary>
        protected override void PresentLoadSaveSettings()
        {
            new SettingsLoadSaveDialog(new SettingsLoadSaveViewModel(this)).ShowParentedDialog();
        }

        /// <summary>
        /// Get ITreeView of user settings</summary>
        /// <remarks>For use by SettingsDialog only</remarks>
        protected override ITreeView UserSettings
        {
            get { return m_userSettings; }
        }

        internal ITreeView UserSettingsInternal
        {
            get { return m_userSettings; }
        }

        internal List<PropertyDescriptor> GetPropertiesInternal(Tree<object> tree)
        {
            return base.GetProperties(tree);
        }

        internal Path<object> GetSettingsPathInternal(string pathName)
        {
            return base.GetSettingsPath(pathName);
        }

        internal void SerializeInternal(Stream stream)
        {
            base.Serialize(stream);
        }

        internal bool DeserializeInternal(Stream stream)
        {
            return base.Deserialize(stream);
        }

        private void mainWindow_Loaded(object sender, EventArgs e)
        {
            Initialize();
        }

        private void mainWindow_Closed(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private class TreeView : Tree<object>, ITreeView, IItemView
        {
            public TreeView(object root)
                : base(root)
            {
            }

            #region ITreeView Members

            public object Root
            {
                get { return this; }
            }

            public IEnumerable<object> GetChildren(object parent)
            {
                foreach (object child in ((Tree<object>)parent).Children)
                    yield return child;
            }

            #endregion

            #region IItemView Members

            /// <summary>
            /// Gets item's display information</summary>
            /// <param name="item">Item being displayed</param>
            /// <param name="info">Item info, to fill out</param>
            public void GetInfo(object item, ItemInfo info)
            {
                object value = ((Tree<object>)item).Value;
                if (value is string)
                {
                    info.Label = (string)value;
                    //info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.FolderImage);
                    info.AllowSelect = false;
                }
                else
                {
                    UserSettingsInfo settingsInfo = value as UserSettingsInfo;
                    info.Label = settingsInfo.Name;
                    info.AllowLabelEdit = false;
                    //info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.PreferencesImage);
                    info.IsLeaf = true;
                }
            }

            #endregion
        }

        [Import(AllowDefault = true)]
        private IMainWindow m_mainWindow = null;

        private readonly TreeView m_userSettings = new TreeView(string.Empty);
    }
}
