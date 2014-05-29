//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Threading;

using Sce.Atf.Wpf.Models;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls.Primitives;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Behavior for TreeView that works alongside the TreeViewModel class to support
    /// multi-selection and 'bring into view' abilities</summary>
    public class AtfTreeViewBehavior : Behavior<TreeView>
    {
        #region IsMultiSelected Attached Property

        /// <summary>
        /// Attached property for multi-selection used on TreeViewItems</summary>
        /// 
        /// <AttachedPropertyComments>
        /// <summary>
        /// Attached property for multi-selection used on TreeViewItems</summary>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty IsMultiSelectedProperty =
            DependencyProperty.RegisterAttached("IsMultiSelected", typeof(bool), typeof(AtfTreeViewBehavior),
            new PropertyMetadata(false, new PropertyChangedCallback(IsMultiSelected_PropertyChanged)));

        /// <summary>
        /// Gets whether multi-selection is enabled for dependency object</summary>
        /// <param name="obj">Dependency object tested</param>
        /// <returns>True iff multi-selection is enabled</returns>
        public static bool GetIsMultiSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMultiSelectedProperty);
        }

        /// <summary>
        /// Sets whether multi-selection is enabled for dependency object</summary>
        /// <param name="obj">Dependency object set</param>
        /// <param name="value">Whether multi-selection is enabled for the dependency object</param>
        public static void SetIsMultiSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMultiSelectedProperty, value);
        }

        private static void IsMultiSelected_PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var tvi = (TreeViewItem)sender;
            bool isMultiSelected = (bool)e.NewValue;

            /*
            // Every TreeViewItem is tagged with the owner treeView
            // This is required when virtualizing as not all TreeViewItem will be in the visual
            // tree and so can not just search up through the visual tree
            // Could use an attached DP instead of the Tag if required
            TreeView treeView = tvi.Tag as TreeView;
            if (treeView == null)
            {
                treeView = tvi.FindAncestor<TreeView>();
                tvi.Tag = treeView;
            }

            if (treeView != null)
            {
                // The TreeView is tagged with the list of previously selected items
                // this list is used when deciding where to move focus when the TreeView.SelectedItem is 
                // unselected (see below)
                IList<TreeViewItem> selectedItemsList = treeView.Tag as IList<TreeViewItem>;
                if (selectedItemsList == null)
                {
                    selectedItemsList = new List<TreeViewItem>();
                    treeView.Tag = selectedItemsList;
                }

                if (isMultiSelected)
                {
                    selectedItemsList.Add(tvi);
                }
                else
                {
                    selectedItemsList.Remove(tvi);
                }

                if (!isMultiSelected && tvi.IsSelected)
                {
                    tvi.IsSelected = false;

                    // There is a bug in WPF which means an item unselected in code keeps
                    // keyboard focus so can not be reslected until keyboard focus has been moved
                    // What we need to do is move keyboard focus to the next multiselected item
                    // or if no items are selected then move focus to the parent treeview
                    if (tvi.IsKeyboardFocused)
                    {
                        if (selectedItemsList.Count > 0)
                        {
                            TreeViewItem lastSelected = selectedItemsList[selectedItemsList.Count - 1];
                            lastSelected.Focus();
                        }
                        else
                        {
                            treeView.Focus();
                        }
                    }
                }
            }*/
        }

        #endregion

        #region EnsureVisiblePath Property

        /// <summary>
        /// Ensure path is visible property bound to the TreeViewModel</summary>
        public static readonly DependencyProperty EnsureVisiblePathProperty =
            DependencyProperty.RegisterAttached("EnsureVisiblePath", typeof(Path<Node>), typeof(AtfTreeViewBehavior),
            new PropertyMetadata(new PropertyChangedCallback(EnsureVisiblePath_PropertyChanged)));

        /// <summary>
        /// Gets whether ensuring path is visible is enabled for dependency object</summary>
        /// <param name="obj">Dependency object tested</param>
        /// <returns>True iff ensuring path is visible is enabled</returns>
        public static Path<Node> GetEnsureVisiblePath(DependencyObject obj)
        {
            return (Path<Node>)obj.GetValue(EnsureVisiblePathProperty);
        }

        /// <summary>
        /// Sets whether ensuring path is visible is enabled for dependency object</summary>
        /// <param name="obj">Dependency object set</param>
        /// <param name="value">Whether ensuring path is visible is enabled for the dependency object</param>
        public static void SetEnsureVisiblePath(DependencyObject obj, Path<Node> value)
        {
            obj.SetValue(EnsureVisiblePathProperty, value);
        }

        private static void EnsureVisiblePath_PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl nextItemsControl = ((AtfTreeViewBehavior)sender).AssociatedObject;
            if (nextItemsControl == null)
                return;

            var path = e.NewValue as Path<Node>;
            if (path != null)
            {
                // Wait for any tree items which have not yet had chance to expand
                Dispatcher.CurrentDispatcher.WaitForPriority(DispatcherPriority.ContextIdle);


                // Walk the path ensuring all items are expanded and visible
                foreach (Node node in path)
                {
                    TreeViewItem nextTreeViewItem;

                    var vsp = nextItemsControl.GetFrameworkElementByType<VirtualizingStackPanel>();
                    if (vsp == null)
                    {
                        nextTreeViewItem = (TreeViewItem)nextItemsControl.ItemContainerGenerator.ContainerFromItem(node);
                    }
                    else
                    {
                        // When using virtualization it is possible that the TreeViewItem for this Node has not been
                        // created (if it is out of view)
                        // In this case we need to call BringIndexIntoView on the panel passing it the node index
                        nextTreeViewItem = (TreeViewItem)nextItemsControl.ItemContainerGenerator.ContainerFromItem(node);
                        if (nextTreeViewItem == null)
                        {
                            vsp.Dispatcher.Invoke(DispatcherPriority.ContextIdle,
                                (Action<VirtualizingStackPanel, int>)InvokeBringIndexIntoView, vsp, node.Index);

                            nextTreeViewItem = (TreeViewItem)nextItemsControl.ItemContainerGenerator.ContainerFromItem(node);
                            
                            System.Diagnostics.Debug.Assert(nextTreeViewItem != null);
                        }

                        System.Diagnostics.Debug.Assert(nextTreeViewItem.DataContext == node);
                    }

                    // Expand if not expanded and wait for the child items to be generated
                    if (node != path.Last && !nextTreeViewItem.IsExpanded)
                    {
                        nextTreeViewItem.IsExpanded = true;
                        Dispatcher.CurrentDispatcher.WaitForPriority(DispatcherPriority.ContextIdle);
                    }

                    nextItemsControl = nextTreeViewItem;

                    // If this is the final node in the path then ensure that it is scrolled into view
                    if (node == path.Last)
                    {
                        nextItemsControl.Dispatcher.Invoke(DispatcherPriority.ContextIdle, (Action)nextItemsControl.BringIntoView);
                    }
                }
            }
        }

        #endregion

        #region SynchronisingSelection Property

        /// <summary>
        /// Gets or sets whether synchronising selection dependency property</summary>
        public bool SynchronisingSelection
        {
            get { return (bool)GetValue(SynchronisingSelectionProperty); }
            set { SetValue(SynchronisingSelectionProperty, value); }
        }

        /// <summary>
        /// Whether synchronising selection dependency property</summary>
        public static readonly DependencyProperty SynchronisingSelectionProperty =
            DependencyProperty.Register("SynchronisingSelection", typeof(bool), typeof(AtfTreeViewBehavior), new UIPropertyMetadata(false));

        #endregion

        /// <summary>
        /// Raises behavior Attached event and performs custom actions</summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(AssociatedObject_SelectedItemChanged);
            AssociatedObject.PreviewMouseDown += new MouseButtonEventHandler(AssociatedObject_PreviewMouseDown);
            AssociatedObject.PreviewMouseUp += new MouseButtonEventHandler(AssociatedObject_PreviewMouseUp);
        }

        #region Event Handlers

        private void AssociatedObject_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Node node = AssociatedObject.SelectedItem as Node;
            if (node != null)
            {
                try
                {
                    SynchronisingSelection = true;

                    if (IsShiftPressed)
                    {
                        ExtendSelection(node);
                    }
                    else if (!IsCtrlPressed && !m_isMouseDown)
                    {
                        SetSelection(node);
                    }
                    else if (!node.IsSelected)
                    {
                        node.IsSelected = true;
                    }
                }
                finally
                {
                    SynchronisingSelection = false;
                }

            }
        }

        private void AssociatedObject_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            m_isMouseDown = true;

            if (!IsAltPressed)
            {
                // Get the tree view item which was hit
                bool isExpanderHit;
                TreeViewItem tvi = GetTreeViewItemAtPoint(e.GetPosition(AssociatedObject), out isExpanderHit);

                // If the expander was hit first then ignore this click and allow it to pass
                // through to toggle the expander
                if (tvi != null && !isExpanderHit)
                {
                    Node node = (Node)tvi.DataContext;

                    try
                    {
                        SynchronisingSelection = true;
                        if (e.ChangedButton == MouseButton.Left)
                        {
                            m_selecting = true;
                            if (IsCtrlPressed)
                            {
                                SynchronisingSelection = true;
                                node.IsSelected = !node.IsSelected;
                                SynchronisingSelection = false;

                                if (node.IsSelected)
                                    tvi.Focus();
                                e.Handled = true;
                            }
                            else if (IsShiftPressed)
                            {
                                ExtendSelection(node);
                                tvi.Focus();
                                e.Handled = true;
                            }
                            else
                            {
                                if (node.IsSelected)
                                {
                                    // even though selection set isn't changing, MouseUp expects m_selecting to be true.
                                    m_leftClickedSelectedNode = node;
                                    tvi.Focus();
                                    //e.Handled = true;
                                }
                                else
                                {
                                    SetSelection(node);
                                    tvi.Focus();
                                    e.Handled = true;
                                }

                                //if (ExpandOnSingleClick)
                                //{
                                //    if (!node.IsLeaf && !node.Expanded)
                                //        node.Expanded = true;
                                //}
                            }
                        }
                        else if (e.ChangedButton == MouseButton.Right)
                        {
                            if (!node.IsSelected)
                            {
                                m_selecting = true;
                                SetSelection(node);
                                tvi.Focus();
                                e.Handled = true;
                            }
                        }
                    }
                    finally
                    {
                        SynchronisingSelection = false;
                    }
                }
            }
        }

        private void AssociatedObject_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            m_isMouseDown = false;

            if (m_selecting)
            {
                m_selecting = false;

                // if this was a left-click on an already selected node
                if (m_leftClickedSelectedNode != null && e.ChangedButton == MouseButton.Left)
                {
                    bool isExpanderHit;
                    TreeViewItem tvi = GetTreeViewItemAtPoint(e.GetPosition(AssociatedObject), out isExpanderHit);

                    if (tvi != null && !isExpanderHit
                        && (tvi.DataContext as Node) == m_leftClickedSelectedNode)
                    {
                        try
                        {
                            SynchronisingSelection = true;

                            SetSelection(m_leftClickedSelectedNode); // click on selected node deselects all others
                        }
                        finally
                        {
                            SynchronisingSelection = false;
                        }
                        // if the mouse up was a left click over the clicked node's label, and label editing
                        //  is allowed, start label editing after a brief pause (to allow for a double-click).
                        //if (LabelEditModeContains(LabelEditModes.EditOnClick) &&
                        //    m_lastMouseDownWasDoubleClick == false &&
                        //    e.Button == MouseButtons.Left &&
                        //    hitRecord.Node == m_leftClickedSelectedNode &&
                        //    hitRecord.Type == HitType.Label &&
                        //    hitRecord.Node.AllowLabelEdit)
                        //{
                        //    m_editLabelTimer.Interval = SystemInformation.DoubleClickTime;
                        //    m_editLabelTimer.Tag = m_leftClickedSelectedNode;
                        //    m_editLabelTimer.Enabled = true;
                        //}
                    }

                }
            }

            m_leftClickedSelectedNode = null;
            
        }

        #endregion

        private void SetSelection(Node selected)
        {
            foreach (Node node in SelectedNodes)
                node.IsSelected = false;

            if (selected != null)
                selected.IsSelected = true;

            m_extendSelectionBaseNode = selected;

            //m_currentKeyedNode = selected;
        }

        private void ExtendSelection(Node clickedNode)
        {
            if (m_extendSelectionBaseNode != null)
            {
                bool selecting = false;
                foreach (Node n in VisibleNodes)
                {
                    n.IsSelected = selecting;

                    if (n == m_extendSelectionBaseNode ||
                        n == clickedNode)
                    {
                        if (m_extendSelectionBaseNode != clickedNode)
                            selecting = !selecting;
                        n.IsSelected = true;
                    }
                }
            }
        }

        private IEnumerable<Node> VisibleNodes
        {
            get
            {
                Stack<Node> nodes = new Stack<Node>();

                foreach(Node node in Roots.Reverse())
                    nodes.Push(node);

                while (nodes.Count > 0)
                {
                    Node node = nodes.Pop();
                    yield return node;

                    if (node.Expanded && node.ChildrenInternal != null)
                    {
                        for (int i = node.ChildrenInternal.Count - 1; i >= 0; i--)
                            nodes.Push(node.ChildrenInternal[i]);
                    }
                }
            }
        }

        private IEnumerable<Node> SelectedNodes
        {
            get
            {
                Stack<Node> nodes = new Stack<Node>();
                foreach(Node node in Roots)
                    nodes.Push(node);

                while (nodes.Count > 0)
                {
                    Node node = nodes.Pop();
                    if (node.IsSelected)
                        yield return node;

                    if (node.ChildrenInternal != null)
                    {
                        for (int i = node.ChildrenInternal.Count - 1; i >= 0; i--)
                            nodes.Push(node.ChildrenInternal[i]);
                    }
                }
            }
        }

        private IEnumerable<Node> Roots
        {
            get
            {
                foreach (var item in AssociatedObject.ItemsSource)
                {
                    yield return item as Node;
                }
            }
        }

        private static bool IsCtrlPressed
        {
            get
            {
                return Keyboard.IsKeyDown(Key.LeftCtrl)
                    || Keyboard.IsKeyDown(Key.RightCtrl);
            }
        }

        private static bool IsAltPressed
        {
            get
            {
                return Keyboard.IsKeyDown(Key.LeftAlt)
                    || Keyboard.IsKeyDown(Key.RightAlt);
            }
        }

        private static bool IsShiftPressed
        {
            get
            {
                return Keyboard.IsKeyDown(Key.LeftShift)
                    || Keyboard.IsKeyDown(Key.RightShift);
            }
        }

        private TreeViewItem GetTreeViewItemAtPoint(Point p, out bool isExpanderHit)
        {
            isExpanderHit = false;

            var dep = AssociatedObject.InputHitTest(p) as DependencyObject;
            if (dep != null)
            {
                return FindTreeViewItem(dep, out isExpanderHit);
            }
            return null;
        }

        private static TreeViewItem FindTreeViewItem(DependencyObject dep, out bool isExpanderHit)
        {
            isExpanderHit = false;

            DependencyObject current = dep;

            while (current != null)
            {
                // Make the assumption that we have a toggle button in the
                // control template! Really this should be a template part
                ToggleButton expander = current as ToggleButton;
                if (expander != null)
                {
                    isExpanderHit = true;
                }

                TreeViewItem result = current as TreeViewItem;
                if (result != null)
                {
                    return result;
                }

                if (current is Visual || current is Visual3D)
                {
                    current = VisualTreeHelper.GetParent(current);
                }
                else
                {
                    // If we're in Logical Land then we must walk 
                    // up the logical tree until we find a 
                    // Visual/Visual3D to get us back to Visual Land.
                    current = LogicalTreeHelper.GetParent(current);
                }
            }

            return null;
        }

        private static void InvokeBringIndexIntoView(VirtualizingStackPanel panel, int index)
        {
            s_bringIntoViewMethod.Invoke(panel, new object[]{index});
        }

        // Use refelection so as not to be forced to create a custom VirtualizingStackPanel to expose this method
        private static MethodInfo s_bringIntoViewMethod 
            = typeof(VirtualizingStackPanel).GetMethod("BringIndexIntoView", BindingFlags.NonPublic | BindingFlags.Instance);

        private bool m_isMouseDown;
        private bool m_selecting;
        private Node m_leftClickedSelectedNode;
        private Node m_extendSelectionBaseNode;

    }
}
