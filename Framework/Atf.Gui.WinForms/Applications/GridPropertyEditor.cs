//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component to edit DOM object values and attributes using the GridControl</summary>
    [Export(typeof(GridPropertyEditor))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public class GridPropertyEditor : IDisposable, IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Optional command service for running the context menu</param>
        /// <param name="controlHostService">Control host service for registering the GridControl</param>
        /// <param name="contextRegistry">Context registry for finding an IPropertyEditingContext
        /// or ISelectionContext</param>
        [ImportingConstructor]
        public GridPropertyEditor(
            ICommandService commandService,
            IControlHostService controlHostService,
            IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_controlHostService = controlHostService;
            m_contextRegistry = contextRegistry;

            Configure(out m_gridControl, out m_controlInfo);

            m_gridControl.MouseUp += gridControl_MouseUp;
        }

        /// <summary>
        /// Configures the property editor</summary>
        /// <param name="gridControl">Grid control</param>
        /// <param name="controlInfo">Information about the control for the hosting service</param>
        protected virtual void Configure(out GridControl gridControl, out ControlInfo controlInfo)
        {
            gridControl = new GridControl();
            controlInfo = new ControlInfo(
                "Grid Property Editor", //Is the ID in the layout. We'll localize DisplayName instead.
                "Edits selected object properties".Localize(),
                StandardControlGroup.Bottom, null,
                "https://github.com/SonyWWS/ATF/wiki/Property-Editing-in-ATF".Localize())
            {
                DisplayName = "Grid Property Editor".Localize()
            };
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;

            m_controlHostService.RegisterControl(m_gridControl, m_controlInfo, this);

            if (m_settingsService != null)
            {
                m_settingsService.RegisterSettings(this,
                    new BoundPropertyDescriptor(m_gridControl, () => m_gridControl.Settings, "Settings", "", ""));
            }
        }

        #endregion

        /// <summary>
        /// Disposes resources</summary>
        public void Dispose()
        {
            m_gridControl.Dispose();
        }

        /// <summary>
        /// Gets the internal GridControl</summary>
        public GridControl GridControl
        {
            get { return m_gridControl; }
        }

        /// <summary>
        /// Gets the information about the control for the hosting service</summary>
        public ControlInfo ControlInfo
        {
            get { return m_controlInfo; }
        }

        /// <summary>
        /// Gets or sets the default SelectionPropertyEditingContext object. This object
        /// is used if there is no IPropertyEditingContext available from the IContextRegistry.
        /// Set this to control custom property filtering behavior for the current
        /// ISelectionContext, by overriding the SelectionPropertyEditingContext's
        /// GetPropertyDescriptors(). Can't be null.</summary>
        public SelectionPropertyEditingContext DefaultPropertyEditingContext
        {
            get { return m_defaultContext; }
            set { m_defaultContext = value; }
        }

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the control gets focus, or a parent "host" control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        void IControlHostClient.Activate(Control control)
        {
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
        /// <returns>True if the control can close, or false to cancel</returns>
        bool IControlHostClient.Close(Control control)
        {
            return true;
        }

        #endregion

        private void gridControl_MouseUp(object sender, MouseEventArgs e)
        {
            OnGridControlMouseUp(e);
        }

        /// <summary>
        /// Method called on MouseUp events</summary>
        /// <param name="e">MouseEventArgs for event</param>
        protected virtual void OnGridControlMouseUp(MouseEventArgs e)
        {
            if (m_commandService != null)
            {
                // if no property is specified, return the whole property editing context
                Point clientPt = new Point(e.X, e.Y);
                object target = m_gridControl.GetDescriptorAt(clientPt);
                if (target == null)
                    target = GetContext();

                object context = m_gridControl.GridView.EditingContext;
                IEnumerable<object> commands =
                    m_contextMenuCommandProviders.GetCommands(context, target);

                Point screenPoint = m_gridControl.PointToScreen(clientPt);
                m_commandService.RunContextMenu(commands, screenPoint);
            }
        }

        private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            IPropertyEditingContext context = GetContext();
            m_gridControl.Bind(context);
        }

        private IPropertyEditingContext GetContext()
        {
            // first try to get a client-defined IPropertyEditingContext
            IPropertyEditingContext context = m_contextRegistry.GetMostRecentContext<IPropertyEditingContext>();
            if (context != null)
            {
                m_defaultContext.SelectionContext = null;
            }
            else
            {
                // otherwise, try to get a client-defined ISelectionContext and adapt it
                ISelectionContext selectionContext = m_contextRegistry.GetMostRecentContext<ISelectionContext>();
                m_defaultContext.SelectionContext = selectionContext;
                if (selectionContext != null)
                    context = m_defaultContext;
            }

            return context;
        }

        private readonly IControlHostService m_controlHostService;
        private readonly IContextRegistry m_contextRegistry;
        private readonly ICommandService m_commandService;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;

        private readonly GridControl m_gridControl;
        private readonly ControlInfo m_controlInfo;
        private SelectionPropertyEditingContext m_defaultContext = new SelectionPropertyEditingContext();
    }
}
