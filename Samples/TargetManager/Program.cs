//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Applications.NetworkTargetServices;

using HelpAboutCommand = Sce.Atf.Applications.HelpAboutCommand;

namespace TargetManager
{
    /// <summary>
    /// This is a target manager sample application.
    /// TargetManager shows how to use the TargetEnumerationService to discover, add, configure and select targets. 
    /// Targets are network endpoints, such as TCP/IP addresses, PS3 DevKits (to be added) or Vita DevKits. 
    /// TargetEnumerationService is implemented as a MEF component that supports the ITargetConsumer interface 
    /// for querying and enumerating targets. 
    /// It consumes target providers created by the application.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-Target-Manager-Sample.
    /// For a discussion of this sample's programming, see 
    /// https://github.com/SonyWWS/ATF/wiki/Target-Manager-Programming-Discussion. </summary>
    static class Program
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

            var catalog = new TypeCatalog(
                typeof(SettingsService),            // persistent settings and user preferences dialog
                typeof(FileDialogService),          // proivdes standard Windows file dialogs, to let the user open and close files. Used by SkinService.
                typeof(SkinService),                // allows for customization of an application’s appearance by using inheritable properties that can be applied at run-time
                typeof(StatusService),              // status bar at bottom of main Form
                typeof(CommandService),             // handles commands in menus and toolbars
                typeof(ControlHostService),         // docking control host

                typeof(StandardFileExitCommand),    // standard File exit menu command
                typeof(HelpAboutCommand),           // Help -> About command
                typeof(AtfUsageLogger),             // logs computer info to an ATF server
                typeof(CrashLogger),                // logs unhandled exceptions to an ATF server

                typeof(ContextRegistry),            // component that tracks application contexts; needed for context menu 

                // add target info plugins and TargetEnumerationService
                typeof(Deci4pTargetProvider),       // provides information about development devices available via Deci4p
                typeof(TcpIpTargetProvider),        // provides information about development devices available via TCP/IP
                typeof(TargetCommands),             // commands to operate on currently selected targets. 
                typeof(TargetEnumerationService)    // queries and enumerates target objects, consuming target providers created by the application
             );

            // Set up the MEF container with these components
            var container = new CompositionContainer(catalog);

            // Configure the main Form
            var batch = new CompositionBatch();
            var mainForm = new MainForm(new ToolStripContainer())
            {
                Text = "ATF TargetManager Sample".Localize(),
                Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage))
            };

            // Add the main Form instance to the container
            batch.AddPart(mainForm);
            batch.AddPart(new WebHelpCommands("https://github.com/SonyWWS/ATF/wiki/ATF-Target-Manager-Sample".Localize()));
            container.Compose(batch);

            var controlHostService = container.GetExportedValue<ControlHostService>();
            controlHostService.RegisteredCommands = ControlHostService.CommandRegister.None; // turn off standard window commands for simpele & single window UI 

            // Initialize components that require it. Initialization often can't be done in the constructor,
            //  or even after imports have been satisfied by MEF, since we allow circular dependencies between
            //  components, via the System.Lazy class. IInitializable allows components to defer some operations
            //  until all MEF composition has been completed.
            container.InitializeAll();

            // Show the main form and start message handling. The main Form Load event provides a final chance
            //  for components to perform initialization and configuration.
            Application.Run(mainForm);

            // Give components a chance to clean up.
            container.Dispose();
           
        }
    }
}
