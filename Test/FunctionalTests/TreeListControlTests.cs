//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;

namespace FunctionalTests
{
    class TreeListControlTests : TestBase
    {
        protected override string GetAppName()
        {
            return "TreeListControl";
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void LaunchApplication()
        {
            // Since this sample app, unlike other ATF sample apps,  does not instantiate  
            // AutomationService or any other standard ATF components, need to use customized 
            // steps instead of the standard ExecuteFullTest() call
            int port;
            LaunchTestApplication(GetAppExePath(), out port);
            VerifyApplicationClosed();
        }
    }
}