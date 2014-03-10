//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adapter that adds mouse click and drag selection to AdaptableControl. The
    /// context must be convertible to ISelectionContext.</summary>
    public class SelectionAdapter : ControlAdapter, ISelectionAdapter, IItemDragAdapter, ISelectionPathProvider
    {
        /// <summary>
        /// Gets or sets the keys used for toggle-select, where a click reverses the
        /// state of a selected item</summary>
        public Keys ToggleModifierKey
        {
            get { return m_toggleModifierKey; }
            set { m_toggleModifierKey = value; }
        }
        private Keys m_toggleModifierKey = Keys.Control;

        /// <summary>
        /// Gets or sets the keys used for extend-select, where a click adds to the
        /// selected items</summary>
        public Keys ExtendModifierKey
        {
            get { return m_extendModifierKey; }
            set { m_extendModifierKey = value; }
        }
        private Keys m_extendModifierKey = Keys.Shift;


        /// <summary>
        /// Removes the selection path for the item </summary>
        public bool RemoveSelectionPath(object item)
        {
            if (m_selectionPathMap.ContainsKey(item))
            {
                m_selectionPathMap.Remove(item);
                return true;
            }
            return false;
        }

        // update selection path for the given item based on the m_selectionContext state 
        public void UpdateSelectionPath(object item, AdaptablePath<object> path)
        {
            if (m_selectionContext.SelectionContains(item))
            {
                // item added or remain in selection, update hit path
                if (m_selectionPathMap.ContainsKey(item))
                    m_selectionPathMap[item] = path;
                else
                    m_selectionPathMap.Add(item, path);
            }        
        }

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the order that the adapters
        /// were defined on the control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            // get picking adapters in reverse order on the control            
            m_pickingAdapters = control.AsAll<IPickingAdapter>().ToArray();
            Array.Reverse(m_pickingAdapters);

            m_pickingAdapters2 = control.AsAll<IPickingAdapter2>().ToArray();
            Array.Reverse(m_pickingAdapters2);
            
            m_dragSelector = control.As<IDragSelector>();
            if (m_dragSelector != null)
                m_dragSelector.Selected += dragSelector_Selected;

            control.ContextChanged += control_ContextChanged;
        }

        /// <summary>
        /// Binds the adapter to the adaptable control.Called in the reverse order that the adapters
        /// were defined on the control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void BindReverse(AdaptableControl control)
        {
            control.MouseUp += control_MouseUp;
            control.MouseDown += control_MouseDown;
            control.DragDrop += control_DragDrop;
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            if (m_dragSelector != null)
            {
                m_dragSelector.Selected -= dragSelector_Selected;
            }

            control.ContextChanged -= control_ContextChanged;
            control.MouseUp -= control_MouseUp;
            control.MouseDown -= control_MouseDown;
            control.DragDrop -= control_DragDrop;
            m_pickingAdapters2 = null;
            m_pickingAdapters = null;
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            if (m_selectionContext != null)
                m_selectionContext.SelectionChanged -= selection_Changed;
            m_selectionContext = AdaptedControl.ContextCast<ISelectionContext>();
            if (m_selectionContext != null)
                m_selectionContext.SelectionChanged += selection_Changed;
            m_selectionPathProviderInfo.SelectionContext = m_selectionContext;
        }

        /// <summary>
        /// Event that is raised when an already selected item is clicked by user</summary>
        public event EventHandler<DiagramHitEventArgs> SelectedItemHit;

        private void control_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) && e.Clicks == 1)
            {
                // handle click if no adapter owns the mouse
                if (((Control.ModifierKeys & Keys.Alt) == 0) &&
                    !AdaptedControl.Capture)
                {
                    Point mousePt = new Point(e.X, e.Y);
                    m_hitRecord = Pick(mousePt);

                    Keys modifiers = Control.ModifierKeys;

                    if (m_hitRecord.Item != null)
                    {
                        //If we have any subItems it means that we are selecting things within an expanded group.
                        //lets pass down the subItem instead of the Item, because that is most likely what we
                        //are interested in.
                        object picked_item = (m_hitRecord.SubItem != null) ? m_hitRecord.SubItem : m_hitRecord.Item;

                        // hit on already selected item?
                        bool isSelected = m_selectionContext.SelectionContains(picked_item);
                        isSelected = UpdateSelection(picked_item, modifiers, isSelected, m_hitRecord.HitPath);
                        if (isSelected)
                        {
                            SelectedItemHit.Raise(this, new DiagramHitEventArgs(m_hitRecord));
                        }
                    }
                }
            }
        }

        private void control_MouseUp(object sender, MouseEventArgs e)
        {
            // never on drag, and only on left click
            if (!AdaptedControl.Capture && e.Button == MouseButtons.Left)
            {
                if (m_hitRecord != null && m_hitRecord.Item == null)
                {
                    Keys modifiers = Control.ModifierKeys;
                    if ((modifiers & (m_toggleModifierKey | m_extendModifierKey)) == 0)
                    {
                        // click with no modifiers, clear
                        m_selectionContext.Clear();
                    }
                }
            }
        }

        private void control_DragDrop(object sender, DragEventArgs e)
        {
            if (m_selectionContext != null || !m_selectionContext.Selection.Any())
            {
                // items may be selected  programmatically after the drag & drop, 
                // raise SelectedItemHit to enable label editor start editing with F2 without mouse click over the selected item first 

                // items may be placed using mouse position as upper-left corner; 
                // offset slightly of the mouse position to get better chance to pick the newly dropped item.
                const int offset = 20;
                var point = AdaptedControl.PointToClient(new Point(e.X + offset, e.Y + offset));
                m_hitRecord = Pick(point);
                if (m_hitRecord != null)
                    SelectedItemHit.Raise(this, new DiagramHitEventArgs(m_hitRecord));
            }
        }

        private void dragSelector_Selected(object sender, DragSelectionEventArgs e)
        {
            List<object> pickedItems = new List<object>();
            Region region = new Region(e.Bounds);
            foreach (IPickingAdapter pickingAdapter in m_pickingAdapters)
            {
                pickedItems.AddRange(pickingAdapter.Pick(region));
            }
            region.Dispose();
            
            foreach (IPickingAdapter2 pickingAdapter in m_pickingAdapters2)
            {
                pickedItems.AddRange(pickingAdapter.Pick(e.Bounds));
            }

            Keys modifiers = Control.ModifierKeys;
            if ((modifiers & m_toggleModifierKey) != 0)
            {
                m_selectionContext.ToggleRange(pickedItems);
            }
            else if ((modifiers & m_extendModifierKey) != 0)
            {
                m_selectionContext.AddRange(pickedItems);
            }
            else
            {
                m_selectionContext.SetRange(pickedItems);
            }
        }

        private DiagramHitRecord Pick(Point p)
        {
            DiagramHitRecord hitRecord = null;
            foreach (IPickingAdapter pickingAdapter in m_pickingAdapters)
            {
                hitRecord = pickingAdapter.Pick(p);
                if (hitRecord.Item != null)
                    break;
            }
            if (hitRecord == null || hitRecord.Item == null)
            {
                foreach (IPickingAdapter2 pickingAdapter in m_pickingAdapters2)
                {
                    hitRecord = pickingAdapter.Pick(p);
                    if (hitRecord.Item != null)
                        break;
                }
            }
            return hitRecord;
        }

        private bool UpdateSelection(object item, Keys modifiers, bool isSelected, AdaptablePath<object> hitPath )
        {
            bool result = false;
            if ((modifiers & m_toggleModifierKey) != 0)
            {
                m_selectionContext.Toggle(item);
                result = !isSelected;
            }
            else if ((modifiers & m_extendModifierKey) != 0)
            {
                m_selectionContext.Add(item);
                result = true;
            }
            else
            {
                if (isSelected)
                    m_selectionContext.Add(item);
                else
                    m_selectionContext.Set(item);

                result = true;
            }
            UpdateSelectionPath(item, hitPath);
            return result;
        }

        /// <summary>
        /// Begins dragging any selected items managed by the adapter. May be called
        /// by another adapter when it begins dragging.</summary>
        /// <param name="initiator">Control adapter that is initiating the drag</param>
        void IItemDragAdapter.BeginDrag(ControlAdapter initiator)
        {
        }

        void IItemDragAdapter.EndingDrag()
        {
        }

        /// <summary>
        /// Ends dragging any selected items managed by the adapter. May be called
        /// by another adapter when it ends dragging.</summary>
        void IItemDragAdapter.EndDrag()
        {
            // An item may be moved after a drag, need to update its DefaultPart bound by raising SelectedItemHit
            if (m_hitRecord != null && m_hitRecord.DefaultPart != null)
            {
              
                Point clientPoint = AdaptedControl.PointToClient(Cursor.Position);
                var hitRecord = Pick(clientPoint);
                if (hitRecord.Item == m_hitRecord.Item)
                {
                    SelectedItemHit.Raise(this, new DiagramHitEventArgs(hitRecord));
                }
            }
        }

        public AdaptablePath<object> GetSelectionPath(object item)
        {
            if (item != null && m_selectionPathMap.ContainsKey(item))
                return m_selectionPathMap[item];
            return null;
        }

        public SelectionPathProviderInfo Info
        {
            get { return m_selectionPathProviderInfo; }
        }

        public AdaptablePath<object> IncludedPath(object item)
        {
            foreach (var path in m_selectionPathMap)
                if (path.Value!= null &&  path.Value.IndexOf(item) >= 0)
                    return path.Value;
            return null;
        }

        private void selection_Changed(object sender, EventArgs e)
        {
            // remove all items not currently selected
            foreach (var key in m_selectionPathMap.Keys.ToArray())
            {
                if (!m_selectionContext.SelectionContains(key))
                    m_selectionPathMap.Remove(key);
            }
        }

        private IPickingAdapter[] m_pickingAdapters;
        private IPickingAdapter2[] m_pickingAdapters2;
        private IDragSelector m_dragSelector;
        private ISelectionContext m_selectionContext;
        private DiagramHitRecord m_hitRecord;

        private SelectionPathProviderInfo m_selectionPathProviderInfo = new SelectionPathProviderInfo();
        private Dictionary<object, AdaptablePath<object>> m_selectionPathMap = new Dictionary<object, AdaptablePath<object>>();
    }
}
