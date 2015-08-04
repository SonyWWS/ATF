//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LocalizableStringExtractor;
using NUnit.Framework;
using Sce.Atf;

namespace UnitTests
{
    [TestFixture]
    public class TestExtractor
    {
        [Test]
        public void TestExtractFromCs()
        {
            List<LocalizableString> strings = ExtractFromCs("AboutDialog.cs");
            
            Assert.IsNotNull(strings.Find(s =>
                s.Text == "About" &&
                s.Context == "" ));

            Assert.IsNotNull(strings.Find(s =>
                s.Text == "Authoring Tools Framework (ATF {0})," &&
                s.Context == "{0} is the version number"));

            Assert.IsNotNull(strings.Find(s =>
                s.Text == "Check out the file\r\n\r\n{0}\r\n\r\nto be able to save the changes?" &&
                s.Context == ""));

            Assert.IsNotNull(strings.Find(s =>
                s.Text == "some LocalizedDescription string" &&
                s.Context == ""));

            Assert.IsNotNull(strings.Find(s =>
                s.Text == "the single arg property description" &&
                s.Context == ""));

            Assert.IsNotNull(strings.Find(s =>
                s.Text == "the 4 arg property display name" &&
                s.Context == ""));

            Assert.IsNotNull(strings.Find(s =>
                s.Text == "the 4 arg property description" &&
                s.Context == ""));

            // Test C# string literal with all possible character literals.
            // https://msdn.microsoft.com/en-us/library/aa691087(v=vs.71).aspx
            Assert.IsNotNull(strings.Find(s =>
                s.Text == "\'\"\\\0\a\b\f\\n\r\t\v\xf\x2a\x02a\0x002a9\u002a\U0000002a" &&
                s.Context == ""));

            // Test verbatim string with escaped double-quotes (the only possible escaped character).
            // https://msdn.microsoft.com/en-us/library/aa691090(v=vs.71).aspx
            string result = @"abc 123""
\\\n\r\t\a
";
            Assert.IsNotNull(strings.Find(s =>
                s.Text == result &&
                s.Context == ""));
            
            result = @"def 123""
\\\n\r\t\a
";
            Assert.IsNotNull(strings.Find(s =>
                s.Text == result &&
                s.Context == ""));
        }

        [Test]
        public void TestExtractFromXaml()
        {
            List<LocalizableString> strings = ExtractFromXaml("Standard.xaml");
            Assert.AreEqual(2, strings.Count);
            Assert.AreEqual(strings[0].Text, "Name With Spaces");
            Assert.AreEqual(strings[1].Text, @"%$Name/\Symbols$%");
        }

        [Test]
        public void TestExtractFromXaml2()
        {
            List<LocalizableString> strings = ExtractFromXaml("FindTargetsDialog.xaml");
            Assert.AreEqual(12, strings.Count);
            Assert.AreEqual(strings[0].Text, "Find Targets");
            Assert.AreEqual(strings[1].Text, "Rescan");
            Assert.AreEqual(strings[2].Text, "Cancel Scan");
            Assert.AreEqual(strings[3].Text, "Target");
            Assert.AreEqual(strings[4].Text, "Host");
            Assert.AreEqual(strings[5].Text, "Status");
            Assert.AreEqual(strings[6].Text, "Protocol");
            Assert.AreEqual(strings[7].Text, "Info");
            Assert.AreEqual(strings[8].Text, "No Targets Available");
            Assert.AreEqual(strings[9].Text, "Add");
            Assert.AreEqual(strings[10].Text, "Add All");
            Assert.AreEqual(strings[11].Text, "OK");
        }

        [Test]
        public void TestParseMethodCallForStringLiterals()
        {
            IList<string> paramStrings;

            // Test that the whole method name matches
            bool success = Extractor.ParseMethodCallForStringLiterals(
                "WrongLocalize(\"some text\")", 5,
                "Localize", out paramStrings);
            Assert.IsFalse(success);

            // Test that the whole method name matches
            success = Extractor.ParseMethodCallForStringLiterals(
                "LocalizeSomethingElse(\"some text\")", 0,
                "Localize", out paramStrings);
            Assert.IsFalse(success);

            // Test a simple one parameter method call
            success = Extractor.ParseMethodCallForStringLiterals(
                "Localize(aVariable)", 0,
                "Localize", out paramStrings);
            Assert.IsTrue(success);                 // Return value now indicates success of the parse, regardless of any string literals found
            Assert.AreEqual(1, paramStrings.Count); // ...and param strings now returns an entry for each argument, regardless of any string literals found
            Assert.AreEqual("", paramStrings[0]);   // ...and in this case, the argument wasn't a string literal

            // Test a simple one parameter method call with a string literal
            success = Extractor.ParseMethodCallForStringLiterals(
                "Localize(\"some text\")", 0,
                "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.AreEqual(1, paramStrings.Count);
            Assert.AreEqual("some text", paramStrings[0]);

            // Test a simple two parameter method call
            success = Extractor.ParseMethodCallForStringLiterals(
                "Localize(firstVariable, anotherVariable)", 0,
                "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.AreEqual(2, paramStrings.Count);
            Assert.AreEqual("", paramStrings[0]);
            Assert.AreEqual("", paramStrings[1]);

            // Test a simple two parameter method call
            success = Extractor.ParseMethodCallForStringLiterals(
                "Localize(\"some text\", \"some context\")", 0,
                "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.AreEqual(2, paramStrings.Count);
            Assert.AreEqual("some text", paramStrings[0]);
            Assert.AreEqual("some context", paramStrings[1]);

            // Test an extension method with no explicit parameters
            success = Extractor.ParseMethodCallForStringLiterals(
                "\"some text\".Localize()", 12,
                "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.AreEqual(1, paramStrings.Count);
            Assert.AreEqual("some text", paramStrings[0]);

            // Test an extension method with one explicit parameter
            success = Extractor.ParseMethodCallForStringLiterals(
                "\"some text\".Localize( \"some context\")", 12,
                "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.AreEqual(2, paramStrings.Count);
            Assert.AreEqual("some text", paramStrings[0]);
            Assert.AreEqual("some context", paramStrings[1]);

            // Test an extension method with one explicit parameter. Parse string concatenations.
            string expected1 = " that is rather long";
            string expected2 = "some context";
            string test = "\"some text\" + \"" + expected1 + "\".Localize( \"" + expected2 + "\")";
            int startIndex = test.IndexOf("Localize", StringComparison.InvariantCulture);
            success = Extractor.ParseMethodCallForStringLiterals(test, startIndex, "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.AreEqual(2, paramStrings.Count);
            Assert.AreEqual(expected1, paramStrings[0]);
            Assert.AreEqual(expected2, paramStrings[1]);

            test = @"
        public static DocumentClientInfo DocumentClientInfo = new DocumentClientInfo(
            ""Event Sequence"".Localize(),
            new string[] { "".xml"", "".esq"" },
            Sce.Atf.Resources.DocumentImage,
            Sce.Atf.Resources.FolderImage,
            true);";
            startIndex = test.IndexOf("Localize", StringComparison.InvariantCulture);
            success = Extractor.ParseMethodCallForStringLiterals(test, startIndex, "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.AreEqual(1, paramStrings.Count);
            Assert.AreEqual("Event Sequence", paramStrings[0]);

            // Test that \r and \n get turned into carriage-return and newline characters.
            test = "\"Check out the file\r\n\r\n{0}\r\n\r\nto be able to save the changes?\".Localize()";
            startIndex = test.IndexOf("Localize", StringComparison.InvariantCulture);
            success = Extractor.ParseMethodCallForStringLiterals(test, startIndex, "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.AreEqual(1, paramStrings.Count);
            Assert.AreEqual(
                "Check out the file\r\n\r\n{0}\r\n\r\nto be able to save the changes?",
                paramStrings[0]);
        }

        [Test]
        public void TestGetMatchingCloseParen()
        {
            Assert.AreEqual(-1, Extractor.GetMatchingCloseParen("", 0));
            Assert.AreEqual(0, Extractor.GetMatchingCloseParen(")", 0));
            Assert.AreEqual(-1, Extractor.GetMatchingCloseParen(")", 1));
            Assert.AreEqual(1, Extractor.GetMatchingCloseParen(" )", 0));
            Assert.AreEqual(14, Extractor.GetMatchingCloseParen(" blahblahblah )", 0));
            Assert.AreEqual(1, Extractor.GetMatchingCloseParen(" ) )", 1));
            Assert.AreEqual(3, Extractor.GetMatchingCloseParen(" ) )", 2));
            Assert.AreEqual(-1, Extractor.GetMatchingCloseParen("()", 0));
            Assert.AreEqual(-1, Extractor.GetMatchingCloseParen("(", 0));
            Assert.AreEqual(2, Extractor.GetMatchingCloseParen("())", 0));
            Assert.AreEqual(3, Extractor.GetMatchingCloseParen(" ())", 0));
            Assert.AreEqual(4, Extractor.GetMatchingCloseParen(" ( ))", 0));
            Assert.AreEqual(5, Extractor.GetMatchingCloseParen(" ( ) )", 0));
            Assert.AreEqual(8, Extractor.GetMatchingCloseParen(" ( () ) )", 0));
            Assert.AreEqual(11, Extractor.GetMatchingCloseParen(" ( () () ) )", 0));
            Assert.AreEqual(21, Extractor.GetMatchingCloseParen(" ( () () ) ( () () ) )", 0));
        }

        [Test]
        public void TestTryParseForStringLiteral()
        {
            var f = new TestParseForStringLiteralFixture("", "");

            f.TestForFail("");
            f.TestForFail(" ");
            f.TestForFail("\"");
            f.TestForFail("\"\\\"");
            f.TestForFail("\\");

            f.TestForSuccess("@\"\"", "", 3);
            f.TestForSuccess("@\"blah\"", "blah", 7);

            // Define input strings that should match.
            var validTests = new []
            {
                // test single-substring input
                f.Test("\"\"",                  ""),            // empty string
                f.Test("\"blah\"",              "blah"),        // word with no spaces
                f.Test("\" blah\"",             " blah"),       // space preceeding word
                f.Test("\" blah \"",            " blah "),      // spaces flanking word 
                f.Test("\"(blah)\"",            "(blah)"),      // word in parentheses
                f.Test("\"\\\"blah\\\"\"",      "\"blah\""),    // word in escaped quotes
                f.Test("\"\\n\\\"blah\\\"\"",   "\n\"blah\""),   // word and escaped newline in escaped quotes
                f.Test("\"\\\"\\nblah\\\"\"",   "\"\nblah\""),   // ...different position
                f.Test("\"\\\"blah\\n\\\"\"",   "\"blah\n\""),   // ...different position
                f.Test("\"\\\"blah\\\"\\n\"",   "\"blah\"\n"),   // ...different position

                // test single-substrinbg input, containing all possible character literals.
                // https://msdn.microsoft.com/en-us/library/aa691087(v=vs.71).aspx
                f.Test("\"\\\'\\\"\\\\\\0\\a\\b\\f\\\\n\\r\\t\\v\\xf\\x2a\\x02a\\0x002a9\\u002a\\U0000002a\"",
                                   "\'\"\\\0\a\b\f\\n\r\t\v\xf\x2a\x02a\0x002a9\u002a\U0000002a"),

                // Test verbatim string with escaped double-quotes (the only possible escaped character).
                // https://msdn.microsoft.com/en-us/library/aa691090(v=vs.71).aspx
                f.Test("@\"abc 123\"\"\"\"\n\\\\\\n\\r\\t\\a\n\"", "abc 123\"\"\n\\\\\\n\\r\\t\\a\n"),

                // test multiple-substring input
                f.Test("\"\" + \"\"",                   ""),
                f.Test("\"\" + \"One\"",                "One"),
                f.Test("\" \" + \"One\"",               " One"),
                f.Test("\"One\" + \" \"",               "One "),
                f.Test("\"\" + \"\" + \"\"",            ""),
                f.Test("\"\" + \"One\" + \"Two\"",      "OneTwo"),
                f.Test("\" \" + \"One\" + \"Two\"",     " OneTwo"),
                f.Test("\"One\" + \" \" + \"Two\"",     "One Two"),
                f.Test("\"One\" + \"Two\" + \"Three\"", "OneTwoThree"),
            };

            // Test: matchable inputs (without extraneous string data before or after)
            f.PreMatch = "";
            f.PostMatch = "";
            foreach (var test in validTests)
                f.TestForSuccess(test);

            // Test: matchable inputs (with whitespace before and extraneous text after)
            f.PreMatch = " \n \n";
            f.PostMatch = " , blah";
            foreach (var test in validTests)
                f.TestForSuccess(test);

            // Test: matchable inputs (with extraneous whitespace before, and "matchable but out-of-reach" string data after)
            f.PreMatch = "  \n  \n";
            f.PostMatch = " , " + validTests[3].Input[0];
            foreach (var test in validTests)
                f.TestForSuccess(test);
        }

        [Test]
        public void TestTryParseStringLiteralFromEnd()
        {
            string literal;
            bool success = Extractor.TryParseStringLiteralFromEnd(
                "", 0, out literal);
            Assert.IsFalse(success);

            success = Extractor.TryParseStringLiteralFromEnd(
                "abc", 2, out literal);
            Assert.IsFalse(success);

            success = Extractor.TryParseStringLiteralFromEnd(
                "\"\"", 1, out literal);
            Assert.IsTrue(success);
            Assert.AreEqual("", literal);

            success = Extractor.TryParseStringLiteralFromEnd(
                "\"abc\"", 4, out literal);
            Assert.IsTrue(success);
            Assert.AreEqual("abc", literal);

            success = Extractor.TryParseStringLiteralFromEnd(
                "@\"abc\"", 5, out literal);
            Assert.IsTrue(success);
            Assert.AreEqual("abc", literal);

            success = Extractor.TryParseStringLiteralFromEnd(
                "@\"abc_\"\"@/%\"", 11, out literal);
            Assert.IsTrue(success);
            Assert.AreEqual("abc_\"@/%", literal);

            success = Extractor.TryParseStringLiteralFromEnd(
                "\"\\\\abc\\\\def\"", 11, out literal);
            Assert.IsTrue(success);
            Assert.AreEqual("\\abc\\def", literal);

            var test     = "@\"\\t\" + \"\\\\abc\\\\def\"";
            var expected = "\\abc\\def";
            success = Extractor.TryParseStringLiteralFromEnd(test, test.Length - 1, out literal);
            Assert.IsTrue(success);
            Assert.AreEqual(expected, literal);

            test        = " + \".\\n\" +\r\n    \"Error\"";
            expected    = "Error";
            success = Extractor.TryParseStringLiteralFromEnd(test, test.Length - 1, out literal);
            Assert.IsTrue(success);
            Assert.AreEqual(expected, literal);
        }

        [Test]
        public void TestTryParseMethodParams()
        {
            const string kQuote = "\"";
            const string kBackslash = "\\";
            const string kEscapedQuote = kBackslash + kQuote;

            // return a new string containing the input string in quotes
            var quoted = new Func<string, string>(s => kQuote + s + kQuote);

            string sub1, sub2, sub3;
            string test;
            string expectedLiteral;
            string literal;
            int expectedAfterEnd;
            int afterEnd;

            Assert.IsFalse(RunTryParseMethodParams("", 0, out literal, out afterEnd));          // no string, fail
            Assert.IsFalse(RunTryParseMethodParams("abc", 0, out literal, out afterEnd));       // no method-argument-terminating-character, fail
            Assert.IsFalse(RunTryParseMethodParams(" abc(); ", 0, out literal, out afterEnd));  // no method-argument-terminating-character, fail
            Assert.IsFalse(RunTryParseMethodParams(quoted(""), 0, out literal, out afterEnd));  // no method-argument-terminating-character, fail

            // empty string literal with terminating character ',' (no spaces)
            expectedLiteral = "";
            test = quoted(expectedLiteral) + ",";
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // empty string literal with terminating character ',' (spaces flanking the literal)
            expectedLiteral = "";
            test = " " + quoted(expectedLiteral) + " ,";
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // empty string literal with terminating character ')' (no spaces)
            expectedLiteral = "";
            test = quoted(expectedLiteral) + ")";
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // empty string literal with terminating character ')' (spaces flanking the literal)
            expectedLiteral = "";
            test = " " + quoted(expectedLiteral) + " )";
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // simple single-char string literal with terminating character ')' (no spaces)
            expectedLiteral = "a";
            test = quoted(expectedLiteral) + ")";
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // simple single-char string literal with terminating character ')' (spaces flanking the literal)
            expectedLiteral = "a";
            test = " " + quoted(expectedLiteral) + ")";
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // simple multiple-char string literal with terminating character ')' (spaces flanking the literal)
            expectedLiteral = "abc";
            test = " " + quoted(expectedLiteral) + ")";
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // simple multiple-char string literal with terminating character ')' (extra chars after terminating character)
            expectedLiteral = "abc";
            test = " " + quoted(expectedLiteral) + ",";
            expectedAfterEnd = test.Length;
            test += "blah blah blah";
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // complex string literal with terminating char ')'
            sub1 = "abc";
            sub2 = "def";
            expectedLiteral = sub1 + sub2;
            var subTest = quoted(sub1) + " + " + quoted(sub2);
            test = " " + subTest + ")";
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // complex string literal with terminating char ')' (one string literal is verbatim)
            sub1 = "abc";
            sub2 = "def";
            expectedLiteral = sub1 + sub2;
            subTest = "@" + quoted(sub1) + " + " + quoted(sub2);
            test = " " + subTest + ")";
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // complex string literal with terminating char ')' (both string literals are verbatim)
            sub1 = "abc";
            sub2 = "def";
            expectedLiteral = sub1 + sub2;
            subTest = "@" + quoted(sub1) + " + @" + quoted(sub2);
            test = " " + subTest + ")";
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // complex string literal with terminating char ')' (two of three string literals are verbatim)
            sub1 = "abc";
            sub2 = "def";
            sub3 = "ghi";
            expectedLiteral = sub1 + sub2 + sub3;
            subTest = "@" + quoted(sub1) + " + @" + quoted(sub2) + "+" + quoted(sub3);
            test = " " + subTest + ")";
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // simple string literal containing unbalanced escaped quotes
            expectedLiteral = "ghi\'" + kEscapedQuote;
            test = kQuote + expectedLiteral + kQuote + "X)";
            expectedLiteral = expectedLiteral.Replace(kEscapedQuote, kQuote);
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // simple string literal containing balanced escaped quotes
            sub1 = "ghi\'";
            expectedLiteral = kQuote + sub1 + kQuote;
            subTest = kEscapedQuote + sub1 + kEscapedQuote;
            test = kQuote + subTest + kQuote + "X)";
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // complex string literal containing unbalanced escaped quotes
            sub1 = "\'abc";
            sub2 = "def";
            sub3 = "ghi\'" + kQuote; // unbalanced escaped quote
            // quote chars in expected result string literals, need to be escaped quotes
            subTest = "@" + quoted(sub1) + " + @" + quoted(sub2) + "+" + quoted(sub3.Replace(kQuote, kEscapedQuote));
            test = " " + subTest + "X)";
            expectedLiteral = sub1 + sub2 + sub3;
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);

            // complex string literal containing balanced escaped quotes
            sub1 = "\'abc";
            sub2 = "def";
            sub3 = kQuote + "ghi\'" + kQuote; // balanced escaped quotes
            expectedLiteral = sub1 + sub2 + sub3;
            // quote chars in expected result string literals, need to be escaped quotes
            subTest = "@" + quoted(sub1) + " + @" + quoted(sub2) + "+" + quoted(sub3.Replace(kQuote, kEscapedQuote));
            test = " " + subTest + "X)";
            expectedAfterEnd = test.Length;
            Assert.IsTrue(RunTryParseMethodParams(test, 0, out literal, out afterEnd));
            Assert.AreEqual(expectedLiteral, literal);
            Assert.AreEqual(expectedAfterEnd, afterEnd);
        }

        // Extracts the list of localizable strings from the given *.cs file.
        // 'sourcePath' is assumed to be a filename in \Cases.
        private List<LocalizableString> ExtractFromCs(string sourcePath)
        {
            string finalPath = Path.Combine("LocalizableStringExtractor\\Cases", sourcePath);
            var extractor = new Extractor();
            List<LocalizableString> strings = extractor.ExtractFromCs(finalPath).ToList();
            return strings;
        }

        private List<LocalizableString> ExtractFromXaml(string sourcePath)
        {
            string finalPath = Path.Combine("LocalizableStringExtractor\\Cases", sourcePath);
            var extractor = new Extractor();
            List<LocalizableString> strings = extractor.ExtractFromXaml(finalPath).ToList();
            return strings;
        }

        [Test]
        public void TestExtractLocalizableStrings()
        {
            var extractor = new Extractor();
            const string destinationXml = "LocalizationCopy.xml";
            try
            {
                File.Copy(@"LocalizableStringExtractor\Cases\Localization.xml", destinationXml, true);
                extractor.ExtractLocalizableStrings(
                    new [] { @"LocalizableStringExtractor\Cases" },
                    destinationXml,
                    EmptyArray<string>.Instance);

                // Read back in the output XML file and check that it has the right data.
                var stringItems = new List<LocalizableString>(extractor.ReadLocalizationFile(destinationXml));

                LocalizableString item = stringItems.Find(a => a.Text == "Original" && a.Translation == "Translation");
                Assert.IsTrue(item != null);

                item = stringItems.Find(a => a.Text == @"%$Name/\Symbols$%"); //from \Cases\Standard.xaml
                Assert.IsTrue(item != null);

                item = stringItems.Find(a => a.Text == @"About"); //from \Cases\AboutDialog.xaml
                Assert.IsTrue(item != null);

                item = stringItems.Find(a => a.Text == "Speaker" && a.Context == "an electronic speaker, for playing sounds");
                Assert.IsTrue(item != null && item.Translation == "Speaker translated");

                item = stringItems.Find(a => a.Text == "Speaker" && a.Context == "a person speaking at a podium");
                Assert.IsTrue(item != null && item.Translation == "Speaker translated");
            }
            finally
            {
                File.Delete(destinationXml);
            }
        }

        private bool RunTryParseMethodParams(string s, int iFirstChar, out string literal, out int afterEnd)
        {
            return RunTryParseMethodParams(s, iFirstChar, s.Length, out literal, out afterEnd);
        }

        private bool RunTryParseMethodParams(string s, int iFirstChar, int stopIndex, out string literal, out int afterEnd)
        {
            return Extractor.TryParseMethodParams(s, iFirstChar, stopIndex, out literal, out afterEnd);
        }

        private class TestParseForStringLiteralFixture
        {
            public TestParseForStringLiteralFixture(string preMatch, string postMatch)
            {
                PreMatch = preMatch;
                PostMatch = postMatch;
            }

            public string PreMatch { get; set; }

            public string PostMatch { get; set; }

            public void TestForSuccess(TestData testData) { RunTest(testData, true); }

            public void TestForSuccess(string input, string expectedMatch, int expectedAfterLiteral) { RunTest(input, expectedMatch, expectedAfterLiteral, true); }

            public void TestForFail(string expectedMatch)
            {
                RunTest(expectedMatch, expectedMatch, expectedMatch.Length, false);
            }

            private void RunTest(TestData testData, bool assertSuccess)
            {
                // Add the leading and trailing strings to the input
                var input = PreMatch + testData.Input + PostMatch;
                var expectedAfterLiteral = PreMatch.Length + testData.Input.Length;

                RunTest(input, testData.Expected, expectedAfterLiteral, assertSuccess);
            }

            private void RunTest(string input, string expected, int expectedAfterLiteral, bool assertSuccess)
            {
                string actual;
                int actualAfterLiteral;

                Assert.AreEqual(assertSuccess, Extractor.TryParseForStringLiteral(input, 0, input.Length, out actual, out actualAfterLiteral));
                if (assertSuccess)
                {
                    Assert.AreEqual(expected, actual);
                    Assert.AreEqual(expectedAfterLiteral, actualAfterLiteral);
                }
            }

            public class TestData
            {
                public TestData(string input, string expected)
                {
                    Input = input;
                    Expected = expected;
                }

                public string Input { get; private set; }
                public string Expected { get; private set; }
            }

            public TestData Test(string input, string expected)
            {
                return new TestData(input, expected);
            }
        }
    }
}
