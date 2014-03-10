//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Net.Sockets;

using NUnit.Framework;
using Sce.Atf.Applications;

namespace FunctionalTests
{
    public abstract class TestBase : FunctionalTestBase
    {
        [TearDown]
        public virtual void TearDown()
        {
            TestCleanup();
        }

        /// <summary>
        /// This is one time setup before executing the actual script file
        /// </summary>
        /// <param name="scriptPath"></param>
        protected override void SetupScript(string scriptPath)
        {
            string commonScriptDirPath = Path.Combine(Path.GetDirectoryName(scriptPath), @"..\CommonTestScripts");
            //Python doesn't always like back slashes
            commonScriptDirPath = commonScriptDirPath.Replace("\\", "/");
            //Add the common scripts folder to the path
            ExecuteStatementSafe(string.Format("import sys; sys.path.append(\"{0}\")", commonScriptDirPath));

            //It's also always good to have the current folder in the path
            string curScriptDir = Path.GetDirectoryName(scriptPath).Replace("\\", "/");
            ExecuteStatementSafe(string.Format("sys.path.append(\"{0}\")", curScriptDir));
        }

        protected abstract string GetAppName();

        protected override string GetAppExePath()
        {
            return Path.GetFullPath(string.Format(@"..\{0}.exe", GetAppName()));
        }

        protected override string GetScriptsDirectoryPath()
        {
            return Path.GetFullPath(string.Format(@".\{0}TestScripts", GetAppName()));
        }

        public class Consts
        {
            public const string RunAllTestsCategory = "All";
            public const string SmokeTestCategory = "SmokeTest";
            public const string ReloadFileWindowTitle = "File Changed, Reload?";
        }
    }

    public abstract class TestBaseLegacy : TestBase
    {
        protected override string GetAppExePath()
        {
            return Path.GetFullPath(string.Format(@"..\Legacy\{0}.exe", GetAppName()));
        }

        /// <summary>
        /// This is one time setup before executing the actual script file
        /// </summary>
        /// <param name="scriptPath"></param>
        protected override void SetupScript(string scriptPath)
        {
            string commonScriptDirPath = Path.Combine(Path.GetDirectoryName(scriptPath), @"..\..\CommonTestScripts");
            commonScriptDirPath = commonScriptDirPath.Replace("\\", "/");
            ExecuteStatementSafe(string.Format("import sys; sys.path.append(\"{0}\")", commonScriptDirPath));

            //It's also always good to have the current folder in the path
            string curScriptDir = Path.GetDirectoryName(scriptPath).Replace("\\", "/");
            ExecuteStatementSafe(string.Format("sys.path.append(\"{0}\")", curScriptDir));
        }

        protected override void CloseApplication()
        {
            //Before closing app, close all documents, discarding any unsaved changes
            AutomationService.ExecuteStatement("atfFile.DiscardAll()");
            //Not all apps have documents, so an error from the DiscardAll command is okay (consider checking this on a per app basis)
            //Assert.True(string.IsNullOrEmpty(result), "Verify no error after discarding documents: " + result);
            
            //This will check for error dialogs and if an uncaught exception occurred.
            //Do this as late as possible, but must be done before closing the app.
            Assert.False(AutomationService.CheckUnexpectedErrors(), "Verify no unexpected errors have occurred");

            try
            {
                //The ATF2 command for closing the application
                string result = AutomationService.ExecuteStatement("atfAppHostService.DoCommand(Command.FileExit)");
                Assert.True(string.IsNullOrEmpty(result), "Verify no error when closing application: " + result);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Socket exception when closing the application: {0}", e.Message);
                //Might need to kill the process here?
            }
        }
    }
}
