//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.IO;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Input;


namespace CircuitEditorSample.Tests
{
    /// <summary>
    /// Circuit creation command component</summary>
    [Export(typeof(IInitializable))]
    public class CircuitTestCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="schemaLoader"></param>
        [ImportingConstructor]
        public CircuitTestCommands(ICommandService commandService, 
            IContextRegistry contextRegistry,
            SchemaLoader schemaLoader)
        {
            m_commandService = commandService;
            m_schemaLoader = schemaLoader;
        }

        private enum CommandTag
        {
            TestCreateCircuitDoc,
         
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering command to create circuit</summary>
        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(CommandTag.TestCreateCircuitDoc, 
                 StandardMenu.Help,  
                 StandardCommandGroup.HelpAbout, 
                 "Create Circuit Programmatically".Localize(),
                 "Create Circuit Programmatically".Localize(),
                 Keys.None,
                 null,
                CommandVisibility.ApplicationMenu,
                this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            return commandTag is CommandTag;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        public void DoCommand(object commandTag)
        {
            if (commandTag.Equals(CommandTag.TestCreateCircuitDoc))
            {
                var circuitDomNode = CircuitEditorTester.CreateTestCircuitProgrammatically(m_schemaLoader);
                if (m_circuitEditor != null && m_controlHostService != null)
                {
                    AdaptableControl control = m_circuitEditor.CreateCircuitControl(circuitDomNode);
                    var viewingContext = circuitDomNode.Cast<ViewingContext>();
                    viewingContext.Control = control;

                    var circuitDocument = circuitDomNode.Cast<CircuitDocument>();
                    var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"SCEA\Circuit Editor\Test Circuit.circuit");
                    var controlInfo = new ControlInfo(fileName, fileName, StandardControlGroup.Center);
                    circuitDocument.ControlInfo = controlInfo;
                    circuitDocument.Uri = new Uri(fileName, UriKind.Absolute);

                    var editingContext = circuitDomNode.Cast<CircuitEditingContext>();
                    control.Context = editingContext;

                    editingContext.GetLocalBound = Editor.GetLocalBound;
                    editingContext.GetWorldOffset = Editor.GetWorldOffset;
                    editingContext.GetTitleHeight = Editor.GetTitleHeight;
                    editingContext.GetLabelHeight = Editor.GetLabelHeight;
                    editingContext.GetSubContentOffset = Editor.GetSubContentOffset;
 
                    // now that the data is complete, initialize all other extensions to the Dom data
                    circuitDomNode.InitializeExtensions();
                    circuitDocument.Dirty = false;
                    m_controlHostService.RegisterControl(control, controlInfo, m_circuitEditor); 
                }
            }
             
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

    
        [Import(AllowDefault = true)]
        private Editor m_circuitEditor = null;
        [Import(AllowDefault = true)]
        private IControlHostService m_controlHostService= null;

        private ICommandService m_commandService;
        private SchemaLoader m_schemaLoader;
    }

   
}
