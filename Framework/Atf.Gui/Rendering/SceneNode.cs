//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// A node in the scene graph. SceneNodes hold all of the render objects and constraints
    /// that are associated with a source object. SceneNodes are arranged in a graph that determines
    /// which objects to render, and in what traversal order.</summary>
    public class SceneNode
    {
        /// <summary>
        /// Constructs a SceneNode for the given source object</summary>
        /// <param name="source">Underlying source object (e.g., a DomNode)</param>
        public SceneNode(object source)
        {
            m_sourceObject = source;
        }

        /// <summary>
        /// Constructs a SceneNode that is a copy of the given SceneNode</summary>
        /// <param name="original">SceneNode to copy</param>
        /// <param name="copyChildren">Whether children are copied as well. Currently must be false.</param>
        public SceneNode(SceneNode original, bool copyChildren)
        {
            m_sourceObject = original.m_sourceObject;

            LinkedListNode<IRenderObject> renderObjectNode = original.m_renderObjects.InternalList.First;
            while (renderObjectNode != null)
            {
                m_renderObjects.InternalList.AddLast(renderObjectNode.Value);
                renderObjectNode = renderObjectNode.Next;
            }

            if (copyChildren)
                throw new NotImplementedException("'copyChildren' must be false currently");

            m_visible = original.m_visible;
        }

        /// <summary>
        /// Gets the underlying object as a DomNode, using the ATF adapters</summary>
        public DomNode DomNode
        {
            get { return m_sourceObject.As<DomNode>(); }
        }

        /// <summary>
        /// Gets the source object that this SceneNode represents</summary>
        public object Source
        {
            get { return m_sourceObject; }
        }

        /// <summary>
        /// Gets the collection of render objects associated with this SceneNode.
        /// Adding to this collection sorts by type dependency by using
        /// IRenderObject.GetDependencies().</summary>
        public ICollection<IRenderObject> RenderObjects
        {
            get { return m_renderObjects; }
        }

        /// <summary>
        /// Gets the child SceneNodes</summary>
        public IList<SceneNode> Children
        {
            get { return m_children; }
        }

        /// <summary>
        /// Gets the render state stack for the SceneNode</summary>
        public RenderStateStack StateStack
        {
            get { return m_stateStack; }
        }

        /// <summary>
        /// Gets or sets the SceneNode's visibility</summary>
        public bool IsVisibile
        {
            get { return m_visible; }
            set { m_visible = value; }
        }

        /// <summary>
        /// Finds a SceneNode for a given source object. Performs a hierarchical search under
        /// this SceneNode.</summary>
        /// <param name="source">Source object to search for</param>
        /// <returns>The SceneNode, or null if it wasn't found</returns>
        public SceneNode FindNode(object source)
        {
            if (source == null)
                return null;

            Queue<SceneNode> queue = new Queue<SceneNode>();
            queue.Enqueue(this);
            return FindInternal(source, queue);
        }

        /// <summary>
        /// Finds a SceneNode according to a given source object path. Performs a hierarchical search
        /// under this SceneNode.</summary>
        /// <param name="path">The search path</param>
        /// <returns>The SceneNode associated with the last object in the path, or null if no
        /// object was found</returns>
        public SceneNode FindNode(Path<object> path)
        {
            SceneNode current = this;
            foreach (object pathObject in path)
            {
                SceneNode temp = current.FindNode(pathObject);
                if (temp != null)
                    current = temp;
            }

            return (current.Source == path.Last) ? current : null;
        }

        /// <summary>
        /// Finds a SceneNode path corresponding to a given source object path. Performs a hierarchical
        /// search under this SceneNode.</summary>
        /// <param name="obj">Source object to search for</param>
        /// <returns>The path if found</returns>
        public SceneNode[] FindNodePath(object obj)
        {
            List<SceneNode> path = new List<SceneNode>();
            FindNodePathInternal(obj, path);

            return path.ToArray();
        }

        /// <summary>
        /// Adds a render object of the given generic type to the render object list</summary>
        /// <typeparam name="T">The type of render object to add</typeparam>
        /// <param name="addOnSubTree">Whether to add recusively on sub tree</param>
        /// <returns>True if the render object was successfully added</returns>
        public bool AddRenderObject<T>(bool addOnSubTree)
            where T : class, IRenderObject
        {
            bool appended = false;
            if (m_sourceObject != null)
            {
                //IRenderObject renderObject = m_sourceObject.CreateInterface<T>();
                IRenderObject renderObject = m_sourceObject.As<T>();
                if (renderObject != null)
                {
                    m_renderObjects.Add(renderObject);
                    appended = true;
                }
            }

            if (addOnSubTree)
            {
                foreach (SceneNode child in m_children)
                    child.AddRenderObject<T>(addOnSubTree);
            }

            return appended;
        }

        /// <summary>
        /// Removes all instances of the given render object type from the render object list</summary>
        /// <param name="interfaceType">The type of render object to remove</param>
        /// <param name="removeOnSubTree">Whether to remove recursively on sub tree</param>
        public void RemoveRenderObject(Type interfaceType, bool removeOnSubTree)
        {
            LinkedListNode<IRenderObject> current = m_renderObjects.InternalList.First;
            while (current != null)
            {
                LinkedListNode<IRenderObject> next = current.Next;
                if (interfaceType.IsAssignableFrom(current.Value.GetType()))
                    m_renderObjects.InternalList.Remove(current);
                current = next;
            }

            if (removeOnSubTree)
            {
                foreach (SceneNode child in m_children)
                    child.RemoveRenderObject(interfaceType, removeOnSubTree);
            }
        }

        /// <summary>
        /// Removes all render objects and constraints from this SceneNode</summary>
        public void Clear()
        {
            foreach (IRenderObject renderObject in m_renderObjects)
                renderObject.Release();

            m_renderObjects.Clear();
        }

        /// <summary>
        /// Removes all render objects, constraints and children from this SceneNode
        /// and all of its descendants</summary>
        public void ClearSubGraph()
        {
            Clear();

            foreach (SceneNode child in m_children)
                child.ClearSubGraph();

            m_children.Clear();
        }

        private SceneNode FindInternal(object source, Queue<SceneNode> queue)
        {
            SceneNode result = null;
            while (queue.Count > 0 && result == null)
            {
                SceneNode current = queue.Dequeue();
                if (current.m_sourceObject == source)
                    return current;

                foreach (SceneNode child in current.m_children)
                    queue.Enqueue(child);
            }

            return null;
        }

        private void FindNodePathInternal(object source, List<SceneNode> path)
        {
            if (m_sourceObject == source)
            {
                path.Add(this);
            }
            else
            {
                bool found = false;
                foreach (SceneNode child in Children)
                {
                    child.FindNodePathInternal(source, path);
                    if (path.Count > 0)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                    path.Add(this);
            }
        }

        private readonly object m_sourceObject;
        private readonly RenderObjectList m_renderObjects = new RenderObjectList();
        private readonly List<SceneNode> m_children = new List<SceneNode>();
        private readonly RenderStateStack m_stateStack = new RenderStateStack();
        private bool m_visible = true;
    }
}
