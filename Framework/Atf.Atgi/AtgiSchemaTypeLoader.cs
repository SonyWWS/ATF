//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Xml.Schema;

using Sce.Atf.Dom;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// Universal ATGI plug-in that registers a resolver and persister for ATGI files. Can read
    /// any version of an ATGI file as if its namespace matched the atgi.xsd namespace.</summary>
    public class AtgiSchemaTypeLoader : XmlSchemaTypeLoader
    {
        /// <summary>
        /// Gets the namespace of the version of ATGI that is in use</summary>
        public string Namespace
        {
            get { return m_namespace; }
        }

        /// <summary>
        /// Gets the collection containing ATGI node types</summary>
        public XmlSchemaTypeCollection TypeCollection
        {
            get { return m_typeCollection; }
        }

        /// <summary>
        /// Method called after the schema set has been loaded and the DomNodeTypes have been created, but
        /// before the DomNodeTypes have been frozen. This means that DomNodeType.SetIdAttribute, for example, has
        /// not been called on the DomNodeTypes. Is called shortly before OnDomNodeTypesFrozen.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
            {
                if (typeCollection.TargetNamespace.EndsWith("atgi"))
                {
                    m_namespace = typeCollection.TargetNamespace;
                    m_typeCollection = typeCollection;
                    Schema.Initialize(typeCollection);

                    // "name" is the standard id attribute in ATGI files
                    foreach (DomNodeType nodeType in typeCollection.GetNodeTypes())
                        nodeType.SetIdAttribute("name");

                    // Register base interfaces
                    Schema.animType.Type.Define(new ExtensionInfo<Anim>());
                    Schema.animChannelType.Type.Define(new ExtensionInfo<AnimChannel>());
                    Schema.animChannelType_animData.Type.Define(new ExtensionInfo<AnimData>());
                    Schema.animclipType.Type.Define(new ExtensionInfo<AnimClip>());
                    Schema.customDataType.Type.Define(new ExtensionInfo<CustomData>());
                    Schema.customDataAttributeType.Type.Define(new ExtensionInfo<CustomDataAttribute>());
                    Schema.vertexArray_array.Type.Define(new ExtensionInfo<DataSet>());
                    Schema.instanceType.Type.Define(new ExtensionInfo<Instance>());
                    Schema.jointType.Type.Define(new ExtensionInfo<Joint>());
                    Schema.lodgroupType.Type.Define(new ExtensionInfo<LodGroup>());
                    Schema.materialType.Type.Define(new ExtensionInfo<Material>());
                    Schema.materialType_binding.Type.Define(new ExtensionInfo<MaterialBinding>());
                    Schema.meshType.Type.Define(new ExtensionInfo<Mesh>());
                    Schema.nodeType.Type.Define(new ExtensionInfo<Node>());
                    Schema.poseType.Type.Define(new ExtensionInfo<Pose>());
                    Schema.poseType_element.Type.Define(new ExtensionInfo<PoseElement>());
                    Schema.vertexArray_array.Type.Define(new ExtensionInfo<DataSet>());
                    Schema.vertexArray_primitives.Type.Define(new ExtensionInfo<PrimitiveSet>());
                    Schema.sceneType.Type.Define(new ExtensionInfo<Scene>());
                    Schema.shaderType.Type.Define(new ExtensionInfo<Shader>());
                    Schema.shaderType_binding.Type.Define(new ExtensionInfo<ShaderBinding>());
                    Schema.textureType.Type.Define(new ExtensionInfo<Texture>());
                    Schema.worldType.Type.Define(new ExtensionInfo<World>());
                    Schema.worldType.Type.Define(new ExtensionInfo<DomResource>());

                    // Register adapter for node property editing
                    // I had to comment out the line below, because adding the AdapterCreator freezes the types
                    // which makes it impossible to add constraints (e.g. BoundingBox & Transform constraint) later.
                    //Schema.nodeType.Type.AddAdapterCreator(new AdapterCreator<NodeProperties>());

                    break;
                }
            }
        }

        private string m_namespace;
        private XmlSchemaTypeCollection m_typeCollection;
    }
}
