//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

using NUnit.Framework;

namespace FunctionalTests.Legacy
{
    [TestFixture]
    public class DomTreeEditorLegacyTests : TestBaseLegacy
    {
        protected override string GetAppName()
        {
            return "DomTreeEditor";
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void CreateNewDocument()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(@".\CommonTestScripts", "CreateNewDocumentSingular.py"));
            ExecuteFullTest(scriptPath);
        }
    }
}