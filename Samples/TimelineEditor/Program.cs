//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace TimelineEditorSample
{
    /// <summary>
    /// This is a timeline editor sample application.
    /// This sample is a relatively full-featured timeline editor whose components have been used in real production tools. 
    /// It shows how to use MEF to put an application together using optional components, use of the application shell framework, 
    /// how to use the timeline manipulators and a manipulator architecture that allows adding or removing a 
    /// TimelineControl's functionality without modifying the TimelineControl, how to use the palette as a parts depot for timeline objects, 
    /// use of the property and grid property editors, sub-document support, how to enable opening multiple documents simultaneously, 
    /// and copy and paste within and between documents.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-Timeline-Editor-Sample. </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application</summary>
        [STAThread]
        static void Main()
        {
            // It's important to call these before starting the app; otherwise theming and bitmaps
            //  may not render correctly.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.DoEvents(); // see http://www.codeproject.com/buglist/EnableVisualStylesBug.asp?df=100&forumid=25268&exp=0&select=984714

            // Set up localization support early on, so that user-readable strings will be localized
            //  during the initialization phase below. Use XML files that are embedded resources.
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CurrentCulture;
            Localizer.SetStringLocalizer(new EmbeddedResourceStringLocalizer());

            // Register the embedded image resources so that they will be available for all users of ResourceUtil,
            //  such as the PaletteService.
            ResourceUtil.Register(typeof(Resources));

            // enable metadata driven property editing
            DomNodeType.BaseOfAllTypes.AddAdapterCreator(new AdapterCreator<CustomTypeDescriptorNodeAdapter>());

            var catalog = new TypeCatalog(
                typeof(SettingsService),                // persistent settings and user preferences dialog
                typeof(StatusService),                  // status bar at bottom of main Form
                typeof(LiveConnectService),             // allows easy interop between apps on same router subnet
                typeof(Outputs),                        // passes messages to all IOutputWriter components
                typeof(OutputService),                  // rich text box for displaying error and warning messages. Implements IOutputWriter
                typeof(CommandService),                 // handles commands in menus and toolbars
                typeof(ControlHostService),             // docking control host
                typeof(AtfUsageLogger),                 // logs computer info to an ATF server
                typeof(CrashLogger),                    // logs unhandled exceptions to an ATF server
                typeof(UnhandledExceptionService),      // catches unhandled exceptions, displays info, and gives user a chance to save
                typeof(FileDialogService),              // standard Windows file dialogs
                typeof(DocumentRegistry),               // central document registry with change notification
                typeof(AutoDocumentService),            // opens documents from last session, or creates a new document, on startup
                typeof(RecentDocumentCommands),         // standard recent document commands in File menu
                typeof(StandardFileCommands),           // standard File menu commands for New, Open, Save, SaveAs, Close
                typeof(StandardFileExitCommand),        // standard File exit menu command
                typeof(MainWindowTitleService),         // tracks document changes and updates main form title
                typeof(TabbedControlSelector),          // enable ctrl-tab selection of documents and controls within the app
                typeof(ContextRegistry),                // central context registry with change notification
                typeof(StandardEditCommands),           // standard Edit menu commands for copy/paste
                typeof(StandardEditHistoryCommands),    // standard Edit menu commands for undo/redo
                typeof(StandardSelectionCommands),      // standard Edit menu selection commands
                typeof(RenameCommand),                  // allows for renaming of multiple selected objects
                
                //StandardPrintCommands does not currently work with Direct2D
                //typeof(StandardPrintCommands),        // standard File menu print commands

                typeof(PaletteService),                 // global palette, for drag/drop instancing
                typeof(HistoryLister),                  // vistual list of undo/redo stack
                typeof(PropertyEditor),                 // property grid for editing selected objects
                typeof(GridPropertyEditor),             // grid control for editing selected objects
                typeof(PropertyEditingCommands),        // commands for PropertyEditor and GridPropertyEditor
                typeof(PerformanceMonitor),             // displays the frame rate and memory usage
                typeof(FileWatcherService),                // service to watch for changes to files
                typeof(DefaultTabCommands),             // provides the default commands related to document tab Controls
                typeof(SkinService),                    // allows for customization of an application’s appearance by using inheritable properties that can be applied at run-time
                
                // Client-specific plug-ins
                typeof(TimelineEditor),                 // timeline editor component
                typeof(TimelineCommands),               // defines Timeline-specific commands
                typeof(HelpAboutCommand),               // Help -> About command

                // Testing related
                typeof(PythonService),                  // scripting service for automated tests
                typeof(ScriptConsole),                  // provides a dockable command console for entering Python commands
                typeof(AtfScriptVariables),             // exposes common ATF services as script variables
                typeof(AutomationService)               // provides facilities to run an automated script using the .NET remoting service
                );
            
            var container = new CompositionContainer(catalog);

            var toolStripContainer = new ToolStripContainer();
            toolStripContainer.Dock = DockStyle.Fill;

            var mainForm = new MainForm(toolStripContainer)
            {
                Text = "Timeline Editor Sample".Localize(),
                Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage))
            };

            var batch = new CompositionBatch();
            batch.AddPart(mainForm);
            batch.AddPart(new WebHelpCommands("https://github.com/SonyWWS/ATF/wiki/ATF-Timeline-Editor-Sample".Localize()));

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