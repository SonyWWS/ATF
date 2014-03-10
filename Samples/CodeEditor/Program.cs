//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Perforce;

namespace CodeEditor
{
    /// <summary>
    /// This is a code editor sample application.
    /// This editor uses the ActiproSoftare SyntaxEditor to provide an editing Control.
    /// It provides language syntax sensitive editing for plain text, C#, Lua, Squirrel,
    /// Python, XML, COLLADA and Cg files.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-Code-Editor-Sample. </summary>
    class Program
    {
        /// <summary>
        /// The main entry point for the application</summary>
        [STAThread]
        static void Main(string[] args)
        {
            // important to call these before creating application host
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.DoEvents(); // see http://www.codeproject.com/buglist/EnableVisualStylesBug.asp?df=100&forumid=25268&exp=0&select=984714

            // Set up localization support early on, so that user-readable strings will be localized
            //  during the initialization phase below. Use XML files that are embedded resources.
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CurrentCulture;
            Localizer.SetStringLocalizer(new EmbeddedResourceStringLocalizer());

            var catalog = new TypeCatalog(
                typeof(SettingsService),                // persistent settings and user preferences dialog
                typeof(StatusService),                  // status bar at bottom of main Form
                typeof(CommandService),                 // handles commands in menus and toolbars
                typeof(ControlHostService),             // docking control host
                typeof(WindowLayoutService),            // multiple window layout support
                typeof(WindowLayoutServiceCommands),    // window layout commands
                typeof(FileDialogService),              // standard Windows file dialogs
                typeof(AutoDocumentService),            // opens documents from last session, or creates a new document, on startup
                typeof(Outputs),                        // service that provides static methods for writing to IOutputWriter objects.
                typeof(OutputService),                  // rich text box for displaying error and warning messages. Implements IOutputWriter.
                typeof(RecentDocumentCommands),         // standard recent document commands in File menu
                typeof(StandardFileCommands),           // standard File menu commands for New, Open, Save, SaveAs, Close
                typeof(StandardFileExitCommand),        // standard File exit menu command
                typeof(HelpAboutCommand),               // Help -> About command
                typeof(AtfUsageLogger),                 // logs computer info to an ATF server
                typeof(CrashLogger),                    // logs unhandled exceptions to an ATF server
                typeof(ContextRegistry),                // central context registry with change notification
                typeof(DocumentRegistry),               // central document registry with change notification
                typeof(MainWindowTitleService),         // tracks document changes and updates main form title
                typeof(TabbedControlSelector),          // enable ctrl-tab selection of documents and controls within the app
                typeof(DefaultTabCommands),             // provides the default commands related to document tab Controls
                typeof(Editor),                         // code editor component
                typeof(PythonService),                  // scripting service for automated tests
                typeof(ScriptConsole),                  // provides a dockable command console for entering Python commands
                typeof(AtfScriptVariables),             // exposes common ATF services as script variables
                typeof(AutomationService),              // provides facilities to run an automated script using the .NET remoting service
                typeof(PerforceService),                // Perforce plugin
                typeof(SourceControlCommands),          // source control commmands to interact with Perforce plugin
                typeof(SourceControlContext)            // source control context component
                );

            var container = new CompositionContainer(catalog);

            var toolStripContainer = new ToolStripContainer();
            toolStripContainer.Dock = DockStyle.Fill;

            var mainForm = new MainForm(toolStripContainer);
            var image = GdiUtil.GetImage("CodeEditor.Resources.File_edit.ico");
            mainForm.Icon = GdiUtil.CreateIcon(image, 32, true);

            mainForm.Text = "Code Editor".Localize();

            var batch = new CompositionBatch();
            batch.AddPart(mainForm);
            batch.AddPart(new WebHelpCommands("https://github.com/SonyWWS/ATF/wiki/ATF-Code-Editor-Sample".Localize()));
            container.Compose(batch);

            // Initialize components that require it. Initialization often can't be done in the constructor,
            //  or even after imports have been satisfied by MEF, since we allow circular dependencies between
            //  components, via the System.Lazy class. IInitializable allows components to defer some operations
            //  until all MEF composition has been completed.
            container.InitializeAll();
            
            // Show the main form and start message handling. The main Form Load event provides a final chance
            //  for components to perform initialization and configuration.
            Application.Run(mainForm);

            container.Dispose();
        }
    }
}
