//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
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

            var assemblyPaths = new List<string>
            {
                //Assembly.GetExecutingAssembly().Location,
                AppDomain.CurrentDomain.Load(new AssemblyName("NativeTestHelpers")).Location
            };

            string displayName = Assembly.GetExecutingAssembly().Location;

            return RunAllTests(displayName, assemblyPaths);
        }

        /// <summary>
        /// Runs all tests</summary>
        /// <param name="displayName">Name of the test, which is normally the executing
        /// assembly's Location.</param>
        /// <param name="assemblyPaths">List of assemblies to actually test</param>
        /// <returns>0 if tests ran successfully, a negative number otherwise</returns>
        public static int RunAllTests(string displayName, List<string> assemblyPaths)
        {
            TestRunner runner;
            try
            {
                var package = new TestPackage(displayName, assemblyPaths);
                runner = new DefaultTestRunnerFactory().MakeTestRunner(package);
                runner.Load(package);
            }
            catch (System.IO.FileLoadException)
            {
                // likely caused by ATF source zip file downloaded from internet without unblocking it
                Console.WriteLine("NUnit failed to load {0}", displayName);
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
