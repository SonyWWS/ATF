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

            Assert.Null(Adapters.As(test, null));
            // test extension method
            Assert.Null(test.As(null));
        }

        [Test]
        public void TestAs_WithNullType()
        {
            Assert.Throws<ArgumentNullException>(delegate { Adapters.As(this, null); });
        }

        [Test]
        public void TestAs_CastFirst()
        {
            SimpleAdaptable adaptable = new SimpleAdaptable();
            Assert.AreSame(Adapters.As(adaptable, typeof(IAdaptable)), adaptable);
            Assert.False(adaptable.AsCalled);
        }

        [Test]
        public void TestAs_GetAdapterIfCastFails()
        {
            SimpleAdaptable adaptable = new SimpleAdaptable();
            Assert.NotNull(Adapters.As(adaptable, typeof(string)));
            Assert.True(adaptable.AsCalled);
        }

        [Test]
        public void TestAs_CastAndGetAdapterFail()
        {
            SimpleAdaptable adaptable = new SimpleAdaptable();
            Assert.Null(Adapters.As(adaptable, typeof(TestAdapters)));
            Assert.True(adaptable.AsCalled);
        }

        [Test]
        public void TestAsGeneric_WithNullObject()
        {
            object test = null;

            Assert.Null(Adapters.As<object>(test));
            // test extension method
            Assert.Null(test.As<object>());
        }

        [Test]
        public void TestAsGeneric_CastFirst()
        {
            SimpleAdaptable adaptable = new SimpleAdaptable();
            Assert.AreSame(Adapters.As<IAdaptable>(adaptable), adaptable);
            Assert.False(adaptable.AsCalled);
        }

        [Test]
        public void TestAsGeneric_GetAdapterIfCastFails()
        {
            SimpleAdaptable adaptable = new SimpleAdaptable();
            Assert.NotNull(Adapters.As<string>(adaptable));
            Assert.True(adaptable.AsCalled);
        }

        [Test]
        public void TestAsGeneric_CastAndGetAdapterFail()
        {
            SimpleAdaptable adaptable = new SimpleAdaptable();
            Assert.Null(Adapters.As<TestAdapters>(adaptable));
            Assert.True(adaptable.AsCalled);
        }

        [Test]
        public void TestCast()
        {
            SimpleAdaptable adaptable = null;

            // test null check
            Assert.Throws<AdaptationException>(delegate() { Adapters.Cast(adaptable, typeof(object)); });
            Assert.Throws<AdaptationException>(delegate() { adaptable.Cast(typeof(object)); });

            // test successful adaptation
            adaptable = new SimpleAdaptable();
            Assert.NotNull(Adapters.Cast(adaptable, typeof(IAdaptable)));
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
            Assert.Throws<AdaptationException>(delegate() { Adapters.Cast<object>(adaptable); });
            Assert.Throws<AdaptationException>(delegate() { adaptable.Cast<object>(); });

            // test successful adaptation
            adaptable = new SimpleAdaptable();
            Assert.NotNull(Adapters.Cast<IAdaptable>(adaptable));
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
            Assert.False(Adapters.Is(adaptable, typeof(object)));
            Assert.False(adaptable.Is(typeof(object)));

            // test successful adaptation
            adaptable = new SimpleAdaptable();
            Assert.True(Adapters.Is(adaptable, typeof(IAdaptable)));
            adaptable = new SimpleAdaptable();
            Assert.NotNull(adaptable.Is(typeof(IAdaptable)));

            // test failed adaptation
            adaptable = new SimpleAdaptable();
            Assert.False(Adapters.Is(adaptable, typeof(TestAdapters)));
            adaptable = new SimpleAdaptable();
            Assert.False(adaptable.Is(typeof(TestAdapters)));
        }

        [Test]
        public void TestIsGeneric()
        {
            SimpleAdaptable adaptable = null;

            // test null check
            Assert.False(Adapters.Is<object>(adaptable));
            Assert.False(adaptable.Is<object>());

            // test successful adaptation
            adaptable = new SimpleAdaptable();
            Assert.True(Adapters.Is<IAdaptable>(adaptable));
            adaptable = new SimpleAdaptable();
            Assert.NotNull(adaptable.Is<IAdaptable>());

            // test failed adaptation
            adaptable = new SimpleAdaptable();
            Assert.False(Adapters.Is<TestAdapters>(adaptable));
            adaptable = new SimpleAdaptable();
            Assert.False(adaptable.Is<TestAdapters>());
        }

        [Test]
        public void TestAsAll()
        {
            string str = null;

            // test null check
            CollectionAssert.IsEmpty(Adapters.AsAll(str, typeof(object)));
            CollectionAssert.IsEmpty(str.AsAll(typeof(object)));

            // test handling non-IDecoratable
            str = "a";
            Utilities.TestSequenceEqual(Adapters.AsAll(str, typeof(string)), str);
            Utilities.TestSequenceEqual(str.AsAll(typeof(string)), str);

            // test IDecoratable calls
            SimpleDecoratable test;
            test = new SimpleDecoratable();
            Utilities.TestSequenceEqual(Adapters.AsAll(test, typeof(string)), SimpleDecoratable.Decorators);
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
            CollectionAssert.IsEmpty(Adapters.AsAll<object>(str));
            CollectionAssert.IsEmpty(str.AsAll<object>());

            // test handling non-IDecoratable
            str = "a";
            Utilities.TestSequenceEqual(Adapters.AsAll<string>(str), str);
            Utilities.TestSequenceEqual(str.AsAll<string>(), str);

            // test IDecoratable calls
            SimpleDecoratable test;
            test = new SimpleDecoratable();
            Utilities.TestSequenceEqual(Adapters.AsAll<string>(test), SimpleDecoratable.Decorators);
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
            CollectionAssert.IsEmpty(Adapters.AsIEnumerable(test, typeof(object)));
            CollectionAssert.IsEmpty(test.AsIEnumerable(typeof(object)));

            // test adaptation failure
            test = new object[1];
            CollectionAssert.IsEmpty(Adapters.AsIEnumerable(test, typeof(object)));
            CollectionAssert.IsEmpty(test.AsIEnumerable(typeof(object)));

            // test adaptation success
            test = new object[] { "a", "b" };
            Utilities.TestSequenceEqual(Adapters.AsIEnumerable(test, typeof(string)), "a", "b");
            Utilities.TestSequenceEqual(test.AsIEnumerable(typeof(string)), "a", "b");

            // test adaptation success/failure
            test = new object[] { "a", this };
            Utilities.TestSequenceEqual(Adapters.AsIEnumerable(test, typeof(string)), "a");
            Utilities.TestSequenceEqual(test.AsIEnumerable(typeof(string)), "a");
        }

        [Test]
        public void TestAsIEnumerableGeneric()
        {
            object[] test = null;

            // test null check
            CollectionAssert.IsEmpty(Adapters.AsIEnumerable<object>(test));
            CollectionAssert.IsEmpty(test.AsIEnumerable<object>());

            // test adaptation failure
            test = new object[1];
            CollectionAssert.IsEmpty(Adapters.AsIEnumerable<object>(test));
            CollectionAssert.IsEmpty(test.AsIEnumerable<object>());

            // test adaptation success
            test = new object[] { "a", "b" };
            Utilities.TestSequenceEqual(Adapters.AsIEnumerable<object>(test), "a", "b");
            Utilities.TestSequenceEqual(test.AsIEnumerable<object>(), "a", "b");

            // test adaptation success/failure
            test = new object[] { "a", this };
            Utilities.TestSequenceEqual(Adapters.AsIEnumerable<string>(test), "a");
            Utilities.TestSequenceEqual(test.AsIEnumerable<string>(), "a");
        }

        [Test]
        public void TestAny()
        {
            object[] test = null;

            // test null check
            Assert.False(Adapters.Any(test, typeof(object)));
            Assert.False(test.Any(typeof(object)));

            // test adaptation failure
            test = new object[1];
            Assert.False(Adapters.Any(test, typeof(object)));
            Assert.False(test.Any(typeof(object)));

            // test adaptation success
            test = new object[] { "a", "b" };
            Assert.True(Adapters.Any(test, typeof(string)));
            Assert.True(test.Any(typeof(string)));

            // test adaptation success/failure
            test = new object[] { "a", this };
            Assert.True(Adapters.Any(test, typeof(string)));
            Assert.True(test.Any(typeof(string)));
        }

        [Test]
        public void TestAnyGeneric()
        {
            object[] test = null;

            // test null check
            Assert.False(Adapters.Any<object>(test));
            Assert.False(test.Any<object>());

            // test adaptation failure
            test = new object[1];
            Assert.False(Adapters.Any<object>(test));
            Assert.False(test.Any<object>());

            // test adaptation success
            test = new object[] { "a", "b" };
            Assert.True(Adapters.Any<string>(test));
            Assert.True(test.Any<string>());

            // test adaptation success/failure
            test = new object[] { "a", this };
            Assert.True(Adapters.Any<string>(test));
            Assert.True(test.Any<string>());
        }

        [Test]
        public void TestAll()
        {
            object[] test = null;

            // test null check
            Assert.True(Adapters.All(test, typeof(object)));
            Assert.True(test.All(typeof(object)));

            // test adaptation failure
            test = new object[1];
            Assert.False(Adapters.All(test, typeof(object)));
            Assert.False(test.All(typeof(object)));

            // test adaptation success
            test = new object[] { "a", "b" };
            Assert.True(Adapters.All(test, typeof(string)));
            Assert.True(test.All(typeof(string)));

            // test adaptation success/failure
            test = new object[] { "a", this };
            Assert.False(Adapters.All(test, typeof(string)));
            Assert.False(test.All(typeof(string)));
        }

        [Test]
        public void TestAllGeneric()
        {
            object[] test = null;

            // test null check
            Assert.True(Adapters.All<object>(test));
            Assert.True(test.All<object>());

            // test adaptation failure
            test = new object[1];
            Assert.False(Adapters.All<object>(test));
            Assert.False(test.All<object>());

            // test adaptation success
            test = new object[] { "a", "b" };
            Assert.True(Adapters.All<string>(test));
            Assert.True(test.All<string>());

            // test adaptation success/failure
            test = new object[] { "a", this };
            Assert.False(Adapters.All<string>(test));
            Assert.False(test.All<string>());
        }
    }
}
