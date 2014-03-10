//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestPath
    {
        [Test]
        public void TestConstructor()
        {
            Path<object> test = new Path<object>("a");
            Assert.AreEqual(test.First, "a");
            Assert.AreEqual(test.Last, "a");
            Assert.AreEqual(test.Count, 1);
            Utilities.TestSequenceEqual<object>(test, "a");
        }

        [Test]
        public void TestEnumerableConstructor()
        {
            object[] array = new object[] { "a", "b" };
            Path<object> test = new Path<object>((IEnumerable<object>)array);
            Assert.AreEqual(test.First, "a");
            Assert.AreEqual(test.Last, "b");
            Assert.AreEqual(test.Count, 2);
            Utilities.TestSequenceEqual<object>(test, array);
        }

        [Test]
        public void TestCollectionConstructor()
        {
            object[] array = new object[] { "a", "b" };
            Path<object> test = new Path<object>(array);
            Assert.AreEqual(test.First, "a");
            Assert.AreEqual(test.Last, "b");
            Assert.AreEqual(test.Count, 2);
            Utilities.TestSequenceEqual<object>(test, array);
        }
    }
}
