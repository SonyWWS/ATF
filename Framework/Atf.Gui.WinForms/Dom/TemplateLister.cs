//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Editor that presents an ITemplatingContext using a TreeControl</summary>
    [InheritedExport(typeof(TemplateLister))]
    [InheritedExport(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public class TemplateLister : TreeControlEditor, IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service for the underlying TreeControlEditor,
        /// which can be used to open context menus on a right-click</param>
        [ImportingConstructor]
        public TemplateLister(ICommandService commandService)
            : base(commandService)
        {
            Configure(out m_controlInfo);
        }

       
        /// <summary>
        /// Configures the TemplateLister</summary>
        /// <param name="controlInfo">Information about the control for the hosting service</param>
        protected virtual void Configure(
            out ControlInfo controlInfo)
        {
            controlInfo = new ControlInfo(
                "Templates".Localize(),
                "Reference subgraphs from templates".Localize(),
                StandardControlGroup.Right,
                m_templateLibraryImage);

            TreeControl.ShowRoot = false;
            TreeControl.AllowDrop = true;
            TreeControl.SelectionMode = SelectionMode.One;            
        }

        /// <summary>
        /// Gets the control info instance, which determines the appearance and
        /// initial location of the control in the application.</summary>
        public ControlInfo ControlInfo
        {
            get { return m_controlInfo; }
        }

        /// <summary>
        /// Gets or sets the templating context to be viewed and edited by the user</summary>
        public ITemplatingContext TemplateContext
        {
            get { return m_templatingContext; }
            set
            {
                if (m_templatingContext != value)
                {
                    m_templatingContext = value;
                    TreeView = m_templatingContext;
                }
            }
        }

        [Import]
        private IControlHostService m_controlHostService;

        [Import]
        private IContextRegistry m_contextRegistry;

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;

            m_controlHostService.RegisterControl(TreeControl, m_controlInfo, this);
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the control gets focus, or a parent "host" control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        void IControlHostClient.Activate(Control control)
        {
            if (m_templatingContext != null)
                m_contextRegistry.ActiveContext = m_templatingContext;
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another control or "host" control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        void IControlHostClient.Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control.</summary>
        /// <param name="control">Client control to be closed</param>
        /// <returns>true if the control can close, or false to cancel.</returns>
        bool IControlHostClient.Close(Control control)
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Called for every mouse move event in which drag-and-drop objects are being dragged.
        /// Begins a drag-and-drop operation on the underlying tree control.</summary>
        /// <param name="items">Enumeration of items being dragged</param>
        protected override void OnStartDrag(IEnumerable<object> items)
        {
            IDataObject dataObject = m_templatingContext.GetInstances(items);
            TreeControl.DoDragDrop(dataObject, DragDropEffects.All | DragDropEffects.Link);
        }

        /// <summary>
        /// Raises LastHitChanged event and sets active item</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnLastHitChanged(EventArgs e)
        {
            if (m_templatingContext != null)
                m_templatingContext.SetActiveItem(LastHit);

            base.OnLastHitChanged(e);
        }

        /// <summary>
        /// Handles DragOver event and sets drag effect</summary>
        /// <param name="e">Drag event arguments</param>
        protected override void OnDragOver(DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (m_templatingContext != null)
            {
                var instancingContext = m_templatingContext.As<IInstancingContext>();
                if (instancingContext != null && instancingContext.CanInsert(e.Data))
                    e.Effect = DragDropEffects.Move;
            }
        }

        /// <summary>
        /// Handles DragDrop event and performs data insertion in a transaction</summary>
        /// <param name="e">Drag event arguments</param>
        protected override void OnDragDrop(DragEventArgs e)
        {
            if (m_templatingContext != null)
            {
                var instancingContext = m_templatingContext.As<IInstancingContext>();
                if (instancingContext != null)
                {
                    string commandName = "Drag and Drop".Localize();
                    instancingContext.As<ITransactionContext>().DoTransaction(
                        delegate
                        {
                            instancingContext.Insert(e.Data);
                        },
                        commandName);

                    if (StatusService != null)
                        StatusService.ShowStatus(commandName);
                }
            }
        }

        private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            TemplateContext = m_contextRegistry.GetMostRecentContext<ITemplatingContext>();
        }


        private ITemplatingContext m_templatingContext;
        private readonly ControlInfo m_controlInfo;

        private  readonly Image m_templateLibraryImage = ResourceUtil.GetImage16(Resources.ComponentsImage);
    }
}
