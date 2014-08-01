//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Sce.Atf.Collections;
using Sce.Atf.Wpf.Interop;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Base ATF WPF application class. Derive WPF applications from this class to avoid rewriting
    /// boilerplate code.</summary>
    [InheritedExport(typeof(IComposer))]
    public abstract class AtfApp : Application, IComposer
    {
        /// <summary>
        /// Constructor</summary>
        public AtfApp()
        {
            // Setup thread name and culture
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CurrentCulture;
            Thread.CurrentThread.Name = "Main";

            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(
              typeof(FrameworkElement), new FrameworkPropertyMetadata(
                  XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));

            m_initializables = new List<Lazy<IInitializable>>();
            m_disposables = new List<Lazy<IDisposable>>();

            this.Exit += AtfAppExit;
        }

        #region IComposer Members

        /// <summary>
        /// Gets the CompositionContainer</summary>
        public CompositionContainer Container { get; private set; }

        #endregion

        [ImportMany]
        private List<Lazy<IInitializable>> m_initializables = null;

        [ImportMany]
        private List<Lazy<IDisposable>> m_disposables = null;

        [Import]
        private MainWindowAdapter m_mainWindow = null;
             
        /// <summary>
        /// Gets the AggregateCatalog, which aggregates several catalogs containing composable parts into one catalog</summary>
        /// <returns>AggregateCatalog</returns>
        protected abstract AggregateCatalog GetCatalog();

        /// <summary>
        /// Performs custom actions on CompositionBeginning events</summary>
        protected virtual void OnCompositionBeginning() { }

        /// <summary>
        /// Performs custom actions on CompositionComplete events</summary>
        protected virtual void OnCompositionComplete() { }

        /// <summary>
        /// Performs custom actions when an application starts up</summary>
        /// <param name="e">StartupEventArgs args</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (Compose())
            {
                OnCompositionBeginning();

                foreach (var initializable in m_initializables)
                {
                    initializable.Value.Initialize();
                }

                OnCompositionComplete();

                m_mainWindow.ShowMainWindow();
            }
            else
            {
                Shutdown();
            }
        }

        /// <summary>
        /// Performs custom actions when an application exits</summary>
        /// <param name="e">ExitEventArgs args</param>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            foreach (var disposable in m_disposables)
            {
                disposable.Value.Dispose();
            }
            
            if (Container != null)
            {
                Container.Dispose();
            }
        }

        private bool Compose()
        {
            var catalog = GetCatalog();
            Container = new CompositionContainer(catalog);
            
            try
            {
                Container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                MessageBox.Show(compositionException.ToString());
                return false;
            }
            
            return true;
        }

        void AtfAppExit(object sender, ExitEventArgs e)
        {
            IsShuttingDown = true;
        }

        internal bool IsShuttingDown { get; private set; }
    }
}
