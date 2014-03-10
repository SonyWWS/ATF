//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf;

using NUnit.Framework;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestFileStreamResolver
    {
        [Test]
        public void Test()
        {
            string testDir = "c:/test";
            FileStreamResolver test = new FileStreamResolver(testDir);
            Uri uri = test.ResolveUri(null, "test");
            Assert.AreEqual(uri.AbsolutePath, "c:/test/test");
        }
    }
}
