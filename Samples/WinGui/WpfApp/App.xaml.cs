//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition.Hosting;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Interop;
using Sce.Atf.Wpf.Models;

using WinGuiCommon;

using CommandService = Sce.Atf.Wpf.Applications.CommandService;
using ControlHostService = Sce.Atf.Wpf.Applications.ControlHostService;
using UnhandledExceptionService = Sce.Atf.Wpf.Applications.UnhandledExceptionService;
using WindowLayoutServiceCommands = Sce.Atf.Wpf.Applications.WindowLayoutServiceCommands;

namespace WpfApp
{
    /// <summary>
    /// This is a basic WPF sample application.
    /// It illustrates how to compose a WPF application with ATF components using MEF.
    /// It is a starting point for an application, such as an editor, though it does not offer any editing capabilities.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-Wpf-App-Sample.
    /// For a discussion of this sample's programming, see 
    /// https://github.com/SonyWWS/ATF/wiki/WinForms-and-WPF-Apps-Programming-Discussion. </summary>
    public partial class App : AtfApp
    {
        protected override AggregateCatalog GetCatalog()
        {
            var typeCatalog = new TypeCatalog(
                typeof(SettingsService),                // persistent settings and user preferences dialog
                typeof(CommandService),                 // handles commands in menus and toolbars
                typeof(ControlHostService),             // docking control host
                typeof(AtfUsageLogger),                 // logs computer info to an ATF server
                typeof(CrashLogger),                    // logs unhandled exceptions to an ATF server
                typeof(UnhandledExceptionService),      // catches unhandled exceptions, displays info, and gives user a chance to save
                typeof(ContextRegistry),                // central context registry with change notification
                typeof(StandardFileExitCommand),        // standard File exit menu command
                typeof(FileDialogService),              // standard Windows file dialogs
                typeof(DocumentRegistry),               // central document registry with change notification
                typeof(StandardFileCommands),           // standard File menu commands for New, Open, Save, SaveAs, Close
                typeof(AutoDocumentService),            // opens documents from last session, or creates a new document, on startup
                typeof(RecentDocumentCommands),         // standard recent document commands in File menu
                typeof(MainWindowTitleService),         // tracks document changes and updates main form title
                typeof(WindowLayoutService),            // service to allow multiple window layouts
                typeof(WindowLayoutServiceCommands),    // command layer to allow easy switching between and managing of window layouts
                typeof(SchemaLoader),                   // loads schema and extends types
                typeof(MainWindow),                     // main app window (analog to 'MainForm' in WinFormsApp)
                typeof(Editor),                         // Sample editor class that creates and saves application documents
                typeof(PythonService),                  // scripting service for automated tests
                typeof(ScriptConsole),                  // provides a dockable command console for entering Python commands
                typeof(AtfScriptVariables),             // exposes common ATF services as script variables
                typeof(AutomationService)               // provides facilities to run an automated script using the .NET remoting service
            );

            return new AggregateCatalog(typeCatalog, StandardInteropParts.Catalog, StandardViewModels.Catalog);
        }
    }
}