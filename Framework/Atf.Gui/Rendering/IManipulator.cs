//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Interface for manipulators</summary>
    public interface IManipulator : IRenderObject
    {
        /// <summary>
        /// Handles the initial hit on the manipulator</summary>
        /// <param name="hits">The hit record array generated for the hit</param>
        /// <param name="x">X hit coordinate in screen space</param>
        /// <param name="y">Y hit coordinate in screen space</param>
        /// <param name="pickAction">Pick action</param>
        /// <param name="renderAction">The render action used</param>
        /// <param name="camera">The camera</param>
        /// <param name="context">The context (e.g., ITransactionContext and ISelectionContext) that
        /// this manipulator is working on</param>
        void OnHit(HitRecord[] hits, float x, float y,
            IPickAction pickAction, IRenderAction renderAction, Camera camera, object context);

        /// <summary>
        /// Handles an incremental drag on the manipulator</summary>
        /// <param name="hits">Hit records generated for the OnHit method. Index 0 is the closest.
        /// Is the exact same data passed in to OnHit; it's not updated for the mouse move.</param>
        /// <param name="x">X hit coordinate in screen space</param>
        /// <param name="y">Y hit coordinate in screen space</param>
        /// <param name="pickAction">Pick action</param>
        /// <param name="renderAction">The render action used</param>
        /// <param name="camera">The camera</param>
        /// <param name="context">The context (e.g., ITransactionContext and ISelectionContext) that
        /// this manipulator is working on</param>
        void OnDrag(HitRecord[] hits, float x, float y,
            IPickAction pickAction, IRenderAction renderAction, Camera camera, object context);

        /// <summary>
        /// Handles the end of the drag on the manipulator</summary>
        /// <param name="hits">Hit records generated for the OnHit method. Index 0 is the closest.
        /// Is the exact same data passed in to OnHit; it's not updated for the mouse move.</param>
        /// <param name="x">X hit coordinate in screen space</param>
        /// <param name="y">Y hit coordinate in screen space</param>
        /// <param name="pickAction">Pick action</param>
        /// <param name="renderAction">The render action used</param>
        /// <param name="camera">The camera</param>
        /// <param name="context">The context (e.g., ITransactionContext and ISelectionContext) that
        /// this manipulator is working on</param>
        void OnEndDrag(HitRecord[] hits, float x, float y,
            IPickAction pickAction, IRenderAction renderAction, Camera camera, object context);
    }
}
