//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using NUnit.Framework;

using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestChildCountRule : DomTest
    {
        [Test]
        public void TestValidate()
        {
            DomNodeType childType = new DomNodeType("child");
            DomNodeType parentType = new DomNodeType("parent");
            ChildInfo childInfo = new ChildInfo("child", childType, true);
            parentType.Define(childInfo);
            DomNode parent = new DomNode(parentType);
            IList<DomNode> childList = parent.GetChildList(childInfo);
            DomNode child1 = new DomNode(childType);
            DomNode child2 = new DomNode(childType);
            DomNode child3 = new DomNode(childType);

            ChildCountRule test = new ChildCountRule(1, 2);
            
            // 0 children. Not valid.
            Assert.False(test.Validate(parent, null, childInfo));

            // 1 child. Valid.
            childList.Add(child1);
            Assert.True(test.Validate(parent, null, childInfo));
            
            // 2 children. Valid.
            childList.Add(child2);
            Assert.True(test.Validate(parent, null, childInfo));

            // 3 children. Not valid.
            childList.Add(child3);
            Assert.False(test.Validate(parent, null, childInfo));

            // 0 children. Not valid.
            childList.Clear();
            Assert.False(test.Validate(parent, null, childInfo));
        }
    }
}
