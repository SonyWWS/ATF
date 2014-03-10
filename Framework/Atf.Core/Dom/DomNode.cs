//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Sce.Atf.Adaptation;
using System.Diagnostics;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Node in a DOM data tree</summary>
    public sealed class DomNode : IAdaptable, IDecoratable
    {
        /// <summary>
        /// Constructor for a DomNode with no specified relationship to a parent or external
        /// container. The ChildInfo property is null until this DomNode is parented. Note
        /// that the root DomNode of a document must have a ChildInfo in order to be persisted
        /// as an XML file.</summary>
        /// <param name="nodeType">Type of DomNode</param>
        public DomNode(DomNodeType nodeType)
            : this(nodeType, null)
        {
        }

        /// <summary>
        /// Constructor for a DomNode with a ChildInfo</summary>
        /// <param name="nodeType">Type of DomNode</param>
        /// <param name="childInfo">Metadata for DomNode, specifying its relationship to its parent,
        /// or its place in some external container. If null, the ChildInfo property is
        /// set when this DomNode is parented. The root DomNode cannot have a null ChildInfo if it
        /// is persisted in an XML file.</param>
        /// <remarks>Extensions like DomNodeAdapters are initialized when they are first accessed unless
        /// InitializeExtensions() is used.</remarks>
        public DomNode(DomNodeType nodeType, ChildInfo childInfo)
        {
            if (nodeType.IsAbstract)
                throw new InvalidOperationException("Can't instantiate an abstract node type");

            m_type = nodeType;
            m_childInfo = childInfo;

            if (!nodeType.IsFrozen)
                nodeType.Freeze();

            m_data = nodeType.FieldCount > 0 ? new object[nodeType.FieldCount] : EmptyArray<object>.Instance;

            int extensionOffset = nodeType.FirstExtensionIndex;
            int i = 0;
            foreach (ExtensionInfo extensionInfo in nodeType.Extensions)
            {
                object extension = extensionInfo.Create(this);
                m_data[i + extensionOffset] = extension;
                i++;
            }
        }

        /// <summary>
        /// Gets the DomNode's type</summary>
        public DomNodeType Type
        {
            get { return m_type; }
        }

        /// <summary>
        /// Gets the parent DomNode, or null if a root</summary>
        public DomNode Parent
        {
            get { return m_parent; }
        }

        /// <summary>
        /// Gets the root DomNode of this DomNode's subtree</summary>
        public DomNode GetRoot()
        {
            DomNode node = this;
            while (node.m_parent != null)
                node = node.m_parent;

            return node;
        }

        /// <summary>
        /// Gets the metadata describing this node's relationship to its parent</summary>
        public ChildInfo ChildInfo
        {
            get { return m_childInfo; }
        }

        /// <summary>
        /// Gets the lineage of this node, starting with the node, and ending with
        /// the root node. Is the reverse order of GetPath.</summary>
        public IEnumerable<DomNode> Lineage
        {
            get
            {
                DomNode node = this;
                while (node != null)
                {
                    yield return node;
                    node = node.m_parent;
                }
            }
        }

        /// <summary>
        /// Gets the ancestry of this node, starting with the node's parent, and
        /// ending with the root node</summary>
        public IEnumerable<DomNode> Ancestry
        {
            get
            {
                DomNode node = m_parent;
                while (node != null)
                {
                    yield return node;
                    node = node.m_parent;
                }
            }
        }

        /// <summary>
        /// Gets the path of nodes starting with the root of this tree and ending with
        /// this node. Is the reverse order of Lineage.</summary>
        /// <returns>The enumeration of DomNodes from the root to this node</returns>
        /// <remarks>Useful for passing into the Sce.Atf.Path constructor</remarks>
        public IEnumerable<DomNode> GetPath()
        {
            List<DomNode> path = new List<DomNode>(Lineage);
            for (int i = path.Count; --i >= 0; )
                yield return path[i];
        }

        /// <summary>
        /// Returns a value indicating if this node is a descendant of the given node</summary>
        /// <param name="node">Node to test</param>
        /// <returns>True iff this node is a descendant of the given node</returns>
        public bool IsDescendantOf(DomNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            DomNode ancestor = m_parent;
            while (ancestor != null)
            {
                if (ancestor == node)
                    return true;
                ancestor = ancestor.m_parent;
            }

            return false;
        }

        /// <summary>
        /// Gets the enumeration of the children of this node</summary>
        public IEnumerable<DomNode> Children
        {
            get
            {
                foreach (ChildInfo childInfo in m_type.Children)
                    foreach (DomNode child in GetChildren(childInfo))
                        yield return child;
            }
        }

        /// <summary>
        /// Gets the enumeration of the subtree rooted at this node, in pre-order (also known
        /// as depth-first)</summary>
        public IEnumerable<DomNode> Subtree
        {
            get
            {
                Stack<DomNode> nodes = new Stack<DomNode>();
                nodes.Push(this);

                while (nodes.Count > 0)
                {
                    DomNode node = nodes.Pop();
                    yield return node;

                    // push children in reverse order so pop gives nodes in pre-order
                    foreach (ChildInfo childInfo in node.Type.Children)
                    {
                        int i = childInfo.Index + node.Type.FirstChildIndex;
                        if (childInfo.IsList)
                        {
                            NodeList.ChildList children = node.m_data[i] as NodeList.ChildList;
                            if (children != null)
                            {
                                for (int j = children.Count - 1; j >= 0; j--)
                                    nodes.Push(children[j]);
                            }
                        }
                        else
                        {
                            DomNode child = node.m_data[i] as DomNode;
                            if (child != null)
                                nodes.Push(child);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the enumeration of the subtree rooted at this node, in level-order (also known
        /// as breadth-first)</summary>
        public IEnumerable<DomNode> LevelSubtree
        {
            get
            {
                var queue = new Queue<DomNode>();
                queue.Enqueue(this);
                var visited = new HashSet<DomNode> { this };

                while (queue.Count > 0)
                {
                    DomNode node = queue.Dequeue();
                    yield return node;

                    foreach (var child in Children)
                        if (!visited.Contains(child))
                        {
                            visited.Add(child);
                            queue.Enqueue(child);
                        }

                }
            }
        }

        /// <summary>
        /// Gets the nodes in the enumeration that are not descendants of any other
        /// node in the enumeration</summary>
        /// <param name="nodes">Enumeration of nodes</param>
        /// <returns>Nodes in the enumeration that are not descendants of any other
        /// node in the enumeration</returns>
        public static IEnumerable<DomNode> GetRoots(IEnumerable<DomNode> nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException("nodes");

            HashSet<DomNode> nodeSet = new HashSet<DomNode>(nodes);
            foreach (DomNode node in nodes)
            {
                DomNode ancestor = node.Parent;
                while (ancestor != null)
                {
                    if (nodeSet.Contains(ancestor))
                        break;

                    ancestor = ancestor.Parent;
                }
                if (ancestor == null)
                    yield return node;
            }
        }

        /// <summary>
        /// Gets the lowest common ancestor (LCA) of a pair of DOM nodes. The LCA
        /// is the common ancestor of the two nodes that is furthest from the root
        /// node.</summary>
        /// <param name="node1">First node</param>
        /// <param name="node2">Second node</param>
        /// <returns>LCA of the nodes, or null if none</returns>
        public static DomNode GetLowestCommonAncestor(DomNode node1, DomNode node2)
        {
            if (node1 == null)
                return node2;
            if (node2 == null)
                return node1;

            HashSet<DomNode> path1 = new HashSet<DomNode>(node1.Lineage);
            foreach (DomNode node in node2.Lineage)
                if (path1.Contains(node))
                    return node;

            return null;
        }

        /// <summary>
        /// Gets the lowest common ancestor (LCA) of a sequence of DOM nodes. The LCA
        /// is the common ancestor of the nodes that is furthest from the root node.</summary>
        /// <param name="nodes">Sequence of 2 or more DOM nodes</param>
        /// <returns>LCA of the nodes, or null if none</returns>
        public static DomNode GetLowestCommonAncestor(IEnumerable<DomNode> nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException("nodes");

            DomNode lca = null;
            foreach (DomNode node in nodes)
            {
                lca = GetLowestCommonAncestor(lca, node);
                if (lca == null)
                    break; // no lca
            }

            return lca;
        }

        /// <summary>
        /// For all extensions implementing IAdapter, sets Adaptee to this DomNode. This
        /// usually happens just before the adapter is requested for the first time, but
        /// some adapters (e.g. validators) are never referenced but must be attached to
        /// the underlying DOM data.</summary>
        /// <remarks>Call this method when a newly constructed DOM sub-tree is complete.</remarks>
        public void InitializeExtensions()
        {
            foreach (DomNode node in Subtree)
            {
                foreach (ExtensionInfo extensionInfo in node.Type.Extensions)
                {
                    IAdapter nodeAdapter = node.GetExtension(extensionInfo) as IAdapter;
                    if (nodeAdapter != null && nodeAdapter.Adaptee == null)
                        nodeAdapter.Adaptee = node;
                }
            }
        }

        /// <summary>
        /// Gets the attribute corresponding to the attribute metadata</summary>
        /// <param name="attributeInfo">Attribute metadata</param>
        /// <returns>Attribute corresponding to the attribute metadata</returns>
        public object GetAttribute(AttributeInfo attributeInfo)
        {
            object attribute = GetLocalAttribute(attributeInfo);
            if (attribute == null)
                attribute = attributeInfo.DefaultValue;

            return attribute;
        }

        /// <summary>
        /// Gets the local attribute, or null, corresponding to the attribute metadata</summary>
        /// <param name="attributeInfo">Attribute metadata</param>
        /// <returns>Local attribute, or null, corresponding to the attribute metadata</returns>
        public object GetLocalAttribute(AttributeInfo attributeInfo)
        {
            int index = m_type.GetDataIndex(attributeInfo);
            return m_data[index];
        }

        /// <summary>
        /// Gets the node's id as a string, or null</summary>
        /// <returns>Node's id as a string, or null</returns>
        public string GetId()
        {
            string id = null;
            AttributeInfo idAttribute = m_type.IdAttribute;
            if (idAttribute != null) // has an id?
            {
                // only local value is valid
                object value = GetLocalAttribute(idAttribute);
                if (value != null)
                    id = value.ToString();
            }

            return id;
        }

        /// <summary>
        /// Gets whether or not the attribute's value (like by calling GetAttribute) is equal to
        /// the default value.</summary>
        /// <param name="attributeInfo">Attribute metadata</param>
        /// <returns>True if the attribute's value is equal to the default value</returns>
        public bool IsAttributeDefault(AttributeInfo attributeInfo)
        {
            object attribute = GetLocalAttribute(attributeInfo);
            return attribute == null || attribute.Equals(attributeInfo.DefaultValue);
        }

        /// <summary>
        /// Sets the attribute corresponding to the attribute metadata only if the current
        /// value is the default value</summary>
        /// <param name="attributeInfo">Attribute metadata</param>
        /// <param name="value">New attribute value</param>
        public void SetAttributeIfDefault(AttributeInfo attributeInfo, object value)
        {
            if (IsAttributeDefault(attributeInfo))
                SetAttribute(attributeInfo, value);
        }

        /// <summary>
        /// Sets the attribute corresponding to the attribute metadata</summary>
        /// <param name="attributeInfo">Attribute metadata</param>
        /// <param name="value">New attribute value</param>
        public void SetAttribute(AttributeInfo attributeInfo, object value)
        {
            int index = m_type.GetDataIndex(attributeInfo);
            object oldValue = m_data[index];

            // The old value should be the same as GetAttribute() would have returned, to ensure
            //  that the old value is a valid value.
            if (oldValue == null) //'null' means "use default".
                oldValue = attributeInfo.DefaultValue;

            // Do nothing if 'value' is the same as either the local value or the default value.
            if (!attributeInfo.Type.AreEqual(oldValue, value))
            {
                AttributeEventArgs e = new AttributeEventArgs(this, attributeInfo, oldValue, value);
                RaiseAttributeChanging(e);
                m_data[index] = value;
                DiagnosticAttributeChanged.Raise(this, e);
                RaiseAttributeChanged(e);
            }
        }

        /// <summary>
        /// Gets the child corresponding to the given child metadata</summary>
        /// <param name="childInfo">Child metadata for when there is a maximum of one child</param>
        /// <returns>Child corresponding to the given child metadata, or null</returns>
        public DomNode GetChild(ChildInfo childInfo)
        {
            if (childInfo.IsList)
                throw new InvalidOperationException("field is a list");

            int index = m_type.GetDataIndex(childInfo);
            return m_data[index] as DomNode;
        }

        /// <summary>
        /// Gets the child list corresponding to the given child metadata</summary>
        /// <param name="childInfo">Child metadata for when there can be more than one child</param>
        /// <returns>Child list corresponding to the given child metadata, or null</returns>
        public IList<DomNode> GetChildList(ChildInfo childInfo)
        {
            if (!childInfo.IsList)
                throw new InvalidOperationException("field is a singleton");

            return new NodeList(this, childInfo);
        }

        /// <summary>
        /// Gets an enumeration of the child or children of this DomNode corresponding to the child metadata</summary>
        /// <param name="childInfo">Child metadata, specifying either a list of children or a single child</param>
        /// <returns>An enumeration of the children corresponding to the given child metadata.
        /// Does not return null.</returns>
        public IEnumerable<DomNode> GetChildren(ChildInfo childInfo)
        {
            int index = m_type.GetDataIndex(childInfo);
            if (childInfo.IsList)
            {
                NodeList.ChildList children = m_data[index] as NodeList.ChildList;
                if (children != null)
                {
                    foreach (DomNode child in children)
                        yield return child;
                }
            }
            else
            {
                DomNode child = m_data[index] as DomNode;
                if (child != null)
                    yield return child;
            }
        }

        /// <summary>
        /// Sets the child corresponding to the child metadata</summary>
        /// <param name="childInfo">Child metadata</param>
        /// <param name="child">New child</param>
        public void SetChild(ChildInfo childInfo, DomNode child)
        {
            if (childInfo.IsList)
                throw new InvalidOperationException("field is a list");

            int index = m_type.GetDataIndex(childInfo);
            DomNode oldChild = m_data[index] as DomNode;

            ChildEventArgs removeEventArgs = new ChildEventArgs(this, childInfo, oldChild, 0);
            ChildEventArgs insertEventArgs = new ChildEventArgs(this, childInfo, child, 0);

            if (m_data[index] != null)
            {
                RaiseChildRemoving(removeEventArgs);
                m_data[index] = null;
                oldChild.SetParent(null, null);
                RaiseChildRemoved(removeEventArgs);
            }

            if (child != null)
            {
                if (child.m_parent != null)
                {
                    child.RemoveFromParent();
                }

                RaiseChildInserting(insertEventArgs);
                m_data[index] = child;
                child.SetParent(this, childInfo);
                RaiseChildInserted(insertEventArgs);
            }
        }

        /// <summary>
        /// Removes this DomNode from its parent; if node is a root, does nothing</summary>
        public void RemoveFromParent()
        {
            if (m_parent != null)
            {
                if (m_childInfo.IsList)
                {
                    IList<DomNode> nodes = m_parent.GetChildList(m_childInfo);
                    nodes.Remove(this);
                }
                else
                {
                    m_parent.SetChild(m_childInfo, null);
                }

                SetParent(null, null);
            }
        }

        /// <summary>
        /// Gets the extension object corresponding to the extension metadata</summary>
        /// <param name="extensionInfo">Extension metadata</param>
        /// <returns>Extension object corresponding to the extension metadata</returns>
        public object GetExtension(ExtensionInfo extensionInfo)
        {
            int index = m_type.GetDataIndex(extensionInfo);
            object extension = m_data[index];
            return extension;
        }

        /// <summary>
        /// Event that is raised before an attribute is changed to a new value on this DomNode
        /// or any DomNode of the sub-tree</summary>
        public event EventHandler<AttributeEventArgs> AttributeChanging
        {
            add { GetEventHandlers().AttributeChanging += value; }
            remove { GetEventHandlers().AttributeChanging -= value; }
        }

        /// <summary>
        /// Event that is raised after an attribute is changed to a new value on this DomNode
        /// or any DomNode of the sub-tree</summary>
        public event EventHandler<AttributeEventArgs> AttributeChanged
        {
            add { GetEventHandlers().AttributeChanged += value; }
            remove { GetEventHandlers().AttributeChanged -= value; }
        }

        /// <summary>
        /// Event that is raised before a child is inserted into a child list on this DomNode
        /// or any DomNode of the sub-tree</summary>
        public event EventHandler<ChildEventArgs> ChildInserting
        {
            add { GetEventHandlers().ChildInserting += value; }
            remove { GetEventHandlers().ChildInserting -= value; }
        }

        /// <summary>
        /// Event that is raised after a child is inserted into a child list on this DomNode
        /// or any DomNode of the sub-tree</summary>
        public event EventHandler<ChildEventArgs> ChildInserted
        {
            add { GetEventHandlers().ChildInserted += value; }
            remove { GetEventHandlers().ChildInserted -= value; }
        }

        /// <summary>
        /// Event that is raised before a child is removed from a child list on this DomNode
        /// or any DomNode of the sub-tree</summary>
        public event EventHandler<ChildEventArgs> ChildRemoving
        {
            add { GetEventHandlers().ChildRemoving += value; }
            remove { GetEventHandlers().ChildRemoving -= value; }
        }

        /// <summary>
        /// Event that is raised after a child is removed from a child list on this DomNode
        /// or any DomNode of the sub-tree</summary>
        public event EventHandler<ChildEventArgs> ChildRemoved
        {
            add { GetEventHandlers().ChildRemoved += value; }
            remove { GetEventHandlers().ChildRemoved -= value; }
        }

        /// <summary>
        /// Subscribes the destination node to this DomNode's events</summary>
        /// <param name="destination">DomNode to be added to this DomNode's subscribers</param>
        public void SubscribeToEvents(DomNode destination)
        {
            GetEventHandlers().Subscribe(destination);
        }

        /// <summary>
        /// Unsubscribes the destination node from this DomNode's events</summary>
        /// <param name="destination">DomNode to be removed from this DomNode's subscribers</param>
        public void UnsubscribeFromEvents(DomNode destination)
        {
            GetEventHandlers().Unsubscribe(destination);
        }

        #region IAdaptable, IDecoratable, and Related Methods

        /// <summary>
        /// Gets an adapter of the specified type, or null</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type, or null</returns>
        public object GetAdapter(Type type)
        {
            // try a normal cast
            if (type.IsAssignableFrom(typeof(DomNode)))
                return this;

            // try to get an adapter
            return DomNodeType.GetAdapter(this, type);
        }

        /// <summary>
        /// Gets all decorators of the specified type, or null</summary>
        /// <param name="type">Decorator type</param>
        /// <returns>Enumeration of decorators that are of the specified type</returns>
        public IEnumerable<object> GetDecorators(Type type)
        {
            return DomNodeType.GetAdapters(this, type);
        }

        #endregion

        // Deep Copying Support

        /// <summary>
        /// Creates a deep copy of the graph of the original DomNodes</summary>
        /// <param name="originals">Original nodes</param>
        /// <param name="originalToCopyMap">for each DomNode, the passed-in dictionary will be filled so orginal DomNode will be mapped to copied DomNode </param>
        /// <returns>Array of copies of original nodes</returns>
        /// <remarks>Extensions are not copied</remarks>
        public static DomNode[] Copy(IEnumerable<DomNode> originals, Dictionary<DomNode, DomNode> originalToCopyMap=null)
        {
            // the first pass creates the copies and builds a mapping from original to copy
            if (originalToCopyMap == null)
                originalToCopyMap = new Dictionary<DomNode, DomNode>();
            else
                originalToCopyMap.Clear();
            List<DomNode> copies = new List<DomNode>();
            foreach (DomNode original in originals)
            {
                DomNode copy = original.Copy(originalToCopyMap);
                copies.Add(copy);
            }

            // the second pass changes references in the copies that should now refer to other copies
            foreach (DomNode copy in copies)
                copy.UpdateReferences(originalToCopyMap);

            return copies.ToArray();
        }

        private DomNode Copy(IDictionary<DomNode, DomNode> originalToCopyMap)
        {
            // copy the adapted DomNode and all its descendants
            DomNode copy = new DomNode(m_type);
            originalToCopyMap.Add(this, copy);

            // clone local attributes
            foreach (AttributeInfo attributeInfo in m_type.Attributes)
            {
                object value = GetLocalAttribute(attributeInfo);
                if (value != null)
                    copy.SetAttribute(attributeInfo, attributeInfo.Type.Clone(value));
            }

            // clone children
            foreach (ChildInfo childInfo in m_type.Children)
            {
                if (childInfo.IsList)
                {
                    IList<DomNode> copyChildList = copy.GetChildList(childInfo);
                    foreach (DomNode child in GetChildList(childInfo))
                    {
                        DomNode childCopy = child.Copy(originalToCopyMap);
                        copyChildList.Add(childCopy);
                    }
                }
                else
                {
                    DomNode child = GetChild(childInfo);
                    if (child != null)
                    {
                        DomNode childCopy = child.Copy(originalToCopyMap);
                        copy.SetChild(childInfo, childCopy);
                    }
                }
            }

            return copy;
        }

        private void UpdateReferences(IDictionary<DomNode, DomNode> originalToCopyMap)
        {
            foreach (AttributeInfo attributeInfo in Type.Attributes)
            {
                if (attributeInfo.Type.Type == AttributeTypes.Reference)
                {
                    DomNode target = GetAttribute(attributeInfo) as DomNode;
                    // try to map reference to corresponding copy
                    DomNode targetCopy;
                    if (target != null)
                    {
                        if (originalToCopyMap.TryGetValue(target, out targetCopy))
                        {
                            SetAttribute(attributeInfo, targetCopy);
                        }
                    }
                }
            }

            foreach (DomNode child in Children)
            {
                child.UpdateReferences(originalToCopyMap);
            }
        }

        // Lightweight Event Support

        private class NodeEventHandlers
        {
            public NodeEventHandlers(DomNode node)
            {
                m_node = node;
            }

            public event EventHandler<AttributeEventArgs> AttributeChanging;

            internal void RaiseAttributeChanging(AttributeEventArgs e)
            {
                AttributeChanging.Raise(m_node, e);

                if (m_subscribers != null)
                    foreach (DomNode subscriber in m_subscribers)
                        subscriber.RaiseAttributeChanging(e);
            }

            internal IEnumerable<EventHandler<AttributeEventArgs>> GetAttributeChangingHandlers()
            {
                if (AttributeChanging != null)
                    foreach (EventHandler<AttributeEventArgs> listener in AttributeChanging.GetInvocationList())
                        yield return listener;

                if (m_subscribers != null)
                    foreach (DomNode subscriber in m_subscribers)
                        foreach (EventHandler<AttributeEventArgs> listener in subscriber.GetAttributeChangingHandlers())
                            yield return listener;
            }

            public event EventHandler<AttributeEventArgs> AttributeChanged;

            internal void RaiseAttributeChanged(AttributeEventArgs e)
            {
                AttributeChanged.Raise(m_node, e);

                if (m_subscribers != null)
                    foreach (DomNode subscriber in m_subscribers)
                        subscriber.RaiseAttributeChanged(e);
            }

            internal IEnumerable<EventHandler<AttributeEventArgs>> GetAttributeChangedHandlers()
            {
                if (AttributeChanged != null)
                    foreach (EventHandler<AttributeEventArgs> listener in AttributeChanged.GetInvocationList())
                        yield return listener;

                if (m_subscribers != null)
                    foreach (DomNode subscriber in m_subscribers)
                        foreach (EventHandler<AttributeEventArgs> listener in subscriber.GetAttributeChangedHandlers())
                            yield return listener;
            }

            public event EventHandler<ChildEventArgs> ChildInserting;

            internal void RaiseChildInserting(ChildEventArgs e)
            {
                ChildInserting.Raise(m_node, e);

                if (m_subscribers != null)
                    foreach (DomNode subscriber in m_subscribers)
                        subscriber.RaiseChildInserting(e);
            }

            internal IEnumerable<EventHandler<ChildEventArgs>> GetChildInsertingHandlers()
            {
                if (ChildInserting != null)
                    foreach (EventHandler<ChildEventArgs> listener in ChildInserting.GetInvocationList())
                        yield return listener;

                if (m_subscribers != null)
                    foreach (DomNode subscriber in m_subscribers)
                        foreach (EventHandler<ChildEventArgs> listener in subscriber.GetChildInsertingHandlers())
                            yield return listener;
            }

            public event EventHandler<ChildEventArgs> ChildInserted;

            internal void RaiseChildInserted(ChildEventArgs e)
            {
                ChildInserted.Raise(m_node, e);

                if (m_subscribers != null)
                    foreach (DomNode subscriber in m_subscribers)
                        subscriber.RaiseChildInserted(e);
            }

            internal IEnumerable<EventHandler<ChildEventArgs>> GetChildInsertedHandlers()
            {
                if (ChildInserted != null)
                    foreach (EventHandler<ChildEventArgs> listener in ChildInserted.GetInvocationList())
                        yield return listener;

                if (m_subscribers != null)
                    foreach (DomNode subscriber in m_subscribers)
                        foreach (EventHandler<ChildEventArgs> listener in subscriber.GetChildInsertedHandlers())
                            yield return listener;
            }

            public event EventHandler<ChildEventArgs> ChildRemoving;

            internal void RaiseChildRemoving(ChildEventArgs e)
            {
                ChildRemoving.Raise(m_node, e);

                if (m_subscribers != null)
                    foreach (DomNode subscriber in m_subscribers)
                        subscriber.RaiseChildRemoving(e);
            }

            internal IEnumerable<EventHandler<ChildEventArgs>> GetChildRemovingHandlers()
            {
                if (ChildRemoving != null)
                    foreach (EventHandler<ChildEventArgs> listener in ChildRemoving.GetInvocationList())
                        yield return listener;

                if (m_subscribers != null)
                    foreach (DomNode subscriber in m_subscribers)
                        foreach (EventHandler<ChildEventArgs> listener in subscriber.GetChildRemovingHandlers())
                            yield return listener;
            }

            public event EventHandler<ChildEventArgs> ChildRemoved;

            internal void RaiseChildRemoved(ChildEventArgs e)
            {
                ChildRemoved.Raise(m_node, e);

                if (m_subscribers != null)
                    foreach (DomNode subscriber in m_subscribers)
                        subscriber.RaiseChildRemoved(e);
            }

            internal IEnumerable<EventHandler<ChildEventArgs>> GetChildRemovedHandlers()
            {
                if (ChildRemoved != null)
                    foreach (EventHandler<ChildEventArgs> listener in ChildRemoved.GetInvocationList())
                        yield return listener;

                if (m_subscribers != null)
                    foreach (DomNode subscriber in m_subscribers)
                        foreach (EventHandler<ChildEventArgs> listener in subscriber.GetChildRemovedHandlers())
                            yield return listener;
            }

            internal void Subscribe(DomNode subscriber)
            {
                if (m_subscribers == null)
                    m_subscribers = new List<DomNode>();

                m_subscribers.Add(subscriber);
            }

            internal void Unsubscribe(DomNode subscriber)
            {
                m_subscribers.Remove(subscriber);
                //if (m_subscribers.Count == 0)
                //    m_subscribers = null;
            }

            private readonly DomNode m_node;
            private List<DomNode> m_subscribers;
        }

        private void RaiseAttributeChanging(AttributeEventArgs e)
        {
            foreach (DomNode node in Lineage)
                if (node.m_eventHandlers != null)
                    node.m_eventHandlers.RaiseAttributeChanging(e);
        }

        internal IEnumerable<EventHandler<AttributeEventArgs>> GetAttributeChangingHandlers()
        {
            foreach (DomNode node in Lineage)
                if (node.m_eventHandlers != null)
                    foreach (var listener in node.m_eventHandlers.GetAttributeChangingHandlers())
                        yield return listener;
        }

        private void RaiseAttributeChanged(AttributeEventArgs e)
        {
            foreach (DomNode node in Lineage)
                if (node.m_eventHandlers != null)
                    node.m_eventHandlers.RaiseAttributeChanged(e);
        }

        internal IEnumerable<EventHandler<AttributeEventArgs>> GetAttributeChangedHandlers()
        {
            foreach (DomNode node in Lineage)
                if (node.m_eventHandlers != null)
                    foreach (var listener in node.m_eventHandlers.GetAttributeChangedHandlers())
                        yield return listener;
        }

        private void RaiseChildInserting(ChildEventArgs e)
        {
            foreach (DomNode node in Lineage)
                if (node.m_eventHandlers != null)
                    node.m_eventHandlers.RaiseChildInserting(e);
        }

        internal IEnumerable<EventHandler<ChildEventArgs>> GetChildInsertingHandlers()
        {
            foreach (DomNode node in Lineage)
                if (node.m_eventHandlers != null)
                    foreach (var listener in node.m_eventHandlers.GetChildInsertingHandlers())
                        yield return listener;
        }

        private void RaiseChildInserted(ChildEventArgs e)
        {
            foreach (DomNode node in Lineage)
                if (node.m_eventHandlers != null)
                    node.m_eventHandlers.RaiseChildInserted(e);
        }

        internal IEnumerable<EventHandler<ChildEventArgs>> GetChildInsertedHandlers()
        {
            foreach (DomNode node in Lineage)
                if (node.m_eventHandlers != null)
                    foreach (var listener in node.m_eventHandlers.GetChildInsertedHandlers())
                        yield return listener;
        }

        private void RaiseChildRemoving(ChildEventArgs e)
        {
            foreach (DomNode node in Lineage)
                if (node.m_eventHandlers != null)
                    node.m_eventHandlers.RaiseChildRemoving(e);
        }

        internal IEnumerable<EventHandler<ChildEventArgs>> GetChildRemovingHandlers()
        {
            foreach (DomNode node in Lineage)
                if (node.m_eventHandlers != null)
                    foreach (var listener in node.m_eventHandlers.GetChildRemovingHandlers())
                        yield return listener;
        }

        private void RaiseChildRemoved(ChildEventArgs e)
        {
            foreach (DomNode node in Lineage)
                if (node.m_eventHandlers != null)
                    node.m_eventHandlers.RaiseChildRemoved(e);
        }

        internal IEnumerable<EventHandler<ChildEventArgs>> GetChildRemovedHandlers()
        {
            foreach (DomNode node in Lineage)
                if (node.m_eventHandlers != null)
                    foreach (var listener in node.m_eventHandlers.GetChildRemovedHandlers())
                        yield return listener;
        }

        private NodeEventHandlers GetEventHandlers()
        {
            if (m_eventHandlers == null)
                m_eventHandlers = new NodeEventHandlers(this);

            return m_eventHandlers;
        }

        /// <summary>
        /// Tests for equality with another object</summary>
        /// <param name="obj">Other object</param>
        /// <returns>True iff the DomNode equals the other object; equality means that
        /// the other object is the same as this DomNode or an adapter attached to it</returns>
        public override bool Equals(object obj)
        {
            DomNode node = obj as DomNode;
            if (node == null)
            {
                IAdapter adapter = obj.As<IAdapter>();
                if (adapter != null)
                    node = adapter.Adaptee as DomNode;
            }

            return object.ReferenceEquals(this, node);
        }

        /// <summary>
        /// Generates a hash code for the instance</summary>
        /// <returns>Hash code for the instance</returns>
        public override int GetHashCode()
        {
            // override is to suppress compiler warning, but the base method does what we want
            return base.GetHashCode();
        }

        private NodeList.ChildList GetChildListObject(ChildInfo childInfo)
        {
            int index = m_type.GetDataIndex(childInfo);
            return m_data[index] as NodeList.ChildList;
        }

        private void SetChildListObject(ChildInfo childInfo, NodeList.ChildList list)
        {
            int index = m_type.GetDataIndex(childInfo);
            m_data[index] = list;
        }

        private void SetParent(DomNode parent, ChildInfo childInfo)
        {
            m_parent = parent;
            m_childInfo = childInfo;
        }

        /// <summary>
        /// Gets string representation of DomNode</summary>
        /// <returns>String representation of DomNode</returns>
        public override string ToString()
        {
            if (m_type != null)
                return string.Format("0x{0:x}, {1}", GetHashCode(), m_type);

            return base.ToString();
        }

        /// <summary>
        /// Gets the DebugInfo object which is useful for analyzing this DomNode in Visual Studio's debugger.
        /// Naming this with an '_' puts it first in the list of properties.</summary>
        // ReSharper disable once UnusedMember.Local
        private DomNodeDebugger _DebugInfo
        {
            get { return new DomNodeDebugger(this); }
        }

        internal static event EventHandler<ChildEventArgs> DiagnosticChildInserted;
        internal static event EventHandler<ChildEventArgs> DiagnosticChildRemoved;
        internal static event EventHandler<AttributeEventArgs> DiagnosticAttributeChanged;

        private readonly DomNodeType m_type;
        private DomNode m_parent;
        private ChildInfo m_childInfo;
        private readonly object[] m_data;
        private NodeEventHandlers m_eventHandlers;

        // The properties of this class are designed to appear in the Visual Studio debugger in
        //  a useful way. For example, IList<> is more useful than IEnumerable<> in the debugger view.
        private class DomNodeDebugger
        {
            public DomNodeDebugger(DomNode node)
            {
                m_node = node;
            }

            public IList<AttributeDebugger> Attributes
            {
                get { return m_node.Type.Attributes.Select(attrInfo => new AttributeDebugger(attrInfo, m_node.GetAttribute(attrInfo))).ToList(); }
            }

            public IList<ChildDebugger> Children
            {
                get { return m_node.Children.Select(child => new ChildDebugger(child.ChildInfo, child)).ToList(); }
            }

            public IList<object> Extensions
            {
                get
                {
                    var extensions = new List<object>();
                    for( int i = m_node.Type.FirstExtensionIndex; i < m_node.Type.FieldCount; i++)
                        extensions.Add(m_node.m_data[i]);
                    return extensions;
                }
            }

            public IList<ListenerDebugger> AttributeChangingListeners
            {
                get { return m_node.GetAttributeChangingHandlers().Select(listener => new ListenerDebugger(listener)).ToList(); }
            }

            public IList<ListenerDebugger> AttributeChangedListeners
            {
                get { return m_node.GetAttributeChangedHandlers().Select(listener => new ListenerDebugger(listener)).ToList(); }
            }

            public IList<ListenerDebugger> ChildInsertingListeners
            {
                get { return m_node.GetChildInsertingHandlers().Select(listener => new ListenerDebugger(listener)).ToList(); }
            }

            public IList<ListenerDebugger> ChildInsertedListeners
            {
                get { return m_node.GetChildInsertedHandlers().Select(listener => new ListenerDebugger(listener)).ToList(); }
            }

            public IList<ListenerDebugger> ChildRemovingListeners
            {
                get { return m_node.GetChildRemovingHandlers().Select(listener => new ListenerDebugger(listener)).ToList(); }
            }

            public IList<ListenerDebugger> ChildRemovedListeners
            {
                get { return m_node.GetChildRemovedHandlers().Select(listener => new ListenerDebugger(listener)).ToList(); }
            }

            public override string ToString()
            {
                return "Additional debug info";
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly DomNode m_node;
        }

        [DebuggerDisplay("{Value}", Name = "{AttributeInfo}")]
        private class AttributeDebugger
        {
            public AttributeDebugger(AttributeInfo info, object value)
            {
                AttributeInfo = info;
                Value = value;
            }

            public readonly AttributeInfo AttributeInfo;
            public readonly object Value;
        }

        [DebuggerDisplay("{Child}", Name = "{ChildInfo}")]
        private class ChildDebugger
        {
            public ChildDebugger(ChildInfo info, DomNode child)
            {
                ChildInfo = info;
                Child = child;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public readonly ChildInfo ChildInfo; //is visible inside the child; no need to show it at top-level.
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public readonly DomNode Child; //no need to show "Child" property; go straight to DomNode members.
        }

        [DebuggerDisplay("{Target}")]
        private class ListenerDebugger
        {
            public ListenerDebugger(Delegate d)
            {
                Method = d.Method;
                Target = d.Target;
            }

            public readonly MethodInfo Method;
            public readonly object Target;
        }

        // Lightweight List Support

        private class NodeList : IList<DomNode>
        {
            public NodeList(DomNode node, ChildInfo childInfo)
            {
                m_node = node;
                m_childInfo = childInfo;
            }

            // IList<DomNode> Members

            public int IndexOf(DomNode item)
            {
                IList<DomNode> list = GetList();
                if (list == null)
                    return -1;

                return list.IndexOf(item);
            }

            public void Insert(int index, DomNode item)
            {
                // we must un-parent the item before it can be added to our node
                if (item.m_parent != null)
                {
                    // if the item was already parented to our node, we may have to adjust the index
                    if (item.m_parent == m_node)
                    {
                        int currentIndex = IndexOf(item);
                        if (currentIndex < index)
                            index--;
                    }
                    item.RemoveFromParent();
                }

                IList<DomNode> list = GetOrCreateList();
                list.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                IList<DomNode> list = GetListForIndexing();
                list.RemoveAt(index);
                DestroyListIfEmpty();
            }

            public DomNode this[int index]
            {
                get
                {
                    IList<DomNode> list = GetListForIndexing();
                    return list[index];
                }
                set
                {
                    IList<DomNode> list = GetListForIndexing();
                    list[index] = value;
                }
            }

            // ICollection<DomNode> Members

            public void Add(DomNode item)
            {
                IList<DomNode> list = this;
                list.Insert(list.Count, item);
            }

            public void Clear()
            {
                DestroyList();
            }

            public bool Contains(DomNode item)
            {
                IList<DomNode> list = GetList();
                if (list == null)
                    return false;

                return list.Contains(item);
            }

            public void CopyTo(DomNode[] array, int arrayIndex)
            {
                IList<DomNode> list = GetList();
                if (list != null)
                    list.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get
                {
                    IList<DomNode> list = GetList();
                    if (list == null)
                        return 0;

                    return list.Count;
                }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(DomNode item)
            {
                IList<DomNode> list = GetList();
                if (list != null)
                {
                    bool result = list.Remove(item);
                    DestroyListIfEmpty();
                    return result;
                }

                return false;
            }

            // IEnumerable<DomNode> Members

            public IEnumerator<DomNode> GetEnumerator()
            {
                return new Enumerator(this);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private IList<DomNode> GetOrCreateList()
            {
                IList<DomNode> result = GetList();
                if (result == null)
                    result = CreateList();

                return result;
            }

            private IList<DomNode> GetListForIndexing()
            {
                IList<DomNode> result = GetList();
                if (result == null)
                    throw new IndexOutOfRangeException("List is empty");

                return result;
            }

            private void DestroyListIfEmpty()
            {
                IList<DomNode> list = GetList();
                if (list != null && list.Count == 0)
                    DestroyList();
            }

            private DomNode Convert(object value)
            {
                DomNode node = value as DomNode;
                if (node == null)
                    throw new ArgumentException("item must be DomNode");

                return node;
            }

            private class Enumerator : IEnumerator<DomNode>
            {
                public Enumerator(NodeList owner)
                {
                    m_owner = owner;
                    IList<DomNode> list = m_owner.GetList();

                    if (list != null)
                        m_enumerator = list.GetEnumerator();
                }

                // IEnumerator<DomNode> Members

                public DomNode Current
                {
                    get
                    {
                        if (m_enumerator != null)
                            return m_enumerator.Current;

                        throw new InvalidOperationException("No current element");
                    }
                }

                // IDisposable Members

                public void Dispose()
                {
                    if (m_enumerator != null)
                        m_enumerator.Dispose();
                }

                // IEnumerator Members

                object System.Collections.IEnumerator.Current
                {
                    get { return Current; }
                }

                public bool MoveNext()
                {
                    if (m_enumerator != null)
                        return m_enumerator.MoveNext();

                    // check here if collection was modified by adding one or more elements
                    if (m_owner.GetList() != null)
                        throw new InvalidOperationException("Underlying collection was modified");

                    return false;
                }

                public void Reset()
                {
                    if (m_enumerator != null)
                        m_enumerator.Reset();
                }

                private readonly NodeList m_owner;
                private readonly IEnumerator<DomNode> m_enumerator;
            }

            private IList<DomNode> CreateList()
            {
                ChildList list = new ChildList(m_node, m_childInfo);
                m_node.SetChildListObject(m_childInfo, list);
                return list;
            }

            private IList<DomNode> GetList()
            {
                return m_node.GetChildListObject(m_childInfo);
            }

            private void DestroyList()
            {
                // if the list exists, we need to clear it to generate remove events
                ChildList list = m_node.GetChildListObject(m_childInfo);
                if (list != null)
                {
                    list.Clear();
                    m_node.SetChildListObject(m_childInfo, null);
                }
            }

            public override string ToString()
            {
                return string.Format("Count = {0}", Count);
            }

            private readonly DomNode m_node;
            private readonly ChildInfo m_childInfo;

            // ChildList Class

            // IList<DomNode> implementation for general child DomNode lists
            public class ChildList : Collection<DomNode>
            {
                public ChildList(DomNode node, ChildInfo childInfo)
                {
                    m_node = node;
                    m_childInfo = childInfo;
                }

                protected override void InsertItem(int index, DomNode item)
                {
                    if (item == null)
                        throw new ArgumentNullException("item");

                    DomNode parent = m_node;
                    var e = new ChildEventArgs(parent, m_childInfo, item, index);

                    parent.RaiseChildInserting(e);
                    base.InsertItem(index, item);
                    item.SetParent(parent, m_childInfo);
                    DiagnosticChildInserted.Raise(item, e);
                    parent.RaiseChildInserted(e);
                }

                protected override void RemoveItem(int index)
                {
                    DomNode item = this[index];

                    DomNode parent = m_node;
                    var e = new ChildEventArgs(parent, m_childInfo, item, index);

                    parent.RaiseChildRemoving(e);
                    base.RemoveItem(index);
                    item.SetParent(null, null);
                    DiagnosticChildRemoved.Raise(item, e);
                    parent.RaiseChildRemoved(e);
                }

                protected override void SetItem(int index, DomNode item)
                {
                    // implement set by removing and inserting so we don't have to interleave remove
                    //  and insert, or have intermediate states with null items
                    RemoveItem(index);
                    InsertItem(index, item);
                }

                protected override void ClearItems()
                {
                    // not very efficient, but greatly simplifies the implementation
                    while (Count > 0)
                        RemoveAt(Count - 1);
                }

                private readonly DomNode m_node;
                private readonly ChildInfo m_childInfo;
            }
        }
    }
}
