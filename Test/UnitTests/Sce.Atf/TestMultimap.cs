//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestMultimap
    {
        [Test]
        public void TestWithValueType()
        {
            var m = new Multimap<int, int>();

            m.Add(0, 0);
            Assert.True(m.ContainsKey(0));
            Assert.True(m.ContainsKeyValue(0, 0));
            Assert.True(m.FindFirst(0) == 0);
            Assert.True(m.FindFirst(-1) == default(int));

            m.Add(0, 1);
            Utilities.TestSequenceEqual(m.Find(0), new[] {0, 1 });
            Assert.True(m.ContainsKey(0));
            Assert.True(m.ContainsKeyValue(0, 0));
            Assert.True(m.ContainsKey(0));
            Assert.True(m.ContainsKeyValue(0, 1));

            m.Add(0, 2);
            Utilities.TestSequenceEqual(m.Find(0), new[] { 0, 1, 2 });

            m.Add(100, 0);
            Utilities.TestSequenceEqual(m.Find(0), new[] { 0, 1, 2 });
            Utilities.TestSequenceEqual(m.Find(100), new[] { 0 });

            m.Add(100, 1);
            Utilities.TestSequenceEqual(m.Find(0), new[] { 0, 1, 2 });
            Utilities.TestSequenceEqual(m.Find(100), new[] { 0, 1 });

            // Add a duplicate value for a key
            m.Add(100, 0);
            Utilities.TestSequenceEqual(m.Find(0), new[] { 0, 1, 2 });
            Utilities.TestSequenceEqual(m.Find(100), new[] { 1, 0 });//note the 0 is last

            // Add another duplicate value for a key
            m.Add(100, 1);
            Utilities.TestSequenceEqual(m.Find(0), new[] { 0, 1, 2 });
            Utilities.TestSequenceEqual(m.Find(100), new[] { 0, 1 });//note the 1 is last

            m.Add(100, 2);
            Utilities.TestSequenceEqual(m.Find(0), new[] { 0, 1, 2 });
            Utilities.TestSequenceEqual(m.Find(100), new[] { 0, 1, 2 });

            m.AddFirst(100, -1);
            Utilities.TestSequenceEqual(m.Find(0), new[] { 0, 1, 2 });
            Utilities.TestSequenceEqual(m.Find(100), new[] { -1, 0, 1, 2 });
            Assert.True(m.ContainsKeyValue(100, -1));
            Assert.True(m.ContainsKeyValue(100, 0));
            Assert.True(m.ContainsKeyValue(100, 1));
            Assert.True(m.ContainsKeyValue(100, 2));

            m.AddFirst(100, 2);
            Utilities.TestSequenceEqual(m.Find(0), new[] { 0, 1, 2 });
            Utilities.TestSequenceEqual(m.Find(100), new[] { 2, -1, 0, 1 });
            Assert.True(m.FindFirst(0)==0);
            Assert.True(m.FindFirst(100) == 2);
            Assert.True(m.FindLast(0) == 2);
            Assert.True(m.FindLast(100) == 1);
            Assert.True(m.ContainsKeyValue(100, 2));

            Assert.True(m.ContainsKey(0));
            Assert.True(m.ContainsKey(100));

            // Remove the 0 key
            Assert.True(m.Remove(0));
            Assert.False(m.ContainsKey(0));
            Assert.False(m[0].Any());
            Assert.True(m.ContainsKey(100));
            Utilities.TestSequenceEqual(m.Find(0), new int[] {});
            Utilities.TestSequenceEqual(m.Find(100), new[] { 2, -1, 0, 1 });
            Utilities.TestSequenceEqual(m.Keys, new[] { 100 });

            // Remove values from the 100 key
            Assert.True(m.Remove(100, -1));
            Assert.True(m.Remove(100, 0));
            Utilities.TestSequenceEqual(m.Find(0), new int[] { });
            Utilities.TestSequenceEqual(m.Find(100), new[] { 2, 1 });
            Assert.False(m.Remove(100, -1));
            Assert.False(m.Remove(100, 0));
            Utilities.TestSequenceEqual(m.Find(0), new int[] { });
            Utilities.TestSequenceEqual(m.Find(100), new[] { 2, 1 });
            Assert.True(m.Remove(100, 1));
            Assert.True(m.ContainsKeyValue(100, 2));
            Assert.False(m.ContainsKeyValue(100, 1));
            Assert.True(m.Remove(100, 2));
            Assert.False(m.ContainsKeyValue(100, 2));
            Assert.False(m.ContainsKey(100));
            Assert.False(m[100].Any());
            Utilities.TestSequenceEqual(m.Keys, new int[] {});
        }

        [Test]
        public void TestDoubleAddAndRemoveWithReferenceType()
        {
            var m = new Multimap<object, int>();
            var keyA = new object();
            m.Add(keyA, 1);
            m.Add(keyA, 1);
            Assert.True(m.Remove(keyA, 1));
            Assert.False(m.ContainsKey(keyA));
            Assert.False(m[keyA].Any());
            Assert.True(m.FindFirst(keyA)==0);
            Assert.True(m.FindFirst(new object())==0);
        }

        [Test]
        public void TestDoubleAddAndRemoveWithValueType()
        {
            var m = new Multimap<int, int>();
            const int keyA = 1;
            m.Add(keyA, 1);
            m.Add(keyA, 1);
            Assert.True(m.Remove(keyA, 1));
            Assert.False(m.ContainsKey(keyA));
            Assert.False(m[keyA].Any());
            Assert.True(m.FindFirst(keyA)==0);
            Assert.True(m.FindFirst(-1) == 0);
        }

        [Test]
        public void TestCustomComparer()
        {
            // Pass in a custom IEqualityComparer object to use for the keys
            var m = new Multimap<MyKey, int>(new MyKey());

            //keyA and keyB should be considered to be the same
            var keyA = new MyKey() { ID = 1 };
            var keyB = new MyKey() { ID = 1 };

            m.Add(keyA, 1);
            m.Add(keyB, 2);
            Utilities.TestSequenceEqual(m[keyA], new[] { 1, 2 });
            Utilities.TestSequenceEqual(m[keyB], new[] { 1, 2 });

            // Make sure only one can be removed
            Assert.True(m.Remove(keyA));
            Assert.False(m.Remove(keyB));

            m.Add(keyA, 1);
            m.Add(keyB, 2);

            // Make sure only one can be removed, starting with keyB
            Assert.True(m.Remove(keyB));
            Assert.False(m.Remove(keyA));

            Assert.False(m.ContainsKey(keyA));
            Assert.False(m.ContainsKey(keyB));
            Assert.False(m[keyA].Any());
            Assert.False(m[keyB].Any());
            Assert.True(m.FindFirst(keyA) == 0);
        }

        private class MyKey : IEqualityComparer<object>
        {
            public int ID;

            public new bool Equals(object objectX, object objectY)
            {
                var x = objectX as MyKey;
                if (x != null)
                {
                    var y = objectY as MyKey;
                    if (y != null)
                    {
                        return x.ID == y.ID;
                    }
                }
                return false;
            }

            public int GetHashCode(object obj)
            {
                var x = obj as MyKey;
                if (x != null)
                    return x.ID;
                return obj.GetHashCode();
            }
        }
    }
}
