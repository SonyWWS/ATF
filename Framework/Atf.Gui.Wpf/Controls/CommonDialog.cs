//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Threading;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interface for actual hosted dialog content.</summary>
    public interface IDialogContent
    {
        /// <summary>
        /// Gets and sets the hosted dialog content.</summary>
        IDialogContentHost Host { get; set; }
    }

    /// <summary>
    /// Interface for managing the panel that contains the dialog content.</summary>
    public interface IDialogSite
    {
        /// <summary>
        /// Gets the panel that contains the dialog content.</summary>
        Panel Site { get; }

        /// <summary>
        /// Display the panel.</summary>
        void ShowSite();

        /// <summary>
        /// Hide the panel.</summary>
        void HideSite();
    }

    /// <summary>
    /// Interface for host of IDialogContent. For example, this could be a dialog window.
    /// Provides basic interface to allow the dialog content to request a Close operation.</summary>
    public interface IDialogContentHost
    {
        /// <summary>
        /// Gets and sets the owning Window.</summary>
        Window Owner { get; set; }

        /// <summary>
        /// Show dialog</summary>
        /// <returns>The dialog result.</returns>
        bool? ShowDialog();

        /// <summary>
        /// Request that the dialog close with given dialog result.
        /// DialogClosing event is raised, allowing subscribers the chance to cancel.</summary>
        /// <param name="dialogResult">The desired dialog result to close with</param>
        void RequestClose(bool? dialogResult);

        /// <summary>
        /// Event raised when close is requested.
        /// HostClosingEventArgs allow the close to be cancelled by subscribers.</summary>
        event EventHandler<HostClosingEventArgs> DialogClosing;
    }

    /// <summary>
    /// EventArgs for the DialogClosing event, allowing subscribers to cancel the close.</summary>
    public class HostClosingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Gets and sets the requested dialog result to close with</summary>
        public bool? DialogResult { get; set; }
    }

    /// <summary>
    /// Factory class that creates IDialogContentHost objects. 
    /// Override this class and add it to MEF composition to create custom dialogs.</summary>
    [InheritedExport(typeof(IInitializable))]
    public class DialogFactory : IInitializable
    {
        #region IInitializable Implementation

        /// <summary>
        /// Finish initializing component</summary>
        public void Initialize()
        {
            s_instance = this;
        }

        #endregion

        /// <summary>
        /// Factory method to create dialogs</summary>
        /// <param name="content">The content to display</param>
        /// <returns>The created dialog</returns>
        public static IDialogContentHost Create(IDialogContent content)
        {
            return s_instance.CreateDialog(content);
        }

        /// <summary>
        /// Does the actual work of creating the dialog. Override this to create custom dialogs.</summary>
        /// <param name="content">The content to display</param>
        /// <returns>The created dialog</returns>
        public virtual IDialogContentHost CreateDialog(IDialogContent content)
        {
            var dialog = new CommonDialogHost();
            dialog.Content = content;
            content.Host = dialog;
            return dialog;
        }

        private static DialogFactory s_instance = new DialogFactory();
    }

    /// <summary>
    /// Factory for EmbeddedDialogContentHost.</summary>
    public class EmbeddedDialogFactory : DialogFactory
    {
        /// <summary>
        /// Does the actual work of creating the dialog. Override this to create custom dialogs.</summary>
        /// <param name="content">The content to display</param>
        /// <returns>
        /// The created dialog</returns>
        public override IDialogContentHost CreateDialog(IDialogContent content)
        {
            return new EmbeddedDialogContentHost(content);
        }
    }

    /// <summary>
    /// Dialog with hosted content returning a value.</summary>
    public class EmbeddedDialogContentHost : IDialogContentHost
    {
        private readonly Control m_dialogContent;
        private bool? m_dialogResult;
        private DispatcherFrame m_frame;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedDialogContentHost"/> class.</summary>
        /// <param name="content">The content</param>
        /// <exception cref="System.ArgumentException">Content is null</exception>
        public EmbeddedDialogContentHost(IDialogContent content)
        {
            if(content == null)
                throw new ArgumentException("context");
            var contentWrapper = new ContentControl();
            contentWrapper.Content = content;
            contentWrapper.Margin = new Thickness(12);
            content.Host = this;
            m_dialogContent = contentWrapper;
        }

        #region Implementation of IDialogContentHost

        /// <summary>
        /// Gets and sets the owning Window.</summary>
        public Window Owner { get; set; }

        /// <summary>
        /// Show dialog.</summary>
        /// <returns>
        /// The dialog result</returns>
        /// <exception cref="System.InvalidOperationException">
        /// "Already showing dialog" or
        /// "Owner must implement IDialogSite"</exception>
        public bool? ShowDialog()
        {
            if(m_frame != null)
                throw new InvalidOperationException("Already showing dialog");

            var owner = Owner as IDialogSite;
            if(owner == null)
                throw new InvalidOperationException("Owner must implement IDialogSite");
           
            m_dialogResult = null;

            owner.Site.Children.Add(m_dialogContent);
            owner.ShowSite();

            m_frame = new DispatcherFrame();
            Dispatcher.PushFrame(m_frame);

            return m_dialogResult;
        }

        /// <summary>
        /// Request that the dialog close with given dialog result.
        /// DialogClosing event is raised, allowing subscribers the chance to cancel.
        /// </summary>
        /// <param name="dialogResult">The desired dialog result to close with</param>
        /// <exception cref="System.InvalidOperationException">Owner is null</exception>
        public void RequestClose(bool? dialogResult)
        {
            var owner = Owner as IDialogSite;
            if (owner == null)
                throw new InvalidOperationException("");

            var args = new HostClosingEventArgs() { DialogResult = dialogResult };
            DialogClosing.Raise(this, args);

            if (!args.Cancel)
            {
                owner.HideSite();
                owner.Site.Children.Remove(m_dialogContent);
                m_dialogResult = args.DialogResult;
                m_frame.Continue = false;
            }
        }

        /// <summary>
        /// Event raised when close is requested.
        /// HostClosingEventArgs allow the close to be cancelled by subscribers.</summary>
        public event EventHandler<HostClosingEventArgs> DialogClosing;

        #endregion
    }

    /// <summary>
    /// UserControl that contains the dialog content</summary>
    public abstract class DialogContentControl : UserControl, IDialogContent
    {
        /// <summary>
        /// Gets and sets the position the dialog window should be shown at when it is opened</summary>
        public WindowStartupLocation WindowStartupLocation { get; set; }

        /// <summary>
        /// Dependency property for the title</summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(object), typeof(DialogContentControl), new PropertyMetadata(default(object)));

        /// <summary>
        /// Gets ands sets the dialog content title</summary>
        public object Title
        {
            get { return (object)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Dependency property for show in taskbar</summary>
        public static readonly DependencyProperty ShowInTaskbarProperty =
            DependencyProperty.Register("ShowInTaskbar", typeof(bool), typeof(DialogContentControl), new PropertyMetadata(true));

        /// <summary>
        /// Gets and sets whether to show the dialog window in the taskbar</summary>
        public bool ShowInTaskbar
        {
            get { return (bool)GetValue(ShowInTaskbarProperty); }
            set { SetValue(ShowInTaskbarProperty, value); }
        }

        /// <summary>
        /// Dependency property for size to content</summary>
        public static readonly DependencyProperty SizeToContentProperty =
            DependencyProperty.Register("SizeToContent", typeof(SizeToContent), typeof(DialogContentControl), new PropertyMetadata(SizeToContent.Manual));

        /// <summary>
        /// Gets and sets how the dialog window will size itself to fit the size of its contents</summary>
        public SizeToContent SizeToContent
        {
            get { return (SizeToContent)GetValue(SizeToContentProperty); }
            set { SetValue(SizeToContentProperty, value); }
        }

        /// <summary>
        /// Dependency property for resize mode</summary>
        public static readonly DependencyProperty ResizeModeProperty =
            DependencyProperty.Register("ResizeMode", typeof(ResizeMode), typeof(DialogContentControl), new PropertyMetadata(ResizeMode.CanResize));

        /// <summary>
        /// Gets and sets whether and how the window can be resized</summary>
        public ResizeMode ResizeMode
        {
            get { return (ResizeMode)GetValue(ResizeModeProperty); }
            set { SetValue(ResizeModeProperty, value); }
        }

        /// <summary>
        /// Dependency property for dialog width</summary>
        public static readonly DependencyProperty DialogWidthProperty =
            DependencyProperty.Register("DialogWidth", typeof(double), typeof(DialogContentControl), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets and sets the width of the dialog window</summary>
        public double DialogWidth
        {
            get { return (double)GetValue(DialogWidthProperty); }
            set { SetValue(DialogWidthProperty, value); }
        }

        /// <summary>
        /// Dependency property for dialog minimum width</summary>
        public static readonly DependencyProperty DialogMinWidthProperty =
            DependencyProperty.Register("DialogMinWidth", typeof(double), typeof(DialogContentControl), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets and sets the minimum widthe of the dialog</summary>
        public double DialogMinWidth
        {
            get { return (double)GetValue(DialogMinWidthProperty); }
            set { SetValue(DialogMinWidthProperty, value); }
        }

        /// <summary>
        /// Dependency property for dialog height</summary>
        public static readonly DependencyProperty DialogHeightProperty =
            DependencyProperty.Register("DialogHeight", typeof(double), typeof(DialogContentControl), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets and sets the height of the dialog</summary>
        public double DialogHeight
        {
            get { return (double)GetValue(DialogHeightProperty); }
            set { SetValue(DialogHeightProperty, value); }
        }
        
        /// <summary>
        /// Dependency property for dialog minimum height</summary>
        public static readonly DependencyProperty DialogMinHeightProperty =
            DependencyProperty.Register("DialogMinHeight", typeof(double), typeof(DialogContentControl), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets and sets the minimum height of the dialog</summary>
        public double DialogMinHeight
        {
            get { return (double)GetValue(DialogMinHeightProperty); }
            set { SetValue(DialogMinHeightProperty, value); }
        }

        /// <summary>
        /// Constructor</summary>
        protected DialogContentControl()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            DataContextChanged += OnDataContextChanged;
        }

        /// <summary>
        /// Event fired when the data context for the control is changed</summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event args</param>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (m_viewModel != null)
                m_viewModel.CloseDialog -= ViewModel_CloseDialog;
            
            m_viewModel = DataContext as IDialogViewModel;
            
            if (m_viewModel != null)
                m_viewModel.CloseDialog += ViewModel_CloseDialog;
        }

        /// <summary>
        /// Gets and sets the Window that hosts the dialog content.</summary>
        public IDialogContentHost Host
        {
            get { return m_host; }
            set
            {
                if (m_host != null)
                    m_host.DialogClosing -= OnDialogClosing;

                m_host = value;

                if (m_host != null)
                {
                    m_host.DialogClosing += OnDialogClosing;

                    var window = m_host as Window;
                    if (window != null)
                    {
                        window.SetBinding(Window.TitleProperty, new Binding("Title") { Source = this });
                        window.WindowStartupLocation = WindowStartupLocation;
                        window.SetBinding(Window.ShowInTaskbarProperty, new Binding("ShowInTaskbar") { Source = this });
                        window.SetBinding(Window.SizeToContentProperty, new Binding("SizeToContent") { Source = this });
                        window.SetBinding(Window.ResizeModeProperty, new Binding("ResizeMode") { Source = this });
                        window.SetBinding(WidthProperty, new Binding("DialogWidth") { Source = this });
                        window.SetBinding(HeightProperty, new Binding("DialogHeight") { Source = this });
                        window.SetBinding(MinWidthProperty, new Binding("DialogMinWidth") { Source = this });
                        window.SetBinding(MinHeightProperty, new Binding("DialogMinHeight") { Source = this });
                    }
                }
            }
        }

        /// <summary>
        /// This is called when user clicks X button in dialog title bar</summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event args</param>
        protected virtual void OnDialogClosing(object sender, HostClosingEventArgs e)
        {
            var vm = m_viewModel;

            if (!m_closing && vm != null && vm.CancelCommand != null)
            {
                // Catch case where dialog is closed using the close button via windows
                // Feed this through the view model
                if (!vm.CancelCommand.CanExecute(null))
                {
                    // VM has prevented dialog close so cancel the operation
                    e.Cancel = true;
                }
                else
                {
                    try
                    {
                        m_closing = true;
                        vm.CancelCommand.Execute(null);

                        // Only set dialog result if this is a modal dialog
                        //if (ComponentDispatcher.IsThreadModal)
                        //    m_host.DialogResult = e.Cancel;

                        //e.DialogResult = e.Cancel;
                    }
                    finally
                    {
                        m_closing = false;
                    }
                }
            }

            // Removed this as the DataContext is required in overriding methods
            if (!e.Cancel)
            {
                // DAN: Workaround as for some reason even after dialog is closed
                // The command manager keeps querying the commands in the data model!
                DataContext = null;
            }
        }

        // This is called when the view model requests the close - e.g. if the vm CloseCommand has been executed
        private void ViewModel_CloseDialog(object sender, CloseDialogEventArgs e)
        {
            if (!m_closing)
            {
                try
                {
                    m_closing = true;
                    m_host.RequestClose(e.DialogResult);
                }
                finally
                {
                    m_closing = false;
                }
            }
        }

        private IDialogContentHost m_host;
        private IDialogViewModel m_viewModel;
        private bool m_closing;
    }

    /// <summary>
    /// Base theme class for WPF dialogs</summary>
    public class CommonDialogBase : Window
    {
        /// <summary>
        /// Backing store for the HeightAdjustment property</summary>
        public static readonly DependencyProperty HeightAdjustmentProperty = DependencyProperty.Register(
            "HeightAdjustment",
            typeof(double),
            typeof(CommonDialogBase),
            new FrameworkPropertyMetadata(36.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets and sets the height adjustment, a value used for tweaking margins/padding in the custom chrome.</summary>
        public double HeightAdjustment
        {
            get { return (double)GetValue(HeightAdjustmentProperty); }
            set { SetValue(HeightAdjustmentProperty, value); }
        }

        /// <summary>
        /// Constructor</summary>
        public CommonDialogBase()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes
        /// call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.</summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // This is required to remove the chrome black glass from the window
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)(InvalidateMeasure));

            SizeToContentDecorator oldSizingDecorator = this.m_sizingDecorator;
            this.m_sizingDecorator = this.GetTemplateChild(SizingDecoratorName) as SizeToContentDecorator;

            if (!object.ReferenceEquals(oldSizingDecorator, this.m_sizingDecorator))
            {
                if (oldSizingDecorator != null)
                {
                    oldSizingDecorator.DesiredSizeChanged -= this.OnContentDesiredSizeChanged;
                }

                if (this.m_sizingDecorator != null)
                {
                    this.m_sizingDecorator.DesiredSizeChanged += this.OnContentDesiredSizeChanged;
                }
            }
        }

        /// <summary>
        /// Gets whether the dialog is overriding the standard Windows chrome</summary>
        protected virtual bool IsOverridingWindowsChrome
        {
            get { return true; }
        }

        /// <summary>
        /// Called when the Content's Desired Size Changes</summary>
        protected virtual void OnContentDesiredSizeChanged()
        {
            this.InvalidateMeasure();
        }

        /// <summary>
        /// Called when the Content's Desired Size Changes</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnContentDesiredSizeChanged(object sender, RoutedEventArgs args)
        {
            this.OnContentDesiredSizeChanged();
        }

        /// <summary>
        /// The name for the sizing decorator template part</summary>
        private const string SizingDecoratorName = "PART_SizingDecorator";

        /// <summary>
        /// The sizing decorator template part</summary>
        private SizeToContentDecorator m_sizingDecorator;
    }

    /// <summary>
    /// Common dialog that delegates its view model binding to hosted dialog content</summary>
    public class CommonDialogHost : CommonDialogBase, IDialogContentHost
    {
        #region IDialogContentHost implementation (members that aren't already implemented by CommonDialogBase)

        /// <summary>
        /// Request that the dialog close with given dialog result.
        /// DialogClosing event is raised, allowing subscribers the chance to cancel.</summary>
        /// <param name="dialogResult">The desired dialog result to close with</param>
        public void RequestClose(bool? dialogResult)
        {
            // Only set dialog result if this is a modal dialog
            // This will have same effect as calling Close()
            if (ComponentDispatcher.IsThreadModal)
            {
                try
                {
                    DialogResult = dialogResult;
                }
                catch (InvalidOperationException)
                {
                    // Occasional strange behavior when trying to set DialogResult
                    // when the window does not think it is modal?
                }
            }
            else
            {
                Close();
            }
        }

        /// <summary>
        /// Event raised when close is requested.
        /// HostClosingEventArgs allow the close to be cancelled by subscribers.</summary>
        public event EventHandler<HostClosingEventArgs> DialogClosing;

        #endregion

        /// <summary>
        /// Complete initialization</summary>
        /// <param name="e">Event args</param>
        protected override void OnInitialized(EventArgs e)
        {
            if (IsOverridingWindowsChrome)
            {
                SetResourceReference(StyleProperty, typeof(CommonDialogHost));
            }

            base.OnInitialized(e);
        }

        /// <summary>
        /// Event raised when the dialog is being closed.</summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel)
            {
                var args = new HostClosingEventArgs { DialogResult = DialogResult };
                DialogClosing.Raise(this, args);

                e.Cancel = args.Cancel;

                // Prevent main application disappearing behind other windows:
                // http://stackoverflow.com/questions/13209526/main-window-disappears-behind-other-applications-windows-after-a-sub-window-use
                if (!e.Cancel && Owner != null)
                    Owner.Focus();
            }
        }

        static CommonDialogHost()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommonDialogHost), new FrameworkPropertyMetadata(typeof(CommonDialogHost)));
        }
    }

    /// <summary>
    /// Normal Common dialog which deals with view model binding directly</summary>
    public class CommonDialog : CommonDialogBase
    {
        /// <summary>
        /// Constructor</summary>
        public CommonDialog()
        {
            DataContextChanged += CommonDialog_DataContextChanged;
        }

        /// <summary>
        /// Complete initialization</summary>
        /// <param name="e">The <see cref="T:System.Windows.EventArgs" /> that contains the event data</param>
        protected override void OnInitialized(EventArgs e)
        {
            if (IsOverridingWindowsChrome)
            {
                SetResourceReference(StyleProperty, typeof(CommonDialog));
            }

            base.OnInitialized(e);
        }

        /// <summary>
        /// Event raised when the dialog is being closed</summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            var vm = m_viewModel;

            if (!m_closing && vm != null && vm.CancelCommand != null)
            {
                // Catch case where dialog is closed using the close button via windows
                // Feed this through the view model
                if (!vm.CancelCommand.CanExecute(null))
                {
                    e.Cancel = true;
                }
                else
                {
                    try
                    {
                        m_closing = true;
                        vm.CancelCommand.Execute(null);

                        // Only set dialog result if this is a modal dialog
                        if (ComponentDispatcher.IsThreadModal)
                            DialogResult = e.Cancel;
                    }
                    finally
                    {
                        m_closing = false;
                    }
                }
            }

            // Prevent main application disappearing behind other windows:
            // http://stackoverflow.com/questions/13209526/main-window-disappears-behind-other-applications-windows-after-a-sub-window-use
            if (!e.Cancel && Owner != null) Owner.Focus();
        }

        static CommonDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommonDialog), new FrameworkPropertyMetadata(typeof(CommonDialog)));
        }

        private void CommonDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (m_viewModel != null)
            {
                m_viewModel.CloseDialog -= ViewModel_CloseDialog;
            }

            m_viewModel = DataContext as IDialogViewModel;
            if (m_viewModel != null)
            {
                m_viewModel.CloseDialog += ViewModel_CloseDialog;

                SetBinding(TitleProperty, new Binding("Title") {Source = DataContext});
            }
        }

        private void ViewModel_CloseDialog(object sender, CloseDialogEventArgs e)
        {
            if (!m_closing)
            {
                try
                {
                    m_closing = true;

                    // DAN: Workaround as for some reason even after dialog is closed
                    // The command manager keeps querying the commands in the data model!
                    DataContext = null;

                    // Only set dialog result if this is a modal dialog
                    if (ComponentDispatcher.IsThreadModal)
                    {
                        try
                        {
                            DialogResult = e.DialogResult;

                        }
                        catch (InvalidOperationException)
                        {
                            // Occasional strange behavior when trying to set DialogResult
                            // when the window does not think it is modal?
                        }
                    }

                    // Is this required?
                    Close();
                }
                finally
                {
                    m_closing = false;
                }
            }
        }

        private IDialogViewModel m_viewModel;
        private bool m_closing;
    }

    /// <summary>
    /// In-Window Extension Methods</summary>
    public static class DialogManager
    {
        /// <summary>
        /// Sets up the size of the dialog and adds it to the specified container.</summary>
        /// <param name="window">The window in which to host the dialog content</param>
        /// <param name="dialog">The user control containing the dialog content</param>
        /// <returns>An event handler for when the size of the dialog changes</returns>
        public static SizeChangedEventHandler SetupAndOpenDialog(IDialogSite window, Control dialog)
        {
            dialog.MinHeight = window.Site.ActualHeight / 4.0;
            dialog.MaxHeight = window.Site.ActualHeight;

            SizeChangedEventHandler sizeHandler = null; //an event handler for auto resizing an open dialog.
            sizeHandler = (sender, args) =>
                {
                    dialog.MinHeight = window.Site.ActualHeight / 4.0;
                    dialog.MaxHeight = window.Site.ActualHeight;
                };

            window.Site.SizeChanged += sizeHandler;

            window.Site.Children.Add(dialog); //add the dialog to the container

            return sizeHandler;
        }
    }
}
