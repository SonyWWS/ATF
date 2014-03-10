//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Applications.WebServices;

namespace FileExplorerSample
{
    /// <summary>
    /// This is the FileExplorer sample application.
    /// FileExplorer is a sample viewer that displays the user's file hierarchy and files in a tree view control.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-File-Explorer-Sample.
    /// For a discussion of this sample's programming, see 
    /// https://github.com/SonyWWS/ATF/wiki/File-Explorer-Programming-Discussion. </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application</summary>
        [STAThread]
        static void Main() 
        {
            // important to call these before creating application host
            Application.EnableVisualStyles();
            Application.DoEvents(); // see http://www.codeproject.com/buglist/EnableVisualStylesBug.asp?df=100&forumid=25268&exp=0&select=984714

            // Set up localization support early on, so that user-readable strings will be localized
            //  during the initialization phase below. Use XML files that are embedded resources.
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CurrentCulture;
            Localizer.SetStringLocalizer(new EmbeddedResourceStringLocalizer());

            // Create a catalog with all the components that make up the application, except for
            //  our MainForm.
            var catalog = new TypeCatalog(
                typeof(SettingsService),                // persistent settings and user preferences dialog
                typeof(CommandService),                 // handles commands in menus and toolbars
                typeof(AtfUsageLogger),                 // logs computer info to an ATF server
                typeof(CrashLogger),                    // logs unhandled exceptions to an ATF server
                typeof(UnhandledExceptionService),      // catches unhandled exceptions, displays info, and gives user a chance to save
                typeof(StandardFileExitCommand),        // standard File exit menu command
                typeof(HelpAboutCommand),               // Help -> About command
                typeof(FolderViewer),                   // manages TreeControl to display folder hierarchy
                typeof(FileViewer),                     // managed ListView to display last selected folder contents

                typeof(NameDataExtension),              // extension to display file name
                typeof(SizeDataExtension),              // extension to display file size
                typeof(CreationTimeDataExtension),      // extension to display file creation time

                typeof(UserFeedbackService),            // component to send feedback form to SHIP
                typeof(VersionUpdateService),           // component to update to latest version on SHIP

                typeof(PythonService),                  // scripting service for automated tests
                typeof(ScriptConsole),                  // provides a dockable command console for entering Python commands
                typeof(AtfScriptVariables),             // exposes common ATF services as script variables
                typeof(AutomationService)               // provides facilities to run an automated script using the .NET remoting service
                );          

            var container = new CompositionContainer(catalog);

            // manually add the MainForm
            var batch = new CompositionBatch();
            var mainForm = new MainForm
            {
                Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage))
            };
            // our custom main Form with SplitContainer
            batch.AddPart(mainForm);
            batch.AddPart(new WebHelpCommands("https://github.com/SonyWWS/ATF/wiki/ATF-File-Explorer-Sample".Localize()));
            container.Compose(batch);

            // initialize all components which require it
            container.InitializeAll();
            
            Application.Run(mainForm);

            container.Dispose();
        }
    }
}