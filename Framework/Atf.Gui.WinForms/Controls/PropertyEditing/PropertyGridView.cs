//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf.Applications;
using Sce.Atf.Dom;
using PropertyDescriptor = System.ComponentModel.PropertyDescriptor;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Control for displaying properties in a two column grid, with property names on the left
    /// and property values on the right. Properties with associated IPropertyEditor instances
    /// can embed controls into the right column, while all other properties are edited in
    /// a standard .NET way with a <see cref="PropertyEditingControl"/>.</summary>
    public class PropertyGridView : PropertyView
    {
        /// <summary>
        /// Constructor</summary>
        public PropertyGridView()
        {
            SuspendLayout();

            m_overlayButtonToolTip = new ToolTip();
            m_resetButton = new OverlayButton(this);
            m_resetButton.ToolTip = m_overlayButtonToolTip;
            m_resetButton.ToolTipText = "Resets the property to its default value".Localize();
            m_resetButton.BackgroundImage = ResourceUtil.GetImage16(Resources.ResetImage);

            m_scrollBar = new VScrollBar();
            m_scrollBar.Dock = DockStyle.Right;
            m_scrollBar.ValueChanged += scrollBar_ValueChanged;

            m_toolTip = new ToolTip();

            m_editingControl = new PropertyEditingControl();

            m_editingControl.TabStop = false;
            m_editingControl.EditButtonSize = new Size(17, 17);

            m_editingControl.DragOver += editingControl_DragOver;
            m_editingControl.DragDrop += editingControl_DragDrop;
            m_editingControl.MouseHover += editingControl_MouseHover;
            m_editingControl.MouseLeave += editingControl_MouseLeave;

            CategoryBackgroundBrush = new SolidBrush(SystemColors.ControlLight);
            CategoryNameBrush = new SolidBrush(SystemColors.ControlText);
            CategoryLinePen = new Pen(SystemColors.Control);
            CategoryExpanderPen = new Pen(SystemColors.ControlDarkDark);
            PropertyBackgroundBrush = new SolidBrush(SystemColors.Window);
            PropertyBackgroundHighlightBrush = new SolidBrush(SystemColors.Highlight);
            PropertyTextBrush = new SolidBrush(SystemColors.ControlText);
            PropertyReadOnlyTextBrush = new SolidBrush(SystemColors.GrayText);
            PropertyTextHighlightBrush = new SolidBrush(SystemColors.HighlightText);
            PropertyExpanderPen = new Pen(SystemColors.ControlDarkDark);
            PropertyLinePen = new Pen(SystemColors.Control);

           
            
            EditingContext = null;
            Font = new Font("Segoe UI", 9.0f);
            ShowResetButton = true;
            ShowCopyButton = true;
            Controls.AddRange(new Control[]{                
                m_editingControl,
                m_scrollBar,                
            });
            ResumeLayout();            
            m_resetButton.Click += (sender, e) =>
                {                    
                    SelectedProperty.Context.ResetValue();                    
                    Invalidate();
                };
        }

        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CategoryBackgroundBrush.Dispose();
                CategoryNameBrush.Dispose();
                CategoryLinePen.Dispose();
                CategoryExpanderPen.Dispose();
                PropertyBackgroundBrush.Dispose();
                PropertyBackgroundHighlightBrush.Dispose();
                PropertyTextBrush.Dispose();
                PropertyReadOnlyTextBrush.Dispose();
                PropertyTextHighlightBrush.Dispose();
                PropertyExpanderPen.Dispose();
                PropertyLinePen.Dispose();
                m_resetButton.Dispose();          
                m_overlayButtonToolTip.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets or sets whether to show reset button for selected property</summary>
        public bool ShowResetButton
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to show copy button for selected property</summary>
        public bool ShowCopyButton
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets whether to show the vertical scrollbar</summary>
        public bool ShowScrollbar
        {
            get { return m_showScrollbar; }
            set { m_showScrollbar = value; }
        }

        /// <summary>
        /// Gets whether or not category headers are drawn</summary>
        /// <remarks>Use the <see cref="PropertySorting"/> property to set this indirectly.</remarks>
        public bool ShowCategories
        {
            get
            {
                return (PropertySorting
                    & (PropertySorting.Categorized | PropertySorting.CategoryAlphabetical)) != 0;
            }
        }

        /// <summary>
        /// Gets or sets whether to automatically resize columns to best fit their contents</summary>
        public bool AutoSizeColumns
        {
            get { return m_autoSizeColumns; }
            set { m_autoSizeColumns = value; }
        }

        /// <summary>
        /// Reads persisted control setting information from the given XML element</summary>
        /// <param name="root">Root element of XML settings</param>
        protected override void ReadSettings(XmlElement root)
        {
            if (!AutoSizeColumns)
            {
                string s = root.GetAttribute("SplitterAmount");
                float splitterAmount;
                if (float.TryParse(s, out splitterAmount))
                    m_splitterAmount = splitterAmount;
            }
        }

        /// <summary>
        /// Writes persisted control setting information to the given XML element</summary>
        /// <param name="root">Root element of XML settings</param>
        protected override void WriteSettings(XmlElement root)
        {
            if (!AutoSizeColumns)
                root.SetAttribute("SplitterAmount", m_splitterAmount.ToString());
        }

        /// <summary>
        /// Gets the PropertyDescriptor for the property under the client point, or null if none</summary>
        /// <param name="clientPoint">Client point</param>
        /// <returns>PropertyDescriptor for the property under the client point, or null if none</returns>
        public PropertyDescriptor GetDescriptorAt(Point clientPoint)
        {
            int bottom;
            return GetDescriptorAt(clientPoint, out bottom);
        }

        /// <summary>
        /// Gets the PropertyDescriptor for the property under the client point, or null if none</summary>
        /// <param name="clientPoint">Client point</param>
        /// <param name="editingContext">The editing context to be used with the resulting property descriptor</param>
        /// <returns>PropertyDescriptor for the property under the client point, or null if none</returns>
        public PropertyDescriptor GetDescriptorAt(Point clientPoint, out IPropertyEditingContext editingContext)
        {
            int bottom;

            object picked = Pick(clientPoint, out bottom, out editingContext);
            Property property = picked as Property;
            if (property != null)
                return property.Descriptor;

            return null;
        }

        /// <summary>
        /// Gets the PropertyDescriptor for the property under the client point, or null if none</summary>
        /// <param name="clientPoint">Client point</param>
        /// <param name="bottom">Will be set to the Y position, in client coordinates, of the bottom
        /// of the row for this property. Is only meaningful if a PropertyDescriptor is found.</param>
        /// <returns>PropertyDescriptor for the property under the client point, or null if none</returns>
        public PropertyDescriptor GetDescriptorAt(Point clientPoint, out int bottom)
        {
            IPropertyEditingContext editingContext;
            object picked = Pick(clientPoint, out bottom, out editingContext);
            Property property = picked as Property;
            if (property != null)
                return property.Descriptor;

            return null;
        }

        /// <summary>
        /// Gets or sets whether a tooltip is displayed over property names with property descriptions</summary>
        public bool AllowTooltips
        {
            get { return m_allowTooltips; }
            set { m_allowTooltips = value; }
        }

        /// <summary>
        /// Gets and sets whether the user can navigate into parts of composite properties</summary>
        public bool AllowEditingComposites
        {
            get { return m_allowEditingComposites; }
            set
            {
                m_allowEditingComposites = value;
                if (!m_allowEditingComposites)
                    m_history.Clear();
            }
        }

        /// <summary>
        /// Gets whether the view can navigate back to the previous binding</summary>
        public bool CanNavigateBack
        {
            get { return m_history.Count > 0; }
        }

        /// <summary>
        /// Navigates back to the previous binding</summary>
        public void NavigateBack()
        {
            if (m_history.Count == 0)
                throw new InvalidOperationException("No history");

            IPropertyEditingContext context = m_history.Pop();

            try
            {
                m_openingComposite = true;
                EditingContext = context;
            }
            finally
            {
                m_openingComposite = false;
            }

        }

        /// <summary>
        /// Gets the client window x-coordinate of the vertical separator between the names and values columns</summary>
        /// <returns>Client window x-coordinate of the vertical separator</returns>
        public int GetMiddleX()
        {
            int width = Width;
            if (m_scrollBar.Visible)
                width -= m_scrollBar.Width;

            int middle = (int)(m_splitterAmount * width);
            return middle;
        }

        /// <summary>
        /// Sets whether the control can drag and drop</summary>
        public override bool AllowDrop
        {
            set
            {
                base.AllowDrop = value;
                m_editingControl.AllowDrop = value;
            }
        }

        /// <summary>
        /// Processes a dialog key</summary>
        /// <param name="keyData">One of the System.Windows.Forms.Keys values that represents the key to process</param>
        /// <returns>True iff the key was processed by the control</returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            // Handle tab keys, too, because only properties with their own Controls have the TabIndex.
            if (keyData == Keys.Down || keyData == Keys.Tab)
            {
                Property property = GetNextProperty(SelectedProperty);

                // current wrap around approach,
                // can't handle embedded PropertyGridView.
                // disabled for now: Alan                
                //if (property == null)
                //    property = GetFirstProperty();
                
                if (property != null)
                {
                    StartPropertyEdit(property);
                    TryMakeSelectionVisible();
                    return true;
                }
                
            }

            if (keyData == Keys.Up || keyData == (Keys.Tab | Keys.Shift))
            {
                Property property = GetPreviousProperty(SelectedProperty);

                // current wrap around approach,
                // can't handle embedded PropertyGridView.
                // disabled for now: Alan
                // check if we should wrap around
                //if (property == null)
               //     property = GetLastProperty();
                if (property != null)
                {
                    StartPropertyEdit(property, true);
                    TryMakeSelectionVisible();
                    return true;
                }
                
            }
            return base.ProcessDialogKey(keyData);            
        }

        /// <summary>
        /// Raises the SelectedPropertyChanged event and performs custom actions</summary>
        /// <param name="e">EventArgs that contains the event data</param>
        protected override void OnSelectedPropertyChanged(EventArgs e)
        {
            base.OnSelectedPropertyChanged(e);

            // This logic complements SetEditingControlTop.
            if (SelectedProperty == null)
                m_editingControl.Hide();
        }

        /// <summary>
        /// Performs custom actions and raises the <see cref="E:System.Windows.Forms.Control.MouseDown"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            m_mouseDown = new Point(e.X, e.Y);

            bool handled = m_resetButton.MouseDown(e);            
            if (handled)
            {       
                Invalidate();
                return;
            }

            // pick only if not over splitter
            int middle = GetMiddleX();


            if (Math.Abs(e.X - middle) > SystemDragSize.Width)
            {
                int bottom;
                IPropertyEditingContext editingContext;
                object picked = Pick(e.Location, out bottom, out editingContext);
                if (picked is Category)
                {
                    Category category = picked as Category;
                    category.Expanded = !category.Expanded; // toggle

                    // this will cause any currently active child controls to flush
                    Select();
                    m_editingControl.Hide();

                    Refresh();
                }
                else if (picked is Property)
                {
                    Property property = picked as Property;

                    if (e.X < middle && property.ChildProperties != null && property.ChildProperties.Count > 0)
                    {
                        // toggle expansion
                        property.ChildrenExpanded = !property.ChildrenExpanded;

                        // this will cause any currently active child controls to flush
                        Select();
                        m_editingControl.Hide();
                        Refresh();
                    }
                    else
                    {
                        if (!property.Descriptor.IsReadOnly)
                        {
                            // this will cause any currently active child controls to flush
                            Select();
                            m_editingControl.Hide();

                            StartPropertyEdit(property);
                        }
                    }
                }
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"></see> event and performs custom actions</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!m_dragging)
            {
                m_toolTip.RemoveAll();
                bool handled = m_resetButton.MouseMove(e);                                              
                if (handled)
                {                            
                    Cursor = Cursors.Arrow;                    
                    return;
                }                 
            }

            base.OnMouseMove(e);

            int middle = GetMiddleX();

            if (!m_dragging && (e.Button == MouseButtons.Left))
            {
                m_dragging = Math.Abs(m_mouseDown.X - middle) <= SystemDragSize.Width;
            }

            // manage tool tip
            int bottom;
            IPropertyEditingContext editingContext;
            Property property = null;
            if (e.X < middle)
                property = Pick(e.Location, out bottom, out editingContext) as Property;

            if (property != m_hoverProperty)
            {
                m_hoverProperty = property;
                if (m_allowTooltips)
                {
                    m_toolTip.RemoveAll();
                    if (m_hoverProperty != null && !m_dragging)
                    {
                        m_toolTip.Show(property.Descriptor.Description, this, e.X, e.Y + 20);
                    }
                }
            }

            // don't split or give split feedback if control is empty
            if (EditingContext != null)
            {
                if (!m_dragging)
                {
                    Cursor cursor = Cursors.Arrow;
                    if (Math.Abs(e.X - middle) < SystemDragSize.Width)
                        cursor = Cursors.VSplit;

                    if (cursor != Cursor)
                        Cursor = cursor;
                }
                else
                {
                    int width = Width;
                    if (m_scrollBar.Visible)
                        width -= m_scrollBar.Width;

                    float split = (float)e.X / (float)width;
                    split = Math.Max(0.125f, split);
                    split = Math.Min(0.875f, split);

                    if (m_splitterAmount != split)
                    {
                        m_splitterAmount = split;

                        Invalidate();
                    }
                }
            }            
        }

        /// <summary>
        /// Performs custom actions and raises the <see cref="E:System.Windows.Forms.Control.MouseUp"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            m_resetButton.MouseUp(e);            
            m_dragging = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        /// <summary>
        /// Performs custom actions and raises the <see cref="E:System.Windows.Forms.Control.MouseLeave"></see> event</summary>
        /// <param name="e">A <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            Cursor = Cursors.Arrow;

            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Performs custom actions and raises the MouseWheel event</summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int value = m_scrollBar.Value - e.Delta / 2;
            SetVerticalScroll(value);

            base.OnMouseWheel(e);
        }

        /// <summary>
        /// Performs custom actions and raises the <see cref="E:System.Windows.Forms.Control.Layout"></see> event</summary>
        /// <param name="levent">A <see cref="T:System.Windows.Forms.LayoutEventArgs"></see> that contains the event data</param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            // Trigger a paint event so that the two columns are resized in response to a reduction
            //  in the width of this PropertyGridView, for example.
            Invalidate();

            base.OnLayout(levent);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event and performs custom actions</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            m_resetButton.Visible = false;           
            SuspendLayout();

            Graphics g = e.Graphics;                   
            UpdateScrollbar();
            m_scrollBar.Refresh();

            int y = -m_scroll;
            int width = Width;
            if (m_scrollBar.Visible)
            {
                width -= m_scrollBar.Width;
            }

            int middle = GetMiddleX();
            int childLeft = middle + 1;
            int childWidth = width - childLeft;
            int tabIndex = 0; //for when user presses Enter in a property editing control, so Windows sets Focus on next property
            const int SubCategoryIndent = 13;
           
            foreach (object obj in Items)
            {
                Category c = obj as Category;
                if (c != null)
                {
                    if (c.Visible)
                    {
                        int indent = (c.Parent == null) ? 0 : SubCategoryIndent;
                        DrawCategoryRow(c, c.Expanded, indent, y, width - indent, g);
                        y += RowHeight;
                    }
                }
                else
                {
                    Property property = (Property)obj;                    
                    if (property.Control != null)
                    {
                        bool visible = property.Visible;
                        if (visible)
                        {
                            property.Control.Top = y;
                            property.Control.Left = childLeft;
                            property.Control.Width = childWidth;
                            property.Control.TabIndex = tabIndex++;
                        }

                        property.Control.Visible = visible;
                    }

                    if (property.Visible)
                    {
                        int indent = 0;
                        if (property.Category != null &&
                            property.Category.Parent != null)
                            indent = SubCategoryIndent;
                        DrawPropertyRow(property, indent, y, width - indent, middle, g);
                        y += GetRowHeight(property);
                    }
                }
            }
                     
            SetEditingControlTop();
            m_editingControl.Left = childLeft;
            m_editingControl.Width = childWidth;

            m_editingControl.TabIndex = tabIndex++;
            m_editingControl.Font = Font;
            // This isn't quite right. How do we know when a property editing has finished due to pressing the Enter key?
            // Let's get this figured out and then we can get rid of setting the TabIndex above, because the TabIndex
            //  only works if there is a custom property editing control.
            //m_editingControl.VisibleChanged += (sender, args) =>
            //    {
            //        if (!m_editingControl.Visible)
            //        {
            //            Property property = GetNextProperty(SelectedProperty);
            //            if (property != null)
            //                StartPropertyEdit(property);
            //        }
            //    };

            ResumeLayout();            
        }

        /// <summary>
        /// Forces the control to invalidate its client area and immediately redraw itself and any child controls</summary>
        /// <PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public override void Refresh()
        {
            FlushEditingControl();

            base.Refresh();
        }

        /// <summary>
        /// Performs custom actions for the EditingContextChanging event</summary>
        protected override void OnEditingContextChanging()
        {
            if (!m_openingComposite)
            {
                m_history.Clear();
            }

            FlushEditingControl();
            m_editingControl.Hide();
        }

        /// <summary>
        /// Performs custom actions for the EditingContextChanged event</summary>
        protected override void OnEditingContextChanged()
        {
            foreach (Property p in Properties)
            {
                Control control = p.Control;
                if (control != null)
                {
                    control.SizeChanged -= control_SizeChanged;
                    control.SizeChanged += control_SizeChanged;
                    control.Enter -= control_Enter;
                    control.Enter += control_Enter;
                    control.MouseUp -= control_MouseUp;
                    control.MouseUp += control_MouseUp;

                    //ICompositePropertyControl compositeControl = control as ICompositePropertyControl;
                    //if (compositeControl != null)
                    //{
                    //    compositeControl.CompositeOpened -= compositeControl_CompositeOpened;
                    //    compositeControl.CompositeOpened += compositeControl_CompositeOpened;
                    //}
                }
            }

            if (AutoSizeColumns)
                AutoAdjustSplitter();

            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.SizeChanged"></see> event and performs custom actions</summary>
        /// <param name="e">EventArgs that contains the event data</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (AutoSizeColumns)
                AutoAdjustSplitter();
        }

        /// <summary>
        /// Raises the System.Windows.Forms.Control.VisibleChanged event and performs custom actions</summary>
        /// <param name="e">An System.EventArgs that contains the event data</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (AutoSizeColumns)
                AutoAdjustSplitter();
        }

        private void AutoAdjustSplitter()
        {
            if (!Properties.Any())
                return;

            int maxPropNameWidth = ExpanderSize * 2;
            foreach (Property property in Properties)
            {
                int depth = GetDepth(property) + (ShowCategories ? 1 : 0);
                int expanderX = ExpanderSize * 2 * (depth + 1);
                int width = expanderX + TextRenderer.MeasureText(property.Descriptor.DisplayName, Font).Width;
                if (width > maxPropNameWidth)
                    maxPropNameWidth = width;
            }

            // Limit the property name column to 65% of the total width
            m_splitterAmount = Math.Min(0.65f, (float)maxPropNameWidth / (float)Width);
        }

        private void scrollBar_ValueChanged(object sender, EventArgs e)
        {
            m_scroll = m_scrollBar.Value;

            Invalidate();
        }

        // if an embedded control's size changes, reposition all controls
        private void control_SizeChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void control_Enter(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            foreach (Property p in Properties)
            {
                if (p.Control == control && p != SelectedProperty)
                {
                    StartPropertyEdit(p);
                    break;
                }
            }
        }

        private void control_MouseUp(object sender, MouseEventArgs e)
        {
            // Let listeners see right-click events so that they can show context menus.
            Point screenPnt = ((Control)sender).PointToScreen(e.Location);
            Point myClientPnt = PointToClient(screenPnt);
            var newE = new MouseEventArgs(e.Button, e.Clicks, myClientPnt.X, myClientPnt.Y, e.Delta);
            OnMouseUp(newE);
        }

        private void editingControl_DragOver(object sender, DragEventArgs e)
        {
            // raise child event on this control
            OnDragOver(e);
        }

        private void editingControl_DragDrop(object sender, DragEventArgs e)
        {
            // raise child event on this control
            OnDragDrop(e);
        }

        private void editingControl_MouseHover(object sender, EventArgs e)
        {
            // raise event on this control
            OnMouseHover(e);
        }

        private void editingControl_MouseLeave(object sender, EventArgs e)
        {
            // raise event on this control
            OnMouseLeave(e);
        }

        //private void compositeControl_CompositeOpened(object sender, CompositeOpenedEventArgs e)
        //{
        //    //if (m_allowEditingComposites)
        //    //{
        //    //    m_history.Push(EditingContext);
        //    //    object[] newSelection = new object[] { e.Part };
        //    //    PropertyEditingContext newContext = new PropertyEditingContext(newSelection, EditingContext);
        //    //    try
        //    //    {
        //    //        m_openingComposite = true;
        //    //        Bind(newContext);
        //    //    }
        //    //    finally
        //    //    {
        //    //        m_openingComposite = false;
        //    //    }
        //    //}
        //}

        private void StartPropertyEdit(Property property, bool fromEnd = false)
        {
            Select(); // force any edit to conclude

            SelectedProperty = property;

            if (property.Control != null) // embedded control?
            {
                property.Control.Select();

                
                // as PropertyView derives from Control, not ContainerControl, 
                // need to manually set input focus here
                if (property.Control.Controls.Count > 0)
                {
                    if (fromEnd)
                    {
                        for (int i = property.Control.Controls.Count - 1;
                            i >= 0; i--)
                        {
                            Control control = property.Control.Controls[i];
                            if (control.CanFocus && !(control is ToolStrip))
                            {
                                control.Focus();
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (Control control in property.Control.Controls)
                        {
                            if (control.CanFocus && !(control is ToolStrip))
                            {
                                control.Focus();
                                break;
                            }
                        }
                    }

                    
                }
                else if (property.Control.CanFocus)
                    property.Control.Focus();

                m_editingControl.Hide();
            }
            else // property editing control
            {
                m_editingControl.Bind(property.Context);
                SetEditingControlTop();
                m_editingControl.Show();
                m_editingControl.Focus();
            }

            Invalidate();
        }

        private void FlushEditingControl()
        {
            if (m_editingControl.Visible)
                m_editingControl.Flush();
        }

        /// <summary>
        /// Refreshes all editing controls, invalidating them and immediately drawing them</summary>
        protected override void RefreshEditingControls()
        {
            m_editingControl.Refresh();

            base.RefreshEditingControls();
        }

        private void SetEditingControlTop()
        {
            if (SelectedProperty != null)
            {
                int y = -m_scroll + GetRowY(SelectedProperty);
                m_editingControl.Top = y + 1;
            }
        }

        // Returns the object (Category or Property) at this client window point
        private object Pick(Point clientPnt, out int bottom, out IPropertyEditingContext editingContext)
        {
            editingContext = EditingContext;
            int top = bottom = -m_scroll;
            int middleX = GetMiddleX();//name & value divider's X coordinate, in client coordinates
            foreach (object obj in VisibleItems)
            {
                if (obj is Category)
                {
                    top += RowHeight;
                    if (clientPnt.Y < top)
                        return obj;
                }
                else
                {
                    Property property = (Property)obj;
                    top += GetRowHeight(property);
                    if (clientPnt.Y < top)
                    {
                        bottom = top;

                        // Check for embedded PropertyGridView on the "values" side
                        if (property.Control != null &&
                            clientPnt.X > middleX)
                        {
                            foreach (PropertyGridView childPropertyGridView in FindChildControls<PropertyGridView>(property.Control))
                            {
                                if (childPropertyGridView != null)
                                {
                                    Point screenPnt = PointToScreen(clientPnt);
                                    Point childClientPnt = childPropertyGridView.PointToClient(screenPnt);
                                    int childBottomY;
                                    IPropertyEditingContext childEditingContext;
                                    object result = childPropertyGridView.Pick(childClientPnt, out childBottomY, out childEditingContext);
                                    if (result != null)
                                    {
                                        Point childClientBottomPnt = new Point(0, childBottomY);
                                        Point screenBottomPnt = childPropertyGridView.PointToScreen(childClientBottomPnt);
                                        Point ourClientBottomPnt = PointToClient(screenBottomPnt);
                                        bottom = ourClientBottomPnt.Y;
                                        editingContext = childEditingContext;
                                        return result;
                                    }
                                }
                            }
                        }

                        return obj;
                    }
                }
            }
            return null;
        }

        private IEnumerable<C> FindChildControls<C>(Control control) where C : Control
        {
            foreach (Control child in control.Controls)
            {
                if (child is C)
                    yield return (C)child;
                foreach (C result in FindChildControls<C>(child))
                    if (result != null)
                        yield return result;
            }
        }

        // Gets the top coordinate (0 based) of the property. If property is null or can't be
        //  found, returns the bottom of the last row.
        private int GetRowY(Property property)
        {
            object lastItem = null;
            int lastTop = 0;
            foreach (Pair<object, int> itemPosition in ItemPositions)
            {
                lastItem = itemPosition.First;
                lastTop = itemPosition.Second;
                Property p = itemPosition.First as Property;
                if (p != null && p == property)
                    return lastTop;
            }
            return lastTop + GetRowHeight(lastItem);
        }

        // Gets the client window position (top is zero) of every visible category or property
        private IEnumerable<Pair<object, int>> ItemPositions
        {
            get
            {
                int top = 0;
                foreach (object obj in VisibleItems)
                {
                    if (obj is Category)
                    {
                        yield return new Pair<object, int>(obj, top);
                        top += RowHeight;
                    }
                    else
                    {
                        Property property = (Property)obj;
                        yield return new Pair<object, int>(obj, top);
                        top += GetRowHeight(property);
                    }
                }
            }
        }


        /// <summary>
        /// Calculates the total height of a category or property</summary>
        /// <param name="item">Category or property whose row height is calculated</param>
        /// <returns>Total height</returns>
        protected int GetRowHeight(object item)
        {
            if (item == null)
                return 0;
            if (item is Category)
                return RowHeight;
            return GetRowHeight((Property)item);
        }

        /// <summary>
        /// Calculates the total height of a property</summary>
        /// <param name="property">Property whose row height is calculated</param>
        /// <returns>Total height</returns>
        protected int GetRowHeight(Property property)
        {
            Control control;
            if (property.Control != null)
                control = property.Control;
            else
                control = m_editingControl;

            return Math.Max(RowHeight, control.Height + 2);
        }

        /// <summary>
        /// Calculates the depth of a property in its parent tree</summary>
        /// <param name="property">Property whose depth is calculated</param>
        /// <returns>Depth of a property in its parent tree</returns>
        protected int GetDepth(Property property)
        {
            int depth = 0;
            while (property.Parent != null)
            {
                ++depth;
                property = property.Parent;
            }
            return depth;
        }

        /// <summary>
        /// Gets the minimum height the control needs to show all properties
        /// without needing a vertical scroll bar</summary>
        /// <returns>Minimum height required to show all properties</returns>
        public int GetPreferredHeight()
        {
            int height = 0;
            foreach (Property property in Properties)
            {
                if (ShowCategories && property.FirstInCategory)
                    height += GdiUtil.ExpanderSize + Margin.Top + Margin.Bottom;
                if ((property.Category == null || property.Category.Expanded)
                    && (property.Parent == null || property.Parent.ChildrenExpanded))
                    height += GetRowHeight(property);
            }
            return height;
        }

        /// <summary>
        /// Tries to make the selection visible in the scrolling area</summary>
        private void TryMakeSelectionVisible()
        {
            if (SelectedProperty != null)
            {
                foreach (Pair<object, int> itemPosition in ItemPositions)
                {
                    Property p = itemPosition.First as Property;
                    if (p != null && p == SelectedProperty)
                    {
                        int currPos = itemPosition.Second;
                        if (currPos < m_scroll)
                            SetVerticalScroll(currPos);
                        else if (currPos + RowHeight > m_scroll + Height - RowHeight * 2)
                            SetVerticalScroll(currPos + RowHeight - (Height - RowHeight * 2));
                        break;
                    }
                }
            }
        }

        private void SetVerticalScroll(int scroll)
        {
            m_scrollBar.Value = Math.Max(m_scrollBar.Minimum, Math.Min(m_scrollBar.Maximum, scroll));
        }

        // Updates the position, size, and scroll speed of the scrollbar. Does not directly draw the scrollbar.
        private void UpdateScrollbar()
        {
            int bottom = GetRowY(null);

            WinFormsUtil.UpdateScrollbars(
                m_scrollBar,
                null,
                new Size(0, Height),
                new Size(0, bottom));

            m_scrollBar.SmallChange = RowHeight;
            m_editingControl.Height = RowHeight;
        }

        /// <summary>
        /// Selects the given property by scrolling it into view if necessary, and setting focus
        /// on the editing Control.</summary>
        /// <param name="descriptor">The property descriptor for the desired property</param>
        /// <returns>True if the property was found and false otherwise</returns>
        public bool SelectProperty(PropertyDescriptor descriptor)
        {
            Refresh();

            foreach (Pair<object, int> itemPosition in ItemPositions)
            {
                Property p = itemPosition.First as Property;
                if (p != null && p.Descriptor.Equals(descriptor))
                {
                    SetVerticalScroll(itemPosition.Second - 2 * RowHeight);
                    StartPropertyEdit(p);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets or sets the category background brush</summary>
        public Brush CategoryBackgroundBrush
        {
            get { return m_categoryBackgroundBrush; }
            set
            {
                if (m_categoryBackgroundBrush != null)
                    m_categoryBackgroundBrush.Dispose();
                m_categoryBackgroundBrush = value;
            }
        }

        /// <summary>
        /// Gets or sets the category name brush</summary>
        public Brush CategoryNameBrush
        {
            get { return m_categoryNameBrush; }
            set
            {
                if (m_categoryNameBrush != null)
                    m_categoryNameBrush.Dispose();
                m_categoryNameBrush = value;
            }
        }

        /// <summary>
        /// Gets or sets the category line pen</summary>
        public Pen CategoryLinePen
        {
            get { return m_categoryLinePen; }
            set
            {
                if (m_categoryLinePen != null)
                    m_categoryLinePen.Dispose();
                m_categoryLinePen = value;
            }
        }

        /// <summary>
        /// Gets or sets the category expander pen</summary>
        public Pen CategoryExpanderPen
        {
            get { return m_categoryExpanderPen; }
            set
            {
                if (m_categoryExpanderPen != null)
                    m_categoryExpanderPen.Dispose();
                m_categoryExpanderPen = value;
            }
        }

        /// <summary>
        /// Gets or sets the property background brush</summary>
        public Brush PropertyBackgroundBrush
        {
            get { return m_propertyBackgroundBrush; }
            set
            {
                if (m_propertyBackgroundBrush != null)
                    m_propertyBackgroundBrush.Dispose();
                m_propertyBackgroundBrush = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected property background brush</summary>
        public Brush PropertyBackgroundHighlightBrush
        {
            get { return m_propertyBackgroundHighlightBrush; }
            set
            {
                if (m_propertyBackgroundHighlightBrush != null)
                    m_propertyBackgroundHighlightBrush.Dispose();
                m_propertyBackgroundHighlightBrush = value;
            }
        }

        /// <summary>
        /// Gets or sets the property text brush</summary>
        public Brush PropertyTextBrush
        {
            get { return m_propertyTextBrush; }
            set
            {
                if (m_propertyTextBrush != null)
                    m_propertyTextBrush.Dispose();
                m_propertyTextBrush = value;
            }
        }

        /// <summary>
        /// Gets or sets the property read only text brush</summary>
        public Brush PropertyReadOnlyTextBrush
        {
            get { return m_propertyReadOnlyTextBrush; }
            set
            {
                if (m_propertyReadOnlyTextBrush != null)
                    m_propertyReadOnlyTextBrush.Dispose();
                m_propertyReadOnlyTextBrush = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected property text brush</summary>
        public Brush PropertyTextHighlightBrush
        {
            get { return m_propertyTextHighlightBrush; }
            set
            {
                if (m_propertyTextHighlightBrush != null)
                    m_propertyTextHighlightBrush.Dispose();
                m_propertyTextHighlightBrush = value;
            }
        }

        /// <summary>
        /// Gets or sets the property expander pen</summary>
        public Pen PropertyExpanderPen
        {
            get { return m_propertyExpanderPen; }
            set
            {
                if (m_propertyExpanderPen != null)
                    m_propertyExpanderPen.Dispose();
                m_propertyExpanderPen = value;
            }
        }

        /// <summary>
        /// Gets or sets the property line pen</summary>
        public Pen PropertyLinePen
        {
            get { return m_propertyLinePen; }
            set
            {
                if (m_propertyLinePen != null)
                    m_propertyLinePen.Dispose();
                m_propertyLinePen = value;
            }
        }

        private Brush m_categoryBackgroundBrush;
        private Brush m_categoryNameBrush;
        private Pen m_categoryLinePen;
        private Pen m_categoryExpanderPen;
        private Brush m_propertyBackgroundBrush;
        private Brush m_propertyBackgroundHighlightBrush;
        private Brush m_propertyTextBrush;
        private Brush m_propertyReadOnlyTextBrush;
        private Brush m_propertyTextHighlightBrush;
        private Pen m_propertyExpanderPen;
        private Pen m_propertyLinePen;

        #region Drawing Methods

        /// <summary>
        /// Draws a category row</summary>
        /// <param name="category">Category whose row is drawn</param>
        /// <param name="expanded">Whether category expanded or not</param>
        /// <param name="x">X-coordinate of upper top corner of rectangle to draw</param>
        /// <param name="y">Y-coordinate of upper top corner of rectangle to draw</param>
        /// <param name="width">Row width to draw</param>
        /// <param name="g">Graphics object</param>
        protected virtual void DrawCategoryRow(Category category, bool expanded, int x, int y, int width, Graphics g)
        {
            int xPadding = Margin.Left;
            int yPadding = (int)((RowHeight - FontHeight) / 2);

            g.FillRectangle(CategoryBackgroundBrush, x, y, width, RowHeight - 1);

            GdiUtil.DrawExpander(x + xPadding, y + (RowHeight - ExpanderSize) / 2, expanded, g, CategoryExpanderPen);

            int offset = ExpanderSize + 2 * xPadding;
            RectangleF nameRect = new RectangleF(
                x + offset,
                y + yPadding,
                width - offset,
                RowHeight);
            g.DrawString(category.Name, BoldFont, CategoryNameBrush, nameRect, LeftStringFormat);

            g.DrawLine(CategoryLinePen, x, y + RowHeight, x + width, y + RowHeight);
        }

        /// <summary>
        /// Draws a property row</summary>
        /// <param name="property">Property whose row is drawn</param>
        /// <param name="x">X-coordinate of upper top corner of rectangle to draw</param>
        /// <param name="y">Y-coordinate of upper top corner of rectangle to draw</param>
        /// <param name="width">Row width to draw</param>
        /// <param name="middle">X-coordinate of vertical separator between the names and values columns</param>
        /// <param name="g">Graphics object</param>
        protected virtual void DrawPropertyRow(Property property, int x, int y, int width, int middle, Graphics g)
        {
            
            int height = GetRowHeight(property);
            g.FillRectangle(PropertyBackgroundBrush, x, y, width, height - 1);

            Brush nameBrush = PropertyTextBrush;
            if (property == SelectedProperty)
            {                
                g.FillRectangle(PropertyBackgroundHighlightBrush, x, y, middle - x, height);
                nameBrush = PropertyTextHighlightBrush;
            }

           
            int xPadding = Margin.Left;
            int yPadding = (int)((RowHeight - FontHeight) / 2);
            int depth = GetDepth(property) + (ShowCategories ? 1 : 0);
            int expanderX = ExpanderSize * 2 * depth;
            if (property.ChildProperties != null && property.ChildProperties.Count > 0)
                GdiUtil.DrawExpander(x + expanderX, y + (RowHeight - ExpanderSize) / 2, property.ChildrenExpanded, g, PropertyExpanderPen);
          
            Rectangle nameRect = new Rectangle(
                x + xPadding,
                y + yPadding,
                middle - 2 * xPadding,
                height - 1);
            g.DrawString(property.Descriptor.Name, Font, nameBrush, nameRect, LeftStringFormat);

            Rectangle valueRect = new Rectangle(middle + 1, y + 1, width - middle - 1, height - 1);

            if (property.Control == null)
            {
                bool isOverride = property.Descriptor.CanResetValue(LastSelectedObject);
                Font font = isOverride ? BoldFont : Font;
                bool isReadOnly = property.Descriptor.IsReadOnly;
                Brush brush = isReadOnly ? PropertyReadOnlyTextBrush : PropertyTextBrush;

                TypeDescriptorContext context = new TypeDescriptorContext(
                    LastSelectedObject, property.Descriptor, null);
                PropertyEditingControl.DrawProperty(
                    property.Descriptor, context, valueRect, font, brush, g);
                if (property == SelectedProperty)
                    m_editingControl.Refresh();
            }
            else
            {
                // draw custom control immediately, to keep in sync with standard DrawProperty above.
                property.Control.Refresh();                
            }

            g.DrawLine(PropertyLinePen, middle, y-1, middle, y + height-1);
            g.DrawLine(PropertyLinePen, x, y + height-1 , width, y + height-1);

            // show copy and reset buttons for the selected property.            
            if (SelectedProperty == property)
            {                
                // show reset button
                if (ShowResetButton && CanResetCurrent
                    && middle > m_resetButton.Width)
                {
                    m_resetButton.Visible = true;
                    m_resetButton.Top = (height - m_resetButton.Height) / 2 + y;
                    m_resetButton.Left = middle - m_resetButton.Width - 3;                  
                    m_resetButton.Draw(g);
                }                
            }
        }

        #endregion

        private readonly Stack<IPropertyEditingContext> m_history = new Stack<IPropertyEditingContext>();

        private float m_splitterAmount = 0.5f;
        private Property m_hoverProperty;
        private Point m_mouseDown;
        private readonly ToolTip m_toolTip;
        private readonly PropertyEditingControl m_editingControl;
        private readonly OverlayButton m_resetButton;        
        private readonly ToolTip m_overlayButtonToolTip;
        private readonly VScrollBar m_scrollBar;
        private int m_scroll;

        private bool m_dragging;
        private bool m_openingComposite;
        private bool m_allowTooltips;
        private bool m_allowEditingComposites;
        private bool m_showScrollbar = true;
        private bool m_autoSizeColumns = true;
    }
}
