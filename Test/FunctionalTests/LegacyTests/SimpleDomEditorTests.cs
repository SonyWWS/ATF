//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

using NUnit.Framework;

namespace FunctionalTests.Legacy
{
    [TestFixture]
    public class SimpleDomEditorLegacyTests : TestBaseLegacy
    {
        protected override string GetAppName()
        {
            return "SimpleDomEditor";
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void CreateNewDocument()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(@".\CommonTestScripts", "CreateNewDocument.Legacy.py"));
            ExecuteFullTest(scriptPath);
        }
    }
}