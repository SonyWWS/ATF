//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Xml.Schema;
using System.Xml;

namespace DomGen
{
    static class Program
    {
        // usage: DomGen {schemaPath} {outputPath} {schemaNamespace} {classNamespace}
        [STAThread]
        static void Main(string[] args)
        {
            string inputPath;
            string outputPath;
            string schemaNamespace;
            string codeNamespace;
            string className;
            bool useCacheFile = false;
            bool upToDate = false;
            
            if (args.Length < 4)
            {
                Console.WriteLine(
                    "usage:\n" +
                    "DomGen {schemaPath} {outputPath} {schemaNamespace}  {classNamespace} {options}\n" +
                    "eg:      DomGen echo.xsd Schema.cs http://sce/audio Sce.Audio -a\n" +
                    "options:\n" +
                    "-a or -adapters\n" +
                    "   Will generate DomNodeAdapters with properties\n" +
                    "-annotatedOnly\n" +
                    "   If the -annotatedOnly argument is set: types will be included ONLY if\n" +
                    "   <sce.domgen include=\"true\"> is defined for this type, all other types will\n" +
                    "   be skipped.\n" +
                    "   If -annotatedOnly is NOT set: types will be included by default UNLESS\n" +
                    "   <sce.domgen include=\"false\"> is explicitly set for this type" +
                    "-enums\n" +
                    "   Will generate Enum types for all AttributeTypes which have string value restrictions\n" +
                    "-cache\n" +
                    "   If -cache is set will generate an intermediate file and will not rebuild the output unless\n" +
                    "   the schema has changed\n");
                return;
            }

            inputPath = args[0];
            outputPath = args[1];
            schemaNamespace = args[2];
            codeNamespace = args[3];
            // class name should always be the same as output file name
            className = Path.GetFileNameWithoutExtension(outputPath);

            for (int i = 4; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "-cache")
                    useCacheFile = true;
            }

            var typeLoader = new SchemaLoader();
            XmlSchema schema = null;
            string cacheFile = outputPath + @".dep";

            // Temp: Test to see if not rebuilding speeds up our builds...
            if (useCacheFile && File.Exists(cacheFile))
            {
                // Use special resolver
                var resolver = new HashingXmlUrlResolver();
                typeLoader.SchemaResolver = resolver;
                schema = typeLoader.Load(inputPath);

                if (schema != null)
                {
                    try
                    {
                        string previousHashString = null;

                        using (TextReader reader = File.OpenText(cacheFile))
                        {
                            previousHashString = reader.ReadLine();
                        }

                        // Generate hash by concat of hash of each file stream loaded
                        var sb = new StringBuilder();
                        foreach (byte[] hash in resolver.Hashes)
                        {
                            sb.Append(Convert.ToBase64String(hash));
                        }

                        string hashString = sb.ToString();

                        upToDate = (previousHashString == hashString);

                        if (upToDate == false)
                        {
                            using (TextWriter writer = new StreamWriter(cacheFile))
                            {
                                writer.WriteLine(hashString);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else
            {
                schema = typeLoader.Load(inputPath);
            }

            if (upToDate == false)
            {
                UTF8Encoding encoding = new UTF8Encoding();
                using (FileStream strm = File.Open(outputPath, FileMode.Create))
                {
                    string s = SchemaGen.Generate(typeLoader, schemaNamespace, codeNamespace, className, args);
                    byte[] bytes = encoding.GetBytes(s);
                    strm.Write(bytes, 0, bytes.Length);
                }
            }
        }

    }

    /// <summary>
    /// resolver which takes a hash of all streams which are resolved
    /// </summary>
    class HashingXmlUrlResolver : XmlUrlResolver
    {
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            object entity = base.GetEntity(absoluteUri, role, ofObjectToReturn);
            Stream s = entity as Stream;
            if (s!= null)
            {
                long pos = s.Position;
                var md5 = MD5.Create();
                byte[] hash = md5.ComputeHash(s);
                s.Position = pos;
                m_hashes.Add(hash);
            }
            return entity;
        }

        public IEnumerable<byte[]> Hashes { get { return m_hashes; } }

        private readonly List<byte[]> m_hashes = new List<byte[]>();
    }
}
