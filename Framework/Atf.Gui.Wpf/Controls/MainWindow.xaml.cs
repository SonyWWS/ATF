//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml for main windows</summary>
    [Export(typeof(MainWindow))]
    [Export(typeof(Window))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class MainWindow : Window, IInitializable, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Constructor</summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component</summary>
        public void Initialize()
        {
        }

        #endregion

        /// <summary>
        /// Gets or sets the Control that is the main content of the MainWindow, e.g., a DockPanel</summary>
        public Control MainContent 
        {
            get { return m_mainContent; }
            set 
            {
                value.SetValue(DockPanel.DockProperty, Dock.Top);
                dockPanel.Children.Add(value);
                m_mainContent = value;
            }
        }
        private Control m_mainContent;


        #region IPartImportsSatisfiedNotification Members

        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
        public void OnImportsSatisfied()
        {
            // For some reason MS decided not to make ToolBar a bindable itemscollection
            // have to import the view model manually and do this
            var binder = new Models.ToolBarTrayBinder(toolBarTray, m_toolBarViewModel.ToolBars);
        }

        #endregion

        [Import(Contracts.ViewModel)]
        private ToolBarViewModel m_toolBarViewModel = null;
    }

}
