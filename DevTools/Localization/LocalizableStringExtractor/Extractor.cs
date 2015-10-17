//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace LocalizableStringExtractor
{
    /// <summary>
    /// Uses a settings file to specify pairs of a directory and the resulting XML file. See
    /// DirectoriesToLocalize.txt. This class parses *.csproj files and the referenced *.cs
    /// files and *.xaml files, looking for ATF-specific method calls on string literals.
    /// The output is one XML file for each directory (and its subdirectories, recursively)
    /// of source code.
    /// 
    /// This class doesn't output error messages or bring up dialog boxes. Use the Log
    /// property to access diagnostic output.
    /// 
    /// See "ATF Localization Guide.doc" for an explanation of how this localization process
    /// works.</summary>
    public class Extractor : INotifyPropertyChanged
    {
        /// <summary>
        /// Extracts all localizable strings whose paths are in the default settings file and
        /// creates XML files according to the settings' output filenames. Consider calling
        /// ShowLog() afterwards, or calling Log to get the log text directly.</summary>
        public void ExtractAll()
        {
            ExtractAll(GetSettingsPath());
        }

        /// <summary>
        /// Extracts all localizable strings whose paths are in the given settings file and
        /// creates XML files according to the settings' output filenames. Consider calling
        /// ShowLog() afterwards, or calling Log to get the log text directly.</summary>
        /// <param name="settingsPath">Path and filename of the settings file to load</param>
        public void ExtractAll(string settingsPath)
        {
            Progress = 0.0;
            m_cancelRequested = false;
            m_log.Clear();
            ReadSettings(settingsPath);
            foreach (TargetRule rule in m_directories)
            {
                ExtractLocalizableStrings(rule.SourceList, rule.Target, rule.IgnoreList);
                if (m_cancelRequested)
                    return;
            }
            Progress = 1.0;
        }

        /// <summary>
        /// Gets a value from 0 to 1 representing the progress of ExtractAll(). Subscribe to
        /// the WPF's PropertyChanged event to receive notifications of this property changing.</summary>
        public double Progress
        {
            get { return m_progress; }
            private set
            {
                if (m_progress != value)
                {
                    m_progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }

        /// <summary>
        /// If ExtractAll() has been called on one thread, this method can be called on another
        /// thread to request cancellation.</summary>
        public void CancelAsync()
        {
            m_cancelRequested = true;
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #endregion

        /// <summary>
        /// Extracts localizable strings from the project files in the given root directory and
        /// outputs the strings to the given XML file.</summary>
        /// <param name="sourceList">a list of paths in which to search for csproj and sln files</param>
        /// <param name="ignoreList">a list of paths to be ignored</param>
        /// <param name="xmlFile">the fully qualified path of the XML file that is to be created and
        /// populated with localizable string data. If this file exists already, it will be deleted.</param>
        public void ExtractLocalizableStrings(IEnumerable<string> sourceList, string xmlFile, IEnumerable<string> ignoreList)
        {
            m_log.AppendLine("=================== STARTING ==================");

            m_log.AppendLine("Reading existing translations (if any): " + xmlFile);
            var preservedTranslations = new HashSet<LocalizableString>(
                ReadLocalizationFile(xmlFile).Where(a => a.Translation != a.Text));

            var stringData = new HashSet<LocalizableString>();

            foreach (var source in sourceList)
            {
                var sourceRoot = Path.GetFullPath(source);

                m_log.AppendLine("Parsing: " + sourceRoot);
                int numCs = 0;
                int numXaml = 0;

                var filePathList = new List<string>();

                var notIgnored = new Func<string, bool>(f => !ignoreList.Any(i => f.EndsWith(i)));

                // Parse solution files.
                var slnFiles = new List<string>();
                if (sourceRoot.EndsWith(".sln"))
                    slnFiles.Add(sourceRoot);
                else if (Directory.Exists(sourceRoot))
                    slnFiles.AddRange(Directory.GetFiles(sourceRoot, "*.sln", SearchOption.TopDirectoryOnly).Where(f => notIgnored(f)));

                foreach (var slnFile in slnFiles)
                {
                    m_log.AppendLine("Adding files included from projects in '" + slnFile + "'.");
                    foreach (var projFile in MsBuildUtils.MsBuildFile.GetProjects(slnFile).Select(p => p.FileName).Where(f => notIgnored(f)))
                    {
                        m_log.AppendLine("Adding files included from '" + projFile + "'.");
                        filePathList.AddRange(MsBuildUtils.MsBuildFile.GetProjectFilenames(projFile).Where(f => notIgnored(f)));
                    }
                }

                // Parse project files.
                var projFiles = new List<string>();
                if (sourceRoot.EndsWith(".csproj"))
                    projFiles.Add(sourceRoot);
                else if (Directory.Exists(sourceRoot))
                    projFiles.AddRange(Directory.GetFiles(sourceRoot, "*.csproj", SearchOption.TopDirectoryOnly).Where(f => notIgnored(f)));
                foreach (var projFile in projFiles)
                {
                    m_log.AppendLine("Adding files included from '" + projFile + "'.");
                    filePathList.AddRange(MsBuildUtils.MsBuildFile.GetProjectFilenames(projFile));
                }

                // If no solution file or project file was found, then look at all files in the directory, recursively.
                if (filePathList.Count == 0)
                    filePathList.AddRange(Directory.GetFiles(sourceRoot, "*.*", SearchOption.AllDirectories).Where(f => notIgnored(f)));

                // Parse all of the code files, looking for localizable strings.
                var filePaths = filePathList.ToArray();
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
                    if (m_directories != null)
                        Progress += 1.0 / (m_directories.Count * filePaths.Length);
                    if (m_cancelRequested)
                    {
                        m_log.AppendLine("======== CANCELLED (Nothing was written to this XML file.) ========");
                        return;
                    }
                }

                m_log.AppendLine("Parsed " + numCs + " *.cs and " + numXaml + " *.xaml files.");
                m_log.AppendLine("Unique strings to localize: " + stringData.Count);

                // Merge in preserved translations.
                // Report prior translations whose original text is no longer in the source code.
                bool foundOrphanedTranslation = false;
                foreach (var translation in preservedTranslations)
                {
                    if (stringData.Contains(translation))
                    {
                        // Remove the item that does not have a Translation.
                        stringData.Remove(translation);
                    }
                    else
                    {
                        if (!foundOrphanedTranslation)
                        {
                            foundOrphanedTranslation = true;
                            m_log.AppendLine(
                                "---Warning: the following text strings were translated in the original XML file," +
                                "but are no longer in the source code:");
                        }
                        m_log.AppendLine(translation.Text);
                    }

                    // Add this item, which does have a Translation.
                    stringData.Add(translation);
                }
                if (foundOrphanedTranslation)
                    m_log.AppendLine("---End of Warning");
            }

            m_log.AppendLine("Writing: " + xmlFile);

            // Sort in alphabetical order (ignoring case), to make finding strings easier for people.
            // This will also make the order more stable, as code gets moved around.
            var finalStringData = new List<LocalizableString>(stringData);
            finalStringData.Sort((a, b) =>
            {
                // First sort by Text and then Context.
                int result = String.Compare(a.Text, b.Text, StringComparison.CurrentCultureIgnoreCase);
                if (result == 0)
                    result = String.Compare(a.Context, b.Context, StringComparison.CurrentCultureIgnoreCase);
                return result;
            });

            WriteLocalizationFile(xmlFile, finalStringData);

            m_log.AppendLine("=================== FINISHED ==================");
        }

        /// <summary>
        /// Extracts localizable strings from the given XAML file.</summary>
        /// <param name="filePath">The file path</param>
        /// <returns></returns>
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

        /// <summary>
        /// Extracts localizable strings from the given C# file</summary>
        /// <param name="filePath">The file path</param>
        /// <returns></returns>
        public IEnumerable<LocalizableString> ExtractFromCs(string filePath)
        {
            string file = File.ReadAllText(filePath);
            var results = new List<LocalizableString>();
            ExtractFromCsInternal(file, results);
            return results;
        }

        protected virtual void ExtractFromCsInternal(string file, List<LocalizableString> results)
        {
            results.AddRange(ExtractMethodFromCs(file, "Localize", LocalizeMethodHandler));
            results.AddRange(ExtractMethodFromCs(file, "LocalizedDescription", LocalizedDescriptionMethodHandler));
            results.AddRange(ExtractMethodFromCs(file, "Property", PropertyMethodHandler));
            results.AddRange(ExtractMethodFromCs(file, "LocalizedName", LocalizedNameConstructorHandler));
        }

        /// <summary>
        /// Extracts localizable strings from the given C# file</summary>
        /// <param name="file">The contents of a C# file</param>
        /// <param name="methodName">The name of the C# method that takes one or two parameters
        /// that are string literals. The first parameter is the string literal to be translated.
        /// The second parameter, if present, will be the context string.</param>
        /// <param name="handler">The delegate that handles every found matched method call</param>
        /// <returns></returns>
        public IEnumerable<LocalizableString> ExtractMethodFromCs(string file, string methodName, FoundMethodHandler handler)
        {
            int startSearch = 0;
            int localizeStart;
            while ((localizeStart = file.IndexOf(methodName, startSearch, StringComparison.Ordinal)) >= 0)
            {
                IList<string> paramStrings;
                if (ParseMethodCallForStringLiterals(file, localizeStart, methodName, out paramStrings))
                {
                    if (handler != null)
                    {
                        foreach (var localizedString in handler.Invoke(paramStrings))
                            yield return localizedString;
                    }
                }
                startSearch = localizeStart + methodName.Length;
            }
        }

        /// <summary>
        /// Delegate for methods returning an enumerable of LocalizableStrings, given a list of string literals</summary>
        /// <param name="paramStrings">A list of string literals, parsed from the parameters of a method call.  
        /// Position of string literals in list match their parameter position in method call.</param>
        /// <returns>An enumeration of LocalizableString instances, generated from select items in paramStrings list</returns>
        public delegate IEnumerable<LocalizableString> FoundMethodHandler(IList<string> paramStrings);

        /// <summary>
        /// Parses the method call with one or two string literal parameters</summary>
        /// <param name="s">The string to search</param>
        /// <param name="methodNameIndex">Index of the beginning of the method name</param>
        /// <param name="methodName">Name of the method</param>
        /// <param name="paramStrings">The parameter strings</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The string must begin with the given method name at the given index</exception>
        public static bool ParseMethodCallForStringLiterals(string s, int methodNameIndex, string methodName, out IList<string> paramStrings)
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
            var paramIndex = afterNameIndex + 1;

            // Get string literals defined in method parameters
            var stopIndex = s.Length;
            int afterParseIndex;
            while (TryParseMethodParams(s, paramIndex, stopIndex, out literal, out afterParseIndex))
            {
                paramStrings.Add(literal);
                paramIndex = afterParseIndex;
                paramIndex = SkipWhiteSpace(s, paramIndex);
                if (paramIndex >= stopIndex || s[afterParseIndex-1] != ',')
                    break;
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
        /// <returns><c>True</c> if a string literal was found.</returns>
        public static bool TryParseStringLiteralFromEnd(string s, int iLastChar, out string literal)
        {
            literal = string.Empty;
            var iFirstChar = iLastChar;

            if (iFirstChar >= s.Length || s[iFirstChar] != '"')
                return false;

            // Parsing escape sequences going backwards is difficult. Let's find the beginning
            //  of all the concatenated strings and then parse going forward.
            while (--iFirstChar >= 0)
            {
                char c = s[iFirstChar];
                // Look for the beginning of the string literal, but check for an escape character.
                // In a normal string literal, '\' is the escape character.
                // In an verbatim string literal, a double-quote is the escape character.
                char prevC = iFirstChar > 0 ? s[iFirstChar - 1] : ' ';
                if (c == '"')
                {
                    if (prevC != '\\' && prevC != '"')
                    {
                        // We hit an unescaped double-quote. End the string literal for now.
                        // Check for the verbatim string literal symbol '@'.
                        if (prevC == '@')
                            iFirstChar--;
                        break;
                    }
                    // Skip the escape character.
                    iFirstChar--;
                }
            }

            // Now go forwards.
            int afterEnd;
            return TryParseForStringLiteral(s, iFirstChar, iLastChar + 1, out literal, out afterEnd);
        }

        /// <summary>
        /// Attempt to extract a string literal value from the next parameter in a method call</summary>
        /// <param name="s">The string containing the method call</param>
        /// <param name="iFirstChar">Index of the beginning of the next method parameter</param>
        /// <param name="stopIndex">Index at which parsing should stop, presumably the end of the method call</param>
        /// <param name="outLiteral">The string literal parsed from the next method parameter. An empty string is returned if parameter was not a string literal</param>
        /// <param name="outAfterEnd">The index one character after the end of the parsed parameter</param>
        /// <returns><c>True</c> if a method parameter was parsed (regardless of whether it was a string literal or not), otherwise false</returns>
        public static bool TryParseMethodParams(string s, int iFirstChar, int stopIndex, out string outLiteral, out int outAfterEnd)
        {
            outLiteral = "";
            outAfterEnd = iFirstChar;

            var sb = new StringBuilder();
            var terminated = false;
            while(outAfterEnd < s.Length)
            {
                var matchChars = new[] { ',', '(', ')', '@', '"', ';' };
                var nextMatchChar = s.IndexOfAny(matchChars, outAfterEnd, stopIndex - outAfterEnd);

                // trivial rejection: past string range
                if (nextMatchChar < iFirstChar)
                    return false;

                // trivial rejection: first char is the end of a method call (close paren)
                var c = s[nextMatchChar];

                switch (c)
                {
                    // end of method parameter - we're done here
                    case ',':
                    case ')':
                        outAfterEnd = nextMatchChar + 1;
                        terminated = true;
                        break;

                    // illegal terminating character - fail
                    case ';':
                        return false;

                    // start of string literal - parse past it, append it to result, and continue
                    case '@':
                    case '"':
                    {
                        if (!TryParseForStringLiteral(s, nextMatchChar, stopIndex, out outLiteral, out outAfterEnd))
                            return false;
                        sb.Append(outLiteral);
                        break;
                    }

                    // start of parenthesized expression - parse past it, and continue
                    case '(':
                        var start = nextMatchChar + 1;
                        outAfterEnd = GetMatchingCloseParen(s, start);
                        if (outAfterEnd < start)
                            return false;
                        outAfterEnd++;
                        break;
                }

                if (terminated)
                    break;
            }

            outLiteral = sb.ToString();
            return (outAfterEnd - iFirstChar) > 1 && terminated;
        }

        /// <summary>
        /// Given the specified point in a string, retrieve the index of the next top-level closing parentheses</summary>
        /// <param name="s">The string in which to search</param>
        /// <param name="iFirstChar">The the start of the search range</param>
        /// <returns>The index of the next top-level closing parentheses within the specified range of the string, of -1 if if none found</returns>
        public static int GetMatchingCloseParen(string s, int iFirstChar)
        {
            var nextCloseParen = s.IndexOf(')', iFirstChar);
            var nextOpenParen = s.IndexOf('(', iFirstChar);
            while (nextOpenParen >= iFirstChar && nextOpenParen < nextCloseParen)
            {
                var pastSubParens = GetMatchingCloseParen(s, nextOpenParen + 1) + 1;
                if (pastSubParens < iFirstChar)
                    return pastSubParens;

                nextCloseParen = s.IndexOf(')', pastSubParens);
                nextOpenParen = s.IndexOf('(', pastSubParens);
            }

            return nextCloseParen;
        }

        /// <summary>
        /// Parses the given string starting at a '"', '@', or whitespace, looking for a C# string
        /// literal or concatenation of C# string literals separated by '+'. Outputs the resulting
        /// unescaped string. For example, the text of these four characters as they might appear
        /// in a *.cs file, "\n", will result in a string with just a single newline character.
        /// Does not handle comments.</summary>
        /// <param name="s">The string to search</param>
        /// <param name="iFirstChar">The zero-based index to begin searching for the opening "
        /// or the @" (if it's a verbatim string literal). Initial whitespace will be skipped.</param>
        /// <param name="iStopChar">Index into the string before which parsing should stop</param>
        /// <param name="literal">The resulting string literal, if successful.</param>
        /// <param name="afterEnd">The index after the last closing double-quote.</param>
        /// <returns><c>True</c> if a string literal was found.</returns>
        public static bool TryParseForStringLiteral(string s, int iFirstChar, int iStopChar, out string literal, out int afterEnd)
        {
            literal = "";
            afterEnd = iFirstChar;

            // trivial rejection
            if (string.IsNullOrEmpty(s) || iFirstChar >= s.Length)
                return false;

            var match = StringLiteralRegex.Match(s, iFirstChar, iStopChar - iFirstChar);
            var validMatchCount = 0;
            var sb = new StringBuilder();

            // In order to parse a sequence of added strings as one string literal, 
            // iteratively run the regex on the input.  Stop when either no match is found, 
            // or we match past the end of the current string literal.
            while (match.Success)
            {
                // Regex defines two capturing groups, one in the 'explicit string literal' pattern, 
                // and one in the 'regular string literal' pattern.  So match result should have 3 groups.
                if (match.Groups.Count != 3)
                    throw new Exception("Expecting the regex to have specified 2 groups, not " + match.Groups.Count);

                // Figure out which pattern actually matched
                bool  explicitStringLiteral;
                Group matchingGroup;
                if (match.Groups[1].Success)
                {
                    matchingGroup = match.Groups[1];
                    explicitStringLiteral = true;
                }
                else if (match.Groups[2].Success)
                {
                    matchingGroup = match.Groups[2];
                    explicitStringLiteral = false;
                }
                else // Something went really wrong if neither group matched
                    throw new Exception("Did not match for an explicit string literal, nor a regular string literal");

                // If a terminating character is found before the current match, the match is not part of current string literal
                var nextTerm = GetNextTerm(s, afterEnd);
                if (nextTerm >= afterEnd && nextTerm < matchingGroup.Index)
                    break;

                // Current match is the next item in the current string literal.
                // If it's a 'regular' string literal, convert doubly-escaped characters
                validMatchCount++;
                var matchStr = matchingGroup.Value;
                if (matchStr.Length > 0)
                    Unescape(ref matchStr, explicitStringLiteral);

                // add to string literal parsed so far
                sb.Append(matchStr);

                // skip to end of matched string, and evaluate next found match
                afterEnd = match.Index + match.Length;
                match = match.NextMatch();
            }

            literal = sb.ToString();
            return validMatchCount > 0;
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
        /// <param name="outputFile">Name and path of the XML file to be created</param>
        /// <param name="stringData">Localizable string data to write to the XML file. Duplicates
        /// should have been removed and the desired sorting should have been decided.</param>
        public void WriteLocalizationFile(string outputFile, IEnumerable<LocalizableString> stringData)
        {
            using (var stream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                WriteLocalizationFile(stream, stringData);
        }

        /// <summary>
        /// Creates an XML file with the given name and writes the given string data to that file.</summary>
        /// <param name="stream">Writable stream of the target Localization.xml file</param>
        /// <param name="stringData">Localizable string data to write to the XML file. Duplicates
        /// should have been removed and the desired sorting should have been decided.</param>
        public void WriteLocalizationFile(Stream stream, IEnumerable<LocalizableString> stringData)
        {
            // Prepare new document in memory.
            var xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", System.Text.Encoding.UTF8.WebName, "yes"));

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
                stringElement.SetAttribute("translation", stringItem.Translation);
            }

            // Write out the document.
            using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                xmlDoc.WriteTo(writer);
                writer.Flush();
            }
        }

        /// <summary>
        /// Opens an XML file with the given name and reads its localization content.</summary>
        /// <param name="inputFile">XML filename to be read</param>
        /// <returns>The set of localizable strings, contexts (if any) and translations (if any).</returns>
        public IList<LocalizableString> ReadLocalizationFile(string inputFile)
        {
            if (File.Exists(inputFile))
            {
                using (var stream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                    return ReadLocalizationFile(stream);
            }
            return new List<LocalizableString>();
        }

        /// <summary>
        /// Reads an XML file stream and parses its localization content.</summary>
        /// <param name="stream">the stream of the XML file to be read</param>
        /// <returns>The set of localizable strings, contexts (if any) and translations (if any).</returns>
        public static IList<LocalizableString> ReadLocalizationFile(Stream stream)
        {
            var result = new List<LocalizableString>();
            var existingFile = new XmlDocument();
            using (var reader = new XmlTextReader(stream))
            {
                reader.Namespaces = false;
                existingFile.Load(reader);

                if (existingFile.DocumentElement == null)
                    return result;

                XmlNodeList declaredStrings = existingFile.DocumentElement.SelectNodes("StringItem");
                if (declaredStrings == null || declaredStrings.Count == 0)
                    return result;

                foreach (var item in declaredStrings.Cast<XmlNode>())
                {
                    if (item.Attributes == null)
                        continue;

                    XmlAttribute[] attributes = item.Attributes.Cast<XmlAttribute>().ToArray();

                    // The "id" is the original text and is required.
                    XmlAttribute attribute = attributes.FirstOrDefault(a => a.Name == "id");
                    if (attribute != null)
                    {
                        string text = attribute.Value;

                        attribute = attributes.FirstOrDefault(a => a.Name == "context");
                        string context = attribute != null ? attribute.Value : "";

                        attribute = attributes.FirstOrDefault(a => a.Name == "translation");
                        var translation = attribute != null ? attribute.Value : "";

                        result.Add(new LocalizableString(text, context, translation));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// The list of fully qualified paths of source files and ignored files, in order to produce
        /// the target Localization.xml file.</summary>
        public class TargetRule
        {
            public TargetRule(List<string> sourceList, string target, List<string> ignoreList )
            {
                SourceList = sourceList;
                Target = target;
                IgnoreList = ignoreList;
            }
            public readonly List<string> SourceList;
            public readonly string Target;
            public readonly List<string> IgnoreList;
        }

        /// <summary>
        /// Gets the log from the last run of ExtractAll()</summary>
        public string Log
        {
            get { return m_log.ToString(); }
        }

        /// <summary>
        /// Opens the settings file in the user's default text editor</summary>
        public void OpenSettingsFile()
        {
            OpenSettingsFile(GetSettingsPath());
        }

        /// <summary>
        /// Opens the settings file in the user's default text editor</summary>
        /// <param name="settingsPath"></param>
        public void OpenSettingsFile(string settingsPath)
        {
            if (File.Exists(settingsPath))
                Process.Start(settingsPath);
        }

        /// <summary>
        /// StringLiteralRegex is a singleton copy of the Regex instance used to match string literals 
        /// in TryParseForStringLiteral().  The instance could be created locally (and more simply) in 
        /// the calling method.  However I thought its complexity warranted some explanation, hence 
        /// its elaborate construction below.  Keeping it this way for now, in case it needs to be 
        /// modified further. -pskibitzke</summary>
        private static Regex StringLiteralRegex
        {
            get
            {
                if (s_stringLiteralRegex != null)
                    return s_stringLiteralRegex;

                // regex syntax for character matching patterns
                const string kRxQuote = "\\\"";
                const string kRxVerbatimQuote = "@" + kRxQuote;
                const string kRxBackslash = "\\\\";
                const string kRxPlus = "\\+";
                const string kRxAnyChar = ".";
                const string kRxAnySpaceOrNewline = "[\\s\\r\\n]*";
                const string kBackslashAny = kRxBackslash + kRxAnyChar;

                // regex syntax for group and set patterns
                var rxIgnoreCharSet = new Func<string, string>(rxPattern => "[^" + rxPattern + "]");
                var rxCaptureGroup = new Func<string, string>(rxPattern => "(" + rxPattern + ")");
                var rxNonCaptureGroup = new Func<string, string>(rxPattern => "(?:" + rxPattern + ")");
                var rxZeroOrMore = new Func<string, string>(rxPattern => rxPattern + "*");
                var rxZeroOrOne = new Func<string, string>(rxPattern => rxPattern + "?");

                // create regex pattern: 
                // match for a double-quote-delimited string, containing zero or more characters of any type, including escaped quotes

                var anyNonEscapedNorQuote = rxIgnoreCharSet(kRxBackslash + kRxQuote);
                var allNonEscapedNorQuote = rxZeroOrMore(anyNonEscapedNorQuote);

                var anyNonQuote = rxIgnoreCharSet(kRxQuote);
                var allNonQuote = rxZeroOrMore(anyNonQuote);

                var anyNonQuoteNorEscaped = rxIgnoreCharSet(kRxQuote + kRxBackslash);
                var allNonQuoteNorEscaped = rxZeroOrMore(anyNonQuoteNorEscaped);

                var escapedSection = rxNonCaptureGroup(kBackslashAny + allNonQuoteNorEscaped);
                var allBackslashedSections = rxZeroOrMore(escapedSection);

                var doubleQuotedSection = rxNonCaptureGroup(kRxQuote + kRxQuote + allNonQuote);
                var allDoubleQuotedSections = rxZeroOrMore(doubleQuotedSection);

                var stringLiteral = kRxQuote + rxCaptureGroup(allNonEscapedNorQuote + allBackslashedSections) + kRxQuote;
                var verbatimStringLiteral = kRxVerbatimQuote + rxCaptureGroup(allNonQuote + allDoubleQuotedSections) + kRxQuote;

                var plus = rxNonCaptureGroup(kRxAnySpaceOrNewline + kRxPlus + kRxAnySpaceOrNewline);
                var optionalPlus = rxZeroOrOne(plus);

                var pattern = rxNonCaptureGroup(verbatimStringLiteral + "|" + stringLiteral) + optionalPlus;

                s_stringLiteralRegex = new Regex(pattern);

                return s_stringLiteralRegex;
            }
        }
        private static Regex s_stringLiteralRegex;

        /// <summary>
        /// Convert a string, containing a C# code representation of a string literal, into
        /// the character string resulting from compiling that code.</summary>
        /// <param name="str">The C# code representation of a string literal</param>
        /// <param name="explicitStringLiteral">Whether or not str contains a normal or explicit string literal</param>
        private static void Unescape(ref string str, bool explicitStringLiteral)
        {
            // Explicit string literals have no reserved (read:escaped) characters,  
            // except for double-quote (ie, "), which terminates the string literal.
            // Two double quotes specify that a single double quote character is part of
            // the string, and not the terminator of it.
            if (explicitStringLiteral)
            {
                str = str.Replace("\"\"", "\"");
                return;
            }

            var hexToStringChar = new Func<string, string>
            (
                hexStr => ((char)Int32.Parse(hexStr, NumberStyles.HexNumber)).ToString()
            );

            // Normal string literals can contain several types of reserved characters,
            // which for the most part can be converted using System.Text.RegularExpressions.Regex.Unescape().
            // However, it does have (documented) problems with escaped hex numbers, 
            // whose syntax permits a variable number of characters for the value,  
            // introducing ambiguity in how they should be parsed for conversion. It seems 
            // to also have a problem with 16-bit and 32-bit escaped unicode values as well
            // 
            // As a result, before calling Unescape(), we manually convert escaped hex and unicode values.

            const string kHexPattern    = "\\\\x([0-9A-Fa-f]{1,4})";
            const string kUni16Pattern  = "\\\\u([0-9A-Fa-f]{4})";
            const string kUni32Pattern  = "\\\\U([0-9A-Fa-f]{8})";

            str = Regex.Replace(str, kHexPattern, m => (hexToStringChar(m.Value.Substring(2))));
            str = Regex.Replace(str, kUni16Pattern, m => (hexToStringChar(m.Value.Substring(2))));
            str = Regex.Replace(str, kUni32Pattern, m => (hexToStringChar(m.Value.Substring(2))));

            // unescape everything else
            str = Regex.Unescape(str);
        }

        private static readonly char[] s_termChars = { ',', '(', ')' };

        private static int GetNextTerm(string s, int start)
        {
            return s.IndexOfAny(s_termChars, start);
        }

        private static string GetSettingsPath()
        {
            return "DirectoriesToLocalize.txt";
        }

        /// <summary>
        /// Parses the settings file which contains the paths of the
        /// directories that should be examined for localizable strings.</summary>
        /// <param name="settingsPath">Path and filename of the settings file to load</param>
        private void ReadSettings(string settingsPath)
        {
            string[] lines = File.ReadAllLines(settingsPath);
            m_directories = new List<TargetRule>();

            int lineNum = 0;
            foreach (string line in lines)
            {
                lineNum++;;
                if (line.Length == 0 || line[0] == '#')
                    continue;

                //break these:
                //  Framework\Atf.Perforce Framework\Atf.Perforce\Resources\Localization.xml
                //  "Samples\CircuitEditor" Samples\CircuitEditor\Resources\Localization.xml
                //into the separate source and target paths and combine with the ATF root directory.

                // "[^"]+"|[^ ]+ matches anything in quotes or anything without spaces.
                MatchCollection paths = Regex.Matches(line, "\"[^\"]+\"|[^ ]+");
                if (paths.Count < 2)
                    throw new InvalidOperationException(string.Format("bad settings line:{0}", line));

                var foundXml = false;
                var sourceList = new List<string>();
                var target = "";
                var ignoreList = new List<string>();

                for (var i = 0; i < paths.Count; i++)
                {
                    var path = paths[i].ToString().Trim('\"');
                    var isXml = path.EndsWith(".xml");
                    var isIgnoreTag = path == "-ignore";

                    // must have at least one source file specified
                    if (isXml && (i==0 || foundXml))
                        throw new ArgumentException("File '" + settingsPath + "' Line " + lineNum + ": At least one source file or path must be listed before specifying one (and only one) target xml");

                    // must specify target xml before ignore list
                    if (isIgnoreTag && !foundXml)
                        throw new ArgumentException("File '" + settingsPath + "' Line " + lineNum + ": Must specify one (and only one) target xml before ignored items are listed");

                    // Looking for source files, and target xml
                    if (!foundXml)
                    {
                        if (isXml)
                        {
                            target = path;
                            foundXml = true;
                        }
                        else
                            sourceList.Add(path);
                    }
                    else // Looking for ignore files
                    {
                        if (!isIgnoreTag)
                            ignoreList.Add(path);
                    }
                }

                if (string.IsNullOrEmpty(target))
                    throw new ArgumentException("File '" + settingsPath + ": no target xml was listed");

                m_directories.Add(new TargetRule(sourceList, target, ignoreList));
            }
        }

        // Extracts calls like "Localizable string".Localize() and Localizer.Localize("Localizable string")
        private IEnumerable<LocalizableString> LocalizeMethodHandler(IList<string> paramStrings)
        {
            if (paramStrings.Count == 1 && !string.IsNullOrEmpty(paramStrings[0]))
                yield return new LocalizableString(paramStrings[0], string.Empty);
            else if (paramStrings.Count == 2 && !string.IsNullOrEmpty(paramStrings[0]) && !string.IsNullOrEmpty(paramStrings[1]))
                yield return new LocalizableString(paramStrings[0], paramStrings[1]);
        }

        private IEnumerable<LocalizableString> LocalizedDescriptionMethodHandler(IList<string> paramStrings)
        {
            if (paramStrings.Count == 1 && !string.IsNullOrEmpty(paramStrings[0]))
                yield return new LocalizableString(paramStrings[0], string.Empty);
        }

        private IEnumerable<LocalizableString> LocalizedNameConstructorHandler(IList<string> paramStrings)
        {
            if (paramStrings.Count == 1 && !string.IsNullOrEmpty(paramStrings[0]))
                yield return new LocalizableString(paramStrings[0], string.Empty);
        }

        private IEnumerable<LocalizableString> PropertyMethodHandler(IList<string> paramStrings)
        {
            if (paramStrings.Count == 1 && !string.IsNullOrEmpty(paramStrings[0]))
                yield return new LocalizableString(paramStrings[0], string.Empty);
            if (paramStrings.Count == 4)
            {
                if (!string.IsNullOrEmpty(paramStrings[1]))
                    yield return new LocalizableString(paramStrings[1], string.Empty);
                if (!string.IsNullOrEmpty(paramStrings[3]))
                    yield return new LocalizableString(paramStrings[3], string.Empty);
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

        private double m_progress;
        private bool m_cancelRequested;
        private List<TargetRule> m_directories;
        private readonly StringBuilder m_log = new StringBuilder();
    }
}
