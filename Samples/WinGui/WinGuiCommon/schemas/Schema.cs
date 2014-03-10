// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "winGuiCommonData.xsd" "Schema.cs" "winGuiCommonData_1_0" "WinGuiCommon"
// -------------------------------------------------------------------------------------------------------------------

using Sce.Atf.Dom;

namespace WinGuiCommon
{
    public static class Schema
    {
        public const string NS = "winGuiCommonData_1_0";

        public static void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            winGuiCommonDataType.Type = typeCollection.GetNodeType("winGuiCommonDataType");
            winGuiCommonDataType.eventChild = winGuiCommonDataType.Type.GetChildInfo("event");

            eventType.Type = typeCollection.GetNodeType("eventType");
            eventType.nameAttribute = eventType.Type.GetAttributeInfo("name");
            eventType.timeAttribute = eventType.Type.GetAttributeInfo("time");
            eventType.durationAttribute = eventType.Type.GetAttributeInfo("duration");
            eventType.resourceChild = eventType.Type.GetChildInfo("resource");

            resourceType.Type = typeCollection.GetNodeType("resourceType");
            resourceType.nameAttribute = resourceType.Type.GetAttributeInfo("name");
            resourceType.sizeAttribute = resourceType.Type.GetAttributeInfo("size");
            resourceType.compressedAttribute = resourceType.Type.GetAttributeInfo("compressed");

            animationResourceType.Type = typeCollection.GetNodeType("animationResourceType");
            animationResourceType.nameAttribute = animationResourceType.Type.GetAttributeInfo("name");
            animationResourceType.sizeAttribute = animationResourceType.Type.GetAttributeInfo("size");
            animationResourceType.compressedAttribute = animationResourceType.Type.GetAttributeInfo("compressed");
            animationResourceType.tracksAttribute = animationResourceType.Type.GetAttributeInfo("tracks");
            animationResourceType.durationAttribute = animationResourceType.Type.GetAttributeInfo("duration");

            geometryResourceType.Type = typeCollection.GetNodeType("geometryResourceType");
            geometryResourceType.nameAttribute = geometryResourceType.Type.GetAttributeInfo("name");
            geometryResourceType.sizeAttribute = geometryResourceType.Type.GetAttributeInfo("size");
            geometryResourceType.compressedAttribute = geometryResourceType.Type.GetAttributeInfo("compressed");
            geometryResourceType.bonesAttribute = geometryResourceType.Type.GetAttributeInfo("bones");
            geometryResourceType.verticesAttribute = geometryResourceType.Type.GetAttributeInfo("vertices");
            geometryResourceType.primitiveTypeAttribute = geometryResourceType.Type.GetAttributeInfo("primitiveType");

            winGuiCommonDataRootElement = typeCollection.GetRootElement("winGuiCommonData");
        }

        public static class winGuiCommonDataType
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

        public static ChildInfo winGuiCommonDataRootElement;
    }
}
