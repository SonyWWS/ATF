// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "skin.xsd" "SkinSchema.cs" "atfskin" "Sce.Atf.Applications"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace Sce.Atf.Applications
{
    internal static class SkinSchema
    {
        public const string NS = "atfskin";

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
            skinType.Type = getNodeType("atfskin", "skinType");
            skinType.styleChild = skinType.Type.GetChildInfo("style");

            styleType.Type = getNodeType("atfskin", "styleType");
            styleType.nameAttribute = styleType.Type.GetAttributeInfo("name");
            styleType.targetTypeAttribute = styleType.Type.GetAttributeInfo("targetType");
            styleType.setterChild = styleType.Type.GetChildInfo("setter");

            setterType.Type = getNodeType("atfskin", "setterType");
            setterType.propertyNameAttribute = setterType.Type.GetAttributeInfo("propertyName");
            setterType.valueInfoChild = setterType.Type.GetChildInfo("valueInfo");
            setterType.listInfoChild = setterType.Type.GetChildInfo("listInfo");

            valueInfoType.Type = getNodeType("atfskin", "valueInfoType");
            valueInfoType.converterAttribute = valueInfoType.Type.GetAttributeInfo("converter");
            valueInfoType.typeAttribute = valueInfoType.Type.GetAttributeInfo("type");
            valueInfoType.valueAttribute = valueInfoType.Type.GetAttributeInfo("value");
            valueInfoType.constructorParamsChild = valueInfoType.Type.GetChildInfo("constructorParams");
            valueInfoType.setterChild = valueInfoType.Type.GetChildInfo("setter");

            constructorParamsType.Type = getNodeType("atfskin", "constructorParamsType");
            constructorParamsType.valueInfoChild = constructorParamsType.Type.GetChildInfo("valueInfo");

            listInfoType.Type = getNodeType("atfskin", "listInfoType");
            listInfoType.valueInfoChild = listInfoType.Type.GetChildInfo("valueInfo");

            skinRootElement = getRootElement(NS, "skin");
        }

        public static class skinType
        {
            public static DomNodeType Type;
            public static ChildInfo styleChild;
        }

        public static class styleType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo targetTypeAttribute;
            public static ChildInfo setterChild;
        }

        public static class setterType
        {
            public static DomNodeType Type;
            public static AttributeInfo propertyNameAttribute;
            public static ChildInfo valueInfoChild;
            public static ChildInfo listInfoChild;
        }

        public static class valueInfoType
        {
            public static DomNodeType Type;
            public static AttributeInfo converterAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo valueAttribute;
            public static ChildInfo constructorParamsChild;
            public static ChildInfo setterChild;
        }

        public static class constructorParamsType
        {
            public static DomNodeType Type;
            public static ChildInfo valueInfoChild;
        }

        public static class listInfoType
        {
            public static DomNodeType Type;
            public static ChildInfo valueInfoChild;
        }

        public static ChildInfo skinRootElement;
    }
}
