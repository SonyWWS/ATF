//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

using NUnit.Framework;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestEmptyArray
    {
        [Test]
        public void TestInstance()
        {
            int[] test = EmptyArray<int>.Instance;
            Assert.NotNull(test);
            Assert.True(test.Length == 0);
        }
    }
}
