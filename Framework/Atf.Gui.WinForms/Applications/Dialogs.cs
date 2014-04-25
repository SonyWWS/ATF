//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Provides static methods to abstract the display of parented dialogs. 
    /// These methods can be used in place of calling form.ShowDialog(parentForm)
    /// by components that need to operate in both WinForms and WPF applications.</summary>
    [Export(typeof(Dialogs))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Dialogs : IPartImportsSatisfiedNotification, IInitializable
    {
        [Import]
        private IDialogService m_dialogService;

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component</summary>
        public void Initialize()
        {
            // implement IInitializable to bring component into existence
        }

        #endregion

        #region IPartImportsSatisfiedNotification Members

        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            s_dialogService = m_dialogService;
        }

        #endregion

        /// <summary>
        /// Shows WinForms dialog parented to main application window</summary>
        /// <param name="form">Form to show</param>
        /// <returns>Dialog result</returns>
        public static DialogResult ShowParentedDialog(Form form)
        {
            return s_dialogService.ShowParentedDialog(form);
        }

        /// <summary>
        /// Configures the application to use the given dialog service</summary>
        /// <param name="dialogService">Dialog service</param>
        public static void Configure(IDialogService dialogService)
        {
            s_dialogService = dialogService;
        }

        private static IDialogService s_dialogService;
    }
}
