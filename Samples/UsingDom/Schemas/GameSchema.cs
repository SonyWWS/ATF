// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "game.xsd" "GameSchema.cs" "Game.UsingDom" "UsingDom"
// -------------------------------------------------------------------------------------------------------------------

using Sce.Atf.Dom;

namespace UsingDom
{
    public static class GameSchema
    {
        public const string NS = "Game.UsingDom";

        public static void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            gameType.Type = typeCollection.GetNodeType("gameType");
            gameType.nameAttribute = gameType.Type.GetAttributeInfo("name");
            gameType.gameObjectChild = gameType.Type.GetChildInfo("gameObject");

            gameObjectType.Type = typeCollection.GetNodeType("gameObjectType");
            gameObjectType.nameAttribute = gameObjectType.Type.GetAttributeInfo("name");

            ogreType.Type = typeCollection.GetNodeType("ogreType");
            ogreType.nameAttribute = ogreType.Type.GetAttributeInfo("name");
            ogreType.sizeAttribute = ogreType.Type.GetAttributeInfo("size");
            ogreType.strengthAttribute = ogreType.Type.GetAttributeInfo("strength");

            dwarfType.Type = typeCollection.GetNodeType("dwarfType");
            dwarfType.nameAttribute = dwarfType.Type.GetAttributeInfo("name");
            dwarfType.ageAttribute = dwarfType.Type.GetAttributeInfo("age");
            dwarfType.experienceAttribute = dwarfType.Type.GetAttributeInfo("experience");

            treeType.Type = typeCollection.GetNodeType("treeType");
            treeType.nameAttribute = treeType.Type.GetAttributeInfo("name");

            gameRootElement = typeCollection.GetRootElement("game");
        }

        public static class gameType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo gameObjectChild;
        }

        public static class gameObjectType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
        }

        public static class ogreType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo sizeAttribute;
            public static AttributeInfo strengthAttribute;
        }

        public static class dwarfType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo ageAttribute;
            public static AttributeInfo experienceAttribute;
        }

        public static class treeType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
        }

        public static ChildInfo gameRootElement;
    }
}
