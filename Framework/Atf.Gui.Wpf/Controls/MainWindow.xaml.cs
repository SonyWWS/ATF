//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml for main windows</summary>
    [Export(typeof(MainWindow))]
    [Export(typeof(Window))]
    [Export(typeof(IMainWindowContentSite))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class MainWindow : Window, IMainWindowContentSite, IInitializable, INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor</summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets and sets the ViewModel for the toolbar</summary>
        [Import(Contracts.ViewModel)]
        public ToolBarViewModel ToolBarViewModel
        {
            get { return m_toolBarViewModel; }
            set 
            { 
                m_toolBarViewModel = value;
                
                // For some reason MS decided not to make ToolBar a bindable itemscollection
                // have to import the view model manually and do this
                BuildToolBars();
                m_toolBarViewModel.PropertyChanged += ToolBarViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Gets and sets the ViewModel for the main menu</summary>
        [Import(Contracts.ViewModel)]
        public MainMenuViewModel MainMenuViewModel
        {
            get { return m_mainMenuViewModel; }
            set
            {
                m_mainMenuViewModel = value;
                OnPropertyChanged(new PropertyChangedEventArgs("MainMenuViewModel"));
            }
        }

        /// <summary>
        /// Gets and sets the items in the status list</summary>
        [ImportMany(AllowRecomposition = true)]
        public IStatusItem[] StatusItems
        {
            get { return m_statusItems; }
            set
            {
                m_statusItems = value;
                OnPropertyChanged(new PropertyChangedEventArgs("StatusItems"));
            }
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component</summary>
        public void Initialize()
        {
        }

        #endregion

        #region IMainWindowContentSite Members

        /// <summary>
        /// Gets and sets the control that represents the main content of the window. Handles docking on set.</summary>
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
        
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var h = PropertyChanged;
            if (h != null)
                h(this, e);
        }

        #endregion

        private void BuildToolBars()
        {
            toolBarTray.ToolBars.Clear();

            foreach (IToolBar toolBarModel in ToolBarViewModel.ToolBars)
            {
                var items = ToolBarViewModel.GetToolBarItems(toolBarModel);
                if (items.Any())
                {
                    var toolBar = new ToolBar();
                    toolBar.SetResourceReference(StyleProperty, Wpf.Resources.ToolBarStyleKey);
                    toolBar.DataContext = toolBarModel;
                    toolBar.ItemsSource = items;
                    toolBar.ItemTemplateSelector = ToolBarItemTemplateSelector.Instance;
                    toolBarTray.ToolBars.Add(toolBar);
                }
            }
        }

        private void ToolBarViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == s_toolBarsPropertyName)
                BuildToolBars();
        }

        private static readonly string s_toolBarsPropertyName
            = TypeUtil.GetProperty<ToolBarViewModel>(x => x.ToolBars).Name;

        private ToolBarViewModel m_toolBarViewModel;
        private MainMenuViewModel m_mainMenuViewModel;
        private IStatusItem[] m_statusItems;
        private Control m_mainContent;
    }

    public interface IMainWindowContentSite
    {
        Control MainContent { get; set; }
    }

}
