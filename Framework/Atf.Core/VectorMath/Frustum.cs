//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// 3D viewing frustum, made from 6 planes that are in the View Coordinate System
    /// (x: right, y: up, z: out of screen). The frustum can be in one of two modes--
    /// perspective or orthographic.</summary>
    public class Frustum : IFormattable
    {
        /// <summary>
        /// Identifiers of the 6 planes that make up the frustm</summary>
        public const int
            iRight = 0,
            iLeft = 1,
            iTop = 2,
            iBottom = 3,
            iNear = 4,
            iFar = 5;

        /// <summary>
        /// Constructor</summary>
        public Frustum()
        {
            m_planes = new Plane3F[6];
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="other">Other frustum</param>
        public Frustum(Frustum other)
            : this()
        {
            Set(other);
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="fovY">Y field-of-view</param>
        /// <param name="aspectRatio">Aspect ration of frustum cross section</param>
        /// <param name="near">Near plane distance</param>
        /// <param name="far">Far plane distance</param>
        public Frustum(float fovY, float aspectRatio, float near, float far)
            : this()
        {
            SetPerspective(fovY, aspectRatio, near, far);
        }

        /// <summary>
        /// Constructor of frustum for an orthographic projection</summary>
        /// <param name="right">Right plane constant</param>
        /// <param name="left">Left plane constant</param>
        /// <param name="top">Top plane constant</param>
        /// <param name="bottom">Bottom plane constant</param>
        /// <param name="near">Near plane constant</param>
        /// <param name="far">Far plane constant</param>
        public Frustum(float right, float left, float top, float bottom, float near, float far)
            : this()
        {
            SetOrtho(right, left, top, bottom, near, far);
        }

        /// <summary>
        /// Sets this frustum to the given frustum</summary>
        /// <param name="other">Frustum</param>
        public void Set(Frustum other)
        {
            for (int i = 0; i < 6; i++)
            {
                m_planes[i] = other.m_planes[i];
            }
        }

        /// <summary>
        /// Sets this frustum to frustum represented by the given values</summary>
        /// <param name="fovY">Y field-of-view</param>
        /// <param name="aspectRatio">Aspect ration of frustum cross section</param>
        /// <param name="near">Near plane distance</param>
        /// <param name="far">Far plane distance</param>
        public void SetPerspective(float fovY, float aspectRatio, float near, float far)
        {
            float tanY = (float)Math.Tan((double)fovY / 2.0);
            float tanX = tanY * aspectRatio;
            Vec3F xAxis = new Vec3F(-1, 0, -tanX);
            Vec3F yAxis = new Vec3F(0, -1, -tanY);
            float nX = xAxis.Length;
            float nY = yAxis.Length;

            // The view coordinate system has +y pointing up, +x to the right, +z out of the screen.
            m_planes[iRight].Set(new Vec3F(-1, 0, -tanX) / nX, 0.0f);
            m_planes[iLeft].Set(new Vec3F(1, 0, -tanX) / nX, 0.0f);
            m_planes[iTop].Set(new Vec3F(0, -1, -tanY) / nY, 0.0f);
            m_planes[iBottom].Set(new Vec3F(0, 1, -tanY) / nY, 0.0f);
            m_planes[iNear].Set(new Vec3F(0, 0, -1), near);
            m_planes[iFar].Set(new Vec3F(0, 0, 1), -far);
        }

        /// <summary>
        /// Sets the frustum for an orthographic projection</summary>
        /// <param name="right">Right plane constant</param>
        /// <param name="left">Left plane constant</param>
        /// <param name="top">Top plane constant</param>
        /// <param name="bottom">Bottom plane constant</param>
        /// <param name="near">Near plane constant</param>
        /// <param name="far">Far plane constant</param>
        public void SetOrtho(float right, float left, float top, float bottom, float near, float far)
        {
            // The view coordinate system has +y pointing up, +x to the right, +z out of the screen.
            m_planes[iRight].Set(new Vec3F(-1, 0, 0), -right);
            m_planes[iLeft].Set(new Vec3F(1, 0, 0), left);
            m_planes[iTop].Set(new Vec3F(0, -1, 0), -top);
            m_planes[iBottom].Set(new Vec3F(0, 1, 0), bottom);
            m_planes[iNear].Set(new Vec3F(0, 0, -1), near);
            m_planes[iFar].Set(new Vec3F(0, 0, 1), -far);
        }

        /// <summary>
        /// Clips this frustum to the sub-frustum represented by the given values</summary>
        /// <param name="left">Left x, in normalized viewing coordinates. -0.5 is farthest left.</param>
        /// <param name="right">Right x, in normalized viewing coordinates. 0.5 is farthest right.</param>
        /// <param name="top">Top y, in normalized viewing coordinates. +0.5 is at top of screen.</param>
        /// <param name="bottom">Bottom y, in normalized viewing coordinates. -0.5 is at bottom of screen.</param>
        public void Clip(float left, float right, float top, float bottom)
        {
            if (!IsOrtho)
            {
                ClipPerspective(left, right, top, bottom);
            }
            else
            {
                ClipOrtho(left, right, top, bottom);
            }
        }

        /// <summary>
        /// Gets or sets the far clipping plane</summary>
        public float FarZ
        {
            get { return -m_planes[iFar].Distance; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();
                m_planes[iFar].Distance = -value;
            }

        }

        /// <summary>
        /// Gets or sets the near clipping plane</summary>
        public float NearZ
        {
            get { return m_planes[iNear].Distance; }
            set
            {
                //if (value <= 0)
                //    throw new ArgumentOutOfRangeException();
                m_planes[iNear].Distance = value;
            }
        }

        /// <summary>
        /// Gets the distance to the right frustum plane</summary>
        public float Right
        {
            get { return -m_planes[iRight].Distance; }
        }

        /// <summary>
        /// Gets the distance to the left frustum plane</summary>
        public float Left
        {
            get { return m_planes[iLeft].Distance; }
        }

        /// <summary>
        /// Gets the distance to the top frustum plane</summary>
        public float Top
        {
            get { return -m_planes[iTop].Distance; }
        }

        /// <summary>
        /// Gets the distance to the bottom frustum plane</summary>
        public float Bottom
        {
            get { return m_planes[iBottom].Distance; }
        }

        /// <summary>
        /// Gets the distance to the near frustum plane</summary>
        public float Near
        {
            get { return m_planes[iNear].Distance; }
        }

        /// <summary>
        /// Gets the distance to the far frustum plane</summary>
        public float Far
        {
            get { return -m_planes[iFar].Distance; }
        }

        /// <summary>
        /// Gets a copy of the specified plane. Use Frustum.iRight, etc., to request
        /// a particular plane. The resulting plane has the normal facing inwards.
        /// Thus, Plane3F.SignedDistance returns a postive number for any point
        /// on the inside half of that plane, and a negative result means that the point
        /// is definitely on the outside of that plane--and thus outside the entire
        /// frustum.</summary>
        /// <param name="i">Plane index, [0,5]. Use Frustum.iRight, etc.</param>
        /// <returns>Copy of the requested plane, whose normal points inward</returns>
        public Plane3F GetPlane(int i)
        {
            return m_planes[i];
        }

        /// <summary>
        /// Gets a value indicating if frustum is orthographic</summary>
        public bool IsOrtho
        {
            get
            {
                float cosFovX = Math.Abs(Vec3F.Dot(m_planes[iRight].Normal, m_planes[iLeft].Normal));
                return cosFovX >= 0.9999;
            }
        }

        /// <summary>
        /// Gets X field-of-view</summary>
        public float FovX
        {
            get
            {
                float dot = Vec3F.Dot(m_planes[iLeft].Normal, new Vec3F(1, 0, 0));
                float fovX = (float)Math.Acos(dot);
                return fovX * 2;
            }
        }

        /// <summary>
        /// Gets Y field-of-view</summary>
        public float FovY
        {
            get
            {
                float dot = Vec3F.Dot(m_planes[iBottom].Normal, new Vec3F(0, 1, 0));
                float fovY = (float)Math.Acos(dot);
                return fovY * 2;
            }
        }

        /// <summary>
        /// Tests if frustum contains the given sphere</summary>
        /// <param name="sphere">Sphere</param>
        /// <returns>True iff frustum contains the given sphere</returns>
        public bool Contains(Sphere3F sphere)
        {
            for (int i = 0; i < 6; i++)
            {
                float distance = m_planes[i].SignedDistance(sphere.Center);
                if (distance < -sphere.Radius)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if frustum contains any part of the given box. The frustum and the box must be in
        /// the same space. The frustum does not have to be symmetrical and could have been transformed
        /// into object space using Frustum.Transform().</summary>
        /// <param name="box">The box</param>
        /// <returns>True iff frustum contains the given box</returns>
        public bool Contains(Box box)
        {
            Vec3F center = (box.Min + box.Max) * 0.5f;
            Vec3F center_to_max = (box.Max - box.Min) * 0.5f;

            for (int i = 0; i < 6; i++)
            {
                // Compute the distance from the plane to the center. A negative distance means that
                // the center point is on the "out" side of the plane and is thus outside the frustum.
                float center_dist = m_planes[i].SignedDistance(center);

                // Now we need the distance to the plane that the nearest corner contributes.
                // Map the plane's normal to be in the positive octant along with 'center_to_max'.
                // Note that 'center_to_max' is guaranteed to have all 3 coordinates be positive
                // (because box.Max is greater than box.Min for all 3). To transform the plane's
                // normal, we simply take the absolute value of each coordinate. This plane's normal
                // must be unit length so that the distance is correct. 'nearest_corner_dist' is
                // always non-negative. (It could be zero if the box has zero size.)
                float nearest_corner_dist =
                    center_to_max.X * Math.Abs(m_planes[i].Normal.X) +
                    center_to_max.Y * Math.Abs(m_planes[i].Normal.Y) +
                    center_to_max.Z * Math.Abs(m_planes[i].Normal.Z);

                if (center_dist + nearest_corner_dist < 0)
                {
                    return false;
                }
            }

            // This is an approximation. It is possible that the box is still outside the frustum.
            return true;
        }

        /// <summary>
        /// Tests if any part of the polygon is inside this frustum. Errs on the side of inclusion.
        /// The polygon and the frustum and the eye must be in the same space -- probably object space
        /// to avoid the expense of transforming all the polygons into view space unnecessarily.</summary>
        /// <param name="vertices">A polygon. Can be non-convex and non-planar for the frustum test.
        /// Must be convex and planar for backface culling.</param>
        /// <param name="eye">The camera's eye associated with this frustum. Is only used if 'backfaceCull'
        /// is 'true'.</param>
        /// <param name="backfaceCull">Should back-facing polygons always be excluded?</param>
        /// <returns>True iff any part of the polygon is inside this frustum</returns>
        public bool ContainsPolygon(Vec3F[] vertices, Vec3F eye, bool backfaceCull)
        {
            // If all of the points are outside any one plane, then the polygon is not contained.
            for (int p = 0; p < 6; p++)
            {
                int v = 0;
                for (; v < vertices.Length; v++)
                {
                    float dist = m_planes[p].SignedDistance(vertices[v]);
                    if (dist > 0.0f)
                        break;
                }
                if (v == vertices.Length)
                    return false;
            }

            if (backfaceCull)
            {
                // First calculate polygon normal, pointing "out from" the visible side.
                Vec3F normal = new Vec3F();
                for (int i = 2; i < vertices.Length; i++)
                {
                    normal = Vec3F.Cross(vertices[0] - vertices[1],
                        vertices[i] - vertices[1]);

                    // Make sure that these 3 verts are not collinear
                    if (normal.LengthSquared != 0)
                        break;
                }

                // We can't use the near plane's normal, since it may no longer be pointing in the correct
                // direction (due to non-uniform scalings, shear transforms, etc.).
                if (Vec3F.Dot(normal, vertices[0] - eye) <= 0)
                {
                    return false;
                }
            }

            // This is an approximation. To be absolutely sure, we'd have to test the 8 points
            // of this frustum against the plane of the polygon and against the plane of each edge,
            // perpendicular to the polygon plane.
            return true;
        }

        /// <summary>
        /// Transforms this frustum by the given matrix</summary>
        /// <param name="m">Transformation matrix. Can be a nearly-general transform and include non-uniform
        /// scaling and shearing.</param>
        public void Transform(Matrix4F m)
        {
            Matrix4F transposeOfInverse = new Matrix4F(m);
            transposeOfInverse.Invert(transposeOfInverse);
            transposeOfInverse.Transpose(transposeOfInverse);

            for (int i = 0; i < 6; i++)
            {
                m.Transform(m_planes[i], transposeOfInverse, out m_planes[i]);
            }
        }

        /// <summary>
        /// Obtains a <see cref="T:System.String"></see> that represents this frustum</summary>
        /// <returns>A <see cref="T:System.String"></see> representing this frustum</returns>
        public override string ToString()
        {
            return string.Format("Frustum:\nright: {0}\nLeft: {1}\nTop:  {2}\nBottom: {3}\nNear {4}\nFar: {5}",
                Right, Left, Top, Bottom, Near, Far);
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
                 Right.ToString(format, formatProvider),
                 Left.ToString(format, formatProvider),
                 Top.ToString(format, formatProvider),
                 Bottom.ToString(format, formatProvider),
                 Near.ToString(format, formatProvider),
                 Far.ToString(format, formatProvider),
                 listSeparator);
        }
        #endregion

        //(-0.5,-0.5) is the lower left corner and (0.5,0.5) is the upper right corner.
        private void ClipPerspective(float left, float right, float top, float bottom)
        {
            float near = Near;
            float h = near * (float)Math.Tan(FovY / 2.0f) * 2.0f;
            float w = near * (float)Math.Tan(FovX / 2.0f) * 2.0f;

            CalcPlaneNormal(iLeft, new Vec3F(0, 1, 0), new Vec3F(right * w, 0, -near));
            CalcPlaneNormal(iRight, new Vec3F(0, -1, 0), new Vec3F(left * w, 0, -near));
            CalcPlaneNormal(iBottom, new Vec3F(1, 0, 0), new Vec3F(0, bottom * h, -near));
            CalcPlaneNormal(iTop, new Vec3F(-1, 0, 0), new Vec3F(0, top * h, -near));
        }

        //(-0.5,-0.5) is the lower left corner and (0.5,0.5) is the upper right corner.
        private void ClipOrtho(float nLeft, float nRight, float nTop, float nBottom)
        {
            float viewHeight = Top * 2.0f;
            float viewWidth = Right * 2.0f;

            // Translate from normalized screen coordinates to view coordinates.
            float vLeft = nLeft * viewWidth;
            float vRight = nRight * viewWidth;
            float vTop = nTop * viewHeight;
            float vBottom = nBottom * viewHeight;

            // How the planes are set needs to be kept in sync with SetOrtho().
            // The viewing coordinate system has the origin in the middle, +x to the right
            // and +y up and +z out of the screen.
            m_planes[iRight].Distance = -vRight;
            m_planes[iLeft].Distance = vLeft;
            m_planes[iTop].Distance = -vTop;
            m_planes[iBottom].Distance = vBottom;
        }

        private void CalcPlaneNormal(int planeIndex, Vec3F u1, Vec3F u2)
        {
            m_planes[planeIndex].Normal = Vec3F.Cross(u1, u2);
            m_planes[planeIndex].Normal.Normalize();
            m_planes[planeIndex].Distance = 0;
        }

        /// <summary>
        /// Array of 6 frustum planes. The normals all point towards the inside of the frustum.</summary>
        private readonly Plane3F[] m_planes;
    }
}
