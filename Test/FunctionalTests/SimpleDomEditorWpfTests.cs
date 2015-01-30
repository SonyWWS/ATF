//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;
using NUnit.Framework;

namespace FunctionalTests
{
    [TestFixture]
    public class SimpleDomEditorWpfTests : TestBase
    {
        protected override string GetAppName()
        {
            return "SimpleDomEditorWpf";
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        [Ignore("Fails on build machine when running as service, passes when ran as local user")]
        public void CreateNewDocument()
        {
            TimeOutInSecs = 60;
            string scriptPath = Path.GetFullPath(Path.Combine(@".\CommonTestScripts", "CreateNewDocument.py"));
            ExecuteFullTest(scriptPath);
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void LaunchApplication()
        {
            TimeOutInSecs = 60;
            string scriptPath = Path.GetFullPath(Path.Combine(@".\CommonTestScripts", "LaunchApplication.py"));
            ExecuteFullTest(scriptPath);
        }
    }
}
