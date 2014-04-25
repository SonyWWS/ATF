//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

using Sce.Atf.VectorMath;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestFrustum
    {

        [Test]
        public void TestToString()
        {
            TestToStringWithCulture(CultureInfo.GetCultureInfo("en-US")); // English in the United States
            TestToStringWithCulture(CultureInfo.GetCultureInfo("en-GB")); // English in the United Kingdom
            TestToStringWithCulture(CultureInfo.GetCultureInfo("ja"));    // Japanese
            TestToStringWithCulture(CultureInfo.GetCultureInfo("nl-NL")); // Dutch in the Netherlands
            TestToStringWithCulture(CultureInfo.GetCultureInfo("de-DE")); // German in Germany
            TestToStringWithCulture(CultureInfo.GetCultureInfo("fr"));    // French
            TestToStringWithCulture(CultureInfo.InvariantCulture);        // culture-invariant, for file I/O
        }

        private void TestToStringWithCulture(CultureInfo culture)
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = culture;
            try
            {
                string listSeparator = culture.TextInfo.ListSeparator;
                string decimalSeparator = culture.NumberFormat.NumberDecimalSeparator;
                var o = new Frustum(1.1f, 2.2f, 3.3f, 4.4f, 5.5f, 6.6f);

                string s = o.ToString(null, null);
                TestToStringResults(o, s, listSeparator, decimalSeparator);

                s = o.ToString("G", culture);
                TestToStringResults(o, s, listSeparator, decimalSeparator);

                s = o.ToString("R", culture);
                TestToStringResults(o, s, listSeparator, decimalSeparator);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        private void TestToStringResults(Frustum o, string s, string listSeparator, string decimalSeparator)
        {
            string[] results = s.Split(new[] { listSeparator }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(results.Length, 6);
            foreach (string oneFloatString in results)
                Assert.True(oneFloatString.Contains(decimalSeparator));
            Assert.AreEqual(float.Parse(results[0]), o.Right);
            Assert.AreEqual(float.Parse(results[1]), o.Left);
            Assert.AreEqual(float.Parse(results[2]), o.Top);
            Assert.AreEqual(float.Parse(results[3]), o.Bottom);
            Assert.AreEqual(float.Parse(results[4]), o.Near);
            Assert.AreEqual(float.Parse(results[5]), o.Far);
        }
    }
}
