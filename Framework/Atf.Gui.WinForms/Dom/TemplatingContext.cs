//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Editing Context for templates library; this is the context that is bound to the TemplateLister 
    /// when a circuit document becomes the active context. </summary>
    /// <remarks>This context has its own independent Selection, 
    /// The ITreeView implementation controls the hierarchy in the TemplateLister's TreeControl.
    /// The IItemView implementation controls icons and labels in the TemplateLister's TreeControl.</remarks>
    public abstract class TemplatingContext : SelectionContext,
        IInstancingContext,
        ITemplatingContext,
        IObservableContext,
        INamingContext
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the circuit's DomNode:
        /// subscribe to DomNode tree change events</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;

            base.OnNodeSet();

            if (Reloaded == null) return; // inhibit compiler warning
        }

        /// <summary>
        /// Gets whether or not the user is actively moving the tempelate items</summary>
        protected bool IsMovingItems { get; set; }

        /// <summary>
        /// Gets root template folder</summary>
        public abstract TemplateFolder RootFolder { get; }

        // required  DomNodeType info
        /// <summary>
        /// Gets type of template</summary>
        protected abstract DomNodeType TemplateType { get; }


        #region INamingContext Members

        // Standard naming (anything derived from the normal base class)

        /// <summary>
        /// Gets the item's name in the context, or null if none</summary>
        /// <param name="item">Item</param>
        /// <returns>Item's name in the context, or null if none</returns>
        public string GetName(object item)
        {
            var templatesFolder = item.As<TemplateFolder>();
            if (templatesFolder != null)
                return templatesFolder.Name;

            var template = item.As<Template>();
            if (template != null)
                return template.Name;
            
            return null;
        }

        /// <summary>
        /// Returns whether the item can be named</summary>
        /// <param name="item">Item to name</param>
        /// <returns>True iff the item can be named</returns>
        public bool CanSetName(object item)
        {
            return item.Is<TemplateFolder>() || item.Is<Template>();
        }

        /// <summary>
        /// Sets the item's name</summary>
        /// <param name="item">Item to name</param>
        /// <param name="name">New item name</param>
        public void SetName(object item, string name)
        {
            var templatesFolder = item.As<TemplateFolder>();
            if (templatesFolder != null)
                templatesFolder.Name = name;
            else
            {
                var template = item.As<Template>();
                if (template != null)
                    template.Name = name; 
            }
        
        }

        #endregion

        #region ITemplatingContext Members

        /// <summary>
        /// Sets the active item in the prototyping context; used by UI components to
        /// set insertion point as the user selects and edits</summary>
        /// <param name="item">Active layer or item</param>
        public void SetActiveItem(object item)
        {
            if (item.Is<TemplateFolder>() || item.Is<Template>())
                m_activeItem = item;
            else
                m_activeItem = RootFolder; // so we can drag items in a folder out to root folder(which is hidden by default)
        }

        /// <summary>
        /// Gets the IDataObject for the items being dragged or selected, for
        /// use in a drag-and-drop or copy-paste operation</summary>
        /// <param name="items">Objects being dragged</param>
        /// <returns>IDataObject representing the dragged items</returns>
        public IDataObject GetInstances(IEnumerable<object> items)
        {
            List<object> instances = new List<object>();
            foreach (object item in items)
            {
                if (item.Is<Template>())              
                    instances.Add(item.Cast<Template>());
                else if (item.Is<TemplateFolder>())
                    instances.Add(item.Cast<TemplateFolder>());
            }
            return new DataObject(instances.ToArray());
        }

        /// <summary>
        /// Returns true iff the reference can reference the specified target item</summary>
        /// <param name="item">Template item to be referenced</param>
        /// <returns>True iff the reference can reference the specified target item</returns>
        public abstract bool CanReference(object item);

        /// <summary>
        /// Creates a reference instance that references the specified target item</summary>
        /// <param name="item">Item to create reference for</param>
        /// <returns>Reference instance that references specified target item</returns>
        public abstract object CreateReference(object item);

        #endregion

        #region IInstancingContext Members

        /// <summary>
        /// Returns whether the context can copy the selection</summary>
        /// <returns>True iff the context can copy</returns>
        public bool CanCopy()
        {
            return Selection.Count > 0 && Selection.All(x => x.Is<Template>());
        }

        /// <summary>
        /// Copies the selection. Returns a data object representing the copied items.</summary>
        /// <returns>Data object representing the copied items; e.g., a
        /// System.Windows.Forms.IDataObject object</returns>
        public object Copy()
        {
            return GetInstances(Selection);
        }

        /// <summary>
        /// Returns whether the context can insert the data object</summary>
        /// <param name="insertingObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
        /// <returns>True iff the context can insert the data object</returns>
        /// <remarks>Because non-template objects are promoted into the template library via command explicitly,
        /// CanInsert() here only needs to deal moving items around inside the template lister; 
        /// currently we do not allow drag items from outside then drop onto the template lister
        /// </remarks>
        public bool CanInsert(object insertingObject)
        {
            var dataObject = (IDataObject)insertingObject;
            var items = dataObject.GetData(typeof(object[])) as object[];
            if (items == null || !items.Any())
                return false;

            if (!m_activeItem.Is<TemplateFolder>())
                return false;
            bool moving = items.All(item => item.Is<Template>() || item.Is<TemplateFolder>());
            if (!moving) 
                return false;
            // disallow moving any item inside an external template folder
            if(items.Any(IsExternalTemplate))
                return false;
            // disallow cross-document moving
            if (IsExternalTemplate(m_activeItem.Cast<TemplateFolder>()))
                return false;
            return true;
        }

        /// <summary>
        /// Inserts the data object into the context</summary>
        /// <param name="insertingObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
        /// <remarks>ApplicationUtil calls this method in its Insert method, BUT
        /// if the context also implements IHierarchicalInsertionContext,
        /// IHierarchicalInsertionContext is preferred and the IInstancingContext
        /// implementation is ignored for insertion.</remarks>
        public void Insert(object insertingObject)
        {
            IDataObject dataObject = (IDataObject)insertingObject;
            object[] items = dataObject.GetData(typeof(object[])) as object[];
            if (items == null)
                return;

            var folder = m_activeItem.As<TemplateFolder>() ?? RootFolder;
            var domNodes = items.AsIEnumerable<DomNode>();

            IsMovingItems = domNodes.All(x => IsTemplateItem(x, x.Parent));

            var itemCopies = IsMovingItems ? domNodes.ToArray() // shallow copy, for moving items around inside template lister
                : DomNode.Copy(domNodes); // DOM deep copy

            if (IsMovingItems)
            {
                // Note: since both templates and template folders are implemented as DomNodes, 
                // inserting a DomNode to a new parent will auto-remove the node from its old parent, 
                // so we only need to take care of the insertion part
                foreach (var item in itemCopies)
                {
                    if (item.Is<Template>())
                        folder.Templates.Add(item.Cast<Template>());
                    else if (item.Is<TemplateFolder>())
                        folder.Folders.Add(item.Cast<TemplateFolder>());
                }
            }
            else //insert items as templates
            {
                m_lastPromoted.Clear();
                if (IsExternalTemplate(folder))
                    folder = RootFolder; // perhaps shouldn't prompt items to an external folder directly, let's add to root folder 
                for (int index = 0; index < itemCopies.Length; ++index)
                {
                    var item = itemCopies[index];
                    var template = new DomNode(TemplateType).Cast<Template>();
                    template.Target = item;
                    template.Guid = Guid.NewGuid();
                    folder.Templates.Add(template);
                    m_lastPromoted.Add(items[index], template);
                }
            }

            IsMovingItems = false;
        }

        /// <summary>
        /// Returns whether the context can delete the selection</summary>
        /// <returns>True iff the context can delete</returns>
        public bool CanDelete()
        {
            return Selection.Count > 0;
        }

        /// <summary>
        /// Deletes the selection</summary>
        public void Delete()
        {
            foreach (DomNode node in Selection.AsIEnumerable<DomNode>())
                node.RemoveFromParent();

            Selection.Clear();
        }

        #endregion


        #region ITreeView Members

        /// <summary>
        /// Gets the root object of the tree view</summary>
        object ITreeView.Root
        {
            get { return RootFolder; }
        }

        /// <summary>
        /// Obtains enumeration of the children of the given parent object</summary>
        /// <param name="parent">Parent object</param>
        /// <returns>Enumeration of children of the parent object</returns>
        IEnumerable<object> ITreeView.GetChildren(object parent)
        {
            var folder = parent.As<TemplateFolder>();
            if (folder != null)
            {
                foreach (var childFolder in folder.Folders)
                    yield return childFolder;
                foreach (var item in folder.Templates)
                    yield return item;
            }
        }

        #endregion

        #region IItemView Members

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public virtual void GetInfo(object item, ItemInfo info)
        {
            var folder = item.As<TemplateFolder>();
            if (folder != null)
            {
                info.Label = folder.Name;
                if (folder.Url != null)
                {
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(info.IsExpandedInView ? Sce.Atf.Resources.ReferenceFolderOpen : 
                        Sce.Atf.Resources.ReferenceFolderClosed);
                    info.HoverText = info.Description = folder.Url.LocalPath;                   
                }
                else
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(Sce.Atf.Resources.FolderIcon);
            }
            else
            {
                var template = item.As<Template>();
                if (template != null)
                {
                    info.Label = template.Name;
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(Sce.Atf.Resources.ComponentImage);
                    info.IsLeaf = true;
                }
            }
        }

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
        /// Event that is raised when the collection has been reloaded</summary>
        public event EventHandler Reloaded;
       

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            if (IsTemplateItem(e.DomNode, e.DomNode.Parent))
                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            if (IsTemplateItem(e.Child, e.Parent))
                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            if (IsTemplateItem(e.Child, e.Parent))
                ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

        private static bool IsTemplateItem(DomNode child, DomNode parent)
        {
            return
                child.Is<TemplateFolder>() ||
                (parent != null && parent.Is<TemplateFolder>());
        }

        #endregion

        /// <summary>
        /// Gets object last promoted to template library</summary>
        /// <param name="original">Original item</param>
        /// <returns>Object last promoted to template library</returns>
        public object LastPromoted(object original)
        {
            return m_lastPromoted.ContainsKey(original) ? m_lastPromoted[original] : null;
        }

        /// <summary>
        /// Returns whether an item is a global template</summary>
        /// <param name="item">Item to test</param>
        /// <returns>True iff item is a global template</returns>
        public bool IsExternalTemplate(object item)
        {
            var domNode = item.As<DomNode>();
            if (domNode == null)
                return false;
            foreach (var node in domNode.Lineage)
            {
                TemplateFolder templateFolder = node.As<TemplateFolder>();
                if (templateFolder != null && templateFolder.Url != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Searches for a template by its GUID</summary>
        /// <param name="parentFolder">Template's parent folder to search through</param>
        /// <param name="guid">GUID to search for</param>
        /// <returns>Template matching given GUID</returns>
        public Template SearchForTemplateByGuid(TemplateFolder parentFolder, Guid guid)
        {        
            foreach (var template in parentFolder.Templates)
            {
                if (template.Guid  == guid)
                {
                    return template;
                   
                }
            }

            foreach (var templateFolder in parentFolder.Folders)
            {
                var template = SearchForTemplateByGuid(templateFolder, guid);
                if (template != null)
                    return template;
            }

            return null;
        }

        /// <summary>
        /// Replace template with another one</summary>
        /// <param name="template">Template to replace</param>
        /// <param name="sourceModel">Template to replace it by as DomNode</param>
        public void ReplaceTemplateModel(Template template,  DomNode sourceModel)
        {
            template.Target = DomNode.Copy(new[] { sourceModel }).First(); // DOM deep copy;
            m_lastPromoted.Add(sourceModel, template);
        }

        /// <summary>
        /// Validate template folder URI.
        /// Folder invalid if the specified file URI is already used in one of the template folders</summary>
        /// <param name="uri">Template folder URI</param>
        /// <returns>True iff folder is valid</returns>
        public bool ValidateNewFolderUri(Uri uri)
        {
            return ValidateNewFolderUri(RootFolder, uri);
        }

        private bool ValidateNewFolderUri(TemplateFolder parentFolder, Uri uri)
        {
            if (uri == null)
                return false;

            foreach (var templateFolder in parentFolder.Folders)
            {
                if (templateFolder.Url == uri)
                    return false; //TODO: pop up warning dialog
                if (!ValidateNewFolderUri(templateFolder, uri))
                    return false;
            }

            return true;
        }

        private object m_activeItem;
        private Dictionary<object, object> m_lastPromoted = new Dictionary<object, object>();
    }
}
