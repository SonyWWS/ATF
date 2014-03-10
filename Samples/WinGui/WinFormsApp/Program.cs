using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

using WinGuiCommon;

namespace WinFormsApp
{
    /// <summary>
    /// This is a basic WinForms sample application.
    /// It illustrates how to compose a WinForms application with ATF components using MEF.
    /// It is a starting point for an application, such as an editor, though it does not offer any editing capabilities.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-Win-Forms-App-Sample.
    /// For a discussion of this sample's programming, see 
    /// https://github.com/SonyWWS/ATF/wiki/WinForms-and-WPF-Apps-Programming-Discussion. </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application</summary>
        [STAThread]
        static void Main()
        {
            // Important to call these before starting the app.  Otherwise theming and bitmaps may not render correctly.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.DoEvents(); // see http://www.codeproject.com/buglist/EnableVisualStylesBug.asp?df=100&forumid=25268&exp=0&select=984714

            // Set up localization support early on, so that user-readable strings will be localized
            //  during the initialization phase below. Use XML files that are embedded resources.
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CurrentCulture;
            Localizer.SetStringLocalizer(new EmbeddedResourceStringLocalizer());

            // Using MEF, declare the composable parts that will make up this application
            TypeCatalog catalog = new TypeCatalog(
                typeof(SettingsService),                // persistent settings and user preferences dialog
                typeof(CommandService),                 // handles commands in menus and toolbars
                typeof(ControlHostService),             // docking control host
                typeof(ContextRegistry),                // central context registry with change notification
                typeof(StandardFileExitCommand),        // standard File exit menu command

                typeof(FileDialogService),              // standard Windows file dialogs
                typeof(DocumentRegistry),               // central document registry with change notification
                typeof(StandardFileCommands),           // standard File menu commands for New, Open, Save, SaveAs, Close
                typeof(AutoDocumentService),            // opens documents from last session, or creates a new document, on startup
                typeof(RecentDocumentCommands),         // standard recent document commands in File menu
                typeof(MainWindowTitleService),         // tracks document changes and updates main form title
                typeof(AtfUsageLogger),                 // logs computer info to an ATF server
                typeof(WindowLayoutService),            // service to allow multiple window layouts
                typeof(WindowLayoutServiceCommands),    // command layer to allow easy switching between and managing of window layouts
                
                // Client-specific plug-ins
                typeof(Editor),                         // editor class component that creates and saves application documents
                typeof(SchemaLoader),                   // loads schema and extends types

                typeof(PythonService),                  // scripting service for automated tests
                typeof(ScriptConsole),                  // provides a dockable command console for entering Python commands
                typeof(AtfScriptVariables),             // exposes common ATF services as script variables
                typeof(AutomationService)               // provides facilities to run an automated script using the .NET remoting service
                );

            // Create the MEF container for the composable parts
            CompositionContainer container = new CompositionContainer(catalog);

            // Create the main form, give it a toolstrip
            ToolStripContainer toolStripContainer = new ToolStripContainer();
            toolStripContainer.Dock = DockStyle.Fill;
            MainForm mainForm = new MainForm(toolStripContainer)
            {
                Text = "Sample Application".Localize(),
                Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage))
            };

            // Create an MEF composable part from the main form, and add into container
            CompositionBatch batch = new CompositionBatch();
            AttributedModelServices.AddPart(batch, mainForm);
            container.Compose(batch);

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
