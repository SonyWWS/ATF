//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Listens for Open Sound Control messages that can trigger the execution of commands
    /// in this application. The format for the OSC addresses are /{menu name}/{command name}
    /// with spaces and illegal characters removed. (See IOscService.)
    /// 
    /// In the TypeCatalog, this OscCommandReceiver component can only see commands that were
    /// previously registered with the ICommandService. So, in order to see all commands, put
    /// this component last in the TypeCatalog.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(OscCommandReceiver))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OscCommandReceiver : IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">The command service used to find existing commands. Currently, it
        /// must be a CommandServiceBase type of object</param>
        /// <param name="oscService">The Open Sound Control service used to receive OSC messages</param>
        [ImportingConstructor]
        public OscCommandReceiver(ICommandService commandService, IOscService oscService)
        {
            //We only currently work with CommandServiceBase since there's no other way to
            //  get the registered CommandInfo objects. Maybe create an ICommandInfoProvider?
            m_commandService = (CommandServiceBase)commandService;
            m_oscService = oscService;
        }

        /// <summary>
        /// Initializes this component</summary>
        public virtual void Initialize()
        {
            foreach (CommandInfo cmdInfo in m_commandService.GetCommandInfos())
            {
                string oscAddress = '/' + m_commandService.GetCommandPath(cmdInfo);
                oscAddress = OscServices.FixPropertyAddress(oscAddress);
                m_addressesToCommands.Add(oscAddress, cmdInfo);
            }

            m_oscService.MessageReceived += OscServiceMessageReceived;
        }

        /// <summary>
        /// Gets the OSC addresses that can trigger commands in this service</summary>
        /// <returns></returns>
        public IEnumerable<string> GetOscAddresses()
        {
            return m_addressesToCommands.Keys;
        }
        
        private void OscServiceMessageReceived(object sender, OscMessageReceivedArgs e)
        {
            CommandInfo cmdInfo;
            if (m_addressesToCommands.TryGetValue(e.Address, out cmdInfo))
            {
                ICommandClient cmdClient = m_commandService.GetClient(cmdInfo.CommandTag);
                if (cmdClient != null)
                {
                    if (cmdClient.CanDoCommand(cmdInfo.CommandTag))
                        cmdClient.DoCommand(cmdInfo.CommandTag);
                }
                e.Handled = true;
            }
        }

        private readonly CommandServiceBase m_commandService;
        private readonly IOscService m_oscService;
        private readonly Dictionary<string, CommandInfo> m_addressesToCommands = new Dictionary<string, CommandInfo>();
    }
}
