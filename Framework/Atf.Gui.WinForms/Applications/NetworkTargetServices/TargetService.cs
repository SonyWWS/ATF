//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Standard component implementation of ITargetService</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ITargetService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TargetService : IInitializable, ITargetService, ICommandClient
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service to use</param>
        /// <param name="main">Main form</param>
        [ImportingConstructor]
        public TargetService
            (ICommandService commandService, IMainWindow main)
        {
            m_commandService = commandService;
            m_mainWindow = main;
        }

        /// <summary>
        /// Constructor used from ATF2.9</summary>
        public TargetService()
        {
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            if (m_settingsService != null)
            {                
                m_settingsService.RegisterSettings(this,
                    new BoundPropertyDescriptor(this, () => Targets, "Targets".Localize(), null, null)
                );

            }

            RegisterCommand(m_commandService);            
        }

        #endregion

        /// <summary>
        /// Registers a command for the command client</summary>
        /// <param name="commandService">Command service</param>
        protected void RegisterCommand(ICommandService commandService)
        {
            if (commandService != null)
            {
                commandService.RegisterCommand(
                    Commands.EditTarget,
                Sce.Atf.Applications.StandardMenu.Edit,
                Sce.Atf.Applications.StandardCommandGroup.EditPreferences,
                "Target ...".Localize(),
                "Edit targets".Localize(),
                this);
            }
        }
               
        #region ITargetService Members
        /// <summary>
        /// Gets or sets the single selection mode</summary>
        /// <remarks>Default value is true</remarks>
        public bool SingleSelectionMode
        {
            get { return m_singleSelectionMode; }
            set { m_singleSelectionMode = value; }
        }

        /// <summary>
        /// Enable/Disable if the user can edit the port number</summary>
        /// <remarks>Default value is true</remarks>
        public bool CanEditPortNumber
        {
            get { return m_canEditPortNumber; }
            set { m_canEditPortNumber = value; }
        }

        /// <summary>
        /// Gets or sets default port number. If set, the default port is pre-filled in the
        /// "Add New Target" dialog box.</summary>
        public int DefaultPortNumber
        {
            get { return m_defaultPortNumber; }
            set
            {
                if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort)
                    throw new ArgumentOutOfRangeException();
                m_defaultPortNumber = value;
            }
        }

        /// <summary>
        /// Gets an array of all targets. Each target provides the name, IP address, and port
        /// that can be used to connect.</summary>
        /// <returns>Array of all targets</returns>
        public Target[] GetAllTargets()
        {
            Target[] retVal = null;
            List<Target> lst = new List<Target>();
            if (m_targets.Count > 0)
            {
                foreach (Target t in m_targets.Values)
                    lst.Add((Target)t.Clone());
                retVal = lst.ToArray();                
            }
            return retVal;
        }

        /// <summary>
        /// Gets an array of all selected targets. Each target provides the name, IP address,
        /// and port that can be used to connect.</summary>
        /// <returns>Array of all selected targets</returns>
        public Target[] GetSelectedTargets()
        {
            Target[] retVal = null;
            List<Target> lst = new List<Target>();
            if (m_targets.Count > 0)
            {
                foreach (Target t in m_targets.Values)
                {
                    if (t.Selected)
                        lst.Add((Target)t.Clone());
                }
                if (lst.Count > 0)
                {
                    retVal = lst.ToArray();
                }
            }
            return retVal;
        }

        /// <summary>
        /// Gets the selected target</summary>
        /// <returns>Selected target</returns>
        public Target GetSelectedTarget()
        {
            Target target = null;            
            if (m_targets.Count > 0)
            {
                foreach (Target t in m_targets.Values)
                    if (t.Selected)
                    {
                        target = (Target)t.Clone();
                        break;
                    }               
            }
            return target;
        }

        /// <summary>
        /// Selects a target, given its name.
        /// An exception is thrown if the parameter is invalid or the target is not found.</summary>
        /// <param name="name">Name of the target to be selected</param>
        public void SelectTarget(string name)
        {
            if (StringUtil.IsNullOrEmptyOrWhitespace(name))
                throw new ArgumentNullException();
            if (m_targets.Count == 0)
                throw new InvalidOperationException();

            Target t = null;
            if (!m_targets.TryGetValue(name, out t))
                throw new Exception(name + " target not found");
            if (m_singleSelectionMode)
            {
                foreach (Target trg in m_targets.Values)
                    trg.Selected = false;
            }
            t.Selected = true;
        }

        /// <summary>
        /// Adds a new target. If the target already exists, an exception is thrown.</summary>
        /// <param name="name">Name of the target machine</param>
        /// <param name="host">Host name or IP address</param>
        /// <param name="port">Port number</param>
        public void AddTarget(string name, string host, int port)
        {
            if (StringUtil.IsNullOrEmptyOrWhitespace(name) || StringUtil.IsNullOrEmptyOrWhitespace(host))
                throw new ArgumentNullException();

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                    throw new ArgumentOutOfRangeException();

            if (m_targets.ContainsKey(name))
                throw new Exception(name + " already exist");

            Target t = new Target(name.Trim(), host.Trim(), port);
            m_targets.Add(t.Name, t);
        }

        /// <summary>
        /// Shows the target dialog</summary>
        /// <returns>Target dialog result</returns>
        public virtual DialogResult ShowTargetDialog()
        {
            return ShowTargetDialog(m_mainWindow.DialogOwner);
        }

        /// <summary>
        /// Sets the list of supported protocols</summary>
        /// <param name="protocols">Array of supported protocols</param>
        public void SetProtocols(string[] protocols)
        {
            if (protocols == null || protocols.Length == 0)
                return;
            if (m_protocols != null)
                throw new Exception("protocols already been set");
            m_protocols = protocols;
           
        }

        /// <summary>
        /// Shows the target dialog</summary>
        /// <param name="dialogOwner">Dialog owner window handle</param>
        /// <returns>Target dialog result</returns>
        protected DialogResult ShowTargetDialog(IWin32Window dialogOwner)
        {
            TargetDialog dlg = new TargetDialog(
                m_targets,
                m_singleSelectionMode,
                m_defaultPortNumber,
                m_canEditPortNumber,
                m_protocols);
            return dlg.ShowDialog(dialogOwner);
        }
        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            return Commands.EditTarget.Equals(commandTag);            
        }

        /// <summary>
        /// Do the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {
            if (Commands.EditTarget.Equals(commandTag))
            {
                ShowTargetDialog();
            }
        }

       
        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        /// <summary>
        /// Gets or sets the targets</summary>
        public string Targets
        {
            get
            {
                // generate xml string contain targets
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                XmlElement root = xmlDoc.CreateElement("Targets");
                xmlDoc.AppendChild(root);
                try
                {
                    foreach (Target t in m_targets.Values)
                    {
                        XmlElement elem = xmlDoc.CreateElement("Target");
                        elem.SetAttribute("name", t.Name);
                        elem.SetAttribute("host", t.Host);
                        elem.SetAttribute("protocol", t.Protocol);
                        elem.SetAttribute("port", t.Port.ToString());
                        elem.SetAttribute("selected", t.Selected.ToString());
                        root.AppendChild(elem);
                    }
                    if (xmlDoc.DocumentElement.ChildNodes.Count == 0)
                        xmlDoc.RemoveAll();
                }
                catch
                {
                    xmlDoc.RemoveAll();
                }

                return xmlDoc.InnerXml.Trim();
            }
            set
            {
                try
                {  
                    Dictionary<string, object> protocols = new Dictionary<string, object>();
                    if (m_protocols != null)
                    {
                        foreach (string p in m_protocols)
                            protocols[p] = null;
                    }                   
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(value);
                    XmlNodeList nodes = xmlDoc.DocumentElement.SelectNodes("Target");
                    if (nodes == null || nodes.Count == 0)
                        return;

                    foreach (XmlElement elem in nodes)
                    {
                        Target t = new Target(elem.GetAttribute("name"),
                            elem.GetAttribute("host"),
                            int.Parse(elem.GetAttribute("port"))); 
                        t.Selected = bool.Parse(elem.GetAttribute("selected"));
                        string protocol = elem.GetAttribute("protocol");
                        if (!String.IsNullOrEmpty(protocol) && protocols.ContainsKey(protocol))
                            t.Protocol = protocol;
                        m_targets[t.Name] = t;
                    }
                    Target trg = GetSelectedTarget();
                    if(trg != null)
                        SelectTarget(trg.Name);
                }
                catch{}
            }
        }

        /// <summary>
        /// Dictionary for target machine name string/Target pairs</summary>
        protected Dictionary<string, Target> m_targets =
            new Dictionary<string, Target>();

        private readonly ICommandService m_commandService;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;

        private bool m_singleSelectionMode = true;
        private bool m_canEditPortNumber = true;
        private int m_defaultPortNumber = -1;
        private string[] m_protocols;
        private readonly IMainWindow m_mainWindow;
        private enum Commands
        {
            EditTarget,
        }
    }
}
