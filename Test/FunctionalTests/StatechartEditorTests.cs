//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

using NUnit.Framework;

namespace FunctionalTests
{
    [TestFixture]
    public class StatechartEditorTests : TestBase
    {
        protected override string GetAppName()
        {
            return "StatechartEditor";
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
        public void EditSaveCloseAndReopen()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void UndoRedo()
        {
            ExecuteFullTest(ConstructScriptPath());
        }
    }
}
