//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestLazies
    {
        [Test]
        public void TestGetValues()
        {
            Lazy<object> lazy1 = new Lazy<object>();
            Lazy<object> lazy2 = new Lazy<object>();
            Lazy<object>[] test = new Lazy<object>[] { lazy1, lazy2 };
            // test static method
            Utilities.TestSequenceEqual(test.GetValues(), lazy1.Value, lazy2.Value);
            // test extension method
            Utilities.TestSequenceEqual(test.GetValues(), lazy1.Value, lazy2.Value);
        }
    }
}
