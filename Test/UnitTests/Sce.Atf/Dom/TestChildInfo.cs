//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

using NUnit.Framework;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestChildInfo : DomTest
    {
        [Test]
        public void TestConstructor()
        {
            DomNodeType type = new DomNodeType("child");
            ChildInfo test = new ChildInfo("test", type);
            Assert.AreEqual(test.Name, "test");
            Assert.AreEqual(test.Type, type);
            Assert.False(test.IsList);
        }

        [Test]
        public void TestListConstructor()
        {
            DomNodeType type = new DomNodeType("child");
            ChildInfo test = new ChildInfo("test", type, true);
            Assert.AreEqual(test.Name, "test");
            Assert.AreEqual(test.Type, type);
            Assert.True(test.IsList);
        }

        [Test]
        public void TestValidation()
        {
            DomNodeType type = new DomNodeType("child");
            ChildInfo test = new ChildInfo("test", type);
            CollectionAssert.IsEmpty(test.Rules);

            var rule = new SimpleChildRule();
            test.AddRule(rule);

            Utilities.TestSequenceEqual(test.Rules, rule);

            Assert.True(test.Validate(null, null));
            Assert.True(rule.Validated);
        }
    }
}
