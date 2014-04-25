//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Provides facilities to run an automated script using the .NET remoting service</summary>
    [Serializable]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AutomationService : MarshalByRefObject, IDisposable, IInitializable
    {
        /// <summary>
        /// Retrieves a handle to the top-level window whose class name and window name match the specified strings</summary>
        /// <param name="lpClassName">Class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx function</param>
        /// <param name="lpWindowName">Window name (the window's title)</param>
        /// <returns>If the function succeeds, the return value is a handle to the window that has the specified class name and window name</returns>
        /// <remarks>Description copied from http://msdn.microsoft.com/en-us/library/windows/desktop/ms633499%28v=vs.85%29.aspx. </remarks>
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern Int32 FindWindow(String lpClassName, String lpWindowName);
        
        /// <summary>
        /// Sends the specified message to a window or windows</summary>
        /// <param name="hWnd">Handle to the window whose window procedure will receive the message</param>
        /// <param name="msg">Message to be sent</param>
        /// <param name="wParam">Additional message-specific information</param>
        /// <param name="lParam">Additional message-specific information</param>
        /// <returns>Result of the message processing</returns>
        /// <remarks>Description copied from http://msdn.microsoft.com/en-us/library/windows/desktop/ms644950%28v=vs.85%29.aspx. </remarks>
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(int hWnd, int msg, int wParam, int lParam);

        /// <summary>
        /// Places (posts) a message in the message queue associated with the thread that created the specified window 
        /// and returns without waiting for the thread to process the message</summary>
        /// <param name="hWnd">Handle to the window whose window procedure is to receive the message</param>
        /// <param name="msg">Message to be posted</param>
        /// <param name="wParam">Additional message-specific information</param>
        /// <param name="lParam">Additional message-specific information</param>
        /// <returns>Additional message-specific information</returns>
        /// <remarks>Description copied from http://msdn.microsoft.com/en-us/library/windows/desktop/ms644944%28v=vs.85%29.aspx. </remarks>
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(int hWnd, int msg, int wParam, int lParam);

        /// <summary>
        /// Constructor</summary>
        public AutomationService()
        {
            m_enableAutomation = false;
            m_port = String.Empty;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++ )
            {
                if (args[i].ToLower().CompareTo("-automation") == 0)
                {
                    m_enableAutomation = true;
                }
                else if ((args[i].ToLower().CompareTo("-port") == 0) && (i < args.Length - 1))
                {
                    m_port = args[i + 1];
                    i++;
                }
            }

            if (m_enableAutomation && string.IsNullOrEmpty(m_port))
            {
                Outputs.WriteLine(OutputMessageType.Warning, "AutomationService is enabled, but no port was specified. Using old default");
                m_port = Port;
            }
        }

        /// <summary>
        /// Destructor</summary>
        ~AutomationService()
        {
            Dispose();
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            if (m_enableAutomation)
            {
                try
                {
                    StartService();
                    s_scriptingService = m_scriptingService;
                    if (m_settingsService != null)
                    {
                        m_settingsService.SettingsPath = Path.Combine(Environment.CurrentDirectory, "AutomationSettings.xml");
                    }
                    //Catch unhandled exceptions, and fail test if one occurs
                    Application.ThreadException += Application_ThreadException;
                    //Don't start test until main form is loaded
                    m_mainWindow.Loaded += m_mainWindow_Loaded;

                    s_liveConnectService = m_liveConnectService;
                    if (s_liveConnectService != null)
                        s_liveConnectService.MessageReceived += s_liveConnectService_MessageReceived;
                }
                catch { }
            }
        }
        
        #endregion

        /// <summary>
        /// Starts the .NET remoting service</summary>
        protected void StartService()
        {
            try
            {
                if (s_dispatcher == null)
                {
                    s_dispatcher = Dispatcher.CurrentDispatcher;
                }

                BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
                provider.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

                Dictionary<string, string> property = new Dictionary<string, string>();
                property["port"] = m_port;
                //Use a unique name each time in case cleanup fails
                property["name"] = Guid.NewGuid().ToString();

                s_registeredChannel = new TcpChannel(property, null, provider);
                ChannelServices.RegisterChannel(s_registeredChannel, false);
                RemotingConfiguration.RegisterWellKnownServiceType(this.GetType(),
                                            TcpName, WellKnownObjectMode.Singleton);
            }
            catch { }
        }

        /// <summary>
        /// Gets a unique port number</summary>
        /// <returns>Unique port number</returns>
        public static int GetUniquePortNumber()
        {
            const int basePortNumber = 4000;
            const int rangeSize = 2000;
            return basePortNumber + (DateTime.Now.Second + DateTime.Now.Millisecond) % rangeSize;
        }

        /// <summary>
        /// Port for .NET remoting service</summary>
        /// <remarks>Eventually delete this public default Port, but leave for now to make older
        /// component version compatible with the ATF_WIP branch.</remarks>
        public const string Port = "2334";
        /// <summary>
        /// URI for service type</summary>
        public const string TcpName = "AutomationChannel";

        /// <summary>
        /// Disposes of unmanaged resources</summary>
        public void Dispose()
        {
            if (s_registeredChannel != null)
            {
                try
                {
                    ChannelServices.UnregisterChannel(s_registeredChannel);
                    RemotingServices.Disconnect(this);
                    s_registeredChannel.StopListening(null);
                    s_registeredChannel = null;
                }
                catch { }
            }
            s_dispatcher = null;
        }
        
        /// <summary>
        /// Gets the result of running the script:
        ///  0: No script was executed.
        ///  1: Script executed successfully.
        ///  -1: Error while executing the script (did not get "Success" message back).
        ///  Other: some error occurred.</summary>
        public int Result
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets whether valid command line arguments were passed in to run an automated script</summary>
        public bool EnableAutomation
        {
            get { return m_enableAutomation; }
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            s_unhandledException = true;
            Console.WriteLine("Unhandled exception occurred: {0}", e.Exception.Message);
            Console.WriteLine("Stack trace: {0}", e.Exception.StackTrace);
        }

        private void m_mainWindow_Loaded(object sender, EventArgs e)
        {
            s_mainFormLoaded = true;
        }
        
        /// <summary>
        /// Checks if any error occurs. This can be an unhandled exception or
        /// an error dialog (checked by name and by type of known error dialogs).</summary>
        /// <returns>True iff error occurred</returns>
        public bool CheckUnexpectedErrors()
        {
            if (s_unhandledException)
            {
                return true;
            }

            //Don't use foreach, which can lead to a "collection was modified" error.
            //Iterate last to first in case a form does close, to avoid out of bounds index.
            for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
            {
                Form frm = Application.OpenForms[i];
                if (frm.Text == "Error" ||
                    frm.Text == "Unexpected Error" ||
                    frm.Text == "Microsoft .NET Framework" ||
                    frm.GetType() == typeof(ThreadExceptionDialog))
                {
                    Console.WriteLine("Form indicating error found: {0}", frm.ToString());
                    return true;
                }
            }

            return false;
        }
        
        private delegate string FnExecute(string statement);

        /// <summary>
        /// Executes a single statement</summary>
        /// <param name="statement">Statement</param>
        /// <returns>Result of statement executing</returns>
        public string ExecuteStatement(string statement)
        {
            //Must execute on the main thread to avoid various errors
            FnExecute f = new FnExecute(s_scriptingService.ExecuteStatement);
            return (string)s_dispatcher.Invoke(f, statement);
        }

        /// <summary>
        /// Executes a script file</summary>
        /// <param name="scriptPath">Script file path</param>
        /// <returns>Result of script file executing</returns>
        public string ExecuteScript(string scriptPath)
        {
            //Must execute on the main thread to avoid various errors
            FnExecute f = new FnExecute(s_scriptingService.ExecuteFile);
            return (string)s_dispatcher.Invoke(f, scriptPath);
        }

        /// <summary>
        /// Called by test client to verify connection is ready. If this call throws
        /// an exception, most likely the service has not finished starting yet. Returns
        /// false if the main form has not finished loading yet.</summary>
        /// <returns>True iff the application is done loading</returns>
        public bool Connect()
        {
            if (s_mainFormLoaded)
            {
                ExecuteStatement("print(\"Automation has connected\")");
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Searches through all open forms and dialogs for one with the 
        /// specified text. If a form is found, the default "AcceptButton"
        /// is clicked. If a dialog is found, a keyboard "return" message is 
        /// sent to the dialog to select the default action.</summary>
        /// <param name="formText">Text to search for</param>
        /// <returns>True iff text found</returns>
        public bool ClickButton(string formText)
        {
            bool found = false;
            //This will find the dialog if it is a form:
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.Text == formText)
                {
                    Console.WriteLine("Found form with text: {0}", formText);
                    found = true;
                    if (frm.AcceptButton != null)
                    {
                        frm.AcceptButton.PerformClick();
                        break;
                    }
                }
            }

            if (!found)
            {
                //This will work for non dialogs (e.g. MessageBox.Show())
                int win = FindWindow(null, formText);
                if (win != 0)
                {
                    Console.WriteLine("Found dialog window with text: {0}", formText);
                    const int WM_KEYDOWN = 0x100;
                    const int VK_RETURN = 0x0D;
                    PostMessage(win, WM_KEYDOWN, VK_RETURN, 0);
                    found = true;

                    //None of these work:
                    //const int WM_KEYUP = 0x101;
                    //const int VK_SPACE = 0x20;
                    //SendMessage(win, WM_KEYDOWN, VK_SPACE, 0);
                    //SendMessage(win, WM_KEYUP, VK_SPACE, 0);
                    //SendMessage(win, WM_KEYDOWN, VK_RETURN, 0);
                    //SendMessage(win, WM_KEYUP, VK_RETURN, 0);
                    //PostMessage(win, WM_KEYDOWN, VK_SPACE, 0);
                }
            }

            return found;
        }

        /// <summary>
        /// Sends message via LiveConnect</summary>
        /// <param name="msg">Message</param>
        public void SendMessage(string msg)
        {
            s_liveConnectService.Send(msg);
        }

        /// <summary>
        /// Gets last message that was received via LiveConnect</summary>
        /// <returns>Last message</returns>
        public string GetLastMessage()
        {
            return s_lastMessage;
        }

        /// <summary>
        /// Performs custom actions on Live Connect message received</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void s_liveConnectService_MessageReceived(object sender, LiveConnectService.LiveConnectMessageArgs e)
        {
            //For testing purposes we ignore messages from other machines so we don't have to worry
            //about messages from other machines interfering with a test
            if (e.SenderName.ToLower().Contains(Environment.MachineName.ToLower()))
                s_lastMessage = e.MessageString;
        }

        /// <summary>
        ///Scripting service</summary>
        [NonSerialized]
        [Import(AllowDefault = true)]
        protected ScriptingService m_scriptingService;

        /// <summary>
        /// Settings service</summary>
        [NonSerialized]
        [Import(AllowDefault = true)]
        protected SettingsService m_settingsService;
        
        [NonSerialized]
        [Import(AllowDefault = true)]
        private IMainWindow m_mainWindow;

        [Import(AllowDefault = true)]
        private LiveConnectService m_liveConnectService;
        
        /// <summary>
        /// Whether valid command line arguments were passed in to run an automated script</summary>
        protected bool m_enableAutomation;

        /// <summary>
        /// Message port</summary>
        protected string m_port;

        //Members used in a method called by a remote client must be static, since the remote
        //client creates its own instance of this class.  The ScriptingService must be
        //declared twice, since the MEF component won't initialize if it is a static member.
        /// <summary>
        /// ScriptingService</summary>
        protected static ScriptingService s_scriptingService;
        private static bool s_unhandledException;
        private static TcpChannel s_registeredChannel;
        private static Dispatcher s_dispatcher;
        private static LiveConnectService s_liveConnectService;
        private static string s_lastMessage;
        /// <summary>
        /// Indicates whether main form loaded</summary>
        protected static bool s_mainFormLoaded;
    }
}
