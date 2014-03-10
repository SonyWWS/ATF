//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;

namespace UnitTests.Atf.Adaptation
{
    [TestFixture]
    public class TestAdapter
    {
        [Test]
        public void TestConstructor()
        {
            SimpleAdapter test = new SimpleAdapter();
            Assert.Null(test.Adaptee);
            Assert.False(test.OnAdapteeChangedCalled);
        }

        [Test]
        public void TestAdaptee()
        {
            SimpleAdapter test = new SimpleAdapter();
            test.Adaptee = this;
            Assert.AreSame(test.Adaptee, this);
            Assert.True(test.OnAdapteeChangedCalled);
            test.Adaptee = null;
            Assert.Null(test.Adaptee);
        }

        [Test]
        public void TestAs()
        {
            SimpleAdapter test;

            test = new SimpleAdapter();
            Assert.AreSame(test.GetAdapter(typeof(ISimpleInterface)), test);
            Assert.True(test.AdaptCalled);

            test = new SimpleAdapter();
            Assert.Null(test.GetAdapter(typeof(TestAdapter)));
            Assert.True(test.AdaptCalled);

            test = new SimpleAdapter(this);
            Assert.AreSame(test.GetAdapter(typeof(TestAdapter)), this);
            Assert.True(test.AdaptCalled);
        }

        [Test]
        public void TestAsAll()
        {
            SimpleAdapter test;

            test = new SimpleAdapter();
            Utilities.TestSequenceEqual(test.GetDecorators(typeof(ISimpleInterface)), test);
            Assert.True(test.AdaptCalled);

            test = new SimpleAdapter();
            CollectionAssert.IsEmpty(test.GetDecorators(typeof(TestAdapter)));
            Assert.True(test.AdaptCalled);

            SimpleAdapter adaptee = new SimpleAdapter();
            test = new SimpleAdapter(adaptee); // wrap itself
            Utilities.TestSequenceEqual(test.GetDecorators(typeof(SimpleAdapter)), test, adaptee);
            Assert.True(test.AdaptCalled);
        }
    }
}
