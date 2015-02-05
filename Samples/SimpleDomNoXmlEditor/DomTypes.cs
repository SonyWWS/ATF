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
    /// Contains the DomNodeType, ChildInfo, and AttributeInfo types describing this sample application's Document Object Model (DOM).
    /// Normally, these would be defined by a schema file and set up in a schema loader, as in the Simple DOM Editor sample. 
    /// This sample shows how to define the types without a schema file.</summary>
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
        /// Static constructor, add type information for palette and setting up property descriptors</summary>
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
                    eventType.Name,
                    "Event in a sequence".Localize(),
                    Resources.EventImage));

            animationResourceType.Type.SetTag(
                new NodeTypePaletteItem(
                    animationResourceType.Type,
                    animationResourceType.Name,
                    "Animation resource".Localize(),
                    Resources.AnimationImage));

            geometryResourceType.Type.SetTag(
                new NodeTypePaletteItem(
                    geometryResourceType.Type,
                    geometryResourceType.Name,
                    "Geometry resource".Localize(),
                    Resources.GeometryImage));

            // register property descriptors on state, transition, folder types

            eventType.Type.SetTag(
                new PropertyDescriptorCollection(
                    new Sce.Atf.Dom.PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Name".Localize(),
                                eventType.nameAttribute,
                                null,
                                "Event name".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Time".Localize(),
                                eventType.timeAttribute,
                                null,
                                "Event starting time".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Duration".Localize(),
                                eventType.durationAttribute,
                                null,
                                "Event duration".Localize(),
                                false),
                    }));

            animationResourceType.Type.SetTag(
                new PropertyDescriptorCollection(
                    new Sce.Atf.Dom.PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Name".Localize(),
                                animationResourceType.nameAttribute,
                                null,
                                "Animation name".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Size".Localize(),
                                animationResourceType.sizeAttribute,
                                null,
                                "Size of animation, in bytes".Localize(),
                                true),
                            new AttributePropertyDescriptor(
                                "# Tracks".Localize(),
                                animationResourceType.tracksAttribute,
                                null,
                                "Number of tracks in animation".Localize(),
                                true),
                            new AttributePropertyDescriptor(
                                "Duration".Localize(),
                                animationResourceType.durationAttribute,
                                null,
                                "Duration of animation, in milliseconds".Localize(),
                                true),
                            new AttributePropertyDescriptor(
                                "Compressed".Localize(),
                                animationResourceType.compressedAttribute,
                                null,
                                "Whether or not animation is compressed".Localize(),
                                false,
                                new BoolEditor()),
                    }));

            geometryResourceType.Type.SetTag(
                new PropertyDescriptorCollection(
                    new Sce.Atf.Dom.PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Name".Localize(),
                                geometryResourceType.nameAttribute,
                                null,
                                "Geometry name".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Size".Localize(),
                                geometryResourceType.sizeAttribute,
                                null,
                                "Size of geometry, in bytes".Localize(),
                                true),
                            new AttributePropertyDescriptor(
                                "# Bones".Localize(),
                                geometryResourceType.bonesAttribute,
                                null,
                                "Number of bones in geometry".Localize(),
                                true),
                            new AttributePropertyDescriptor(
                                "# Vertices".Localize(),
                                geometryResourceType.verticesAttribute,
                                null,
                                "Number of vertices in geometry".Localize(),
                                true),
                            new AttributePropertyDescriptor(
                                "Primitive Kind".Localize(),
                                geometryResourceType.primitiveTypeAttribute,
                                null,
                                "Kind of primitives in geometry".Localize(),
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
            /// <summary>
            /// Type for event sequence</summary>
            public readonly static DomNodeType Type = new DomNodeType("eventSequenceType");
            /// <summary>
            /// ChildInfo for event in sequence</summary>
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

                nameAttribute.DefaultValue = Name; //set it here so as to not require tricky static field ordering
            }

            /// <summary>
            /// The user-readable name</summary>
            public static string Name = "Event".Localize();

            /// <summary>
            /// Type for event</summary>
            public readonly static DomNodeType Type = new DomNodeType("eventType");

            /// <summary>
            /// AttributeInfo for event name</summary>
            public readonly static AttributeInfo nameAttribute =
                new AttributeInfo("name", new AttributeType(AttributeTypes.String.ToString(), typeof(string)));
            /// <summary>
            /// AttributeInfo for event time</summary>
            public readonly static AttributeInfo timeAttribute =
                new AttributeInfo("time", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
            /// <summary>
            /// AttributeInfo for event duration</summary>
            public readonly static AttributeInfo durationAttribute =
                new AttributeInfo("duration", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
            /// <summary>
            /// ChildInfo for resources associated with event</summary>
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

            /// <summary>
            /// Type for resource in general; there are several resource types</summary>
            public readonly static DomNodeType Type = new DomNodeType("resourceType");
            /// <summary>
            /// AttributeInfo for resource name</summary>
            public readonly static AttributeInfo nameAttribute =
                new AttributeInfo("name", new AttributeType(AttributeTypes.String.ToString(), typeof(string)));
            /// <summary>
            /// AttributeInfo for resource size</summary>
            public readonly static AttributeInfo sizeAttribute =
                new AttributeInfo("size", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
            /// <summary>
            /// AttributeInfo for whether resource compressed</summary>
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

                nameAttribute.DefaultValue = Name; //set it here so as to not require tricky static field ordering
            }

            /// <summary>
            /// The user-readable name</summary>
            public static string Name = "Animation".Localize();

            /// <summary>
            /// Type for animation resource</summary>
            public readonly static DomNodeType Type = new DomNodeType("animationResourceType");

            /// <summary>
            /// AttributeInfo for animation resource name</summary>
            public readonly static AttributeInfo nameAttribute =
                new AttributeInfo("name", new AttributeType(AttributeTypes.String.ToString(), typeof(string)));
            /// <summary>
            /// AttributeInfo for animation size</summary>
            public readonly static AttributeInfo sizeAttribute =
                new AttributeInfo("size", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
            /// <summary>
            /// AttributeInfo for whether animation compressed</summary>
            public readonly static AttributeInfo compressedAttribute =
                new AttributeInfo("compressed", new AttributeType(AttributeTypes.Boolean.ToString(), typeof(bool)));
            /// <summary>
            /// AttributeInfo for number of animation tracks</summary>
            public readonly static AttributeInfo tracksAttribute =
                new AttributeInfo("tracks", new AttributeType(AttributeTypes.Int32.ToString(), typeof(int)));
            /// <summary>
            /// AttributeInfo for animation duration</summary>
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

                nameAttribute.DefaultValue = Name; //set it here so as to not require tricky static field ordering
            }

            /// <summary>
            /// The user-readable name</summary>
            public static string Name = "Geometry".Localize();

            /// <summary>
            /// Geometry resource DomNodeType</summary>
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
