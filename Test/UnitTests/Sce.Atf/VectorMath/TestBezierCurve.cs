//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

using Sce.Atf.VectorMath;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestBezierCurve
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
                var o = new BezierCurve( new[]
                {
                    new Vec3F(1.1f, 2.2f, 3.3f),
                    new Vec3F(4.4f, 5.5f, 6.6f),
                    new Vec3F(7.7f, 8.8f, 9.9f),
                    new Vec3F(10.10f, 11.11f, 12.12f),
                });

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

        private void TestToStringResults(BezierCurve o, string s, string listSeparator, string decimalSeparator)
        {
            string[] results = s.Split(new[] { listSeparator }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(results.Length, 12);
            foreach(string oneFloatString in results)
                Assert.True(oneFloatString.Contains(decimalSeparator));

            Vec3F[] originals = o.ControlPoints;
            Assert.AreEqual(float.Parse(results[0]), originals[0].X);
            Assert.AreEqual(float.Parse(results[1]), originals[0].Y);
            Assert.AreEqual(float.Parse(results[2]), originals[0].Z);

            Assert.AreEqual(float.Parse(results[3]), originals[1].X);
            Assert.AreEqual(float.Parse(results[4]), originals[1].Y);
            Assert.AreEqual(float.Parse(results[5]), originals[1].Z);

            Assert.AreEqual(float.Parse(results[6]), originals[2].X);
            Assert.AreEqual(float.Parse(results[7]), originals[2].Y);
            Assert.AreEqual(float.Parse(results[8]), originals[2].Z);

            Assert.AreEqual(float.Parse(results[9]), originals[3].X);
            Assert.AreEqual(float.Parse(results[10]), originals[3].Y);
            Assert.AreEqual(float.Parse(results[11]), originals[3].Z);
        }
    }
}
