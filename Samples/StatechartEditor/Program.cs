﻿//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Dom;

namespace StatechartEditorSample
{
    /// <summary>
    /// This sample is an editor for statecharts. 
    /// It uses an XML schema to define the data file format, reads and writes XML statechart files, 
    /// and allows them to be edited using a graphical representation of states and transitions. 
    /// It uses the AdaptableControl to display and edit the statechart. 
    /// Annotations, which are comments pasted on to the document canvas, can be added. 
    /// Multiple documents can be edited simultaneously. 
    /// Many ATF Editor components are used to implement standard editing commands. 
    /// StatechartEditor also demonstrates prototyping: how the user can create a 
    /// custom set of statechart fragments that can be inserted into documents. 
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-State-Chart-Editor-Sample. </summary>
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

                typeof(SingleInstanceService),          // enforces the single-instance constraint on a GUI application

                typeof(SettingsService),                // persistent settings and user preferences dialog
                typeof(StatusService),                  // status bar at bottom of main Form
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
                typeof(MainWindowTitleService),         // tracks document changes and updates main form title
                typeof(TabbedControlSelector),          // enable ctrl-tab selection of documents and controls within the app

                typeof(ContextRegistry),                // central context registry with change notification
                typeof(StandardFileExitCommand),        // standard File exit menu command
                typeof(StandardEditCommands),           // standard Edit menu commands for copy/paste
                typeof(StandardEditHistoryCommands),    // standard Edit menu commands for undo/redo
                typeof(StandardSelectionCommands),      // standard Edit menu selection commands
                typeof(StandardLockCommands),           // standard Edit menu lock/unlock commands
                typeof(StandardLayoutCommands),         // standard Format menu layout commands
                typeof(StandardViewCommands),           // standard View menu commands
                
                //StandardPrintCommands does not currently work with Direct2D
                //typeof(StandardPrintCommands),        // standard File menu print commands

                typeof(HelpAboutCommand),               // Help -> About command

                typeof(PaletteService),                 // global palette, for drag/drop instancing
                typeof(HistoryLister),                  // visual list of undo/redo stack
                typeof(PropertyEditor),                 // property grid for editing selected objects
                typeof(GridPropertyEditor),             // grid control for editing selected objects
                typeof(PropertyEditingCommands),        // commands for PropertyEditor and GridPropertyEditor, like Reset,
                                                        //  Reset All, Copy Value, Paste Value, Copy All, Paste All

                typeof(PrototypeLister),                // editable palette of instantiable item groups

                typeof(Outputs),                        // service that provides static methods for writing to IOutputWriter objects.
                typeof(ErrorDialogService),             // displays errors to the user in a message box. Implements IOutputWriter.
                typeof(OutputService),                  // rich text box for displaying error and warning messages. Implements IOutputWriter.

                typeof(Editor),                         // editor which manages statechart documents and controls
                typeof(PaletteClient),                  // component which adds items to palette
                typeof(DefaultTabCommands),             // provides the default commands related to document tab Controls
                typeof(SchemaLoader),                   // loads schema and extends types
                typeof(PythonService),                  // scripting service for automated tests
                typeof(ScriptConsole),                  // provides a dockable command console for entering Python commands
                typeof(AtfScriptVariables),             // exposes common ATF services as script variables
                typeof(AutomationService)               // provides facilities to run an automated script using the .NET remoting service
                );                  

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

            // Add the main Form instance to the container
            batch.AddPart(mainForm);
            batch.AddPart(new WebHelpCommands("https://github.com/SonyWWS/ATF/wiki/ATF-State-Chart-Editor-Sample".Localize()));
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
