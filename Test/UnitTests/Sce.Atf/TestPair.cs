//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestPair
    {
        [Test]
        public void TestConstructor()
        {
            Pair<string, string> test = new Pair<string, string>("a", "b");
            Assert.AreSame(test.First, "a");
            Assert.AreSame(test.Second, "b");
        }

        [Test]
        public void TestKvpConstructor()
        {
            Pair<string, string> test = new Pair<string, string>(new KeyValuePair<string, string>("a", "b"));
            Assert.AreSame(test.First, "a");
            Assert.AreSame(test.Second, "b");
        }

        [Test]
        public void TestEquals()
        {
            Pair<string, string> test = new Pair<string, string>("a", "b");
            Assert.False(test.Equals((object)null));
            Assert.False(test.Equals(this));
            Assert.True(test.Equals((object)test));
            Assert.True(test.Equals((object)new Pair<string, string>("a", "b")));
        }

        [Test]
        public void TestEqualsPair()
        {
            Pair<string, string> test = new Pair<string, string>("a", "b");
            Assert.True(test.Equals(test));
            Assert.True(test.Equals(new Pair<string, string>("a", "b")));
        }

        [Test]
        public void TestGetHashCode()
        {
            Pair<string, string> test = new Pair<string, string>("a", "b");
            Assert.AreEqual(test.GetHashCode(), "a".GetHashCode() ^ "b".GetHashCode());
            Pair<string, string> oneNull = new Pair<string, string>("a", null);
            Assert.AreNotEqual(oneNull.GetHashCode(), "a".GetHashCode());
            Pair<string, string> twoNulls = new Pair<string, string>();
            Assert.AreNotEqual(twoNulls.GetHashCode(), 0);
        }

        [Test]
        public void TestToString()
        {
            Pair<string, string> test = new Pair<string, string>("a", "b");
            Assert.AreEqual(test.ToString(), "a b");
            Pair<string, string> oneNull = new Pair<string, string>("a", null);
            Assert.AreEqual(oneNull.ToString(), "a null");
            Pair<string, string> twoNulls = new Pair<string, string>();
            Assert.AreEqual(twoNulls.ToString(), "null null");
        }

        [Test]
        public void TestCompareToPair()
        {
            Pair<int, int> test = new Pair<int, int>(1, 2);
            Assert.True(test.CompareTo(test) == 0);
            Pair<int, int> below = new Pair<int, int>(1, 1);
            Assert.True(below.CompareTo(test) < 0);
            Pair<int, int> above = new Pair<int, int>(2, 1);
            Assert.True(above.CompareTo(test) > 0);
        }

        [Test]
        public void TestToKvp()
        {
            Pair<int, int> test = new Pair<int, int>(1, 2);
            KeyValuePair<int, int> kvp = test.ToKeyValuePair();
            Assert.AreEqual(kvp, new KeyValuePair<int, int>(1, 2));
        }
    }
}
