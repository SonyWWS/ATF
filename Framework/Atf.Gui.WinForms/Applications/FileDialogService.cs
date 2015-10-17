//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Standard file dialog service, using ATF OpenFileDialog, SaveFileDialog and
    /// ConfirmationDialog classes. Use this component to provide file dialogs for an
    /// application. An implementation of IFileDialogService is required by
    /// StandardFileCommands to implement an application's standard File menu commands:
    /// File/New, File/Open, File/Save, File/Save As, and File/Close.</summary>
    [Export(typeof(IFileDialogService))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FileDialogService : IFileDialogService, IInitializable
    {
        #region IFileDialogService Members

        /// <summary>
        /// Gets and sets a string to be used as the initial directory for the first time this application runs
        /// on the user's computer. The default value is the user's "My Documents" folder. This property
        /// can only be set to a directory that exists.</summary>
        public string InitialDirectory
        {
            get
            {
                if (m_firstRunInitialDirectory == null || !Directory.Exists(m_firstRunInitialDirectory))
                    m_firstRunInitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                //In case the "My Documents" directory does not exist (e.g. if the application is running as a system account)
                if (string.IsNullOrEmpty(m_firstRunInitialDirectory))
                    m_firstRunInitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);

                return m_firstRunInitialDirectory;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) &&
                    Directory.Exists(value))
                {
                    m_firstRunInitialDirectory = value;
                }
            }
        }

        /// <summary>
        /// Gets and sets a string to be used as the initial directory for the open/save dialog box
        /// regardless of whatever directory the user may have previously navigated to. The default
        /// value is null. Set to null to cancel this behavior.</summary>
        public string ForcedInitialDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets file name for file "Open" operation</summary>
        /// <param name="pathName">File name</param>
        /// <param name="filter">File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</param>
        /// <returns>Dialog result</returns>
        public FileDialogResult OpenFileName(ref string pathName, string filter)
        {
            CustomOpenFileDialog dialog = new CustomOpenFileDialog();
            dialog.Filter = filter;
            dialog.RestoreDirectory = true;
            dialog.ForcedInitialDirectory = GetInitialDirectory();

            DialogResult result = dialog.ShowDialog(GetDialogOwner());
            if (result == DialogResult.OK)
                pathName = dialog.FileName;

            return DialogResultToFileDialogResult(result);
        }

        /// <summary>
        /// Gets multiple file names for file "Open" operation</summary>
        /// <param name="pathNames">File names</param>
        /// <param name="filter">File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</param>
        /// <returns>Dialog result</returns>
        public FileDialogResult OpenFileNames(ref string[] pathNames, string filter)
        {
            CustomOpenFileDialog dialog = new CustomOpenFileDialog();
            dialog.Filter = filter;
            dialog.RestoreDirectory = true;
            dialog.Multiselect = true;
            dialog.ForcedInitialDirectory = GetInitialDirectory();

            DialogResult result = dialog.ShowDialog(GetDialogOwner());
            if (result == DialogResult.OK)
                pathNames = dialog.FileNames;

            return DialogResultToFileDialogResult(result);
        }

        /// <summary>
        /// Gets file name for file "Save" operation</summary>
        /// <param name="pathName">File name</param>
        /// <param name="filter">File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</param>
        /// <returns>Dialog result</returns>
        public FileDialogResult SaveFileName(ref string pathName, string filter)
        {
            CustomSaveFileDialog dialog = new CustomSaveFileDialog();
            dialog.Filter = filter;
            dialog.RestoreDirectory = true;

            // Remove the path portion, if any, just as in ATF 3.5 and earlier. http://tracker.ship.scea.com/jira/browse/WWSATF-1406
            pathName = Path.GetFileName(pathName);
            dialog.FileName = pathName;
            
            dialog.ForcedInitialDirectory = GetInitialDirectory();

            DialogResult result = dialog.ShowDialog(GetDialogOwner());
            if (result == DialogResult.OK)
                pathName = dialog.FileName;

            return DialogResultToFileDialogResult(result);
        }

        /// <summary>
        /// Confirms that file should be closed</summary>
        /// <param name="message">Confirmation message</param>
        /// <returns>Dialog result</returns>
        public FileDialogResult ConfirmFileClose(string message)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Close".Localize("Close file"), message);
            dialog.YesButtonText = "&Save".Localize("The '&' is optional and means that Alt+S is the keyboard shortcut on this button");
            dialog.NoButtonText = "&Discard".Localize("The '&' is optional and means that Alt+D is the keyboard shortcut on this button");
            DialogResult result = dialog.ShowDialog(GetDialogOwner());
            dialog.Dispose();
            return DialogResultToFileDialogResult(result);
        }

        /// <summary>
        /// Returns a value indicating if the file path exists</summary>
        /// <param name="pathName">File path</param>
        /// <returns><c>True</c> if the file path exists</returns>
        public bool PathExists(string pathName)
        {
            return File.Exists(pathName);
        }

        #endregion

        /// <summary>
        /// Gets or sets the IMainWindow that is used to make the open file dialog modal</summary>
        [Import(AllowDefault = true)]
        public IMainWindow MainWindow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the main form which is used to make the open file dialog modal</summary>
        [Import(AllowDefault = true)]
        public Form MainForm
        {
            get;
            set;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            if (m_settingsService != null)
            {
                BoundPropertyDescriptor recentDocuments =
                    new BoundPropertyDescriptor(this, () => RecentDirectoriesAsXml, "RecentDirectories", null, null);

                // Using the type name so that derived classes don't lose old user settings
                m_settingsService.RegisterSettings("Sce.Atf.Applications.FileDialogService",
                    recentDocuments);
            }
        }

        #endregion

        private string RecentDirectoriesAsXml
        {
            get
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0",
                    System.Text.Encoding.UTF8.WebName, "yes"));
                XmlElement root = xmlDoc.CreateElement("FileDialogServiceSettings");
                xmlDoc.AppendChild(root);

                foreach (var filterDirPair in CustomFileDialog.FilterToLastUsedDirectory)
                {
                    XmlElement infoElement = xmlDoc.CreateElement("lastUsedDir");
                    root.PrependChild(infoElement);

                    infoElement.SetAttribute("filter", filterDirPair.Key);
                    infoElement.SetAttribute("dir", filterDirPair.Value);
                }

                return xmlDoc.InnerXml;
            }
            set
            {
                // Attempt to read the settings in the XML format.
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(value);
                XmlElement root = xmlDoc.DocumentElement;
                foreach (XmlNode filterDirNode in root.GetElementsByTagName("lastUsedDir"))
                {
                    string filter = filterDirNode.Attributes["filter"].Value;
                    string dir = filterDirNode.Attributes["dir"].Value;
                    CustomFileDialog.FilterToLastUsedDirectory[filter] = dir;
                }
            }
        }

        private bool HasRunBeforeOnUserAccount
        {
            get
            {
                if (m_settingsPathProvider != null)
                    return File.Exists(m_settingsPathProvider.SettingsPath);

                return true;
            }
        }

        private IWin32Window GetDialogOwner()
        {
            if (MainWindow != null)
                return MainWindow.DialogOwner;

            if (MainForm != null)
                return MainForm;

            // We really don't want the open file dialog to be non-modal. ATF 2.8 used Form.ActiveForm.
            return Form.ActiveForm;
        }

        private string GetInitialDirectory()
        {
            string path = ForcedInitialDirectory;
            if (!string.IsNullOrEmpty(path))
                return path;

            if (!HasRunBeforeOnUserAccount)
                return InitialDirectory;

            return null; //let the operating system decide
        }

        /// <summary>Utility function to convert WinForms DialogResult to the platform agnostic FileDialogResult</summary>
        /// <param name="result">DialogResult to convert to FileDialogResult</param>
        /// <returns>FileDialogResult corresponding to the argument value, or FileDialogResult.Cancel if no match is found</returns>
        private FileDialogResult DialogResultToFileDialogResult(DialogResult result)
        {
            switch (result)
            {
                case DialogResult.Yes:
                    return FileDialogResult.Yes;
                case DialogResult.No:
                    return FileDialogResult.No;
                case DialogResult.OK:
                    return FileDialogResult.OK;
                case DialogResult.Cancel:
                    return FileDialogResult.Cancel;
                default:
                    return FileDialogResult.Cancel;
            }
        }

        private string m_firstRunInitialDirectory;

        [Import(AllowDefault = true)]
        private ISettingsPathsProvider m_settingsPathProvider;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;
    }
}
