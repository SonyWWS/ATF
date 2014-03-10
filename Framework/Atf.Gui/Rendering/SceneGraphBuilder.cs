//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Class that can build a scene graph from a root source object, typically a DomNode</summary>
    public class SceneGraphBuilder
    {
        /// <summary>
        /// Constructs a SceneGraphBuilder that builds any IRenderObject and that works
        /// with DomNodes. If the source objects are not DomNodes, use the constructor
        /// that takes an ITreeView.</summary>
        public SceneGraphBuilder()
            : this(typeof(IRenderObject), new DomNodeTreeView())
        {
        }

        /// <summary>
        /// Constructs a SceneGraphBuilder that builds only the specified type of
        /// IRenderObjects and that adapts DomNodes</summary>
        /// <param name="type">The render object type to use</param>
        public SceneGraphBuilder(Type type)
            : this(type, new DomNodeTreeView())
        {
        }

        /// <summary>
        /// Constructs a SceneGraphBuilder that builds only the specified type of
        /// IRenderObjects and uses the given ITreeView to find children</summary>
        /// <param name="type">The render object type to use</param>
        /// <param name="treeView">Scene's tree view</param>
        public SceneGraphBuilder(Type type, ITreeView treeView)
        {
            if (!typeof(IRenderObject).IsAssignableFrom(type))
                throw new InvalidOperationException("Must be an IRenderObject");

            m_treeView = treeView;
            m_type = type;
        }

        /// <summary>
        /// Builds scene graph recursively from the given source object</summary>
        /// <param name="source">The root source object (e.g., a DomNode)</param>
        /// <param name="parent">Parent SceneNode to which to attach the built sub graph</param>
        /// <returns>The built sub graph root</returns>
        public SceneNode Build(object source, SceneNode parent)
        {
            bool cancelLoad = false;
            return BuildInternal(source, parent, true, ref cancelLoad);
        }
        
        /// <summary>
        /// Builds scene graph recursively from the given source object</summary>
        /// <param name="source">The root source object (e.g., a DomNode)</param>
        /// <param name="parent">Parent SceneNode to which to attach the built sub graph</param>
        /// <param name="cancelLoad">A reference to a bool that can be set by another thread
        /// (like from the progress dialog box) to cancel building</param>
        /// <returns>The built sub graph root</returns>
        public SceneNode Build(object source, SceneNode parent, ref bool cancelLoad)
        {
            return BuildInternal(source, parent, true, ref cancelLoad);
        }

        /// <summary>
        /// Builds a single SceneNode from the given source object</summary>
        /// <param name="source">Source object (e.g., DomNode)</param>
        /// <param name="parent">SceneNode to parent the built node</param>
        /// <returns>The built node</returns>
        public SceneNode BuildNode(object source, SceneNode parent)
        {
            return BuildNodeInternal(source, parent, false);
        }

        private SceneNode BuildInternal(object source, SceneNode parent, bool forceBuild,
            ref bool cancelLoad)
        {
            SceneNode node = BuildNodeInternal(source, parent, forceBuild);

            if (node == null)
                return parent;

            if (cancelLoad)
                return node;

            lock (s_lock)
            {
                // We must catch cyclic graphs to avoid infinite recursion.
                if (s_ancestors.Contains(source))
                    return node;
                try
                {
                    s_ancestors.Add(source);

                    ISceneGraphHierarchy modelNode = source.As<ISceneGraphHierarchy>();
                    IEnumerable<object> children = (modelNode != null) ? modelNode.GetChildren() : m_treeView.GetChildren(source);
                    
                    foreach (object child in children )
                        BuildInternal(child, node, false, ref cancelLoad);
                }
                finally
                {
                    s_ancestors.Remove(source);
                }
            }

            return node;
        }

        /// <summary>
        /// Builds just this SceneNode without traversing any children of the source object</summary>
        /// <param name="source">The source object to be represented by the new SceneNode</param>
        /// <param name="parent">The parent that the new SceneNode will be attached to</param>
        /// <param name="forceBuild">If true, even if there are no constraints and no renderable objects,
        /// the SceneNode is still created</param>
        /// <returns>Either:
        /// 1. A new SceneNode, attached to 'parent'. The child objects should be traversed.
        /// 2. 'parent', which means that this source object had no useful info and 'forceBuild' was false.
        /// The child objects should be traversed.
        /// 3. null if this source object is an object reference and that reference should not be
        /// rendered, so the caller must not traverse the children of this source object.</returns>
        private SceneNode BuildNodeInternal(object source, SceneNode parent, bool forceBuild)
        {
            // Check if source object is an object reference and if it's renderable.
            if (parent.Source != null)
            {
                IRenderableParent renderableParent = parent.Source.As<IRenderableParent>();
                if (renderableParent != null)
                {
                    if (renderableParent.IsRenderableChild(source) == false)
                        return null;
                }
            }

            SceneNode node = new SceneNode(source);

            // Next build RenderObjects 
            foreach (IBuildSceneNode buildNode in source.AsAll<IBuildSceneNode>())
            {
                if (buildNode.CreateByGraphBuilder)
                {
                    // Build render objects
                    IRenderObject renderObject = buildNode as IRenderObject;
                    if (renderObject != null)
                    {
                        if (renderObject.Init(node))
                        {
                            // Add to node which will sort by type dependency.
                            node.RenderObjects.Add(renderObject);
                        }
                    }

                    // Call the OnBuildNode method
                    buildNode.OnBuildNode(node);
                }
            }

            if (node.RenderObjects.Count > 0 
                || node.Source.Is<IBoundable>() 
                || forceBuild)
            {
                if (parent != null)
                    parent.Children.Add(node);
            }
            else
            {
                node = parent;
            }

            return node;
        }

        // Private class to provide the default way of enumerating a DomNode's children. This
        //  implementation of ITreeView will be used by default, with the assumption that the
        //  default source objects will be DomNodes.
        private class DomNodeTreeView : ITreeView
        {
            #region ITreeView Members

            public object Root
            {
                get { return null; }
            }

            /// <summary>
            /// Returns enumeration of children of DomNode</summary>
            /// <param name="parent">Parent</param>
            /// <returns>Enumeration of children</returns>
            public IEnumerable<object> GetChildren(object parent)
            {
                DomNode node = parent as DomNode;
                if (node != null)
                {
                    foreach (DomNode child in node.Children)
                        yield return child;
                }
            }

            #endregion
        }

        private readonly ITreeView m_treeView;
        private Type m_type;
        private static readonly object s_lock = new object();//to control access to the static members
        private static readonly HashSet<object> s_ancestors = new HashSet<object>();
    }
}
