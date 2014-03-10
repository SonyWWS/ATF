//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Reflection;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Dom;

namespace UsingDom
{
    public class GameSchemaLoader : XmlSchemaTypeLoader
    {
        public GameSchemaLoader()
        {
            // set resolver to locate embedded .xsd file
            SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "UsingDom/Schemas");
            Load("game.xsd");
        }

        public string NameSpace
        {
            get { return m_namespace; }
        }
        private string m_namespace;

        public XmlSchemaTypeCollection TypeCollection
        {
            get { return m_typeCollection; }
        }
        private XmlSchemaTypeCollection m_typeCollection;

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
