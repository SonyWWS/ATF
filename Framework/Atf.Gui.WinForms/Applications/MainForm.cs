//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Application main form, with optional toolstrip container. Persists form size, state, and
    /// toolstrip state.</summary>
    [Export(typeof(Form))]
    [Export(typeof(IMainWindow))]
    [Export(typeof(IWin32Window))]
    [Export(typeof(ISynchronizeInvoke))]
    [Export(typeof(MainForm))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MainForm : Form, IMainWindow
    {
        /// <summary>
        /// Required designer variable</summary>
        private readonly System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Default constructor</summary>
        public MainForm()
            : this(null)
        {
        }

        /// <summary>
        /// Constructor using ToolStripContainer</summary>
        /// <param name="toolStripContainer">Optional tool strip container for toolbars and controls</param>
        public MainForm(ToolStripContainer toolStripContainer)
        {
            InitializeComponent();

            StartPosition = FormStartPosition.Manual; // so we can persist bounds
            m_mainFormBounds = Bounds;

            if (toolStripContainer != null)
            {
                m_toolStripContainer = toolStripContainer;
                m_toolStripContainer.Dock = DockStyle.Fill;
                Controls.Add(m_toolStripContainer);
            }            
        }

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;

        [Import(AllowDefault = true)]
        private ICommandService m_commandService;

        /// <summary>
        /// Gets the form's ToolStripContainer</summary>
        public ToolStripContainer ToolStripContainer
        {
            get { return m_toolStripContainer; }
        }

        #region IMainWindow Members

        /// <summary>
        /// Gets a handle for displaying WinForms dialogs with an owner</summary>
        public IWin32Window DialogOwner
        {
            get { return this; }
        }

        /// <summary>
        /// Event that is raised before the application is loaded</summary>
        public event EventHandler Loading;

        /// <summary>
        /// Event that is raised after the application is loaded</summary>
        public event EventHandler Loaded;

        #endregion

        /// <summary>
        /// Processes a command key</summary>
        /// <param name="msg">A <see cref="T:System.Windows.Forms.Message"></see>, passed by reference, that represents the Win32 message to process</param>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"></see> values that represents the key to process</param>
        /// <returns>True if the keystroke was processed and consumed by the control; otherwise false to allow further processing</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool consumed = false;

            // Route to command service if not a text input control. If it is a text input control,
            //  only route to the command service if the keypress is not standard textbox input.
            Control focusedControl = WinFormsUtil.GetFocusedControl();
            if (
                (!(focusedControl is TextBoxBase) && !(focusedControl is ComboBox)) ||
                !KeysUtil.IsTextBoxInput(focusedControl, keyData)
                )
            {
                if (m_commandService != null)
                {
                    consumed = m_commandService.ProcessKey(keyData);
                }

                // If the command key wasn't processed, then let the base handle it so that certain
                //  default behavior like the access keys (the underlines that appear when Alt is held down)
                //  still work correctly.
                if (!consumed)
                {
                    consumed = base.ProcessCmdKey(ref msg, keyData);
                }
            }

            return consumed;
        }

        /// <summary>
        /// Raises the form Load event</summary>
        /// <param name="e">Event args</param>
        protected override void OnLoad(System.EventArgs e)
        {
            try
            {
                m_loading = true;                
                m_showen = false;  // used to test if OnShown is called before finishing OnLoad.
                
                // TODO:  m_settingsService is intended to be initialized via MEF for ATF 3.0 applications.
                //        For legacy applications which don't use MEF, it appears these same settings are registered explicitly
                //        much earlier in ApplicationHostService.cs.  The problem is that we need to ensure we don't try to 
                //        apply settings until this point, which is an open issue for legacy applications, although the default 
                //        ISettingsService "gets lucky" and defers callbacks long enough to get by.
                //        See also other comments in this file labled SCREAM_TOOLBAR_STATE_ISSUE
                if (m_settingsService != null)
                {
                    m_settingsService.RegisterSettings(this,
                        new BoundPropertyDescriptor(this, () => MainFormBounds, "MainFormBounds", null, null),
                        new BoundPropertyDescriptor(this, () => MainFormWindowState, "MainFormWindowState", null, null));

                    if (m_toolStripContainer != null)
                    {
                        m_settingsService.RegisterSettings(this,
                            new BoundPropertyDescriptor(this, () => ToolStripContainerSettings, "ToolStripContainerSettings", null, null));
                    }
                }

                // deserialize  mainform WindowState here works better with DockPanelSuite; 
                // this fixes an issue to restore window size causes the form to span dual monitors   
                // when the program starts maximized
                m_mainFormLoaded = true;
                if (m_maximizeWindow)
                    WindowState = FormWindowState.Maximized;

                // Call base.OnLoad(e) last to ensure Loading event to occur before Loaded event.            
                base.OnLoad(e);
                Loading.Raise(this, e);

                // if OnShown already called then we need raise OnLoaded event here.
                if (m_showen)
                    Loaded.Raise(this, e);
            }
            finally
            {
                m_loading = false;
            }
        }

        // Note: in some cases OnShown method will be called while still 
        // inside OnLoad which will mess up the order of Loading and Loaded events.
        // The following two boolean variables 
        private bool m_loading; 
        private bool m_showen; // shown is called.

        /// <summary>
        /// Raises the form Shown event</summary>
        /// <param name="e">Event args</param>
        protected override void OnShown(EventArgs e)
        {
            m_showen = true;
            base.OnShown(e);            
            // if m_loading is true then this is called before
            // OnLoad finshed, so in this case don't raise
            // Loaded event because we still processing Loading event.
            if (!m_loading)
                Loaded.Raise(this, e);
        }

        /// <summary>
        /// Raises the form SizeChanged event</summary>
        /// <param name="e">Event args</param>
        protected override void OnSizeChanged(System.EventArgs e)
        {
            SetBounds();
            base.OnSizeChanged(e);
        }

        /// <summary>
        /// Raises the form LocationChanged event</summary>
        /// <param name="e">Event args</param>
        protected override void OnLocationChanged(System.EventArgs e)
        {
            SetBounds();
            base.OnLocationChanged(e);
        }

        private void SetBounds()
        {
            // Only set our shadow of the main form's bounds if in normal state
            if (WindowState == FormWindowState.Normal)
                m_mainFormBounds = Bounds;
        }

        /// <summary>
        /// Cleans up any resources being used</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
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
        /// Gets and sets main form bounds</summary>
        public Rectangle MainFormBounds
        {
            get
            {
                return m_mainFormBounds;
            }
            set
            {
                // Make sure the MainForm is visible. Setting Bounds sets m_mainFormBounds via SetBounds()
                if (WinFormsUtil.IsOnScreen(value))
                    Bounds = value;
            }
        }

        /// <summary>
        /// Gets and sets main form window state</summary>
        public FormWindowState MainFormWindowState
        {
            get
            {
                FormWindowState state = WindowState;
                if (state == FormWindowState.Minimized)
                    state = FormWindowState.Normal; // only return normal or maximized

                return state;
            }
            set
            {
                if (value == FormWindowState.Maximized && !m_mainFormLoaded)
                {
                    m_maximizeWindow = true;
                    WindowState = FormWindowState.Normal;
                }
                else
                    WindowState = value;
            }
        }


        

        /// <summary>
        /// Gets and sets the ToolStripContainer's state</summary>
        [Browsable(false)]
        public string ToolStripContainerSettings
        {
            get
            {
                // The default constructor doesn't create this.
                if (m_toolStripContainer == null)
                    return String.Empty;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
                XmlElement root = xmlDoc.CreateElement("ToolStripContainerSettings");
                xmlDoc.AppendChild(root);


                // check if tool strip is locked       
                bool locked = false;
                var toolStrips = m_toolStripContainer.TopToolStripPanel.Controls.AsIEnumerable<ToolStrip>()
                        .Concat(m_toolStripContainer.BottomToolStripPanel.Controls.AsIEnumerable<ToolStrip>())
                        .Concat(m_toolStripContainer.LeftToolStripPanel.Controls.AsIEnumerable<ToolStrip>())
                        .Concat(m_toolStripContainer.RightToolStripPanel.Controls.AsIEnumerable<ToolStrip>());
                foreach (var toolStrip in toolStrips)
                {
                    // skip invisible or unnamed  toolStrip
                    if (!toolStrip.Visible || toolStrip.Name == null || toolStrip.Name.Trim().Length == 0)
                        continue;
                    locked = toolStrip.GripStyle == ToolStripGripStyle.Hidden
                        && !toolStrip.AllowItemReorder;
                    break;
                }

                if (locked)
                {
                    root.SetAttribute("Locked", "true");
                }

                SavePanelState(m_toolStripContainer.TopToolStripPanel, "TopToolStripPanel", xmlDoc, root);
                SavePanelState(m_toolStripContainer.LeftToolStripPanel, "LeftToolStripPanel", xmlDoc, root);
                SavePanelState(m_toolStripContainer.BottomToolStripPanel, "BottomToolStripPanel", xmlDoc, root);
                SavePanelState(m_toolStripContainer.RightToolStripPanel, "RightToolStripPanel", xmlDoc, root);

                return xmlDoc.InnerXml;
            }
            set
            {
                // TODO:  Instead of bailing here, which means we will ignore saved settings, we need to either
                //        cache the settings we're given here until we CAN apply them, or else we need to
                //        ensure that we don't register for settings until we're able to process them since we
                //        do not know when the settings service will decide to deliver values to us.
                //        A third option would be to continue to bail here, but FORCE a redelivery of the settings
                //        later when we're ready for them (ie: OnLoad).
                //        See also other comments in this file labled SCREAM_TOOLBAR_STATE_ISSUE
                if (!m_mainFormLoaded || m_toolStripContainer==null)
                    return;

                var comparer = new ElementSortComparer<XmlElement>();
                Dictionary<string, ToolStrip> toolStrips = new Dictionary<string, ToolStrip>();
                Dictionary<string, ToolStripItem> toolStripItems = new Dictionary<string, ToolStripItem>();

                try
                {
                    // build dictionaries of existing toolstrips and items
                    PrepareLoadPanelState(m_toolStripContainer.TopToolStripPanel, toolStrips, toolStripItems);
                    PrepareLoadPanelState(m_toolStripContainer.LeftToolStripPanel, toolStrips, toolStripItems);
                    PrepareLoadPanelState(m_toolStripContainer.BottomToolStripPanel, toolStrips, toolStripItems);
                    PrepareLoadPanelState(m_toolStripContainer.RightToolStripPanel, toolStrips, toolStripItems);

                    SuspendLayout();
                    m_toolStripContainer.SuspendLayout();

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(value);

                    XmlElement root = xmlDoc.DocumentElement;
                    if (root == null || root.Name != "ToolStripContainerSettings")
                        throw new InvalidOperationException("Invalid Toolstrip settings");


                    if (root.GetAttribute("Locked") == "true")
                    {
                        foreach (var toolStrip in toolStrips.Values)
                        {
                            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
                            toolStrip.AllowItemReorder = false;
                        }                        
                    }

                    // walk xml to restore matching toolstrips and items to their previous state
                    XmlNodeList Panels = root.SelectNodes("ToolStripPanel");
                    foreach (XmlElement panelElement in Panels)
                    {
                        string panelName = panelElement.GetAttribute("Name");
                        ToolStripPanel panel;
                        if (panelName == "TopToolStripPanel")
                            panel = m_toolStripContainer.TopToolStripPanel;
                        else if (panelName == "LeftToolStripPanel")
                            panel = m_toolStripContainer.LeftToolStripPanel;
                        else if (panelName == "BottomToolStripPanel")
                            panel = m_toolStripContainer.BottomToolStripPanel;
                        else if (panelName == "RightToolStripPanel")
                            panel = m_toolStripContainer.RightToolStripPanel;
                        else
                            continue;

                        List<XmlElement> stripElements = new List<XmlElement>();
                        foreach (XmlElement toolStripElement in panelElement.ChildNodes)
                            stripElements.Add(toolStripElement);

                        // sort toolstrips on Y then X.
                        // create list of ToolStrips for each row.
                        // rows are sorted.
                        SortedDictionary<int, List<XmlElement>>
                            rowOrder = new SortedDictionary<int, List<XmlElement>>();
                        foreach (XmlElement toolStripElement in stripElements)
                        {
                            string[] location = toolStripElement.GetAttribute("Location").Split(',');
                            int yloc = int.Parse(location[1]);
                            List<XmlElement> toolstripList;
                            if (!rowOrder.TryGetValue(yloc, out toolstripList))
                            {
                                toolstripList = new List<XmlElement>();
                                rowOrder.Add(yloc, toolstripList);
                            }
                            toolstripList.Add(toolStripElement);
                        }
                        // sort on x.
                        foreach (var toolstripList in rowOrder.Values)
                            toolstripList.Sort(comparer);
                        
                        int cIndex = 0; // keeps track of the toolstrip's index in Controls Collection.
                        int prevHeight = 0; // height of the previous toolstrip in row order.
                        // Don't use persisted y-position directly, instead compute y-position from cumulative heights of all the previous ToolStrips.  
                        // Persisted Y-position is only used for determining Row number for each ToolStrip.
                        int yPos = rowOrder.Count == 0 ? 0 : rowOrder.Keys.First();
                        foreach (var toolStripElements in rowOrder.Values)
                        {
                            yPos += prevHeight;                            
                            foreach (XmlElement toolStripElement in toolStripElements)
                            {
                                string toolStripName = toolStripElement.GetAttribute("Name");
                                ToolStrip toolStrip;
                                if (!toolStrips.TryGetValue(toolStripName, out toolStrip))
                                    continue;

                                toolStrip.Parent.Controls.Remove(toolStrip);
                                panel.Controls.Add(toolStrip);
                                panel.Controls.SetChildIndex(toolStrip, cIndex++);                                
                                if (toolStrip.Height > prevHeight) prevHeight = toolStrip.Height;
                                string[] coords = toolStripElement.GetAttribute("Location").Split(',');
                                int xPos = int.Parse(coords[0]);                                                               
                                toolStrip.Location = new Point(xPos, yPos);
                                XmlNodeList itemNodes = toolStripElement.ChildNodes;
                                int j = 0;
                                foreach (XmlElement itemElement in itemNodes)
                                {
                                    string itemName = itemElement.GetAttribute("Name");
                                    ToolStripItem item;
                                    if (toolStripItems.TryGetValue(itemName, out item))
                                    {
                                        item.Owner.Items.Remove(item);
                                        toolStrip.Items.Insert(j, item);

                                        // ToolStripItem has two visibility properties: Visible and Available. 
                                        // Visible indicates whether the item is displayed,  
                                        // Available indicates whether the ToolStripItem should be placed on a ToolStrip.
                                        // Use Available property here for toolstrip layout.
                                        string visible = itemElement.GetAttribute("Visible");
                                        if (visible == "false")
                                            item.Available = false;
                                        else
                                            item.Available = true;

                                        if (item is ToolStripButton)
                                        {
                                            var button = item as ToolStripButton;
                                            // Only if the attribute is available and is set to "true"...
                                            string isChecked = itemElement.GetAttribute("Checked");
                                            if (isChecked == "true")
                                                button.Checked = true;
                                        }

                                        j++;
                                    }
                                }                                
                            } // foreach (XmlElement toolStripElement in toolStripElements)
                        } // foreach (var toolStripElements in rowOrder.Values)                       
                    } // foreach (XmlElement panelElement in Panels)
                }
                finally
                {
                    foreach (ToolStrip toolStrip in toolStrips.Values)
                        toolStrip.ResumeLayout(true);

                    m_toolStripContainer.TopToolStripPanel.ResumeLayout(true);
                    m_toolStripContainer.LeftToolStripPanel.ResumeLayout(true);
                    m_toolStripContainer.BottomToolStripPanel.ResumeLayout(true);
                    m_toolStripContainer.RightToolStripPanel.ResumeLayout(true);
                    m_toolStripContainer.ResumeLayout(true);
                    ResumeLayout(false);
                }
            }
        }

        private void PrepareLoadPanelState(
            ToolStripPanel panel,
            Dictionary<string, ToolStrip> toolStrips,
            Dictionary<string, ToolStripItem> toolStripItems)
        {
            panel.SuspendLayout();

            foreach (ToolStrip toolStrip in panel.Controls)
            {
                // skip invisible toolstrip
                if (!toolStrip.Visible)
                    continue;
                toolStrips.Add(toolStrip.Name, toolStrip);
                toolStrip.SuspendLayout();                
                foreach (ToolStripItem toolStripItem in toolStrip.Items)
                    toolStripItems.Add(toolStripItem.Name, toolStripItem);
            }
        }

        private static void SavePanelState(
            ToolStripPanel panel,
            string panelName,
            XmlDocument xmlDoc,
            XmlElement root)
        {
            XmlElement panelElement = xmlDoc.CreateElement("ToolStripPanel");
            root.AppendChild(panelElement);
            panelElement.SetAttribute("Name", panelName);

            foreach (ToolStrip toolStrip in panel.Controls)
            {
                // skip invisible or unnamed  toolStrip
                if (!toolStrip.Visible || toolStrip.Name == null || toolStrip.Name.Trim().Length == 0)
                    continue;
                
                XmlElement toolStripElement = xmlDoc.CreateElement("ToolStrip");
                panelElement.AppendChild(toolStripElement);

                toolStripElement.SetAttribute("Name", toolStrip.Name);
                toolStripElement.SetAttribute("Location", string.Format("{0},{1}", toolStrip.Location.X, toolStrip.Location.Y));

                // don't persist menu strip items
                if (toolStrip is MenuStrip)
                    continue;

                foreach (ToolStripItem item in toolStrip.Items)
                {
                    // skip unnamed tool strip items
                    if (item.Name == null || item.Name.Trim().Length == 0)
                        continue;

                    XmlElement itemElement = xmlDoc.CreateElement("ToolStripItem");
                    toolStripElement.AppendChild(itemElement);

                    itemElement.SetAttribute("Name", item.Name);

                    // Buttons that are in the Overflow area have Visible set to false but Available to true.
                    // We want to identify buttons that have been intentially made not visible
                    //  through use of the Customize drop-down menu. See AddCustomizationDropDown()
                    //  in CommandService.
                    if (!(item is ToolStripDropDownButton) &&
                        item.Placement == ToolStripItemPlacement.None &&
                        item.Available == false)
                    {
                        itemElement.SetAttribute("Visible", "false");
                    }

                    if ((item is ToolStripButton) && ((ToolStripButton)item).Checked)
                        itemElement.SetAttribute("Checked", "true");
                }
            }
        }

        private class ElementSortComparer<T> : IComparer<XmlElement>
        {
            int IComparer<XmlElement>.Compare(XmlElement element1, XmlElement element2)
            {
                string[] coords1 = element1.GetAttribute("Location").Split(',');
                string[] coords2 = element2.GetAttribute("Location").Split(',');
                return int.Parse(coords1[0]) - int.Parse(coords2[0]);
            }
        }

        private int ToolStripWidth(ToolStrip toolStrip)
        {
            int result = 0;
            foreach (ToolStripItem item in toolStrip.Items)
                if (item.Visible)
                    result += item.Width;
            return result;

        }

        private readonly ToolStripContainer m_toolStripContainer;

        private Rectangle m_mainFormBounds;
        private bool m_maximizeWindow;
        private bool m_mainFormLoaded;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support. Do not modify
        /// the contents of this method with the code tool.</summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this, "$this");
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
