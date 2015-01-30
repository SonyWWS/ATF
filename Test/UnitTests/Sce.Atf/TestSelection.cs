//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestSelection
    {
        [Test]
        public void TestIEnumerable()
        {
            Selection<object> test = new Selection<object>();
            CollectionAssert.IsEmpty(test);
            test.Add("a");
            Utilities.TestSequenceEqual(test, "a");
            test.Add("b");
            Utilities.TestSequenceEqual(test, "a", "b");
        }

        [Test]
        public void TestCount()
        {
            Selection<object> test = new Selection<object>();
            Assert.True(test.Count == 0);
            test.Add("a");
            Assert.True(test.Count == 1);
            test.Add("b");
            Assert.True(test.Count == 2);
        }

        [Test]
        public void TestAdd()
        {
            Selection<object> test = new Selection<object>();
            test.Add("a");
            Utilities.TestSequenceEqual(test, "a");
            test.Add("b");
            Utilities.TestSequenceEqual(test, "a", "b");

            // On 1/12/2012, contractor Brandon Ehle wanted to use Selection<int>. I agreed that it would be
            //  useful because GridView was doing a lot of casting to 'object' and boxing ints. By changing
            //  Selection<T> to work with value types, I removed the requirement that 'null' not be added
            //  so that value types and reference types are treated equivalently, and to allow for default
            //  value-types to be added. For example, I think Selection<int> should be able to hold 0.
            //  --Ron Little
            //Assert.Throws<ArgumentNullException>(delegate() { test.Add(null); });
        }

        [Test]
        public void TestRemove()
        {
            Selection<object> test = new Selection<object>();
            test.Add("a");
            test.Add("b");
            Assert.False(test.Remove("c"));
            Assert.True(test.Remove("a"));
            Utilities.TestSequenceEqual(test, "b");
            Assert.False(test.Remove("a"));
            Assert.True(test.Remove("b"));
            CollectionAssert.IsEmpty(test);
        }

        [Test]
        public void TestReadonly()
        {
            Selection<object> test = new Selection<object>();
            Assert.False(test.IsReadOnly);
        }

        [Test]
        public void TestContains()
        {
            Selection<object> test = new Selection<object>();
            Assert.False(test.Contains("a"));
            test.Add("a");
            Assert.True(test.Contains("a"));
            test.Add("b");
            Assert.True(test.Contains("b"));
        }

        [Test]
        public void TestClear()
        {
            Selection<object> test = new Selection<object>();
            test.Add("a");
            test.Add("b");
            test.Clear();
            CollectionAssert.IsEmpty(test);
        }

        [Test]
        public void TestCopyTo()
        {
            Selection<object> test = new Selection<object>();
            test.Add("a");
            test.Add("b");
            object[] array1 = new object[2];
            test.CopyTo(array1, 0);
            Assert.AreEqual(array1, new object[] { "a", "b" });
            object[] array2 = new object[3];
            test.CopyTo(array2, 1);
            Assert.AreEqual(array2, new object[] { null, "a", "b" });
        }

        [Test]
        public void TestIndexOf()
        {
            Selection<object> test = new Selection<object>();
            Assert.True(test.IndexOf("a") == -1);
            test.Add("a");
            Assert.True(test.IndexOf("a") == 0);
            Assert.True(test.IndexOf("b") == -1);
            test.Add("b");
            Assert.True(test.IndexOf("b") == 1);
        }

        [Test]
        public void TestRemoveAt()
        {
            Selection<object> test = new Selection<object>();
            test.Add("a");
            test.Add("b");
            Assert.Throws<ArgumentOutOfRangeException>(delegate() { test.RemoveAt(2); });
            test.RemoveAt(1);
            Utilities.TestSequenceEqual(test, "a");
            Assert.Throws<ArgumentOutOfRangeException>(delegate() { test.RemoveAt(1); });
            test.RemoveAt(0);
            CollectionAssert.IsEmpty(test);
            Assert.Throws<ArgumentOutOfRangeException>(delegate() { test.RemoveAt(0); });
        }

        [Test]
        public void TestIndexer()
        {
            Selection<object> test = new Selection<object>();
            Assert.Throws<ArgumentOutOfRangeException>(delegate() { object temp = test[0]; });
            test.Add("a");
            test.Add("b");
            Assert.AreEqual(test[0], "a");
            Assert.AreEqual(test[1], "b");
            Assert.Throws<ArgumentOutOfRangeException>(delegate() { object temp = test[2]; });

            test[0] = "c";
            Assert.AreEqual(test[0], "c");
            test[1] = "d";
            Assert.AreEqual(test[1], "d");
        }

        [Test]
        public void TestInsert()
        {
            Selection<object> test = new Selection<object>();
            test.Insert(0, "a");
            Utilities.TestSequenceEqual(test, "a");
            test.Insert(0, "b");
            Utilities.TestSequenceEqual(test, "b", "a");
            Assert.Throws<ArgumentOutOfRangeException>(delegate() { test.Insert(-1, "c"); });
            Assert.Throws<ArgumentOutOfRangeException>(delegate() { test.Insert(3, "c"); });
            test.Insert(1, "c");
            Utilities.TestSequenceEqual(test, "b", "c", "a");
        }

        [Test]
        public void TestUniqueness()
        {
            Selection<object> test = new Selection<object>();
            test.Add("a");
            test.Add("a");
            Utilities.TestSequenceEqual(test, "a");
            object b = new object();
            test.Add("b");
            Utilities.TestSequenceEqual(test, "a", "b");
            test[1] = "a"; // overwrite b with a, should also remove first instance of a
            Utilities.TestSequenceEqual(test, "a");
        }

        [Test]
        public void TestLastSelected()
        {
            Selection<object> test = new Selection<object>();
            Assert.Null(test.LastSelected);
            test.Add("a");
            Assert.AreSame(test.LastSelected, "a");
            test.Add("b");
            Assert.AreSame(test.LastSelected, "b");
            test.Remove("b");
            Assert.AreSame(test.LastSelected, "a");
        }

        [Test]
        public void TestGetLastSelected()
        {
            Selection<object> test = new Selection<object>();
            Assert.Null(test.GetLastSelected<string>());
            test.Add("a");
            Assert.AreSame(test.GetLastSelected<string>(), "a");
            test.Add(this); // any non-string
            Assert.AreSame(test.GetLastSelected<string>(), "a");
            Assert.AreSame(test.GetLastSelected<TestSelection>(), this);
        }

        [Test]
        public void TestGetSnapshot()
        {
            Selection<object> test = new Selection<object>();
            CollectionAssert.IsEmpty(test.GetSnapshot());
            test.Add("a");
            object[] snapshot = test.GetSnapshot();
            Assert.AreEqual(snapshot, new object[] { "a" });
            test.Add("b");
            snapshot = test.GetSnapshot();
            Assert.AreEqual(snapshot, new object[] { "a", "b" });
        }

        [Test]
        public void TestGetSnapshotGeneric()
        {
            Selection<object> test = new Selection<object>();
            CollectionAssert.IsEmpty(test.GetSnapshot());
            test.Add("a");
            test.Add(this);
            Assert.AreEqual(test.GetSnapshot<object>(), new object[] { "a", this });
            Assert.AreEqual(test.GetSnapshot<string>(), new string[] { "a" });
        }

        [Test]
        public void TestAsIEnumerable()
        {
            Selection<object> test = new Selection<object>();
            CollectionAssert.IsEmpty(test.AsIEnumerable<object>());
            test.Add("a");
            test.Add(this);
            Utilities.TestSequenceEqual(test.AsIEnumerable<string>(), "a");
            Utilities.TestSequenceEqual(test.AsIEnumerable<object>(), "a", this);
        }

        [Test]
        public void TestChangeEvents()
        {
            Selection<object> test = new Selection<object>();
            test.Changing += test_Changing;
            test.Changed += test_Changed;
            test.Add("a");
            Assert.True(m_changedEvents == 1);
            test.Add("a");
            Assert.True(m_changedEvents == 1); // no change!
            test.Add("b");
            Assert.True(m_changedEvents == 2);
            test[0] = "b";
            Assert.True(m_changedEvents == 3);
        }

        private void test_Changing(object sender, EventArgs e)
        {
            m_changingEvents++;
            Assert.True(m_changingEvents > m_changedEvents);
        }
        private int m_changingEvents = 0;

        private void test_Changed(object sender, EventArgs e)
        {
            m_changedEvents++;
            Assert.True(m_changingEvents == m_changedEvents);
        }
        private int m_changedEvents = 0;

        [Test]
        public void TestItemsChangedEvents()
        {
            Selection<object> test = new Selection<object>();
            test.ItemsChanged += test_ItemsChanged;
            test.Add("a");
            Assert.NotNull(ItemsChangedEventArgs);
            Utilities.TestSequenceEqual(ItemsChangedEventArgs.AddedItems, "a");
            CollectionAssert.IsEmpty(ItemsChangedEventArgs.RemovedItems);
            CollectionAssert.IsEmpty(ItemsChangedEventArgs.ChangedItems);

            ItemsChangedEventArgs = null;
            test.Set("b");
            Assert.NotNull(ItemsChangedEventArgs);
            Utilities.TestSequenceEqual(ItemsChangedEventArgs.AddedItems, "b");
            Utilities.TestSequenceEqual(ItemsChangedEventArgs.RemovedItems, "a");
            CollectionAssert.IsEmpty(ItemsChangedEventArgs.ChangedItems);
        }

        void test_ItemsChanged(object sender, ItemsChangedEventArgs<object> e)
        {
            ItemsChangedEventArgs = e;
        }
        private ItemsChangedEventArgs<object> ItemsChangedEventArgs;
    }
}
