//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Controls
{
    internal partial class SwitchToDialog : CommonDialog
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="controlHostService">Control host service to use</param>
        public SwitchToDialog(IControlHostService controlHostService)
        {
            InitializeComponent();

            DataContext = this;

            m_controlHostService = controlHostService;
            
            m_controlInfos = new ObservableCollection<IControlInfo>();
            foreach (var content in m_controlHostService.Contents)
                m_controlInfos.Add(content);

            Loaded += OnLoaded;

            m_controlInfoView = CollectionViewSource.GetDefaultView(m_controlInfos);
            m_controlInfoView.MoveCurrentToFirst();
            MoveSelectionByOne(true);
        }

        /// <summary>
        /// Sets focus to the currently active dialog</summary>
        public static void FocusCurrentInstance()
        {
            if (s_currentlyActiveDialog != null)
            {
                s_currentlyActiveDialog.Focus();
            }
        }

        /// <summary>
        /// Gets the list of IControlInfos as an observable collection</summary>
        public ObservableCollection<IControlInfo> ControlInfos
        {
            get { return m_controlInfos; }
        }

        /// <summary>
        /// Gets whether there is a currently active dialog</summary>
        public static bool IsInUse
        {
            get { return (s_currentlyActiveDialog != null); }
        }

        /// <summary>
        /// Event raised when the dialog is closed</summary>
        /// <param name="e">Event args</param>
        protected override void OnClosed(EventArgs e)
        {
            if (!m_cancel)
            {
                var currentItem = (IControlInfo)m_controlInfoView.CurrentItem;
                if (currentItem != null)
                {
                    m_controlHostService.Show(currentItem.Content);
                }
            }

            s_currentlyActiveDialog = null;
            base.OnClosed(e);
        }

        /// <summary>
        /// Completes initialization of the dialog</summary>
        /// <param name="e">Event args</param>
        protected override void OnInitialized(EventArgs e)
        {
            s_currentlyActiveDialog = this;
            
            base.OnInitialized(e);
        }

        /// <summary>
        /// Event raised when a key is pressed</summary>
        /// <param name="e">Event args</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                if (e.IsDown)
                {
                    bool flag = e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift);
                    MoveSelectionByOne(!flag);
                }
            }
            else if (e.Key == Key.Up)
            {
                if (e.IsDown)
                {
                    MoveSelectionByOne(false);
                }
            }
            else if (e.Key == Key.Down)
            {
                if (e.IsDown)
                {
                    MoveSelectionByOne(true);
                }
            }
            else if ((e.Key != Key.LeftShift) && (e.Key != Key.RightShift))
            {
                m_cancel = true;
                base.Close();
            }
        }

        /// <summary>
        /// Event raised when a key is released</summary>
        /// <param name="e">Event args</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if ((e.Key == Key.LeftCtrl) || (e.Key == Key.RightCtrl))
            {
                base.Close();
            }
            else
            {
                base.OnKeyUp(e);
            }
        }

        /// <summary>
        /// Always returns false</summary>
        protected override bool IsOverridingWindowsChrome
        {
            get { return false; }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)(() =>
            {
                try
                {
                    if (PresentationSource.FromVisual(this) != null)
                    {
                        MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                    }
                }
                catch (Win32Exception)
                {
                }
            }));
        }

        private void MoveSelectionByOne(bool forwards)
        {
            if (m_controlInfoView != null)
            {
                if (!forwards)
                {
                    m_controlInfoView.MoveCurrentToPrevious();
                    if (m_controlInfoView.CurrentItem == null)
                    {
                        m_controlInfoView.MoveCurrentToLast();
                    }
                }
                else
                {
                    m_controlInfoView.MoveCurrentToNext();
                    if (m_controlInfoView.CurrentItem == null)
                    {
                        m_controlInfoView.MoveCurrentToFirst();
                    }
                }

                ControlList.ScrollIntoView(m_controlInfoView.CurrentItem);
            }
        }

        private static SwitchToDialog s_currentlyActiveDialog;

        private bool m_cancel;
        private readonly ObservableCollection<IControlInfo> m_controlInfos;
        private readonly ICollectionView m_controlInfoView;
        private readonly IControlHostService m_controlHostService;
    }
}