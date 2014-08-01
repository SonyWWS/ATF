//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Implementation of a FilePicker dialog</summary>
    public class FilePicker : Control
    {
        /// <summary>
        /// Gets and sets the file path</summary>
        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        /// <summary>
        /// Dependency property for the file path</summary>
        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath",
                typeof(string), typeof(FilePicker),
                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Gets and sets the filter for allowable filename extensions</summary>
        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        /// <summary>
        /// Dependency property for the filter</summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter",
                typeof(string), typeof(FilePicker), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets and sets the default extension to use for the filename</summary>
        public string DefaultExtension
        {
            get { return (string)GetValue(DefaultExtensionProperty); }
            set { SetValue(DefaultExtensionProperty, value); }
        }

        /// <summary>
        /// Dependency property for the default extension</summary>
        public static readonly DependencyProperty DefaultExtensionProperty =
            DependencyProperty.Register("DefaultExtension",
                typeof(string), typeof(FilePicker), new UIPropertyMetadata(null));


        /// <summary>
        /// Gets and sets the element to focus when bringing up the dialog</summary>
        public FrameworkElement ElementToFocus
        {
            get { return (FrameworkElement)GetValue(ElementToFocusProperty); }
            set { SetValue(ElementToFocusProperty, value); }
        }

        /// <summary>
        /// Dependency property for the element to focus</summary>
        public static readonly DependencyProperty ElementToFocusProperty =
            DependencyProperty.Register("ElementToFocus", typeof(FrameworkElement),
            typeof(FilePicker), new UIPropertyMetadata(null));


        #region public IFileDialogService FileDialogService

        /// <summary>
        /// Gets and sets the FileDialogService to use for showing the file dialog</summary>
        public IFileDialogService FileDialogService
        {
            get { return (IFileDialogService)GetValue(FileDialogServiceProperty); }
            set { SetValue(FileDialogServiceProperty, value); }
        }

        /// <summary>
        /// Dependency property for the file dialog service</summary>
        public static readonly DependencyProperty FileDialogServiceProperty =
            DependencyProperty.Register("FileDialogService", 
                typeof(IFileDialogService), typeof(FilePicker), new UIPropertyMetadata(null));

        #endregion

        static FilePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilePicker), new FrameworkPropertyMetadata(typeof(FilePicker)));
        }

        /// <summary>
        /// Constructor</summary>
        public FilePicker()
        {
            BrowseCommand = new DelegateCommand(Browse);
        }

        /// <summary>
        /// Gets and sets the ICommand to initiate browsing for a file</summary>
        public ICommand BrowseCommand { get; set; }

        private void Browse()
        {
            var focusElement = ElementToFocus;
            if (focusElement != null)
            {
                focusElement.Focus();
                Keyboard.Focus(focusElement);
            }

            if (FileDialogService != null)
            {
                //FileDialogService
                //FileDialogService.FileName = FilePath;
                //FileDialogService.Filter = Filter;
                //FileDialogService.DefaultExtension = DefaultExtension;
                //if (FileDialog.Show())
                //    FilePath = FileDialog.FileName;
            }
            else
            {
                try
                {
                    var d = new OpenFileDialog {FileName = FilePath, Filter = Filter, DefaultExt = DefaultExtension};
                    if (d.ShowDialog() == true)
                    {
                        FilePath = d.FileName;
                    }
                }
                catch (ArgumentException ex)
                {
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                }
            }
        }
    }
}
