// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "eventSequence.xsd" "Schema.cs" "eventSequence_1_0" "SimpleDomEditorWpfSample"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace SimpleDomEditorWpfSample
{
    public static class Schema
    {
        public const string NS = "eventSequence_1_0";

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
            eventSequenceType.Type = getNodeType("eventSequence_1_0", "eventSequenceType");
            eventSequenceType.eventChild = eventSequenceType.Type.GetChildInfo("event");

            eventType.Type = getNodeType("eventSequence_1_0", "eventType");
            eventType.nameAttribute = eventType.Type.GetAttributeInfo("name");
            eventType.timeAttribute = eventType.Type.GetAttributeInfo("time");
            eventType.durationAttribute = eventType.Type.GetAttributeInfo("duration");
            eventType.resourceChild = eventType.Type.GetChildInfo("resource");

            resourceType.Type = getNodeType("eventSequence_1_0", "resourceType");
            resourceType.nameAttribute = resourceType.Type.GetAttributeInfo("name");
            resourceType.sizeAttribute = resourceType.Type.GetAttributeInfo("size");
            resourceType.compressedAttribute = resourceType.Type.GetAttributeInfo("compressed");

            animationResourceType.Type = getNodeType("eventSequence_1_0", "animationResourceType");
            animationResourceType.nameAttribute = animationResourceType.Type.GetAttributeInfo("name");
            animationResourceType.sizeAttribute = animationResourceType.Type.GetAttributeInfo("size");
            animationResourceType.compressedAttribute = animationResourceType.Type.GetAttributeInfo("compressed");
            animationResourceType.tracksAttribute = animationResourceType.Type.GetAttributeInfo("tracks");
            animationResourceType.durationAttribute = animationResourceType.Type.GetAttributeInfo("duration");

            geometryResourceType.Type = getNodeType("eventSequence_1_0", "geometryResourceType");
            geometryResourceType.nameAttribute = geometryResourceType.Type.GetAttributeInfo("name");
            geometryResourceType.sizeAttribute = geometryResourceType.Type.GetAttributeInfo("size");
            geometryResourceType.compressedAttribute = geometryResourceType.Type.GetAttributeInfo("compressed");
            geometryResourceType.bonesAttribute = geometryResourceType.Type.GetAttributeInfo("bones");
            geometryResourceType.verticesAttribute = geometryResourceType.Type.GetAttributeInfo("vertices");
            geometryResourceType.primitiveTypeAttribute = geometryResourceType.Type.GetAttributeInfo("primitiveType");

            eventSequenceRootElement = getRootElement(NS, "eventSequence");
        }

        public static class eventSequenceType
        {
            public static DomNodeType Type;
            public static ChildInfo eventChild;
        }

        public static class eventType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo timeAttribute;
            public static AttributeInfo durationAttribute;
            public static ChildInfo resourceChild;
        }

        public static class resourceType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo sizeAttribute;
            public static AttributeInfo compressedAttribute;
        }

        public static class animationResourceType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo sizeAttribute;
            public static AttributeInfo compressedAttribute;
            public static AttributeInfo tracksAttribute;
            public static AttributeInfo durationAttribute;
        }

        public static class geometryResourceType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo sizeAttribute;
            public static AttributeInfo compressedAttribute;
            public static AttributeInfo bonesAttribute;
            public static AttributeInfo verticesAttribute;
            public static AttributeInfo primitiveTypeAttribute;
        }

        public static ChildInfo eventSequenceRootElement;
    }
}
