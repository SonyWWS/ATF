//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Sce.Atf.Dom;

namespace DomGen
{
    public static class SchemaGen
    {
        /// <summary>
        /// Generates the Schema CSharp class</summary>
        /// <param name="typeLoader">Type loader with loaded type and annotation information</param>
        /// <param name="schemaNamespace">Target namespace of the schema</param>
        /// <param name="codeNamespace">Namespace of the class ot be generated</param>
        /// <param name="className">Name of the class to be generated</param>
        /// <param name="args">Commandline arguments</param>
        /// <returns>CSharp Schema class as string</returns>
        public static string Generate(SchemaLoader typeLoader, string schemaNamespace, string codeNamespace, string className, string[] args)
        {
            bool generateAdapters = false;
            bool annotatedOnly = false;
            bool generateEnums = false;

            for (int i = 4; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "-a" || arg == "-adapters")
                    generateAdapters = true;
                else if (arg == "-annotatedOnly")
                    annotatedOnly = true;
                else if (arg == "-enums")
                    generateEnums = true;
            }

            XmlSchemaTypeCollection typeCollection = GetTypeCollection(typeLoader, schemaNamespace);

            string targetNamespace = typeCollection.TargetNamespace;
            XmlQualifiedName[] namespaces = typeCollection.Namespaces;
            List<DomNodeType> nodeTypes = new List<DomNodeType>(typeCollection.GetNodeTypes());

            // Append additional DomNodeTypes from other namespaces, for example if the xs:import was used.
            // Don't include the built-in "anyType".
            // The reason to append is that if there is a conflict with the imported types, priority should
            //  be given to the types defined in the importing schema file.
            foreach (XmlQualifiedName knownNamespace in namespaces)
            {
                if (knownNamespace.Namespace != targetNamespace &&
                    knownNamespace.Namespace != "http://www.w3.org/2001/XMLSchema")
                    nodeTypes.AddRange(typeCollection.GetNodeTypes(knownNamespace.Namespace));
            }

            List<ChildInfo> rootElements = new List<ChildInfo>(typeCollection.GetRootElements());

            StringBuilder sb = new StringBuilder();

            GenerateFileProlog(codeNamespace, args, sb);

            WriteLine(sb, "    public static class {0}", className);
            WriteLine(sb, "    {{");

            WriteLine(sb, "        public const string NS = \"{0}\";", targetNamespace);
            WriteLine(sb, "");
            WriteLine(sb, "        public static void Initialize(XmlSchemaTypeCollection typeCollection)");
            WriteLine(sb, "        {{");
            WriteLine(sb, "            Initialize((ns,name)=>typeCollection.GetNodeType(ns,name),");
            WriteLine(sb, "                (ns,name)=>typeCollection.GetRootElement(ns,name));");
            WriteLine(sb, "        }}");
            WriteLine(sb, "");
            WriteLine(sb, "        public static void Initialize(IDictionary<string, XmlSchemaTypeCollection> typeCollections)");
            WriteLine(sb, "        {{");
            WriteLine(sb, "            Initialize((ns,name)=>typeCollections[ns].GetNodeType(name),");
            WriteLine(sb, "                (ns,name)=>typeCollections[ns].GetRootElement(name));");
            WriteLine(sb, "        }}");
            WriteLine(sb, "");
            WriteLine(sb, "        private static void Initialize(Func<string, string, DomNodeType> getNodeType, Func<string, string, ChildInfo> getRootElement)");
            WriteLine(sb, "        {{");
            
            StringBuilder fieldSb = new StringBuilder();
            Dictionary<string, string> domNodeTypeToClassName = new Dictionary<string, string>(nodeTypes.Count);
            Dictionary<string, string> classNameToDomNodeType = new Dictionary<string, string>(nodeTypes.Count);
            foreach (DomNodeType nodeType in nodeTypes)
            {
                // Determine if the type should be included or skipped
                // If the -annotatedOnly argument is set: types will be included
                //   ONLY IF <sce.domgen include="true"> is defined for this type, all other types will be skipped
                // If -annotatedOnly is NOT set: types will be included by default
                //   UNLESS <sce.domgen include="false" is explicitly set for this type
                bool include = !annotatedOnly;
                XmlNode annotation;
                if (typeLoader.DomGenAnnotations.TryGetValue(nodeType.Name, out annotation))
                {
                    foreach (XmlAttribute attribute in annotation.Attributes)
                    {
                        if (attribute.Name == "include")
                            include = Boolean.Parse(attribute.Value); // will throw if value is not a valid boolean
                    }
                }
                if (!include)
                    continue; // skip this type if it's not to be included
                
                string ns = codeNamespace;
                if (ns == null)
                    ns = targetNamespace;

                string typeName = GetClassName(namespaces, nodeType, domNodeTypeToClassName, classNameToDomNodeType);

                GenerateInitializers(nodeType, typeName, sb);
                WriteLine(sb, "");

                WriteLine(fieldSb, "");
                GenerateFields(nodeType, typeName, fieldSb);
            }

            if (generateEnums)
            {
                GenerateEnums(fieldSb, typeLoader, namespaces, domNodeTypeToClassName, classNameToDomNodeType);
            }

            foreach (ChildInfo rootElement in rootElements)
            {
                string fieldName = rootElement.Name + "RootElement";
                WriteLine(fieldSb, "");
                WriteLine(fieldSb, "        public static ChildInfo {0};", fieldName);
                WriteLine(sb, "            {0} = getRootElement(NS, \"{1}\");", fieldName, rootElement.Name);
            }

            WriteLine(sb, "        }}");

            sb.Append(fieldSb.ToString());

            WriteLine(sb, "    }}");

            if (generateAdapters)
            {
                WriteLine(sb, "");

                foreach (DomNodeType nodeType in nodeTypes)
                {
                    string modifiers = (nodeType.IsAbstract) ? "abstract " : "";

                    string adapterClassName = GetClassName(nodeType, domNodeTypeToClassName);

                    string adapterBaseClassName = "DomNodeAdapter";
                    if (nodeType.BaseType != DomNodeType.BaseOfAllTypes)
                        adapterBaseClassName = GetClassName(nodeType.BaseType, domNodeTypeToClassName);

                    WriteLine(sb, "    public {0}partial class {1} : {2}", modifiers, adapterClassName, adapterBaseClassName);
                    WriteLine(sb, "    {{");

                    foreach (AttributeInfo attrInfo in nodeType.Attributes)
                    {
                        if (attrInfo.DefiningType != nodeType)
                            continue;

                        string typeName = attrInfo.Type.Type.ToString();
                        typeName = s_attributeTypes[typeName];

                        string attrName = attrInfo.Name;
                        string propertyName = attrName;
                        if (s_cSharpKeywords.Contains(propertyName))
                            propertyName = "@" + propertyName;

                        WriteLine(sb, "        public {0} {1}", typeName, propertyName);
                        WriteLine(sb, "        {{");
                        string attrInfoName = className + "." + adapterClassName + "." + attrName + "Attribute";
                        WriteLine(sb, "            get {{ return GetAttribute<{0}>({1}); }}", typeName, attrInfoName);
                        WriteLine(sb, "            set {{ SetAttribute({0}, value); }}", attrInfoName);
                        WriteLine(sb, "        }}");
                    }

                    foreach (ChildInfo childInfo in nodeType.Children)
                    {
                        if (childInfo.DefiningType != nodeType)
                            continue;

                        string childName = childInfo.Name;
                        string propertyName = childName;
                        string typeName = GetClassName(childInfo.Type, domNodeTypeToClassName);

                        string childInfoName = className + "." + adapterClassName + "." + childName + "Child";

                        if (childInfo.IsList)
                        {
                            WriteLine(sb, "        public IList<{0}> {1}", typeName, propertyName);
                            WriteLine(sb, "        {{");
                            WriteLine(sb, "            get {{ return GetChildList<{0}>({1}); }}", typeName, childInfoName);
                            WriteLine(sb, "        }}");
                        }
                        else
                        {
                            WriteLine(sb, "        public {0} {1}", typeName, propertyName);
                            WriteLine(sb, "        {{");
                            WriteLine(sb, "            get {{ return GetChild<{0}>({1}); }}", typeName, childInfoName);
                            WriteLine(sb, "            set {{ SetChild({0}, value); }}", childInfoName);
                            WriteLine(sb, "        }}");
                        }
                    }

                    WriteLine(sb, "    }}");
                    WriteLine(sb, "");
                }
            }

            GenerateFileEpilog(sb);

            return sb.ToString();
        }

        private static void GenerateEnums(StringBuilder sb, 
            SchemaLoader typeLoader, 
            XmlQualifiedName[] namespaces,
            Dictionary<string, string> domNodeTypeToClassName, 
            Dictionary<string, string> classNameToDomNodeType)
        {
            // Temp code
            // Currently the only way I can see to find out which strings are enum type
            // is to search for the StringEnumRule on the AttributeType
            // If this exists then use reflection to access the values list
            // This could and should be done in some nicer way in the future!
            System.Reflection.FieldInfo fInfo = typeof(StringEnumRule).GetField("m_values", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fInfo != null)
            {
                var enumAttributeTypes = new HashSet<AttributeType>();

                foreach (AttributeType attributeType in typeLoader.GetAttributeTypes())
                {
                    StringEnumRule rule = attributeType.Rules.FirstOrDefault(x => x is StringEnumRule) as StringEnumRule;
                    if (rule != null)
                    {
                        if (enumAttributeTypes.Add(attributeType))
                        {
                            string[] values = fInfo.GetValue(rule) as string[];
                            if (values != null)
                            {
                                string enumTypeName = GetClassName(namespaces, attributeType, domNodeTypeToClassName, classNameToDomNodeType);
                                WriteLine(sb, "");
                                WriteLine(sb, "        public enum {0}", enumTypeName);
                                WriteLine(sb, "        {{");

                                foreach (string value in values)
                                {
                                    WriteLine(sb, "             " + value + ",");
                                }

                                if (values.Length > 0)
                                    sb.Length = sb.Length - 1;

                                WriteLine(sb, "        }}");

                            }
                        }
                    }
                }
            }
        }

        private static void GenerateFields(DomNodeType nodeType, string typeName, StringBuilder sb)
        {
            WriteLine(sb, "        public static class {0}", typeName);
            WriteLine(sb, "        {{");

            WriteLine(sb, "            public static DomNodeType Type;");
            foreach (AttributeInfo attributeInfo in nodeType.Attributes)
            {
                string attrInfoName = CreateIdentifier(attributeInfo.Name + "Attribute");
                WriteLine(sb, "            public static AttributeInfo {0};", attrInfoName);
            }
            foreach (ChildInfo child in nodeType.Children)
            {
                string childInfoName = CreateIdentifier(child.Name + "Child");
                WriteLine(sb, "            public static ChildInfo {0};", childInfoName);
            }

            WriteLine(sb, "        }}");
        }

        private static void GenerateInitializers(DomNodeType nodeType, string typeName, StringBuilder sb)
        {
            string ns = "";
            string name = nodeType.Name;
            int separator = name.LastIndexOf(':'); // find colon separating ns from name
            if (separator >= 0)
            {
                ns = name.Substring(0, separator);
                name = name.Substring(separator + 1);
            }

            WriteLine(sb, "            {0}.Type = getNodeType(\"{1}\", \"{2}\");", typeName, ns, name);

            foreach (AttributeInfo attributeInfo in nodeType.Attributes)
            {
                string attrInfoName = CreateIdentifier(attributeInfo.Name + "Attribute");
                WriteLine(sb, "            {1}.{0} = {1}.Type.GetAttributeInfo(\"{2}\");", attrInfoName, typeName, attributeInfo.Name);
            }
            foreach (ChildInfo childInfo in nodeType.Children)
            {
                string childInfoName = CreateIdentifier(childInfo.Name + "Child");
                WriteLine(sb, "            {1}.{0} = {1}.Type.GetChildInfo(\"{2}\");", childInfoName, typeName, childInfo.Name);
            }
        }

        private static XmlSchemaTypeCollection GetTypeCollection(XmlSchemaTypeLoader typeLoader, string schemaNamespace)
        {
            XmlSchemaTypeCollection typeCollection;
            if (schemaNamespace != "")
            {
                typeCollection = typeLoader.GetTypeCollection(schemaNamespace);
            }
            else
            {
                IEnumerable<XmlSchemaTypeCollection> collections = typeLoader.GetTypeCollections();
                typeCollection = collections.First();
            }
            if (typeCollection == null)
            {
                throw new InvalidOperationException(string.Format("schema namespace '{0}' is missing or has no types", schemaNamespace));
            }
            return typeCollection;
        }

        private static string GetClassName(
            XmlQualifiedName[] namespaces, NamedMetadata nodeType,
            Dictionary<string,string> domNodeTypeToClassName,
            Dictionary<string,string> classNameToDomNodeType)
        {
            // Remove the namespace decoration, including the ':'.
            string className = nodeType.Name;
            foreach (XmlQualifiedName xmlName in namespaces)
            {
                if (className.StartsWith(xmlName.Namespace) &&
                    className.Length > xmlName.Namespace.Length &&
                    className[xmlName.Namespace.Length] == ':')
                {
                    className = className.Substring(xmlName.Namespace.Length + 1);
                    break;
                }
            }

            className = CreateIdentifier(className);

            // If we've already seen this class name, revert to the version with the namespace prepended, otherwise
            //  the *.cs file created won't compile.
            if (classNameToDomNodeType.ContainsKey(className))
                className = CreateIdentifier(nodeType.Name);

            domNodeTypeToClassName[nodeType.Name] = className;
            classNameToDomNodeType[className] = nodeType.Name;

            return className;
        }

        private static string GetClassName(DomNodeType nodeType, Dictionary<string, string> domNodeTypeToClassName)
        {
            return domNodeTypeToClassName[nodeType.Name];
        }

        private static string CreateIdentifier(string name)
        {
            string result;
            result = name.Replace('/', '_');
            result = result.Replace('\\', '_');
            result = result.Replace(':', '_');
            result = result.Replace('.', '_');
            result = result.Replace('-', '_');
            return result;
        }

        private static void GenerateFileProlog(string codeNamespace, string[] args, StringBuilder sb)
        {
            WriteLine(sb, "// -------------------------------------------------------------------------------------------------------------------");
            WriteLine(sb, "// Generated code, do not edit");

            sb.Append("// Command Line:  DomGen");
            foreach (string s in args)
            {
                sb.Append(" \"" + s + '"');
            }
            sb.Append(Environment.NewLine);

            WriteLine(sb, "// -------------------------------------------------------------------------------------------------------------------");
            WriteLine(sb, "");
            WriteLine(sb, "using System;");
            WriteLine(sb, "using System.Collections.Generic;");
            WriteLine(sb, "");
            WriteLine(sb, "using Sce.Atf.Dom;");
            WriteLine(sb, "");
            WriteLine(sb, "namespace {0}", codeNamespace);
            WriteLine(sb, "{{");
        }

        private static void GenerateFileEpilog(StringBuilder sb)
        {
            WriteLine(sb, "}}");
        }

        private static void WriteLine(StringBuilder sb, string s, params object[] p)
        {
            sb.Append(string.Format(s, p));
            sb.Append(Environment.NewLine);
        }

        private static HashSet<string> s_cSharpKeywords = new HashSet<string>(
            new string[]
            {
                "abstract", "event", "new", "struct", "as", "explicit", "null", "switch",
                "base", "extern", "object", "this", "bool", "false", "operator", "throw",
                "break", "finally", "out", "true", "byte", "fixed", "override", "try",
                "case", "float", "params", "typeof", "catch", "for", "private", "uint",
                "char", "foreach", "protected", "ulong", "checked", "goto", "public", "unchecked",
                "class", "if", "readonly", "unsafe", "const", "implicit", "ref", "ushort",
                "continue", "in", "return", "using", "decimal", "int", "sbyte", "virtual",
                "default", "interface", "sealed", "volatile", "delegate", "internal", "short", "void",
                "do", "is", "sizeof", "while", "double", "lock", "stackalloc", "else", "long", "static",
                "enum", "namespace", "string"
            });

        private static Dictionary<string, string> s_attributeTypes = new Dictionary<string, string>();

        static SchemaGen()
        {
            s_attributeTypes.Add("Boolean", "bool");
            s_attributeTypes.Add("BooleanArray", "bool[]");
            s_attributeTypes.Add("Int8", "byte");
            s_attributeTypes.Add("Int8Array", "byte[]");
            s_attributeTypes.Add("UInt8", "ubyte");
            s_attributeTypes.Add("UInt8Array", "ubyte[]");
            s_attributeTypes.Add("Int16", "short");
            s_attributeTypes.Add("Int16Array", "short[]");
            s_attributeTypes.Add("UInt16", "ushort");
            s_attributeTypes.Add("UInt16Array", "ushort[]");
            s_attributeTypes.Add("Int32", "int");
            s_attributeTypes.Add("Int32Array", "int[]");
            s_attributeTypes.Add("UInt32", "uint");
            s_attributeTypes.Add("UInt32Array", "uint[]");
            s_attributeTypes.Add("Int64", "long");
            s_attributeTypes.Add("Int64Array", "long[]");
            s_attributeTypes.Add("UInt64", "ulong");
            s_attributeTypes.Add("UInt64Array", "ulong[]");
            s_attributeTypes.Add("Single", "float");
            s_attributeTypes.Add("SingleArray", "float[]");
            s_attributeTypes.Add("Double", "double");
            s_attributeTypes.Add("DoubleArray", "double[]");
            s_attributeTypes.Add("Decimal", "decimal");
            s_attributeTypes.Add("DecimalArray", "decimal[]");
            s_attributeTypes.Add("String", "string");
            s_attributeTypes.Add("StringArray", "string[]");
            s_attributeTypes.Add("DateTime", "DateTime");
            s_attributeTypes.Add("Uri", "Uri");
            s_attributeTypes.Add("Reference", "DomNode");
        }
    }
}
