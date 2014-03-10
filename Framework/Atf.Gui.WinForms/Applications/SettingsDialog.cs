//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Dialog for presenting user settings to the user for editing. Used only by
    /// SettingsService.</summary>
    internal class SettingsDialog : Form
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="settingsService">Settings service</param>
        /// <param name="dialogOwner">Dialog owner window HWND</param>
        /// <param name="pathName">Path of settings to display initially, or null</param>
        public SettingsDialog(SettingsService settingsService, IWin32Window dialogOwner, string pathName)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            SplitterRatio = 0.33f;

            m_settingsService = settingsService;
            m_dialogOwner = dialogOwner;

            m_originalState = m_settingsService.UserState; // for cancel

            m_treeControl = new TreeControl(TreeControl.Style.SimpleTree);
            m_treeControl.Dock = DockStyle.Fill;
            m_treeControl.SelectionMode = SelectionMode.One;
            m_treeControl.ShowRoot = false;
            m_treeControl.ImageList = ResourceUtil.GetImageList16();
            m_treeControl.ExpandAll();

            m_treeControl.NodeSelectedChanged += treeControl_NodeSelectedChanged;

            m_treeControlAdapter = new TreeControlAdapter(m_treeControl);
            m_treeControlAdapter.TreeView = settingsService.UserSettings;

            treePanel.Controls.Add(m_treeControl);

            m_propertyGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
            m_propertyGrid.Dock = DockStyle.Fill;
            propertiesPanel.Controls.Add(m_propertyGrid);

            // select an initial node so something is displayed in the PropertyGrid
            TreeControl.Node firstNode;
            if (pathName != null)
                firstNode = m_treeControlAdapter.ExpandPath(m_settingsService.GetSettingsPath(pathName));
            else
                firstNode = m_treeControl.ExpandToFirstLeaf();

            firstNode.Selected = true;
            ShowProperties(m_settingsService.GetProperties((Tree<object>)firstNode.Tag)); //does auto-setting of column widths

            defaultsButton.Click += DefaultsButton_Click;
        }

        /// <summary>
        /// Gets or sets the control's persistent settings</summary>
        public string Settings
        {
            get
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root = xmlDoc.CreateElement("SettingsDialog");
                root.SetAttribute("SplitterRatio", SplitterRatio.ToString());

                string grid = m_propertyGrid.Settings;
                root.SetAttribute("PropertyGrid", grid);

                xmlDoc.AppendChild(root);

                return xmlDoc.InnerXml;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(value);

                XmlElement root = xmlDoc.DocumentElement;
                if (root == null || root.Name != "SettingsDialog")
                    return;

                string s;
                s = root.GetAttribute("SplitterRatio");
                float f;
                if (float.TryParse(s, out f))
                    SplitterRatio = f;

                s = root.GetAttribute("PropertyGrid");
                m_propertyGrid.Settings = s;
            }
        }

        /// <summary>
        /// Cleans up any resources being used</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the tree control</summary>
        public TreeControl TreeControl
        {
            get { return m_treeControl; }
        }

        private void treeControl_NodeSelectedChanged(object sender, TreeControl.NodeEventArgs e)
        {
            if (e.Node.Selected)
            {
                ShowProperties(m_settingsService.GetProperties((Tree<object>)e.Node.Tag));
            }
        }

        private void DefaultsButton_Click(object sender, EventArgs e)
        {
            ConfirmationDialog dialog =
                new ConfirmationDialog(
                    "Reset All Preferences".Localize("Reset all preferences to their default values?"),
                    "Reset all preferences to their default values?".Localize());
            DialogResult result = dialog.ShowDialog(m_dialogOwner);
            if (result == DialogResult.Yes)
            {
                m_settingsService.SetDefaults();
                m_propertyGrid.Refresh();
            }
        }

        private void SettingsDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                m_settingsService.UserState = m_originalState;
            }
        }

        private void ShowProperties(List<PropertyDescriptor> properties)
        {
            if (properties != null)
            {
                m_propertyGrid.Bind(new PropertyCollectionWrapper(properties.ToArray()));
            }
        }

        /// <summary>
        /// Gets or sets the fraction of the width of this control devoted to the left panel (which shows
        /// the setting categories). This value should be between [0,1].</summary>
        private float SplitterRatio
        {
            // It's should never be possible to get a width of zero here.
            get { return ((float)splitContainer.SplitterDistance / splitContainer.Width); }
            set { splitContainer.SplitterDistance = (int)(value * splitContainer.Width); }
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor</summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
            this.cancelButton = new Button();
            this.okButton = new Button();
            this.defaultsButton = new Button();
            this.buttonPanel = new Panel();
            this.upperPanel = new Panel();
            this.splitContainer = new SplitContainer();
            this.treePanel = new Panel();
            this.propertiesPanel = new Panel();
            this.buttonPanel.SuspendLayout();
            this.upperPanel.SuspendLayout();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = DialogResult.OK;
            this.okButton.Name = "okButton";
            // 
            // defaultsButton
            // 
            resources.ApplyResources(this.defaultsButton, "defaultsButton");
            this.defaultsButton.Name = "defaultsButton";
            // 
            // buttonPanel
            // 
            this.buttonPanel.BorderStyle = BorderStyle.FixedSingle;
            this.buttonPanel.Controls.Add(this.defaultsButton);
            this.buttonPanel.Controls.Add(this.okButton);
            this.buttonPanel.Controls.Add(this.cancelButton);
            resources.ApplyResources(this.buttonPanel, "buttonPanel");
            this.buttonPanel.Name = "buttonPanel";
            // 
            // upperPanel
            // 
            this.upperPanel.Controls.Add(this.splitContainer);
            resources.ApplyResources(this.upperPanel, "upperPanel");
            this.upperPanel.Name = "upperPanel";
            // 
            // splitContainer
            // 
            resources.ApplyResources(this.splitContainer, "splitContainer1");
            this.splitContainer.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.treePanel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.propertiesPanel);
            // 
            // treePanel
            // 
            resources.ApplyResources(this.treePanel, "treePanel");
            this.treePanel.Name = "treePanel";
            // 
            // propertiesPanel
            // 
            resources.ApplyResources(this.propertiesPanel, "propertiesPanel");
            this.propertiesPanel.Name = "propertiesPanel";
            // 
            // SettingsDialog
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.upperPanel);
            this.Controls.Add(this.buttonPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.FormClosing += new FormClosingEventHandler(this.SettingsDialog_FormClosing);
            this.buttonPanel.ResumeLayout(false);
            this.upperPanel.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private readonly SettingsService m_settingsService;
        private readonly IWin32Window m_dialogOwner;
        private readonly object m_originalState;
        private readonly TreeControl m_treeControl;
        private readonly TreeControlAdapter m_treeControlAdapter;
        private readonly Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyGrid;

        private Button cancelButton;
        private Button okButton;
        private Button defaultsButton;
        private Panel buttonPanel;
        private Panel upperPanel;
        private SplitContainer splitContainer;
        private Panel treePanel;
        private Panel propertiesPanel;

        /// <summary>
        /// Required designer variable</summary>
        private readonly System.ComponentModel.Container components = null;
    }
}
