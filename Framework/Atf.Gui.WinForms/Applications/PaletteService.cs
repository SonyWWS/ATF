//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class to manage the global palette of objects that can be dragged on to other controls</summary>
    [Export(typeof(IPaletteService))]
    [Export(typeof(IInitializable))]
    [Export(typeof(PaletteService))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public class PaletteService : TreeControlEditor, IPaletteService, IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service used to run context menus</param>
        /// <param name="controlHostService">Control host service used to register the palette control</param>
        [ImportingConstructor]
        public PaletteService(
            ICommandService commandService,
            IControlHostService controlHostService)
            : base(commandService)
        {
            m_controlHostService = controlHostService;

            m_searchInput = new StringSearchInputUI();
            m_searchInput.Updated += searchInput_Updated;

            m_control = new UserControl();
            m_control.Dock = DockStyle.Fill;
            m_control.SuspendLayout();
            m_control.Name = "Palette".Localize();
            m_control.Text = "Palette".Localize();
            m_control.Controls.Add(m_searchInput);
            m_control.Controls.Add(TreeControl);
            m_control.Layout += controls_Layout;
            m_control.ResumeLayout();

            m_controlHostService.RegisterControl(
                m_control,
                new ControlInfo(
                    "Palette".Localize(),
                    "Creates new instances".Localize(),
                    StandardControlGroup.Left, null,
                    "https://github.com/SonyWWS/ATF/search?utf8=%E2%9C%93&q=PaletteService+or+Palette".Localize()),
                this);
        }

        /// <summary>
        /// Gets or sets the category comparer. The default value is null. By default, the
        /// categories are sorted alphabetically. Set this early, before any palette items
        /// are added.</summary>
        /// <value>The category comparer, that compares two category names (strings)</value>
        /// <exception cref="System.InvalidOperationException">CategoryComparer can only be set before palette items are added</exception>
        public IComparer<string> CategoryComparer
        {
            get { return m_categoryComparer; }
            set
            {
                if (value != m_categoryComparer)
                {
                    if (m_paletteTreeAdapter != null)
                        throw new InvalidOperationException(
                            "CategoryComparer can only be set before palette items are added");
                    m_categoryComparer = value;
                }
            }
        }

        /// <summary>
        /// Configures the editor</summary>
        /// <param name="treeControl">Tree control to display data</param>
        /// <param name="treeControlAdapter">Tree control adapter to drive control</param>
        protected override void Configure(
            out TreeControl treeControl,
            out TreeControlAdapter treeControlAdapter)
        {
            treeControl = new TreeControl(TreeControl.Style.CategorizedPalette);
            treeControl.ImageList = ResourceUtil.GetImageList16();
            treeControl.AllowDrop = true;
            treeControl.SelectionMode = SelectionMode.MultiExtended;

            treeControlAdapter = new TreeControlAdapter(treeControl);
        }

        private IComparer<string> m_categoryComparer;

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            if (m_mainWindow == null &&
                m_mainForm != null)
            {
                m_mainWindow = new MainFormAdapter(m_mainForm);
            }

            if (m_mainWindow == null)
                throw new InvalidOperationException("Can't get main window");

            m_mainWindow.Loading += mainWindow_Loaded;
        }

        #endregion

        
        /// <summary>
        /// Gets or sets an XML string representing the currently expanded categories</summary>
        /// <remarks>Intended for persisting expanded categories to the settings XML file only</remarks>
        internal string ExpandedCategories
        {
            get
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
                XmlElement root = xmlDoc.CreateElement("Categories");
                xmlDoc.AppendChild(root);

                Tree<object> expansion = TreeControlAdapter.GetExpansion();

                foreach (Tree<object> subTree in expansion.Children)
                {
                    XmlElement columnElement = xmlDoc.CreateElement("Category");
                    root.AppendChild(columnElement);

                    columnElement.SetAttribute("Name", (string)subTree.Value);
                    bool expanded = !subTree.IsLeaf;
                    columnElement.SetAttribute("Expanded", expanded.ToString());
                }
                return xmlDoc.InnerXml;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(value);

                XmlElement root = xmlDoc.DocumentElement;
                if (root == null || root.Name != "Categories")
                    throw new Exception("Invalid DomPalette settings");

                XmlNodeList columns = root.SelectNodes("Category");
                foreach (XmlElement columnElement in columns)
                {
                    string name = columnElement.GetAttribute("Name");
                    string s = columnElement.GetAttribute("Expanded");
                    bool expanded;
                    if (bool.TryParse(s, out expanded))
                    {
                        if (expanded)
                            TreeControlAdapter.Expand(name);
                        else
                            TreeControlAdapter.Collapse(name);
                    }
                }
            }
        }

        #region IPaletteService Members

        /// <summary>
        /// Adds an item to the palette in the given category</summary>
        /// <param name="item">Palette item</param>
        /// <param name="categoryName">Category name</param>
        /// <param name="client">Client that instantiates item during drag-drop operations</param>
        public void AddItem(object item, string categoryName, IPaletteClient client)
        {
            if (m_objectClients.ContainsKey(item))
                throw new InvalidOperationException("duplicate item");

            if (categoryName != null)
            {
                m_objectClients.Add(item, client);
                TreeAdapter.AddItem(item, categoryName);
            }
        }

        /// <summary>
        /// Removes an item from the palette</summary>
        /// <param name="item">Item to remove</param>
        public void RemoveItem(object item)
        {
            TreeAdapter.RemoveItem(item);

            m_objectClients.Remove(item);
        }

        #endregion

        /// <summary>
        /// Clears all items from the palette</summary>
        public void RemoveAllItems()
        {
            m_searchInput.ClearSearch();

            TreeAdapter.RemoveAllItems();

            m_objectClients.Clear();
            TreeControl.Root.Clear();

            TreeAdapter.RefreshControl();
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
        /// another Control or "host" control gets focus.</summary>
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

        private void controls_Layout(object sender, LayoutEventArgs e)
        {
            int yoffset = m_searchInput.Visible ? m_searchInput.Height : 0;
            TreeControl.Bounds = new Rectangle(0, yoffset, m_control.Width, m_control.Height - yoffset);
        }

        /// <summary>
        /// Method called for every mouse move event in which drag-and-drop objects are dragged.
        /// Begins a drag-and-drop operation on the underlying tree control.</summary>
        /// <param name="items">Enumeration of items being dragged</param>
        protected override void OnStartDrag(IEnumerable<object> items)
        {
            List<object> convertedItems = new List<object>();
            foreach (object item in items)
            {
                IPaletteClient client;
                if (m_objectClients.TryGetValue(item, out client))
                {
                    object convertedItem = client.Convert(item);
                    if (convertedItem != null)
                        convertedItems.Add(convertedItem);
                }
            }

            if (convertedItems.Count > 0)
            {
                TreeControl.DoDragDrop(convertedItems.ToArray(), DragDropEffects.All);
            }
        }

        private void mainWindow_Loaded(object sender, EventArgs e)
        {
            TreeView = TreeAdapter;
            if (PersistExpandedCategories && m_settingsService != null)
            {                
                m_settingsService.RegisterSettings(
                    this, new BoundPropertyDescriptor(this, () => ExpandedCategories,
                        "ExpandedCategories", null, null));
            }
            else
            {
                TreeControl.ExpandAll();
            }
        }

        private void searchInput_Updated(object sender, EventArgs e)
        {
            if (TreeControl.Root == null)
                return;

            if (m_searchInput.IsNullOrEmpty())
            {
                if (m_searching)
                {
                    // get the tree control to force-reload the tree data
                    TreeAdapter.RefreshControl();
                    RestoreExpansion();
                }
                m_searching = false;
                return;
            }
            else
            {
                if (!m_searching)
                    RememberExpansion();
                m_searching = true;
            }

            TreeAdapter.RefreshControl();

            // expand categories that have matched children
            foreach (object category in TreeAdapter.GetChildren(TreeAdapter))
            {
                foreach (object typeName in TreeAdapter.GetChildren(category))
                {
                    ItemInfo info = new WinFormsItemInfo();
                    TreeAdapter.GetInfo(typeName, info);
                    if (m_searchInput.Matches(info.Label))
                    {
                        TreeControlAdapter.Expand(category);
                        break;
                    }
                }
            }

            RestoreExpansion();
        }

        private void RememberExpansion()
        {
            m_expandedCollections.Clear();
            foreach (string category in TreeAdapter.GetChildren(TreeAdapter))
            {
                if (TreeControlAdapter.IsExpanded(category))
                    m_expandedCollections.Add(category);
            }
        }

        private void RestoreExpansion()
        {
            foreach (string category in m_expandedCollections)
            {
                TreeControlAdapter.Expand(category);
            }
        }

        // Gets the PaletteTreeAdapter with lazy construction, so as to avoid having to call
        //  a virtual method in the constructor. Calling GetCategoryComparer() in the Initialize()
        //  might be too late, because other MEF components may call AddItem() in their Initialize()
        //  methods.
        private PaletteTreeAdapter TreeAdapter
        {
            get
            {
                if (m_paletteTreeAdapter == null)
                    m_paletteTreeAdapter = new PaletteTreeAdapter(this, m_searchInput, CategoryComparer);
                return m_paletteTreeAdapter;
            }
        }

        // class to adapt IPaletteContext to the ITreeView required by TreeControlAdapter
        private class PaletteTreeAdapter : ITreeView, IItemView, IObservableContext
        {
            public PaletteTreeAdapter(PaletteService paletteService, StringSearchInputUI searchInput,
                IComparer<string> categoryComparer)
            {
                m_paletteService = paletteService;
                m_searchInput = searchInput;
                m_categories = new SortedDictionary<string, List<object>>(categoryComparer);

                if (ItemChanged == null) return; // inhibit compiler warning
                if (Reloaded == null) return;
            }
            private readonly PaletteService m_paletteService;
            private readonly StringSearchInputUI m_searchInput;

            public void AddItem(object item, string categoryName)
            {
                int index;
                List<object> category;
                if (!m_categories.TryGetValue(categoryName, out category))
                {
                    index = m_categories.Count;
                    category = new List<object>();
                    m_categories.Add(categoryName, category);

                    ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(index, categoryName, this));
                }
                index = category.Count;
                category.Add(item);

                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(index, item, categoryName));
            }

            public void RemoveItem(object item)
            {
                // not very efficient, but hopefully not a common occurrence
                foreach (KeyValuePair<string, List<object>> pair in m_categories)
                {
                    int index = pair.Value.IndexOf(item);
                    if (index >= 0)
                    {
                        pair.Value.RemoveAt(index);
                        ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(index, item, pair.Key));
                        break;
                    }
                }
            }

            public void RemoveAllItems()
            {
                m_categories.Clear();
            }
            
            public void RefreshControl()
            {
                Reloaded.Raise(this, null);
            }

            #region ITreeView Members

            /// <summary>
            /// Gets the root object of the tree</summary>
            public object Root
            {
                get { return this; }
            }

            /// <summary>
            /// Gets the children of the given parent object</summary>
            /// <param name="parent">Parent object</param>
            /// <returns>Enumeration of children of the parent object</returns>
            public IEnumerable<object> GetChildren(object parent)
            {
                if (parent == this)
                {
                    foreach (string categoryName in m_categories.Keys)
                        yield return categoryName;
                }
                else
                {
                    string categoryName = parent as string;
                    if (categoryName != null)
                    {
                        List<object> category;
                        if (m_categories.TryGetValue(categoryName, out category))
                        {
                            foreach (object item in category)
                            {
                                ItemInfo info = new WinFormsItemInfo();
                                GetInfo(item, info);
                                if (m_searchInput.IsNullOrEmpty() || 
                                    m_searchInput.Matches(info.Label))
                                    yield return item;
                            }
                        }
                    }
                }
            }

            #endregion

            #region IItemView Members

            /// <summary>
            /// Gets item's display information</summary>
            /// <param name="item">Item being displayed</param>
            /// <param name="info">Item info, to fill out</param>
            public void GetInfo(object item, ItemInfo info)
            {
                info.AllowLabelEdit = false;

                if (item != this)
                {
                    string categoryName = item as string;
                    if (categoryName != null && m_categories.ContainsKey(categoryName))
                    {
                        info.Label = categoryName;
                    }
                    else
                    {
                        IPaletteClient client = m_paletteService.m_objectClients[item];
                        client.GetInfo(item, info);
                    }
                }
            }

            #endregion

            #region IObservableContext Members

            /// <summary>
            /// Event that is raised when an item is inserted</summary>
            public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

            /// <summary>
            /// Event that is raised when an item is removed</summary>
            public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

            /// <summary>
            /// Event that is raised when an item is changed</summary>
            public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

            /// <summary>
            /// Event that is raised when collection has been reloaded</summary>
            public event EventHandler Reloaded;

            #endregion

            private readonly SortedDictionary<string, List<object>> m_categories;
        }

        /// <summary>
        /// Gets or sets a value determining whether expanded categories are saved to settings
        /// when closing and restored when restarting the application</summary>
        public bool PersistExpandedCategories
        {
            get { return m_persistExpandedCategories; }
            set { m_persistExpandedCategories = value; }
        }

        private readonly IControlHostService m_controlHostService;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;

        [Import(AllowDefault = true)]
        private IMainWindow m_mainWindow;

        [Import(AllowDefault = true)]
        private Form m_mainForm;

        private readonly UserControl m_control;
        private readonly StringSearchInputUI m_searchInput;
        private bool m_searching;
        private bool m_persistExpandedCategories = true;
        private readonly List<string> m_expandedCollections = new List<string>();

        private PaletteTreeAdapter m_paletteTreeAdapter;
        private readonly Dictionary<object, IPaletteClient> m_objectClients =
            new Dictionary<object, IPaletteClient>();
    }
}
