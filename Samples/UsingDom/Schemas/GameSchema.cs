// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "game.xsd" "GameSchema.cs" "Game.UsingDom" "UsingDom"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace UsingDom
{
    public static class GameSchema
    {
        public const string NS = "Game.UsingDom";

        public static void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            Initialize((ns,name)=>typeCollection.GetNodeType(ns,name),
                (ns,name)=>typeCollection.GetRootElement(ns,name));
        }

        public static void Initialize(IDictionary<string, XmlSchemaTypeCollection> typeCollections)
        {
            Initialize((ns,name)=>typeCollections[ns].GetNodeType(name),
                (ns,name)=>typeCollections[ns].GetRootElement(name));
        }

        private static void Initialize(Func<string, string, DomNodeType> getNodeType, Func<string, string, ChildInfo> getRootElement)
        {
            gameType.Type = getNodeType("Game.UsingDom", "gameType");
            gameType.nameAttribute = gameType.Type.GetAttributeInfo("name");
            gameType.gameObjectChild = gameType.Type.GetChildInfo("gameObject");

            gameObjectType.Type = getNodeType("Game.UsingDom", "gameObjectType");
            gameObjectType.nameAttribute = gameObjectType.Type.GetAttributeInfo("name");

            ogreType.Type = getNodeType("Game.UsingDom", "ogreType");
            ogreType.nameAttribute = ogreType.Type.GetAttributeInfo("name");
            ogreType.sizeAttribute = ogreType.Type.GetAttributeInfo("size");
            ogreType.strengthAttribute = ogreType.Type.GetAttributeInfo("strength");

            dwarfType.Type = getNodeType("Game.UsingDom", "dwarfType");
            dwarfType.nameAttribute = dwarfType.Type.GetAttributeInfo("name");
            dwarfType.ageAttribute = dwarfType.Type.GetAttributeInfo("age");
            dwarfType.experienceAttribute = dwarfType.Type.GetAttributeInfo("experience");

            treeType.Type = getNodeType("Game.UsingDom", "treeType");
            treeType.nameAttribute = treeType.Type.GetAttributeInfo("name");

            gameRootElement = getRootElement(NS, "game");
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
