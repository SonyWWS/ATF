//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    class TestWeakKey
    {
        [Test]
        public void TestWeakKeyHashBehavior()
        {
            var controlA = new TestControlA();
            var controlB = new TestControlA();

            var weakA = new WeakKey<Control>(controlA);
            var weakA2 = new WeakKey<Control>(controlA);

            Assert.IsTrue(weakA.Equals(weakA));
            Assert.IsTrue(weakA.GetHashCode() == weakA2.GetHashCode());
            Assert.IsTrue(weakA.Equals(weakA2));
            Assert.IsTrue(weakA != weakA2);
            Assert.IsTrue(!weakA.Equals(controlA));
            Assert.IsTrue(!controlA.Equals(weakA));

            var weakB = new WeakKey<Control>(controlB);
            Assert.IsTrue(!weakA.Equals(weakB));
            Assert.IsTrue(weakA.GetHashCode() != weakB.GetHashCode()); //Always true? Not sure, but I hope so!

            var set = new HashSet<WeakKey<Control>>();
            set.Add(weakA);
            Assert.IsTrue(set.Count == 1);
            set.Add(weakA2);
            Assert.IsTrue(set.Count == 1);
            set.Add(weakB);
            Assert.IsTrue(set.Count == 2);

            // Couldn't get this to work.
            //controlA = null;
            //controlB = null;
            //GC.Collect(); //defaults are all generation and 'forced' collection
            //foreach (SkinService.WeakKey<Control> key in set)
            //    Assert.IsTrue(!key.IsAlive);

            // Proves that WeakReference can't do the job!
            //var weakRefA = new WeakReference(controlA);
            //var weakRefA2 = new WeakReference(controlA);
            //var weakRefB = new WeakReference(controlB);

            //var set2 = new HashSet<WeakReference>();
            //set2.Add(weakRefA);
            //Assert.IsTrue(set2.Count == 1);
            //set2.Add(weakRefA2);
            //Assert.IsTrue(set2.Count == 1); //it's 2!
            //set2.Add(weakRefB);
            //Assert.IsTrue(set2.Count == 2); //it's 3!
        }
    }
}
