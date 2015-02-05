//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using NUnit.Framework;

using Sce.Atf.Adaptation;

namespace UnitTests.Atf.Adaptation
{
    [TestFixture]
    public class TestAdapters
    {
        [Test]
        public void TestAs_WithNullObject()
        {
            object test = null;

            Assert.Null(test.As(null));
            // test extension method
            Assert.Null(test.As(null));
        }

        [Test]
        public void TestAs_WithNullType()
        {
            Assert.Throws<ArgumentNullException>(delegate { this.As(null); });
        }

        [Test]
        public void TestAs_CastFirst()
        {
            SimpleAdaptable adaptable = new SimpleAdaptable();
            Assert.AreSame(adaptable.As(typeof(IAdaptable)), adaptable);
            Assert.False(adaptable.AsCalled);
        }

        [Test]
        public void TestAs_GetAdapterIfCastFails()
        {
            SimpleAdaptable adaptable = new SimpleAdaptable();
            Assert.NotNull(adaptable.As(typeof(string)));
            Assert.True(adaptable.AsCalled);
        }

        [Test]
        public void TestAs_CastAndGetAdapterFail()
        {
            SimpleAdaptable adaptable = new SimpleAdaptable();
            Assert.Null(adaptable.As(typeof(TestAdapters)));
            Assert.True(adaptable.AsCalled);
        }

        [Test]
        public void TestAsGeneric_WithNullObject()
        {
            object test = null;

            Assert.Null(test.As<object>());
            // test extension method
            Assert.Null(test.As<object>());
        }

        [Test]
        public void TestAsGeneric_CastFirst()
        {
            SimpleAdaptable adaptable = new SimpleAdaptable();
            Assert.AreSame(adaptable.As<IAdaptable>(), adaptable);
            Assert.False(adaptable.AsCalled);
        }

        [Test]
        public void TestAsGeneric_GetAdapterIfCastFails()
        {
            SimpleAdaptable adaptable = new SimpleAdaptable();
            Assert.NotNull(adaptable.As<string>());
            Assert.True(adaptable.AsCalled);
        }

        [Test]
        public void TestAsGeneric_CastAndGetAdapterFail()
        {
            SimpleAdaptable adaptable = new SimpleAdaptable();
            Assert.Null(adaptable.As<TestAdapters>());
            Assert.True(adaptable.AsCalled);
        }

        [Test]
        public void TestCast()
        {
            SimpleAdaptable adaptable = null;

            // test null check
            Assert.Throws<AdaptationException>(delegate() { adaptable.Cast(typeof(object)); });
            Assert.Throws<AdaptationException>(delegate() { adaptable.Cast(typeof(object)); });

            // test successful adaptation
            adaptable = new SimpleAdaptable();
            Assert.NotNull(adaptable.Cast(typeof(IAdaptable)));
            Assert.False(adaptable.AsCalled);
            adaptable = new SimpleAdaptable();
            Assert.NotNull(adaptable.Cast(typeof(IAdaptable)));
            Assert.False(adaptable.AsCalled);
        }

        [Test]
        public void TestCastGeneric()
        {
            SimpleAdaptable adaptable = null;

            // test null check
            Assert.Throws<AdaptationException>(delegate() { adaptable.Cast<object>(); });
            Assert.Throws<AdaptationException>(delegate() { adaptable.Cast<object>(); });

            // test successful adaptation
            adaptable = new SimpleAdaptable();
            Assert.NotNull(adaptable.Cast<IAdaptable>());
            Assert.False(adaptable.AsCalled);
            adaptable = new SimpleAdaptable();
            Assert.NotNull(adaptable.Cast<IAdaptable>());
            Assert.False(adaptable.AsCalled);
        }

        [Test]
        public void TestIs()
        {
            SimpleAdaptable adaptable = null;

            // test null check
            Assert.False(adaptable.Is(typeof(object)));
            Assert.False(adaptable.Is(typeof(object)));

            // test successful adaptation
            adaptable = new SimpleAdaptable();
            Assert.True(adaptable.Is(typeof(IAdaptable)));
            adaptable = new SimpleAdaptable();
            Assert.NotNull(adaptable.Is(typeof(IAdaptable)));

            // test failed adaptation
            adaptable = new SimpleAdaptable();
            Assert.False(adaptable.Is(typeof(TestAdapters)));
            adaptable = new SimpleAdaptable();
            Assert.False(adaptable.Is(typeof(TestAdapters)));
        }

        [Test]
        public void TestIsGeneric()
        {
            SimpleAdaptable adaptable = null;

            // test null check
            Assert.False(adaptable.Is<object>());
            Assert.False(adaptable.Is<object>());

            // test successful adaptation
            adaptable = new SimpleAdaptable();
            Assert.True(adaptable.Is<IAdaptable>());
            adaptable = new SimpleAdaptable();
            Assert.NotNull(adaptable.Is<IAdaptable>());

            // test failed adaptation
            adaptable = new SimpleAdaptable();
            Assert.False(adaptable.Is<TestAdapters>());
            adaptable = new SimpleAdaptable();
            Assert.False(adaptable.Is<TestAdapters>());
        }

        [Test]
        public void TestAsAll()
        {
            string str = null;

            // test null check
            CollectionAssert.IsEmpty(str.AsAll(typeof(object)));
            CollectionAssert.IsEmpty(str.AsAll(typeof(object)));

            // test handling non-IDecoratable
            str = "a";
            Utilities.TestSequenceEqual(str.AsAll(typeof(string)), str);
            Utilities.TestSequenceEqual(str.AsAll(typeof(string)), str);

            // test IDecoratable calls
            SimpleDecoratable test;
            test = new SimpleDecoratable();
            Utilities.TestSequenceEqual(test.AsAll(typeof(string)), SimpleDecoratable.Decorators);
            Assert.True(test.AsAllCalled);
            test = new SimpleDecoratable();
            Utilities.TestSequenceEqual(test.AsAll(typeof(string)), SimpleDecoratable.Decorators);
            Assert.True(test.AsAllCalled);
        }

        [Test]
        public void TestAsAllGeneric()
        {
            string str = null;

            // test null check
            CollectionAssert.IsEmpty(str.AsAll<object>());
            CollectionAssert.IsEmpty(str.AsAll<object>());

            // test handling non-IDecoratable
            str = "a";
            Utilities.TestSequenceEqual(str.AsAll<string>(), str);
            Utilities.TestSequenceEqual(str.AsAll<string>(), str);

            // test IDecoratable calls
            SimpleDecoratable test;
            test = new SimpleDecoratable();
            Utilities.TestSequenceEqual(test.AsAll<string>(), SimpleDecoratable.Decorators);
            Assert.True(test.AsAllCalled);
            test = new SimpleDecoratable();
            Utilities.TestSequenceEqual(test.AsAll<string>(), SimpleDecoratable.Decorators);
            Assert.True(test.AsAllCalled);
        }

        [Test]
        public void TestAsIEnumerable()
        {
            object[] test = null;

            // test null check
            CollectionAssert.IsEmpty(test.AsIEnumerable(typeof(object)));
            CollectionAssert.IsEmpty(test.AsIEnumerable(typeof(object)));

            // test adaptation failure
            test = new object[1];
            CollectionAssert.IsEmpty(test.AsIEnumerable(typeof(object)));
            CollectionAssert.IsEmpty(test.AsIEnumerable(typeof(object)));

            // test adaptation success
            test = new object[] { "a", "b" };
            Utilities.TestSequenceEqual(test.AsIEnumerable(typeof(string)), "a", "b");
            Utilities.TestSequenceEqual(test.AsIEnumerable(typeof(string)), "a", "b");

            // test adaptation success/failure
            test = new object[] { "a", this };
            Utilities.TestSequenceEqual(test.AsIEnumerable(typeof(string)), "a");
            Utilities.TestSequenceEqual(test.AsIEnumerable(typeof(string)), "a");
        }

        [Test]
        public void TestAsIEnumerableGeneric()
        {
            object[] test = null;

            // test null check
            CollectionAssert.IsEmpty(test.AsIEnumerable<object>());
            CollectionAssert.IsEmpty(test.AsIEnumerable<object>());

            // test adaptation failure
            test = new object[1];
            CollectionAssert.IsEmpty(test.AsIEnumerable<object>());
            CollectionAssert.IsEmpty(test.AsIEnumerable<object>());

            // test adaptation success
            test = new object[] { "a", "b" };
            Utilities.TestSequenceEqual(test.AsIEnumerable<object>(), "a", "b");
            Utilities.TestSequenceEqual(test.AsIEnumerable<object>(), "a", "b");

            // test adaptation success/failure
            test = new object[] { "a", this };
            Utilities.TestSequenceEqual(test.AsIEnumerable<string>(), "a");
            Utilities.TestSequenceEqual(test.AsIEnumerable<string>(), "a");
        }

        [Test]
        public void TestAny()
        {
            object[] test = null;

            // test null check
            Assert.False(test.Any(typeof(object)));
            Assert.False(test.Any(typeof(object)));

            // test adaptation failure
            test = new object[1];
            Assert.False(test.Any(typeof(object)));
            Assert.False(test.Any(typeof(object)));

            // test adaptation success
            test = new object[] { "a", "b" };
            Assert.True(test.Any(typeof(string)));
            Assert.True(test.Any(typeof(string)));

            // test adaptation success/failure
            test = new object[] { "a", this };
            Assert.True(test.Any(typeof(string)));
            Assert.True(test.Any(typeof(string)));
        }

        [Test]
        public void TestAnyGeneric()
        {
            object[] test = null;

            // test null check
            Assert.False(test.Any<object>());
            Assert.False(test.Any<object>());

            // test adaptation failure
            test = new object[1];
            Assert.False(test.Any<object>());
            Assert.False(test.Any<object>());

            // test adaptation success
            test = new object[] { "a", "b" };
            Assert.True(test.Any<string>());
            Assert.True(test.Any<string>());

            // test adaptation success/failure
            test = new object[] { "a", this };
            Assert.True(test.Any<string>());
            Assert.True(test.Any<string>());
        }

        [Test]
        public void TestAll()
        {
            object[] test = null;

            // test null check
            Assert.True(test.All(typeof(object)));
            Assert.True(test.All(typeof(object)));

            // test adaptation failure
            test = new object[1];
            Assert.False(test.All(typeof(object)));
            Assert.False(test.All(typeof(object)));

            // test adaptation success
            test = new object[] { "a", "b" };
            Assert.True(test.All(typeof(string)));
            Assert.True(test.All(typeof(string)));

            // test adaptation success/failure
            test = new object[] { "a", this };
            Assert.False(test.All(typeof(string)));
            Assert.False(test.All(typeof(string)));
        }

        [Test]
        public void TestAllGeneric()
        {
            object[] test = null;

            // test null check
            Assert.True(test.All<object>());
            Assert.True(test.All<object>());

            // test adaptation failure
            test = new object[1];
            Assert.False(test.All<object>());
            Assert.False(test.All<object>());

            // test adaptation success
            test = new object[] { "a", "b" };
            Assert.True(test.All<string>());
            Assert.True(test.All<string>());

            // test adaptation success/failure
            test = new object[] { "a", this };
            Assert.False(test.All<string>());
            Assert.False(test.All<string>());
        }
    }
}
