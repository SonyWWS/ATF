//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Collection editing control</summary>
    public class CollectionEditingControl : ListBox, ICompositePropertyControl, ICacheablePropertyControl
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="context">Context in which properties are edited</param>
        public CollectionEditingControl(PropertyEditorControlContext context)
        {
            m_context = context;

            AllowDrop = true; // allow drag and drop
            SelectionMode = SelectionMode.One; // restrict selection
            BorderStyle = BorderStyle.None;
            DoubleBuffered = true;

            RefreshList();
        }

        /// <summary>
        /// Gets the context for this property editing control</summary>
        public PropertyEditorControlContext Context
        {
            get { return m_context; }
        }

        /// <summary>
        /// Gets the index of the item at the client y coordinate</summary>
        /// <param name="y">Client y coordinate</param>
        /// <returns>Index of the item at the client y coordinate</returns>
        public int GetItemIndex(int y)
        {
            int index = (y / ItemHeight) + TopIndex;
            index = Math.Min(index, Items.Count - 1);
            return index;
        }

        /// <summary>
        /// Gets the index of insertion of a new item being dropped at y coordinate</summary>
        /// <param name="y">Client y coordinate</param>
        /// <returns>Target index for a new item being inserted, in the range [0,Items.Count]</returns>
        public int GetInsertionIndex(int y)
        {
            int index = ((y + (ItemHeight / 2)) / ItemHeight) + TopIndex;
            index = Math.Min(index, Items.Count);
            return index;
        }

        #region ICompositePropertyControl Members

        /// <summary>
        /// Event that is raised when part of the composite is opened by the user</summary>
        public event EventHandler<CompositeOpenedEventArgs> CompositeOpened;

        #endregion

        #region ICacheablePropertyControl

        /// <summary>
        /// Whether or not this Control is cacheable across selection changes. </summary>
        public virtual bool Cacheable
        {
            get { return true; }
        }

        #endregion

        /// <summary>
        /// Forces the control to invalidate its client area and immediately redraw itself and any child controls</summary>
        /// <PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public override void Refresh()
        {
            RefreshList();
            base.Refresh();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDoubleClick"></see> event after performing custom processing</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            object item = GetItemObject(e.Y);
            if (item != null)
            {
                PropertyDescriptor[] descriptors = GetDescriptors(item);
                OnPartOpened(new CompositeOpenedEventArgs(item, descriptors));
            }

            base.OnMouseDoubleClick(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"></see> event after performing custom processing</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            m_mouseDownLocation = e.Location;

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"></see> event after performing custom processing</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            Size dragBoxSize = SystemInformation.DragSize;

            if ((e.Button & MouseButtons.Left) != 0)
            {
                if ((dragBoxSize.Width < Math.Abs(m_mouseDownLocation.X - e.X)) ||
                    (dragBoxSize.Height < Math.Abs(m_mouseDownLocation.Y - e.Y)))
                {
                    object item = GetItemObject(m_mouseDownLocation.Y);
                    if (item != null)
                    {
                        item = ConvertDragDropItem(item);
                        if (item != null)
                        {
                            DoDragDrop(item, DragDropEffects.All | DragDropEffects.Link);
                        }
                    }
                }
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Gets string to represent the item in the ListBox</summary>
        /// <param name="item">The item</param>
        /// <returns>String to represent the item in the ListBox</returns>
        protected virtual string GetItemString(object item)
        {
            return GetItemText(item);
        }

        /// <summary>
        /// Gets the property descriptors for the given item in the collection</summary>
        /// <param name="item">The item in the collection</param>
        /// <returns>Property descriptor array for the given item in the collection</returns>
        protected virtual PropertyDescriptor[] GetDescriptors(object item)
        {
            return PropertyUtils.GetDefaultProperties2(item);
        }

        /// <summary>
        /// Converts the item for drag and drop</summary>
        /// <param name="item">Item to convert</param>
        /// <returns>Object for drag and drop</returns>
        protected virtual object ConvertDragDropItem(object item)
        {
            return item;
        }

        /// <summary>
        /// Raises the PartOpened event after performing custom processing</summary>
        /// <param name="e">The <see cref="Sce.Atf.Controls.PropertyEditing.CompositeOpenedEventArgs"/> instance containing the event data</param>
        protected virtual void OnPartOpened(CompositeOpenedEventArgs e)
        {
            EventHandler<CompositeOpenedEventArgs> handler = CompositeOpened;
            if (handler != null)
                handler(this, e);
        }

        private void RefreshList()
        {
            object value = m_context.GetValue();
            if (value == null) 
                return;
            ICollection collection = (ICollection)value;
            Items.Clear();
            foreach (object item in collection)
                Items.Add(GetItemString(item));
        }

        private object GetItemObject(int y)
        {
            object value = m_context.GetValue();
            if (value == null)
                return null;
            ICollection collection = (ICollection)value; 
            int index = GetItemIndex(y);
            foreach (object item in collection)
            {
                if (index == 0)
                    return item;
                index--;
            }
            return null;
        }

        private readonly PropertyEditorControlContext m_context;
        private Point m_mouseDownLocation;
    }
}
