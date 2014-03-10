//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;

namespace FunctionalTests
{
    [TestFixture]
    public class ModelViewerTests : TestBase
    {
        protected override string GetAppName()
        {
            return "ModelViewer";
        }
        
        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void OpenDocument()
        {
            ExecuteFullTest(ConstructScriptPath());
        }
    }
}
