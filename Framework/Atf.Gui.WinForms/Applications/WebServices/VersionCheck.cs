//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Sce.Atf.Applications.WebServices
{
    /// <summary>
    /// Callback for after version check completes</summary>
    /// <param name="val">Message</param>
    /// <param name="error"><c>True</c> if error occurred</param>
    public delegate void CheckCompletedHandler(string val, bool error);

    /// <summary>
    /// Checks to see if there is an update available on SourceForge</summary>
    /// <remarks>
    /// It checks for the first available package according to the
    /// criteria specified for the project. More more info, see:
    ///   http://wiki.ship.scea.com/confluence/display/SUPPORT/Bug+Submit+and+Version+Check+services
    /// If that package's version is greater than this assembly's
    /// version, the user is notified that an update is available
    /// and is given the option of opening the SourceForge page.
    ///
    /// Note that each project has a unique identifier used to
    /// map to the SourceForge project. This is the value of
    /// the AssemblyTitle attribute, or the ProjectMappingAttribute
    /// if present.</remarks>
    public class VersionCheck
    {
        /// <summary>
        /// Event that is raised when check is complete</summary>
        public event CheckCompletedHandler CheckComplete;

        /// <summary>
        /// Constructor.
        /// Mapping name and version are obtained from attributes on the assembly:
        ///   
        ///   AssemblyTitleAttribute specifies mapping (or ProjectMappingAttribute, if present).
        ///   AssemblyVersionAttribute specifies version.
        /// The file release in SourceForge should be named:
        ///   "{mapping name} {version}"
        /// ,e.g.,
        ///   "SoundConvert 1.0"
        ///   "SCREAM Tool 5.1.0".</summary>
        public VersionCheck()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            // use ProjectMappingAttribute for mapping
            ProjectMappingAttribute mapAttr = (ProjectMappingAttribute)Attribute.GetCustomAttribute(assembly, typeof(ProjectMappingAttribute));
            if (mapAttr != null && mapAttr.Mapping != null && mapAttr.Mapping.Trim().Length != 0)
                m_appMappingName = mapAttr.Mapping.Trim();
            else
                m_appMappingName = null;

            m_appVersion = assembly.GetName().Version;

            // Force control to have a handle.
            // Usually a control will get a handle assigned to it only when it becomes visible for the first time.
            // Note: that the handle of the control will tie it to the thread that creates it.
            // We want to tie the control to the main thread.
            #pragma warning disable 168 //suppress warning about unused local variable
            IntPtr dummyHandle = m_dummyControl.Handle;
            #pragma warning restore 168
        }

        /// <summary>
        /// Gets the application version</summary>
        /// <value>Application version</value>
        public Version AppVersion
        {
            get{ return m_appVersion;}
        }

        /// <summary>
        /// Gets the server version</summary>
        /// <value>Server version</value>
        public Version ServerVersion
        {
            get {return m_serverVersion;}
        }

        /// <summary>
        /// Checks the version</summary>
        /// <param name="async">Asynchronous if set to <c>true</c> [async]</param>
        public void Check(bool async)
        {

            try
            {
                if (m_appMappingName == null || m_appMappingName.Length == 0)
                    throw new Exception("Cannot check for update.\nAssembly mapping name not found");
                if (m_appVersion == null)
                    throw new Exception("App version not defined");

                if (async)
                {
                    Thread thread = new Thread(DoCheck);
                    thread.Priority = ThreadPriority.Lowest;
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                    thread.Name = "thread: Checking for update";
                    thread.IsBackground = true; // so terminating main app will kill it
                    thread.Start();
                }
                else
                {
                    DoCheck();
                }
            }
            catch (Exception e)
            {
                NotifyClients("Version Check Failed".Localize() + e.Message, true);
            }
        }

        /// <summary>
        /// Gets the latest product version from server. Returns download URL 
        /// if newer version found.</summary>
        private void DoCheck()
        {
            try
            {
                if (s_checkInProgress)
                    return;
                s_checkInProgress = true;
                com.scea.ship.versionCheck.VersionCheckerService checker = new com.scea.ship.versionCheck.VersionCheckerService();
                object[] versionInfo = checker.getLatestVersionInfo(m_appMappingName);
                string strServerVersion = ((string)versionInfo[0]).Trim();
                string[] arrStr = strServerVersion.Split(' ');
                m_serverVersion = new Version(arrStr[arrStr.Length - 1]);

                string url = null;

                if (m_serverVersion > m_appVersion)
                {
                    url = ((string)versionInfo[2]).Trim();
                }

                NotifyClients(url, false);

            }
            catch (Exception e)
            {
                NotifyClients("Version check failed.\nError: " + e.Message, true);
            }
            finally
            {
                s_checkInProgress = false;
            }

        }

        /// <summary>
        /// Notify clients of version check results.
        /// Invokes the event on the subscriber's thread, not the publisher's thread.</summary>
        /// <param name="msg">Message for client</param>
        /// <param name="error"><c>True</c> if error occurred</param>
        private void NotifyClients(string msg, bool error)
        {
            if (m_dummyControl.InvokeRequired)
            {
                // switch to main thread (GUI thread.
                m_dummyControl.Invoke(new Action<string,bool>(NotifyClients), new object[] { msg, error });
            }
            else
            {
                if (CheckComplete != null)
                {

                    CheckCompletedHandler handler = CheckComplete;
                    handler(msg, error);
                }
            }
        }

        private readonly string m_appMappingName;
        private readonly Version m_appVersion;
        private Version m_serverVersion;
        private static bool s_checkInProgress;

        // dummy control so we can use its Invoke method.
        private readonly Control m_dummyControl = new Control();
    }
}

