//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;

using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Interop
{
    /// <summary>
    /// Service to manage lifetime and notifications for main window</summary>
    [Export(typeof(IMainWindow))]
    [Export(typeof(MainWindowAdapter))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MainWindowAdapter : IMainWindow, IInitializable

    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="mainWindow">Main window</param>
        [ImportingConstructor]
        public MainWindowAdapter(Window mainWindow)
        {
            MainWindow = mainWindow;
            MainWindow.Title = GetProductTitle();
            MainWindow.Loaded += (s, e) => Loaded.Raise(this, EventArgs.Empty);
            MainWindow.Closing += mainWindow_Closing;
            MainWindow.Closed += (s,e) =>  Closed.Raise(this, EventArgs.Empty);
            MainWindow.SizeChanged += (s,e) => StoreBounds(s as Window);
            MainWindow.LocationChanged += (s,e) => StoreBounds(s as Window);
        }

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService = null;

        /// <summary>
        /// Gets main window</summary>
        public Window MainWindow { get; private set; }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by setting up Setting Services</summary>
        public void Initialize()
        {
            if (m_settingsService != null)
            {
                m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => MainFormBounds, "MainFormBounds", null, null), new BoundPropertyDescriptor(this, () => MainFormWindowState, "MainFormWindowState", null, null));
            }
        }

        #endregion

        #region IMainWindow Members

        /// <summary>
        /// Gets or sets the main window text</summary>
        public string Text
        {
            get { return MainWindow.Title; }
            set 
            { 
                MainWindow.Dispatcher.InvokeIfRequired(()=>
                    MainWindow.Title = value );
            }
        }

        /// <summary>
        /// Gets a Win32 handle for displaying WinForms dialogs with an owner</summary>
        public System.Windows.Forms.IWin32Window DialogOwner
        {
            get
            {
                Shim shim = new Shim(MainWindow);
                return shim;
            }
        }

        /// <summary>
        /// Assists interoperation between Windows Presentation Foundation (WPF) and Win32 applications</summary>
        class Shim : System.Windows.Forms.IWin32Window
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="owner">Owning window</param>
            public Shim(System.Windows.Window owner)
            {
                // Create a WindowInteropHelper for the WPF Window
                interopHelper = new WindowInteropHelper(owner);
            }

            private WindowInteropHelper interopHelper;

            #region IWin32Window Members

            /// <summary>
            /// Gets the handle to the window represented by the implementer</summary>
            public IntPtr Handle
            {
                get
                {
                    // Return the surrogate handle
                    return interopHelper.Handle;
                }
            }

            #endregion
        }

        /// <summary>
        /// Closes the application's main window</summary>
        public void Close()
        {
            MainWindow.Close();
        }

        /// <summary>
        /// Event that is raised before the application is loaded</summary>
        public event EventHandler Loading;

        /// <summary>
        /// Event that is raised after the application is loaded</summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Event that is raised before the main window closes. Subscribers can cancel
        /// the closing action.</summary>
        public event CancelEventHandler Closing;

        /// <summary>
        /// Event that is raised after the main window is closed</summary>
        public event EventHandler Closed;

        #endregion

        /// <summary>
        /// Shows the main window</summary>
        public void ShowMainWindow()
        {
            Loading.Raise(this, EventArgs.Empty);
            MainWindow.Show();
        }

        /// <summary>
        /// Gets and sets main form bounds</summary>
        public Rect MainFormBounds
        {
            get { return m_mainFormBounds; }
            set
            {
                MainWindow.Width = Math.Max(value.Width, SystemParameters.MinimumWindowWidth);
                MainWindow.Height = Math.Max(value.Height, SystemParameters.MinimumWindowHeight);
                MainWindow.Left = Math.Max(SystemParameters.VirtualScreenLeft, 
                    Math.Min(value.X, SystemParameters.VirtualScreenWidth + SystemParameters.VirtualScreenLeft - MainWindow.Width));
                MainWindow.Top = Math.Max(SystemParameters.VirtualScreenTop, 
                    Math.Min(value.Y, SystemParameters.VirtualScreenHeight + SystemParameters.VirtualScreenTop - MainWindow.Height));
            }
        }
        private Rect m_mainFormBounds;

        /// <summary>
        /// Gets and sets main form window state</summary>
        public WindowState MainFormWindowState
        {
            get
            {
                var state = MainWindow.WindowState;
                if (state == WindowState.Minimized)
                    state = WindowState.Normal; // only return normal or maximized

                return state;
            }
            set { MainWindow.WindowState = value; }
        }

        private string GetProductTitle()
        {
            // Try to get assembly product name
            object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (attributes.Length > 0)
                return ((AssemblyProductAttribute)attributes[0]).Product;

            // if this fails, fall back on assembly title
            attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
                return ((AssemblyTitleAttribute)attributes[0]).Title;

            return null;
        }

        private void StoreBounds(Window wnd)
        {
            if (wnd.WindowState == WindowState.Normal)
                m_mainFormBounds = new Rect(wnd.Left, wnd.Top, wnd.Width, wnd.Height);
            else
                m_mainFormBounds = new Rect(wnd.RestoreBounds.Left, wnd.RestoreBounds.Top, wnd.RestoreBounds.Width, wnd.RestoreBounds.Height);
        }

        #region Event Handlers

        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            var h = Closing;
            if (h != null)
            {
                var args = new CancelEventArgs();
                h(this, args);
                e.Cancel = args.Cancel;
            }
        }

        #endregion
    }
}
