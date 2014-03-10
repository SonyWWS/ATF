//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

using NUnit.Framework;

namespace FunctionalTests.Legacy
{
    [TestFixture]
    public class UsingDomLegacyTests : TestBaseLegacy
    {
        protected override string GetAppName()
        {
            return "UsingDom";
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void LaunchApplication()
        {
            //Launching this app saves a file to <pathToExe>\game.xml.
            //So delete that file if it exists, launch the app, then make sure the file was saved and is valid XML
            string gameXmlPath = Path.Combine(Path.GetDirectoryName(GetAppExePath()), "game.xml");
            if (File.Exists(gameXmlPath))
            {
                File.SetAttributes(gameXmlPath, FileAttributes.Normal);
                File.Delete(gameXmlPath);
            }

            Assert.False(File.Exists(gameXmlPath), "Verify file does not exist before running application");
            
            Process p = new Process();
            p.StartInfo.FileName = GetAppExePath();
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(GetAppExePath());
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;

            p.Start();

            if (p.WaitForExit(1000 * TimeOutInSecs))
            {
                string output = p.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
                Assert.True(File.Exists(gameXmlPath), "Verify app saved the file");
                XmlDocument doc = new XmlDocument();
                doc.Load(gameXmlPath);
                Assert.False(string.IsNullOrEmpty(doc.OuterXml), "Verify file is valid xml");
            }
            else
            {
                //Timedout
                p.Kill();
                throw new TimeoutException("App did not close on its own");
            }
        }
    }
}