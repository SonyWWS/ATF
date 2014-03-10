//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

using NUnit.Framework;

namespace FunctionalTests
{
    [TestFixture]
    public class TreeListEditorTests : TestBase
    {
        protected override string GetAppName()
        {
            return "TreeListEditor";
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