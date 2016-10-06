//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace DomPropertyEditorSample
{
    /// <summary>
    /// Used for updating PropertyEditor on Undo/Redo</summary>
    public class GameEditingContext : EditingContext, ITreeView, IItemView, IObservableContext
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// Subscribes to events for DomNode tree changes and raises Reloaded event.</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += (sender, e) => ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
            DomNode.ChildInserted += (sender, e) => ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
            DomNode.ChildRemoved += (sender,e)=> ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));

            Reloaded.Raise(this, EventArgs.Empty);
            base.OnNodeSet();
        }

        #region IObservableContext Members
        /// <summary>
        /// Event handler for node inserted in DomNode tree.</summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;
        /// <summary>
        /// Event handler for node removed from DomNode tree.</summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;
        /// <summary>
        /// Event handler for node changed in DomNode tree.</summary>
        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;
        /// <summary>
        /// Event that is raised when the DomNode tree has been reloaded.</summary>
        public event EventHandler Reloaded;
        #endregion

        #region ITreeView Members

        object ITreeView.Root
        {
            get { return DomNode; }
        }

        IEnumerable<object> ITreeView.GetChildren(object parent)
        {
             DomNode node = parent as DomNode;
             if (node != null) return node.Children;
             return EmptyArray<object>.Instance;             
        }

        #endregion

        #region IItemView Members

        void IItemView.GetInfo(object item, ItemInfo info)
        {
              var node = item as DomNode;
              if (node != null)
              {
                  string name = node.GetId();
                  if (string.IsNullOrEmpty(name))
                  {
                      var attInfo = node.Type.GetAttributeInfo("name");
                      if (attInfo == null) attInfo = node.Type.GetAttributeInfo("label");
                      if (attInfo != null && attInfo.Type.Type == AttributeTypes.String)
                          name = (string)node.GetAttribute(attInfo);                                               
                  }
                      
                  info.AllowLabelEdit = false;
                  info.IsLeaf = !node.Children.Any();
                  info.Label = string.IsNullOrWhiteSpace(name) ? node.Type.Name : name;
              }
        }

        #endregion
    }
}
