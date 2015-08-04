//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Form for presenting the load and save settings operations to the user. Used only
    /// by SettingsService.</summary>
    public partial class SettingsLoadSaveDialog : Form
    {
        private readonly SettingsService m_settingsService;

        private readonly SaveFileDialog m_saveDialog;
        private readonly OpenFileDialog m_openDialog;

        /// <summary>
        /// Creates settings load/save dialog</summary>
        /// <param name="settings">Settings service</param>
        public SettingsLoadSaveDialog(SettingsService settings)
        {
            if (settings == null)
                throw new ArgumentNullException();
            m_settingsService = settings;
            InitializeComponent();

            m_saveDialog = new SaveFileDialog();
            m_saveDialog.OverwritePrompt = true;
            m_saveDialog.Title = "Export settings".Localize();
            m_saveDialog.Filter = "Setting file".Localize() + "(*.xml)|*.xml";
            m_saveDialog.InitialDirectory = m_settingsService.DefaultSettingsPath;

            m_openDialog = new OpenFileDialog();
            m_openDialog.Title = "Import settings".Localize();
            m_openDialog.CheckFileExists = true;
            m_openDialog.CheckPathExists = true;
            m_openDialog.Multiselect = false;
            m_openDialog.Filter = m_saveDialog.Filter;
            m_openDialog.InitialDirectory = m_settingsService.DefaultSettingsPath;
        }

        private void m_btnProceed_Click(object sender, EventArgs e)
        {
            if (m_saveRadioButton.Checked)
            {
                if (m_saveDialog.ShowDialog(this) == DialogResult.OK)
                {
                    SaveSettings(m_saveDialog.FileName);
                }
                else
                {
                    return;
                }
            }
            else if (m_loadRadioButton.Checked)
            {
                if (m_openDialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (!LoadSettings(m_openDialog.FileName))
                        return;
                }
                else
                {
                    return;
                }
            }

        }

        private void SaveSettings(string fullFileName)
        {
            Stream stream = null;
            try
            {
                stream = File.Create(fullFileName);
                m_settingsService.Serialize(stream);

            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        private bool LoadSettings(string fullFileName)
        {
            bool retVal = false;
            Stream stream = null;
            try
            {
                stream = File.OpenRead(fullFileName);
                retVal = m_settingsService.Deserialize(stream);
                if (retVal)
                    m_settingsService.SaveSettings();
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(OutputMessageType.Error, ex.Message);
            }
            finally
            {
                //m_mainForm.Refresh();
                if (stream != null)
                    stream.Close();
            }
            return retVal;
        }
    }
}