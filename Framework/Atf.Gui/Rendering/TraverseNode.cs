//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Class for encapsulating the rendering state for each IRenderObject. These objects
    /// are the nodes in the traverse list. Each IRenderObject is responsible for
    /// creating a TraverseNode instance and adding it to the traverse list, if it
    /// wishes to be dispatched. This must be done in the IRenderObject.Traverse method.</summary>
    public class TraverseNode
    {
        /// <summary>
        /// Constructor</summary>
        public TraverseNode()
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="renderObject">RenderObject. A reference is held to this, so that IRenderObject.Dispatch
        /// can be called.</param>
        /// <param name="transform">Transform to use when dispatching the RenderObject; a reference is held</param>
        /// <param name="graphPath">Graph path leading to RenderObject; a reference is held</param>
        /// <param name="renderState">RenderState to use for RenderObject; is copied</param>
        public TraverseNode(
            IRenderObject renderObject,
            Matrix4F transform,
            Stack<SceneNode> graphPath,
            RenderState renderState)
        {
            Init(renderObject, transform, graphPath, renderState);
        }

        /// <summary>
        /// Initializes instance</summary>
        /// <param name="renderObject">RenderObject. A reference is held to this, so that IRenderObject.Dispatch
        /// can be called.</param>
        /// <param name="transform">Transform to use when dispatching the RenderObject; a reference is held</param>
        /// <param name="graphPath">Graph path leading to RenderObject; a reference is held</param>
        /// <param name="renderState">RenderState to use for RenderObject; is copied</param>
        public void Init(
            IRenderObject renderObject,
            Matrix4F transform,
            Stack<SceneNode> graphPath,
            RenderState renderState)
        {
            m_renderObject = renderObject;
            m_transform = transform;
            m_renderState.Init(renderState);
            m_graphPath = graphPath.ToArray();//faster to create every time than to cache!
        }

        /// <summary>
        /// Clears references to other objects. This is important for preventing memory leaks.</summary>
        public void Reset()
        {
            m_renderObject = null;
            m_transform = null;
            m_graphPath = null;
        }

        /// <summary>
        /// Gets and sets the encapsulated render object</summary>
        public IRenderObject RenderObject
        {
            get { return m_renderObject; }
            set { m_renderObject = value; }
        }

        /// <summary>
        /// Gets and sets the Transform to use when dispatching the render object.
        /// The matrix is not copied on either set or get.</summary>
        public Matrix4F Transform
        {
            get { return m_transform; }
            set { m_transform = value; }
        }

        /// <summary>
        /// Gets and sets the graph path leading to the RenderObject.
        /// The array is not copied on either set or get.</summary>
        public SceneNode[] GraphPath
        {
            get { return m_graphPath; }
            set { m_graphPath = value; }
        }

        /// <summary>
        /// Gets and sets the RenderState to use for the RenderObject.
        /// The RenderState is not copied on either set or get.</summary>
        public RenderState RenderState
        {
            get { return m_renderState; }
            set { m_renderState = value; }
        }

        /// <summary>
        /// Gets and sets the bounding box, in world coordinates</summary>
        public Box WorldSpaceBoundingBox
        {
            get { return m_worldSpaceBoundingBox; }
            set { m_worldSpaceBoundingBox = value; }
        }

        private IRenderObject m_renderObject;
        private Matrix4F m_transform;
        private SceneNode[] m_graphPath;
        private RenderState m_renderState = new RenderState();
        private Box m_worldSpaceBoundingBox;
    }
}

