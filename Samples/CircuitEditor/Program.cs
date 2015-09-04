//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using CircuitEditorSample.Tests;
using PropertyGrid = Sce.Atf.Controls.PropertyEditing.PropertyGrid;

namespace CircuitEditorSample
{
    /// <summary>
    /// CircuitEditor is a sample editor for circuits, consisting of modules with input and output pins and connections between them. 
    /// It uses an XML Schema to define the data file format, reads and writes XML circuit files, 
    /// and allows circuits to be edited using a graphical representation of modules and connections. 
    /// It uses the AdaptableControl to display and edit the circuit. Multiple documents can be edited simultaneously. 
    /// CircuitEditor uses several ATF Editor components to implement standard editing commands. CircuitEditor also demonstrates:
    /// * Prototyping: you can create a custom set of circuit fragments that can be inserted into documents.
    /// * Layering: you can assign diagram components to layers that can be turned on and off (visible and invisible).
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-Circuit-Editor-Sample. </summary>
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
            var catalog = new TypeCatalog(

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
                typeof(HelpAboutCommand),               // Help -> About command

                typeof(ContextRegistry),                // central context registry with change notification
                typeof(StandardFileExitCommand),        // standard File exit menu command
                typeof(StandardEditCommands),           // standard Edit menu commands for copy/paste
                typeof(StandardEditHistoryCommands),    // standard Edit menu commands for undo/redo
                typeof(StandardSelectionCommands),      // standard Edit menu selection commands
                typeof(StandardLayoutCommands),         // standard Format menu layout commands
                typeof(StandardViewCommands),           // standard View menu commands

                //StandardPrintCommands does not currently work with Direct2D
                //typeof(StandardPrintCommands),        // standard File menu print commands

                typeof(PaletteService),                 // global palette, for drag/drop instancing

                typeof(MyPropertyEditor),               // property grid for editing selected objects; uses tooltips to show property descriptions
                typeof(GridPropertyEditor),             // grid control for editing selected objects
                typeof(PropertyEditingCommands),        // commands for PropertyEditor and GridPropertyEditor, like Reset,
                                                        //  Reset All, Copy Value, Paste Value, Copy All, Paste All

                typeof(HistoryLister),                  // visual list of undo/redo stack
                typeof(PrototypeLister),                // editable palette of instantiable item groups
                typeof(LayerLister),                    // editable tree view of layers

                typeof(Outputs),                        // passes messages to all log writers
                // typeof(ErrorDialogService),             // displays errors to the user in a message box
                typeof(OutputService),                  // rich text box for displaying error and warning messages. Implements IOutputWriter.
                typeof(DomRecorder),                    // records and displays changes to the DOM for diagnostic purposes

                typeof(Editor),                         // editor which manages circuit documents and controls
                typeof(SchemaLoader),                   // loads circuit schema and extends types
                typeof(GroupingCommands),               // circuit group/ungroup commands
                typeof(CircuitControlRegistry),         // circuit controls management
                typeof(LayeringCommands),               // "Add Layer" command
                typeof(GraphViewCommands),              // zooming with presets
                typeof(PerformanceMonitor),             // displays the frame rate and memory usage
                typeof(DefaultTabCommands),             // provides the default commands related to document tab Controls
                typeof(ModulePlugin),                   // component that defines circuit module types
                typeof(TemplateLister),                 // template library for subgraph referencing or instancing 
                typeof(TemplatingCommands),             // commands for promoting/demoting graph elements to/from template library 
                //typeof(TemplatingSupervisor),         // templated instances copy-on-edit support(optionally)

                typeof(AnnotatingCommands),             // annotating commands
                typeof(DynamicPropertyCommands),           // context commands for user-defined properties in the property editors
                typeof(CircuitTestCommands),            // circuit tester commands

                typeof(PythonService),                  // scripting service for automated tests
                typeof(ScriptConsole),                  // provides a dockable command console for entering Python commands
                typeof(AtfScriptVariables),             // exposes common ATF services as script variables
                typeof(AutomationService)               // provides facilities to run an automated script using the .NET remoting service
            );

            // enable use of the system clipboard
            StandardEditCommands.UseSystemClipboard = true;

            // Set up the MEF container with these components
            var container = new CompositionContainer(catalog);

            // Configure the main Form
            var batch = new CompositionBatch();
            var mainForm = new MainForm(new ToolStripContainer())
            {
                Text = Application.ProductName,
                Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage))
            };
            Sce.Atf.Direct2D.D2dFactory.EnableResourceSharing(mainForm.Handle);

            // Add the main Form instance, etc., to the container
            batch.AddPart(mainForm);
            batch.AddPart(new WebHelpCommands("https://github.com/SonyWWS/ATF/wiki/ATF-Circuit-Editor-Sample".Localize()));
            container.Compose(batch);

            // Add a customized category comparer to the object palette.
            var paletteService = container.GetExportedValue<PaletteService>();
            paletteService.CategoryComparer = new CategoryComparer();

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

        // Demonstrates the PaletteService.CategoryComparer feature.
        private class CategoryComparer : IComparer<string>
        {
            // returns -1, 0, or 1, if x is before, the same as, or after y.
            public int Compare(string x, string y)
            {
                // If both categories are the same (including if they are both s_lastCategory), return 0.
                if (x == y)
                    return 0;

                // Place this category last.
                if (x == s_lastCategory)
                    return 1;
                if (y == s_lastCategory)
                    return -1;

                // Otherwise, sort in alphabetical order.
                return string.Compare(x, y, StringComparison.CurrentCultureIgnoreCase);
            }

            private static readonly string s_lastCategory = "Misc".Localize("abbreviation for miscellaneous");
        }

        [Export(typeof(IInitializable))]
        [Export(typeof(IControlHostClient))]
        [Export(typeof(PropertyEditor))]
        [PartCreationPolicy(CreationPolicy.Any)]
        // Demonstrates using tooltips instead of an embedded Control to display property descriptions.
        private class MyPropertyEditor : PropertyEditor
        {
            /// <summary>
            /// Constructor with parameters</summary>
            /// <param name="commandService">ICommandService</param>
            /// <param name="controlHostService">IControlHostService</param>
            /// <param name="contextRegistry">IContextRegistry</param>
            [ImportingConstructor]
            public MyPropertyEditor(
                ICommandService commandService,
                IControlHostService controlHostService,
                IContextRegistry contextRegistry)
                : base(commandService, controlHostService, contextRegistry)
            {
            }

            protected override void Configure(out PropertyGrid propertyGrid, out ControlInfo controlInfo)
            {
                // Test that DisplayTooltips works instead of the usual DisplayDescriptions.
                propertyGrid = new PropertyGrid(PropertyGridMode.DisplayTooltips, new PropertyGridView());
                controlInfo = new ControlInfo(
                    "Property Editor".Localize(),
                    "Edits selected object properties".Localize(),
                    StandardControlGroup.Right, null,
                    "https://github.com/SonyWWS/ATF/wiki/Property-Editing-in-ATF".Localize());
            }
        }
    }
}
