//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Adapts root node to ISelectionContext, IValidationContext, ITransactionContext,
    /// IHistoryContext, and IInstancing context. This allows command components to
    /// operate on the DOM data.</summary>
    public class EditingContext : Sce.Atf.Dom.EditingContext, IInstancingContext, INamingContext
    {
        #region INamingContext Members

        /// <summary>
        /// Gets the item's name in the context, or null if none</summary>
        /// <param name="item">Item</param>
        /// <returns>Item's name in the context, or null if none</returns>
        public string GetName(object item)
        {
            UIObject uiObject = item.As<UIObject>();
            if (uiObject != null)
                return uiObject.Name;

            return null;
        }

        /// <summary>
        /// Returns whether the item can be named</summary>
        /// <param name="item">Item to name</param>
        /// <returns><c>True</c> if the item can be named</returns>
        public bool CanSetName(object item)
        {
            return item.Is<UIObject>();
        }

        /// <summary>
        /// Sets the item's name</summary>
        /// <param name="item">Item to name</param>
        /// <param name="name">New item name</param>
        public void SetName(object item, string name)
        {
            UIObject uiObject = item.As<UIObject>();
            if (uiObject != null)
                uiObject.Name = name;
        }

        #endregion

        #region IInstancingContext Members

        /// <summary>
        /// Returns whether the context can copy the selection</summary>
        /// <returns><c>True</c> if the context can copy</returns>
        public bool CanCopy()
        {
            return Selection.Any<UIObject>()
                 || Selection.Any<Curve>();
        }

        /// <summary>
        /// Copies the selection. Returns a data object representing the copied items.</summary>
        /// <returns>Data object representing the copied items; e.g., a
        /// System.Windows.Forms.IDataObject object</returns>
        public object Copy()
        {
            IEnumerable<UIObject> uiObjects = Selection.AsIEnumerable<UIObject>();
            IEnumerable<Curve> curves = Selection.AsIEnumerable<Curve>();

            IEnumerable<DomNode> rootNodes = DomNode.GetRoots(uiObjects.AsIEnumerable<DomNode>());
            IEnumerable<DomNode> rootNodes2 = DomNode.GetRoots(curves.AsIEnumerable<DomNode>());

            List<object> copies = new List<object>(DomNode.Copy(rootNodes));
            copies.AddRange(rootNodes2);
            return new DataObject(copies.ToArray());
        }

        /// <summary>
        /// Returns whether the context can insert the data object</summary>
        /// <param name="insertingObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
        /// <returns><c>True</c> if the context can insert the data object</returns>
        public bool CanInsert(object insertingObject)
        {
            IDataObject dataObject = (IDataObject)insertingObject;
            object[] items = dataObject.GetData(typeof(object[])) as object[];
            if (items == null || items.Length == 0)
                return false;

            IEnumerable<DomNode> childNodes = items.AsIEnumerable<DomNode>();
            DomNode parent = m_insertionParent.As<DomNode>();
            if (parent != null)
            {
                foreach (DomNode child in childNodes)
                {
                    // can't add child to parent if it will cause a cycle
                    foreach (DomNode ancestor in parent.Lineage)
                        if (ancestor == child)
                            return false;

                    // don't add child to the same parent
                    if (parent == child.Parent)
                        return false;

                    // make sure parent can hold child of this type
                    if (!CanParent(parent, child.Type))
                        return false;
                }

                return true;
            }
            else
            {
                EmptyRef emptyRef = m_insertionParent as EmptyRef;
                if (emptyRef != null)
                {
                    foreach (DomNode child in childNodes)
                        if (!CanReference(emptyRef, child.Type))
                            return false;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Inserts a reference to an object of given type using a transaction. Called by automated scripts during testing.</summary>
        /// <typeparam name="T">Type of object to insert</typeparam>
        /// <param name="insertingObject">DomNode that contains inserted object</param>
        /// <param name="insertionParent">Parent where object is inserted</param>
        /// <returns>Inserted object</returns>
        public T InsertAsRef<T>(DomNode insertingObject, DomNode insertionParent) where T : class
        {
            ChildInfo childInfo = GetChildInfo(insertionParent, UISchema.UIRefType.Type);
            EmptyRef emptyRef = new EmptyRef(insertionParent, childInfo);
            SetInsertionParent(emptyRef);

            insertingObject.SetAttribute(UISchema.UIType.nameAttribute, typeof(T).Name);
            DataObject dataObject = new DataObject(new object[] { insertingObject });

            ITransactionContext transactionContext = this.As<ITransactionContext>();
            transactionContext.DoTransaction(
                delegate
                {
                    Insert(dataObject);
                }, "Scripted Insert Object");

            UIRef uiRef = emptyRef.Parent.GetChild(childInfo).As<UIRef>();
            DomNode newNode = uiRef.DomNode.As<DomNode>();
            return newNode.As<T>();
        }

        /// <summary>
        /// Inserts new object of given type using a transaction. Called by automated scripts during testing.</summary>
        /// <typeparam name="T">Type of object to insert</typeparam>
        /// <param name="insertingObject">DomNode that contains inserted object</param>
        /// <param name="insertionParent">Parent where object is inserted</param>
        /// <returns>Inserted object</returns>
        public T Insert<T>(DomNode insertingObject, DomNode insertionParent) where T : class
        {
            SetInsertionParent(insertionParent);
            insertingObject.SetAttribute(UISchema.UIType.nameAttribute, typeof(T).Name);
            DataObject dataObject = new DataObject(new object[] { insertingObject });
            
            ITransactionContext transactionContext = this.As<ITransactionContext>();
            transactionContext.DoTransaction(
                delegate
                {
                    Insert(dataObject);
                }, "Scripted Insert Object");

            T newItem = null;
            ChildInfo childInfo = GetChildInfo(insertionParent, insertingObject.Type);
            if (childInfo != null)
            {
                if (childInfo.IsList)
                {
                    IList<DomNode> list = insertionParent.GetChildList(childInfo);
                    //This assumes the new object is always appended at the end of the list
                    DomNode newNode = list[list.Count - 1];
                    newItem = newNode.As<T>();
                }
                else
                {
                    DomNode newNode = insertionParent.GetChild(childInfo);
                    newItem = newNode.As<T>();
                }
            }

            return newItem;
        }

        /// <summary>
        /// Inserts the data object into the context</summary>
        /// <param name="insertingObject">Data to insert</param>
        public void Insert(object insertingObject)
        {
            IDataObject dataObject = (IDataObject)insertingObject;
            object[] items = dataObject.GetData(typeof(object[])) as object[];
            if (items == null || items.Length == 0)
                return;

            IEnumerable<DomNode> childNodes = items.AsIEnumerable<DomNode>();
            // if no items are parented, then we should clone the items, which must be from the clipboard
            bool fromScrap = true;
            foreach (DomNode child in childNodes)
            {
                if (child.Parent != null)
                {
                    fromScrap = false;
                    break;
                }
            }
            if (fromScrap)
            {
                childNodes = DomNode.Copy(childNodes);
                // inited extensions for copied DomNodes
                foreach (DomNode child in childNodes)
                    child.InitializeExtensions();
            }

            DomNode parent = m_insertionParent.As<DomNode>();
            if (parent != null)
            {
                foreach (DomNode child in childNodes)
                {
                    ChildInfo childInfo = GetChildInfo(parent, child.Type);
                    if (childInfo != null)
                    {
                        if (childInfo.IsList)
                        {
                            IList<DomNode> list = parent.GetChildList(childInfo);
                            list.Add(child);
                        }
                        else
                        {
                            parent.SetChild(childInfo, child);
                        }
                    }
                }
            }
            else
            {
                EmptyRef emptyRef = m_insertionParent as EmptyRef;
                if (emptyRef != null)
                {
                    foreach (DomNode child in childNodes)
                    {
                        UIRef uiRef = UIRef.New(child.As<UIObject>());
                        emptyRef.Parent.SetChild(emptyRef.ChildInfo, uiRef.DomNode);
                    }
                }
            }
        }

        /// <summary>
        /// Tests if can delete selected items</summary>
        /// <returns><c>True</c> if can delete selected items</returns>
        public bool CanDelete()
        {
            return Selection.Any<DomNode>(); // 
        }

        /// <summary>
        /// Deletes selected items</summary>
        public void Delete()
        {
            IEnumerable<DomNode> rootNodes = DomNode.GetRoots(Selection.AsIEnumerable<DomNode>());
            foreach (DomNode node in rootNodes)
                if (node.Parent != null)
                    node.RemoveFromParent();

            Selection.Clear();
        }

        #endregion

        /// <summary>
        /// Use DOM type metadata to determine if we can parent a child type to a parent</summary>
        /// <param name="parent">Parent</param>
        /// <param name="childType">Child type</param>
        /// <returns></returns>
        private bool CanParent(DomNode parent, DomNodeType childType)
        {
            return GetChildInfo(parent, childType) != null;
        }

        // use Dom type metadata to get matching child metadata
        private ChildInfo GetChildInfo(DomNode parent, DomNodeType childType)
        {
            foreach (ChildInfo childInfo in parent.Type.Children)
                if (childInfo.Type.IsAssignableFrom(childType))
                    return childInfo;
            return null;
        }

        // check child type against empty ref to determine if reference is valid
        private bool CanReference(EmptyRef emptyRef, DomNodeType childType)
        {
            return
                // dropping shader on sprite?
                ((childType == UISchema.UIShaderType.Type) &&
                emptyRef.ChildInfo.IsEquivalent(UISchema.UISpriteType.ShaderChild)) ||

                // dropping font on text item?
                ((childType == UISchema.UIFontType.Type) &&
                emptyRef.ChildInfo.IsEquivalent(UISchema.UITextItemType.FontChild));
        }

        /// <summary>
        /// Sets the insertion point as the user clicks and drags over the TreeControl. 
        /// The insertion point determines where paste and drag and drop operations insert new objects into the UI data.</summary>
        /// <param name="insertionParent">Parent where object is inserted</param>
        public void SetInsertionParent(object insertionParent)
        {
            m_insertionParent = insertionParent;
        }

        private object m_insertionParent;
    }
}
