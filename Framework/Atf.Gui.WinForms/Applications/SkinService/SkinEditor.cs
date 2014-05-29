//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.Schema;
using System.Windows.Forms;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

using PropertyGrid = Sce.Atf.Controls.PropertyEditing.PropertyGrid;
namespace Sce.Atf.Applications
{
    /// <summary>
    /// SkinEditor</summary>
    internal class SkinEditor : Form
    {        
        public SkinEditor()
        {
            Init();           
        }

        /// <summary>
        /// Load skin</summary>        
        public void OpenSkin(string fileName)
        {
            if (!File.Exists(fileName)) return;            
            using (FileStream stream = File.OpenRead(fileName))
            {
                SetActiveDocument(stream);
                m_activeDocument.Uri = new Uri(fileName);                
            }           
        }

        #region base overrides
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !ConfirmCloseActiveDocument();
        }
        #endregion

        #region private methods
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool close = ConfirmCloseActiveDocument();
            if (!close) return;

            Stream stream = Assembly.GetExecutingAssembly().
                GetManifestResourceStream("Sce.Atf.Applications.SkinService.Skin.tmpl");
            if (stream == null) return;

            try
            {
                SetActiveDocument(stream);
            }
            finally
            {
                stream.Close();                
            }            
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool close = ConfirmCloseActiveDocument();
            if (!close) return;

            using (var dlg = new OpenFileDialog())
            {                
                dlg.Filter = "Skin (*.skn)|*.skn";
                dlg.CheckFileExists = true;              
                dlg.InitialDirectory = SkinService.SkinsDirectory;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    string filename = dlg.FileName;
                    string error = string.Empty;
                    OpenSkin(filename);
                }
            }

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_activeDocument == null) return;
            if (m_activeDocument.Uri != null)
                SaveActiveDocument();
            else
                SaveAsActiveDocument();

        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(m_activeDocument == null) return;
            SaveAsActiveDocument();
           
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
       
        private bool ConfirmCloseActiveDocument()
        {
            bool confirm = true;
            if(m_activeDocument != null &&
                m_activeDocument.Dirty)
            {
                var dlgResult = MessageBox.Show(this, 
                "Would you like to save the changes?".Localize(), 
                "Unsaved changes".Localize(), 
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1);

                if (dlgResult == DialogResult.Yes)
                {
                    if (m_activeDocument.Uri != null)
                        SaveActiveDocument();
                    else
                    confirm = SaveAsActiveDocument();
                    
                }
                else if (dlgResult == DialogResult.Cancel)
                {
                    confirm = false;
                }
            }

            if (confirm)
            {
                m_activeDocument = null;
                m_treeControlAdapter.TreeView = null;
                m_PropertyGrid.Bind(null);
            }
            return confirm;
        }


        private bool SaveAsActiveDocument()
        {
            bool result = false;
            using (var dlg = new SaveFileDialog())
            {
                dlg.OverwritePrompt = true;
                dlg.Filter = "Skin (*.skn)|*.skn";
                if (m_activeDocument.Uri != null)
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(m_activeDocument.Uri.AbsolutePath);
                    dlg.FileName = Path.GetFileName(m_activeDocument.Uri.AbsolutePath);
                }
                else
                {
                    dlg.InitialDirectory = SkinService.SkinsDirectory;
                }

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    result = true;
                    m_activeDocument.Uri = new Uri(dlg.FileName);
                    SaveActiveDocument();
                }
            }
            return result;
        }
        private void SaveActiveDocument()
        {
            if (m_activeDocument == null
                || m_activeDocument.Uri == null) return;

            FileMode fileMode = FileMode.Create;
            using (FileStream stream = new FileStream(m_activeDocument.Uri.AbsolutePath, fileMode))
            {
                DomXmlWriter writer = new DomXmlWriter(s_schemaLoader.TypeCollection);
                writer.Write(m_activeDocument.DomNode, stream, m_activeDocument.Uri);
            }
            m_activeDocument.Dirty = false;
        }

        private void Init()
        {
            if (s_schemaLoader == null)
                s_schemaLoader = new SchemaLoader();

            m_PropertyGrid = new PropertyGrid();
            m_treeControl = new TreeControl();
            m_menu = new MenuStrip();
            var fileMenu = new ToolStripMenuItem();
            var newMenu = new ToolStripMenuItem();
            var openMenu = new ToolStripMenuItem();
            var saveMenu = new ToolStripMenuItem();
            var saveAsMenu = new ToolStripMenuItem();
            var exitMenu = new ToolStripMenuItem();
            var splitter = new SplitContainer();

            m_menu.SuspendLayout();
            splitter.BeginInit();
            splitter.Panel1.SuspendLayout();
            splitter.Panel2.SuspendLayout();
            splitter.SuspendLayout();

            SuspendLayout();
            
            // m_menu
            m_menu.Location = new System.Drawing.Point(0, 0);
            m_menu.Name = "m_menu";            
            m_menu.TabIndex = 0;
            m_menu.Text = "m_menu";
            m_menu.Items.Add(fileMenu);


            // file            
            fileMenu.Name = "fileToolStripMenuItem";
            fileMenu.Size = new System.Drawing.Size(37, 20);
            fileMenu.Text = "File".Localize();
            fileMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                newMenu, 
                openMenu,
                saveMenu,
                saveAsMenu,
                exitMenu
            });


            // new
            newMenu.Name = "newToolStripMenuItem";
            newMenu.ShortcutKeys = Keys.Control | Keys.N;
            newMenu.Text = "New".Localize();
            newMenu.Click += newToolStripMenuItem_Click;            

            //open
            openMenu.Name = "openToolStripMenuItem";
            openMenu.ShortcutKeys = Keys.Control | Keys.O;            
            openMenu.Text = "Open...".Localize();
            openMenu.Click += openToolStripMenuItem_Click;
            
            //save
            saveMenu.Name = "saveToolStripMenuItem";
            saveMenu.ShortcutKeys = Keys.Control | Keys.S;
            saveMenu.Text = "Save".Localize();
            saveMenu.Click += saveToolStripMenuItem_Click;

            // save as
            saveAsMenu.Name = "saveAsToolStripMenuItem";            
            saveAsMenu.Text = "Save As...".Localize();
            saveAsMenu.Click += saveAsToolStripMenuItem_Click;

            // exit
            exitMenu.Name = "exitToolStripMenuItem";            
            exitMenu.Text = "Exit".Localize();
            exitMenu.Click += exitToolStripMenuItem_Click;

            // tree control
            m_treeControl.Dock = DockStyle.Fill;          
            m_treeControl.Name = "m_treeControl";            
            m_treeControl.TabIndex = 1;
            m_treeControl.Width = 150;
            m_treeControl.ShowRoot = false;
            m_treeControl.AllowDrop = false;
            m_treeControl.SelectionMode = SelectionMode.One;
            m_treeControlAdapter = new TreeControlAdapter(m_treeControl);
            
            // propertyGrid1            
            m_PropertyGrid.Dock = DockStyle.Fill;            
            m_PropertyGrid.Name = "propertyGrid1";            
            m_PropertyGrid.TabIndex = 3;
            m_PropertyGrid.PropertySorting = PropertySorting.None;

            // splitter           
            splitter.Dock = DockStyle.Fill;            
            splitter.Name = "splitContainer1";
            splitter.Panel1.Controls.Add(m_treeControl);
            splitter.Panel2.Controls.Add(m_PropertyGrid);            
            splitter.SplitterDistance = 100;
            splitter.TabIndex = 1;
            
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(600, 400);            
            Controls.Add(splitter);            
            Controls.Add(m_menu);
            MainMenuStrip = m_menu;
            Name = "SkinEditor";
            Text = "SkinEditor".Localize();
            Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage));

            m_menu.ResumeLayout(false);
            m_menu.PerformLayout();
            splitter.Panel1.ResumeLayout(false);
            splitter.Panel2.ResumeLayout(false);
            splitter.EndInit();
            splitter.ResumeLayout(false);
            
            ResumeLayout(false);
            PerformLayout();
            splitter.SplitterDistance = 170;
        }

        private void SetActiveDocument(Stream stream)
        {
            DomXmlReader domReader = new DomXmlReader(s_schemaLoader);
            var node = domReader.Read(stream, null);
            node.InitializeExtensions();
            m_activeDocument = node.As<SkinDocument>();            
            var context = node.As<SkinEditingContext>();
            m_treeControlAdapter.TreeView = context;
            m_PropertyGrid.Bind(null);
            context.SelectionChanged += (sender, e) =>
            {
                var selectedNode = context.GetLastSelected<DomNode>();
                var propContext = new CustomPropertyEditingContext(selectedNode, m_activeDocument);
                m_PropertyGrid.Bind(propContext);
            };

            m_activeDocument.UriChanged += (sender, e) => UpdateTitleText();
            m_activeDocument.DirtyChanged += (sender, e) => UpdateTitleText();
        }

        private void UpdateTitleText()
        {
            string str = "SkinEditor";
            if (m_activeDocument != null)
            {

                str += m_activeDocument.Uri != null ?
                    ": " + m_activeDocument.Uri.AbsolutePath : ": Untitled";
                if (m_activeDocument.Dirty)
                    str += "*";
            }
            this.Text = str;                                
        }
        private void PrintDomNode(DomNode node)
        {
            Console.WriteLine(node.Type.Name);
            foreach (AttributeInfo attr in node.Type.Attributes)
                Console.WriteLine("\t{0}: {1}", attr.Name, node.GetAttribute(attr));            
            foreach (DomNode child in node.Children)
                PrintDomNode(child);                        
        }

        #endregion

        #region private members

        PropertyGrid m_PropertyGrid;
        TreeControlAdapter m_treeControlAdapter;
        TreeControl m_treeControl;
        private MenuStrip m_menu;
        private SkinDocument m_activeDocument;
        private static SchemaLoader s_schemaLoader;
        #endregion

        #region private classes

        private class SkinDocument : DomDocument
        {
            public override string Type
            {
                get { return "SkinDocument"; }
            }
        }

        private class SkinEditingContext : EditingContext , ITreeView, IItemView
        {

            #region ITreeView Members

            object ITreeView.Root
            {
                get { return this.DomNode; }
            }

            IEnumerable<object> ITreeView.GetChildren(object parent)
            {
                DomNode parentNode = parent.Cast<DomNode>();
                if (parentNode == this.DomNode)
                {
                    foreach (var child in parentNode.Children)
                        yield return child;
                }
            }

            #endregion

            #region IItemView Members

            void IItemView.GetInfo(object item, ItemInfo info)
            {                
                DomNode node = item.Cast<DomNode>();               
                info.IsLeaf = true;
                 
                if (node.Type.Equals(SkinSchema.styleType.Type))
                    info.Label = (string)node.GetAttribute(SkinSchema.styleType.nameAttribute);                
                else
                    info.Label = node.Type.Name;
            }
            #endregion
        }

        private class SkinStyleProperties : CustomTypeDescriptorNodeAdapter, IDynamicTypeDescriptor
        {

            /// <summary>
            /// Creates an array of property descriptors that are associated with 
            /// the adapted DomNode's DomNodeType.</summary>
            /// <returns>Array of property descriptors</returns>
            protected override System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
            {

                // styleType
                //    setterType 0 to N

                // setterType
                //     valueInfoType  0 to 1
                //     listInfoType 0 to 1

                // valueInfoType
                //  constructorParamsType 0 to 1
                //  setterType  0 to N
                //  attribs:
                //      type
                //      value

                // listInfoType
                //    valueInfoType 0 to N

                // constructorParamsType
                //    valueInfoType 0 to N


                List<System.ComponentModel.PropertyDescriptor> descriptors =
                    new List<System.ComponentModel.PropertyDescriptor>();
             
                var setters = DomNode.GetChildList(SkinSchema.styleType.setterChild);               
                foreach (var setter in setters)
                {
                    ProcessSetterType(setter,"", descriptors);
                }

                return descriptors.ToArray();

                
            }


            private void ProcessSetterType(DomNode setter, string parentPropName,
                List<System.ComponentModel.PropertyDescriptor> descriptors)
            {
                string curPropName = (string)setter.GetAttribute(SkinSchema.setterType.propertyNameAttribute);
                if (string.IsNullOrWhiteSpace(curPropName)) return;
                string propName = !string.IsNullOrEmpty(parentPropName)
                    ? parentPropName + "->" + curPropName : curPropName;

                DomNode valInfo = setter.GetChild(SkinSchema.setterType.valueInfoChild);
                if (valInfo != null)                
                {
                    ProcessValueInfo(valInfo, propName, descriptors);
                }

                DomNode listInfo = setter.GetChild(SkinSchema.setterType.listInfoChild);
                if (listInfo != null)
                {
                    foreach (var vInfo in listInfo.GetChildList(SkinSchema.listInfoType.valueInfoChild))
                        ProcessValueInfo(vInfo, propName, descriptors);
                }
            }

            private void ProcessValueInfo(DomNode valInfo, string propName,
                List<System.ComponentModel.PropertyDescriptor> descriptors)
            {
                string typeName = (string)valInfo.GetAttribute(SkinSchema.valueInfoType.typeAttribute);
                Type type = SkinUtil.GetType(typeName);


                if (type == typeof(Font))
                {
                    FontDescriptor descr
                        = new FontDescriptor(valInfo, propName, null, null, null, null);
                    descriptors.Add(descr);
                }
                else
                {

                    TypeConverter converter;
                    object editor;
                    GetEditorAndConverter(type, out editor, out converter);
                    if (editor != null)
                    {
                        var descr = new SkinSetterAttributePropertyDescriptor(valInfo
                            , propName, SkinSchema.valueInfoType.valueAttribute, null, null, false, editor, converter);
                        descriptors.Add(descr);
                    }
                    else
                    {
                        DomNode ctorParams = valInfo.GetChild(SkinSchema.valueInfoType.constructorParamsChild);
                        if (ctorParams != null)
                        {
                            var vInfoChildList = ctorParams.GetChildList(SkinSchema.constructorParamsType.valueInfoChild);
                            if (vInfoChildList.Count == 1)
                            {
                                ProcessValueInfo(vInfoChildList[0], propName, descriptors);
                            }
                            else
                            {
                                int k = 1;
                                string paramName = propName +" : Arg_";
                                foreach (DomNode vInfoChild in vInfoChildList)
                                {
                                    string name = paramName + k;                           
                                    ProcessValueInfo(vInfoChild, name, descriptors);
                                    k++;
                                }
                            }
                        }
                        
                        foreach (DomNode setterChild in valInfo.GetChildList(SkinSchema.valueInfoType.setterChild))
                        {
                            ProcessSetterType(setterChild, propName, descriptors);
                        }
                    }
                }
            }
                
            private void GetEditorAndConverter(Type type, 
                out object editor, 
                out TypeConverter converter)
            {                
                editor = null;
                converter = null;
                if (type == null) return;

                if (type == typeof(Color))
                {
                    var colorpicker = new Sce.Atf.Controls.PropertyEditing.ColorPickerEditor();
                    colorpicker.EnableAlpha = true;
                    editor = colorpicker;
                    converter = new StringColorConverter();
                }
                else if (type.IsEnum)
                {
                    editor = new LongEnumEditor(type);
                }

                //// First check for very common type names and custom names.
                //switch (type)
                //{
                //    case typeof(Color)
                       
                //        break;
                //    //case "string": //short for "System.String"
                //    //    return typeof(string);

                //    //case "int": //short for "System.Int32"
                //    //    return typeof(int);

                //    //case "float": //short for "System.Single"
                //    //    return typeof(float);

                //    //case "char": //short for "System.Char"
                //    //    return typeof(char);

                //    //case "byte": //short for "System.Byte"
                //    //    return typeof(byte);
                //}

            }
            #region IDynamicTypeDescriptor Members
            bool IDynamicTypeDescriptor.CacheableProperties
            {
                get { return false; }
            }
            #endregion
        }
        
        private class SkinSetterAttributePropertyDescriptor : AttributePropertyDescriptor
        {            
            public SkinSetterAttributePropertyDescriptor(
            DomNode domObj,
            string name,
            AttributeInfo attribute,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter)
                : base(name, attribute, category, description, isReadOnly, editor, typeConverter)
            {
                m_domObject = domObj;
            }

            #region Overrides

            public override DomNode GetNode(object component)
            {
                return m_domObject;
            }

            /// <summary>
            /// Implements Equals() for organizing descriptors in grid controls</summary>
            public override bool Equals(object obj)
            {
                SkinSetterAttributePropertyDescriptor other = obj as SkinSetterAttributePropertyDescriptor;
                if (!base.Equals(other))
                    return false;

                if (m_domObject != other.m_domObject)
                    return false;

                if (Name != other.Name)
                    return false;

                return true;
            }

            /// <summary>
            /// Implements GetHashCode() for organizing descriptors in grid controls</summary>
            public override int GetHashCode()
            {               
                return m_domObject.GetHashCode();               
            }

            #endregion 

            private DomNode m_domObject;
        }


        private class FontDescriptor : Sce.Atf.Dom.PropertyDescriptor
        {
            public FontDescriptor(
            DomNode domObj,
            string name,
            string category,
            string description,
            object editor,
            TypeConverter typeConverter)
                : base(name, typeof(Font), category, description, false, editor, typeConverter)
            {
                m_domObject = domObj;
            }

            #region Overrides

            public override DomNode GetNode(object component)
            {
                return m_domObject;
            }

            /// <summary>
            /// Implements Equals() for organizing descriptors in grid controls</summary>
            public override bool Equals(object obj)
            {
                FontDescriptor other = obj as FontDescriptor;
                if (!base.Equals(other))
                    return false;

                if (m_domObject != other.m_domObject)
                    return false;

                if (Name != other.Name)
                    return false;

                return true;
            }

            /// <summary>
            /// Implements GetHashCode() for organizing descriptors in grid controls</summary>
            public override int GetHashCode()
            {
                return m_domObject.GetHashCode();
            }

            #endregion



            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override object GetValue(object component)
            {
                try
                {
                    if (m_font == null)
                    {

                        DomNode param = m_domObject.GetChild(SkinSchema.valueInfoType.constructorParamsChild);
                        string fname = null;
                        float fontSize = 1.0f;
                        FontStyle fontStyle = FontStyle.Regular;

                        foreach (var arg in param.GetChildren(SkinSchema.constructorParamsType.valueInfoChild))
                        {
                            string typeName = (string)arg.GetAttribute(SkinSchema.valueInfoType.typeAttribute);
                            Type type = SkinUtil.GetType(typeName);
                            string val = (string)arg.GetAttribute(SkinSchema.valueInfoType.valueAttribute);

                            if (type == typeof(string))
                            {
                                fname = val;
                            }
                            else if (type == typeof(float))
                            {
                                fontSize = float.Parse(val);
                            }
                            else if (type == typeof(FontStyle))
                            {
                                fontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), val);
                            }
                        }
                        m_font = new Font(fname, fontSize, fontStyle);

                    }

                }
                catch { } // suppress parsing error.

                return m_font;
            }

            public override void ResetValue(object component)
            {
                // reset attribute.
            }

            public override void SetValue(object component, object value)
            {
                if (m_font != null) m_font.Dispose();
                m_font = (Font)value;

                DomNode param = m_domObject.GetChild(SkinSchema.valueInfoType.constructorParamsChild);
                foreach (var arg in param.GetChildren(SkinSchema.constructorParamsType.valueInfoChild))
                {
                    string typeName = (string)arg.GetAttribute(SkinSchema.valueInfoType.typeAttribute);

                    Type type = SkinUtil.GetType(typeName);
                    if (type == typeof(string))
                    {
                        arg.SetAttribute(SkinSchema.valueInfoType.valueAttribute, m_font.FontFamily.Name);
                    }
                    else if (type == typeof(float))
                    {
                        arg.SetAttribute(SkinSchema.valueInfoType.valueAttribute, m_font.Size.ToString());
                    }
                    else if (type == typeof(FontStyle))
                    {
                        arg.SetAttribute(SkinSchema.valueInfoType.valueAttribute, m_font.Style.ToString());
                    }
                }
            }

            private DomNode m_domObject;
            private Font m_font;
        }


        // <setter propertyName="Font">
        //    <valueInfo type="System.Drawing.Font">
        //        <constructorParams>
        //            <valueInfo type="string" value="Verdana"/>
        //            <valueInfo type="float" value="11"/>
        //            <valueInfo type="System.Drawing.FontStyle" value="Regular"/>        
        //        </constructorParams>
        //    </valueInfo>
        //</setter>
        
        private class StringColorConverter : TypeConverter
        {

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string)
                    || sourceType == typeof(Color);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof(string)
                     || destinationType == typeof(Color);
            }

            /// <summary>
            /// Convert from color to string. </summary>            
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {                
                if (value is Color)                
                    return string.Format("#{0:X}", ((Color)value).ToArgb());                                    
                return value;          
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    if (value is string) return value;
                    return string.Format("#{0:X}", ((Color)value).ToArgb());                    
                }

                if (destinationType == typeof(Color))
                {
                    if (value is Color) return value;
                    string str = value as string;
                    if (!string.IsNullOrWhiteSpace(str))
                        return m_converter.ConvertFromString(str);
                    return Color.Black;
                }
                return null;                
            }
            private System.Drawing.ColorConverter m_converter = new ColorConverter();
        }

        public class CustomPropertyEditingContext : PropertyEditingContext, IAdaptable
        {

            public CustomPropertyEditingContext(object item, object context)
                : base(new[] { item })
            {
                m_context = context;
            }

            #region IAdaptable Members

            object IAdaptable.GetAdapter(Type type)
            {
                if (type == typeof(ITransactionContext))
                {                    
                    return m_context.As<ITransactionContext>();
                }
                return null;
            }

            #endregion
            private object m_context;
        }

        private class SchemaLoader : XmlSchemaTypeLoader
        {            
            public SchemaLoader()
            {
                // set resolver to locate embedded .xsd file
                SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(),
                    "Sce.Atf.Applications.SkinService/Schemas");
                Load("skin.xsd");
            }

            /// <summary>
            /// Gets the game type collection</summary>
            public XmlSchemaTypeCollection TypeCollection
            {
                get { return m_typeCollection; }
            }
            private XmlSchemaTypeCollection m_typeCollection;

            protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
            {
                foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
                {
                    m_typeCollection = typeCollection;
                    SkinSchema.Initialize(typeCollection);
                    SkinSchema.skinType.Type.Define(new ExtensionInfo<SkinDocument>());
                    SkinSchema.skinType.Type.Define(new ExtensionInfo<SkinEditingContext>());
                    SkinSchema.skinType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());

                    SkinSchema.styleType.Type.Define(new ExtensionInfo<SkinStyleProperties>());
                    
                    break;
                }
            }
        }
        #endregion

    } // internal class SkinEditor : Form

    internal static class SkinUtil
    {
        static SkinUtil()
        {
            s_types = new Dictionary<string, Type>();

            s_types.Add("string", typeof(string));
            s_types.Add("float", typeof(float));
            s_types.Add("int", typeof(int));
            s_types.Add("char", typeof(char));
            s_types.Add(typeof(FontStyle).FullName, typeof(FontStyle));
            s_types.Add(typeof(Color).FullName, typeof(Color));
            s_types.Add(typeof(Control).FullName, typeof(Control));
            s_types.Add(typeof(Font).FullName, typeof(Font));
            s_types.Add(typeof(DockColors).FullName, typeof(DockColors));
            s_types.Add(typeof(System.Drawing.Drawing2D.LinearGradientMode).FullName, typeof(System.Drawing.Drawing2D.LinearGradientMode));
            s_types.Add(typeof(ControlGradient).FullName, typeof(ControlGradient));
            s_types.Add(typeof(FlatStyle).FullName, typeof(FlatStyle));
            

        }


        public static Type GetType(string typeName)
        {
            Type type;
            if (s_types.TryGetValue(typeName, out type))
                return type;

            // We don't require the type name to be an assembly qualified name, so we need to search
            //  each loaded assembly instead of using Type.GetType(string).
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null) break;
                
            }

            // type could be null but still adding it to s_types
            // so next function call with same typeName will not needlessly search again
            s_types.Add(typeName, type);
            return type;
        }


        private static Dictionary<string, Type>
            s_types;
    }
}
