//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sce.Atf.Controls.PropertyEditing
{

    /// <summary>
    /// Property editor for choosing from a list of predefined values, like from an enumerated
    /// type. The values can be long and the drop-down list will resize itself to accommodate the
    /// longest display names. If TestEditEnabled is true, the user can enter an arbitrary string.</summary>
    /// <remarks>
    /// It is recommended that you use Sce.Atf.Controls.PropertyEditing.IntEnumTypeConverter
    /// as the TypeConverter for this editor's property, to support enum stored as int.
    /// A LongEnumEditor is created for each PropertyDescriptor, for each
    /// selection change. PropertyDescriptors are shared between property editors,
    /// such as GridView and PropertyGridView.
    /// TestEditEnabled will be ignored if TypeConverter is used</remarks>
    public class LongEnumEditor : IPropertyEditor, IAnnotatedParams
    {
        /// <summary>
        /// Default constructor. Use DefineEnum() to populate the list of items to choose from.</summary>
        public LongEnumEditor()
        {
        }

        /// <summary>
        /// Construct LongEnumEditor using an enum type and option associated images</summary>
        /// <param name="enumType">The enum type</param>
        /// <param name="images">optional icons</param>
        public LongEnumEditor(Type enumType, Image[] images = null)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("enumType must Enum");
            m_names = Enum.GetNames(enumType);
            m_images = images;
        }

        /// <summary>
        /// Gets or sets whether the user can set the value of the property to an
        /// arbitrary text string, rather than only choosing from the list.
        /// The default is false.
        /// This property will be ignred if TypeCoverter is used</summary>
        public bool TextEditEnabled { get; set; }

        private int m_maxDropDownItems = 6;
        /// <summary>
        /// Gets or sets the maximum number of items to be shown in the drop-down portion
        /// of the EnumEditor. Default is 6</summary>
        public int MaxDropDownItems
        {
            get { return m_maxDropDownItems; }
            set
            {
                m_maxDropDownItems = value;
                if (m_maxDropDownItems < 1)
                    m_maxDropDownItems = 1;
            }
        }

        /// <summary>
        /// Defines the enum names and optional associated images</summary>
        /// <param name="names">Enum names array</param>
        /// <param name="images">optional icons</param>
        /// <remarks>Enum names with the format "EnumName==DisplayName" are parsed so that
        /// DisplayName is displayed to the user.</remarks>
        public void DefineEnum(string[] names, Image[] images = null)
        {
            if (names == null || names.Length == 0)
                throw new ArgumentException();

            int[] values;
            EnumUtil.ParseEnumDefinitions(names, out m_names, out m_displayNames, out values);
            // If m_names equal m_displayNames then set m_displayNames to null, for performance reasons.
            if (m_names.SequenceEqual(m_displayNames))
                m_displayNames = null;
            m_images = images;
        }

        #region IPropertyEditor Members

        /// <summary>
        /// Gets a control to edit the given property</summary>
        /// <param name="context">Context for property editing control</param>
        /// <returns>Control to edit the given context</returns>
        public virtual Control GetEditingControl(PropertyEditorControlContext context)
        {
            var ctrl = new EnumEditorControl(context, m_names, m_displayNames, m_images);
            ctrl.DropDownStyle =
                (context == null || context.Descriptor.Converter == null) && TextEditEnabled
                ? ComboBoxStyle.DropDown
                : ComboBoxStyle.DropDownList;
            ctrl.MaxDropDownItems = MaxDropDownItems;
            Atf.Applications.SkinService.ApplyActiveSkin(ctrl);
            return ctrl;
        }

        #endregion

        #region IAnnotatedParams Members

        void IAnnotatedParams.Initialize(string[] parameters)
        {
            DefineEnum(parameters);
        }

        #endregion

        private string[] m_names;
        private string[] m_displayNames;
        private Image[] m_images;

        private class EnumEditorControl : ComboBox, ICacheablePropertyControl
        {
            public EnumEditorControl(PropertyEditorControlContext context,
                string[] names, string[] displayNames, Image[] images)
            {
                m_images = images;
                m_context = context;
                m_displayNames = displayNames;
                m_names = names;
                if (m_images != null && m_images.Length > 0)
                    DrawMode = DrawMode.OwnerDrawFixed;
                IntegralHeight = false;

                var enums = displayNames ?? names;
                BeginUpdate();
                Items.AddRange(enums);
                EndUpdate();

                FlatStyle = FlatStyle.Standard;
                MaxDropDownItems = 6;
                DrawMode = DrawMode.OwnerDrawFixed;
                AutoCompleteMode = AutoCompleteMode.Suggest;
                AutoCompleteSource = AutoCompleteSource.ListItems;
                SelectedIndexChanged += ComboboxSelectedIndexChanged;

                SetDropDownWidth();
                FontChanged += (sender, e) => SetDropDownWidth();
            }

            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                if (e.Index == -1) return;
                e.DrawBackground();
                Rectangle imgRect = Rectangle.Empty;
                if (m_images != null && e.Index < m_images.Length)
                {
                    Image img = m_images[e.Index];
                    imgRect = e.Bounds;
                    imgRect.Width = e.Bounds.Height;
                    if (img != null)
                    {
                        int h = Math.Min(e.Bounds.Height, img.Height);
                        imgRect.Width = h;
                        imgRect.Height = h;
                        e.Graphics.DrawImage(img, imgRect);
                    }
                }

                using (SolidBrush brush = new SolidBrush(ForeColor))
                {
                    Rectangle txtRect = e.Bounds;
                    txtRect.Width = e.Bounds.Width - imgRect.Width;
                    txtRect.X = e.Bounds.X + imgRect.Width;
                    e.Graphics.DrawString(Items[e.Index].ToString(), Font, brush, txtRect);
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

            public override void Refresh()
            {
                RefreshValue();
                base.Refresh();
            }

            protected override void OnLostFocus(EventArgs e)
            {
                SetProperty();
                base.OnLostFocus(e);
            }

            private void ComboboxSelectedIndexChanged(object sender, EventArgs e)
            {
                if (m_refreshing) return;
                SetProperty();
            }

            private void SetProperty()
            {
                if (!m_refreshing)
                {
                    if (Text != m_lastEdit)
                    {
                        m_lastEdit = Text;
                        string val = DisplayNameToId(Text);
                        //Note: m_context.SetValue(val) use converter,
                        // so there is no need to use Converter here.                        
                        // however, m_context.GetValue(); doesn't use converter that is why
                        // we need to use converter in the method RefreshValue().
                        // This is inconsistent behavior.
                        m_context.SetValue(val);
                    }
                }
            }

            private bool m_refreshing;
            private void RefreshValue()
            {
                if (m_refreshing) return;
                try
                {
                    m_refreshing = true;
                    object value = m_context.GetValue();

                    if (value == null)
                        Enabled = false;
                    else
                    {
                        //Note: Need to use Converter if exist because
                        //     m_context.GetValue(); doesn't use converter.
                        string txtVal  = null;
                        var converter = m_context.Descriptor.Converter;
                        if (converter != null && converter.CanConvertTo(typeof(string)))
                            txtVal = IdToDisplayName((string)converter.ConvertTo(value, typeof(string)));
                        else
                            txtVal = IdToDisplayName((string)value);
                        
                        if (txtVal != m_lastEdit)
                        {
                            Text = txtVal;
                            m_lastEdit = txtVal;
                        }
                        Enabled = !m_context.IsReadOnly;
                    }

                }
                finally
                {
                    m_refreshing = false;
                }
            }

            private string IdToDisplayName(string id)
            {
                string name = id;
                if (m_displayNames != null)
                {
                    for (int i = 0; i < m_names.Length; i++)
                    {
                        if (id == m_names[i])
                        {
                            name = m_displayNames[i];
                            break;
                        }
                    }
                }
                return name;
            }

            private string DisplayNameToId(string displayName)
            {
                string id = displayName;
                if (m_displayNames != null)
                {
                    for (int i = 0; i < m_displayNames.Length; i++)
                    {
                        if (displayName == m_displayNames[i])
                        {
                            id = m_names[i];
                        }
                    }
                }
                return id;
            }

            private void SetDropDownWidth()
            {
                int imgWith = 0;
                if (m_images != null && m_images.Length > 0)
                {
                    foreach (Image img in m_images)
                    {
                        if (img != null)
                        {
                            if (img.Width > imgWith)
                                imgWith = img.Width;
                        }
                    }
                }

                var enums = m_displayNames ?? m_names;
                int txtWidth = 0;
                foreach (string txval in enums)
                {
                    Size sz = TextRenderer.MeasureText(txval, Font);
                    if (sz.Width > txtWidth)
                        txtWidth = sz.Width;
                }

                DropDownWidth = imgWith + txtWidth;
            }

            private PropertyEditorControlContext m_context;

            #region ICacheablePropertyControl Members

            bool ICacheablePropertyControl.Cacheable
            {
                get { return false; }
            }

            #endregion
            
            private string m_lastEdit;
            private string[] m_names;
            private string[] m_displayNames;
            private Image[] m_images;
        }
    }
}
