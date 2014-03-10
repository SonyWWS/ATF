//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

using NUnit.Framework;

namespace FunctionalTests.Legacy
{
    [TestFixture]
    public class CircuitEditorLegacyTests : TestBaseLegacy
    {
        protected override string GetAppName()
        {
            return "CircuitEditor";
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
            string scriptPath = Path.GetFullPath(Path.Combine(@".\LegacyTests", @"CircuitEditorLegacyTestScripts\TestGetBounds.py"));
            ExecuteFullTest(scriptPath);
        }
    }
}