//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using PropertyDescriptor = Sce.Atf.Dom.PropertyDescriptor;

namespace StatechartEditorSample
{
    /// <summary>
    /// Loads the UI schema, registers data extensions on the DOM types, and annotates
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
            SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "StatechartEditorSample/schemas");
            Load("Statechart.xsd");
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
        /// Defines DOM adapters for types. Adds palette and PropertyDescriptor information to DomNoteType objects.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
            {
                m_namespace = typeCollection.TargetNamespace;
                m_typeCollection = typeCollection;
                Schema.Initialize(typeCollection);

                // register extensions

                // on the document type, register editing context, layout context, validator, unique id,
                // and reference integrity tracking
                Schema.statechartDocumentType.Type.Define(new ExtensionInfo<Document>());
                Schema.statechartDocumentType.Type.Define(new ExtensionInfo<ViewingContext>());
                Schema.statechartDocumentType.Type.Define(new ExtensionInfo<PrintableDocument>());
                Schema.statechartDocumentType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
                Schema.statechartDocumentType.Type.Define(new ExtensionInfo<EditingContext>());
                Schema.statechartDocumentType.Type.Define(new ExtensionInfo<BoundsValidator>());
                Schema.statechartDocumentType.Type.Define(new ExtensionInfo<StatechartValidator>());
                Schema.statechartDocumentType.Type.Define(new ExtensionInfo<PrototypingContext>());
                Schema.statechartDocumentType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
                Schema.statechartDocumentType.Type.Define(new ExtensionInfo<ReferenceValidator>());
                Schema.statechartDocumentType.Type.Define(new ExtensionInfo<LockingValidator>());

                // register the statechart model interfaces
                Schema.prototypeFolderType.Type.Define(new ExtensionInfo<PrototypeFolder>());
                Schema.prototypeType.Type.Define(new ExtensionInfo<Prototype>());
                Schema.annotationType.Type.Define(new ExtensionInfo<Annotation>());
                Schema.statechartType.Type.Define(new ExtensionInfo<Statechart>());
                Schema.stateType.Type.Define(new ExtensionInfo<State>());
                Schema.conditionalStateType.Type.Define(new ExtensionInfo<ConditionalState>());
                Schema.finalStateType.Type.Define(new ExtensionInfo<FinalState>());
                Schema.historyStateType.Type.Define(new ExtensionInfo<HistoryState>());
                Schema.reactionType.Type.Define(new ExtensionInfo<Reaction>());
                Schema.startStateType.Type.Define(new ExtensionInfo<StartState>());
                Schema.transitionType.Type.Define(new ExtensionInfo<Transition>());

                // types are initialized, annotate them with palette information

                Schema.annotationType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.annotationType.Type,
                        "Comment".Localize(),
                        "Comment on a statechart".Localize(),
                        Resources.AnnotationImage));

                Schema.reactionType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.reactionType.Type,
                        "Reaction".Localize(),
                        "Reaction".Localize(),
                        Resources.ReactionImage));

                Schema.stateType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.stateType.Type,
                        "State".Localize(),
                        "State in a statechart".Localize(),
                        Resources.StateImage));

                Schema.startStateType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.startStateType.Type,
                        "Start State".Localize(),
                        "Initial state in a statechart".Localize(),
                        Resources.StartImage));

                Schema.conditionalStateType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.conditionalStateType.Type,
                        "Conditional State".Localize(),
                        "State with a condition to reduce the number of transitions".Localize(),
                        Resources.ConditionalImage));

                Schema.historyStateType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.historyStateType.Type,
                        "History State".Localize(),
                        "State that restores history".Localize(),
                        Resources.HistoryImage));

                Schema.finalStateType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.finalStateType.Type,
                        "Final State".Localize(),
                        "Final state in a statechart".Localize(),
                        Resources.FinalImage));

                Schema.reactionType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Event".Localize(),
                                Schema.reactionType.eventAttribute,
                                null,
                                "Event that triggers reaction".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Guard".Localize(),
                                Schema.reactionType.guardAttribute,
                                null,
                                "Guard condition required for reaction".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Action".Localize(),
                                Schema.reactionType.actionAttribute,
                                null,
                                "Action taken".Localize(),
                                false),
                    }));

                Schema.stateType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Label".Localize(),
                                Schema.stateType.labelAttribute, // 'nameAttribute' is unique id, label is user visible name
                                null,
                                "State label".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Entry Action".Localize(),
                                Schema.stateType.entryActionAttribute,
                                null,
                                "Action taken when state is entered".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Exit Action".Localize(),
                                Schema.stateType.exitActionAttribute,
                                null,
                                "Action taken when state is exited".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Width".Localize(),
                                Schema.stateType.widthAttribute,
                                null,
                                "Width of state".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Height".Localize(),
                                Schema.stateType.heightAttribute,
                                null,
                                "Height of state".Localize(),
                                false),
                    }));

                Schema.historyStateType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "History Type".Localize(),
                                Schema.historyStateType.typeAttribute,
                                null,
                                "Whether history is shallow or deep".Localize(),
                                false,
                                new EnumUITypeEditor(
                                    new string[]
                                    {
                                        "Shallow".Localize("Shallow history in a statechart"),
                                        "Deep".Localize("Deep history in a statechart")
                                    }))
                    }));

                // the statechart schema defines only one type collection
                break;
            }
        }
    }
}
