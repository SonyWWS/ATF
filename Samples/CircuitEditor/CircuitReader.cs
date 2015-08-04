//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Sce.Atf;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    internal class CircuitReader : DomXmlReader
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="loader">Type loader to translate element names to DOM node types</param>
        public CircuitReader(XmlSchemaTypeLoader loader)
            : base(loader)
        {
            var typeloader = loader as SchemaLoader;
            m_version = typeloader.Version; // tool's current schema version 
        }

        /// <summary>
        /// Reads a node tree from a stream</summary>
        /// <param name="stream">Read stream</param>
        /// <param name="uri">URI of stream</param>
        /// <returns>Node tree, from stream</returns>
        public override DomNode Read(Stream stream, Uri uri)
        {
            return base.Read(TransformXmlIfNeeded(stream), uri);
        }

        /// <summary>
        /// Update the XML document if needed, mostly due to version/schema changes</summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        Stream TransformXmlIfNeeded(Stream stream)
        {
            // Linq to XML
            long position = stream.Position;
            var doc = XDocument.Load(stream);
            XAttribute versionAttribute = doc.Root.Attribute("version");
            string versionValue = "1.0"; //default version
            if (versionAttribute != null)
                versionValue = versionAttribute.Value;

            var version = new Version(versionValue);
            if (version.Major != m_version.Major) // need to transform the xml document for version migration
            {
                var migrator = new Migrator();
                migrator.Transform(doc, version, m_version);
               
                // save the updated xml tree
                var xmlStream = new MemoryStream();
                doc.Save(xmlStream);
                xmlStream.Flush(); // adjust this if you want read your data from the stream
                xmlStream.Position = 0;
                return xmlStream;
            }
            else
            {
                stream.Position = position; // rewind and return the original stream
                return stream;
            }
        }

        /// <summary>
        /// Converts the given string to an attribute value and sets the given node using attributeInfo</summary>
        /// <param name="node">DomNode </param>
        /// <param name="attributeInfo">attributeInfo to set</param>
        /// <param name="valueString">The string representation of the attribute value</param>
        protected override void ReadAttribute(DomNode node, AttributeInfo attributeInfo, string valueString)
        {
            base.ReadAttribute(node, attributeInfo, valueString);
            if (node.Type == Schema.templateFolderType.Type && attributeInfo.Name == "referenceFile")
            {
                Uri uri;
                if (Uri.TryCreate(valueString, UriKind.RelativeOrAbsolute, out uri))
                {
                    ImportTemplateLibrary(node, uri);
                }
            }
        }

        /// <summary>
        ///  Imports templates and template folders stored in an external file</summary>
        /// <param name="toFolder">Template folder into which to import templates and template folders</param>
        /// <param name="uri"></param>
        private void ImportTemplateLibrary(DomNode  toFolder, Uri uri)
        {
            string filePath = uri.LocalPath;

            if (File.Exists(filePath))
            {
                // read the existing templates document
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var newReader = new CircuitReader(TypeLoader);
                    DomNode templatesRoot = newReader.Read(stream, uri);
                    ImportTemplates(toFolder, templatesRoot, uri);
                }
            }
        }

        /// <summary>
        /// Imports templates and template folders stored in an external file</summary>
        /// <param name="toFolder">Template folder in which to import templates and template folders</param>
        /// <param name="fromRoot">Root of templates to import</param>
        /// <param name="uri"></param>
        internal void ImportTemplates(DomNode toFolder, DomNode fromRoot, Uri uri)
        {
            TagTemplateTree(fromRoot, uri);
 
            // assume all templates and their containing folders are children of a root template folder 
            foreach (var domNode in fromRoot.LevelSubtree) // add top-level folders
            {
                if (domNode.Type == Schema.templateFolderType.Type) // this should be the root template folder of the imported DOM tree
                {
                    // import the children of the root template folder, but not the root itself
                    foreach (var child in domNode.Children.ToArray())
                    {
                        if (child.Type == Schema.templateFolderType.Type)
                        {
                            toFolder.GetChildList(Schema.templateFolderType.templateFolderChild).Add(child);
                        }
                        else if (child.Type == Schema.templateType.Type)
                        {
                            toFolder.GetChildList(Schema.templateFolderType.templateChild).Add(child);
                        }
                    }
                    break; // skip the rest of the document contents
                }
            }
        }

        /// <summary>
        /// Tag template DOM tree for local ids </summary>
        /// <param name="rootNode"> the node when tagging starts</param>
        /// <param name="uri"></param>
        internal void TagTemplateTree(DomNode rootNode, Uri uri)
        {
            object tagValue = uri.LocalPath.ToUpper();
            foreach (var importedNode in rootNode.Subtree)
            {
                // documents can be opened recursively, so tags may be set already by a child node reader
                if (importedNode.GetTag(typeof (CategoryUniqueIdValidator.IDocumentTag)) == null)
                    importedNode.SetTag(typeof(CategoryUniqueIdValidator.IDocumentTag), tagValue);
            }
        }

        /// <summary>
        /// Resolves XML node references</summary>
        protected override void ResolveReferences()
        {
            base.ResolveReferences();

            if (!UnresolvedReferences.Any())
                return;

            // look up nodes that have "guid" attribute
            var nodeGuidDictionary = new Dictionary<string, DomNode>();
            foreach (var node in Root.Subtree)
            {
                var guidAttr= node.Type.GetAttributeInfo("guid");
                if (guidAttr == null)
                    continue;
                var guidStr = node.GetAttribute(guidAttr) as string;
                if (!string.IsNullOrEmpty(guidStr))
                    nodeGuidDictionary[guidStr] = node;
            }

            // resolve template references by GUIDs
            foreach (var nodeReference in UnresolvedReferences)
            {
                if (nodeReference.AttributeInfo.Name == "guidRef")
                {
                    DomNode refNode;
                    if (nodeGuidDictionary.TryGetValue(nodeReference.Value, out refNode))
                         nodeReference.Node.SetAttribute(nodeReference.AttributeInfo, refNode);
                    else
                    {
                        Outputs.Write(OutputMessageType.Error, "Couldn't resolve node reference by GUID: " + nodeReference.Value);
                        
                        // if DomNode is a template reference, create a missing template 
                        if (nodeReference.Node.Type == Schema.moduleTemplateRefType.Type || 
                            nodeReference.Node.Type == Schema.groupTemplateRefType.Type)
                        {
                           var templateNode =  GetOrCreateMissingTemplateNode(nodeReference.Value);
                           nodeReference.Node.SetAttribute(nodeReference.AttributeInfo, templateNode);
                        }
                     }
                }
            }
        }

        private DomNode GetOrCreateMissingTemplateNode(string guid)
        {
            if (m_missingTemplates == null)
                m_missingTemplates = new Dictionary<string, DomNode>();
            DomNode templateNode;
            if (m_missingTemplates.TryGetValue(guid, out templateNode))
                return templateNode;

            templateNode = new DomNode(Schema.missingTemplateType.Type);
            templateNode.SetAttribute(Schema.missingTemplateType.guidAttribute, guid);
            var moduleChild = new DomNode(Schema.missingModuleType.Type);
            templateNode.SetChild(Schema.missingTemplateType.moduleChild, moduleChild);
            return templateNode;
        }

        private Dictionary<string, DomNode> m_missingTemplates;

        private readonly Version m_version; 
    }
}
