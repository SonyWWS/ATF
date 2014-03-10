//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using NUnit.Framework;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestDataValidator : DomTest
    {
        public TestDataValidator()
        {
            m_childType = new DomNodeType("child");
            m_parentType = new DomNodeType("parent");
            m_parentType.Define(new ExtensionInfo<ValidationContext>());
            m_parentType.Define(new ExtensionInfo<DataValidator>());

            m_childCountRule = new ChildCountRule(2, 3);
            m_childInfo = new ChildInfo("child", m_childType, true);
            m_parentType.Define(m_childInfo);
            m_childInfo.AddRule(m_childCountRule);

            m_parent = new DomNode(m_parentType);
            m_parent.InitializeExtensions();

            m_validationContext = m_parent.As<ValidationContext>();

            m_child1 = new DomNode(m_childType);
            m_child2 = new DomNode(m_childType);
            m_child3 = new DomNode(m_childType);
            m_child4 = new DomNode(m_childType);
        }

        public void Clean()
        {
            IList<DomNode> childList = m_parent.GetChildList(m_childInfo);
            childList.Clear();
        }

        [Test]
        public void TestChildCountOutsideValidationContext()
        {
            Clean();
            IList<DomNode> childList = m_parent.GetChildList(m_childInfo);

            childList.Add(m_child1);
            childList.Add(m_child2);
            childList.Add(m_child3);

            childList.Remove(m_child3);
            childList.Remove(m_child2);
            childList.Remove(m_child1);
        }

        [Test]
        public void TestChildCountTemporarilyInvalid()
        {
            Clean();
            IList<DomNode> childList = m_parent.GetChildList(m_childInfo);

            // start in a valid state, then make changes that are temporarily invalid, but finish valid
            childList.Add(m_child1);
            childList.Add(m_child2);
            m_validationContext.RaiseBeginning(); //valid: 2 children
            childList.Remove(m_child1); //temporarily invalid, but should not cause an exception
            childList.Add(m_child3); //valid again (2 children)
            childList.Add(m_child4); //still valid (3 children)
            m_validationContext.RaiseEnding(); //valid: 3 children
            m_validationContext.RaiseEnded(); //valid: 3 children
        }

        [Test]
        public void TestChildCountInvalid()
        {
            Clean();
            IList<DomNode> childList = m_parent.GetChildList(m_childInfo);

            // start in a valid state, then make changes that result in invalid child count
            childList.Add(m_child1);
            childList.Add(m_child2);
            m_validationContext.RaiseBeginning();
            childList.Remove(m_child1); //temporarily invalid, but should not cause an exception
            childList.Add(m_child1); //valid again (2 children)
            childList.Add(m_child3); //valid (3 children)
            childList.Add(m_child4); //invalid (4 children)
            m_validationContext.RaiseEnding();
            Assert.Throws<InvalidTransactionException>(
                () => m_validationContext.RaiseEnded());

            // do it again, to help make sure no state is being saved
            m_validationContext.RaiseBeginning();
            childList.Clear();
            childList.Add(m_child1); //invalid
            childList.Add(m_child2); //valid (2 children)
            m_validationContext.RaiseCancelled();

            // one more time
            m_validationContext.RaiseBeginning();
            childList.Clear();
            childList.Add(m_child1); //invalid
            childList.Add(m_child2); //valid (2 children)
            childList.Add(m_child3); //valid (3 children)
            childList.Remove(m_child3); //valid (2 children)
            childList.Add(m_child3); //valid (3 children)
            childList.Add(m_child4); //invalid (4 children)
            m_validationContext.RaiseEnding();
            Assert.Throws<InvalidTransactionException>(
                () => m_validationContext.RaiseEnded());
        }

        DomNodeType m_parentType, m_childType;
        ChildInfo m_childInfo;
        ChildCountRule m_childCountRule;
        DomNode m_parent, m_child1, m_child2, m_child3, m_child4;
        ValidationContext m_validationContext;
    }
}
