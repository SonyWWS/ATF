//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using PropertyDescriptor = Sce.Atf.Dom.PropertyDescriptor;

namespace FsmEditorSample
{
    /// <summary>
    /// Loads the FSM schema, registers data extensions on the DOM types, annotates
    /// the types with display information and PropertyDescriptors</summary>
    [Export(typeof(SchemaLoader))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SchemaLoader : XmlSchemaTypeLoader, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        [ImportingConstructor]
        public SchemaLoader(PropertyEditor propertyEditor)
        {
            m_propertyEditor = propertyEditor;
            // set resolver to locate embedded .xsd file
            SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "FsmEditorSample/schemas");
            Load("FSM_customized.xsd");
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // Set custom display options for the 2-column PropertyEditor
            PropertyGridView propertyGridView = m_propertyEditor.PropertyGrid.PropertyGridView;
            if (propertyGridView.CustomizeAttributes != null)
                throw new InvalidOperationException("Someone else set PropertyGridView's CustomizeAttributes already");
            propertyGridView.CustomizeAttributes = new[]
                {
                    new PropertyView.CustomizeAttribute("Triggers".Localize(), horizontalEditorOffset:0, nameHasWholeRow:true),                    
                };
        }

        #endregion

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
        /// Create property descriptors for types.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
            {
                m_namespace = typeCollection.TargetNamespace;
                m_typeCollection = typeCollection;
                Schema.Initialize(typeCollection);

                // register extensions

                // extend FSM root node with FSM, document, editing context, and printing support
                Schema.fsmType.Type.Define(new ExtensionInfo<Fsm>());
                Schema.fsmType.Type.Define(new ExtensionInfo<EditingContext>());
                Schema.fsmType.Type.Define(new ExtensionInfo<ViewingContext>());
                Schema.fsmType.Type.Define(new ExtensionInfo<TransitionRouter>());
                Schema.fsmType.Type.Define(new ExtensionInfo<Document>());
                Schema.fsmType.Type.Define(new ExtensionInfo<PrintableDocument>());

                // extend with adapter to synch multiple histories in document with document "Dirty" flag
                Schema.fsmType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
                // extend with adapter for prototyping
                Schema.fsmType.Type.Define(new ExtensionInfo<PrototypingContext>());
                // extend with adapters to validate references and unique ids
                Schema.fsmType.Type.Define(new ExtensionInfo<ReferenceValidator>());
                Schema.fsmType.Type.Define(new ExtensionInfo<UniqueIdValidator>());

                // define FSM object model
                Schema.prototypeFolderType.Type.Define(new ExtensionInfo<PrototypeFolder>());
                Schema.prototypeType.Type.Define(new ExtensionInfo<Prototype>());
                Schema.stateType.Type.Define(new ExtensionInfo<State>());
                Schema.transitionType.Type.Define(new ExtensionInfo<Transition>());
                Schema.annotationType.Type.Define(new ExtensionInfo<Annotation>());

                // annotate state and annotation types with display information for palette

                Schema.stateType.labelAttribute.DefaultValue = "State".Localize();

                Schema.stateType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.stateType.Type,
                        (string)Schema.stateType.labelAttribute.DefaultValue,
                        "State in a finite state machine".Localize(),
                        Resources.StateImage));

                Schema.annotationType.Type.SetTag(
                    new NodeTypePaletteItem(
                        Schema.annotationType.Type,
                        "Comment".Localize(),
                        "Comment on state machine".Localize(),
                        Resources.AnnotationImage));

                // register property descriptors on state, transition, folder types

                // TransitionType have a collection of child triggers.
                // use EmbeddedCollectionEditor to edit children (edit, add, remove, move).
                // Note: EmbeddedCollectionEditor needs some work (efficiency and implementation issues).                
                var collectionEditor = new EmbeddedCollectionEditor();

                // the following  lambda's handles (add, remove, move ) items.
                collectionEditor.GetItemInsertersFunc = (context) =>
                {
                    var insertors
                        = new EmbeddedCollectionEditor.ItemInserter[1];

                    var list = context.GetValue() as IList<DomNode>;
                    if (list != null)
                    {
                        var childDescriptor
                            = context.Descriptor as ChildPropertyDescriptor;
                        if (childDescriptor != null)
                        {
                            insertors[0] = new EmbeddedCollectionEditor.ItemInserter(childDescriptor.ChildInfo.Type.Name,
                        delegate
                        {
                            DomNode node = new DomNode(childDescriptor.ChildInfo.Type);
                            if (node.Type.IdAttribute != null)
                            {
                                node.SetAttribute(node.Type.IdAttribute, node.Type.Name);
                            }
                            list.Add(node);
                            return node;
                        });
                            return insertors;
                        }
                    }
                    return EmptyArray<EmbeddedCollectionEditor.ItemInserter>.Instance;
                };


                collectionEditor.RemoveItemFunc = (context, item) =>
                {
                    var list = context.GetValue() as IList<DomNode>;
                    if (list != null)
                        list.Remove(item.Cast<DomNode>());
                };


                collectionEditor.MoveItemFunc = (context, item, delta) =>
                {
                    var list = context.GetValue() as IList<DomNode>;
                    if (list != null)
                    {
                        DomNode node = item.Cast<DomNode>();
                        int index = list.IndexOf(node);
                        int insertIndex = index + delta;
                        if (insertIndex < 0 || insertIndex >= list.Count)
                            return;
                        list.RemoveAt(index);
                        list.Insert(insertIndex, node);
                    }

                };

                Schema.stateType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Name".Localize(),
                                Schema.stateType.labelAttribute, // 'nameAttribute' is unique id, label is user visible name
                                null,
                                "State name".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Size".Localize(),
                                Schema.stateType.sizeAttribute,
                                null,
                                "State size".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Hidden".Localize(),
                                Schema.stateType.hiddenAttribute,
                                null,
                                "Whether or not state is hidden".Localize(),
                                false,
                                new BoolEditor()),
                            new AttributePropertyDescriptor(
                                "Start".Localize(),
                                Schema.stateType.startAttribute,
                                null,
                                "Whether or not state is the start state".Localize(),
                                false,
                                new BoolEditor()),
                            new AttributePropertyDescriptor(
                                "Entry Action".Localize(),
                                Schema.stateType.entryActionAttribute,
                                null,
                                "Action performed when entering state".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Action".Localize(),
                                Schema.stateType.actionAttribute,
                                null,
                                "Action performed while in state".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Exit Action".Localize(),
                                Schema.stateType.exitActionAttribute,
                                null,
                                "Action performed when exiting state".Localize(),
                                false),
                    }));

               
                Schema.triggerType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Label".Localize(),
                                Schema.triggerType.labelAttribute,
                                null,
                                "Label displayed on trigger".Localize(),
                                false),                            
                            new AttributePropertyDescriptor(
                                "Id".Localize(),
                                Schema.triggerType.idAttribute,
                                null,
                                "Trigger id".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "Action".Localize(),
                                Schema.triggerType.actionAttribute,
                                null,
                                "Action on trigger".Localize(),
                                false)

                }));



               


                Schema.transitionType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Label".Localize(),
                                Schema.transitionType.labelAttribute,
                                null,
                                "Label displayed on transition".Localize(),
                                false),                            
                            new AttributePropertyDescriptor(
                                "Action".Localize(),
                                Schema.transitionType.actionAttribute,
                                null,
                                "Action performed when making transition".Localize(),
                                false),
                           new ChildPropertyDescriptor(
                               "Triggers".Localize(),
                               Schema.transitionType.triggerChild,
                               null,
                               "List of triggers".Localize(),
                               false,
                               collectionEditor)
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

                Schema.annotationType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Text".Localize(),
                                Schema.annotationType.textAttribute,
                                null,
                                "Comment text".Localize(),
                                false)
                    }));

                // the fsm schema defines only one type collection
                break;
            }
        }

        private readonly PropertyEditor m_propertyEditor;
    }
}
