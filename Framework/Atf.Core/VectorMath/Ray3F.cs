//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// A 3D ray</summary>
    public struct Ray3F : IFormattable
    {
        /// <summary>
        /// Ray origin, or starting point</summary>
        public Vec3F Origin;

        /// <summary>
        /// Ray direction, should always be a unit vector</summary>
        public Vec3F Direction;

        /// <summary>
        /// Constructs a 3D ray consisting of a point and a unit-vector direction</summary>
        /// <param name="origin">Ray origin</param>
        /// <param name="direction">Ray direction; must be a unit vector in order for
        /// IntersectPlane method to work</param>
        public Ray3F(Vec3F origin, Vec3F direction)
        {
            Origin = origin;
            Direction = direction;
        }

        /// <summary>
        /// Finds the intersection point of the ray with the given plane. The ray is
        /// treated as an infinite line, so it will intersect going "forwards" or
        /// "backwards". The plane equation is the opposite of that for Plane3F. For
        /// all points 'p' on the plane: planeNormal * p + D = 0</summary>
        /// <param name="planeNormal">Plane normal</param>
        /// <param name="planeDistance">Plane constant</param>
        /// <returns>Intersection point of ray with plane</returns>
        /// <remarks>Does not check for ray parallel to plane</remarks>
        public Vec3F IntersectPlane(Vec3F planeNormal, float planeDistance)
        {
            float t = -(Vec3F.Dot(planeNormal, Origin) + planeDistance) /
                Vec3F.Dot(planeNormal, Direction);
            return Origin + (Direction * t);
        }

        /// <summary>
        /// Finds the intersection point of the ray with the given plane. Checks
        /// for the ray being parallel with the plane or the ray pointing away
        /// from the plane</summary>
        /// <param name="plane">Must be constructed "by the rules" of Plane3F</param>
        /// <param name="intersectionPoint">The resulting intersection point or
        /// the zero-point if there was no intersection</param>
        /// <returns>True if the ray points towards the plane and intersects it</returns>
        public bool IntersectPlane(Plane3F plane, out Vec3F intersectionPoint)
        {
            // both the normal and direction must be unit vectors.
            float cos = Vec3F.Dot(plane.Normal, Direction);
            if (Math.Abs(cos) > 0.0001f)
            {
                // dist > 0 means "on the same side as the normal", aka, "the front".
                float dist = plane.SignedDistance(Origin);
                // There are two cases for the ray shooting away from the plane:
                // 1. If the ray is in front of the plane and pointing away,
                //  then 'cos' and 'dist' are both > 0.
                // 2. If the ray is in back of the plane and pointing away,
                //  then 'cos' and 'dist' are both < 0.
                // So, if the signs are the same, then there's no intersection.
                // So, if 'cos' * 'dist' is positive, then there's no intersection.
                if (cos * dist < 0.0)
                {
                    // There are two cases for the ray hitting the plane:
                    // 1. Origin is behind the plane, so the ray and normal are aligned
                    //  and 'dist' is < 0. We need to negate 'dist'.
                    // 2. Origin is in front of the plane, so the ray needs to be negated
                    //  so that cos(angle-180) is calculated. 'dist' is > 0.
                    // Either way, we've got a -1 thrown into the mix. Tricky!
                    float t = -dist / cos;
                    intersectionPoint = Origin + (Direction * t);
                    return true;
                }
            }
            intersectionPoint = new Vec3F(0, 0, 0);
            return false;
        }

        /// <summary>
        /// Finds the intersection point of the ray with the given convex polygon</summary>
        /// <param name="vertices">Polygon vertices</param>
        /// <param name="intersectionPoint">Intersection point</param>
        /// <returns>True iff the ray intersects the polygon</returns>
        public bool IntersectPolygon(Vec3F[] vertices, out Vec3F intersectionPoint)
        {
            Vec3F nearestVert, normal;
            return IntersectPolygon(vertices, out intersectionPoint, out nearestVert, out normal, false);
        }

        /// <summary>
        /// Finds the intersection point of the ray with the given convex polygon, if any. Returns the nearest
        /// vertex of the polygon to the intersection point. Can optionally backface cull the polygon.
        /// For backface culling, the front of the polygon is considered to be the side that has
        /// the vertices ordered counter-clockwise.</summary>
        /// <param name="vertices">Polygon vertices</param>
        /// <param name="intersectionPoint">Intersection point</param>
        /// <param name="nearestVert">Nearest polygon vertex to the point of intersection</param>
        /// <param name="normal">Normal unit-length vector, facing out from the side whose
        /// vertices are ordered counter-clockwise</param>
        /// <param name="backFaceCull">True if backface culling should be done</param>
        /// <returns>True iff the ray intersects the polygon</returns>
        public bool IntersectPolygon(Vec3F[] vertices, out Vec3F intersectionPoint,
            out Vec3F nearestVert, out Vec3F normal, bool backFaceCull)
        {
            bool intersects = true;
            int sign = 0;
            normal = new Vec3F();

            // First calc polygon normal
            for (int i = 2; i < vertices.Length; i++)
            {
                normal = Vec3F.Cross(vertices[i] - vertices[1], vertices[0] - vertices[1]);

                // Make sure that these 3 verts are not collinear
                float lengthSquared = normal.LengthSquared;
                if (lengthSquared != 0)
                {
                    normal *= (1.0f / (float)Math.Sqrt(lengthSquared));
                    break;
                }
            }

            if (backFaceCull)
            {
                if (Vec3F.Dot(normal, Direction) >= 0.0)
                {
                    intersectionPoint = new Vec3F();
                    nearestVert = new Vec3F();
                    return false;
                }
            }

            // Build plane and check if the ray intersects the plane.
            intersects = IntersectPlane(
                new Plane3F(normal, vertices[1]),
                out intersectionPoint);

            // Now check all vertices against intersection point
            for (int i = 0; i < vertices.Length && intersects; i++)
            {
                int i1 = i;
                int i0 = (i == 0 ? vertices.Length - 1 : i - 1);

                Vec3F cross = Vec3F.Cross(vertices[i1] - vertices[i0],
                    intersectionPoint - vertices[i0]);

                // Check if edge and intersection vectors are collinear
                if (cross.LengthSquared == 0.0f)
                {
                    // check if point is on edge
                    intersects =
                        ((vertices[i1] - vertices[i0]).LengthSquared >=
                        (intersectionPoint - vertices[i0]).LengthSquared);

                    break;
                }
                else
                {
                    float dot = Vec3F.Dot(cross, normal);

                    if (i == 0)
                        sign = Math.Sign(dot);
                    else if (Math.Sign(dot) != sign)
                        intersects = false;
                }
            }

            // if we have a definite intersection, find the closest snap-to vertex.
            if (intersects)
            {
                nearestVert = vertices[0];
                float nearestDistSqr = (intersectionPoint - nearestVert).LengthSquared;
                for (int i = 1; i < vertices.Length; i++)
                {
                    float distSqr = (vertices[i] - intersectionPoint).LengthSquared;
                    if (distSqr < nearestDistSqr)
                    {
                        nearestDistSqr = distSqr;
                        nearestVert = vertices[i];
                    }
                }
            }
            else
            {
                nearestVert = s_blankPoint;
            }

            return intersects;
        }

        /// <summary>
        /// Returns the point on this ray resulting from projecting 'point' onto this ray.
        /// This is also the same as finding the closest point on this ray to 'point'.</summary>
        /// <param name="point">Point to project onto this ray</param>
        /// <returns>Closest point on this ray to 'point'</returns>
        public Vec3F ProjectPoint(Vec3F point)
        {
            Vec3F normalizedDir = Vec3F.Normalize(Direction);
            float distToX = Vec3F.Dot(point - Origin, normalizedDir);
            Vec3F x = Origin + distToX * normalizedDir;
            return x;
        }

        /// <summary>
        /// Returns the signed distance along the ray from the ray's origin to the projection of 'p'
        /// onto this ray</summary>
        /// <param name="p">Point to be projected onto this ray</param>
        /// <returns>Signed distance along the ray, from the ray's origin to the projected point.
        /// A negative number means that 'p' falls "behind" the ray.</returns>
        public float GetDistanceToProjection(Vec3F p)
        {
            return Vec3F.Dot(p - Origin, Direction);
        }

        /// <summary>
        /// Moves this ray in a perpendicular direction from its current location so that 'point' will
        /// be on the infinite line that includes the new ray. This is almost like setting the origin
        /// to be 'point', except the current origin is moved in a strictly perpendicular direction.</summary>
        /// <param name="point">Point on infinite line that includes the new ray</param>
        public void MoveToIncludePoint(Vec3F point)
        {
            Vec3F x = ProjectPoint(point);
            Origin = Origin + (point - x);
        }

        /// <summary>
        /// Transforms the ray by the given matrix. The Direction member will be normalized. There are
        /// no particular restrictions on M. Any transformation done to a point in world space or
        /// object space can be done on this ray, including any combination of rotations, translations,
        /// uniform scales, non-uniform scales, etc.</summary>
        /// <param name="M">Transformation matrix</param>
        public void Transform(Matrix4F M)
        {
            M.Transform(Origin, out Origin);
            M.TransformVector(Direction, out Direction);
            Direction.Normalize();
        }

        /// <summary>
        /// Returns a string representation of this object for GUIs. For persistence, use
        /// ToString("R", CultureInfo.InvariantCulture).</summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        #region IFormattable
        /// <summary>
        /// Returns the string representation of this object</summary>
        /// <param name="format">Optional standard numeric format string for a floating point number.
        /// If null, "R" is used for round-trip support in case the string is persisted.
        /// http://msdn.microsoft.com/en-us/library/vstudio/dwhawy9k(v=vs.100).aspx </param>
        /// <param name="formatProvider">Optional culture-specific formatting provider. This is usually
        /// a CultureInfo object or NumberFormatInfo object. If null, the current culture is used.
        /// Use CultureInfo.InvariantCulture for persistence.</param>
        /// <returns></returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            string listSeparator = StringUtil.GetNumberListSeparator(formatProvider);

            // For historic reasons, use "R" for round-trip support, in case this string is persisted.
            if (format == null)
                format = "R";

            return String.Format(
                "{0}{6} {1}{6} {2}{6} {3}{6} {4}{6} {5}",
                Origin.X.ToString(format, formatProvider),
                Origin.Y.ToString(format, formatProvider),
                Origin.Z.ToString(format, formatProvider),
                Direction.X.ToString(format, formatProvider),
                Direction.Y.ToString(format, formatProvider),
                Direction.Z.ToString(format, formatProvider),
                listSeparator);
        }
        #endregion

        private static readonly Vec3F s_blankPoint;
    }
}
