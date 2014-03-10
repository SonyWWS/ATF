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

namespace ModelViewerSample
{
    /// <summary>
    /// This is a 3d model viewer sample application.
    /// This sample shows all the required steps to load and render 3d models 
    /// using ATF rendering subsystem.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-Model-Viewer-Sample.
    /// For a discussion of this sample's programming, see 
    /// https://github.com/SonyWWS/ATF/wiki/Model-Viewer-Programming-Discussion.
    /// 
    /// If you are building game level editor please use the standalone LevelEditor
    /// located at wws_shared\sdk\trunk\components\wws_leveleditor
    /// For more info about the LevelEditor project, please visit the LevelEditor home page
    /// http://wiki.ship.scea.com/confluence/display/WWSSDKLEVELEDITOR/LevelEditor
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.</summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
                        
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
                typeof(AtfUsageLogger),                 // logs computer info to an ATF server
                typeof(CrashLogger),                    // logs unhandled exceptions to an ATF server
                typeof(UnhandledExceptionService),      // catches unhandled exceptions, displays info, and gives user a chance to save
                typeof(FileDialogService),              // standard Windows file dialogs
                typeof(DocumentRegistry),               // central document registry with change notification                
                typeof(RecentDocumentCommands),         // standard recent document commands in File menu
                typeof(StandardFileCommands),           // standard File menu commands for New, Open, Save, SaveAs, Close
                typeof(MainWindowTitleService),         // tracks document changes and updates main form title                                
                typeof(StandardFileExitCommand),        // standard File exit menu command                                                               
                typeof(HelpAboutCommand),               // Help -> About command
                typeof(PythonService),                  // scripting service for automated tests
                typeof(ScriptConsole),                  // provides a dockable command console for entering Python commands
                typeof(AtfScriptVariables),             // exposes common ATF services as script variables
                typeof(AutomationService),              // provides facilities to run an automated script using the .NET remoting service
                typeof(Outputs),                        // passes messages to all IOutputWriter components
                typeof(ShoutOutputService),             // rich text box for displaying error and warning messages. Implements IOutputWriter

                typeof(Sce.Atf.Atgi.AtgiResolver),      // loads ATGI resources from a file
                typeof(Sce.Atf.Collada.ColladaResolver),// loads Collada resources from a file

                // this sample
                typeof(ModelViewer),                    // recognizes model file extensions and uses the above model resolvers to load models
                typeof(RenderCommands),                 // provides commands for switching the RenderView's rendering mode, etc.
                typeof(RenderView)                      // displays a 3D scene in a Windows Control
                );


            // Set up the MEF container with these components
            var container = new CompositionContainer(catalog);

            // Configure the main Form
            var batch = new CompositionBatch();
            var mainForm = new MainForm(new ToolStripContainer())
            {
                Text = "Model Viewer".Localize(),
                Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage))
            };
            // Add the main Form instance to the container
            batch.AddPart(mainForm);
            batch.AddPart(new WebHelpCommands("https://github.com/SonyWWS/ATF/wiki/ATF-Model-Viewer-Sample".Localize()));
            container.Compose(batch);

            var stdfile = container.GetExportedValue<StandardFileCommands>();
            stdfile.RegisterCommands = StandardFileCommands.CommandRegister.FileOpen;

            // Initialize components             
            foreach (IInitializable initializable in container.GetExportedValues<IInitializable>())
                initializable.Initialize();      
          
            // Show the main form and start message handling. The main Form Load event provides a final chance
            // for components to perform initialization and configuration.
            Application.Run(mainForm);

            // Give components a chance to clean up.
            container.Dispose();
        }
    }
}
