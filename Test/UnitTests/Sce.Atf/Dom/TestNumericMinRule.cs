//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using NUnit.Framework;

using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestNumericMinRule
    {
        [Test]
        public void TestMinInclusive()
        {
            NumericMinRule rule = new NumericMinRule(1, true);

            TestInclusive<SByte>(rule, 2, 1, 0);
            TestInclusive<Byte>(rule, 2, 1, 0);
            TestInclusive<Int16>(rule, 2, 1, 0);
            TestInclusive<UInt16>(rule, 2, 1, 0);
            TestInclusive<Int32>(rule, 2, 1, 0);
            TestInclusive<UInt32>(rule, 2, 1, 0);
            TestInclusive<Int64>(rule, 2, 1, 0);
            TestInclusive<UInt64>(rule, 2, 1, 0);
            TestInclusive<Single>(rule, 2, 1, 0);
            TestInclusive<Double>(rule, 2, 1, 0);
        }

        private void TestInclusive<T>(NumericMinRule rule, T valid, T equal, T invalid)
        {
            Assert.True(rule.Validate((T)valid, null));
            Assert.True(rule.Validate((T)equal, null));
            Assert.False(rule.Validate((T)invalid, null));
        }

        [Test]
        public void TestMinExclusive()
        {
            NumericMinRule rule = new NumericMinRule(1, false);

            TestExclusive<SByte>(rule, 2, 1, 0);
            TestExclusive<Byte>(rule, 2, 1, 0);
            TestExclusive<Int16>(rule, 2, 1, 0);
            TestExclusive<UInt16>(rule, 2, 1, 0);
            TestExclusive<Int32>(rule, 2, 1, 0);
            TestExclusive<UInt32>(rule, 2, 1, 0);
            TestExclusive<Int64>(rule, 2, 1, 0);
            TestExclusive<UInt64>(rule, 2, 1, 0);
            TestExclusive<Single>(rule, 2, 1, 0);
            TestExclusive<Double>(rule, 2, 1, 0);
        }

        private void TestExclusive<T>(NumericMinRule rule, T valid, T equal, T invalid)
        {
            Assert.True(rule.Validate((T)valid, null));
            Assert.False(rule.Validate((T)equal, null));
            Assert.False(rule.Validate((T)invalid, null));
        }
    }
}
