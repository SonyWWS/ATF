//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    public class TestActiveCollection
    {
        [Test]
        public void TestIEnumerable()
        {
            ActiveCollection<object> test = new ActiveCollection<object>();
            CollectionAssert.IsEmpty(test);
            test.Add("a");
            Utilities.TestSequenceEqual(test, "a");
            test.Add("b");
            Utilities.TestSequenceEqual(test, "a", "b");
        }

        [Test]
        public void TestCount()
        {
            ActiveCollection<object> test = new ActiveCollection<object>();
            Assert.True(test.Count == 0);
            test.Add("a");
            Assert.True(test.Count == 1);
            test.Add("b");
            Assert.True(test.Count == 2);
        }

        [Test]
        public void TestAdd()
        {
            ActiveCollection<object> test = new ActiveCollection<object>();
            test.Add("a");
            Utilities.TestSequenceEqual(test, "a");
            test.Add("b");
            Utilities.TestSequenceEqual(test, "a", "b");
            Assert.Throws<ArgumentNullException>(delegate() { test.Add(null); });
        }

        [Test]
        public void TestRemove()
        {
            ActiveCollection<object> test = new ActiveCollection<object>();
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
            ActiveCollection<object> test = new ActiveCollection<object>();
            Assert.False(test.IsReadOnly);
        }

        [Test]
        public void TestContains()
        {
            ActiveCollection<object> test = new ActiveCollection<object>();
            Assert.False(test.Contains("a"));
            test.Add("a");
            Assert.True(test.Contains("a"));
            test.Add("b");
            Assert.True(test.Contains("b"));
        }

        [Test]
        public void TestClear()
        {
            ActiveCollection<object> test = new ActiveCollection<object>();
            test.Add("a");
            test.Add("b");
            test.Clear();
            CollectionAssert.IsEmpty(test);
        }

        [Test]
        public void TestCopyTo()
        {
            ActiveCollection<object> test = new ActiveCollection<object>();
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
        public void TestUniqueness()
        {
            ActiveCollection<object> test = new ActiveCollection<object>();
            test.Add("a");
            test.Add("a");
            Utilities.TestSequenceEqual(test, "a");
            object b = new object();
            test.Add("b");
            Utilities.TestSequenceEqual(test, "a", "b");
        }

        [Test]
        public void TestActiveItem()
        {
            ActiveCollection<object> test = new ActiveCollection<object>();
            Assert.Null(test.ActiveItem);
            test.Add("a");
            Assert.AreSame(test.ActiveItem, "a");
            test.Add("b");
            Assert.AreSame(test.ActiveItem, "b");
            test.Remove("b");
            Assert.AreSame(test.ActiveItem, "a");
        }

        [Test]
        public void TestGetActiveItem()
        {
            ActiveCollection<object> test = new ActiveCollection<object>();
            Assert.Null(test.GetActiveItem<string>());
            test.Add("a");
            Assert.AreSame(test.GetActiveItem<string>(), "a");
            test.Add("b");
            Assert.AreSame(test.GetActiveItem<string>(), "b");
            test.Add(this); // any non-string
            Assert.AreSame(test.GetActiveItem<string>(), "b");
            Assert.AreSame(test.GetActiveItem<TestActiveCollection>(), this);
        }

        [Test]
        public void TestGetSnapshot()
        {
            ActiveCollection<object> test = new ActiveCollection<object>();
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
            ActiveCollection<object> test = new ActiveCollection<object>();
            CollectionAssert.IsEmpty(test.GetSnapshot());
            test.Add("a");
            test.Add(this);
            Assert.AreEqual(test.GetSnapshot<object>(), new object[] { "a", this });
            Assert.AreEqual(test.GetSnapshot<string>(), new string[] { "a" });
        }

        [Test]
        public void TestAsIEnumerable()
        {
            ActiveCollection<object> test = new ActiveCollection<object>();
            CollectionAssert.IsEmpty(test.AsIEnumerable<object>());
            test.Add("a");
            test.Add(this);
            Utilities.TestSequenceEqual(test.AsIEnumerable<string>(), "a");
            Utilities.TestSequenceEqual(test.AsIEnumerable<object>(), "a", this);
        }
        
        [Test]
        public void TestChangeEvents()
        {
            ActiveCollection<object> test = new ActiveCollection<object>();
            test.ActiveItemChanging += test_ActiveItemChanging;
            test.ActiveItemChanged += test_ActiveItemChanged;
            object a = new object();
            test.Add(a);
            Assert.True(m_changedEvents == 1);
            test.Add(a);
            Assert.True(m_changedEvents == 1); // no change!
            object b = new object();
            test.Add(b);
            Assert.True(m_changedEvents == 2);
        }

        private void test_ActiveItemChanging(object sender, EventArgs e)
        {
            m_changingEvents++;
            Assert.True(m_changingEvents > m_changedEvents);
        }
        private int m_changingEvents = 0;

        private void test_ActiveItemChanged(object sender, EventArgs e)
        {
            m_changedEvents++;
            Assert.True(m_changingEvents == m_changedEvents);
        }
        private int m_changedEvents = 0;
    }
}
