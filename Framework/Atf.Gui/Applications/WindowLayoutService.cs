//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml;

using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Window layout service</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(IWindowLayoutService))]
    [Export(typeof(WindowLayoutService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WindowLayoutService : IInitializable, IWindowLayoutService
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="dockStateProvider">Dock state provider to use</param>
        [ImportingConstructor]
        public WindowLayoutService(IDockStateProvider dockStateProvider)
        {
            DockStateProvider = dockStateProvider;
        }

        #region IInitializable Implementation

        /// <summary>
        /// Initialize</summary>
        public virtual void Initialize()
        {
            if (DockStateProvider != null)
                DockStateProvider.DockStateChanged += DockStateProviderDockStateChanged;

            if (SettingsService == null)
                return;

            SettingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(this, () => PersistedSettings, SettingsDisplayName, null, null));
        }

        #endregion

        #region IWindowLayoutService Implementation

        /// <summary>
        /// Gets or sets the current layout. If setting a new layout name, a new layout is created</summary>
        public string CurrentLayout
        {
            get { return m_current; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                // The active document may have changed, so it's useful to reapply the same layout. (From Guerrilla.)
                //if (string.Compare(value, m_current) == 0)
                //    return;

                if (!IsValidLayoutName(value))
                    return;

                OnLayoutsChanging();

                // Add new layout or change to existing layout
                LayoutInformation info;
                if (m_layouts.TryGetValue(value, out info))
                {
                    //
                    // Change to existing layout
                    //

                    // Load information about the dock state
                    DockStateProvider.DockState = info.DockState.ToString();

                    // Load information about each window layout client
                    foreach (var pair in info.LayoutData)
                    {
                        IWindowLayoutClient client = pair.First;
                        object layoutData = pair.Second;

                        if (client != null)
                            client.LayoutData = layoutData;
                    }
                }
                else
                {
                    //
                    // Add new layout
                    //

                    // Save information about the dock state
                    info = new LayoutInformation { DockState = FixupXmlData(DockStateProvider.DockState.ToString()) };

                    // Save information about each window layout client
                    foreach (var client in m_clients.GetValues())
                        info.LayoutData.Add(new Pair<IWindowLayoutClient, object>(client, client.LayoutData));

                    m_layouts[value] = info;
                }

                m_current = value;

                OnLayoutsChanged();
            }
        }

        /// <summary>
        /// Gets all of the layout names</summary>
        public IEnumerable<string> Layouts
        {
            get { return m_layouts.Keys; }
        }

        /// <summary>
        /// Adds a layout; used to add default layouts. IWindowLayoutClients are queried for their
        /// current states.</summary>
        /// <param name="newLayoutName">New layout name</param>
        /// <param name="dockState">Dock state</param>
        public void AddLayout(string newLayoutName, object dockState)
        {
            // Save information about the dock state
            LayoutInformation info = new LayoutInformation { DockState = dockState };

            // Save information about each window layout client
            foreach (var client in m_clients.GetValues())
                info.LayoutData.Add(new Pair<IWindowLayoutClient, object>(client, client.LayoutData));

            m_layouts[newLayoutName] = info;
        }

        /// <summary>
        /// Rename a layout</summary>
        /// <param name="oldLayoutName">Old layout name</param>
        /// <param name="newLayoutName">New layout name</param>
        /// <returns>True if layout renamed, or false if layout not removed or doesn't exist or new name is invalid</returns>
        public bool RenameLayout(string oldLayoutName, string newLayoutName)
        {
            if (string.IsNullOrEmpty(oldLayoutName) ||
                string.IsNullOrEmpty(newLayoutName))
                return false;

            LayoutInformation info;
            if (!m_layouts.TryGetValue(oldLayoutName, out info))
                return false;

            if (!IsValidLayoutName(newLayoutName))
                return false;

            OnLayoutsChanging();

            m_layouts.Remove(oldLayoutName);
            m_layouts.Add(newLayoutName, info);

            OnLayoutsChanged();

            return true;
        }

        /// <summary>
        /// Remove (i.e. delete) a layout</summary>
        /// <param name="layoutName">Layout name</param>
        /// <returns>True if layout removed, or false if layout not removed or doesn't exist</returns>
        public bool RemoveLayout(string layoutName)
        {
            if (string.IsNullOrEmpty(layoutName))
                return false;

            // Nothing to remove
            if (!m_layouts.ContainsKey(layoutName))
                return false;

            OnLayoutsChanging();

            // Remove layout
            m_layouts.Remove(layoutName);

            // If this layout was the current layout then
            // change the current layout to something else.
            // This addresses an issue when the user would
            // delete a layout then create a new layout with
            // that same name. In that case the new layout
            // never actually got created/saved/persisted.
            if (this.IsCurrent(layoutName))
                m_current = s_defaultLayoutName;

            OnLayoutsChanged();

            return true;
        }

        /// <summary>
        /// Event that is raised before either CurrentLayout or Layouts changes</summary>
        public event EventHandler<EventArgs> LayoutsChanging;

        /// <summary>
        /// Event that is raised after either CurrentLayout or Layouts changes</summary>
        public event EventHandler<EventArgs> LayoutsChanged;

        #endregion

        /// <summary>
        /// Gets the dock state provider that is being used</summary>
        public IDockStateProvider DockStateProvider { get; private set; }

        /// <summary>
        /// Gets or sets the settings service to use</summary>
        /// <remarks>Layouts won't be persisted if null</remarks>
        [Import(AllowDefault = true)]
        public ISettingsService SettingsService { get; set; }

        /// <summary>
        /// Gets or sets persisted settings</summary>
        public string PersistedSettings
        {
            get
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                XmlElement root = xmlDoc.CreateElement(SettingsDocumentElementName);
                xmlDoc.AppendChild(root);

                try
                {
                    root.SetAttribute(
                        SettingsCurrentAttributeName,
                        string.IsNullOrEmpty(m_current) ? s_defaultLayoutName : m_current);

                    foreach (var kv in m_layouts)
                    {
                        string layoutName = kv.Key;
                        LayoutInformation info = kv.Value;

                        XmlElement elemLayout = xmlDoc.CreateElement(SettingsLayoutElementName);
                        elemLayout.SetAttribute(SettingsLayoutAttributeName, layoutName);

                        {
                            XmlElement elemDockState = xmlDoc.CreateElement(SettingsDockStateElementName);
                            elemDockState.InnerXml = FixupXmlData(info.DockState.ToString());
                            elemLayout.AppendChild(elemDockState);
                        }

                        foreach (var pair in info.LayoutData)
                        {
                            IWindowLayoutClient client = pair.First;
                            object layoutData = pair.Second;

                            if ((client == null) || (layoutData == null))
                                continue;

                            string name = client.GetType().FullName;

                            try
                            {
                                XmlElement elemUserdata = xmlDoc.CreateElement(SettingsLayoutDataElementName);
                                elemUserdata.SetAttribute(SettingsLayoutDataAttributeName, name);
                                elemUserdata.InnerXml = FixupXmlData(layoutData.ToString());
                                elemLayout.AppendChild(elemUserdata);
                            }
                            catch (Exception ex)
                            {
                                Outputs.WriteLine(
                                    OutputMessageType.Error,
                                    "Exception persisting window layout user data: {0}", ex.Message);
                            }
                        }

                        root.AppendChild(elemLayout);
                    }

                    if (xmlDoc.DocumentElement == null)
                        xmlDoc.RemoveAll();
                    else if (xmlDoc.DocumentElement.ChildNodes.Count == 0)
                        xmlDoc.RemoveAll();
                }
                catch (Exception ex)
                {
                    Outputs.WriteLine(
                        OutputMessageType.Error,
                        "Exception saving layout persisted settings: {0}", ex.Message);

                    xmlDoc.RemoveAll();
                }

                return xmlDoc.InnerXml.Trim();
            }

            set
            {
                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(value);

                    if (xmlDoc.DocumentElement == null)
                        return;

                    OnLayoutsChanging();

                    m_current =
                        !xmlDoc.DocumentElement.HasAttribute(SettingsCurrentAttributeName)
                            ? s_defaultLayoutName
                            : xmlDoc.DocumentElement.GetAttribute(SettingsCurrentAttributeName);

                    foreach (XmlElement elem in xmlDoc.DocumentElement.ChildNodes)
                    {
                        string attr = elem.GetAttribute(SettingsLayoutAttributeName);
                        if (string.IsNullOrEmpty(attr))
                            continue;

                        var info = new LayoutInformation();

                        foreach (XmlElement elemChild in elem.ChildNodes)
                        {
                            if (string.Compare(elemChild.Name, SettingsDockStateElementName) == 0)
                            {
                                // Get dock state element
                                info.DockState = elemChild.InnerXml;
                            }
                            else if (string.Compare(elemChild.Name, SettingsLayoutDataElementName) == 0)
                            {
                                // Get layout data element(s)

                                string type = elemChild.GetAttribute(SettingsLayoutDataAttributeName);
                                if (string.IsNullOrEmpty(type))
                                    continue;

                                // Check in m_clients.GetValues() for the type
                                IWindowLayoutClient client =
                                    m_clients.GetValues().FirstOrDefault(
                                        t => string.Compare(type, t.GetType().FullName) == 0);

                                // Go to next element
                                if (client == null)
                                    continue;

                                string persistedLayoutData = elemChild.InnerXml;

                                // Pass persisted data back to window layout client
                                client.LayoutData = persistedLayoutData;

                                // Save client and data
                                info.LayoutData.Add(new Pair<IWindowLayoutClient, object>(client, persistedLayoutData));
                            }
                        }

                        // Make sure we obtained a dock state
                        if (info.DockState == null)
                            continue;

                        m_layouts[attr] = info;
                    }

                    OnLayoutsChanged();
                }
                catch (Exception ex)
                {
                    Outputs.WriteLine(
                        OutputMessageType.Error,
                        "Exception loading layout persisted settings: {0}", ex.Message);
                }
            }
        }

        /// <summary>
        /// Checks if a layout name is valid, meaning that it doesn't contain illegal characters</summary>
        /// <param name="layoutName">Layout name</param>
        /// <returns>True iff valid</returns>
        public static bool IsValidLayoutName(string layoutName)
        {
            if (string.IsNullOrEmpty(layoutName))
                return false;

            char[] invalids =
                Path.GetInvalidPathChars()
                .Concat(Path.GetInvalidFileNameChars())
                .Distinct()
                .ToArray();

            return layoutName.IndexOfAny(invalids) < 0;
        }

        private void OnLayoutsChanging()
        {
            m_changing = true;

            LayoutsChanging.Raise(this, EventArgs.Empty);
        }

        private void OnLayoutsChanged()
        {
            try
            {
                LayoutsChanged.Raise(this, EventArgs.Empty);
            }
            finally
            {
                m_changing = false;
            }
        }

        private void DockStateProviderDockStateChanged(object sender, EventArgs e)
        {
            if (m_changing)
                return;

            m_current = s_defaultLayoutName;
        }

        private static string FixupXmlData(string xmlData)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            XmlNode xmlDeclNode = GetXmlDeclaration(xmlDoc);
            if (xmlDeclNode != null)
                xmlDoc.RemoveChild(xmlDeclNode);

            IEnumerable<XmlNode> xmlCommentNodes = GetXmlComments(xmlDoc).ToList();
            foreach (var node in xmlCommentNodes)
                xmlDoc.RemoveChild(node);

            return xmlDoc.InnerXml;
        }

        private static XmlNode GetXmlDeclaration(XmlDocument xmlDoc)
        {
            return xmlDoc.ChildNodes
                .Cast<XmlNode>()
                .FirstOrDefault(n => n.NodeType == XmlNodeType.XmlDeclaration);
        }

        private static IEnumerable<XmlNode> GetXmlComments(XmlDocument xmlDoc)
        {
            return xmlDoc.ChildNodes
                .Cast<XmlNode>()
                .Where(n => n.NodeType == XmlNodeType.Comment);
        }

        private bool m_changing;
        private string m_current = s_defaultLayoutName;

        private const string SettingsDisplayName = "WindowLayouts";
        private const string SettingsDocumentElementName = "WindowLayoutSettings";
        private const string SettingsCurrentAttributeName = "current";

        private const string SettingsLayoutElementName = "Layout";
        private const string SettingsLayoutAttributeName = "name";

        private const string SettingsDockStateElementName = "DockState";

        private const string SettingsLayoutDataElementName = "LayoutData";
        private const string SettingsLayoutDataAttributeName = "type";

#pragma warning disable 649 // Field is never assigned to and will always have its default value

        [ImportMany]
        private IEnumerable<Lazy<IWindowLayoutClient>> m_clients;

#pragma warning restore 649

        private readonly Dictionary<string, LayoutInformation> m_layouts =
            new Dictionary<string, LayoutInformation>(StringComparer.CurrentCulture);

        private static readonly string s_defaultLayoutName = string.Empty;

        #region Private Classes

        private class LayoutInformation
        {
            public LayoutInformation()
            {
                LayoutData = new List<Pair<IWindowLayoutClient, object>>();
            }

            public object DockState { get; set; }

            public List<Pair<IWindowLayoutClient, object>> LayoutData { get; private set; }
        }

        #endregion
    }
}