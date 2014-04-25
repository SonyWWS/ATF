//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

using Sce.Atf.VectorMath;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestBezierCurve2F
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
                var o = new BezierCurve2F(
                    new Vec2F(1.1f, 2.2f),
                    new Vec2F(4.4f, 5.5f),
                    new Vec2F(7.7f, 8.8f),
                    new Vec2F(10.10f, 11.11f));

                string s = o.ToString(null, null);
                TestToStringResults(o, s, listSeparator, decimalSeparator);

                string s2 = o.ToString();
                Assert.AreEqual(s, s2);

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

        private void TestToStringResults(BezierCurve2F o, string s, string listSeparator, string decimalSeparator)
        {
            string[] results = s.Split(new[] { listSeparator }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(results.Length, 8);
            foreach (string oneFloatString in results)
                Assert.True(oneFloatString.Contains(decimalSeparator));

            Assert.AreEqual(float.Parse(results[0]), o.P1.X);
            Assert.AreEqual(float.Parse(results[1]), o.P1.Y);

            Assert.AreEqual(float.Parse(results[2]), o.P2.X);
            Assert.AreEqual(float.Parse(results[3]), o.P2.Y);

            Assert.AreEqual(float.Parse(results[4]), o.P3.X);
            Assert.AreEqual(float.Parse(results[5]), o.P3.Y);

            Assert.AreEqual(float.Parse(results[6]), o.P4.X);
            Assert.AreEqual(float.Parse(results[7]), o.P4.Y);
        }
    }
}
