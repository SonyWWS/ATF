//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;

using Sce.Atf.Applications;
using Sce.Atf.Wpf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Component to edit DOM object values and attributes using the PropertyGrid</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(IControlHostClient))]
    [Export(typeof(PropertyEditor))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public class PropertyEditor : IInitializable, IControlHostClient
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public PropertyEditor(
            IControlHostService controlHostService,
            IContextRegistry contextRegistry)
        {
            ControlHostService = controlHostService;
            ContextRegistry = contextRegistry;
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by subscribing to event and registering control.</summary>
        public virtual void Initialize()
        {
            ContextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;

            m_controlDef = new ControlDef()
            {
                Name = "Property Grid".Localize(),
                Description = "Edits selected object properties".Localize(),
                Group = StandardControlGroup.Right,
                Id = s_propertyGridId.ToString()
            };
            ControlHostService.RegisterControl(m_controlDef, m_propertyGridView, this);
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        void IControlHostClient.Activate(object control)
        {
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        void IControlHostClient.Deactivate(object control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control.</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <param name="mainWindowClosing">True if the main window is closing</param>
        /// <returns>True if the Control can close, or false to cancel</returns>
        bool IControlHostClient.Close(object control, bool mainWindowClosing)
        {
            return true;
        }

        #endregion

        private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            IPropertyEditingContext context = GetContext();
            m_propertyGridView.EditingContext = context;
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

        private ControlDef m_controlDef;
        private readonly PropertyGridView m_propertyGridView = new PropertyGridView();
        private readonly SelectionPropertyEditingContext m_defaultContext = new SelectionPropertyEditingContext();
        private static Guid s_propertyGridId = new Guid("047BA5E9-2A48-4B1E-9362-1843965E5476");

    }
}
