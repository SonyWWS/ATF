//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using TimelineEditorSample.DomNodeAdapters;

namespace TimelineEditorSample
{
    /// <summary>
    /// Loads the UI schema, registers data extensions on the DOM types and annotates
    /// the types with display information and PropertyDescriptors</summary>
    public class SchemaLoader : XmlSchemaTypeLoader
    {
        /// <summary>
        /// Constructor</summary>
        public SchemaLoader()
        {
            // set resolver to locate embedded .xsd file
            SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "TimelineEditorSample.schemas");
            Load("timeline.xsd");
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
        /// not been called on the DomNodeTypes. Is called shortly before OnDomNodeTypesFrozen.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
            {
                m_namespace = typeCollection.TargetNamespace;
                m_typeCollection = typeCollection;
                Schema.Initialize(typeCollection);

                // There's no document type, so we will use the timeline type.
                Schema.timelineType.Type.Define(new ExtensionInfo<TimelineDocument>());
                Schema.timelineType.Type.Define(new ExtensionInfo<TimelineContext>());
                Schema.timelineType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());

                // register extensions
                Schema.timelineType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
                Schema.timelineType.Type.Define(new ExtensionInfo<ReferenceValidator>());
                Schema.timelineType.Type.Define(new ExtensionInfo<TimelineValidator>());

                // register the timeline model interfaces
                Schema.timelineType.Type.Define(new ExtensionInfo<Timeline>());
                Schema.groupType.Type.Define(new ExtensionInfo<Group>());
                Schema.trackType.Type.Define(new ExtensionInfo<Track>());
                Schema.intervalType.Type.Define(new ExtensionInfo<Interval>());
                Schema.eventType.Type.Define(new ExtensionInfo<BaseEvent>());
                Schema.keyType.Type.Define(new ExtensionInfo<Key>());
                Schema.markerType.Type.Define(new ExtensionInfo<Marker>());
                Schema.timelineRefType.Type.Define(new ExtensionInfo<TimelineReference>());

                // the timeline schema defines only one type collection
                break;
            }
        }

        /// <summary>
        /// Parses annotations in schema sets. Override this to handle custom annotations.
        /// Supports annotations for property descriptors and palette items.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        /// <param name="annotations">Dictionary of annotations in schema</param>
        protected override void ParseAnnotations(
            XmlSchemaSet schemaSet,
            IDictionary<NamedMetadata, IList<XmlNode>> annotations)
        {
            base.ParseAnnotations(schemaSet, annotations);

            IList<XmlNode> xmlNodes;

            foreach (DomNodeType nodeType in m_typeCollection.GetNodeTypes())
            {
                // parse XML annotation for property descriptors
                if (annotations.TryGetValue(nodeType, out xmlNodes))
                {
                    PropertyDescriptorCollection propertyDescriptors = Sce.Atf.Dom.PropertyDescriptor.ParseXml(nodeType, xmlNodes);

                    // Customizations
                    // The flags and enum support from annotation used to be in ATF 2.8.
                    //  Please request this feature from the ATF team if you need it and a ParseXml overload
                    //  can probably be created.
                    System.ComponentModel.PropertyDescriptor gameFlow = propertyDescriptors["Special Event"];
                    if (gameFlow != null)
                    {
                        FlagsUITypeEditor editor = (FlagsUITypeEditor)gameFlow.GetEditor(typeof(FlagsUITypeEditor));
                        editor.DefineFlags( new string[] {
                            "Reward==Give player the reward",
                            "Trophy==Give player the trophy",
                            "LevelUp==Level up",
                            "BossDies==Boss dies",
                            "PlayerDies==Player dies",
                            "EndCinematic==End cinematic",
                            "EndGame==End game",
                         });
                    }

                    nodeType.SetTag<PropertyDescriptorCollection>(propertyDescriptors);

                    // parse type annotation to create palette items
                    XmlNode xmlNode = FindElement(xmlNodes, "scea.dom.editors");
                    if (xmlNode != null)
                    {
                        string menuText = FindAttribute(xmlNode, "menuText");
                        if (menuText != null) // must have menu text and category
                        {
                            string description = FindAttribute(xmlNode, "description");
                            string image = FindAttribute(xmlNode, "image");
                            NodeTypePaletteItem item = new NodeTypePaletteItem(nodeType, menuText, description, image);
                            nodeType.SetTag<NodeTypePaletteItem>(item);
                        }
                    }
                }
            }
        }
    }
}
