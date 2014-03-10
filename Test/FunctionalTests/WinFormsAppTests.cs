//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;
using NUnit.Framework;

namespace FunctionalTests
{
    [TestFixture]
    public class WinFormsAppTests : TestBase
    {
        protected override string GetAppName()
        {
            return "WinFormsApp";
        }
        
        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void CreateNewDocument()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(@".\CommonTestScripts", "CreateNewDocument.py"));
            ExecuteFullTest(scriptPath);
        }
    }
}
