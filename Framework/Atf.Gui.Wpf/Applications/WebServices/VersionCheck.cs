//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Reflection;
using System.Threading;
using System.Windows;

using VersionCheckerService = Sce.Atf.Applications.WebServices.com.scea.ship.versionCheck.VersionCheckerService;

namespace Sce.Atf.Wpf.Applications.WebServices
{
    /// <summary>
    /// Callback for after version check completes</summary>
    /// <param name="val">Message</param>
    /// <param name="error">if true, error</param>
    public delegate void CheckCompletedHandler(string val, bool error);

    /// <summary>
    /// Check to see if there is an update available on SourceForge</summary>
    /// <remarks>
    /// It checks for the first available package according to the
    /// criteria specified for the project at
    ///   http://ship.scea.com/appupdate
    /// If that package's version is greater than this assembly's
    /// version, the user is notified that an update is available
    /// and is given the option of opening the SourceForge page.
    ///
    /// Note that each project has a unique identifier used to
    /// map to the SourceForge project.  This is the value of
    /// the AssemblyTitle attribute, or the ProjectMappingAttribute
    /// if present.</remarks>
    public class VersionCheck
    {
        /// <summary>
        /// Event that is raised when check is complete</summary>
        public event CheckCompletedHandler CheckComplete;

        /// <summary>
        /// Mapping name and version are obtained from attributes on the assembly:
        ///   
        ///   AssemblyTitleAttribute specifies mapping (or ProjectMappingAttribute, if present).
        ///   AssemblyVersionAttribute specifies version.
        /// The file release in SourceForge should be named:
        ///   "{mapping name} {version}"
        /// e.g.
        ///   "SoundConvert 1.0"
        ///   "SCREAM Tool 5.1.0"</summary>
        public VersionCheck()
        {
            var assembly = Assembly.GetEntryAssembly();
            // use ProjectMappingAttribute for mapping
            var mapAttr = (ProjectMappingAttribute)Attribute.GetCustomAttribute(assembly, typeof(ProjectMappingAttribute));
            if (mapAttr != null && mapAttr.Mapping != null && mapAttr.Mapping.Trim().Length != 0)
                m_appMappingName = mapAttr.Mapping.Trim();
            else
                m_appMappingName = null;

            AppVersion = assembly.GetName().Version;
        }

        /// <summary>
        /// Gets the app version</summary>
        /// <value>The app version</value>
        public Version AppVersion { get; private set; }

        /// <summary>
        /// Gets the server version</summary>
        /// <value>The server version</value>
        public Version ServerVersion { get; private set; }

        /// <summary>
        /// Checks the version</summary>
        /// <param name="async">if set to <c>true</c> [async]</param>
        public void Check(bool async)
        {
            try
            {
                Requires.NotNullOrEmpty(m_appMappingName, "Cannot check for update.\nAssembly mapping name not found");
                Requires.NotNull(AppVersion, "App version not defined");

                if (async)
                {
                    var thread = new Thread(DoCheck);
                    thread.Name = "thread: Checking for update";
                    thread.IsBackground = true; // so terminating main app will kill it
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Priority = ThreadPriority.Lowest;
                    thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                    thread.Start();
                }
                else
                {
                    DoCheck();
                }
            }
            catch (Exception e)
            {
                NotifyClients(("Version Check Failed" + e.Message).Localize(), true);
            }
        }

        /// <summary>
        /// Get the latest product version from server, return download url 
        /// if newer version found</summary>
        private void DoCheck()
        {
            try
            {
                if (ms_checkInProgress)
                    return;
                
                ms_checkInProgress = true;
                var checker = new VersionCheckerService();
                object[] versionInfo = checker.getLatestVersionInfo(m_appMappingName);
                string strServerVersion = ((string)versionInfo[0]).Trim();
                string[] arrStr = strServerVersion.Split(' ');
                ServerVersion = new Version(arrStr[arrStr.Length - 1]);

                string url = null;

                if (ServerVersion > AppVersion)
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
                ms_checkInProgress = false;
            }

        }

        /// <summary>
        /// Invoke the Event on the subscriber's thread not the publisher's thread</summary>
        private void NotifyClients(string msg, bool error)
        {
            Application.Current.Dispatcher.InvokeIfRequired(
                delegate
                    {
                        if (CheckComplete != null)
                        {
                            var handler = CheckComplete;
                            if (handler != null)
                                handler(msg, error);
                        }
                    });
        }

        private readonly string m_appMappingName;
        private static bool ms_checkInProgress;
    }
}

