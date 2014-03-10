//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// A simple class that launches a webpage when the user presses F1 on a particular Control
    /// (or otherwise causes the HelpRequested event to be raised).</summary>
    /// <example>
    /// m_control = new UserControl();
    /// m_control.AddHelp("https://github.com/SonyWWS/ATF/wiki/ATF-Circuit-Editor-Sample".Localize());
    /// // to open a local CHM file using hh.exe:
    /// m_control.AddHelp("hh MetricsViewer_Online_Help.chm::/MetricsViewer_UG/Projects.htm".Localize());
    /// </example>
    /// <remarks>
    /// If the Control is to be registered with the ControlHostService, then pass the help URL to it,
    /// either in a RegisterControl() extension method, or as a property of a ControlInfo object.</remarks>
    public class WebHelp
    {
        /// <summary>
        /// Constructor. Consider using the AddHelp extension method instead.</summary>
        /// <param name="control">The Control that this WebHelp object will be permanently attached to</param>
        public WebHelp(Control control)
        {
            control.HelpRequested += ControlHelpRequested;
        }

        /// <summary>
        /// Gets or sets the URL that will be opened by the default browser if the user presses F1. Setting
        /// to null or the empty string will cause nothing to happen when F1 is pressed. This string is passed
        /// directly to a process launcher, so it could be a local html file or a *.txt file or even an
        /// executable.</summary>
        /// <remarks>To open a CHM file, the syntax for running Microsoft's hh.exe is:
        /// hh help_name.chm::/path/topic.htm
        /// For examples and tips, see here: http://www.help-info.de/en/Help_Info_HTMLHelp/hh_command.htm
        /// To open a PDF file, see http://www.adobe.com/content/dam/Adobe/en/devnet/acrobat/pdfs/pdf_open_parameters.pdf
        /// </remarks>
        public string Url
        {
            get;
            set;
        }

        /// <summary>
        /// Opens the URL. This URL is opened automatically when the user presses F1.</summary>
        public void OpenUrl()
        {
            System.Diagnostics.Process.Start(Url);
        }

        internal static bool SupressHelpRequests;

        private void ControlHelpRequested(object sender, HelpEventArgs helpEvent)
        {
            if (!SupressHelpRequests && !string.IsNullOrEmpty(Url))
            {
                helpEvent.Handled = true;
                OpenUrl();
            }
        }
    }

    public static class WebHelps
    {
        /// <summary>
        /// Adds a listening object to the given Control so that the given URL is opened when the user presses
        /// F1 when that Control has focus.</summary>
        /// <param name="control">The control that a new WebHelp object will be permanently attached to</param>
        /// <param name="url">The default URL to be opened when the user presses F1. This string is passed
        /// directly to a process launcher, so it could be a local html file, PDF file, CHM file, or even an
        /// executable.</param>
        /// <returns>The new WebHelp object. Under most circumstances, there is no need to keep a reference
        /// to this new object. Exceptions are if the URL needs to change dynamically or if the URL needs to
        /// be opened manually without the user pressing F1.</returns>
        /// <remarks>Don't call this extension method multiple times for the same Control! If you need to change
        /// the URL dynamically, create a WebHelp object and set the URL property as needed.</remarks>
        /// <remarks>To open a CHM file, the syntax for running Microsoft's hh.exe is:
        /// hh help_name.chm::/path/topic.htm
        /// For examples and tips, see here: http://www.help-info.de/en/Help_Info_HTMLHelp/hh_command.htm
        /// To open a PDF file, see http://www.adobe.com/content/dam/Adobe/en/devnet/acrobat/pdfs/pdf_open_parameters.pdf
        /// </remarks>
        public static WebHelp AddHelp(this Control control, string url)
        {
            return new WebHelp(control) { Url = url };
        }
    }
}
