//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Wrapper control for a two-column PropertyGridView combined with a toolbar.
    /// Use this as a replacement for the .NET System.Windows.Forms.PropertyGrid.</summary>
    public class PropertyGrid : ScrollableControl
    {
        /// <summary>
        /// Default constructor that allows the user to select property organization schemes
        /// and that displays property descriptions in an area at the bottom of the Control</summary>
        public PropertyGrid()
            : this(PropertyGridMode.PropertySorting | PropertyGridMode.DisplayDescriptions, new PropertyGridView())
        {
        }

        /// <summary>
        /// Constructor using the default PropertyGridView and the given mode flags</summary>
        /// <param name="mode">Flags specifying the PropertyGrid's features and appearance</param>
        public PropertyGrid(PropertyGridMode mode)
            : this(mode, new PropertyGridView())
        {
        }

        /// <summary>
        /// Constructor using given PropertyGridView and mode flags</summary>
        /// <param name="mode">The flags specifying the PropertyGrid's features and appearance</param>
        /// <param name="propertyGridView">The customized PropertyGridView</param>
        public PropertyGrid(PropertyGridMode mode, PropertyGridView propertyGridView)
        {
            m_propertyGridView = propertyGridView;
            m_propertyGridView.BackColor = SystemColors.Window;
            m_propertyGridView.Dock = DockStyle.Fill;
            m_propertyGridView.EditingContextChanged += propertyGrid_EditingContextChanged;
            m_propertyGridView.MouseUp += propertyGrid_MouseUp;
            m_propertyGridView.DragOver += propertyGrid_DragOver;
            m_propertyGridView.DragDrop += propertyGrid_DragDrop;
            m_propertyGridView.MouseHover += propertyGrid_MouseHover;
            m_propertyGridView.MouseLeave += propertyGrid_MouseLeave;
            if (m_descriptionTextBox != null)
            {
                m_propertyGridView.DescriptionSetter = p =>
                {
                    if (p != null)
                        m_descriptionTextBox.SetDescription(p.DisplayName, p.Description);
                    else
                        m_descriptionTextBox.ClearDescription();
                };
            }

            m_toolStrip = new ToolStrip();
            m_toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            m_toolStrip.Dock = DockStyle.Top;

            if ((mode & PropertyGridMode.PropertySorting) != 0)
            {
                m_propertyOrganization = new ToolStripDropDownButton(null, s_categoryImage);
                m_propertyOrganization.ToolTipText = "Property Organization".Localize(
                    "Could be rephrased as 'How do you want these properties to be organized?'");
                m_propertyOrganization.ImageTransparentColor = Color.Magenta;
                m_propertyOrganization.DropDownItemClicked += organization_DropDownItemClicked;

                var item1 = new ToolStripMenuItem("Unsorted".Localize());
                item1.Tag = PropertySorting.None;

                var item2 = new ToolStripMenuItem("Alphabetical".Localize());
                item2.Tag = PropertySorting.Alphabetical;

                var item3 = new ToolStripMenuItem("Categorized".Localize());
                item3.Tag = PropertySorting.Categorized;

                var item4 = new ToolStripMenuItem("Categorized Alphabetical Properties".Localize());
                item4.Tag = PropertySorting.Categorized | PropertySorting.Alphabetical;

                var item5 = new ToolStripMenuItem("Alphabetical Categories".Localize());
                item5.Tag = PropertySorting.Categorized | PropertySorting.CategoryAlphabetical;

                var item6 = new ToolStripMenuItem("Alphabetical Categories And Properties".Localize());
                item6.Tag = PropertySorting.ByCategory;

                m_propertyOrganization.DropDownItems.Add(item1);
                m_propertyOrganization.DropDownItems.Add(item2);
                m_propertyOrganization.DropDownItems.Add(item3);
                m_propertyOrganization.DropDownItems.Add(item4);
                m_propertyOrganization.DropDownItems.Add(item5);
                m_propertyOrganization.DropDownItems.Add(item6);

                m_toolStrip.Items.Add(m_propertyOrganization);
                m_toolStrip.Items.Add(new ToolStripSeparator());
            }

            if ((mode & PropertyGridMode.DisableSearchControls) == 0)
            {
                var dropDownButton = new ToolStripDropDownButton();
                dropDownButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
                dropDownButton.Image = ResourceUtil.GetImage16(Resources.SearchImage);
                dropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
                dropDownButton.Name = "PropertyGridSearchButton";
                dropDownButton.Size = new System.Drawing.Size(29, 22);
                dropDownButton.Text = "Search".Localize("'Search' is a verb");

                m_patternTextBox = new ToolStripAutoFitTextBox();
                m_patternTextBox.Name = "patternTextBox";
                m_patternTextBox.MaximumWidth = 1080;
                m_patternTextBox.KeyUp += patternTextBox_KeyUp;
                m_patternTextBox.TextBox.PreviewKeyDown += patternTextBox_PreviewKeyDown;

                var clearSearchButton = new ToolStripButton();
                clearSearchButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
                clearSearchButton.Image = ResourceUtil.GetImage16(Resources.DeleteImage);
                dropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
                clearSearchButton.Name = "PropertyGridClearSearchButton";
                clearSearchButton.Size = new System.Drawing.Size(29, 22);
                clearSearchButton.Text = "Clear Search".Localize("'Clear' is a verb");
                clearSearchButton.Click += clearSearchButton_Click;

                m_toolStrip.Items.AddRange(
                    new ToolStripItem[] { 
                    dropDownButton, 
                    m_patternTextBox,
                    clearSearchButton
                    });
            }

            if ((mode & PropertyGridMode.HideResetAllButton) == 0)
            {
                // Reset all button.
                var resetAllButton = new ToolStripButton();
                resetAllButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
                resetAllButton.Image = ResourceUtil.GetImage16(Resources.ResetImage);
                resetAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
                resetAllButton.Name = "PropertyGridResetAllButton";
                resetAllButton.Size = new Size(29, 22);
                resetAllButton.ToolTipText = "Reset all properties".Localize();
                resetAllButton.Click += (sender, e) =>
                    {
                        ITransactionContext transaction = m_propertyGridView.EditingContext.As<ITransactionContext>();                        
                        transaction.DoTransaction(delegate
                        {
                            ResetAll();
                        },
                        "Reset All Properties".Localize("'Reset' is a verb and this is the name of a command"));
                    };
                m_toolStrip.Items.Add(resetAllButton);
            }

            if ((mode & PropertyGridMode.AllowEditingComposites) != 0)
            {
                m_navigateOut = new ToolStripButton(null, s_navigateOutImage, navigateOut_Click);
                m_navigateOut.Enabled = true;
                m_navigateOut.ToolTipText = "Navigate back to parent of selected object".Localize();

                m_toolStrip.Items.Add(m_navigateOut);

                m_propertyGridView.AllowEditingComposites = true;
            }

            SuspendLayout();

            if ((mode & PropertyGridMode.DisplayTooltips) != 0)
                m_propertyGridView.AllowTooltips = true;

            if ((mode & PropertyGridMode.DisplayDescriptions) == 0)
                Controls.Add(m_propertyGridView);
            else
            {
                m_splitContainer.Orientation = Orientation.Horizontal;
                m_splitContainer.BackColor = SystemColors.InactiveBorder;
                m_splitContainer.FixedPanel = FixedPanel.Panel2;
                m_splitContainer.SplitterWidth = 8;
                m_splitContainer.Dock = DockStyle.Fill;

                m_splitContainer.Panel1.Controls.Add(m_propertyGridView);

                m_descriptionTextBox = new DescriptionControl(this);
                m_descriptionTextBox.BackColor = SystemColors.Window;
                m_descriptionTextBox.Dock = DockStyle.Fill;

                m_splitContainer.Panel2.Controls.Add(m_descriptionTextBox);
                Controls.Add(m_splitContainer);

                m_propertyGridView.SelectedPropertyChanged += propertyGrid_SelectedRowChanged;
                m_descriptionTextBox.ClearDescription();
            }

            if (m_toolStrip.Items.Count > 0)
            {
                UpdateToolstripItems();
                Controls.Add(m_toolStrip);
            }
            else
            {
                m_toolStrip.Dispose();
                m_toolStrip = null;
            }

            Name = "PropertyGrid";
            Font = m_propertyGridView.Font;
            FontChanged += (sender, e) => m_propertyGridView.Font = Font;                
            ResumeLayout(false);
            PerformLayout();            
        }

        /// <summary>
        /// Binds the control to the selected object</summary>
        /// <param name="selectedObject">Object or array of objects with properties</param>
        public void Bind(object selectedObject)
        {
            object[] selectedObjects = selectedObject as object[];
            if (selectedObjects == null)
                selectedObjects = new[] { selectedObject };
            Bind(new PropertyEditingContext(selectedObjects));
        }

        /// <summary>
        /// Binds the control to a property editing context</summary>
        /// <param name="context">Context in which properties are edited</param>
        public void Bind(IPropertyEditingContext context)
        {
            m_propertyGridView.EditingContext = context;
            SkinService.ApplyActiveSkin(m_propertyGridView);
        }

        /// <summary>
        /// Property grid view's DragDrop event</summary>
        public event DragEventHandler PropertyGridDragDrop
        {
            add { m_propertyGridView.DragDrop += value; }
            remove { m_propertyGridView.DragDrop -= value; }
        }

        /// <summary>
        /// Property grid view's DragOver event</summary>
        public event DragEventHandler PropertyGridDragOver
        {
            add { m_propertyGridView.DragOver += value; }
            remove { m_propertyGridView.DragOver -= value; }
        }

        /// <summary>
        /// Property grid view's MouseDown event</summary>
        public event MouseEventHandler PropertyGridMouseDown
        {
            add { m_propertyGridView.MouseDown += value; }
            remove { m_propertyGridView.MouseDown -= value; }
        }

        /// <summary>
        /// Property grid view's MouseHover event</summary>
        public event EventHandler PropertyGridMouseHover
        {
            add { m_propertyGridView.MouseHover += value; }
            remove { m_propertyGridView.MouseHover -= value; }
        }

        /// <summary>
        /// Property grid view's MouseLeave event</summary>
        public event EventHandler PropertyGridMouseLeave
        {
            add { m_propertyGridView.MouseLeave += value; }
            remove { m_propertyGridView.MouseLeave -= value; }
        }

        /// <summary>
        /// Property grid view's MouseUp event</summary>
        public event MouseEventHandler PropertyGridMouseUp
        {
            add { m_propertyGridView.MouseUp += value; }
            remove { m_propertyGridView.MouseUp -= value; }
        }

        /// <summary>
        /// Gets the two column grid PropertyView</summary>
        public PropertyGridView PropertyGridView
        {
            get { return m_propertyGridView; }
        }

        /// <summary>
        /// Gets and sets the control's property display sort order</summary>
        public PropertySorting PropertySorting
        {
            get { return m_propertyGridView.PropertySorting; }
            set
            {
                m_propertyGridView.PropertySorting = value;
                UpdateToolstripItems();
            }
        }

        /// <summary>
        /// Gets and sets the control's persistent settings</summary>
        public string Settings
        {
            get { return m_propertyGridView.Settings; }
            set
            {
                m_propertyGridView.Settings = value;
                UpdateToolstripItems();
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Windows.Forms.ToolStrip"/> corresponding to this PropertyGrid</summary>
        public ToolStrip ToolStrip
        {
            get { return m_toolStrip; }
        }

        /// <summary>
        /// Gets whether the current property being edited can be reset</summary>
        public bool CanResetCurrent
        {
            get { return m_propertyGridView.CanResetCurrent; }
        }

        /// <summary>
        /// Resets the current property being edited to its default value</summary>
        public void ResetCurrent()
        {
            m_propertyGridView.ResetCurrent();
        }

        /// <summary>
        /// Resets all properties to their default values</summary>
        public void ResetAll()
        {
            m_propertyGridView.ResetAll();
        }

        /// <summary>
        /// Refreshes all property representations in the control, updating them to reflect the latest
        /// changes in the underlying data. Is a subset of Refresh(), because the scrollable control
        /// is not updated.</summary>
        public void RefreshProperties()
        {
            m_propertyGridView.Refresh();
        }

        /// <summary>
        /// Gets the PropertyDescriptor for the property under the client point, or null if none</summary>
        /// <param name="clientPoint">Client point, as from a mouse event. For points that
        /// are in screen coordinates, as from drag events, convert to client coordinates
        /// by using PointToClient().</param>
        /// <returns>PropertyDescriptor for the property under the client point, or null if none</returns>
        public PropertyDescriptor GetDescriptorAt(Point clientPoint)
        {
            return m_propertyGridView.GetDescriptorAt(clientPoint);
        }

        /// <summary>
        /// Gets the PropertyDescriptor for the property under the client point, or null if none</summary>
        /// <param name="clientPoint">Client point, as from a mouse event. For points that
        /// are in screen coordinates, as from drag events, convert to client coordinates
        /// by using PointToClient().</param>
        /// <param name="editingContext">The IPropertyEditingContext to be used with the resulting PropertyDescriptor.</param>
        /// <returns>PropertyDescriptor for the property under the client point, or null if none</returns>
        public PropertyDescriptor GetDescriptorAt(Point clientPoint, out IPropertyEditingContext editingContext)
        {
            return m_propertyGridView.GetDescriptorAt(clientPoint, out editingContext);
        }

        /// <summary>
        /// Gets the PropertyDescriptor for the property under the client point, or null if none</summary>
        /// <param name="clientPoint">Client point, as from a mouse event. For points that
        /// are in screen coordinates, as from drag events, convert to client coordinates
        /// by using PointToClient().</param>
        /// <param name="bottom">Will be set to the y position, in client coordinates, of the bottom
        /// of the row for this property. Is only meaningful if a PropertyDescriptor is found.</param>
        /// <returns>PropertyDescriptor for the property under the client point, or null if none</returns>
        public PropertyDescriptor GetDescriptorAt(Point clientPoint, out int bottom)
        {
            return m_propertyGridView.GetDescriptorAt(clientPoint, out bottom);
        }

        /// <summary>
        /// Gets or sets the current selected property's descriptor</summary>
        public PropertyDescriptor SelectedPropertyDescriptor
        {
            get { return m_propertyGridView.SelectedPropertyDescriptor; }
            set
            {
                m_propertyGridView.SelectProperty(value);
                if (m_descriptionTextBox != null)
                {
                    if (value != null)
                        m_descriptionTextBox.SetDescription(value.DisplayName, value.Description);
                    else
                        m_descriptionTextBox.ClearDescription();
                }
            }

        }

        /// <summary>
        /// Sets the name and description in the description area at the bottom of the PropertyGrid</summary>
        /// <param name="name">Name, the bold heading</param>
        /// <param name="description">Longer, non-bold description below the heading</param>
        /// <remarks>This method can be used to set a per-type description when the selection changes.
        /// When the user selects a property row, the description based on the PropertyDescriptor 
        /// is displayed and replaces any description that may have been set previously.</remarks>
        public void SetDescription(string name, string description)
        {
            if (m_descriptionTextBox != null)
                m_descriptionTextBox.SetDescription(name, description);
        }

        #region Overrides

        /// <summary>
        /// Sets whether the control allows drag and drop</summary>
        public override bool AllowDrop
        {
            set
            {
                base.AllowDrop = value;
                m_propertyGridView.AllowDrop = value;
            }
        }

        /// <summary>
        /// Forces the control to invalidate its client area and immediately redraw itself and any
        /// child controls. Refreshes all the property representations in the control, updating
        /// them to reflect the latest changes in the underlying data.</summary>
        public override void Refresh()
        {
            m_propertyGridView.Refresh();
            base.Refresh();
        }

        #endregion

        /// <summary>
        /// Handles the URL in the selected property's description that was clicked on by
        /// the user.</summary>
        /// <param name="url">URL (e.g., https://, file://, etc.)</param>
        /// <remarks>The default behavior is to let Windows open the URL.</remarks>
        protected virtual void LinkClicked(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        #region Event Handlers

        private void propertyGrid_EditingContextChanged(object sender, EventArgs e)
        {
            if (m_navigateOut != null)
            {
                m_navigateOut.Enabled = m_propertyGridView.CanNavigateBack;
            }
        }

        private void propertyGrid_MouseUp(object sender, MouseEventArgs e)
        {
            // pass mouse right-clicks up to clients for context menu implementation
            if (e.Button == MouseButtons.Right)
            {
                OnMouseUp(e);
            }
        }

        private void propertyGrid_DragDrop(object sender, DragEventArgs e)
        {
            // raise event on this control
            OnDragDrop(e);
        }

        private void propertyGrid_DragOver(object sender, DragEventArgs e)
        {
            // raise event on this control
            OnDragOver(e);
        }

        private void propertyGrid_MouseHover(object sender, EventArgs e)
        {
            // raise event on this control
            OnMouseHover(e);
        }

        private void propertyGrid_MouseLeave(object sender, EventArgs e)
        {
            // raise event on this control
            OnMouseLeave(e);
        }

        private void propertyGrid_SelectedRowChanged(object sender, EventArgs e)
        {
            if (m_descriptionTextBox != null)
            {
                PropertyDescriptor descriptor = m_propertyGridView.SelectedPropertyDescriptor;
                if (descriptor != null)
                {
                    m_descriptionTextBox.SetDescription(descriptor.DisplayName, descriptor.Description);
                }
                else
                {
                    m_descriptionTextBox.ClearDescription();
                }
            }
        }

        private void organization_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            m_propertyGridView.PropertySorting = (PropertySorting)e.ClickedItem.Tag;
            UpdateToolstripItems();
        }

        private void navigateOut_Click(object sender, EventArgs e)
        {
            m_propertyGridView.NavigateBack();
        }

        private void clearSearchButton_Click(object sender, System.EventArgs e)
        {
            m_patternTextBox.Text = string.Empty;
            m_propertyGridView.FilterPattern = m_patternTextBox.Text;
        }

        private void patternTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            m_propertyGridView.FilterPattern = m_patternTextBox.Text;
        }

        private void patternTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
                clearSearchButton_Click(sender, e);
        }
        #endregion

        private void UpdateToolstripItems()
        {
            if (m_propertyOrganization != null)
            {
                foreach (ToolStripMenuItem item in m_propertyOrganization.DropDownItems)
                    item.Checked = ((PropertySorting)item.Tag == m_propertyGridView.PropertySorting);
            }
        }

        private class DescriptionControl : RichTextBox
        {
            public DescriptionControl(PropertyGrid propertyGrid)
            {
                m_propertyGrid = propertyGrid;
                m_textBrush = new SolidBrush(SystemColors.WindowText);
                CreateBoldFont();
                ReadOnly = true;

                // The method of inserting hyperlinks below will not work if DetectUrls is true
                //  because editing RichTextBox's text adjacent to the hyperlink will break the
                //  hyperlink.
                DetectUrls = false;
            }

            protected override void OnLinkClicked(LinkClickedEventArgs e)
            {
                base.OnLinkClicked(e);

                //The hyperlink's user-readable text is (annoyingly) included with the URL like this:
                //  "Atlassian#http://atlassian.com"
                //Since we only support hyperlinks in the wiki format (DetectUrls is false), the '#'
                //  will always be there, between the user-readable text and the URL. We just want the URL.
                string url = e.LinkText;
                int iSeparator = url.IndexOf('#');
                if (iSeparator >= 0)
                {
                    //The empty string is returned if iSeparator + 1 == url.Length.
                    url = url.Substring(iSeparator + 1);
                }

                // Unescape backslashes.
                url = url.Replace(@"\\", @"\");

                m_propertyGrid.LinkClicked(url);
            }

            /// <summary>
            /// Displays the given property name and description in this Control. The property
            /// name will be displayed in bold and the description will start on the next line.
            /// Hyperlinks in the description are supported using the following format:
            /// "[Atlassian|http://atlassian.com]".</summary>
            /// <param name="name">Property name</param>
            /// <param name="description">Property description</param>
            public void SetDescription(string name, string description)
            {
                m_name = name;
                m_description = description;

                List<MarkupToken> tokens = ParseMarkup(m_description).ToList();
                var visibleDescriptionBuilder = new StringBuilder();
                foreach (var token in tokens)
                    visibleDescriptionBuilder.Append(token.Text);
                string visibleDescription = visibleDescriptionBuilder.ToString();

                // For the special case of having no name or description, don't display the text box.
                if (string.IsNullOrEmpty(m_name) && string.IsNullOrEmpty(visibleDescription))
                {
                    m_propertyGrid.m_splitContainer.Panel2Collapsed = true;
                }
                else
                {
                    m_propertyGrid.m_splitContainer.Panel2Collapsed = false;
                    Size sz = new Size(ClientSize.Width, int.MaxValue);
                    sz = TextRenderer.MeasureText(visibleDescription, Font, sz, m_TextFormatFlags);
                    if (!string.IsNullOrEmpty(m_name))
                        sz.Height += m_boldFont.Height + 2 + 12;

                    int splitter_distance = (int)Math.Max(m_propertyGrid.m_splitContainer.Height - sz.Height, 0.5f * m_propertyGrid.m_splitContainer.Height);

                    // SplitterDistance must be between Panel1MinSize and Height - Panel2MinSize.
                    splitter_distance = Math.Max(splitter_distance, m_propertyGrid.m_splitContainer.Panel1MinSize);
                    splitter_distance = Math.Min(splitter_distance, m_propertyGrid.m_splitContainer.Height - m_propertyGrid.m_splitContainer.Panel2MinSize);

                    // m_propertyGrid.m_splitContainer.Height can be zero under some unusual circumstances involving
                    //  the auto-hide feature. So, let's make sure splitter_distance is non-negative.
                    if (splitter_distance >= 0)
                        m_propertyGrid.m_splitContainer.SplitterDistance = splitter_distance;
                }

                m_propertyGrid.m_splitContainer.Invalidate();

                // Update the text
                Text = string.Empty;
                if (!string.IsNullOrEmpty(m_name))
                {
                    SelectionFont = m_boldFont;
                    AppendText(m_name + Environment.NewLine);
                }
                if (!string.IsNullOrEmpty(visibleDescription))
                {
                    SelectionFont = Font;

                    foreach(var token in tokens)
                    {
                        if (!string.IsNullOrEmpty(token.Url))
                            InsertLink(token.Text, token.Url);
                        else
                            AppendText(token.Text);
                    }
                }
            }

            // Parses the text for one style of Atlassian Confluence hyperlink mark-ups.
            // For example:
            //      "Some text [Atlassian|http://atlassian.com] blah blah."
            // ==> to 3 MarkupTokens
            //      "Some text ", {"Atlassian", http://atlassian.com}, " blah blah."
            private static IEnumerable<MarkupToken> ParseMarkup(string wikiText)
            {
                if (string.IsNullOrEmpty(wikiText))
                    yield break;

                IList<string> tokens = wikiText.SplitAndKeepDelimiters('[', '|', ']');
                for (int i = 0; i < tokens.Count; i++)
                {
                    string text, url;
                    
                    // Look for a wiki-formed hyperlink.
                    string a = tokens[i];
                    text = a;
                    url = null;

                    if (a == "[")
                    {
                        string b = i + 1 < tokens.Count ? tokens[i + 1] : string.Empty;
                        string c = i + 2 < tokens.Count ? tokens[i + 2] : string.Empty;
                        string d = i + 3 < tokens.Count ? tokens[i + 3] : string.Empty;
                        string e = i + 4 < tokens.Count ? tokens[i + 4] : string.Empty;
                        if (c == "|" && e == "]")
                        {
                            text = b;
                            url = d;
                            i += 4;
                        }
                    }

                    yield return new MarkupToken { Text = text, Url = url };
                }
            }

            private class MarkupToken
            {
                public string Text; //the display text
                public string Url;  //the URL behind the text, or null
            }

            public void ClearDescription()
            {
                SetDescription(null, null);
            }

            /// <summary>
            /// Gets or sets the text brush used to color the text. Currently, only a
            /// SolidBrush has any effect.</summary>
            /// <value>The text brush</value>
            /// <remarks>This property is for compatibility with skin (*.skn) files
            /// and is accessed through reflection. The given brush will be owned by
            /// this DescriptionControl and will be disposed of when it's no longer needed.</remarks>
            public Brush TextBrush
            {
                get { return m_textBrush; }
                set
                {
                    DisposeTextBrush();
                    m_textBrush = value;

                    var solidBrush = m_textBrush as SolidBrush;
                    if (solidBrush != null)
                        ForeColor = solidBrush.Color;
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    DisposeBoldFont();
                    DisposeTextBrush();
                }

                base.Dispose(disposing);
            }

            protected override void OnFontChanged(EventArgs e)
            {
                base.OnFontChanged(e); //updates FontHeight property
                DisposeBoldFont();
                CreateBoldFont();
                SetDescription(m_name, m_description);
                
                // It's odd that this is necessary, but changing skins requires this call,
                //  otherwise the m_name text does not appear in bold.
                Invalidate();
            }

            private void CreateBoldFont()
            {
                if (m_boldFont == null)
                {
                    m_boldFont = new Font(Font, FontStyle.Bold);
                }
            }

            private void DisposeBoldFont()
            {
                if (m_boldFont != null)
                {
                    m_boldFont.Dispose();
                    m_boldFont = null;
                }
            }

            private void DisposeTextBrush()
            {
                if (m_textBrush != null)
                {
                    m_textBrush.Dispose();
                    m_textBrush = null;
                }
            }

            /// <summary>
            /// Converts a normal Unicode string to the RTF format, like for a RichTextBox</summary>
            /// <param name="unicodeString">The Unicode string</param>
            /// <returns>7-bit ASCII with higher Unicode characters escaped for RTF files</returns>
            private string ConvertToRtf(string unicodeString)
            {
                //http://en.wikipedia.org/wiki/Rich_Text_Format#Character_encoding
                const char substitutionChar = '?';
                IList<int> codePoints = StringUtil.GetUnicodeCodePoints(unicodeString);
                var sb = new StringBuilder();
                foreach (int codePoint in codePoints)
                {
                    if (codePoint < 128) //RTF is 7-bit ASCII
                    {
                        sb.Append((char) codePoint);
                    }
                    else if (codePoint < 65536) //escaped character can be signed 16 bit
                    {
                        sb.Append(@"\u");
                        sb.Append((short) codePoint);
                        sb.Append(substitutionChar);
                    }
                    else //too large! Maybe a code page could be set and then referenced?
                    {
                        sb.Append(substitutionChar);
                    }
                }
                return sb.ToString();
            }

            // The below region of code allows a RichTextBox to have hyperlinks inserted.
            // The original project is here:
            // http://www.codeproject.com/Articles/9196/Links-with-arbitrary-text-in-a-RichTextBox
            // It is licensed under Code Project License 1.02.
            //  http://www.codeproject.com/info/cpol10.aspx
            // Changes made:
            // 1. The accessibility was changed from public to private.
            // 2. Various cosmetic changes were made to fit our code standards.
            // 3. In SetSelectionStyle(), added the try-finally to avoid a possible memory leak.
            // 4. Added backslash escaping, so that URLs with backslashes (like with local file
            //  system paths) don't get corrupted.
            // 5. Added support for Unicode characters.
            #region RichTextBoxEx project

            #region Interop Defines
            [ StructLayout( LayoutKind.Sequential )]
		    private struct CHARFORMAT2_STRUCT
		    {
			    public UInt32	cbSize; 
			    public UInt32   dwMask; 
			    public UInt32   dwEffects; 
			    public Int32    yHeight; 
			    public Int32    yOffset; 
			    public Int32	crTextColor; 
			    public byte     bCharSet; 
			    public byte     bPitchAndFamily; 
			    [MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
			    public char[]   szFaceName; 
			    public UInt16	wWeight;
			    public UInt16	sSpacing;
			    public int		crBackColor; // Color.ToArgb() -> int
			    public int		lcid;
			    public int		dwReserved;
			    public Int16	sStyle;
			    public Int16	wKerning;
			    public byte		bUnderlineType;
			    public byte		bAnimation;
			    public byte		bRevAuthor;
			    public byte		bReserved1;
		    }

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
		    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		    private const int WM_USER			 = 0x0400;
		    private const int EM_SETCHARFORMAT	 = WM_USER+68;

		    private const int SCF_SELECTION	= 0x0001;

		    #region CHARFORMAT2 Flags
		    private const UInt32 CFE_LINK		= 0x0020;
		    private const UInt32 CFM_LINK		= 0x00000020;
		    #endregion

		    #endregion

            /// <summary>
            /// Insert a given text at at the current input position as a link.
            /// The link text is followed by a hash (#) and the given hyperlink text, both of
            /// them invisible. When clicked on, the whole link text and hyperlink string are given in the
            /// LinkClickedEventArgs.</summary>
            /// <param name="text">Text to be inserted</param>
            /// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
            private void InsertLink(string text, string hyperlink)
            {
                InsertLink(text, hyperlink, SelectionStart);
            }

            /// <summary>
            /// Insert a given text at a given position as a link. The link text is followed by
            /// a hash (#) and the given hyperlink text, both of them invisible.
            /// When clicked on, the whole link text and hyperlink string are given in the
            /// LinkClickedEventArgs.</summary>
            /// <param name="text">Text to be inserted</param>
            /// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
            /// <param name="position">Insert position</param>
            private void InsertLink(string text, string hyperlink, int position)
            {
                if (position < 0 || position > Text.Length)
                    throw new ArgumentOutOfRangeException("position");

                // RichTextBox will treat the backslash as the beginning of a tag.
                // We need to escape them now but then unescape them in the link handler!
                hyperlink = hyperlink.Replace(@"\", @"\\");

                // The RTF format is 7-bit ANSI only. We have to convert Unicode characters.
                text = ConvertToRtf(text);
                hyperlink = ConvertToRtf(hyperlink);

                SelectionStart = position;
                SelectedRtf = @"{\rtf1\ansi " + text + @"\v #" + hyperlink + @"\v0}";
                Select(position, text.Length + hyperlink.Length + 1);
                SetSelectionLink(true);
                Select(position + text.Length + hyperlink.Length + 1, 0);
            }

            /// <summary>
		    /// Set the current selection's link style</summary>
		    /// <param name="link">true: set link style, false: clear link style</param>
            private void SetSelectionLink(bool link)
		    {
			    SetSelectionStyle(CFM_LINK, link ? CFE_LINK : 0);
		    }

		    private void SetSelectionStyle(UInt32 mask, UInt32 effect)
		    {
			    var cf = new CHARFORMAT2_STRUCT();
			    cf.cbSize = (UInt32)Marshal.SizeOf(cf);
			    cf.dwMask = mask;
			    cf.dwEffects = effect;

			    var wpar = new IntPtr(SCF_SELECTION);
			    IntPtr lpar = Marshal.AllocCoTaskMem( Marshal.SizeOf( cf ) );
		        try
		        {
                    Marshal.StructureToPtr(cf, lpar, false);
                    SendMessage(Handle, EM_SETCHARFORMAT, wpar, lpar);
                }
		        finally
		        {
                    Marshal.FreeCoTaskMem(lpar);
		        }
		    }

            #endregion RichTextBoxEx project

            private string m_name;
            private string m_description;
            private Brush m_textBrush;
            private Font m_boldFont;
            private readonly PropertyGrid m_propertyGrid;
        }

        private readonly SplitContainer m_splitContainer = new SplitContainer();
        private readonly ToolStripDropDownButton m_propertyOrganization;
        private readonly ToolStripButton m_navigateOut;
        private readonly ToolStrip m_toolStrip;
        private readonly PropertyGridView m_propertyGridView;
        private readonly DescriptionControl m_descriptionTextBox;
        private static readonly Image s_categoryImage = Sce.Atf.ResourceUtil.GetImage16(Resources.ByCategoryImage);
        private static readonly Image s_navigateOutImage = Sce.Atf.ResourceUtil.GetImage16(Resources.NavLeftImage);
        private readonly ToolStripAutoFitTextBox m_patternTextBox;
        private const TextFormatFlags m_TextFormatFlags = TextFormatFlags.WordBreak | TextFormatFlags.Left;
    }
}
