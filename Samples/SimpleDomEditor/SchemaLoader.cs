//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using PropertyDescriptor = Sce.Atf.Dom.PropertyDescriptor;

namespace SimpleDomEditorSample
{
    /// <summary>
    /// Loads the event schema, registers data extensions on the DOM types, and annotates
    /// the types with display information and PropertyDescriptors.</summary>
    [Export(typeof(SchemaLoader))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SchemaLoader : XmlSchemaTypeLoader
    {
        /// <summary>
        /// Constructor</summary>
        public SchemaLoader()
        {
            // set resolver to locate embedded .xsd file
            SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "SimpleDomEditorSample/schemas");
            Load("eventSequence.xsd");
        }

        /// <summary>
        /// Gets the schema namespace</summary>
        public string NameSpace
        {
            get { return m_namespace; }
        }
        private string m_namespace;

        /// <summary>
        /// Gets the schema type collection</summary>
        public XmlSchemaTypeCollection TypeCollection
        {
            get { return m_typeCollection; }
        }
        private XmlSchemaTypeCollection m_typeCollection;

        /// <summary>
        /// Method called after the schema set has been loaded and the DomNodeTypes have been created, but
        /// before the DomNodeTypes have been frozen. This means that DomNodeType.SetIdAttribute, for example, has
        /// not been called on the DomNodeTypes. Is called shortly before OnDomNodeTypesFrozen.
        /// Defines DOM adapters on the DOM types.
        /// Sets up information for types in palette. Constructs PropertyDescriptors for types.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
            {
                m_namespace = typeCollection.TargetNamespace;
                m_typeCollection = typeCollection;
                Schema.Initialize(typeCollection);

                // register extensions
                Schema.eventSequenceType.Type.Define(new ExtensionInfo<EventSequenceDocument>());
                Schema.eventSequenceType.Type.Define(new ExtensionInfo<EventSequenceContext>());
                Schema.eventSequenceType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
                Schema.eventSequenceType.Type.Define(new ExtensionInfo<EventSequence>());
                Schema.eventSequenceType.Type.Define(new ExtensionInfo<ReferenceValidator>());
                Schema.eventSequenceType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
                Schema.eventSequenceType.Type.Define(new ExtensionInfo<DomNodeQueryable>());

                Schema.eventType.Type.Define(new ExtensionInfo<Event>());
                Schema.eventType.Type.Define(new ExtensionInfo<EventContext>());

                Schema.resourceType.Type.Define(new ExtensionInfo<Resource>());

                // Set some defaults in a localization-friendly way.
                Schema.eventType.nameAttribute.DefaultValue = "Event".Localize();
                Schema.animationResourceType.nameAttribute.DefaultValue = "Animation".Localize();
                Schema.geometryResourceType.nameAttribute.DefaultValue = "Geometry".Localize();


                // Enable metadata driven property editing for events and resources
                var creator = new AdapterCreator<CustomTypeDescriptorNodeAdapter>();
                Schema.eventType.Type.AddAdapterCreator(creator);
                Schema.resourceType.Type.AddAdapterCreator(creator);

                // annotate types with display information for palette

                Schema.eventType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.eventType.Type,
                        (string)Schema.eventType.nameAttribute.DefaultValue,
                        "Event in a sequence".Localize(),
                        Resources.EventImage));

                Schema.animationResourceType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.animationResourceType.Type,
                        (string)Schema.animationResourceType.nameAttribute.DefaultValue,
                        "Animation resource".Localize(),
                        Resources.AnimationImage));

                Schema.geometryResourceType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.geometryResourceType.Type,
                        (string)Schema.geometryResourceType.nameAttribute.DefaultValue,
                        "Geometry resource".Localize(),
                        Resources.GeometryImage));

                // register property descriptors on state, transition, folder types

                Schema.eventType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Name".Localize(),
                                Schema.eventType.nameAttribute,
                                null,
                                "Event name".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Time".Localize(),
                                Schema.eventType.timeAttribute,
                                null,
                                "Event starting time".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Duration".Localize(),
                                Schema.eventType.durationAttribute,
                                null,
                                "Event duration".Localize(),
                                false),
                    }));

                Schema.animationResourceType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Name".Localize(),
                                Schema.animationResourceType.nameAttribute,
                                null,
                                "Animation name".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Size".Localize(),
                                Schema.animationResourceType.sizeAttribute,
                                null,
                                "Size of animation, in bytes".Localize(),
                                true),
                            new AttributePropertyDescriptor(
                                "# Tracks".Localize(),
                                Schema.animationResourceType.tracksAttribute,
                                null,
                                "Number of tracks in animation".Localize(),
                                true),
                            new AttributePropertyDescriptor(
                                "Duration".Localize(),
                                Schema.animationResourceType.durationAttribute,
                                null,
                                "Duration of animation, in milliseconds".Localize(),
                                true),
                            new AttributePropertyDescriptor(
                                "Compressed".Localize(),
                                Schema.animationResourceType.compressedAttribute,
                                null,
                                "Whether or not animation is compressed".Localize(),
                                false,
                                new BoolEditor()),
                    }));

                var primitiveKinds = new string[]
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

                // TODO: Seems like default values for enums should be set automatically by XmlSchemaTypeLoader.
                Schema.geometryResourceType.primitiveTypeAttribute.DefaultValue = primitiveKinds[0];

                Schema.geometryResourceType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Name".Localize(),
                                Schema.geometryResourceType.nameAttribute,
                                null,
                                "Geometry name".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Size".Localize(),
                                Schema.geometryResourceType.sizeAttribute,
                                null,
                                "Size of geometry, in bytes".Localize(),
                                true),
                            new AttributePropertyDescriptor(
                                "# Bones".Localize(),
                                Schema.geometryResourceType.bonesAttribute,
                                null,
                                "Number of bones in geometry".Localize(),
                                true),
                            new AttributePropertyDescriptor(
                                "# Vertices".Localize(),
                                Schema.geometryResourceType.verticesAttribute,
                                null,
                                "Number of vertices in geometry".Localize(),
                                true),
                            new AttributePropertyDescriptor(
                                "Primitive Kind".Localize(),
                                Schema.geometryResourceType.primitiveTypeAttribute,
                                null,
                                "Kind of primitives in geometry".Localize(),
                                false,
                                new EnumUITypeEditor(primitiveKinds),
                                new EnumTypeConverter(primitiveKinds)),
                    }));

                break; // schema only defines one type collection
            }
        }
    }
}
