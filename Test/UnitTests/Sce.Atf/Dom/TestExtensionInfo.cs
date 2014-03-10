//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

using NUnit.Framework;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestExtensionInfo : DomTest
    {
        [Test]
        public void TestConstructor()
        {
            var extension = new ExtensionInfo<TestExtensionInfo>();
            Assert.AreEqual(extension.Name, "UnitTests.Atf.Dom.TestExtensionInfo");
            Assert.AreEqual(extension.Type, typeof(TestExtensionInfo));

            var extension2 = new ExtensionInfo<TestExtensionInfo>("Foo");
            Assert.AreEqual(extension2.Name, "Foo");
        }

        [Test]
        // Tests http://tracker.ship.scea.com/jira/browse/WWSATF-1370
        // Test adding two types of extensions that have the same Name but different FullName.
        public void TestDuplicateNames()
        {
            var domType = new DomNodeType("domType");
            var extension = new ExtensionInfo<TestExtensionInfo>();
            domType.Define(extension);

            var domDerivedType = new DomNodeType("domDerivedType", domType);
            var anotherExtension = new ExtensionInfo<AnotherName.TestExtensionInfo>();
            domDerivedType.Define(anotherExtension);

            var domNode = new DomNode(domDerivedType);
            domNode.InitializeExtensions();
            Assert.IsTrue(domNode.GetExtension(extension).GetType() == typeof(TestExtensionInfo));
            Assert.IsTrue(domNode.GetExtension(anotherExtension).GetType() == typeof(AnotherName.TestExtensionInfo));

            ExtensionInfo getInfo = domType.GetExtensionInfo("UnitTests.Atf.Dom.TestExtensionInfo");
            Assert.IsNotNull(getInfo);
            Assert.AreEqual(getInfo, extension);

            getInfo = domDerivedType.GetExtensionInfo("UnitTests.Atf.Dom.TestExtensionInfo");
            Assert.IsNotNull(getInfo);
            Assert.AreEqual(getInfo, extension);
            
            ExtensionInfo anotherGetInfo = domDerivedType.GetExtensionInfo("UnitTests.Atf.Dom.AnotherName.TestExtensionInfo");
            Assert.IsNotNull(anotherGetInfo);
            Assert.AreEqual(anotherGetInfo, anotherExtension);
        }

        [Test]
        public void TestCreate()
        {
            var domType = new DomNodeType("test");
            var extension = new ExtensionInfo<TestExtensionInfo>();
            var created = extension.Create(new DomNode(domType)) as TestExtensionInfo;
            Assert.NotNull(created);
        }
    }
}

namespace UnitTests.Atf.Dom.AnotherName
{
    public class TestExtensionInfo : UnitTests.Atf.Dom.TestExtensionInfo
    {
    }
}
