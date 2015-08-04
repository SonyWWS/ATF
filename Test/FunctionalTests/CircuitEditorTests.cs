//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

using NUnit.Framework;

namespace FunctionalTests
{
    [TestFixture]
    public class CircuitEditorTests : TestBase
    {
        protected override string GetAppName()
        {
            return "CircuitEditor";
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
        [Category(Consts.SmokeTestCategory)]
        public void EditSaveCloseAndReopen()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void TestLayers()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void DeleteLayers()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void CutPaste()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void CopyPaste()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void TestGroups()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void TestTemplates()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void TestVersionMigrator()
        {
            ExecuteFullTest(ConstructScriptPath());
        }
    }
}
