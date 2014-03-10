//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;
using NUnit.Framework;

namespace FunctionalTests
{
    [TestFixture]
    public class WpfAppTests : TestBase
    {
        protected override string GetAppName()
        {
            return "WpfApp";
        }
        
        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void CreateNewDocument()
        {
            TimeOutInSecs = 60;
            string scriptPath = Path.GetFullPath(Path.Combine(@".\CommonTestScripts", "CreateNewDocument.py"));
            ExecuteFullTest(scriptPath);
        }
    }
}
