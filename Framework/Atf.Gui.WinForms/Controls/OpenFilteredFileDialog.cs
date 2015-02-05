using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;


namespace Sce.Atf.Controls
{
    /// <summary>
    /// This class behaves the same as the System.Windows.Forms.OpenFileDialog class, 
    /// but allows a custom file filter to be added to exclude files in the dialog's ListView.
    /// Use for importing assets, for example.</summary>
    public class OpenFilteredFileDialog : FilteredFileDialogBase
    {
        /// <summary>
        /// Constructor</summary>
        public OpenFilteredFileDialog()
            : base()
        {
            Text = "Open".Localize();
        }


        /// <summary>
        /// Gets the resulting selected file name(s)</summary>
        public string[] FileNames
        {
            get { return SelectedFileNames.ToArray(); }
        }

        /// <summary>
        /// Gets the resulting selected file name</summary>
        public string FileName
        {
            get { return SelectedFileNames.FirstOrDefault(); }
        }

        /// <summary>
        /// Gets or sets settings service</summary>
        public ISettingsService SettingsService { get; set; }

        /// <summary>
        /// Performs custom actions on System.Windows.Forms.Form.Load event</summary>
        /// <param name="e">Event args</param>
        protected override void OnLoad(EventArgs e)
        {
            // load settings
            if (SettingsService != null && !s_settingsRegistered)
            {
                SettingsService.RegisterSettings("9DC15B28-D9F3-4B05-BFBB-FF707E94CEEA",
                                                 new PropertyDescriptor[]
                                                     {
                                                         new BoundPropertyDescriptor(this,
                                                                                     () => LastSize, "Last Size", null,
                                                                                     "Last size for OpenFilteredFileDialog")
                                                         ,
                                                         new BoundPropertyDescriptor(this,
                                                                                     () => LastLocation, "Last Location",
                                                                                     null,
                                                                                     "Last location for OpenFilteredFileDialog"),
                                                         new BoundPropertyDescriptor(this,
                                                                                     () => LastColumnWidths, "Column Widths",
                                                                                     null,
                                                                                     "Column Widths for OpenFilteredFileDialog"),
                                                        new BoundPropertyDescriptor(this,
                                                                                     () => LastAccessedDirectory, "Last Accessed Directory",
                                                                                     null,
                                                                                     "Last Accessed Directory"),

                                                  });
                s_settingsRegistered = true;
            }

            if (!LastSize.IsEmpty)
            {
                Size = LastSize;
                Location = LastLocation;
                base.ColumnWidths = LastColumnWidths;
            }

            if (string.IsNullOrEmpty(InitialDirectory))
                InitialDirectory = LastAccessedDirectory;
            base.OnLoad(e);
        }

        /// <summary>
        /// Performs custom actions on System.Windows.Forms.Form.Closing event</summary>
        /// <param name="e">Cancel event args</param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            s_lastColumnWidths = base.ColumnWidths;

            if (SettingsService != null && s_settingsRegistered)
            {
                // save settings
                if (WindowState == FormWindowState.Normal)
                {
                    s_lastSize = Size;
                    s_lastLocation = Location;
                }
                else
                {
                    s_lastSize = RestoreBounds.Size;
                    s_lastLocation = RestoreBounds.Location;
                }

                if (SelectedFileNames.Any())
                {
                    LastAccessedDirectory = Path.GetDirectoryName(SelectedFileNames.First());
                }


            }
            base.OnClosing(e);
        }

        /// <summary>
        /// Gets or sets last size of dialog</summary>
        Size LastSize 
        {
            get { return s_lastSize; }
            set { s_lastSize = value; }
        }

        /// <summary>
        /// Gets or sets last dialog location, upper left corner</summary>
        Point LastLocation
        {
            get { return s_lastLocation; }
            set { s_lastLocation = value; }
        }

        /// <summary>
        /// Gets or sets last accessed directory</summary>
        string LastAccessedDirectory
        {
            get { return s_lastAccessedDirectory; }
            set { s_lastAccessedDirectory = value; }
        }

        /// <summary>
        /// Gets or sets last column widths</summary>
        int[] LastColumnWidths
        {
            get { return s_lastColumnWidths; }
            set { s_lastColumnWidths = value; }
        }

        static private Size  s_lastSize;
        static private Point s_lastLocation;
        static private int[] s_lastColumnWidths;
        static private string s_lastAccessedDirectory;
        static private bool  s_settingsRegistered;

    }
}
