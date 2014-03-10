//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestIntSet
    {
        // Verifies the following properties of an IntSet:
        // 1. The integers are in order.
        // 2. There are no duplicates.
        // 3. The overall count is the same as the sum of the range counts.
        // 4. There are no empty ranges.
        // 5. No two adjacent ranges can be combined.
        public void Verify(IntSet intSet)
        {
            // iterate by ranges
            int manualCount = 0;
            IntSet.Range previousRange = null;
            foreach (IntSet.Range range in intSet.Ranges)
            {
                if (previousRange != null)
                    Assert.True(previousRange.Max + 1 < range.Min);
                previousRange = range;

                Assert.True(range.Min <= range.Max);
                Assert.True(range.Count > 0);
                Assert.True(range.PreviousItemsCount == manualCount);

                manualCount += range.Count;
            }
            Assert.True(manualCount == intSet.Count);

            // iterate by integers
            manualCount = 0;
            int previousItem = int.MinValue;
            foreach (int item in intSet)
            {
                Assert.True(previousItem < item);
                previousItem = item;
                manualCount++;
            }
            Assert.True(manualCount == intSet.Count);
        }

        // This should be broken up into smaller tests someday.
        [Test]
        public void TestEverything()
        {
            IntSet intSet;
            IEnumerator<IntSet.Range> ranges;
            int index;

            // {3-5} - {4} = {3,5}
            intSet = new IntSet();
            intSet.AddRange(3, 5);
            intSet.Remove(4);
            Assert.True(intSet.Count == 2);
            ranges = intSet.Ranges.GetEnumerator();
            ranges.MoveNext();
            Assert.True(ranges.Current.Min == 3 && ranges.Current.Max == 3 && ranges.Current.PreviousItemsCount == 0);
            ranges.MoveNext();
            Assert.True(ranges.Current.Min == 5 && ranges.Current.Max == 5 && ranges.Current.PreviousItemsCount == 1);
            Verify(intSet);

            // {3,5} + {4} = {3-5}
            intSet.Add(4);
            Assert.True(intSet.Count == 3);
            ranges = intSet.Ranges.GetEnumerator();
            ranges.MoveNext();
            Assert.True(ranges.Current.Min == 3 && ranges.Current.Max == 5 && ranges.Current.PreviousItemsCount == 0);
            Assert.False(ranges.MoveNext());
            Verify(intSet);

            // {3-5} + {6-9} = {3-9}
            intSet.AddRange(6, 9);
            Assert.True(intSet.Count == 7);
            ranges = intSet.Ranges.GetEnumerator();
            ranges.MoveNext();
            Assert.True(ranges.Current.Min == 3 && ranges.Current.Max == 9 && ranges.Current.PreviousItemsCount == 0);
            Assert.False(ranges.MoveNext());
            Verify(intSet);

            // {3-9} - {4} - {5} - {3} = {6-9}
            intSet.Remove(4);
            intSet.Remove(5);
            intSet.Remove(3);
            Assert.True(intSet.Count == 4);
            ranges = intSet.Ranges.GetEnumerator();
            ranges.MoveNext();
            Assert.True(ranges.Current.Min == 6 && ranges.Current.Max == 9 && ranges.Current.PreviousItemsCount == 0);
            Assert.False(ranges.MoveNext());
            Verify(intSet);

            // {6-9} + {13-13} + {11} + {10-12} = {6-13}
            intSet.AddRange(13, 13);
            intSet.Add(11);

            Assert.True(intSet.Contains(6));
            Assert.True(intSet.Contains(7));
            Assert.True(intSet.Contains(8));
            Assert.True(intSet.Contains(9));
            Assert.True(intSet.Contains(11));
            Assert.True(intSet.Contains(13));
            Assert.True(intSet.Contains(6, out index) && index == 0);
            Assert.True(intSet.Contains(7, out index) && index == 1);
            Assert.True(intSet.Contains(8, out index) && index == 2);
            Assert.True(intSet.Contains(9, out index) && index == 3);
            Assert.True(intSet.Contains(11, out index) && index == 4);
            Assert.True(intSet.Contains(13, out index) && index == 5);
            Assert.False(intSet.Contains(5));
            Assert.False(intSet.Contains(14));
            Assert.False(intSet.Contains(10));
            Assert.False(intSet.Contains(12));

            intSet.AddRange(10, 12);

            Assert.True(intSet.Contains(6));
            Assert.True(intSet.Contains(7));
            Assert.True(intSet.Contains(8));
            Assert.True(intSet.Contains(9));
            Assert.True(intSet.Contains(10));
            Assert.True(intSet.Contains(11));
            Assert.True(intSet.Contains(12));
            Assert.True(intSet.Contains(13));
            Assert.False(intSet.Contains(5));
            Assert.False(intSet.Contains(14));

            Assert.True(intSet.Count == 8);
            ranges = intSet.Ranges.GetEnumerator();
            ranges.MoveNext();
            Assert.True(ranges.Current.Min == 6 && ranges.Current.Max == 13 && ranges.Current.PreviousItemsCount == 0);
            Assert.False(ranges.MoveNext());
            Verify(intSet);

            // {6-13} - {6-12} = {13}
            intSet.RemoveRange(6, 12);
            Assert.True(intSet.Count == 1);
            Assert.True(intSet.Contains(13));
            ranges = intSet.Ranges.GetEnumerator();
            ranges.MoveNext();
            Assert.True(ranges.Current.Min == 13 && ranges.Current.Max == 13 && ranges.Current.PreviousItemsCount == 0);
            Assert.False(ranges.MoveNext());
            Verify(intSet);

            // {13} - {13-13} = {}
            intSet.RemoveRange(13, 13);
            Assert.True(intSet.Count == 0);
            Assert.False(intSet.Contains(13));
            ranges = intSet.Ranges.GetEnumerator();
            Assert.False(ranges.MoveNext());
            Verify(intSet);

            // {} + {5-9} - {6-7} = {5,8-9}
            intSet.AddRange(5, 9);
            intSet.RemoveRange(6, 7);
            Assert.True(intSet.Count == 3);
            ranges = intSet.Ranges.GetEnumerator();
            ranges.MoveNext();
            Assert.True(ranges.Current.Min == 5 && ranges.Current.Max == 5 && ranges.Current.PreviousItemsCount == 0);
            ranges.MoveNext();
            Assert.True(ranges.Current.Min == 8 && ranges.Current.Max == 9 && ranges.Current.PreviousItemsCount == 1);
            Assert.False(ranges.MoveNext());
            Verify(intSet);

            // test Clear()
            intSet.Clear();
            Verify(intSet);

            // randomly add and remove
            Random rnd = new Random(0);
            for (int i = 0; i < 1000; i++)
            {
                int one;
                int begin;
                int end;

                begin = rnd.Next(0, 100);
                end = begin + rnd.Next(0, 7);
                intSet.AddRange(begin, end);

                one = rnd.Next(0, 100);
                intSet.Add(one);

                begin = rnd.Next(0, 100);
                end = begin + rnd.Next(0, 7);
                intSet.RemoveRange(begin, end);

                one = rnd.Next(0, 100);
                intSet.Remove(one);

                Verify(intSet);
            }

            // test locking
            intSet.Lock();
            Assert.Throws<NotSupportedException>(delegate { intSet.Add(20); });
            Assert.Throws<NotSupportedException>(delegate { intSet.AddRange(20, 20); });
            Assert.Throws<NotSupportedException>(delegate { intSet.Remove(6); });
            Assert.Throws<NotSupportedException>(delegate { intSet.RemoveRange(1, 20); });
            Verify(intSet);
        }
    }
}
