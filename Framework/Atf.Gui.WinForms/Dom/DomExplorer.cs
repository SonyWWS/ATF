//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Component to implement a DOM explorer, which is a tree view and property grid for exploring a DOM.
    /// It is useful as a raw view on DOM data for diagnosing DOM problems.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(DomExplorer))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public class DomExplorer : IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="controlHostService">Control host service</param>
        [ImportingConstructor]
        public DomExplorer(IControlHostService controlHostService)
        {
            m_controlHostService = controlHostService;

            m_treeControl = new TreeControl();
            m_treeControl.Dock = DockStyle.Fill;
            m_treeControl.AllowDrop = true;
            m_treeControl.SelectionMode = SelectionMode.MultiExtended;
            m_treeControl.ImageList = ResourceUtil.GetImageList16();
            m_treeControl.NodeSelectedChanged += treeControl_NodeSelectedChanged;

            m_treeControlAdapter = new TreeControlAdapter(m_treeControl);
            m_treeView = new TreeView();

            m_propertyGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
            m_propertyGrid.Dock = DockStyle.Fill;

            m_splitContainer = new SplitContainer();
            m_splitContainer.Text = "Dom Explorer";
            m_splitContainer.Panel1.Controls.Add(m_treeControl);
            m_splitContainer.Panel2.Controls.Add(m_propertyGrid);
        }

        /// <summary>
        /// Gets or sets the root DomNode to explore</summary>
        public DomNode Root
        {
            get { return m_treeView.RootNode; }
            set
            {
                if (value != null)
                {
                    m_treeView.RootNode = value;
                    m_treeControlAdapter.TreeView = m_treeView;
                }
                else
                {
                    m_treeControlAdapter.TreeView = null;
                    m_treeView.RootNode = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether DomNode adapters are displayed in the tree view</summary>
        public bool ShowAdapters
        {
            get { return m_treeView.ShowAdapters; }
            set { m_treeView.ShowAdapters = value; }
        }

        /// <summary>
        /// Gets the TreeControl</summary>
        public TreeControl TreeControl
        {
            get { return m_treeControl; }
        }

        /// <summary>
        /// Gets the TreeControlAdapter</summary>
        public TreeControlAdapter TreeControlAdapter
        {
            get { return m_treeControlAdapter; }
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering control</summary>
        public virtual void Initialize()
        {
            m_controlHostService.RegisterControl(m_splitContainer,
                "DOM Explorer".Localize(),
                "Generic View of DOM".Localize(),
                StandardControlGroup.Bottom,
                this);
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Activates the client control</summary>
        /// <param name="control">Client control to be activated</param>
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
        /// <returns><c>True</c> if the control can close, or false to cancel</returns>
        bool IControlHostClient.Close(Control control)
        {
            return true;
        }

        #endregion

        private void treeControl_NodeSelectedChanged(object sender, TreeControl.NodeEventArgs e)
        {
            if (e.Node.Selected)
            {
                object item = e.Node.Tag;
                {
                    DomNode node = item as DomNode;
                    if (node != null)
                    {
                        // Build property descriptors for node's attributes
                        List<PropertyDescriptor> descriptors = new List<PropertyDescriptor>();
                        foreach (AttributeInfo attributeInfo in node.Type.Attributes)
                        {
                            descriptors.Add(
                                new AttributePropertyDescriptor(
                                    attributeInfo.Name,
                                    attributeInfo,
                                    "Attributes",
                                    null,
                                    true));
                        }

                        // use property collection wrapper to expose the descriptors to the property grid
                        m_propertyGrid.Bind(new PropertyCollectionWrapper(descriptors.ToArray(), node));
                    }
                    else // for NodeAdapters
                    {
                        // Treat NodeAdapters like normal .NET objects and expose directly to the property grid
                        DomNodeAdapter adapter = item as DomNodeAdapter;
                        m_propertyGrid.Bind(adapter);
                    }
                }
            }
        }

        private readonly IControlHostService m_controlHostService;
        private readonly TreeControl m_treeControl;
        private readonly SplitContainer m_splitContainer;
        private readonly TreeControlAdapter m_treeControlAdapter;
        private readonly Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyGrid;
        private readonly TreeView m_treeView;

        private class TreeView : ITreeView, IItemView, IObservableContext
        {
            public DomNode RootNode
            {
                get { return m_root; }
                set
                {
                    if (m_root != null)
                    {
                        m_root.AttributeChanged -= root_AttributeChanged;
                        m_root.ChildInserted -= root_ChildInserted;
                        m_root.ChildRemoving -= root_ChildRemoving;
                        m_root.ChildRemoved -= root_ChildRemoved;
                    }

                    m_root = value;

                    if (m_root != null)
                    {
                        m_root.AttributeChanged += root_AttributeChanged;
                        m_root.ChildInserted += root_ChildInserted;
                        m_root.ChildRemoving += root_ChildRemoving;
                        m_root.ChildRemoved += root_ChildRemoved;
                    }

                    Reloaded.Raise(this, EventArgs.Empty);
                }
            }

            public bool ShowAdapters
            {
                get { return m_showAdapters; }
                set { m_showAdapters = value; }
            }

            #region ITreeView Members

            public object Root
            {
                get { return m_root; }
            }

            public IEnumerable<object> GetChildren(object parent)
            {
                DomNode node = parent as DomNode;
                if (node != null)
                {
                    if (m_showAdapters)
                    {
                        // get all adapters, and wrap so that the TreeControlAdapter doesn't confuse
                        //  them with their parent DomNode; remember that the DomNode and its adapters
                        //  are logically Equal.
                        IEnumerable<DomNodeAdapter> adapters = node.AsAll<DomNodeAdapter>();
                        foreach (DomNodeAdapter adapter in adapters)
                            yield return new Adapter(adapter);
                    }
                    // get child Dom objects
                    foreach (DomNode child in node.Children)
                        yield return child;
                }
            }

            #endregion

            #region IItemView Members

            public void GetInfo(object item, ItemInfo info)
            {
                info.IsLeaf = !HasChildren(item);

                DomNode node = item as DomNode;
                if (node != null && node.ChildInfo != null)
                {
                    info.Label = node.ChildInfo.Name;
                    //info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.DomObjectImage);
                    return;
                }

                Adapter adapter = item as Adapter;
                if (adapter != null)
                {
                    DomNodeAdapter nodeAdapter = adapter.Adaptee as DomNodeAdapter;
                    StringBuilder sb = new StringBuilder();

                    Type type = nodeAdapter.GetType();
                    sb.Append(type.Name);
                    sb.Append(" (");
                    foreach (Type interfaceType in type.GetInterfaces())
                    {
                        sb.Append(interfaceType.Name);
                        sb.Append(",");
                    }
                    sb[sb.Length - 1] = ')'; // remove trailing comma

                    info.Label = sb.ToString();
                    //info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.DomObjectInterfaceImage);

                    return;
                }
            }

            #endregion

            #region IObservableContext Members

            /// <summary>
            /// Event that is raised when a tree item is inserted</summary>
            public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

            /// <summary>
            /// Event that is raised when a tree item is removed</summary>
            public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

            /// <summary>
            /// Event that is raised when a tree item is changed</summary>
            public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

            /// <summary>
            /// Event that is raised when the tree is reloaded</summary>
            public event EventHandler Reloaded;

            #endregion

            public bool HasChildren(object item)
            {
                foreach (object child in (this).GetChildren(item))
                    return true;
                return false;
            }

            private void root_AttributeChanged(object sender, AttributeEventArgs e)
            {
                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
            }

            private void root_ChildInserted(object sender, ChildEventArgs e)
            {
                int index = GetChildIndex(e.Child, e.Parent);
                if (index >= 0)
                {
                    ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(index, e.Child, e.Parent));
                }
            }

            private void root_ChildRemoving(object sender, ChildEventArgs e)
            {
                m_lastRemoveIndex = GetChildIndex(e.Child, e.Parent);
            }

            private void root_ChildRemoved(object sender, ChildEventArgs e)
            {
                if (m_lastRemoveIndex >= 0)
                {
                    ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(m_lastRemoveIndex, e.Child, e.Parent));
                }
            }

            private int GetChildIndex(object child, object parent)
            {
                // get child index by re-constructing what we'd give the tree control
                IEnumerable<object> treeChildren = GetChildren(parent);
                int i = 0;
                foreach (object treeChild in treeChildren)
                {
                    if (treeChild.Equals(child))
                        return i;
                    i++;
                }
                return -1;
            }

            private DomNode m_root;
            private int m_lastRemoveIndex;
            private bool m_showAdapters = true;
        }
    }
}
