//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Standard component implementation of ITargetService</summary>
    [Export(typeof(ITargetService))]
    [Export(typeof(TargetService))]
    [Export(typeof(ICommandClient))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TargetService : ITargetService, ICommandClient, IInitializable
    {
        enum Command
        {
            EditTargets
        }

        [ImportMany]
        private IEnumerable<Lazy<IProtocol>> m_protocols = null;

        [Import(AllowDefault = true)]
        private ICommandService m_commandService = null;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService = null;

        #region IInitializable Members

        /// <summary>
        /// Finish initializing component by registering settings and commands</summary>
        public void Initialize()
        {
            if (m_settingsService != null)
            {
                // create setting for storing targets
                m_settingsService.RegisterSettings(
                   this,
                   new BoundPropertyDescriptor[]
                        {
                            new BoundPropertyDescriptor(this, () => TargetsAsCsv, "Targets".Localize(), null, null),
                        });
            }

            if (m_commandService != null)
            {
                m_commandService.RegisterCommand(
                        Command.EditTargets,
                        StandardMenu.Edit,
                        StandardCommandGroup.EditPreferences,
                        "_Targets".Localize() + "...",
                        "Edit Targets".Localize(),
                        Keys.None,
                        null,
                        CommandVisibility.Menu,
                        this);
            }
        }

        #endregion

        #region ITargetService Members

        /// <summary>
        /// Get enumeration of protocols supported</summary>
        public IEnumerable<IProtocol> Protocols
        {
            get { return m_protocols.GetValues<IProtocol>(); }
        }

        /// <summary>
        /// Get all available targets
        /// </summary>
        public IEnumerable<ITarget> Targets
        {
            get { return m_targets.Select<TargetViewModel, ITarget>(x => x.Target); }
        }

        /// <summary>
        /// Get selected targets
        /// </summary>
        public IEnumerable<ITarget> SelectedTargets
        {
            get
            {
                return 
                    m_targets.Where<TargetViewModel>(x => x.IsSelected)
                             .Select<TargetViewModel, ITarget>(x => x.Target);
            }
        }

        /// <summary>
        /// Get an array of selected targets
        /// </summary>
        public ITarget SelectedTarget
        {
            get
            {
                var selected = m_targets.FirstOrDefault(x => x.IsSelected);
                return selected != null ? selected.Target : null;
            }
            set
            {
                if (value != null)
                {
                    if (!m_targets.Select(x => x.Target).Contains(value))
                    {
                        var protocol = Protocols.FirstOrDefault(x => x.Id == value.ProtocolId);
                        if(protocol == null)
                            throw new InvalidOperationException("Could not find Protocol for slected Target");

                        m_targets.Add(new TargetViewModel(value, protocol));
                    }
                }

                foreach (var target in m_targets)
                {
                    target.IsSelected = target.Target.Equals(value);
                }
            }
        }

        /// <summary>
        /// Select a target, given its name</summary>
        /// <exception cref="ArgumentException"> if the parameter is invalid or the target not found</exception>
        /// <param name="target">Name of the target to be selected</param>
        public void SelectTarget(ITarget target)
        {
            Requires.NotNull(target, "target");

            var vm = m_targets.FirstOrDefault<TargetViewModel>(x => x.Target == target);
            if (vm == null)
                throw new ArgumentException("Target not found");

            foreach (var t in m_targets)
                t.IsSelected = false;

            vm.IsSelected = true;
        }

        /// <summary>
        /// Show target dialog</summary>
        /// <returns>Nullable Boolean signifying how window was closed by user</returns>
        public bool? ShowTargetDialog()
        {
            var tvm = new TargetDialogViewModel(m_targets, m_protocols.GetValues<IProtocol>());
            bool? result = DialogUtils.ShowDialogWithViewModel<TargetDialog>(tvm);

            if (result.HasValue && result.Value)
            {
                m_targets.Clear();
                m_targets.AddRange(tvm.Targets);
            }

            return result;
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command if it handles it</summary>
        /// <param name="tag">Command to be done</param>
        /// <returns><c>True</c> if client can do the command</returns>
        public bool CanDoCommand(object tag)
        {
            return tag is Command;
        }

        /// <summary>
        /// Do the command</summary>
        /// <param name="tag">Command to be done</param>
        public void DoCommand(object tag)
        {
            if (tag is Command)
            {
                switch ((Command)tag)
                {
                    case Command.EditTargets:
                        ShowTargetDialog();
                        break;
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command state to update</param>
        public void UpdateCommand(object commandTag, Sce.Atf.Applications.CommandState commandState) { }

        #endregion

        /// <summary>
        /// Internal use only
        /// </summary>
        public TargetViewModel[] SerializableTargets
        { 
            get 
            {
                return m_targets.Where<TargetViewModel>(x => x.Target.GetType().IsSerializable).ToArray<TargetViewModel>();
            }
            set 
            { 
                m_targets = new List<TargetViewModel>();

                // Try and match up deserialized targets with protocols
                foreach(var target in value)
                {
                    var protocol = m_protocols.GetValues<IProtocol>().FirstOrDefault<IProtocol>(x => x.Id == target.Target.ProtocolId);

                    // Ignore targets with no matching protocol
                    if (protocol != null)
                    {
                        target.Protocol = protocol;
                        m_targets.Add(target);
                    }
                }
            }
        }

        /// <summary>
        /// Internal use only
        /// </summary>
        public string TargetsAsCsv
        {
            get
            {
                string result = null;
                using (var stream = new MemoryStream())
                {
                    var writer = XmlWriter.Create(stream);

                    writer.WriteStartElement("Targets");

                    foreach (TargetViewModel t in m_targets)
                    {
                        IXmlSerializable ts = t.Target as IXmlSerializable;
                        if (ts == null)
                            continue;

                        Type type = t.Target.GetType();
                        writer.WriteStartElement("Target");
                        {
                            writer.WriteAttributeString("selected", t.IsSelected.ToString());

                            writer.WriteStartElement(type.Name, type.Namespace);
                            {
                                ts.WriteXml(writer);
                                writer.WriteEndElement();
                            }
                            
                            writer.WriteEndElement();
                        }
                    }

                    writer.WriteEndElement();
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    result = new StreamReader(stream).ReadToEnd();
                }
                
                return result;
            }
            set
            {
                try
                {
                    using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(value)))
                    {
                        using (var reader = XmlReader.Create(stream))
                        {
                            while (reader.ReadToFollowing("Target"))
                            {
                                string selected = reader.GetAttribute("selected");

                                reader.ReadStartElement();
                                
                                var fullName = reader.NamespaceURI + "." + reader.LocalName;
                                var type = FindType(fullName);
                                if (type == null)
                                    continue;

                                var content = Activator.CreateInstance(type) as IXmlSerializable;
                                if (content != null)
                                {
                                    var itarget = content as ITarget;
                                    
                                    content.ReadXml(reader);
                                    
                                    var target = new TargetViewModel(
                                        itarget,
                                        m_protocols.GetValues<IProtocol>().FirstOrDefault<IProtocol>(x => x.Id == itarget.ProtocolId));
                                    
                                    if(SelectedTarget == null && String.Compare(selected, "True", StringComparison.OrdinalIgnoreCase) == 0)
                                        target.IsSelected = true;

                                    m_targets.Add(target);
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Find Type for its name</summary>
        /// <param name="typeName">Type name</param>
        /// <returns>Type corresponding to name</returns>
        public static Type FindType(string typeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(typeName))
                .FirstOrDefault(foundType => foundType != null);
        }

        private List<TargetViewModel> m_targets = new List<TargetViewModel>();
    }
}
