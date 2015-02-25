//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalizableStringExtractor;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class TestExtractor
    {
        [Test]
        public void TestExtractFromCs()
        {
            List<LocalizableString> strings = ExtractFromCs("Cases/AboutDialog.cs");
            Assert.IsTrue(strings.Count == 2);
            Assert.AreEqual(strings[0].Text, "About");
            Assert.AreEqual(strings[0].Context, "");
            Assert.AreEqual(strings[1].Text, "Authoring Tools Framework (ATF {0}), by Ron Little, Jianhua Shen," +
                                             " Julianne Harrington, Alan Beckus, Matt Mahony, Pat O'Leary, Paul Skibitzke, and Max Elliott." +
                                             " Copyright © 2014 Sony Computer Entertainment America LLC");
            Assert.AreEqual(strings[1].Context, "{0} is the version number");
        }

        [Test]
        public void TestExtractFromXaml()
        {
            List<LocalizableString> strings = ExtractFromXaml("Cases/Standard.xaml");
            Assert.IsTrue(strings.Count == 2);
            Assert.AreEqual(strings[0].Text, "Name With Spaces");
            Assert.AreEqual(strings[1].Text, @"%$Name/\Symbols$%");
        }

        [Test]
        public void TestExtractFromXaml2()
        {
            List<LocalizableString> strings = ExtractFromXaml("Cases/FindTargetsDialog.xaml");
            Assert.IsTrue(strings.Count == 12);
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
        public void ParseMethodCall()
        {
            IList<string> paramStrings;
            
            // Test that the whole method name matches
            bool success = Extractor.ParseMethodCallOnStringLiterals(
                "WrongLocalize(\"some text\")", 5,
                "Localize", out paramStrings);
            Assert.IsFalse(success);

            // Test that the whole method name matches
            success = Extractor.ParseMethodCallOnStringLiterals(
                "LocalizeSomethingElse(\"some text\")", 0,
                "Localize", out paramStrings);
            Assert.IsFalse(success);

            // Test a simple one parameter method call
            success = Extractor.ParseMethodCallOnStringLiterals(
                "Localize(aVariable)", 0,
                "Localize", out paramStrings);
            Assert.IsFalse(success);
            Assert.IsTrue(paramStrings.Count == 0);

            // Test a simple one parameter method call with a string literal
            success = Extractor.ParseMethodCallOnStringLiterals(
                "Localize(\"some text\")", 0,
                "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.IsTrue(paramStrings.Count == 1);
            Assert.IsTrue(paramStrings[0] == "some text");

            // Test a simple two parameter method call
            success = Extractor.ParseMethodCallOnStringLiterals(
                "Localize(firstVariable, anotherVariable)", 0,
                "Localize", out paramStrings);
            Assert.IsFalse(success);
            Assert.IsTrue(paramStrings.Count == 0);

            // Test a simple two parameter method call
            success = Extractor.ParseMethodCallOnStringLiterals(
                "Localize(\"some text\", \"some context\")", 0,
                "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.IsTrue(paramStrings.Count == 2);
            Assert.IsTrue(paramStrings[0] == "some text");
            Assert.IsTrue(paramStrings[1] == "some context");

            // Test an extension method with no explicit parameters
            success = Extractor.ParseMethodCallOnStringLiterals(
                "\"some text\".Localize()", 12,
                "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.IsTrue(paramStrings.Count == 1);
            Assert.IsTrue(paramStrings[0] == "some text");

            // Test an extension method with one explicit parameter
            success = Extractor.ParseMethodCallOnStringLiterals(
                "\"some text\".Localize( \"some context\")", 12,
                "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.IsTrue(paramStrings.Count == 2);
            Assert.IsTrue(paramStrings[0] == "some text");
            Assert.IsTrue(paramStrings[1] == "some context");

            // Test an extension method with one explicit parameter. Parse string concatenations.
            string test = "\"some text\" + \" that is rather long\".Localize( \"some context\")";
            int startIndex = test.IndexOf("Localize", StringComparison.InvariantCulture);
            success = Extractor.ParseMethodCallOnStringLiterals(test, startIndex, "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.IsTrue(paramStrings.Count == 2);
            Assert.IsTrue(paramStrings[0] == "some text that is rather long");
            Assert.IsTrue(paramStrings[1] == "some context");

            test = @"
        public static DocumentClientInfo DocumentClientInfo = new DocumentClientInfo(
            ""Event Sequence"".Localize(),
            new string[] { "".xml"", "".esq"" },
            Sce.Atf.Resources.DocumentImage,
            Sce.Atf.Resources.FolderImage,
            true);";
            startIndex = test.IndexOf("Localize", StringComparison.InvariantCulture);
            success = Extractor.ParseMethodCallOnStringLiterals(test, startIndex, "Localize", out paramStrings);
            Assert.IsTrue(success);
            Assert.IsTrue(paramStrings.Count == 1);
            Assert.IsTrue(paramStrings[0] == "Event Sequence");
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
            Assert.IsTrue(literal == "");

            success = Extractor.TryParseStringLiteralFromEnd(
                "\"abc\"", 4, out literal);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "abc");

            success = Extractor.TryParseStringLiteralFromEnd(
                "@\"abc\"", 5, out literal);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "abc");

            success = Extractor.TryParseStringLiteralFromEnd(
                "@\"abc_@/%\"", 9, out literal);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "abc_@/%");

            success = Extractor.TryParseStringLiteralFromEnd(
                "\"\\\\abc\\\\def\"", 11, out literal);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "\\abc\\def");

            string test = "@\"\\t\" + \"\\\\abc\\\\def\"";
            success = Extractor.TryParseStringLiteralFromEnd(test, test.Length - 1, out literal);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "\\t\\abc\\def");
        }

        [Test]
        public void TestTryParseStringLiteralFromBeginning()
        {
            string literal;
            int afterEnd;
            bool success = Extractor.TryParseStringLiteralFromBeginning(
                "", 0, out literal, out afterEnd);
            Assert.IsFalse(success);

            success = Extractor.TryParseStringLiteralFromBeginning(
                "abc", 0, out literal, out afterEnd);
            Assert.IsFalse(success);

            success = Extractor.TryParseStringLiteralFromBeginning(
                " abc(); ", 1, out literal, out afterEnd);
            Assert.IsFalse(success);

            success = Extractor.TryParseStringLiteralFromBeginning(
                "\"\"", 0, out literal, out afterEnd);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "");
            Assert.IsTrue(afterEnd == 2);

            success = Extractor.TryParseStringLiteralFromBeginning(
                "\"a\"", 0, out literal, out afterEnd);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "a");
            Assert.IsTrue(afterEnd == 3);

            success = Extractor.TryParseStringLiteralFromBeginning(
                " \"a\"", 0, out literal, out afterEnd);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "a");
            Assert.IsTrue(afterEnd == 4);

            success = Extractor.TryParseStringLiteralFromBeginning(
                " \"abc\"", 0, out literal, out afterEnd);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "abc");
            Assert.IsTrue(afterEnd == 6);

            success = Extractor.TryParseStringLiteralFromBeginning(
                " \"abc\",blah blah blah", 0, out literal, out afterEnd);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "abc");
            Assert.IsTrue(afterEnd == 6);

            success = Extractor.TryParseStringLiteralFromBeginning(
                " \"abc\" + \"def\"", 0, out literal, out afterEnd);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "abcdef");
            Assert.IsTrue(afterEnd == 14);

            success = Extractor.TryParseStringLiteralFromBeginning(
                " @\"abc\" + \"def\"", 0, out literal, out afterEnd);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "abcdef");
            Assert.IsTrue(afterEnd == 15);

            success = Extractor.TryParseStringLiteralFromBeginning(
                " @\"abc\" + @\"def\"", 0, out literal, out afterEnd);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "abcdef");
            Assert.IsTrue(afterEnd == 16);

            success = Extractor.TryParseStringLiteralFromBeginning(
                " @\"abc\" + @\"def\"+\"ghi\"", 0, out literal, out afterEnd);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "abcdefghi");
            Assert.IsTrue(afterEnd == 22);

            string test = "\"ghi\'\\\"\"X";
            success = Extractor.TryParseStringLiteralFromBeginning(
                test, 0, out literal, out afterEnd);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "ghi'\"");
            Assert.IsTrue(test[afterEnd] == 'X');

            test = " @\"\'abc\" + @\"def\"+\"ghi\'\\\"\"X";
            success = Extractor.TryParseStringLiteralFromBeginning(
                test, 0, out literal, out afterEnd);
            Assert.IsTrue(success);
            Assert.IsTrue(literal == "'abcdefghi'\"");
            Assert.IsTrue(test[afterEnd] == 'X');
        }

        [Test]
        public void TestTryGetChar()
        {
            // Test empty string.
            char c;
            int iFirstChar = 0;
            bool result = Extractor.TryGetChar("", ref iFirstChar, false, out c);
            Assert.IsFalse(result);
            Assert.IsTrue(iFirstChar == 0);

            // Test normal string.
            iFirstChar = 0;
            result = Extractor.TryGetChar("abc\"", ref iFirstChar, false, out c);
            Assert.IsTrue(result);
            Assert.IsTrue(iFirstChar == 1);
            Assert.IsTrue(c == 'a');
            result = Extractor.TryGetChar("abc\"", ref iFirstChar, false, out c);
            Assert.IsTrue(result);
            Assert.IsTrue(iFirstChar == 2);
            Assert.IsTrue(c == 'b');
            result = Extractor.TryGetChar("abc\"", ref iFirstChar, false, out c);
            Assert.IsTrue(result);
            Assert.IsTrue(iFirstChar == 3);
            Assert.IsTrue(c == 'c');
            result = Extractor.TryGetChar("abc\"", ref iFirstChar, false, out c);
            Assert.IsFalse(result);
            Assert.IsTrue(iFirstChar == 3);

            // Test normal verbatim string.
            iFirstChar = 0;
            result = Extractor.TryGetChar("abc\"", ref iFirstChar, true, out c);
            Assert.IsTrue(result);
            Assert.IsTrue(iFirstChar == 1);
            Assert.IsTrue(c == 'a');
            result = Extractor.TryGetChar("abc\"", ref iFirstChar, true, out c);
            Assert.IsTrue(result);
            Assert.IsTrue(iFirstChar == 2);
            Assert.IsTrue(c == 'b');
            result = Extractor.TryGetChar("abc\"", ref iFirstChar, true, out c);
            Assert.IsTrue(result);
            Assert.IsTrue(iFirstChar == 3);
            Assert.IsTrue(c == 'c');
            result = Extractor.TryGetChar("abc\"", ref iFirstChar, true, out c);
            Assert.IsFalse(result);
            Assert.IsTrue(iFirstChar == 3);

            // Test normal string with all possible escape characters.
            // https://msdn.microsoft.com/en-us/library/aa691087(v=vs.71).aspx
            string test = @"\'\""\\\0\a\b\f\\n\r\t\v\xf\x2a\x02a\0x002a9\u002a\U0000002a";
            string correctS = "\'\"\\\0\a\b\f\\n\r\t\v\xf\x2a\x02a\0x002a9\u002a\U0000002a";
            iFirstChar = 0;
            foreach (char correctC in correctS)
            {
                result = Extractor.TryGetChar(test, ref iFirstChar, false, out c);
                Assert.IsTrue(result);
                Assert.IsTrue(c == correctC);
            }
            Assert.IsTrue(iFirstChar == test.Length);

            // Test verbatim string with escaped double-quotes (the only possible escaped character).
            // https://msdn.microsoft.com/en-us/library/aa691090(v=vs.71).aspx
            test =@"abc 123""""
\\\n\r\t\a
";
            correctS = @"abc 123""
\\\n\r\t\a
";
            iFirstChar = 0;
            foreach (char correctC in correctS)
            {
                result = Extractor.TryGetChar(test, ref iFirstChar, true, out c);
                Assert.IsTrue(result);
                Assert.IsTrue(c == correctC);
            }
            Assert.IsTrue(iFirstChar == test.Length);
        }

        private List<LocalizableString> ExtractFromCs(string sourcePath)
        {
            string finalPath = Path.Combine("..\\..", sourcePath);
            var extractor = new Extractor();
            List<LocalizableString> strings = extractor.ExtractFromCs(finalPath).ToList();
            return strings;
        }

        private List<LocalizableString> ExtractFromXaml(string sourcePath)
        {
            string finalPath = Path.Combine("..\\..", sourcePath);
            var extractor = new Extractor();
            List<LocalizableString> strings = extractor.ExtractFromXaml(finalPath).ToList();
            return strings;
        }
    }
}
