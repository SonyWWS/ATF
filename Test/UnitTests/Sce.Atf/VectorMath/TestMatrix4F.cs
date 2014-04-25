//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

using Sce.Atf.VectorMath;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestMatrix4F
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
                var o = new Matrix4F(
                    1.1f, 2.2f, 3.3f, 4.4f,
                    5.5f, 6.6f, 7.7f, 8.8f,
                    9.9f, 10.10f, 11.11f, 12.12f,
                    13.13f, 14.14f, 15.15f, 16.16f);

                string s = o.ToString(null, null);
                TestToStringResults(o, s, listSeparator, decimalSeparator);

                s = o.ToString();
                Assert.True(s.Contains(listSeparator));

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

        private void TestToStringResults(Matrix4F o, string s, string listSeparator, string decimalSeparator)
        {
            string[] results = s.Split(new[] { listSeparator }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(results.Length, 16);
            for (int i = 0; i < 16; i++)
            {
                Assert.True(results[i].Contains(decimalSeparator));
                float result = float.Parse(results[i]);
                Assert.AreEqual(result, o[i]);
            }
        }
    }
}
