//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Interface for IRenderObjects with special pick dispatch code</summary>
    public interface IRenderPick : IRenderObject
    {
        /// <summary>
        /// Dispatches the RenderObject for picking</summary>
        /// <param name="graphPath">The path leading to the RenderObject</param>
        /// <param name="renderState">The render state to use</param>
        /// <param name="renderAction">The render action dispatching the object</param>
        /// <param name="camera">The camera used in the dispatch process</param>
        void PickDispatch(
            SceneNode[] graphPath,
            RenderState renderState,
            IRenderAction renderAction,
            Camera camera);
    }
}
