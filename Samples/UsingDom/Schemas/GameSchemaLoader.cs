//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Reflection;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Dom;

namespace UsingDom
{
    /// <summary>
    /// Loads the game schema and defines data extensions on the DOM types</summary>
    public class GameSchemaLoader : XmlSchemaTypeLoader
    {
        /// <summary>
        /// Constructor</summary>
        public GameSchemaLoader()
        {
            // set resolver to locate embedded .xsd file
            SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "UsingDom/Schemas");
            Load("game.xsd");
        }

        /// <summary>
        /// Gets the game namespace</summary>
        public string NameSpace
        {
            get { return m_namespace; }
        }
        private string m_namespace;

        /// <summary>
        /// Gets the game type collection</summary>
        public XmlSchemaTypeCollection TypeCollection
        {
            get { return m_typeCollection; }
        }
        private XmlSchemaTypeCollection m_typeCollection;

        /// <summary>
        /// Method called after the schema set has been loaded and the DomNodeTypes have been created, but
        /// before the DomNodeTypes have been frozen. This means that DomNodeType.SetIdAttribute, for example, has
        /// not been called on the DomNodeTypes. Is called shortly before OnDomNodeTypesFrozen.
        /// Defines DOM adapters on the DOM types.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
            {
                m_namespace = typeCollection.TargetNamespace;
                m_typeCollection = typeCollection;
                GameSchema.Initialize(typeCollection);
                                
                // register extensions
                GameSchema.gameType.Type.Define(new ExtensionInfo<Game>());
                GameSchema.gameType.Type.Define(new ExtensionInfo<ReferenceValidator>());
                GameSchema.gameType.Type.Define(new ExtensionInfo<UniqueIdValidator>());

                GameSchema.gameObjectType.Type.Define(new ExtensionInfo<GameObject>());
                GameSchema.dwarfType.Type.Define(new ExtensionInfo<Dwarf>());
                GameSchema.ogreType.Type.Define(new ExtensionInfo<Ogre>());                
                break;
            }
        }
    }
}
