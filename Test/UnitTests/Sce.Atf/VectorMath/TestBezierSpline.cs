//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

using Sce.Atf.VectorMath;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestBezierSpline
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
                var o = new BezierSpline();
                var controlPoint1 = new Vec3F(1.1f, 2.2f, 3.3f);
                var controlPoint2 = new Vec3F(4.4f, 5.5f, 6.6f);
                var controlPoint3 = new Vec3F(7.7f, 8.8f, 9.9f);
                var incomingTangent = new Vec3F(-1.0f, -2.0f, -3.0f);
                var outgoingTangent = new Vec3F(1.0f, 2.0f, 3.0f);
                o.Add(new BezierPoint(controlPoint1, incomingTangent, outgoingTangent));
                o.Add(new BezierPoint(controlPoint2, incomingTangent, outgoingTangent));
                o.Add(new BezierPoint(controlPoint3, incomingTangent, outgoingTangent));

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

        private void TestToStringResults(BezierSpline o, string s, string listSeparator, string decimalSeparator)
        {
            string[] results = s.Split(new[] { listSeparator }, StringSplitOptions.RemoveEmptyEntries);

            // {# of points}, {control point #1}, {cp #2}, {cp #3} where each control point has 3 vectors, each with 3 floats
            Assert.AreEqual(results.Length, 1 + 3*3*3);
            Assert.True(results[1].Contains(decimalSeparator));
            Assert.True(results[2].Contains(decimalSeparator));
            Assert.True(results[3].Contains(decimalSeparator));
            Assert.AreEqual(int.Parse(results[0]), 3); //3 control points
            Assert.AreEqual(float.Parse(results[1]), 1.1f);
            Assert.AreEqual(float.Parse(results[2]), 2.2f);
            Assert.AreEqual(float.Parse(results[3]), 3.3f);
        }
    }
}
