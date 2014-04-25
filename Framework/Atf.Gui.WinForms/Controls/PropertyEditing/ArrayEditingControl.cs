//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Control for editing arrays</summary>
    public class ArrayEditingControl : Control
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="context">Property editor control context</param>
        public ArrayEditingControl(PropertyEditorControlContext context)
        {
            m_context = context;
            m_initialSelectedObject = m_context.LastSelectedObject.As<DomNodeAdapter>().DomNode;
            m_initialSelectedObject.AttributeChanged += DomNode_AttributeChanged;
            m_toolStrip = new ToolStrip { Dock = DockStyle.Top };
            InitToolStrip();
            Controls.Add(m_toolStrip);
            Height = m_toolStrip.Height;
            m_toolStrip.SizeChanged += toolStrip_SizeChanged;

            // Get active contexts and subscribe to ContextChanged event
            IContextRegistry contextRegistry = m_context.ContextRegistry;
            if (contextRegistry != null)
            {
                contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
                TransactionContext = contextRegistry.GetActiveContext<ITransactionContext>();
            }
            else if (context.TransactionContext != null)
            {
                TransactionContext = context.TransactionContext;
            }
        }

        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_initialSelectedObject.AttributeChanged -= DomNode_AttributeChanged;
            }

            base.Dispose(disposing);
        }
        
        #region ToolStrip

        /// <summary>
        /// Initialize toolstrip</summary>
        void InitToolStrip()
        {
            // add button
            m_addButton = new ToolStripButton
            {
                Text = "Add".Localize(),
                Image = s_addImage,
                ToolTipText = "Add array element".Localize()
            };
            m_addButton.Click += addButton_Click;
            m_toolStrip.Items.Add(m_addButton);
            
            // delete button
            m_deleteButton = new ToolStripButton
            {
                Text = "Delete".Localize(),
                Image = s_removeImage,
                ToolTipText = "Delete array element(s)"
            };
            UpdateDeleteButton(false);
            m_deleteButton.Click += deleteButton_Click;
            m_toolStrip.Items.Add(m_deleteButton);

            // move buttons
            m_moveUpButton = new ToolStripButton
            {
                Text = "Up".Localize("this is the name of a button that causes the selected item to be moved up in a list"),
                Image = s_moveUpImage,
                ToolTipText = "Move array element up"
            };
            
            m_moveDownButton = new ToolStripButton
            {
                Text = "Down".Localize("this is the name of a button that causes the selected item to be moved down in a list"),
                Image = s_moveDownImage,
                ToolTipText = "Move array element down"
            };

            UpdateMoveButtons(false);
            m_moveUpButton.Click += m_moveUpButton_Click;
            m_moveDownButton.Click += m_moveDownButton_Click;
            m_toolStrip.Items.Add(m_moveUpButton);
            m_toolStrip.Items.Add(m_moveDownButton);
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
            m_deleteButton.DisplayStyle = style;
        }

        /// <summary>
        /// Performs custom actions on toolstrip Add button click events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void addButton_Click(object sender, EventArgs e)
        {
            TransactionContext.DoTransaction(
                delegate
                {
                    var array = m_context.Descriptor.GetValue(m_context.LastSelectedObject) as Array;
                    Type type = array.GetType().GetElementType();
                    var newArray = Array.CreateInstance(type, array.Length + 1);
                    array.CopyTo(newArray, 0);
                    m_context.Descriptor.SetValue(m_context.LastSelectedObject, newArray);
                }, "add array element".Localize());
        }

        /// <summary>
        /// Performs custom actions on toolstrip Delete button click events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void deleteButton_Click(object sender, EventArgs e)
        {
            DeleteSelectedItems();
        }

        /// <summary>
        /// Performs custom actions on toolstrip Move up button click events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void m_moveUpButton_Click(object sender, EventArgs e)
        {
            MoveSelectedItems(Up);
        }

        /// <summary>
        /// Performs custom actions on toolstrip Move down button click events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void m_moveDownButton_Click(object sender, EventArgs e)
        {
            MoveSelectedItems(Down);
        }

        private void UpdateMoveButtons(bool countSelected)
        {
            if (m_context.IsReadOnly)
            {
                m_moveUpButton.Enabled = false;
                m_moveDownButton.Enabled = false;
                return;
            }

            int selectedCount = 0;
            int minSelectedIdx = int.MaxValue;
            int maxSelectedIdx = int.MinValue;
            if (countSelected)
            {
                foreach (var control in m_itemControls.Values)
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
                m_moveUpButton.Enabled = false;
                m_moveUpButton.ToolTipText = "Disabled because no elements selected".Localize();

                m_moveDownButton.Enabled = false;
                m_moveDownButton.ToolTipText = "Disabled because no elements selected".Localize();
            }
            else
            {
                if (minSelectedIdx > 0)
                {
                    m_moveUpButton.Enabled = true;
                    m_moveUpButton.ToolTipText = string.Format("Move {0} selected elements up".Localize(), selectedCount);
                }
                else
                {
                    m_moveUpButton.Enabled = false;
                    m_moveUpButton.ToolTipText = string.Format("Can't move up because first element selected".Localize(), selectedCount);
                }

                if (maxSelectedIdx < m_itemControls.Count - 1)
                {
                    m_moveDownButton.Enabled = true;
                    m_moveDownButton.ToolTipText = string.Format("Move {0} selected elements down".Localize(), selectedCount);
                }
                else
                {
                    m_moveDownButton.Enabled = false;
                    m_moveDownButton.ToolTipText = string.Format("Can't move down because last element selected".Localize(), selectedCount);
                }
            }
        }

        private void MoveSelectedItems(int direction)
        {
            if (direction != Up && direction != Down)
                return;

            var movePairs = new List<KeyValuePair<int, ItemControl>>();
            foreach (var pair in m_itemControls)
                if (pair.Value != null && pair.Value.Selected)
                    movePairs.Add(pair);

            // When moving down, we want to move the highest index item first
            // when moving up we want to start with the lowest index item.
            if (direction < 0)
                movePairs.Sort((a, b) => a.Value.Index.CompareTo(b.Value.Index)); // ascending
            else
                movePairs.Sort((a, b) => -a.Value.Index.CompareTo(b.Value.Index)); // descending

            TransactionContext.DoTransaction(
                delegate
                {
                    foreach (var movePair in movePairs)
                    {
                        var array = m_context.Descriptor.GetValue(m_context.LastSelectedObject) as Array;
                        var tmp = array.GetValue(movePair.Key);
                        array.SetValue(array.GetValue(movePair.Key + direction), movePair.Key);
                        array.SetValue(tmp, movePair.Key + direction);
                    }
                }, "Move elements".Localize());

            RebuildItemControls(m_context.LastSelectedObject);

            // Restore selection
            foreach (var movePair in movePairs)
                m_itemControls[movePair.Key + direction].Selected = true;

            UpdateMoveButtons(true);
            UpdateDeleteButton(true);
        }

        private void UpdateAddButton()
        {
            m_addButton.Enabled =
                m_context.Descriptor.Is<AttributePropertyDescriptor>() &&
                !m_context.Descriptor.IsReadOnly;
        }

        private void UpdateDeleteButton(bool countSelected)
        {
            if (!m_context.Descriptor.Is<AttributePropertyDescriptor>() ||
                m_context.Descriptor.IsReadOnly)
            {
                m_deleteButton.Enabled = false;
                return;
            }

            int selectedCount = 0;
            if (countSelected)
                selectedCount = m_itemControls.Values.Count(itemControl => itemControl.Selected);

            if (selectedCount == 0)
            {
                m_deleteButton.Enabled = false;
                m_deleteButton.ToolTipText = "Disabled because no array elements selected".Localize();
            }
            else
            {
                m_deleteButton.Enabled = true;

                if (selectedCount > 1)
                    m_deleteButton.ToolTipText = string.Format("Delete {0} selected array elements".Localize(), selectedCount);
                else
                    m_deleteButton.ToolTipText = "Delete selected array element".Localize();
            }
        }
        
        private void DeleteSelectedItems()
        {
            // Delete selected items
            // We don't delete the corresponding controls here!
            // This happens when the changes are picked up by the following Refresh call
            List<int> deleteItemIndexes = new List<int>();
            foreach (var pair in m_itemControls)
            {
                int index = pair.Key;
                ItemControl itemControl = pair.Value;
                if (itemControl != null && itemControl.Selected)
                    deleteItemIndexes.Add(index);
            }

            // Do the actual removal in a separate loop to avoid modifying
            // the m_itemControls collection while iterating through it
            TransactionContext.DoTransaction(
                delegate
                {
                    Array oldValues = m_context.Descriptor.GetValue(m_context.LastSelectedObject) as Array;
                    var newValues = Array.CreateInstance(oldValues.GetType().GetElementType(), oldValues.Length - deleteItemIndexes.Count);
                    int newValIndex = 0;
                    for (int i = 0; i < oldValues.Length; i++)
                    {
                        if (!deleteItemIndexes.Contains(i))
                        {
                            newValues.SetValue(oldValues.GetValue(i), newValIndex);
                            newValIndex++;
                        }
                    }

                    m_context.SetValue(newValues);
                }, deleteItemIndexes.Count > 1 ? "delete array elements".Localize() : "delete array element".Localize());
        }
        #endregion

        #region ITransactionContext Handling
        private void OnItemChanged(object item, object newValue)
        {
            var array = newValue as Array;
            if(array.Length != m_lastKnownSize)
                RebuildItemControls(m_context.LastSelectedObject);

            for(int i = 0; i < m_itemControls.Values.Count; i++)
            {
                m_itemControls[i].Value = array.GetValue(i);
                m_itemControls[i].Refresh();
            }
        }

        /// <summary>
        /// Performs custom actions on ActiveContextChanged events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            TransactionContext = m_context.ContextRegistry.GetActiveContext<ITransactionContext>();
        }

        private ITransactionContext TransactionContext { get; set; }

        /// <summary>
        /// Performs custom actions on DOM node AttributeChanged events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            var adapter = m_context.LastSelectedObject.As<DomNodeAdapter>();
            if (adapter != null && 
                e.DomNode == adapter.DomNode && 
                m_context.Descriptor.Is<AttributePropertyDescriptor>() &&
                e.AttributeInfo == m_context.Descriptor.As<AttributePropertyDescriptor>().AttributeInfo)
                    OnItemChanged(e.DomNode, e.NewValue);
        }
        #endregion
        
        #region ItemControl Events and Selection
        private void SubscribeItemEvents(ItemControl itemControl)
        {
            itemControl.SelectionBoxClicked += itemControl_SelectionBoxClicked;
            itemControl.GotFocus += itemControl_GotFocus;
            itemControl.ValueChanged += itemControl_ValueChanged;            
        }

        /// <summary>
        /// Performs custom actions on ItemControl ValueChanged events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void itemControl_ValueChanged(object sender, EventArgs e)
        {
            // save off the old values
            Array old_values =
                m_context.Descriptor.GetValue(m_context.LastSelectedObject) as Array;

            // create the new values
            Array new_values = old_values.Clone() as Array;

            // set the new values
            ItemControl selectedItem = sender as ItemControl;
            new_values.SetValue(selectedItem.Value, selectedItem.Index);

            TransactionContext.DoTransaction(
                delegate
                {
                    // set the new values
                    m_context.SetValue(new_values);
                    selectedItem.Value = selectedItem.Value;
                }, "edit array element".Localize());
        }

        private void UnsubscribeItemEvents(ItemControl itemControl)
        {
            itemControl.SelectionBoxClicked -= itemControl_SelectionBoxClicked;
            itemControl.GotFocus -= itemControl_GotFocus;
            itemControl.ValueChanged -= itemControl_ValueChanged;            
        }

        /// <summary>
        /// Raises and handles OnSizeChanged event, resizing controls</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            // compute desired height.
            int itemHeight = Controls.Count > 1 ? Controls[1].Height : 0;
            int desiredHieght = Controls[0].Height
                + (Controls.Count-1) * itemHeight;

            if (Height != desiredHieght)
            {
                Height = desiredHieght;                
            }
            
            int top = 0;
            foreach (Control c in Controls)
            {
                c.Top = top;
                top += c.Height;
            }
            
            base.OnSizeChanged(e);

        }
       
        /// <summary>
        /// Performs custom actions on ItemControl SelectionBoxClicked events.
        /// The number label on an ItemControl has been clicked:
        /// Select it respecting pressed modifyer keys.</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void itemControl_SelectionBoxClicked(object sender, EventArgs e)
        {
            SelectItemControl(sender as ItemControl);
        }

        /// <summary>
        /// Performs custom actions on ItemControl SelectionBoxClicked events.
        /// Select the number label part of an ItemControl when the editing part gets the focus.</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void itemControl_GotFocus(object sender, EventArgs e)
        {
            SelectItemControl(sender as ItemControl);
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
            else // No modifyer key: New, normal selection
            {
                m_firstSelectedIndex = index;
                foreach (ItemControl c in m_itemControls.Values)
                    c.Selected = (c.Index == index);
            }

            UpdateDeleteButton(true);
            UpdateMoveButtons(true);
        }
        private int m_firstSelectedIndex; // used for ItemControl selection with modifyer keys
        #endregion

        #region Data and Visual Refreshing
        /// <summary>
        /// Refresh control</summary>
        public override void Refresh()
        {
            // Don't call base to avoid unnecessary events that lead to flickering

            // Rebuild if the selected object has changed
            object selected = m_context.LastSelectedObject;
            if (m_activeCollectionNode != selected && selected != null)
                RebuildItemControls(selected);
        }

        // Rebuild all item controls when the selected collection object has changed
        private void RebuildItemControls(object collectionObject)
        {
            m_activeCollectionNode = collectionObject;
            var array = m_context.Descriptor.GetValue(m_context.LastSelectedObject) as Array;
            m_lastKnownSize = array.Length;

            // Set index column width to be big enough to display the highest expected index,
            // using double the number of current items as first estimate.
            // This could be done more accurately with MeasureString and could also be dynamically
            // adjusted if the number exceeds the current max due to add operations.
            m_indexColumnWidth = Math.Max(30, (array.Length * 2).ToString().Length * 10);

            try
            {
                SuspendLayout();

                m_toolStrip.Enabled = false;

                // Clear and dispose all current item controls
                foreach (ItemControl itemControl in m_itemControls.Values)
                {
                    UnsubscribeItemEvents(itemControl);
                    Controls.Remove(itemControl);
                    itemControl.Dispose();
                }
                m_itemControls.Clear();


                // Add controls for added items
                // Currently only adding at the end is supported. If we ever want to support 
                // inserting in the middle, then we'd probably want to have this step before 
                // the index-reordering one.
                int top = m_toolStrip.Height;
                var controlsToAdd = new List<ItemControl>();
                int id = 0;
                foreach (object item in array)
                {
                    // Work around for a very strange bug related to the parent Control having a custom font.
                    // It seems to be important to not have this ItemControl's Font property set when adding
                    //  it to our Controls, in the call to Controls.AddRange() below.
                    // The parent Control is probably a Sce.Atf.Controls.PropertyEditing.PropertyGridView.
                    // See this tracker item: http://tracker.ship.scea.com/jira/browse/CORETEXTEDITOR-363
                    Font defaultFont = (Parent != null) ? Parent.Font : Font;

                    var itemControl = new ItemControl(m_itemControls.Count, item, m_indexColumnWidth)
                    {
                        Width = m_toolStrip.Width,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                        Top = top,
                        Index = id,
                        Font = defaultFont
                    };

                    top += itemControl.Height;                    
                    m_itemControls.Add(id, itemControl);
                    controlsToAdd.Add(itemControl);
                    SubscribeItemEvents(itemControl);
                    id++;
                }
                Controls.AddRange(controlsToAdd.ToArray());
                              
                Height = top; // update height of main collection control

                UpdateAddButton();
                UpdateDeleteButton(true);
                UpdateMoveButtons(true);
            }
            catch (Win32Exception ex)
            {
                // For very large collections (with 1000+ items) it is possible that Windows runs out of Window handles
                // Such collections are currently not usable with this editor. To support such collections we'd have to
                // implement some kind of virtual mode (i.e. stream items in and out) but that would pose some new
                // challenges for editing operations (add, remove, move)

                // Clean up to release some window handles and keep the application running smoothly
                foreach (ItemControl control in m_itemControls.Values)
                {
                    if (Controls.Contains(control))
                        Controls.Remove(control);
                    control.Dispose();
                }
                m_itemControls.Clear();

                // Report error. Must be done after cleaning up, as before we may not even have the resources to show the message box.
                MessageBox.Show(
                    "Failed to create item controls, probably because there were not enough Window handles available. Consider using a different editor for collections of this size and nature.\r\n\r\n"
                    + ex.GetType().ToString() + Environment.NewLine
                    + ex.Message,
                    "Failed to create item controls", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ResumeLayout();
                m_toolStrip.Enabled = true;
            }

            SkinService.ApplyActiveSkin(this);
            // refreshes alternate back color for even/odd selected items.
            foreach (ItemControl control in m_itemControls.Values)
                control.Selected = control.Selected;

        }
        #endregion

        #region Private ItemControl Class
        /// <summary>
        /// Control shell for individual child items</summary>
        private class ItemControl : Panel
        {
            public ItemControl(int index, object item, int indexColumnWidth)
            {
               
                m_index = index;
                              
                m_editControl = new NumericTextBox(item.GetType())
                {
                    Dock = DockStyle.Fill,
                    BorderStyle = System.Windows.Forms.BorderStyle.None
                };

                m_editControl.Width = Width - indexColumnWidth;
                m_editControl.GotFocus += editControl_GotFocus;

                m_editControl.Value = item;
                m_editControl.Invalidated += editControl_Invalidated;
                m_editControl.ValueEdited += m_editControl_ValueChanged;

                m_selectButton = new Label
                {
                    Width = indexColumnWidth,
                    Dock = DockStyle.Left,
                    Text = Index.ToString(),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = UnselectedColor,
                    FlatStyle = FlatStyle.Flat,
                    Font = s_regularFont
                };

                m_selectButton.MouseDown += selectButton_MouseDown;

                Height = m_editControl.Height;

                Controls.Add(m_editControl);
                Controls.Add(m_selectButton);

                m_editControl.SizeChanged += (sender, e) =>
                    {
                        Height = m_editControl.Height;
                    };
            }

            void m_editControl_ValueChanged(object sender, EventArgs e)
            {
                ValueChanged.Raise(this, e);
            }

            // If the height of the contained control (property grid) changes
            // we need to adjust the height of the item control to match it
            void editControl_Invalidated(object sender, InvalidateEventArgs e)
            {
                Height = m_editControl.Height;
            }

            // Forward focus event from edit control
            void editControl_GotFocus(object sender, EventArgs e)
            {
                OnGotFocus(e);
            }

            // Forward the refresh call to the contained edit control
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
                            m_selectButton.ForeColor = m_selected ? SystemColors.HighlightText : ForeColor;
                            m_selectButton.BackColor = m_selected ? SystemColors.Highlight : UnselectedColor;
                        }
                    }
                }
            }

            /// <summary>
            /// Event that is raised when the user clicks the index box on the left and thereby
            /// selects or unselects one or more items</summary>
            /// <remarks>The event has to be handled by the parent CollectionControl, because
            /// clicking on the SelectionBox may also select or unselect other items,
            /// depending on current selection and modifier keys (Ctrl, Shift).</remarks>
            public event EventHandler SelectionBoxClicked;

            public event EventHandler ValueChanged;

            void selectButton_MouseDown(object sender, EventArgs e)
            {
                m_selectButton.Focus();
                SelectionBoxClicked.Raise(this, e);
            }

            public object Value
            {
                get { return m_editControl.Value; }
                set { m_editControl.Value = value; }
            }

            /// <summary>
            /// Gets or sets the selected state of the ItemControl and updates
            /// background and foreground color of the index header column accordingly</summary>
            public bool Selected
            {
                get { return m_selected; }
                set
                {
                    m_selected = value;
                    m_selectButton.ForeColor = value ? SystemColors.HighlightText : ForeColor;
                    m_selectButton.BackColor = value ? SystemColors.Highlight : UnselectedColor;
                    m_selectButton.Refresh();
                }
            }

            /// <summary>
            /// Gets the color used when the item is not selected, alternating for odd and even indexed items</summary>
            private Color UnselectedColor
            {
                get
                {
                    Color alternate = BackColor;
                    alternate = alternate.GetBrightness() > 0.5f ? ControlPaint.Dark(alternate, 0.15f)
                        : ControlPaint.Light(alternate, 0.15f);
                    return Index % 2 == 0 ? BackColor : alternate;
                }
            }
                       
            private int m_index;
            private bool m_selected;
            private Label m_selectButton;
            private readonly NumericTextBox m_editControl;
        }
        #endregion

        PropertyEditorControlContext m_context;

        private bool m_showToolStripLabels = true; // true iff the toolstrips shows button labels (rather than just icons)
        private int m_showToolStripLabelThreshold = 300; // threshold for showing tool strip labels: below = icons only, above = icons & labels
        private object m_activeCollectionNode; // currently selected collection object containing the items
        private int m_indexColumnWidth = 30; // width of index column, will grow if there are many items in the collection
        private int m_lastKnownSize = int.MinValue;
        private DomNode m_initialSelectedObject;

        private const int Up = -1;
        private const int Down = 1;

        // Cached item controls - cleared when main selection changes
        private readonly Dictionary<int, ItemControl> m_itemControls = new Dictionary<int, ItemControl>();

        // Controls
        private readonly ToolStrip m_toolStrip;
        private ToolStripButton m_addButton;
        private ToolStripButton m_deleteButton;
        private ToolStripButton m_moveUpButton;
        private ToolStripButton m_moveDownButton;

        // Resources
        private static readonly Image s_addImage = ResourceUtil.GetImage16(Resources.AddImage);
        private static readonly Image s_removeImage = ResourceUtil.GetImage16(Resources.RemoveImage);
        private static readonly Image s_moveUpImage = ResourceUtil.GetImage16(Resources.ArrowUpImage);
        private static readonly Image s_moveDownImage = ResourceUtil.GetImage16(Resources.ArrowDownImage);
        private static readonly Font s_regularFont = new Font(FontFamily.GenericSansSerif, 8.25f, FontStyle.Regular);
    }
}
