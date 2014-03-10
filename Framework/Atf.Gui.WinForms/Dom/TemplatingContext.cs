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
    /// Editing Context for templates library; this is the context that is bound to the TemplateLister when a circuit document becomes the active context. </summary>
    /// <remarks>This context has its own independent Selection, 
    /// The ITreeView implementation controls the hierarchy in the TemplateLister's TreeControl.
    /// The IItemView implementation controls icons and labels in the TemplateLister's TreeControl.</remarks>
    public abstract class TemplatingContext : SelectionContext,
        IInstancingContext,
        ITemplatingContext,
        IObservableContext,
        INamingContext
    {
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

        public abstract TemplateFolder RootFolder { get; }

        // required  DomNodeType info
        protected abstract DomNodeType TemplateType { get; }


        #region INamingContext Members

        // Standard naming (anything derived from the normal base class)

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

        public bool CanSetName(object item)
        {
            return item.Is<TemplateFolder>() || item.Is<Template>();
        }

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

        public void SetActiveItem(object item)
        {
            if (item.Is<TemplateFolder>() || item.Is<Template>())
                m_activeItem = item;
            else
                m_activeItem = RootFolder; // so we can drag items in a folder out to root folder(which is hidden bu default)
        }

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

        public abstract bool CanReference(object item);
        public abstract object CreateReference(object item);

        #endregion

        #region IInstancingContext Members

        public bool CanCopy()
        {
            return Selection.Count > 0 && Selection.All(x => x.Is<Template>());
        }

        public object Copy()
        {
            return GetInstances(Selection);
        }

        public bool CanInsert(object insertingObject)
        {
            var dataObject = (IDataObject)insertingObject;
            var items = dataObject.GetData(typeof(object[])) as object[];
            if (items == null)
                return false;

            if (!m_activeItem.Is<TemplateFolder>())
                return false;
            return items.All(item => item.Is<Template>() || item.Is<TemplateFolder>());
        }

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
                foreach (var item in itemCopies)
                {
                    if (item.Is<Template>())
                        folder.Templates.Add(item.Cast<Template>());
                    else if (item.Is<TemplateFolder>())
                        folder.Folders.Add(item.Cast<TemplateFolder>());
                }
            }
            else
            {
                m_lastPromoted.Clear();
                for (int index = 0; index < itemCopies.Length; ++index)
                {
                    var item = itemCopies[index];
                    var template = new DomNode(TemplateType).Cast<Template>();
                    template.Model = item;
                    template.Guid = Guid.NewGuid();
                    folder.Templates.Add(template);
                    m_lastPromoted.Add(items[index], template);
                }
            }


            IsMovingItems = false;
        }

        public bool CanDelete()
        {
            return Selection.Count > 0;
        }

        public void Delete()
        {
            foreach (DomNode node in Selection.AsIEnumerable<DomNode>())
                node.RemoveFromParent();

            Selection.Clear();
        }

        #endregion


        #region ITreeView Members

        object ITreeView.Root
        {
            get { return RootFolder; }
        }

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

        public virtual void GetInfo(object item, ItemInfo info)
        {
            var folder = item.As<TemplateFolder>();
            if (folder != null)
            {
                info.Label = folder.Name;
                //if (folder.)
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

        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

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

        public object LastPromoted(object original)
        {
            return m_lastPromoted.ContainsKey(original) ? m_lastPromoted[original] : null;
        }

        public virtual bool IsGlobalTemplate(object item)
        {
            return false;

        }

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

        public void ReplaceTemplateModel(Template template,  DomNode sourceModel)
        {
            template.Model = DomNode.Copy(new[] { sourceModel }).First(); // DOM deep copy;
            m_lastPromoted.Add(sourceModel, template);
        }

        /// <summary>
        /// Invalid if the specified file  uri is already used in one of the template folder</summary>
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
                    return false;
                if (!ValidateNewFolderUri(templateFolder, uri))
                    return false;
            }

            return true;
        }

        private object m_activeItem;
        private Dictionary<object, object> m_lastPromoted = new Dictionary<object, object>();
    }
}
