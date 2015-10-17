//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Set of integers with low memory usage for large sets containing large consecutive runs
    /// of integers. The integers are always in order from smallest to largest. The ranges are
    /// optimal, with no empty ranges and no two adjacent ranges that can be combined.</summary>
    [Serializable]
    public class IntSet : ICollection<int>
    {
        /// <summary>
        /// Adds an integer to the set if it is not already in the set</summary>
        /// <param name="item">Integer to add to the set</param>
        public void Add(int item)
        {
            AttemptModify();

            int rangeIndex = FindRange(item);
            if (rangeIndex >= 0)
                return; // already in set

            int insertionIndex = -rangeIndex - 1;

            AddNonOverlappingRange(insertionIndex, item, item);
        }

        /// <summary>
        /// Adds a range of integers to this set that are not already in the set</summary>
        /// <param name="begin">First integer in the range</param>
        /// <param name="end">Last integer in the range</param>
        public void AddRange(int begin, int end)
        {
            AttemptModify();
            if (end < begin)
                throw new ArgumentException("the end of the range must be >= the beginning");

            // Find all of the existing ranges that overlap this new range, or find the insertion
            //  index if there was no overlap.
            int rangeBeginIndex, rangeEndIndex, insertionIndex;
            FindRanges(begin, end, out rangeBeginIndex, out rangeEndIndex, out insertionIndex);

            // If there was overlap, remove them and set the insertion index.
            if (rangeBeginIndex >= 0)
            {
                insertionIndex = rangeBeginIndex;
                begin = Math.Min(begin, m_ranges[rangeBeginIndex].Min);
                end = Math.Max(end, m_ranges[rangeEndIndex].Max);
                RemoveRangeIndices(rangeBeginIndex, rangeEndIndex);
            }

            AddNonOverlappingRange(insertionIndex, begin, end);
        }

        /// <summary>
        /// Removes the integer from the set</summary>
        /// <param name="item">Integer to remove from the set</param>
        /// <returns><c>True</c> if integer was in the set</returns>
        /// <exception cref="T:System.NotSupportedException">The set is read-only</exception>
        public bool Remove(int item)
        {
            AttemptModify();

            int rangeIndex = FindRange(item);
            if (rangeIndex < 0)
                return false;

            Range range = m_ranges[rangeIndex];

            if (range.Min == item)
            {
                if (range.Min != range.Max)
                    range.Min++;
                else
                    m_ranges.RemoveAt(rangeIndex);
            }
            else if (range.Max == item)
            {
                if (range.Max != range.Min)
                    range.Max--;
                else
                    m_ranges.RemoveAt(rangeIndex);
            }
            else
            {
                m_ranges.Insert(rangeIndex + 1, new Range(item + 1, range.Max));
                range.Max = item - 1;
            }

            m_count--;
            return true;
        }

        /// <summary>
        /// Removes a range of integers from the set</summary>
        /// <param name="begin">First integer of the range</param>
        /// <param name="end">Last integer of the range</param>
        /// <exception cref="T:System.NotSupportedException">The set is read-only</exception>
        public void RemoveRange(int begin, int end)
        {
            AttemptModify();
            if (end < begin)
                throw new ArgumentException("the end of the range must be >= the beginning");

            // Find all of the existing ranges that overlap this new range.
            int rangeBeginIndex, rangeEndIndex, insertionIndex;
            FindRanges(begin, end, out rangeBeginIndex, out rangeEndIndex, out insertionIndex);
            if (rangeBeginIndex < 0)
                return; //no overlap; nothing to remove

            // Check first and last ranges to see if they need to be split.
            int newBeginRangeMin = -1;
            Range beginRange = m_ranges[rangeBeginIndex];
            if (beginRange.Min < begin)
                newBeginRangeMin = beginRange.Min;

            int newEndRangeMax = -1;
            Range endRange = m_ranges[rangeEndIndex];
            if (end < endRange.Max)
                newEndRangeMax = endRange.Max;

            // Remove overlapping ranges and insert new ranges, pushing them in, end-first
            RemoveRangeIndices(rangeBeginIndex, rangeEndIndex);
            if (newEndRangeMax >= 0)
                AddNonOverlappingRange(rangeBeginIndex, end + 1, newEndRangeMax);
            if (newBeginRangeMin >= 0)
                AddNonOverlappingRange(rangeBeginIndex, newBeginRangeMin, begin - 1);
        }

        /// <summary>
        /// Removes all items from the set</summary>
        public void Clear()
        {
            AttemptModify();
            m_ranges.Clear();
            m_count = 0;
        }

        /// <summary>
        /// Determines whether the set contains a specific integer</summary>
        /// <param name="item">The integer to locate in the set</param>
        /// <returns><c>True</c> if the integer is found</returns>
        public bool Contains(int item)
        {
            return FindRange(item) >= 0;
        }

        /// <summary>
        /// Determines whether the given integer is contained within this set, and if so, gives the index (zero-based)
        /// of the integer within this sorted set</summary>
        /// <param name="item">Integer to look for</param>
        /// <param name="index">Zero-based index of the given integer within the sorted set or -1 if not found</param>
        /// <returns><c>True</c> if the integer is found</returns>
        public bool Contains(int item, out int index)
        {
            int rangeIndex = FindRange(item);
            if (rangeIndex >= 0)
            {
                UpdateRangeCounts();
                Range range = m_ranges[rangeIndex];
                index = range.PreviousItemsCount + (item - range.Min);
                return true;
            }
            else
            {
                index = -1;
                return false;
            }
        }

        /// <summary>
        /// Copies the elements of the set to an array, starting at a particular array index</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements
        /// copied from set. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">ArrayIndex is less than 0</exception>
        /// <exception cref="T:System.ArgumentNullException">Array is null</exception>
        /// <exception cref="T:System.ArgumentException">Array is multidimensional.
        /// -or-arrayIndex is equal to or greater than the length of array.
        /// -or-The number of elements in the source set is greater than the available space from arrayIndex to the end of the destination array.
        /// -or-Type T cannot be cast automatically to the type of the destination array.</exception>
        public void CopyTo(int[] array, int arrayIndex)
        {
            int i = 0;
            foreach (int value in this)
                array[i++] = value;
        }

        /// <summary>
        /// Gets the number of elements contained in the set</summary>
        public int Count
        {
            get { return m_count; }
        }

        /// <summary>
        /// Gets a value indicating whether the set is read-only</summary>
        public bool IsReadOnly
        {
            get { return m_locked; }
        }

        /// <summary>
        /// A one-way lock. After calling, IsReadOnly is true and this set of integers cannot be modified</summary>
        public void Lock()
        {
            m_locked = true;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the integers of the set</summary>
        /// <returns>Enumerator that iterates through the integers of the set</returns>
        public IEnumerator<int> GetEnumerator()
        {
            return new RangeIterator(this);
        }

        /// <summary>
        /// Gets the sorted enumeration of all the ranges of integers</summary>
        public IEnumerable<Range> Ranges
        {
            get
            {
                UpdateRangeCounts();
                return m_ranges;
            }
        }

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the integers of the set</summary>
        /// <returns>Enumerator that iterates through the integers of the set</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new RangeIterator(this);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Find the range containing value, or insertion point for new range</summary>
        /// <returns>Index of range containing value, or negative ordinal (index + 1) of new range insertion point</returns>
        private int FindRange(int value)
        {
            int low = 0;
            int high = m_ranges.Count - 1;
            while (low <= high)
            {
                int middle = (low + high) / 2;
                Range range = m_ranges[middle];

                if (range.Contains(value))
                {
                    return middle;
                }
                else if (range.Min > value)
                {
                    high = middle - 1;
                }
                else if (range.Max < value)
                {
                    low = middle + 1;
                }
                else
                {
                    return -middle;
                }
            }

            return -(low + 1);
        }

        // Finds the range indices that contain any members of the range [begin,end]. Outputs [-1,-1] if
        //  no existing ranges are found in which case 'newRangeIndex' will indicate where the new range
        //  should be inserted. If any overlapping ranges are found, newRangeIndex will be -1.
        private void FindRanges(
            int begin, int end,
            out int rangeBeginIndex, out int rangeEndIndex,
            out int newRangeIndex)
        {
            rangeBeginIndex = -1;
            rangeEndIndex = -1;
            newRangeIndex = 0;

            if (m_ranges.Count == 0)
                return;

            // Find the index of the first range that overlaps with [begin,end], or return
            //  [-1,-1] if there is no overlap.
            int suggestedBegin = FindRange(begin);
            if (suggestedBegin < 0)
            {
                // This negative value is the ordinal range # of where 'begin' should be placed.
                //  Subtract 1 to get index.
                suggestedBegin = -suggestedBegin - 1;

                // If 'begin' is past the last range, then 'end' is too.
                // If 'end' is before the next one, then we're done.
                if (suggestedBegin >= m_ranges.Count ||
                    end < m_ranges[suggestedBegin].Min)
                {
                    newRangeIndex = suggestedBegin;
                    return;
                }
            }

            // There's definitely an overlap. Find end index.
            rangeBeginIndex = suggestedBegin;
            int suggestedEnd = FindRange(end);
            if (suggestedEnd < 0)
            {
                // Get the index of the next range, as if we were going to insert a new one, then
                //  subtract 1 to get the next lower.
                suggestedEnd = -suggestedEnd - 1 - 1;
            }
            rangeEndIndex = suggestedEnd;
            newRangeIndex = -1;
        }

        // Removes the given range of ranges.
        private void RemoveRangeIndices(int rangeBeginIndex, int rangeEndIndex)
        {
            int numRemoved = 0;
            for (int i = rangeBeginIndex; i <= rangeEndIndex; i++)
                numRemoved += m_ranges[i].Count;
            m_count -= numRemoved;

            m_ranges.RemoveRange(rangeBeginIndex, rangeEndIndex - rangeBeginIndex + 1);
        }

        // Directly adds the given range that must not include any existing members of the set.
        // Merges with adjacent ranges if possible. Updates count.
        private void AddNonOverlappingRange(int insertionIndex, int begin, int end)
        {
            m_count += (end - begin + 1);

            int rangeBelowIndex = FindRange(begin - 1);
            int rangeAboveIndex = FindRange(end + 1);

            if (rangeBelowIndex >= 0)
            {
                Range below = m_ranges[rangeBelowIndex];
                if (rangeAboveIndex >= 0)
                {
                    m_ranges[rangeBelowIndex] = new Range(below.Min, m_ranges[rangeAboveIndex].Max);
                    m_ranges.RemoveAt(rangeAboveIndex);
                }
                else
                {
                    m_ranges[rangeBelowIndex] = new Range(below.Min, end);
                }
            }
            else if (rangeAboveIndex >= 0)
            {
                m_ranges[rangeAboveIndex] = new Range(begin, m_ranges[rangeAboveIndex].Max);
            }
            else
            {
                m_ranges.Insert(insertionIndex, new Range(begin, end));
            }
        }

        // Does a lazy update of the PreviousItemsCount member of the ranges.
        private void UpdateRangeCounts()
        {
            if (m_dirtyRanges)
            {
                int previousItemsCount = 0;
                for (int i = 0; i < m_ranges.Count; i++)
                {
                    Range range = m_ranges[i];
                    range.PreviousItemsCount = previousItemsCount;
                    previousItemsCount += range.Count;
                }
                m_dirtyRanges = false;
            }
        }

        // Call from the public methods, prior to modifying the set.
        private void AttemptModify()
        {
            if (IsReadOnly)
                throw new NotSupportedException("this set is read-only");
            m_dirtyRanges = true;
        }

        #endregion

        #region Child Classes

        /// <summary>
        /// Represents a range of consecutive integers within the set and for indicating the placement
        /// of this range within the sorted set</summary>
        public class Range
        {
            /// <summary>
            /// A constructor for a range consisting of just one integer</summary>
            /// <param name="value">The only integer in this range</param>
            public Range(int value)
            {
                m_min = value;
                m_max = value;
                m_previousItemsCount = 0;
            }

            /// <summary>
            /// A constructor for a range of integers</summary>
            /// <param name="min">Lowest integer in the range</param>
            /// <param name="max">Highest integer in the range</param>
            public Range(int min, int max)
            {
                m_min = min;
                m_max = max;
                m_previousItemsCount = 0;
            }

            /// <summary>
            /// Gets the minimum integer in this range</summary>
            public int Min
            {
                get { return m_min; }
                internal set { m_min = value; }
            }

            /// <summary>
            /// Gets the maximum integer in this range</summary>
            public int Max
            {
                get { return m_max; }
                internal set { m_max = value; }
            }

            /// <summary>
            /// Checks if the given integer is within this range</summary>
            /// <param name="value">The integer to test for</param>
            /// <returns><c>True</c> if the integer is contained within this range</returns>
            public bool Contains(int value)
            {
                return
                    value >= m_min &&
                    value <= m_max;
            }

            /// <summary>
            /// Gets the number of integers in this range</summary>
            public int Count
            {
                get { return m_max - m_min + 1; }
            }

            /// <summary>
            /// Gets the number of integers in all of the prior ranges within the sorted set</summary>
            public int PreviousItemsCount
            {
                get { return m_previousItemsCount; }
                internal set { m_previousItemsCount = value; }
            }

            private int m_min;
            private int m_max;
            private int m_previousItemsCount;
        }

        private class RangeIterator : IEnumerator<int>
        {
            public RangeIterator(IntSet set)
            {
                m_set = set;
                m_rangeIndex = -1;
            }

            #region IEnumerator<int> Members

            public int Current
            {
                get
                {
                    if (m_rangeIndex < 0)
                        throw new InvalidOperationException("Enumerator not valid");
                    return m_current;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                if (m_rangeIndex < 0)
                {
                    if (m_set.m_ranges.Count == 0)
                        return false;

                    m_rangeIndex = 0;
                    m_current = m_set.m_ranges[m_rangeIndex].Min;
                }
                else
                {
                    Range range = m_set.m_ranges[m_rangeIndex];
                    if (range.Max == m_current)
                    {
                        m_rangeIndex++;
                        if (m_set.m_ranges.Count == m_rangeIndex)
                        {
                            m_rangeIndex = -1;
                            return false;
                        }
                        m_current = m_set.m_ranges[m_rangeIndex].Min;
                    }
                    else
                    {
                        m_current++;
                    }
                }
                return true;
            }

            public void Reset()
            {
                m_rangeIndex = -1;
            }

            #endregion

            private readonly IntSet m_set;
            private int m_rangeIndex;
            private int m_current;
        }

        #endregion

        private readonly List<Range> m_ranges = new List<Range>();
        private int m_count;
        private bool m_dirtyRanges;
        private bool m_locked;
    }
}
