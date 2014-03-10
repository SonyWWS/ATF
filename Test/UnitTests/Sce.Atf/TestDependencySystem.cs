//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf;

using NUnit.Framework;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestDependencySystem
    {
        [Test]
        public void TestEmpty()
        {
            DependencySystem<object> test = new DependencySystem<object>();
            Assert.False(test.NeedsUpdate);
            CollectionAssert.IsEmpty(test.GetInvalidDependents());
        }

        private void TestUpdate(DependencySystem<object>.InvalidDependent update, object dependent, params object[] dependencies)
        {
            Assert.AreEqual(update.Dependent, dependent);
            Utilities.TestSequenceContainSameItems(update.Dependencies, dependencies);
        }

        [Test]
        public void TestSimpleUpdate()
        {
            DependencySystem<object> test = new DependencySystem<object>();
            object a = 1;
            object b = 2;
            test.AddDependency(b, a);

            Assert.False(test.NeedsUpdate);

            test.Invalidate(a);

            List<DependencySystem<object>.InvalidDependent> updates = new List<DependencySystem<object>.InvalidDependent>(test.GetInvalidDependents());
            Assert.AreEqual(updates.Count, 1);
            TestUpdate(updates[0], b, a);

            Assert.False(test.NeedsUpdate);
        }

        [Test]
        public void TestUpdateWithoutDependency()
        {
            DependencySystem<object> test = new DependencySystem<object>();
            object a = 1;
            object b = 2;
            test.AddDependency(b, a);
            test.Invalidate(b);

            Assert.False(test.NeedsUpdate);
        }

        [Test]
        public void TestUpdate()
        {
            DependencySystem<object> test = new DependencySystem<object>();
            object a = 1;
            object b = 2;
            object c = 3;
            object d = 4;
            test.AddDependency(d, c);
            test.AddDependency(c, b);
            test.AddDependency(b, a);

            Assert.False(test.NeedsUpdate);

            test.Invalidate(a);

            List<DependencySystem<object>.InvalidDependent> updates = new List<DependencySystem<object>.InvalidDependent>(test.GetInvalidDependents());
            Assert.AreEqual(updates.Count, 3);
            TestUpdate(updates[0], b, a);
            TestUpdate(updates[1], c, b);
            TestUpdate(updates[2], d, c);
        }
    }
}
