//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;

using Sce.Atf.Adaptation;

namespace UnitTests.Atf.Adaptation
{
    [TestFixture]
    public class TestAdaptableActiveCollection
    {
        [Test]
        public void TestAdaptation()
        {
            AdaptableActiveCollection<object> test = new AdaptableActiveCollection<object>();

            var a1 = new UnitTests.Atf.Adaptation.SimpleAdaptable();
            object o1 = new object();

            test.Add(a1);
            test.Add(o1);

            Utilities.TestSequenceEqual(test.AsIEnumerable<object>(), a1, o1);
            Utilities.TestSequenceEqual(test.AsIEnumerable<string>(), a1.As<string>());
            CollectionAssert.IsEmpty(test.AsIEnumerable<TestAdaptableActiveCollection>());

            CollectionAssert.AreEqual(test.GetSnapshot<object>(), new object[] { a1, o1 });
            CollectionAssert.AreEqual(test.GetSnapshot<string>(), new string[] { a1.As<string>() });
            CollectionAssert.IsEmpty(test.GetSnapshot<TestAdaptableActiveCollection>());

            Assert.AreSame(test.GetActiveItem<string>(), a1.As<string>());
            Assert.AreSame(test.GetActiveItem<object>(), o1);
            Assert.Null(test.GetActiveItem<TestAdaptableActiveCollection>());
        }
    }
}
