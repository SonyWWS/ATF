//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Editor that presents an ILayeringContext using a TreeControl</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(LayerLister))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LayerLister : TreeControlEditor, IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service for opening right-click context menus</param>
        [ImportingConstructor]
        public LayerLister(ICommandService commandService)
            : base(commandService)
        {
            Configure(out m_controlInfo);

            TreeControl.NodeCheckStateEdited += treeControl_NodeCheckStateEdited;
        }

        /// <summary>
        /// Configures the LayerLister</summary>
        /// <param name="controlInfo">Information about the control for the hosting service</param>
        protected virtual void Configure(
            out ControlInfo controlInfo)
        {
            controlInfo = new ControlInfo(
                "Layers", //Is the ID in the layout. We'll localize DisplayName instead.
                "Edits document layers".Localize(),
                StandardControlGroup.Right,
                s_layerImage)
            {
                DisplayName = "Layers".Localize()
            };

            TreeControl.ShowRoot = false;
            TreeControl.AllowDrop = true;
            TreeControl.SelectionMode = SelectionMode.MultiExtended;
        }

        /// <summary>
        /// Gets the control info instance, which determines the appearance and
        /// initial location of the control in the application</summary>
        public ControlInfo ControlInfo
        {
            get { return m_controlInfo; }
        }

        /// <summary>
        /// Gets or sets the layering context to be viewed and edited by the user</summary>
        public ILayeringContext LayeringContext
        {
            get { return m_layeringContext; }
            set
            {
                if (m_layeringContext != value)
                {
                    m_layeringContext = value;
                    TreeView = m_layeringContext;
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
        /// Notifies the client that its control has been activated. Activation occurs when
        /// the control gets focus, or a parent "host" control gets focus.</summary>
        /// <param name="control">Client control that was activated</param>
        void IControlHostClient.Activate(Control control)
        {
            if (m_layeringContext != null)
                m_contextRegistry.ActiveContext = m_layeringContext;
        }

        /// <summary>
        /// Notifies the client that its control has been deactivated. Deactivation occurs when
        /// another control or "host" control gets focus.</summary>
        /// <param name="control">Client control that was deactivated</param>
        void IControlHostClient.Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's control</summary>
        /// <param name="control">Client control to be closed</param>
        /// <returns><c>True</c> if the control can close, or false to cancel</returns>
        bool IControlHostClient.Close(Control control)
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Performs custom actions after the LastHitChanged event</summary>
        /// <param name="e">Event args</param>
        protected override void OnLastHitChanged(EventArgs e)
        {
            if (m_layeringContext != null)
                m_layeringContext.SetActiveItem(LastHit);

            base.OnLastHitChanged(e);
        }

        private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            LayeringContext = m_contextRegistry.GetMostRecentContext<ILayeringContext>();

            if (LayeringContext != null)
            {
                TreeControl.Text = "Copy items from the document and paste them here to create layers whose visibility can be controlled by clicking on a check box.".Localize();
            }
            else
            {
                TreeControl.Text = null;
            }
        }

        private void treeControl_NodeCheckStateEdited(object sender, TreeControl.NodeEventArgs e)
        {
            ShowLayer(e.Node.Tag, e.Node.CheckState == CheckState.Checked);
        }

        /// <summary>
        /// Shows or hides a specified layer</summary>
        /// <param name="layer">Layer to show or hide</param>
        /// <param name="show">True to show, false to hide layer</param>
        public void ShowLayer(object layer, bool show)
        {
            ITransactionContext transactionContext = m_layeringContext.As<ITransactionContext>();
            transactionContext.DoTransaction(delegate
                {
                    m_layeringContext.SetVisible(layer, show);
                },
                "Show/Hide Layer".Localize());
        }

        private readonly ControlInfo m_controlInfo;
        private ILayeringContext m_layeringContext;

        private static readonly Image s_layerImage = ResourceUtil.GetImage16(Resources.LayerImage);
    }
}
