//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;
using System.IO;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Obj
{
    /// <summary>
    /// Material file</summary>
    public class MtlFile
    {
        /// <summary>
        /// Gets materials definition dictionary</summary>
        public Dictionary<string, MaterialDef> Materials
        {
            get { return m_materials; }
        }

        /// <summary>
        /// Gets material file name</summary>
        public string Name { get; set; }

        /// <summary>
        /// Reads material file</summary>
        /// <param name="filename">Material file pathname</param>
        public void Read(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");

            StreamReader reader = null;

            if (!File.Exists(filename))
                return;

            try
            {
                // Create an instance of StreamReader to read from a file.
                reader = new StreamReader(filename);

                string buf;
                bool isValid = false;
                MaterialDef currentMtl = null;

                while ((buf = reader.ReadLine()) != null)
                {
                    buf = buf.Trim();
                    if (string.IsNullOrEmpty(buf)) continue;
                    if (buf[0] == '#') continue;

                    string[] split = buf.Split(s_stringDelimiters, StringSplitOptions.RemoveEmptyEntries);
                    switch (buf[0])
                    {
                        case 'n': // newmtl name
                            if (split.Length >= 2)
                            {
                                string newMtl = split[1];

                                // Check for duplicate material
                                if (!Materials.ContainsKey(newMtl))
                                {
                                    // Create a new material
                                    currentMtl = new MaterialDef(newMtl);
                                    Materials.Add(newMtl, currentMtl);
                                    isValid = true;
                                }
                                else
                                    isValid = false;
                            }
                            break;

                        case 'K': // k* r g b
                            if (isValid && split.Length == 4)
                                switch (buf[1])
                                {
                                    case 'a': // ambient
                                        currentMtl.Ambient = new Vec4F(
                                            float.Parse(split[1]),
                                            float.Parse(split[2]),
                                            float.Parse(split[3]),
                                            1.0f);
                                        break;
                                    case 'd': // diffuse
                                        currentMtl.Diffuse = new Vec4F(
                                            float.Parse(split[1]),
                                            float.Parse(split[2]),
                                            float.Parse(split[3]),
                                            1.0f);
                                        break;
                                    case 's': // specular
                                        currentMtl.Specular = new Vec4F(
                                            float.Parse(split[1]),
                                            float.Parse(split[2]),
                                            float.Parse(split[3]),
                                            1.0f);
                                        break;
                                }
                            else
                                throw new Exception(string.Format("Error parsing k{0} in file: {1}", buf[1], filename));
                            break;

                        case 'N': // Ns shininess
                            if (isValid && split.Length == 2 && buf[1] == 's')
                                currentMtl.Shininess = float.Parse(split[1]);
                            break;

                        case 'm': // map_Kd texture
                            if (isValid && split.Length == 2 && split[0].Equals("map_Kd"))
                                currentMtl.TextureName = split[1];
                            break;

                        case 'd': // d alpha
                        case 'T': // Tf r g b or Tr alpha
                            if (isValid && split.Length <= 4 && (buf[0] == 'd' || buf[1] == 'f' || buf[1] == 'r'))
                                currentMtl.Alpha = float.Parse(split[1]);
                            break;

                        default:
                            break;
                    }
                }
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        private readonly Dictionary<string, MaterialDef> m_materials = new Dictionary<string, MaterialDef>();
        private static readonly char[] s_stringDelimiters = new[] { ' ', '\t' };
    }
}
