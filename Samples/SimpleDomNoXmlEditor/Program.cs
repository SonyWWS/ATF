//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

namespace SimpleDomNoXmlEditorSample
{
    /// <summary>
    /// This is a simple DOM editor sample application. 
    /// It is very similar to the SimpleDomEditor sample, except that it does not use XML.
    /// It does not define DOM types with a schema file.
    /// This sample editor demonstrates the use of the DOM (Document Object Model), including defining a data model; 
    /// how to implement IDocumentClient and use the document framework to manage multiple documents; implement File menu items and more; 
    /// how to implement a UI parts palette; how to display editable lists of events and resources; how to adapt data to a list; 
    /// how to use ContextRegistry to track the active editing context; 
    /// how to adapt data so that ATF command components can be used to get undo/redo, cut/paste, and selection commands; 
    /// how to enable property editing on selected UI elements; and how to implement a standard Help/About dialog.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-Simple-DOM-No-XML-Editor-Sample.
    /// For a discussion of this sample's programming, see 
    /// https://github.com/SonyWWS/ATF/wiki/Simple-DOM-No-XML-Editor-Programming-Discussion. </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application</summary>
        [STAThread]
        static void Main(string[] args)
        {
            // important to call these before creating application host
            Application.EnableVisualStyles();
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
                //typeof(StandardLockCommands),           // standard Edit menu lock/unlock commands
                typeof(HelpAboutCommand),               // Help -> About command

                typeof(PaletteService),                 // global palette, for drag/drop instancing
                typeof(HistoryLister),                  // vistual list of undo/redo stack
                typeof(PropertyEditor),                 // property grid for editing selected objects
                typeof(GridPropertyEditor),             // grid control for editing selected objects
                typeof(PropertyEditingCommands),        // commands for PropertyEditor and GridPropertyEditor, like Reset,
                                                        //  Reset All, Copy Value, Paste Value, Copy All, Paste All

                typeof(Outputs),                        // passes messages to all log writers
                typeof(ErrorDialogService),             // displays errors to the user in a message box

                typeof(HelpAboutCommand),               // custom command component to display Help/About dialog
                typeof(DefaultTabCommands),             // provides the default commands related to document tab Controls
                typeof(Editor),                         // editor which manages event sequence documents
                typeof(DomTypes),                       // defines the DOM's metadata for this sample app
                typeof(PaletteClient),                  // component which adds items to palette
                typeof(EventListEditor),                // adds drag/drop and context menu to event sequence ListViews
                typeof(ResourceListEditor),             // adds "slave" resources ListView control, drag/drop and context menu
                typeof(PythonService),                  // scripting service for automated tests
                typeof(ScriptConsole),                  // provides a dockable command console for entering Python commands
                typeof(AtfScriptVariables),             // exposes common ATF services as script variables
                typeof(AutomationService)               // provides facilities to run an automated script using the .NET remoting service
                );

            // Set up the MEF container with these components
            var container = new CompositionContainer(catalog);

            // Configure the main Form and add it to the composition container
            var batch = new CompositionBatch();
            var toolStripContainer = new ToolStripContainer();
            var mainForm = new MainForm(toolStripContainer)
            {
                Text = "Simple DOM, No XML, Editor Sample".Localize(),
                Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage))
            };

            batch.AddPart(mainForm);
            batch.AddPart(new WebHelpCommands("https://github.com/SonyWWS/ATF/wiki/ATF-Simple-DOM-No-XML-Editor-Sample".Localize()));

            // Compose the MEF container
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
