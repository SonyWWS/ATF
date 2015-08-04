//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using LocalizableStringExtractor;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class TestLocalizableString
    {
        [Test]
        public void TestEquality()
        {
            var a = new LocalizableString("a", "", "");
            var b = new LocalizableString("b", "", "");
            var a2 = new LocalizableString("a", "2", "");
            var aTranslated = new LocalizableString("a", "", "long a");

            Assert.AreEqual(a, a);
            Assert.AreEqual(a.GetHashCode(), a.GetHashCode());
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a, a2);
            Assert.AreEqual(a, aTranslated);
            Assert.AreEqual(a.GetHashCode(), aTranslated.GetHashCode());
        }
    }
}
