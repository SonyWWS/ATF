//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Text;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// A set of vertices representing a closed planar polygon</summary>
    public class Polygon3F : IFormattable
    {
        /// <summary>
        /// Polygon vertices. There should be no duplicates.</summary>
        public Vec3F[] Vertices;

        /// <summary>
        /// Constructor using a vertex array</summary>
        /// <param name="vertices">Coplanar vertices. There should be no duplicates.</param>
        public Polygon3F(Vec3F[] vertices)
        {
            Vertices = new Vec3F[vertices.Length];
            vertices.CopyTo(Vertices, 0);
        }

        /// <summary>
        /// Triangulate the polygon</summary>
        /// <param name="normal">Polygon normal</param>
        /// <returns>List of triangles from the polygon</returns>
        public IList<Triangle3F> Triangulate(Vec3F normal)
        {
            List<Triangle3F> tris = new List<Triangle3F>();

            // Build TriVx array
            LinkedList<TriVx> polygon = new LinkedList<TriVx>();
            foreach (Vec3F v in Vertices)
                polygon.AddLast(new TriVx(v));

            if (polygon.Count < 3)
                throw new InvalidOperationException("Polygon has less than 3 vertices");

            List<LinkedListNode<TriVx>> ears = new List<LinkedListNode<TriVx>>();

            if (polygon.Count == 3)
            {
                // This is a triangle so just add any arbitrary vertex as an ear
                polygon.First.Value.IsEar = true;
                ears.Add(polygon.First);
            }
            else
            {
                LinkedListNode<TriVx> current = null;

                // Update reflex flag
                for (current = polygon.First; current != null; current = current.Next)
                    current.Value.IsReflex = IsReflex(current, normal);

                // Construct ears from all convex vertices
                for (current = polygon.First; current != null; current = current.Next)
                {
                    if (!current.Value.IsReflex && IsEar(current, normal))
                    {
                        current.Value.IsEar = true;
                        ears.Add(current);
                    }
                }
            }

            while (polygon.Count > 0 && ears.Count > 0)
            {
                LinkedListNode<TriVx> node = ears[0];
                LinkedListNode<TriVx> prev = PrevNode(node);
                LinkedListNode<TriVx> next = NextNode(node);

                tris.Add(new Triangle3F(prev.Value.V, node.Value.V, next.Value.V));

                if (polygon.Count == 3)
                    polygon.Clear();
                else
                {
                    ears.Remove(node);
                    polygon.Remove(node);

                    // Now test adjacent vertices
                    // If adjacent vertex is convex it remains convex, otherwise it needs testing
                    if (prev.Value.IsReflex)
                        prev.Value.IsReflex = IsReflex(prev, normal);

                    if (!prev.Value.IsReflex)
                        UpdateEar(prev, ears, normal);

                    // Do the same for next 
                    if (next.Value.IsReflex)
                        next.Value.IsReflex = IsReflex(next, normal);

                    if (!next.Value.IsReflex)
                        UpdateEar(next, ears, normal);
                }
            }

            return tris;
        }

        private bool IsEar(LinkedListNode<TriVx> node, Vec3F normal)
        {
            bool isEar = true;
            TriVx v0 = PrevNode(node).Value;
            TriVx v1 = node.Value;
            TriVx v2 = NextNode(node).Value;

            foreach (TriVx v in node.List)
            {
                if (v.IsReflex && v != v0 && v != v1 && v != v2)
                {
                    // Chec for containment
                    if (IsLeftOf(v0, v1, v, normal) &&
                        IsLeftOf(v1, v2, v, normal) &&
                        IsLeftOf(v2, v0, v, normal))
                    {
                        isEar = false;
                    }
                }
            }
            return isEar;
        }

        private void UpdateEar(LinkedListNode<TriVx> node, List<LinkedListNode<TriVx>> ears, Vec3F normal)
        {
            if (IsEar(node, normal))
            {
                // If it's not already an ear add it to the ear list
                if (!node.Value.IsEar)
                {
                    ears.Add(node);
                    node.Value.IsEar = true;
                }
            }
            else
            {
                // Ok, it's not an ear - check if it is marked as one
                if (node.Value.IsEar)
                {
                    ears.Remove(node);
                    node.Value.IsEar = false;
                }
            }
        }

        private bool IsReflex(LinkedListNode<TriVx> node, Vec3F normal)
        {
            TriVx v0 = PrevNode(node).Value;
            TriVx v1 = node.Value;
            TriVx v2 = NextNode(node).Value;
            return !IsLeftOf(v0, v1, v2, normal);
        }

        private bool IsLeftOf(TriVx v0, TriVx v1, TriVx v2, Vec3F normal)
        {
            Vec3F cross = Vec3F.Cross(v1.V - v0.V, v2.V - v0.V);
            return Vec3F.Dot(cross, normal) > 0.0f;
        }

        private LinkedListNode<TriVx> NextNode(LinkedListNode<TriVx> node)
        {
            if (node.Next != null)
                return node.Next;
            else
                return node.List.First;
        }

        private LinkedListNode<TriVx> PrevNode(LinkedListNode<TriVx> node)
        {
            if (node.Previous != null)
                return node.Previous;
            else
                return node.List.Last;
        }

        private class TriVx
        {
            public readonly Vec3F V;
            public bool IsEar;
            public bool IsReflex;

            public TriVx(Vec3F v)
            {
                V = v;
            }
        }

        /// <summary>
        /// Returns the string representation of this Scea.VectorMath.Polygon3F structure</summary>
        /// <returns>A <see cref="T:System.String"></see> representing the 3D polygon</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary> Returns the string representation of this Scea.VectorMath.Polygon3F structure 
        /// with the specified formatting information</summary>
        /// <param name="format">Standard numeric format string characters valid for a floating point</param>
        /// <param name="formatProvider">The culture specific formatting provider</param>
        /// <returns>A <see cref="T:System.String"></see> representing the 3D vector</returns> 
        public string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Vertices.Length.ToString("D", formatProvider));
            for (int i = 0; i < Vertices.Length; ++i)
            {
                sb.Append(String.Format
                (
                     "(, {0}, {1}, {2})",
                     ((double)Vertices[i].X).ToString(format, formatProvider),
                     ((double)Vertices[i].Y).ToString(format, formatProvider),
                     ((double)Vertices[i].Z).ToString(format, formatProvider)
                ));
            }
            return sb.ToString();

        }


    }
}
