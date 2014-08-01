//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Controls;

namespace Sce.Atf.Wpf.Models
{
    internal sealed class SettingsLoadSaveViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="settings">Settings service to use for saving and loading the settings</param>
        public SettingsLoadSaveViewModel(SettingsService settings)
        {
            Title = "Load and Save Settings".Localize();

            if (settings == null)
                throw new ArgumentNullException();

            m_settingsService = settings;

            m_saveDialog = new SaveFileDialog();
            m_saveDialog.OverwritePrompt = true;
            m_saveDialog.Title = "Export settings";
            m_saveDialog.Filter = "Setting file(*.xml)|*.xml";

            m_openDialog = new OpenFileDialog();
            m_openDialog.Title = "Import settings";
            m_openDialog.CheckFileExists = true;
            m_openDialog.CheckPathExists = true;
            m_openDialog.Multiselect = false;
            m_openDialog.Filter = "Setting file(*.xml)|*.xml";

            Action = SettingsAction.Save;
        }

        public SettingsAction Action
        {
            get { return m_action; }
            set
            {
                m_action = value;
                OnPropertyChanged(ActionArgs);
            }
        }

        /// <summary>
        /// Event called when the dialog is closing.</summary>
        /// <param name="args"></param>
        protected override void OnCloseDialog(CloseDialogEventArgs args)
        {
            if (args.DialogResult == true)
            {
                if (Action == SettingsAction.Save)
                {
                    if (m_saveDialog.ShowCommonDialogWorkaround() == true)
                    {
                        SaveSettings(m_saveDialog.FileName);
                    }
                    else
                    {
                        return;
                    }
                }
                else if (Action == SettingsAction.Load)
                {
                    if (m_openDialog.ShowCommonDialogWorkaround() == true)
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

            RaiseCloseDialog(args);
        }

        private void SaveSettings(string fullFileName)
        {
            Stream stream = null;
            try
            {
                stream = File.Create(fullFileName);
                m_settingsService.SerializeInternal(stream);

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
                retVal = m_settingsService.DeserializeInternal(stream);
                if (retVal)
                    m_settingsService.SaveSettings();
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(OutputMessageType.Error, ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return retVal;
        }

        private SettingsAction m_action;
        private static readonly PropertyChangedEventArgs ActionArgs
            = ObservableUtil.CreateArgs<FindFileDialogViewModel>(x => x.Action);

        private readonly SettingsService m_settingsService;

        private readonly SaveFileDialog m_saveDialog;
        private readonly OpenFileDialog m_openDialog;
    }
}
