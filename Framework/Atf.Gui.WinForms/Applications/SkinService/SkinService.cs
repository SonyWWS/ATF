//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;
#if TRACE_SKINSERVICE
using System.Diagnostics;
#endif
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service that allows for easy customization of an application’s appearance by using inheritable properties 
    /// that can be applied at run-time and loaded from *.skn (XML format) files. 
    /// Skin files can affect any public property of any control in an application.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(SkinService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SkinService : IInitializable, ICommandClient
    {
        /// <summary>
        /// Skin changed or applied event</summary>
        public static event EventHandler SkinChangedOrApplied;

        /// <summary>
        /// Initialize instance</summary>
        void IInitializable.Initialize()
        {
            CommonInit();            
            if (m_settingsService != null)
            {
                m_settingsService.RegisterSettings(this,
                                new BoundPropertyDescriptor(this, () => MruSkinFile, "Most Recently Used Skin File".Localize(), null, null));
            }
        }

        /// <summary>
        /// Common initialization</summary>
        protected void CommonInit()
        {
            if(ShowCommands)
                RegisterCommands();

            WinFormsUtil.WindowCreated += WindowCreated;
            WinFormsUtil.WindowDestroyed += WindowDestroyed;            
        }

        #region ICommandClient Members

        /// <summary>
        /// Skin commands enum</summary>
        public enum SkinCommands
        {            
            /// <summary>
            /// Show Skin Editor</summary>
            SkinEdit,
            /// <summary>
            /// Load and apply a skin file</summary>
            SkinLoad,
            /// <summary>
            /// Reset active skin to the default skin</summary>
            SkinReset
        }

        /// <summary>
        /// Skin command group</summary>
        public enum SkinCommandGroup
        {
            /// <summary>
            /// Edit the current skin file command</summary>
            ViewSkin
        }

        /// <summary>
        /// SkinEdit command</summary>
        public static CommandInfo SkinEdit =
            new CommandInfo(
                SkinCommands.SkinEdit,
                StandardMenu.View,
                SkinCommandGroup.ViewSkin,
                "Edit Skin...".Localize("Show Skin Editor and open current skin file."),
                "Edit skin.".Localize());

        /// <summary>
        /// SkinLoad command</summary>
        public static CommandInfo SkinLoad =
            new CommandInfo(
                SkinCommands.SkinLoad,
                StandardMenu.View,
                SkinCommandGroup.ViewSkin,
                "Load Skin...".Localize("Load and apply a skin file."),
                "Load and apply a skin file.".Localize());

        /// <summary>
        /// SkinReset command</summary>
        public static CommandInfo SkinReset =
            new CommandInfo(
                SkinCommands.SkinReset,
                StandardMenu.View,
                SkinCommandGroup.ViewSkin,
                "Reset Skin to Default".Localize("Reset active skin to the default skin."),
                "Reset active skin to the default skin.".Localize());

        /// <summary>
        /// Checks whether the client can do the command if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns><c>True</c> if client can do the command</returns>
        public virtual bool CanDoCommand(object commandTag)
        {
            if (!(commandTag is SkinCommands))
                return false;

            // The skin editor edits the active skin, so we can't change the current skin while
            //  the editor is running and we can't launch a second editor either.
            if (m_skinEditor != null)
                return false;

            bool enabled = false;

            switch((SkinCommands)commandTag)
            {
                case SkinCommands.SkinEdit:
                    enabled = true;
                    break;
                
                case SkinCommands.SkinLoad:
                    enabled = FileDialogService != null;
                    break;

                case SkinCommands.SkinReset:
                    enabled = ActiveSkin != null;
                    break;
            }

            return enabled;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public virtual void DoCommand(object commandTag)
        {
            if (!(commandTag is SkinCommands))
                return;

            switch ((SkinCommands)commandTag)
            {
                case SkinCommands.SkinEdit:
                    if (m_skinEditor == null)
                    {
                        m_skinEditor = new SkinEditor();
                        m_skinEditor.Show(MainForm);
                        if (ActiveSkin != null)
                        {
                            m_skinEditor.OpenSkin(ActiveSkin.SkinFile);
                        }
                        m_skinEditor.FormClosed += SkinEditor_FormClosed;
                        m_skinEditor.SkinChanged += SkinEditor_SkinChanged;
                        m_mainForm.FormClosing += m_mainForm_FormClosing;
                    }
                    break;

                case SkinCommands.SkinLoad:
                    string forcedDirectory =
                        ActiveSkin == null ? SkinsDirectory :
                        Directory.GetParent(ActiveSkin.SkinFile).FullName;
                    string newSkinPath = null;
                    var dlgResult = FileDialogService.OpenFileName(ref newSkinPath, Info.GetFilterString(), forcedDirectory);
                    if (dlgResult == FileDialogResult.OK)
                    {
                        OpenAndApplySkin(newSkinPath);
                        SkinsDirectory = Directory.GetParent(newSkinPath).FullName;
                    }
                    break;
               
                case SkinCommands.SkinReset:
                    ResetSkin();
                    break;
            }
        }

        private void SkinEditor_SkinChanged(object sender, DocumentEventArgs e)
        {
            using (Stream stream = m_skinEditor.GetCurrentSkin())
            {
                string skinFile = UnsavedSkinString;
                if (e.Document != null && e.Document.Uri != null)
                    skinFile = e.Document.Uri.LocalPath;
                OpenSkinFile(stream, skinFile);
                if (ActiveSkin != null)
                {
                    // Success! This means the editor is not shutting down and is still editing a valid skin.
                    ApplyActiveSkin();
                }
                else
                {
                    // Either revert to the previous skin or reset to the original no-skin look.
                    if (!OpenAndApplySkin(MruSkinFile))
                        ResetSkin();
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public virtual void UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        /// <summary>
        /// Gets information about this document client, such as the skin file extensions that are
        /// supported, whether or not it allows multiple documents to be open, etc.</summary>
        public DocumentClientInfo Info
        {
            get { return s_info; }
        }

        /// <summary>
        /// Returns a value indicating if the client can open a skin file at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>true, if the client can open a skin file at the given URI</returns>
        public bool CanOpen(Uri uri)
        {
            return s_info.IsCompatibleUri(uri);
        }

        /// <summary>
        /// Returns whether the client can open a skin file at the given path</summary>
        /// <param name="path">Document path</param>
        /// <returns><c>True</c> if the client can open a skin file at the given path</returns>
        public bool CanOpen(string path)
        {
            return s_info.IsCompatiblePath(path);
        }

        /// <summary>
        /// Loads the specified skin, sets it as the active skin, and applies it to the main form
        /// and all other skinnable objects</summary>
        /// <param name="skinFilePath">Path to skin file</param>
        /// <returns><c>True</c> if the skin was successfully opened, false otherwise</returns>
        public bool OpenAndApplySkin(string skinFilePath)
        {
            if (String.IsNullOrEmpty(skinFilePath))
                return false;

            if (!CanOpen(skinFilePath))
                return false;

            OpenSkinFile(skinFilePath);
            if (ActiveSkin == null)
                return false;

            ApplyActiveSkin();
            return true;
        }

        /// <summary>
        /// Loads the skin from the specified stream, sets it as the active skin, and applies it to the main form
        /// and all other skinnable objects</summary>
        /// <param name="stream">Stream containing the skin file</param>
        /// <returns><c>True</c> if the skin was successfully opened, false otherwise</returns>
        public bool OpenAndApplySkin(Stream stream)
        {
            OpenSkinFile(stream, EmbeddedSkinString);

            if (ActiveSkin == null)
                return false;

            ApplyActiveSkin();

            return true;
        }

        /// <summary>
        /// Loads the specified skin file and sets the active skin to it.</summary>
        /// <param name="uri">Document URI</param>
        /// <remarks>No return value, but side effect is to set ActiveSkin upon success</remarks>
        public void OpenSkinFile(Uri uri)
        {
            string filePath = (uri.IsAbsoluteUri)
                ? uri.LocalPath
                : PathUtil.GetAbsolutePath(uri.OriginalString, Directory.GetCurrentDirectory());

            OpenSkinFile(filePath);
        }

        /// <summary>
        /// Loads the specified skin file and sets the active skin to it</summary>
        /// <param name="filePath">Skin file path</param>
        /// <remarks>No return value, but side effect is to set ActiveSkin upon success</remarks>
        public void OpenSkinFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                OpenSkinFile(stream, filePath);                
            }
        }

        /// <summary>
        /// Loads the skin from the specified stream and sets the active skin to it</summary>
        /// <param name="stream">Stream containing the skin</param>
        /// <param name="skinFileDisplayText">Skin file path</param>
        /// <remarks>No return value, but ActiveSkin will be null if there was a failure and not
        /// null if successful.</remarks>
        public void OpenSkinFile(Stream stream, string skinFileDisplayText)
        {
            ActiveSkin = null;
            if (stream == null || stream == Stream.Null || !stream.CanRead)
                return;

            try
            {                
                ActiveSkin = new Skin { SkinFile = skinFileDisplayText, Styles = new List<SkinStyle>() };               
                var skinFile = new XmlDocument();               
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    reader.Namespaces = false;
                    skinFile.Load(reader);                   
                }
                                
                // If the skin file passed validation, load it up
                LoadSkin(skinFile);

                // Store the file path so it will be persisted and reloaded the next
                // time the app is launched.
                if (skinFileDisplayText != UnsavedSkinString)
                    MruSkinFile = skinFileDisplayText;
            }
            catch (Exception exception)
            {
                Outputs.WriteLine(
                    OutputMessageType.Error,
                    ("Error loading skin file.".Localize() + ": {0}"),
                    exception.Message);
                ActiveSkin = null;
            }
        }


        /// <summary>
        /// Applies the active skin to the main form and to all other skinnable Controls</summary>
        public void ApplyActiveSkin()
        {
            //Trace.WriteLine("ApplyActiveSkin()", "SkinService");
            if (ActiveSkin == null)
                return;

            ApplySkinToNonClientArea();

            // restore the old values before applying the new values
            RestoreOriginalPropertyValues();
            s_originalPropertyValues.Clear();

            // SkinnableObjects includes MainForm, but many of the SkinnableObjects may
            //  already be children of MainForm, so we should avoid skinning them twice.
            var skinnedControls = new HashSet<object>();

            foreach (object control in SkinnableObjects)
                ApplyActiveSkin(control, skinnedControls);

            SkinChangedOrApplied.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Restores all of the Controls to their original values and sets the ActiveSkin to null</summary>
        public void ResetSkin()
        {
            RestoreOriginalPropertyValues();
            s_originalPropertyValues.Clear();
            ActiveSkin = null;
            m_mruSkinFile = ""; // Don't use null because that may not persist in the settings system

            foreach (var nc in s_formNcRenderers.Values)
                nc.CustomPaintDisabled = true;
            SkinChangedOrApplied.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Applies the active skin to a control (or any other type of object). If it is a Control, then
        /// the skin will be applied to all of its child controls</summary>
        /// <param name="control">Control to which skin is applied</param>
        public static void ApplyActiveSkin(Object control)
        {            
            ApplyActiveSkin(control, null);
        }

        /// <summary>
        /// Gets or sets the starting directory that the user sees when opening a skin
        /// file when there is no currently active skin. If this property is null, the
        /// default directory is chosen, which will be the user's previously picked skin directory.
        /// If there is an active skin, this property is ignored. The default value is null.</summary> 
        public static string SkinsDirectory { get; private set; }

        /// <summary>
        /// Gets full path of Active skin or null</summary>
        public static string ActiveSkinFile
        {
            get { return ActiveSkin == null ? null : ActiveSkin.SkinFile;}
        }

        /// <summary>
        /// Gets the controls to which a skin can be applied. Might be the empty list, but won't be null.</summary>
        protected virtual IEnumerable<Control> SkinnableControls
        {
            get { return SkinnableObjects.OfType<Control>(); }
        }
        
        /// <summary>
        /// Gets the objects to which a skin can be applied. Might be the empty list, but won't be null.</summary>
        protected virtual IEnumerable<object> SkinnableObjects
        {
            get
            {
                if (MainForm != null)
                    yield return MainForm;
                
                // This is probably a good time to clean-up any garbage-collected items.
                s_skinnableObjects.RemoveWhere(key => !key.IsAlive);


                foreach (WeakKey<object> existing in s_skinnableObjects)
                {
                    object target = existing.Target;
                    // We have to check if the target got garbage-collected, since that can happen at any time.

                    var ctrl = target as Control;
                    if (target != null && (ctrl == null || !ctrl.IsDisposed))
                        yield return target;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to show skin commands</summary>
        public bool ShowCommands
        {
            get { return m_showCommands; }
            set
            {
                if (value == m_showCommands)
                    return;

                m_showCommands = value;

                if (value)
                    RegisterCommands();
                else
                    UnregisterCommands();
            }
        }

        /// <summary>
        /// Registers skin commands for this service</summary>
        /// <remarks>Only Load and Reset skin commands are registered at this time</remarks>
        protected void RegisterCommands()
        {
            if (CommandService != null)
            {
                CommandService.RegisterCommand(SkinLoad, this);                
                CommandService.RegisterCommand(SkinEdit, this);                
                CommandService.RegisterCommand(SkinReset, this);
            }
        }

        /// <summary>
        /// Unregisters skin commands for this service</summary>
        /// <remarks>Only Load and Reset skin commands are unregistered at this time</remarks>
        protected void UnregisterCommands()
        {
            if (CommandService != null)
            {
                CommandService.UnregisterCommand(SkinLoad, this);                
                CommandService.UnregisterCommand(SkinEdit, this);                
                CommandService.UnregisterCommand(SkinReset, this);
            }
        }

        /// <summary>
        /// Gets or sets the command service</summary>
        [Import(AllowDefault = true)]
        protected ICommandService CommandService { get; set; }

        /// <summary>
        /// Gets or sets the file dialog service</summary>
        [Import(AllowDefault = true)]
        protected IFileDialogService FileDialogService { get; set; }

        /// <summary>
        /// Gets or sets the main form</summary>
        [Import(AllowDefault = true)]
        protected Form MainForm
        {
            get { return m_mainForm; }
            set
            {
                // we'd have to unsubscribe from the Load event and maybe reskin?
                if (m_mainForm != null && m_mainForm != value)
                    throw new InvalidOperationException("setting the MainForm multiple times is not currently supported");
                m_mainForm = value;
                m_mainForm.Load += m_mainForm_Load;                
                m_mainForm.FormClosed  += m_mainForm_FormClosed;                
                var ncRenderer = new FormNcRenderer(m_mainForm);
                ncRenderer.Skin = s_ncSkin;
                ncRenderer.CustomPaintDisabled = ActiveSkin == null;
                s_formNcRenderers.Add(m_mainForm, ncRenderer);               
            }
        }
        
        /// <summary>
        /// Gets or sets the most-recently-used skin file. Setting this has the effect of loading the
        /// skin file, making it the active skin, and applying it to the main form and skinnable controls.</summary>
        protected string MruSkinFile
        {
            get
            {
                return m_mruSkinFile;
            }
            set
            {
              if (string.IsNullOrWhiteSpace(value) || String.CompareOrdinal(m_mruSkinFile, value) == 0)
                    return;

                //Trace.WriteLine("MruSkinFile set, probably by settings service", "SkinService");
                m_mruSkinFile = value;
                m_settingsLoaded = true; //assume the settings service called us
                if (ActiveSkin == null ||
                    String.CompareOrdinal(ActiveSkin.SkinFile, m_mruSkinFile) != 0)
                {
                    // In Legacy ATF, only OpenSkinFile() needs to be called because the settings are loaded before
                    //  the MainForm.Load event is raised. But in ATF 3 apps, the MainForm.Load event (mostly?) fires
                    //  first and then the settings are loaded!
                    if (m_mainFormLoaded)
                        OpenAndApplySkin(m_mruSkinFile);                        
                    else
                        OpenSkinFile(m_mruSkinFile);
                    SkinsDirectory = Directory.GetParent(m_mruSkinFile).FullName;
                }
            }
        }

        #region Private

        private void SkinEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_skinEditor = null;
            m_mainForm.FormClosing -= m_mainForm_FormClosing;
        }

        private void m_mainForm_Load(object sender, EventArgs e)
        {
            //Trace.WriteLine("m_mainForm_Load", "SkinService");
            m_mainFormLoaded = true;
            if (m_settingsLoaded)
                ApplyActiveSkin();            
        }

        void m_mainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Unhook everything to avoid a possible freeze in WinFormsUtil while the app shuts down.
            WinFormsUtil.WindowCreated -= WindowCreated;
            WinFormsUtil.WindowDestroyed -= WindowDestroyed;            
        }

        void m_mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel) return;
            if (m_skinEditor != null)
            {
                bool confirmClose = m_skinEditor.ConfirmCloseActiveDocument();
                e.Cancel = !confirmClose;
                if(confirmClose)
                    m_skinEditor.Close();               
                
            }
        }

        #if TRACE_SKINSERVICE
        private static Skin ActiveSkin
        {
            get { return s_activeSkin; }
            set
            {
                if (s_activeSkin != value)
                {
                    s_activeSkin = value;
                    s_skinnedControls.Clear();
                }
            }
        }
        #else
        private static Skin ActiveSkin
        {
            get;
            set;
        }
        #endif

        private static void WindowCreated(Form form)
        {            
            ApplyActiveSkin(form, null);
            s_skinnableObjects.Add(new WeakKey<object>(form));

            var ncRenderer = new FormNcRenderer(form);
            ncRenderer.Skin = s_ncSkin;
            ncRenderer.CustomPaintDisabled = ActiveSkin == null;
            s_formNcRenderers.Add(form, ncRenderer);
        }
        private static void WindowDestroyed(Form form)
        {
            s_skinnableObjects.Remove(new WeakKey<object>(form));
            s_formNcRenderers.Remove(form);
        }

        private void LoadSkin(XmlDocument xmlDoc)
        {           
            ActiveSkin.Styles.Clear();
            var rootStyles = new List<SkinStyle>();

            // iterate through the document and populate the skin list with styles
            XmlElement root = xmlDoc.DocumentElement;

            // get all the styles
            XmlNodeList styles = root.SelectNodes(StyleElement);
            
            if (styles == null)
                throw new FileFormatException("Error loading the skin file.");

            foreach (XmlElement style in styles)
            {                
                try
                {
                    string targetTypeValue = style.GetAttribute(TargetTypeAttribute);
                    Type targetType = GetTypeFromString(targetTypeValue);

                    // It's not really an error if TargetType doesn't exist, because the same skin file
                    //  can be used by multiple apps that have different types defined.
                    if (targetType != null)
                    {
                        var styleDef = new SkinStyle(targetType);
                        styleDef.Setters.AddRange(GetSetters(style));
                        InsertStyleIntoTree(styleDef, rootStyles);
                    }
                }
                catch (Exception ex)
                {
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                }
            }

            // Make each SkinStyle contain all the Setters for both itself and from its base SkinStyle (if any).
            SetInheritedSetters(new List<Setter>(), rootStyles);

            if (rootStyles.Count > 0)
                ActiveSkin.Styles.AddRange(rootStyles);
            else
                ActiveSkin = null;
            
        }

        /// <summary>
        /// Inserts newStyle into a dependency tree</summary>
        /// <param name="newStyle">A SkinStyle that is not currently in the tree</param>
        /// <param name="roots">The sibling roots at this current level of the tree. These must be
        /// independent of each other (i.e., they must not represent subclasses of each other).
        /// This list may be modified to add or remove styles.</param>
        private static void InsertStyleIntoTree(SkinStyle newStyle, List<SkinStyle> roots)
        {
            // We may need to make newStyle a child of one of the roots.
            foreach (SkinStyle root in roots)
            {
                if (newStyle.TargetType.IsSubclassOf(root.TargetType))
                {
                    InsertStyleIntoTree(newStyle, root.Dependents);
                    return;
                }
            }

            // Or we may need to make one or more of 'roots' be children of 'newStyle' and to add
            //  newStyle to the list of roots.
            // Or newStyle is independent of existing roots and should simply be added to the list
            //  of roots.
            int numRemaining = 0;
            for(int i = 0; i < roots.Count; i++)
            {
                SkinStyle root = roots[i];
                if (root.TargetType.IsSubclassOf(newStyle.TargetType))
                    newStyle.Dependents.Add(root);
                else
                    roots[numRemaining++] = root;
            }
            roots.RemoveRange(numRemaining, roots.Count - numRemaining);
            roots.Add(newStyle);
        }

        /// <summary>
        /// Propagates the list of setters to 'roots', adding to each root's list of setters unless
        /// the root already contains a setter that targets the same property.</summary>
        /// <param name="inheritedSetters"></param>
        /// <param name="roots"></param>
        private static void SetInheritedSetters(List<Setter> inheritedSetters, List<SkinStyle> roots)
        {
            foreach (SkinStyle root in roots)
            {
                foreach (Setter inheritedSetter in inheritedSetters)
                {
                    bool overridden = false;
                    foreach (Setter derivedSetter in root.Setters)
                    {
                        if (inheritedSetter.PropertyName == derivedSetter.PropertyName)
                        {
                            overridden = true;
                            break;
                        }
                    }
                    if (!overridden)
                        root.Setters.Add(inheritedSetter);
                }
                SetInheritedSetters(root.Setters, root.Dependents);
            }
        }

        private IEnumerable<Setter> GetSetters(XmlElement parentElement)
        {
            var setters = new List<Setter>();

            // get a list of setters in each style
            XmlNodeList setterNodes = parentElement.SelectNodes(SetterElement);

            // skip over empty styles
            if (setterNodes == null || setterNodes.Count == 0)
                return setters;

            foreach (XmlElement xmlElement in setterNodes)
            {
                string propertyName = xmlElement.GetAttribute(PropertyNameAttribute);

                // validate that there is exactly one value child, or list child, of a setter
                XmlNodeList valueNodes = xmlElement.SelectNodes(ValueInfoElement);
                XmlNodeList listNodes = xmlElement.SelectNodes(ListInfoElement);

                if (valueNodes != null && valueNodes.Count > 0)
                {
                    if (listNodes != null && listNodes.Count > 0)
                        throw new FileFormatException("Setter cannot define both a ValueInfo and ListInfo.");//don't localize --Ron

                    if (valueNodes.Count != 1)
                        throw new FileFormatException("Setter can specify no more than one ValueInfo");//don't localize --Ron

                    ValueInfo value = GetValue((XmlElement)valueNodes[0]);
                    setters.Add(new Setter(propertyName, value));
                }
                else if (listNodes != null)
                {
                    if (listNodes.Count != 1)
                        throw new FileFormatException("Setter can specify no more than one ListInfo");//don't localize --Ron

                    ListInfo list = GetList((XmlElement)listNodes[0]);
                    setters.Add(new Setter(propertyName, list));
                }
                else
                    throw new FileFormatException("Each setter must specify one ValueInfo, or one ListInfo");//don't localize --Ron
            }

            return setters;
        }

        private ValueInfo GetValue(XmlElement valueElement)
        {
            var value = new ValueInfo { Type = GetValueType(valueElement) };
            value.Converter = GetValueConverter(valueElement, value.Type);
            
            // get the constructor params and setters for the value
            value.ConstructorParams.AddRange(GetValueConstructorParams(valueElement));
            value.Setters.AddRange(GetSetters(valueElement));

            // if there are no constructorParams or setters,
            // look for the value of the XmlElement
            if (value.Setters.Count == 0 && value.ConstructorParams.Count == 0)
            {
                value.Value = valueElement.GetAttribute(ValueAttribute);

                // this is the final value, so if we still don't have a converter then it's an error.
                if (value.Converter == null)
                    throw new FileFormatException("There is no converter for the type \"" + value.Type + "\"");//don't localize --Ron
            }

            return value;
        }

        private ListInfo GetList(XmlElement listElement)
        {
            var list = new ListInfo();
            list.Values.AddRange(from XmlElement valueElement
                                    in listElement.SelectNodes(ValueInfoElement)
                                 select GetValue(valueElement));
            return list;
        }

        private IEnumerable<ValueInfo> GetValueConstructorParams(XmlElement valueElement)
        {
            var constructorParams = new List<ValueInfo>();
            XmlNodeList constructorParamNodes = valueElement.SelectNodes(ConstructorParamsElement);

            if (constructorParamNodes==null || constructorParamNodes.Count > 1)
                throw new FileFormatException("This skin file is corrupt.");

            foreach (XmlElement constructorParamElement in constructorParamNodes)
            {
                constructorParams.AddRange(from XmlElement constructorParamValue 
                                           in constructorParamElement.SelectNodes(ValueInfoElement) 
                                           select GetValue(constructorParamValue));
            }

            return constructorParams;
        }

        private static Type GetTypeFromString(string typeString)
        {
            if (String.IsNullOrEmpty(typeString))
                return null;

            // First check for very common type names and custom names.
            switch(typeString)
            {
                case "System.Drawing.Color": //very common
                    return typeof(Color);

                case "string": //short for "System.String"
                    return typeof(string);

                case "int": //short for "System.Int32"
                    return typeof(int);

                case "float": //short for "System.Single"
                    return typeof(float);

                case "char": //short for "System.Char"
                    return typeof(char);

                case "byte": //short for "System.Byte"
                    return typeof(byte);
            }

            Type type;
            if (s_stringToType.TryGetValue(typeString, out type))
                return type;

            // We don't require the type name to be an assembly qualified name, so we need to search
            //  each loaded assembly instead of using Type.GetType(string).
            foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeString);
                if (type != null)
                    break;
            }

            s_stringToType.Add(typeString, type);
            return type;
        }
        
        private Type GetSetterType(XmlElement setterElement)
        {
            var parentElement = setterElement.ParentNode as XmlElement;
            string propertyName = setterElement.GetAttribute(PropertyNameAttribute);

            // parent must be either a style type or a value type
            bool parentIsStyle = (String.CompareOrdinal(parentElement.Name, StyleElement) == 0);
            
            if (parentIsStyle)
            {
                string styleTargetTypeString = parentElement.GetAttribute(TargetTypeAttribute);
                Type styleTargetType = GetTypeFromString(styleTargetTypeString);

                if (styleTargetType == null)
                    throw new TypeLoadException(string.Format("{0} doesn't exist in this application. Skin cannot load.", styleTargetTypeString));

                PropertyInfo propertyInfo = styleTargetType.GetProperty(propertyName, PropertyLookupType);
                return propertyInfo.PropertyType;
            }
            else
            {
                // parent is a value type
                Type valueType = GetValueType(parentElement);
                PropertyInfo propertyInfo = valueType.GetProperty(propertyName, PropertyLookupType);
                return propertyInfo.PropertyType;
            }
        }

        private Type GetValueType(XmlElement valueElement)
        {
            // To find the type of the value, we resolve the type in this order:
            //    1. a "type" attribute on the value element. if the value's parent is
            //        "constructorParams" and it does not have a type attribute, we error out.    
            //    2. the type of the property specified by "propertyName" of the parent setter.
            var parentElement = valueElement.ParentNode as XmlElement;
            string typeString = valueElement.GetAttribute(TypeAttribute);

            if (String.IsNullOrEmpty(typeString) && String.CompareOrdinal(parentElement.Name, ConstructorParamsElement) == 0)
                throw new FileFormatException("Constructor parameters must have their type explicitly specified in the skin file.");

            // If GetTypeFromString returns null, search the parent for the type. 
            // Note the parent must be a setter type.
            Type valueType = GetTypeFromString(typeString) ?? GetSetterType(parentElement);

            return valueType;
        }

        private static TypeConverter GetValueConverter(XmlElement valueElement, Type destinationType)
        {
            // Get the converter for the value, checking for an explicit converter first.
            TypeConverter converter = null;
            Type converterType = GetTypeFromString(valueElement.GetAttribute(ConverterAttribute));
            if (converterType != null)
                converter = Activator.CreateInstance(converterType) as TypeConverter;

            // If an explicit converter was not found, see if our default converter will work.
            if (converter == null && s_defaultTypeConverter.CanConvertTo(null, destinationType))
                converter = s_defaultTypeConverter;

            return converter;
        }

        private static object GetInstance(object obj, PropertyInfo propertyInfo, ValueInfo valueInfo)
        {
            // Convert a ValueInfo to an actual object instance
            object instance = null;

            // If the property on 'obj' already exists and doesn't require special constructor
            //  parameters, then re-use it. This fixes problems when the property has been set
            //  already using an object of a derived type, in which case the skin file should
            //  not try to re-create the object, but should instead just set properties on the
            //  existing object.
            if (propertyInfo != null && propertyInfo.CanRead && valueInfo.ConstructorParams.Count == 0)
                instance = propertyInfo.GetValue(obj, null);

            if (instance == null)
            {
                // We must create the object for the property.
                if (valueInfo.ConstructorParams.Count == 0)
                {
                    instance = valueInfo.Type == typeof (string)
                        ? valueInfo.Value.Clone()
                        : Activator.CreateInstance(valueInfo.Type);
                }
                else
                {
                    Type[] paramTypeArray = valueInfo.ConstructorParams.Select(param => param.Type).ToArray();
                    ConstructorInfo constructor = valueInfo.Type.GetConstructor(paramTypeArray);

                    if (constructor != null)
                        instance =
                            constructor.Invoke(valueInfo.ConstructorParams.Select(
                            param => GetInstance(null, null, param)).ToArray());
                }
            }

            // Then set any properties
            foreach (Setter setter in valueInfo.Setters)
            {
                PropertyInfo childPropertyInfo = valueInfo.Type.GetProperty(setter.PropertyName, PropertyLookupType);
                if (childPropertyInfo == null)
                {
                    Outputs.WriteLine(OutputMessageType.Warning,
                                        "The skin " + ActiveSkin.SkinFile +
                                        " attempted to set a property on an object of type " + valueInfo.Type.FullName +
                                        ", but this property, " + setter.PropertyName + ", doesn't exist.");
                    continue;
                }

                if (setter.ValueInfo != null)
                    childPropertyInfo.SetValue(instance, GetInstance(instance, childPropertyInfo, setter.ValueInfo), null);
                else if (setter.ListInfo != null)
                    childPropertyInfo.SetValue(instance, GetInstance(setter.ListInfo), null);
                else
                    throw new InvalidOperationException("Setter '" + setter.PropertyName + "' doesn't have a valueInfo, nor listInfo, specified.  Must have one (and only one) of either.");
            }

            if (valueInfo.Setters.Count == 0 && valueInfo.Value != null)
            {
                // this is a built-in type and we need to go ahead and set the value
                // of the instance using the converter
                if(valueInfo.Converter.CanConvertTo(null, valueInfo.Type))
                    instance = valueInfo.Converter.ConvertTo(null,CultureInfo.InvariantCulture,valueInfo.Value, valueInfo.Type);
            }
            
            return instance;
        }

        private static object GetInstance(ListInfo listInfo)
        {
            // Convert a ListInfo to an actual List instance
            object instance = null;

            if (listInfo.Values.Count < 1)
                return instance;

            Type typedListType = typeof(List<>).MakeGenericType(listInfo.Values[0].Type);
            instance = Activator.CreateInstance(typedListType);

            var addMethod = typedListType.GetMethod("Add");
            foreach (ValueInfo item in listInfo.Values)
            {
                var newEntry = GetInstance(null, null, item);
                addMethod.Invoke(instance, new object[] { newEntry });
            }
            
            return instance;
        }

        private static void RestoreOriginalPropertyValues()
        {
            // Make a note of any keys whose Control has been garbage-collected.
            var keysToRemove = new List<Tuple<WeakKey<object>, PropertyInfo>>();

            foreach (KeyValuePair<Tuple<WeakKey<object>, PropertyInfo>, object> keyValue in s_originalPropertyValues)
            {
                Tuple<WeakKey<object>, PropertyInfo> tuple = keyValue.Key;
                
                object control = tuple.Item1.Target;
                if (control != null)
                {
                    if (control.Is<Control>() && control.As<Control>().IsDisposed)
                    {
                        keysToRemove.Add(tuple);
                        continue;
                    }

                    tuple.Item2.SetValue(control, keyValue.Value, null);

                    // ToolStrips and MenuStrips are controls, but none of their contents are.
                    // Since it is essential that they be skinnable, we need to custom handle them here.
                    var toolStrip = control as ToolStrip;
                    if (toolStrip != null)
                        ApplyCommonValuesToToolStrip(toolStrip);
                }
                else
                {
                    keysToRemove.Add(tuple);
                }
            }

            // Remove any keys whose Control has been garbage-collected.
            foreach (Tuple<WeakKey<object>, PropertyInfo> key in keysToRemove)
                s_originalPropertyValues.Remove(key);
        }

        private static void ApplyActiveSkin(Object control, HashSet<Object> skinnedControls)
        {
            // Whether or not we have a skin, let's keep a hold of this so that we can skin
            //  it once a skin has been loaded. This is promised behavior. We want to allow
            //  clients to call ApplyActiveSkin before a skin has loaded.
            s_skinnableObjects.Add(new WeakKey<object>(control));

            if (ActiveSkin == null)
                return;

            // Save the original property values first before applying any new ones because it appears
            //  that applying the new values to parent Controls might affect their children. The ability
            //  to reset all the Controls no longer works if we save the original property values at the
            //  last possible moment.
            SaveOriginalPropertyValues(control);
            ApplyNewPropertyValues(control, skinnedControls);
        }

        private static void SaveOriginalPropertyValues(Object obj)
        {
            if (obj == null)
                return;

            var control = obj as Control;
            if (control != null)
                control.SuspendLayout();
            

            Type controlType = obj.GetType();

            // Save off all the current property values that the skin wants to modify.
            SkinStyle style = FindBestSkinStyle(controlType, ActiveSkin.Styles);
            if (style != null)
            {
                Type styleType = style.TargetType;

                foreach (Setter setter in style.Setters)
                    SaveOriginalPropertyValues(obj, setter);
            }

            if (control != null)
            {
                foreach (Control child in control.Controls)
                    SaveOriginalPropertyValues(child);
                control.ResumeLayout();
            }
        }

        private static void SaveOriginalPropertyValues(object obj, Setter setter)
        {
            Type objType = obj.GetType();

            // There's no point in saving the property value of an object that is a value type,
            //  because each boxed instance of a value type is unique.
            if (objType.IsValueType)
                return;

            try
            {
                // Save off the old property value. The desired type (Setter.ValueInfo.Type) is
                //  allowed to be a derived type of obj's type, so the setter's PropertyName may
                //  not exist on 'obj'.
                PropertyInfo propertyInfo = objType.GetProperty(setter.PropertyName, PropertyLookupType);
                if (propertyInfo != null)
                {
                    var tuple = new Tuple<WeakKey<object>, PropertyInfo>(
                        new WeakKey<object>(obj), propertyInfo);
                    if (!s_originalPropertyValues.ContainsKey(tuple))
                    {
                        object propertyValue = propertyInfo.GetValue(obj, null);

                        // 'null' is a valid property value in some cases.
                        if (propertyValue != null &&
                            typeof(ICloneable).IsAssignableFrom(propertyInfo.PropertyType))
                        {
                            propertyValue = ((ICloneable)propertyValue).Clone();
                        }

                        s_originalPropertyValues.Add(tuple, propertyValue);

                        if (setter.ValueInfo != null)
                        {
                            foreach (Setter childSetter in setter.ValueInfo.Setters)
                                SaveOriginalPropertyValues(propertyValue, childSetter);
                        }
                    }
                }
                else
                {
                    Outputs.WriteLine(OutputMessageType.Warning,
                                        "The skin " + ActiveSkin.SkinFile +
                                        " attempted to set a property on an object of type " + objType +
                                        ", but this property, " + setter.PropertyName + ", doesn't exist.");
                }
            }
            catch (Exception)
            {
                // we don't want to error out
            }
        }

        private void ApplySkinToNonClientArea()
        {                
            if (ActiveSkin == null) return;
            SkinStyle style = null;
            foreach (SkinStyle st in ActiveSkin.Styles)
            {
                if (s_ncSkin.GetType() == st.TargetType)
                    style = st;
            }
            if (style != null)
            {
                foreach (Setter setter in style.Setters)
                {
                    // set the property to the new value
                    PropertyInfo propertyInfo = style.TargetType.GetProperty(setter.PropertyName, PropertyLookupType);
                    object newPropertyValue;
                    if (setter.ValueInfo != null)
                        newPropertyValue = GetInstance(s_ncSkin, propertyInfo, setter.ValueInfo);
                    else if (setter.ListInfo != null)
                        newPropertyValue = GetInstance(setter.ListInfo);
                    else
                        throw new Exception("Setter '" + setter.PropertyName + "' does not have its ValueInfo nor ListInfo set");

                    propertyInfo.SetValue(s_ncSkin, newPropertyValue, null);
                }
                foreach (var nc in s_formNcRenderers.Values)
                {
                    nc.Skin = s_ncSkin;
                    nc.CustomPaintDisabled = false;
                }
            }
            else
            {
                foreach (var nc in s_formNcRenderers.Values)
                    nc.CustomPaintDisabled = true;
            }
        }

        private static void ApplyNewPropertyValues(object obj, HashSet<object> skinnedControls)
        {
            if (obj == null)
                return;

            var control = obj as Control;            
            if (control != null)
                control.SuspendLayout();

            // Only set the properties if this is the first time we've seen this Control.
            if (skinnedControls == null || skinnedControls.Add(obj))
            {
                #if TRACE_SKINSERVICE
                if (!s_skinnedControls.Add(control))
                    Trace.WriteLine("DUPLICATE CONTROL WOULD NOT HAVE BEEN CAUGHT: " + control, "SkinService");
                #endif
                
                Type controlType = obj.GetType();

                // Apply the best matching SkinStyle, if any.
                SkinStyle style = FindBestSkinStyle(controlType, ActiveSkin.Styles);
                if (style != null)
                {
                    foreach (Setter setter in style.Setters)
                    {
                        // set the property to the new value
                        PropertyInfo propertyInfo = style.TargetType.GetProperty(setter.PropertyName, PropertyLookupType);
                        object newPropertyValue;
                        if (setter.ValueInfo != null)
                            newPropertyValue = GetInstance(obj, propertyInfo, setter.ValueInfo);
                        else if (setter.ListInfo != null)
                            newPropertyValue = GetInstance(setter.ListInfo);
                        else
                            throw new Exception("Setter '" + setter.PropertyName + "' does not have its ValueInfo nor ListInfo set");

                        propertyInfo.SetValue(obj, newPropertyValue, null);
                    }
                }

                // This is an inaccessible piece of our underlying docking framework.
                // The BackColor should always be transparent for this.
                if (control != null &&
                    controlType == s_inertButtonType)
                    control.BackColor = Color.Transparent;

                // ToolStrips and MenuStrips are controls, but none of their contents are.
                // Since it is essential that they be skinnable, we need to custom handle them here.
                if (obj is ToolStrip)
                    ApplyCommonValuesToToolStrip((ToolStrip)obj);
            }

            // Children, such as menus and toolbars, may have been added since ActiveSkin last changed.
            if (control != null)
            {
                foreach (Control child in control.Controls)
                {                                     
                    ApplyNewPropertyValues(child, skinnedControls);
                }

                control.ResumeLayout();
            }
        }

        // Finds the best matching skin style for the given target type, or null.
        private static SkinStyle FindBestSkinStyle(Type targetType, List<SkinStyle> roots)
        {
            //todo: do we want to cache the result? We'd have to clear the cache when the skin changes.
            foreach (SkinStyle root in roots)
            {
                if (targetType == root.TargetType)
                    return root;
                if (targetType.IsSubclassOf(root.TargetType))
                {
                    SkinStyle derived = FindBestSkinStyle(targetType, root.Dependents);
                    if (derived != null)
                        return derived;
                    return root;
                }
            }
            return null;
        }

        private static void ApplyCommonValuesToToolStrip(ToolStrip toolStrip)
        {
            ApplyCommonValuesToToolStrips(toolStrip.Items, toolStrip.Font, toolStrip.BackColor, toolStrip.ForeColor);
        }

        private static void ApplyCommonValuesToToolStrips(IEnumerable items, Font font, Color backColor, Color foreColor)
        {
            foreach (ToolStripItem item in items)
            {
                item.ForeColor = foreColor;
                item.BackColor = backColor;
                item.Font = font;

                if (item is ToolStripDropDownItem)
                    ApplyCommonValuesToToolStrips(((ToolStripDropDownItem)item).DropDownItems, font, backColor, foreColor);
            }
        }

        private class DefaultTypeConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(Double) ||
                    destinationType == typeof(Single) ||
                    destinationType.IsEnum ||
                    destinationType == typeof(int) ||
                    destinationType == typeof(byte) ||
                    destinationType == typeof(bool) ||
                    destinationType == typeof(Color) ||
                    destinationType == typeof(string))
                        return true;

                return false;
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if(value.GetType() != typeof(string))
                    throw new InvalidOperationException("Can only convert from strings to other representations.");
                
                // We determine what to do based on type of the destination property.
                if (destinationType == typeof(Double))
                    return new DoubleConverter().ConvertTo(null, culture, value, destinationType);
                if (destinationType == typeof(Single))
                    return new SingleConverter().ConvertTo(null, culture, value, destinationType);
                if (destinationType.IsEnum)
                    return Enum.Parse(destinationType, (string)value);
                if (destinationType == typeof(int))
                    return int.Parse((string)value, NumberStyles.Integer, culture);
                if (destinationType == typeof(byte))
                    return new ByteConverter().ConvertTo(null, culture, value, destinationType);
                if (destinationType == typeof(bool))
                    return bool.Parse((string)value);
                if (destinationType == typeof(Color))
                {
                    // special case to handle "System.Drawing.Color.Empty"
                    if (string.Compare((string)value, "Empty", false) == 0)
                        return Color.Empty;

                    return new ColorConverter().ConvertFromString(null, culture, (string)value);
                }
                if (destinationType == typeof(string))
                    return value;

                return null;
            }
        }

        private class Skin
        {
            public string SkinFile;
            public List<SkinStyle> Styles = new List<SkinStyle>(); 
        }

        private class SkinStyle
        {
            public SkinStyle(Type targetType)
            {
                if (targetType == null)
                    throw new ArgumentNullException();
                TargetType = targetType;
            }

            public readonly Type TargetType; //is never null
            public readonly List<Setter> Setters = new List<Setter>(); //will contain all relevant Setters of any base SkinStyle
            public readonly List<SkinStyle> Dependents = new List<SkinStyle>();//SkinStyles whose TargetType is a sub-class of our TargetType

            // For debugging, it's convenient to see the target type.
            public override string ToString()
            {
                return "TargetType = " + TargetType;
            }
        }

        private class Setter
        {
            public Setter(string propertyName, ValueInfo valueInfo)
            {
                PropertyName = propertyName;
                ValueInfo = valueInfo;
            }

            public Setter(string propertyName, ListInfo listInfo)
            {
                PropertyName = propertyName;
                ListInfo = listInfo;
            }

            public readonly string PropertyName;
            public readonly ValueInfo ValueInfo;
            public readonly ListInfo ListInfo;

            // For debugging, it's convenient to see the property name.
            public override string ToString()
            {
                return "PropertyName = " + PropertyName;
            }
        }

        private class ValueInfo
        {
            public Type Type;
            public TypeConverter Converter;
            public string Value;
            public readonly List<ValueInfo> ConstructorParams = new List<ValueInfo>();
            public readonly List<Setter> Setters = new List<Setter>();
        }

        private class ListInfo
        {
            public List<ValueInfo> Values = new List<ValueInfo>();
        }

        #endregion

        private Form m_mainForm;
        private string m_mruSkinFile;
        private bool m_settingsLoaded;
        private bool m_mainFormLoaded;
        private bool m_showCommands = true;

        private SkinEditor m_skinEditor;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;
        
        private static readonly DefaultTypeConverter s_defaultTypeConverter = new DefaultTypeConverter();

        // form to NcRenderer map.
        private static Dictionary<Form, FormNcRenderer>
            s_formNcRenderers = new Dictionary<Form, FormNcRenderer>();

        private static FormNcRenderer.SkinInfo
            s_ncSkin = new FormNcRenderer.SkinInfo();
        // This HashSet contains all Controls of this app that have native handles (and so are visible)
        //  that were created after our system hooks were put in place. So, it may not contain MainForm.
        //  When a Control's native handle is disposed of, then it will be removed from here. Other
        //  objects that are not Controls can be passed in, so let's use a WeakReference. Ideally, this
        //  would not contain any of the children of any Form that is also in the HashSet.
        private static readonly HashSet<WeakKey<object>> s_skinnableObjects = new HashSet<WeakKey<object>>();
        
        private static readonly Dictionary<Tuple<WeakKey<object>, PropertyInfo>, object> s_originalPropertyValues =
            new Dictionary<Tuple<WeakKey<object>, PropertyInfo>, object>();
        private static readonly Dictionary<string, Type> s_stringToType = new Dictionary<string, Type>();
        private static readonly DocumentClientInfo s_info = new DocumentClientInfo(
            "Skin",
            new[] { ".skn" },
            null,
            null,
            false);
        private static readonly Type s_inertButtonType =
            Type.GetType("WeifenLuo.WinFormsUI.Docking.VS2005DockPaneCaption+InertButton, WeifenLuo.WinFormsUI.Docking");
        
        #if TRACE_SKINSERVICE
        //Controls that have been skinned by the ActiveSkin. Needs to be cleared each time s_activeSkin changes.
        private static readonly HashSet<Control> s_skinnedControls = new HashSet<Control>();
        private static Skin s_activeSkin;
        #endif

        /// <summary>
        /// String for embedded skin</summary>
        protected const string EmbeddedSkinString = "Embedded Skin";

        private const string UnsavedSkinString = "Unsaved Skin";
        private const string SkinSchema = "Sce.Atf.Applications.SkinService.Schemas.skin.xsd";
        private const string ValueInfoElement = "valueInfo";
        private const string ListInfoElement = "listInfo";
        private const string SetterElement = "setter";
        private const string ConstructorParamsElement = "constructorParams";
        private const string StyleElement = "style";
        private const string PropertyNameAttribute = "propertyName";
        private const string ValueAttribute = "value";
        private const string TypeAttribute = "type";
        private const string TargetTypeAttribute = "targetType";
        //private const string NameAttribute = "name";
        private const string ConverterAttribute = "converter";
        private const BindingFlags PropertyLookupType = BindingFlags.Instance | 
                                                        BindingFlags.Static   | 
                                                        BindingFlags.Public;
    }
}
