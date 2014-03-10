//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Embedded property editor for adding, removing and editing items in a collection</summary>
    /// <remarks>Supports multiple ItemInserters, allowing you to insert child items of
    /// various types (typically derived from a common base class).</remarks>
    public class EmbeddedCollectionEditor : IPropertyEditor, IAnnotatedParams
    {
        #region IAnnotatedParams Members

        /// <summary>
        /// Initializes the control. Sets the Parameters property to the received annotation parameters.</summary>
        /// <param name="parameters">Annotation parameters</param>
        public void Initialize(string[] parameters)
        {
            Parameters = parameters;
        }

        #endregion


        #region IPropertyEditor Implementation

        /// <summary>
        /// Gets a control to edit the given property</summary>
        /// <param name="context">Context for property editing control</param>
        /// <returns>Control to edit the given context</returns>
        public Control GetEditingControl(PropertyEditorControlContext context)
        {
            if (context.LastSelectedObject == null)
                return null;

            bool toolStripLabelEnabled = (Parameters != null && Parameters.Length > 2 && Boolean.Parse(Parameters[2]));
            return new CollectionControl(this, context, toolStripLabelEnabled);
        }

        #endregion


        /// <summary>
        /// Gets custom parameters read from a schema file</summary>
        public string[] Parameters { get; private set; }

        /// <summary>
        /// Gets or sets a delegate to retrieve the ItemInserters based on the current context</summary>
        public Func<PropertyEditorControlContext, IEnumerable<ItemInserter>> GetItemInsertersFunc { get; set; }

        /// <summary>
        /// Gets or sets a delegate to remove selected child items</summary>
        public Action<PropertyEditorControlContext, object> RemoveItemFunc { get; set; }

        /// <summary>
        /// Gets or sets a delegate to move selected child items</summary>
        public Action<PropertyEditorControlContext, object, int> MoveItemFunc { get; set; }

        /// <summary>
        /// Implements logic to insert items into a collection while also containing
        /// metadata (ItemTypeName, Image) that is used in a GUI control</summary>
        public class ItemInserter
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="itemTypeName">Type name of the item to be inserted</param>
            /// <param name="insertItemFunc">Delegate that creates, inserts and returns the new item</param>
            public ItemInserter(string itemTypeName, Func<object> insertItemFunc)
            {
                ItemTypeName = itemTypeName;
                InsertItemFunc = insertItemFunc;
            }

            /// <summary>
            /// Constructor</summary>
            /// <param name="itemTypeName">Type name of the item to be inserted</param>
            /// <param name="image">Optional icon image for representing the inserter</param>
            /// <param name="insertItemFunc">Delegate that creates, inserts and returns the new item</param>
            public ItemInserter(string itemTypeName, Image image, Func<object> insertItemFunc)
            {
                ItemTypeName = itemTypeName;
                Image = image;
                InsertItemFunc = insertItemFunc;
            }

            /// <summary>
            /// Gets or sets a human-friendly type name of the item to be inserted.
            /// Doesn't have to correspond to a sytem type.</summary>
            public string ItemTypeName { get; set; }

            /// <summary>
            /// Gets or sets an optional icon image for representing the inserter in a GUI control</summary>
            public Image Image { get; set; }

            /// <summary>
            /// Gets or sets a delegate that actually creates, inserts and returns the new item</summary>
            public Func<object> InsertItemFunc { get; set; }
        }


        /// <summary>
        /// Control for a collection of child items</summary>
        /// <remarks>This control is embedded into a property grid</remarks>
        private class CollectionControl : Control, ICacheablePropertyControl
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="editor">Embedded property editor</param>
            /// <param name="context">Context for embedded property editing controls</param>
            /// <param name="toolStripLabelsEnabled">Whether toolstrip labels are enabled or not</param>
            public CollectionControl(EmbeddedCollectionEditor editor, PropertyEditorControlContext context, bool toolStripLabelsEnabled)
            {
                m_editor = editor;
                m_context = context;

                // Get active contexts and subscribe to ContextChanged event
                IContextRegistry contextRegistry = m_context.ContextRegistry;
                if (contextRegistry != null)
                {
                    contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
                    ObservableContext = contextRegistry.GetActiveContext<IObservableContext>();
                    ValidationContext = contextRegistry.GetActiveContext<IValidationContext>();
                    TransactionContext = contextRegistry.GetActiveContext<ITransactionContext>();
                }
                else if (context.TransactionContext != null)
                {
                    ObservableContext = context.TransactionContext.As<IObservableContext>();
                    ValidationContext = context.TransactionContext.As<IValidationContext>();
                    TransactionContext = context.TransactionContext;
                }

                // Initialize Controls

                m_toolStrip = new ToolStrip { Dock = DockStyle.Top };
                m_toolStrip.PreviewKeyDown += m_toolStrip_PreviewKeyDown;
                m_toolStrip.KeyDown += m_toolStrip_OnKeyDown;

                m_addButton = new ToolStripButton
                {
                    Text = "Add".Localize(),
                    Image = s_addImage,
                    Enabled = false,
                    DisplayStyle = ToolStripItemDisplayStyle.Image
                };
                m_addButton.Click += addButton_Click;
                m_toolStrip.Items.Add(m_addButton);

                m_addSplitButton = new ToolStripSplitButton { Image = s_addImage, Visible = false };
                m_addSplitButton.ButtonClick += addButton_Click;
                m_toolStrip.Items.Add(m_addSplitButton);

                m_deleteButton = new ToolStripButton
                {
                    Text = "Delete".Localize(),
                    Image = s_removeImage,
                    DisplayStyle = ToolStripItemDisplayStyle.Image
                };
                UpdateDeleteButton(false);
                m_deleteButton.Click += deleteButton_Click;
                m_toolStrip.Items.Add(m_deleteButton);

                m_upButton = new ToolStripButton
                {
                    Text = "Up".Localize("this is the name of a button that causes the selected item to be moved up in a list"),
                    Image = s_upImage,
                    DisplayStyle = ToolStripItemDisplayStyle.Image
                };
                m_upButton.Click += upButton_Click;
                m_toolStrip.Items.Add(m_upButton);

                m_downButton = new ToolStripButton
                {
                    Text = "Down".Localize("this is the name of a button that causes the selected item to be moved down in a list"),
                    Image = s_downImage,
                    DisplayStyle = ToolStripItemDisplayStyle.Image
                };
                m_downButton.Click += downButton_Click;
                m_toolStrip.Items.Add(m_downButton);

                UpdateMoveButtons(false);

                m_itemsCountLabel = new ToolStripStatusLabel
                {
                    Text = "[0 items]",
                    DisplayStyle = ToolStripItemDisplayStyle.Text
                };
                m_toolStrip.Items.Add(m_itemsCountLabel);

                Controls.Add(m_toolStrip);
                Height = m_toolStrip.Height;
                m_toolStrip.GripStyle = ToolStripGripStyle.Hidden;

                if (toolStripLabelsEnabled)
                    m_toolStrip.SizeChanged += toolStrip_SizeChanged;
            }

            private void m_toolStrip_OnKeyDown(object sender, KeyEventArgs e)
            {
                //Cancel the Tab keypress and give it to our parent Control instead, to support the Tab key to
                //  cycle between properties in the property editor.
                if (e.KeyData == Keys.Tab || e.KeyData == (Keys.Tab | Keys.Shift))
                {
                    e.SuppressKeyPress = true; //also sets Handled to true.
                    const int WM_KEYDOWN = 0x100;
                    PostMessage(Parent.Handle, WM_KEYDOWN, e.KeyValue, 0);
                }
            }

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            private static extern int PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);

            private void m_toolStrip_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
            {
                //Allow the KeyDown event to be raised for the Tab key, so that we can cancel this
                //  keypress and give it to our parent Control instead, to support the Tab key to
                //  cycle between properties in the property editor.
                if (e.KeyData == Keys.Tab || e.KeyData == (Keys.Tab | Keys.Shift))
                {
                    e.IsInputKey = true;
                }
            }

            /// <summary>
            /// Performs custom actions on toolstrip SizeChanged events</summary>
            /// <param name="sender">Sender</param>
            /// <param name="e">Event args</param>
            void toolStrip_SizeChanged(object sender, EventArgs e)
            {
                if (m_showToolStripLabels)
                    m_showToolStripLabelThreshold = m_toolStrip.Items.Cast<ToolStripItem>().Sum(item => item.Width);
                m_showToolStripLabels = m_toolStrip.Width > m_showToolStripLabelThreshold;

                ToolStripItemDisplayStyle style = m_showToolStripLabels
                    ? ToolStripItemDisplayStyle.ImageAndText
                    : ToolStripItemDisplayStyle.Image;

                m_addButton.DisplayStyle = style;
                m_addSplitButton.DisplayStyle = style;
                m_deleteButton.DisplayStyle = style;
                m_upButton.DisplayStyle = style;
                m_downButton.DisplayStyle = style;
            }

            /// <summary>
            /// Performs custom actions on ActiveContextChanged events</summary>
            /// <param name="sender">Sender</param>
            /// <param name="e">Event args</param>
            void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
            {
                ObservableContext = m_context.ContextRegistry.GetActiveContext<IObservableContext>();
                ValidationContext = m_context.ContextRegistry.GetActiveContext<IValidationContext>();
                TransactionContext = m_context.ContextRegistry.GetActiveContext<ITransactionContext>();
                
                foreach (ItemControl control in m_itemControls.Values)
                {                    
                    control.Clear();                 
                }

            }

            #region ICacheablePropertyControl

            /// <summary>
            /// Gets true iff this control can be used indefinitely, regardless of whether the associated
            /// PropertyEditorControlContext's SelectedObjects property changes, i.e., the selection changes. 
            /// This property must be constant for the life of this control.</summary>
            public bool Cacheable
            {
                get { return true; } //provides huge performance gains
            }

            #endregion

            /// <summary>
            /// Disposes of resources</summary>
            /// <param name="disposing">True to release both managed and unmanaged resources;
            /// false to release only unmanaged resources</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // Unsubscribe all events on outside objects
                    ObservableContext = null;
                    ValidationContext = null;

                    foreach (ItemControl itemControl in m_unusedItemControls)
                        itemControl.Dispose();
                    m_unusedItemControls.Clear();
                }

                base.Dispose(disposing);
            }

            /// <summary>
            /// Forces the control to invalidate its client area and immediately redraw itself and any child controls</summary>
            /// <remarks>Called when:
            /// 1. Selected collection object changes.
            /// 2. Selected property in the grid changes.
            /// 3. Items inserted or removed from the collection.
            /// If we have an IObservableContext, we only need to handle the first case.</remarks>
            public override void Refresh()
            {
                // base.Refresh();
                // Don't call base to avoid unnecessary events that lead to flickering

                // If the selection has changed or we're not in a transaction...
                object selected = m_context.LastSelectedObject;
                if ((m_activeCollectionNode != selected && selected != null) ||
                    !m_inTransaction)
                {
                    ProcessPendingChanges();
                }
            }

            private IEnumerable<object> GetItemsFromContext()
            {
                object value = m_context.Descriptor.GetValue(m_activeCollectionNode);
                IEnumerable childNodes = value as IEnumerable;
                if (childNodes != null)
                    return childNodes.AsIEnumerable<object>();
                if (value != null)
                    return new[] { value };
                return EmptyEnumerable<object>.Instance;
            }

            private IObservableContext ObservableContext
            {
                get { return m_observableContext; }
                set
                {
                    if (m_observableContext == value)
                        return;

                    if (m_observableContext != null)
                    {
                        m_observableContext.ItemInserted -= observableContext_ItemInserted;
                        m_observableContext.ItemRemoved -= observableContext_ItemRemoved;
                        m_observableContext.ItemChanged -= observableContext_ItemChanged;
                    }

                    m_observableContext = value;

                    if (m_observableContext != null)
                    {
                        m_observableContext.ItemInserted += observableContext_ItemInserted;
                        m_observableContext.ItemRemoved += observableContext_ItemRemoved;
                        m_observableContext.ItemChanged += observableContext_ItemChanged;
                    }
                }
            }

            private IValidationContext ValidationContext
            {
                set
                {
                    if (m_validationContext == value)
                        return;

                    if (m_validationContext != null)
                    {
                        m_validationContext.Beginning -= validationContext_Beginning;
                        m_validationContext.Cancelled -= validationContext_Cancelled;
                        m_validationContext.Ended -= validationContext_Ended;
                    }

                    m_validationContext = value;

                    if (m_validationContext != null)
                    {
                        m_validationContext.Beginning += validationContext_Beginning;
                        m_validationContext.Cancelled += validationContext_Cancelled;
                        m_validationContext.Ended += validationContext_Ended;
                    }
                }
            }

            private ITransactionContext TransactionContext {get;set;}

            /// <summary>
            /// Performs custom actions on ItemInserted events</summary>
            /// <param name="sender">Sender</param>
            /// <param name="e">Item inserted event args</param>
            void observableContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
            {
                if (GetItemsFromContext().Contains(e.Item))
                    OnItemInserted(e.Item);
            }

            private void OnItemInserted(object item)
            {                
                // If an item is removed and then inserted again in the same transaction
                // it was just moved, so we just need to update its position
                // If it was modified as well it will already be in the changed list
                // and will have to be rebuilt
                if (m_pendingItemsRemoved.Contains(item))
                    m_pendingItemsRemoved.Remove(item);
                else
                    m_pendingItemsInserted.Add(item);

                // If we're not in a transaction we have to process all changes right away
                if (!m_inTransaction)
                    ProcessPendingChanges();
            }

            /// <summary>
            /// Performs custom actions on ItemRemoved events</summary>
            /// <param name="sender">Sender</param>
            /// <param name="e">Item removed event args</param>
            void observableContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
            {
                if (m_itemControls.ContainsKey(e.Item))
                    OnItemRemoved(e.Item);
            }

            private void OnItemRemoved(object item)
            {                
                // If added and removed in the same transaction, add & remove
                // cancel each other out and we don't need to process them
                if (m_pendingItemsRemoved.Contains(item))
                    m_pendingItemsRemoved.Remove(item);
                else
                    m_pendingItemsRemoved.Add(item);

                // If we're not in a transaction we have to process all changes right away
                if (!m_inTransaction)
                    ProcessPendingChanges();
            }

            /// <summary>
            /// Performs custom actions on ItemChanged events</summary>
            /// <param name="sender">Sender</param>
            /// <param name="e">Item changed event args</param>
            void observableContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
            {
                ItemControl itemControl;
                if (m_itemControls.TryGetValue(e.Item, out itemControl))
                    OnItemChanged(e.Item);
            }

            private void OnItemChanged(object item)
            {
                m_pendingItemsChanged.Add(item);

                // If we're not in a transaction we have to process all changes right away
                if (!m_inTransaction)
                    ProcessPendingChanges();
            }

            /// <summary>
            /// Performs custom actions on validation beginning events</summary>
            /// <param name="sender">Sender</param>
            /// <param name="e">Event args</param>
            void validationContext_Beginning(object sender, EventArgs e)
            {
                m_inTransaction = true;
            }

            /// <summary>
            /// Performs custom actions on validation cancelled events</summary>
            /// <param name="sender">Sender</param>
            /// <param name="e">Event args</param>
            void validationContext_Cancelled(object sender, EventArgs e)
            {
                m_inTransaction = false;
                ClearPendingChanges(); // transaction cancelled, discard all pending changes
            }

            /// <summary>
            /// Performs custom actions on validation ended events</summary>
            /// <param name="sender">Sender</param>
            /// <param name="e">Event args</param>
            void validationContext_Ended(object sender, EventArgs e)
            {
                m_inTransaction = false;
                // This is causing a race condition where m_pendingItemsRemoved is accessed in ProcessPendingChanges while being modified elsewhere.
                // This is happening on Hakan and Willem's machines, at least.  Ricky to investigate.
                //ProcessPendingChanges();         
            }

            private bool m_processingPendingChanges;

            /// <summary>
            /// Processes all pending adds, removes, moves and changes</summary>
            private void ProcessPendingChanges()
            {
                if (m_processingPendingChanges)
                    return;
                try
                {
                    m_processingPendingChanges = true;
                    SuspendLayout();
                    //m_toolStrip.Enabled = false; //This sets the focus to the next control in the tab group, which changes the property selection

                    // Check if the selection has changed
                    object selected = m_context.LastSelectedObject;
                    if (m_activeCollectionNode != selected && selected != null)
                    {
                        ClearPendingChanges(); // no need to process pending changes as we're rebuilding anyway

                        m_activeCollectionNode = selected;

                        IEnumerable items = GetItemsFromContext();
                        int itemCount = items.Cast<object>().Count();

                        // Enable singleton mode iff the collection will always have exactly 1 item
                        // in this case we can hide toolbar and the item's index collumn
                        m_singletonMode =
                            (m_editor.GetItemInsertersFunc == null || !m_editor.GetItemInsertersFunc(m_context).Any()) // can't insert
                            && m_editor.RemoveItemFunc == null // can't remove
                            && itemCount == 1; // currently 1 item

                        // Set index column width to be big enough to display the highest expected index,
                        // using double the number of current items as first estimate.
                        // This could be done more accurately with MeasureString and could also be dynamically
                        // adjusted if the number exceeds the current max due to add operations.
                        m_indexColumnWidth = Math.Max(30, (itemCount * 2).ToString().Length * 10);

                        m_toolStrip.Visible = !m_singletonMode;

                        foreach (ItemControl itemControl in m_itemControls.Values)
                        {
                            itemControl.Visible = false;
                            itemControl.Clear();
                            m_unusedItemControls.Add(itemControl);
                        }
                        m_itemControls.Clear();

                        // Add items for insertion
                        foreach (object item in items)
                            m_pendingItemsInserted.Add(item);

                        UpdateAddButton();
                    }

                    // Hide controls for removed items
                    int smallestRemovedIndex = int.MaxValue;
                    foreach (object item in m_pendingItemsRemoved)
                    {
                        ItemControl itemControl;

                        if (m_itemControls.TryGetValue(item, out itemControl))
                        {
                            if (itemControl.Index < smallestRemovedIndex)
                                smallestRemovedIndex = itemControl.Index;

                            itemControl.Visible = false;
                            UnsubscribeItemEvents(itemControl);
                            m_itemControls.Remove(item);
                            itemControl.Clear();
                            m_unusedItemControls.Add(itemControl);
                        }
                    }

                    // Set new item selection after any deletes
                    if (smallestRemovedIndex != int.MaxValue)
                    {
                        int i = m_itemControls.Count - 1;
                        foreach (ItemControl itemObj in m_itemControls.Values.OrderBy(itemControl => itemControl.Index))
                        {
                            if (itemObj.Index > smallestRemovedIndex || i == 0)
                            {
                                itemObj.Selected = true;
                                break;
                            }

                            --i;
                        }
                    }

                    // Refresh controls for items that have changed
                    foreach (object item in m_pendingItemsChanged)
                    {
                        ItemControl itemControl;
                        if (m_itemControls.TryGetValue(item, out itemControl))
                            itemControl.Refresh();
                    }

                                      
                    // Reorder controls for items that have moved, either
                    // because a lower index item was deleted, or because they've been
                    // (directly or indirectly) been moved up or down
                    int index = 0;
                    int top = m_singletonMode ? 0 : m_toolStrip.Height;
                    foreach (object item in GetItemsFromContext())
                    {                                                
                        ItemControl itemControl;
                        if (m_itemControls.TryGetValue(item, out itemControl))
                        {
                            itemControl.Index = index;
                            itemControl.Top = top;

                            index++;
                            top += itemControl.Height;
                        }
                    }

                    // Add controls for added items
                    // Currently only adding at the end is supported. If we ever want to support 
                    // inserting in the middle, then we'd probably want to have this step before 
                    // the index-reordering one.
                    foreach (object item in m_pendingItemsInserted)
                    {
                        ItemControl itemControl;
                        if (m_unusedItemControls.Count > 0)
                        {
                            itemControl = m_unusedItemControls[m_unusedItemControls.Count - 1];
                            m_unusedItemControls.RemoveAt(m_unusedItemControls.Count - 1);
                            itemControl.Init(m_itemControls.Count, item,
                                m_singletonMode, m_indexColumnWidth, TransactionContext);
                            itemControl.Visible = true;

                        }
                        else
                        {
                            itemControl = new ItemControl(m_itemControls.Count, item,
                                m_singletonMode, m_indexColumnWidth, TransactionContext);
                            Controls.Add(itemControl);
                            itemControl.MouseUp += itemControl_MouseUp;
                        }
                        itemControl.Width = m_toolStrip.Width;
                        itemControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                        itemControl.Top = top;

                        if (m_singletonMode)
                            itemControl.Dock = DockStyle.Fill;

                        m_oldHeights[itemControl] = itemControl.Height;
                        top += itemControl.Height;
                        m_itemControls.Add(item, itemControl);
                        SubscribeItemEvents(itemControl);
                    }

                    Height = top; // update height of main collection control

                    ClearPendingChanges(); // all pending changes processed

                    UpdateDeleteButton(true);
                    UpdateMoveButtons(true);
                    UpdateItemsCountLabel();
                }
                catch (Win32Exception ex)
                {
                    // For very large collections (with 1000+ items) it is possible that Windows runs out of Window handles
                    // Such collections are currently not usable with this editor. To support such collections we'd have to
                    // implement some kind of virtual mode (i.e. stream items in and out) but that would pose some new
                    // challengs for editing operations (add, remove, move)

                    // Clean up to release some window handles and keep the application running smoothly
                    foreach (ItemControl control in m_itemControls.Values)
                    {
                        if (Controls.Contains(control))
                            Controls.Remove(control);
                        control.Clear();
                        control.Dispose();
                    }
                    m_itemControls.Clear();

                    foreach (ItemControl control in m_unusedItemControls)
                        control.Dispose();
                    m_unusedItemControls.Clear();

                    ClearPendingChanges();

                    // Report error. Must be done after cleaning up, as before we may not even have the resources to show the message box.
                    MessageBox.Show(
                        "Failed to create item controls, probably because there were not enough Window handles available. Consider using a different editor for collections of this size and nature.\r\n\r\n"
                        + ex.GetType().ToString() + Environment.NewLine
                        + ex.Message,
                        "Failed to create item controls", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    //m_toolStrip.Enabled = true;
                    ResumeLayout();
                    m_processingPendingChanges = false;
                }
            }

            private void itemControl_MouseUp(object sender, MouseEventArgs e)
            {
                // Let listeners see right-click events so that they can show context menus.
                Point screenPnt = ((Control)sender).PointToScreen(e.Location);
                Point myClientPnt = PointToClient(screenPnt);
                var newE = new MouseEventArgs(e.Button, e.Clicks, myClientPnt.X, myClientPnt.Y, e.Delta);
                OnMouseUp(newE);
            }

                      
            private void ClearPendingChanges()
            {
                if (m_pendingItemsInserted.Count > 0)
                {
                    m_pendingItemsInserted.Clear();
                }
                
                m_pendingItemsRemoved.Clear();
                m_pendingItemsChanged.Clear();
            }

            // Get available inserters and update the AddButton
            private void UpdateAddButton()
            {
                if (m_editor.GetItemInsertersFunc != null)
                {
                    // Cleanup
                    foreach (ToolStripItem item in m_addSplitButton.DropDownItems)
                        item.Click -= addButton_Click;
                    m_addSplitButton.DropDownItems.Clear();
                    m_addSplitButton.Tag = null;
                    m_addButton.Tag = null;

                    IEnumerable<ItemInserter> inserters = m_editor.GetItemInsertersFunc(m_context);
                    int inserterCount = inserters.Count();
                    if (inserterCount == 0) // Show disabled normal button
                    {
                        m_addSplitButton.Visible = false;

                        m_addButton.Visible = true;
                        m_addButton.Enabled = false;
                        m_addButton.Text = "Add".Localize();
                        m_addButton.ToolTipText = "Always disabled".Localize();
                    }
                    else if (inserterCount == 1) // Show enabled normal button
                    {
                        m_addSplitButton.Visible = false;

                        m_addButton.Visible = true;
                        ItemInserter inserter = inserters.First();
                        m_addButton.Tag = inserter;
                        m_addButton.Enabled = true;
                        m_addButton.Text = "Add".Localize();
                        m_addButton.ToolTipText = string.Format("Add {0}".Localize(), inserter.ItemTypeName);
                    }
                    else // inserterCount >= 2: Show enabled split button
                    {
                        m_addButton.Visible = false;

                        m_addSplitButton.Visible = true;
                        m_addSplitButton.Enabled = true;
                        m_addSplitButton.ToolTipText = "Choose child type to add".Localize(
                            "Could be phrased, 'Choose the type of child to add to this list of child objects'");
                        foreach (ItemInserter inserter in inserters)
                        {
                            ToolStripItem item = new ToolStripButton(inserter.ItemTypeName, inserter.Image);
                            item.Width = TextRenderer.MeasureText(inserter.ItemTypeName, item.Font).Width;
                            item.ToolTipText = string.Format("Add {0}".Localize(), inserter.ItemTypeName);
                            item.Tag = inserter;
                            item.Click += addButton_Click;
                            m_addSplitButton.DropDownItems.Add(item);
                        }
                        SetDefaultInserter(inserters.First());
                    }
                }
            }

            private void UpdateDeleteButton(bool countSelected)
            {
                if (m_editor.RemoveItemFunc == null)
                {
                    m_deleteButton.Enabled = false;
                    m_deleteButton.ToolTipText = "Always disabled".Localize();
                }
                else
                {
                    int selectedCount = 0;
                    if (countSelected)
                        selectedCount = m_itemControls.Values.Count(itemControl => itemControl.Selected);
                    if (selectedCount == 0)
                    {
                        m_deleteButton.Enabled = false;
                        m_deleteButton.ToolTipText = "Disabled because no items are selected".Localize();
                    }
                    else
                    {
                        m_deleteButton.Enabled = true;
                        m_deleteButton.ToolTipText = string.Format("Delete {0} selected items".Localize(), selectedCount);
                    }
                }
            }

            private void UpdateMoveButtons(bool countSelected)
            {
                if (m_editor.MoveItemFunc == null)
                {
                    m_upButton.Enabled = false;
                    m_upButton.ToolTipText = "Always disabled".Localize();

                    m_downButton.Enabled = false;
                    m_downButton.ToolTipText = "Always disabled".Localize();
                }
                else
                {
                    int selectedCount = 0;
                    int minSelectedIdx = int.MaxValue;
                    int maxSelectedIdx = int.MinValue;
                    if (countSelected)
                    {
                        foreach (ItemControl control in m_itemControls.Values)
                        {
                            if (control.Selected)
                            {
                                selectedCount++;
                                if (control.Index < minSelectedIdx)
                                    minSelectedIdx = control.Index;
                                if (control.Index > maxSelectedIdx)
                                    maxSelectedIdx = control.Index;
                            }
                        }
                    }
                    if (selectedCount == 0)
                    {
                        m_upButton.Enabled = false;
                        m_upButton.ToolTipText = "Disabled because no items are selected".Localize();

                        m_downButton.Enabled = false;
                        m_downButton.ToolTipText = "Disabled because no items are selected".Localize();
                    }
                    else
                    {
                        if (minSelectedIdx > 0)
                        {
                            m_upButton.Enabled = true;
                            m_upButton.ToolTipText = string.Format("Move {0} selected items up".Localize(), selectedCount);
                        }
                        else
                        {
                            m_upButton.Enabled = false;
                            m_upButton.ToolTipText = string.Format("Can't move up because first item is selected".Localize(), selectedCount);
                        }

                        if (maxSelectedIdx < m_itemControls.Count - 1)
                        {
                            m_downButton.Enabled = true;
                            m_downButton.ToolTipText = string.Format("Move {0} selected items down".Localize(), selectedCount);
                        }
                        else
                        {
                            m_downButton.Enabled = false;
                            m_downButton.ToolTipText = string.Format("Can't move down because last item is selected".Localize(), selectedCount);
                        }
                    }
                }
            }

            private void UpdateItemsCountLabel()
            {
                int count = (m_itemControls == null) ? 0 : m_itemControls.Values.Count;
                m_itemsCountLabel.Text = string.Format("[{0} items]".Localize("a number of items"), count);
                m_itemsCountLabel.Visible = true;
            }

            // Set the default item type to be inserted when multiple are available
            private void SetDefaultInserter(ItemInserter inserter)
            {
                m_addSplitButton.Tag = inserter;
                m_addSplitButton.Enabled = true;
                string text = string.Format("Add {0}".Localize(), inserter.ItemTypeName);
                m_addSplitButton.Text = text;
                m_addSplitButton.ToolTipText = text;
            }

            private void SubscribeItemEvents(ItemControl itemControl)
            {
                itemControl.SelectionBoxClicked += itemControl_SelectionBoxClicked;
                itemControl.GotFocus += itemControl_GotFocus;

                if (itemControl.CanChangeSize)
                    itemControl.SizeChanged += itemControl_SizeChanged;
            }

            private void UnsubscribeItemEvents(ItemControl itemControl)
            {
                itemControl.SelectionBoxClicked -= itemControl_SelectionBoxClicked;
                itemControl.GotFocus -= itemControl_GotFocus;

                if (itemControl.CanChangeSize)
                    itemControl.SizeChanged -= itemControl_SizeChanged;
            }

            // Select the number lable part of an ItemControl when the editing part gets the focus</summary>
            void itemControl_GotFocus(object sender, EventArgs e)
            {
                SelectItemControl(sender as ItemControl);
            }

            void itemControl_SizeChanged(object sender, EventArgs e)
            {
                ItemControl itemControl = sender as ItemControl;
                if (itemControl == null)
                    return;

                int oldHeight;
                if (m_oldHeights.TryGetValue(itemControl, out oldHeight) && oldHeight != itemControl.Height)
                {
                    // Delta = change in height
                    int delta = itemControl.Height - oldHeight;

                    // Increase the height by delta
                    Height += delta;

                    // Move later ItemControls down by delta);)
                    foreach (ItemControl other in m_itemControls.Values)
                        if (other.Index > itemControl.Index)
                            other.Top += delta;

                    m_oldHeights[itemControl] = itemControl.Height;
                }
            }

            /// <summary>
            /// Add button processing. Performs custom actions when:
            /// 1. The single, normal Add button is clicked.
            /// 2. The default button on the SplitButton is clicked.
            /// 3. A menu item in the SplitButton's DropDownList is clicked.
            /// The sender's Tag should be an ItemInserter and determines which item type is inserted.</summary>
            /// <param name="sender">Sender</param>
            /// <param name="e">Event args</param>
            void addButton_Click(object sender, EventArgs e)
            {
                ToolStripItem toolStripItem = sender as ToolStripItem;
                if (toolStripItem == null)
                    return;

                ItemInserter inserter = toolStripItem.Tag as ItemInserter;
                if (inserter == null)
                    return;

                object item = null;
                m_context.TransactionContext.DoTransaction(
                    delegate { item = inserter.InsertItemFunc(); }, "Insert child".Localize());

                // If we have an ObservableContext, we'll get an ItemInserted event back and handle it there
                // without an ObsverbableContext, we need to trigger the same logic from here
                if (ObservableContext == null)
                    OnItemInserted(item);

                if (m_addSplitButton.Visible && m_addSplitButton.DropDownItems.Count > 1)
                    SetDefaultInserter(inserter);
            }

            /// <summary>
            /// Performs custom actions when Delete button clicked to delete selected items. We don't delete the corresponding controls here!
            /// This happens when the changes are picked up by the following Refresh call.</summary>
            /// <param name="sender">Sender</param>
            /// <param name="e">Event args</param>
            void deleteButton_Click(object sender, EventArgs e)
            {
                DeleteSelectedItems();
            }

            /// <summary>
            /// Performs custom actions on Up button click events</summary>
            /// <param name="sender">Sender</param>
            /// <param name="e">Event args</param>
            void upButton_Click(object sender, EventArgs e)
            {
                MoveSelectedItems(-1);
            }

            /// <summary>
            /// Performs custom actions on Down button click events</summary>
            /// <param name="sender">Sender</param>
            /// <param name="e">Event args</param>
            void downButton_Click(object sender, EventArgs e)
            {
                MoveSelectedItems(1);
            }

            /// <summary>
            /// Performs custom actions when the number label on an ItemControl has been clicked.
            /// Select it respecting pressed modifier keys.</summary>
            /// <param name="sender">Sender</param>
            /// <param name="e">Event args</param>
            void itemControl_SelectionBoxClicked(object sender, EventArgs e)
            {
                SelectItemControl(sender as ItemControl);
            }

            /// <summary>
            /// Deletes the selected items</summary>
            private void DeleteSelectedItems()
            {
                if (m_editor.RemoveItemFunc == null)
                    return;

                List<object> deleteItems = new List<object>();
                foreach (var pair in m_itemControls)
                {
                    object node = pair.Key;
                    ItemControl itemControl = pair.Value;
                    if (itemControl != null && itemControl.Selected)
                    {
                        itemControl.Selected = false;
                        deleteItems.Add(node);
                    }
                }

                // Do the actual removal in a separate loop to avoid modifying
                // the m_itemControls collection while iterating through it
                TransactionContext.DoTransaction(
                    delegate
                    {
                        foreach (object deleteItem in deleteItems)
                            m_editor.RemoveItemFunc(m_context, deleteItem);
                    }, "Remove children".Localize());

                // If we have an ObservableContext, we'll get ItemRemoved event back and handle it there
                // without an ObsverbableContext, we need to trigger the same logic from here
                if (ObservableContext == null)
                {
                    foreach (object deleteItem in deleteItems)
                        OnItemRemoved(deleteItem);
                }
            }

            /// <summary>
            /// Moves selected items up or down</summary>
            /// <param name="direction">Direction: -1 to move up, 1 to move down</param>
            private void MoveSelectedItems(int direction)
            {
                if (m_editor.MoveItemFunc == null || direction == 0)
                    return;

                var movePairs = new List<KeyValuePair<object, ItemControl>>();
                var unselected = new List<ItemControl>();
                foreach (var pair in m_itemControls)
                    if (pair.Value != null && pair.Value.Selected)
                        movePairs.Add(pair);
                    else
                        unselected.Add(pair.Value);


                // When moving down, we want to move the highest index item first
                // when moving up we want to start with the lowest index item.
                if (direction < 0)
                    movePairs.Sort((a, b) => a.Value.Index.CompareTo(b.Value.Index)); // ascending
                else
                    movePairs.Sort((a, b) => -a.Value.Index.CompareTo(b.Value.Index)); // descending

                // Do the actual removal in a separate loop to avoid modifying
                // the m_itemControls collection while iterating through it
                TransactionContext.DoTransaction(
                    delegate
                    {
                        foreach (var movePair in movePairs)
                            m_editor.MoveItemFunc(m_context, movePair.Key, direction);
                    }, "Move children".Localize());


                ProcessPendingChanges();
             
                // Restore selection
                foreach (var movePair in movePairs)
                    m_itemControls[movePair.Key].Selected = true;
                foreach (var item in unselected)
                    item.Selected = false;
            }

            private void SelectItemControl(ItemControl selectedControl)
            {
                bool shiftKeyDown = (ModifierKeys & Keys.Shift) != 0;
                bool ctrlKeyDown = (ModifierKeys & Keys.Control) != 0;

                int index = -1;
                if (selectedControl != null)
                    index = selectedControl.Index;

                // Nothing selected: clear selection
                if (selectedControl == null || index < 0)
                {
                    foreach (ItemControl c in m_itemControls.Values)
                        c.Selected = false;
                    return;
                }

                if (shiftKeyDown)
                {
                    if (ctrlKeyDown) // Shift+Ctrl: Expand range
                    {
                        int min = index;
                        int max = index;
                        foreach (ItemControl c in m_itemControls.Values)
                        {
                            if (c.Selected)
                            {
                                if (c.Index < min)
                                    min = c.Index;
                                else if (c.Index > max)
                                    max = c.Index;
                            }
                        }
                        foreach (ItemControl c in m_itemControls.Values)
                            c.Selected = (c.Index >= min && c.Index <= max);
                    }
                    else // Shift only: Select range from first selection
                    {
                        int min = Math.Min(index, m_firstSelectedIndex);
                        int max = Math.Max(index, m_firstSelectedIndex);
                        foreach (ItemControl c in m_itemControls.Values)
                            c.Selected = (c.Index >= min && c.Index <= max);
                    }
                }
                else if (ctrlKeyDown) // Ctrl only: Toggle selection
                {
                    m_firstSelectedIndex = index;
                    selectedControl.Selected = !selectedControl.Selected;
                }
                else // No modifier key: New, normal selection
                {
                    m_firstSelectedIndex = index;
                    foreach (ItemControl c in m_itemControls.Values)
                        c.Selected = (c.Index == index);
                }

                UpdateDeleteButton(true);
                UpdateMoveButtons(true);
            }


            // Context and editor - assigned by constructor and never changed
            private readonly PropertyEditorControlContext m_context;
            private readonly EmbeddedCollectionEditor m_editor;

            // Cached item controls - cleared when main selection changes
            private readonly Dictionary<object, ItemControl> m_itemControls = new Dictionary<object, ItemControl>();

            // Controls that have been marked as not visible and are available for new items.
            private readonly List<ItemControl> m_unusedItemControls = new List<ItemControl>();

            private readonly Dictionary<ItemControl, int> m_oldHeights = new Dictionary<ItemControl, int>();

            // Contexts: we need to keep them as members to be able to unsubscribe subscribed events
            private IObservableContext m_observableContext;
            private IValidationContext m_validationContext;

            private bool m_inTransaction; // true while inside a transaction; set by ValidationContext events

            // Pending sets - captured updates that haven't been processed yet
            private readonly HashSet<object> m_pendingItemsInserted = new HashSet<object>();
            private readonly HashSet<object> m_pendingItemsRemoved = new HashSet<object>();
            private readonly HashSet<object> m_pendingItemsChanged = new HashSet<object>();

            private object m_activeCollectionNode; // currently selected collection object containing the items

            private int m_firstSelectedIndex; // used for ItemControl selection with modifier keys

            private bool m_singletonMode; // special mode for degenerate collections that always have exactly 1 item
            private int m_indexColumnWidth = 30; // width of index column, will grow if there are many items in the collection
            private bool m_showToolStripLabels = true; // true iff the toolstrips shows button labels (rather than just icons)
            private int m_showToolStripLabelThreshold = 300; // threshold for showing tool strip labels: below = icons only, above = icons & labels

            // Controls
            // We need m_addButton AND m_addSplitButton because we have no other
            // way to hide the arrow when only a single ItemInserter is available
            private ToolStrip m_toolStrip;
            private ToolStripButton m_addButton;
            private ToolStripSplitButton m_addSplitButton;
            private ToolStripButton m_deleteButton;
            private ToolStripButton m_upButton;
            private ToolStripButton m_downButton;
            private ToolStripStatusLabel m_itemsCountLabel;

            // Resources
            private static readonly Image s_addImage = ResourceUtil.GetImage16(Resources.AddImage);
            private static readonly Image s_removeImage = ResourceUtil.GetImage16(Resources.RemoveImage);
            private static readonly Image s_upImage = ResourceUtil.GetImage16(Resources.ArrowUpImage);
            private static readonly Image s_downImage = ResourceUtil.GetImage16(Resources.ArrowDownImage);
        }


        /// <summary>
        /// Control shell for individual child items</summary>
        private class ItemControl : Panel
        {
            public ItemControl(int index, object item, bool singletonMode, int indexColumnWidth, object context)
            {
                m_editControl = new PropertyGridView
                {
                    ShowScrollbar = false,
                    PropertySorting = PropertySorting.None,
                    Dock = DockStyle.Fill,                    
                };
                
                m_editControl.GotFocus += editControl_GotFocus;
                m_editControl.Invalidated += editControl_Invalidated;
                m_editControl.MouseUp += editControl_MouseUp;
                Controls.Add(m_editControl);

                Init(index, item, singletonMode, indexColumnWidth, context);
            }

            protected override void Dispose(bool disposing)
            {
                Clear();
                base.Dispose(disposing);
            }

            public void Clear()
            {
                m_editControl.BindingContext = null;
            }

            public void Init(int index, object item, bool singletonMode, int indexColumnWidth, object context)
            {            
                m_index = index;
                m_singletonMode = singletonMode;                
                var editingContext = new EmbeddedPropertyEditingContext(new[] { item }, context);
                foreach (PropertyDescriptor desc in PropertyEditingContext.GetPropertyDescriptors(editingContext))
                {
                    if (desc.GetEditor(typeof(object)) != null)
                    {
                        CanChangeSize = true;
                        break;
                    }
                }

                if (!m_singletonMode)
                {
                    // we need a label
                    if (m_selectButton == null)
                    {
                        m_selectButton = new Label();
                        Controls.Add(m_selectButton);
                    }
                    m_selectButton.Width = indexColumnWidth;
                    m_selectButton.Dock = DockStyle.Left;
                    m_selectButton.Text = Index.ToString();
                    m_selectButton.TextAlign = ContentAlignment.MiddleCenter;
                    m_selectButton.BackColor = UnselectedColor;
                    m_selectButton.FlatStyle = FlatStyle.Flat;
                    m_selectButton.MouseDown += selectButton_MouseDown;
                }
                else
                {
                    // get rid of label
                    if (m_selectButton != null)
                    {
                        Controls.Remove(m_selectButton);
                        m_selectButton.Dispose();
                        m_selectButton = null;
                    }
                }

                m_editControl.EditingContext = editingContext;
                Height = m_editControl.GetPreferredHeight();                
            }

            // If the height of the contained control (property grid) changes
            // we need to adjust the height of the item control to match it
            private void editControl_Invalidated(object sender, InvalidateEventArgs e)
            {
                Height = m_editControl.GetPreferredHeight();
            }

            // Forward focus event from edit control
            private void editControl_GotFocus(object sender, EventArgs e)
            {
                OnGotFocus(e);
            }

            private void editControl_MouseUp(object sender, MouseEventArgs e)
            {
                // Let listeners see right-click events so that they can show context menus.
                Point screenPnt = ((Control)sender).PointToScreen(e.Location);
                Point myClientPnt = PointToClient(screenPnt);
                var newE = new MouseEventArgs(e.Button, e.Clicks, myClientPnt.X, myClientPnt.Y, e.Delta);
                OnMouseUp(newE);
            }

            // Forward the refresh call to the contained edit control (property grid)
            public override void Refresh()
            {
                base.Refresh();
                m_editControl.Refresh();
            }

            /// <summary>
            /// Gets or sets the index (as displayed in the index header column on the left side)</summary>
            /// <remarks>Changing the index does not by itself move the item in the list.</remarks>
            public int Index
            {
                get { return m_index; }
                set
                {
                    if (m_index != value)
                    {
                        m_index = value;
                        if (m_selectButton != null)
                        {
                            m_selectButton.Text = value.ToString();
                            m_selectButton.ForeColor = m_selected ? SystemColors.HighlightText : SystemColors.ControlText;
                            m_selectButton.BackColor = m_selected ? SystemColors.Highlight : UnselectedColor;
                        }
                    }
                }
            }

            /// <summary>
            /// Event that occurs when the user clicks the index box on the left and thereby
            /// selects or unselects one or more items</summary>
            /// <remarks>The event has to be handled by the parent CollectionControl, because
            /// clicking on the SelectionBox may also select or unselect other items,
            /// depending on current selection and modifier keys (Ctrl, Shift).</remarks>
            public event EventHandler SelectionBoxClicked;

            void selectButton_MouseDown(object sender, EventArgs e)
            {
                SelectionBoxClicked.Raise(this, e);
            }

            /// <summary>
            /// Gets or sets the selected state of the ItemControl and updates
            /// background and foreground color of the index header column accordingly</summary>
            public bool Selected
            {
                get { return m_selected; }
                set
                {
                    if (m_singletonMode)
                        return;                
                    m_selected = value;
                    m_selectButton.ForeColor = value ? SystemColors.HighlightText : SystemColors.ControlText;
                    m_selectButton.BackColor = value ? SystemColors.Highlight : UnselectedColor;

                    if (!value)
                        m_editControl.ClearSelectedProperty();

                    m_selectButton.Refresh();
                }
            }

            /// <summary>
            /// Gets the color used when the item is not selected, alternating for odd and even indexed items</summary>
            private Color UnselectedColor
            {
                get { return Index % 2 == 0 ? SystemColors.Control : SystemColors.ControlLight; }
            }

            public bool CanChangeSize { get; private set; }

            private int m_index;
            private bool m_selected;
            private Label m_selectButton;
            private PropertyGridView m_editControl;

            // In singleton mode theres only 1 item in the collection
            // and it is impossible to add or remove items
            // therefore there is no point in displaying the index header column
            private bool m_singletonMode;

            // We need a PropertyEditingContext that implements ITransactionContext so that property editing
            //  controls that are embedded within the EmbeddedCollectionEditor can have changes recorded
            //  for undo/redo.
            private class EmbeddedPropertyEditingContext : PropertyEditingContext, IAdaptable
            {
                // Note: 'transactionContext' might be null, like in the PropertyEditing sample app.
                public EmbeddedPropertyEditingContext(object[] selection, object context)
                    : base(selection)
                {
                    m_context = context;
                }

                #region IAdaptable Members

                public object GetAdapter(Type type)
                {                    
                    var adaptable = m_context.As<IAdaptable>();
                    if(adaptable != null)
                        return adaptable.GetAdapter(type);
                    return null;
                        
                }

                #endregion

                private object m_context;
            }
        }
    }
}