//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestStringUtil
    {
        [TestCase(               "",                    "",  0)]
        [TestCase(              "a",                   "b", -1)]
        [TestCase(              "1",                   "2", -1)]
        [TestCase(           "base",          "baseSuffix", -1)]
        [TestCase(             "aa",                  "ab", -1)]
        [TestCase(             "1a",                  "1b", -1)]
        [TestCase(             "1a",                 "1aa", -1)]
        [TestCase(            "1aa",                  "1z", -1)]
        [TestCase(             "a1",                  "a2", -1)]
        [TestCase(             "a2",                 "a10", -1)]
        [TestCase(        "test200",            "test1000", -1)]
        [TestCase(         "test_1",              "test_2", -1)]
        [TestCase(         "test_2",             "test_10", -1)]
        [TestCase(       "test_(2)",           "test_(10)", -1)]
        [TestCase(      "test2.ext",          "test19.ext", -1)]
        [TestCase(       "base.ext", "base.long_extension", -1)]
        [TestCase(        "base.aa",              "base.z", -1)]
        [TestCase(       "base(aa)",             "base(z)", -1)]
        [TestCase( "base1base2.ext",     "base1base10.ext", -1)]
        [TestCase("base1zzzzzz.ext",     "base2zzzzzz.ext", -1)]
        [TestCase("aaa",                             "ZZZ", -1)]
        [TestCase("AAA",                             "zzz", -1)]
        [TestCase("long name with & unusual * ^ $ characters1", "long name with & unusual * ^ $ characters2", -1)]
        [TestCase("12345678901234567890", "12345678901234567891", -1)] //last digit is different
        [TestCase("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567891", -1)]//exceeds 64-bits. last digit is different.
        [TestCase("12345678901234567890", "22345678901234567890", -1)] //first digit is different; test string-to-int
        [TestCase("1234567890123456789012345678901234567890", "2234567890123456789012345678901234567890", -1)]//exceeds 64-bits. first digit is different; test string-to-int
        //Exceeds 64-bits. 2nd # is larger, but alphabetically smaller. Breaks the algorithm.
        //[TestCase("2234567890123456789012345678901234567890", "12345678901234567890123456789012345678900", -1)]
        public void TestCompareNaturalOrder(string a, string b, int result)
        {
            Assert.AreEqual(result, StringUtil.CompareNaturalOrder(a, b));

            int reverseResult = -result; // 0 => 0, 1 => -1, -1 => 1
            Assert.AreEqual(reverseResult, StringUtil.CompareNaturalOrder(b, a));

            Assert.AreEqual(0, StringUtil.CompareNaturalOrder(a, a));
            Assert.AreEqual(0, StringUtil.CompareNaturalOrder(b, b));
        }

        [Test]
        public void TestSplitAndKeepDelimiters()
        {
            // No delimiters.
            string text = "nothing to parse";
            IList<string> result = text.SplitAndKeepDelimiters();
            Assert.AreEqual(result[0], "nothing to parse");

            // No text!
            text = "";
            result = text.SplitAndKeepDelimiters();
            Assert.IsTrue(result.Count == 0);
            result = text.SplitAndKeepDelimiters(' ');
            Assert.IsTrue(result.Count == 0);

            // Null should be OK, too.
            text = null;
            result = text.SplitAndKeepDelimiters();
            Assert.IsTrue(result.Count == 0);
            result = text.SplitAndKeepDelimiters(' ');
            Assert.IsTrue(result.Count == 0);

            // Do some real parsing.
            text = "[wiki links|http://www.google.com]";
            result = text.SplitAndKeepDelimiters('[', '|', ']');
            Assert.IsTrue(result.Count == 5);
            Assert.AreEqual(result[0], "[");
            Assert.AreEqual(result[1], "wiki links");
            Assert.AreEqual(result[2], "|");
            Assert.AreEqual(result[3], "http://www.google.com");
            Assert.AreEqual(result[4], "]");

            text = "This string has some [wiki links|http://www.google.com].";
            result = text.SplitAndKeepDelimiters('[', '|', ']');
            Assert.IsTrue(result.Count == 7);
            Assert.AreEqual(result[0], "This string has some ");
            Assert.AreEqual(result[1], "[");
            Assert.AreEqual(result[2], "wiki links");
            Assert.AreEqual(result[3], "|");
            Assert.AreEqual(result[4], "http://www.google.com");
            Assert.AreEqual(result[5], "]");
            Assert.AreEqual(result[6], ".");

            text = "Lots of [links|a]: [here|b] and [here|c] and [here|d]";
            result = text.SplitAndKeepDelimiters('[', '|', ']');
            Assert.IsTrue(result.Count == 24);
            Assert.AreEqual(result[0], "Lots of ");
            Assert.AreEqual(result[1], "[");
            Assert.AreEqual(result[2], "links");
            Assert.AreEqual(result[3], "|");
            Assert.AreEqual(result[4], "a");
            Assert.AreEqual(result[22], "d");
            Assert.AreEqual(result[23], "]");

            // Make sure that multiple delimiters in a row works as expected.
            // Make sure that we can pass in an explicit array of delimiters, too.
            text = "]]|||[[";
            result = text.SplitAndKeepDelimiters(new[]{'[', '|', ']'});
            Assert.IsTrue(result.Count == 7);
            Assert.AreEqual(result[0], "]");
            Assert.AreEqual(result[1], "]");
            Assert.AreEqual(result[2], "|");
            Assert.AreEqual(result[3], "|");
            Assert.AreEqual(result[4], "|");
            Assert.AreEqual(result[5], "[");
            Assert.AreEqual(result[6], "[");
        }
    }
}
