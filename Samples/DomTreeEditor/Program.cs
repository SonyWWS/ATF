//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Dom;

namespace DomTreeEditorSample
{
    /// <summary>
    /// DomTreeEditor is a sample editor that operates on simple user interface definition files.
    /// The UI data is hierarchical and DomTreeEditor displays it in a TreeControl.
    /// You can select and edit UI elements in the tree, and you can edit the properties on the selected elements.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-DOM-Tree-Editor-Sample. </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application</summary>
        [STAThread]
        static void Main()
        {
            // It's important to call these before starting the app; otherwise theming and bitmaps
            // may not render correctly.
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

                typeof(DocumentRegistry),               // central document registry with change notification
                typeof(FileDialogService),              // standard Windows file dialogs
                typeof(AutoDocumentService),            // opens documents from last session, or creates a new document, on startup
                typeof(RecentDocumentCommands),         // standard recent document commands in File menu
                typeof(StandardFileCommands),           // standard File menu commands for New, Open, Save, SaveAs, Close
                typeof(StandardFileExitCommand),        // standard File exit menu command
                typeof(TabbedControlSelector),          // enable ctrl-tab selection of documents and controls within the app
                typeof(MainWindowTitleService),         // tracks document changes and updates main form title
                typeof(HelpAboutCommand),               // Help -> About command

                typeof(ContextRegistry),                // central context registry with change notification
                typeof(StandardEditCommands),           // standard Edit menu commands for copy/paste
                typeof(StandardEditHistoryCommands),    // standard Edit menu commands for undo/redo
                typeof(StandardSelectionCommands),      // standard Edit menu selection commands

                typeof(PaletteService),                 // global palette, for drag/drop instancing
                typeof(HistoryLister),                  // vistual list of undo/redo stack
                typeof(PropertyEditor),                 // property grid for editing selected objects
                typeof(GridPropertyEditor),             // grid control for editing selected objects
                typeof(PropertyEditingCommands),        // commands for PropertyEditor and GridPropertyEditor
                typeof(CurveEditor),                    // edits curves using the CurveEditingControl

                typeof(SchemaLoader),                   // component that loads XML schema and sets up types
                typeof(Editor),                         // component that manages UI documents
                typeof(PaletteClient),                  // component that adds UI types to palette
                typeof(TreeLister),                     // component that displays the UI in a tree control
                typeof(DefaultTabCommands),             // provides the default commands related to document tab Controls
                typeof(DomExplorer),                    // component that gives diagnostic view of DOM
                typeof(PythonService),                  // scripting service for automated tests
                typeof(ScriptConsole),                  // provides a dockable command console for entering Python commands
                typeof(AtfScriptVariables),             // exposes common ATF services as script variables
                typeof(AutomationService)               // provides facilities to run an automated script using the .NET remoting service
                );

            // Set up the MEF container with these components
            var container = new CompositionContainer(catalog);

            // Configure the main Form with a ToolStripContainer so the CommandService can
            //  generate toolbars.
            var toolStripContainer = new ToolStripContainer();
            toolStripContainer.Dock = DockStyle.Fill;
            var mainForm = new MainForm(toolStripContainer)
            {
                Text = "Dom Tree Editor".Localize(),
                Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage))
            };

            // Add the main Form instance to the container
            var batch = new CompositionBatch();
            batch.AddPart(mainForm);
            batch.AddPart(new WebHelpCommands("https://github.com/SonyWWS/ATF/wiki/ATF-DOM-Tree-Editor-Sample".Localize()));
            container.Compose(batch);

            // Initialize components that require it. Initialization often can't be done in the constructor,
            //  or even after imports have been satisfied by MEF, since we allow circular dependencies between
            //  components, via the System.Lazy class. IInitializable allows components to defer some operations
            //  until all MEF composition has been completed.
            container.InitializeAll();

            // Example of programmatic layout creation:
            {
                var windowLayoutService = container.GetExportedValue<IWindowLayoutService>();
                var dockStateProvider = container.GetExportedValue<IDockStateProvider>();

                // Load custom XML and add it to the Window Layout Service
                int layoutNum = 0;
                Assembly assembly = Assembly.GetExecutingAssembly();
                foreach (string resourceName in assembly.GetManifestResourceNames())
                {
                    if (resourceName.Contains("Layout") && resourceName.EndsWith(".xml"))
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.Load(assembly.GetManifestResourceStream(resourceName));

                        layoutNum++;
                        windowLayoutService.SetOrAddLayout(dockStateProvider, "Programmatic Layout " + layoutNum, xmlDoc.InnerXml);
                    }
                }
            }

            // Show the main form and start message handling. The main Form Load event provides a final chance
            //  for components to perform initialization and configuration.
            Application.Run(mainForm);

            // Give components a chance to clean up.
            container.Dispose();
        }
    }
}
