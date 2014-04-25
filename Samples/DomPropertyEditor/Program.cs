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


namespace DomPropertyEditorSample
{
    /// <summary>
    /// The purpose of this sample is to:
    /// 1- Show how to decorate a DomNode with property descriptors so it can be edited 
    ///    using the PropertyEditor component.
    /// 2- List most of the available property descriptors and type-editors.
    /// The most important file in this sample is SchemaLoader.
    /// SchemaLoader decorates the DomNodeTypes with descriptors and editors depending on 
    /// property type.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-DOM-Property-Editor-Sample. </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.</summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Set up localization support early on, so that user-readable strings will be localized
            //  during the initialization phase below. Use XML files that are embedded resources.
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CurrentCulture;
            Localizer.SetStringLocalizer(new EmbeddedResourceStringLocalizer());

            // Enable metadata driven property editing for the DOM
            DomNodeType.BaseOfAllTypes.AddAdapterCreator(new AdapterCreator<CustomTypeDescriptorNodeAdapter>());

            // Create a type catalog with the types of components we want in the application
            var catalog = new TypeCatalog(

                typeof(CommandService),                 // handles commands in menus and toolbars
                typeof(ControlHostService),             // docking control host
                typeof(AtfUsageLogger),                 // logs computer info to an ATF server
                typeof(CrashLogger),                    // logs unhandled exceptions to an ATF server
                typeof(UnhandledExceptionService),      // catches unhandled exceptions, displays info, and gives user a chance to save
                typeof(FileDialogService),
                typeof(SkinService),
                typeof(StandardFileExitCommand),        // standard File exit menu command
                typeof(StandardEditHistoryCommands),    // tracks document changes and updates main form title
                typeof(HelpAboutCommand),               // Help -> About command
                typeof(ContextRegistry),                // central context registry with change notification
                typeof(PropertyEditor),                 // property grid for editing selected objects
                typeof(GridPropertyEditor),             // grid control for editing selected objects
                typeof(PropertyEditingCommands),        // commands for PropertyEditor and GridPropertyEditor
                typeof(SettingsService),
                typeof(PythonService),                  // scripting service for automated tests
                typeof(ScriptConsole),                  // provides a dockable command console for entering Python commands
                typeof(AtfScriptVariables),             // exposes common ATF services as script variables
                typeof(AutomationService),              // provides facilities to run an automated script using the .NET remoting service

                typeof(SchemaLoader),                   // component that loads XML schema and sets up types
                typeof(Editor)                          // component that manages UI documents
                );

            // Set up the MEF container with these components
            var container = new CompositionContainer(catalog);

            // Configure the main Form

            // Configure the main Form with a ToolStripContainer so the CommandService can
            //  generate toolbars.
            var toolStripContainer = new ToolStripContainer();
            toolStripContainer.Dock = DockStyle.Fill;
            var mainForm = new MainForm(toolStripContainer)
            {
                Text = "DOM Property Editor".Localize(),
                Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage))
            };

            // Add the main Form instance to the container
            var batch = new CompositionBatch();
            batch.AddPart(mainForm);
            // batch.AddPart(new WebHelpCommands("https://github.com/SonyWWS/ATF/wiki/ATF-DOM-Tree-Editor-Sample".Localize()));
            container.Compose(batch);

            // Initialize components that require it. Initialization often can't be done in the constructor,
            //  or even after imports have been satisfied by MEF, since we allow circular dependencies between
            //  components, via the System.Lazy class. IInitializable allows components to defer some operations
            //  until all MEF composition has been completed.
            container.InitializeAll();

            var propEditor = container.GetExportedValue<PropertyEditor>();
            propEditor.PropertyGrid.PropertySorting = Sce.Atf.Controls.PropertyEditing.PropertySorting.Categorized;
            // Show the main form and start message handling. The main Form Load event provides a final chance
            //  for components to perform initialization and configuration.
            Application.Run(mainForm);

            // Give components a chance to clean up.
            container.Dispose();

        }
    }
}
