// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "timeline.xsd" "..\Schema.cs" "timeline" "TimelineEditorSample"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace TimelineEditorSample
{
    public static class Schema
    {
        public const string NS = "timeline";

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
            timelineType.Type = getNodeType("timeline", "timelineType");
            timelineType.groupChild = timelineType.Type.GetChildInfo("group");
            timelineType.markerChild = timelineType.Type.GetChildInfo("marker");
            timelineType.timelineRefChild = timelineType.Type.GetChildInfo("timelineRef");

            groupType.Type = getNodeType("timeline", "groupType");
            groupType.nameAttribute = groupType.Type.GetAttributeInfo("name");
            groupType.expandedAttribute = groupType.Type.GetAttributeInfo("expanded");
            groupType.trackChild = groupType.Type.GetChildInfo("track");

            trackType.Type = getNodeType("timeline", "trackType");
            trackType.nameAttribute = trackType.Type.GetAttributeInfo("name");
            trackType.intervalChild = trackType.Type.GetChildInfo("interval");
            trackType.keyChild = trackType.Type.GetChildInfo("key");

            intervalType.Type = getNodeType("timeline", "intervalType");
            intervalType.startAttribute = intervalType.Type.GetAttributeInfo("start");
            intervalType.descriptionAttribute = intervalType.Type.GetAttributeInfo("description");
            intervalType.nameAttribute = intervalType.Type.GetAttributeInfo("name");
            intervalType.lengthAttribute = intervalType.Type.GetAttributeInfo("length");
            intervalType.colorAttribute = intervalType.Type.GetAttributeInfo("color");

            eventType.Type = getNodeType("timeline", "eventType");
            eventType.startAttribute = eventType.Type.GetAttributeInfo("start");
            eventType.descriptionAttribute = eventType.Type.GetAttributeInfo("description");

            keyType.Type = getNodeType("timeline", "keyType");
            keyType.startAttribute = keyType.Type.GetAttributeInfo("start");
            keyType.descriptionAttribute = keyType.Type.GetAttributeInfo("description");
            keyType.specialEventAttribute = keyType.Type.GetAttributeInfo("specialEvent");

            markerType.Type = getNodeType("timeline", "markerType");
            markerType.startAttribute = markerType.Type.GetAttributeInfo("start");
            markerType.descriptionAttribute = markerType.Type.GetAttributeInfo("description");
            markerType.nameAttribute = markerType.Type.GetAttributeInfo("name");
            markerType.colorAttribute = markerType.Type.GetAttributeInfo("color");

            timelineRefType.Type = getNodeType("timeline", "timelineRefType");
            timelineRefType.nameAttribute = timelineRefType.Type.GetAttributeInfo("name");
            timelineRefType.startAttribute = timelineRefType.Type.GetAttributeInfo("start");
            timelineRefType.descriptionAttribute = timelineRefType.Type.GetAttributeInfo("description");
            timelineRefType.colorAttribute = timelineRefType.Type.GetAttributeInfo("color");
            timelineRefType.refAttribute = timelineRefType.Type.GetAttributeInfo("ref");

            timelineRootElement = getRootElement(NS, "timeline");
        }

        public static class timelineType
        {
            public static DomNodeType Type;
            public static ChildInfo groupChild;
            public static ChildInfo markerChild;
            public static ChildInfo timelineRefChild;
        }

        public static class groupType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static ChildInfo trackChild;
        }

        public static class trackType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo intervalChild;
            public static ChildInfo keyChild;
        }

        public static class intervalType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
        }

        public static class eventType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
        }

        public static class keyType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo specialEventAttribute;
        }

        public static class markerType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo colorAttribute;
        }

        public static class timelineRefType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo refAttribute;
        }

        public static ChildInfo timelineRootElement;
    }
}
