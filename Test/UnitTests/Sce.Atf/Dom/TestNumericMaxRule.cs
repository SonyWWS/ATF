//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Dom;

using NUnit.Framework;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestNumericMaxRule
    {
        [Test]
        public void TestMaxInclusive()
        {
            NumericMaxRule rule = new NumericMaxRule(1, true);

            TestInclusive<SByte>(rule, 0, 1, 2);
            TestInclusive<Byte>(rule, 0, 1, 2);
            TestInclusive<Int16>(rule, 0, 1, 2);
            TestInclusive<UInt16>(rule, 0, 1, 2);
            TestInclusive<Int32>(rule, 0, 1, 2);
            TestInclusive<UInt32>(rule, 0, 1, 2);
            TestInclusive<Int64>(rule, 0, 1, 2);
            TestInclusive<UInt64>(rule, 0, 1, 2);
            TestInclusive<Single>(rule, 0, 1, 2);
            TestInclusive<Double>(rule, 0, 1, 2);
        }

        private void TestInclusive<T>(NumericMaxRule rule, T valid, T equal, T invalid)
        {
            Assert.True(rule.Validate((T)valid, null));
            Assert.True(rule.Validate((T)equal, null));
            Assert.False(rule.Validate((T)invalid, null));
        }

        [Test]
        public void TestMaxExclusive()
        {
            NumericMaxRule rule = new NumericMaxRule(1, false);

            TestExclusive<SByte>(rule, 0, 1, 2);
            TestExclusive<Byte>(rule, 0, 1, 2);
            TestExclusive<Int16>(rule, 0, 1, 2);
            TestExclusive<UInt16>(rule, 0, 1, 2);
            TestExclusive<Int32>(rule, 0, 1, 2);
            TestExclusive<UInt32>(rule, 0, 1, 2);
            TestExclusive<Int64>(rule, 0, 1, 2);
            TestExclusive<UInt64>(rule, 0, 1, 2);
            TestExclusive<Single>(rule, 0, 1, 2);
            TestExclusive<Double>(rule, 0, 1, 2);
        }

        private void TestExclusive<T>(NumericMaxRule rule, T valid, T equal, T invalid)
        {
            Assert.True(rule.Validate((T)valid, null));
            Assert.False(rule.Validate((T)equal, null));
            Assert.False(rule.Validate((T)invalid, null));
        }
    }
}
