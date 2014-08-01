//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Models;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Standard file dialog service, using ATF OpenFileDialog, SaveFileDialog and
    /// ConfirmationDialog classes. Use this component to provide file dialogs for an
    /// application. An implementation of IFileDialogService is required by
    /// StandardFileCommands to implement an application's standard File menu commands,
    /// File/New, File/Open, File/Save, File/Save As, and File/Close.</summary>
    [Export(typeof(IFileDialogService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FileDialogService : IFileDialogService
    {
        #region IFileDialogService Members

        /// <summary>
        /// Gets and sets a string that MAY be used to set the initial directory that the user sees. The exact
        /// behavior depends on the operating system and whether or not a path is in the pathName parameter.
        /// By default, and if this property is set to null, returns the user's "MyDocuments" folder.
        /// See http://msdn.microsoft.com/en-us/library/ms646839(VS.85).aspx</summary>
        public string InitialDirectory
        {
            get
            {
                if (m_initialDirectory == null)
                    m_initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return m_initialDirectory;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) &&
                    Directory.Exists(value))
                {
                    m_initialDirectory = value;
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
            var dialog = new OpenFileDialog();
            dialog.Filter = filter;
            dialog.RestoreDirectory = true;
            dialog.InitialDirectory = m_initialDirectory;

            var result = dialog.ShowCommonDialogWorkaround();
            if (result == true)
                pathName = dialog.FileName;

            return ToFileDialogResult(result);
        }

        /// <summary>
        /// Gets multiple file names for file "Open" operation</summary>
        /// <param name="pathNames">File names</param>
        /// <param name="filter">File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</param>
        /// <returns>true iff operation is not cancelled</returns>
        public FileDialogResult OpenFileNames(ref string[] pathNames, string filter)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = filter;
            dialog.RestoreDirectory = true;
            dialog.Multiselect = true;
            dialog.InitialDirectory = m_initialDirectory;

            var result = dialog.ShowCommonDialogWorkaround();
            if (result == true)
                pathNames = dialog.FileNames;

            return ToFileDialogResult(result);
        }

        /// <summary>
        /// Get file name for file "Save" operation</summary>
        /// <param name="pathName">File name</param>
        /// <param name="filter">File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</param>
        /// <returns>Dialog result</returns>
        public FileDialogResult SaveFileName(ref string pathName, string filter)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = filter;
            dialog.RestoreDirectory = true;
            dialog.FileName = pathName;
            dialog.InitialDirectory = m_initialDirectory;

            var result = dialog.ShowDialog(Application.Current.MainWindow);
            if (result == true)
                pathName = dialog.FileName;

            return ToFileDialogResult(result);
        }

        /// <summary>
        /// Confirm that file should be closed</summary>
        /// <param name="message">Confirmation message</param>
        /// <returns>Dialog result</returns>
        public FileDialogResult ConfirmFileClose(string message)
        {
            var vm = new ConfirmationDialogViewModel("Close".Localize("Close file"), message)
            {
                YesButtonText = "Save".Localize(), NoButtonText = "Discard".Localize()
            };

            vm.ShowDialog();

            return ToFileDialogResult(vm.Result);
        }

        /// <summary>
        /// Returns a value indicating if the file path exists</summary>
        /// <param name="pathName">File path</param>
        /// <returns>true if the file path exists</returns>
        public bool PathExists(string pathName)
        {
            return File.Exists(pathName);
        }

        #endregion

        private static FileDialogResult ToFileDialogResult(MessageBoxResult result)
        {
            switch (result)
            {
                case MessageBoxResult.Yes:
                    return FileDialogResult.Yes;
                case MessageBoxResult.No:
                    return FileDialogResult.No;
                case MessageBoxResult.OK:
                    return FileDialogResult.OK;
                default:
                    return FileDialogResult.Cancel;
            }
        }

        private static FileDialogResult ToFileDialogResult(bool? result)
        {
            return result == true ? FileDialogResult.OK : FileDialogResult.Cancel;
        }

        private string m_initialDirectory;
    }
}
