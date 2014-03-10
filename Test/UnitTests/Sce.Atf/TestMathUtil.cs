//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestMathUtil
    {
        const float EPS = 0.00001f;

        [Test]
        public void TestAreApproxEqual()
        {
            Assert.True(MathUtil.AreApproxEqual(0, 0, 0));
            Assert.True(MathUtil.AreApproxEqual(1, 1 + EPS, EPS * (1 + EPS)));
            Assert.False(MathUtil.AreApproxEqual(1, 1 + EPS, EPS * (1 - EPS)));
        }

        [Test]
        public void TestClamp()
        {
            Assert.AreEqual(MathUtil.Clamp(0, -1, 1), 0);
            Assert.AreEqual(MathUtil.Clamp(0, 0, 1), 0);
            Assert.AreEqual(MathUtil.Clamp(-EPS, 0, 1), 0);
            Assert.AreEqual(MathUtil.Clamp(1, 0, 1), 1);
            Assert.AreEqual(MathUtil.Clamp(1 + EPS, 0, 1), 1);
        }

        [Test]
        public void TestClosest()
        {
            Assert.AreEqual(MathUtil.Closest(0, -1, 1 - EPS), 1 - EPS);
            Assert.AreEqual(MathUtil.Closest(0, 0, 1), 0);
            Assert.AreEqual(MathUtil.Closest(-EPS, 0, 1), 0);
            Assert.AreEqual(MathUtil.Closest(1, 0, 1), 1);
            Assert.AreEqual(MathUtil.Closest(1 + EPS, 0, 1), 1);
        }

        [Test]
        public void TestRemainder()
        {
            Assert.AreEqual(MathUtil.Remainder(4, 2), 0);
            Assert.AreEqual(MathUtil.Remainder(3, 2), 1);
            Assert.AreEqual(MathUtil.Remainder(-3, 2), 1);
            Assert.AreEqual(MathUtil.Remainder(3, -2), -1);
            Assert.AreEqual(MathUtil.Remainder(0, 2), 0);
            Assert.AreEqual(MathUtil.Remainder(0, -2), -0);
        }

        [Test]
        public void TestInterp()
        {
            Assert.True(MathUtil.AreApproxEqual(MathUtil.Interp(0, 0, 1), 0, EPS));
            Assert.True(MathUtil.AreApproxEqual(MathUtil.Interp(1, 0, 1), 1, EPS));
            Assert.True(MathUtil.AreApproxEqual(MathUtil.Interp(0.333f, 0, 1), 0.333f, EPS));
        }

        [Test]
        public void TestLogBase2()
        {
            Assert.AreEqual(MathUtil.LogBase2(1), 0);
            Assert.AreEqual(MathUtil.LogBase2(4), 2);
            Assert.AreEqual(MathUtil.LogBase2(15), 3);
            Assert.AreEqual(MathUtil.LogBase2(16), 4);
            Assert.AreEqual(MathUtil.LogBase2(1 << 30), 30);
        }

        [Test]
        public void TestOnlyOneBitSet()
        {
            Assert.True(MathUtil.OnlyOneBitSet(1));
            Assert.True(MathUtil.OnlyOneBitSet(256));
            Assert.False(MathUtil.OnlyOneBitSet(3));
            Assert.False(MathUtil.OnlyOneBitSet(0));
        }

        [Test]
        public void TestRemap()
        {
            Assert.True(MathUtil.AreApproxEqual(MathUtil.Remap(0, 0, 1, 2, 3), 2, EPS));
            Assert.True(MathUtil.AreApproxEqual(MathUtil.Remap(1, 0, 1, 2, 3), 3, EPS));
            Assert.True(MathUtil.AreApproxEqual(MathUtil.Remap(0.333f, 0, 1, 2, 3), 2.333f, EPS));
        }

        [Test]
        public void TestReverseInterp()
        {
            Assert.True(MathUtil.AreApproxEqual(MathUtil.ReverseInterp(0, 0, 1), 0, EPS));
            Assert.True(MathUtil.AreApproxEqual(MathUtil.ReverseInterp(1, 0, 1), 1, EPS));
            Assert.True(MathUtil.AreApproxEqual(MathUtil.ReverseInterp(0.333f, 0, 1), 0.333f, EPS));
        }

        [Test]
        public void TestSnap()
        {
            Assert.True(MathUtil.AreApproxEqual(MathUtil.Snap(0.0, 1.0), 0, EPS));
            Assert.True(MathUtil.AreApproxEqual(MathUtil.Snap(0.5 - EPS, 1.0), 0, EPS));
            Assert.True(MathUtil.AreApproxEqual(MathUtil.Snap(0.5 + EPS, 1.0), 1, EPS));
            // int overload
            Assert.AreEqual(MathUtil.Snap(0, 1), 0);
            Assert.AreEqual(MathUtil.Snap(1, 4), 0);
            Assert.AreEqual(MathUtil.Snap(3, 4), 4);
        }

        [Test]
        public void TestWrap()
        {
            Assert.AreEqual(MathUtil.Wrap(0, 0, 1), 0);
            Assert.AreEqual(MathUtil.Wrap(1, 0, 4), 1);
            Assert.AreEqual(MathUtil.Wrap(5, 0, 4), 1);
        }
    }
}
