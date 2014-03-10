//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;

using Sce.Atf.Adaptation;

namespace UnitTests.Atf.Adaptation
{
    [TestFixture]
    public class TestAdapterCreator
    {
        [Test]
        public void TestCanAdapt()
        {
            AdapterCreator<SimpleAdapter> creator = new AdapterCreator<SimpleAdapter>();

            // test CanAdapt
            Assert.True(creator.CanAdapt(this, typeof(ISimpleInterface)));
            Assert.False(creator.CanAdapt(this, typeof(string)));
        }

        [Test]
        public void TestGetAdapter()
        {
            AdapterCreator<SimpleAdapter> creator = new AdapterCreator<SimpleAdapter>();
            object obj;

            // test successful request (SimpleAdapter implements ISimpleInterface)
            obj = creator.GetAdapter(this, typeof(ISimpleInterface));
            SimpleAdapter simpleAdapter = obj as SimpleAdapter;
            Assert.NotNull(simpleAdapter);
            Assert.AreSame(simpleAdapter.Adaptee, this);

            // test failed request (SimpleAdapter is not a string)
            obj = creator.GetAdapter(this, typeof(string));
            Assert.Null(obj);
        }
    }
}
