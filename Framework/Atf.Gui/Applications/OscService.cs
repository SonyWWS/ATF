//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Net.Sockets;
using System.ComponentModel;
using System.Threading;
using Bespoke.Common.Osc;
using Bespoke.Common;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications.NetworkTargetServices;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;
using PropertyDescriptor = System.ComponentModel.PropertyDescriptor;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Allows Open Sound Control (OSC) devices to get/set properties on C# objects</summary>
    /// <remarks>Consider also using OscCommands. If you want to communicate with
    /// the Liine Lemur iPad app, consider using LemurOscService instead of OscService.
    /// For further information about using Open Sound Control, please see this ATF wiki page:
    /// https://github.com/SonyWWS/ATF/wiki/OSC-Support </remarks>
    /// <remarks>In the Open Sound Control specification 1.1, in section 7.2, it declares that
    /// this protocol may be renamed to Open Systems Control in the future, so this class name
    /// uses the acronym, to avoid having to have a breaking change in the future.</remarks>
    [Export(typeof(IInitializable))]
    [Export(typeof(IOscService))]
    [Export(typeof(OscService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OscService : IOscService, IInitializable
    {
        /// <summary>
        /// Initializes this component, causing it to listen for OSC messages on the receiving
        /// port and listening for changes in watched C# objects.</summary>
        public virtual void Initialize()
        {
            InitializeServer();

            m_receivingEndPoint.Address = GetLocalIPAddresses().First();

            if (m_contextRegistry != null)
                m_contextRegistry.ActiveContextChanged += m_contextRegistry_ActiveContextChanged;

            if (m_settingsService != null)
            {
                var settings = new PropertyDescriptor[]
                {
                    new BoundPropertyDescriptor(this,
                        () => ReceivingPort,
                        "OSC Receiving Port Number".Localize(), null,
                        "The IP Port number that this app listens to for receiving Open Sound Control messages".Localize()),
                    new BoundPropertyDescriptor(this,
                        () => DestinationEndpoint,
                        "Primary Destination IP Endpoint".Localize(), null,
                        "The primary IP address and port number that this app sends Open Sound Control messages to. Additional destinations can be added due to auto-configuration".Localize(),
                        null, new EndPointConverter()),
                    new BoundPropertyDescriptor(this,
                        () => PreferredReceivingIPAddress,
                        "Preferred Receiving IP Address".Localize(), null,
                        "The preferred IP address that this app listens to for receiving Open Sound Control messages".Localize())
                };
                m_settingsService.RegisterUserSettings("Open Sound Control", settings);
                m_settingsService.RegisterSettings(this, settings);
            }

            m_sendingThread = new Thread(SendingThreadStart);
            m_sendingThread.Name = "OSC sending thread";
            m_sendingThread.IsBackground = true; //so that the thread can be killed if app dies.
            m_sendingThread.SetApartmentState(ApartmentState.STA);
            m_sendingThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            m_sendingThread.Start();
        }

        /// <summary>
        /// Gets or sets the port # that is used to receive UDP or TCP/IP messages. The default is 8000.</summary>
        public int ReceivingPort
        {
            get { return m_receivingEndPoint.Port; }
            set
            {
                if (value != m_receivingEndPoint.Port)
                {
                    if (value < 0 || value > 65535)
                        throw new ArgumentOutOfRangeException("ReceivingPort", value, "The IP port number must be between 0 and 65535");
                    m_receivingEndPoint.Port = value;
                    InitializeServer();
                }
            }
        }

        /// <summary>
        /// Gets this application's receiving IP address. Is only valid after Initialize() is called.
        /// When setting, consider using the static GetLocalIPAddresses to know what valid receiving
        /// IP addresses are available.</summary>
        public IPAddress ReceivingIPAddress
        {
            get { return m_receivingEndPoint.Address; }
            set
            {
                if (!value.Equals(m_receivingEndPoint.Address))
                {
                    m_receivingEndPoint.Address = value;
                    InitializeServer();
                }
            }
        }

        /// <summary>
        /// Gets or sets the preferred receiving IP address, in the case where there are multiple valid local
        /// IP addresses to choose from, this is the one that will be used. When setting, the value will be
        /// ignored if it's not a currently valid local IP address.</summary>
        public IPAddress PreferredReceivingIPAddress
        {
            get { return ReceivingIPAddress; }
            set
            {
                foreach (IPAddress localIP in GetLocalIPAddresses())
                {
                    if (localIP.Equals(value))
                    {
                        ReceivingIPAddress = value;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the local receiving endpoint, which is a combination of the ReceivingIPAddress and ReceivingPort</summary>
        public IPEndPoint ReceivingEndpoint
        {
            get { return m_receivingEndPoint; }
            set
            {
                if (!value.Equals(m_receivingEndPoint))
                {
                    m_receivingEndPoint = value;
                    InitializeServer();
                }
            }
        }

        /// <summary>
        /// Gets or sets the primary destination endpoint (IP address and port #). Additional
        /// connected devices can be communicated with if they initiate contact. Cannot be null.
        /// If the IP address portion of the endpoint is IPAddress.None ("255.255.255.255"),
        /// then no messages will be sent and a possibly large amount of processing will be saved.</summary>
        public IPEndPoint DestinationEndpoint
        {
            get { return m_destinationEndPoint; }
            set
            {
                m_destinationEndPoint = value;
                m_inputDevices.Clear();
                if (!value.Address.Equals(IPAddress.None))
                    m_inputDevices.Add(value);
                foreach (IPEndPoint endPoint in m_autoInputDevices)
                    m_inputDevices.Add(endPoint);
            }
        }

        /// <summary>
        /// Gets the current list of known IP endpoints (IP address and port #) to broadcast to.
        /// This will always include DestinationEndpoint, but may also include additional endpoints,
        /// for example if we've received messages, we automatically add the sender to this list.</summary>
        public IEnumerable<IPEndPoint> DestinationEndpoints
        {
            get { return m_inputDevices; }
        }

        /// <summary>
        /// A utility method for getting local IP addresses that could work for OSC communications.
        /// The first one in the enumeration is the best guess, but there could be multiple valid
        /// network adapters to choose from.</summary>
        /// <returns>At least one valid local IP address, even if it's just the "local host" or
        /// loopback address of 127.0.0.1.</returns>
        public static IEnumerable<IPAddress> GetLocalIPAddresses()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    yield return ip;
            }
            yield return IPAddress.Loopback;
        }

        /// <summary>
        /// Gets or sets the context registry that is used to know when the active context changes
        /// and to look for context interfaces such as IObservableContext and ISelectionContext.
        /// Must be set before calling Initialize().</summary>
        [Import(AllowDefault = true)]
        public IContextRegistry ContextRegistry
        {
            get { return m_contextRegistry; }
            set
            {
                if (IsInitialized)
                    throw new InvalidOperationException("OscService is already initialized");
                m_contextRegistry = value;
            }
        }

        /// <summary>
        /// Gets or sets the optional synchronization object, which is typically a System.Windows.Forms.Form
        /// object. This is used to change data in response to OSC messages on the main GUI thread so that
        /// 1) apps can be single-threaded when accessing the DOM, for example, and 2) so that when OSC
        /// messages arrive more rapidly than can be handled, that we avoid creating a long queue of
        /// messages that are all setting the same property (like if the user is rapidly moving a slider
        /// control).</summary>
        [Import(AllowDefault = true)]
        public ISynchronizeInvoke MainForm
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the observable context which is used to know if an object changes that
        /// needs to cause an OSC message to be sent. If ContextRegistry is not null, then the
        /// active context will attempt to be adapted to an observable context.</summary>
        public IObservableContext ObservableContext
        {
            get { return m_observableContext; }
            set
            {
                if (m_observableContext != value)
                {
                    if (m_observableContext != null)
                    {
                        m_observableContext.ItemChanged -= m_observableContext_ItemChanged;
                        m_observableContext.ItemInserted -= m_observableContext_ItemInserted;
                        m_observableContext.ItemRemoved -= m_observableContext_ItemRemoved;
                    }

                    m_observableContext = value;

                    if (m_observableContext != null)
                    {
                        m_observableContext.ItemChanged += m_observableContext_ItemChanged;
                        m_observableContext.ItemInserted += m_observableContext_ItemInserted;
                        m_observableContext.ItemRemoved += m_observableContext_ItemRemoved;
                    }

                    ClearAddressableCache();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current selection context which is used to identify objects that
        /// can receive or send property changes via OSC messages</summary>
        public ISelectionContext SelectionContext
        {
            get { return m_selectionContext; }
            set
            {
                if (m_selectionContext != value)
                {
                    if (m_selectionContext != null)
                        m_selectionContext.SelectionChanged -= m_selectionContext_SelectionChanged;
                    m_selectionContext = value;
                    if (m_selectionContext != null)
                        m_selectionContext.SelectionChanged += m_selectionContext_SelectionChanged;

                    ClearAddressableCache();
                }
            }
        }

        /// <summary>
        /// Adds an association of an OSC address with a particular C# property</summary>
        /// <param name="propertyInfo"></param>
        /// <param name="oscAddress"></param>
        /// <returns>The possibly fixed oscAddress, if there were illegal characters in
        /// it or if it was a duplicate</returns>
        public string AddPropertyAddress(PropertyInfo propertyInfo, string oscAddress)
        {
            oscAddress = OscServices.FixPropertyAddress(oscAddress);
            oscAddress = m_namer.Name(oscAddress);
            var info = new OscAddressInfo(oscAddress, propertyInfo);
            AddOscAddress(oscAddress, info);
            return oscAddress;
        }

        /// <summary>
        /// Adds an association of an OSC address with a particular C# property</summary>
        /// <param name="classType"></param>
        /// <param name="propertyName"></param>
        /// <param name="oscAddress"></param>
        /// <returns>The possibly fixed oscAddress, if there were illegal characters in
        /// it or if it was a duplicate</returns>
        public string AddPropertyAddress(Type classType, string propertyName, string oscAddress)
        {
            return AddPropertyAddress(classType.GetProperty(propertyName), oscAddress);
        }

        /// <summary>
        /// Adds an association of an OSC address with a particular C# property</summary>
        /// <param name="typeName"></param>
        /// <param name="propertyName"></param>
        /// <param name="oscAddress"></param>
        /// <returns>The possibly fixed oscAddress, if there were illegal characters in
        /// it or if it was a duplicate</returns>
        public string AddPropertyAddress(string typeName, string propertyName, string oscAddress)
        {
            Type type = GetTypeFromString(typeName);

            //Is this an error? What if the client app uses the same data file for different apps, like with SkinService?
            if (type == null)
                return oscAddress;

            PropertyInfo propertyInfo = type.GetProperty(propertyName);
            // The type was found, but it doesn't have the property. Definitely an error!
            if (propertyInfo == null)
                throw new InvalidOperationException("OscService was called with bad data. '" + type + "' does not contain property " + propertyName);

            return AddPropertyAddress(propertyInfo, oscAddress);
        }

        /// <summary>
        /// Adds an association of an OSC address with a property descriptor</summary>
        /// <param name="descriptor">A .NET property descriptor that can get or set values</param>
        /// <param name="oscAddress">A properly formatted OSC address. If it's not correct, it
        /// will be fixed and the corrected version will be returned.</param>
        /// <returns>The possibly fixed oscAddress, if there were illegal characters in
        /// it or if it was a duplicate</returns>
        public string AddPropertyAddress(PropertyDescriptor descriptor, string oscAddress)
        {
            oscAddress = OscServices.FixPropertyAddress(oscAddress);
            oscAddress = m_namer.Name(oscAddress);
            var info = new OscAddressInfo(oscAddress, descriptor);
            AddOscAddress(oscAddress, info);

            // Special DOM handling for greatly improved performance.
            var attrPropDesc = descriptor as AttributePropertyDescriptor;
            if (attrPropDesc != null)
            {
                DomNodeType owningDomNodeType;
                var childAttrPropDesc = attrPropDesc as ChildAttributePropertyDescriptor;
                if (childAttrPropDesc != null)
                    owningDomNodeType = childAttrPropDesc.Path.First().OwningType;
                else
                    owningDomNodeType = attrPropDesc.AttributeInfo.OwningType;
                AddToDomTypeCache(owningDomNodeType, info);
            }

            return oscAddress;
        }

        /// <summary>
        /// Returns whether or not Initialize has been called</summary>
        public bool IsInitialized
        {
            get { return m_udpServer != null; }
        }

        /// <summary>
        /// Gets a user-readable status message that describes the current status of this service</summary>
        public string StatusMessage
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine(m_serverStatusMsg);
                sb.AppendLine("IP addresses to broadcast to: " + m_inputDevices.Count);
                sb.AppendLine("Number of OSC messages received: " + NumMessagesEverReceived);
                sb.AppendLine("Number of OSC messages sent: " + NumMessagesEverSent);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the mappings of OSC addresses (e.g., "/1/fader1") to C# properties</summary>
        public IEnumerable<OscAddressInfo> AddressInfos
        {
            get { return m_oscAddressToInfo.Values; }
        }

        /// <summary>
        /// Gets the OSC address infos for the given object</summary>
        /// <param name="selected">The object to retrieve compatible OscAddressInfos for. It is assumed
        /// to be compatible with types of objects from the ISelectionContext.</param>
        /// <returns>A non-null, but possibly empty enumeration of all of the OscAddressInfo objects that
        /// describe properties of the given selected object.</returns>
        public IEnumerable<OscAddressInfo> GetInfos(object selected)
        {
            object common = SelectedToCommon(selected);
            if (common != null)
            {
                foreach (Tuple<OscAddressInfo, object> infoAddressablePair in GetAddressables(selected))
                    yield return infoAddressablePair.Item1;
            }
        }

        #region IOscService implementation
        
        /// <summary>
        /// Sends the OSC addresses and data objects to each destination endpoint, asynchronously</summary>
        /// <param name="addressesAndData">Each pair is an OSC address string and the non-null data payload
        /// that will be sent to each destination device.</param>
        /// <remarks>This method will return quickly. If there's a network slowdown of some kind, it may be
        /// possible that new values for a particular OSC address will replace the old unsent values. The
        /// order of the pairs can be changed before they are sent. The pairs may not be sent all in one
        /// OSC bundle, if the size is too large.</remarks>
        public void Send(IEnumerable<Tuple<string, object>> addressesAndData)
        {
            lock (m_outgoingQueue)
            {
                foreach (var tuple in addressesAndData)
                    m_outgoingQueue[tuple.Item1] = tuple;
            }
            m_outgoingDataAvailableEvent.Set();
        }

        /// <summary>
        /// Notifies listeners that an OSC message has been received. The OSC address and data payload
        /// will be provided and listeners can set the Handled property to true if no further processing
        /// should be done on this message.</summary>
        public event EventHandler<OscMessageReceivedArgs> MessageReceived
        {
            add { m_messageReceivedDelegates.Add(value); }
            remove { m_messageReceivedDelegates.Remove(value); }
        }

        #endregion

        /// <summary>
        /// Converts a type of object reported by an event in the IObservableContext into a type
        /// of object that is compatible with an object in the selection context (after passing
        /// through SelectedToCommon). The default is to assume that no conversion is necessary.</summary>
        /// <param name="observable">The object from an IObservableContext event</param>
        /// <returns>The equivalent object that will match SelectedToCommon, or null</returns>
        protected virtual object ObservableToCommon(object observable)
        {
            return observable;
        }

        /// <summary>
        /// Converts a type of object from an ISelectionContext into a type of object that is
        /// compatible with an object from IObservableContext (after passing through
        /// ObservableToCommon). The default is to assume that no conversion is necessary.</summary>
        /// <param name="selected">The object from an ISelectionContext</param>
        /// <returns>The equivalent object that will match ObservableToCommon, or null</returns>
        protected virtual object SelectedToCommon(object selected)
        {
            return selected;
        }

        /// <summary>
        /// Gets a set of OSC addresses and associated data payloads to add to an update that will
        /// be sent due to changes in 'common'.</summary>
        /// <param name="common">The object whose properties or data are being broadcast. This would have
        /// come from ObservableToCommon().</param>
        /// <returns></returns>
        protected virtual IEnumerable<Tuple<string, object>> GetCustomDataToSend(object common)
        {
            yield break;
        }

        /// <summary>
        /// Transforms data, if necessary, before sending it to a connected OSC device</summary>
        /// <param name="data">The data taken from 'common', using 'info'</param>
        /// <param name="common">The object whose properties or data are being broadcast. This would have
        /// come from ObservableToCommon().</param>
        /// <param name="info">The OSC address info for this data</param>
        /// <returns>The transformed data, ready to be sent</returns>
        protected virtual object PrepareDataForSending(object data, object common, OscAddressInfo info)
        {
            return data;
        }

        /// <summary>
        /// Sends the pairs of OSC addresses and data objects to the current destination endpoints
        /// immediately, on the current thread.</summary>
        /// <param name="addressesAndData"></param>
        protected virtual void SendSynchronously(IList<Tuple<string, object>> addressesAndData)
        {
            SendPacket(addressesAndData, 0, addressesAndData.Count);
        }

        /// <summary>
        /// Raises the MessageReceived event</summary>
        /// <param name="args">Event args containing the OSC address and data. Set the Handled
        /// property to true to avoid further processing.</param>
        protected virtual void OnMessageRecieved(OscMessageReceivedArgs args)
        {
            foreach (var del in m_messageReceivedDelegates)
            {
                if (args.Handled) //check first, in case derived class set Handled to true
                    return;
                del.Invoke(this, args);
            }
        }

        /// <summary>
        /// Sends a single packet synchronously to each destination device, with the given pairs
        /// of OSC addresses and data objects</summary>
        /// <param name="addressesAndData">The list of pairs</param>
        /// <param name="first">The index of the first pair to send</param>
        /// <param name="count">The number of pairs to send</param>
        protected void SendPacket(IList<Tuple<string, object>> addressesAndData, int first, int count)
        {
            if (addressesAndData.Count < first + count)
                return;

            OscPacket packet;
            if (count == 1)
            {
                packet = new OscMessage(ReceivingEndpoint, addressesAndData[first].Item1, addressesAndData[first].Item2);
            }
            else
            {
                var bundle = new OscBundle(ReceivingEndpoint, null);
                for (int i = first; i < first + count; i++)
                {
                    Tuple<string, object> tuple = addressesAndData[i];
                    var message = new OscMessage(ReceivingEndpoint, tuple.Item1, tuple.Item2);
                    bundle.Append(message);
                }
                packet = bundle;
            }

            foreach (IPEndPoint destination in DestinationEndpoints)
            {
                packet.Send(destination);
                NumMessagesEverSent++;
            }
        }

        // A separate thread is used to send the OSC addresses and associated values in case the
        //  network connection is disrupted, to avoid freezing the main GUI thread. This also might
        //  help in case the network is flooded with messages since only the most recent values are
        //  sent, if this sending thread falls behind.
        private void SendingThreadStart()
        {
            while (m_outgoingDataAvailableEvent.WaitOne())
            {
                List<Tuple<string, object>> outgoingAddressesAndData;
                lock (m_outgoingQueue)
                {
                    outgoingAddressesAndData = new List<Tuple<string, object>>(m_outgoingQueue.Values);
                    m_outgoingQueue.Clear();
                }
                SendSynchronously(outgoingAddressesAndData);
            }
        }

        // The Bespoke library thread will call this. Messages can be received very rapidly when
        //  the user is adjusting a knob or slider continuously -- much more rapidly than the GUI
        //  can update. So, we need to queue only the most recent message for each OSC address.
        private void m_oscUdpServer_MessageReceived(object sender, OscMessageReceivedEventArgs e)
        {
            NumMessagesEverReceived++;

            bool consumeMore = false;
            lock (m_incomingQueue)
            {
                int oldCount = m_incomingQueue.Count;
                m_incomingQueue[e.Message.Address] = e.Message;
                if (m_incomingQueue.Count > oldCount)
                    consumeMore = true;
            }

            if (consumeMore)
            {
                if (MainForm != null)
                    MainForm.BeginInvoke(new Action(ConsumeMessages), null);
                else
                    ConsumeMessages();
            }
        }

        // The main GUI thread will call this and clear out the current queue of OSC messages.
        private void ConsumeMessages()
        {
            List<OscMessage> messages;
            lock (m_incomingQueue)
            {
                messages = new List<OscMessage>(m_incomingQueue.Values);
                m_incomingQueue.Clear();
            }

            // Make a note of all OSC input devices that have contacted us. This allows for auto-configuration.
            // And give listeners to our MessageReceived event a chance to handle this message before we do
            //  further processing.
            int messagesHandled = 0;
            for(int i = 0; i < messages.Count; i++)
            {
                OscMessage message = messages[i];
                m_inputDevices.Add(message.SourceEndPoint);
                m_autoInputDevices.Add(message.SourceEndPoint);
                var args = new OscMessageReceivedArgs(message.Address, message.Data);
                OnMessageRecieved(args);
                if (args.Handled)
                {
                    messagesHandled++;
                    messages[i] = null;
                }
            }
            
            // Avoid expensive work if possible.
            if (messagesHandled == messages.Count)
                return;

            // Before accessing OscAddressInfo.Addressable, we must call this.
            CheckAddressableCache();

            // Set a flag so that when we set properties, we don't send OSC messages.
            // Also, this will make sure that we don't modify OscAddressInfo.Addressable while
            //  we're enumerating it.
            m_consumingMsg = true;

            try
            {
                // Only enter the transaction below if we receive useful OSC messages. Some client apps add
                //  a lot of overhead to each transaction, like re-drawing everything.
                var valuesToSet = new List<ValueSettingInfo>();
                foreach (OscMessage message in messages)
                {
                    // The message may have been handled already and removed from the list.
                    if (message == null)
                        continue;

                    // Make sure there's data before continuing.
                    if (message.Data.Count <= 0)
                        continue;

                    // Find matching OscAddressInfos
                    foreach (OscAddressInfo info in GetMatchingInfos(message.Address))
                    {
                        foreach (object addressable in info.Addressable)
                        {
                            object convertedData = ConvertOscData(message.Data, info.PropertyType);
                            if (convertedData != null)
                            {
                                valuesToSet.Add(new ValueSettingInfo(message.Address, addressable, convertedData, info));
                            }
                        }
                    }
                }

                if (valuesToSet.Count > 0)
                {
                    // If hundreds of objects are selected, DoTransaction() gives a critical performance boost.
                    // DoTransaction also enables undo/redo support.
                    m_transactionContext.DoTransaction(() =>
                    {
                        foreach (var valueToSet in valuesToSet)
                            SetValue(valueToSet.OscAddress, valueToSet.Addressable, valueToSet.ConvertedData, valueToSet.OscAddressInfo);
                    }, "OSC Input".Localize("The name of a command"));
                }
            }
            finally
            {
                m_consumingMsg = false;
            }
        }

        // We need this class for our VS2008 support, because the .NET Framework 3.5 doesn't have a 4-element Tuple.
        private class ValueSettingInfo
        {
            public ValueSettingInfo(string oscAddress, object addressable, object convertedData, OscAddressInfo oscAddressInfo)
            {
                OscAddress = oscAddress;
                Addressable = addressable;
                ConvertedData = convertedData;
                OscAddressInfo = oscAddressInfo;
            }

            public readonly string OscAddress;
            public readonly object Addressable;
            public readonly object ConvertedData;
            public readonly OscAddressInfo OscAddressInfo;
        }

        // To do: handle the wild card characters, '*' and '?', robustly. Add unit tests. Use a tree structure?
        private void AddOscAddress(string oscAddress, OscAddressInfo info)
        {
            m_oscAddressToInfo[oscAddress] = info;
            if (oscAddress.EndsWith("*"))
                m_wildCards[oscAddress] = info;
        }

        private IEnumerable<OscAddressInfo> GetMatchingInfos(string oscAddress)
        {
            // Look for an exact match first.
            OscAddressInfo info;
            if (m_oscAddressToInfo.TryGetValue(oscAddress, out info))
                yield return info;

            // Look for pattern matches.
            // To do: we're assuming for now that the wild card strings all end in '*' which means
            //  match anything afterwards. We need to support '*' and '?' anywhere.
            foreach (KeyValuePair<string, OscAddressInfo> pair in m_wildCards)
            {
                string ourOscAddress = pair.Key;
                if (string.Compare(ourOscAddress, 0, oscAddress, 0, ourOscAddress.Length - 1) == 0)
                    yield return pair.Value;
            }
        }

        /// <summary>
        /// Tries to match the OSC message's data with the given property type, converting it if possible.</summary>
        /// <param name="messageData">List of objects that are the data payload from this incoming message.
        /// Must have at least one object in it.</param>
        /// <param name="propertyType">The property's type</param>
        /// <returns>The compatible data or null.</returns>
        private object ConvertOscData(IList<object> messageData, Type propertyType)
        {
            // Usually, the list has one object in it and we want the first one.
            object data = messageData[0];

            // Special handling for arrays of floats
            if (propertyType == typeof(float[]) && messageData[0] is float)
            {
                float[] floats = new float[messageData.Count];
                for (int i = 0; i < messageData.Count; i++)
                    floats[i] = (float)messageData[i];
                data = floats;
            }

            // Normal property setting
            Type dataType = data.GetType();
            if (propertyType.IsAssignableFrom(dataType))
                return data;

            // Special handling for booleans
            else if (propertyType.IsAssignableFrom(typeof(bool)))
            {
                if (dataType == typeof(float))
                    return ((float)data) == 1.0f;
            }

            // Special handling of float-to-int
            else if (propertyType.IsAssignableFrom(typeof(int)))
            {
                if (dataType == typeof(float))
                    return (int)Math.Round(((float)data)); // Round to nearest rather than taking the floor.
            }

            return null;
        }

        /// <summary>
        /// Sets a value on an OSC-addressable property</summary>
        /// <param name="oscAddress">Original OSC address that might be different than OscAddressInfo's Address
        /// due to wild card matching.</param>
        /// <param name="addressable">Object that contains a property that matches 'oscAddress'</param>
        /// <param name="value">The new value to set the property to</param>
        /// <param name="info">The matching OscAddressInfo object which knows how to set the value on the addressable object</param>
        protected virtual void SetValue(string oscAddress, object addressable, object value, OscAddressInfo info)
        {
            info.SetValue(addressable, value);
        }

        private void m_observableContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
        {
            ObservableContextChanged(e.Item);
        }

        private void m_observableContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            ClearAddressableCache();
            ObservableContextChanged(e.Item);
        }

        private void m_observableContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            ObservableContextChanged(e.Item);
        }

        private void ObservableContextChanged(object observable)
        {
            if (CanUpdateConnectedDevices)
            {
                object common = ObservableToCommon(observable);
                if (common != null)
                    UpdateConnectedDevices(common);
            }
        }

        private bool CanUpdateConnectedDevices
        {
            get
            {
                // If there's no one listening, then let's stop working
                if (m_inputDevices.Count == 0)
                    return false;

                // This isn't perfect. What if one piece of data changing causes another to change
                //  and that 2nd change needs to fed back to the OSC input device?
                if (m_consumingMsg)
                    return false;

                return true;
            }
        }

        // Make sure to call CanUpdateConnectedDevices first.
        // If 'common' is not in the selection set or if it's null, it will be ignored.
        private void UpdateConnectedDevices(object common)
        {
            if (common == null)
                return;

            // Only update if this matches the last selected object? Probably should be a customization point here.
            if (m_selectionContext != null)
            {
                object lastSelectedAsCommon = SelectedToCommon(m_selectionContext.LastSelected);
                if (lastSelectedAsCommon != common)
                    return;
            }

            // Assemble a list of OSC addresses and the corresponding data payload. Start by creating
            //  a list using any custom data that needs to be sent.
            List<Tuple<string, object>> addressesAndDataList = GetCustomDataToSend(common).ToList();

            // Must call before reading 'm_commonToInfo'
            CheckAddressableCache();

            foreach (Tuple<OscAddressInfo, object> tuple in m_commonToInfo[common])
            {
                OscAddressInfo info = tuple.Item1;
                object addressable = tuple.Item2;

                object data = info.GetValue(addressable);
                data = PrepareDataForSending(data, common, info);
                if (data != null)
                    addressesAndDataList.Add(new Tuple<string, object>(info.Address, data));
            }

            // Notify all of our connected devices
            if (addressesAndDataList.Count > 0)
                Send(addressesAndDataList);
        }

        /// <summary>
        /// Gets or sets the number of messages ever received. This is for reporting to the user, for
        /// diagnostic purposes.</summary>
        internal long NumMessagesEverReceived;

        /// <summary>
        /// Gets or sets the number of messages ever sent. This is for reporting to the user, for
        /// diagnostic purposes.</summary>
        internal long NumMessagesEverSent;

        private void ClearAddressableCache()
        {
            m_dirtySelectionInfo = true; //should be the only place where this is set to true
        }

        // Call this before using m_commonToInfo or OscAddressInfo.Addressable
        private void CheckAddressableCache()
        {
            if (m_dirtySelectionInfo)
            {
                m_dirtySelectionInfo = false; //should be the only place where this is set to false other than initialization

                if (m_consumingMsg)
                    throw new InvalidOperationException(
                        "CheckAddressableCache() is being called at a bad time." +
                        "OscAddressInfo.Addressable might be modified while it's being enumerated.");

                // Clear out mappings of common objects to OscAddressInfo objects and clear out the cache
                //  of addressable objects for each OscAddressInfo.
                m_commonToInfo.Clear();
                foreach (OscAddressInfo info in m_oscAddressToInfo.Values)
                    info.Addressable.Clear();

                // Rebuild what we just cleared.
                if (m_selectionContext != null)
                {
                    // We need all selected objects to be added to matching info.Addressable lists so that the
                    //  OSC input device can efficiently change properties on all selected objects.
                    foreach (object selected in m_selectionContext.Selection)
                    {
                        // Find all compatible OscAddressInfo objects for this selected object (after
                        //  a possible conversion to a 'common' form). There could be many because
                        //  each OscAddressInfo maps to a single C# property.
                        object common = SelectedToCommon(selected);
                        if (common == null)
                            continue;

                        foreach (Tuple<OscAddressInfo, object> infoAddressablePair in GetAddressables(selected))
                        {
                            // For when we receive an OSC message and need to update many selected objects:
                            infoAddressablePair.Item1.Addressable.Add(infoAddressablePair.Item2);

                            // For when an object changes (IObservableContext) and we need to notify OSC input devices:
                            m_commonToInfo.Add(common, infoAddressablePair);
                        }
                    }
                }
            }
        }

        private IEnumerable<Tuple<OscAddressInfo, object>> GetAddressables(object common)
        {
            // Special DOM handling, for greatly improved performance.
            var domNode = common.As<DomNode>();
            if (domNode != null)
            {
                IEnumerable<OscAddressInfo> infos = QueryDomTypeCache(domNode.Type);
                foreach (OscAddressInfo info in infos)
                    yield return new Tuple<OscAddressInfo, object>(info, domNode);
                yield break;
            }

            // Because of the flexibility of CommonToAddressable(), there doesn't seem to be
            //  a way of speeding this search up with a dictionary. 'common' could be adaptable
            //  to any type!
            foreach (OscAddressInfo info in m_oscAddressToInfo.Values)
            {
                object addressable = info.CommonToAddressable(common);
                if (addressable != null)
                    yield return new Tuple<OscAddressInfo, object>(info, addressable);
            }
        }

        private IEnumerable<OscAddressInfo> QueryDomTypeCache(DomNodeType domNodeType)
        {
            List<OscAddressInfo> infos;
            if (!m_domTypeToInfo.TryGetValue(domNodeType, out infos))
            {
                // We need to create a new list. Even if a base type has a list, we need to copy it
                //  in case a new OSC address is added later that is just for domNodeType.
                infos = new List<OscAddressInfo>();
                DomNodeType baseDomType = domNodeType.BaseType;
                while (baseDomType != null)
                {
                    List<OscAddressInfo> baseInfos;
                    if (m_domTypeToInfo.TryGetValue(baseDomType, out baseInfos))
                    {
                        infos.AddRange(baseInfos);
                        break;
                    }
                    baseDomType = baseDomType.BaseType;
                }
                m_domCacheHasBeenQueried = true;
                m_domTypeToInfo[domNodeType] = infos;
            }
            return infos;
        }

        private void AddToDomTypeCache(DomNodeType owningDomNodeType, OscAddressInfo info)
        {
            // If QueryDomTypeCache has been called and has modified the list of OscAddressInfo
            //  objects in m_domTypeToInfo, then we need to clear the cache and rebuild it, at
            //  least for every DomNodeType that derives from this one.
            if (m_domCacheHasBeenQueried)
                throw new InvalidOperationException("New OSC addresses can't currently be added after start-up");

            List<OscAddressInfo> infos;
            if (!m_domTypeToInfo.TryGetValue(owningDomNodeType, out infos))
            {
                infos = new List<OscAddressInfo>();
                m_domTypeToInfo[owningDomNodeType] = infos;
            }
            infos.Add(info);
        }

        private void oscServer_ReceiveErrored(object sender, ExceptionEventArgs e)
        {
            Outputs.WriteLine(OutputMessageType.Warning, "OscService received an error: " + e.Exception.ToString());
        }

        private void m_selectionContext_SelectionChanged(object sender, EventArgs e)
        {
            ClearAddressableCache();
            if (CanUpdateConnectedDevices)
            {
                CheckAddressableCache();

                // Only update if this matches the last selected object.
                object lastSelectedAsCommon = SelectedToCommon(m_selectionContext.LastSelected);
                UpdateConnectedDevices(lastSelectedAsCommon);
            }
        }

        private void m_contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            ObservableContext = m_contextRegistry.GetActiveContext<IObservableContext>();
            SelectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
            m_transactionContext = m_contextRegistry.GetActiveContext<ITransactionContext>();
        }

        private static Type GetTypeFromString(string typeString)
        {
            if (String.IsNullOrEmpty(typeString))
                return null;

            Type type = null;

            // We don't require the type name to be an assembly qualified name, so we need to search
            //  each loaded assembly instead of using Type.GetType(string).
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeString);
                if (type != null)
                    break;
            }

            return type;
        }

        private void InitializeServer()
        {
            if (m_udpServer != null)
            {
                m_udpServer.ReceiveErrored -= oscServer_ReceiveErrored;
                m_udpServer.MessageReceived -= m_oscUdpServer_MessageReceived;
                m_udpServer.Stop();
                m_udpServer = null;
            }

            m_udpServer = new OscServer(TransportType.Udp, IPAddress.Any, ReceivingPort, true);

            //Bespoke uses a List instead of a HashSet and doesn't do pattern matching, so
            //  it's inefficient and non-conformant. Let's get all messages and do the filtering
            //  and matching ourselves.
            m_udpServer.FilterRegisteredMethods = false;

            // Receive notifications for all OSC messages, no matter how they're bundled.
            m_udpServer.MessageReceived += m_oscUdpServer_MessageReceived;

            // These two go together. ConsumeParsingExceptions needs to be false for our event
            //  handler to be called.
            m_udpServer.ReceiveErrored += oscServer_ReceiveErrored;
            m_udpServer.ConsumeParsingExceptions = false;

            // A possible error is that another ATF-app is using the given port #. What can we do
            //  in that case?
            try
            {
                m_udpServer.Start();
                m_serverStatusMsg = "Running";
            }
            catch (SocketException e)
            {
                Outputs.WriteLine(OutputMessageType.Error, e.Message);
                m_serverStatusMsg = "Not running. Could be two apps using the same port #. Exception text: \"" +
                    e.Message + '\"';
            }

            // to-do: subscribe to an app shutdown event? Or have a finalizer?
            //m_oscUdpServer.Stop();
        }

        /// <summary>
        /// Contains information for mapping an OSC address to a property on a C# class</summary>
        public class OscAddressInfo
        {
            // to-do: only have a PropertyDescriptor? BoundPropertyDescriptor can't be used
            //  because it has to be created per instance of the target type whereas we
            //  want to apply the same property descriptor to multiple objects.
            internal OscAddressInfo(string oscAddress, PropertyInfo propertyInfo)
            {
                Address = oscAddress;
                m_propertyInfo = propertyInfo;
                CompatibleType = propertyInfo.DeclaringType;
                PropertyType = propertyInfo.PropertyType;
            }

            internal OscAddressInfo(string oscAddress, PropertyDescriptor descriptor)
            {
                Address = oscAddress;
                m_descriptor = descriptor;
                CompatibleType = descriptor.ComponentType;
                PropertyType = descriptor.PropertyType;
            }

            /// <summary>
            /// The OSC address that corresponds to a particular property on CompatibleType</summary>
            public readonly string Address;

            /// <summary>
            /// The type of object that has a C# property that corresponds to the OSC address</summary>
            public readonly Type CompatibleType;

            /// <summary>
            /// The C# property's type, which the OSC data needs to match</summary>
            public readonly Type PropertyType;

            /// <summary>
            /// Gets the name of the property</summary>
            public string PropertyName
            {
                get { return (m_descriptor != null) ? m_descriptor.Name : m_propertyInfo.Name; }
            }

            /// <summary>
            /// Gets the property descriptor, if one was used to create this OscAddressInfo. May be null.</summary>
            public PropertyDescriptor PropertyDescriptor
            {
                get { return m_descriptor; }
            }

            /// <summary>
            /// Attempts to convert an object that came from SelectedToCommon() into an object that is
            /// compatible with an OSC Address. The default is to use ATF's adaptability (which uses
            /// the C# 'as' operator first and then looks for the IAdaptable interface).</summary>
            /// <param name="common">The object from ISelectionContext, translated to some 'common'
            /// compatible type.</param>
            /// <remarks>There's a performance hit by allowing this adaptability. Let's keep
            /// this private to keep our options open.</remarks>
            internal object CommonToAddressable(object common)
            {
                return common.As(CompatibleType);
            }

            /// <summary>
            /// Gets the value of the given compatible object</summary>
            /// <param name="compatible">An object whose type is CompatibleType</param>
            /// <returns>The value of the addressable property or null</returns>
            internal object GetValue(object compatible)
            {
                if (m_descriptor != null)
                    return m_descriptor.GetValue(compatible);
                else
                    return m_propertyInfo.GetValue(compatible, null);
            }

            /// <summary>
            /// Sets the value of the associated property on 'compatible'</summary>
            /// <param name="compatible">An object whose type is CompatibleType</param>
            /// <param name="data">The object to set this property to</param>
            internal void SetValue(object compatible, object data)
            {
                if (m_descriptor != null)
                    PropertyUtils.SetProperty(compatible, m_descriptor, data);
                else
                    m_propertyInfo.SetValue(compatible, data, null);
            }

            /// <summary>
            /// Gets a list of selected objects that have been converted into an OSC-addressable object
            /// of a type PropertyInfo.DeclaringType. Call CheckAddressableCache() before
            /// enumerating through this list.</summary>
            internal readonly List<object> Addressable = new List<object>();

            private readonly PropertyDescriptor m_descriptor;
            private readonly PropertyInfo m_propertyInfo;
        }

        /// <summary>
        /// A type converter that converts 'from' strings and converts 'to' IPEndPoints. The format for
        /// an IPEndPoint is like: "192.168.0.120:9000", for IPv4.</summary>
        private class EndPointConverter : TypeConverter
        {
            /// <summary>
            /// Determines whether a string can be converted to an IPEndPoint</summary>
            /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
            /// <param name="sourceType">A System.Type that represents the type you want to convert from</param>
            /// <returns>True iff this instance can convert from the specified context</returns>
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            /// <summary>
            /// Converts the given string to an IPEndPoint</summary>
            /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
            /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"></see> to use as the current culture.</param>
            /// <param name="value">The object to convert</param>
            /// <returns>An object that represents the converted value</returns>
            /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed</exception>
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                string s = value as string;
                if (s != null)
                {
                    IPEndPoint endPoint = TcpIpTargetInfo.TryParseIPEndPoint(s);
                    if (endPoint != null)
                        return endPoint;

                    // This exception should be localized because it is displayed by the user preferences dialog.
                    throw new NotSupportedException("The format for an IP end point is like \"192.168.0.120:9000\"".Localize());
                }

                return base.ConvertFrom(context, culture, value);
            }
        }

        // This boolean flag and the multimap go together. Before reading the multimap, call
        //  CheckAddressableCache().
        // The multimap is:
        //  key: a 'common' object that came from SelectedToCommon() or ObservableToCommon()
        //  Tuple, first: an OscAddressInfo that defines a property that 'common' logically has.
        //  Tuple, second: The actual 'addressable' object that corresponds to 'common'
        //    and is inside the OscAddressInfo.Addressable property.
        private bool m_dirtySelectionInfo;
        private readonly Multimap<object, Tuple<OscAddressInfo, object>> m_commonToInfo =
            new Multimap<object, Tuple<OscAddressInfo, object>>();

        // Map OSC address patterns (e.g., "/1/fader1") to a C# type's property info.
        private readonly Dictionary<string, OscAddressInfo> m_oscAddressToInfo =
            new Dictionary<string, OscAddressInfo>();
        private readonly Dictionary<string, OscAddressInfo> m_wildCards =
            new Dictionary<string, OscAddressInfo>();

        // Map DomNodeTypes to a set of OSC address infos that work with it.
        private bool m_domCacheHasBeenQueried;
        private readonly Dictionary<DomNodeType, List<OscAddressInfo>> m_domTypeToInfo =
            new Dictionary<DomNodeType, List<OscAddressInfo>>();

        private readonly HashSet<IPEndPoint> m_inputDevices =
            new HashSet<IPEndPoint>();//all input devices, including auto-configured ones
        private readonly HashSet<IPEndPoint> m_autoInputDevices =
            new HashSet<IPEndPoint>(); //to remember auto-configured devices
        private OscServer m_udpServer;
        private IPEndPoint m_receivingEndPoint = new IPEndPoint(IPAddress.Loopback, 8000);
        private IPEndPoint m_destinationEndPoint = new IPEndPoint(IPAddress.None, 8000); // By default, let's not broadcast.
        private bool m_consumingMsg;
        private readonly UniqueNamer m_namer = new UniqueNamer();
        private Thread m_sendingThread;
        private readonly AutoResetEvent m_outgoingDataAvailableEvent = new AutoResetEvent(false);

        private string m_serverStatusMsg = "Not yet initialized";

        // All access to this "queue" must be by locking it first. The Bespoke library thread will
        //  lock it before adding messages, and the main GUI thread will lock it before consuming messages.
        private readonly Dictionary<string, OscMessage> m_incomingQueue = new Dictionary<string, OscMessage>();

        private readonly Dictionary<string, Tuple<string, object>> m_outgoingQueue = new Dictionary<string, Tuple<string, object>>();

        // A list of delegates that are listeners to the MessageReceived event. I could have used
        //  the MulticastDelegate's GetInvocationList(), but I didn't like that it creates an array
        //  on each call.
        private List<EventHandler<OscMessageReceivedArgs>> m_messageReceivedDelegates =
            new List<EventHandler<OscMessageReceivedArgs>>();

        // Other components and contexts that we can use
        private IContextRegistry m_contextRegistry;
        private IObservableContext m_observableContext;
        private ISelectionContext m_selectionContext;
        private ITransactionContext m_transactionContext;
        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService = null;
    }
}
