//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// This custom writer only writes out the sub-circuits that are actually referenced 
    /// by a SubCircuitInstance</summary>
    internal class CircuitWriter : DomXmlWriter
    {
        public CircuitWriter(XmlSchemaTypeCollection typeCollection)
            : base(typeCollection)
        {
            PreserveSimpleElements = true;
        }

        // Scan for all sub-circuits that are referenced directly or indirectly by a module in
        //  the root of the document
        public override void Write(DomNode root, Stream stream, Uri uri)
        {
            #pragma warning disable 618 //mastered sub-circuits are obsolete
            m_usedSubCircuits = new HashSet<Sce.Atf.Controls.Adaptable.Graphs.SubCircuit>();
            foreach (var module in root.Cast<Circuit>().Elements)
                FindUsedSubCircuits(module.DomNode);
            #pragma warning restore 618

            base.Write(root, stream, uri);
        }

        #pragma warning disable 618 //mastered sub-circuits are obsolete
        private void FindUsedSubCircuits(DomNode rootNode)
        {
            foreach (DomNode node in rootNode.Subtree)
            {
                var instance = node.As<SubCircuitInstance>();
                if (instance != null)
                {
                    if (m_usedSubCircuits.Add(instance.SubCircuit))
                    {
                        // first time seeing this sub-circuit, so let's recursively add whatever it references
                        foreach (Module module in instance.SubCircuit.Elements)
                            FindUsedSubCircuits(module.DomNode);
                    }
                }
            }
        }
        #pragma warning restore 618

        // Filter out sub-circuits that are not actually needed
        protected override void WriteElement(DomNode node, XmlWriter writer)
        {
            #pragma warning disable 618 //mastered sub-circuits are obsolete
            var subCircuit = node.As<SubCircuit>();
            if (subCircuit != null && !m_usedSubCircuits.Contains(subCircuit))
                return;
            #pragma warning restore 618

            base.WriteElement(node, writer);
        }

        protected override void WriteChildElementsRecursive(DomNode node, XmlWriter writer)
        {
            // Filter out external template file references that should not be in-lined
            if (node.Is<TemplateFolder>())
            {
                var pathUri = node.GetAttribute(Schema.templateFolderType.referenceFileAttribute) as Uri;
                if (pathUri != null)
                    return;
            }
            base.WriteChildElementsRecursive(node, writer);
        }

		protected override string Convert(DomNode node, AttributeInfo attributeInfo)
		{
			object value = node.GetAttribute(attributeInfo);
			if (attributeInfo.Type.Type == AttributeTypes.Reference && attributeInfo.Name == "guidRef")
			{
				// guidRef refers a template whose guid value should be persisted
				var templateNode = value as DomNode;
				return templateNode.GetAttribute(Schema.templateType.guidAttribute) as string;
			}

			return base.Convert(node, attributeInfo);
		}

        #pragma warning disable 618 //mastered sub-circuits are obsolete
        private HashSet<Sce.Atf.Controls.Adaptable.Graphs.SubCircuit> m_usedSubCircuits;
        #pragma warning restore 618
    }

}
