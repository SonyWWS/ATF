//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class to adapt a System.Windows.Forms.Form to IMainWindow. This class can be used
    /// as a lightweight adapter for components that need to support both Form and IMainWindow
    /// for backwards compatibility.</summary>
    [Export(typeof(IMainWindow))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MainFormAdapter : IMainWindow
    {        
        /// <summary>
        /// Constructor</summary>
        /// <param name="mainForm">Application's main form</param>
        [ImportingConstructor]
        public MainFormAdapter(Form mainForm)
        {
            m_mainForm = mainForm;
            m_mainForm.Load += mainForm_Load;
            m_mainForm.Shown += mainForm_Shown;
            m_mainForm.FormClosing += mainForm_FormClosing;
            m_mainForm.FormClosed += mainForm_FormClosed;
        }

        #region IMainWindow Members

        /// <summary>
        /// Gets or sets the main window text</summary>
        public string Text
        {
            get { return m_mainForm.Text; }
            set { m_mainForm.Text = value; }
        }

        /// <summary>
        /// Gets a Win32 handle for displaying WinForms dialogs with an owner</summary>
        public IWin32Window DialogOwner
        {
            get { return m_mainForm; }
        }

        /// <summary>
        /// Closes the application's main window</summary>
        public void Close()
        {
            m_mainForm.Close();
        }

        /// <summary>
        /// Event that is raised before the application is loaded</summary>
        public event EventHandler Loading;

        /// <summary>
        /// Event that is raised after the application is loaded</summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Event that is raised before the application closes. Subscribers can cause
        /// the closing action to be cancelled.</summary>
        public event CancelEventHandler Closing;

        /// <summary>
        /// Event that is raised after the application is closed</summary>
        public event EventHandler Closed;

        #endregion

        private void mainForm_Load(object sender, EventArgs e)
        {
            Loading.Raise(this, e);
        }

        private void mainForm_Shown(object sender, EventArgs e)
        {
            Loaded.Raise(this, e);
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = Closing.RaiseCancellable(this, e);
        }

        private void mainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Closed.Raise(this, e);
        }

        private readonly Form m_mainForm;
    }
}
