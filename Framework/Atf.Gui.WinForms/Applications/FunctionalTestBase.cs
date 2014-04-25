//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Base testing facilities</summary>
    public abstract class FunctionalTestBase
    {
        /// <summary>
        /// Test cleanup, to be called after execution of each test (pass or fail).
        /// Logs output, and cleans up any lingering process.
        /// To avoid adding a dependency on NUnit in this class,
        /// derived classes should call this within NUnit's TearDown method, e.g.:
        ///     [TearDown]
        ///     public virtual void TearDown()
        ///     {
        ///         TestCleanup();
        ///     }
        ///  </summary>
        public virtual void TestCleanup()
        {
            if (m_tagConsoleOutput)
                Console.WriteLine("===== Application's console output =====");
            Console.WriteLine(m_scriptOutput.ToString());
            if (m_tagConsoleOutput)
                Console.WriteLine("===== End console output =====");
            m_scriptOutput = new StringBuilder();

            m_timeOutInSecs = DefaultTimeOutInSeconds;
            try
            {
                if (m_automationService != null)
                {
                    m_automationService.Dispose();
                    m_automationService = null;
                }
            }
            catch { }

            //Kill the application process if it is still running
            if (m_process != null)
            {
                try
                {
                    if (!m_process.HasExited)
                    {
                        m_process.Kill();
                    }
                }
                catch { }

                m_process = null;
            }
        }

        /// <summary>
        /// Get or set the fixed port number used for remote testing</summary>
        /// <remarks>The default value 0 indicates using dynamic port number that is auto-generated</remarks>
        public int FixedRemoteTestingPort
        {
            get { return m_fixedRemoteTestingPort; }
            set { m_fixedRemoteTestingPort = value; }
        }

        /// <summary>
        /// Get or set the time out in seconds used for each individual test</summary>
        /// <remarks>The default is 30 seconds</remarks>
        public int TimeOutInSecs
        {
            get { return m_timeOutInSecs; }
            set { m_timeOutInSecs = value; }
        }

        /// <summary>
        /// Gets the AutomationService object, which is used for 
        /// communicating with the application under test</summary>
        public AutomationService AutomationService
        {
            get { return m_automationService; }
        }
        
        /// <summary>
        /// Get or set whether the console output be tagged from test script vs. from the application</summary>
        /// <remarks>Sometimes it is difficult to tell the difference between output from the test script vs. from the application.  
        /// Distinguishing between the two is helpful when debugging a failing test, especially if a test fails on the build server but not locally, 
        /// and all we have to debug the fail is the log output</remarks>
        public bool TagConsoleOutput 
        {
            get { return m_tagConsoleOutput; }
            set { m_tagConsoleOutput = value; }
        }

        /// <summary>
        /// Derived classes must define the path the application under test</summary>
        /// <returns>Path to the application's .exe file</returns>
        protected abstract string GetAppExePath();

        /// <summary>
        /// Derived classes must define the path to the folder where test scripts exist</summary>
        /// <returns>Path to the directory containing test scripts</returns>
        protected abstract string GetScriptsDirectoryPath();

        /// <summary>
        /// Sets up the applicaton for automation, runs the script,
        /// processes the result, and closes the app</summary>
        /// <param name="scriptPath">Path to the test script to run</param>
        protected void ExecuteFullTest(string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException("Script file not found: " + scriptPath);
            }
            Console.WriteLine("Starting test script: {0}", scriptPath);

            //Must setup our own app settings before launching the process
            SetupAppSettings();
            int port;
            LaunchTestApplication(GetAppExePath(), out port);
            Connect(port);
            SetupScript(scriptPath);
            ProcessScript(scriptPath);

            CloseApplication();
            VerifyApplicationClosed();
        }

        /// <summary>
        /// To make tests more predictable (and hence reliable), automation needs to specify
        /// some settings.  (For example, turn off the auto-load previously opened documents
        /// function, which can lead to popup dialogs if a previous document has an error or has moved).
        /// Do this by copying our own "AutomationSettings.xml" file to the executable directory.
        /// When the application under test is launched, the AutomationCommandLineExecutor 
        /// will tell the settings service to use this file, bypassing any user-defined settings.
        /// 
        /// Note the AutomationSettings.xml is expected to be found at:
        /// [folderContainingTestProject.exe]\Resources\AutomationSettings.xml
        /// (In other words, the test project needs to copy the AutomationSettings.xml file 
        /// to its output under a "Resources" directory). </summary>
        protected virtual void SetupAppSettings()
        {
            //Copy our own DefaultSettings.xml to the same folder as the exe we are going to launch
            string exeDir = Path.GetDirectoryName(GetAppExePath());
            string defaultSettingsDstPath = Path.Combine(exeDir, "AutomationSettings.xml");
            string defaultSettingsSrcPath = Path.Combine(@".\Resources", "AutomationSettings.xml");
            if (File.Exists(defaultSettingsDstPath))
            {
                //More reliable delete if destination is read-only
                File.SetAttributes(defaultSettingsDstPath, FileAttributes.Normal);
                File.Delete(defaultSettingsDstPath);
            }

            if (File.Exists(defaultSettingsSrcPath))
                File.Copy(defaultSettingsSrcPath, defaultSettingsDstPath, true);
            else
                Console.WriteLine("AutomationSettings.xml file not found");
        }

        /// <summary>
        /// Launches the specified ATF application in automation mode.
        /// "Automation mode" simply means passing an "-automation" flag to the exe, which
        /// tells the app to initialize the AutomationService</summary>
        /// <param name="appExePath">Path to the .exe</param>
        /// <param name="port">Port number of the AutomationService</param>
        /// <returns>The launched process</returns>
        protected Process LaunchTestApplication(string appExePath, out int port)
        {
            Check(File.Exists(appExePath), "Verify app exe exists: " + appExePath);

            if (m_fixedRemoteTestingPort == 0)
                port = AutomationService.GetUniquePortNumber();
            else
                port = m_fixedRemoteTestingPort;
            m_process = new Process();
            m_process.StartInfo.FileName = appExePath;
            m_process.StartInfo.Arguments = string.Format("-automation -port {0}", port);
            m_process.StartInfo.WorkingDirectory = Path.GetDirectoryName(appExePath);
            m_process.StartInfo.UseShellExecute = false;
            m_process.StartInfo.RedirectStandardOutput = true;

            m_process.Start();

            //Process the output as it is received, otherwise long scripts timeout because
            //the buffer fills up.
            m_process.OutputDataReceived += new DataReceivedEventHandler(Process_OutputDataReceived);
            m_process.BeginOutputReadLine();

            return m_process;
        }

        /// <summary>
        /// Connects to the AutomationService.  Tries multiple times, to allow the application
        /// time to launch and initialize</summary>
        /// <param name="port"></param>
        /// <returns>The AutomationService when successfully connected</returns>
        protected AutomationService Connect(int port)
        {
            Console.WriteLine("PortNumber: {0}", port);
            bool connected = false;
            int cnt = 0;
            //The application may still be initializing, so allow a few tries before failing.
            //The legacy AssetManager is consistently the slowest to initialize, sometimes taking
            //close to a full second.  I'll pad this by 10x in case other machines are slower
            while (!connected && cnt++ < 100)
            {
                try
                {
                    //RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
                    string uri = string.Format("tcp://localhost:{0}/{1}",
                                                port, AutomationService.TcpName);

                    //Getting the object works, even if the service hasn't started yet
                    m_automationService = (AutomationService)Activator.GetObject(typeof(AutomationService), uri);
                    //... but calling a method will fail if the service isn't ready
                    connected = m_automationService.Connect();
                }
                catch
                {
                    Console.WriteLine("Automation service exception on try#{0}", cnt);
                }
                if (!connected)
                {
                    if (m_process.HasExited)
                    {
                        Console.WriteLine("Not connected and process has exited, quitting early (app might have crashed)");
                        break;
                    }
                    Console.WriteLine("Automation service not ready on try#{0}", cnt);
                    Thread.Sleep(200);
                }
            }

            if (!connected)
            {
                Console.WriteLine("Connecting to automation service failed, process info:");
                Console.WriteLine("  Has exited: {0}", m_process.HasExited);
                if (m_process.HasExited)
                {
                    Console.WriteLine("  Exit code: {0}", m_process.ExitCode);
                }
                else
                {
                    Console.WriteLine("  is responding: {0}", m_process.Responding);
                }
            }
            Check(connected, "Verify can connect to the automation service");

            return m_automationService;
        }

        /// <summary>
        /// This is one time setup before executing the actual script file</summary>
        /// <param name="scriptPath">Path to the script to be executed</param>
        protected virtual void SetupScript(string scriptPath)
        {
            //Add the script's directory to the system path to make loading other modules easier
            string curScriptDir = Path.GetDirectoryName(scriptPath).Replace("\\", "/");
            ExecuteStatementSafe(string.Format("import sys; sys.path.append(\"{0}\")", curScriptDir));
        }

        /// <summary>
        /// Executes script and verifies the result succeeded.  The last
        /// message from the script needs to be "Success" to succeed</summary>
        /// <param name="scriptPath"></param>
        protected void ProcessScript(string scriptPath)
        {
            string fullResult = ExecuteScriptSafe(scriptPath);

            if (m_tagConsoleOutput)
                Console.WriteLine("===== Script output ======");
            Console.WriteLine(fullResult);
            if (m_tagConsoleOutput)
                Console.WriteLine("===== End script output ======");
            //An empty string from a script is possible, but bad practice, so still fail on an empty string
            Check(!string.IsNullOrEmpty(fullResult), "Verify script returned a result");
            
            //Successfully ran scripts will print "Success" at the end of execution.  Failed scripts
            //will print an exception with some type of error message.  Check the last message, and make sure
            //the expected message was printed.  If not, return an error code
            string[] resultLines = fullResult.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string lastMsg = resultLines[resultLines.Length - 1];

            if (lastMsg.ToLower().CompareTo("success") != 0)
            {
                //On an exception, the script result is the stack trace and exception message.
                //Fail on the full result, so the bamboo summary page will show the fail message
                //and stack trace without having to dig into the full log.
                Check(false, fullResult);
            }
        }

        /// <summary>
        /// Some tests require executing a python statement that expects no result</summary>
        /// <param name="statement">Python statement to execute</param>
        protected void ExecuteStatementVerifyNoResult(string statement)
        {
            string result = ExecuteStatementSafe(statement);
            if (!string.IsNullOrEmpty(result))
            {
                Console.WriteLine("Statement result: {0}", result);
                Check(false, result);
            }
        }

        /// <summary>
        /// Executes a python statement on the application in a separate thread 
        /// with a default timeout (1s).  If the statement exceeds the timeout,
        /// the test is failed.  This ensures executing the statement will not
        /// freeze the whole test.</summary>
        /// <param name="statement">Python statement to execute</param>
        /// <returns>The message returned by python</returns>
        protected string ExecuteStatementSafe(string statement)
        {
            return ExecuteStatementSafe(statement, 1);
        }
        /// <summary>
        /// Executes a python statement on the application in a separate thread 
        /// with the specified timeout.  If the statement exceeds the timeout,
        /// the test is failed.  This ensures executing the statement will not
        /// freeze the whole test.</summary>
        /// <param name="statement">Python statement to execute</param>
        /// <param name="timeoutInSec">Timeout in seconds</param>
        /// <returns>The message returned by python</returns>
        protected string ExecuteStatementSafe(string statement, int timeoutInSec)
        {
            string result = null;
            ThreadStart tStart = new ThreadStart(delegate()
                {
                    try
                    {
                        result = m_automationService.ExecuteStatement(statement);
                    }
                    //If this call does timeout, then an exception may cause a crash when the connection is forced closed
                    catch (SocketException)
                    { }
                });
            Thread t = new Thread(tStart);
            t.Start();

            if (!t.Join(1000 * timeoutInSec))
            {
                throw new TimeoutException(string.Format("Executing statement timedout: {0}", statement));
            }

            if (!string.IsNullOrEmpty(result))
            {
                result = result.Trim();
            }

            return result;
        }

        /// <summary>
        /// Executes a python script on the application in a separate thread 
        /// with a default timeout (30 seconds).  If the statement exceeds the timeout,
        /// the test is failed.  This ensures executing the script will not
        /// freeze the whole test.</summary>
        /// <param name="scriptPath">Path to the python script</param>
        /// <returns>The message returned by python</returns>
        protected string ExecuteScriptSafe(string scriptPath)
        {
            return ExecuteScriptSafe(scriptPath, m_timeOutInSecs);
        }
        /// <summary>
        /// Executes a python script on the application in a separate thread 
        /// with the specified timeout.  If the statement exceeds the timeout,
        /// the test is failed.  This ensures executing the script will not
        /// freeze the whole test.</summary>
        /// <param name="scriptPath">Path to the python script</param>
        /// <param name="timeoutInSecs">Timeout in seconds</param>
        /// <returns>The message returned by python</returns>
        protected string ExecuteScriptSafe(string scriptPath, int timeoutInSecs)
        {
            string result = null;
            ThreadStart tStart = new ThreadStart(delegate()
            {
                try
                {
                    result = m_automationService.ExecuteScript(scriptPath);
                }
                //If this call does timeout, then an exception may cause a crash when the connection is forced closed
                catch (SocketException)
                { }
            });
            Thread t = new Thread(tStart);
            t.Start();

            if (!t.Join(1000 * timeoutInSecs))
            {
                Console.WriteLine("Script timed out: {0}", scriptPath);
                throw new TimeoutException(string.Format("Executing script timed out: {0}", scriptPath));
            }

            return result;
        }

        /// <summary>
        /// Closes the application and checks for errors in these steps:
        /// 1) Discard all documents (close without saving)
        /// 2) Check the application for any error dialogs
        /// 3) Execute the FileExit command, which closes the application</summary>
        protected virtual void CloseApplication()
        {
            //Before closing app, close all documents, discarding any unsaved changes
            ExecuteStatementSafe("atfFile.DiscardAll()", 20);
            //Not all apps have documents, so an error from the DiscardAll command is okay (consider checking this on a per app basis)
            //Assert.True(string.IsNullOrEmpty(result), "Verify no error after discarding documents: " + result);

            //This will check for error dialogs and if an uncaught exception occurred.
            //Do this as late as possible, but must be done before closing the app.
            Check(!m_automationService.CheckUnexpectedErrors(), "Verify no unexpected errors have occurred");

            try
            {
                //This is the close application command for ATF3
                string result = ExecuteStatementSafe("atfFileExit.DoCommand(StandardCommand.FileExit)", 15);
                Check(string.IsNullOrEmpty(result), "Verify no error when closing application: " + result);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Socket exception when closing the application: {0}", e.Message);
                //Might need to kill the process here?
            }
        }
        
        /// <summary>
        /// Verifies the application has actually exited.  Uses looping to give the application
        /// sufficient time on slow systems before failing</summary>
        protected void VerifyApplicationClosed()
        {
            int cnt = 0;

            //The process generally exits in about 200ms, but if an error occurs
            //it takes much longer.  So allow up to 10 seconds, so in case of an abnormal
            //exit we can tell the difference between the application not closing at all
            //and an odd exit code
            while (!m_process.HasExited && cnt++ < 100)
            {
                Thread.Sleep(100);
            }

            Console.WriteLine("Process exited is {0} on loop#{1}", m_process.HasExited, cnt);
            Check(m_process.HasExited, "Verify process exited");
            Check(m_process.ExitCode == 0, "Verify process exited gracefully");
        }

        /// <summary>
        /// Constructs the script path.  Assumes the calling method is the test name,
        /// which matches the script name.</summary>
        /// <returns></returns>
        protected string ConstructScriptPath()
        {
            string testName = new StackTrace().GetFrame(1).GetMethod().Name;
            string path = Path.Combine(GetScriptsDirectoryPath(), testName + ".py");
            return path;
        }

        /// <summary>
        /// Functional tests exception class</summary>
        public class FunctionalTestException : Exception
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="message">Exception message</param>
            public FunctionalTestException(string message) : base(message)
            { }
        }

        private void Check(bool condition, string message)
        {
            if (!condition)
                throw new FunctionalTestException(message);
        }

        /// <summary>
        /// Accumulates the log output from the process as it runs.  If this is only collected
        /// at the end, the process will freeze if the buffer fills up</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Data received event arguments</param>
        protected void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            m_scriptOutput.AppendLine(e.Data);
        }

        private const int DefaultTimeOutInSeconds = 30;

        private int m_timeOutInSecs = DefaultTimeOutInSeconds;
        private Process m_process;
        private AutomationService m_automationService;
        private StringBuilder m_scriptOutput = new StringBuilder();
        private int m_fixedRemoteTestingPort = 0;
        private bool m_tagConsoleOutput = true;
    }
}
