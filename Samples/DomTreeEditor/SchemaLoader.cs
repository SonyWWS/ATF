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
    /// Loads the UI schema, registers data extensions on the DOM types, and annotates
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
        /// not been called on the DomNodeTypes. Is called shortly before OnDomNodeTypesFrozen.
        /// Adds property descriptors for types.</summary>
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
                UISchema.UIType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
                

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
                UISchema.curveType.Type.Define(new ExtensionInfo<CurveLimitValidator>());                
                UISchema.controlPointType.Type.Define(new ExtensionInfo<ControlPoint>());
                

                //
                // tag UI types with display info; it will be used both in the palette
                //  and in the UITreeLister tree view.

                UISchema.UIType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UIType.Type,
                        "UI".Localize(),
                        "UI Container".Localize(),
                        "Sce.Atf.Resources.Data16.png"));

                UISchema.UIPackageType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UIPackageType.Type,
                        "Package".Localize(),
                        "Package, containing a complete collection of UI items".Localize(),
                        "DomTreeEditorSample.Resources.package.png"));

                UISchema.UIFormType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UIFormType.Type,
                        "Form".Localize(),
                        "Form, containing a UI screen".Localize(),
                        "DomTreeEditorSample.Resources.form.png"));

                UISchema.UISpriteType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UISpriteType.Type,
                        "Sprite".Localize(),
                        "A moveable UI element".Localize(),
                        "DomTreeEditorSample.Resources.sprite.png"));

                UISchema.UIShaderType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UIShaderType.Type,
                        "Shader".Localize(),
                        "Shader for rendering graphics".Localize(),
                        "DomTreeEditorSample.Resources.shader.png"));

                UISchema.UITextureType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UITextureType.Type,
                        "Texture".Localize(),
                        "Texture for rendering graphics".Localize(),
                        "DomTreeEditorSample.Resources.texture.png"));

                UISchema.UIFontType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UIFontType.Type,
                        "Font".Localize(),
                        "Font for rendering text".Localize(),
                        "DomTreeEditorSample.Resources.font.png"));

                UISchema.UITextItemType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UITextItemType.Type,
                        "Text".Localize(),
                        "Text, to display on Form".Localize(),
                        "DomTreeEditorSample.Resources.text.png"));

                UISchema.UITransformType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UITransformType.Type,
                        "Transform".Localize(),
                        "Transform, for sprites and text".Localize(),
                        "DomTreeEditorSample.Resources.transform.png"));

                UISchema.UIAnimationType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.UIAnimationType.Type,
                        "Animation".Localize(),
                        "Color and position animation curves, for form and control".Localize(),
                        null)                        
                );

                UISchema.curveType.Type.SetTag(
                    new NodeTypePaletteItem(
                        UISchema.curveType.Type,
                        "Curve".Localize(),
                        "animation curve".Localize(),
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
                            "Shader".Localize(),
                            UISchema.UIShaderType.FxFileAttribute,
                            null,
                            "Shader file path".Localize(),
                            false,
                            new FileUriEditor("Fx Files (*.fx)|*.fx")),
                        new AttributePropertyDescriptor(
                            "Shader param".Localize(),
                            UISchema.UIShaderType.ShaderParamAttribute,
                            null,
                            "Shader param".Localize(),
                            false,
                            new NumericEditor(typeof(Int32)))
                    }));

                UISchema.UITextureType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                        new AttributePropertyDescriptor(
                            "Texture file".Localize(),
                            UISchema.UITextureType.TextureFileAttribute,
                            null,
                            "Texture file path".Localize(),
                            false,
                            new FileUriEditor("Texture Files (*.tga)|*.tga")),
                        new AttributePropertyDescriptor(
                            "Texture folder".Localize(),
                            UISchema.UITextureType.TextureFolderAttribute,
                            null,
                            "Texture folder path".Localize(),
                            false,
                            new FolderBrowserDialogUITypeEditor("Texture folder path")),
                        new AttributePropertyDescriptor(
                            "Texture numbers".Localize(),
                            UISchema.UITextureType.TextureArrayAttribute,
                            null,
                            "Texture number array".Localize(),
                            false,
                            new ArrayEditor()),
                        new AttributePropertyDescriptor(
                            "Texture revision date".Localize(),
                            UISchema.UITextureType.TextureRevDateAttribute,
                            null,
                            "Texture revision date".Localize(),
                            false,
                            new DateTimeEditor()),
                    }));

                UISchema.UIFontType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                        new AttributePropertyDescriptor(
                            "Font".Localize(),
                            UISchema.UIFontType.FontFileAttribute,
                            null,
                            "Font file path".Localize(),
                            false,
                            new FileUriEditor("Font Files (*.ttf)|*.ttf")),
                        new AttributePropertyDescriptor(
                            "Font parameters".Localize(),
                            UISchema.UIFontType.FontParamsAttribute,
                            null,
                            "Font parameters".Localize(),
                            false,
                            new CollectionEditor()),
                    }));

                UISchema.UIAnimationType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                        new AttributePropertyDescriptor(
                            "Animation transform".Localize(),
                            UISchema.UIAnimationType.AnimationTransformAttribute,
                            null,
                            "Animation transform".Localize(),
                            false,
                            new NumericMatrixEditor(typeof(float), 3, 3)),
                        new AttributePropertyDescriptor(
                            "Animal kinds".Localize(),
                            UISchema.UIAnimationType.AnimalKindsAttribute,
                            null,
                            "Kinds of animal to animate".Localize(),
                            false,
                            new CollectionEditor()),
                    }));

                UISchema.UIControlType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[]
                    {
                        new ChildAttributePropertyDescriptor(
                            "Translation".Localize(),
                            UISchema.UITransformType.TranslateAttribute,
                            UISchema.UIControlType.TransformChild,
                            null,
                            "Item position".Localize(),
                            false,
                            new NumericTupleEditor(typeof(float), new string[] { "X", "Y", "Z" })),
                        new ChildAttributePropertyDescriptor(
                            "Rotation".Localize(),
                            UISchema.UITransformType.RotateAttribute,
                            UISchema.UIControlType.TransformChild,
                            null,
                            "Item rotation".Localize(),
                            false,
                            new NumericTupleEditor(typeof(float), new string[] { "X", "Y", "Z" })),
                        new ChildAttributePropertyDescriptor(
                            "Scale".Localize(),
                            UISchema.UITransformType.ScaleAttribute,
                            UISchema.UIControlType.TransformChild,
                            null,
                            "Item scale".Localize(),
                            false,                            
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
