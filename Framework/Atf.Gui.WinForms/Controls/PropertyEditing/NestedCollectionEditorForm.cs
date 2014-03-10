using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Nested collections editor form</summary>
    public partial class NestedCollectionEditorForm : Form
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that can be used to gain additional context information</param>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="value">An instance of the value being edited</param>
        /// <param name="getCollectionItemCreators">Callback for getting available types and constructor arguments (object []) 
        /// to create and add to this collection and its sub-collections</param>
        /// <param name="getItemInfo">Callback for getting item's display information</param>
        public NestedCollectionEditorForm(ITypeDescriptorContext context, ISelectionContext selectionContext, object value, Func<Path<object>, IEnumerable<Pair<Type, NestedCollectionEditor.CreateCollectionObject>>> getCollectionItemCreators, Func<object, ItemInfo, bool> getItemInfo)
        {
            InitializeComponent();

            addButton.Click += addButton_Click;
            deleteButton.Click += deleteButton_Click;
            upButton.Click += upButton_Click;
            downButton.Click += downButton_Click;
            FormClosed += nestedCollectionEditorForm_FormClosed;
            GetCollectionItemCreators = getCollectionItemCreators;

            Bind(context, selectionContext, value, getItemInfo);
        }

        /// <summary>
        /// Gets or sets a delegate to get available types to create and add to this collection and its sub-collections</summary>
        public Func<Path<object>, IEnumerable<Pair<Type, NestedCollectionEditor.CreateCollectionObject>>> GetCollectionItemCreators
        {
            get { return m_getCollectionItemCreators; }
            set { m_getCollectionItemCreators = value; }
        }


        private void Bind(ITypeDescriptorContext context, ISelectionContext selectionContext, object value, Func<object, ItemInfo, bool> getItemInfo)
        {
            splitContainer1.SuspendLayout();

            m_treeControl = new TreeControl(TreeControl.Style.SimpleTree);
            m_treeControl.Dock = DockStyle.Fill;
            m_treeControl.SelectionMode = SelectionMode.One;
            m_treeControl.ImageList = ResourceUtil.GetImageList16();
            m_treeControl.Location = new Point(comboBox1.Location.X, comboBox1.Location.Y + 2 * FontHeight);
            m_treeControl.Width = upButton.Location.X - m_treeControl.Location.X - Margin.Right - Margin.Left;
            m_treeControl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            m_treeControl.Height = splitContainer1.Panel1.Height - m_treeControl.Location.Y - (Margin.Bottom + Margin.Top);
            splitContainer1.Panel1.Controls.Add(m_treeControl);

            m_propertyGrid = new PropertyGrid();
            m_defaultContext.SelectionContext = selectionContext;
            m_propertyGrid.Bind(m_defaultContext);
            m_propertyGrid.Dock = DockStyle.Fill;
            splitContainer1.Panel2.Controls.Add(m_propertyGrid);

            splitContainer1.ResumeLayout();

            m_rootValue = value;
            m_collectionTreeView = new TreeView(context, value);
            m_collectionTreeView.GetItemInfo = getItemInfo;
            m_treeControlAdapter = new TreeControlAdapter(m_treeControl);
            m_collectionTreeView.SelectionChanged += collectionTreeView_SelectionChanged;
            m_treeControl.ShowRoot = false;
            m_treeControlAdapter.TreeView = m_collectionTreeView;
            m_context = context;
            UpdateAvailaibleTypes(new Path<object>(context.Instance));

        }

        /// <summary>
        /// Performs custom actions on TreeView SelectionChanged events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void collectionTreeView_SelectionChanged(object sender, EventArgs e)
        {
            Path<object> lastPath = m_collectionTreeView.LastSelected as Path<object>;
            if (lastPath == null)
                m_propertyGrid.Bind(null);
            else
                m_propertyGrid.Bind(lastPath.Last);
            UpdateAvailaibleTypes(lastPath);

        }

        private void addButton_Click(object sender, EventArgs e)
        {
            foreach (Pair<Type, NestedCollectionEditor.CreateCollectionObject> kvp in m_availaibleTypeCreators)
            {
                if (kvp.First.Name == comboBox1.Text)
                {
                    // insert instance based on the current selection in the tree view
                    Path<object> lastPath = m_collectionTreeView.LastSelected as Path<object>;

                    object instance = null;
                    if (kvp.Second == null)
                        instance = Activator.CreateInstance(kvp.First, true);
                    else
                        instance = kvp.Second(m_rootValue);

                    if (lastPath == null || lastPath.Count == 1) // root
                    {
                        IList listToInsert = m_rootValue as IList;
                        if (listToInsert != null)
                        {
                            ITransactionContext transactionContext = m_defaultContext.As<ITransactionContext>();
                            transactionContext.DoTransaction(delegate
                                {
                                    listToInsert.Add(instance);
                                },
                                "Added Collection Item".Localize());

                            m_treeControlAdapter.Refresh(m_collectionTreeView.Root);
                        }
                    }
                    else
                    {
                        IList listToInsert = m_collectionTreeView.ItemCollection(lastPath.Last) as IList;
                        if (listToInsert != null)
                        {
                            int index = listToInsert.IndexOf(lastPath.Last);
                            listToInsert.Insert(index + 1, instance);
                            m_treeControlAdapter.Refresh(m_collectionTreeView.Root);
                        }
                    }

                    // select the newly added item
                    foreach (var path in m_treeControlAdapter.GetPaths(instance))
                    {
                        m_collectionTreeView.Selection = new object[] { path };
                        break;
                    }
                    RefreshOwnerForm();
                    m_treeControl.Focus();

                    break;
                }
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            var lastPath = m_collectionTreeView.LastSelected as Path<object>;
            if (lastPath != null)
            {
                var listToRemove = m_collectionTreeView.ItemCollection(lastPath.Last) as IList;
                if (listToRemove != null)
                {
                    int index = listToRemove.IndexOf(lastPath.Last);
                    int next = index + 1 < listToRemove.Count ? index + 1 : index - 1;
                    object nextItem = next >= 0 ? listToRemove[next] : null;

                    var transactionContext = m_defaultContext.As<ITransactionContext>();
                    transactionContext.DoTransaction(delegate { listToRemove.Remove(lastPath.Last); },
                                                     "Removed Collection Item".Localize());

                    m_collectionTreeView.RemoveItemCollection(lastPath.Last);
                    if (nextItem != null)
                    {
                        foreach (var path in m_treeControlAdapter.GetPaths(nextItem))
                        {
                            m_collectionTreeView.Selection = new object[] { path };
                            break;
                        }
                    }
                    else
                    {
                        m_collectionTreeView.Selection = EmptyEnumerable<object>.Instance;
                        UpdateAvailaibleTypes(new Path<object>(m_context.Instance));
                    }

                    m_treeControlAdapter.Refresh(m_collectionTreeView.Root);
                }
                RefreshOwnerForm();
                m_treeControl.Focus();
            }
        }

        /// <summary>
        /// Performs custom actions on Down button click events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void downButton_Click(object sender, EventArgs e)
        {
            // moves the selected item one position down in its collection
            Path<object> lastPath = m_collectionTreeView.LastSelected as Path<object>;
            if (lastPath != null && lastPath.Count > 1) // not a root
            {
                IList listToInsert = m_collectionTreeView.ItemCollection(lastPath.Last) as IList;
                if (listToInsert != null)
                {
                    int index = listToInsert.IndexOf(lastPath.Last);
                    if (index >= 0 && index < listToInsert.Count - 1)
                    {
                        ITransactionContext transactionContext = m_defaultContext.As<ITransactionContext>();
                        transactionContext.DoTransaction(delegate
                        {

                            listToInsert.RemoveAt(index);
                            listToInsert.Insert(index + 1, lastPath.Last);
                        },
                        "Moved Collection Item Down".Localize());
                        m_treeControlAdapter.Refresh(m_collectionTreeView.Root);
                    }
                }
            }

            RefreshOwnerForm();
            m_treeControl.Focus();
        }

        /// <summary>
        /// Performs custom actions on Up button click events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void upButton_Click(object sender, EventArgs e)
        {
            // moves the selected item one position up in its collection
            Path<object> lastPath = m_collectionTreeView.LastSelected as Path<object>;
            if (lastPath != null && lastPath.Count > 1) // not a root
            {
                IList listToInsert = m_collectionTreeView.ItemCollection(lastPath.Last) as IList;
                if (listToInsert != null)
                {
                    int index = listToInsert.IndexOf(lastPath.Last);
                    if (index > 0)
                    {
                        ITransactionContext transactionContext = m_defaultContext.As<ITransactionContext>();
                        transactionContext.DoTransaction(delegate
                        {
                            listToInsert.RemoveAt(index);
                            listToInsert.Insert(index - 1, lastPath.Last);
                        },
                        "Moved Collection Item Up".Localize());
                        m_treeControlAdapter.Refresh(m_collectionTreeView.Root);
                    }
                }
            }

            RefreshOwnerForm();
            m_treeControl.Focus();
        }

        /// <summary>
        /// Performs custom actions on FormClosed events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void nestedCollectionEditorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            RefreshOwnerForm();
        }

        /// <summary>
        /// Refreshes owner form</summary>
        void RefreshOwnerForm()
        {
            if (Owner is NestedCollectionEditorForm)
            {
                ((NestedCollectionEditorForm)Owner).ForceRefresh();
            }
        }

        /// <summary>
        /// Forces refresh of tree control</summary>
        void ForceRefresh()
        {
            m_treeControlAdapter.Refresh(m_collectionTreeView.Root);
            if (Owner is NestedCollectionEditorForm)
            {
                ((NestedCollectionEditorForm)Owner).ForceRefresh();
            }

        }

        private void UpdateAvailaibleTypes(Path<object> objectPath)
        {
            comboBox1.Items.Clear();
            m_availaibleTypeCreators.Clear();

            if (GetCollectionItemCreators != null)
            {
                foreach (Pair<Type, NestedCollectionEditor.CreateCollectionObject> typeCreator in GetCollectionItemCreators(objectPath))
                {
                    comboBox1.Items.Add(typeCreator.First.Name);
                    m_availaibleTypeCreators.Add(new Pair<Type, NestedCollectionEditor.CreateCollectionObject>(typeCreator.First, typeCreator.Second));
                }
            }

            if (!comboBox1.Items.Contains(comboBox1.Text))
            {
                if (comboBox1.Items.Count > 0)
                    comboBox1.Text = comboBox1.Items[0].ToString();
                else
                    comboBox1.Text = string.Empty;
            }
        }



        private class TreeView : ITreeView, IItemView, ISelectionContext
        {
            private readonly object mRoot;
            public TreeView(ITypeDescriptorContext context, object value)
            {
                mRoot = value;
                m_selection = new Selection<object>();
                m_selection.Changed += selection_Changed;

                // suppress compiler warning
                if (SelectionChanging == null) return;
            }

            /// <summary>
            /// A delegate for getting item's display information</summary>
            public Func<object, ItemInfo, bool> GetItemInfo
            {
                get { return m_getItemInfo; }
                set { m_getItemInfo = value; }
            }

            #region ITreeView Members

            public object Root
            {
                get { return mRoot; }
            }

            public IEnumerable<object> GetChildren(object parent)
            {
                ICollection collection = parent as ICollection;
                if (collection != null)
                {
                    foreach (object subItem in collection)
                    {
                        if (!m_itemCollection.ContainsKey(subItem))
                            m_itemCollection.Add(subItem, collection);
                        yield return subItem;
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
                if (m_getItemInfo == null || !m_getItemInfo(item, info))
                {
                    info.AllowLabelEdit = false;
                    info.Label = item.ToString();
                }
            }

            #endregion

            #region ISelectionContext Members

            public IEnumerable<object> Selection
            {
                get { return m_selection; }
                set { m_selection.SetRange(value); }
            }

            public IEnumerable<T> GetSelection<T>()
                        where T : class
            {
                return m_selection.AsIEnumerable<T>();
            }

            public object LastSelected
            {
                get { return m_selection.LastSelected; }
            }

            public T GetLastSelected<T>()
                        where T : class
            {
                return m_selection.GetLastSelected<T>();
            }

            public bool SelectionContains(object item)
            {
                return m_selection.Contains(item);
            }

            public int SelectionCount
            {
                get { return m_selection.Count; }
            }

            public event EventHandler SelectionChanging;

            public event EventHandler SelectionChanged;

            #endregion

            internal ICollection ItemCollection(object item)
            {
                ICollection result;
                m_itemCollection.TryGetValue(item, out result);
                return result;
            }

            internal void RemoveItemCollection(object item)
            {
                if (m_itemCollection.ContainsKey(item))
                    m_itemCollection.Remove(item);
            }


            private void selection_Changed(object sender, EventArgs e)
            {
                SelectionChanged.Raise(this, EventArgs.Empty);
            }

            private Func<object, ItemInfo, bool> m_getItemInfo;
            private readonly Dictionary<object, ICollection> m_itemCollection = new Dictionary<object, ICollection>();
            private readonly Selection<object> m_selection;
        }

        private PropertyGrid m_propertyGrid;
        private TreeControl m_treeControl;
        private TreeView m_collectionTreeView;
        private TreeControlAdapter m_treeControlAdapter;
        private ITypeDescriptorContext m_context;
        private Func<Path<object>, IEnumerable<Pair<Type, NestedCollectionEditor.CreateCollectionObject>>> m_getCollectionItemCreators;
        private readonly List<Pair<Type, NestedCollectionEditor.CreateCollectionObject>> m_availaibleTypeCreators = new List<Pair<Type, NestedCollectionEditor.CreateCollectionObject>>();
        private object m_rootValue;
        private readonly SelectionPropertyEditingContext m_defaultContext = new SelectionPropertyEditingContext();

    }
}
