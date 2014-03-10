//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;

namespace FunctionalTests
{
    [TestFixture]
    public class CodeEditorTests : TestBase
    {
        protected override string GetAppName()
        {
            return "CodeEditor";
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void CreateNewDocument()
        {
            ExecuteFullTest(ConstructScriptPath());
        }
    }
}