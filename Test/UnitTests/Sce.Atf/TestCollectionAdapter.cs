//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestCollectionAdapter
    {
        [Test]
        public void TestConstructor()
        {
            #pragma warning disable 219 //disable warning about 'test' not being used
            CollectionAdapter<object, string> test;
            #pragma warning restore 219
            Assert.Throws<ArgumentNullException>(
                delegate() { test = new CollectionAdapter<object, string>(null); });
        }

        [Test]
        public void TestIEnumerable()
        {
            object a = new object();
            List<object> list = new List<object>(new object[] { a, "a" });
            Utilities.TestSequenceEqual(new CollectionAdapter<object, object>(list), a, "a");
            Utilities.TestSequenceEqual(new CollectionAdapter<object, string>(list), null, "a");
            Utilities.TestSequenceEqual(new CollectionAdapter<object, TestCollectionAdapter>(list), null, null);
        }

        [Test]
        public void TestCount()
        {
            List<object> list = new List<object>(new object[] { "a", "b" });
            var test = new CollectionAdapter<object, string>(list);
            Assert.True(test.Count == 2);
            list.Clear();
            Assert.True(test.Count == 0);
        }

        [Test]
        public void TestAdd()
        {
            List<object> list = new List<object>();
            var test = new CollectionAdapter<object, string>(list);
            test.Add("a");
            Utilities.TestSequenceEqual(list, "a");
        }

        [Test]
        public void TestAddNull()
        {
            List<object> list = new List<object>();
            var test = new CollectionAdapter<object, string>(list);
            test.Add(null);
            Utilities.TestSequenceEqual(list, (object)null);
        }

        [Test]
        public void TestAddInvalid()
        {
            List<string> list = new List<string>();
            var test = new CollectionAdapter<string, object>(list);
            Assert.Throws<InvalidOperationException>(delegate() { test.Add(new object()); });
        }

        [Test]
        public void TestRemove()
        {
            List<object> list = new List<object>(new object[] { "a", "b" });
            var test = new CollectionAdapter<object, string>(list);
            Assert.True(test.Remove("a"));
            Utilities.TestSequenceEqual(list, "b");
        }

        [Test]
        public void TestRemoveNull()
        {
            List<object> list = new List<object>();
            var test = new CollectionAdapter<object, string>(list);
            Assert.False(test.Remove(null));
        }

        [Test]
        public void TestRemoveInvalid()
        {
            List<string> list = new List<string>();
            var test = new CollectionAdapter<string, object>(list);
            Assert.Throws<InvalidOperationException>(delegate() { test.Remove(new object()); });
        }

        [Test]
        public void TestReadonly()
        {
            List<object> list = new List<object>(new object[] { "a", "b" });
            var test = new CollectionAdapter<object, string>(list);
            Assert.False(test.IsReadOnly);
        }

        //[Test]
        //public void TestContains()
        //{
        //    Selection<object> test = new Selection<object>();
        //    Assert.False(test.Contains("a"));
        //    test.Add("a");
        //    Assert.True(test.Contains("a"));
        //    test.Add("b");
        //    Assert.True(test.Contains("b"));
        //}

        //[Test]
        //public void TestClear()
        //{
        //    Selection<object> test = new Selection<object>();
        //    test.Add("a");
        //    test.Add("b");
        //    test.Clear();
        //    Assert.Empty(test);
        //}

        //[Test]
        //public void TestCopyTo()
        //{
        //    Selection<object> test = new Selection<object>();
        //    test.Add("a");
        //    test.Add("b");
        //    object[] array1 = new object[2];
        //    test.CopyTo(array1, 0);
        //    Assert.Equal(array1, new object[] { "a", "b" });
        //    object[] array2 = new object[3];
        //    test.CopyTo(array2, 1);
        //    Assert.Equal(array2, new object[] { null, "a", "b" });
        //}

        //[Test]
        //public void TestIndexOf()
        //{
        //    Selection<object> test = new Selection<object>();
        //    Assert.True(test.IndexOf("a") == -1);
        //    test.Add("a");
        //    Assert.True(test.IndexOf("a") == 0);
        //    Assert.True(test.IndexOf("b") == -1);
        //    test.Add("b");
        //    Assert.True(test.IndexOf("b") == 1);
        //}

        //[Test]
        //public void TestRemoveAt()
        //{
        //    Selection<object> test = new Selection<object>();
        //    test.Add("a");
        //    test.Add("b");
        //    Assert.Throws<ArgumentOutOfRangeException>(delegate() { test.RemoveAt(2); });
        //    test.RemoveAt(1);
        //    Utilities.TestSequenceEqual(test, "a");
        //    Assert.Throws<ArgumentOutOfRangeException>(delegate() { test.RemoveAt(1); });
        //    test.RemoveAt(0);
        //    Assert.Empty(test);
        //    Assert.Throws<ArgumentOutOfRangeException>(delegate() { test.RemoveAt(0); });
        //}

        //[Test]
        //public void TestIndexer()
        //{
        //    Selection<object> test = new Selection<object>();
        //    Assert.Throws<ArgumentOutOfRangeException>(delegate() { object temp = test[0]; });
        //    test.Add("a");
        //    test.Add("b");
        //    Assert.Equal(test[0], "a");
        //    Assert.Equal(test[1], "b");
        //    Assert.Throws<ArgumentOutOfRangeException>(delegate() { object temp = test[2]; });

        //    test[0] = "c";
        //    Assert.Equal(test[0], "c");
        //    test[1] = "d";
        //    Assert.Equal(test[1], "d");
        //}

        //[Test]
        //public void TestInsert()
        //{
        //    Selection<object> test = new Selection<object>();
        //    test.Insert(0, "a");
        //    Utilities.TestSequenceEqual(test, "a");
        //    test.Insert(0, "b");
        //    Utilities.TestSequenceEqual(test, "b", "a");
        //    Assert.Throws<ArgumentOutOfRangeException>(delegate() { test.Insert(-1, "c"); });
        //    Assert.Throws<ArgumentOutOfRangeException>(delegate() { test.Insert(3, "c"); });
        //    test.Insert(1, "c");
        //    Utilities.TestSequenceEqual(test, "b", "c", "a");
        //}

    }
}
