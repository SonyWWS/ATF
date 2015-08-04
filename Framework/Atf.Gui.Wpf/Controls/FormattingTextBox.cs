//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Sce.Atf.Wpf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// A TextBox with a bindable StringFormat property</summary>
    public class FormattingTextBox : TextBox
    {
        /// <summary>
        /// Dependency property for BeginCommand</summary>
        public static readonly DependencyProperty BeginCommandProperty =
            DependencyProperty.Register("BeginCommand", 
                typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));

        /// <summary>
        /// Gets and sets the ICommand to begin an editing transaction</summary>
        public ICommand BeginCommand
        {
            get { return (ICommand)base.GetValue(BeginCommandProperty); }
            set { base.SetValue(BeginCommandProperty, value); }
        }

        /// <summary>
        /// Dependency property for CancelCommand</summary>
        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register("CancelCommand", 
                typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));

        /// <summary>
        /// Gets and sets the ICommand to cancel the edit</summary>
        public ICommand CancelCommand
        {
            get { return (ICommand)base.GetValue(CancelCommandProperty); }
            set { base.SetValue(CancelCommandProperty, value); }
        }

        /// <summary>
        /// Dependency property for CommitCommand</summary>
        public static readonly DependencyProperty CommitCommandProperty = 
            DependencyProperty.Register("CommitCommand",
              typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));

        /// <summary>
        /// Gets and sets the ICommand to commit the edit</summary>
        public ICommand CommitCommand
        {
            get { return (ICommand)base.GetValue(CommitCommandProperty); }
            set { base.SetValue(CommitCommandProperty, value); }
        }

        /// <summary>
        /// Dependency property for FinishEditingCommand</summary>
        public static readonly DependencyProperty FinishEditingCommandProperty = 
            DependencyProperty.Register("FinishEditingCommand", 
                typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));

        /// <summary>
        /// Gets and sets the ICommand that completes an editing transaction</summary>
        public ICommand FinishEditingCommand
        {
            get { return (ICommand)base.GetValue(FinishEditingCommandProperty); }
            set { base.SetValue(FinishEditingCommandProperty, value); }
        }

        /// <summary>
        /// Dependency property for LostFocusCommand</summary>
        public static readonly DependencyProperty LostFocusCommandProperty = 
            DependencyProperty.Register("LostFocusCommand", 
                typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));

        /// <summary>
        /// Gets and sets the ICommand for the action to take when the textbox loses focus</summary>
        public ICommand LostFocusCommand
        {
            get { return (ICommand)base.GetValue(LostFocusCommandProperty); }
            set { base.SetValue(LostFocusCommandProperty, value); }
        }

        /// <summary>
        /// Dependency property for TextChangedCommand</summary>
        public static readonly DependencyProperty TextChangedCommandProperty = 
            DependencyProperty.Register("TextChangedCommand", 
                typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));

        /// <summary>
        /// Gets and sets the ICommand for the action to take when the text is changed</summary>
        public ICommand TextChangedCommand
        {
            get { return (ICommand)base.GetValue(TextChangedCommandProperty); }
            set { base.SetValue(TextChangedCommandProperty, value); }
        }

        /// <summary>
        /// Dependency property for UpdateCommand</summary>
        public static readonly DependencyProperty UpdateCommandProperty = 
            DependencyProperty.Register("UpdateCommand", 
                typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));

        /// <summary>
        /// Gets and sets the ICommand to update the text box</summary>
        public ICommand UpdateCommand
        {
            get { return (ICommand)base.GetValue(UpdateCommandProperty); }
            set { base.SetValue(UpdateCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets IFormatProvider that controls formatting</summary>
        public IFormatProvider FormatProvider
        {
            get { return (IFormatProvider)GetValue(FormatProviderProperty); }
            set { SetValue(FormatProviderProperty, value); }
        }

        /// <summary>
        /// Format provider dependency property</summary>
        public static readonly DependencyProperty FormatProviderProperty =
            DependencyProperty.Register("FormatProvider", 
                typeof(IFormatProvider), typeof(FormattingTextBox), new UIPropertyMetadata(CultureInfo.InvariantCulture));

        /// <summary>
        /// Gets or sets formatting string</summary>
        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        /// <summary>
        /// Formatting string dependency property</summary>
        public static readonly DependencyProperty StringFormatProperty =
            DependencyProperty.Register("StringFormat", 
                typeof(string), typeof(FormattingTextBox), new UIPropertyMetadata(null, StringFormatChanged));

        /// <summary>
        /// Gets or sets text in TextBox</summary>
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Text in TextBox dependency property</summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value",
                typeof(string), typeof(FormattingTextBox),
                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ValueChangedCallback, null, false, UpdateSourceTrigger.PropertyChanged));

        /// <summary>
        /// Gets and sets whether we are currently in an editing transaction/// </summary>
        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        /// <summary>
        /// Dependency property for IsEditing</summary>
        public static readonly DependencyProperty IsEditingProperty = 
            DependencyProperty.Register("IsEditing",
                typeof(bool), typeof(FormattingTextBox), 
                    new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, IsEditingChanged, CoerceIsEditing));

        /// <summary>
        /// Called when the IsEditing property changes. If true, set the focus to the textbox.</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnIsEditingChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                if (base.IsInitialized)
                {
                    base.Focus();
                }
                else
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(this.SetFocus));
                }
            }
        }

        /// <summary>
        /// Called when the IsReadOnly property changes. Reevaluates the IsEditing property 
        /// based on the value of IsReadOnly.</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnIsReadOnlyChanged(DependencyPropertyChangedEventArgs e)
        {
            CoerceValue(IsEditingProperty);
        }

        /// <summary>
        /// Called when the text changes</summary>
        /// <param name="e">Event args</param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            
            if (!m_ignoreTextChanges)
            {
                ValueEditorUtil.ExecuteCommand(this.TextChangedCommand, this, base.Text);
                if (IsEditing)
                {
                    m_lostFocusAction = LostFocusAction.Commit;
                }
            }
        }

        /// <summary>
        /// Called when a key is pressed</summary>
        /// <param name="e">Event args</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            bool handlesCommitKeys = ValueEditorUtil.GetHandlesCommitKeys(this);
            
            if (e.Key == Key.Return)
            {
                var lostFocusAction = m_lostFocusAction;
                m_lostFocusAction = LostFocusAction.None;
                
                bool shiftNone= (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.None;
                if (lostFocusAction == LostFocusAction.Commit)
                {
                    if (shiftNone)
                    {
                        CommitChange();
                    }
                    else
                    {
                        UpdateChange();
                    }
                }
                
                if (shiftNone)
                {
                    OnFinishEditing();
                }
                
                e.Handled |= handlesCommitKeys;
            }
            else if ((e.Key == Key.Escape) && IsEditing)
            {
                var lostFocusAction = m_lostFocusAction;
                m_lostFocusAction = LostFocusAction.None;
                if (lostFocusAction != LostFocusAction.None)
                {
                    CancelChange();
                }
                
                OnFinishEditing();
                e.Handled |= handlesCommitKeys;
            }
            
            base.OnKeyDown(e);
        }

        /// <summary>
        /// Called when the keyboard is focused on this element</summary>
        /// <param name="e">Event args</param>
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (!IsReadOnly)
            {
                IsEditing = true;
                SelectAll();
            }
            
            base.OnGotKeyboardFocus(e);
        }

        /// <summary>
        /// Called when focus is lost</summary>
        /// <param name="e">Event args</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            IsEditing = false;
            ValueEditorUtil.ExecuteCommand(LostFocusCommand, this, null);
            e.Handled = true;
        }

        /// <summary>
        /// Called when the keyboard is not focused on this element</summary>
        /// <param name="e">Event args</param>
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            InternalLostFocus();
        }

        /// <summary>
        /// Called when the keyboard is not focused on this element</summary>
        /// <param name="e">Event args</param>
        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewLostKeyboardFocus(e);
            InternalLostFocus();
        }

        /// <summary>
        /// Constructor</summary>
        static FormattingTextBox()
        {
            IsReadOnlyProperty.OverrideMetadata(typeof(FormattingTextBox), 
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, IsReadOnlyChanged));
        }

        private static object CoerceIsEditing(DependencyObject target, object value)
        {
            var editor = target as FormattingTextBox;

            if (((editor != null) && ((bool)value)) && editor.IsReadOnly)
            {
                return false;
            }

            return value;
        }

        private static void IsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = d as FormattingTextBox;
            if (editor != null)
                editor.OnIsEditingChanged(e);
        }

        private void SetFocus()
        {
            if (base.Visibility == Visibility.Visible)
            {
                base.Focus();
            }
        }

        private static void StringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = d as FormattingTextBox;
            if (editor != null)
                editor.UpdateTextFromValue();
        }

        private static void IsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = d as FormattingTextBox;
            if (editor != null)
                editor.OnIsReadOnlyChanged(e);
        }

        private string UnFormat(string s)
        {
            if (StringFormat == null)
                return s;

            var r = new Regex(@"(.*)(\{0.*\})(.*)");
            Match match = r.Match(StringFormat);
            if (!match.Success)
                return s;
            if (match.Groups.Count > 1 && !String.IsNullOrEmpty(match.Groups[1].Value))
                s = s.Replace(match.Groups[1].Value, "");
            if (match.Groups.Count > 3 && !String.IsNullOrEmpty(match.Groups[3].Value))
                s = s.Replace(match.Groups[3].Value, "");

            return s;
        }

        private static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = d as FormattingTextBox;
            if (editor != null && !editor.m_ignoreTextChanges)
                editor.UpdateTextFromValue();
        }

        // Commits the change from Text to Value.
        private void UpdateChange()
        {
            if (!m_transactionOpen)
            {
                ValueEditorUtil.ExecuteCommand(BeginCommand, this, null);
            }

            Value = UnFormat(Text);

            ValueEditorUtil.ExecuteCommand(UpdateCommand, this, null);
            m_transactionOpen = true;
            ValueEditorUtil.UpdateBinding(this, ValueProperty, UpdateBindingType.Target);

            UpdateTextFromValue();
        }

        private void UpdateTextFromValue()
        {
            m_ignoreTextChanges = true;

            var f = StringFormat;
            if (f != null)
            {
                if (!f.Contains("{0"))
                    f = string.Format("{{0:{0}}}", f);

                Text = String.Format(FormatProvider, f, Value);
            }
            else
            {
                Text = Value != null ? String.Format(FormatProvider, "{0}", Value) : null;
            }

            m_ignoreTextChanges = false;
        }

        private void OnFinishEditing()
        {
            var finishEditingCommand = FinishEditingCommand;
            if (finishEditingCommand != null)
            {
                ValueEditorUtil.ExecuteCommand(finishEditingCommand, this, null);
            }
            else
            {
                //Keyboard.Focus(null);
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void CancelChange()
        {
            if (!m_transactionOpen)
            {
                ValueEditorUtil.ExecuteCommand(BeginCommand, this, null);
            }

            ValueEditorUtil.UpdateBinding(this, ValueProperty, false);
            ValueEditorUtil.ExecuteCommand(CancelCommand, this, null);
            m_transactionOpen = false;
            ValueEditorUtil.UpdateBinding(this, ValueProperty, UpdateBindingType.Target);
            
            UpdateTextFromValue();
        }

        // Commits the change from Text to Value.
        private void CommitChange()
        {
            if (!m_transactionOpen)
            {
                ValueEditorUtil.ExecuteCommand(BeginCommand, this, null);
            }

            Value = UnFormat(Text);

            ValueEditorUtil.UpdateBinding(this, ValueProperty, false);
            ValueEditorUtil.ExecuteCommand(CommitCommand, this, null);
            m_transactionOpen = false;
            ValueEditorUtil.UpdateBinding(this, ValueProperty, UpdateBindingType.Target);
            
            UpdateTextFromValue();
        }

        private void InternalLostFocus()
        {
            var lostFocusAction = m_lostFocusAction;
            
            m_lostFocusAction = LostFocusAction.None;
            if (lostFocusAction == LostFocusAction.Commit)
            {
                CommitChange();
            }
            else if (lostFocusAction == LostFocusAction.Cancel)
            {
                CancelChange();
            }
        }

        private enum LostFocusAction
        {
            None,
            Commit,
            Cancel
        }

        private bool m_transactionOpen;
        private bool m_ignoreTextChanges;
        private LostFocusAction m_lostFocusAction;
    }
}
