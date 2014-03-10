//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Reflection;

using Sce.Atf;

using NUnit.Framework;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestResourceStreamResolver
    {
        [Test]
        public void Test()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            ResourceStreamResolver test = new ResourceStreamResolver(
                assembly,
                "UnitTests.Atf/Resources");
            Uri uri = test.ResolveUri(null, "test.xsd");
            Assert.AreEqual(uri.ToString(), "file:///UnitTests.Atf/Resources/test.xsd");

            Stream strm = (Stream)test.GetEntity(uri, null, null);
            Assert.NotNull(strm);
        }
    }
}
