//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
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


namespace DomPropertyEditorSample
{
    /// <summary>
    /// Loads the game schema, and annotates
    /// the types with display information and PropertyDescriptors.</summary>
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
            SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "DomPropertyEditorSample/Schemas");
            Load("game.xsd");
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
                    new PropertyView.CustomizeAttribute("Orcs".Localize(), horizontalEditorOffset:0, nameHasWholeRow:true),
                    new PropertyView.CustomizeAttribute("Armor".Localize(), horizontalEditorOffset:64),
                    new PropertyView.CustomizeAttribute("Club".Localize(), horizontalEditorOffset:64)
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
                Schema.gameType.Type.Define(new ExtensionInfo<GameEditingContext>());
                Schema.gameType.Type.Define(new ExtensionInfo<UniqueIdValidator>());


                // Add required property descriptors to GameObject and OrcType so it can be edited 
                // with two column PropertyGrid.

                // Note: this is programmatic approach: Decorate DomNode types with 
                //       property descriptors.
                //       Alternatively schema annotations can used.
                //       However, programmatic approach is recommend because of type safety.

                // To have custom URLs in the property's description in PropertyGrid,
                //  use this Atlassian Confluence wiki format:
                string url = "[More info|https://github.com/SonyWWS/ATF/wiki/DOM-Property-Editor-Programming-Discussion].".Localize();
                
                // To open a document when the user clicks the link, you can use this format,
                //  but command-line arguments can't be passed. To prepare command-line arguments,
                //  override PropertyGrid's LinkClicked().
                //string url = @"[More info|./Resources/help.chm].".Localize(); //can't have command-line arguments passed

                // Descriptors for armorType.
                string general = "General".Localize();
                var armorDescriptors = new PropertyDescriptorCollection(null);
                armorDescriptors.Add(new AttributePropertyDescriptor(
                           "Name".Localize(),
                           Schema.armorType.nameAttribute,
                           general,
                           "Armor name".Localize() + " " + url,
                           false
                    ));

                armorDescriptors.Add(new AttributePropertyDescriptor(
                           "Defense".Localize(),
                           Schema.armorType.defenseAttribute,
                           general,
                           "Armor defense".Localize() + " " + url,
                           false,
                           new NumericEditor(typeof(int))
                    ));

                armorDescriptors.Add(new AttributePropertyDescriptor(
                           "Price".Localize(),
                           Schema.armorType.priceAttribute,
                           general,
                           "Armor price in gold".Localize() + " " + url,
                           false,
                           new NumericEditor(typeof(int))
                    ));

                Schema.armorType.Type.SetTag(armorDescriptors);
                
                // club type property descriptors.
                var clubDescriptors = new PropertyDescriptorCollection(null);
                clubDescriptors.Add(new AttributePropertyDescriptor(
                         "Spike".Localize(),
                         Schema.clubType.spikesAttribute,
                         general,
                         "Club Has Spikes".Localize() + " " + url,
                         false,
                         new BoolEditor()
                  ));


                clubDescriptors.Add(new AttributePropertyDescriptor(
                       "Damage".Localize(),
                       Schema.clubType.DamageAttribute,
                       general,
                       "Amount of damage per strike".Localize() + " " + url,
                       false,
                       new NumericEditor(typeof(int))
                ));

                clubDescriptors.Add(new AttributePropertyDescriptor(
                        "Weight".Localize(),
                        Schema.clubType.wieghtAttribute,
                        general,
                        "Weight of the club".Localize() + " " + url,
                        false,
                        new NumericEditor(typeof(float))
                 ));

                Schema.clubType.Type.SetTag(clubDescriptors);
               
                var gobDescriptors = new PropertyDescriptorCollection(null);
                gobDescriptors.Add(
                     new AttributePropertyDescriptor(
                           "Name".Localize(),
                            Schema.gameObjectType.nameAttribute,
                            null,
                            "Object name".Localize() + " " + url,
                            false
                            ));

                // bool editor:  shows checkBox instead of textual (true,false).
                gobDescriptors.Add(
                     new AttributePropertyDescriptor(
                            "Visible".Localize(),
                            Schema.gameObjectType.visibleAttribute,
                            null,
                            "Show/Hide object in editor".Localize() + " " + url,
                            false,
                            new BoolEditor()
                            ));

                // NumericTupleEditor can be used for vector values.
                string xformCategory = "Transformation".Localize();
                var transEditor =
                    new NumericTupleEditor(typeof(float), new string[] { "Tx", "Ty", "Tz" });

                gobDescriptors.Add(
                    new AttributePropertyDescriptor(
                           "Translate".Localize(),
                           Schema.gameObjectType.translateAttribute,
                           xformCategory,
                           "Object's position".Localize() + " " + url,
                           false,
                           transEditor
                           ));

                var scaleEditor =
                    new NumericTupleEditor(typeof(float), new string[] { "Sx", "Sy", "Sz" });
                gobDescriptors.Add(
                    new AttributePropertyDescriptor(
                           "Scale".Localize(),
                           Schema.gameObjectType.scaleAttribute,
                           xformCategory,
                           "Object's scale".Localize() + " " + url,
                           false,
                           scaleEditor
                           ));

                var rotationEditor =
                    new NumericTupleEditor(typeof(float), new string[] { "Rx", "Ry", "Rz" });
                rotationEditor.ScaleFactor = 360.0f / (2.0f * (float)Math.PI); // Radians to Degrees
                gobDescriptors.Add(
                    new AttributePropertyDescriptor(
                           "Rotation".Localize(),
                           Schema.gameObjectType.rotateAttribute,
                           xformCategory,
                           "Object's orientation".Localize() + " " + url,
                           false,
                           rotationEditor
                           ));

                Schema.gameObjectType.Type.SetTag(gobDescriptors);

                // Defines property descriptors for orcType.
                var orcDescriptors = new PropertyDescriptorCollection(null);
                string chCategory = "Character attributes".Localize();

                // Bounded int editor: used for editing bounded int properties.
                orcDescriptors.Add(
                    new AttributePropertyDescriptor(
                           "Skill".Localize(),
                           Schema.orcType.skillAttribute,
                           chCategory,
                           "Skill".Localize() + " " + url,
                           false,
                           new BoundedIntEditor(1,120)
                           ));

                // Bounded float editor: similar to bounded int editor 
                // but it operates on float instead.
                orcDescriptors.Add(
                   new AttributePropertyDescriptor(
                          "Weight".Localize(),
                          Schema.orcType.weightAttribute,
                          chCategory,
                          "Weight".Localize() + " " + url,
                          false,                          
                          new BoundedFloatEditor(80, 400)
                          ));
                   

                // Enum can be stored as string or as int.
                //  OrcLevel is stored as int
                //  OrcEmotion is stored as string.
                //  please see the schema.
                
                
                // Create image for showing character level.                
                var levVals = Enum.GetValues(typeof(OrcLevel));
                Image[] levImages = new Image[levVals.Length];
                foreach (var en in levVals)
                {
                    levImages[(int)en] = new Bitmap(this.GetType(),
                            "Resources." + en + ".png");
                }


                // show usage for enum stored as int.
                // Shows how to edit enum that is stored as string.
                var lvEnumEditor = new LongEnumEditor(typeof(OrcLevel), levImages);
                var lvTypeConverter = new IntEnumTypeConverter(typeof(OrcLevel));

                orcDescriptors.Add(
                  new AttributePropertyDescriptor(
                         "Level".Localize(),
                         Schema.orcType.levelAttribute,
                         chCategory,
                         "Character level".Localize() + " " + url,
                         false,
                         lvEnumEditor,
                         lvTypeConverter
                         ));

                // create images used for showing emotion
                // for enum OrcEmotion 
                var emoVals = Enum.GetValues(typeof(OrcEmotion));
                Image[] emoImages = new Image[emoVals.Length];
                foreach (var en in emoVals)
                {
                    emoImages[(int)en] = new Bitmap(this.GetType(),
                            "Resources." + en + ".png");
                }
                // Shows how to edit enum that is stored as string.
                var emotionEditor = new LongEnumEditor(typeof(OrcEmotion), emoImages);
                orcDescriptors.Add(
                  new AttributePropertyDescriptor(
                         "Emotion".Localize(),
                         Schema.orcType.emotionAttribute,
                         chCategory,
                         "Emotion".Localize() + " " + url,
                         false,
                         emotionEditor
                         ));


                // FlagsUITypeEditor store flags as int.
                // doesn't implement IPropertyEditor
                
                FlagsUITypeEditor goalsEditor = new FlagsUITypeEditor(Enum.GetNames(typeof(OrcGoals)));
                FlagsTypeConverter goalsConverter = new FlagsTypeConverter(Enum.GetNames(typeof(OrcGoals)));
                orcDescriptors.Add(
                 new AttributePropertyDescriptor(
                        "Goals".Localize(),
                        Schema.orcType.goalsAttribute,
                        chCategory,
                        "Goals".Localize() + " " + url,
                        false,
                        goalsEditor,
                        goalsConverter
                        ));


                orcDescriptors.Add(
                new AttributePropertyDescriptor(
                       "Health".Localize(),
                       Schema.orcType.healthAttribute,
                       chCategory,
                       "Orc's health".Localize() + " " + url,
                       false,
                       new NumericEditor(typeof(int))
                       ));


                //EmbeddedCollectionEditor edit children (edit, add, remove, move).
                // note: EmbeddedCollectionEditor needs some work (efficiency and implementation issues).
                var collectionEditor = new EmbeddedCollectionEditor();

                // the following  lambda's handles (add, remove, move ) items.
                collectionEditor.GetItemInsertersFunc = (context)=>
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

                string weaponCategory = "Weapons and Defense".Localize();
                orcDescriptors.Add(
                 new ChildPropertyDescriptor(
                        "Armor".Localize(),
                        Schema.orcType.armorChild,
                        weaponCategory,
                        "Armors".Localize() + " " + url,
                        false,
                        collectionEditor
                        ));

                orcDescriptors.Add(
                new ChildPropertyDescriptor(
                       "Club".Localize(),
                       Schema.orcType.clubChild,
                       weaponCategory,
                       "Club".Localize() + " " + url,
                       false,
                       collectionEditor
                       ));

                orcDescriptors.Add(
                new ChildPropertyDescriptor(
                       "Orcs".Localize(),
                       Schema.orcType.orcChild,
                       "Children".Localize(),
                       "Orc children".Localize() + " " + url,
                       false,
                       collectionEditor
                       ));



                 string renderingCategory = "Rendering".Localize();

                // color picker.
                // note: ColorPickerEditor doesn't implement IPropertyEditor
                 orcDescriptors.Add(
                  new AttributePropertyDescriptor(
                         "Skin".Localize(),
                         Schema.orcType.skinColorAttribute,
                         renderingCategory,
                         "Skin color".Localize() + " " + url,
                         false,
                         new ColorPickerEditor(),
                         new IntColorConverter()
                         ));

                
                // file picker.
                 orcDescriptors.Add(
                  new AttributePropertyDescriptor(
                         "Texture file".Localize(),
                         Schema.orcType.textureFileAttribute,
                         renderingCategory,
                         "Texture file".Localize() + " " + url,
                         false,
                         new FileUriEditor("Texture file (*.dds)|*.dds")
                         ));
                 
                // Edit matrix.
                //NumericMatrixEditor
                 orcDescriptors.Add(
                  new AttributePropertyDescriptor(
                         "Texture Transform".Localize(),
                         Schema.orcType.textureTransformAttribute,
                         renderingCategory,
                         "Texture Transform".Localize() + " " + url,
                         false,
                         new NumericMatrixEditor()
                         ));


                // Edit array.
                // ArrayEditor, need some work, it has some efficiency and implementation issues.
                orcDescriptors.Add(
                  new AttributePropertyDescriptor(
                         "Texture Array".Localize(),
                         Schema.orcType.textureArrayAttribute,
                         renderingCategory,
                         "Texture Array".Localize() + " " + url,
                         false,
                         new ArrayEditor()
                         ));


                // readonly property,
                // show datetime as readonly.
                orcDescriptors.Add(
                 new AttributePropertyDescriptor(
                        "Revision data".Localize(),
                        Schema.orcType.TextureRevDateAttribute,
                        renderingCategory,
                        "Texture revision data and time".Localize() + " " + url,
                        true
                        ));
            
                // folder picker.
                // FolderUriEditor and FolderBrowserDialogUITypeEditor
                orcDescriptors.Add(
                 new AttributePropertyDescriptor(
                        "Resource Folder".Localize(),
                        Schema.orcType.resourceFolderAttribute,
                        renderingCategory,
                        "Resource folder".Localize() + " " + url,
                        false,
                        new FolderUriEditor()
                        ));
                
                Schema.orcType.Type.SetTag(orcDescriptors);
               
                // only one namespace
                break;
            }            
        }


        /// <summary>
        /// Enum used for orc character level.</summary>
        private enum OrcLevel
        {
            Apprentice,
            Journeyman,
            Adept,
            Master,
        }

        /// <summary>
        /// Enum used for OrcType.</summary>
        private enum OrcEmotion
        {
            Adequate,
            Capable,
            Enthusiastic,
            Happy,
            Thrilled,
            Annoyed,
            Angry,
            Frustrated,
            Depressed,
            Hostile
        }

        /// <summary>
        /// Enum used for OrcGoals.
        /// </summary>
        [Flags]
        private enum OrcGoals
        {
            WorldDomination = 1,
            Work = 2,
            Eat = 4,
            Sleep = 8,        
        }

        private readonly PropertyEditor m_propertyEditor;        
    }
}
