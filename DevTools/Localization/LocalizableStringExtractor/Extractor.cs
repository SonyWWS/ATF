using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using Microsoft.Win32;

using PathUtilities = Atf.Utilities.PathUtilities;

namespace LocalizableStringExtractor
{
    /// <summary>
    /// Utility class that uses a settings file and extracts strings that need to be localized from the ATF
    /// assemblies.</summary>
    public class Extractor
    {
        /// <summary>
        /// Extracts all localizable strings whose paths are in the settings file and creates
        /// xml files according to the settings' output filenames.</summary>
        public void ExtractAll()
        {
            s_log.Clear();
            ReadSettings();
            foreach (SourceTargetPair rule in m_assemblies)
            {
                ExtractLocalizableStrings(rule.Source,rule.Target);
            }
            ShowLog();
        }

        public void OpenSettingsFile()
        {
            Process.Start(GetSettingsPath());
        }

        /// <summary>
        /// Extracts localizable strings from the given assembly and outputs them to the given xml file.</summary>
        /// <param name="assembly">the fully qualified path of the Atf-based managed *.exe or *.dll</param>
        /// <param name="xmlFile">the fully qualified path of the xml file that is to be created and
        /// populated with localizable string data. If this file exists already, it will be deleted.</param>
        public static void ExtractLocalizableStrings(string assembly, string xmlFile)
        {
            s_log.AppendLine("=================== STARTING ==================");
            s_log.AppendLine("Disassembling: " + assembly);
            string[] ilLines = Disassemble(assembly);

            if (ilLines.Length == 0)
            {
                s_log.AppendLine("Error: no MSIL lines were found! Bad path?");
                return;
            }

            s_log.AppendLine("Extracting localized strings from: " + assembly);
            IEnumerable<LocalizableString> stringData = ExtractFromIl(ilLines);

            s_log.AppendLine("Writing: " + xmlFile);
            WriteLocalizationFile(xmlFile, stringData);

            s_log.AppendLine("=================== FINISHED ==================");
        }

        /// <summary>
        /// Disassembles the given managed .Net assembly (dll or exe) into an array of strings
        /// of intermediate language (IL) code.</summary>
        /// <param name="assembly">fully qualified path of the assembly file</param>
        /// <returns>an array of strings for each line of the intermediate language version</returns>
        public static string[] Disassemble(string assembly)
        {
            string[] ilLines = new string[0];

            // Determine the temporary directory (in case there are many resource files) and temporary file name.
            string assemblyDir = Path.GetDirectoryName(assembly);
            string tempDir = Path.Combine(assemblyDir, "TempDisassemblerFiles");
            string ilPath = Path.Combine(tempDir, "output.il");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Run the disassembler, create a temporary *.il file, read it in.
                string disassembler = GetDisassemblerPath();
                string arguments = string.Format("\"{0}\" /output=\"{1}\"", assembly, ilPath);
                Process process = Process.Start(disassembler, arguments);
                process.WaitForExit();
                ilLines = File.ReadAllLines(ilPath);
            }
            finally
            {
                PathUtilities.DeleteDirectory(tempDir);
            }

            return ilLines;
        }

        /// <summary>
        /// Extracts all of the localizable strings from the Microsoft intermediate language text.</summary>
        /// <param name="ilLines"></param>
        /// <returns></returns>
        public static IEnumerable<LocalizableString> ExtractFromIl(string[] ilLines)
        {
            for (int i = 0; i < ilLines.Length; i++)
            {
                // This is the overload that takes just one string, the string to be localized.
                // In MSIL:
                //    IL_0018:  ldstr      "Error"
                //    IL_001d:  call       string Sce.Atf.Localizer::Localize(string)
                if (ilLines[i].Contains("Sce.Atf.Localizer::Localize(string)"))
                {
                    int beginningLineOfQuote;
                    string text = GetStringFromIl(ilLines, i - 1, out beginningLineOfQuote);
                    if (text.Length != 0)
                        yield return new LocalizableString(text, string.Empty);
                }

                // This is the overload that takes two string parameters: text and context.
                // In C#:
                //  return "Back".Localize("One of the camera's views of a scene.");
                // In MSIL:
                //  IL_002c:  ldstr      "Back"
                //  IL_0031:  ldstr      "One of the camera's views of a scene."
                //  IL_0036:  call       string [Sce.Atf]Sce.Atf.Localizer::Localize(string,
                //                                                                    string)
                else if (ilLines[i].Contains("Sce.Atf.Localizer::Localize(string,"))
                {
                    int beginningLineOfQuote; //long string literals are broken up over multiple lines of MSIL.
                    string context = GetStringFromIl(ilLines, i - 1, out beginningLineOfQuote);
                    if (context.Length != 0)
                    {
                        string text = GetStringFromIl(ilLines, beginningLineOfQuote - 1, out beginningLineOfQuote);
                        if (text.Length != 0)
                            yield return new LocalizableString(text, context);
                    }
                }
            }
        }

        /// <summary>
        /// Creates an XML file with the given name and writes the given string data to that file.</summary>
        /// <param name="outputFile">the name and path of the XML file to be created</param>
        /// <param name="stringData">the localizable string data to write to the XML file</param>
        public static void WriteLocalizationFile(string outputFile, IEnumerable<LocalizableString> stringData)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0",System.Text.Encoding.UTF8.WebName, "yes"));

            XmlElement root = xmlDoc.CreateElement("StringLocalizationTable");
            xmlDoc.AppendChild(root);

            var sisulizer = new SisulizerWorkaround();

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

            using (XmlTextWriter writer = new XmlTextWriter(outputFile, Encoding.UTF8))
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
                        s_log.AppendLine("Warning: this id will have its context modified" +
                            " to make it unique to work around a Sisulizer bug: " + id);
                    }
                }
            }

            private readonly HashSet<string> m_uniqueContexts = new HashSet<string>();
        }

        /// <summary>
        /// Finds where the Microsoft Intermediate Language Disassembler, ildasm.exe, was installed.</summary>
        /// <returns>the fully qualified path for ildasm.exe</returns>
        public static string GetDisassemblerPath()
        {
            string toolDir = (string)Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Microsoft SDKs\Windows",
                "CurrentInstallFolder",
                null);
            
            string toolFileName = toolDir + @"bin\ildasm.exe";
            
            if (!File.Exists(toolFileName))
                throw new InvalidOperationException(string.Format("Could not find 'ildasm.exe' at {0}", toolFileName));
            
            return toolFileName;
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
        /// Parses one (or more) lines of Microsoft intermediate language text for a single string literal.</summary>
        /// <param name="ilLines">An array of lines of text to parse</param>
        /// <param name="endLineOfQuote">The index in ilLines that contains the end of the string literal. It may contain the
        /// beginning of the string literal, too, but long text strings are broken up over many lines of MSIL.</param>
        /// <param name="startLineOfQuote">The resulting index in ilLines that contains the beginning of the string
        /// literal. If the string literal fit on one line, then this will be the same as 'endLineOfQuote'.</param>
        /// <returns>the string literal or the empty string, if none was found</returns>
        /// <remarks>There are some potentially tricky issues with line breaks, non-ASCII characters,
        /// and reserved characters which need to be escaped. 'bytearray' is not supported at the moment, for example.
        /// http://stackoverflow.com/questions/9113440/where-can-i-find-a-list-of-escaped-characters-in-msil-string-constants
        /// </remarks>
        public static string GetStringFromIl(string[] ilLines, int endLineOfQuote, out int startLineOfQuote)
        {
            startLineOfQuote = endLineOfQuote;
            if (!ilLines[endLineOfQuote].Contains("\""))
            {
                s_log.AppendLine(
                    "Error: This intermediate language (IL) line was expected to contain a string literal.");
                s_log.AppendLine(
                    "Localize() can only ever be used on string literals, and non-ASCII characters currently aren't supported.");
                for (int i = endLineOfQuote - 10; i < endLineOfQuote + 10; i++)
                {
                    if (i >= 0 && i < ilLines.Length)
                    {
                        if (i == endLineOfQuote)
                            s_log.Append("error ==> ");
                        s_log.AppendLine(ilLines[i]);
                    }
                }
                return string.Empty;
            }

            // Back up until we see a "ldstr". If we get an out-of-bounds exception, then the input was bad!
            while (!ilLines[startLineOfQuote].Contains("ldstr"))
                startLineOfQuote--;

            var result = new StringBuilder();
            int currentLineOfQuote = startLineOfQuote;
            while (currentLineOfQuote <= endLineOfQuote)
            {
                int beginQuote = ilLines[currentLineOfQuote].IndexOf('\"');
                int endQuote = ilLines[currentLineOfQuote].LastIndexOf('\"');
                result.Append(ilLines[currentLineOfQuote].Substring(beginQuote + 1, endQuote - beginQuote - 1));
                currentLineOfQuote++;
            }

            // handle MSIL-specific escapes
            result.Replace("\\?", "?");
            result.Replace("\\\"", "\"");
            result.Replace("\\t", "\t");
            result.Replace("\\n", "\n");
            result.Replace("\\r", "\r");

            // to-do: octals, end-of-line-seperators, bytearray

            return result.ToString();
        }

        /// <summary>
        /// A pair of fully qualified paths. The source is the assembly (*.exe or *.dll) path
        /// and the target is the name of the output xml file.</summary>
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

        private static string GetSettingsPath()
        {
            string atfRootDir = GetAtfRoot();
            string settingsPath = Path.Combine(atfRootDir, @"DevTools\Localization\AssembliesToLocalize.txt");
            return settingsPath;
        }

        /// <summary>
        /// Parses the \Localization\AssembliesToLocalize.txt file which contains the paths of the
        /// assemblies that should be examined for localizable strings.</summary>
        /// <returns>the fully qualified paths of the assemblies to look at</returns>
        private void ReadSettings()
        {
            string atfRootDir = GetAtfRoot();
            string[] lines = File.ReadAllLines(GetSettingsPath());
            m_assemblies = new List<SourceTargetPair>();

            foreach (string line in lines)
            {
                if (line.Length == 0 || line[0] == '#')
                    continue;

                //break these:
                //  ..\..\bin\wws_atf\Debug.vs2010\Atf.Core.dll "Framework\Atf.Core\Resources\Sce.Atf.Localization.xml"
                //  "..\..\bin\wws_atf\Debug.vs2010\Atf.Gui.dll" Framework\Atf.Gui\Resources\Sce.Atf.Applications.Localization.xml
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

                m_assemblies.Add(new SourceTargetPair(source, target));
            }
        }

        private static void ShowLog()
        {
            if (s_log.Length == 0)
                return;

            Clipboard.SetText(s_log.ToString());
            MessageBox.Show("A log report was pasted into the clipboard");
        }

        private List<SourceTargetPair> m_assemblies;
        private static readonly StringBuilder s_log = new StringBuilder();
    }
}
