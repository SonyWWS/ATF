//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Interface for pick actions, which do the actual work of picking</summary>
    public interface IPickAction : IRenderAction
    {
        /// <summary>
        /// Sets the filter that determines the single type of RenderObjects to dispatch in the pick
        /// operation. To allow all types, set to 'null'. Setting this also clears the internal
        /// HitRecord array.</summary>
        Type TypeFilter
        {
            set;
        }

        /// <summary>
        /// Gets and sets the filter that determines the multiple types of RenderObjects to dispatch
        /// in the pick operation. To allow all types, set to 'null'. Setting this also clears the
        /// internal HitRecord array.</summary>
        ICollection<Type> TypesFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes the pick selection area in screen coordinates</summary>
        /// <param name="camera">The camera</param>
        /// <param name="x1">Upper left x coordinate</param>
        /// <param name="y1">Upper Left y coordinate</param>
        /// <param name="x2">Bottom right x coordinate</param>
        /// <param name="y2">Bottom right y coordinate</param>
        /// <param name="multiPick">Perform a multiple pick operation</param>
        /// <param name="usePickingFrustum">Whether to set the frustum according to the picking rectangle</param>
        void Init(Camera camera, int x1, int y1, int x2, int y2, bool multiPick, bool usePickingFrustum);

        /// <summary>
        /// Get the HitRecord array. Must be called after Init() and Dispatch().</summary>
        /// <returns>The HitRecord array</returns>
        HitRecord[] GetHits();

        /// <summary>
        /// Gets the HitRecord array from a given traverse list</summary>
        /// <param name="multiPick"><c>True</c> if all available HitRecords are returned; false if just the closest</param>
        /// <returns>HitRecord array</returns>
        /// <remarks>Must be called after Init() and Dispatch()</remarks>
        HitRecord[] GetHits(bool multiPick);

        /// <summary>
        /// Gets the HitRecord array from a given traverse list. 
        /// Must be called after Init() and Dispatch().</summary>
        /// <param name="traverseList">The traverse list</param>
        /// <returns>The HitRecord array</returns>
        HitRecord[] GetHits(ICollection<TraverseNode> traverseList);

        /// <summary>
        /// Gets the HitRecord array from a given traverse list</summary>
        /// <param name="traverseList">The traverse list</param>
        /// <param name="multiPick"><c>True</c> if all available HitRecords are returned; false if just the closest</param>
        /// <returns>HitRecord array</returns>
        /// <remarks>Must be called after Init() and Dispatch()</remarks>
        HitRecord[] GetHits(ICollection<TraverseNode> traverseList, bool multiPick);

        /// <summary>
        /// Shoots a ray into the scene and returns the intersection point</summary>
        /// <param name="camera">The camera</param>
        /// <param name="x">Ray x coordinate in screen space</param>
        /// <param name="y">Ray y coordinate in screen space</param>
        /// <param name="scene">The given scene</param>
        /// <param name="point">The point of intersection</param>
        /// <returns><c>True</c> if the ray intersects the scene</returns>
        bool Intersect(Camera camera, int x, int y, Scene scene, ref Vec3F point);

        /// <summary>
        /// Shoots a ray into the scene and returns the intersection point</summary>
        /// <param name="camera">The camera</param>
        /// <param name="x">Ray x coordinate in screen space</param>
        /// <param name="y">Ray y coordinate in screen space</param>
        /// <param name="scene">The given scene</param>
        /// <param name="point">The point of intersection</param>
        /// <param name="surfaceNormal">The surface normal of the target object at the intersection
        /// point, or the zero vector if the surface normal could not be found. Check against
        /// Vec3F.ZeroVector before using.</param>
        /// <returns><c>True</c> if the ray intersects the scene</returns>
        bool Intersect(Camera camera, int x, int y, Scene scene, ref Vec3F point, out Vec3F surfaceNormal);

        /// <summary>
        /// Shoots a ray into the scene and returns the intersection point</summary>
        /// <param name="camera">The camera</param>
        /// <param name="x">Ray x coordinate in screen space</param>
        /// <param name="y">Ray y coordinate in screen space</param>
        /// <param name="scene">The given scene</param>
        /// <param name="traverseList">Traverse list to use when performing the intersection</param>
        /// <param name="point">The point of intersection</param>
        /// <returns><c>True</c> if the ray intersects the scene</returns>
        bool Intersect(Camera camera, int x, int y, Scene scene, ICollection<TraverseNode> traverseList, ref Vec3F point);

        /// <summary>
        /// Shoots a ray into the scene and returns the intersection point and possibly the nearest
        /// vertex and surface normal</summary>
        /// <param name="camera">The camera</param>
        /// <param name="x">Ray x coordinate in screen space</param>
        /// <param name="y">Ray y coordinate in screen space</param>
        /// <param name="scene">The given scene</param>
        /// <param name="point">The point of intersection</param>
        /// <param name="firstHit">The HitRecord giving possible nearest vertex and surface normal</param>
        /// <returns><c>True</c> if the ray intersects the scene</returns>
        bool Intersect(Camera camera, int x, int y, Scene scene, ref Vec3F point,
            out HitRecord firstHit);
    }
}
