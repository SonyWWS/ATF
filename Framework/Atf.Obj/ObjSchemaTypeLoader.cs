//Sony Computer Entertainment Confidential

using System.Xml.Schema;

using Sce.Atf.Dom;

namespace Sce.Atf.Obj
{
    /// <summary>
    /// XML schema loader that creates DomNodeTypes, parses annotations, and creates attribute rules for objects in the schema</summary>
    public class ObjSchemaTypeLoader : XmlSchemaTypeLoader
    {
        /// <summary>
        /// Gets a string representation of the XmlSchema's namespace</summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Gets the collection containing node types</summary>
        public XmlSchemaTypeCollection TypeCollection { get; private set; }

        /// <summary>
        /// Method called after the schema set has been loaded and the DomNodeTypes have been created, but
        /// before the DomNodeTypes have been frozen. This means that DomNodeType.SetIdAttribute, for example, has
        /// not been called on the DomNodeTypes. Is called shortly before OnDomNodeTypesFrozen.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
            {
                if (!typeCollection.TargetNamespace.EndsWith("obj")) continue;

                Namespace = typeCollection.TargetNamespace;
                TypeCollection = typeCollection;
                Schema.Initialize(typeCollection);

                foreach (DomNodeType nodeType in typeCollection.GetNodeTypes())
                    nodeType.SetIdAttribute("name");

                // Register base interfaces
                Schema.vertexArray_array.Type.Define(new ExtensionInfo<DataSet>());
                Schema.vertexArray_primitives.Type.Define(new ExtensionInfo<PrimitiveSet>());
                Schema.meshType.Type.Define(new ExtensionInfo<Mesh>());
                Schema.nodeType.Type.Define(new ExtensionInfo<DomResource>());
                Schema.nodeType.Type.Define(new ExtensionInfo<Node>());
                Schema.shaderType.Type.Define(new ExtensionInfo<Shader>());

                break;
            }
        }
    }
}
