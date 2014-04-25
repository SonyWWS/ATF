//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component that enforces the single-instance constraint on a GUI application. By default, the
    /// product name, product version, and user name are used to identify this application. If another
    /// ATF application is running that has the same ID, then this newer process is exited.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(SingleInstanceService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SingleInstanceService : MarshalByRefObject, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        [ImportingConstructor]
        public SingleInstanceService()
            : this(Application.ProductName + '-' + Application.ProductVersion + '-' + Environment.UserName)
        {
        }

        /// <summary>
        /// Constructor for applications that need a custom application ID which will be used to
        /// identify if another instance of this application is already running.</summary>
        /// <param name="applicationId">The string used to identify this process. If another running process
        /// uses the same application ID, then this process will be shut down.</param>
        public SingleInstanceService(string applicationId)
        {
            // This method creates additional unwanted directories.
            //string applicationId = Application.UserAppDataPath.Replace(@"\", "-");

            //255 characters will break IpcChannel constructor. 250 works. Take the first 250 characters since
            //  the product name should be the most unique part of the mutex's name.
            if (applicationId.Length > 250)
                applicationId = applicationId.Substring(0, 250);

            // The Mutex constructor will crash if given '\', unless it's part of a valid path.
            // RemotingServices.Connect() will crash if there are '/' characters.
            applicationId = applicationId.Replace('/', '-');
            applicationId = applicationId.Replace('\\', '-');

            // if this is a single instance application, and another instance is running, exit in constructor
            bool gotMutex;
            m_mutex = new Mutex(true, applicationId, out gotMutex);
            if (gotMutex) // first instance
            {
                // registering IPC channel
                var ipcChannel = new IpcChannel(applicationId);
                ChannelServices.RegisterChannel(ipcChannel, false);
                RemotingServices.Marshal(this, "SingleInstanceService");
            }
            else
            {
                string url = "ipc://" + applicationId + "/" + "SingleInstanceService";

                //the instance could be quitting or have quited between the mutex check and connection,
                //  especially during debug session
                try
                {
                    SingleInstanceService singleInstanceService =
                        (SingleInstanceService)RemotingServices.Connect(typeof(SingleInstanceService), url);

                    // pass this invocation's command line to running single instance
                    singleInstanceService.CommandLine = Environment.GetCommandLineArgs();
                }
                catch
                {
                }

                // exit in constructor, which will exit after composition but before any components are initialized
                Environment.Exit(-1);
            }
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component</summary>
        public void Initialize()
        {
            // this will raise the CommandLineChanged event; any subscribers will get
            //  the initial command line and any command lines from subsequent invocations
            //  of the app.
            CommandLine = Environment.GetCommandLineArgs();
        }

        #endregion

        /// <summary>
        /// Gets or sets the command line. Setting the command line to a different value
        /// raises the CommandLineChanged event.</summary>
        /// <remarks>This property should not be set by the user. It is public so a remote
        /// instance of the service can pass its command line to the first running instance.</remarks>
        public string[] CommandLine
        {
            get
            {
                return m_commandLine;
            }
            set
            {
                //Note that this isn't comparing the elements of the array, just that the two objects are different.
                // For example, the CommandLineChanged event is raised each time another instances of the app launches.
                // I think this is desirable behavior, so that the app can know about this, and can, for example,
                //  bring itself to the forefront.
                if (value != null && value != m_commandLine)
                {
                    m_commandLine = value;
                    CommandLineChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Event that is raised after the command line changes. This event is raised every
        /// time another instance of this application launches.</summary>
        public event EventHandler CommandLineChanged;

        /// <summary>
        /// The default is to have a lease time of 5 minutes. This is apparently way too short and is the
        /// cause of the "Requested Service not found" exception when another instance of this app starts
        /// and communicates with this app. So, let's set the initial lease time to be forever.
        /// http://stackoverflow.com/questions/2651852/requested-service-not-found </summary>
        /// <returns>An ILease object</returns>
        public override object InitializeLifetimeService()
        {
            ILease lease = (ILease)base.InitializeLifetimeService();
            if (lease.CurrentState == LeaseState.Initial)
                lease.InitialLeaseTime = TimeSpan.Zero; //means "forever"
            return lease;
        }

        private Mutex m_mutex; //we can't dispose of this until our app shuts down
        private string[] m_commandLine;
    }
}
