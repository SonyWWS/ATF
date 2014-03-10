//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Reflection;

using NUnit.Core;
using NUnit.Util;

namespace UnitTests
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
            return RunAllTests(assemblyName);
        }

        /// <summary>
        /// Runs all tests.
        /// </summary>
        /// <param name="testAssemblyName">Name of the test assembly.</param>
        /// <returns>0 if tests ran successfully, -1 otherwise</returns>
        public static int RunAllTests(string testAssemblyName)
        {
            TestRunner runner;
            try
            {
                runner = MakeTestRunner(testAssemblyName);
            }
            catch (System.IO.FileLoadException)
            {
                // likely caused by ATF source zip file downloaded from internet without unblocking it
                Console.WriteLine("NUnit failed to load {0}", testAssemblyName);
                Console.WriteLine(@"Possibly need to unblock the downloaded ATF source zip file before unzipping");
                Console.WriteLine(@"(right click on the zip file -> Properties -> Unblock)");
                return -3;
            }
            catch (Exception)
            {
                return -2;

            }
            
            runner.Run(new UnitTestListener());

            if (runner.TestResult.IsFailure)
                return -1;

            return 0;
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
        public void TestOutput(TestOutput testOutput) { }
        public void TestStarted(TestName testName) { }
        public void UnhandledException(Exception exception) { }

        public void TestFinished(TestResult result)
        {
            if (result.IsSuccess)
                return;

            string failureText = result.Name + ": " + result.Message;

            if (result.StackTrace != null)
            {
                try
                {
                    string[] stackFrames =
                        result.StackTrace.Split(new string[] { "at " }, StringSplitOptions.RemoveEmptyEntries);

                    string[] callSite =
                        stackFrames[stackFrames.Length - 1].Split(new string[] { " in " },
                                                                  StringSplitOptions.RemoveEmptyEntries);

                    string[] fileLine = callSite[1].Split(new string[] { ":line " }, StringSplitOptions.None);

                    string failingTest = callSite[0];
                    string file = fileLine[0];
                    string line = fileLine[1];

                    string message = "";
                    if (result.Message != "")
                        message = ": " + result.Message.Replace("\r\n", "").Replace("\t", " ");

                    failureText = file + "(" + line + "): error UT0000: " + failingTest + message;
                }
                catch
                {
                    //The above prints out the specific failing line/file, but doesn't work when
                    //running directly from the command line.
                }
            }

            Console.WriteLine(failureText);
        }
    }
}
