//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// A service to catch all unhandled exceptions from the UI thread and present
    /// the user with the option of continuing the application so work can be saved.</summary>
    /// <remarks>See also Sce.Atf.CrashLogger, for non-GUI unhandled exception logging to a
    /// remote server. If both are used, put CrashLogger in the MEF TypeCatalog before 
    /// UnhandledExceptionService.</remarks>
    [Export(typeof(IInitializable))]
    [Export(typeof(UnhandledExceptionService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UnhandledExceptionService : IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        public UnhandledExceptionService()
        {
        }

        #region IInitializable Members

        public virtual void Initialize()
        {
            // Catch all the GUI thread unhandled exceptions. In a GUI app, this event will be raised
            //  by default and will prevent CurrrentDomain.UnhandledException from being raised, unless
            //  the debugger is running or unless Application.SetUnhandledExceptionMode is called.
            // We prefer for this event to be raised so that we can present the user with a dialog box
            //  and give the user the opportunity to continue (so that they can save their work, for
            //  example.)
            // http://msdn.microsoft.com/en-us/library/system.windows.forms.application.setunhandledexceptionmode.aspx
            Application.ThreadException += Application_ThreadException;

            // Catch all unhandled exceptions. Note that this is raised, by default, when the debugger
            //  launches the app or if Application.SetUnhandledExceptionMode() was called with
            //  UnhandledExceptionMode.ThrowException. If this event is raised, it appears that
            //  continuing is not really possible.
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (DialogOwner != null)
            {
                if (m_mainWindow != null)
                    DialogOwner = m_mainWindow.DialogOwner;
                else if (m_mainForm != null)
                    DialogOwner = m_mainForm;
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the optional crash logger, which is used to log an unhandled exception that
        /// is caught on the GUI thread (the System.Windows.Forms.Application.ThreadException event)</summary>
        [Import(AllowDefault = true)]
        public ICrashLogger CrashLogger { get; set; }

        /// <summary>
        /// Gets or sets the dialog owner, which is optionally used to show a modal dialog box.
        /// Can be null.</summary>
        protected IWin32Window DialogOwner { get; set; }

        /// <summary>
        /// Gets or sets the optional user feedback service which is used to present a form
        /// to the user to allow them to submit feedback after a crash</summary>
        [Import(AllowDefault = true)]
        protected IUserFeedbackService UserFeedbackService { get; set; }

        /// <summary>
        /// Shows the exception dialog, which allows the user to choose whether or not to continue.
        /// If the user chooses not to continue, Environment.Exit(1) is called.</summary>
        /// <param name="exception">Exception</param>
        protected virtual void ShowExceptionDialog(Exception exception)
        {
            DialogResult res = DialogResult.None;
            try
            {
                var dlg = new UnhandledExceptionDialog();
                
                // Call ToString() to get the call stack. The Message property may not include that.
                dlg.ExceptionTextBox.Text = exception.ToString();

                res = dlg.ShowDialog(DialogOwner);

                if (UserFeedbackService != null)
                    UserFeedbackService.ShowFeedbackForm();
            }
            finally
            {
                if (res == DialogResult.No)
                    Environment.Exit(1);
            }
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Exception exception = e.Exception;

            // For the GUI thread exception handling only, check if there's a crash logger available,
            //  because we will have prevented it from seeing the AppDomain.CurrentDomain.UnhandledException event.
            if (CrashLogger != null)
                CrashLogger.LogException(exception);

            ShowExceptionDialog(exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowExceptionDialog(new Exception(e.ExceptionObject.ToString()));
        }

        [Import(AllowDefault = true)]
        private IMainWindow m_mainWindow;

        [Import(AllowDefault = true)]
        private Form m_mainForm;
    }
}
