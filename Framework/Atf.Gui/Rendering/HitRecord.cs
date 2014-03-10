//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Hit record returned by IPickAction, used to represent single-selection hits (by casting
    /// a ray) and multi-selection hits (by using a frustum). Used for geometry-based picking and
    /// by render-picking (OpenGL pick).</summary>
    public class HitRecord
    {
        /// <summary>
        /// Constructs a hit record representing either a windowed selection or single-click selection.
        /// For single clicks, set the WorldIntersection property and possibly Normal and NearestVert properties.</summary>
        /// <param name="graphPath">The graph path to the RenderObject</param>
        /// <param name="renderObject">The RenderObject that was hit</param>
        /// <param name="transform">The world transform of the RenderObject</param>
        /// <param name="renderObjectData">Application specific hit data</param>
        public HitRecord(SceneNode[] graphPath, IRenderObject renderObject, Matrix4F transform, uint[] renderObjectData)
        {
            m_graphPath = graphPath;
            m_renderObject = renderObject;
            m_transform = transform;
            m_renderObjectData = renderObjectData;
            m_useIntersectionPt = false;
        }

        /// <summary>
        /// Gets the graph path leading to the RenderObject</summary>
        public SceneNode[] GraphPath
        {
            get { return m_graphPath; }
        }

        /// <summary>
        /// Gets the RenderObject that was hit</summary>
        public IRenderObject RenderObject
        {
            get { return m_renderObject; }
        }

        /// <summary>
        /// Gets the world transformation of the RenderObject</summary>
        public Matrix4F Transform
        {
            get { return m_transform; }
        }

        /// <summary>
        /// Gets application specific hit data</summary>
        public uint[] RenderObjectData
        {
            get { return m_renderObjectData; }
        }

        /// <summary>
        /// Gets whether or not there is an intersection point. Windowed-selections and frustum picks
        /// do not create hit records with intersection points.</summary>
        public bool HasWorldIntersection
        {
            get { return m_useIntersectionPt; }
        }

        /// <summary>
        /// Gets or sets the world-space intersection point for this hit record. Check the HasWorldIntersection property before accessing this property.</summary>
        public Vec3F WorldIntersection
        {
            get
            {
                if (m_useIntersectionPt == false)
                    throw new InvalidOperationException("The world intersection point is not available.");
                return m_intersectionPt;
            }
            set
            {
                m_useIntersectionPt = true;
                m_intersectionPt = value;
            }
        }

        /// <summary>
        /// Gets whether or not there is a surface normal available for this hit record.
        /// Check this before getting the Normal property.</summary>
        public bool HasNormal
        {
            get { return m_hasNormal; }
        }

        /// <summary>
        /// Gets or sets the surface normal at the point of intersection, in world coordinates, as a unit vector.
        /// Is only accessible if HasNormal property is true.</summary>
        public Vec3F Normal
        {
            get
            {
                if (m_hasNormal == false)
                    throw new InvalidOperationException("The surface normal has not been set.");
                return m_normal;
            }
            set
            {
                m_hasNormal = true;
                m_normal = value;
            }
        }

        /// <summary>
        /// Gets whether or not the NearestVert property has been set</summary>
        public bool HasNearestVert
        {
            get { return m_hasNearestVert; }
        }

        /// <summary>
        /// Gets or sets nearest vertex of the triangle or mesh that was hit, in world space. Check the
        /// HasNearestVert property first before accessing this property.</summary>
        public Vec3F NearestVert
        {
            get
            {
                if (m_hasNearestVert == false)
                    throw new InvalidOperationException("The nearest vertex has not been set.");
                return m_nearestVert;
            }
            set
            {
                m_hasNearestVert = true;
                m_nearestVert = value;
            }
        }

        /// <summary>
        /// Sorts the array of hit records from nearest to farthest intersection point from the camera eye</summary>
        /// <param name="hits">The hit records. Must have world intersection points.</param>
        /// <param name="eye">The camera eye or any world point to compare against</param>
        public static void Sort(HitRecord[] hits, Vec3F eye)
        {
            s_comparer.Eye = eye;
            Array.Sort(hits, s_comparer);
        }

        private class HitRecordComparer : IComparer<HitRecord>
        {
            public int Compare(HitRecord a, HitRecord b)
            {
                if (a == b)
                    return 0; //to avoid IComparer exception in release build! what?!

                float eyeToA = (a.WorldIntersection - Eye).LengthSquared;
                float eyeToB = (b.WorldIntersection - Eye).LengthSquared;

                if (eyeToA < eyeToB)
                    return -1; //'a' is closer to the camera eye
                if (eyeToA > eyeToB)
                    return 1; //'b' is closer to the camera eye
                
                return 0; //they're exactly the same distance from the camera eye
            }

            public Vec3F Eye;
        }

        private readonly SceneNode[] m_graphPath;
        private readonly IRenderObject m_renderObject;
        private readonly Matrix4F m_transform;
        private readonly uint[] m_renderObjectData;

        private bool m_useIntersectionPt;//if 'true', m_intersectionPt has been set.
        private Vec3F m_intersectionPt;//intersection point in world coordinates

        private bool m_hasNormal;
        private Vec3F m_normal; //unit-length normal vector of the surface at point of intersection

        private bool m_hasNearestVert;
        private Vec3F m_nearestVert; //world space location of nearest vertex of triangle or mesh to the hit point

        private static readonly HitRecordComparer s_comparer = new HitRecordComparer();
    }
 }
