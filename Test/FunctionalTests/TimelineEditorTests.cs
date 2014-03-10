//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Threading;
using System.Xml;

using NUnit.Framework;

namespace FunctionalTests
{
    [TestFixture]
    public class TimelineEditorTests : TestBase
    {
        protected override string GetAppName()
        {
            return "TimelineEditor";
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void CreateNewDocument()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(@".\CommonTestScripts", "CreateNewDocument.py"));
            ExecuteFullTest(scriptPath);
        }

        [Test]
        public void AddAllItems()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void CreateReferencedDocument()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void CreateReferencedDocumentInUnsavedTimeline()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void CreateReferencedDocumentInSubDir()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void EditSaveCloseAndReopen()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        //[Test]  //Finding dialog doesn't work on build servers when running from bamboo
        public void ModifyMasterDocumentExternally()
        {
            string scriptPath = Path.Combine(GetScriptsDirectoryPath(), "EditSaveCloseAndReopen" + ".py");
            //Do the normal test stuff, but don't close the application
            SetupAppSettings();
            int port;
            LaunchTestApplication(GetAppExePath(), out port);
            Connect(port);
            SetupScript(scriptPath);
            ProcessScript(scriptPath);

            //A timeline named "EditAndSave.timeline" should now be opened
            string markerName = ExecuteStatementSafe("docNew.Timeline.Markers[0].Name");
            string docPath = ExecuteStatementSafe("docNew.Uri.AbsolutePath");
            //Trim the quotes that are automatically inserted to surround the path
            docPath = docPath.TrimStart(new char[] { '\'', '\"' });
            docPath = docPath.TrimEnd(new char[] { '\'', '\"' });
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(docPath);
            XmlNodeList nodes = xmlDoc.GetElementsByTagName("marker");
            Assert.GreaterOrEqual(nodes.Count, 1,  "Could not find marker node");
            nodes[0].Attributes["name"].Value = "renamed externally";
            //This will cause a popup to reload the document
            xmlDoc.Save(docPath);

            //No guaranteed way to make sure the file watcher event has triggered and the reference file has been reloaded.
            //May need to increase this delay for other/slower computers
            Thread.Sleep(1500);
            Assert.True(AutomationService.ClickButton(Consts.ReloadFileWindowTitle),
                        "Verify reload file dialog found");

            //The docNew reference is old now that the doc is reloaded, so don't use that variable
            string markerNameNew = ExecuteStatementSafe("editor.ActiveDocument.Timeline.Markers[0].Name");
            markerNameNew = markerNameNew.TrimStart(new char[] { '\'', '\"' });
            markerNameNew = markerNameNew.TrimEnd(new char[] { '\'', '\"' });
            Assert.AreEqual("renamed externally", markerNameNew, "Verify marker name was reloaded automatically");

            //Do the normal cleanup
            CloseApplication();
            VerifyApplicationClosed();
        }

        [Test]
        public void ModifyChildDocumentExternally()
        {
            string scriptPath = Path.Combine(GetScriptsDirectoryPath(), "CreateReferencedDocument" + ".py");
            //Do the normal test stuff, but don't close the application
            SetupAppSettings();
            int port;
            LaunchTestApplication(GetAppExePath(), out port);
            Connect(port);
            SetupScript(scriptPath);
            ProcessScript(scriptPath);

            //A timeline named "child.timeline" should now be saved, and still open as "docChild".  
            //Get the path to the child document
            string childDocPath = ExecuteStatementSafe("docChild.Uri.AbsolutePath");
            //Trim the quotes that are automatically inserted to surround the path
            childDocPath = childDocPath.TrimStart(new char[] { '\'', '\"' });
            childDocPath = childDocPath.TrimEnd(new char[] { '\'', '\"' });
            //This is necessary in case the path has a space in it. e.g., replaces "%20" with " "
            childDocPath = Uri.UnescapeDataString(childDocPath);

            //Close the child document to avoid the popup that occurs when a master document is modified
            ExecuteStatementSafe("atfFile.Close(docChild)");

            //Get the current child group name
            string childGroupName = ExecuteStatementSafe("docParent.Timeline.References[0].Target.Groups[0].Name");
            
            //Modify the child group name externally
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(childDocPath);
            XmlNodeList nodes = xmlDoc.GetElementsByTagName("group");
            Assert.GreaterOrEqual(nodes.Count, 1, "Could not find group node");
            string renamedValue = "renamed externally";
            nodes[0].Attributes["name"].Value = renamedValue;
            //The below crashes the TimelineEditor due to a "File being used by another process" error
            //xmlDoc.Save(childDocPath);
            using (FileStream fs = new FileStream(childDocPath, FileMode.Open, FileAccess.Write))
            {
                using (XmlWriter writer = new XmlTextWriter(fs, System.Text.Encoding.UTF8))
                {
                    xmlDoc.Save(writer);
                }
            }

            //No guaranteed way to make sure the file watcher event has triggered and the reference file has been reloaded.
            //May need to increase this delay for other/slower computers
            Thread.Sleep(500);
            //Below doesn't always work on build server when running from bamboo.  (Works locally).
            //Assert.True(m_automationService.ClickButton(Consts.ReloadFileWindowTitle),
            //            "Verify reload file dialog found");

            //Now verify the reference document was reloaded automatically:
            string childGroupNameNew = ExecuteStatementSafe("docParent.Timeline.References[0].Target.Groups[0].Name");
            //Note: properties are automatically surrounded by single quotes:
            Assert.AreEqual(string.Format("\'{0}\'", renamedValue), childGroupNameNew, "Verify group name was reloaded automatically");

            //Do the normal cleanup
            CloseApplication();
            VerifyApplicationClosed();
        }
    }
}
