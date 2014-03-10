//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Xsl;

using NUnit.Core;
using NUnit.Util;

namespace FunctionalTests
{
    class Program
    {
        static int Main(string[] args)
        {
            ServiceManager.Services.AddService(new SettingsService());
            ServiceManager.Services.AddService(new DomainManager());
            ServiceManager.Services.AddService(new ProjectService());
            ServiceManager.Services.AddService(new AddinRegistry());
            ServiceManager.Services.AddService(new AddinManager());
            ServiceManager.Services.AddService(new TestAgency());

            string assemblyName = Assembly.GetExecutingAssembly().Location;

            DesktopRecorderLauncher recorder = new DesktopRecorderLauncher();
            recorder.Start();
            int ret = RunAllTests(assemblyName);
            recorder.Stop();

            return ret;
        }

        /// <summary>
        /// Runs all tests.
        /// </summary>
        /// <param name="testAssemblyName">Name of the test assembly.</param>
        /// <returns>0 if tests ran successfully, -1 otherwise</returns>
        public static int RunAllTests(string testAssemblyName)
        {
            TestRunner runner = MakeTestRunner(testAssemblyName);
            string category = ParseTestCategory();
            runner.Run(new UnitTestListener(), new TestFilter(category));
            XmlResultWriter writer = new XmlResultWriter("TestResult.xml");
            writer.SaveTestResult(runner.TestResult);

            XslCompiledTransform xsl = new XslCompiledTransform();
            xsl.Load("Resources/NUnitToJUnit.xslt");
            xsl.Transform("TestResult.xml", "TestResult.JUnit.xml");

            if (runner.TestResult.IsFailure)
                return -1;

            return 0;
        }

        /// <summary>
        /// Parses command line for a test category to run.  For now, only
        /// supports reading a single category. </summary>
        /// <returns></returns>
        private static string ParseTestCategory()
        {
            string category = null;
            const string flag = "-category:";
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                int index = arg.ToLower().IndexOf(flag);
                if (index >= 0)
                {
                    category = arg.Substring(flag.Length);
                    break;
                }
            }

            return category;
        }

        private static TestRunner MakeTestRunner(string testAssemblyName)
        {
            TestPackage package = new TestPackage(testAssemblyName);
            TestRunner runner = new DefaultTestRunnerFactory().MakeTestRunner(package);
            runner.Load(package);

            return runner;
        }
    }

    internal class UnitTestListener : MarshalByRefObject, EventListener
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void RunFinished(Exception exception) { }
        public void RunFinished(TestResult result) { }
        public void RunStarted(string name, int testCount) { }
        public void SuiteFinished(TestResult result) { }
        public void SuiteStarted(TestName testName) { }
        
        public void TestOutput(TestOutput testOutput)
        {
            Console.WriteLine(testOutput.Text);
        }

        public void TestStarted(TestName testName) { }
        public void UnhandledException(Exception exception) { }

        public void TestFinished(TestResult result)
        {
            if (result.IsSuccess)
                return;

            string failureText = result.Name + ": " + result.Message;

            if (result.StackTrace != null)
            {
                string[] stackFrames =
                    result.StackTrace.Split(new string[] { "at " }, StringSplitOptions.RemoveEmptyEntries);

                string[] callSite =
                    stackFrames[stackFrames.Length - 1].Split(new string[] { " in " }, StringSplitOptions.RemoveEmptyEntries);

                string[] fileLine = callSite[1].Split(new string[] { ":line " }, StringSplitOptions.None);

                string failingTest = callSite[0];
                string file = fileLine[0];
                string line = fileLine[1];

                string message = "";
                if (result.Message != "")
                    message = ": " + result.Message.Replace("\r\n", "").Replace("\t", " ");

                failureText = file + "(" + line + "): error UT0000: " + failingTest + message;
            }

            Console.WriteLine(failureText);
        }
    }

    [Serializable]
    internal class TestFilter : ITestFilter
    {
        public TestFilter(string category)
        {
            m_category = category;
        }

        /// <summary>
        /// Never used?
        /// </summary>
        public bool IsEmpty
        {
            get { return false; }
        }

        /// <summary>
        /// Set a breakpoint and this is never hit, not sure what this is for.
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public bool Match(ITest test)
        {
            return true;
        }

        /// <summary>
        /// This function is called to determine whether to run the specified test
        /// or not. </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public bool Pass(ITest test)
        {
            if (test.IsSuite)
            {
                return true;
            }

            if (string.IsNullOrEmpty(m_category) || 
                m_category.ToLower() == TestBase.Consts.RunAllTestsCategory.ToLower())
            {
                return true;
            }

            return test.Categories.Contains(m_category);
        }

        private readonly string m_category;
    }

    internal class DesktopRecorderLauncher
    {
        public void Start()
        {
            try
            {
                bool enabled = false;
                foreach (string arg in Environment.GetCommandLineArgs())
                {
                    if (arg.ToLower() == "-record")
                    {
                        enabled = true;
                        break;
                    }
                }

                if (enabled)
                {
                    string exePath = @"C:\Program Files (x86)\CamStudio 2.6b\camstudio_cl.exe";
                    if (File.Exists(exePath))
                    {
                        string outputFileName = "atf_desktop_recording.avi";
                        if (File.Exists(outputFileName))
                        {
                            File.SetAttributes(outputFileName, FileAttributes.Normal);
                            File.Delete(outputFileName);
                        }
                        m_process = new Process();
                        string args = string.Format("-outfile {0}", outputFileName);
                        m_process.StartInfo = new ProcessStartInfo(exePath, args);
                        m_process.StartInfo.UseShellExecute = false;
                        m_process.StartInfo.RedirectStandardInput = true;
                        m_process.Start();
                    }
                    else
                    {
                        Console.WriteLine("No desktop recording software found, desktop will not be recorded");
                    }
                }
                else
                {
                    Console.WriteLine("Desktop recording not enabled");
                }
            }
            catch { }
        }

        public void Stop()
        {
            if (m_process != null)
            {
                try
                {
                    Console.WriteLine("Closing recording software");
                    using (StreamWriter sw = m_process.StandardInput)
                    {
                        sw.WriteLine(sw.NewLine);
                        sw.Flush();
                    }
                }
                catch { }
            }
        }

        private Process m_process;
    }
}
