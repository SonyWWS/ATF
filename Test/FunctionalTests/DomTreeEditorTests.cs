//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;

namespace FunctionalTests
{
    [TestFixture]
    public class DomTreeEditorTests : TestBase
    {
        protected override string GetAppName()
        {
            return "DomTreeEditor";
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void CreateNewDocument()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void AddAllItems()
        {
            ExecuteFullTest(ConstructScriptPath());
        }
    }
}