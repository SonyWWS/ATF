//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service that allows the user to add and remove dynamic plugin assemblies. In order
    /// for removal to take effect, the application must be restarted. Additions can take
    /// effect without restarting if consumers of dynamic plugins subscribe to the PluginLoaded
    /// event.</summary>
    [Export(typeof(PluginManagerService))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PluginManagerService : ICommandClient, IInitializable
    {
        [ImportingConstructor]
        public PluginManagerService(ICommandService commandService)
        {
            m_commandService = commandService;
        }

        private ICommandService m_commandService;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService = null;

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(
                CommandId.EditPlugins,
                StandardMenu.Edit,
                StandardCommandGroup.EditPreferences,
                Localizer.Localize("Plugins") + " ...",
                Localizer.Localize("Edit plugin list"),
                this);

            if (m_settingsService != null)
            {
                m_settingsService.RegisterSettings(
                     this,
                     new BoundPropertyDescriptor(this, "AssemblyList", "AssemblyList", null, null, null));
            }
        }

        #endregion

        #region IPluginManagerService members

        /// <summary>
        /// Gets or sets the path to the directory that holds plugin assemblies</summary>
        public string PluginPath
        {
            get { return m_pluginPath; }
            set { m_pluginPath = value; }
        }

        /// <summary>
        /// Gets or sets the initial plugin assembly file names, delimited by ';'.
        /// e.g., "Sce.Perforce.dll;Sce.Atgi.dll"</summary>
        /// <remarks>This property should be set at the beginning of the application,
        /// before IApplicationHostService.Start is called, so as not to override the user's
        /// local setting</remarks>
        public string AssemblyList
        {
            get { return m_assemblyList; }
            set { m_assemblyList = value; }
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="tag">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        bool ICommandClient.CanDoCommand(object tag)
        {
            return CommandId.EditPlugins.Equals(tag);
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="tag">Command to be done</param>
        void ICommandClient.DoCommand(object tag)
        {
            if (CommandId.EditPlugins.Equals(tag))
            {
                PluginManagerForm pluginManagerForm = new PluginManagerForm(m_pluginPath, m_assemblyList);
                pluginManagerForm.ShowDialog();
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        void ICommandClient.UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        private void LoadAssemblyList()
        {
            Dictionary<string, Assembly> loadedAssemblies = AssemblyUtil.GetDictionaryOfLoadedAssemblies();
            string[] pluginNames = m_assemblyList.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string name in pluginNames)
            {
                string path = Path.Combine(m_pluginPath, name);
                if (File.Exists(path))
                {
                    if (!m_loadedAssemblies.Contains(name))
                    {
                        try
                        {
                            Assembly assembly = null;
                            AssemblyName assemName = AssemblyUtil.GetAssemblyName(path);
                            if (assemName == null)
                                continue;
                            loadedAssemblies.TryGetValue(assemName.FullName, out assembly);
                            if (assembly == null)
                                assembly = Assembly.LoadFrom(path);

                            if (assembly != null)
                            {
                                m_loadedAssemblies.Add(name);
                                ////////plugins.Add(assembly);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Alert the user and continue; the assumption is that the app should be
                            //  able to continue since dynamic plugins should be optional
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
                else
                {
                    LogUtil.WriteLine(Localizer.Localize("Could not find plugin: ") + path);
                }
            }
        }

        private string m_pluginPath = string.Empty;
        private string m_assemblyList = string.Empty;
        private List<string> m_loadedAssemblies = new List<string>();
    }
}
