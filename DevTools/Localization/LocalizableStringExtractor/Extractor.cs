//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;

using PathUtilities = Atf.Utilities.PathUtilities;

namespace LocalizableStringExtractor
{
    /// <summary>
    /// Utility class that uses a settings file and extracts strings that need to be localized
    /// from code files relative to the root of the ATF directory. See DirectoriesToLocalize.txt.</summary>
    public class Extractor
    {
        /// <summary>
        /// Extracts all localizable strings whose paths are in the settings file and creates
        /// XML files according to the settings' output filenames. Places the log file in the
        /// clipboard and shows a modal dialog box to the user when finished.</summary>
        public void ExtractAll()
        {
            Progress = 0.0;
            m_log.Clear();
            ReadSettings();
            foreach (SourceTargetPair rule in m_directories)
            {
                ExtractLocalizableStrings(rule.Source,rule.Target);
            }
            Progress = 1.0;
            ShowLog();
        }

        /// <summary>
        /// Occurs when Progress has changed</summary>
        public event EventHandler ProgressChanged;

        /// <summary>
        /// Gets a value from 0 to 1 representing the progress of ExtractAll()</summary>
        public double Progress
        {
            get { return m_progress; }
            private set
            {
                if (m_progress != value)
                {
                    m_progress = value;
                    if (ProgressChanged != null)
                        ProgressChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Extracts localizable strings from the given root directory and outputs them to the given XML file.</summary>
        /// <param name="sourceRoot">the root of the source code directory to search for code files</param>
        /// <param name="xmlFile">the fully qualified path of the XML file that is to be created and
        /// populated with localizable string data. If this file exists already, it will be deleted.</param>
        public void ExtractLocalizableStrings(string sourceRoot, string xmlFile)
        {
            m_log.AppendLine("=================== STARTING ==================");
            m_log.AppendLine("Parsing: " + sourceRoot);

            int numCs = 0;
            int numXaml = 0;
            var stringData = new HashSet<LocalizableString>();
            string[] filePaths = Directory.GetFiles(sourceRoot, "*.*", SearchOption.AllDirectories);
            foreach (string filePath in filePaths)
            {
                if (filePath.EndsWith(".cs"))
                {
                    numCs++;
                    foreach (LocalizableString stringItem in ExtractFromCs(filePath))
                        stringData.Add(stringItem);
                }
                else if (filePath.EndsWith(".xaml"))
                {
                    numXaml++;
                    foreach (LocalizableString stringItem in ExtractFromXaml(filePath))
                        stringData.Add(stringItem);
                }
                Progress += 1.0/(m_directories.Count*filePaths.Length);
            }

            // Sort them, to make searching within the xml file a bit easier.
            var sortedData = new List<LocalizableString>(stringData.Count);
            sortedData.AddRange(stringData);
            sortedData.Sort();

            m_log.AppendLine("Parsed " + numCs + " *.cs and " + numXaml + " *.xaml files.");
            m_log.AppendLine("Unique strings to localize: " + sortedData.Count);
            m_log.AppendLine("Writing: " + xmlFile);

            WriteLocalizationFile(xmlFile, sortedData);

            m_log.AppendLine("=================== FINISHED ==================");
        }

        public IEnumerable<LocalizableString> ExtractFromXaml(string filePath)
        {
            string file = File.ReadAllText(filePath);
            foreach (Match match in s_xamlLocRegex.Matches(file))
            {
                string localizableString = match.Groups[1].Value;
                yield return new LocalizableString(localizableString, string.Empty);
            }
        }

        private static Regex s_xamlLocRegex = new Regex(@"=""{\w+:Loc (.+?)}""", RegexOptions.Compiled);

        public IEnumerable<LocalizableString> ExtractFromCs(string filePath)
        {
            const string methodName = "Localize";
            string file = File.ReadAllText(filePath);
            int startSearch = 0;
            int localizeStart;
            while ((localizeStart = file.IndexOf(methodName, startSearch, StringComparison.InvariantCulture)) >= 0)
            {
                IList<string> paramStrings;
                if (ParseMethodCallOnStringLiterals(file, localizeStart, "Localize", out paramStrings))
                {
                    if (paramStrings.Count == 1)
                        yield return new LocalizableString(paramStrings[0], string.Empty);
                    else if (paramStrings.Count == 2)
                        yield return new LocalizableString(paramStrings[0], paramStrings[1]);
                }
                startSearch = localizeStart + methodName.Length;
            }
        }

        /// <summary>
        /// Parses the method call with one or two string literal parameters</summary>
        /// <param name="s">The string to search</param>
        /// <param name="methodNameIndex">Index of the beginning of the method name</param>
        /// <param name="methodName">Name of the method</param>
        /// <param name="paramStrings">The parameter strings</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The string must begin with the given method name at the given index</exception>
        public static bool ParseMethodCallOnStringLiterals(string s, int methodNameIndex, string methodName, out IList<string> paramStrings)
        {
            if (!SubstringEquals(s, methodNameIndex, methodName))
                throw new InvalidOperationException("The string must begin with the given method name at the given index");

            paramStrings = new List<string>();
            
            // Make sure we're at the start of a method name.
            if (methodNameIndex > 0 && IsValidNameChar(s[methodNameIndex - 1]))
                return false;

            // Make sure the next character is a '('
            int afterNameIndex = methodNameIndex + methodName.Length;
            afterNameIndex = SkipWhiteSpace(s, afterNameIndex);
            if (afterNameIndex >= s.Length || s[afterNameIndex] != '(')
                return false;

            // Make sure the line isn't commented out. This fails if the comment is inside a multi-line verbatim string literal!
            int lineStart = BackToLineStart(s, methodNameIndex);
            lineStart = SkipWhiteSpace(s, lineStart);
            if (SubstringEquals(s, lineStart, "//"))
                return false;

            // Check if this is an extension method on a string.
            //"".Localize() is the smallest possible string literal w/ extension method.
            string literal;
            if (methodNameIndex >= 3 && s[methodNameIndex - 1] == '.')
            {
                if (TryParseStringLiteralFromEnd(s, methodNameIndex - 2, out literal))
                    paramStrings.Add(literal);
            }

            // Get string literal parameter, if any. Skip past the '('.
            int paramIndex = afterNameIndex + 1;
            int endParamIndex;
            while (TryParseStringLiteralFromBeginning(s, paramIndex, out literal, out endParamIndex))
            {
                paramStrings.Add(literal);
                paramIndex = endParamIndex;
                paramIndex = SkipWhiteSpace(s, paramIndex);
                if (s[paramIndex] != ',')
                    break;
                paramIndex++;
            }

            return paramStrings.Count > 0;
        }

        /// <summary>
        /// Starts with '"' and work backwards. Constructs a string literal excluding the
        ///  beginning and ending double quotes. Concatenates a series of string literals
        ///  separated by '+'. Does not handle comments.</summary>
        /// <param name="s">The string to search</param>
        /// <param name="iLastChar">The zero-based index of the closing ".</param>
        /// <param name="literal">The resulting string literal, if successful.</param>
        /// <returns>True iff a string literal was found.</returns>
        public static bool TryParseStringLiteralFromEnd(string s, int iLastChar, out string literal)
        {
            literal = string.Empty;

            if (iLastChar >= s.Length || s[iLastChar] != '"')
                return false;

            // Parsing escape sequences going backwards is difficult. Let's find the beginning
            //  of all the concatenated strings and then parse going forward.
            bool addingLiterals = true;
            while (--iLastChar >= 0)
            {
                char c = s[iLastChar];
                if (addingLiterals)
                {
                    // Look for the beginning of the string literal, but check for an escape character.
                    // In a normal string literal, '\' is the escape character.
                    // In an verbatim string literal, a double-quote is the escape character.
                    char prevC = iLastChar > 0 ? s[iLastChar - 1] : ' ';
                    if (c == '"' && prevC != '\\' && prevC != '"')
                    {
                        // We hit an unescaped double-quote. End the string literal for now.
                        // Check for the verbatim string literal symbol '@'.
                        if (prevC == '@')
                            iLastChar--;
                        addingLiterals = false;
                    }
                }
                else
                {
                    // Check for string concatenation.
                    if (c == '"')
                        addingLiterals = true;
                    // If it's not the '+' between string literals, then we're done.
                    else if (c != '+' && !Char.IsWhiteSpace(c))
                        break;
                }
            }

            // Now go forwards.
            iLastChar++;
            int afterEnd;
            return TryParseStringLiteralFromBeginning(s, iLastChar, out literal, out afterEnd);
        }

        /// <summary>
        /// Start with '"' and work forwards. Construct a string literal excluding the
        ///  beginning and ending double quotes. Concatenate a series of string literals
        ///  separated by '+'. Does not handle comments.</summary>
        /// <param name="s">The string to search</param>
        /// <param name="iFirstChar">The zero-based index to begin searching for the opening "
        /// or the @" (if it's a verbatim string literal). Initial whitespace will be skipped.</param>
        /// <param name="literal">The resulting string literal, if successful.</param>
        /// <param name="afterEnd">The index after the last closing double-quote.</param>
        /// <returns>True iff a string literal was found.</returns>
        public static bool TryParseStringLiteralFromBeginning(string s, int iFirstChar, out string literal, out int afterEnd)
        {
            afterEnd = iFirstChar;
            string oneLiteral;
            bool success = false;
            var sb = new StringBuilder();
            while (iFirstChar < s.Length)
            {
                if (TryParseSingleStringLiteralFromBeginning(s, iFirstChar, out oneLiteral, out afterEnd))
                {
                    success = true;
                    sb.Append(oneLiteral);
                    iFirstChar = SkipSpaceBetweenConcatenatedStrings(s, afterEnd);
                    continue;
                }
                break;
            }
            literal = sb.ToString();
            return success;
        }

        private static int SkipSpaceBetweenConcatenatedStrings(string s, int iFirstChar)
        {
            while (iFirstChar < s.Length)
            {
                char c = s[iFirstChar++];
                if (c != '+' && !Char.IsWhiteSpace(c))
                {
                    iFirstChar--;
                    break;
                }
            }
            return iFirstChar;
        }

        private static bool TryParseSingleStringLiteralFromBeginning(string s, int iFirstChar, out string literal, out int afterEnd)
        {
            literal = string.Empty;
            afterEnd = iFirstChar;

            iFirstChar = SkipWhiteSpace(s, iFirstChar);

            bool verbatim = false;
            if (SubstringEquals(s, iFirstChar, "@\""))
            {
                verbatim = true;
                iFirstChar += 2;
            }
            else if (SubstringEquals(s, iFirstChar, "\""))
            {
                iFirstChar++;
            }
            else
            {
                return false;
            }

            var sb = new StringBuilder();
            while (iFirstChar < s.Length)
            {
                char c;
                if (TryGetChar(s, ref iFirstChar, verbatim, out c))
                    sb.Append(c);
                else
                {
                    iFirstChar++; //skip past the "
                    break;
                }
            }

            literal = sb.ToString();
            afterEnd = iFirstChar;
            return true;
        }

        /// <summary>
        /// Tries to get the next character in a C# string literal.</summary>
        /// <param name="s">The string to search.</param>
        /// <param name="iFirstChar">The first character. Will be incremented with however many
        /// characters are actually read from 's'. Will stay the same if an unescaped " is found.</param>
        /// <param name="verbatim">True if the string literal to be parsed is a verbatim C#
        /// string literal. These are strings that begin with @" in the code.</param>
        /// <param name="c">The resulting character, if successful.</param>
        /// <returns>True if a character was read or false if the closing " was found.</returns>
        public static bool TryGetChar(string s, ref int iFirstChar, bool verbatim, out char c)
        {
            if (iFirstChar >= s.Length)
            {
                c = ' ';
                return false;
            }

            c = s[iFirstChar++];
            char nextC = iFirstChar < s.Length ? s[iFirstChar] : ' ';

            // In an verbatim string literal, " is the only escape character.
            if (verbatim)
            {
                if (c == '"')
                {
                    if (nextC == '"')
                    {
                        iFirstChar++;
                        return true;
                    }
                    iFirstChar--;
                    return false;
                }
                return true;
            }

            // Options: https://msdn.microsoft.com/en-us/library/aa691090%28v=vs.71%29.aspx
            if (c == '\\')
            {
                iFirstChar++;
                // simple-escape-sequence
                if (nextC == '\'')
                    c = '\'';
                else if (nextC == '"')
                    c = '"';
                else if (nextC == '\\')
                    c = '\\';
                else if (nextC == '0')
                    c = '\0';
                else if (nextC == 'a')
                    c = '\a';
                else if (nextC == 'b')
                    c = '\b';
                else if (nextC == 'f')
                    c = '\f';
                else if (nextC == 'n')
                    c = '\n';
                else if (nextC == 'r')
                    c = '\r';
                else if (nextC == 't')
                    c = '\t';
                else if (nextC == 'v')
                    c = '\v';
                // hexadecimal-escape-sequence
                else if (nextC == 'x')
                    c = (char)GetHexDigits(s, ref iFirstChar, 4);
                // unicode-escape-sequence https://msdn.microsoft.com/en-us/library/aa664669(v=vs.71).aspx
                else if (nextC == 'u')
                    c = (char)GetHexDigits(s, ref iFirstChar, 4);
                else if (nextC == 'U')
                    c = (char)GetHexDigits(s, ref iFirstChar, 8);
            }
            else if (c == '"')
            {
                iFirstChar--;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets hexadecimal digits from the given string</summary>
        /// <param name="s">The string to search</param>
        /// <param name="iFirstChar">The zero-based index to start at</param>
        /// <param name="maxDigits">The maximum number of hexadecimal digits to parse</param>
        /// <returns>The resulting number</returns>
        public static ulong GetHexDigits(string s, ref int iFirstChar, int maxDigits)
        {
            ulong hex = 0;
            for (long i = 0; i < maxDigits; i++)
            {
                char c = s[iFirstChar];
                ushort hexDigit;
                if (!TryGetHexDigit(c, out hexDigit))
                    break;
                hex = hex*16 + hexDigit;
                iFirstChar++;
            }
            return hex;
        }

        /// <summary>
        /// Tries getting a hexadecimal digit from the given character</summary>
        /// <param name="c">The character to test</param>
        /// <param name="hexDigit">The hexadecimal digit</param>
        /// <returns>True if successful</returns>
        public static bool TryGetHexDigit(char c, out ushort hexDigit)
        {
            hexDigit = 0;
            if (c >= '0' && c <= '9')
            {
                hexDigit = (ushort)(c - '0');
                return true;
            }
            if (c >= 'a' && c <= 'f')
            {
                hexDigit = (ushort)(c - 'a' + 10);
                return true;
            }
            if (c >= 'A' && c <= 'F')
            {
                hexDigit = (ushort)(c - 'A' + 10);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Skips forward past any white space</summary>
        /// <param name="s">The string to search</param>
        /// <param name="startIndex">The start index</param>
        /// <returns>The index of the next non-whitespace character, or a number equal
        /// to length of the string</returns>
        public static int SkipWhiteSpace(string s, int startIndex)
        {
            while (startIndex < s.Length && Char.IsWhiteSpace(s[startIndex]))
                startIndex++;
            return startIndex;
        }

        /// <summary>
        /// Returns the index of the beginning of the line before 'startIndex'</summary>
        /// <param name="s">The string to search</param>
        /// <param name="startIndex">The start index</param>
        /// <returns></returns>
        public static int BackToLineStart(string s, int startIndex)
        {
            // The actual string in Windows is "\r\n", but once we find '\n', we're done.
            while (startIndex > 0 && s[startIndex] != '\n')
                startIndex--;
            return startIndex;
        }

        /// <summary>
        /// Compares the given substring to another string</summary>
        /// <param name="s">The string to compare a part of</param>
        /// <param name="startIndex">The start index of 's'</param>
        /// <param name="comparison">The string to compare to</param>
        /// <returns>'true' if the sub-string of 's' equals 'comparison' and 'false' otherwise</returns>
        public static bool SubstringEquals(string s, int startIndex, string comparison)
        {
            if (startIndex + comparison.Length > s.Length)
                return false;
            if (comparison.Length == 0)
                return true;

            int i = comparison.Length - 1;
            do
            {
                if (s[startIndex + i] != comparison[i])
                    return false;
            } while (--i >= 0);

            return true;
        }

        /// <summary>
        /// Gets whether the given character is valid in a C# identifier</summary>
        /// <param name="c">The character</param>
        /// <returns></returns>
        public static bool IsValidNameChar(char c)
        {
            return c == '_' || Char.IsLetterOrDigit(c);
        }

        /// <summary>
        /// Creates an XML file with the given name and writes the given string data to that file.</summary>
        /// <param name="outputFile">the name and path of the XML file to be created</param>
        /// <param name="stringData">the localizable string data to write to the XML file</param>
        public void WriteLocalizationFile(string outputFile, IEnumerable<LocalizableString> stringData)
        {
            // Prepare the document in memory.
            var xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0",System.Text.Encoding.UTF8.WebName, "yes"));

            XmlElement root = xmlDoc.CreateElement("StringLocalizationTable");
            xmlDoc.AppendChild(root);

            var sisulizer = new SisulizerWorkaround(m_log);

            foreach (LocalizableString stringItem in stringData)
            {
                XmlElement stringElement = xmlDoc.CreateElement("StringItem");
                root.AppendChild(stringElement);

                string workaroundContext;
                sisulizer.FixContext(stringItem.Text, stringItem.Context, out workaroundContext);

                // id -- the English string that is used as a key at run-time to look-up the translation.
                stringElement.SetAttribute("id", stringItem.Text);

                // context -- will be read by some other means and given to translation company.
                stringElement.SetAttribute("context", workaroundContext);

                // translation -- starts out as English but will be replaced by actual translation
                //  by Sisulizer, for example.
                stringElement.SetAttribute("translation", stringItem.Text);
            }

            // Write out the document.
            using (var writer = new XmlTextWriter(outputFile, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                xmlDoc.WriteTo(writer);
                writer.Flush();
            }
        }

        // This works around a bug in Sisulizer 1.6 where if an id (original string) has
        //  "{0}" (or most likely any "{n}") and a non-empty context, then the context must
        //  be unique, apparently, or it gets ignored.
        private class SisulizerWorkaround
        {
            public SisulizerWorkaround(StringBuilder log)
            {
                m_log = log;
            }

            public void FixContext(string id, string context, out string workaroundContext)
            {
                workaroundContext = context;
                if (!string.IsNullOrEmpty(context) && id.Contains("{0}"))
                {
                    // Just add spaces until it's unique? This might be the least confusing to a translator.
                    // There should be very few duplicates like this.
                    while (!m_uniqueContexts.Add(workaroundContext))
                    {
                        workaroundContext += ' ';
                        m_log.AppendLine("Warning: this id will have its context modified" +
                            " to make it unique to work around a Sisulizer bug: " + id);
                    }
                }
            }

            private readonly HashSet<string> m_uniqueContexts = new HashSet<string>();
            private readonly StringBuilder m_log;
        }

        /// <summary>
        /// Gets the path for the root directory of the ATF installation, ending with a '\'. For example:
        /// "C:\sceadev\wws_shared\sdk\trunk\components\wws_atf\"</summary>
        /// <returns></returns>
        public static string GetAtfRoot()
        {
            string dir = Directory.GetCurrentDirectory();
            string[] dirElements = PathUtilities.GetPathElements(dir);
            
            int atfIndex = dirElements.Length - 1;
            for (; atfIndex >= 0; atfIndex--)
            {
                if (dirElements[atfIndex].ToLower().Contains("atf"))
                    break;
            }
            if (atfIndex < 0)
                throw new InvalidOperationException("Could not find root of ATF installation, like 'wws_atf'");

            return PathUtilities.CreatePath(dirElements, 0, atfIndex, true);
        }

        /// <summary>
        /// A pair of fully qualified paths. The source is the root directory of the source code
        /// and the target is the name of the output XML file.</summary>
        public class SourceTargetPair
        {
            public SourceTargetPair(string source, string target)
            {
                Source = source;
                Target = target;
            }
            public readonly string Source;
            public readonly string Target;
        }

        /// <summary>
        /// Shows the log file for all log output of this Extractor object</summary>
        public void ShowLog()
        {
            if (m_log.Length == 0)
                return;

            Clipboard.SetText(m_log.ToString());
            MessageBox.Show("A log report was pasted into the clipboard");
        }

        /// <summary>
        /// Opens the settings file</summary>
        public void OpenSettingsFile()
        {
            Process.Start(GetSettingsPath());
        }

        private static string GetSettingsPath()
        {
            string atfRootDir = GetAtfRoot();
            string settingsPath = Path.Combine(atfRootDir, @"DevTools\Localization\DirectoriesToLocalize.txt");
            return settingsPath;
        }

        /// <summary>
        /// Parses the settings file which contains the paths of the
        /// directories that should be examined for localizable strings.</summary>
        private void ReadSettings()
        {
            string atfRootDir = GetAtfRoot();
            string[] lines = File.ReadAllLines(GetSettingsPath());
            m_directories = new List<SourceTargetPair>();

            foreach (string line in lines)
            {
                if (line.Length == 0 || line[0] == '#')
                    continue;

                //break these:
                //  Framework\Atf.Perforce Framework\Atf.Perforce\Resources\Localization.xml
                //  "Samples\CircuitEditor" Samples\CircuitEditor\Resources\Localization.xml
                //into the separate source and target paths and combine with the ATF root directory.

                // "[^"]+"|[^ ]+ matches anything in quotes or anything without spaces.
                MatchCollection paths = Regex.Matches(line, "\"[^\"]+\"|[^ ]+");
                if (paths.Count != 2)
                    throw new InvalidOperationException(string.Format("bad settings line:{0}", line));

                string source = paths[0].ToString();
                source = source.Trim('\"');
                source = Path.Combine(atfRootDir, source);

                string target = paths[1].ToString();
                target = target.Trim('\"');
                target = Path.Combine(atfRootDir, target);

                m_directories.Add(new SourceTargetPair(source, target));
            }
        }

        private double m_progress;
        private List<SourceTargetPair> m_directories;
        private readonly StringBuilder m_log = new StringBuilder();
    }
}
