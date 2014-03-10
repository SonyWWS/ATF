//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Form for plug-in manager
    /// </summary>
    public partial class PluginManagerForm : Form
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="pluginPath">Plugin directory path</param>
        /// <param name="assemblyList">Plugin assembly file name list</param>
        public PluginManagerForm(string pluginPath, string assemblyList)
        {
            InitializeComponent();

            if (pluginPath != null && Directory.Exists(pluginPath))
            {
                Dictionary<string, Assembly> assemblies = AssemblyUtil.GetDictionaryOfLoadedAssemblies();

                // get set of plugins that user has selected (they may or may not be loaded,
                //  since the app must be restarted for this to take effect).
                HashSet<string> selectedAssemblies = new HashSet<string>(assemblyList.Split(';'));

                // iterate over all plugin files in the plugin directory path
                foreach (string file in Directory.GetFiles(pluginPath, "*.dll", SearchOption.TopDirectoryOnly))
                {
                    string filename = Path.GetFileName(file);

                    Assembly assembly = null;
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(file);

                    if (assemblies.ContainsKey(assemblyName.FullName))
                        assembly = assemblies[assemblyName.FullName];
                    else
                        assembly = Assembly.LoadFrom(file);

                    if (assembly == null)
                        continue;

                    // check for ATF plugin marker, if not found skip it.
                    if (Attribute.GetCustomAttribute(assembly, typeof(AtfPluginAttribute)) == null)
                        continue;

                    string description = "", version = "", author = "";

                    Attribute attribute = Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
                    if (attribute != null)
                        description = ((AssemblyDescriptionAttribute)attribute).Description;

                    attribute = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute));
                    if (attribute != null)
                        author = ((AssemblyCompanyAttribute)attribute).Company;

                    attribute = Attribute.GetCustomAttribute(assembly, typeof(AssemblyFileVersionAttribute));
                    if (attribute != null)
                        version = ((AssemblyFileVersionAttribute)attribute).Version;

                    ListViewItem item = new ListViewItem(filename, 0);
                    item.SubItems.Add(description);
                    item.SubItems.Add(version);
                    item.SubItems.Add(author);
                    item.Checked = selectedAssemblies.Contains(filename);
                    m_listView.Items.Add(item);
                }
            }
        }

        private void m_okButton_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ListViewItem item in m_listView.Items)
            {
                if (item.Checked)
                {
                    sb.Append(item.SubItems[0].Text);
                    sb.Append(";");
                }
            }

            if (sb.Length > 0)
                sb.Length--; // remove trailing ';'

            m_pluginManagerService.AssemblyList = sb.ToString();
            //m_pluginManagerService.LoadAssemblyList(m_pluginDictionary);

            Close();
        }

        private void m_cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private PluginManagerService m_pluginManagerService;
        //private PluginDictionary m_pluginDictionary;
    }
}