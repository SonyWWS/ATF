//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Editor that presents an IPrototypingContext using a TreeControl</summary>
    [Export(typeof(PrototypeLister))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PrototypeLister : TreeControlEditor, IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service for the underlying TreeControlEditor,
        /// which can be used to open context menus on a right-click</param>
        [ImportingConstructor]
        public PrototypeLister(ICommandService commandService)
            : base(commandService)
        {
            Configure(out m_controlInfo);
        }

        /// <summary>
        /// Configures the PrototypeLister</summary>
        /// <param name="controlInfo">Information about the Control for the hosting service</param>
        protected virtual void Configure(
            out ControlInfo controlInfo)
        {
            controlInfo = new ControlInfo(
                "Prototypes", //Is the ID in the layout. We'll localize DisplayName instead.
                "Creates new instances from prototypes".Localize(),
                StandardControlGroup.Right,
                s_factoryImage)
            {
                DisplayName = "Prototypes".Localize()
            };

            TreeControl.ShowRoot = false;
            TreeControl.AllowDrop = true;
            TreeControl.SelectionMode = SelectionMode.One;            
        }

        /// <summary>
        /// Gets the ControlInfo instance, which determines the appearance and
        /// initial location of the Control in the application</summary>
        public ControlInfo ControlInfo
        {
            get { return m_controlInfo; }
        }

        /// <summary>
        /// Gets or sets the prototyping context to be viewed and edited by the user</summary>
        public IPrototypingContext PrototypeContext
        {
            get { return m_prototypingContext; }
            set
            {
                if (m_prototypingContext != value)
                {
                    m_prototypingContext = value;
                    TreeView = m_prototypingContext;
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
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        void IControlHostClient.Activate(Control control)
        {
            if (m_prototypingContext != null)
                m_contextRegistry.ActiveContext = m_prototypingContext;
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        void IControlHostClient.Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control.</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns>True if the Control can close, or false to cancel</returns>
        bool IControlHostClient.Close(Control control)
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Called for every mouse move event in which drag-and-drop objects are being dragged.
        /// Begins a drag-and-drop operation on the underlying tree Control.</summary>
        /// <param name="items">Enumeration of items being dragged</param>
        protected override void OnStartDrag(IEnumerable<object> items)
        {
            IDataObject dataObject = m_prototypingContext.GetInstances(items);
            TreeControl.DoDragDrop(dataObject, DragDropEffects.All | DragDropEffects.Link);
        }

        /// <summary>
        /// Performs custom actions after the LastHitChanged event</summary>
        /// <param name="e">Event args</param>
        protected override void OnLastHitChanged(EventArgs e)
        {
            if (m_prototypingContext != null)
                m_prototypingContext.SetActiveItem(LastHit);

            base.OnLastHitChanged(e);
        }

        private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            PrototypeContext = m_contextRegistry.GetMostRecentContext<IPrototypingContext>();
            TreeControl.Text = (PrototypeContext != null) ? 
                "Copy items from the document and paste them here to create prototypes that can be dragged and dropped onto a canvas.".Localize()
                : null;
        }

        private IPrototypingContext m_prototypingContext;
        private readonly ControlInfo m_controlInfo;

        private static readonly Image s_factoryImage = ResourceUtil.GetImage16(Resources.FactoryImage);
    }
}
