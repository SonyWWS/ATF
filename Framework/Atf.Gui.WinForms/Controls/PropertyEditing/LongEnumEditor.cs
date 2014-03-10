//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Permissions;

using Sce.Atf.Dom;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Property editor that supplies an autocomplete ComboBox to facilitate  
    /// editing long enum-like attributes and values. The enum list can be updated
    /// programmatically.</summary>
    /// <remarks>
    /// It is recommended that you use Sce.Atf.Controls.PropertyEditing.EnumTypeConverter
    /// as the TypeConverter for this editor's property, to support display names.
    /// A LongEnumEditor is created for each PropertyDescriptor, for each
    /// selection change. PropertyDescriptors are shared between property editors,
    /// such as GridView and PropertyGridView.</remarks>
    public class LongEnumEditor : IPropertyEditor, IAnnotatedParams
    {

        /// <summary>
        /// Default constructor
        /// </summary>
        public LongEnumEditor()
        {
        }

        /// <summary>
        /// Construct LongEnumEditor using the given argument</summary>
        /// <param name="enumType">The enum type</param>
        public LongEnumEditor(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new System.ArgumentException("enumType must Enum");
            Array ar = Enum.GetValues(enumType);
            int[] vals = new int[ar.GetLength(0)];
            Array.Copy(ar,vals,vals.Length);
            DefineEnum(Enum.GetNames(enumType), vals);
        }
        
        /// <summary>
        /// Event that is raised when the selection index of the dropdown list is changing.
        /// Set e.Cancel to true if you do not wish the selection to change.
        /// The 'sender' parameter is the new string.</summary>
        public static event CancelEventHandler EnumSelectionChanging;

        /// <summary>
        /// Sets the height of the dropdown control</summary>
        /// <remarks>The dropdown control uses the default height if the value is not set.</remarks>
        public int DropDownHeight
        {
            set { m_dropDownHeight = value; }
        }

        #region IAnnotatedParams Members

        /// <summary>
        /// Initializes the control</summary>
        /// <param name="parameters">Editor parameters</param>
        public void Initialize(string[] parameters)
        {
            DefineEnum(parameters);
        }

        #endregion

        /// <summary>
        /// Defines the enum names and values</summary>
        /// <param name="names">Enum names array</param>
        /// <remarks>Enum values default to successive integers, starting with 0. Enum names
        /// with the format "EnumName=X" are parsed so that EnumName gets the value X, where X is
        /// an int.</remarks>
        public void DefineEnum(string[] names)
        {
            EnumUtil.ParseEnumDefinitions(names, out m_names, out m_displayNames, out m_values);
        }

        /// <summary>
        /// Defines the enum names and values</summary>
        /// <param name="names">Enum names array</param>
        /// <param name="values">Enum values array</param>
        public void DefineEnum(string[] names, int[] values)
        {
            if (names == null || values == null || names.Length != values.Length)
                throw new ArgumentException("names and/or values null, or of unequal length");

            m_names = names;
            m_displayNames = names;
            m_values = values;

        }

        private delegate string ConvertIdToDisplayDelegate(string id, ref bool valueMatched);
        private delegate string ConvertDisplayToIdDelegate(string id);

        
        #region IPropertyEditor Members

        /// <summary>
        /// Gets a control to edit the given property</summary>
        /// <param name="context">Context for property editing control</param>
        /// <returns>Control to edit the given context</returns>
        public virtual Control GetEditingControl(PropertyEditorControlContext context)
        {
            LongEnumControlWrapper wrapper = new LongEnumControlWrapper(context, m_dropDownHeight);
            wrapper.EnumControl.ConvertIdToDisplayCallback = ConvertIdToDisplay;
            wrapper.EnumControl.ConvertDisplayToIdCallback = ConvertDisplayToId;
            UpdateEnumControl(wrapper.EnumControl);
            return wrapper;            
        }

        #endregion

        private string ConvertIdToDisplay(string id, ref bool valueMatched)
        {
            valueMatched = false;
            if (m_names != null)
            {
                for (int i = 0; i < m_names.Length; i++)
                    if (id == m_names[i])
                    {
                        valueMatched = true;
                        return m_displayNames[i];
                    }

            }
            return id;
        }

        private string ConvertDisplayToId(string displayName)
        {
            for (int i = 0; i < m_displayNames.Length; i++)
                if (displayName == m_displayNames[i])
                    return m_names[i];
            return displayName;
        }

        // Updates the enum list on all the Controls created by this LongEnumEditor.
        private void UpdateEnumControl(LongEnumControl enumControl)
        {
            if (m_names == null) // no enumeration defined
                return;

            AutoCompleteStringCollection filterVals = new AutoCompleteStringCollection();
            enumControl.m_listBox.Items.Clear();
            for (int i = 0; i < m_names.Length; i++)
            {
                enumControl.m_listBox.Items.Add(m_displayNames[i]);
                filterVals.Add(m_displayNames[i]);
            }
            if (filterVals.Count > 0)
                enumControl.AutoCompleteCustomSource = filterVals;
        }

        // Wrap LongEnumControl so we can set the proper height
        private class LongEnumControlWrapper : Control, ICacheablePropertyControl
        {
            public LongEnumControlWrapper(PropertyEditorControlContext context, int dropDownHeight)
            {
                DoubleBuffered = true;
                m_longEnumControl = new LongEnumControl(context, dropDownHeight);
                
                Controls.Add(m_longEnumControl);
                m_longEnumControl.SizeChanged += (sender, e) => Height = m_longEnumControl.Height + 2;
                
                SizeChanged += (sender, e) =>
                    {
                        m_longEnumControl.Location = new Point(1, 1);
                        m_longEnumControl.Size = new Size(Width - 1, Height - 1);
                    };
               
            }

            public override void Refresh()
            {
                m_longEnumControl.Refresh();

                base.Refresh();
            }

            public LongEnumControl EnumControl
            {
                get { return m_longEnumControl; }
            }

            #region ICacheablePropertyControl

            /// <summary>
            /// Whether or not this Control is cacheable across selection changes. </summary>
            /// <remarks>Can't be cached because the enum may change programmatically.</remarks>
            public bool Cacheable
            {
                get { return false; }
            }

            #endregion


            /// <summary>
            /// Update child control background</summary>            
            protected override void OnBackColorChanged(EventArgs e)
            {
                foreach (Control control in Controls)
                    control.BackColor = BackColor;
                base.OnBackColorChanged(e);
            }

            private LongEnumControl m_longEnumControl;

        }

        /// <summary>
        /// Control for editing long enum</summary>
        private class LongEnumControl : ComboBox
        {
            private Control m_dropDownCtrl; // actual drop-down control
            private bool m_isDroppedDown;// Indicates if drop-down is currently shown.
            private PopupControl m_popupCtrl = new PopupControl();
            internal ListBox m_listBox;

            private Font m_defaultFont;
            private Font m_boldFont;
            private Color m_defaultColor;

            private DateTime m_lastHideTime = DateTime.Now;
            private bool m_keepDropDownOpen;

            internal ConvertIdToDisplayDelegate ConvertIdToDisplayCallback;
            internal ConvertDisplayToIdDelegate ConvertDisplayToIdCallback;


            public LongEnumControl(PropertyEditorControlContext context, int dropDownHeight)
            {
                m_listBox = new ListBox();
                m_listBox.PreviewKeyDown += listBox_PreviewKeyDown;
                if (dropDownHeight > 0)
                    m_listBox.Height = dropDownHeight;
                m_listBox.SelectedIndexChanged += comboBox_SelectedIndexChanged;
                DropDownControl = m_listBox;
                DoubleBuffered = true;
                this.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                this.AutoCompleteSource = AutoCompleteSource.CustomSource;
                this.FormattingEnabled = true;
                m_context = context;

                this.FlatStyle = System.Windows.Forms.FlatStyle.System;
                this.SelectedIndexChanged += comboBox_SelectedIndexChanged;

                m_defaultFont = new Font(Font, FontStyle.Regular);
                m_boldFont = new Font(Font, FontStyle.Bold);
                m_defaultColor = this.ForeColor;
            }

            private void listBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
            {
                m_keepDropDownOpen = false;
                if (e.KeyData == Keys.Up)
                {
                    m_keepDropDownOpen = true;
                }
                else if (e.KeyData == Keys.Down)
                {
                    m_keepDropDownOpen = true;
                }
                else if (e.KeyData == Keys.Enter || e.KeyData == Keys.Tab || e.KeyData == (Keys.Tab | Keys.Shift))
                {
                    SetProperty();
                    HideDropDown();
                }
                else if (e.KeyData == Keys.Escape)
                {
                    RefreshValue();
                    HideDropDown();
                }
            }

            public Control DropDownControl
            {
                get { return m_dropDownCtrl; }
                set { AssignControl(value); }
            }

            public bool IsDroppedDown
            {
                get { return this.m_isDroppedDown; }
            }

            #region Win32 message handlers

            public override bool PreProcessMessage(ref Message m)
            {
                if (m.Msg == (Sce.Atf.User32.WM_REFLECT + Sce.Atf.User32.WM_COMMAND))
                {
                    if (Sce.Atf.User32.HIWORD((int)m.WParam) == Sce.Atf.User32.CBN_DROPDOWN)
                        return false;
                }
                return base.PreProcessMessage(ref m);
            }

            private static readonly DateTime m_showTime = DateTime.Now;

            private void AutoDropDown()
            {
                if (m_popupCtrl != null && m_popupCtrl.Visible)
                    HideDropDown();
                else if ((DateTime.Now - m_lastHideTime).TotalMilliseconds > 150.0)
                    ShowDropDown();
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == Sce.Atf.User32.WM_LBUTTONDOWN)
                {
                    AutoDropDown();
                    return;
                }

                if (m.Msg == (Sce.Atf.User32.WM_REFLECT + Sce.Atf.User32.WM_COMMAND))
                {
                    switch (Sce.Atf.User32.HIWORD((int)m.WParam))
                    {
                        case Sce.Atf.User32.CBN_DROPDOWN:
                            AutoDropDown();
                            return;

                        case Sce.Atf.User32.CBN_CLOSEUP:
                            if ((DateTime.Now - m_showTime).TotalSeconds > 1.0)
                                HideDropDown();
                            return;
                    }
                }

                base.WndProc(ref m);
            }

            #endregion


            /// <summary>
            /// Assigns control to be used as drop-down</summary>
            /// <param name="control">Control</param>
            private void AssignControl(Control control)
            {
                // If specified control is different then...
                if (control != DropDownControl)
                {
                    // Reference the user-specified drop down control.
                    m_dropDownCtrl = control;
                }
            }

            /// <summary>
            /// Displays drop-down area of combo box, if not already shown</summary>
            public void ShowDropDown()
            {
                if (m_popupCtrl != null && !IsDroppedDown)
                {
                    Point location = PointToScreen(new Point(0, Height));

                    // Actually show popup.
                    PopupResizeMode resizeMode = PopupResizeMode.BottomRight;
                    m_popupCtrl.Show(this.DropDownControl, location.X, location.Y, Width, Height, resizeMode);
                    m_isDroppedDown = true;

                    m_popupCtrl.PopupControlHost = this;
                }
            }

            /// <summary>
            /// Hides drop-down area of combo box, if shown</summary>
            public void HideDropDown()
            {
                if (m_popupCtrl != null && IsDroppedDown)
                {
                    // Hide drop-down control.
                    m_popupCtrl.Hide();
                    m_isDroppedDown = false;
                    m_lastHideTime = DateTime.Now;
                }
            }

            /// <summary>
            /// Checks if input key</summary>
            /// <param name="keyData">Key</param>
            /// <returns>True iff key is input key</returns>
            /// <remarks>Up and down keys need to be seen by our containing Control (PropertyView or one of the
            /// derived classes, PropertyGridView or GridView) to allow for navigation to
            /// other properties.</remarks>
            protected override bool IsInputKey(Keys keyData)
            {
                if (keyData == Keys.Up ||
                    keyData == Keys.Down)
                {
                    return false;
                }

                return base.IsInputKey(keyData);
            }

            /// <summary>
            /// Process command key</summary>
            /// <param name="msg">Windows message</param>
            /// <param name="keyData">Key</param>
            /// <returns>True iff key processed</returns>
            /// <remarks>Calling the base ProcessCmdKey allows this keypress to be consumed by owning
            /// Controls like PropertyView and PropertyGridView and ControlHostService.
            /// Returning false allows the keypress to escape to IsInputKey and to any built-in
            /// text editing functionality, like Ctrl+A (copy text), Ctrl+V, Ctrl+X, Delete, etc.
            /// Returning true means that this keypress has been consumed by this method and this
            /// event is not passed on to any other methods or Controls.</remarks>
            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                switch (keyData)
                {
                    // If we don't handle Enter here, the DOM is not modified until focus is lost.
                    // We return 'false' to allow the built-in functionality of selecting all the
                    //  text, which is an important visual cue that the Enter key did something.
                    case Keys.Enter:
                        SetProperty();
                        return false;

                    // If we don't handle Ctrl+A here, the ComboBox does something weird, perhaps
                    //  inserting an unprintable character or newline or something that clears
                    //  the editable text in the ComboBox. We return 'true' to avoid the bad
                    //  built-in behavior.
                    case (Keys.Control | Keys.A):
                        SelectAll();
                        return true;
                }

                // For some reason, the ATF WinFormsUtil.GetFocusedControl() returns null and so the check
                //  for standard text input doesn't get performed by ControlHostService. So we need
                //  to do it. http://sf.ship.scea.com/sf/go/artf34968
                if (KeysUtil.IsTextBoxInput(this, keyData))
                    return false;

                return base.ProcessCmdKey(ref msg, keyData);
            }

            protected override void OnValidating(System.ComponentModel.CancelEventArgs e)
            {
                //do nothing
            }

            public override void Refresh()
            {
                if (!Focused)
                    RefreshValue();

                base.Refresh();
            }

            /// <param name="e">Event args</param>
            protected override void OnLostFocus(EventArgs e)
            {
                SetProperty();

                base.OnLostFocus(e);
            }

            /// <summary>
            /// Update child control background</summary>            
            protected override void OnBackColorChanged(EventArgs e)
            {
                foreach (Control control in Controls)
                    control.BackColor = BackColor;
                DropDownControl.BackColor = BackColor;
                     
                base.OnBackColorChanged(e);
            }

            private void RefreshValue()
            {
                try
                {
                    m_refreshing = true;

                    object value = m_context.GetValue();
                    if (value == null)
                        this.Enabled = false;
                    else
                    {
                        this.Enabled = true;
                        bool textEditable =
                            m_context.Descriptor.Converter == null &&
                            m_context.Descriptor.Converter.CanConvertFrom((value.GetType()));

                        bool valueMatched = false;
                        if (ConvertIdToDisplayCallback != null)
                            this.Text = ConvertIdToDisplayCallback((string)value, ref valueMatched);

                        if (valueMatched)
                        {
                            bool defaultValue = false;
                            AttributePropertyDescriptor attribute = m_context.Descriptor as AttributePropertyDescriptor;
                            if (attribute != null)
                                if (value.Equals(attribute.AttributeInfo.DefaultValue))
                                    defaultValue = true;

                            if (defaultValue)
                            {
                                Font = m_defaultFont;
                                this.ForeColor = m_defaultColor;
                            }
                            else
                            {
                                if (textEditable)
                                {
                                    Font = m_boldFont;
                                    this.ForeColor = System.Drawing.Color.CadetBlue;
                                }
                                else
                                {
                                    Font = m_boldFont;
                                    this.ForeColor = m_defaultColor;
                                }
                            }
                        }
                        else
                        {
                            Font = m_boldFont;
                            this.ForeColor = m_defaultColor;
                        }
                    }
                }
                finally
                {
                    m_refreshing = false;
                }
            }

            private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (m_listBox.SelectedIndex < 0 || m_listBox.SelectedIndex >= m_listBox.Items.Count)
                    return;

                CancelEventHandler handler = EnumSelectionChanging;
                if (handler != null)
                {
                    CancelEventArgs arg = new CancelEventArgs();
                    handler(m_listBox.Items[m_listBox.SelectedIndex], arg);
                    if (arg.Cancel)
                        return;
                }

                base.Text = (string)m_listBox.Items[m_listBox.SelectedIndex];
                if (!m_keepDropDownOpen)
                {
                    //Only set the property if we're closing the drop down, so that we we don't generate
                    //  undo/redo events on each up or down arrow keypress.
                    SetProperty();
                    HideDropDown();
                }
            }

            private void SetProperty()
            {
                if (!m_refreshing)
                {
                    string value = this.Text;
                    if (ConvertDisplayToIdCallback != null)
                        value = ConvertDisplayToIdCallback(value);
                    m_context.SetValue(value);                     
                }
            }

            private PropertyEditorControlContext m_context;
            private bool m_refreshing;

            #region embedded private classes

            private class PopupControl
            {
                public PopupControl()
                {
                    InitializeDropDown();
                }

                private void dropDown_Closed(object sender, ToolStripDropDownClosedEventArgs e)
                {
                    if (AutoResetWhenClosed)
                        DisposeHost();

                    // Hide drop down within popup control.
                    if (PopupControlHost != null)
                        PopupControlHost.HideDropDown();
                }

                public void Show(Control control, int x, int y, int width, int height, PopupResizeMode resizeMode)
                {
                    InitializeHost(control);

                    m_dropDown.ResizeMode = resizeMode;
                    m_dropDown.Show(x, y, width, height);

                    control.Focus();
                }

                public void Hide()
                {
                    if (m_dropDown != null && m_dropDown.Visible)
                    {
                        m_dropDown.Hide();
                        DisposeHost();
                    }
                }

                #endregion

                #region Internal methods

                protected void DisposeHost()
                {
                    if (m_host != null)
                    {
                        // Make sure host is removed from drop down.
                        if (m_dropDown != null)
                            m_dropDown.Items.Clear();

                        // Dispose of host.
                        m_host = null;
                    }

                    PopupControlHost = null;
                }

                protected void InitializeHost(Control control)
                {
                    InitializeDropDown();

                    // If control is not yet being hosted then initialize host.
                    if (control != Control)
                        DisposeHost();

                    // Create a new host?
                    if (m_host == null)
                    {
                        m_host = new ToolStripControlHost(control);
                        m_host.AutoSize = false;
                        m_host.Padding = Padding;
                        m_host.Margin = Margin;
                    }

                    // Add control to drop-down.
                    m_dropDown.Items.Clear();
                    m_dropDown.Padding = m_dropDown.Margin = Padding.Empty;
                    m_dropDown.Items.Add(m_host);
                }

                protected void InitializeDropDown()
                {
                    // Does a drop down exist?
                    if (m_dropDown == null)
                    {
                        m_dropDown = new PopupDropDown(false);
                        m_dropDown.Closed += new ToolStripDropDownClosedEventHandler(dropDown_Closed);
                    }
                }

                public bool Visible
                {
                    get { return (this.m_dropDown != null && this.m_dropDown.Visible) ? true : false; }
                }

                public Control Control
                {
                    get { return (this.m_host != null) ? this.m_host.Control : null; }
                }

                public Padding Padding
                {
                    get { return this.m_padding; }
                    set { this.m_padding = value; }
                }

                public Padding Margin
                {
                    get { return this.m_margin; }
                    set { this.m_margin = value; }
                }

                public bool AutoResetWhenClosed
                {
                    get { return this.m_autoReset; }
                    set { this.m_autoReset = value; }
                }

                /// <summary>
                /// Gets or sets the popup control host. This is used to hide/show popup.</summary>
                public LongEnumControl PopupControlHost
                {
                    get { return m_popupControlHost; }
                    set { m_popupControlHost = value; }
                }

                LongEnumControl m_popupControlHost;

                private ToolStripControlHost m_host;
                private PopupDropDown m_dropDown;

                private Padding m_padding = Padding.Empty;
                private Padding m_margin = new Padding(1, 1, 1, 1);

                private bool m_autoReset = false;
            }

            private class PopupDropDown : ToolStripDropDown
            {
                public PopupDropDown(bool autoSize)
                {
                    AutoSize = autoSize;
                    Padding = Margin = Padding.Empty;
                }

                protected override void OnClosing(ToolStripDropDownClosingEventArgs e)
                {
                    Control hostedControl = GetHostedControl();
                    if (hostedControl != null)
                        hostedControl.SizeChanged -= hostedControl_SizeChanged;
                    base.OnClosing(e);
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    base.OnPaint(e);

                    // Draw grip area at bottom-right of popup.
                    e.Graphics.FillRectangle(SystemBrushes.ButtonFace, 1, Height - 16, Width - 2, 14);
                    GripBounds = new Rectangle(Width - 17, Height - 16, 16, 16);
                    GripRenderer.Render(e.Graphics, GripBounds.Location);
                }

                protected override void OnSizeChanged(EventArgs e)
                {
                    base.OnSizeChanged(e);

                    // When drop-down window is being resized by the user (i.e. not locked),
                    // update size of hosted control.
                    if (!m_lockedThisSize)
                        RecalculateHostedControlLayout();
                }

                protected void hostedControl_SizeChanged(object sender, EventArgs e)
                {
                    // Only update size of this container when it is not locked.
                    if (!m_lockedHostedControlSize)
                        ResizeFromContent(-1);
                }

                public void Show(int x, int y, int width, int height)
                {
                    // If no hosted control is associated, this procedure is pointless!
                    Control hostedControl = GetHostedControl();
                    if (hostedControl == null)
                        return;

                    // Initially hosted control should be displayed within a drop down of 1x1, however
                    // its size should exceed the dimensions of the drop-down.
                    {
                        m_lockedHostedControlSize = true;
                        m_lockedThisSize = true;

                        // Display actual popup and occupy just 1x1 pixel to avoid automatic reposition.
                        Size = new Size(1, 1);
                        base.Show(x, y);

                        m_lockedHostedControlSize = false;
                        m_lockedThisSize = false;
                    }

                    // Resize drop-down to fit its contents.
                    ResizeFromContent(width);

                    // If client area was enlarged using the minimum width paramater, then the hosted
                    // control must also be enlarged.
                    if (m_refreshSize)
                        RecalculateHostedControlLayout();

                    // Assign event handler to control.
                    hostedControl.SizeChanged += hostedControl_SizeChanged;
                }

                protected void ResizeFromContent(int width)
                {
                    if (m_lockedThisSize)
                        return;

                    // Prevent resizing hosted control to 1x1 pixel!
                    m_lockedHostedControlSize = true;

                    // Resize from content again because certain information was not available before.
                    Rectangle bounds = Bounds;
                    bounds.Size = SizeFromContent(width);

                    if (!CompareResizeMode(PopupResizeMode.None))
                    {
                        if (width > 0 && bounds.Width - 2 > width)
                            if (!CompareResizeMode(PopupResizeMode.Right))
                                bounds.X -= bounds.Width - 2 - width;
                    }

                    Bounds = bounds;

                    m_lockedHostedControlSize = false;
                }

                protected void RecalculateHostedControlLayout()
                {
                    if (m_lockedHostedControlSize)
                        return;

                    m_lockedThisSize = true;

                    // Update size of hosted control.
                    Control hostedControl = GetHostedControl();
                    if (hostedControl != null)
                    {
                        // Fetch control bounds and adjust as necessary.
                        Rectangle bounds = hostedControl.Bounds;
                        if (CompareResizeMode(PopupResizeMode.TopLeft) || CompareResizeMode(PopupResizeMode.TopRight))
                            bounds.Location = new Point(1, 16);
                        else
                            bounds.Location = new Point(1, 1);

                        bounds.Width = ClientRectangle.Width - 2;
                        bounds.Height = ClientRectangle.Height - 2;
                        if (IsGripShown)
                            bounds.Height -= 16;

                        if (bounds.Size != hostedControl.Size)
                            hostedControl.Size = bounds.Size;
                        if (bounds.Location != hostedControl.Location)
                            hostedControl.Location = bounds.Location;
                    }

                    m_lockedThisSize = false;
                }

                public Control GetHostedControl()
                {
                    if (Items.Count > 0)
                    {
                        ToolStripControlHost host = Items[0] as ToolStripControlHost;
                        if (host != null)
                            return host.Control;
                    }
                    return null;
                }

                public bool CompareResizeMode(PopupResizeMode resizeMode)
                {
                    return (ResizeMode & resizeMode) == resizeMode;
                }

                protected Size SizeFromContent(int width)
                {
                    Size contentSize = Size.Empty;

                    m_refreshSize = false;

                    // Fetch hosted control.
                    Control hostedControl = GetHostedControl();
                    if (hostedControl != null)
                    {
                        if (CompareResizeMode(PopupResizeMode.TopLeft) || CompareResizeMode(PopupResizeMode.TopRight))
                            hostedControl.Location = new Point(1, 16);
                        else
                            hostedControl.Location = new Point(1, 1);
                        contentSize = SizeFromClientSize(hostedControl.Size);

                        // Use minimum width (if specified).
                        if (width > 0 && contentSize.Width < width)
                        {
                            contentSize.Width = width;
                            m_refreshSize = true;
                        }
                    }

                    // If a grip box is shown then add it into the drop down height.
                    if (IsGripShown)
                        contentSize.Height += 16;

                    // Add some additional space to allow for borders.
                    contentSize.Width += 2;
                    contentSize.Height += 2;

                    return contentSize;
                }

                protected const int HTTRANSPARENT = -1;
                protected const int HTLEFT = 10;
                protected const int HTRIGHT = 11;
                protected const int HTTOP = 12;
                protected const int HTTOPLEFT = 13;
                protected const int HTTOPRIGHT = 14;
                protected const int HTBOTTOM = 15;
                protected const int HTBOTTOMLEFT = 16;
                protected const int HTBOTTOMRIGHT = 17;

                [StructLayout(LayoutKind.Sequential)]
                internal struct MINMAXINFO
                {
                    public Point reserved;
                    public Size maxSize;
                    public Point maxPosition;
                    public Size minTrackSize;
                    public Size maxTrackSize;
                }

                [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
                protected override void WndProc(ref Message m)
                {
                    if (!ProcessGrip(ref m, false))
                        base.WndProc(ref m);
                }

                [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
                private bool ProcessGrip(ref Message m, bool contentControl)
                {
                    if (ResizeMode != PopupResizeMode.None)
                    {
                        switch (m.Msg)
                        {
                            case Sce.Atf.User32.WM_NCHITTEST:
                                return OnNcHitTest(ref m, contentControl);

                            case Sce.Atf.User32.WM_GETMINMAXINFO:
                                return OnGetMinMaxInfo(ref m);
                        }
                    }
                    return false;
                }

                [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
                private bool OnGetMinMaxInfo(ref Message m)
                {
                    Control hostedControl = GetHostedControl();
                    if (hostedControl != null)
                    {
                        MINMAXINFO minmax = (MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(MINMAXINFO));

                        // Maximum size.
                        if (hostedControl.MaximumSize.Width != 0)
                            minmax.maxTrackSize.Width = hostedControl.MaximumSize.Width;
                        if (hostedControl.MaximumSize.Height != 0)
                            minmax.maxTrackSize.Height = hostedControl.MaximumSize.Height;

                        // Minimum size.
                        minmax.minTrackSize = new Size(32, 32);
                        if (hostedControl.MinimumSize.Width > minmax.minTrackSize.Width)
                            minmax.minTrackSize.Width = hostedControl.MinimumSize.Width;
                        if (hostedControl.MinimumSize.Height > minmax.minTrackSize.Height)
                            minmax.minTrackSize.Height = hostedControl.MinimumSize.Height;

                        Marshal.StructureToPtr(minmax, m.LParam, false);
                    }
                    return true;
                }

                private bool OnNcHitTest(ref Message m, bool contentControl)
                {
                    Point location = PointToClient(new Point(Sce.Atf.User32.LOWORD(m.LParam), Sce.Atf.User32.HIWORD(m.LParam)));
                    IntPtr transparent = new IntPtr(HTTRANSPARENT);

                    // Check for simple gripper dragging.
                    if (GripBounds.Contains(location))
                    {
                        m.Result = contentControl ? transparent : (IntPtr)HTBOTTOMRIGHT;
                        return true;
                    }

                    return false;
                }

                /// <summary>
                /// Gets or sets type of resize mode. Grips are automatically drawn at bottom-left and bottom-right corners.</summary>
                public PopupResizeMode ResizeMode
                {
                    get { return m_resizeMode; }
                    set
                    {
                        if (value != m_resizeMode)
                        {
                            m_resizeMode = value;
                            Invalidate();
                        }
                    }
                }

                /// <summary>
                /// Gets or sets bounds of active grip box position</summary>
                protected Rectangle GripBounds
                {
                    get { return this.m_gripBounds; }
                    set { this.m_gripBounds = value; }
                }

                /// <summary>
                /// Gets whether a grip box is shown.</summary>
                protected bool IsGripShown
                {
                    get
                    {
                        return (ResizeMode == PopupResizeMode.TopLeft || ResizeMode == PopupResizeMode.TopRight ||
                                ResizeMode == PopupResizeMode.BottomLeft || ResizeMode == PopupResizeMode.BottomRight);
                    }
                }



                private PopupResizeMode m_resizeMode = PopupResizeMode.None;
                private Rectangle m_gripBounds = Rectangle.Empty;

                private bool m_lockedHostedControlSize = false;
                private bool m_lockedThisSize = false;
                private bool m_refreshSize = false;

            }

            private class GripRenderer
            {

                private GripRenderer()
                {
                }

                private static void InitializeGripBitmap(Graphics g, Size size, bool forceRefresh)
                {
                    if (m_sGripBitmap == null || forceRefresh || size != m_sGripBitmap.Size)
                    {
                        // Draw size grip into a bitmap image.
                        m_sGripBitmap = new Bitmap(size.Width, size.Height, g);
                        using (Graphics gripG = Graphics.FromImage(m_sGripBitmap))
                            ControlPaint.DrawSizeGrip(gripG, SystemColors.ButtonFace, 0, 0, size.Width, size.Height);
                    }
                }

                public static void Render(Graphics g, Point location, Size size)
                {
                    InitializeGripBitmap(g, size, false);


                    // Reverse size grip for left-aligned.
                    if (size.Width < 0)
                        location.X -= size.Width;
                    if (size.Height < 0)
                        location.Y -= size.Height;

                    g.DrawImage(GripBitmap, location.X, location.Y, size.Width, size.Height);
                }

                public static void Render(Graphics g, Point location)
                {
                    Render(g, location, new Size(16, 16));
                }

                private static Bitmap GripBitmap
                {
                    get { return m_sGripBitmap; }
                }

                private static Bitmap m_sGripBitmap;
            }

            [Flags]
            private enum PopupResizeMode
            {
                None = 0,

                // Individual styles.
                Left = 1,
                Top = 2,
                Right = 4,
                Bottom = 8,

                // Combined styles.
                All = (Top | Left | Bottom | Right),
                TopLeft = (Top | Left),
                TopRight = (Top | Right),
                BottomLeft = (Bottom | Left),
                BottomRight = (Bottom | Right),
            }
            #endregion
        }

        private string[] m_names;
        private string[] m_displayNames;
        private int[] m_values;
        private int m_dropDownHeight;
    }
}
