//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Adapts root DomNode to ITreeView, IItemView, and IObservableContext so it can be
    /// displayed by the TreeLister</summary>
    public class TreeView : DomNodeAdapter, ITreeView, IItemView, IObservableContext
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the tree view's DomNode</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += root_AttributeChanged;
            DomNode.ChildInserted += root_ChildInserted;
            DomNode.ChildRemoved += root_ChildRemoved;
            Reloaded.Raise(this, EventArgs.Empty);
            
            base.OnNodeSet();
        }

        #region IObservableContext Members

        /// <summary>
        /// Event that is raised when a tree item is inserted</summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        /// <summary>
        /// Event that is raised when a tree item is removed</summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        /// <summary>
        /// Event that is raised when a tree item is changed</summary>
        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

        /// <summary>
        /// Event that is raised when the tree is reloaded</summary>
        public event EventHandler Reloaded;

        #endregion

        #region ITreeView Members

        /// <summary>
        /// Gets the root object of the tree</summary>
        public object Root
        {
            get { return DomNode; }
        }

        /// <summary>
        /// Gets the children of the given parent object</summary>
        /// <param name="parent">Parent object</param>
        /// <returns>Children of the parent object</returns>
        public IEnumerable<object> GetChildren(object parent)
        {
            DomNode node = parent as DomNode;
            if (node != null)
            {
                // get child Dom nodes and empty reference "slots"
                foreach (ChildInfo childInfo in node.Type.Children)
                {
                    // skip over control points.
                    if (childInfo == UISchema.curveType.controlPointChild)
                        continue;

                    // skip over UITransformType
                    if (childInfo.Type == UISchema.UITransformType.Type)
                        continue;

                    if (childInfo.IsList)
                    {
                        foreach (DomNode child in node.GetChildList(childInfo))
                            yield return child;
                    }
                    else
                    {
                        DomNode child = node.GetChild(childInfo);
                        if (child != null)
                        {
                            yield return child;
                        }
                        else if (childInfo.Type == UISchema.UIRefType.Type)
                        {
                            yield return new EmptyRef(node, childInfo);
                        }
                    }
                }
            }
        }

        #endregion

        #region IItemView Members

        /// <summary>
        /// Gets item's display information</summary>
        /// <param name="item">Item being displayed</param>
        /// <param name="info">Item info, to fill out</param>
        public void GetInfo(object item, ItemInfo info)
        {
            DomNode node = item as DomNode;
            if (node != null)
            {
                if (node.Type == UISchema.UIRefType.Type)
                {
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.RefImage);
                    string label = string.Empty;
                    UIRef uiRef = node.As<UIRef>();
                    UIObject uiTarget = uiRef.UIObject;
                    if (uiTarget != null)
                        label = uiTarget.Name;

                    info.Label = "[" + label + "]";
                }
                else if (node.Is<Curve>())
                {
                    Curve cv = node.Cast<Curve>();
                    info.Label = string.IsNullOrWhiteSpace(cv.DisplayName) ? cv.Name : cv.DisplayName;
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.CurveImage);

                }
                else
                {
                    NodeTypePaletteItem paletteItem = node.Type.GetTag<NodeTypePaletteItem>();
                    if (paletteItem != null)
                    {
                        info.ImageIndex = info.GetImageList().Images.IndexOfKey(paletteItem.ImageName);
                        
                    }

                    info.Label = node.GetId();
                }

                info.IsLeaf = !GetChildren(item).Any();                
            }
            else
            {
                EmptyRef emptyRef = item as EmptyRef;
                if (emptyRef != null)
                {
                    info.Label = "Ref";
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.RefEmptyImage);
                    info.IsLeaf = true;
                }
            }

            if (string.IsNullOrEmpty(info.Label))
                throw new ArgumentException("info.lable");
        }

        #endregion

        private void root_AttributeChanged(object sender, AttributeEventArgs e)
        {
            ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));

            // because references use the name of the referenced item as their label, we should
            //  update all references to this DomNode. Fortunately, we can use the ReferenceValidator
            //  which is attached to this (root) node to get all the references.

            if (e.AttributeInfo.Equivalent(UISchema.UIObjectType.nameAttribute))
            {
                ReferenceValidator validator = this.As<ReferenceValidator>();
                foreach (Pair<DomNode, AttributeInfo> reference in validator.GetReferences(e.DomNode))
                {
                    if ((reference.First.Type == UISchema.UIRefType.Type) &&
                        (reference.Second.Equivalent(UISchema.UIRefType.refAttribute)))
                    {
                        ItemChanged.Raise(this, new ItemChangedEventArgs<object>(reference.First));
                    }
                }
            }
        }

        private void root_ChildInserted(object sender, ChildEventArgs e)
        {
            ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, e.Child, e.Parent));
        }

        private void root_ChildRemoved(object sender, ChildEventArgs e)
        {
            ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(-1, e.Child, e.Parent));
        }

        private int GetChildIndex(object child, object parent)
        {
            // get child index by re-constructing what we'd give the tree control
            System.Collections.IEnumerable treeChildren = GetChildren(parent);
            int i = 0;
            foreach (object treeChild in treeChildren)
            {
                if (treeChild.Equals(child))
                    return i;
                i++;
            }
            return -1;
        }
    }
}
