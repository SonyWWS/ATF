//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml;


using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Provides information about development devices (targets) available for TCP/IP</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ITargetProvider))]
    [Export(typeof(TcpIpTargetProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TcpIpTargetProvider : ITargetProvider, IInitializable
    {
        #region  ITargetProvider members

        /// <summary>
        /// Gets the provider's user-readable name</summary>
        public virtual string Name { get { return @"TCP Target".Localize(); } }

        /// <summary>
        /// Retrieves the targets' data</summary>
        /// <param name="targetConsumer">The target consumer to retrieve the data for</param>
        /// <returns>The targets to consume on the target consumer</returns>
        public IEnumerable<TargetInfo> GetTargets(ITargetConsumer targetConsumer)
        {
            foreach (var target in m_targets)
                yield return target;
        }

        /// <summary>
        /// Gets whether you can create a new target using the CreateNew method</summary>
        public bool CanCreateNew { get { return true; } }

        /// <summary>
        /// Creates a new target</summary>
        /// <remarks>Creates and returns a TargetInfo, but does not add it to the watched list</remarks>
        /// <returns>New target</returns>
        public virtual TargetInfo CreateNew()
        {
            var newTarget = new TcpIpTargetInfo();
            return newTarget;
        }

        /// <summary>
        /// Adds the target to the provider</summary>
        /// <param name="target">Target to add to the provider</param>
        /// <returns><c>True</c> if the target is successfully added</returns>
        public bool AddTarget(TargetInfo target)
        {
            if (target is TcpIpTargetInfo && !m_targets.Contains(target))
            {
                m_targets.Add(target);
                foreach (var targetConsumer in TargetConsumers)
                    targetConsumer.TargetsChanged(this, m_targets);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the target from the provider</summary>
        /// <param name="target">Target to remove from the provider</param>
        /// <returns><c>True</c> if the target is successfully removed</returns>
        public bool Remove(TargetInfo target)
        {
            var tcpTarget = m_targets.FirstOrDefault(n =>  n == target);
            if (tcpTarget != null)
            {
                m_targets.Remove(tcpTarget);
                foreach (var targetConsumer in TargetConsumers)
                    targetConsumer.TargetsChanged(this, m_targets);
                return true;
            }
            return false;
        }

        #endregion

        #region IInitializable Members

        void IInitializable.Initialize()
        {
         
            if (m_settingsService != null)
            {
                m_settingsService.Saving += settingsService_Saving;
                m_settingsService.Reloaded += settingsService_Reloaded;
                m_settingsService.RegisterSettings(this,
                    new BoundPropertyDescriptor(this, () => PersistedTargets, "Targets".Localize(), null, null)
                );
              
            }
        }
        #endregion

        /// <summary>
        /// Gets the identifier of the provider</summary>
        /// <returns>A string that contains the identifier.</returns>
        public virtual string Id { get { return @"Sce.Atf.TcpIpTargetProvider"; } }


#pragma warning disable 649 // Field is never assigned to and will always have its default value

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;

#pragma warning restore 649

        /// <summary>
        /// Performs actions when targets reloaded event occurs</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        protected void settingsService_Reloaded(object sender, EventArgs e)
        {
            if (m_targetsLoaded)
                return;
            string settingsPath=string.Empty;

            try
            {
                string perUserSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SCE\\ATF\\TargetSettings.xml");
                string allUsersSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "SCE\\ATF\\TargetSettings.xml");

                var loadingStages = new[]
                {
                    new 
                    {
                        SettingsPath = perUserSettingsPath,
                        Scope = TargetScope.PerUser
                    }, 

                    new  
                    {
                        SettingsPath = allUsersSettingsPath,
                        Scope = TargetScope.AllUsers
                    },                          
                };

                foreach (var stage in loadingStages)
                {
                    settingsPath = stage.SettingsPath;
                    XmlElement root = null;
                    if (File.Exists(settingsPath))
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.Load(settingsPath);
                        root = xmlDoc.SelectSingleNode("Targets") as XmlElement;

                        if (root == null)
                            continue;


                         DeserializeTargets(xmlDoc.InnerXml, stage.Scope);

                    }

                }

                m_targetsLoaded = true;
            }
            catch (Exception ex)
            {
                Outputs.Write(OutputMessageType.Error, settingsPath + ": ");
                Outputs.WriteLine(OutputMessageType.Error, ex.Message);
            }

        }

        /// <summary>
        /// Performs actions when save per-user and all-users targets occurs</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        protected void settingsService_Saving(object sender, EventArgs e)
        {
            string settingsPath = string.Empty;
            try
            {
                string perUserSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SCE\\ATF\\TargetSettings.xml");
                string allUsersSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "SCE\\ATF\\TargetSettings.xml");

                var savingStages = new[]
                {
                    new 
                    {
                        SettingsPath = perUserSettingsPath,
                        Scope = TargetScope.PerUser
                    }, 

                    new  
                    {
                        SettingsPath = allUsersSettingsPath,
                        Scope = TargetScope.AllUsers
                    },                          
                };

                foreach (var stage in savingStages)
                {
                    settingsPath = stage.SettingsPath;
                    string settingsDirectory = Path.GetDirectoryName(settingsPath);
                    if (!Directory.Exists(settingsDirectory))
                        Directory.CreateDirectory(settingsDirectory);

                    XmlElement targetsNode = null;
                    var xmlDoc = new XmlDocument();
                    if (File.Exists(settingsPath))
                    {
                        xmlDoc.Load(settingsPath);
                        targetsNode = xmlDoc.SelectSingleNode("Targets") as XmlElement;
                    }

                    else
                    {
                        xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                        targetsNode = xmlDoc.CreateElement("Targets");
                        xmlDoc.AppendChild(targetsNode);
                    }
                    if (targetsNode == null)
                        return;                 

                    // remove all existing targets provided by this provider
                    var targetsToRemove = new List<XmlNode>();
                    var tcpTargets = targetsNode.SelectSingleNode("TcpTargets");
                    if (tcpTargets != null)
                    {
                        foreach (XmlNode child in tcpTargets.ChildNodes)
                        {
                            var elem = child as XmlElement;
                            if (elem != null)
                            {
                                var providerIdentifier = elem.GetAttribute("provider");
                                if (providerIdentifier == Id)
                                    targetsToRemove.Add(child);
                            }
                        }
                        foreach (var target in targetsToRemove)
                            tcpTargets.RemoveChild(target);
                    }
                    else
                    {
                        tcpTargets = xmlDoc.CreateElement("TcpTargets");
                        targetsNode.AppendChild(tcpTargets);
                    }
 
                    string results = SerializeTargets(stage.Scope);                                 
                    var tempRootNode = xmlDoc.CreateElement("TcpTargets");
                    tempRootNode.InnerXml = results;
                    if (tempRootNode.FirstChild != null)
                        foreach (XmlNode child in tempRootNode.FirstChild.ChildNodes)
                        {

                            var target = child.CloneNode(false);
                            tcpTargets.AppendChild(target);
                        }
                          
                    xmlDoc.Save(settingsPath);
                }
            }
            catch (Exception ex)
            {
                Outputs.Write(OutputMessageType.Error, settingsPath + ": ");
                Outputs.WriteLine(OutputMessageType.Error, ex.Message);
            }

        }

        /// <summary>
        /// Gets or sets persisted targets</summary>
        public string PersistedTargets
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
                    string results = SerializeTargets(TargetScope.PerApp);

                    root.InnerXml = results;
                    if (xmlDoc.DocumentElement.ChildNodes.Count == 0)
                        xmlDoc.RemoveAll();
                }
                catch
                {
                    xmlDoc.RemoveAll();
                }

                return xmlDoc.InnerXml;
            }

            set
            {
                try
                {
                    DeserializeTargets(value, TargetScope.PerApp);

                }
                catch { }
            }
        }

       
        /// <summary>
        /// Converts targets of the given scope into a string value for persisting</summary>
        /// <param name="scope">Target scope</param>
        /// <returns>String representation of targets</returns>
        protected virtual string SerializeTargets( TargetScope scope)
        {
            XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
            XmlElement root = xmlDoc.CreateElement("TcpTargets");
            xmlDoc.AppendChild(root);
            try
            {
                foreach (TcpIpTargetInfo target in m_targets)
                {
                    if (target.Scope != scope)
                        continue;

                    XmlElement elem = xmlDoc.CreateElement("TcpTarget");
                    elem.SetAttribute("name", target.Name);
                    elem.SetAttribute("platform", target.Platform);
                    elem.SetAttribute("endpoint", target.Endpoint);
                    elem.SetAttribute("protocol", target.Protocol);
                    if (scope != TargetScope.PerApp)
                        elem.SetAttribute("provider", Id);
                    if (target.FixedPort > 0)
                        elem.SetAttribute("fixedport", target.FixedPort.ToString());
                     
                    root.AppendChild(elem);
                }

                if (xmlDoc.DocumentElement.ChildNodes.Count == 0)
                    xmlDoc.RemoveAll();
            }
            catch
            {
                xmlDoc.RemoveAll();
            }

            return xmlDoc.InnerXml;
        }

        /// <summary>
        /// Deserializes target objects from a string value</summary>
        /// <param name="value">String representation of targets</param>
        /// <param name="scope">Target scope</param>
        protected virtual void DeserializeTargets(string value, TargetScope scope)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(value);
            XmlNodeList nodes = xmlDoc.DocumentElement.SelectNodes("TcpTargets/TcpTarget");
            if (nodes == null || nodes.Count == 0)
                return;

            foreach (XmlElement elem in nodes)
            {
               
                if (scope != TargetScope.PerApp)
                {
                    var provider = elem.GetAttribute("provider");
                    if (provider != Id)
                        continue;
                }
                var tcpTarget = new TcpIpTargetInfo
                                    {
                                        Name = elem.GetAttribute("name"),
                                        Platform = elem.GetAttribute("platform"),
                                        Endpoint = elem.GetAttribute("endpoint"),
                                        Protocol = elem.GetAttribute("protocol"),
                                        Scope = scope,
                                    };
                int fixedPort = 0;
                if (int.TryParse(elem.GetAttribute("fixedport"), out fixedPort))
                {
                    tcpTarget.FixedPort = fixedPort;
                }
                m_targets.Add(tcpTarget);
            }

            foreach (var targetConsumer in TargetConsumers)
                targetConsumer.TargetsChanged(this, m_targets);
 
        }
     
        /// <summary>
        /// Gets or sets target consumers</summary>
        [ImportMany]
        protected IEnumerable<ITargetConsumer> TargetConsumers { get; set; }

        private List<TargetInfo> m_targets = new List<TargetInfo>();
        private bool m_targetsLoaded;
    }

    /// <summary>
    /// Provides information about development devices (targets) available for X86</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ITargetProvider))]
    [Export(typeof(X86TargetProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class X86TargetProvider : TcpIpTargetProvider
    {
        /// <summary>
        /// Gets the provider's user-readable name</summary>
        public override string Name { get { return @"X86 Target".Localize(); } }

        /// <summary>
        /// Gets identifier of provider</summary>
        /// <returns>String that contains identifier</returns>
        public override string Id { get { return @"Sce.Atf.X86TcpIpTargetProvider"; } }


        /// <summary>
        /// Creates a new target</summary>
        /// <remarks>Creates and returns a TargetInfo, but does not add it to the watched list</remarks>
        /// <returns>TargetInfo for new target</returns>
        public override TargetInfo CreateNew()
        {
            var newTarget = new X86TargetInfo();
            return newTarget;
        }
    }

    /// <summary>
    /// Provides information about development devices (targets) available for PS3</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ITargetProvider))]
    [Export(typeof(Ps3TargetProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Ps3TargetProvider : TcpIpTargetProvider
    {
        /// <summary>
        /// Gets the provider's user-readable name</summary>
        public override string Name { get { return "PS3 Target".Localize(); } }

        /// <summary>
        /// Gets identifier of provider</summary>
        /// <returns>String that contains identifier</returns>
        public override string Id { get { return @"Sce.Atf.Ps3TcpIpTargetProvider"; } }


        /// <summary>
        /// Creates a new target</summary>
        /// <remarks>Creates and returns a TargetInfo, but does not add it to the watched list</remarks>
        /// <returns>TargetInfo for new target</returns>
        public override TargetInfo CreateNew()
        {
            var newTarget = new Ps3TargetInfo();
            return newTarget;
        }
    }

    /// <summary>
    /// Provides information about development devices (targets) available for PS4</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ITargetProvider))]
    [Export(typeof(Ps4TargetProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Ps4TargetProvider : TcpIpTargetProvider
    {
        /// <summary>
        /// Gets provider's user-readable name</summary>
        public override string Name { get { return "PS4 Target".Localize(); } }

        /// <summary>
        /// Gets identifier of provider</summary>
        /// <returns>String that contains identifier</returns>
        public override string Id { get { return @"Sce.Atf.Ps4TcpIpTargetProvider"; } }


        /// <summary>
        /// Creates a new target</summary>
        /// <remarks>Creates and returns a TargetInfo, but does not add it to the watched list</remarks>
        /// <returns>TargetInfo for new target</returns>
        public override TargetInfo CreateNew()
        {
            var newTarget = new Ps4TargetInfo();
            return newTarget;
        }
    }
 
}
