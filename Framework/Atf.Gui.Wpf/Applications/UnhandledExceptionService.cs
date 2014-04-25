//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
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
    public class UnhandledExceptionService : Sce.Atf.Applications.UnhandledExceptionService, IInitializable
    {
        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by calling base Initialize() and
        /// subscribing to DispatcherUnhandledException</summary>
        public override void Initialize()
        {
            base.Initialize();

            System.Windows.Application.Current.DispatcherUnhandledException += Application_DispatcherUnhandledException;
        }

        #endregion

        /// <summary>
        /// Shows the exception dialog that allows the user to choose whether or not to continue.
        /// If the user chooses not to continue, Environment.Exit(1) is called.</summary>
        /// <param name="exception">Exception raised</param>
        protected override void ShowExceptionDialog(Exception exception)
        {
            bool? result = null;
            try
            {
                var dlg = new UnhandledExceptionDialog();
                // Call ToString() to get the call stack. The Message property may not include that.
                dlg.DataContext = new UnhandledExceptionViewModel(exception.ToString());
                dlg.Owner = Application.Current.MainWindow;
                result = dlg.ShowDialog();

                if (UserFeedbackService != null)
                    UserFeedbackService.ShowFeedbackForm();
            }
            finally
            {
                if (result.HasValue && !result.Value)
                    Environment.Exit(1);
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // For the GUI thread exception handling only, check if there's a crash logger available,
            //  because we will have prevented it from seeing the AppDomain.CurrentDomain.UnhandledException event.
            if (CrashLogger != null)
                CrashLogger.LogException(e.Exception);

            ShowExceptionDialog(e.Exception);
            e.Handled = true;
        }
    }
}
