//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// A button which has drop down content.</summary>
    public class SplitButton : Button
    {
        /// <summary>
        /// Gets ands sets the brush used to paint the dropdown arrow</summary>
        public Brush DropDownArrowBrush
        {
            get { return (Brush)GetValue(DropDownArrowBrushProperty); }
            set { SetValue(DropDownArrowBrushProperty, value); }
        }

        /// <summary>
        /// Dependency property for DropDownArrowBrush</summary>
        public static readonly DependencyProperty DropDownArrowBrushProperty =
            DependencyProperty.Register("DropDownArrowBrush", typeof(Brush), typeof(SplitButton));

        /// <summary>
        /// Gets and sets the drop down menu</summary>
        public ContextMenu DropDownMenu
        {
            get { return (ContextMenu)GetValue(DropDownMenuProperty); }
            set { SetValue(DropDownMenuProperty, value); }
        }

        /// <summary>
        /// Dependency property for DropDownMenu</summary>
        public static readonly DependencyProperty DropDownMenuProperty =
            DependencyProperty.Register("DropDownMenu", typeof(ContextMenu), typeof(SplitButton));

        /// <summary>
        /// Gets and sets the ICommand that executes the pre-dropdown event</summary>
        public ICommand PreDropDownCommand
        {
            get { return (ICommand)GetValue(PreDropDownCommandProperty); }
            set { SetValue(PreDropDownCommandProperty, value); }
        }

        /// <summary>
        /// Dependency property for PreDropDownCommand</summary>
        public static readonly DependencyProperty PreDropDownCommandProperty =
            DependencyProperty.Register("PreDropDownCommand", typeof(ICommand), typeof(SplitButton));

        /// <summary>
        /// Called when a new template is applied to the control.</summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_dropDownButton = GetTemplateChild("PART_DropDownButton") as UIElement;
            if (m_dropDownButton == null)
                throw new InvalidOperationException("PART_DropDownButton does not exist");

            m_dropDownButton.PreviewMouseLeftButtonDown += DropDownButtonPreviewMouseLeftButtonDown;
        }

        /// <summary>
        /// Complete initialization</summary>
        /// <param name="e">Event args</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += MenuButtonLoaded;
            Unloaded += MenuButtonUnloaded;
        }

        private void DropDownButtonPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DropDownMenu != null)
            {
                if (m_dropDownMenuOpen)
                {
                    DropDownMenu.IsOpen = false;
                    m_dropDownMenuOpen = false;
                }
                else
                {
                    if ((PreDropDownCommand != null) && PreDropDownCommand.CanExecute(CommandParameter))
                    {
                        PreDropDownCommand.Execute(CommandParameter);
                    }
                    
                    DropDownMenu.PlacementTarget = this;
                    DropDownMenu.Placement = PlacementMode.Bottom;
                    DropDownMenu.IsOpen = true;
                    m_dropDownMenuOpen = true;
                }
                
                e.Handled = true;
            }
        }

        private void DropDownMenuClosed(object sender, RoutedEventArgs e)
        {
            m_dropDownMenuOpen = false;
        }

        private void MenuButtonLoaded(object sender, RoutedEventArgs e)
        {
            if (DropDownMenu != null)
            {
                DropDownMenu.Closed += DropDownMenuClosed;
            }
        }

        static SplitButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton),
                                         new FrameworkPropertyMetadata(typeof(SplitButton)));
        }

        private void MenuButtonUnloaded(object sender, RoutedEventArgs e)
        {
            if (DropDownMenu != null)
            {
                DropDownMenu.Closed -= new RoutedEventHandler(DropDownMenuClosed);
            }
        }

        private bool m_dropDownMenuOpen;
        private UIElement m_dropDownButton;
    }
}
