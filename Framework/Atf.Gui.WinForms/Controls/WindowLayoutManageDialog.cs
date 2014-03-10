//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Applications;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Dialog to manage window layouts</summary>
    public partial class WindowLayoutManageDialog : Form
    {
        /// <summary>
        /// Constructor</summary>
        public WindowLayoutManageDialog()
        {
            InitializeComponent();

            m_selectLayoutLabel =
                new Label
                    {
                        Text = "Select a layout".Localize(),
                        AutoSize = false,
                        Width = m_split.Panel2.Width,
                        Height = m_split.Panel2.Height,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    };

            m_split.Panel2.Controls.Add(m_selectLayoutLabel);
            m_screenshot.Hide();
        }

        /// <summary>
        /// Gets or sets the directory that layout screenshots live in</summary>
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

                    if (files == null)
                        return;

                    if (files.Length <= 0)
                        return;

                    foreach (var file in files)
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        Image image = Image.FromFile(file);

                        m_screenshots.Add(name, image);
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

        /// <summary>
        /// Gets or sets the list of layout names</summary>
        public IEnumerable<string> LayoutNames
        {
            get { return m_names; }
            set
            {
                try
                {
                    m_layouts.BeginUpdate();

                    m_names.Clear();
                    m_layouts.Items.Clear();

                    foreach (var name in value)
                        AddLayout(name, m_layouts, m_names);
                }
                finally
                {
                    m_layouts.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Gets the layouts that have been renamed</summary>
        /// <remarks>In KeyValuePair, key is old name and value is new name.</remarks>
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

        private void WindowLayoutManageDialogLoad(object sender, EventArgs e)
        {
            int startingWidth = m_layouts.Columns[0].Width;
            m_layouts.Columns[0].Width = -1;
            if (m_layouts.Columns[0].Width < startingWidth)
                m_layouts.Columns[0].Width = startingWidth;
        }

        private void LayoutsSelectedIndexChanged(object sender, EventArgs e)
        {
            TogglePanel2Control();

            var items = m_layouts.SelectedItems.Cast<ListViewItem>();
            if (items.Count() != 1)
            {
                m_screenshot.Image = null;
            }
            else
            {
                Image image;
                if (!m_screenshots.TryGetValue(items.ElementAt(0).Text, out image))
                    return;

                m_screenshot.Image = image;
            }
        }

        private void LayoutsAfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            ListViewItem lstItem = m_layouts.Items[e.Item];
            Point toolTipPos = GetSubItemTopRightPoint(lstItem.SubItems[0]);

            string oldName = lstItem.Text;
            string newName = e.Label;

            // Invalid label
            if (string.IsNullOrEmpty(newName))
            {
                e.CancelEdit = true;
                m_toolTip.Show(m_layouts, toolTipPos, null, ToolTipIcon.Warning, "Name can't be empty!");
                return;
            }

            // No duplicates
            if (m_names.Contains(newName))
            {
                e.CancelEdit = true;

                if (string.Compare(newName, oldName) != 0)
                    m_toolTip.Show(m_layouts, toolTipPos, null, ToolTipIcon.Warning, "A layout with this name already exists!");

                return;
            }

            // No invalid characters
            if (!WindowLayoutService.IsValidLayoutName(newName))
            {
                e.CancelEdit = true;
                m_toolTip.Show(m_layouts, toolTipPos, null, ToolTipIcon.Warning, "Name contains illegal characters!");
                return;
            }

            // Keep track of renames
            {
                var tag = (LayoutTag)lstItem.Tag;
                m_renamedLayouts[tag.OldName] = newName;
            }

            // Rename in the name of layouts list
            {
                m_names.Remove(oldName);
                m_names.Add(newName);
            }

            // Rename screenshot
            try
            {
                m_screenshot.Image = null;

                // Dispose of existing screenshot so it can be renamed
                Image image;
                if (m_screenshots.TryGetValue(oldName, out image))
                {
                    image.Dispose();
                    m_screenshots.Remove(oldName);
                }

                //
                // Try and remove existing screenshot on disk
                //

                string sourceFile =
                    Path.Combine(
                        m_screenshotDirectory.FullName + Path.DirectorySeparatorChar,
                        oldName + WindowLayoutServiceCommandsBase.ScreenshotExtension);

                if (!File.Exists(sourceFile))
                    return;

                string destFile =
                    Path.Combine(
                        m_screenshotDirectory.FullName + Path.DirectorySeparatorChar,
                        newName + WindowLayoutServiceCommandsBase.ScreenshotExtension);

                File.Move(sourceFile, destFile);

                // Add renamed screenshot back
                if (File.Exists(destFile))
                    m_screenshots.Add(newName, Image.FromFile(destFile));
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(
                    OutputMessageType.Error,
                    "Manage Layouts: Exception " +
                    "renaming screenshot: {0}",
                    ex.Message);
            }
            finally
            {
                // Try and use the renamed screenshot
                Image image;
                if (m_screenshots.TryGetValue(newName, out image))
                    m_screenshot.Image = image;
            }
        }

        private void BtnRenameClick(object sender, EventArgs e)
        {
            var selectedItems = m_layouts.SelectedItems.Cast<ListViewItem>();
            if (selectedItems.Count() != 1)
                return;

            selectedItems.ElementAt(0).BeginEdit();
        }

        private void BtnDeleteClick(object sender, EventArgs e)
        {
            var selectedItems = m_layouts.SelectedItems.Cast<ListViewItem>();
            if (selectedItems.Count() <= 0)
                return;

            try
            {
                m_layouts.BeginUpdate();

                foreach (var lstItem in selectedItems)
                {
                    string name = lstItem.Text;

                    RemoveLayout(lstItem, m_layouts, m_names);
                    RemoveScreenshot(name, m_screenshotDirectory, m_screenshots);

                    // Don't schedule a rename
                    var tag = (LayoutTag)lstItem.Tag;
                    m_renamedLayouts.Remove(tag.OldName);

                    // Make sure to use the old name
                    m_deletedLayouts.Add(tag.OldName);
                }
            }
            finally
            {
                m_layouts.EndUpdate();
            }
        }

        private void TogglePanel2Control()
        {
            if (m_layouts.SelectedItems.Count > 0)
            {
                m_screenshot.Show();
                m_selectLayoutLabel.Hide();
            }
            else
            {
                m_screenshot.Hide();
                m_selectLayoutLabel.Show();
            }
        }

        private static void AddLayout(string name, ListView listView, List<string> names)
        {
            try
            {
                listView.BeginUpdate();

                names.Add(name);

                var lstItem = new ListViewItem(name) {Tag = new LayoutTag(name)};
                listView.Items.Add(lstItem);
            }
            finally
            {
                listView.EndUpdate();
            }
        }

        private static void RemoveLayout(ListViewItem lstItem, ListView listView, List<string> names)
        {
            try
            {
                listView.BeginUpdate();

                names.Remove(lstItem.Text);
                listView.Items.Remove(lstItem);
            }
            finally
            {
                listView.EndUpdate();
            }
        }

        private static void RemoveScreenshot(string name, DirectoryInfo screenshotDirectory, Dictionary<string, Image> screenshots)
        {
            try
            {
                Image image;
                if (screenshots.TryGetValue(name, out image))
                {
                    image.Dispose();
                    screenshots.Remove(name);
                }

                string path =
                    Path.Combine(
                        screenshotDirectory.FullName + Path.DirectorySeparatorChar,
                        name + WindowLayoutServiceCommandsBase.ScreenshotExtension);

                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(
                    OutputMessageType.Error,
                    "Manage layouts: Exception " +
                    "deleting screenshot: {0}",
                    ex.Message);
            }
        }

        private static Point GetSubItemTopRightPoint(ListViewItem.ListViewSubItem subItem)
        {
            var topRight = new Point(subItem.Bounds.X + subItem.Bounds.Right, subItem.Bounds.Y);
            return topRight;
        }

        #region Private Classes

        private class LayoutTag
        {
            public LayoutTag(string oldName)
            {
                OldName = oldName;
            }

            public string OldName { get; private set; }
        }

        #endregion

        private DirectoryInfo m_screenshotDirectory;

        private Label m_selectLayoutLabel;

        private readonly List<string> m_names =
            new List<string>();

        private readonly List<string> m_deletedLayouts =
            new List<string>();

        private readonly BalloonToolTipHelper m_toolTip =
            new BalloonToolTipHelper();

        private readonly Dictionary<string, Image> m_screenshots =
            new Dictionary<string, Image>();

        private readonly Dictionary<string, string> m_renamedLayouts =
            new Dictionary<string, string>(StringComparer.CurrentCulture);
    }
}
