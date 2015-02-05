//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service that manages user editable settings (preferences) and application settings persistence</summary>
    [Export(typeof(ISettingsService))]
    [Export(typeof(SettingsService))]
    [Export(typeof(ISettingsPathsProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SettingsService : ISettingsService, ICommandClient, IPartImportsSatisfiedNotification, ISettingsPathsProvider
    {
        /// <summary>
        /// Constructor</summary>
        public SettingsService()
        {
            Assembly assembly = Assembly.GetEntryAssembly();

            // Can be null if called from unmanaged code, like in UnitTests.
            if (assembly == null)
                assembly = Assembly.GetExecutingAssembly();

            AssemblyName assemblyName = assembly.GetName();
            m_applicationName = assemblyName.Name;
            Version version = assemblyName.Version;
            m_versionString = version.Major + "." + version.Minor;

            string startupPath = Path.GetDirectoryName(new Uri(assemblyName.CodeBase).LocalPath);
            m_defaultSettingsPath = Path.Combine(startupPath, "DefaultSettings.xml");

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            m_settingsPath = string.Format("{0}\\{1}\\{2}\\AppSettings.xml", appDataPath, m_applicationName, m_versionString);

            SplitterRatio = 0.33f;
            RegisterSettings(
                "6CF685C0-D063-4F0C-B385-B8D70875BB81",
                new BoundPropertyDescriptor(this, () => SplitterRatio, "SplitterRatio", "", ""));
        }

        /// <summary>
        /// Gets or sets the file path where settings data should be persisted</summary>
        /// <remarks>Path is initialized to Environment.SpecialFolder.ApplicationData\Application.ProductName\Version\AppSettings.xml</remarks>
        public string SettingsPath
        {
            get { return m_settingsPath; }
            set { m_settingsPath = value; }
        }

        /// <summary>
        /// Gets or sets the file path where default settings data can be found. Default data is used
        /// when no persisted settings data can be found.</summary>
        /// <remarks>Path is initialized to Application.StartupPath\DefaultSettings.xml</remarks>
        public string DefaultSettingsPath
        {
            get { return m_defaultSettingsPath; }
            set { m_defaultSettingsPath = value; }
        }

        /// <summary>
        /// Gets or sets whether the user can edit settings</summary>
        /// <remarks>Default is true</remarks>
        public bool AllowUserEdits
        {
            get { return m_allowUserEdits; }
            set { m_allowUserEdits = value; }
        }

        /// <summary>
        /// Gets or sets whether the user can load and save settings files</summary>
        /// <remarks>Default is true</remarks>
        public bool AllowUserLoadSave
        {
            get { return m_allowUserLoadSave; }
            set { m_allowUserLoadSave = value; }
        }

        /// <summary>
        /// Gets or sets the current state of all properties (Memento pattern)</summary>
        public object State
        {
            get
            {
                MemoryStream stream = new MemoryStream();
                Serialize(stream);
                return stream;
            }
            set
            {
                MemoryStream stream = value as MemoryStream;
                if (stream == null)
                    throw new ArgumentException("Not a valid memento");
                stream.Position = 0;
                Deserialize(stream);
            }
        }

        /// <summary>
        /// Gets or sets the current state of all properties that are user settable (Memento pattern)</summary>
        public object UserState
        {
            get
            {
                // Collect all the pairs of property descriptors and the current value of that property
                List<Pair<PropertyDescriptor, object>> values = new List<Pair<PropertyDescriptor, object>>();
                foreach (PropertyDescriptor propertyDescriptor in UserPropertyDescriptors)
                    values.Add(new Pair<PropertyDescriptor, object>(propertyDescriptor, propertyDescriptor.GetValue(null)));
                return values;
            }
            set
            {
                // Restore the state by setting the properties to their former values
                List<Pair<PropertyDescriptor, object>> values = (List<Pair<PropertyDescriptor, object>>)value;
                foreach (Pair<PropertyDescriptor, object> pair in values)
                    pair.First.SetValue(null, pair.Second);
            }
        }

        /// <summary>
        /// Sets all settings to their default values</summary>
        /// <remarks>Settings that have no default are not changed</remarks>
        public void SetDefaults()
        {
            foreach (SettingsInfo info in m_settings.Values)
            {
                foreach (SettingsInfo.Setting setting in info.Settings.Values)
                {
                    if (setting.PropertyDescriptor != null &&
                        setting.PropertyDescriptor.CanResetValue(null))
                    {
                        setting.PropertyDescriptor.ResetValue(null);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the fraction of the width of this control devoted to the left panel (which shows
        /// the setting categories). This value should be between [0,1]. The default is 0.33f.</summary>
        public float SplitterRatio
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether or not the left tree control navigation
        /// should follow the Windows Explorer model</summary>     
        public TreeControl.KeyboardShortcuts NavigationBehavior
        {
            get { return m_navigationBehavior; }
            set { m_navigationBehavior = value; }
        }

        #region IPartImportsSatisfiedNotification Members

        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            if (m_mainWindow == null &&
                m_mainForm != null)
            {
                m_mainWindow = new MainFormAdapter(m_mainForm);
            }

            if (m_mainWindow == null)
                throw new InvalidOperationException("Can't get main window");

            m_mainWindow.Loading += mainWindow_Loaded;
            m_mainWindow.Closed += mainWindow_Closed;

            string settingsDirectory = Path.GetDirectoryName(m_settingsPath);
            if (!Directory.Exists(settingsDirectory))
                Directory.CreateDirectory(settingsDirectory);
        }

        #endregion

        #region ISettingsService Members

        /// <summary>
        /// Registers persistent application settings</summary>
        /// <param name="uid">Unique identifier for settings</param>
        /// <param name="settings">The property descriptors that have the ability to get and set the
        /// setting when 'null' is passed into their GetValue and SetValue methods.
        /// BoundPropertyDescriptor does this.</param>
        public void RegisterSettings(string uid, params PropertyDescriptor[] settings)
        {
            SettingsInfo settingsInfo;
            if (!m_settings.TryGetValue(uid, out settingsInfo))
            {
                settingsInfo = new SettingsInfo(uid);
                m_settings.Add(uid, settingsInfo);
            }

            foreach (PropertyDescriptor descriptor in settings)
                settingsInfo.Add(descriptor);
        }

        /// <summary>
        /// Registers settings that can be presented to the user for editing. These are not
        /// persisted unless RegisterSettings() is also called.</summary>
        /// <param name="pathName">Path to settings</param>
        /// <param name="settings">The property descriptors that have the ability to get and set the
        /// setting when 'null' is passed into their GetValue and SetValue methods.
        /// BoundPropertyDescriptor does this.</param>
        public void RegisterUserSettings(string pathName, params PropertyDescriptor[] settings)
        {
            if (string.IsNullOrEmpty(pathName))
                throw new ArgumentException("pathName");

            string[] path = pathName.Split(s_delimiters, 16);

            // get root folder
            Tree<object> folder = m_userSettings;

            // for each subsequent segment of the path, get folder
            for (int i = 0; i < path.Length - 1; i++)
                folder = GetOrCreateFolder(path[i], folder);

            // get the node that should hold the settings, if it already exists
            string name = path[path.Length - 1];
            UserSettingsInfo existing = null;
            int index = 0;
            foreach (Tree<object> node in folder.Children)
            {
                UserSettingsInfo info = node.Value as UserSettingsInfo;
                if (info != null)
                {
                    if (info.Name == name)
                    {
                        existing = info;
                        break;
                    }
                    if (info.Name.CompareTo(name) < 0)
                        index++;
                }
            }

            // add the settings, either by merging with the existing node or creating a new one
            if (existing != null)
            {
                foreach (PropertyDescriptor pd in settings)
                    existing.Settings.Add(pd);
            }
            else
            {
                Tree<object> node = new Tree<object>(new UserSettingsInfo(name, settings));
                folder.Children.Insert(index, node);
            }
        }

        /// <summary>
        /// Presents the settings dialog to the user, with the tree control opened to
        /// the given path</summary>
        /// <param name="pathName">Path of settings to display initially, or null</param>
        public virtual void PresentUserSettings(string pathName)
        {
            using (SettingsDialog settingsDialog = new SettingsDialog(this, GetDialogOwner(), pathName))
            {
                settingsDialog.Settings = m_propertyViewState;
                settingsDialog.Text = "Preferences".Localize();
                if (NavigationBehavior == TreeControl.KeyboardShortcuts.WindowsExplorer)
                {
                    settingsDialog.TreeControl.NavigationKeyBehavior = TreeControl.KeyboardShortcuts.WindowsExplorer;
                    settingsDialog.TreeControl.ExpandOnSingleClick = true;
                    settingsDialog.TreeControl.ToggleOnDoubleClick = false;

                }
                if (settingsDialog.ShowDialog(m_mainWindow.DialogOwner) == DialogResult.OK)
                {
                    SaveSettings();
                }
                m_propertyViewState = settingsDialog.Settings;
            }
        }

        /// <summary>
        /// Event that is raised before settings are saved</summary>
        public event EventHandler Saving;

        /// <summary>
        /// Event that is raised before the settings are loaded (or reloaded)</summary>
        public event EventHandler Loading;

        /// <summary>
        /// Event that is raised when the settings have been loaded or reloaded.</summary>
        public event EventHandler Reloaded;

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="tag">Command</param>
        /// <returns>True if client can do the command</returns>
        public bool CanDoCommand(object tag)
        {
            bool enabled = false;
            if (tag is CommandId)
            {
                switch ((CommandId)tag)
                {
                    case CommandId.EditPreferences:
                    case CommandId.EditImportExportSettings:
                        enabled = true;
                        break;
                }
            }
            return enabled;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="tag">Command</param>
        public void DoCommand(object tag)
        {
            if (tag is CommandId)
            {
                switch ((CommandId)tag)
                {
                    case CommandId.EditPreferences:
                        PresentUserSettings(null);
                        break;

                    case CommandId.EditImportExportSettings:
                        SettingsLoadSaveDialog settingsLoadSaveDialog = new SettingsLoadSaveDialog(this);
                        settingsLoadSaveDialog.ShowDialog(m_mainWindow.DialogOwner);
                        break;
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        /// <summary>
        /// Saves application settings safely and creates a backup in the process</summary>
        public void SaveSettings()
        {
            string tempNew = string.Empty;

            string mutexName = GetMutexName(m_settingsPath);
            using (Mutex saveMutex = new Mutex(false, mutexName))
            {
                try
                {
                    saveMutex.WaitOne();

                    // Create zero-size file.
                    tempNew = Path.GetTempFileName();

                    using (Stream stream = File.Create(tempNew))
                        Serialize(stream);

                    // Make sure the settings directory exists. Do nothing if it already exists.
                    string settingsDir = Path.GetDirectoryName(m_settingsPath);
                    Directory.CreateDirectory(settingsDir);

                    // Erase old backup (if any) and move current settings file (if any).
                    string tempBackup = Path.Combine(settingsDir, "~Settings.xml");
                    if (File.Exists(tempBackup)) // seems unnecessary, but Dan put this check in the WPF version --Ron
                        File.Delete(tempBackup);
                    if (File.Exists(m_settingsPath))
                        File.Move(m_settingsPath, tempBackup);

                    // Move temporary file to be the new settings file, then delete backup.
                    File.Move(tempNew, m_settingsPath);
                    File.Delete(tempBackup);
                }
                catch (TargetInvocationException)
                {
                    // Catch and ignore TargetInvocationException happening if Windows
                    // is shut down with the application still running.
                    // TO DO: Find a way to successfully save settings and exit on shutdown.
                }
                catch (Exception ex)
                {
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                }
                finally
                {
                    // Attempt clean-up. No exception is thrown if file doesn't exist.
                    File.Delete(tempNew);
                    saveMutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Loads all saved settings</summary>
        public void LoadSettings()
        {
            try
            {
                bool defaultSettingsExists = File.Exists(m_defaultSettingsPath);
                bool appSettingsExists = File.Exists(m_settingsPath);

                // only update property descriptors during the DefaultSettings.xml pass
                // if DefaultSettings.xml exists and AppSettings.xml does not exist
                SettingsInfo.CanMakeChanges = defaultSettingsExists && !appSettingsExists;

                // first, load default settings, if they exist:
                if (defaultSettingsExists)
                {
                    using (Stream stream = File.OpenRead(m_defaultSettingsPath))
                        Deserialize(stream);
                }

                // restore for AppSettings.xml pass
                SettingsInfo.CanMakeChanges = true;

                // now load user settings, overriding defaults:
                if (appSettingsExists)
                {
                    using (Stream stream = File.OpenRead(m_settingsPath))
                        Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(OutputMessageType.Error, ex.Message);
            }
            finally
            {
                // restore value
                SettingsInfo.CanMakeChanges = true;
            }
        }

        // for use by SettingsLoadSaveDialog only
        internal void Serialize(Stream stream)
        {
            Saving.Raise(this, EventArgs.Empty);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
            XmlElement root = xmlDoc.CreateElement("settings");
            xmlDoc.AppendChild(root);

            // add application name and version to the root element.
            root.SetAttribute("appName", m_applicationName);
            root.SetAttribute("appVersion", m_versionString);

            foreach (SettingsInfo info in m_settings.Values)
            {
                XmlElement block = xmlDoc.CreateElement("block");
                block.SetAttribute("id", info.Name);

                foreach (SettingsInfo.Setting setting in info.Settings.Values)
                {
                    PropertyDescriptor descriptor = setting.PropertyDescriptor;
                    if (descriptor != null)
                    {
                        object value = descriptor.GetValue(null);
                        if (CanWriteValue(value))
                            WriteValue(descriptor.Name, value, block);
                    }
                    else
                    {
                        WriteValue(setting.Name, setting.ValueString, block);
                    }
                }
                // skip empty block
                if (block.ChildNodes.Count > 0)
                    root.AppendChild(block);
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CloseOutput = false;
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                xmlDoc.WriteTo(writer);
            }
        }

        /// <summary>
        /// Deserializes persisted settings and set property descriptor values</summary>
        /// <remarks>This is marked 'internal' for use by SettingsLoadSaveDialog only.</remarks>
        /// <param name="stream">Persisted settings</param>
        /// <returns>True iff successful</returns>
        internal bool Deserialize(Stream stream)
        {
            // create XML DOM from stream
            // if failed, display message box and return
            OnLoading();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(stream);

                XmlElement root = xmlDoc.DocumentElement;

                // get all the blocks
                XmlNodeList blocks = root.SelectNodes("block");
                if (blocks == null || blocks.Count == 0)
                    throw new Exception("The setting file is empty");

                foreach (XmlElement block in blocks)
                {
                    try
                    {
                        string id = block.GetAttribute("id");

                        SettingsInfo info;
                        if (!m_settings.TryGetValue(id, out info))
                        {
                            info = new SettingsInfo(id);
                            m_settings.Add(id, info);
                        }

                        // get a list of value element in each block
                        XmlNodeList valueNodes = block.SelectNodes("value");

                        // skip over empty block
                        if (valueNodes == null || valueNodes.Count == 0)
                            continue;

                        foreach (XmlElement xmlElement in valueNodes)
                        {
                            string name = xmlElement.GetAttribute("name");
                            string valueString = GetElementValueString(xmlElement);

                            info.Add(name, valueString);
                        }
                    }
                    catch (Exception ex)
                    {
                        Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(
                    OutputMessageType.Error,
                    "Can't load settings".Localize() + ": {0}",
                    ex.Message);

                return false;
            }
            finally
            {
                OnReloaded();
            }

            return true;
        }

        /// <summary>
        /// Raises the Loading event</summary>
        protected virtual void OnLoading()
        {
            Loading.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the Reloaded event</summary>
        protected virtual void OnReloaded()
        {
            Reloaded.Raise(this, EventArgs.Empty);
        }

        // for use by SettingsDialog only
        internal ITreeView UserSettings
        {
            get { return m_userSettings; }
        }

        // for use by SettingsDialog only
        internal List<PropertyDescriptor> GetProperties(Tree<object> tree)
        {
            UserSettingsInfo info = tree.Value as UserSettingsInfo;
            if (info != null)
                return info.Settings;

            return null;
        }

        // for use by SettingsDialog only
        internal Path<object> GetSettingsPath(string pathName)
        {
            string[] pathSegments = pathName.Split(s_delimiters, 16);
            object[] path = new object[pathSegments.Length + 1];

            // first node is the settings tree root
            Tree<object> node = m_userSettings;
            path[0] = m_userSettings.Value;

            // middle nodes are folders
            for (int i = 1; i < path.Length - 1; i++)
            {
                node = GetOrCreateFolder(pathSegments[i - 1], node);
                path[i] = node.Value;
            }

            // leaf node is user settings object
            foreach (Tree<object> leaf in node.Children)
            {
                UserSettingsInfo info = node.Value as UserSettingsInfo;
                if (info != null && info.Name == pathSegments[pathSegments.Length - 1])
                {
                    path[path.Length - 1] = leaf.Value;
                    break;
                }
            }

            return new Path<object>(path);
        }

        private void mainWindow_Loaded(object sender, EventArgs e)
        {
            Initialize();
        }

        private void mainWindow_Closed(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void Initialize()
        {
            // register our menu commands
            if (m_commandService != null)
            {
                if (m_allowUserEdits)
                {
                    m_commandService.RegisterCommand(
                        CommandId.EditPreferences,
                        StandardMenu.Edit,
                        StandardCommandGroup.EditPreferences,
                        "Preferences...".Localize("Edit user preferences"),
                        "Edit user preferences".Localize(),
                        this);
                }

                if (m_allowUserLoadSave)
                {
                    m_commandService.RegisterCommand(
                        CommandId.EditImportExportSettings,
                        StandardMenu.Edit,
                        StandardCommandGroup.EditPreferences,
                        "Load or Save Settings...".Localize(),
                        "User can save or load application settings from files".Localize(),
                        this);
                }
            }

            // load settings as late as possible, so that other components have a chance to
            //  register their settings first
            LoadSettings();
        }

        private Tree<object> GetOrCreateFolder(string name, Tree<object> tree)
        {
            // search for folder
            Tree<object> result = null;
            int index = 0;
            foreach (Tree<object> child in tree.Children)
            {
                string folderName = child.Value as string;
                if (folderName != null)
                {
                    if (folderName == name)
                    {
                        result = child;
                        break;
                    }

                    if (folderName.CompareTo(name) < 0)
                        index++;
                }
                else // child is UserSettingsInfo
                {
                    index++; // folders should follow settings nodes
                }

            }

            // if not found, create it
            if (result == null)
            {
                result = new Tree<object>(name);
                tree.Children.Insert(index, result);
            }

            return result;
        }

        private bool CanWriteValue(object value)
        {
            if (value == null)
                return false;

            TypeConverter converter = TypeDescriptor.GetConverter(value.GetType());
            return CanConvertToAndFromString(converter) || value.GetType().IsSerializable;
        }

        private void WriteValue(string name, object value, XmlElement block)
        {
            if (value == null)
                return;

            // skip persisting if any exception occurs
            string valueString = null;
            Type type = value.GetType();
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (CanConvertToAndFromString(converter))
            {
                valueString = converter.ConvertToInvariantString(value);
            }
            else if (type.IsSerializable)
            {
                // serialize
                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream stream = new MemoryStream())
                {
                    formatter.Serialize(stream, value);
                    valueString = Convert.ToBase64String(stream.GetBuffer());
                }
            }

            if (valueString==null)
                return;

            XmlDocument xmlDoc = block.OwnerDocument;
            XmlElement elmValue = xmlDoc.CreateElement("value");
            elmValue.SetAttribute("name", name);
            elmValue.SetAttribute("type", type.Name);

            // if the valueString is xmlDoc then
            XmlDocument temp = StringToXmlDoc(valueString);
            if (temp != null)
            {
                // remove xml declaration if exists
                XmlDeclaration decl = temp.FirstChild as XmlDeclaration;
                if (decl != null)
                    temp.RemoveChild(decl);
                elmValue.InnerXml = temp.DocumentElement.OuterXml;
            }
            else
            {
                elmValue.InnerText = valueString;
            }

            block.AppendChild(elmValue);
        }

        // handle normal elements, and those with embedded XML documents
        private static string GetElementValueString(XmlElement element)
        {
            string valueString = element.InnerText;
            if (string.IsNullOrEmpty(valueString))
                valueString = element.InnerXml;

            return valueString;
        }

        private static object GetValue(Type type, string valueString)
        {
            object value = null;
            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(type);
                if (CanConvertToAndFromString(converter))
                {
                    value = converter.ConvertFromInvariantString(valueString);
                }
                else
                {
                    // deserialize
                    byte[] data = Convert.FromBase64String(valueString);
                    using (MemoryStream stream = new MemoryStream(data))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        value = formatter.Deserialize(stream);
                    }
                }
            }
            catch
            {
                value = null;
            }
            return value;
        }

        private static bool CanConvertToAndFromString(TypeConverter converter)
        {
            return converter.CanConvertFrom(typeof(string)) &&
                   converter.CanConvertTo(typeof(string));
        }

        private XmlDocument StringToXmlDoc(string strXml)
        {
            XmlDocument xmlDoc = null;
            try
            {
                int len = (strXml.Length > 20) ? 20 : strXml.Length;
                string test = StringUtil.RemoveAllWhiteSpace(strXml.Substring(0, len)).ToLower();
                if (test.Contains("<?xmlversion="))
                {
                    xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(strXml);
                }
            }
            catch
            {
                xmlDoc = null;
            }
            return xmlDoc;

        }

        // Gets the property descriptors of just the user settings. The component is 'null' for
        //  the property descriptors, when getting or setting the value for a particular property.
        private IEnumerable<PropertyDescriptor> UserPropertyDescriptors
        {
            get
            {
                foreach (Tree<object> node in m_userSettings.Children)
                {
                    UserSettingsInfo info = node.Value as UserSettingsInfo;
                    if (info != null)
                    {
                        foreach (PropertyDescriptor property in info.Settings)
                            yield return property;
                    }
                }
            }
        }

        private IWin32Window GetDialogOwner()
        {
            if (m_mainWindow != null)
                return m_mainWindow.DialogOwner;
            else if (m_mainForm != null)
                return m_mainForm;

            return null;
        }

        // Gets a mutex name that can be used to lock access to a particular file.
        private static string GetMutexName(string pathName)
        {
            string safeName = pathName;

            //255 characters will break IpcChannel constructor. 250 works.
            if (safeName.Length > 250)
                safeName = safeName.Substring(safeName.Length - 250);

            // The Mutex constructor will crash if given '\', unless it's part of a valid path.
            // NextInstanceMonitor.ActivateApplication() will crash if there are '/' characters.
            safeName = safeName.Replace('/', '-');
            safeName = safeName.Replace('\\', '-');

            return safeName;
        }

        private string PropertyViewState
        {
            get { return m_propertyViewState; }
            set { m_propertyViewState = value; }
        }

        private class SettingsInfo
        {
            public SettingsInfo(string name)
            {
                Name = name;
            }

            public void Add(string name, string valueString)
            {
                Setting setting;
                if (Settings.TryGetValue(name, out setting))
                {
                    setting.Set(name, valueString);
                }
                else
                {
                    Settings.Add(name, new Setting(name, valueString));
                }
            }

            public void Add(PropertyDescriptor descriptor)
            {
                Setting setting;
                if (Settings.TryGetValue(descriptor.Name, out setting))
                {
                    setting.Set(descriptor);
                }
                else
                {
                    Settings.Add(descriptor.Name, new Setting(descriptor));
                }
            }

            public readonly string Name;
            public readonly SortedDictionary<string, Setting> Settings = new SortedDictionary<string, Setting>();

            public class Setting
            {
                public Setting(PropertyDescriptor descriptor)
                {
                    PropertyDescriptor = descriptor;
                }

                public Setting(string name, string valueString)
                {
                    Name = name;
                    ValueString = valueString;
                }

                public void Set(string name, string valueString)
                {
                    Name = name;
                    ValueString = valueString;
                    if (PropertyDescriptor != null)
                    {
                        SetValue();
                    }
                }

                public void Set(PropertyDescriptor descriptor)
                {
                    PropertyDescriptor = descriptor;
                    if (Name != null && ValueString != null)
                    {
                        SetValue();
                    }
                }

                private void SetValue()
                {
                    if (!CanMakeChanges)
                        return;

                    object value = GetValue(PropertyDescriptor.PropertyType, ValueString);
                    PropertyDescriptor.SetValue(null, value);
                }

                public string Name;
                public string ValueString;
                public PropertyDescriptor PropertyDescriptor;
            }

            /// <summary>
            /// Sets whether property descriptors are allowed to make changes</summary>
            /// <remarks>Used when persisted settings are being loaded from disk
            /// to prevent property descriptors from being set multiple times, as
            /// in the case where DefaultSettings.xml and AppSettings.xml both exist.</remarks>
            public static bool CanMakeChanges { private get; set; }
        }

        private class UserSettingsInfo
        {
            public UserSettingsInfo(string name, PropertyDescriptor[] settings)
            {
                Name = name;
                Settings = new List<PropertyDescriptor>(settings);
            }

            public readonly string Name;
            public readonly List<PropertyDescriptor> Settings;
        }

        private class TreeView : Tree<object>, ITreeView, IItemView
        {
            public TreeView(object root)
                : base(root)
            {
            }

            #region ITreeView Members

            public object Root
            {
                get { return this; }
            }

            public IEnumerable<object> GetChildren(object parent)
            {
                foreach (object child in ((Tree<object>)parent).Children)
                    yield return child;
            }

            #endregion

            #region IItemView Members

            /// <summary>
            /// Gets item's display information</summary>
            /// <param name="item">Item being displayed</param>
            /// <param name="info">Item info, to fill out</param>
            public void GetInfo(object item, ItemInfo info)
            {
                object value = ((Tree<object>)item).Value;
                if (value is string)
                {
                    info.Label = (string)value;
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.FolderImage);
                    info.AllowSelect = false;
                }
                else
                {
                    UserSettingsInfo settingsInfo = value as UserSettingsInfo;
                    info.Label = settingsInfo.Name;
                    info.AllowLabelEdit = false;
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.PreferencesImage);
                    info.IsLeaf = true;
                }
            }

            #endregion
        }

        [Import(AllowDefault = true)]
        private ICommandService m_commandService;

        [Import(AllowDefault = true)]
        private IMainWindow m_mainWindow;

        [Import(AllowDefault = true)]
        private Form m_mainForm;

        private readonly string m_applicationName;
        private readonly string m_versionString;
        private string m_settingsPath;
        private string m_defaultSettingsPath;
        TreeControl.KeyboardShortcuts m_navigationBehavior = TreeControl.KeyboardShortcuts.Default;
        private string m_propertyViewState;
        private readonly SortedDictionary<string, SettingsInfo> m_settings = new SortedDictionary<string, SettingsInfo>();
        private readonly TreeView m_userSettings = new TreeView(string.Empty);
        private bool m_allowUserLoadSave = true;
        private bool m_allowUserEdits = true;

        private static readonly char[] s_delimiters = new[] { '/', '.', '\\' };
    }
}
