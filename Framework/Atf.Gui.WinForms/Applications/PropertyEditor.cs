//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Controls.PropertyEditing;

using PropertyGrid = Sce.Atf.Controls.PropertyEditing.PropertyGrid;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component to edit DOM object values and attributes using the PropertyGrid</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(IControlHostClient))]
    [Export(typeof(PropertyEditor))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public class PropertyEditor : IInitializable, IControlHostClient, IDisposable
    {
        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="commandService">ICommandService</param>
        /// <param name="controlHostService">IControlHostService</param>
        /// <param name="contextRegistry">IContextRegistry</param>
        [ImportingConstructor]
        public PropertyEditor(
            ICommandService commandService,
            IControlHostService controlHostService,
            IContextRegistry contextRegistry)
        {
            CommandService = commandService;
            ControlHostService = controlHostService;
            ContextRegistry = contextRegistry;
        }

        /// <summary>
        /// Configures the property editor</summary>
        /// <param name="propertyGrid">Property grid control</param>
        /// <param name="controlInfo">Information about the control for the hosting service</param>
        protected virtual void Configure(out PropertyGrid propertyGrid, out ControlInfo controlInfo)
        {
            propertyGrid = new PropertyGrid();
            controlInfo = new ControlInfo(
                "Property Editor", //Is the ID in the layout. We'll localize DisplayName instead.
                "Edits selected object properties".Localize(),
                StandardControlGroup.Right, null,
                "https://github.com/SonyWWS/ATF/wiki/Property-Editing-in-ATF".Localize())
            {
                DisplayName = "Property Editor".Localize()
            };
        }

        /// <summary>
        /// Gets the internal property grid. Is available after Initialize() and Configure() are called.</summary>
        public PropertyGrid PropertyGrid
        {
            get { return m_propertyGrid; }
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

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by subscribing to event, registering control, and setting up Settings Service</summary>
        public virtual void Initialize()
        {
            Configure(out m_propertyGrid, out m_controlInfo);

            m_propertyGrid.PropertyGridView.ContextRegistry = ContextRegistry;
            m_propertyGrid.MouseUp += propertyGrid_MouseUp;

            ContextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;

            ControlHostService.RegisterControl(m_propertyGrid, m_controlInfo, this);

            if (SettingsService != null)
            {
                SettingsService.RegisterSettings(this,
                    new BoundPropertyDescriptor(m_propertyGrid, () => m_propertyGrid.Settings, "Settings", null, null));
            }
        }

        #endregion

        /// <summary>
        /// Disposes resources</summary>
        public void Dispose()
        {
            m_propertyGrid.Dispose();
        }

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        void IControlHostClient.Activate(Control control)
        {
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
        /// <returns><c>True</c> if the Control can close, or false to cancel</returns>
        bool IControlHostClient.Close(Control control)
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Method called during a MouseUp event on the underlying PropertyGrid. Displays the
        /// context menu.</summary>
        /// <param name="e">Mouse event args from the PropertyGrid MouseUp event</param>
        protected virtual void OnPropertyGridMouseUp(MouseEventArgs e)
        {
            if (CommandService != null)
            {
                // if no property is specified, return the whole property editing context
                Point clientPt = new Point(e.X, e.Y);
                IPropertyEditingContext context;
                object target = m_propertyGrid.GetDescriptorAt(clientPt, out context);
                if (target == null)
                    target = GetContext();

                IEnumerable<object> commands = m_contextMenuCommandProviders.GetCommands(context, target)
                    .Where(x => !IsStandardEditCommand(x)); // filter out standard edit commands as they are not applicable for property editing,
                // even if the current active context supports IInstancingContext.

                Point screenPoint = m_propertyGrid.PointToScreen(clientPt);

                CommandService.RunContextMenu(commands, screenPoint);
            }
        }

        private bool IsStandardEditCommand(object commandTag)
        {
            if (commandTag is StandardCommand)
            {
                if ((StandardCommand)commandTag == StandardCommand.EditCut)
                    return true;
                if ((StandardCommand)commandTag == StandardCommand.EditCopy)
                    return true;
                if ((StandardCommand)commandTag == StandardCommand.EditPaste)
                    return true;
                if ((StandardCommand)commandTag == StandardCommand.EditDelete)
                    return true;
            }
            return false;
        }

        private void propertyGrid_MouseUp(object sender, MouseEventArgs e)
        {
            OnPropertyGridMouseUp(e);
        }

        private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            IPropertyEditingContext context = GetContext();
            m_propertyGrid.Bind(context);
        }

        private IPropertyEditingContext GetContext()
        {
            // first try to get a client-defined IPropertyEditingContext
            IPropertyEditingContext context = ContextRegistry.GetMostRecentContext<IPropertyEditingContext>();
            if (context != null)
            {
                m_defaultContext.SelectionContext = null;
            }
            else
            {
                // otherwise, try to get a client-defined ISelectionContext and adapt it
                ISelectionContext selectionContext = ContextRegistry.GetMostRecentContext<ISelectionContext>();
                m_defaultContext.SelectionContext = selectionContext;
                if (selectionContext != null)
                    context = m_defaultContext;
            }

            return context;
        }

        // Imported MEF Components
        /// <summary>
        /// Gets IControlHostService</summary>
        protected IControlHostService ControlHostService { get; private set; }
        /// <summary>
        /// Gets IContextRegistry</summary>
        protected IContextRegistry ContextRegistry { get; private set; }
        /// <summary>
        /// Gets ICommandService</summary>
        protected ICommandService CommandService { get; private set; }

        // Optional MEF Imports
        /// <summary>
        /// Gets or sets ISettingsService</summary>
        [Import(AllowDefault = true)]
        public ISettingsService SettingsService { get; set; }

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;

        private PropertyGrid m_propertyGrid;
        private ControlInfo m_controlInfo;
        private SelectionPropertyEditingContext m_defaultContext = new SelectionPropertyEditingContext();
    }
}
