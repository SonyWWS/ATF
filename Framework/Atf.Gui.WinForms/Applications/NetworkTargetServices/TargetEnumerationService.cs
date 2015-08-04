//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Linq;

using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;


namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Service that queries and enumerates target objects. It consumes target providers created by the application. 
    /// Each provider is responsible for discovering and reporting targets of a specific type and their parameters, 
    /// while the service combines all the targets' information into a heterogeneous list view for displaying and editing.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ITargetConsumer))]
    [Export(typeof(TargetEnumerationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TargetEnumerationService : IControlHostClient, ITargetConsumer, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="controlHostService">Control host service</param>
        [ImportingConstructor]
        public TargetEnumerationService(IControlHostService controlHostService)
        {
            m_controlHostService = controlHostService;            
        }

        /// <summary>
        /// Default constructor, which is needed for ATF2.9 wrapper of TargetEnumerationService</summary>
        public TargetEnumerationService()
        {
        }

        /// <summary>
        /// Gets or sets whether the user interface is in a dockable panel or as a dialog box.
        /// This must be set before the components are initialized or TypeCatalog is initialized.</summary>
        static public bool ShowAsDialog
        {
            get { return s_showAsDialog; }
            set { s_showAsDialog = value; }
        }

        /// <summary>
        /// Gets or sets whether the service is enabled</summary>
        public bool Enabled
        {
            get { return m_enabled; }
            set
            {
                m_enabled = value;
                m_userControl.Enabled = m_enabled;
            }
        }

        /// <summary>
        /// Event that is raised when selected targets changed</summary>
        public event EventHandler<SelectedTargetsChangedArgs> SelectedTargetsChanged;

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // force creation of the window handles on the GUI thread
            // see http://forums.msdn.microsoft.com/en-US/clr/thread/fa033425-0149-4b9a-9c8b-bcd2196d5471/
            var handle = MainForm.Handle;

            var control = SetUpTargetsView();

            if (!ShowAsDialog)
            {
                m_controlHostService.RegisterControl(
                    control,
                    new ControlInfo(
                       "Targets".Localize(),
                       "Controls for managing targets.".Localize(),
                        StandardControlGroup.Bottom),
                   this);
            }

            if (m_settingsService != null)
            {
                m_settingsService.RegisterSettings(this,
                 new BoundPropertyDescriptor(this, () => PersistedUISettings, "UI Settings".Localize(), null, null));
                m_settingsService.RegisterSettings(this,
                   new BoundPropertyDescriptor(this, () => PersistedSelectedTargets, "Selected Targets".Localize(), null, null));
            }

        }
        #endregion

        /// <summary>
        /// Gets or sets the context menu command providers to use</summary>
        public IEnumerable<IContextMenuCommandProvider> ContextMenuCommandProviders
        {
            get
            {
                if (m_actualContextMenuCommandProviders != null)
                    return m_actualContextMenuCommandProviders;
                if (m_contextMenuCommandProviders != null)
                {
                    m_actualContextMenuCommandProviders =
                        new List<IContextMenuCommandProvider>(m_contextMenuCommandProviders.GetValues());
                    return m_actualContextMenuCommandProviders;
                }
                return EmptyEnumerable<IContextMenuCommandProvider>.Instance;
            }


            set { m_actualContextMenuCommandProviders = new List<IContextMenuCommandProvider>(value); }
        }

        /// <summary>
        /// Shows the control in a modal dialog</summary>
        /// <param name="title">Dialog title</param>
        public void ShowDialog(string title)
        {
            System.Diagnostics.Debug.Assert(ShowAsDialog);

            if (!Enabled)
                return;

            if (title == null)
            {
                title = "Targets".Localize();
            }

            var targetForm = new Form();

            // No icon and no task bar button.
            // No maximize or minimize buttons.
            targetForm.ShowIcon = false;
            targetForm.ShowInTaskbar = false;
            targetForm.MaximizeBox = false;
            targetForm.MinimizeBox = false;
            
            targetForm.Controls.Add(m_userControl);
            targetForm.Text = title;

            targetForm.ShowDialog();
        }

        #region Persisting
        /// <summary>
        /// Gets or sets the UI settings to persist with the settings service</summary>
        public string PersistedUISettings
        {
            get { return m_listView.Settings; }
            set { m_listView.Settings = value; }
        }

        /// <summary>
        /// Gets or sets the persisted selected targets</summary>
        public string PersistedSelectedTargets
        {
            get
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                XmlElement root = xmlDoc.CreateElement("SelectedTargets");
                xmlDoc.AppendChild(root);

                for (int index = 0; index < m_listView.Items.Count; ++index )
                {
                    if (m_listView.Items[index].Checked)
                    {
                        var target = m_targets[index];
                        XmlElement elem = xmlDoc.CreateElement("Target");
                        elem.SetAttribute("name", target.Name);
                        elem.SetAttribute("platform", target.Platform);
                        elem.SetAttribute("endpoint", target.Endpoint);
                        elem.SetAttribute("protocol", target.Protocol);
                        elem.SetAttribute("scope", target.Scope.ToString());
                        root.AppendChild(elem);
                    }                 
                }

                if (xmlDoc.DocumentElement.ChildNodes.Count == 0)
                    xmlDoc.RemoveAll();

                return xmlDoc.InnerXml;
            }

            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(value);
                    XmlNodeList nodes = xmlDoc.DocumentElement.SelectNodes("Target");
                    if (nodes == null || nodes.Count == 0)
                        return;

                    foreach (XmlElement elem in nodes)
                    {
                        var target = new DummmyTargetInfo
                        {
                            Name = elem.GetAttribute("name"),
                            Endpoint = elem.GetAttribute("endpoint"),
                            Protocol = elem.GetAttribute("protocol"),
                            Platform = elem.GetAttribute("platform"),
                            Scope = (TargetScope)Enum.Parse(typeof(TargetScope), elem.GetAttribute("scope")),
                        };

                        m_targetsToSelect.Add(target);
                        SelectTargets(m_targetsToSelect, true);
                    }
                }
            }
        }
   
        private class DummmyTargetInfo: TargetInfo{}
   
        #endregion

        #region ITargetConsumer

        /// <summary>
        /// Processes updated TargetInfos</summary>
        /// <param name="targetProvider">The data provider</param>
        /// <param name="targets">A sequence of targets</param>
        public void TargetsChanged(ITargetProvider targetProvider, IEnumerable<TargetInfo> targets)
        {
            // This needs to be thread safe in case it happens on a background or network processing thread.
            // If the MainForm isn't even set up yet, however, assume we're still running single threaded at
            // startup and processing intial settings.  In that case, just go ahead and deliver the information
            // directly.
            if (MainForm == null || !MainForm.IsHandleCreated || MainForm.IsDisposed)
            {
                UpdateTargetsView(targetProvider, targets);
            }
            else
            {
                MainForm.Invoke(new MethodInvoker(() => UpdateTargetsView(targetProvider, targets)));
            }       
        }

        /// <summary>
        /// Gets all targets</summary>
        public IEnumerable<TargetInfo> AllTargets
        {
            get
            {
                foreach (var target in m_targets)
                    yield return target;
            }
        }

        /// <summary>
        /// Gets or sets selected targets</summary>
        public IEnumerable<TargetInfo> SelectedTargets
        {
            get
            {
                for (int index = 0; index < m_listView.Items.Count; ++index )
                {
                    if ((m_listView.Items[index] != null) && m_listView.Items[index].Checked)
                     yield return m_targets[index];
                }
            }
            set
            {
                SelectTargets(value,false);
            }
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered by this IControlHostClient.</remarks>
        public void Activate(Control control)
        {
            // bring focus to listview so selected targets are more visible(normal highlight)
            m_userControl.ActiveControl = m_listView;
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered by this IControlHostClient.</remarks>
        public void Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control.</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns>True if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered by this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.</remarks>
        public bool Close(Control control)
        {          
            return true;
        }

        #endregion

        private void SelectTargets(IEnumerable<TargetInfo> targets, bool valueEqual)
        {
            if (MainForm == null || !MainForm.IsHandleCreated || MainForm.IsDisposed)
                return;
            MainForm.Invoke(new MethodInvoker(() => ListViewSelectTargets(targets, valueEqual)));
        }
       
        [ImportMany]
        private IEnumerable<ITargetProvider> m_targetProviders= null;

        /// <summary>
        /// Gets or sets target providers</summary>
        public IEnumerable<ITargetProvider> TargetProviders
        {
            get { return m_targetProviders; }
            set { m_targetProviders = value; }
        }

        /// <summary>
        /// Gets or sets the main form</summary>
        [Import(AllowDefault = true)]
        protected Form MainForm { get; set; }

        /// <summary>
        /// Gets or sets the setting service to use</summary>
        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;

        /// <summary>
        /// Gets or sets the command service to use</summary>
        [Import(AllowDefault = true)]
        public ICommandService CommandService { get; set; }


        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;
        private List<IContextMenuCommandProvider> m_actualContextMenuCommandProviders;

        /// <summary>
        /// Sets up targets view Control</summary>
        /// <returns>Control for viewing targets</returns>
        protected Control SetUpTargetsView()
        {
            m_userControl = new UserControl { Margin = new Padding(3) };
            //panel.BackColor = Color.DeepSkyBlue;
            m_userControl.Dock = DockStyle.Fill;
           
            m_userControl.AutoSize = true;

            m_addTargetButton = new SplitButton();
            m_addTargetButton.Text = "Add Target".Localize();
            m_addTargetButton.Location = new Point(m_userControl.Margin.Left,
                m_userControl.Height - m_userControl.Margin.Bottom - m_addTargetButton.Height - m_addTargetButton.Margin.Size.Height);

            m_addTargetButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            m_addTargetButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
  
            m_listView = new DataBoundListView();          
            m_listView.DataSource = m_targets;
            m_listView.BindingContext = m_userControl.BindingContext;
            m_listView.AlternatingRowColors = true;
            m_listView.MultiSelect = false;
            m_listView.CheckBoxes = true;
            
            //m_listView.BackColor = SystemColors.MenuHighlight;
            m_listView.Location = new Point(m_userControl.Margin.Left, m_userControl.Margin.Top);
            m_listView.Size = new Size(m_userControl.Width - m_userControl.Margin.Left - m_userControl.Margin.Right,
                m_userControl.Height - m_userControl.Margin.Top - m_userControl.Margin.Bottom - 
                m_addTargetButton.Height - m_addTargetButton.Margin.Top - m_addTargetButton.Margin.Size.Height);
            m_listView.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            m_listView.Name = "targetsListView";
            m_listView.CellValidating += listView_CellValidating;
            m_listView.ItemChecked += listView_ItemChecked;
            m_listView.ItemCheck += listView_ItemCheck;
            m_listView.MouseUp += listView_MouseUp;
         
            m_userControl.Controls.Add(m_listView);
            m_userControl.Controls.Add(m_addTargetButton);

            if (ShowAsDialog)
            {
                m_okButton = new System.Windows.Forms.Button();
                m_okButton.Name = "m_okButton";
                m_okButton.Text = "OK";
                m_okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
                m_okButton.Location = new Point(m_userControl.Width - m_userControl.Margin.Right - m_okButton.Width,
                    m_userControl.Height - m_userControl.Margin.Bottom - m_okButton.Height - m_okButton.Margin.Size.Height);
                m_okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                m_userControl.Controls.Add(m_okButton);
            }

        
            m_addTargetButton.ShowSplit = true;
            m_addTargetButton.ContextMenuStrip = new ContextMenuStrip();
            foreach (var targetProvider in TargetProviders)
            {
                if (targetProvider.CanCreateNew)
                    m_addTargetButton.ContextMenuStrip.Items.Add(GetAddNewTargetString(targetProvider.Name));
            }
            m_addTargetButton.ContextMenuStrip.ItemClicked += ContextMenuStrip_ItemClicked;
            if (m_addTargetButton.ContextMenuStrip.Items.Count ==1)
            {
                m_addTargetButton.ShowSplit = false;
                ToolStripItem onlyItem = m_addTargetButton.ContextMenuStrip.Items[0];
                m_addTargetButton.Text = onlyItem.Text;
                m_addTargetButton.Click += delegate
                {
                    foreach (var targetProvider in TargetProviders)
                    {
                        if (GetAddNewTargetString(targetProvider.Name) == m_addTargetButton.Text)
                        {
                            targetProvider.AddTarget(targetProvider.CreateNew());
                            break;
                        }
                    }
                };
            }

            m_userControl.Name = "Targets".Localize();

            return m_userControl;
        }

     
        /// <summary>
        /// Performs custom actions when menu strip item clicked</summary>
        /// <param name="sender">Sender control</param>
        /// <param name="e">Event args</param>
        void ContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (var targetProvider in TargetProviders)
            {
                if (GetAddNewTargetString(targetProvider.Name) == e.ClickedItem.Text)
                {
                    targetProvider.AddTarget(targetProvider.CreateNew());
                    break;
                }
            }
        }

        /// <summary>
        /// Performs custom actions on MouseUp event</summary>
        /// <param name="sender">Sender control</param>
        /// <param name="e">Event args</param>
        void listView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                // This will clear the selected targets cache in TargetCommands, not necessary but may be slightly beneficial
                ContextMenuCommandProviders.GetCommands(this, null);
                return;
            }

            if (CommandService == null)
                return;

            if (ContextMenuCommandProviders == null)
                return;
            
            m_targetsPicked.Clear();
            foreach (int index in m_listView.SelectedIndices)
                m_targetsPicked.Add(m_targets[index]);
            IEnumerable<object> commands = ContextMenuCommandProviders.GetCommands(this, m_targetsPicked);

            Point screenPoint = m_listView.PointToScreen(new Point(e.X, e.Y));
            CommandService.RunContextMenu(commands, screenPoint);         
        }

        /// <summary>
        /// Performs custom actions when the check state of an item changes</summary>
        /// <param name="sender">Sender control</param>
        /// <param name="e">Event args</param>
        void listView_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (m_listView.SortingItems)
                return;
            if (m_listView.MultiSelect)
            {
                m_targetsLastChecked = SelectedTargets.ToList();
            }
            else if (e.NewValue == CheckState.Checked)
                m_targetsLastChecked = SelectedTargets.ToList();
        }

        /// <summary>
        /// Performs custom actions when checked state of an item changes</summary>
        /// <param name="sender">Sender control</param>
        /// <param name="e">Event args</param>
        void listView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (m_listView.SortingItems)
                return;
            if (m_listView.MultiSelect)
            {            
                var currentSelected = SelectedTargets.ToList();

                if (!m_targetsLastChecked.OrderBy(x => x.Endpoint).SequenceEqual(currentSelected.OrderBy(x => x.Endpoint)))
                    OnSelectedTargetsChanged(new SelectedTargetsChangedArgs(m_targetsLastChecked, SelectedTargets));
            }
            else 
            {
                if (e.Item.Checked)
                {
                    //unchek the rest
                    foreach (ListViewItem item in m_listView.Items)
                    {
                        if ((item != null) && item.Checked && item != e.Item)
                            item.Checked = false;
                    }
                    var currentSelected = SelectedTargets.ToList();

                    if (!m_targetsLastChecked.OrderBy(x => x.Endpoint).SequenceEqual(currentSelected.OrderBy(x => x.Endpoint)))
                        OnSelectedTargetsChanged(new SelectedTargetsChangedArgs(m_targetsLastChecked, SelectedTargets));
                }
            }


        }

        /// <summary>
        /// Raises the SelectedTargetsChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnSelectedTargetsChanged(SelectedTargetsChangedArgs e)
        {
             SelectedTargetsChanged.Raise(this, e);
        }

        /// <summary>
        /// Updates target view for the given target provider with its updated targets</summary>
        /// <param name="targetProvider">Target provider</param>
        /// <param name="targets">Enumeration of targets</param>
        private void UpdateTargetsView(ITargetProvider targetProvider, IEnumerable<TargetInfo> targets)
        {
            var itemsToAdd = new List<TargetInfo>();
            var itemsToRemove = new List<TargetInfo>();
            var itemsToSelect = new List<TargetInfo>();
            m_providerTargets.Remove(targetProvider); // clear cached targets of the given provider

            // add new targets
            foreach (var target in targets)
            {
                m_providerTargets.Add(targetProvider, target);
                if (!m_targets.Contains(target))
                    itemsToAdd.Add(target);
            }

            if (itemsToAdd.Count > 3)
                m_targets.AddRange(itemsToAdd);
            else
            {
                foreach (var item in itemsToAdd)
                    m_targets.Add(item);
            }
 
            // scan new targets to select, remove obsolete targets that no longer belong to any provider      
            foreach ( var target in m_targets)
            {
                bool toRemove = true;
                
                foreach (var provider in m_providerTargets.Keys)
                {
                    if (m_providerTargets.ContainsKeyValue(provider, target))
                        toRemove = false;

                    var matchdItem=  m_targetsToSelect.FirstOrDefault(
                        n => n.Scope == target.Scope && n.Protocol == target.Protocol &&
                             n.Name == target.Name && n.Endpoint == target.Endpoint);
                    if (matchdItem != null)
                    {
                        itemsToSelect.Add(target);
                        m_targetsToSelect.Remove(matchdItem);
                    }
                }
                if (toRemove)
                    itemsToRemove.Add(target);
            }

            foreach (var item in itemsToRemove)
                m_targets.Remove(item);
            if (itemsToRemove.Count >0)
                m_listView.Refresh();
            
            if (itemsToSelect.Count >0)
                ListViewSelectTargets(itemsToSelect, true);
        }

        /// <summary>
        /// Sets which targets are selected (as indicated by a checked radio button) and clears
        /// previously selected targets.</summary>
        /// <param name="targets">The targets to indicate as being selected / checked.</param>
        /// <param name="valueEqual">If 'true', then the given TargetInfos will be tested for
        /// equivalency with existing TargetInfo objects but will not replace them.</param>
        private void ListViewSelectTargets(IEnumerable<TargetInfo> targets, bool valueEqual)
        {
            //--------------------------------------------------------------------------------------------------
            // The list returned by SelectedTargets is based on the checked states in the list view.
            //
            // When the user clicks a radio button in the GUI to set a new target, the previous selection
            // is un-checked. (See listView_ItemChecked.) As a result, there is only ever 1 item in the
            // SelectedTargets list. Client code assumes this, and uses SelectedTargets.FirstOrDefault() to
            // get the (presumed) one and only selected target.
            //
            // But when client code sets SelectedTargets by hand, the previous selected item in the list
            // control is not automatically un-checked. As a result, SelectedTargets returns 2 items!
            // If the previously selected item happened to be first in the list control, SelectedTargets
            // will have the old, supposedly de-selected, item at the head of the list. SelectedTargets.FirstOrDefault()
            // will yield the wrong item, and the client will attempt to connect to the wrong device.
            //
            // So, we need to clear all the checked states in the list control before setting a new selection.
            //--------------------------------------------------------------------------------------------------
            foreach (ListViewItem item in m_listView.Items)
            {
                if ((item != null) && item.Checked)
                    item.Checked = false;
            }

            List<TargetInfo> targetsToSelect = null; 
            if (valueEqual)
            {
                targetsToSelect = new List<TargetInfo>();
                foreach ( var target in m_targets)
                {
                    var matchdItem = targets.FirstOrDefault(
                      n => n.Scope == target.Scope && n.Protocol == target.Protocol &&
                           n.Name == target.Name && n.Endpoint == target.Endpoint);
                    if (matchdItem != null)
                    {
                        targetsToSelect.Add(target);
                        m_targetsToSelect.RemoveAll( n => n.Scope == target.Scope && n.Protocol == target.Protocol &&
                           n.Name == target.Name && n.Endpoint == target.Endpoint);
                        break;
                    }
                }           
            }
            else
            {
                targetsToSelect = targets.ToList();
                m_targetsToSelect.Clear();
            }
          
           
            foreach (var target in targetsToSelect)
            {
                int index = m_targets.IndexOf(target);
                if (index != -1 && index < m_listView.Items.Count)
                    m_listView.Items[index].Checked = true;
            }          
        }

        /// <summary>
        /// Performs custom actions before a cell is validated</summary>
        /// <param name="sender">Sender control</param>
        /// <param name="e">Event args</param>
        void listView_CellValidating(object sender, DataBoundListView.ListViewCellValidatingEventArgs e)
        {
            string propertyName = m_listView.ItemProperties[e.ColumnIndex].Name;
            var target = m_targets[e.RowIndex];
            if (target is IPropertyValueValidator)
            {
                string errorMessage;
                e.Cancel =
                    !((IPropertyValueValidator) target).Validate(propertyName, e.FormattedValue, out errorMessage);
                if (e.Cancel)
                    Outputs.WriteLine(OutputMessageType.Warning, errorMessage);
            }
        }

        private string GetAddNewTargetString(string targetName)
        {
            return string.Format("Add New {0}".Localize(), targetName);
        }

        private IControlHostService m_controlHostService;
        private SortableBindingList<TargetInfo> m_targets = new SortableBindingList<TargetInfo>();
        private List<TargetInfo> m_targetsToSelect = new List<TargetInfo>();
        private List<TargetInfo> m_targetsPicked = new List<TargetInfo>();
        private List<TargetInfo> m_targetsLastChecked =  new List<TargetInfo>();
        private Multimap<ITargetProvider, TargetInfo> m_providerTargets = new Multimap<ITargetProvider, TargetInfo>();
        private DataBoundListView m_listView;
        private SplitButton m_addTargetButton;
        private Button m_okButton;
        private UserControl m_userControl;
        private bool m_enabled = true;

        static private bool s_showAsDialog = false;
    }
}
