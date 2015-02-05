//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Controls;

namespace StatechartEditorSample
{
    /// <summary>
    /// Adds the Help/About command, which displays a dialog box with a description
    /// of the application (specified by a derived class) plus the ATF version number</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(HelpAboutCommand))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class HelpAboutCommand : Sce.Atf.Applications.HelpAboutCommand
    {
        /// <summary>
        /// Shows the About dialog box</summary>
        /// <remarks>Derived classes should copy this method and customize it.
        /// It is recommended to display the ATF version number to aid in diagnosing
        /// problems. (Pass in 'true' to AboutDialog's constructor or use AtfVersion.)</remarks>
        protected override void ShowHelpAbout()
        {
            RichTextBox richTextBox = new RichTextBox();
            richTextBox.BorderStyle = BorderStyle.None;
            richTextBox.ReadOnly = true;
            Stream textFileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "StatechartEditorSample.Resources.About.rtf");
            richTextBox.LoadFile(textFileStream, RichTextBoxStreamType.RichText);

            string appURL = "https://github.com/SonyWWS/ATF/wiki";

            AboutDialog dialog = new AboutDialog(
                "Statechart Editor".Localize(), appURL, richTextBox, null, null, true);
            dialog.ShowDialog();
        }
    }
}
