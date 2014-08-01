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

namespace WinGuiCommon
{
    /// <summary>
    /// Loads the event schema, registers data extensions on the DOM types, and annotates
    /// the types with display information and PropertyDescriptors</summary>
    [Export(typeof(SchemaLoader))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SchemaLoader : XmlSchemaTypeLoader
    {
        /// <summary>
        /// Constructor</summary>
        public SchemaLoader()
        {
            // set resolver to locate embedded .xsd file
            SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "WinGuiCommon/schemas");
            Load("sampleAppData.xsd");
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
        /// Defines DOM adapters for types. Adds information for palette to types.
        /// Adds PropertyDescriptors to types for property editors.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
            {
                m_namespace = typeCollection.TargetNamespace;
                m_typeCollection = typeCollection;
                Schema.Initialize(typeCollection);

                // register extensions
                Schema.winGuiCommonDataType.Type.Define(new ExtensionInfo<WinGuiCommonDataDocument>());
                Schema.winGuiCommonDataType.Type.Define(new ExtensionInfo<WinGuiCommonDataContext>());
                Schema.winGuiCommonDataType.Type.Define(new ExtensionInfo<WinGuiWpfDataDocument>());
                Schema.winGuiCommonDataType.Type.Define(new ExtensionInfo<WinGuiWpfDataContext>());
                Schema.winGuiCommonDataType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
                Schema.winGuiCommonDataType.Type.Define(new ExtensionInfo<WinGuiCommonData>());
                Schema.winGuiCommonDataType.Type.Define(new ExtensionInfo<ReferenceValidator>());
                Schema.winGuiCommonDataType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
                Schema.winGuiCommonDataType.Type.Define(new ExtensionInfo<DomNodeQueryable>());

                Schema.eventType.Type.Define(new ExtensionInfo<WinGuiCommon.Event>());
                Schema.eventType.Type.Define(new ExtensionInfo<EventContext>());

                Schema.resourceType.Type.Define(new ExtensionInfo<WinGuiCommon.Resource>());

                // Enable metadata driven property editing for events and resources
                AdapterCreator<CustomTypeDescriptorNodeAdapter> creator =
                    new AdapterCreator<CustomTypeDescriptorNodeAdapter>();
                Schema.eventType.Type.AddAdapterCreator(creator);
                Schema.resourceType.Type.AddAdapterCreator(creator);

                // annotate types with display information for palette

                Schema.eventType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.eventType.Type,
                        Localizer.Localize("Event"),
                        Localizer.Localize("Event in a sequence"),
                        WinGuiCommon.Resources.EventImage));

                Schema.animationResourceType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.animationResourceType.Type,
                        Localizer.Localize("Animation"),
                        Localizer.Localize("Animation resource"),
                        WinGuiCommon.Resources.AnimationImage));

                Schema.geometryResourceType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.geometryResourceType.Type,
                        Localizer.Localize("Geometry"),
                        Localizer.Localize("Geometry resource"),
                        WinGuiCommon.Resources.GeometryImage));

                // register property descriptors on state, transition, folder types

                Schema.eventType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Name"),
                                Schema.eventType.nameAttribute,
                                null,
                                Localizer.Localize("Event name"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Time"),
                                Schema.eventType.timeAttribute,
                                null,
                                Localizer.Localize("Event starting time"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Duration"),
                                Schema.eventType.durationAttribute,
                                null,
                                Localizer.Localize("Event duration"),
                                false),
                    }));

                Schema.animationResourceType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Name"),
                                Schema.animationResourceType.nameAttribute,
                                null,
                                Localizer.Localize("Animation name"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Size"),
                                Schema.animationResourceType.sizeAttribute,
                                null,
                                Localizer.Localize("Size of animation, in bytes"),
                                true),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("# Tracks"),
                                Schema.animationResourceType.tracksAttribute,
                                null,
                                Localizer.Localize("Number of tracks in animation"),
                                true),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Duration"),
                                Schema.animationResourceType.durationAttribute,
                                null,
                                Localizer.Localize("Duration of animation, in milliseconds"),
                                true),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Compressed"),
                                Schema.animationResourceType.compressedAttribute,
                                null,
                                Localizer.Localize("Whether or not animation is compressed"),
                                false,
                                new BoolEditor()),
                    }));

                string[] primitiveKinds = new string[]
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

                Schema.geometryResourceType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Name"),
                                Schema.geometryResourceType.nameAttribute,
                                null,
                                Localizer.Localize("Geometry name"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Size"),
                                Schema.geometryResourceType.sizeAttribute,
                                null,
                                Localizer.Localize("Size of geometry, in bytes"),
                                true),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("# Bones"),
                                Schema.geometryResourceType.bonesAttribute,
                                null,
                                Localizer.Localize("Number of bones in geometry"),
                                true),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("# Vertices"),
                                Schema.geometryResourceType.verticesAttribute,
                                null,
                                Localizer.Localize("Number of vertices in geometry"),
                                true),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Primitive Kind"),
                                Schema.geometryResourceType.verticesAttribute,
                                null,
                                Localizer.Localize("Kind of primitives in geometry"),
                                true,
                                new EnumUITypeEditor(primitiveKinds),
                                new EnumTypeConverter(primitiveKinds)),
                    }));

                break; // schema only defines one type collection
            }
        }
    }
}
