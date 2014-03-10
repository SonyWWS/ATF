//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using NUnit.Framework;

using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestAttributeInfo : DomTest
    {
        [Test]
        public void TestConstructor()
        {
            AttributeType type = new AttributeType("test", typeof(string));
            AttributeInfo test = new AttributeInfo("test", type);
            Assert.AreEqual(test.Name, "test");
            Assert.AreEqual(test.Type, type);
            Assert.AreEqual(test.DefaultValue, type.GetDefault());
        }

        [Test]
        public void TestDefaultValue()
        {
            AttributeType type = new AttributeType("test", typeof(string));
            AttributeInfo test = new AttributeInfo("test", type);
            test.DefaultValue = "foo";
            Assert.AreEqual(test.DefaultValue, "foo");
            test.DefaultValue = null;
            Assert.AreEqual(test.DefaultValue, type.GetDefault());
            Assert.Throws<InvalidOperationException>(delegate { test.DefaultValue = 1; });

            AttributeType length2Type = new AttributeType("length2Type", typeof(int[]), 2);
            AttributeInfo length2Info = new AttributeInfo("length2", length2Type);
            Assert.AreEqual(length2Info.DefaultValue, length2Type.GetDefault());
            Assert.AreEqual(length2Info.DefaultValue, new int[] { default(int), default(int) });
            DomNodeType nodeType = new DomNodeType("testNodeType");
            nodeType.Define(length2Info);
            DomNode node = new DomNode(nodeType);
            Assert.AreEqual(node.GetAttribute(length2Info), length2Info.DefaultValue);
            node.SetAttribute(length2Info, new int[] { 1, 2 });
            Assert.AreEqual(node.GetAttribute(length2Info), new int[] { 1, 2 });
            node.SetAttribute(length2Info, new int[] { 1 });
            Assert.AreEqual(node.GetAttribute(length2Info), new int[] { 1 });

            AttributeType length1Type = new AttributeType("length1Type", typeof(int[]), 1);
            AttributeInfo length1Info = new AttributeInfo("length1", length1Type);
            Assert.AreEqual(length1Info.DefaultValue, length1Type.GetDefault());
            Assert.AreEqual(length1Info.DefaultValue, new int[] { default(int) });
            nodeType = new DomNodeType("testNodeType");
            nodeType.Define(length1Info);
            node = new DomNode(nodeType);
            Assert.AreEqual(node.GetAttribute(length1Info), length1Info.DefaultValue);
            node.SetAttribute(length1Info, new int[] { 1 });
            Assert.AreEqual(node.GetAttribute(length1Info), new int[] { 1 });
        }

        [Test]
        public void TestValidation()
        {
            AttributeType type = new AttributeType("test", typeof(string));
            AttributeInfo test = new AttributeInfo("test", type);
            CollectionAssert.IsEmpty(test.Rules);

            var rule = new SimpleAttributeRule();
            test.AddRule(rule);

            Utilities.TestSequenceEqual(test.Rules, rule);

            Assert.True(test.Validate("bar"));
            Assert.True(rule.Validated);

            Assert.False(test.Validate(1)); // wrong type
        }
    }
}
