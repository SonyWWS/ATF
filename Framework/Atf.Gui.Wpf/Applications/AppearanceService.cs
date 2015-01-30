//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Xml;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Wpf.Controls.PropertyEditing;
using Sce.Atf.Wpf.Models;
using Sce.Atf.Wpf.Skins;
using PropertyGrid = Sce.Atf.Wpf.Controls.PropertyEditing.PropertyGrid;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Service to manage appearance, such as skins</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(AppearanceService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AppearanceService : IInitializable
    {
        [Import(AllowDefault = true)] ISettingsService m_settingsService = null;
    
        #region IInitializable Members

        /// <summary>
        /// Finish initializing component, registering user settings</summary>
        public void Initialize()
        {
            if (m_settingsService != null)
            {
                m_settingsService.RegisterSettings(
                    this, new BoundPropertyDescriptor(this, () => PersistedSettings, "Skins".Localize(), null, null));

                m_settingsService.RegisterUserSettings(
                    "Appearance".Localize(),
                    new[]
                        {
                            new BoundPropertyDescriptor(
                                this,
                                () => CurrentSkin,
                                "Theme".Localize(),
                                "Theme".Localize(),
                                "Theme".Localize(),
                                new ThemesValueEditor(this),
                                null),
                        });
            }
        }

        #endregion

        /// <summary>
        /// Get enumeration of registered skins</summary>
        public IEnumerable<Skin> RegisteredSkins
        {
            get { return m_registeredSkins.Values; }
        }

        /// <summary>
        /// Get or set current skin</summary>
        public string CurrentSkin
        {
            get { return m_currentSkin; }
            set
            {
                    if (m_currentSkin != value)
                    {
                        OnAppearanceChanging();

                        try
                        {
                            Application.Current.Resources.BeginInit();

                            if (!string.IsNullOrEmpty(m_currentSkin))
                            {
                                Skin skin;
                                m_registeredSkins.TryGetValue(m_currentSkin, out skin);
                                if (skin != null)
                                    skin.Unload();
                            }

                            m_currentSkin = value;

                            if (!string.IsNullOrEmpty(m_currentSkin))
                            {
                                Skin skin;
                                m_registeredSkins.TryGetValue(m_currentSkin, out skin);
                                if (skin != null)
                                    skin.Load();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Appearance error: " + ex.Message);
                            throw;
                        }
                        finally
                        {
                            Application.Current.Resources.EndInit();
                        }
                        
                        OnAppearanceChanged();
                    }
            }
        }

        /// <summary>
        /// Register a skin</summary>
        /// <param name="skin">Skin to register</param>
        public void RegisterSkin(Skin skin)
        {
            if (!m_registeredSkins.ContainsKey(skin.Name))
                m_registeredSkins[skin.Name] = skin;

            if(string.IsNullOrEmpty(m_currentSkin))
            {
                CurrentSkin = skin.Name;
            }
        }

        private const string SettingsDocumentElementName = "AppearanceSettings";
        private const string SettingsCurrentAttributeName = "current";

        private const string SettingsSkinElementName = "Skin";
        private const string SettingsSkinAttributeName = "name";
        private const string SettingsSkinAttributeUriName = "uri";


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
                    if (!string.IsNullOrEmpty(CurrentSkin))
                    {
                        root.SetAttribute(SettingsCurrentAttributeName, CurrentSkin);
                    }

                    foreach (var kv in m_registeredSkins)
                    {
                        var skin = kv.Value;
                        var skinName = kv.Key;

                        var skinElement = xmlDoc.CreateElement(SettingsSkinElementName);
                        skinElement.SetAttribute(SettingsSkinAttributeName, skinName);

                        var referencedAssemblySkin = skin as ReferencedAssemblySkin;
                        if (referencedAssemblySkin != null)
                            skinElement.SetAttribute(SettingsSkinAttributeUriName, referencedAssemblySkin.Uri.ToString());

                        root.AppendChild(skinElement);
                    }
                }
                catch (Exception ex)
                {
                    Outputs.WriteLine(
                        OutputMessageType.Error,
                        "Exception saving appearance persisted settings: {0}", ex.Message);

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

                    var current = xmlDoc.DocumentElement.GetAttribute(SettingsCurrentAttributeName);

                    foreach (XmlElement elem in xmlDoc.DocumentElement.ChildNodes)
                    {
                        string name = elem.GetAttribute(SettingsSkinAttributeName);
                        string uriString = elem.GetAttribute(SettingsSkinAttributeUriName);

                        if (!string.IsNullOrEmpty(uriString))
                        {
                            Uri uri;
                            if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
                                RegisterSkin(new ReferencedAssemblySkin(name, uri));
                        }
                    }

                    CurrentSkin = !string.IsNullOrEmpty(current) 
                        ? current 
                        : m_registeredSkins.Keys.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        string.Format("Exception loading appearance persisted settings: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// Handle AppearanceChanging event</summary>
        protected void OnAppearanceChanging()
        {
        }

        /// <summary>
        /// Handle AppearanceChanged event</summary>
        protected void OnAppearanceChanged()
        {
        }

        private string m_currentSkin;
        private readonly Dictionary<string, Skin> m_registeredSkins =
            new Dictionary<string, Skin>();
    }

    /// <summary>
    /// ValueEditor for themes</summary>
    public class ThemesValueEditor : ValueEditor
    {
        /// <summary>
        /// Constructor with AppearanceService</summary>
        /// <param name="appearanceService">AppearanceService</param>
        public ThemesValueEditor(AppearanceService appearanceService)
        {
            AppearanceService = appearanceService;
        }

        /// <summary>
        /// Gets whether this editor uses a custom context</summary>
        public override bool UsesCustomContext { get { return true; } }

        /// <summary>
        /// Gets custom context for PropertyNode</summary>
        /// <param name="node">PropertyNode</param>
        /// <returns>Custom context for editor</returns>
        public override object GetCustomContext(PropertyNode node)
        {
            return (node == null) ? null : new StandardValuesEditorContext(node, AppearanceService.RegisteredSkins.Select(x => x.Name));
        }

        /// <summary>
        /// Get the template to be used for the control</summary>
        /// <param name="node">Unused</param>
        /// <param name="container">DependencyObject to query for the template</param>
        /// <returns>The template</returns>
        public override DataTemplate GetTemplate(PropertyNode node, DependencyObject container)
        {
            return FindResource<DataTemplate>(PropertyGrid.StandardValuesEditorTemplateKey, container);
        }

        /// <summary>
        /// Get or set AppearanceService</summary>
        public AppearanceService AppearanceService { get; set; }
    }

    /// <summary>
    /// View model of themes in appearance</summary>
    public class ThemeValuesEditorContext : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Constructor with skins</summary>
        /// <param name="skins">Skins enumeration</param>
        public ThemeValuesEditorContext(IEnumerable<string> skins)
        {
            StandardValues = skins;
        }

        /// <summary>
        /// Get standard theme values, which are a skins enumeration</summary>
        public IEnumerable<string> StandardValues { get; private set; }
    }
}
