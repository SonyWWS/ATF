//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Spreadsheet-like Control for displaying properties of many objects simultaneously. Only
    /// the properties that are in common with all the selected objects are displayed.</summary>
    public class GridView : PropertyView, IPropertyEditingControlOwner
    {
        /// <summary>
        /// Default constructor</summary>
        public GridView() : this(new ColumnHeaders())
        {
        }

        /// <summary>
        /// Constructor with column headers</summary>
        /// <param name="columnHeaders">Column headers</param>
        public GridView(ColumnHeaders columnHeaders)
        {
            m_selectedRows = new Selection<int>();
            m_selectedRows.Changed += selectedRows_Changed;

            m_columnHeaders = columnHeaders;
            m_columnHeaders.GridView = this;
            m_columnHeaders.Height = HeaderHeight;
            m_columnHeaders.Dock = DockStyle.Top;

            m_vScrollBar = new VScrollBar();
            m_vScrollBar.Dock = DockStyle.Right;
            m_vScrollBar.ValueChanged += vScrollBar_ValueChanged;

            m_hScrollBar = new HScrollBar();
            m_hScrollBar.Dock = DockStyle.Bottom;
            m_hScrollBar.ValueChanged += hScrollBar_ValueChanged;

            Controls.Add(m_columnHeaders);
            Controls.Add(m_vScrollBar); // add vertical scrollbar here, so header can't overlap it
            Controls.Add(m_hScrollBar);

            SetPens();

            Color color1 = Color.LightSteelBlue;
            m_evenRowBrush = new SolidBrush(color1);
            Color color2 = ColorUtil.GetShade(color1, 1.2f);
            m_oddRowBrush = new SolidBrush(color2);

            base.DoubleBuffered = true;

            // enable dragging and dropping of column headers by default
            DragDropColumnsEnabed = true;

            // enable selection in the grid
            SelectionEnabed = true;

            // enable selection of more than one item in the grid
            MultiSelectionEnabled = true;

            // enable wrapping with the up and down arrow keys
            UpDownKeySelectionWrapEnabled = true;
        }

        /// <summary>
        /// Event that is raised after the selected row(s) changes</summary>
        public event EventHandler SelectedRowsChanged;

        /// <summary>
        /// Raises the SelectedRowsChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnSelectedRowsChanged(EventArgs e)
        {
            EventHandler handler = SelectedRowsChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Event that is raised after the value in a row changes</summary>
        public event EventHandler<RowChangedEventArgs> RowValueChanged;
        
        /// <summary>
        /// Raises the RowValueChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnRowValueChanged(RowChangedEventArgs e)
        {
            if (RowValueChanged != null)
            {
                RowValueChanged(this, e);
            }
        }
    
        /// <summary>
        /// Gets the currently selected row indices</summary>
        public IEnumerable<int> SelectedIndices
        {
            get
            {
                return m_selectedRows.GetSnapshot().AsEnumerable();
            }
        }

        /// <summary>
        /// Gets the number of selected rows in a fast way</summary>
        public int SelectedCount { get { return m_selectedRows.Count; } }

        /// <summary>
        /// Get whether a row is being edited and the edit controls are visible</summary>
        public bool EditingRowVisible { get { return m_editingRowVisible; } }

        /// <summary>
        /// Sets the currently selected row indices</summary>
        /// <param name="selectedRows">Row indices, from 0 to n - 1, where n is the
        /// number of bound objects</param>
        public void SetSelection(IEnumerable<int> selectedRows)
        {
            int max = EditingContext.Items.Count();
            List<int> objects = new List<int>();
            foreach (int i in selectedRows)
            {
                if (i < 0 || i >= max)
                    throw new ArgumentException("row index out of allowable range");
                objects.Add(i);
            }

            // if a select all came in, preserve the last selected if there is one
            bool selectAll = (objects.Count != 0) && (objects.Count == max);
            int lastSelected = m_selectedRows.LastSelected;
            if (selectAll && (lastSelected != -1))
            {
                objects.Remove(lastSelected);
                objects.Add(lastSelected);
            }

            m_selectedRows.SetRange(objects);

            TryMakeSelectionVisible();
        }

        /// <summary>
        /// Clears the currently selected row indices</summary>
        public void ClearSelection()
        {
            m_selectedRows.SetRange(EmptyArray<int>.Instance);
        }

        /// <summary>
        /// Gets the Pen used for grid lines</summary>
        public Pen GridLinePen
        {
            get
            {
                return m_gridLinePen;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the System.Windows.Forms.Control
        /// and its child controls and optionally releases the managed resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false
        /// to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_gridLinePen != null)
                    m_gridLinePen.Dispose();
                if (m_evenRowBrush != null)
                    m_evenRowBrush.Dispose();
                if (m_oddRowBrush != null)
                    m_oddRowBrush.Dispose();
                if (m_selectedCellPen != null)
                    m_selectedCellPen.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the PropertyDescriptor for the property under the client point, or null if none</summary>
        /// <param name="clientPoint">Client point</param>
        /// <returns>PropertyDescriptor for property under client point, or null if none</returns>
        public PropertyDescriptor GetDescriptorAt(Point clientPoint)
        {
            Point offset;
            return GetDescriptorAt(clientPoint, out offset);
        }

        /// <summary>
        /// Gets the PropertyDescriptor for the property under the client point, or null if none</summary>
        /// <param name="clientPoint">Client point</param>
        /// <param name="offset">Offset from client point</param>
        /// <returns>Position relative to the property rectangle. This can be used to check where property is clicked.</returns>
        public PropertyDescriptor GetDescriptorAt(Point clientPoint, out Point offset)
        {
            HitRecord hitRecord = Pick(clientPoint);
            offset = hitRecord.Offset;
            Property property = hitRecord.Property;
            if (property != null)
                return property.Descriptor;

            return null;
        }

        /// <summary>
        /// Gets the row index for the row under the client point, or -1 if none</summary>
        /// <param name="clientPoint">Client point</param>
        /// <returns>Row index for row under client point, or -1 if none</returns>
        public int GetRowIndexAt(Point clientPoint)
        {
            HitRecord hitRecord = Pick(clientPoint);
            return hitRecord.Row;
        }

        /// <summary>
        /// Sorts the data in the grid, returning true if the data was sorted</summary>
        /// <param name="propertyName">Name of property to sort by</param>
        /// <param name="direction">Direction to sort the data</param>
        /// <returns>True iff data was sorted</returns>
        public bool SortByProperty(string propertyName, ListSortDirection direction)
        {
            // need to persist these values because Properties are not persistent
            m_sortByPropertyName = propertyName;
            m_sortByPropertyDirection = direction;

            return ApplySortByProperty();
        }

        /// <summary>
        /// Sets a custom sort order for the properties</summary>
        /// <param name="customSortOrder">A list of property names in the desired sort order</param>
        public override void SetCustomPropertySortOrder(List<string> customSortOrder)
        {
            // need to persist these values because Properties are not persistent
            m_userPropertySortOrder = customSortOrder;
            base.SetCustomPropertySortOrder(customSortOrder);
        }

        /// <summary>
        /// Returns the name of the property that the grid was last sorted by if there is one; otherwise returns an empty string</summary>
        /// <returns>Name of the property that the grid was last sorted by</returns>
        public string GetLastSortPropertyName()
        {
            string result = string.Empty;

            if (m_lastSortProperty != null)
            {
                result = m_lastSortProperty.Descriptor.Name;
            }

            return result;
        }

        /// <summary>
        /// Returns the current sort order for the given property. This does not mean that data is actually sorted by this property.
        /// It is the current state of the UI sort direction. Use GetLastSortPropertyName to check if the data is sorted by a property.</summary>
        /// <param name="propertyName">Property whose sort order is obtained</param>
        /// <returns>Current sort order for the given property</returns>
        public ListSortDirection GetPropertySortOrder(string propertyName)
        {
            ListSortDirection result = ListSortDirection.Ascending;

            foreach (Property property in Properties)
            {
                if (property.Descriptor.Name.Equals(propertyName))
                {
                    ColumnInfo columnInfo = GetColumnInfo(property);
                    if (columnInfo != null)
                    {
                        // current sort dir is not stored, use the next sort dir to get the current sort dir.
                        result = (columnInfo.NextSortDirection == ListSortDirection.Descending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
                    }
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates the best width for the column at this property based on the data contents
        /// and sets the column to that width</summary>
        /// <param name="p">The property for the column to apply the best fit to</param>
        protected void ApplyColumnBestFit(Property p)
        {
            // if the property is not resizeable, then just leave
            if (p.DisableResize)
                return;

            // a padding value so the column isn't fitted exactly to the data size
            int padding = 4;
            Graphics g = CreateGraphics();

            // fit the column header
            int bestWidth = (int)m_columnHeaders.MeasureProperty(
                p.FirstInCategory,
                p.Descriptor.Category,
                p.Descriptor.DisplayName,
                CanSort(p)).Width + padding;

            if (GetVisible(p))
            {
                int startRow = GetRowAtY(HeaderHeight);
                int endRow = GetRowAtY(Height);
                if (endRow >= SelectedObjects.Length)
                    endRow = SelectedObjects.Length - 1;

                for (int j = startRow; j <= endRow; j++)
                {
                    bool selected = m_selectedRows.Contains(j);
                    int measuredWidth = (int)MeasureProperty(g, p, j, selected).Width + padding;
                    bestWidth = Math.Max(bestWidth, measuredWidth);
                }
            }

            // make sure the column is at least as big as MinimumColumnWidth
            bestWidth = Math.Max(bestWidth, ColumnHeaders.MinimumColumnWidth);

            Dictionary<string, int> columnWidths = new Dictionary<string, int>();
            columnWidths.Add(p.Descriptor.Name, bestWidth);
            SetColumnWidths(columnWidths);
            Invalidate();
        }

        /// <summary>
        /// Calculates the best width for the column at the location based on column data contents
        /// and sets the column to that width</summary>
        /// <param name="location">The client point where the user clicked to select best fit for the column</param>
        public void ApplyColumnBestFit(Point location)
        {
            HitRecord hit = Pick(location);

            if (hit.Type != HitType.None)
            {
                ApplyColumnBestFit(hit.Property);
            }
        }

        /// <summary>
        /// Calculates the best width for the all columns based on the data contents
        /// and sets the columns to that width</summary>
        public void ApplyColumnBestFitAllColumns()
        {
            foreach (Property p in Properties)
            {
                ApplyColumnBestFit(p);
            }
        }

        /// <summary>
        /// Returns a Dictionary of column sizes keyed by property name</summary>
        /// <returns>Dictionary of column sizes keyed by property name</returns>
        public Dictionary<string, int> GetColumnWidths()
        {
            if (m_columnWidths == null)
                m_columnWidths = new Dictionary<string, int>();

            foreach (Property property in Properties)
            {
                ColumnInfo columnInfo = GetColumnInfo(property);
                if (columnInfo != null)
                {
                    if (m_columnWidths.ContainsKey(property.Descriptor.Name))
                        m_columnWidths[property.Descriptor.Name] = columnInfo.Width;
                    else
                        m_columnWidths.Add(property.Descriptor.Name, columnInfo.Width);
                }
            }

            return m_columnWidths;
        }

        /// <summary>
        /// Sets the widths of the columns</summary>
        /// <param name="columnWidths">Dictionary of column widths keyed by property name</param>
        /// <returns>True if at least one column width was found and set</returns>
        public bool SetColumnWidths(Dictionary<string, int> columnWidths)
        {
            // persist these values, reapply if we tear down and rebuild properties
            m_columnWidths = columnWidths;

            return ApplyColumnWidths();
        }
        
        /// <summary>
        /// Enters edit mode on the last selected row on the given property</summary>
        /// <param name="propertyName">The property to select for editing</param>
        public void EnterEditMode(string propertyName)
        {
            foreach (Property p in Properties)
            {
                if (p.Descriptor.Name.Equals(propertyName))
                {
                    EditProperty(p);
                    return;
                }
            }
        }

        /// <summary>
        /// Gets or sets flag for enabling or disabling reordering of column headers by dragging and dropping. Enabled by default.</summary>
        public bool DragDropColumnsEnabed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag for enabling or disabling selection in the grid. Enabled by default.</summary>
        public bool SelectionEnabed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag for enabling or disabling multi-selection in the grid, i.e., more than one item at a time selected. Enabled by default.</summary>
        public bool MultiSelectionEnabled
        {
            get;
            set;
        }
      
        /// <summary>
        /// Gets or sets flag for enabling or disabling arrow keys wrapping for selection. Enabled by default.</summary>
        public bool UpDownKeySelectionWrapEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag for enabling or disabling selection on mouse up rather than mouse down. Disabled by default.</summary>
        public bool MouseUpSelectionEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Returns true iff a property is visible. Considers user show/hide state as well as property visible value.</summary>
        /// <param name="p">Property to test</param>
        /// <returns>True iff a property is visible</returns>
        protected bool GetVisible(Property p)
        {
            return (!GetColumnInfo(p).UserHidden) && p.Visible;
        }

        /// <summary>
        /// Returns a Dictionary of column hidden states, keyed by property name</summary>
        /// <returns>Dictionary of column hidden states</returns>
        public Dictionary<string, bool> GetColumnUserHiddenStates()
        {
            if (m_columnUserHiddenStates == null)
                m_columnUserHiddenStates = new Dictionary<string, bool>();

            // if the properties are loaded, build a fresh dictionary of values
            if (Properties.Any())
                m_columnUserHiddenStates.Clear();

            foreach (Property property in Properties)
            {
                ColumnInfo columnInfo = GetColumnInfo(property);
                if (columnInfo != null)
                {
                    if (m_columnUserHiddenStates.ContainsKey(property.Descriptor.Name))
                        m_columnUserHiddenStates[property.Descriptor.Name] = columnInfo.UserHidden;
                    else
                        m_columnUserHiddenStates.Add(property.Descriptor.Name, columnInfo.UserHidden);
                }
            }

            return m_columnUserHiddenStates;
        }

        /// <summary>
        /// Sets the user-hidden states of the columns</summary>
        /// <param name="columnUserHiddenStates">Dictionary of column user-hidden states keyed by property name</param>
        /// <returns>True if at least one column width was found and set</returns>
        public bool SetColumnUserHiddenStates(Dictionary<string, bool> columnUserHiddenStates)
        {
            // need to persist these values, as the Properties are not persistent
            m_columnUserHiddenStates = columnUserHiddenStates;

            return ApplyColumnUserHiddenStates();
        }

        /// <summary>
        /// Gets the custom sort order of the properties resulting from dragging and dropping column headers</summary>
        /// <returns>The names of the properties sorted by the user's custom sort order resulting from dragging and dropping</returns>
        public List<string> GetCustomPropertySortOrder()
        {
            return m_userPropertySortOrder;
        }

        #region IPropertyEditingControlOwner Members


        /// <summary>
        /// Gets the selected objects for a property edit</summary>
        object[] IPropertyEditingControlOwner.SelectedObjects
        {
            get
            {
                if (m_selectedRows.Count == 0)
                    return EmptyArray<object>.Instance;

                List<object> result = new List<object>(m_selectedRows.Count);
                foreach (int index in m_selectedRows)
                    result.Add(SelectedObjects[index]);
                return result.ToArray();
            }
        }

        #endregion

        /// <summary>
        /// Raises the BackColorChanged event and performs custom actions</summary>
        /// <param name="e">Event args</param>
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            SetPens();
        }

        // Gets an enumeration of ints, from start to end inclusive.
        private IEnumerable<int> GetRangeOfInts(int start, int end)
        {
            for (int i = start; i <= end; i++)
                yield return i;
        }

        /// <summary>
        /// Calling the base ProcessCmdKey allows this key press to be consumed by owning
        /// controls like PropertyView and PropertyGridView and be seen by ControlHostService.
        /// Returning false allows the key press to escape to IsInputKey, OnKeyDown, OnKeyUp, etc.
        /// Returning true means that this key press has been consumed by this method and this
        /// event is not passed on to any other methods or controls.</summary>
        /// <param name="msg">Window message to process</param>
        /// <param name="keyData">Key data</param>
        /// <returns>False to allow the key press to escape to IsInputKey, OnKeyDown, OnKeyUp, etc.
        /// True to consume this key press, so this
        /// event is not passed on to any other methods or controls.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // If ctrl+A is pressed, and the select all command is viable, then select all.
            // We don't want to do select-all when a text box has focus, for example, because it uses
            //  ctrl+A to select all the text within it.
            if (keyData == (Keys.A | Keys.Control) &&
                !m_editingRowVisible &&
                MultiSelectionEnabled)
            {
                m_selectedRows.SetRange(GetRangeOfInts(0, SelectedObjects.Length - 1));
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Performs custom actions and raises the KeyPress event</summary>
        /// <param name="e">Event args</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            // If ctrl+A is pressed, and the select all command is viable, then select all.
            if (e.KeyChar == 'a' &&
                !m_editingRowVisible &&
                MultiSelectionEnabled)
            {
                m_selectedRows.SetRange(GetRangeOfInts(0, SelectedObjects.Length - 1));
                e.Handled = true;
            }

            base.OnKeyPress(e);
        }

        /// <summary>
        /// Processes a dialog key</summary>
        /// <param name="keyData">One of the System.Windows.Forms.Keys values that represents the key to process</param>
        /// <returns>True iff the key was processed by the control</returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (SelectedObjects == null)
                return base.ProcessDialogKey(keyData);

            if (m_columnHeaders.Dragging)
            {
                if (keyData == Keys.Escape)
                {
                    m_columnHeaders.CancelDrag();
                    return true;
                }
            }

            int rowCount = SelectedObjects.Length;

            if (m_editingRowVisible && rowCount > 0)
            {
                int lastSelected = m_selectedRows.LastSelected;
                int nextSelected = lastSelected;

                // if Esc is pressed, leave edit mode
                if (keyData == Keys.Escape)
                {
                    CancelEdit();
                    return true;
                }

                if (keyData == Keys.Enter)
                {
                    CommitEdit();
                    return true;
                }

                if (keyData == Keys.Down)
                {
                    // if wrapping is enabled or we are not at the bottom spot then select the next row
                    if (UpDownKeySelectionWrapEnabled || (nextSelected != (rowCount - 1)))
                    {
                        Property oldProperty = SelectedProperty;
                        LoseFocusOnEditingControl(); //this will save changes and set SelectedProperty to null
                        nextSelected++;
                        if (nextSelected >= rowCount)
                            nextSelected = 0;
                        SelectedProperty = oldProperty;
                    }
                }
                else if (keyData == Keys.Up)
                {
                    // if wrapping is enabled or we are not at the top spot then select the previous row
                    if (UpDownKeySelectionWrapEnabled || nextSelected != 0)
                    {
                        Property oldProperty = SelectedProperty;
                        LoseFocusOnEditingControl(); //this will save changes and set SelectedProperty to null
                        nextSelected--;
                        if (nextSelected < 0)
                            nextSelected = rowCount - 1;
                        SelectedProperty = oldProperty;
                    }
                }

                if (nextSelected != lastSelected)
                {
                    Select(nextSelected, keyData);
                    Invalidate();
                    return true;
                }

                if (keyData == Keys.Left)
                {
                    Property next = GetPreviousEditableProperty(SelectedProperty); //editable also means visible
                    if ((next != null) && (next.Control != null))
                    {
                        next.Control.Select();
                        if (next.Control.CanFocus)
                            next.Control.Focus();
                        SelectedProperty = next;
                        TryMakeSelectionVisible();
                        return true;
                    }
                }
                else if (keyData == Keys.Right)
                {
                    Property next = GetNextEditableProperty(SelectedProperty); //editable also means visible
                    if ((next != null) && (next.Control != null))
                    {
                        next.Control.Select();
                        if (next.Control.CanFocus)
                            next.Control.Focus();
                        SelectedProperty = next;
                        TryMakeSelectionVisible();
                        return true;
                    }
                }
            }
            // if at least one row is selected, and we are not in editing mode
            // allow the user to hit enter to go into editing mode
            else if (SelectedCount != 0)
            {
                // if Enter is pressed, and a property is selected, go into edit mode for this row / property
                if ((keyData == Keys.Enter) && (SelectedProperty != null) && (!SelectedProperty.DisableEditing))
                {
                    // Enter Edit Mode for the currently selected property
                    EditProperty(SelectedProperty);
                    return true;
                }
                int lastSelected = m_selectedRows.LastSelected;
                int nextSelected = lastSelected;
                Property newSelectedProperty = SelectedProperty;
                if ((keyData == Keys.Down) || (keyData == (Keys.Down | Keys.Shift)))
                {
                    // if wrapping is enabled or we are not at the bottom spot then select the next row
                    if (UpDownKeySelectionWrapEnabled || (nextSelected != (rowCount - 1)))
                    {
                        nextSelected++;
                        if (nextSelected >= rowCount)
                            nextSelected = 0;
                    }
                }
                else if ((keyData == Keys.Up) || (keyData == (Keys.Up | Keys.Shift)))
                {
                    // if wrapping is enabled or we are not at the top spot then select the previous row
                    if (UpDownKeySelectionWrapEnabled || nextSelected != 0)
                    {
                        nextSelected--;
                        if (nextSelected < 0)
                            nextSelected = rowCount - 1;
                    }
                }
                else if ((keyData == Keys.Left) || (keyData == (Keys.Left | Keys.Shift)))
                {
                    newSelectedProperty = GetPreviousProperty(SelectedProperty);//gets previous property that is visible
                }
                else if ((keyData == Keys.Right) || (keyData == (Keys.Right | Keys.Shift)))
                {
                    newSelectedProperty = GetNextProperty(SelectedProperty);//gets next property that is visible
                }

                // If a new property was selected with the Up/Down keys, set it and try to get it on screen
                if ((newSelectedProperty != null) && (!newSelectedProperty.Equals(SelectedProperty)))
                {
                    SelectedProperty = newSelectedProperty;
                    TryMakeSelectionVisible();
                    Invalidate();
                    return true;
                }

                if (nextSelected != lastSelected)
                {
                    Select(nextSelected, keyData);
                    Invalidate();
                    return true;
                }
            }

            // any changes to this statement should be reflected in the conditional at the top of this method
            // (ie, "if (SelectedObjects == null)")
            return base.ProcessDialogKey(keyData);
        }

        /// <summary>
        /// Gets the selection bounds</summary>
        /// <returns>Rectangle that bounds the selected rows and property</returns>
        private Rectangle GetLastSelectionBounds()
        {
            int selectionTop = int.MaxValue;
            int selectionBottom = int.MinValue;
            int selectionLeft;
            int selectionRight;

            // get the left - right bounds of the selected property
            if (SelectedProperty == null)
            {
                selectionLeft = 0;
                selectionRight = 0;
            }
            else
            {
                selectionLeft = GetColumnLeft(SelectedProperty);
                selectionRight = selectionLeft + GetColumnInfo(SelectedProperty).Width;
            }

            // get the top - bottom bounds of the last selected row
            int top = HeaderHeight + (m_selectedRows.LastSelected * RowHeight);
            int bottom = top + RowHeight;

            selectionTop = Math.Min(selectionTop, top);
            selectionBottom = Math.Max(selectionBottom, bottom);

            return new Rectangle(selectionLeft, selectionTop, selectionRight - selectionLeft, selectionBottom - selectionTop);
        }

        /// <summary>
        /// Tries to make the selection visible in the scrolling area</summary>
        private void TryMakeSelectionVisible()
        {
            // Dont bother if nothing is selected or if everything is selected
            if (SelectedCount != 0)
            {
                int lastVScroll = m_vScrollBar.Value;
                int lastHScroll = m_hScrollBar.Value;

                Rectangle selectionBounds = GetLastSelectionBounds();

                int height = Height - HeaderHeight;

                // check if the selection is off of the bottom of the scroll area
                if (selectionBounds.Bottom > height + m_vScroll)
                {
                    SetVerticalScroll(selectionBounds.Bottom - height);
                }

                // check if the selection is off of the top of the scroll area
                if (selectionBounds.Top < m_vScroll + HeaderHeight)
                {
                    SetVerticalScroll(selectionBounds.Top - HeaderHeight);
                }

                // check if the selection is off of the right of the scroll area
                if (selectionBounds.Right > Width + m_hScroll)
                {
                    SetHorizontalScroll(selectionBounds.Right - Width);
                }

                // check if the selection is off of the left of the scroll area
                if (selectionBounds.Left < m_hScroll)
                {
                    SetHorizontalScroll(selectionBounds.Left);
                }

                if ((lastVScroll != m_vScrollBar.Value) || (lastHScroll != m_hScrollBar.Value))
                {
                    Invalidate();
                }
            }
        }

        private void Select(int row, Keys modifiers)
        {
            // there is no standard Remove mode in Windows
            bool makeLastSelectedRowVisible = true;
            if (MultiSelectionEnabled && ((modifiers & Keys.Control) == Keys.Control))
            {
                // If the user is removing a row, it's unclear what the "last selected row" should be.
                //  So, it's best to not try to scroll. http://tracker.ship.scea.com/jira/browse/WWSATF-1423
                if (m_selectedRows.Contains(row))
                    makeLastSelectedRowVisible = false;
                m_selectedRows.Toggle(row);
            }
            else if (MultiSelectionEnabled && ((modifiers & Keys.Shift) == Keys.Shift))
                m_selectedRows.Add(row);
            else
                m_selectedRows.Set(row);

            if (makeLastSelectedRowVisible)
                TryMakeSelectionVisible();
        }

        private void SelectRange(IEnumerable<int> items, Keys modifiers)
        {
            // there is no standard Remove mode in Windows
            if ((modifiers & Keys.Control) == Keys.Control)
                m_selectedRows.ToggleRange(items);
            else if ((modifiers & Keys.Shift) == Keys.Shift)
                m_selectedRows.AddRange(items);
            else
            {
                m_selectedRows.SetRange(items);
                TryMakeSelectionVisible();
            }
        }

        private void LoseFocusOnEditingControl()
        {
            Focus();
        }

        /// <summary>
        /// Applies column widths to properties. Need to do this because properties are not persistent.</summary>
        /// <returns>True iff at least one width was applied</returns>
        private bool ApplyColumnWidths()
        {
            bool result = false;

            if (m_columnWidths != null)
            {
                foreach (Property property in Properties)
                {
                    if (m_columnWidths.ContainsKey(property.Descriptor.Name))
                    {
                        ColumnInfo columnInfo = GetColumnInfo(property);
                        if (columnInfo != null)
                        {
                            columnInfo.Width = m_columnWidths[property.Descriptor.Name];
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Applies column user hidden states to the properties. Need to do this because properties are not persistent.</summary>
        /// <returns>True iff at least one state was applied</returns>
        private bool ApplyColumnUserHiddenStates()
        {
            bool result = false;

            foreach (Property property in Properties)
            {
                if (m_columnUserHiddenStates.ContainsKey(property.Descriptor.Name))
                {
                    ColumnInfo columnInfo = GetColumnInfo(property);
                    if (columnInfo != null)
                    {
                        columnInfo.UserHidden = m_columnUserHiddenStates[property.Descriptor.Name];
                        result = true;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Apply sort by property to the properties. Need to do this because properties are not persistent.</summary>
        /// <returns>True iff at least one sort was applied</returns>
        private bool ApplySortByProperty()
        {
            bool result = false;

            foreach (Property property in Properties)
            {
                if (property.Descriptor.Name.Equals(m_sortByPropertyName))
                {
                    if (GridView.CanSort(property))
                    {
                        // sort the data
                        bool ascending = (m_sortByPropertyDirection == ListSortDirection.Ascending);
                        Sort(property, ascending);

                        // update the next sort order
                        ColumnInfo columnInfo = GetColumnInfo(property);
                        if (columnInfo != null)
                        {
                            columnInfo.NextSortDirection = ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                        }

                        Invalidate();

                        result = true;
                    }

                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Restore all of the layout state.</summary>
        private void ApplyAllLayoutState()
        {
            // restore the column ordering
            if (m_userPropertySortOrder != null)
            {
                SetCustomPropertySortOrder(m_userPropertySortOrder);
            }

            if (m_sortByPropertyName != null)
            {
                ApplySortByProperty();
            }

            // restore the column widths
            ApplyColumnWidths();

            // restore the hidden states
            ApplyColumnUserHiddenStates();
        }

        /// <summary>
        /// Performs the mouse selection logic</summary>
        /// <param name="e">The mouse event args from the MouseDown or MouseUp</param>
        private void MouseSelection(MouseEventArgs e)
        {
            HitRecord hit = Pick(e.Location);

            // Select the property right away, if we select a row later in this function we need the selected property set
            // so the TryMakeSelectionVisibility can scroll the horizontal bar to make the selected property
            // visible if it is off screen. Selecting a null property is OK.
            SelectedProperty = hit.Property;

            if (hit.Type != HitType.None)
            {
                Keys keys = Control.ModifierKeys;
                if (m_selectedRows.Count > 0 &&
                    (keys & Keys.Shift) != 0 &&
                    e.Button == MouseButtons.Left)
                {
                    // for shift-select, we must generate a range
                    int first = m_selectedRows.LastSelected;
                    int last = hit.Row;
                    if (first > last)
                    {
                        last = first;
                        first = hit.Row;
                    }
                    m_editingRowVisible = false;
                    SelectRange(Enumerable.Range(first, last - first + 1), keys);
                    // reselect the hit row to update the last selected row to the one we just clicked
                    Select(hit.Row, Keys.Shift);
                }
                else if ((keys & Keys.Control) != 0 &&
                    e.Button == MouseButtons.Left)
                {
                    // for ctrl-select, just toggle that row
                    m_editingRowVisible = false;
                    Select(hit.Row, keys);
                }
                else if (hit.Row >= 0)
                {
                    if (!m_editingRowVisible &&
                        m_selectedRows.Contains(hit.Row))
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            m_mouseUpEnablesEditingRow = true;
                            // update the last selected row to the one we just clicked
                            Select(hit.Row, Keys.Shift);
                        }
                    }
                    else
                    {
                        m_editingRowVisible = false;
                        Select(hit.Row, keys);
                    }
                }
            }
            else
            {
                // if the user clicked on the blank space below all the rows, clear the selection
                m_selectedRows.Clear();
            }
        }

        /// <summary>
        /// Returns the ColumnHeaders</summary>
        /// <returns>ColumnHeaders</returns>
        protected ColumnHeaders GetColumnHeaders()
        {
            return m_columnHeaders;
        }

        /// <summary>
        /// Gets or sets the brush used to render the background of even rows</summary>
        protected Brush EvenRowBrush 
        { 
            get { return m_evenRowBrush; } 
            set { m_evenRowBrush = value; } 
        }

        /// <summary>
        /// Gets or sets the brush used to render the background of odd rows</summary>
        protected Brush OddRowBrush 
        { 
            get { return m_oddRowBrush; } 
            set { m_oddRowBrush = value; } 
        }

        /// <summary>
        /// Performs custom actions and raises the <see cref="E:System.Windows.Forms.Control.FontChanged"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e); //updates FontHeight property
            m_columnHeaders.Height = HeaderHeight;
        }

        /// <summary>
        /// Performs custom actions and raises the <see cref="E:System.Windows.Forms.Control.Invalidated"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            m_columnHeaders.Invalidate();
            base.OnInvalidated(e);
        }

        /// <summary>
        /// Performs custom actions and raises the <see cref="E:System.Windows.Forms.Control.GotFocus"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        /// <summary>
        /// Performs custom actions and raises the <see cref="E:System.Windows.Forms.Control.LostFocus"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        /// <summary>
        /// Performs custom actions and raises the Paint event</summary>
        /// <param name="e">Event args</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            int canvasWidth = GetColumnLeft(null);
            int canvasHeight = EditingRowHeight;
            if (EditingContext != null)
                canvasHeight += RowHeight * SelectedObjects.Length;

            WinFormsUtil.UpdateScrollbars(
                m_vScrollBar,
                m_hScrollBar,
                new Size(Width, Height - HeaderHeight),
                new Size(canvasWidth, canvasHeight));

            m_vScrollBar.SmallChange = m_hScrollBar.SmallChange = RowHeight;

            PositionControls();

            base.OnPaint(e);

            if (EditingContext != null)
            {
                Graphics g = e.Graphics;

                int allRowsHeight = RowHeight * SelectedObjects.Length;
                if (m_editingRowVisible)
                    allRowsHeight += EditingRowHeight - RowHeight;

                int width = GetColumnLeft(null); // get right edge of last column

                int x = -m_hScroll;
                int y = -m_vScroll;

                Brush highlightBrush = SystemBrushes.Highlight;
                Brush highlightTextBrush = SystemBrushes.HighlightText;
                //if (!ContainsFocus) // this causes the first click to not show highlighting
                //{
                //    highlightBrush = SystemBrushes.Control;
                //    highlightTextBrush = SystemBrushes.WindowText;
                //}

                // draw background (normal rows and the property editing row)
                Rectangle rowRect = new Rectangle(x, y + HeaderHeight, width, RowHeight);
                for (int j = 0; j < SelectedObjects.Length; j++)
                {
                    int rowHeight = RowHeight;
                    Brush brush = ((j & 1) == 0) ? m_evenRowBrush : m_oddRowBrush;
                    bool selectedRow = m_selectedRows.Contains(j);
                    if (selectedRow)
                    {
                        brush = highlightBrush;

                        if (m_editingRowVisible && m_selectedRows.LastSelected == j)
                            rowHeight = EditingRowHeight;
                    }

                    FillRowBackground(g, brush, rowRect, j, selectedRow);
                    g.DrawLine(m_gridLinePen, x, rowRect.Bottom - 1, x + width - 1, rowRect.Bottom - 1);
                    rowRect.Y += rowHeight;
                }

                int startRow = GetRowAtY(HeaderHeight);
                int endRow = GetRowAtY(Height);
                if (endRow >= SelectedObjects.Length)
                    endRow = SelectedObjects.Length - 1;

                // draw cells
                int left = x;
                foreach (Property p in Properties)
                {
                    if (GetVisible(p))
                    {
                        int columnWidth = GetColumnInfo(p).Width;

                        int top = GetRowY(startRow);
                        Rectangle valueRect = new Rectangle(left, top, columnWidth, RowHeight);
                        for (int j = startRow; j <= endRow; j++)
                        {
                            bool selected = m_selectedRows.Contains(j);
                            Brush textBrush = SystemBrushes.ControlText;
                            if (p.Descriptor.IsReadOnly)
                                textBrush = SystemBrushes.GrayText;

                            if (selected)
                                textBrush = highlightTextBrush;

                            // draw a selected cell indicator on the selected property, only draw it on the last selected row
                            // dont need to draw it with the edit row. If nothing is selected, dont draw the indicator
                            if ((!m_editingRowVisible) && p.Equals(SelectedProperty) && (m_selectedRows.LastSelected == j) && (SelectedCount != 0))
                            {
                                DrawSelectedPropertyIndicator(g, valueRect);
                            }

                            // skip over the special editing row, that contains the property editing controls
                            if (selected && m_editingRowVisible && (m_selectedRows.LastSelected == j) && (!p.DisableEditing))
                            {
                                valueRect.Y += EditingRowHeight;
                            }
                            else
                            {
                                DrawValue(g, textBrush, p, valueRect, j, selected);
                                valueRect.Y += RowHeight;
                            }
                        }

                        left += columnWidth;
                    }
                    else if (p.FirstInCategory)
                    {
                        Rectangle collapsedRect = new Rectangle(left, RowHeight, RowHeight, allRowsHeight);
                        g.FillRectangle(SystemBrushes.Control, collapsedRect);
                        g.DrawString(p.Category.Name, BoldFont, SystemBrushes.ControlText, collapsedRect, s_verticalFormat);
                        left += RowHeight; // collapsed category
                    }

                    // draw column separator
                    g.DrawLine(m_gridLinePen, left - 1, y + HeaderHeight, left - 1, y + HeaderHeight + allRowsHeight - 1);
                }
            }
        }

        /// <summary>
        /// Draws the background of the entire row</summary>
        /// <param name="g">The graphics object</param>
        /// <param name="defaultBrush">Alternates to distinguish between even and odd rows, or selected rows</param>
        /// <param name="rowRect">The row rectangle in client coordinates, as would be passed to Graphics.FillRectangle</param>
        /// <param name="row">The zero-based row number, useful for getting the selected object via SelectedObjects[row]</param>
        /// <param name="selected">Whether or not this row is selected</param>
        protected virtual void FillRowBackground(Graphics g, Brush defaultBrush, Rectangle rowRect, int row, bool selected)
        {
            g.FillRectangle(defaultBrush, rowRect);
        }

        /// <summary>
        /// Draws the value of the given property of the given object (as determined by the row)</summary>
        /// <param name="g">The graphics object</param>
        /// <param name="defaultBrush">Default brush, taking into account if the row is selected, if this Control
        /// has focus, and if the property is read-only</param>
        /// <param name="p">The property, which defines the column that this value is in</param>
        /// <param name="valueRect">The rectangle in client space that the value should be drawn in</param>
        /// <param name="row">The zero-based row number that this value appears in. The selected object
        /// can be found by using SelectedObjects[row].</param>
        /// <param name="selected">Whether or not this row is selected</param>
        protected virtual void DrawValue(Graphics g, Brush defaultBrush, Property p, Rectangle valueRect, int row, bool selected)
        {
            object obj = SelectedObjects[row];
            bool isOverride = p.Descriptor.CanResetValue(obj);
            Font font = isOverride ? BoldFont : Font;
            
            string value = PropertyUtils.GetPropertyText(obj, p.Descriptor);

            g.DrawString(value, font, defaultBrush, valueRect, LeftStringFormat);
        }

        /// <summary>
        /// Measures a property for rendered size. User should override to match any custom property rendering.</summary>
        /// <param name="g">The graphics object</param>
        /// <param name="p">The property, which defines the column that this value is in</param>
        /// <param name="row">The zero-based row number that this value appears in. The selected object
        /// can be found by using SelectedObjects[row].</param>
        /// <param name="selected">Whether or not this row is selected</param>
        /// <returns>Rendered size of property</returns>
        protected virtual SizeF MeasureProperty(Graphics g, Property p, int row, bool selected)
        {
            object obj = SelectedObjects[row];
            bool isOverride = p.Descriptor.CanResetValue(obj);
            Font font = isOverride ? BoldFont : Font;

            string value = PropertyUtils.GetPropertyText(obj, p.Descriptor);

            return g.MeasureString(value, font);
        }

        /// <summary>
        /// Draws an indicator on the cell of the currently selected property</summary>
        /// <param name="g">The Graphic object</param>
        /// <param name="valueRect">The rectangle that bounds the entire cell for this value</param>
        protected virtual void DrawSelectedPropertyIndicator(Graphics g, Rectangle valueRect)
        {
            Rectangle rect = valueRect;
            rect.Inflate(-1, -1);
            rect.Offset(-1, -1);
            g.DrawRectangle(m_selectedCellPen, rect);
        }

        /// <summary>
        /// Performs custom actions and raises the MouseDown event</summary>
        /// <param name="e">Event args</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();

            // selection in the grid can be disabled for read only grids that are just used for 
            // display purposes like progress bars
            if (!SelectionEnabed)
                return;

            s_mouseDown = new Point(e.X, e.Y);
            m_mouseUpEnablesEditingRow = false;

            HitRecord hit = Pick(s_mouseDown);

            // Make a note if the user clicks on any cell of a currently selected row. This will
            //  let us know that we need to enter edit mode.
            m_clickedOnSelectedRow = hit.Property != null && m_selectedRows.Contains(hit.Row);

            // do the selection if mouse up selection is not enabled
            if (!MouseUpSelectionEnabled)
                MouseSelection(e);

            if (m_editingRowVisible &&
                (SelectedProperty != null) &&
                (SelectedProperty.Control != null))
            {
                SelectedProperty.Control.Visible = true;
                SelectedProperty.Control.Select();

                // Set input focus on parent. This will "wake up" the PropertyEditingControl, for example.
                if (SelectedProperty.Control.CanFocus)
                    SelectedProperty.Control.Focus();
            }

            Invalidate();

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Performs custom actions and raises the MouseUp event</summary>
        /// <param name="e">Event args</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            // do the selection now if mouse up selection is enabled
            if (MouseUpSelectionEnabled)
                MouseSelection(e);

            if (m_mouseUpEnablesEditingRow)
            {
                m_mouseUpEnablesEditingRow = false;

                // If the mouse hasn't been dragged since the mouse down, then let's begin editing.
                // It'd be better if we could use the mouse move distance. For now, as long as the drag
                //  is within the same cell, then we'll count it.
                HitRecord hit = Pick(e.Location);
                if (hit.Property == SelectedProperty &&
                    SelectedProperty != null &&
                    m_selectedRows.Contains(hit.Row) &&
                    (!SelectedProperty.DisableEditing) &&
                    m_clickedOnSelectedRow)
                {
                    EditProperty(SelectedProperty);
                }
            }

            base.OnMouseUp(e);
        }

        /// <summary>
        /// Performs custom actions and raises the MouseWheel event</summary>
        /// <param name="e">The event args instance containing the event data</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int value = m_vScrollBar.Value - e.Delta / 2;
            SetVerticalScroll(value);

            base.OnMouseWheel(e);
        }

        private void SetVerticalScroll(int vScroll)
        {
            m_vScrollBar.Value = Math.Max(m_vScrollBar.Minimum, Math.Min(m_vScrollBar.Maximum, vScroll));
        }

        private void SetHorizontalScroll(int hScroll)
        {
            m_hScrollBar.Value = Math.Max(m_hScrollBar.Minimum, Math.Min(m_hScrollBar.Maximum, hScroll));
        }

        /// <summary>
        /// Performs custom actions after binding has changed</summary>
        protected override void OnEditingContextChanged()
        {
            m_selectedRows.Clear();

            // apply the last sort if one was present
            if (m_lastSortProperty != null)
            {
                ColumnInfo columnInfo = GetColumnInfo(m_lastSortProperty);
                if (columnInfo != null)
                {
                    // flip the logic if the next sort acending to get if the current one is acending
                    bool currentSortAcending = !(columnInfo.NextSortDirection == ListSortDirection.Ascending);
                    Sort(m_lastSortProperty, currentSortAcending);
                    Invalidate();
                }
            }

            // text boxes with auto-complete will cause the entire screen to redraw before m_maxControlHeight has been calculated
            m_maxControlHeight = RowHeight;
            foreach (Property property in Properties)
            {
                if (property.Control != null) // needs a control?
                {
                    property.Control.BackColor = SystemColors.Control;
                    property.Control.Height = RowHeight;
                    SetFont(property.Control, property.Descriptor);

                    PropertyEditingControl editingControl = property.Control as PropertyEditingControl;
                    if (editingControl != null)
                    {
                        // text boxes with auto-complete will cause the entire screen to redraw before this Bind() call returns
                        editingControl.Bind(property.Context);
                        editingControl.PropertyEdited -= propertyEditingControl_PropertyEdited; //to avoid duplicates
                        editingControl.PropertyEdited += propertyEditingControl_PropertyEdited;
                    }

                    // we want to be notified whenever any editing control has lost focus
                    AddLostFocusHandlerRecursively(property.Control);
                }

                if (property.Control != null)
                {
                    m_maxControlHeight = Math.Max(m_maxControlHeight, property.Control.Height);
                }
            }

            // restore all of the layout state
            ApplyAllLayoutState();
        }

        /// <summary>
        /// Performs custom actions after visibility has changed.</summary>
        /// <param name="e">Event Args</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            // if the control is now visible apply all of the layout state
            if (Visible)
                ApplyAllLayoutState();
        }

        /// <summary>
        /// Gets a control for editing the given property</summary>
        /// <param name="property">Property to be edited</param>
        /// <returns>Control for editing the given property</returns>
        protected override Control GetEditingControl(Property property)
        {
            // if editing is disabled, return a null edit control for this property.
            if (property.DisableEditing)
            {
                return null;
            }

            Control control = base.GetEditingControl(property);
            if (control == null)
            {
                PropertyEditingControl editingControl = new PropertyEditingControl();
                editingControl.Bind(property.Context);
                editingControl.Height = RowHeight;
                control = editingControl;
            }

            return control;
        }

        /// <summary>
        /// Sets the column width for a property</summary>
        /// <param name="property">The property to set the column width for</param>
        /// <param name="width">New width</param>
        protected void SetPropertyColumnWidth(Property property, int width)
        {
            // update the persistent column width state
            if (m_columnWidths.ContainsKey(property.Descriptor.Name))
                m_columnWidths[property.Descriptor.Name] = width;
            else
                m_columnWidths.Add(property.Descriptor.Name, width);

            // update the non persistent property state
            m_columnInfo[property.Descriptor].Width = width;
        }

        private void vScrollBar_ValueChanged(object sender, EventArgs e)
        {
            m_vScroll = m_vScrollBar.Value;

            Invalidate();
        }

        private void hScrollBar_ValueChanged(object sender, EventArgs e)
        {
            m_hScroll = m_hScrollBar.Value;

            Invalidate();
        }

        private void selectedRows_Changed(object sender, EventArgs e)
        {
            // clearing the selection always removes the editing row
            if (m_selectedRows.Count == 0)
                m_editingRowVisible = false;

            Invalidate();

            // If the editing row is visible and the user has used the up/down arrow keys, for example,
            //  to change the selection, we want to update the editing controls and set the focus on the
            //  editing control for the current property.
            if (m_editingRowVisible)
            {
                foreach (Property p in Properties)
                {
                    if (p.Control != null)
                        p.Control.Refresh();
                }

                if (SelectedProperty != null)
                    SelectedProperty.Control.Select();
            }

            OnSelectedRowsChanged(e);
        }

        private void propertyEditingControl_PropertyEdited(object sender, PropertyEditedEventArgs e)
        {
            // figure out the index of the row that changed
            // keep an eye on this, if it gets slow we may need to replace with
            // a hash lookup
            int index = 0;
            foreach (object item in EditingContext.Items)
            {
                if (item.Equals(e.Owner))
                {
                    // create a new event using the row index
                    OnRowValueChanged(new RowChangedEventArgs(index));
                    break;
                }
                index++;
            }
        }

        /// <summary>
        /// Adds the LostFocus event handler to this control and to its children recursively</summary>
        /// <param name="control">The control to add a new handler to and its children</param>
        private void AddLostFocusHandlerRecursively(Control control)
        {
            control.LostFocus -= propertyEditingControl_LostFocus; //to avoid duplicates
            control.LostFocus += propertyEditingControl_LostFocus;
            foreach (Control child in control.Controls)
                AddLostFocusHandlerRecursively(child);

            // check for forms owner and add those controls as well
            IFormsOwner formsOwner = control as IFormsOwner;
            if (formsOwner != null)
                foreach (Form form in formsOwner.Forms)
                    AddLostFocusHandlerRecursively(form);
        }

        /// <summary>
        /// Checks to see if the any of the controls in the grid contain focus. If not, leaves edit mode.</summary>
        private void LeaveEditModeWhenGrinDoesNotContainFocus()
        {
            bool containsFocus = ContainsFocus;

            if (!containsFocus)
            {
                foreach (Property p in Properties)
                {
                    if (p.Control != null)
                    {
                        containsFocus |= p.Control.ContainsFocus;

                        // check for forms owner and add those controls as well
                        IFormsOwner formsOwner = p.Control as IFormsOwner;
                        if (formsOwner != null)
                            foreach (Form form in formsOwner.Forms)
                                containsFocus |= form.ContainsFocus;

                        if (containsFocus)
                            break;
                    }
                }
            }

            // we left the GridView control, cancel edits if there are any happening
            if (!containsFocus)
                CancelEdit();
        }

        /// <summary>
        /// Performs custom actions on LoseFocus events. Any time an edit control loses focus, check to see if we have basically left the gridview entirely.</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private void propertyEditingControl_LostFocus(object sender, EventArgs e)
        {
            LeaveEditModeWhenGrinDoesNotContainFocus();
        }

        private int GetColumnLeft(Property property)
        {
            int left = 0;
            foreach (Property p in Properties)
            {
                if (p.FirstInCategory)
                {
                    if (!p.Category.Expanded)
                        left += RowHeight;
                }

                if (p == property)
                    return left;

                if (GetVisible(p))
                {
                    int width = GetColumnInfo(p).Width;
                    left += width;
                }
            }

            return left;
        }

        private void TurnOnEditingRow()
        {
            if (!m_editingRowVisible)
            {
                m_editingRowVisible = true;
                foreach (Property p in Properties)
                {
                    if ((p.Control != null) && GetVisible(p))
                    {
                        p.Control.Visible = true;
                        p.Control.Refresh();
                    }
                }
            }
        }

        /// <summary>
        /// Enters edit mode for a property</summary>
        /// <param name="property">Property to edit</param>
        private void EditProperty(Property property)
        {
            TurnOnEditingRow();
            if (property.Control != null)
            {
                property.Control.Select();

                // Set input focus on parent. This will "wake up" the PropertyEditingControl, for example.
                if (property.Control.CanFocus)
                    property.Control.Focus();
            }

            // refresh the control to make sure any custom editing controls are refreshed
            // with data from the property that this edit control represents
            foreach (Property p in Properties)
            {
                if (p.Control != null)
                {
                    p.Control.Refresh();
                }
            }
        }

        /// <summary>
        /// Commits the current edit operation</summary>
        private void CommitEdit()
        {
            //this will save changes and set SelectedProperty to null
            LoseFocusOnEditingControl();

            m_editingRowVisible = false;

            // clear all the edit controls
            foreach (Property p in Properties)
            {
                if (p.Control != null)
                    p.Control.Visible = false;
            }

            // set the focus back to the grid
            if (CanFocus)
                Focus();
        }

        /// <summary>
        /// Cancels the current edit operation</summary>
        private void CancelEdit()
        {
            m_editingRowVisible = false;

            // clear all the edit controls
            foreach (Property p in Properties)
            {
                if (p.Control != null)
                    p.Control.Visible = false;
            }

            // set the focus back to the grid
            if (CanFocus)
                Focus();
        }

        private enum HitType
        {
            None,
            CategoryExpander,
            ColumnHeader,
            ColumnHeaderRightEdge,
            Cell,
        }

        private class HitRecord
        {
            public HitType Type = HitType.None;
            public Category Category;
            public Property Property;
            public int Row = -1;
            public Point Offset;
        }

        private HitRecord Pick(Point pt)
        {
            HitRecord result = new HitRecord();

            int left = -m_hScroll;
            int xPadding = Margin.Left;

            foreach (Property p in Properties)
            {
                if (p.FirstInCategory)
                {
                    if (pt.Y < HeaderHeight &&
                        pt.X >= left &&
                        pt.X <= left + ExpanderSize + 2 * xPadding)
                    {
                        result.Type = HitType.CategoryExpander;
                        result.Property = p;
                        result.Category = p.Category;
                        result.Row = -1;
                        result.Offset = new Point(pt.X - left, pt.Y);
                        return result;
                    }

                    if (!GetVisible(p))
                        left += RowHeight;
                }

                if (GetVisible(p))
                {
                    int width = GetColumnInfo(p).Width;
                    if (pt.X >= left && pt.X <= left + width)
                    {
                        result.Property = p;
                        if ((pt.Y >= 0) && (pt.Y < HeaderHeight))
                        {
                            if (Math.Abs(left + width - pt.X) < SystemDragSize.Width)
                            {
                                result.Type = HitType.ColumnHeaderRightEdge;
                                result.Offset = new Point(pt.X - left, pt.Y);
                                return result;
                            }
                            else
                            {
                                result.Type = HitType.ColumnHeader;
                                result.Offset = new Point(pt.X - left, pt.Y);
                                return result;
                            }
                        }

                        // check if after the property editing row
                        int row = GetRowAtY(pt.Y);
                        if (row < SelectedObjects.Length)
                        {
                            result.Offset = new Point(pt.X - left, pt.Y % RowHeight);
                            result.Type = HitType.Cell;
                            result.Row = row;
                        }
                        return result;
                    }

                    left += width;
                }
            }

            return result;
        }

        private ColumnInfo GetColumnInfo(Property p)
        {
            ColumnInfo columnInfo;
            if (!m_columnInfo.TryGetValue(p.Descriptor, out columnInfo))
            {
                columnInfo = new ColumnInfo();
                columnInfo.Width = DefaultColumnWidth;
                if (p.Control != null && p.Control.Width > 0)
                    columnInfo.Width = p.Control.Width;
                else
                {
                    // try persisted settings
                    int width;
                    if (m_savedColumnWidths.TryGetValue(p.Descriptor.Name + p.Descriptor.PropertyType, out width))
                    {
                        columnInfo.Width = width;
                    }
                    else if (p.DefaultWidth != 0)
                    {
                        columnInfo.Width = p.DefaultWidth;
                    }
                }
                m_columnInfo.Add(p.Descriptor, columnInfo);
            }

            return columnInfo;
        }

        private void PositionControls()
        {
            int lastSelected = m_selectedRows.LastSelected;
            int top = -m_vScroll + HeaderHeight + lastSelected * RowHeight + BorderWidth; // bottom of selected row
            int left = -m_hScroll;
            int tabIndex = 0;
            foreach (Property p in Properties)
            {
                if (p.Control != null)
                {
                    p.Control.Visible = m_editingRowVisible && left >=0 && GetVisible(p);
                }
                int width = 0;
                if (GetVisible(p))
                {
                    width = GetColumnInfo(p).Width;
                    if (p.Control != null)
                    {
                        p.Control.Top = top;
                        p.Control.Left = left;
                        p.Control.Width = width - 1;
                        p.Control.TabIndex = tabIndex++;
                    }
                }
                else if (p.FirstInCategory)
                {
                    width = RowHeight;
                }

                left += width;
            }
        }

        private void SetPens()
        {
            Color backColor = BackColor;
            Color color = Color.FromArgb(255, backColor.R * 200 / 255, backColor.G * 200 / 255, backColor.B * 200 / 255);
            m_gridLinePen.Color = color;
            m_selectedCellPen.Color = Color.LightGray;
            m_selectedCellPen.DashStyle = DashStyle.Dash;
        }

        // get the index of the row under the given y; the value may be outside the legal
        //  range [0..rows-1]
        private int GetRowAtY(int y)
        {
            int top = -m_vScroll + HeaderHeight;

            if (m_editingRowVisible)
            {
                int lastSelectedRow = m_selectedRows.LastSelected;
                int editingRowTop = top + lastSelectedRow * RowHeight;
                if (y > editingRowTop)
                {
                    int editingRowBottom = editingRowTop + EditingRowHeight;
                    if (y < editingRowBottom)
                        return lastSelectedRow;
                    return lastSelectedRow + ((y - editingRowBottom) / RowHeight) + 1;
                }
            }

            int row = (y - top) / RowHeight;
            return row;
        }

        // Gets the y-coordinate of the top of the given row index.
        private int GetRowY(int row)
        {
            int y = -m_vScroll + HeaderHeight + row * RowHeight;
            if (m_editingRowVisible && m_selectedRows.LastSelected < row)
                y += EditingRowHeight - RowHeight;

            return y;
        }

        // Gets the height of the special row that has the property editing controls
        private int EditingRowHeight
        {
            get { return m_maxControlHeight + 2 * BorderWidth; }
        }

        // Gets the height of the header row
        private int HeaderHeight
        {
            get { return RowHeight; }
        }

        /// <summary>
        /// Reads persisted control setting information from the given XML element</summary>
        /// <param name="root">Root element of XML settings</param>
        protected override void ReadSettings(XmlElement root)
        {
            XmlNodeList columns = root.SelectNodes("PropertyDescriptors");
            if (columns == null || columns.Count == 0)
                return;
            if (columns.Count >1)
                throw new Exception("Duplicated PropertyDescriptors settings");
            XmlElement propertyDescriptors = (XmlElement) columns[0];
            foreach (XmlElement columnElement in propertyDescriptors)
            {
                string name = columnElement.GetAttribute("Name");
                string propertyType = columnElement.GetAttribute("PropertyType");
                string widthString = columnElement.GetAttribute("Width");
                int width;
                if (widthString != null && int.TryParse(widthString, out width))
                {
                    m_savedColumnWidths[name + propertyType] = width;
                }
            }
        }

        /// <summary>
        /// Writes persisted control setting information to the given XML element</summary>
        /// <param name="root">Root element of XML settings</param>
        protected override void WriteSettings(XmlElement root)
        {
            XmlDocument xmlDoc = root.OwnerDocument;
            XmlElement columnsElement = xmlDoc.CreateElement("PropertyDescriptors");
            root.AppendChild(columnsElement);
            foreach (KeyValuePair<PropertyDescriptor, ColumnInfo> pair in m_columnInfo)
            {
                XmlElement columnElement = xmlDoc.CreateElement("Descriptor");
                columnElement.SetAttribute("Name", pair.Key.Name);
                columnElement.SetAttribute("PropertyType", pair.Key.PropertyType.ToString());
                columnElement.SetAttribute("Width", pair.Value.Width.ToString());
                columnsElement.AppendChild(columnElement);
            }
        }


        #region Sorting Objects by Property

        private static bool CanSort(Property property)
        {
            return (!property.DisableSort) &&
                typeof(IComparable).IsAssignableFrom(property.Descriptor.PropertyType);
        }

        private void Sort(Property property, bool ascending)
        {
            // create an array of all the items in the grid
            object[] allObjects = SelectedObjects;

            // create a list of all the objects that are currently selected
            List<object> lastSelectedObjects = new List<object>();
            foreach (int index in m_selectedRows)
            {
                lastSelectedObjects.Add(allObjects[index]);
            }

            // See if the property descriptor we're looking at provides a custom sorting interface
            IComparer comparer = null;
            IPropertyCustomSorter sortingDescriptor = property.Descriptor as IPropertyCustomSorter;
            if (sortingDescriptor != null)
            {
                comparer = sortingDescriptor.GetComparer(ascending);
            }

            // If we didn't get a custom comparitor, just use the default one
            if (comparer == null)
            {
                comparer = new PropertyComparer(property.Descriptor, ascending);
            }

            // apply the sort
            Array.Sort(SelectedObjects, comparer);
            m_lastSortProperty = property;
            m_sortByPropertyName = m_lastSortProperty.Descriptor.Name;
            m_sortByPropertyDirection = ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;

            // create the indexes for the selected objects in the new sort order
            List<int> selectedIndicies = new List<int>();
            foreach (object lastObject in lastSelectedObjects)
            {
                selectedIndicies.Add(Array.IndexOf(allObjects, lastObject));
            }

            // restore the selection
            SelectRange(selectedIndicies, 0);
        }

        private class PropertyComparer : IComparer
        {
            public PropertyComparer(PropertyDescriptor descriptor, bool ascending)
            {
                m_descriptor = descriptor;
                m_ascending = ascending;
            }

            private readonly PropertyDescriptor m_descriptor;
            private readonly bool m_ascending;

            public int Compare(object x, object y)
            {
                IComparable xValue = m_descriptor.GetValue(x) as IComparable;
                IComparable yValue = m_descriptor.GetValue(y) as IComparable;
                if (xValue != null)
                {
                    if (yValue != null)
                    {
                        int result = xValue.CompareTo(yValue);
                        if (!m_ascending)
                            result *= -1;
                        return result;
                    }
                    return -1;
                }
                return 1;
            }
        }

        #endregion

        #region ColumnHeaders Public Class

        /// <summary>
        /// The column headers must be a child control, so that when the user scrolls down,
        /// the embedded controls won't obscure the headers</summary>
        public class ColumnHeaders : Control
        {
            /// <summary>
            /// Constructor for customization. Be sure to set the GridView property after contruction when using this constructor.</summary>
            public ColumnHeaders()
            {
                base.DoubleBuffered = true;
            }

            /// <summary>
            /// Constructor specifying GridView</summary>
            /// <param name="gridView">GridView</param>
            public ColumnHeaders(GridView gridView)
            {
                m_gridView = gridView;

                base.DoubleBuffered = true;
            }

            /// <summary>
            /// Gets or sets the GridView. Be sure to set a grid view if using the default constructor</summary>
            public GridView GridView
            {
                get { return m_gridView; }
                set { m_gridView = value; }
            }

            /// <summary>
            /// Raises Paint event and performs custom actions</summary>
            /// <param name="e">Paint event args</param>
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                Graphics g = e.Graphics;

                int x = -m_gridView.m_hScroll;
                int rowHeight = m_gridView.RowHeight;
                int xPadding = Margin.Left;
                int yPadding = Margin.Top;
                int draggingClickOffsetX = 0;

                // draw column headers
                int left = x;
                int index = 0;
                foreach (Property p in m_gridView.Properties)
                {
                    if (p.Equals(s_columnHeaderMouseDownProperty))
                    {
                        draggingClickOffsetX = s_mouseDown.X - left;
                        s_columnHeaderMouseDownPropertyIndex = index;
                    }

                    // draw header and text
                    if (m_gridView.GetVisible(p))
                    {
                        DrawColumnHeader(g, p, left, 0, rowHeight, xPadding, yPadding, m_columnHeaderBrush);

                        left += m_gridView.GetColumnInfo(p).Width;
                    }
                    else if (p.FirstInCategory) // collapsed category
                    {
                        ControlPaint.DrawBorder3D(g, left, 0, rowHeight, rowHeight, Border3DStyle.Etched, Border3DSide.Bottom | Border3DSide.Right);
                        Sce.Atf.GdiUtil.DrawExpander(left + xPadding, yPadding, false, g);
                        left += rowHeight;
                    }

                    index++;
                }

                // draw the column header again if the user is dragging it around
                if (s_draggingColumnHeader && (s_columnHeaderMouseDownProperty != null))
                {
                    // draw an indicator where the column header would fall if we were to drop it right here
                    DrawDropColumnIndicator(g, m_dropColumnX, 0, rowHeight);

                    // draw the floating column header
                    DrawColumnHeader(g, s_columnHeaderMouseDownProperty, s_mouseMove.X - draggingClickOffsetX, 0, rowHeight, xPadding, yPadding, m_columnHeaderAlphaBrush);
                }
            }

            /// <summary>
            /// Draws an indicator to show where a dragged column header will be dropped</summary>
            /// <param name="g">The Graphics object</param>
            /// <param name="x">X position of indicator</param>
            /// <param name="y">Y position of indicator</param>
            /// <param name="height">Height of indicator</param>
            private void DrawDropColumnIndicator(Graphics g, int x, int y, int height)
            {
                g.DrawLine(m_dropColumnIndicatorPen, x-1, y, x-1, y + height);
            }

            /// <summary>
            /// Draws the ColumnHeader</summary>
            /// <param name="g">The Graphics object</param>
            /// <param name="p">The property for this column header</param>
            /// <param name="x">X-coordinate of upper left corner of ColumnHeader location</param>
            /// <param name="y">Y-coordinate of upper left corner of ColumnHeader location</param>
            /// <param name="rowHeight">Row height of the grid</param>
            /// <param name="xPadding">X padding of the grid cells</param>
            /// <param name="yPadding">Y padding of the grid cells</param>
            /// <param name="menuBar">Menubar brush</param>
            private void DrawColumnHeader(Graphics g, Property p, int x, int y, int rowHeight, int xPadding, int yPadding, Brush menuBar)
            {
                ColumnInfo info = m_gridView.GetColumnInfo(p);
                int width = info.Width;

                Rectangle textRect = new Rectangle(x, y, width, rowHeight);

                // fill a rectangle with the menu bar color
                g.FillRectangle(menuBar, textRect);

                ControlPaint.DrawBorder3D(g, x, y, width, rowHeight, Border3DStyle.Etched, Border3DSide.Bottom | Border3DSide.Right);

                if (p.FirstInCategory)
                {
                    Sce.Atf.GdiUtil.DrawExpander(x + xPadding, y + yPadding, p.Category.Expanded, g);

                    int offset = xPadding + ExpanderSize + xPadding;
                    textRect.X += offset;
                    textRect.Width -= offset;

                    string categoryName = p.Category.Name + ": ";
                    if (!p.HideDisplayName)
                        g.DrawString(categoryName, m_gridView.BoldFont, SystemBrushes.ControlText, textRect, LeftStringFormat);

                    offset = (int)(g.MeasureString(categoryName, m_gridView.BoldFont).Width);
                    textRect.X += offset;
                    textRect.Width -= offset;
                }

                if (CanSort(p))
                {
                    textRect.Width -= SortDirectionIndicatorWidth + xPadding;
                    Sce.Atf.GdiUtil.DrawSortDirectionIndicator(
                        textRect.Right + xPadding,
                        textRect.Top + rowHeight / 2 - 1 - Sce.Atf.GdiUtil.SortDirectionIndicatorHeight / 2,
                        info.NextSortDirection == ListSortDirection.Descending,
                        g);
                }

                if (!p.HideDisplayName)
                    g.DrawString(p.Descriptor.DisplayName, m_gridView.Font, SystemBrushes.ControlText, textRect, LeftStringFormat);
            }

            /// <summary>
            /// Gets the measured size of the column header for this property. Users should
            /// override this method to match any custom rendering.</summary>
            /// <param name="firstInCategory">Whether property is first in category</param>
            /// <param name="categoryName">Property category name</param>
            /// <param name="displayName">Property name</param>
            /// <param name="canSort">Whether can sort by this property</param>
            /// <returns>The size of the column header rendering for this property</returns>
            public virtual SizeF MeasureProperty(bool firstInCategory, string categoryName, string displayName, bool canSort)
            {
                Graphics g = CreateGraphics();

                SizeF measuredSize = new SizeF(0, 0);

                if (firstInCategory)
                {
                    measuredSize = g.MeasureString(categoryName + ": ", m_gridView.BoldFont);
                }

                SizeF nameSize = g.MeasureString(displayName, m_gridView.Font);

                measuredSize.Width += nameSize.Width;
                measuredSize.Height = Math.Max(measuredSize.Height, nameSize.Height);

                if (canSort)
                {
                    measuredSize.Width += (SortDirectionIndicatorWidth + Margin.Left);
                }

                return measuredSize;
            }

            /// <summary>
            /// Performs custom actions and raises the MouseDown event</summary>
            /// <param name="e">Event args</param>
            protected override void OnMouseDown(MouseEventArgs e)
            {
                s_mouseDown = new Point(e.X, e.Y);

                m_gridView.Focus();

                HitRecord hit = m_gridView.Pick(s_mouseDown);

                switch (hit.Type)
                {
                    case HitType.ColumnHeader:

                        // TODO: Why does hitting the column header select a property?
                        m_gridView.SelectedProperty = hit.Property;

                        s_columnHeaderMouseDown = true;
                        s_columnHeaderMouseDownProperty = hit.Property;
                        break;

                    case HitType.ColumnHeaderRightEdge:
                        m_gridView.SelectedProperty = hit.Property;

                        m_gridView.Select();

                        if (!hit.Property.DisableResize)
                        {
                            s_sizing = true;
                            s_sizingProperty = hit.Property;
                            s_sizingOriginalWidth = m_gridView.GetColumnInfo(s_sizingProperty).Width;
                            Cursor = Cursors.VSplit;
                        }
                        break;

                    case HitType.CategoryExpander:
                        hit.Category.Expanded = !hit.Category.Expanded;
                        m_gridView.Invalidate(); 
                        break;
                }

                base.OnMouseDown(e);
            }

            /// <summary>
            /// Performs custom actions and raises the MouseMove event</summary>
            /// <param name="e">Event args</param>
            protected override void OnMouseMove(MouseEventArgs e)
            {
                s_mouseMove = e.Location;

                if (s_sizing && (e.Button & MouseButtons.Left) != 0)
                {
                    int dx = e.X - s_mouseDown.X;
                    int newWidth = s_sizingOriginalWidth + dx;
                    newWidth = Math.Max(newWidth, MinimumColumnWidth);
                    m_gridView.SetPropertyColumnWidth(s_sizingProperty, newWidth);
                    m_gridView.Invalidate();
                }
                else if (e.Button == MouseButtons.None)
                {
                    HitRecord hit = m_gridView.Pick(new Point(e.X, e.Y));
                    if ((hit.Type == HitType.ColumnHeaderRightEdge) && (!hit.Property.DisableResize))
                        Cursor = Cursors.VSplit;
                    else
                        Cursor = Cursors.Arrow;
                }

                if (m_gridView.DragDropColumnsEnabed &&
                    (s_columnHeaderMouseDownProperty != null) &&
                    (!s_columnHeaderMouseDownProperty.DisableDragging))
                {
                    // check to see if we should initiate a drag and drop of a column header
                    // calc how far the mouse moved to make sure the mouse has moved a bit so we dont kick off a drag event on tiny movements
                    int deltaSquared = ((e.Location.X - s_mouseDown.X) * (e.Location.X - s_mouseDown.X)) +
                                       ((e.Location.Y - s_mouseDown.Y) * (e.Location.Y - s_mouseDown.Y));

                    if ((s_columnHeaderMouseDown) && (e.Button == MouseButtons.Left) && (deltaSquared > 2))
                    {
                        s_draggingColumnHeader = true;
                    }
                }

                if (s_draggingColumnHeader)
                {
                    // figure out where to draw the drop column indicator and what property index we would insert at
                    int left = -m_gridView.m_hScroll;
                    int index = 0;
                    m_dropColumnIndex = index;
                    m_dropColumnX = 0;
                    foreach (Property p in m_gridView.Properties)
                    {
                        if (m_gridView.GetVisible(p))
                        {
                            // walk the m_dropColumnIndex past columns that are not draggable. 
                            // we dont want to drop a column that would cause not draggable columns to move around
                            if (p.DisableDragging &&
                                (index == m_dropColumnIndex))
                            {
                                m_dropColumnX += m_gridView.GetColumnInfo(p).Width;
                                m_dropColumnIndex++;
                            }

                            // remember were the indicator should be drawn
                            if (s_mouseMove.X >= (left + (int)(m_gridView.GetColumnInfo(p).Width * 0.5)))
                            {
                                m_dropColumnX = left + m_gridView.GetColumnInfo(p).Width;
                                m_dropColumnIndex = index + 1;
                            }

                            left += m_gridView.GetColumnInfo(p).Width;
                        }
                        else if (p.FirstInCategory) // collapsed category
                        {
                            left += m_gridView.RowHeight;
                        }

                        index++;
                    }

                    // special case hacks so the indicator is clearly visible when it is on the edge of the grid
                    if (m_dropColumnIndex == 0)
                    {
                        m_dropColumnX += 2;
                    }
                    if (m_dropColumnIndex == index)
                    {
                        m_dropColumnX -= 1;
                    }

                    Invalidate();
                }

                base.OnMouseMove(e);
            }

            /// <summary>
            /// Performs custom actions and raises the MouseUp event</summary>
            /// <param name="e">Event args</param>
            protected override void OnMouseUp(MouseEventArgs e)
            {
                Cursor = Cursors.Arrow;
                s_sizing = false;

                // Check if we are releasing a drop column
                if (s_draggingColumnHeader)
                {
                    // make sure we dropped to a new location, if the location is the same don't bother doing anything
                    if (s_columnHeaderMouseDownPropertyIndex != m_dropColumnIndex)
                    {
                        List<string> sortOrder = new List<string>();

                        foreach (Property p in m_gridView.Properties)
                            sortOrder.Add(p.Descriptor.Name);

                        // Remove the property from the list
                        if (sortOrder.Remove(s_columnHeaderMouseDownProperty.Descriptor.Name))
                        {
                            // if we removed a property below the index we are going to insert at
                            // we need to adjust the index we use to insert with
                            if (s_columnHeaderMouseDownPropertyIndex < m_dropColumnIndex)
                            {
                                m_dropColumnIndex--;
                            }
                            
                            // insert the property back in the new location
                            sortOrder.Insert(m_dropColumnIndex, s_columnHeaderMouseDownProperty.Descriptor.Name);
                        }

                        // Set the new custom sort order
                        m_gridView.SetCustomPropertySortOrder(sortOrder);

                        // we did a reorder, need to invalidate all of the grid columns
                        m_gridView.Invalidate();
                    }

                    // invalidate the columnheaders for possible reorder and to get ride of the float column
                    Invalidate();
                }
                else if (s_columnHeaderMouseDown)
                {
                    HitRecord hit = m_gridView.Pick(e.Location);
                    if ((hit.Type == HitType.ColumnHeader) && GridView.CanSort(hit.Property))
                    {
                        // if the mouse up happend on the same property, apply the toggle sort
                        if (s_columnHeaderMouseDownProperty.Equals(hit.Property))
                        {
                            ColumnInfo columnInfo = m_gridView.GetColumnInfo(hit.Property);
                            m_gridView.Sort(hit.Property, columnInfo.NextSortDirection == ListSortDirection.Ascending);
                            columnInfo.NextSortDirection ^= ListSortDirection.Descending; // toggle direction between ascending/descending
                            if (m_gridView.m_editingRowVisible)
                            {
                                // Commit any editing changes.
                                m_gridView.LoseFocusOnEditingControl();
                                // If row order changes, we don't want to edit & display wrong row.
                                m_gridView.m_editingRowVisible = false;
                            }
                            m_gridView.Invalidate();
                        }
                    }
                }

                s_columnHeaderMouseDownProperty = null;
                s_columnHeaderMouseDown = false;
                s_draggingColumnHeader = false;

                base.OnMouseUp(e);
            }

            /// <summary>
            /// Gets whether dragging column header</summary>
            public bool Dragging
            {
                get { return s_draggingColumnHeader;  }
            }

            /// <summary>
            /// Cancels column header dragging operation</summary>
            public void CancelDrag()
            {
                s_draggingColumnHeader = false;
                s_columnHeaderMouseDownProperty = null;
                s_columnHeaderMouseDown = false;

                Invalidate();
            }

            private GridView m_gridView;

            private Brush m_columnHeaderAlphaBrush = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
            private Brush m_columnHeaderBrush = SystemBrushes.ControlLightLight;
            private Pen m_dropColumnIndicatorPen = new Pen(Color.Black, 3.0f);
            private int m_dropColumnX;
            private int m_dropColumnIndex;

            private static Point s_mouseDown;
            private static Point s_mouseMove;
            private static Property s_sizingProperty;
            private static int s_sizingOriginalWidth;
            private static bool s_sizing;
            private static Property s_columnHeaderMouseDownProperty;
            private static int s_columnHeaderMouseDownPropertyIndex;
            private static bool s_columnHeaderMouseDown;
            private static bool s_draggingColumnHeader;
            /// <summary>
            /// Minimum column width</summary>
            public const int MinimumColumnWidth = 24;
        }

        #endregion

        private const int BorderWidth = 1;

        private const int DefaultColumnWidth = 100;
        private const int SortDirectionIndicatorWidth = 16;

        private class ColumnInfo
        {
            public int Width;
            public ListSortDirection NextSortDirection;
            public bool UserHidden;
        }

        // property that was used by the last sort, null if no sort
        private Property m_lastSortProperty;

        private readonly Dictionary<PropertyDescriptor, ColumnInfo> m_columnInfo =
            new Dictionary<PropertyDescriptor, ColumnInfo>(); // cache widths for descriptors
        private readonly Dictionary<string , int> m_savedColumnWidths=
             new Dictionary<string, int>(); // last saved column widths 

        private bool m_editingRowVisible; //used to be equivalent to (m_selectedRows.Count > 0)
        private bool m_mouseUpEnablesEditingRow;
        //When changing m_selectedRows, consider if m_editingRowVisible should be changed, too.
        private readonly Selection<int> m_selectedRows;
 
        private readonly Pen m_gridLinePen = new Pen(Color.Transparent);
        private readonly Pen m_selectedCellPen = new Pen(Color.Transparent);
        private Brush m_evenRowBrush;
        private Brush m_oddRowBrush;

        private readonly ColumnHeaders m_columnHeaders;

        private readonly VScrollBar m_vScrollBar;
        private int m_vScroll;

        private readonly HScrollBar m_hScrollBar;
        private int m_hScroll;

        private int m_maxControlHeight;
        private List<string> m_userPropertySortOrder = new List<string>();
        private Dictionary<string, int> m_columnWidths = new Dictionary<string, int>();
        private string m_sortByPropertyName;
        private ListSortDirection m_sortByPropertyDirection;
        private Dictionary<string, bool> m_columnUserHiddenStates = new Dictionary<string, bool>();
        private bool m_clickedOnSelectedRow;

        /// <summary>
        /// Static constructor</summary>
        static GridView()
        {
            s_verticalFormat = new StringFormat(StringFormatFlags.DirectionVertical | StringFormatFlags.NoWrap);
            s_verticalFormat.Alignment = StringAlignment.Near;
            s_verticalFormat.Trimming = StringTrimming.EllipsisCharacter;
        }

        private static Point s_mouseDown;
        private static readonly StringFormat s_verticalFormat;
    }

    /// <summary>
    /// Row changed event arguments</summary>
    public class RowChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="rowIndex">The index of the row that changed</param>
        public RowChangedEventArgs(int rowIndex)
        {
            RowIndex = rowIndex;
        }

        /// <summary>
        /// The index of the row that was changed</summary>
        public readonly int RowIndex;
    }
}
