//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Abstract base class for assets that can be rendered. For example, each node in an
    /// ATGI model has one of these interfaces attached. Multiple
    /// instances of a model in the Design View (a complex schema type derived from gameObjectType)
    /// refer to the same RenderObject. The RenderObject is asked to build
    /// TraverseNodes (in the Traverse method) and TraverseNodes is used to call
    /// back into the RenderObject's Dispatch().</summary>
    public abstract class RenderObject : DomNodeAdapter, IRenderObject
    {
        #region IBuildSceneNode

        /// <summary>
        /// Gets whether this object should generate a DomConstraint and a
        /// IRenderObject for the SceneNode that was created by SceneGraphBuilder. If this
        /// property is false, the children will still be evaluated separately.</summary>
        public virtual bool CreateByGraphBuilder
        {
            get { return true; }
        }

        /// <summary>
        /// Called by SceneGraphBuilder upon building the node, if CreateByGraphBuilder is
        /// true and after the DomConstraint and IRenderObject have been attached to the
        /// SceneNode.</summary>
        /// <param name="node">Scene node that was built</param>
        public virtual void OnBuildNode(SceneNode node)
        {
        }
        #endregion

        #region IRenderObject Members

        /// <summary>
        /// Initializes the RenderObject and allocates all resources</summary>
        /// <param name="node">The SceneNode corresponding to this render object</param>
        /// <returns>True if initialization was successful</returns>
        public virtual bool Init(SceneNode node)
        {
            return true;
        }

        /// <summary>
        /// Performs any traverse actions</summary>
        /// <param name="graphPath">The path leading to the RenderObject</param>
        /// <param name="renderAction">The render action used to dispatch the object</param>
        /// <param name="camera">The Camera used</param>
        /// <param name="traverseList">The traverse list used in the dispatch phase.
        /// RenderObjects participating in the dispatch phase need to push themselves onto the traverse list.</param>
        /// <returns>The TraverseState</returns>
        public virtual TraverseState Traverse(Stack<SceneNode> graphPath, IRenderAction renderAction, Camera camera, ICollection<TraverseNode> traverseList)
        {
            RenderState state = renderAction.RenderState;

            Box boundingBox = null;

            bool alpha = ((state.RenderMode & RenderMode.Alpha) != 0);
            if (alpha)
            {
                boundingBox = GetBoundingBoxObjectSpace();
                boundingBox.Transform(renderAction.TopMatrix);
            }

            if ((state.RenderMode & RenderMode.Smooth) != 0)
            {
                RenderMode origRenderMode = state.RenderMode;
                state.RenderMode &= ~(RenderMode.Wireframe | RenderMode.WireframeColor);

                TraverseNode node = renderAction.GetUnusedNode();
                node.Init(renderAction.RenderObject, renderAction.TopMatrix, graphPath, state);

                if (alpha)
                    node.WorldSpaceBoundingBox = boundingBox;

                traverseList.Add(node);
                state.RenderMode = origRenderMode;
            }

            if ((state.RenderMode & RenderMode.Wireframe) != 0)
            {
                RenderMode origRenderMode = state.RenderMode;
                state.RenderMode &= ~(RenderMode.Smooth | RenderMode.SolidColor |
                    RenderMode.Textured | RenderMode.Lit);

                TraverseNode node = renderAction.GetUnusedNode();
                node.Init(renderAction.RenderObject, renderAction.TopMatrix, graphPath, state);

                if (alpha)
                    node.WorldSpaceBoundingBox = boundingBox;

                traverseList.Add(node);
                state.RenderMode = origRenderMode;
            }

            return renderAction.TraverseState;
        }

        /// <summary>
        /// Performs any post-traverse actions</summary>
        /// <param name="graphPath">The path leading to the RenderObject</param>
        /// <param name="renderAction">The render action used to dispatch the object</param>
        /// <param name="camera">The Camera used</param>
        /// <param name="traverseList">The traverse list used in the dispatch phase</param>
        /// <returns>The TraverseState</returns>
        public virtual TraverseState PostTraverse(Stack<SceneNode> graphPath, IRenderAction renderAction, Camera camera, ICollection<TraverseNode> traverseList)
        {
            return TraverseState.Continue;
        }

        /// <summary>
        /// Dispatches the RenderObject for rendering</summary>
        /// <param name="graphPath">The path leading to the RenderObject</param>
        /// <param name="renderState">The render state to use</param>
        /// <param name="renderAction">The render action dispatching the object</param>
        /// <param name="camera">The Camera used in the dispatch process</param>
        public virtual void Dispatch(SceneNode[] graphPath, 
                                     RenderState renderState, 
                                     IRenderAction renderAction,
                                     Camera camera)
        {
            renderAction.RenderStateGuardian.Commit(renderState);
            Render(graphPath, renderState, renderAction, camera);
        }

        /// <summary>
        /// Releases all RenderObject resources. Derived classes must call base.Release().</summary>
        public virtual void Release()
        {
        }

        /// <summary>
        /// Gets a collection of other render object interface types this class depends on.
        /// This determines the order of render objects on the SceneNode.</summary>
        /// <returns>Collection of other render object interface types this class depends on</returns>
        public virtual Type[] GetDependencies()
        {
            return new Type[] { typeof(ISetsLocalTransform) };
        }

        /// <summary>
        /// Gets the IIntersectable implementor to be used for picking and snapping. If null,
        /// picking and snapping (if supported at all) are done using OpenGL render-picking.</summary>
        /// <returns>The correct IIntersectable or null, if there is none</returns>
        /// <remarks>There are situations where multiple classes can implement IIntersectable
        /// for a particular adaptable object (like DomNode). This method allows the IRenderObject
        /// to choose the correct one.</remarks>
        public virtual IIntersectable GetIntersectable()
        {
            // See if we've done this expensive search already.
            if (m_intersectableCached)
                return m_intersectable;

            m_intersectableCached = true;
            m_intersectable = Adapters.As<IIntersectable>(this);
            if (m_intersectable != null && !m_intersectable.CanIntersect)
                m_intersectable = null;

            // If we were to return an IIntersectable on a different RenderObject, the
            //  intersection test probably won't be valid. For example, if RenderObject A
            //  is the TranslateManipulator and it does not implement IIntersectable,
            //  but RenderObject B is a RenderSubMesh which does, and both A & B
            //  are attached to the same object, we can't return the IIntersectable
            //  of the RenderSubMesh when requested on a TranslateManipulator.
            if (m_intersectable != null)
            {
                IRenderObject other = m_intersectable as IRenderObject;
                if (other != null && other != this as IRenderObject)
                {
                    m_intersectable = null;
                }
            }

            return m_intersectable;
        }

        #endregion

        /// <summary>
        /// Gets the object space bounding box</summary>
        /// <returns>Object space bounding box</returns>
        protected virtual Box GetBoundingBoxObjectSpace()
        {
            return new Box(new Vec3F(0, 0, 0), new Vec3F(0, 0, 0));
        }

        /// <summary>
        /// Renders the object. Called from Dispatch().</summary>
        /// <param name="graphPath">The path leading to the RenderObject</param>
        /// <param name="renderState">The render state to use</param>
        /// <param name="renderAction">The render action used</param>
        /// <param name="camera">The camera</param>
        protected virtual void Render(
            SceneNode[] graphPath,
            RenderState renderState,
            IRenderAction renderAction, 
            Camera camera)
        {
        }

        /// <summary>
        /// Cached look-up, because this can be expensive to calculate for thousands of
        /// objects that are drawn each frame</summary>
        bool m_intersectableCached;
        private IIntersectable m_intersectable;
    }
}
