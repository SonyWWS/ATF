//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Wpf;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Controls.PropertyEditing;
using Sce.Atf.Wpf.Interop;
using Sce.Atf.Wpf.Models;

using AtfScriptVariables = Sce.Atf.Wpf.Applications.AtfScriptVariables;
using CommandService = Sce.Atf.Wpf.Applications.CommandService;
using ControlHostService = Sce.Atf.Wpf.Applications.ControlHostService;
using FileDialogService = Sce.Atf.Wpf.Applications.FileDialogService;
using PaletteService = Sce.Atf.Wpf.Applications.PaletteService;
using PropertyEditor = Sce.Atf.Wpf.Applications.PropertyEditor;
using SettingsService = Sce.Atf.Wpf.Applications.SettingsService;
using StandardEditCommands = Sce.Atf.Wpf.Applications.StandardEditCommands;

namespace SimpleDomEditorWpfSample
{
    /// <summary>
    /// WPF version of the SimpleDomEditor sample app. It demonstrates how to use the WPF
    /// versions of ATF's docking, document management, and property editing components.</summary>
    public partial class App : AtfApp
    {
        public App()
        {
            // Support localization into other languages by searching for embedded resource
            //  XML files for each target language.
            Localizer.SetStringLocalizer(new EmbeddedResourceStringLocalizer());
        }

        /// <summary>
        /// Gets MEF AggregateCatalog for application</summary>
        protected override AggregateCatalog GetCatalog()
        {
            var typeCatalog = new TypeCatalog(
                typeof(ControlHostService),             // Docking framework
                typeof(MainWindow),                     // Application's main window
                typeof(SchemaLoader),                   // Loads the schema and registers the data types with the DOM

                typeof(SettingsService),                // Persistent settings and user preferences dialog
                typeof(RecentDocumentCommands),         // Adds the standard recent document commands to the File menu
                typeof(MainWindowTitleService),         // Tracks document changes and updates main form title

                typeof(CommandService),                 // Handles menu and toolbar command creation and execution
                typeof(FileDialogService),              // Provides standard file dialogs - Open, Save, Save As
                typeof(StandardFileCommands),           // Adds the standard file commands to the File menu
                typeof(StandardFileExitCommand),        // Adds the Exit command to the File menu
                typeof(StandardEditCommands),           // standard Edit menu commands for copy/paste
                typeof(StandardEditHistoryCommands),    // standard Edit menu commands for undo/redo
                typeof(DocumentRegistry),               // Provides information about the currently loaded and active documents
                typeof(ContextRegistry),                // Provides information about the currently active editing context
                typeof(Composer),                       // Used in drag and drop
                typeof(Editor),                         // Manages and displays EventSequenceDocument documents

                typeof(PaletteService),                 // Creates the Palette pane and handles drag and drop
                typeof(PaletteClient),                  // Populates the palette with items from the schema

                typeof(PropertyEditor),                 // Manages the PropertyGrid to display the properties of the selected event

                typeof(ResourceListEditor),             // Adds "slave" resources ListView control, drag/drop and context menu

                typeof(HelpCommands),                   // Adds the Help menu and standard About command

                typeof(BasicPythonService),             // Scripting service for automated tests
                typeof(AtfScriptVariables),             // Exposes common ATF services as script variables
                typeof(AutomationService)               // Provides facilities to run an automated script using the .NET remoting service

                );

            return new AggregateCatalog(typeCatalog, StandardInteropParts.Catalog, StandardViewModels.Catalog);
        }

        /// <summary>
        /// Performs custom configuration on CompositionBeginning events.</summary>
        protected override void OnCompositionBeginning()
        {
            base.OnCompositionBeginning();

            var helpCommands = Container.GetExportedValueOrDefault<HelpCommands>();
            if (helpCommands != null)
            {
                // We don't have any context sensitive help, so disable these options.
                helpCommands.ShowContextHelp = false;
                helpCommands.EnableContextHelpUserSetting = false;

                // Set the URL for the sample app documentation.
                helpCommands.HelpFilePath = @"https://github.com/SonyWWS/ATF/wiki/ATF-Simple-DOM-Editor-WPF-Sample";
            }
        }
    }
}
