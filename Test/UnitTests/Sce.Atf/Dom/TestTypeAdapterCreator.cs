//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;

using Sce.Atf;
using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    public class TestTypeAdapterCreator
    {
        [Test]
        public void TestGetAdapter()
        {
            DomNodeType nodeType = new DomNodeType(
                "child",
                null,
                EmptyEnumerable<AttributeInfo>.Instance,
                EmptyEnumerable<ChildInfo>.Instance,
                EmptyEnumerable<ExtensionInfo>.Instance);

            nodeType.SetTag<TestTypeAdapterCreator>(this); // add this as metadata to the type

            TypeAdapterCreator<TestTypeAdapterCreator> test = new TypeAdapterCreator<TestTypeAdapterCreator>();
            Assert.AreEqual(test.GetAdapter(new DomNode(nodeType), typeof(TestTypeAdapterCreator)), this);
        }
    }
}
