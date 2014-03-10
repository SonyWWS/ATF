//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Interface that performs picking operations by testing against the geometry of the object,
    /// as opposed to rendering the object and using the OpenGL pick (cf. IRenderPick). Used by
    /// PickAction. Implements methods very similar to IIntersectable.</summary>
    public interface IGeometricPick : IIntersectable
    {
        /// <summary>
        /// Tests if the object intersects with a ray. Uses screen-space epsilons if the object is in
        /// wireframe mode. Check the IIntersectable.CanIntersect property first, unless this object came from IRenderObject.
        /// GetIntersectable().</summary>
        /// <param name="ray">The ray, in object space</param>
        /// <param name="camera">The camera that can be used for determining screen coordinates from
        /// world coordinates, for use in wireframe picking. If null, wireframe picking is disabled
        /// and the object should be picked as if its surface were solid.</param>
        /// <param name="renderState">The render state that the user sees when picking. For example,
        /// if backface culling is turned on, picking should ignore back-facing polygons.</param>
        /// <param name="localToWorld">Matrix for transforming from local to world space</param>
        /// <param name="renderAction">The current render action, to provide the width and height of
        /// the viewport, for example. If null, wireframe picking is disabled.</param>
        /// <param name="intersectionPoint">The point of intersection, in object space</param>
        /// <param name="nearestVert">The nearest vertex of the model to the intersection point, in object
        /// space. If there's no applicable nearest vertex or if that functionality hasn't been implemented
        /// yet, nearestVert should be the same as intersectionPoint.</param>
        /// <param name="surfaceNormal">The surface normal of the target object at the intersection
        /// point, or the zero vector if the surface normal could not be found. Check against
        /// Vec3F.ZeroVector before using.</param>
        /// <param name="userData">Is optional and may be null. If there is an intersection, the list
        /// is filled in with data about the intersection (e.g., primitive #, polygon #, etc.).
        /// The caller places the user data into the hit record's RenderObjectData property.</param>
        /// <returns>True if there was an intersection</returns>
        bool IntersectRay(
            Ray3F ray,
            Camera camera,
            RenderState renderState,
            Matrix4F localToWorld,
            IRenderAction renderAction,
            out Vec3F intersectionPoint,
            out Vec3F nearestVert,
            out Vec3F surfaceNormal,
            List<uint> userData);

        /// <summary>
        /// Tests if the object intersects with the frustum. Check the IIntersectable.CanIntersect property first, unless this object
        /// came from IRenderObject.GetIntersectable().</summary>
        /// <param name="frustum">The viewing frustum, in object space. (Use Frustum.Transform().)</param>
        /// <param name="eye">The camera eye, in object space</param>
        /// <param name="renderState">The render state that the user sees when picking. For example,
        /// if backface culling is turned on, then picking should ignore back-facing polygons.</param>
        /// <param name="userData">Is optional and may be null. If there is an intersection, the list
        /// is filled in with data about the intersection (e.g., primitive #, polygon #, etc.).
        /// The caller places the user data into the hit record's RenderObjectData property.</param>
        /// <returns>True if there was an intersection</returns>
        bool IntersectFrustum(
            Frustum frustum,
            Vec3F eye,
            RenderState renderState,
            List<uint> userData);
    }
}
