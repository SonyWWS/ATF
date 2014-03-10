//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using PropertyDescriptor = Sce.Atf.Dom.PropertyDescriptor;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Loads the UI schema, registers data extensions on the DOM types, annotates
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
            SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "DomTreeEditorSample/Schemas");
            Load("UISchema.xsd");
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
                UISchema.Initialize(typeCollection);
                
                // register UI adapters as extensions on the DOM data

                // register adapters on the root to define document and editing context
                UISchema.UIType.Type.Define(new ExtensionInfo<UI>());
                UISchema.UIType.Type.Define(new ExtensionInfo<EditingContext>());
                UISchema.UIType.Type.Define(new ExtensionInfo<TreeView>());
                UISchema.UIType.Type.Define(new ExtensionInfo<Document>());

                // register adapters on the root for data validation
                UISchema.UIType.Type.Define(new ExtensionInfo<Validator>());            // makes sure referenced resources are in package
                                                                                        //  this must be first so unique naming can work on copied resources
                UISchema.UIType.Type.Define(new ExtensionInfo<ReferenceValidator>());   // prevents dangling references
                UISchema.UIType.Type.Define(new ExtensionInfo<UniqueIdValidator>());    // makes sure ref targets have unique ids

                // register adapters to define the UI object model
                UISchema.UIPackageType.Type.Define(new ExtensionInfo<UIPackage>());
                UISchema.UIFormType.Type.Define(new ExtensionInfo<UIForm>());
                UISchema.UIShaderType.Type.Define(new ExtensionInfo<UIShader>());
                UISchema.UITextureType.Type.Define(new ExtensionInfo<UITexture>());
                UISchema.UIFontType.Type.Define(new ExtensionInfo<UIFont>());
                UISchema.UISpriteType.Type.Define(new ExtensionInfo<UISprite>());
                UISchema.UITextItemType.Type.Define(new ExtensionInfo<UITextItem>());
                UISchema.UIRefType.Type.Define(new ExtensionInfo<UIRef>());
                UISchema.UIAnimationType.Type.Define(new ExtensionInfo<UIAnimation>());
                UISchema.curveType.Type.Define(new ExtensionInfo<Curve>());
                UISchema.controlPointType.Type.Define(new ExtensionInfo<ControlPoint>());
                
                // tag UI types with display info; it will be used both in the palette
                //  and in the UITreeLister tree view.

                UISchema.UIType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UIType.Type,
                        Localizer.Localize("UI"),
                        Localizer.Localize("UI Container"),
                        "Sce.Atf.Resources.Data16.png"));

                UISchema.UIPackageType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UIPackageType.Type,
                        Localizer.Localize("Package"),
                        Localizer.Localize("Package, containing a complete collection of UI items"),
                        "DomTreeEditorSample.Resources.package.png"));

                UISchema.UIFormType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UIFormType.Type,
                        Localizer.Localize("Form"),
                        Localizer.Localize("Form, containing a UI screen"),
                        "DomTreeEditorSample.Resources.form.png"));

                UISchema.UISpriteType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UISpriteType.Type,
                        Localizer.Localize("Sprite"),
                        Localizer.Localize("A moveable UI element"),
                        "DomTreeEditorSample.Resources.sprite.png"));

                UISchema.UIShaderType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UIShaderType.Type,
                        Localizer.Localize("Shader"),
                        Localizer.Localize("Shader for rendering graphics"),
                        "DomTreeEditorSample.Resources.shader.png"));

                UISchema.UITextureType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UITextureType.Type,
                        Localizer.Localize("Texture"),
                        Localizer.Localize("Texture for rendering graphics"),
                        "DomTreeEditorSample.Resources.texture.png"));

                UISchema.UIFontType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UIFontType.Type,
                        Localizer.Localize("Font"),
                        Localizer.Localize("Font for rendering text"),
                        "DomTreeEditorSample.Resources.font.png"));

                UISchema.UITextItemType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UITextItemType.Type,
                        Localizer.Localize("Text"),
                        Localizer.Localize("Text, to display on Form"),
                        "DomTreeEditorSample.Resources.text.png"));

                UISchema.UITransformType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UITransformType.Type,
                        Localizer.Localize("Transform"),
                        Localizer.Localize("Transform, for sprites and text"),
                        "DomTreeEditorSample.Resources.transform.png"));

                UISchema.UIAnimationType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UIAnimationType.Type,
                        Localizer.Localize("Animation"),
                        Localizer.Localize("Color and position animation curves, for form and control"),
                        null)                        
                );

                UISchema.curveType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.curveType.Type,
                        Localizer.Localize("Curve"),
                        Localizer.Localize("animation curve"),
                        null));

                // Tag UI types with descriptors for property editing

                //UISchema.UIObjectType.Type.SetTag(
                //    new PropertyDescriptorCollection(
                //        new PropertyDescriptor[] {
                //        new AttributePropertyDescriptor(
                //            Localizer.Localize("Name"),
                //            UISchema.UIObjectType.nameAttribute,
                //            null,
                //            Localizer.Localize("Item name"),
                //            false)
                //    }));

                //UISchema.UIPackageType.Type.SetTag(
                //    new PropertyDescriptorCollection(
                //        new PropertyDescriptor[] {
                //        new AttributePropertyDescriptor(
                //            Localizer.Localize("Package"),
                //            UISchema.UIPackageType.FxFileAttribute,
                //            null,
                //            Localizer.Localize("Shader file path"),
                //            false,
                //            new CollectionEditor())
                //    }));

                UISchema.UIShaderType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                        new AttributePropertyDescriptor(
                            Localizer.Localize("Shader"),
                            UISchema.UIShaderType.FxFileAttribute,
                            null,
                            Localizer.Localize("Shader file path"),
                            false,
                            new FileUriEditor("Fx Files (*.fx)|*.fx")),
                        new AttributePropertyDescriptor(
                            Localizer.Localize("Shader param"),
                            UISchema.UIShaderType.ShaderParamAttribute,
                            null,
                            Localizer.Localize("Shader param"),
                            false,
                            new NumericEditor(typeof(Int32)))
                    }));

                UISchema.UITextureType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                        new AttributePropertyDescriptor(
                            Localizer.Localize("Texture file"),
                            UISchema.UITextureType.TextureFileAttribute,
                            null,
                            Localizer.Localize("Texture file path"),
                            false,
                            new FileUriEditor("Texture Files (*.tga)|*.tga")),
                        new AttributePropertyDescriptor(
                            Localizer.Localize("Texture folder"),
                            UISchema.UITextureType.TextureFolderAttribute,
                            null,
                            Localizer.Localize("Texture folder path"),
                            false,
                            new FolderBrowserDialogUITypeEditor("Texture folder path")),
                        new AttributePropertyDescriptor(
                            Localizer.Localize("Texture numbers"),
                            UISchema.UITextureType.TextureArrayAttribute,
                            null,
                            Localizer.Localize("Texture number array"),
                            false,
                            new ArrayEditor(),
                            new FloatArrayConverter()),
                        new AttributePropertyDescriptor(
                            Localizer.Localize("Texture revision date"),
                            UISchema.UITextureType.TextureRevDateAttribute,
                            null,
                            Localizer.Localize("Texture revision date"),
                            false,
                            new DateTimeEditor()),
                    }));

                UISchema.UIFontType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                        new AttributePropertyDescriptor(
                            Localizer.Localize("Font"),
                            UISchema.UIFontType.FontFileAttribute,
                            null,
                            Localizer.Localize("Font file path"),
                            false,
                            new FileUriEditor("Font Files (*.ttf)|*.ttf")),
                        new AttributePropertyDescriptor(
                            Localizer.Localize("Font parameters"),
                            UISchema.UIFontType.FontParamsAttribute,
                            null,
                            Localizer.Localize("Font parameters"),
                            false,
                            new CollectionEditor()),
                    }));

                UISchema.UIAnimationType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                        new AttributePropertyDescriptor(
                            Localizer.Localize("Animation transform"),
                            UISchema.UIAnimationType.AnimationTransformAttribute,
                            null,
                            Localizer.Localize("Animation transform"),
                            false,
                            new NumericMatrixEditor(typeof(float), 3, 3)),
                        new AttributePropertyDescriptor(
                            Localizer.Localize("Animal kinds"),
                            UISchema.UIAnimationType.AnimalKindsAttribute,
                            null,
                            Localizer.Localize("Kinds of animal to animate"),
                            false,
                            new CollectionEditor()),
                    }));

                UISchema.UIControlType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[]
                    {
                        new ChildAttributePropertyDescriptor(
                            Localizer.Localize("Translation"),
                            UISchema.UITransformType.TranslateAttribute,
                            UISchema.UIControlType.TransformChild,
                            null,
                            Localizer.Localize("Item position"),
                            false,
                            new NumericTupleEditor(typeof(float), new string[] { "X", "Y", "Z" }),
                            new FloatArrayConverter()),
                        new ChildAttributePropertyDescriptor(
                            Localizer.Localize("Rotation"),
                            UISchema.UITransformType.RotateAttribute,
                            UISchema.UIControlType.TransformChild,
                            null,
                            Localizer.Localize("Item rotation"),
                            false,
                            new NumericTupleEditor(typeof(float), new string[] { "X", "Y", "Z" }),
                            new FloatArrayConverter()),
                        new ChildAttributePropertyDescriptor(
                            Localizer.Localize("Scale"),
                            UISchema.UITransformType.ScaleAttribute,
                            UISchema.UIControlType.TransformChild,
                            null,
                            Localizer.Localize("Item scale"),
                            false,
                            //new NumericTupleEditor(typeof(float), new string[] { "X", "Y", "Z" }),
                            //new FloatArrayConverter())
                            new UniformArrayEditor<float>())
                    }));

                // only one namespace
                break;
            }
        }

        /// <summary>
        /// Parse annotations in schema sets to handle property descriptor annotations</summary>
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
                // Parse XML annotation for property descriptors
                if (annotations.TryGetValue(nodeType, out xmlNodes))
                {
                    PropertyDescriptorCollection propertyDescriptors = Sce.Atf.Dom.PropertyDescriptor.ParseXml(nodeType, xmlNodes);
                    if (propertyDescriptors != null && propertyDescriptors.Count > 0)
                    {
                        // Property descriptor annotation found. Add any descriptors already set for this type.
                        PropertyDescriptorCollection propertyDescriptorsAlreadySet = nodeType.GetTag<PropertyDescriptorCollection>();
                        if (propertyDescriptorsAlreadySet != null)
                            foreach (PropertyDescriptor desc in propertyDescriptorsAlreadySet)
                                propertyDescriptors.Add(desc);
                        // Set all property descriptors
                        nodeType.SetTag<PropertyDescriptorCollection>(propertyDescriptors);
                    }
                }
            }
        }
    }
}
