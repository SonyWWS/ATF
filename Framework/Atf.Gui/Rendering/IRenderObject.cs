//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Interface for renderable objects. There are typically multiple instances of
    /// this interface on a DomNode. To control their order, consider implementing
    /// IBeforeLocalTransform or ISetsLocalTransform, and returning the correct types from
    /// GetDependencies(), so that they are sorted correctly by RenderObjectList.</summary>
    public interface IRenderObject : IBuildSceneNode
    {
        /// <summary>
        /// Initializes the RenderObject and allocates all resources</summary>
        /// <param name="node">The SceneNode corresponding to this render object</param>
        /// <returns>True if initialization was successful</returns>
        bool Init(SceneNode node);

        /// <summary>
        /// Performs any traverse actions, such as placing one or more TraverseNodes onto
        /// the traverse list for a future call to Dispatch</summary>
        /// <param name="graphPath">The path leading to the RenderObject</param>
        /// <param name="renderAction">The RenderAction used to dispatch the object</param>
        /// <param name="camera">The camera used</param>
        /// <param name="traverseList">The traverse list used in the dispatch phase.
        /// RenderObjets participating in the dispatch phase need to push themselves
        /// onto the traverse list. Consider using renderAction.GetUnusedNode() to
        /// get a TraverseNode to place onto the traverse list.</param>
        /// <returns>The TraverseState</returns>
        TraverseState Traverse(
            Stack<SceneNode> graphPath,
            IRenderAction renderAction,
            Camera camera,
            ICollection<TraverseNode> traverseList);

        /// <summary>
        /// Performs any post-traverse actions, such as making sure to undo changes to renderAction's
        /// matrix stack that were done in the Traverse call</summary>
        /// <param name="graphPath">The path leading to the RenderObject</param>
        /// <param name="renderAction">The RenderAction used to dispatch the object</param>
        /// <param name="camera">The camera used</param>
        /// <param name="traverseList">The traverse list used in the dispatch phase</param>
        /// <returns>The TraverseState</returns>
        TraverseState PostTraverse(
            Stack<SceneNode> graphPath,
            IRenderAction renderAction,
            Camera camera,
            ICollection<TraverseNode> traverseList);

        /// <summary>
        /// Dispatches the RenderObject for rendering. The parameters come from the TraverseNode
        /// that was created in the earlier Traverse() phase.</summary>
        /// <param name="graphPath">The path leading to the RenderObject</param>
        /// <param name="renderState">The render state to use</param>
        /// <param name="renderAction">The render action dispatching the object</param>
        /// <param name="camera">The camera used in the dispatch process</param>
        void Dispatch(
            SceneNode[] graphPath,
            RenderState renderState,
            IRenderAction renderAction,
            Camera camera);

        /// <summary>
        /// Releases all RenderObject resources</summary>
        void Release();

        /// <summary>
        /// Gets a collection of other render object interface types this class depends on.
        /// This determines the order of render objects on the SceneNode. See IBeforeLocalTransform
        /// and ISetsLocalTransform.</summary>
        /// <returns>Collection of other render object interface types this class depends on</returns>
        Type[] GetDependencies();

        /// <summary>
        /// Gets the IIntersectable implementer to be used for picking and snapping. If null,
        /// picking and snapping (if supported at all) are done using OpenGl render-picking.</summary>
        /// <returns>The correct IIntersectable or null, if there is none</returns>
        /// <remarks>There are situations where multiple DomObjectInterfaces can implement
        /// IIntersectable on a particular DomObject. This method allows the IRenderObject to
        /// choose the correct one. For maximum support, consider implementing IGeometricPick,
        /// which inherits from IIntersectable.
        /// </remarks>
        IIntersectable GetIntersectable();
    }
}
