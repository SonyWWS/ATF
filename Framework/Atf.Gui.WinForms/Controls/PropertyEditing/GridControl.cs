//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Wrapper Control for the spreadsheet-style GridView Control, combining it with a toolbar.
    /// Use this as a replacement for the .NET System.Windows.Forms.DataGridView.</summary>
    public class GridControl : Control
    {
        /// <summary>
        /// Default constructor that allows the user to select property organization schemes</summary>
        public GridControl()
            : this(PropertyGridMode.PropertySorting, new GridView())
        {
        }

        /// <summary>
        /// Constructor with PropertyGridMode</summary>
        /// <param name="mode">Flags specifying the GridControl's features and appearance</param>
        public GridControl(PropertyGridMode mode)
            : this(mode, new GridView())
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="mode">Flags specifying the GridControl's features and appearance</param>
        /// <param name="gridView">The GridView to be used. Can be sub-classed to customize its behavior.</param>
        public GridControl(PropertyGridMode mode, GridView gridView)
        {
            m_gridView = gridView;
            m_gridView.BackColor = SystemColors.Window;
            m_gridView.Dock = DockStyle.Fill;
            m_gridView.EditingContextChanged += gridView_BindingChanged;
            m_gridView.MouseUp += gridView_MouseUp;
            m_gridView.DragOver += gridView_DragOver;
            m_gridView.DragDrop += gridView_DragDrop;
            m_gridView.MouseHover += gridView_MouseHover;
            m_gridView.MouseLeave += gridView_MouseLeave;
            m_gridView.SelectedPropertyChanged += gridView_SelectedPropertyChanged;

            m_toolStrip = new ToolStrip();
            m_toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            m_toolStrip.Dock = DockStyle.Top;

            if ((mode & PropertyGridMode.PropertySorting) != 0)
            {
                m_propertyOrganization = new ToolStripDropDownButton(null, s_categoryImage);
                m_propertyOrganization.ToolTipText = "Property Organization".Localize(
                    "Could be rephrased as 'How do you want these properties to be organized?'");
                //m_propertyOrganization.ImageTransparentColor = Color.Magenta;
                m_propertyOrganization.DropDownItemClicked += organization_DropDownItemClicked;

                ToolStripMenuItem item1 = new ToolStripMenuItem("Unsorted".Localize());
                item1.Tag = PropertySorting.None;

                ToolStripMenuItem item2 = new ToolStripMenuItem("Alphabetical".Localize());
                item2.Tag = PropertySorting.Alphabetical;

                ToolStripMenuItem item3 = new ToolStripMenuItem("Categorized".Localize());
                item3.Tag = PropertySorting.Categorized;

                ToolStripMenuItem item4 = new ToolStripMenuItem("Categorized Alphabetical Properties".Localize());
                item4.Tag = PropertySorting.Categorized | PropertySorting.Alphabetical;

                ToolStripMenuItem item5 = new ToolStripMenuItem("Alphabetical Categories".Localize());
                item5.Tag = PropertySorting.Categorized | PropertySorting.CategoryAlphabetical;

                ToolStripMenuItem item6 = new ToolStripMenuItem("Alphabetical Categories And Properties".Localize());
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

            if ((mode & PropertyGridMode.ShowHideProperties) != 0)
            {
                m_propertyShowHideButton = new ToolStripButton(null, s_showHidePropertiesImage);
                m_propertyShowHideButton.ToolTipText = "Property Show / Hide".Localize();
                m_propertyShowHideButton.Click += propertyShowHide_Click;
                m_toolStrip.Items.Add(m_propertyShowHideButton);
                m_toolStrip.Items.Add(new ToolStripSeparator());
            }

            if ((mode & PropertyGridMode.DisableDragDropColumnHeaders) != 0)
            {
                m_gridView.DragDropColumnsEnabed = false;
            }

            m_descriptionLabel = new ToolStripAutoFitLabel();
            m_descriptionLabel.TextAlign = ContentAlignment.TopLeft;
            m_descriptionLabel.MaximumWidth = 5000;
            m_toolStrip.Items.Add(m_descriptionLabel);

            SuspendLayout();

            Controls.Add(m_gridView);

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
            Font = new Font("Segoe UI", 9.0f);

            ResumeLayout(false);
            PerformLayout();
        }

        /// <summary>
        /// Binds the control to the selected object</summary>
        /// <param name="selectedObject">Object or array of objects with properties to bind</param>
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
            m_gridView.EditingContext = context;
            SkinService.ApplyActiveSkin(this);
        }

        /// <summary>
        /// Gets the tool strip that appears at the top of the control</summary>
        public ToolStrip ToolStrip
        {
            get { return m_toolStrip; }
        }

        /// <summary>
        /// Gets the multi-column grid PropertyView</summary>
        public GridView GridView
        {
            get { return m_gridView; }
        }

        /// <summary>
        /// Gets and sets the control's property display sort order</summary>
        public PropertySorting PropertySorting
        {
            get { return m_gridView.PropertySorting; }
            set
            {
                m_gridView.PropertySorting = value;
                UpdateToolstripItems();
            }
        }

        /// <summary>
        /// Gets and sets the control's persistent settings</summary>
        public string Settings
        {
            get { return m_gridView.Settings; }
            set
            {
                m_gridView.Settings = value;
                UpdateToolstripItems();
            }
        }

        /// <summary>
        /// Gets whether the current property being edited can be reset</summary>
        public bool CanResetCurrent
        {
            get { return m_gridView.CanResetCurrent; }
        }

        /// <summary>
        /// Resets the current property being edited to its default value</summary>
        public void ResetCurrent()
        {
            m_gridView.ResetCurrent();
        }

        /// <summary>
        /// Resets all properties to their default values</summary>
        public void ResetAll()
        {
            m_gridView.ResetAll();
        }

        /// <summary>
        /// Refreshes all property representations in the control</summary>
        public void RefreshProperties()
        {
            m_gridView.RefreshProperties();
        }

        /// <summary>
        /// Gets the PropertyDescriptor for the property under the client point, or null if none</summary>
        /// <param name="clientPoint">Client point</param>
        /// <returns>PropertyDescriptor for the property under the client point, or null if none</returns>
        public PropertyDescriptor GetDescriptorAt(Point clientPoint)
        {
            return m_gridView.GetDescriptorAt(clientPoint);
        }

        #region Overrides

        /// <summary>
        /// Gets and sets whether the control can drag and drop</summary>
        public override bool AllowDrop
        {
            set
            {
                base.AllowDrop = value;
                m_gridView.AllowDrop = value;
            }
        }

        #endregion

        #region Event Handlers

        private void gridView_BindingChanged(object sender, EventArgs e)
        {
            UpdateDescription();
        }

        private void gridView_MouseUp(object sender, MouseEventArgs e)
        {
            // pass mouse right-clicks up to clients for context menu implementation
            if (e.Button == MouseButtons.Right)
            {
                Point p = m_gridView.PointToScreen(new Point(e.X, e.Y));
                p = PointToClient(p);
                base.OnMouseUp(new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }
        }

        private void gridView_DragDrop(object sender, DragEventArgs e)
        {
            // raise event on this control
            OnDragDrop(e);
        }

        private void gridView_DragOver(object sender, DragEventArgs e)
        {
            // raise event on this control
            OnDragOver(e);
        }

        private void gridView_MouseHover(object sender, EventArgs e)
        {
            // raise event on this control
            OnMouseHover(e);
        }

        private void gridView_MouseLeave(object sender, EventArgs e)
        {
            // raise event on this control
            OnMouseLeave(e);
        }

        private void organization_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            m_gridView.PropertySorting = (PropertySorting)e.ClickedItem.Tag;
            m_gridView.Invalidate();
            UpdateToolstripItems();
        }

        private void propertyShowHide_Click(object sender, EventArgs e)
        {
            if (m_gridView.GetColumnUserHiddenStates().Count > 0)
            {
                GridControlShowHidePropertiesDialog showHidePropertiesDialog = new GridControlShowHidePropertiesDialog(m_gridView);

                showHidePropertiesDialog.ShowIcon = false;
                showHidePropertiesDialog.ShowInTaskbar = false;
                showHidePropertiesDialog.MaximizeBox = false;
                showHidePropertiesDialog.MinimizeBox = false;

                showHidePropertiesDialog.ShowDialog();
            }
        }

        private void gridView_SelectedPropertyChanged(object sender, EventArgs e)
        {
            UpdateDescription();
        }

        private void UpdateDescription()
        {
            string text = string.Empty;
            PropertyDescriptor descriptor = m_gridView.SelectedPropertyDescriptor;
            if (descriptor != null)
                text = descriptor.Description;

            m_descriptionLabel.Text = text;
        }

        #endregion

        private void UpdateToolstripItems()
        {
            if (m_propertyOrganization != null)
            {
                foreach (ToolStripMenuItem item in m_propertyOrganization.DropDownItems)
                    item.Checked = ((PropertySorting)item.Tag == m_gridView.PropertySorting);
            }
        }

        private readonly ToolStripDropDownButton m_propertyOrganization;
        private readonly ToolStripButton m_propertyShowHideButton;
        private readonly ToolStrip m_toolStrip;
        private readonly GridView m_gridView;
        readonly ToolStripAutoFitLabel m_descriptionLabel;

        private static readonly Image s_categoryImage = Sce.Atf.ResourceUtil.GetImage16(Resources.ByCategoryImage);
        private static readonly Image s_showHidePropertiesImage = Sce.Atf.ResourceUtil.GetImage16(Resources.CheckedItemsImage);
    }
}
