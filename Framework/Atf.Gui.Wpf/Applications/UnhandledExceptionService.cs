//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using Sce.Atf.Wpf.Controls;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// A service to catch all unhandled exceptions from the UI thread and present
    /// the user with the option of continuing the application so work can be saved</summary>
    /// <remarks>See also Sce.Atf.CrashLogger, for non-GUI unhandled exception logging to a
    /// remote server. If both are used, put CrashLogger in the TypeCatalog first.</remarks>
    [Export(typeof(IInitializable))]
    [Export(typeof(UnhandledExceptionService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UnhandledExceptionService : IInitializable
    {
        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by calling base Initialize() and
        /// subscribing to DispatcherUnhandledException</summary>
        public void Initialize()
        {
            // set UI exception handling mode
            //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // catch all the GUI thread unhandled exceptions
            System.Windows.Forms.Application.ThreadException += Application_ThreadException;

            // catch all the non-GUI thread unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.Current.DispatcherUnhandledException += Application_DispatcherUnhandledException;
        }

        #endregion

        /// <summary>
        /// Shows the exception dialog, which allows the user to choose whether or not to continue</summary>
        /// <param name="exception">Exception</param>
        /// <returns>Nullable Boolean indicating how a window was closed by the user</returns>
        protected virtual bool? ShowExceptionDialog(Exception exception)
        {
            var dlg = new UnhandledExceptionDialog();
            dlg.DataContext = new UnhandledExceptionViewModel(exception.Message);
            if(Application.Current.MainWindow.IsVisible)
                dlg.Owner = Application.Current.MainWindow;
            return dlg.ShowDialog();
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Exception exception = e.Exception;
            ShowExceptionDialogInternal(exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = new Exception(e.ExceptionObject.ToString());
            ShowExceptionDialogInternal(exception);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowExceptionDialogInternal(e.Exception);
            e.Handled = true;
        }

        private void ShowExceptionDialogInternal(Exception exception)
        {
            bool? result = null;
            try
            {
                var application = Application.Current as AtfApp;
                if (application != null && !application.IsShuttingDown)
                {
                    // DAN: Invoke on UI thread
                    application.Dispatcher.InvokeIfRequired(() =>
                    {
                        result = ShowExceptionDialog(exception);

                        if (m_userFeedbackService != null)
                        {
                            m_userFeedbackService.ShowFeedbackForm();
                        }
                    });
                }
            }
            finally
            {
                if (result.HasValue && !result.Value)
                {
                    Environment.Exit(1);
                }
            }
        }

        [Import(AllowDefault = true)]
        private Atf.Applications.IUserFeedbackService m_userFeedbackService = null;
    }
}
