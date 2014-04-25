//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Forms.VisualStyles;
using System.Collections.Generic;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Universal property editing control that can be embedded in complex property
    /// editing controls. It uses TypeConverters and UITypeEditors to provide a GUI
    /// for every kind of .NET property.</summary>
    public class PropertyEditingControl : Control,
        IWindowsFormsEditorService,
        ITypeDescriptorContext,
        IServiceProvider,
        ICacheablePropertyControl,
        IFormsOwner
    {
        /// <summary>
        /// Constructor</summary>
        public PropertyEditingControl()
        {
            m_editButton = new EditButton();
            m_textBox = new TextBox();

            // force creation of the window handles on the GUI thread
            // see http://forums.msdn.microsoft.com/en-US/clr/thread/fa033425-0149-4b9a-9c8b-bcd2196d5471/
            IntPtr handle;
            handle = m_editButton.Handle;
            handle = m_textBox.Handle;

            base.SuspendLayout();

            m_editButton.Left = base.Right - 18;
            m_editButton.Size = new Size(18, 18);
            m_editButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            m_editButton.Visible = false;

            m_editButton.Click += editButton_Click;
            m_editButton.MouseDown += editButton_MouseDown;

            m_textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            m_textBox.BorderStyle = BorderStyle.None;

            m_textBox.LostFocus += textBox_LostFocus;

            // forward textbox events as if they originated with this control
            m_textBox.DragOver += textBox_DragOver;
            m_textBox.DragDrop += textBox_DragDrop;
            m_textBox.MouseHover += textBox_MouseHover;
            m_textBox.MouseLeave += textBox_MouseLeave;

            m_textBox.Visible = false;

            Controls.Add(m_editButton);
            Controls.Add(m_textBox);

            base.ResumeLayout();
            m_textBox.SizeChanged += (sender, e) => Height = m_textBox.Height + 1;
            m_dropDownForm = new DropDownForm(this);
        }

        /// <summary>
        /// Gets and sets the size of the edit button.
        /// Note: The edit button is rendered differently depending on 
        /// what this control is being used for, e.g., a drop down button
        /// or a "..." button.</summary>
        public Size EditButtonSize
        {
            get { return m_editButton.Size; }
            set
            {
                m_editButton.Size = value;
                m_editButton.Left = base.Right - value.Width;
            }
        }

        /// <summary>
        /// Binds the control to a property and owner</summary>
        /// <param name="context">Context for property editing control</param>
        public void Bind(PropertyEditorControlContext context)
        {
            if (m_textBox.Focused)
                Flush();

            m_context = context;
            m_descriptor = m_context.Descriptor;

            bool visible = m_context != null;
            base.Visible = visible;

            if (visible)
            {
                SetTextBoxFromProperty();

                bool editButtonVisible = false;
                if (!m_context.IsReadOnly)
                {
                    UITypeEditor editor = WinFormsPropertyUtils.GetUITypeEditor(m_descriptor, this);
                    if (editor != null)
                    {
                        editButtonVisible = true;
                        m_editButton.Modal = (editor.GetEditStyle(this) == UITypeEditorEditStyle.Modal);
                    }
                }

                m_editButton.Visible = editButtonVisible;

                // a standard set of values that can be picked from a list, like enum (but only if we're not readonly)
                if (!m_context.IsReadOnly && (m_descriptor.Converter != null))
                {
                    TypeDescriptorContext tdcontext = new TypeDescriptorContext(m_context.LastSelectedObject, m_descriptor, null);
                    if (m_descriptor.Converter.GetStandardValuesExclusive(tdcontext))
                    {
                        // this will redraw the control before we get to the invalidate below
                        m_textBox.AutoCompleteMode = AutoCompleteMode.None;

                        m_textBox.AutoCompleteCustomSource.Clear();
                        AutoCompleteStringCollection standardVals = new AutoCompleteStringCollection();
                        ICollection values = m_descriptor.Converter.GetStandardValues(tdcontext);
                        foreach (object item in values)
                            standardVals.Add(item.ToString());
                        m_textBox.AutoCompleteCustomSource = standardVals;

                        // this will redraw the control before we get to the invalidate below
                        m_textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                        m_textBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    }
                    else
                    {
                        // this will redraw the control before we get to the invalidate below
                        m_textBox.AutoCompleteMode = AutoCompleteMode.None;
                        m_textBox.AutoCompleteSource = AutoCompleteSource.None;
                        m_textBox.AutoCompleteCustomSource.Clear();
                    }
                }
                else
                {
                    // this will redraw the control before we get to the invalidate below
                    m_textBox.AutoCompleteMode = AutoCompleteMode.None;
                    m_textBox.AutoCompleteSource = AutoCompleteSource.None;
                    m_textBox.AutoCompleteCustomSource.Clear();
                }

                PerformLayout();
                Invalidate();
            }
        }

        /// <summary>
        /// Cancels the current edit</summary>
        public void CancelEdit()
        {
            SetTextBoxFromProperty();
            DisableTextBox();
        }

        /// <summary>
        /// Forces the current edit to finish</summary>
        public void Flush()
        {
            SetPropertyFromTextBox();
        }

        #region IFormsOwner

        /// <summary>
        /// If this control uses a top level form as the drop down box, gets that form so 
        /// users of this control can consider if as part of the control for focus situations</summary>
        public IEnumerable<Form> Forms
        {
            get { yield return this.m_dropDownForm; }
        }

        #endregion

        #region ICacheablePropertyControl

        /// <summary>
        /// Gets true iff this control can be used indefinitely, regardless of whether the associated
        /// PropertyEditorControlContext's SelectedObjects property changes, i.e., the selection changes. 
        /// This property must be constant for the life of this control.</summary>
        public virtual bool Cacheable
        {
            get { return true; }
        }

        #endregion

        /// <summary>
        /// Custom string formatting flags, in addition to the regular StringFormat flags</summary>
        public enum CustomStringFormatFlags
        {
            /// <summary>
            /// No formatting</summary>
            None = 0,
            /// <summary>
            /// If the string doesn't fit, ellipses are used on the left side</summary>
            TrimLeftWithEllipses = 1
        }

        /// <summary>
        /// Formatting to use for displaying the property value</summary>
        public class ExtendedStringFormat
        {
            /// <summary>
            /// StringFormat used by System.Drawing.System.Drawing to format string</summary>
            public System.Drawing.StringFormat Format;
            /// <summary>
            /// CustomStringFormatFlags used to format string</summary>
            public CustomStringFormatFlags CustomFlags;
        }

        /// <summary>
        /// Gets or sets the text formatting that is used to display a property as text. Modify this property
        /// to change text rendering in all PropertyEditingControls. Is a subset of ExtendedTextFormat.
        /// Setting this also affects the ExtendedTextFormat property.</summary>
        public static StringFormat TextFormat
        {
            get { return s_textFormat.Format; }
            set { s_textFormat.Format = value; }
        }

        /// <summary>
        /// Gets or sets additional text formatting information. Setting this also affects the TextFormat property.</summary>
        public static ExtendedStringFormat ExtendedTextFormat
        {
            get { return s_textFormat; }
            set { s_textFormat = value; }
        }

        /// <summary>
        /// Gets whether the control is currently drawing the property as an editable value</summary>
        public static bool DrawingEditableValue
        {
            get { return s_drawingEditableValue; }
        }

        /// <summary>
        /// Draws the property editing representation, in the same way that the control
        /// presents the property in the GUI. Use this method to draw inactive properties
        /// when multitasking a single PropertyEditingControl.</summary>
        /// <param name="descriptor">Property descriptor</param>
        /// <param name="context">Type descriptor context</param>
        /// <param name="bounds">Bounds containing graphics</param>
        /// <param name="font">Font to use for rendering property text</param>
        /// <param name="brush">Brush for rendering property text</param>
        /// <param name="g">Graphics object</param>
        public static void DrawProperty(
            PropertyDescriptor descriptor,
            ITypeDescriptorContext context,
            Rectangle bounds,
            Font font,
            Brush brush,
            Graphics g)
        {
            object owner = context.Instance;
            if (owner == null)
                return;

            UITypeEditor editor = WinFormsPropertyUtils.GetUITypeEditor(descriptor, context);
            if (editor != null)
            {
                if (editor.GetPaintValueSupported(context))
                {
                    object value = descriptor.GetValue(owner);
                    Rectangle paintRect = new Rectangle(
                        bounds.Left + 1,
                        bounds.Top + 1,
                        PaintRectWidth,
                        PaintRectHeight);
                    editor.PaintValue(new PaintValueEventArgs(context, value, g, paintRect));
                    bounds.X += PaintRectTextOffset;
                    bounds.Width -= PaintRectTextOffset;

                    g.DrawRectangle(SystemPens.ControlDark, paintRect);
                }
            }

            string valueString = PropertyUtils.GetPropertyText(owner, descriptor);
            bounds.Height = font.Height;

            if (s_textFormat.CustomFlags == CustomStringFormatFlags.TrimLeftWithEllipses)
                valueString = TrimStringLeftWithEllipses(valueString, bounds, font, g);

            g.DrawString(valueString, font, brush, bounds, s_textFormat.Format);
        }

        private const int PaintRectWidth = 19;
        private const int PaintRectHeight = 15;
        private const int PaintRectTextOffset = PaintRectWidth + 4;
        private const int TrimmingEllipsesWidth = 16;

        private static string TrimStringLeftWithEllipses(string text, Rectangle bounds, Font font, Graphics graphics)
        {
            if (text.Length == 0)
                return text;

            if (bounds.Width == 0)
                return "";

            SizeF size = graphics.MeasureString(text, font);

            if ((int)size.Width <= bounds.Width)
                return text;

            int targetWidth = bounds.Width - TrimmingEllipsesWidth;
            if (targetWidth <= 0)
                return "";

            // Shorten our string until it more or less fits.
            int iteration = 0;
            do
            {
                float visibleRatio = (float)targetWidth / size.Width; // < 1
                int guessedTextLength = (int)(text.Length * visibleRatio);

                if (guessedTextLength == text.Length)
                    --guessedTextLength;

                // Short text to guessedTextLength chars by removing chars at the front
                text = text.Remove(0, text.Length - guessedTextLength);

                size = graphics.MeasureString(text, font);
            }
            while (size.Width > targetWidth && ++iteration < 5 && text.Length > 0);

            return "... " + text;
        }

        /// <summary>
        /// Event that is raised when a property is edited</summary>
        public event EventHandler<PropertyEditedEventArgs> PropertyEdited;

        /// <summary>
        /// Performs actions related to a property edit. Calls PropertyEdited event.</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnPropertyEdited(PropertyEditedEventArgs e)
        {
            if (PropertyEdited != null)
            {
                PropertyEdited(this, e);
            }
        }

        #region IServiceProvider Members

        /// <summary>
        /// Gets the service object of the specified type</summary>
        /// <param name="serviceType">An object that specifies the type of service object to get</param>
        /// <returns>A service object of type serviceType, or null if there is no service object of the given type</returns>
        public new object GetService(Type serviceType)
        {
            if (typeof(IWindowsFormsEditorService).IsAssignableFrom(serviceType) ||
                typeof(ITypeDescriptorContext).IsAssignableFrom(serviceType))
                return this;

            return base.GetService(serviceType);
        }

        #endregion

        #region IWindowsFormsEditorService Members

        /// <summary>
        /// Drops down editor control</summary>
        /// <param name="control">The control to drop down</param>
        public void DropDownControl(Control control)
        {
            m_dropDownForm.SetControl(control);

            Point rightBottom = new Point(base.Right, base.Bottom);
            rightBottom = base.Parent.PointToScreen(rightBottom);
            Rectangle bounds = new Rectangle(
                rightBottom.X - control.Width, rightBottom.Y, control.Width, control.Height);

            //Rectangle workingArea = Screen.FromControl(this).WorkingArea;

            m_dropDownForm.Bounds = bounds;
            m_dropDownForm.Visible = true;

            control.Focus();

            while (m_dropDownForm.Visible)
            {
                Application.DoEvents();
                MsgWaitForMultipleObjects(0, 0, true, 250, 255);
            }
        }

        /// <summary>
        /// Closes the dropped down editor</summary>
        public void CloseDropDown()
        {
            if (m_closingDropDown)
                return;

            // set the focus back to the text box right here so this control never loses focus
            EnableTextBox();

            try
            {
                m_closingDropDown = true;
                if (m_dropDownForm.Visible)
                {
                    m_dropDownForm.SetControl(null);
                    m_dropDownForm.Visible = false;
                }
            }
            finally
            {
                m_closingDropDown = false;
            }
        }

        /// <summary>
        /// Opens a dialog editor</summary>
        /// <param name="dialog">The dialog to open</param>
        /// <returns>Result of user interaction with dialog</returns>
        public DialogResult ShowDialog(Form dialog)
        {
            dialog.ShowDialog(this);
            return dialog.DialogResult;
        }

        [DllImport("user32.dll")]
        private static extern int MsgWaitForMultipleObjects(
            int nCount,           // number of handles in array
            int pHandles,         // object-handle array
            bool bWaitAll,        // wait option
            int dwMilliseconds,   // time-out interval
            int dwWakeMask        // input-event type
            );

        /// <summary>
        /// Hides the drop-down editor</summary>
        protected void HideForm()
        {
            CloseDropDown();
        }

        #endregion

        #region ITypeDescriptorContext Members

        /// <summary>
        /// Gets the object that is connected with this type descriptor request</summary>
        /// <returns>The object that invokes the method on the <see cref="T:System.ComponentModel.TypeDescriptor"></see>; 
        /// otherwise, null if there is no object responsible for the call</returns>
        public object Instance
        {
            get { return m_context.LastSelectedObject; }
        }

        /// <summary>
        /// Raises the <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentChanged"></see> event</summary>
        public void OnComponentChanged()
        {
        }

        /// <summary>
        /// Raises the <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentChanging"></see> event</summary>
        /// <returns>True iff this object can be changed</returns>
        public bool OnComponentChanging()
        {
            return true;
        }

        /// <summary>
        /// Gets the <see cref="T:System.ComponentModel.PropertyDescriptor"></see> that is associated with the given context item</summary>
        /// <returns>The <see cref="T:System.ComponentModel.PropertyDescriptor"></see> that describes the given context item; 
        /// otherwise, null if there is no <see cref="T:System.ComponentModel.PropertyDescriptor"></see> responsible for the call</returns>
        public PropertyDescriptor PropertyDescriptor
        {
            get { return m_descriptor; }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Sets whether the control can drag and drop</summary>
        public override bool AllowDrop
        {
            set
            {
                base.AllowDrop = value;
                m_textBox.AllowDrop = value;
            }
        }

        /// <summary>
        /// Process a command shortcut key. If the Control key needs to be held down,
        /// then this is the place to check for it.</summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData == (Keys.Down | Keys.Alt) || keyData == (Keys.Down | Keys.Control))
                && m_editButton.Visible)
            {
                OpenDropDownEditor();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Processes a dialog key</summary>
        /// <param name="keyData">One of the System.Windows.Forms.Keys values that represents the key to process</param>
        /// <returns>True iff the key was processed by the control</returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                // process the Enter keys on the base control as well. The default Control Enter key
                // handler must do something significant. This is the only way I have found for users of the 
                // ProperyEditingControl, in this case the GridView to get its call to ProcessDialogKey
                // This needs to be called before we close this text box, otherwise the ProcessDialogKey will not be
                // consumed and will be fired AGAIN (in this case in the GridView). 
                base.ProcessDialogKey(keyData);

                Flush();
                DisableTextBox();
                // hide the editing control to give visual feedback of committed  change
                Visible = false;

                return true;
            }
            else if (keyData == Keys.Escape)
            {
                // process the Escape keys on the base control as well. The default Control Esc key
                // handler must do something significant. This is the only way I have found for users of the 
                // ProperyEditingControl, in this case the GridView to get its call to ProcessDialogKey
                // This needs to be called before we close this text box, otherwise the ProcessDialogKey will not be
                // consumed and will be fired AGAIN (in this case in the GridView). 
                base.ProcessDialogKey(keyData);

                CancelEdit();

                return true;
            }
            else if (keyData == Keys.Tab || keyData == (Keys.Tab | Keys.Shift))
            {
                Flush();
            }

            return base.ProcessDialogKey(keyData);
        }

        /// <summary>
        /// Performs custom actions and raises the <see cref="E:System.Windows.Forms.Control.Layout"></see> event</summary>
        /// <param name="levent">A <see cref="T:System.Windows.Forms.LayoutEventArgs"></see> that contains the event data</param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (m_context != null)
            {
                int x = 1;
                int width = base.Width;

                UITypeEditor editor = WinFormsPropertyUtils.GetUITypeEditor(m_descriptor, this);
                if (editor != null)
                {
                    if (editor.GetPaintValueSupported(this))
                        x += PaintRectTextOffset;

                    m_textBox.Left = x;
                    m_textBox.Width = width - x - m_editButton.Width;
                }
                else
                {
                    // no editor, use text box and type converters
                    m_textBox.Left = x;
                    m_textBox.Width = width - x;
                }
            }

            base.OnLayout(levent);
        }
      
        /// <summary>
        /// Makes the TextBox in this property editor visible</summary>
        protected void EnableTextBox()
        {
            // pass focus on to our TextBox control
            m_textBox.Show();
            SetTextBoxFromProperty();
            m_textBox.SelectAll();
            m_textBox.Focus();
        }

        /// <summary>
        /// Makes the TextBox in this property editor invisible</summary>
        protected void DisableTextBox()
        {
            m_textBox.Hide();
        }

        /// <summary>
        /// Performs custom actions and raises the <see cref="E:System.Windows.Forms.Control.MouseClick"></see> event</summary>
        /// <param name="e">A MouseEventArgs that contains the event data</param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            EnableTextBox();
            base.OnMouseClick(e);
        }

        /// <summary>
        /// Performs custom actions and raises the <see cref="E:System.Windows.Forms.Control.GotFocus"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnGotFocus(EventArgs e)
        {
            EnableTextBox();
            base.OnGotFocus(e);
        }

        /// <summary>
        /// Forces the control to invalidate its client area and immediately redraw itself and any child controls.</summary>
        /// <PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public override void Refresh()
        {
            if (m_textBox.Visible)
                SetTextBoxFromProperty();

            base.Refresh();
        }

        #endregion

        #region Event Handlers

        private void editButton_MouseDown(object sender, MouseEventArgs e)
        {
            m_absorbEditButtonClick = m_isEditing;
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            if (m_absorbEditButtonClick)
            {
                m_absorbEditButtonClick = false;
                return;
            }

            OpenDropDownEditor();
        }

        private void textBox_LostFocus(object sender, EventArgs e)
        {
            SetPropertyFromTextBox();

            // if we are editting via the drop down, do not disable the text box
            // we want to keep focus on the text box here. other wise focus will just to what ever
            // control is next in line, then will be set right back to this control a moment later.
            if (!m_isEditing)
                DisableTextBox();
        }

        private void textBox_DragOver(object sender, DragEventArgs e)
        {
            // raise event on this control
            OnDragOver(e);
        }

        private void textBox_DragDrop(object sender, DragEventArgs e)
        {
            // raise event on this control
            OnDragDrop(e);
        }

        private void textBox_MouseHover(object sender, EventArgs e)
        {
            // raise event on this control
            OnMouseHover(e);
        }

        private void textBox_MouseLeave(object sender, EventArgs e)
        {
            // raise event on this control
            OnMouseLeave(e);
        }

        #endregion

        private void OpenDropDownEditor()
        {
            try
            {
                m_isEditing = true;

                PropertyEditorControlContext oldContext = m_context;
                object oldValue, value;
                try
                {
                    // Certain property editing controls like the FlagsUITypeEditor's private CheckedListBox will
                    //  not lose focus until the user clicks away, and the user's click may change the
                    //  PropertyEditorControlContext's selection, so let's temporarily freeze the selection.
                    oldContext.CacheSelection();

                    oldValue = m_context.GetValue();
                    UITypeEditor editor = WinFormsPropertyUtils.GetUITypeEditor(m_descriptor, this);

                    // Bring up the editor which can cause Bind() to be called, so make sure that we use the
                    //  correct context and selection after this EditValue call.
                    value = editor.EditValue(this, this, oldValue);

                    oldContext.SetValue(value);
                }
                finally
                {
                    oldContext.ClearCachedSelection();
                }

                // notify that we just changed a value
                NotifyPropertyEdit(oldValue, value);

                // Refresh text box, paint rect
                if (oldContext == m_context)
                {
                    SetTextBoxFromProperty();
                    EnableTextBox();
                }

                Invalidate();
            }
            finally
            {
                m_isEditing = false;
            }
        }

        private void SetPropertyFromTextBox()
        {
            if (m_settingValue)
                return;

            try
            {
                m_settingValue = true;

                string newText = m_textBox.Text;

                // for enum list, ensure case correctness 
                if (m_textBox.AutoCompleteEnabled)
                {
                    bool validText = false;
                    foreach (string item in m_textBox.AutoCompleteCustomSource)
                    {
                        if (string.Compare(item, newText, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            newText = item;
                            validText = true;
                            break;
                        }
                    }

                    if (!validText && m_context != null)
                    {
                        newText = m_initialText;
                    }
                }

                bool userHasEdited = newText != m_initialText;

                if (m_context != null &&
                    userHasEdited)
                {
                    object oldValue = m_context.GetValue();

                    // get prospective new value, normalized for the property descriptor
                    object value;
                    if (TryConvertString(newText, out value))
                    {
                        m_context.SetValue(value);

                        NotifyPropertyEdit(oldValue, value);

                        m_initialText = newText;
                    }
                }
            }
            finally
            {
                m_settingValue = false;
            }
        }

        private bool TryConvertString(string newText, out object value)
        {
            bool succeeded = false;
            value = newText;
            try
            {
                TypeConverter converter = m_descriptor.Converter;
                if (converter != null &&
                    value != null &&
                    converter.CanConvertFrom(value.GetType()))
                {
                    // makes sure a reference to this is available within the ConvertFrom() method in
                    // the CoreRefTypeConverter. We use this to trigger the DropDown when someone pastes a FileName
                    // into the Control
                    value = converter.ConvertFrom(this, CultureInfo.CurrentCulture, value);
                }
                succeeded = true;
            }
            catch (Exception ex)
            {
                // NotSupportedException, FormatException, and Exception can be thrown. For example,
                // for a string "14'" being converted to an Int32. So, I made this a catch (Exception). --Ron
                CancelEdit();
                MessageBox.Show(ex.Message, "Error".Localize());
            }

            return succeeded;
        }

        private void SetTextBoxFromProperty()
        {
            if (m_context != null && m_context.LastSelectedObject != null)
            {
                string propertyText = PropertyUtils.GetPropertyText(m_context.LastSelectedObject, m_descriptor);
                m_initialText = propertyText;
                m_textBox.Text = propertyText;
                m_textBox.Font = Font;
                m_textBox.ReadOnly = m_descriptor.IsReadOnly;                
                m_textBox.ForeColor = (m_descriptor.IsReadOnly) ? SystemColors.GrayText : ForeColor;
            }
        }

        private void NotifyPropertyEdit(object oldValue, object newValue)
        {
            OnPropertyEdited(
                new PropertyEditedEventArgs(m_context.LastSelectedObject, m_descriptor, oldValue, newValue));
        }

        private PropertyEditorControlContext m_context;
        private PropertyDescriptor m_descriptor;

        private string m_initialText = string.Empty;
        private bool m_settingValue;
        private bool m_isEditing;
        private bool m_absorbEditButtonClick;

        private readonly EditButton m_editButton;
        private readonly TextBox m_textBox;

        private readonly DropDownForm m_dropDownForm;
        private bool m_closingDropDown;

        /// <summary>
        /// Static constructor</summary>
        static PropertyEditingControl()
        {
            s_textFormat = new ExtendedStringFormat();
            s_textFormat.Format = new System.Drawing.StringFormat();
            s_textFormat.Format.Alignment = StringAlignment.Near;
            s_textFormat.Format.Trimming = StringTrimming.EllipsisPath;// StringTrimming.EllipsisCharacter;
            s_textFormat.Format.FormatFlags = StringFormatFlags.NoWrap;
            s_textFormat.CustomFlags = CustomStringFormatFlags.None;
        }

        private static ExtendedStringFormat s_textFormat;
        private static bool s_drawingEditableValue;

        private class EditButton : Button
        {
            public EditButton()
            {
                base.SetStyle(ControlStyles.Selectable, true);
                base.BackColor = SystemColors.Control;
                base.ForeColor = SystemColors.ControlText;
                base.TabStop = false;
                base.IsDefault = false;
            }

            /// <summary>
            /// Gets or sets whether the button is for a modal dialog or a drop down control</summary>
            public bool Modal
            {
                get { return m_modal; }
                set
                {
                    m_modal = value;
                    Invalidate();
                }
            }

            protected override void OnMouseDown(MouseEventArgs arg)
            {
                base.OnMouseDown(arg);
                if (arg.Button == MouseButtons.Left)
                {
                    m_pushed = true;
                    Invalidate();
                }
            }

            protected override void OnMouseUp(MouseEventArgs arg)
            {
                base.OnMouseUp(arg);
                if (arg.Button == MouseButtons.Left)
                {
                    m_pushed = false;
                    Invalidate();
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                Rectangle r = ClientRectangle;

                if (m_modal)
                {
                    base.OnPaint(e);
                    // draws "..."
                    int x = r.X + r.Width / 2 - 5;
                    int y = r.Bottom - 5;
                    using (Brush brush = new SolidBrush(Enabled ? SystemColors.ControlText : SystemColors.GrayText))
                    {
                        g.FillRectangle(brush, x, y, 2, 2);
                        g.FillRectangle(brush, x + 4, y, 2, 2);
                        g.FillRectangle(brush, x + 8, y, 2, 2);
                    }
                }
                else
                {
                    if (Application.RenderWithVisualStyles)
                    {
                        ComboBoxRenderer.DrawDropDownButton(
                            g,
                            ClientRectangle,
                            !Enabled ? ComboBoxState.Disabled : (m_pushed ? ComboBoxState.Pressed : ComboBoxState.Normal));
                    }
                    else
                    {
                        ControlPaint.DrawButton(
                            g,
                            ClientRectangle,
                            !Enabled ? ButtonState.Inactive : (m_pushed ? ButtonState.Pushed : ButtonState.Normal));
                    }
                }
            }

            private bool m_modal;
            private bool m_pushed;
        }

        private class DropDownForm : Form
        {
            public DropDownForm(PropertyEditingControl parent)
            {
                m_parent = parent;

                base.StartPosition = FormStartPosition.Manual;
                base.ShowInTaskbar = false;
                base.ControlBox = false;
                base.MinimizeBox = false;
                base.MaximizeBox = false;
                base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                base.Visible = false;
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                    m_parent.CloseDropDown();

                base.OnMouseDown(e);
            }

            protected override void OnClosed(EventArgs e)
            {
                if (Visible)
                    m_parent.CloseDropDown();

                base.OnClosed(e);
            }

            protected override void OnDeactivate(EventArgs e)
            {
                if (Visible)
                    m_parent.CloseDropDown();

                base.OnDeactivate(e);
            }

            public void SetControl(Control control)
            {
                if (m_control != null)
                {
                    Controls.Remove(m_control);
                    m_control.Resize -= control_Resize;
                    m_control = null;
                }
                if (control != null)
                {
                    control.Width = Math.Max(m_parent.Width, control.Width);
                    Size = control.Size;

                    m_control = control;
                    Controls.Add(m_control);

                    m_control.Location = new Point(0, 0);
                    m_control.Visible = true;
                    m_control.Resize += control_Resize;
                }
                Enabled = m_control != null;
            }

            private void control_Resize(object sender, EventArgs e)
            {
                Rectangle bounds = Bounds;
                bounds.Width = Math.Max(m_parent.Width, m_control.Width);
                Rectangle workingArea = SystemInformation.WorkingArea;
                if (bounds.Right > workingArea.Right)
                    bounds.X -= bounds.Right - workingArea.Right;
                if (bounds.X < workingArea.Left)
                    bounds.X = workingArea.Left;

                Bounds = bounds;
            }

            private Control m_control;
            private readonly PropertyEditingControl m_parent;
        }

        private class TextBox : System.Windows.Forms.TextBox
        {
            public TextBox()
            {
                AutoSize = false;
            }

            protected override void OnFontChanged(EventArgs e)
            {
                base.OnFontChanged(e); //updates FontHeight property
                Height = FontHeight;
            }

            // Up and Down keys need to be seen by our containing Control (PropertyView or one of the
            //  derived classes, PropertyGridView or GridView) in order to allow for navigation to
            //  other properties.
            protected override bool IsInputKey(Keys keyData)
            {
                if (keyData == Keys.Up ||
                    keyData == Keys.Down)
                {
                    return false;
                }

                // For the spreadsheet-style property editor, we want to let it change the focused
                //  property using the left and right arrow keys, but only if the caret is at either
                //  end of the text.
                if (keyData == Keys.Left &&
                    SelectionStart == 0 &&
                    SelectionLength == 0)
                {
                    return false;
                }
                if (keyData == Keys.Right &&
                    SelectionStart == Text.Length)
                {
                    return false;
                }

                // If text is already selected, and the user presses the left arrow key (without holding
                //  a modifier key), the caret should go to the front. The default behavior is to put the
                //  caret at the end.
                if (keyData == Keys.Left &&
                    SelectionLength > 0)
                {
                    SelectionLength = 0;
                }

                return base.IsInputKey(keyData);
            }

            protected override void OnKeyPress(KeyPressEventArgs e)
            {
                if (AutoCompleteEnabled)
                {
                    if (char.IsLetterOrDigit(e.KeyChar))
                    {
                        int length = SelectionStart < base.Text.Length ? SelectionStart : base.Text.Length;

                        StringBuilder sb = new StringBuilder();
                        sb.Append(base.Text.Substring(0, length));
                        sb.Append(e.KeyChar);
                        string text = sb.ToString();
                        if (!ValidateInput(text))
                            e.Handled = true;
                    }
                }

                base.OnKeyPress(e);
            }

            public bool AutoCompleteEnabled
            {
                get
                {
                    return AutoCompleteMode == AutoCompleteMode.SuggestAppend && AutoCompleteCustomSource.Count > 0;
                }
            }

            private bool ValidateInput(string text)
            {
                foreach (string item in AutoCompleteCustomSource)
                {
                    if (string.Compare(text, 0, item, 0, text.Length, true, CultureInfo.CurrentCulture) == 0)
                        return true;
                }
                return false;
            }
        }
    }
}
