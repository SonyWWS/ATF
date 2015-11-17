//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
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


        // image for clone button.
        private Bitmap s_cloneImage;
        /// <summary>
        /// Finishes initializing component by subscribing to event, registering control, and setting up Settings Service</summary>
        public virtual void Initialize()
        {
            Configure(out m_propertyGrid, out m_controlInfo);

            // create image for clone button.
            if (s_cloneImage == null)
            {
                s_cloneImage = new Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(s_cloneImage))
                {
                    g.Clear(Color.Transparent);
                    var rect = new Rectangle(1, 1, 8, 10);                        
                    g.FillRectangle(Brushes.White, rect);
                    g.DrawRectangle(Pens.Black, rect);
                    rect.Location = new Point(6, 5);
                    g.FillRectangle(Brushes.White, rect);
                    g.DrawRectangle(Pens.Black, rect);
                }
            }
            
            var cloneButton = new ToolStripButton();
            cloneButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            cloneButton.Image = s_cloneImage;            
            cloneButton.Name = "cloneButton";
            cloneButton.Size = new Size(29, 22);
            cloneButton.ToolTipText = "Clone this editor".Localize();
            cloneButton.Click += (sender, e) => Duplicate();            
            m_propertyGrid.ToolStrip.Items.Add(cloneButton);
            m_propertyGrid.PropertyGridView.ContextRegistry = ContextRegistry;
            m_propertyGrid.MouseUp += propertyGrid_MouseUp;

            ContextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;

            ControlHostService.RegisterControl(m_propertyGrid, m_controlInfo, this);

            if (SettingsService != null)
            {
                SettingsService.RegisterSettings(this,
                    new BoundPropertyDescriptor(m_propertyGrid, () => m_propertyGrid.Settings, "Settings", null, null));

                var groupDescr = new BoundPropertyDescriptor(this.GetType(), () => ClonedEditorGroup, "Docking", null,
                    "Initial docking group for duplicated property editors\r\nCenter, CenterPermanent, and Hidden are not accepted".Localize());

                SettingsService.RegisterSettings(this, groupDescr);
                SettingsService.RegisterUserSettings("Property Editor".Localize(), groupDescr);
            }
        }

        #endregion

        /// <summary>
        /// Duplicates property editor.</summary>
        public void Duplicate()
        {
            new ClonedPropertyEditor(this);
        }

        // the group for cloned editor.
        private static StandardControlGroup s_clonedEditorGroup = StandardControlGroup.Floating;
        public static StandardControlGroup ClonedEditorGroup
        {
            get { return s_clonedEditorGroup; }
            set
            {
                // reject the following values.
                if (value == StandardControlGroup.Center || value == StandardControlGroup.CenterPermanent
                    || value == StandardControlGroup.Hidden) return;

                s_clonedEditorGroup = value;
            }
        }

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
        /// <returns>True if the Control can close, or false to cancel</returns>
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



        [Import(AllowDefault = true)]
        private IDocumentRegistry DocumentRegistry { get; set; }


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


        private class ClonedPropertyEditor : IControlHostClient
        {
            public ClonedPropertyEditor(PropertyEditor propEditor)
            {
                var editingContext = propEditor.PropertyGrid.PropertyGridView.EditingContext;

                // there is no reason to clone empty property editor.
                if (editingContext == null)
                    throw new ArgumentException("propEditor");

                // don't create cloned property editor 
                // if there is nothing to edit
                if (editingContext.PropertyDescriptors == null || !editingContext.PropertyDescriptors.Any()) 
                    return;

                if(editingContext is SelectionPropertyEditingContext)
                    m_context = ((SelectionPropertyEditingContext)editingContext).SelectionContext;
                else
                    m_context = editingContext;

                                                                  
                m_propertyEditor = propEditor;
                m_propertyEditor.Configure(out m_propertyGrid, out m_controlInfo);


                m_selectionButton = new ToolStripButton();
                m_selectionButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
                m_selectionButton.Image = ResourceUtil.GetImage16(Resources.SelectionFindImage);
                m_selectionButton.Name = "selectionButton";
                m_selectionButton.Size = new Size(29, 22);
                m_selectionButton.ToolTipText = "Select bound object(s)".Localize();
                m_selectionButton.Click += (sender, e) =>
                    {
                        // select bound object
                        ISelectionContext selCntx = m_context.As<ISelectionContext>();
                        var edCntx = m_propertyGrid.PropertyGridView.EditingContext;
                        if (selCntx != null && edCntx != null)
                        {
                            selCntx.SetRange(edCntx.Items);
                        }
                    };

                m_propertyGrid.ToolStrip.Items.Add(m_selectionButton);
                m_propertyGrid.PropertyGridView.ContextRegistry = propEditor.ContextRegistry;

                m_controlInfo.Name = propEditor.m_controlInfo.DisplayName + "_" + ++s_cloneId;
                m_controlInfo.Group = PropertyEditor.ClonedEditorGroup;
                m_controlInfo.UnregisterOnClose = true;
                m_propertyEditor.ControlHostService.RegisterControl(m_propertyGrid, m_controlInfo, this);

                m_propertyEditingContext = new CustomPropertyEditingContext(editingContext);
                m_propertyGrid.Bind(m_propertyEditingContext);

                m_propertyGrid.PropertySorting = propEditor.PropertyGrid.PropertySorting;
                
                // copy expansion state
                var zip = propEditor.PropertyGrid.PropertyGridView.Categories.Zip(m_propertyGrid.PropertyGridView.Categories, (src, dest) => new { src, dest });
                foreach (var pair in zip)
                {                  
                    if (pair.dest.Name == pair.src.Name)
                        pair.dest.Expanded = pair.src.Expanded;
                }

                m_propertyGrid.MouseUp += (sender, e) => OnPropertyGridMouseUp(e);

                // subscribe to events.
                // It is necessary to unsubscribe to allow this object to be garbage collected.               

                if (m_propertyEditor.DocumentRegistry != null)
                {
                    m_propertyEditor.DocumentRegistry.DocumentRemoved += DocumentRegistry_DocumentRemoved;
                    m_propertyEditor.DocumentRegistry.ActiveDocumentChanged += DocumentRegistry_ActiveDocumentChanged;
                }                              
                m_observableContext = m_context.As<IObservableContext>();
                m_validationContext = m_context.As<IValidationContext>();

                if (m_observableContext != null)
                {
                    m_observableContext.ItemChanged += observableContext_ItemChanged;
                    m_observableContext.ItemInserted += observableContext_ItemInserted;
                    m_observableContext.ItemRemoved += observableContext_ItemRemoved;
                }
                if (m_validationContext != null)
                {
                    m_validationContext.Beginning += validationContext_Beginning;
                    m_validationContext.Ended += validationContext_Ended;
                    m_validationContext.Cancelled += validationContext_Cancelled;
                }
            }

            private void DocumentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
            {
                var activeDoc = m_propertyEditor.DocumentRegistry.ActiveDocument;
                if (IsObjectPartOfDocument(activeDoc, m_context))
                {
                    m_selectionButton.Enabled = true;
                    m_propertyGrid.Bind(m_propertyEditingContext);
                }
                else
                {
                    m_selectionButton.Enabled = false;
                    m_propertyGrid.Bind(null);
                }
            }

            private void DocumentRegistry_DocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
            {
                // if the removed document is equal to m_context or 
                // m_context is part of (child of) the removed document
                // then close the property editor.
                if (IsObjectPartOfDocument(e.Item, m_context)) ClearAndUnregister();
            }

            private bool IsObjectPartOfDocument(IDocument document, object obj)
            {
                if (document == null || obj == null) return false;
                if (document.Equals(obj)) return true;
                //try enumerable context.
                var enumContext = document.As<IEnumerableContext>();
                if (enumContext != null && enumContext.Items != null)
                {
                    if (enumContext.Items.Any(elm => AdaptableEquals(elm, obj)))
                        return true;                    
                }

                return false;
            }

            #region IValidationContext and IObservalbeContext event handlers.

            private bool m_validating;            
            private void validationContext_Beginning(object sender, EventArgs e)
            {
                m_validating = true;
            }

            private void validationContext_Ended(object sender, EventArgs e)
            {
                EndValidation();
            }

            private void validationContext_Cancelled(object sender, EventArgs e)
            {
                EndValidation();
            }

            private void observableContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
            {
                Invalidate();
            }

            private void observableContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
            {
                Invalidate();
            }

            private void observableContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
            {                
                m_propertyEditingContext.RemoveItem(e.Item);

                if (m_propertyEditingContext.PropertyDescriptors.Any())
                {
                    Invalidate();
                }
                else
                {
                    ClearAndUnregister();
                }
            }

            private void Invalidate()
            {
                if (m_validating) return;
                if(m_propertyEditingContext != null)
                    m_propertyEditingContext.OnReloaded();
            }

            private void EndValidation()
            {
                m_validating = false;
                Invalidate();              
            }

            #endregion

           
            private void ClearAndUnregister()
            {
                var control = m_propertyGrid;
                ClearState();
                m_propertyEditor.ControlHostService.UnregisterControl(control);
            }
            private void ClearState()
            {

                if (m_context == null) return;
                m_context = null;

                if (m_propertyEditor.DocumentRegistry != null)
                {
                    m_propertyEditor.DocumentRegistry.DocumentRemoved -= DocumentRegistry_DocumentRemoved;
                    m_propertyEditor.DocumentRegistry.ActiveDocumentChanged -= DocumentRegistry_ActiveDocumentChanged;
                }                                
                if (m_observableContext != null)
                {                     
                    m_observableContext.ItemChanged -= observableContext_ItemChanged;
                    m_observableContext.ItemInserted -= observableContext_ItemInserted;
                    m_observableContext.ItemRemoved -= observableContext_ItemRemoved;
                    m_observableContext = null;
                }

                if (m_validationContext != null)
                {
                    m_validationContext.Beginning -= validationContext_Beginning;
                    m_validationContext.Ended -= validationContext_Ended;
                    m_validationContext.Cancelled -= validationContext_Cancelled;
                    m_validationContext = null;
                }

                
                m_propertyEditingContext = null;
                m_propertyGrid.Bind(null);
                m_propertyGrid = null;
            }

            #region IControlHostClient Members

            void IControlHostClient.Activate(Control control)
            {
                
            }

            void IControlHostClient.Deactivate(Control control)
            {                
            }

            bool IControlHostClient.Close(Control control)
            {
                ClearState();
                return true;
            }

            #endregion

            private void OnPropertyGridMouseUp(MouseEventArgs e)
            {

                if (m_propertyEditor.CommandService != null
                    && m_propertyGrid.PropertyGridView.EditingContext != null)
                {
                    // if no property is specified, return the whole property editing context
                    Point clientPt = new Point(e.X, e.Y);
                    IPropertyEditingContext context;
                    object target = m_propertyGrid.GetDescriptorAt(clientPt, out context);
                    if (target == null) target = context;

                    IEnumerable<object> commands = m_propertyEditor.m_contextMenuCommandProviders.GetCommands(context, target)
                        .Where(x => !m_propertyEditor.IsStandardEditCommand(x)); // filter out standard edit commands as they are not applicable for property editing,
                    // even if the current active context supports IInstancingContext.

                    Point screenPoint = m_propertyGrid.PointToScreen(clientPt);

                    m_propertyEditor.CommandService.RunContextMenu(commands, screenPoint);
                }
            }

            private ToolStripButton m_selectionButton;

            // the cloned property editor, edits properties 
            // of the objects belong to this context.
            // if this context removed then property editor need 
            // to be closed.
            private object m_context;
            private IObservableContext m_observableContext;
            private IValidationContext m_validationContext;
            private CustomPropertyEditingContext m_propertyEditingContext;
            private static int s_cloneId;
            private PropertyGrid m_propertyGrid;
            private ControlInfo m_controlInfo;
            private PropertyEditor m_propertyEditor;

            private static bool AdaptableEquals(object a, object b)
            {
                if (a == null || b == null)
                {
                    return a == null && b == null;
                }

                if (a.Equals(b))
                    return true;

                Path<object> pathA = a as Path<object>;
                if (pathA != null && pathA.Contains(b))
                    return true;

                Path<object> pathB = b as Path<object>;
                if (pathB != null && pathB.Contains(a))
                    return true;

                return false;
            }
            private class CustomPropertyEditingContext : IPropertyEditingContext, IObservableContext, IAdaptable
            {
               
                public CustomPropertyEditingContext(IPropertyEditingContext context)
                {
                    m_transactionContext = context.As<ITransactionContext>();
                    m_items.AddRange(context.Items);
                }

                public void RemoveItem(object item)
                {                                      
                    for(int i =0; i < m_items.Count;)
                    {                        
                        if (AdaptableEquals(m_items[i],item))
                        {
                            m_items.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }                    
                }

                #region IAdaptable Members

                object IAdaptable.GetAdapter(Type type)
                {                    
                    if (type.IsAssignableFrom(GetType()))
                        return this;
                    else if (type == typeof(ITransactionContext))
                        return m_transactionContext;                   
                    return null;
                }

                #endregion
                private ITransactionContext m_transactionContext;

                #region IPropertyEditingContext Members
                private List<object> m_items = new List<object>();
                public IEnumerable<object> Items
                {
                    get { return m_items; }
                }

                public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
                {
                    get { return PropertyUtils.GetSharedProperties(m_items); }
                }

                #endregion


                /// <summary>
                /// Raises the Reloaded event</summary>
                /// <param name="e">Event args</param>
                public void OnReloaded()
                {
                    Reloaded.Raise(this, EventArgs.Empty);
                }

                #region IObservableContext Members

                /// <summary>
                /// Event that is raised when an item is inserted</summary>
                public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted
                {
                    add { } // not supported
                    remove { }
                }

                /// <summary>
                /// Event that is raised when an item is removed</summary>
                public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved
                {
                    add { } // not supported
                    remove { }
                }

                /// <summary>
                /// Event that is raised when an item is changed</summary>
                public event EventHandler<ItemChangedEventArgs<object>> ItemChanged
                {
                    add { } // not supported
                    remove { }
                }

                /// <summary>
                /// Event that is raised when collection has been reloaded</summary>
                public event EventHandler Reloaded;

                #endregion
            }
        }
    }
}
