//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;

using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestStringEnumRule
    {
        [Test]
        public void TestValidate()
        {
            StringEnumRule test = new StringEnumRule(new string[] { "a", "b" });
            Assert.True(test.Validate("a", null));
            Assert.True(test.Validate("b", null));
            Assert.False(test.Validate("c", null));
        }
    }
}
