////Sony Computer Entertainment Confidential

//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Drawing;
//using System.Drawing.Design;
//using System.Windows.Forms;
//using System.Windows.Forms.Design;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Xml;

//using System.Collections;
//using Sce.Atf;
//using Sce.Atf.Controls.PropertyEditing;

//namespace Sce.Atf.Controls.PropEdit
//{
//    /// <summary>
//    /// Base class for complex property editing controls, providing formats, fonts,
//    /// data binding, persistent settings, and category/property information.
//    /// Normally the PropertyView class should be used, but this is useful when 
//    /// displaying properties of multiple elements, for example a collection.
//    /// For an example of a class that implements this, 
//    /// please see Scea.Dom.Editors.Internal.EmbeddedGridCollectionView.
//    /// </summary>
//    public abstract class EmbeddedPropertyView : Control
//    {
//        #region Construction and Overrides
//        /// <summary>
//        /// Initializes a new instance of the <see cref="PropertyView"/> class.
//        /// </summary>
//        public EmbeddedPropertyView()
//        {
//            UpdateFonts();
//            UpdateRowHeight();

//            base.DoubleBuffered = true;
//            base.AllowDrop = true; // otherwise, embedded child controls can't accept drops
//        }

//        /// <summary>
//        /// Releases the unmanaged resources used by the System.Windows.Forms.Control
//        /// and its child controls and optionally releases the managed resources</summary>
//        /// <param name="disposing">true to release both managed and unmanaged resources; false
//        /// to release only unmanaged resources</param>
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                if (BoldFont != null)
//                    BoldFont.Dispose();
//            }

//            base.Dispose(disposing);
//        }

//        /// <summary>
//        /// Forces the control to invalidate its client area and immediately redraw itself and any child controls.
//        /// </summary>
//        /// <PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
//        public override void Refresh()
//        {
//            // refresh all the editing controls
//            if (Rows == null)
//                return;

//            foreach (Row row in Rows)
//            {
//                foreach (Property p in row.Properties)
//                {
//                    p.Context.Refresh();

//                    Control control = p.Control;
//                    if (control != null)
//                    {
//                        SetFont(row, control, p.Descriptor);
//                        control.Refresh();
//                    }
//                }
//            }

//            base.Refresh();
//        }
//        #endregion

//        #region Fonts
//        /// <summary>
//        /// Bold version of the control's font.
//        /// </summary>
//        protected Font BoldFont;

//        /// <summary>
//        /// Raises the <see cref="E:System.Windows.Forms.Control.FontChanged"></see> event.
//        /// </summary>
//        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
//        /// <remarks>When overriding, call the base method to create fonts and force a re-layout</remarks>
//        protected override void OnFontChanged(EventArgs e)
//        {
//            UpdateFonts();
//            UpdateRowHeight();
//            PerformLayout();
//            Invalidate();

//            base.OnFontChanged(e);
//        }

//        private void UpdateFonts()
//        {
//            if (BoldFont != null)
//                BoldFont.Dispose();

//            BoldFont = new Font(base.Font, FontStyle.Bold);
//        }

//        /// <summary>
//        /// Raises the <see cref="E:System.Windows.Forms.Control.MarginChanged"></see> event.
//        /// </summary>
//        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
//        /// <remarks>When overriding, call the base method to create fonts and force a re-layout</remarks>
//        protected override void OnMarginChanged(EventArgs e)
//        {
//            UpdateRowHeight();
//            PerformLayout();

//            base.OnMarginChanged(e);
//        }

//        private void UpdateRowHeight()
//        {
//            RowHeight = (int)Math.Round(Sce.Atf.GdiUtil.DpiFactor * (base.FontHeight + Margin.Top));
//        }

//        /// <summary>
//        /// Cached Font height
//        /// </summary>
//        protected int RowHeight;

//        /// <summary>
//        /// Sets the font for a property editing control
//        /// </summary>
//        /// <param name="control">Property editing control</param>
//        /// <param name="descriptor">Property descriptor</param>
//        protected void SetFont(Row row, Control control, PropertyDescriptor descriptor)
//        {
//            //bool isOverride = row.EditingContext.CanResetValue(descriptor);
//            //control.Font = isOverride ? BoldFont : null;
//        }
//        #endregion

//        #region Data Binding
//        /// <summary>
//        /// Event that is raised after the binding changes</summary>
//        public event EventHandler BindingChanged;

//        /// <summary>
//        /// Binds this control to a set of contexts</summary>
//        /// <param name="context">Context in which properties are edited</param>
//        public void Bind(List<PropertyEditingContext> contexts)
//        {
//            // make sure this call is from the same thread that created this control,
//            //  which should be the GUI thread.
//            WinFormsUtil.CheckForIllegalCrossThreadCall(this);

//            SuspendLayout();

//            OnBindingChanging();

//            DestroyRows();

//            BuildRows(contexts);

//            OnBindingChanged();

//            ResumeLayout(true);

//            EventHandler handler = BindingChanged;
//            if (handler != null)
//                handler(this, EventArgs.Empty);
//        }

//        /// <summary>
//        /// Called before binding has changed
//        /// </summary>
//        protected virtual void OnBindingChanging()
//        {
//        }

//        /// <summary>
//        /// Called after binding has changed
//        /// </summary>
//        protected virtual void OnBindingChanged()
//        {
//        }

//        protected PropertyEditingContext SelectedEditingContext
//        {
//            get { return (SelectedRow == null) ? null : SelectedRow.EditingContext; }
//        }

//        protected List<Row> Rows;

//        protected Row SelectedRow;
//        #endregion

//        #region Categories, Properties, and PropertySorting
//        private void BuildRows(List<PropertyEditingContext> contexts)
//        {
//            if (contexts == null || contexts.Count <= 0)
//                return;

//            Rows = new List<Row>();
//            for (int i = 0; i < contexts.Count; i++)
//            {
//                Rows.Add(new Row());
//                Rows[i].EditingContext = contexts[i];
//            }

//            foreach(Row row in Rows)
//            {
//                PropertyDescriptor[] descriptors = PropertyEditingContext.GetPropertyDescriptors(row.EditingContext);
//                row.SetNumProperties(descriptors.Length);
//                for (int i = 0; i < descriptors.Length; i++)
//                {
//                    PropertyDescriptor descriptor = descriptors[i];

//                    Property property = new Property();
//                    property.Descriptor = descriptor;
//                    property.DescriptorIndex = i;
//                    property.Context = new PropertyEditorControlContext(
//                        row.EditingContext,
//                        descriptor);

//                    IPropertyEditor editor = descriptor.GetEditor(typeof(IPropertyEditor)) as IPropertyEditor;
//                    if (editor != null)
//                    {
//                        Control control = editor.GetEditingControl(property.Context);

//                        // force creation of the window handle on the GUI thread
//                        // see http://forums.msdn.microsoft.com/en-US/clr/thread/fa033425-0149-4b9a-9c8b-bcd2196d5471/
//                        IntPtr handle = control.Handle;

//                        control.Visible = false;
//                        control.BackColor = SystemColors.Window;
//                        control.Height = (int)Math.Round(Sce.Atf.GdiUtil.DpiFactor * control.Height);
//                        property.Control = control;

//                        SetFont(row, control, property.Descriptor);
//                        Controls.Add(control);
//                    }

//                    row.Properties[i] = property;
//                }
//            }            
//        }

//        private void DestroyRows()
//        {
//            if (Rows == null)
//                return;

//            foreach(Row row in Rows)
//            {
//                if (row.Properties != null)
//                {
//                    foreach (Property p in row.Properties)
//                    {
//                        Control control = p.Control;
//                        if (control != null)
//                        {
//                            Controls.Remove(control);
//                            control.Font = null;
//                            control.Dispose();
//                            p.Control = null;
//                        }
//                    }
//                    row.SetNumProperties(0);
//                }  
//            }
//            Rows = null;
//        }
//        #endregion        
        
//        #region Protected Classes
//        /// <summary>
//        /// Class to hold information associated with each property
//        /// </summary>
//        protected class Property
//        {
//            public PropertyDescriptor Descriptor;
//            public Category Category;
//            public EditingControlContext Context;
//            public Control Control;
//            public int DescriptorIndex;
//            public bool FirstInCategory;

//            public bool Visible
//            {
//                get { return true; }
//            }
//        }

//        /// <summary>
//        /// Class to hold information associated with each property category
//        /// </summary>
//        protected class Category
//        {
//            public Category(EmbeddedPropertyView owner)
//            {
//                m_owner = owner;
//            }
//            public string Name;
//            public bool Expanded
//            {
//                get { return m_owner.m_categoryExpanded[Name]; }
//                set { m_owner.m_categoryExpanded[Name] = value; }
//            }
//            public Property[] Properties;
//            private EmbeddedPropertyView m_owner;
//        }

//        protected class Row
//        {
//            public PropertyEditingContext EditingContext;
//            public Property SelectedProperty;
//            public Property[] Properties;

//            public void SetNumProperties(int size)
//            {
//                Properties = new Property[size];
//            }
            
//            /// <summary>
//            /// Gets the previous visible property in the current sort order
//            /// </summary>
//            /// <param name="property">Current property</param>
//            /// <returns>previous visible property in the current sort order</returns>
//            public Property GetPreviousProperty(Property property)
//            {
//                Property prev = null;
//                foreach (Property p in Properties)
//                {
//                    if (!p.Visible)
//                        continue;
//                    if (p == property)
//                        return prev;
//                    prev = p;
//                }

//                return null;
//            }

//            /// <summary>
//            /// Gets the next visible property in the current sort order
//            /// </summary>
//            /// <param name="property">Current property</param>
//            /// <returns>next visible property in the current sort order</returns>
//            public Property GetNextProperty(Property property)
//            {
//                Property prev = null;
//                foreach (Property p in Properties)
//                {
//                    if (!p.Visible)
//                        continue;
//                    if (property == null)
//                        return p;
//                    if (prev != null)
//                        return p;
//                    if (p == property)
//                        prev = p;
//                }

//                return null;
//            }
//        }
//        #endregion

//        static EmbeddedPropertyView()
//        {
//            LeftStringFormat = new StringFormat();
//            LeftStringFormat.Alignment = StringAlignment.Near;
//            LeftStringFormat.Trimming = StringTrimming.EllipsisCharacter;
//            LeftStringFormat.FormatFlags = StringFormatFlags.NoWrap;
//            RightStringFormat = new StringFormat();
//            RightStringFormat.Alignment = StringAlignment.Far;
//            RightStringFormat.Trimming = StringTrimming.EllipsisCharacter;
//            RightStringFormat.FormatFlags = StringFormatFlags.NoWrap;
//        }

//        /// <summary>
//        /// Left justified string format
//        /// </summary>
//        protected static StringFormat LeftStringFormat;

//        /// <summary>
//        /// Right justified string format
//        /// </summary>
//        protected static StringFormat RightStringFormat;

//        /// <summary>
//        /// Drag threshold size
//        /// </summary>
//        protected static Size SystemDragSize = SystemInformation.DragSize;

//        /// <summary>
//        /// Size of category expanders, in pixels
//        /// </summary>
//        protected const int ExpanderSize = Sce.Atf.GdiUtil.ExpanderSize;

//        protected const int ChildCounterWidth = 20;
//        private Dictionary<string, bool> m_categoryExpanded = new Dictionary<string, bool>();
//    }
//}