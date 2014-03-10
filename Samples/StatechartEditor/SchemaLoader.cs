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
    /// Loads the UI schema, registers data extensions on the DOM types, annotates
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
                        Localizer.Localize("Comment"),
                        Localizer.Localize("Comment on a statechart"),
                        Resources.AnnotationImage));

                Schema.reactionType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.reactionType.Type,
                        Localizer.Localize("Reaction"),
                        Localizer.Localize("Reaction"),
                        Resources.ReactionImage));

                Schema.stateType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.stateType.Type,
                        Localizer.Localize("State"),
                        Localizer.Localize("State in a statechart"),
                        Resources.StateImage));

                Schema.startStateType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.startStateType.Type,
                        Localizer.Localize("Start State"),
                        Localizer.Localize("Initial state in a statechart"),
                        Resources.StartImage));

                Schema.conditionalStateType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.conditionalStateType.Type,
                        Localizer.Localize("Conditional State"),
                        Localizer.Localize("State with a condition to reduce the number of transitions"),
                        Resources.ConditionalImage));

                Schema.historyStateType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.historyStateType.Type,
                        Localizer.Localize("History State"),
                        Localizer.Localize("State that restores history"),
                        Resources.HistoryImage));

                Schema.finalStateType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.finalStateType.Type,
                        Localizer.Localize("Final State"),
                        Localizer.Localize("Final state in a statechart"),
                        Resources.FinalImage));

                Schema.reactionType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Event"),
                                Schema.reactionType.eventAttribute,
                                null,
                                Localizer.Localize("Event that triggers reaction"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Guard"),
                                Schema.reactionType.guardAttribute,
                                null,
                                Localizer.Localize("Guard condition required for reaction"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Action"),
                                Schema.reactionType.actionAttribute,
                                null,
                                Localizer.Localize("Action taken"),
                                false),
                    }));

                Schema.stateType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Label"),
                                Schema.stateType.labelAttribute, // 'nameAttribute' is unique id, label is user visible name
                                null,
                                Localizer.Localize("State label"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Entry Action"),
                                Schema.stateType.entryActionAttribute,
                                null,
                                Localizer.Localize("Action taken when state is entered"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Exit Action"),
                                Schema.stateType.exitActionAttribute,
                                null,
                                Localizer.Localize("Action taken when state is exited"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Width"),
                                Schema.stateType.widthAttribute,
                                null,
                                Localizer.Localize("Width of state"),
                                false),
                            new AttributePropertyDescriptor(
                                Localizer.Localize("Height"),
                                Schema.stateType.heightAttribute,
                                null,
                                Localizer.Localize("Height of state"),
                                false),
                    }));

                Schema.historyStateType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                Localizer.Localize("History Type"),
                                Schema.historyStateType.typeAttribute,
                                null,
                                Localizer.Localize("Whether history is shallow or deep"),
                                false,
                                new EnumUITypeEditor(
                                    new string[]
                                    {
                                        Localizer.Localize("Shallow", "Shallow history in a statechart"),
                                        Localizer.Localize("Deep", "Deep history in a statechart")
                                    }))
                    }));

                // the statechart schema defines only one type collection
                break;
            }
        }
    }
}
