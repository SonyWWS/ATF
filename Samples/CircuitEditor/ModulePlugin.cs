//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

using PropertyDescriptor = Sce.Atf.Dom.PropertyDescriptor;

namespace CircuitEditorSample
{
    /// <summary>
    /// Component that adds module types to the editor. 
    /// This class adds some sample audio modules.</summary>
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ModulePlugin))]
    public class ModulePlugin : IPaletteClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="paletteService">Palette service</param>
        /// <param name="schemaLoader">Schema loader</param>
        [ImportingConstructor]
        public ModulePlugin(
            IPaletteService paletteService,
            SchemaLoader schemaLoader)
        {
            m_paletteService = paletteService;
            m_schemaLoader = schemaLoader;
        }

        protected SchemaLoader SchemaLoader
        {
            get { return m_schemaLoader; }
        }

        private IPaletteService m_paletteService;
        private SchemaLoader m_schemaLoader;

        /// <summary>
        /// Gets the palette category string for the circuit modules</summary>
        public readonly string PaletteCategory = "Circuits".Localize();

        /// <summary>
        /// Gets drawing resource key for boolean pin types</summary>
        public string BooleanPinTypeName
        {
            get { return BooleanPinType.Name; }
        }

        /// <summary>
        /// Gets boolean pin type</summary>
        public AttributeType BooleanPinType
        {
            get { return AttributeType.BooleanType; }
        }

        /// <summary>
        /// Gets float pin type name</summary>
        public string FloatPinTypeName
        {
            get { return FloatPinType.Name; }
        }

        /// <summary>
        /// Gets float pin type</summary>
        public AttributeType FloatPinType
        {
            get { return AttributeType.FloatType; }
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by adding palette information and defining module types</summary>
        public virtual void Initialize()
        {
            // add palette info to annotation type, and register with palette
            var annotationItem = new NodeTypePaletteItem(
                Schema.annotationType.Type,
                "Comment".Localize(),
                "Create a moveable resizable comment on the circuit canvas".Localize(),
                Resources.AnnotationImage);

            m_paletteService.AddItem(
                annotationItem, "Misc".Localize("abbreviation for miscellaneous"), this);

            // define editable properties on annotation
            Schema.annotationType.Type.SetTag(
                new PropertyDescriptorCollection(
                    new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Text".Localize(),
                                Schema.annotationType.textAttribute,
                                null,
                                "Comment Text".Localize(),
                                false),
                            new AttributePropertyDescriptor(
                                "BackColor".Localize(),  // name
                                Schema.annotationType.backcolorAttribute, //AttributeInfo
                                null, // category
                                "Comment background color".Localize(), //description
                                false, //isReadOnly
                                new Sce.Atf.Controls.PropertyEditing.ColorEditor(), // editor
                                new Sce.Atf.Controls.PropertyEditing.IntColorConverter() // typeConverter
                                ),
                           new AttributePropertyDescriptor(
                                "ForeColor".Localize(),  // name
                                Schema.annotationType.foreColorAttribute, //AttributeInfo
                                null, // category
                                "Comment foreground color".Localize(), //description
                                false, //isReadOnly
                                new Sce.Atf.Controls.PropertyEditing.ColorEditor(), // editor
                                new Sce.Atf.Controls.PropertyEditing.IntColorConverter() // typeConverter
                                ),
                   }));

            // define module types

            DefineModuleType(
                new XmlQualifiedName("buttonType", Schema.NS),
                "Button".Localize(),
                "On/Off Button".Localize(),
                Resources.ButtonImage,
                EmptyArray<ElementType.Pin>.Instance,
                new ElementType.Pin[]
                {
                    new ElementType.Pin("Out".Localize(), BooleanPinTypeName, 0)
                },
                m_schemaLoader);

            DefineModuleType(
                new XmlQualifiedName("lightType", Schema.NS),
                "Light".Localize(),
                "Light source".Localize(),
                Resources.LightImage,
                new ElementType.Pin[]
                {
                    new ElementType.Pin("In".Localize(), BooleanPinTypeName, 0)
                },
                EmptyArray<ElementType.Pin>.Instance,
                m_schemaLoader);

            DomNodeType speakerNodeType = DefineModuleType(
                new XmlQualifiedName("speakerType", Schema.NS),
                "Speaker".Localize("an electronic speaker, for playing sounds"),
                "Speaker".Localize("an electronic speaker, for playing sounds"),
                Resources.SpeakerImage,
                new ElementType.Pin[]
                {
                    new ElementType.Pin("In".Localize(), FloatPinTypeName, 0),
                },
                EmptyArray<ElementType.Pin>.Instance,
                m_schemaLoader);
            var speakerManufacturerInfo = new AttributeInfo("Manufacturer".Localize(), AttributeType.StringType);
            speakerNodeType.Define(speakerManufacturerInfo);
            speakerNodeType.SetTag(
                new PropertyDescriptorCollection(
                    new PropertyDescriptor[] {
                        new AttributePropertyDescriptor(
                            "Manufacturer".Localize(),
                            speakerManufacturerInfo,
                            null, //category
                            "Manufacturer".Localize(), //description
                            false) //is not read-only
                    }));

            DefineModuleType(
                new XmlQualifiedName("soundType", Schema.NS),
                "Sound".Localize(),
                "Sound Player".Localize(),
                Resources.SoundImage,
                new ElementType.Pin[]
                {
                    new ElementType.Pin("On".Localize(), "boolean", 0),
                    new ElementType.Pin("Reset".Localize(), "boolean", 1),
                    new ElementType.Pin("Pause".Localize(), "boolean", 2),
                },
                new ElementType.Pin[]
                {
                    new ElementType.Pin("Out".Localize(), "float", 0),
                },
                m_schemaLoader);

            DefineModuleType(
                new XmlQualifiedName("andType", Schema.NS),
                "And".Localize(),
                "Logical AND".Localize(),
                Resources.AndImage,
                new ElementType.Pin[]
                {
                    new ElementType.Pin("In1".Localize("input pin #1"), "boolean", 0),
                    new ElementType.Pin("In2".Localize("input pin #2"), "boolean", 1),
                },
                new ElementType.Pin[]
                {
                    new ElementType.Pin("Out".Localize(), "boolean", 0),
                },
                m_schemaLoader);

            DefineModuleType(
                new XmlQualifiedName("orType", Schema.NS),
                "Or".Localize(),
                "Logical OR".Localize(),
                Resources.OrImage,
                new ElementType.Pin[]
                {
                    new ElementType.Pin("In1".Localize("input pin #1"), "boolean", 0),
                    new ElementType.Pin("In2".Localize("input pin #2"), "boolean", 1),
                },
                new ElementType.Pin[]
                {
                    new ElementType.Pin("Out".Localize(), "boolean", 0),
                },
                m_schemaLoader);

            DefineModuleType(
                new XmlQualifiedName("16To1MultiplexerType", Schema.NS),
                "16-to-1 Multiplexer".Localize(),
                "16-to-1 Multiplexer".Localize(),
                Resources.AndImage,
                new ElementType.Pin[]
                {
                    new ElementType.Pin("In1".Localize(), "boolean", 0),
                    new ElementType.Pin("In2".Localize(), "boolean", 1),
                    new ElementType.Pin("In3".Localize(), "boolean", 2),
                    new ElementType.Pin("In4".Localize(), "boolean", 3),
                    new ElementType.Pin("In5".Localize(), "boolean", 4),
                    new ElementType.Pin("In6".Localize(), "boolean", 5),
                    new ElementType.Pin("In7".Localize(), "boolean", 6),
                    new ElementType.Pin("In8".Localize(), "boolean", 7),
                    new ElementType.Pin("In9".Localize(), "boolean", 8),
                    new ElementType.Pin("In10".Localize(), "boolean", 9),
                    new ElementType.Pin("In11".Localize(), "boolean", 10),
                    new ElementType.Pin("In12".Localize(), "boolean", 11),
                    new ElementType.Pin("In13".Localize(), "boolean", 12),
                    new ElementType.Pin("In14".Localize(), "boolean", 13),
                    new ElementType.Pin("In15".Localize(), "boolean", 14),
                    new ElementType.Pin("In16".Localize(), "boolean", 15),
                    new ElementType.Pin("Select1".Localize(), "boolean", 16),
                    new ElementType.Pin("Select2".Localize(), "boolean", 17),
                    new ElementType.Pin("Select3".Localize(), "boolean", 18),
                    new ElementType.Pin("Select4".Localize(), "boolean", 19),
                },
                new ElementType.Pin[]
                {
                    new ElementType.Pin("Out".Localize(), "boolean", 0),
                },
                m_schemaLoader);

            DefineModuleType(
                new XmlQualifiedName("1To16DemultiplexerType", Schema.NS),
                "1-to-16 Demultiplexer".Localize(),
                "1-to-16 Demultiplexer".Localize(),
                Resources.OrImage,
                new ElementType.Pin[]
                {
                    new ElementType.Pin("Data".Localize(), "boolean", 0),
                    new ElementType.Pin("Select1".Localize(), "boolean", 1),
                    new ElementType.Pin("Select2".Localize(), "boolean", 2),
                    new ElementType.Pin("Select3".Localize(), "boolean", 3),
                    new ElementType.Pin("Select4".Localize(), "boolean", 4),
                },
                new ElementType.Pin[]
                {
                    new ElementType.Pin("Out1".Localize(), "boolean", 0),
                    new ElementType.Pin("Out2".Localize(), "boolean", 1),
                    new ElementType.Pin("Out3".Localize(), "boolean", 2),
                    new ElementType.Pin("Out4".Localize(), "boolean", 3),
                    new ElementType.Pin("Out5".Localize(), "boolean", 4),
                    new ElementType.Pin("Out6".Localize(), "boolean", 5),
                    new ElementType.Pin("Out7".Localize(), "boolean", 6),
                    new ElementType.Pin("Out8".Localize(), "boolean", 7),
                    new ElementType.Pin("Out9".Localize(), "boolean", 8),
                    new ElementType.Pin("Out10".Localize(), "boolean", 9),
                    new ElementType.Pin("Out11".Localize(), "boolean", 10),
                    new ElementType.Pin("Out12".Localize(), "boolean", 11),
                    new ElementType.Pin("Out13".Localize(), "boolean", 12),
                    new ElementType.Pin("Out14".Localize(), "boolean", 13),
                    new ElementType.Pin("Out15".Localize(), "boolean", 14),
                    new ElementType.Pin("Out16".Localize(), "boolean", 15),
                },
                m_schemaLoader);
        }

        #endregion

        #region IPaletteClient Members

        /// <summary>
        /// Gets display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Info object, which client can fill out</param>
        void IPaletteClient.GetInfo(object item, ItemInfo info)
        {
            var paletteItem = (NodeTypePaletteItem)item;
            if (paletteItem != null)
            {
                info.Label = paletteItem.Name;
                info.Description = paletteItem.Description;
                info.ImageIndex = info.GetImageList().Images.IndexOfKey(paletteItem.ImageName);
                info.HoverText = paletteItem.Description;
            }
        }

        /// <summary>
        /// Converts the palette item into an object that can be inserted into an IInstancingContext</summary>
        /// <param name="item">Item to convert</param>
        /// <returns>Object that can be inserted into an IInstancingContext</returns>
        object IPaletteClient.Convert(object item)
        {
            var paletteItem = (NodeTypePaletteItem)item;
            var node = new DomNode(paletteItem.NodeType);
            if (paletteItem.NodeType.IdAttribute != null)
                node.SetAttribute(paletteItem.NodeType.IdAttribute, paletteItem.Name);
            return node;
        }

        #endregion

        /// <summary>
        /// Prepare metadata for the module type, to be used by the palette and circuit drawing engine</summary>
        /// <param name="name"> Schema full name of the DomNodeType for the module type</param>
        /// <param name="displayName">Display name for the module type</param>
        /// <param name="description"></param>
        /// <param name="imageName">Image name </param>
        /// <param name="inputs">Define input pins for the module type</param>
        /// <param name="outputs">Define output pins for the module type</param>
        /// <param name="loader">XML schema loader </param>
        /// <param name="domNodeType">optional DomNode type for the module type</param>
        /// <returns>DomNodeType that was created (or the domNodeType parameter, if it wasn't null)</returns>
        protected DomNodeType DefineModuleType(
            XmlQualifiedName name,
            string displayName,
            string description,
            string imageName,
            ElementType.Pin[] inputs,
            ElementType.Pin[] outputs,
            SchemaLoader loader,
            DomNodeType domNodeType = null)
        {
            if (domNodeType == null)
                // create the type
                domNodeType = new DomNodeType(
                name.ToString(),
                Schema.moduleType.Type,
                EmptyArray<AttributeInfo>.Instance,
                EmptyArray<ChildInfo>.Instance,
                EmptyArray<ExtensionInfo>.Instance);

            DefineCircuitType(domNodeType, displayName, imageName, inputs, outputs);

            // add it to the schema-defined types
            loader.AddNodeType(name.ToString(), domNodeType);

            // add the type to the palette
            m_paletteService.AddItem(
                new NodeTypePaletteItem(
                    domNodeType,
                    displayName,
                    description,
                    imageName),
                PaletteCategory,
                this);

            return domNodeType;
        }

        private void DefineCircuitType(
            DomNodeType type,
            string elementTypeName,
            string imageName,
            ICircuitPin[] inputs,
            ICircuitPin[] outputs)
        {
            // create an element type and add it to the type metadata
            // For now, let all circuit elements be used as 'connectors' which means
            //  that their pins will be used to create the pins on a master instance.
            bool isConnector = true; //(inputs.Length + outputs.Length) == 1;
            var image = string.IsNullOrEmpty(imageName) ? null : ResourceUtil.GetImage32(imageName);
            type.SetTag<ICircuitElementType>(
                new ElementType(
                    elementTypeName,
                    isConnector,
                    new Size(),
                    image,
                    inputs,
                    outputs));


        }
    }
}
