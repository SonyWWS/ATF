//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;
using PropertyDescriptor = System.ComponentModel.PropertyDescriptor;

namespace CircuitEditorSample
{
    /// <summary>
    /// Loads the UI schema, registers data extensions on the DOM types and annotates
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
            SchemaResolver = new ResourceStreamResolver(System.Reflection.Assembly.GetExecutingAssembly(), "CircuitEditorSample/schemas");
            Load("Circuit.xsd");
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

                // register extensions

                // decorate circuit document type
                Schema.circuitDocumentType.Type.Define(new ExtensionInfo<CircuitDocument>());                  // document info
                //Schema.circuitDocumentType.Type.Define(new ExtensionInfo<SampleCircuitEditingContext>());                  // document info
                Schema.circuitDocumentType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());    // ties sub-context histories into document dirty bit
                Schema.circuitDocumentType.Type.Define(new ExtensionInfo<PrototypingContext>());        // document-wide prototype hierarchy
                Schema.circuitDocumentType.Type.Define(new ExtensionInfo<TemplatingContext>());         // document-wide template hierarchy
                Schema.circuitDocumentType.Type.Define(new ExtensionInfo<MasteringValidator>());        // validates sub-circuits
                Schema.circuitDocumentType.Type.Define(new ExtensionInfo<UniqueIdValidator>());         // ensures all ids are unique throughout document
                //Schema.circuitDocumentType.Type.Define(new ExtensionInfo<UniquePathIdValidator>());   // TODO: switch to UniquePathIdValidator to ensures all ids are local unique
                Schema.circuitDocumentType.Type.Define(new ExtensionInfo<CircuitValidator>());          // validate group hierarchy
                // ReferenceValidator should be the last validator attached to the root DomNode to fully track
                // all the DOM editings of all other validators to update references properly 
                Schema.circuitDocumentType.Type.Define(new ExtensionInfo<ReferenceValidator>());        // tracks references and targets

                // decorate circuit type
                Schema.circuitType.Type.Define(new ExtensionInfo<GlobalHistoryContext>());
                Schema.circuitType.Type.Define(new ExtensionInfo<ViewingContext>());                    // manages module and circuit bounds, efficient layout
                Schema.circuitType.Type.Define(new ExtensionInfo<LayeringContext>());                   // circuit layer hierarchy
                Schema.circuitType.Type.Define(new ExtensionInfo<PrintableDocument>());                 // printing

                // decorate group type
                Schema.groupType.Type.Define(new ExtensionInfo<CircuitEditingContext>());                    // main editable circuit adapter
                Schema.groupType.Type.Define(new ExtensionInfo<Group>());
                Schema.groupType.Type.Define(new ExtensionInfo<ViewingContext>());

 
                // register the circuit model interfaces
            
                Schema.subCircuitType.Type.Define(new ExtensionInfo<SubCircuit>());

                Schema.subCircuitInstanceType.Type.Define(new ExtensionInfo<SubCircuitInstance>());
                Schema.connectionType.Type.Define(new ExtensionInfo<WireStyleProvider<Module, Connection, ICircuitPin>>());
                                  
                RegisterCircuitExtensions();

                // types are initialized, register property descriptors on module, folder types

                Schema.moduleType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Name".Localize(),
                                Schema.moduleType.labelAttribute, // 'nameAttribute' is unique id, label is user visible name
                                null,
                                "Module name".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "ID".Localize(),
                                Schema.moduleType.nameAttribute, // 'nameAttribute' is unique id, label is user visible name
                                null,
                                "Unique ID".Localize(),
                                true)
                    }));

                Schema.layerFolderType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Name".Localize(),
                                Schema.layerFolderType.nameAttribute,
                                null,
                                "Layer name".Localize(),
                                false)
                    }));

                Schema.prototypeFolderType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Name".Localize(),
                                Schema.prototypeFolderType.nameAttribute,
                                null,
                                "Prototype folder name".Localize(),
                                false)
                    }));           

                // the circuit schema defines only one type collection
                break;
            }
        } 

        static private void RegisterCircuitExtensions()
        {         
            // adapts the default implementation  of circuit types
            Schema.moduleType.Type.Define(new ExtensionInfo<Module>());
            Schema.connectionType.Type.Define(new ExtensionInfo<Connection>());
            Schema.pinType.Type.Define(new ExtensionInfo<Pin>());
            Schema.groupPinType.Type.Define(new ExtensionInfo<GroupPin>());
            Schema.circuitType.Type.Define(new ExtensionInfo<Circuit>());
            Schema.prototypeFolderType.Type.Define(new ExtensionInfo<PrototypeFolder>());
            Schema.prototypeType.Type.Define(new ExtensionInfo<Prototype>());
            Schema.layerFolderType.Type.Define(new ExtensionInfo<LayerFolder>());
            Schema.moduleRefType.Type.Define(new ExtensionInfo<ModuleRef>());
            Schema.annotationType.Type.Define(new ExtensionInfo<Annotation>());
            Schema.circuitType.Type.Define(new ExtensionInfo<CircuitEditingContext>()); // main editable circuit adapter

            Schema.templateFolderType.Type.Define(new ExtensionInfo<TemplateFolder>());
            Schema.templateType.Type.Define(new ExtensionInfo<Template>());
            Schema.moduleTemplateRefType.Type.Define(new ExtensionInfo<ModuleInstance>());
            Schema.groupTemplateRefType.Type.Define(new ExtensionInfo<GroupInstance>());

            // set document editor information(DocumentClientInfo) for circuit editor:
            Schema.circuitDocumentType.Type.SetTag(Editor.EditorInfo);
        }
    }
}
