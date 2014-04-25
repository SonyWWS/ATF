//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Class to provide static access to MEF composer.
    /// This is kind of nasty and ideally should not be used.
    /// However, WPF static attached properties require this.</summary>
    [Export(typeof(Composer))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Composer : IInitializable, IPartImportsSatisfiedNotification
    {
        [Import]
        private IComposer m_composer = null;

        #region Singleton

        /// <summary>
        /// Gets the current IComposer instance</summary>
        public static IComposer Current { get; private set; }

        #endregion

        #region IPartImportsSatisfiedNotification Members

        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
        public void OnImportsSatisfied()
        {
            Current = m_composer;
        }

        #endregion

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component</summary>
        public void Initialize()
        {
        }

        #endregion

        /// <summary>
        /// Cconfigures the IComposer instance</summary>
        /// <param name="composer">IComposer instance</param>
        public static void Configure(IComposer composer)
        {
            Current = composer;
        }
    }
}
