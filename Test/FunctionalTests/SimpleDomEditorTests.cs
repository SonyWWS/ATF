//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;

namespace FunctionalTests
{
    [TestFixture]
    public class SimpleDomEditorTests : TestBase
    {
        protected override string GetAppName()
        {
            return "SimpleDomEditor";
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void CreateNewDocument()
        {
            ExecuteFullTest(ConstructScriptPath());
        }
    }
}
