//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Sce.Atf.Wpf.Models
{
    internal sealed class ManageWindowLayoutsDialogViewModel : DialogViewModelBase
    {
        public ManageWindowLayoutsDialogViewModel(IEnumerable<Pair<string, Keys>> layouts)
        {
            Title = "Select a layout";

            RenameCommand = new DelegateCommand<LayoutSlot>(Rename, CanRename, false);
            DeleteCommand = new DelegateCommand<LayoutSlot>(Delete, CanDelete, false);

            foreach (var layout in layouts)
            {
                var slot = new LayoutSlot(this, layout.First, layout.Second);
                slot.Renamed += SlotRenamed;
                Layouts.Add(slot);
            }

            LayoutView = CollectionViewSource.GetDefaultView(Layouts);
        }

        #region Renamed Event Handler

        void SlotRenamed(object sender, EventArgs e)
        {
            var slot = sender as LayoutSlot;
            if (slot == null)
                return;

            // Back to original if not valid
            if (string.IsNullOrEmpty(slot.Name) ||
                LayoutSlot.IsValidName(slot.Name) != null)
            {
                slot.Name = slot.OldName;
            }
            else
            {
                m_renamedLayouts[slot.OldName] = slot.Name;

                if (m_screenshots.Contains(slot))
                    m_screenshots.Remove(slot);

                //
                // Try and remove existing screenshot on disk
                //
                string sourceFile =
                    Path.Combine(
                        m_screenshotDirectory.FullName + Path.DirectorySeparatorChar,
                        slot.OldName + WindowLayoutServiceCommandsBase.ScreenshotExtension);

                if (!File.Exists(sourceFile))
                    return;

                string destFile =
                    Path.Combine(
                        m_screenshotDirectory.FullName + Path.DirectorySeparatorChar,
                        slot.Name + WindowLayoutServiceCommandsBase.ScreenshotExtension);

                if (!sourceFile.Equals(destFile))
                {
                    slot.Image = null;
                    
                    try
                    {
                        File.Copy(sourceFile, destFile);
                        
                        if (File.Exists(destFile))
                            m_screenshots.Add(slot);

                        File.Delete(sourceFile);
                    }
                    catch (IOException)
                    {
                    }

                    slot.Image = ImageUtil.CreateFromFile(destFile);
                }
            }
        }

        #endregion

        #region DeleteCommand

        public ICommand DeleteCommand { get; private set; }

        private bool CanDelete(LayoutSlot param)
        {
            var slot = GetCurrentSlot();
            return slot != null && slot.Name != null;
        }

        private void Delete(LayoutSlot param)
        {
            var selectedItem = GetCurrentSlot();

            if (selectedItem != null)
            {
                var name = selectedItem.OldName;

                // Don't schedule a rename
                m_renamedLayouts.Remove(name);

                // Make sure to use the old name
                m_deletedLayouts.Add(name);

                Layouts.Remove(selectedItem);

                try
                {
                    selectedItem.Image = null;

                    if (m_screenshots.Contains(selectedItem))
                        m_screenshots.Remove(selectedItem);

                    // Remove screenshot
                    string path =
                        Path.Combine(
                            m_screenshotDirectory.FullName + Path.DirectorySeparatorChar,
                            selectedItem.Name + WindowLayoutServiceCommandsBase.ScreenshotExtension);

                    if (File.Exists(path))
                        File.Delete(path);
                }
                catch (Exception ex)
                {
                    Outputs.WriteLine(
                        OutputMessageType.Error,
                        "Manage layouts: Exception " + "deleting screenshot: {0}",
                        ex.Message);
                }
            }
        }

        #endregion

        #region RenameCommand

        public ICommand RenameCommand { get; private set; }

        private bool CanRename(LayoutSlot param)
        {
            var slot = GetCurrentSlot();
            return slot != null;
        }

        private void Rename(LayoutSlot param)
        {
            var slot = GetCurrentSlot();
            if (slot != null)
                slot.IsInEditMode = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Sets the directory the layout screenshots live in</summary>
        public DirectoryInfo ScreenshotDirectory
        {
            get { return m_screenshotDirectory; }
            set
            {
                if (value == null)
                    return;

                if (ReferenceEquals(value, m_screenshotDirectory))
                    return;

                m_screenshotDirectory = value;

                try
                {
                    m_screenshots.Clear();

                    if (!Directory.Exists(m_screenshotDirectory.FullName))
                        return;

                    string[] files =
                        Directory.GetFiles(
                            m_screenshotDirectory.FullName,
                            "*" + WindowLayoutServiceCommandsBase.ScreenshotExtension,
                            SearchOption.TopDirectoryOnly);

                    if (!files.Any())
                        return;

                    foreach (var file in files)
                    {
                        var name = Path.GetFileNameWithoutExtension(file);
                        var layoutSlot = m_layouts.FirstOrDefault(x => x.Name.Equals(name));
                        if (layoutSlot != null)
                        {
                            layoutSlot.Image = ImageUtil.CreateFromFile(file);
                            m_screenshots.Add(layoutSlot);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Outputs.WriteLine(
                        OutputMessageType.Error,
                        "Manage Layouts: Exception parsing " +
                        "screenshot directory: {0}", ex.Message);
                }
            }
        }

        public ObservableCollection<LayoutSlot> Layouts
        {
            get { return m_layouts; }
        }

        public ICollectionView LayoutView { get; private set; }

        /// <summary>
        /// Gets the layouts that have been renamed</summary>
        /// <remarks>Key is old name value is new name</remarks>
        public IEnumerable<KeyValuePair<string, string>> RenamedLayouts
        {
            get { return m_renamedLayouts; }
        }

        /// <summary>
        /// Gets the layouts that have been deleted</summary>
        public IEnumerable<string> DeletedLayouts
        {
            get { return m_deletedLayouts; }
        }

        #endregion

        #region Private

        private LayoutSlot GetCurrentSlot()
        {
            return CollectionViewSource.GetDefaultView(Layouts).CurrentItem as LayoutSlot;
        }

        private DirectoryInfo m_screenshotDirectory;

        private readonly ObservableCollection<LayoutSlot> m_layouts =
            new ObservableCollection<LayoutSlot>();

        private readonly List<string> m_deletedLayouts =
            new List<string>();

        private readonly List<LayoutSlot> m_screenshots =
            new List<LayoutSlot>();

        private readonly Dictionary<string, string> m_renamedLayouts =
            new Dictionary<string, string>(StringComparer.CurrentCulture);

        #endregion
    }

    internal class LayoutSlot : NotifyPropertyChangedBase, IDataErrorInfo
    {
        public event EventHandler Renamed;

        public LayoutSlot(ManageWindowLayoutsDialogViewModel parent, string name, Keys shortcut)
        {
            Name = name;
            OldName = name;
            Shortcut = shortcut;
            RenameCommand = parent.RenameCommand;
            DeleteCommand = parent.DeleteCommand;
        }

        #region Properties

        public ICommand RenameCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
                RaisePropertyChanged("Name");
            }
        }

        private string m_name;

        public string OldName { get; private set; }
        public Keys Shortcut { get; set; }

        public bool IsInEditMode
        {
            get { return m_isEditing; }
            set
            {
                if (m_isEditing != value)
                {
                    m_isEditing = value;
                    RaisePropertyChanged("IsInEditMode");

                    if (m_isEditing == false)
                    {
                        Renamed.Raise(this, EventArgs.Empty);
                    }
                }
            }
        }

        private bool m_isEditing;

        public ImageSource Image
        {
            get { return m_image; }
            set
            {
                m_image = value;
                RaisePropertyChanged("Image");
            }
        }

        private ImageSource m_image;

        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                string result = null;
                if (columnName == "Name")
                {
                    if (!string.IsNullOrEmpty(Name))
                        result = IsValidName(Name);
                }

                return result;
            }
        }

        public static string IsValidName(string layoutName)
        {
            if (string.IsNullOrEmpty(layoutName))
                return "Layout name is empty";

            if (!WindowLayoutService.IsValidLayoutName(layoutName))
                return "Layout name contains invalid characters";

            return null;
        }

        #endregion
    }
}
