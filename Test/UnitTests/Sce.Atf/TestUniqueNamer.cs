//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestUniqueNamer
    {
        [Test]
        public void TestName()
        {
            UniqueNamer test = new UniqueNamer();

            Assert.AreEqual(test.Name("a"), "a");
            Assert.AreEqual(test.Name("a1"), "a1");
            Assert.AreEqual(test.Name("a"), "a_1");
            Assert.AreEqual(test.Name("a_1"), "a_2");
        }

        [Test]
        public void TestRetire()
        {
            UniqueNamer test = new UniqueNamer();

            test.Name("a");
            string a_1 = test.Name("a"); // should be a_1
            test.Retire("a");
            Assert.AreEqual(test.Name("a"), "a");
            Assert.AreEqual(test.Name("a"), "a_2");
        }

        [Test]
        public void TestChange()
        {
            UniqueNamer test = new UniqueNamer();

            test.Name("a");
            test.Change("a", "b");

            Assert.AreEqual(test.Name("a"), "a");
            Assert.AreEqual(test.Name("b"), "b_1");
        }

        [Test]
        public void TestMinNumDigits()
        {
            UniqueNamer test = new UniqueNamer('_', 5);

            test.Name("a");
            Assert.AreEqual(test.Name("a"), "a_00001");

            Assert.Throws<NotSupportedException>(delegate { UniqueNamer bad = new UniqueNamer('_', 11); });
        }

        [Test]
        public void TestSuffixes()
        {
            UniqueNamer testSpace = new UniqueNamer(' ');
            Assert.AreEqual(testSpace.Name("a"), "a");
            Assert.AreEqual(testSpace.Name("a"), "a 1");

            UniqueNamer testDash = new UniqueNamer('-');
            Assert.AreEqual(testDash.Name("a"), "a");
            Assert.AreEqual(testDash.Name("a"), "a-1");

            UniqueNamer testForwardSlash = new UniqueNamer('/');
            Assert.AreEqual(testForwardSlash.Name("a"), "a");
            Assert.AreEqual(testForwardSlash.Name("a"), "a/1");

            UniqueNamer testBackSlash = new UniqueNamer('\\');
            Assert.AreEqual(testBackSlash.Name("a"), "a");
            Assert.AreEqual(testBackSlash.Name("a"), "a\\1");

            UniqueNamer testParens = new UniqueNamer('(');
            Assert.AreEqual(testParens.Name("a"), "a");
            Assert.AreEqual(testParens.Name("a"), "a(1)");

            Assert.Throws<ArgumentException>(delegate { UniqueNamer bad = new UniqueNamer('@'); });
        }
    }
}
