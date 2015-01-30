//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Reflection;

using Scea;

namespace Sce.Atf
{
    /// <summary>
    /// A service to log text messages to a remote server. By default, the application
    /// name and version # are retrieved from the entry assembly and the logged
    /// messages is sent to an ATF server available to all of World Wide Studios.</summary>
    [Export(typeof(IServerLogger))] // the preferred way of sending generic log messages
    [Export(typeof(ServerLogger))]  // provides direct access to the RecapConnection object for custom messages
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ServerLogger : IServerLogger
    {
        /// <summary>
        /// Constructor that finds good default values for the application's name and version</summary>
        public ServerLogger()
        {
            // GetEntryAssembly can be null if called from unmanaged code, like in UnitTests.
            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();
            if (assembly == null)
                assembly = Assembly.GetExecutingAssembly();

            // Allow client-customization of the name and version of this application.
            if (ApplicationName == null)
                ApplicationName = assembly.GetName().Name;
            if (ApplicationVersion == null)
                ApplicationVersion = assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Gets and sets the server URI. The name can only be set prior to the first call to
        /// SendLogData.</summary>
        public virtual Uri ServerName
        {
            get { return m_serverName; }
            set
            {
                TestNotInitialized();
                m_serverName = value;
            }
        }

        /// <summary>
        /// Gets the application name for purposes of logging. By default, this is the entry
        /// assembly without the filename extension. 'Set' can only be called prior to
        /// SendLogData being called.</summary>
        public virtual string ApplicationName
        {
            get { return m_applicationName; }
            set
            {
                TestNotInitialized();
                m_applicationName = value;
            }
        }

        /// <summary>
        /// Gets the application version string used to log data. By default, this is the version
        /// of the entry assembly that comes from the C# project settings. 'Set' can only be
        /// called prior to SendLogData being called.</summary>
        public virtual string ApplicationVersion
        {
            get { return m_applicationVersion; }
            set
            {
                TestNotInitialized();
                m_applicationVersion = value;
            }
        }

        #region IServerLogger

        /// <summary>
        /// Sends a log message asynchronously to the logging server</summary>
        /// <param name="data">Message</param>
        public virtual void SendLogData(string data)
        {
            GetConnection().post(RecapConnection.MessageTypes.Log, data);
        }

        #endregion

        /// <summary>
        /// Gets the connection object. Is never null. If the defaults aren't correct,
        /// set the properties such as ServerName, ApplicationName and ApplicationVersion first.</summary>
        /// <returns>Initialized connection object</returns>
        /// <remarks>It would be nice to leave this as internal to allow the ATF team to choose a new
        /// underlying tech, just in case RecapConnection doesn't work out.</remarks>
        internal RecapConnection GetConnection()
        {
            InitializeConnection();
            return m_recapConnection;
        }

        // Initializes the connection, if this hasn't occurred already. Can be called multiple times.
        private void InitializeConnection()
        {
            if (m_recapConnection != null)
                return;

            // Now that ApplicationVersion and ApplicationName are set, we can proceed.
            m_recapConnection = new RecapConnection(ServerName, ApplicationName, ApplicationVersion);

            // RecapConnection's default behavior is to throw the exception again. This leads to an unhandled
            //  exception on a worker thread that brings the whole app down. Exceptions are thrown under normal
            //  conditions such as not having a network connection. Since 'callback' can't be null, let's use
            //  our own do-nothing exception handler.
            m_recapConnection.callback = HandleRecapException;
        }

        private void TestNotInitialized()
        {
            if (m_recapConnection != null)
                throw new InvalidOperationException("The ServerLogger properties must be set before the server connection is created");
        }

        private static void HandleRecapException(Exception e)
        {
        }

        private Uri m_serverName = new Uri("http://sd-cdump-dev002.share.scea.com:8080");
        private string m_applicationName;
        private string m_applicationVersion;
        private RecapConnection m_recapConnection;
    }
}
