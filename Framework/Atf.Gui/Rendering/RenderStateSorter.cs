//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// RenderStateSorter provides accumulation and sorting services for the scene graph traversal.
    /// It implements ICollection for accumulation and IEnumerable for iterating through the sorted list.
    /// TraverseNodes are sorted in the following order:
    /// Wireframe;
    /// Smooth, untextured;
    /// Smooth, textured (subsorted by texture name);
    /// Alpha (subsorted by camera-space z distance);
    /// Z-buffer disabled.</summary>
    public class RenderStateSorter : ICollection<TraverseNode>
    {
        #region ICollection<TraverseNode> Members

        /// <summary>
        /// Constructor</summary>
        public RenderStateSorter()
        {
            m_buckets = new List<RenderBucket>();

            // Added in order from the first to be rendered to the last.
            // The pairs of RenderMode bits are "must have" and "must not have".
            m_buckets.Add( new RenderBucket(
                RenderMode.Wireframe,
                RenderMode.Smooth | RenderMode.Alpha | RenderMode.DisableZBuffer));
            m_buckets.Add(new RenderBucket(
                RenderMode.Smooth,
                RenderMode.Alpha | RenderMode.DisableZBuffer | RenderMode.Textured));
            m_buckets.Add(new RenderBucket(
                RenderMode.Smooth | RenderMode.Textured,
                RenderMode.Alpha | RenderMode.DisableZBuffer));
            m_buckets.Add(new RenderBucket(
                RenderMode.Alpha,
                RenderMode.DisableZBuffer));
            m_buckets.Add(new RenderBucket(
                RenderMode.DisableZBuffer,
                0));
        }

        /// <summary>
        /// Adds the provided TraverseNode to the state sorter</summary>
        /// <param name="item">TraverseNode to add</param>
        public void Add(TraverseNode item)
        {
            foreach (RenderBucket bucket in m_buckets)
            {
                if (bucket.TryAdd(item))
                    return;
            }
        }

        /// <summary>
        /// Clears all gathered TraverseNodes from this RenderStateSorter's internal RenderGatherer</summary>
        public void Clear()
        {
            foreach (RenderBucket bucket in m_buckets)
                bucket.Nodes.Clear();
        }

        /// <summary>
        /// Test if node is in RenderStateSorter</summary>
        /// <param name="item">Node to look for</param>
        /// <returns>Whether or not the requested node was found in the RenderStateSorter</returns>
        public bool Contains(TraverseNode item)
        {
            throw new NotImplementedException(
                "RenderStateSorter.Contains() is not implemented and would be expensive to do so.");
        }

        /// <summary>
        /// Copies the collected TraverseNode set to a flat array at the specified index</summary>
        /// <param name="array">Output array, containing at least RenderStateSorter.Count + arrayIndex elements</param>
        /// <param name="arrayIndex">Element in array to start copying TraverseNodes to</param>
        public void CopyTo(TraverseNode[] array, int arrayIndex)
        {
            foreach (TraverseNode node in this)
                array[arrayIndex++] = node;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see></summary>
        /// <value>Number of elements in instance</value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see></returns>
        public int Count
        {
            get
            {
                int count = 0;
                foreach (RenderBucket bucket in m_buckets)
                {
                    count += bucket.Nodes.Count;
                }
                return count;
            }
        }

        /// <summary>
        /// Gets whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only</summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the specified TraverseNode from the collection</summary>
        /// <param name="item">TraverseNode to remove</param>
        /// <returns>Whether or not item was found in the collection</returns>
        /// <exception cref="NotImplementedException">Remove() is not implemented</exception>
        public bool Remove(TraverseNode item)
        {
            throw new NotImplementedException(
                "RenderStateSorter.Remove() is not implemented and would be expensive to do so.");
        }

        #endregion

        #region IEnumerable<TraverseNode> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection</summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection</returns>
        public IEnumerator<TraverseNode> GetEnumerator()
        {
            return GetEnumeratorInternal();
        }
        
        private IEnumerator<TraverseNode> GetEnumeratorInternal()
        {
            foreach (RenderBucket bucket in m_buckets)
            {
                if (bucket.Nodes.Count == 0)
                    continue;

                // There's a problem if the sorting is done twice -- the results need
                //  to be identical every time in order for render picking to work.
                //  Also, it's inefficient to do all of this work multiple times.
                if (bucket.Sorted == false)
                {
                    if ((bucket.PassBits & RenderMode.Textured) != 0)
                        TraverseSortUtils.SortByTextureName(bucket.Nodes);
                    else if ((bucket.PassBits & RenderMode.Alpha) != 0)
                        TraverseSortUtils.SortByCameraSpaceDepth(bucket.Nodes, m_viewMatrix);
                    else
                        TraverseSortUtils.SortByRenderMode(bucket.Nodes);

                    bucket.Sorted = true;
                }

                foreach (TraverseNode node in bucket.Nodes)
                    yield return node;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        #endregion

        /// <summary>
        /// Sets the view matrix for the alpha-sorted phase</summary>
        /// <param name="viewMatrix">View matrix for the alpha-sorted phase</param>
        public void SetViewMatrix(Matrix4F viewMatrix)
        {
            m_viewMatrix.Set(viewMatrix);
        }

        private class RenderBucket
        {
            public RenderBucket(RenderMode passBits, RenderMode noPassBits)
            {
                PassBits = passBits;
                NoPassBits = noPassBits;
            }
            public bool TryAdd(TraverseNode node)
            {
                bool passedPositive = (node.RenderState.RenderMode & PassBits) == PassBits;
                bool passedNegative = (node.RenderState.RenderMode & NoPassBits) == 0;

                if (passedPositive && passedNegative)
                {
                    m_nodes.Add(node);
                    m_sorted = false;
                    return true;
                }
                return false;
            }
            public List<TraverseNode> Nodes
            {
                get { return m_nodes; }
            }
            
            // be sure to set this to 'true' if Nodes has been sorted
            public bool Sorted
            {
                get { return m_sorted; }
                set { m_sorted = value; }
            }

            public readonly RenderMode PassBits, NoPassBits;
            private readonly List<TraverseNode> m_nodes = new List<TraverseNode>();
            private bool m_sorted; //Has this bucket changed? If so, caller should re-sort.
        }

        private readonly List<RenderBucket> m_buckets;
        private readonly Matrix4F m_viewMatrix = new Matrix4F();
    }
}
