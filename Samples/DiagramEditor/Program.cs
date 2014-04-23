//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using System.Windows.Forms;
using CircuitEditorSample;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;
using GroupingCommands = Sce.Atf.Controls.Adaptable.Graphs.GroupingCommands;
using LayeringCommands = Sce.Atf.Controls.Adaptable.Graphs.LayeringCommands;

namespace DiagramEditorSample
{
    /// <summary>
    /// This is a diagram editor sample application.
    /// DiagramEditor combines the Circuit, FSM, and Statechart editor samples in a single application
    /// to show how multiple editors can share an application shell.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-Diagram-Editor-Sample. </summary>
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

            // Enable metadata driven property editing for the DOM
            DomNodeType.BaseOfAllTypes.AddAdapterCreator(new AdapterCreator<CustomTypeDescriptorNodeAdapter>());

            // Create a type catalog with the types of components we want in the application
            TypeCatalog catalog = new TypeCatalog(

                typeof(SettingsService),                // persistent settings and user preferences dialog
                typeof(StatusService),                  // status bar at bottom of main Form
                typeof(CommandService),                 // handles commands in menus and toolbars
                typeof(ControlHostService),             // docking control host
                typeof(WindowLayoutService),            // multiple window layout support
                typeof(WindowLayoutServiceCommands),    // window layout commands
                typeof(AtfUsageLogger),                 // logs computer info to an ATF server
                typeof(CrashLogger),                    // logs unhandled exceptions to an ATF server
                typeof(UnhandledExceptionService),      // catches unhandled exceptions, displays info, and gives user a chance to save
                typeof(FileDialogService),              // standard Windows file dialogs

                typeof(DocumentRegistry),               // central document registry with change notification
                typeof(AutoDocumentService),            // opens documents from last session, or creates a new document, on startup
                typeof(RecentDocumentCommands),         // standard recent document commands in File menu
                typeof(StandardFileCommands),           // standard File menu commands for New, Open, Save, SaveAs, Close
                typeof(MainWindowTitleService),         // tracks document changes and updates main form title
                typeof(TabbedControlSelector),          // enable ctrl-tab selection of documents and controls within the app

                typeof(ContextRegistry),                // central context registry with change notification
                typeof(StandardFileExitCommand),        // standard File exit menu command
                typeof(StandardEditCommands),           // standard Edit menu commands for copy/paste
                typeof(StandardEditHistoryCommands),    // standard Edit menu commands for undo/redo
                typeof(StandardSelectionCommands),      // standard Edit menu selection commands
                typeof(StandardLayoutCommands),         // standard Format menu layout commands
                typeof(StandardViewCommands),           // standard View menu commands
                
                //StandardPrintCommands does not currently work with Direct2D
                //typeof(StandardPrintCommands),        // standard File menu print commands

                typeof(HelpAboutCommand),               // Help -> About command

                typeof(PaletteService),                 // global palette, for drag/drop instancing

                typeof(PropertyEditor),                 // property grid for editing selected objects
                typeof(GridPropertyEditor),             // grid control for editing selected objects
                typeof(PropertyEditingCommands),        // commands for PropertyEditor and GridPropertyEditor

                typeof(HistoryLister),                  // vistual list of undo/redo stack
                typeof(PrototypeLister),                // editable palette of instantiable item groups
                typeof(LayerLister),                    // editable tree view of layers

                typeof(Outputs),                        // passes messages to all log writers
                typeof(ErrorDialogService),             // displays errors to the user a in message box

                typeof(DiagramTheme),                   // rendering theme for diagrams
                typeof(DefaultTabCommands),             // provides the default commands related to document tab Controls

                // Editors
                typeof(StatechartEditorSample.Editor),          // sample statechart editor
                typeof(StatechartEditorSample.SchemaLoader),    // loads statechart schema and extends types
                typeof(StatechartEditorSample.PaletteClient),   // component which adds palette items

                typeof(CircuitEditorSample.Editor),             // sample circuit editor
                typeof(CircuitEditorSample.SchemaLoader),       // loads circuit schema and extends types
                typeof(GroupingCommands),                       // circuit group/ungroup commands
                typeof(CircuitControlRegistry),                 // circuit controls management
                typeof(MasteringCommands),                      // circuit master/unmaster commands
                typeof(CircuitEditorSample.ModulePlugin),       // component that defines circuit module types
                typeof(LayeringCommands),                       // "Add Layer" context menu command for the Layer Lister

                typeof(FsmEditorSample.Editor),                 // editor which manages FSM documents and controls
                typeof(FsmEditorSample.PaletteClient),          // component which adds palette items
                typeof(FsmEditorSample.SchemaLoader),           // loads FSM schema and extends types

                typeof(PythonService),                          // scripting service for automated tests
                typeof(ScriptConsole),                          // provides a dockable command console for entering Python commands
                typeof(AtfScriptVariables),                     // exposes common ATF services as script variables
                typeof(AutomationService)                       // provides facilities to run an automated script using the .NET remoting service
                );

            StandardEditCommands.UseSystemClipboard = true;

            // Set up the MEF container with these components
            var container = new CompositionContainer(catalog);

            // Configure the main Form
            var batch = new CompositionBatch();
            var mainForm = new MainForm(new ToolStripContainer())
            {
                Text = "Diagram Editor".Localize(),
                Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage))
            };
            Sce.Atf.Direct2D.D2dFactory.EnableResourceSharing(mainForm.Handle);

            // Add the main Form instance to the container
            batch.AddPart(mainForm);
            batch.AddPart(new WebHelpCommands("https://github.com/SonyWWS/ATF/wiki/ATF-Diagram-Editor-Sample".Localize()));
            container.Compose(batch);

            // Initialize components that require it. Initialization often can't be done in the constructor,
            //  or even after imports have been satisfied by MEF, since we allow circular dependencies between
            //  components, via the System.Lazy class. IInitializable allows components to defer some operations
            //  until all MEF composition has been completed.
            container.InitializeAll();
            InitializeScriptingVariables(container);

            // Show the main form and start message handling. The main Form Load event provides a final chance
            //  for components to perform initialization and configuration.
            Application.Run(mainForm);

            // Give components a chance to clean up.
            container.Dispose();
        }

        /// <summary>
        /// Since this application is really just a wrapper around 3 other editors, the scripting service
        /// needs to do some special handling for commonly named variables. </summary>
        /// <param name="container"></param>
        private static void InitializeScriptingVariables(CompositionContainer container)
        {
            try
            {
                ScriptingService scriptingService = container.GetExportedValue<ScriptingService>();
                if (scriptingService != null)
                {
                    scriptingService.RemoveVariable("editor");
                    StatechartEditorSample.Editor stateChartEditor = container.GetExportedValue<StatechartEditorSample.Editor>();
                    if (stateChartEditor != null)
                    {
                        scriptingService.SetVariable("stateChartEditor", stateChartEditor);
                    }
                    CircuitEditorSample.Editor circuitEditor = container.GetExportedValue<CircuitEditorSample.Editor>();
                    if (circuitEditor != null)
                    {
                        scriptingService.SetVariable("circuitEditor", circuitEditor);
                    }
                    FsmEditorSample.Editor fsmEditor = container.GetExportedValue<FsmEditorSample.Editor>();
                    if (fsmEditor != null)
                    {
                        scriptingService.SetVariable("fsmEditor", fsmEditor);
                    }

                    IContextRegistry contextRegistry = container.GetExportedValue<IContextRegistry>();
                    if (contextRegistry != null)
                    {
                        contextRegistry.ActiveContextChanged += delegate
                            {
                                //Note this assumes this is the last ActiveContextChanged listener to be called.
                                //Each of the Circuit/Fsm/StatechartEditors also set these variables
                                //(2 out of 3 will set them to null), so it is important this is the last listener invoked.
                                EditingContext editingContext = contextRegistry.ActiveContext.As<EditingContext>();
                                scriptingService.SetVariable("editingContext", editingContext);
                                IViewingContext viewContext = contextRegistry.GetActiveContext<IViewingContext>();
                                scriptingService.SetVariable("view", viewContext);
                            };
                    }
                }
            }
            catch { }
        }
    }
}
