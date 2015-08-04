//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Xml.Linq;

namespace CircuitEditorSample
{
    /// <summary>
    /// Version migration for xml documents, using LINQ to XML</summary>
    internal class Migrator
    {
        /// <summary>
        /// Gets the base name of the version converter</summary>
        public string BaseName
        {
            get { return m_baseName; }
            set { m_baseName = value; }
        }

        /// <summary>
        /// Gets the namespace of the version converter type</summary>
        public string Namespace
        {
            get { return m_namespace; }
            set { m_namespace = value; }
        }

        internal void Transform(XDocument doc, Version fromVersion, Version toVersion)
        {
            if (fromVersion.Major < toVersion.Major) // upgrades
            {
                for (int i = fromVersion.Major; i < toVersion.Major; ++i)
                {
                    var converterTypeName = string.Format(@"{0}.{1}{2}to{3}", Namespace, BaseName, i, i + 1);
                    // create object from type name
                    Type converterType = Type.GetType(converterTypeName);
                    if (converterType == null)
                        throw new InvalidOperationException("Couldn't find type " + converterTypeName);
                    //TODO review alternative: should we define a new interace like ITransformable , avoid dynamic ?
                    dynamic converter = Activator.CreateInstance(converterType);
                    converter.Transform(doc); // assume converters have Transform() method!                      

                    //// debug
                    //var fileName = string.Format(@"\{0}{1}to{2}.xml", BaseName, i, i + 1);
                    //doc.Save(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + fileName);
                 }
            }
            else if (fromVersion.Major >  toVersion.Major) // downgrades
            {
                //TODO
            }
        }

        private string m_baseName = "CircuitEditor";
        private string m_namespace = "CircuitEditorSample";
    }
}
