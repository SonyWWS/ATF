//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace SimpleDomNoXmlEditorSample
{
    /// <summary>
    /// Contains the DomNodeType, ChildInfo, and AttributeInfo types describing this sample app's Document Object Model (DOM).
    /// Normally, these would be defined by a schema file. This sample shows how to define the types without a schema file.</summary>
    [Export(typeof(DomTypes))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DomTypes
    {
        /// <summary>
        /// Gets an enumeration of DOM types</summary>
        /// <returns></returns>
        public IEnumerable<DomNodeType> GetDomTypes()
        {
            return new DomNodeType[]
            {
                DomTypes.animationResourceType.Type,
                DomTypes.eventSequenceType.Type,
                DomTypes.eventType.Type,
                DomTypes.geometryResourceType.Type,
                DomTypes.resourceType.Type,
            };
        }

        /// <summary>
        /// Static constructor</summary>
        static DomTypes()
        {
            // register extensions
            eventSequenceType.Type.Define(new ExtensionInfo<EventSequenceDocument>());
            eventSequenceType.Type.Define(new ExtensionInfo<EventSequenceContext>());
            eventSequenceType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
            eventSequenceType.Type.Define(new ExtensionInfo<EventSequence>());
            eventSequenceType.Type.Define(new ExtensionInfo<ReferenceValidator>());
            eventSequenceType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
            eventSequenceType.Type.Define(new ExtensionInfo<DomNodeQueryable>());

            eventType.Type.Define(new ExtensionInfo<Event>());
            eventType.Type.Define(new ExtensionInfo<EventContext>());

            resourceType.Type.Define(new ExtensionInfo<Resource>());
            
            // Enable metadata driven property editing for events and resources
            AdapterCreator<CustomTypeDescriptorNodeAdapter> creator =
                new AdapterCreator<CustomTypeDescriptorNodeAdapter>();
            eventType.Type.AddAdapterCreator(creator);
            resourceType.Type.AddAdapterCreator(creator);

            // annotate types with display information for palette

            eventType.Type.SetTag(
                new NodeTypePaletteItem(
                    eventType.Type,
                    Localizer.Localize("Event"),
                    Localizer.Localize("Event in a sequence"),
                    Resources.EventImage));

            animationResourceType.Type.SetTag(
                new NodeTypePaletteItem(
                    animationResourceType.Type,
                    Localizer.Localize("Animation"),
                    Localizer.Localize("Animation resource"),
                    Resources.AnimationImage));

            geometryResourceType.Type.SetTag(
                new NodeTypePaletteItem(
                    geometryResourceType.Type,
                    Localizer.Localize("Geometry"),
                    Localizer.Localize("Geometry resource"),
                    Resources.GeometryImage));

            // register property descriptors on state, transition, folder types

            eventType.Type.SetTag(
                new PropertyDescriptorCollection(
                    new Sce.Atf.Dom.PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Name"),
                                eventType.nameAttribute,
                                null,
                                Localizer.Localize("Event name"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Time"),
                                eventType.timeAttribute,
                                null,
                                Localizer.Localize("Event starting time"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Duration"),
                                eventType.durationAttribute,
                                null,
                                Localizer.Localize("Event duration"),
                                false),
                    }));

            animationResourceType.Type.SetTag(
                new PropertyDescriptorCollection(
                    new Sce.Atf.Dom.PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Name"),
                                animationResourceType.nameAttribute,
                                null,
                                Localizer.Localize("Animation name"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Size"),
                                animationResourceType.sizeAttribute,
                                null,
                                Localizer.Localize("Size of animation, in bytes"),
                                true),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("# Tracks"),
                                animationResourceType.tracksAttribute,
                                null,
                                Localizer.Localize("Number of tracks in animation"),
                                true),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Duration"),
                                animationResourceType.durationAttribute,
                                null,
                                Localizer.Localize("Duration of animation, in milliseconds"),
                                true),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Compressed"),
                                animationResourceType.compressedAttribute,
                                null,
                                Localizer.Localize("Whether or not animation is compressed"),
                                false,
                                new BoolEditor()),
                    }));

            geometryResourceType.Type.SetTag(
                new PropertyDescriptorCollection(
                    new Sce.Atf.Dom.PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Name"),
                                geometryResourceType.nameAttribute,
                                null,
                                Localizer.Localize("Geometry name"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Size"),
                                geometryResourceType.sizeAttribute,
                                null,
                                Localizer.Localize("Size of geometry, in bytes"),
                                true),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("# Bones"),
                                geometryResourceType.bonesAttribute,
                                null,
                                Localizer.Localize("Number of bones in geometry"),
                                true),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("# Vertices"),
                                geometryResourceType.verticesAttribute,
                                null,
                                Localizer.Localize("Number of vertices in geometry"),
                                true),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Primitive Kind"),
                                geometryResourceType.primitiveTypeAttribute,
                                null,
                                Localizer.Localize("Kind of primitives in geometry"),
                                false,
                                new EnumUITypeEditor(primitiveKinds),
                                new EnumTypeConverter(primitiveKinds)),
                    }));
        }

        //Schema equivalent:
        //<!--Event sequence, a sequence of events-->
        //<xs:complexType name ="eventSequenceType">
        //  <xs:sequence>
        //    <xs:element name="event" type="eventType" maxOccurs="unbounded"/>
        //  </xs:sequence>
        //</xs:complexType>
        /// <summary>
        /// Event sequence type</summary>
        public static class eventSequenceType
        {
            static eventSequenceType()
            {
                Type.Define(eventChild);
                eventChild.AddRule(new ChildCountRule(1, int.MaxValue));
            }
            public readonly static DomNodeType Type = new DomNodeType("eventSequenceType");
            public readonly static ChildInfo eventChild = new ChildInfo("event", eventType.Type, true);
        }

        //Schema equivalent:
        //<!--Event, with name, start time, duration and a list of resources-->
        //<xs:complexType name ="eventType">
        //  <xs:sequence>
        //    <xs:element name="resource" type="resourceType" maxOccurs="unbounded"/>
        //  </xs:sequence>
        //  <xs:attribute name="name" type="xs:string"/>
        //  <xs:attribute name="time" type="xs:integer"/>
        //  <xs:attribute name="duration" type="xs:integer"/>
        //</xs:complexType>
        /// <summary>
        /// Event type</summary>
        public static class eventType
        {
            static eventType()
            {
                Type.Define(nameAttribute);
                Type.Define(timeAttribute);
                Type.Define(durationAttribute);
                Type.Define(resourceChild);
                resourceChild.AddRule(new ChildCountRule(1, int.MaxValue));
            }

            public readonly static DomNodeType Type = new DomNodeType("eventType");
            public readonly static AttributeInfo nameAttribute =
                new AttributeInfo("name", new AttributeType(AttributeTypes.String.ToString(), typeof(string)));
            public readonly static AttributeInfo timeAttribute =
                new AttributeInfo("time", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
            public readonly static AttributeInfo durationAttribute =
                new AttributeInfo("duration", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
            public readonly static ChildInfo resourceChild = new ChildInfo("resource", Type, true);
        }

        //Schema equivalent:
        //<!--Abstract base type for resources-->
        //<xs:complexType name="resourceType" abstract="true">
        //  <xs:attribute name="name" type="xs:string" use="required"/>
        //  <xs:attribute name="size" type="xs:integer" use="required"/>
        //  <xs:attribute name="compressed" type="xs:boolean"/>
        //</xs:complexType>
        /// <summary>
        /// Resource sequence type</summary>
        public static class resourceType
        {
            static resourceType()
            {
                Type.IsAbstract = true;
                Type.Define(nameAttribute);
                Type.Define(sizeAttribute);
                Type.Define(compressedAttribute);
            }

            public readonly static DomNodeType Type = new DomNodeType("resourceType");
            public readonly static AttributeInfo nameAttribute =
                new AttributeInfo("name", new AttributeType(AttributeTypes.String.ToString(), typeof(string)));
            public readonly static AttributeInfo sizeAttribute =
                new AttributeInfo("size", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
            public readonly static AttributeInfo compressedAttribute =
                new AttributeInfo("compressed", new AttributeType(AttributeTypes.Boolean.ToString(), typeof(bool)));
        }

        //Schema equivalent:
        //<!--Derive an animation resource type-->
        //<xs:complexType name="animationResourceType">
        //  <xs:complexContent>
        //    <xs:extension base="resourceType">
        //      <xs:attribute name="tracks" type="xs:integer"/>
        //      <xs:attribute name="duration" type="xs:integer"/>
        //    </xs:extension>
        //  </xs:complexContent>
        //</xs:complexType>
        /// <summary>
        /// Animation sequence type</summary>
        public static class animationResourceType
        {
            static animationResourceType()
            {
                Type.BaseType = resourceType.Type;
                Type.Define(nameAttribute);
                Type.Define(sizeAttribute);
                Type.Define(compressedAttribute);
                Type.Define(tracksAttribute);
                Type.Define(durationAttribute);
            }

            public readonly static DomNodeType Type = new DomNodeType("animationResourceType");
            public readonly static AttributeInfo nameAttribute =
                new AttributeInfo("name", new AttributeType(AttributeTypes.String.ToString(), typeof(string)));
            public readonly static AttributeInfo sizeAttribute =
                new AttributeInfo("size", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
            public readonly static AttributeInfo compressedAttribute =
                new AttributeInfo("compressed", new AttributeType(AttributeTypes.Boolean.ToString(), typeof(bool)));
            public readonly static AttributeInfo tracksAttribute =
                new AttributeInfo("tracks", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
            public readonly static AttributeInfo durationAttribute =
                new AttributeInfo("duration", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
        }

        private static string[] primitiveKinds = new string[]
            {
                "Lines",
                "Line_Strips",
                "Polygons",
                "Polylist",
                "Triangles",
                "Triangle_Strips",
                "Bezier_Curves",
                "Bezier_Surfaces",
                "Subdivision_Surfaces"
            };

        //Schema equivalent:
        //<xs:simpleType name="primitiveType">
        //  <xs:restriction base="xs:string">
        //    <xs:enumeration value="Lines"/>
        //    <xs:enumeration value="Line_Strips"/>
        //    <xs:enumeration value="Polygons"/>
        //    <xs:enumeration value="Polylist"/>
        //    <xs:enumeration value="Triangles"/>
        //    <xs:enumeration value="Triangle_Strips"/>
        //    <xs:enumeration value="Bezier_Curves"/>
        //    <xs:enumeration value="Bezier_Surfaces"/>
        //    <xs:enumeration value="Subdivision_Surfaces"/>
        //  </xs:restriction>
        //</xs:simpleType>
        /// <summary>
        /// Primitive type rule, which constrains attribute values</summary>
        public readonly static AttributeRule primitiveTypeRule = new StringEnumRule(primitiveKinds);


        //Schema equivalent:
        //<!--Derive a geometry resource type-->
        //<xs:complexType name="geometryResourceType">
        //  <xs:complexContent>
        //    <xs:extension base="resourceType">
        //      <xs:attribute name="bones" type="xs:integer"/>
        //      <xs:attribute name="vertices" type="xs:integer"/>
        //      <xs:attribute name="primitiveType" type="primitiveType"/>
        //    </xs:extension>
        //  </xs:complexContent>
        //</xs:complexType>
        /// <summary>
        /// Geometry sequence type</summary>
        public static class geometryResourceType
        {
            static geometryResourceType()
            {
                Type.BaseType = resourceType.Type;
                Type.Define(nameAttribute);
                Type.Define(sizeAttribute);
                Type.Define(compressedAttribute);
                Type.Define(bonesAttribute);
                Type.Define(verticesAttribute);
                Type.Define(primitiveTypeAttribute);
                primitiveTypeAttribute.AddRule(primitiveTypeRule);
                primitiveTypeAttribute.DefaultValue = primitiveKinds[0];
            }

            /// <summary>
            /// Geometry resouce DomNodeType</summary>
            public readonly static DomNodeType Type = new DomNodeType("geometryResourceType");
            /// <summary>
            /// Name attribute</summary>
            public readonly static AttributeInfo nameAttribute =
                new AttributeInfo("name", new AttributeType(AttributeTypes.String.ToString(), typeof(string)));
            /// <summary>
            /// Size attribute</summary>
            public readonly static AttributeInfo sizeAttribute =
                new AttributeInfo("size", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
            /// <summary>
            /// Compressed attribute</summary>
            public readonly static AttributeInfo compressedAttribute =
                new AttributeInfo("compressed", new AttributeType(AttributeTypes.Boolean.ToString(), typeof(bool)));
            /// <summary>
            /// Bones attribute</summary>
            public readonly static AttributeInfo bonesAttribute =
                new AttributeInfo("bones", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
            /// <summary>
            /// Vertices attribute</summary>
            public readonly static AttributeInfo verticesAttribute =
                new AttributeInfo("vertices", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
            /// <summary>
            /// Primitive type attribute</summary>
            public readonly static AttributeInfo primitiveTypeAttribute =
                new AttributeInfo("primitiveType", new AttributeType(AttributeTypes.String.ToString(), typeof(string)));
        }

        /// <summary>
        /// Event sequence root child metadata</summary>
        public readonly static ChildInfo eventSequenceRootElement = new ChildInfo("eventSequenceRoot", null);
    }
}
