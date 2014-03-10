//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

//#define USE_TEST_DATA

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;


using Sce.Atf.Applications.NetworkTargetServices.Psp2TmApilib;

namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Provides information about development devices available via Deci4p</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ITargetProvider))]
    [Export(typeof(Deci4pTargetProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Deci4pTargetProvider : ITargetProvider, IInitializable, IDisposable
    {
        /// <summary>
        /// Gets whether VITA SDK is installed</summary>
        static public bool SdkInstalled
        { 
            get
            {
               return Type.GetTypeFromProgID("psp2tmapi.PSP2TMAPI") != null;
            }
        }

        /// <summary>
        /// Gets or sets whether the Deci4p target provider is enabled</summary>
        public bool Enabled
        {
            get { return m_enabled; }
            set { m_enabled = value; }
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_worker = new BackgroundWorker();
            m_worker.WorkerSupportsCancellation = true;
            m_worker.DoWork += BgwDoWork;
            m_worker.RunWorkerCompleted += BgwRunWorkerCompleted;
            m_worker.RunWorkerAsync(this);

            m_timer = new Timer(TimerTick, "Deci4pTicker", 1000, 1000);
        
        }

        #endregion

        /// <summary>
        /// Called when background work begins</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void BgwDoWork(object sender, DoWorkEventArgs e)
        {
            var bgw = sender as BackgroundWorker;
            var result = new List<TargetInfo>();
            result.AddRange(FindTargets());
            e.Result = result;                                  
        }

        /// <summary>
        /// Called when background work ends</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void BgwRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
  
            if (!e.Cancelled && e.Error == null && e.Result is List<TargetInfo>)
            {
                var targets = e.Result as List<TargetInfo>;

                // check if the results have changed from last poll
                bool changed = m_targets.Count != targets.Count;
                if (!changed)
                {
                    foreach (var newTarget in targets)
                    {
                        var matched = m_targets.Find(n => n.Endpoint == newTarget.Endpoint && n.Name == newTarget.Name &&
                                                          n.Protocol == newTarget.Protocol && n.Scope == newTarget.Scope &&
                                                          n.Platform == newTarget.Platform);
                        if (matched == null)
                        {
                            changed = true;
                            break;
                        }
                    }
                }

                if (changed)
                {
                    m_targets.Clear();
                    m_targets.AddRange(targets);
                    foreach (var consumer in TargetConsumers)
                    {
                        consumer.TargetsChanged(this, targets);
                    }
            
                }            
            }
  
         }


        #region  ITargetProvider members

        /// <summary>
        /// Gets the provider's user-readable name</summary>
        public string Name { get { return "Vita Target".Localize(); } }

        /// <summary>
        /// Gets whether you can create a new target using the CreateNew method</summary>
        public bool CanCreateNew { get { return false; } }

        /// <summary>
        /// Creates a new target</summary>
        /// <returns>Nothing</returns>
        /// <remarks>Always throws InvalidOperationException</remarks>
        public TargetInfo CreateNew()
        {
            throw new InvalidOperationException("Vita targets can only be discovered!");
        }

        /// <summary>
        /// Adds the target to the provider</summary>
        /// <param name="target">Target</param>
        /// <returns>True iff the target is successfully added</returns>
        public bool AddTarget(TargetInfo target)
        {
            return false;
        }

        /// <summary>
        /// Removes the target from the provider</summary>
        /// <param name="target">Target</param>
        /// <returns>True iff the target is successfully removed</returns>
        public bool Remove(TargetInfo target)
        {
            return false;
        }

        /// <summary>
        /// Retrieves enumeration of the targets' data</summary>
        /// <param name="targetConsumer">The target consumer to retrieve the data for</param>
        /// <returns>Enumeration of the targets to consume on the target consumer</returns>
        public IEnumerable<TargetInfo> GetTargets(ITargetConsumer targetConsumer)
        {         
                foreach (var target in m_targets)
                    yield return target;
        }


        #endregion

        #region IDisposable Interface

        /// <summary>
        /// Disposes of unmanaged resources</summary>
        public void Dispose()
        {
            try
            {
                lock (s_lock)
                {
                    if (m_tmApi != null)
                    {
                        m_tmApi.Dispose();
                        m_tmApi = null;
                    }

                    if (m_worker != null)
                    {
                        m_worker.CancelAsync();
                        m_worker.DoWork -= BgwDoWork;
                        m_worker.RunWorkerCompleted -= BgwRunWorkerCompleted;
                        m_worker = null;
                    }

                    if (m_timer != null)
                    {
                        m_timer.Dispose();
                        m_timer = null;
                    }
                }            
              
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(OutputMessageType.Error, ex.Message);               
            }
        }

        #endregion

        private IEnumerable<TargetInfo> FindTargets()
        {
            
            if (!m_initialized)
            {
                Type psp2TmType = Type.GetTypeFromProgID("psp2tmapi.PSP2TMAPI");
                if (psp2TmType != null)
                {

                    object tmInstance = Activator.CreateInstance(psp2TmType);
                    m_tmApi = (IPsp2TmApi)tmInstance;

                    const uint buildVersion = 18; //TODO: either user changable(such as settings) or use reflection to extract? 
                    m_tmApi.CheckCompatibility(buildVersion);
                }                     
                m_initialized = true;
                if (m_tmApi == null)
                    Dispose();
            }
#if USE_TEST_DATA
            return CreateTestData(); 
#else
            if (Enabled)
            {
                lock (s_lock)
                {
                    if (m_tmApi != null)
                    {
                        foreach (ITarget target in m_tmApi.Targets)
                        {
                            var psp2Target = new Deci4pTargetInfo()
                                                 {
                                                     Name = target.Name,
                                                     Platform = Deci4pTargetInfo.PlatformName,
                                                     Endpoint = target.HardwareId,
                                                     Protocol = Deci4pTargetInfo.ProtocolName,
                                                     Scope = TargetScope.PerUser,
                                                 };
                            yield return psp2Target;
                        }
                    }
                }
            }

#endif
        }

        // According to research by Randon Ehle, reported by email on 5/7/2012:
        // http://msdn.microsoft.com/en-us/library/h01xszh2.aspx
        // The RunWorkerAsync method submits a request to start the operation running asynchronously.
        // When the request is serviced, the DoWork event is raised, which in turn starts execution of
        // your background operation. If the background operation is already running, calling RunWorkerAsync
        // again will raise an InvalidOperationException.
        // http://msdn.microsoft.com/en-us/library/system.threading.timer.aspx
        // The callback method executed by the timer should be reentrant, because it is called on
        // ThreadPool threads. The callback can be executed simultaneously on two thread pool threads
        // if the timer interval is less than the time required to execute the callback, or if all
        // thread pool threads are in use and the callback is queued multiple times.
        private void TimerTick(object data)
        {
            if (m_worker == null)
                return;

            if (Monitor.TryEnter(m_worker))
            {
                try
                {
                    if (!m_worker.IsBusy)
                        m_worker.RunWorkerAsync(this);
                }
                finally
                {
                    Monitor.Exit(m_worker);
                }
            }
        }

#if USE_TEST_DATA
        private IEnumerable<TargetInfo> CreateTestData()
        {
            const int numTargets = 3;
            var targets = new List<TargetInfo>();
            for (int i = 0; i < numTargets; ++i)
            {
                var target = new Deci4pTargetInfo
                {
                    Name = "MyVitaKit" + (numTargets - i).ToString(),
                    Platform = Deci4pTargetInfo.PlatformName,
                    Endpoint = "Pb26aad004512671032473144150100d",
                    Protocol = Deci4pTargetInfo.ProtocolName,
                    Scope = TargetScope.PerUser,
                };
                targets.Add(target);
            }

            return targets;
        }
#endif

        [ImportMany(typeof(ITargetConsumer))]      
        protected IEnumerable<ITargetConsumer> TargetConsumers { get; set; }


        private BackgroundWorker m_worker;

        private IPsp2TmApi m_tmApi;
        private bool m_initialized;
        private List<TargetInfo> m_targets = new List<TargetInfo>();
        private Timer m_timer;
        private bool m_enabled = true;

        private static readonly object s_lock = new object(); // to control access to m_tmApi
    }

    /// <summary>
    /// Information about Deci4p target</summary>
    [GroupAttribute("Deci4pTargetInfo", Header = "Vita Targets", ExternalEditorProperties = "Name,Platform,Endpoint,Protocol,Scope")]
    public class Deci4pTargetInfo : TargetInfo
    {
        public const string PlatformName = @"Vita";
        public const string ProtocolName = @"Deci4p";
    }

    // COM Interop for Psp2 TMAPI (late-binding)
    namespace Psp2TmApilib
    {
        [TypeConverter(typeof(PowerStatusEnumConverter))]
        [Guid("FBED4567-ABF5-4A96-9D9A-45017FA203A6")]
        internal enum ePowerStatus
        {
            POWER_STATUS_OFF = 0,
            POWER_STATUS_NO_SUPPLY = 1,
            POWER_STATUS_ON = 256,
            POWER_STATUS_SUSPENDED = 512,
        }

        [Guid("2296BA32-7E8B-46C8-838D-290A03094819")]
        [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        internal interface ITargetInfo
        {

            [DispId(1)]
            string HardwareId
            {
                [DispId(1)]
                get;
            }

            [DispId(2)]
            string Name
            {
                [DispId(2)]
                get;
            }

            [DispId(3)]
            string Host
            {
                [DispId(3)]
                get;
            }

            [DispId(6)]
            string ConnectionInfo
            {
                [DispId(6)]
                get;
            }

            [DispId(7)]
            ePowerStatus PowerStatus
            {
                [DispId(7)]
                get;
            }

            [DispId(8)]
            uint CPPackageVersion
            {
                [DispId(8)]
                get;
            }

            [DispId(9)]
            uint NetworkManagerVersion
            {
                [DispId(9)]
                get;
            }
        }


        [Guid("CBA7194A-4994-4881-9100-D47DB8D051BD")]
        [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        internal interface ITargetInfoCollection : IEnumerable
        {
            [DispId(1610743808)]
            uint Count
            {
                [DispId(1610743808)]
                get;
            }

            [DispId(0)]
            ITargetInfo this[[In] uint Index]
            {
                [DispId(0)]
                get;
            }
        }

        
        [Guid("FF0B24DF-D981-400A-B842-E7E5565F5BBB")]
        [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        public interface ITarget
        {        
            [DispId(31)]
            string HardwareId { get; }
            [DispId(40)]
            string Name { get; set; }
            
        }

        [Guid("B053B37D-81ED-4CDC-8F7A-8FE60A165CE7")]
        [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        public interface ITargetCollection : IEnumerable
        {
            [DispId(1610743808)]
            uint Count { get; }
            [DispId(5)]
            ITarget DefaultTarget { get; }
            [DispId(3)]
            ITarget FirstAvailable { get; }
            [DispId(4)]
            ITarget FirstConnected { get; }

            [DispId(0)]
            ITarget this[uint Index] { get; }

            [DispId(1)]
            ITarget GetByHardwareId(string bstrId);
            [DispId(2)]
            ITarget GetByName(string bstrName);
         
            [DispId(7)]
            ITargetCollection GetTargetsByHardwareId(string bstrName);
            [DispId(6)]
            ITargetCollection GetTargetsByName(string bstrName);
        }

        [Guid("5B04E7E7-1AAB-4CBA-8AF0-DA18F7C0B21F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        interface IPsp2TmApi
        {
            void CheckCompatibility(uint uExpectedVersion);
            ITargetInfoCollection DiscoverAllTargets();
            [DispId(2)]
            ITargetCollection Targets { get; }
            void Dispose();
        }

        internal class PowerStatusEnumConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
            {
                return false;
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
            {
                return destType == typeof(string);
            }

            public override object ConvertTo(ITypeDescriptorContext context,
                   CultureInfo culture,
                   object value,
                   Type destType)
            {

                if (value is ePowerStatus && destType == typeof(string))
                {
                    switch ((ePowerStatus)value)
                    {
                        case ePowerStatus.POWER_STATUS_NO_SUPPLY:
                            return "No Supply";
                        case ePowerStatus.POWER_STATUS_OFF:
                            return "Off";
                        case ePowerStatus.POWER_STATUS_ON:
                            return "On";
                        case ePowerStatus.POWER_STATUS_SUSPENDED:
                            return "Suspended";
                    }
                }

                return base.ConvertTo(context, culture, value, destType);
            }
        }
    }
}

