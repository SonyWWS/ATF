//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

using NUnit.Framework;

namespace FunctionalTests.Legacy
{
    [TestFixture]
    public class CodeEditorLegacyTests : TestBaseLegacy
    {
        protected override string GetAppName()
        {
            return "CodeEditor";
        }

        protected override string GetScriptsDirectoryPath()
        {
            return Path.GetFullPath(string.Format(@".\LegacyTests\{0}LegacyTestScripts", GetAppName()));
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void CreateNewDocument()
        {
            ExecuteFullTest(ConstructScriptPath());
        }
    }
}