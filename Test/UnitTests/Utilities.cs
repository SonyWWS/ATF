//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace UnitTests
{
    public static class Utilities
    {
        public static void TestSequenceEqual<T>(IEnumerable<T> actual, params T[] expected)
        {
            Assert.True(Enumerable.SequenceEqual(actual, (IEnumerable<T>)expected));
        }

        public static void TestSequenceContainSameItems<T>(IEnumerable<T> actual, params T[] expected)
        {
            HashSet<T> actualSet = new HashSet<T>(actual);
            Assert.AreEqual(actualSet.Count, expected.Length);
            HashSet<T> expectedSet = new HashSet<T>(expected);
            foreach (T obj in expected)
                Assert.True(actualSet.Contains(obj));
            foreach (T obj in actual)
                Assert.True(expectedSet.Contains(obj));
        }

        //IEnumerator enum1 = actual.GetEnumerator();
        //IEnumerator enum2 = expected.GetEnumerator();
        //bool active1 = false;
        //bool active2 = false;
        //while ((active1 = enum1.MoveNext()) && (active2 = enum2.MoveNext()))
        //    if (!enum1.Current.Equals(enum2.Current))
        //        break;
        //bool equal = !active1 && !active2;

        //public static void TestEventHandler<E>(object sender, E args)
        //    where E : EventArgs
        //{
        //    bool raised = false;
        //}
    }
}
