//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sce.Atf.Models
{
    /// <summary>
    /// Re-usable view model for a tree node with a tri-state check box. The check state is updated 
    /// to reflect child checked states. If all children are checked, then parent is checked. If all 
    /// children are unchecked, then parent is unchecked. If children are a mixture of checked and 
    /// unchecked, then parent is indeterminate.</summary>
    public class CheckedTreeNode : INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor</summary>
        public CheckedTreeNode()
            : this(null, null, true, true)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="value">The value of the tree node</param>
        public CheckedTreeNode(object value)
            : this(value, null, true, true)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="value">The value of the tree node</param>
        /// <param name="name">Name</param>
        /// <param name="isChecked">Checked state</param>
        /// <param name="isEnabled">Enabled/disabled state</param>
        public CheckedTreeNode(object value, string name, bool? isChecked, bool isEnabled)
        {
            Children = new ObservableCollection<CheckedTreeNode>();
            Children.CollectionChanged += Children_CollectionChanged;
            Value = value;
            Name = name;
            IsChecked = isChecked;
            IsEnabled = isEnabled;
        }

        /// <summary>
        /// Gets and sets the value of the tree node</summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets the children of the tree node</summary>
        public ObservableCollection<CheckedTreeNode> Children { get; private set; }

        /// <summary>
        /// Gets and sets the parent of the tree node. Updates the checked state on set.</summary>
        public CheckedTreeNode Parent
        {
            get { return m_parent; }
            set
            {
                m_parent = value;
                if (m_parent != null)
                {
                    m_parent.VerifyCheckState();
                }
            }
        }

        /// <summary>
        /// Gets and sets the checked state. On set, updates the checked state of the parent and children,
        /// and raises the PropertyChanged event.</summary>
        public bool? IsChecked
        {
            get { return m_isChecked; }
            set { SetIsChecked(value, true, true); }
        }

        /// <summary>
        /// Gets and sets whether the checkbox is enabled/disabled. Raises the PropertyChanged event on set.</summary>
        public bool IsEnabled
        {
            get { return m_isEnabled; }
            set
            {
                m_isEnabled = value;
                OnPropertyChanged(s_isEnabledArgs);
            }
        }

        /// <summary>
        /// Gets and sets the name.  Raises the PropertyChanged event on set.</summary>
        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
                OnPropertyChanged(s_nameArgs);
            }
        }

        /// <summary>
        /// Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises PropertyChanged event and performs custom actions</summary>
        /// <param name="e">PropertyChangedEventArgs containing event data</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var child in e.NewItems)
                        ((CheckedTreeNode)child).Parent = this;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var child in e.OldItems)
                        ((CheckedTreeNode)child).Parent = null;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var child in (System.Collections.IEnumerable)sender)
                        ((CheckedTreeNode)child).Parent = null;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private CheckedTreeNode m_parent;

        private bool? m_isChecked = null;
        private static readonly PropertyChangedEventArgs s_isCheckedArgs
            = ObservableUtil.CreateArgs<CheckedTreeNode>(x => x.IsChecked);

        private bool m_isEnabled;
        private static readonly PropertyChangedEventArgs s_isEnabledArgs
            = ObservableUtil.CreateArgs<CheckedTreeNode>(x => x.IsEnabled);

        private string m_name;
        private static readonly PropertyChangedEventArgs s_nameArgs
            = ObservableUtil.CreateArgs<CheckedTreeNode>(x => x.Name);

        private void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == m_isChecked)
                return;

            m_isChecked = value;

            if (updateChildren && m_isChecked.HasValue)
            {
                foreach (var child in Children)
                {
                    child.SetIsChecked(m_isChecked, true, false);
                }
            }

            if (updateParent && Parent != null)
                Parent.VerifyCheckState();

            OnPropertyChanged(s_isCheckedArgs);
        }

        private void VerifyCheckState()
        {
            bool? state = IsChecked;

            bool isFirst = true;
            foreach (var child in Children)
            {
                bool? current = child.IsChecked;
                if (isFirst)
                {
                    state = current;
                    isFirst = false;
                }
                else if (current != state)
                {
                    state = null;
                    break;
                }
            }

            SetIsChecked(state, false, true);
        }

    }
}