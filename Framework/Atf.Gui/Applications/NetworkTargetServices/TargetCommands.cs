//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Commands to operate on currently selected targets. 
    /// Command is accessible only by right click (context menu).</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(IContextMenuCommandProvider))]
    [Export(typeof(TargetCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TargetCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
    {
   
        /// <summary>
        /// Gets or sets the command service to use</summary>
        [Import(AllowDefault = true)]
        public ICommandService CommandService { get; set; }

        [ImportMany]
        private IEnumerable<ITargetProvider> m_targetProviders = null;

        /// <summary>
        /// Gets or sets the target providers</summary>
        public IEnumerable<ITargetProvider> TargetProviders
        {
            get { return m_targetProviders; }
            set { m_targetProviders = value; }
        }

        private enum CommandTag
        {
            VitaNeighborhood,
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            if (CommandService == null)
                return;

            if (Deci4pTargetProvider.SdkInstalled)
            {
                var cmdInfo = new CommandInfo(
                    CommandTag.VitaNeighborhood,
                    null,
                    null,
                    "Edit Vita Target in Neighborhood".Localize(),
                    "Edit Vita Target in Neighborhood".Localize());
                cmdInfo.ShortcutsEditable = false;
                CommandService.RegisterCommand(cmdInfo, this);
            }

            foreach (var targetProvider in TargetProviders)
            {
                if (targetProvider.CanCreateNew)
                {
                    string addCmdTag = GetAddNewCommandName(targetProvider.Name);
                    CommandService.RegisterCommand(
                        new CommandInfo(
                            addCmdTag,
                            null,
                            null,
                            addCmdTag,
                            "Creates a new target".Localize()),
                        this);

                    m_addTargetsCmdTags.Add(addCmdTag);


                    string remCmdTag = GetRemoveCommandName(targetProvider.Name);
                    CommandService.RegisterCommand(
                       new CommandInfo(
                           remCmdTag,
                           null,
                           null,
                           remCmdTag,
                           "Remove selected target".Localize()),
                       this);

                    m_removeTargetsCmdTags.Add(remCmdTag);
                }
            }

         
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Tests if client can perform command</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can perform command</returns>
        bool ICommandClient.CanDoCommand(object commandTag)
        {
            if (m_targetProviders == null)
                return false;

            if (CommandTag.VitaNeighborhood.Equals(commandTag))
            {
                if (m_selectedTargets != null && m_selectedTargets.Any())
                {
                     return m_selectedTargets.All(target => target.Protocol == Deci4pTargetInfo.ProtocolName);
                }
              
                return false;
            }
          
            // add target 
            if (m_addTargetsCmdTags.Contains(commandTag))
            {
                foreach (var targetProvider in TargetProviders)
                {
                    string addCmdTag = GetAddNewCommandName(targetProvider.Name);
                    if (addCmdTag.Equals(commandTag))
                    {
                        return true;
                    }
                }

                return false;
            }
                

            // remove target        
            if (m_removeTargetsCmdTags.Contains(commandTag))
            {
                foreach (var targetProvider in TargetProviders)
                {
                    string remCmdTag = GetRemoveCommandName(targetProvider.Name);
                    if (remCmdTag.Equals(commandTag))
                    {
                        if (m_selectedTargets != null && m_selectedTargets.Any())
                        {
                            // whether all these selected targets are provided by this provider
                            return m_selectedTargets.All(target => targetProvider.GetTargets(m_targetConsumer).Contains(target));
                        }

                    }
                }

                return false;
            }
               
            return false;
        }

        /// <summary>
        /// Does command</summary>
        /// <param name="commandTag">Command</param>
        void ICommandClient.DoCommand(object commandTag)
        {
            if (CommandTag.VitaNeighborhood.Equals(commandTag))
            {
                // Invoke "Edit Vita Target in Neighborhood" command merely launches the PSP2 Neighborhood app that comes with  Vita SDK installer, 
                // as if you double click "Neighborhood for PlayStation(R)Vita" on your desktop icon. 
                // This is intended just for a convenience helper to allow users directly bring up the PSP2 app without a detour to desktop first, 
                // as PSP2 Neighborhood app can reboot, power off, and do much more for the Vita kit.

                // {BA414141-28C6-7F3C-45FF-14C28C11EE88} is the registered Neighborhood for PlayStation(R)Vita Shell extension 
                System.Diagnostics.Process.Start("Explorer.exe", @"/e,/root,::{BA414141-28C6-7F3C-45FF-14C28C11EE88}");
            }
            else if (m_addTargetsCmdTags.Contains(commandTag))
            {
                foreach (var targetProvider in TargetProviders)
                {
                    string addCmdTag = GetAddNewCommandName(targetProvider.Name);
                    if (addCmdTag.Equals(commandTag))
                    {
                        targetProvider.AddTarget(targetProvider.CreateNew());
                        break;
                    }
                }
            }
            else if (m_removeTargetsCmdTags.Contains(commandTag))
            {

                foreach (var item in m_selectedTargets)
                    foreach (var provider in TargetProviders)
                        provider.Remove(item);
            }
        }

        /// <summary>
        /// Updates command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command state</param>
        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
            if (m_removeTargetsCmdTags.Contains(commandTag) && m_selectedTargets != null && m_selectedTargets.Any())
            {
                commandState.Text = GetRemoveCommandName(m_selectedTargets.First().Name);
            }
        }

        #endregion

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets enumeration for tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="selectedTargets">Right clicked object, or null if none</param>
        /// <returns>Enumeration for command tags</returns>
        IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object selectedTargets)
        {
            m_selectedTargets = null;
            if (context.Is<ITargetConsumer>())
            {
                m_targetConsumer = context.Cast<ITargetConsumer>();
                m_selectedTargets = selectedTargets as IEnumerable<TargetInfo>;
                foreach (var cmdTag in m_addTargetsCmdTags)
                    yield return cmdTag;

                if (m_selectedTargets != null && m_selectedTargets.Any())
                    foreach (var cmdTag in m_removeTargetsCmdTags)
                        yield return cmdTag;

                yield return CommandTag.VitaNeighborhood;
            }                
          
        }

        #endregion

        private string GetAddNewCommandName(string targetName)
        {
            return string.Format("Add New {0}".Localize(), targetName);
        }

        private string GetRemoveCommandName(string targetName)
        {
            return string.Format("Remove {0}".Localize(), targetName);
        }

        private List<object> m_addTargetsCmdTags = new List<object>();
        private List<object> m_removeTargetsCmdTags = new List<object>();

        private ITargetConsumer m_targetConsumer;
        private IEnumerable<TargetInfo> m_selectedTargets;

      }

 
}
