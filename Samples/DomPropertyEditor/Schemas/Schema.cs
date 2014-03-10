// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "game.xsd" "Schema.cs" "GameWorld" "DomPropertyEditorSample"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace DomPropertyEditorSample
{
    public static class Schema
    {
        public const string NS = "GameWorld";

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
            gameType.Type = getNodeType("GameWorld", "gameType");
            gameType.nameAttribute = gameType.Type.GetAttributeInfo("name");
            gameType.gameObjectChild = gameType.Type.GetChildInfo("gameObject");

            gameObjectType.Type = getNodeType("GameWorld", "gameObjectType");
            gameObjectType.nameAttribute = gameObjectType.Type.GetAttributeInfo("name");
            gameObjectType.visibleAttribute = gameObjectType.Type.GetAttributeInfo("visible");
            gameObjectType.translateAttribute = gameObjectType.Type.GetAttributeInfo("translate");
            gameObjectType.rotateAttribute = gameObjectType.Type.GetAttributeInfo("rotate");
            gameObjectType.scaleAttribute = gameObjectType.Type.GetAttributeInfo("scale");

            clubType.Type = getNodeType("GameWorld", "clubType");
            clubType.spikesAttribute = clubType.Type.GetAttributeInfo("spikes");
            clubType.DamageAttribute = clubType.Type.GetAttributeInfo("Damage");
            clubType.wieghtAttribute = clubType.Type.GetAttributeInfo("wieght");

            armorType.Type = getNodeType("GameWorld", "armorType");
            armorType.nameAttribute = armorType.Type.GetAttributeInfo("name");
            armorType.defenseAttribute = armorType.Type.GetAttributeInfo("defense");
            armorType.priceAttribute = armorType.Type.GetAttributeInfo("price");

            orcType.Type = getNodeType("GameWorld", "orcType");
            orcType.nameAttribute = orcType.Type.GetAttributeInfo("name");
            orcType.visibleAttribute = orcType.Type.GetAttributeInfo("visible");
            orcType.translateAttribute = orcType.Type.GetAttributeInfo("translate");
            orcType.rotateAttribute = orcType.Type.GetAttributeInfo("rotate");
            orcType.scaleAttribute = orcType.Type.GetAttributeInfo("scale");
            orcType.skillAttribute = orcType.Type.GetAttributeInfo("skill");
            orcType.weightAttribute = orcType.Type.GetAttributeInfo("weight");
            orcType.emotionAttribute = orcType.Type.GetAttributeInfo("emotion");
            orcType.goalsAttribute = orcType.Type.GetAttributeInfo("goals");
            orcType.healthAttribute = orcType.Type.GetAttributeInfo("health");
            orcType.skinColorAttribute = orcType.Type.GetAttributeInfo("skinColor");
            orcType.toeColorAttribute = orcType.Type.GetAttributeInfo("toeColor");
            orcType.textureFileAttribute = orcType.Type.GetAttributeInfo("textureFile");
            orcType.textureArrayAttribute = orcType.Type.GetAttributeInfo("textureArray");
            orcType.textureTransformAttribute = orcType.Type.GetAttributeInfo("textureTransform");
            orcType.TextureRevDateAttribute = orcType.Type.GetAttributeInfo("TextureRevDate");
            orcType.resourceFolderAttribute = orcType.Type.GetAttributeInfo("resourceFolder");
            orcType.clubChild = orcType.Type.GetChildInfo("club");
            orcType.armorChild = orcType.Type.GetChildInfo("armor");
            orcType.orcChild = orcType.Type.GetChildInfo("orc");

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
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
        }

        public static class clubType
        {
            public static DomNodeType Type;
            public static AttributeInfo spikesAttribute;
            public static AttributeInfo DamageAttribute;
            public static AttributeInfo wieghtAttribute;
        }

        public static class armorType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo defenseAttribute;
            public static AttributeInfo priceAttribute;
        }

        public static class orcType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo skillAttribute;
            public static AttributeInfo weightAttribute;
            public static AttributeInfo emotionAttribute;
            public static AttributeInfo goalsAttribute;
            public static AttributeInfo healthAttribute;
            public static AttributeInfo skinColorAttribute;
            public static AttributeInfo toeColorAttribute;
            public static AttributeInfo textureFileAttribute;
            public static AttributeInfo textureArrayAttribute;
            public static AttributeInfo textureTransformAttribute;
            public static AttributeInfo TextureRevDateAttribute;
            public static AttributeInfo resourceFolderAttribute;
            public static ChildInfo clubChild;
            public static ChildInfo armorChild;
            public static ChildInfo orcChild;
        }

        public static ChildInfo gameRootElement;
    }
}
