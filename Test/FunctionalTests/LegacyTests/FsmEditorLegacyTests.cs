//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

using NUnit.Framework;

namespace FunctionalTests.Legacy
{
    [TestFixture]
    public class FsmEditorLegacyTests : TestBaseLegacy
    {
        protected override string GetAppName()
        {
            return "FsmEditor";
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void CreateNewDocument()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(@".\CommonTestScripts", "CreateNewDocument.Legacy.py"));
            ExecuteFullTest(scriptPath);
        }

        [Test]
        public void TestGetBounds()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(@".\LegacyTests", @"FsmEditorLegacyTestScripts\TestGetBounds.py"));
            ExecuteFullTest(scriptPath);
        }
    }
}