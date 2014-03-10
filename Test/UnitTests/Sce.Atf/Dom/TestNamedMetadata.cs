//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using NUnit.Framework;

using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestNamedMetadata
    {
        // derive a concrete child class with rudimentary parenting
        private class Metadata : NamedMetadata
        {
            public Metadata(string name)
                : this(name, null)
            {
            }

            public Metadata(string name, Metadata parent)
                : base(name)
            {
                Parent = parent;
            }

            public Metadata Parent;

            protected override NamedMetadata GetParent()
            {
                return Parent;
            }
        }

        [Test]
        public void TestConstructor()
        {
            Metadata meta = new Metadata("test");
            Assert.AreEqual(meta.Name, "test");

            Assert.Throws<ArgumentNullException>(
                delegate()
                {
                    Metadata badMeta = new Metadata(null);
                });
        }

        [Test]
        public void TestLocalTags()
        {
            Metadata meta = new Metadata("test");

            Assert.Null(meta.GetTag("foo"));

            meta.SetTag("foo", "bar");
            Assert.AreEqual(meta.GetTag("foo"), "bar");

            Assert.Null(meta.GetTag<string>());

            meta.SetTag<string>("baz");
            Assert.AreEqual(meta.GetTag<string>(), "baz");
        }

        [Test]
        public void TestInheritedTags()
        {
            Metadata parent = new Metadata("parent");
            Metadata child = new Metadata("child", parent);

            Assert.Null(child.GetTag("foo"));

            parent.SetTag("foo", "bar");
            Assert.AreEqual(child.GetTag("foo"), "bar");

            Assert.Null(child.GetTag<string>());

            parent.SetTag<string>("baz");
            Assert.AreEqual(child.GetTag<string>(), "baz");
        }
    }
}
