//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Interface that performs fast and accurate geometry-based intersection operations, as opposed
    /// to render-based intersection. Consider trying to get this interface on an object by using
    /// IRenderObject.GetIntersectable().</summary>
    /// <remarks>
    /// Any object that desires to implement the following features should implement this interface:
    /// 1. Accurate snap-to-surface. (Render-based snapping has more precision loss.)
    /// 2. Rotate-to-surface-normal.
    /// 3. Snap-to-nearest-vertex.
    /// 4. Geometric picking for performance reasons (as opposed to OpenGL render-picking). See
    /// IGeometricPick.
    /// </remarks>
    public interface IIntersectable
    {
        /// <summary>
        /// Gets whether or not the intersection tests can actually be performed</summary>
        /// <remarks>This interface may be on a type that does not know until run-time if it is possible
        /// for geometric intersection tests to be done.</remarks>
        bool CanIntersect
        {
            get;
        }

        /// <summary>
        /// Tests if the object intersects with the ray. Check the CanIntersect property first, unless this object
        /// came from IRenderObject.GetIntersectable().</summary>
        /// <param name="ray">The ray, in object space</param>
        /// <param name="backfaceCull">Whether or not backface culling should be done</param>
        /// <param name="intersectionPoint">The point of intersection, in object space</param>
        /// <param name="nearestVert">The nearest vertex of the model to the intersection point, in object
        /// space. If there's no applicable nearest vertex or if that functionality hasn't been implemented
        /// yet, then nearestVert should be the same as intersectionPoint.</param>
        /// <param name="normal">The unit-length normal at the intersection point, in
        /// object space, pointing out from the front of the surface</param>
        /// <returns><c>True</c> if there is an intersection</returns>
        bool IntersectRay(
            Ray3F ray,
            bool backfaceCull,
            out Vec3F intersectionPoint,
            out Vec3F nearestVert,
            out Vec3F normal);

        /// <summary>
        /// Tests if the object intersects with the ray. Check the CanIntersect property first, unless this object
        /// came from IRenderObject.GetIntersectable().</summary>
        /// <param name="ray">The ray, in object space</param>
        /// <param name="backfaceCull">Whether or not backface culling should be done</param>
        /// <param name="intersectionPoint">The point of intersection, in object space</param>
        /// <returns><c>True</c> if there is an intersection</returns>
        bool IntersectRay(
            Ray3F ray,
            bool backfaceCull,
            out Vec3F intersectionPoint);
    }
}
