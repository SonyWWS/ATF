//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using Sce.Atf.Applications;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Context for editing lists. Adds IInstancingContext, ILastHitAware,
    /// IObservableContext, and INotifyPropertyChanged to EditingContext.</summary>
    public class ListEditingContext : EditingContext,
        IInstancingContext,
        ILastHitAware,
        IObservableContext,
        INotifyPropertyChanged
    {
        /// <summary>
        /// Performs initialization when the node is set.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoving += DomNode_ChildRemoving;
            DomNode.ChildRemoved += DomNode_ChildRemoved;
            Reloaded.Raise(this, EventArgs.Empty);
        }

        #region IInstancingContext Members

        /// <summary>
        /// Always returns false</summary>
        /// <returns>false</returns>
        public virtual bool CanCopy()
        {
            return false;
        }

        /// <summary>
        /// Not implemented</summary>
        /// <returns>Nothing, throws NotSupportedException</returns>
        public virtual object Copy()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Always returns false</summary>
        /// <param name="dataObject">Not used</param>
        /// <returns>false</returns>
        public virtual bool CanInsert(object dataObject)
        {
            return false;
        }

        /// <summary>
        /// Not implemented</summary>
        /// <param name="dataObject">Not used</param>
        public virtual void Insert(object dataObject)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns whether the context can delete the selection</summary>
        /// <returns>True if there is at least one selected item</returns>
        public virtual bool CanDelete()
        {
            return Selection != null && Selection.Count > 0;
        }

        /// <summary>
        /// Deletes the selected item(s)</summary>
        public virtual void Delete()
        {
            if (Selection != null)
            {
                foreach (DomNode node in GetSelection<DomNode>().ToArray<DomNode>())
                {
                    node.RemoveFromParent();
                }
            }
        }

        #endregion

        #region ILastHitAware Members

        /// <summary>
        /// Gets and sets the last hit item</summary>
        public object LastHit { get; set; }

        #endregion

        #region IObservableContext Members

        /// <summary>
        /// Event that is raised when an item is inserted</summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        /// <summary>
        /// Event that is raised when an item is removed</summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        /// <summary>
        /// Event that is raised when an item is changed</summary>
        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

        /// <summary>
        /// Event that is raised when collection has been reloaded</summary>
        public event EventHandler Reloaded;

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Event raised when a property changes</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Raise the PropertyChanged event, checking to make sure the specified propertyName
        /// represents a valid property.</summary>
        /// <param name="propertyName">Name of the changed property</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            CheckPropertyName(propertyName);
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        } 

        /// <summary>
        /// Raise the PropertyChanged event</summary>
        /// <param name="e">Event args containing the name of the changed property</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var h = PropertyChanged;
            if (h != null)
                h(this, e);
        }

        /// <summary>
        /// Allows override of index for inserted/removed children</summary>
        /// <remarks>
        /// The default behavior is to return the child list index for the dom node
        /// However there are many situations where this is incorrect e.g.
        /// If this ListContext combines DomNodes from a variety of child lists on the 
        /// parent node.  In this case we need to recalculate the child index taking into
        /// account these mutliple lists</remarks>
        /// <param name="e">ChildEventArgs of inserted/removed child</param>
        /// <returns>Index of child in this view</returns>
        protected virtual int GetChildIndex(ChildEventArgs e)
        {
            return e.Index;
        }

        /// <summary>
        /// Raise the ItemInserted event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnObjectInserted(ItemInsertedEventArgs<object> e)
        {
            ItemInserted.Raise(this, e);
        }

        /// <summary>
        /// Does nothing</summary>
        /// <param name="e">Not used</param>
        protected virtual void OnObjectRemoving(ItemRemovedEventArgs<object> e)
        {
        }

        /// <summary>
        /// Raise the ItemRemoved event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnObjectRemoved(ItemRemovedEventArgs<object> e)
        {
            ItemRemoved.Raise(this, e);
        }

        /// <summary>
        /// Raise the ItemChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnObjectChanged(ItemChangedEventArgs<object> e)
        {
            ItemChanged.Raise(this, e);
        }

        /// <summary>
        /// Raise the Reloaded event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnReloaded(EventArgs e)
        {
            Reloaded.Raise(this, e);
        }

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            OnObjectChanged(new ItemChangedEventArgs<object>(e.DomNode));
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            OnObjectInserted(new ItemInsertedEventArgs<object>(GetChildIndex(e), e.Child, e.Parent));
        }

        private void DomNode_ChildRemoving(object sender, ChildEventArgs e)
        {
            OnObjectRemoving(new ItemRemovedEventArgs<object>(GetChildIndex(e), e.Child, e.Parent));
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            OnObjectRemoved(new ItemRemovedEventArgs<object>(GetChildIndex(e), e.Child, e.Parent));
        }

        [Conditional("DEBUG")]
        private void CheckPropertyName(string propertyName)
        {
            var propertyDescriptor = TypeDescriptor.GetProperties(this)[propertyName];
            if (propertyDescriptor == null)
            {
                throw new InvalidOperationException(string.Format(null,
                    "The property with the propertyName '{0}' doesn't exist.", propertyName));
            }
        }
    }
}
