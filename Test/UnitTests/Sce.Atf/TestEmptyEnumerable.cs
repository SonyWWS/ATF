//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf;

using NUnit.Framework;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestEmptyEnumerable
    {
        [Test]
        public void TestInstance()
        {
            IEnumerable<string> test = EmptyEnumerable<string>.Instance;
            CollectionAssert.IsEmpty(test);
            IEnumerable<string> other = EmptyEnumerable<string>.Instance;
            Assert.AreSame(test, other);
            Assert.AreSame(test.GetEnumerator(), test.GetEnumerator());
            Assert.AreSame(test.GetEnumerator(), other.GetEnumerator());
        }

        [Test]
        public void TestEnumeration()
        {
            IEnumerable<string> test = EmptyEnumerable<string>.Instance;
            foreach (string s in test) ;
        }

        [Test]
        public void TestEnumerable()
        {
            IEnumerator<string> test = EmptyEnumerable<string>.Instance.GetEnumerator();
            Assert.DoesNotThrow(delegate { test.Reset(); });
            Assert.DoesNotThrow(delegate { test.Dispose(); });
            Assert.Throws<InvalidOperationException>(delegate { object dummy = test.Current; });
        }
    }
}
