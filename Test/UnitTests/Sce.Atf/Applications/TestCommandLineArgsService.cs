//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;
using Sce.Atf.Applications;

namespace UnitTests.Atf
{
    [TestFixture]
    class TestCommandLineArgsService
    {
        [Test]
        public void TestParse()
        {
            var commandLineArgsService = new CommandLineArgsService(null, null);
            commandLineArgsService.Parse(new[] {"-automation", "-name", "Jack"});
            var automationOption = commandLineArgsService["automation"];
            Assert.AreEqual(automationOption, true);
            var name = commandLineArgsService["name"];
            Assert.AreEqual(name, "Jack");
        }
    }
}
