//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Render action interface, for dispatching a render graph</summary>
    public interface IRenderAction
    {
        /// <summary>
        /// Copies all of the other render action's properties to this instance</summary>
        /// <param name="other">Other IRenderAction</param>
        void Set(IRenderAction other);

        /// <summary>
        /// Builds a traverse list from the scene and dispatches it for rendering.
        /// Call Clear afterwards to avoid keeping references to large data sets.</summary>
        /// <param name="scene">The scene to dispatch</param>
        /// <param name="camera">The camera</param>
        void Dispatch(Scene scene, Camera camera);

        /// <summary>
        /// Dispatches the given traverse list for rendering</summary>
        /// <param name="traverseList">The traverse list</param>
        /// <param name="scene">The scene to dispatch</param>
        /// <param name="camera">The camera</param>
        void Dispatch(ICollection<TraverseNode> traverseList, Scene scene, Camera camera);

        /// <summary>
        /// Builds a traverse list from the given scene. The TraverseNodes might become invalid
        /// after the next time BuildTraverseList or Dispatch are called. Consider calling Clear
        /// when done with the TraverseNodes.</summary>
        /// <param name="camera">The camera</param>
        /// <param name="scene">The scene for which to build the traverse list</param>
        /// <returns>The traverse list</returns>
        ICollection<TraverseNode> BuildTraverseList(Camera camera, Scene scene);

        /// <summary>
        /// Traverses the given sub-graph and populates the traverse list</summary>
        /// <param name="camera">The camera</param>
        /// <param name="traverseList">The traverse list being built</param>
        /// <param name="node">The root graph node to traverse</param>
        /// <param name="transform">The local transform matrix, i.e., the transform from the parent to this render object</param>
        void TraverseSubGraph(Camera camera,
            ICollection<TraverseNode> traverseList,
            SceneNode node,
            Matrix4F transform);

        /// <summary>
        /// Clears out references to all objects that were used by Dispatch and BuildTraverseList
        /// to prevent large amounts of managed memory from being held on to unnecessarily</summary>
        void Clear();

        /// <summary>
        /// Gets the RenderStateGuardian used to commit RenderStates to the underlying graphics driver</summary>
        RenderStateGuardian RenderStateGuardian
        {
            get;
        }
        
        /// <summary>
        /// Gets the current RenderState</summary>
        RenderState RenderState
        {
            get;
        }

        /// <summary>
        /// Gets the current TraverseState</summary>
        TraverseState TraverseState
        {
            get;
        }

        /// <summary>
        /// Gets the current RenderObject being traversed</summary>
        IRenderObject RenderObject
        {
            get;
        }

        /// <summary>
        /// Gets the top matrix in the matrix stack</summary>
        Matrix4F TopMatrix
        {
            get;
        }

        /// <summary>
        /// Pushes a matrix onto the matrix stack</summary>
        /// <param name="matrix">The matrix pushed</param>
        /// <param name="multiply">If true, multiply matrix by top matrix</param>
        void PushMatrix(Matrix4F matrix, bool multiply);

        /// <summary>
        /// Pops a matrix form the matrix stack</summary>
        /// <returns>The matrix popped</returns>
        Matrix4F PopMatrix();

        /// <summary>
        /// Gets a matrix from the matrix stack</summary>
        /// <param name="relativeIndex">The relative index starting from the top.
        /// 0 is the top, -1 is the next matrix below, etc.</param>
        /// <returns>The matrix at the index</returns>
        Matrix4F GetMatrixAt(int relativeIndex);

        /// <summary>
        /// Returns either a new or previously used TraverseNode for use by IRenderObject's
        /// Traverse(). The caller must place this node on the traverse list and not maintain
        /// a permanent reference to it.</summary>
        /// <returns>Either a new or previously used TraverseNode</returns>
        /// <remarks>
        /// Because each IRenderObject.Traverse() needs one or more of these for every
        /// instance of that model, creating TraverseNodes is a performance critical
        /// area.
        /// The simplest implementation would be to return a new TraverseNode
        /// and not to try to maintain a pool of TraverseNodes.</remarks>
        TraverseNode GetUnusedNode();

        /// <summary>
        /// Gets and sets the title</summary>
        string Title
        {
            get;
            set;
        }

        // Rendering properties

        /// <summary>
        /// Gets or sets the viewport width</summary>
        int ViewportWidth
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the viewport height</summary>
        int ViewportHeight
        {
            get;
            set;
        }
    }
}
