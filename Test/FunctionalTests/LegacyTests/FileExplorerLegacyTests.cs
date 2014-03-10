//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

using NUnit.Framework;

namespace FunctionalTests.Legacy
{
    [TestFixture]
    public class FileExplorerLegacyTests : TestBaseLegacy
    {
        protected override string GetAppName()
        {
            return "FileExplorer";
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void LaunchApplication()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(@".\CommonTestScripts", "LaunchApplication.py"));
            ExecuteFullTest(scriptPath);
        }
    }
}