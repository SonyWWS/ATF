//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Controls;

namespace CircuitEditorSample
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
            var richTextBox = new RichTextBox();
            richTextBox.BorderStyle = BorderStyle.None;
            richTextBox.ReadOnly = true;

            string languageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            string aboutFilePath;
            if (languageCode == "ja")
                aboutFilePath = "CircuitEditorSample.Resources.ja.About.rtf";
            else
                aboutFilePath = "CircuitEditorSample.Resources.About.rtf";
            
            Stream textFileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(aboutFilePath);
            if (textFileStream != null)
                richTextBox.LoadFile(textFileStream, RichTextBoxStreamType.RichText);

            const string appUrl = "https://github.com/SonyWWS/ATF/wiki";
            var dialog = new AboutDialog(
                "Circuit Editor Sample".Localize(), appUrl, richTextBox, null, null, true);
            dialog.ShowDialog();
        }
    }
}
