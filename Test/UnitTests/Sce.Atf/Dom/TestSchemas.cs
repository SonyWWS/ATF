//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Reflection;
using System.Linq;

using NUnit.Framework;

using Sce.Atf;
using Sce.Atf.Dom;
using System.Collections.Generic;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestSchemas
    {
        [Test]
        public void Test()
        {
            XmlSchemaTypeLoader loader = new XmlSchemaTypeLoader();
            loader.SchemaResolver = new ResourceStreamResolver(
                Assembly.GetExecutingAssembly(),
                "UnitTests.Atf/Resources");
            loader.Load("testComplexTypes.xsd");

            DomNodeType abstractType = loader.GetNodeType("test:abstractType");
            Assert.IsTrue(abstractType != null);
            Assert.IsTrue(abstractType.IsAbstract);

            DomNodeType complexType1 = loader.GetNodeType("test:complexType1");
            Assert.IsTrue(complexType1 != null);
            Assert.IsTrue(!complexType1.IsAbstract);
            Assert.IsTrue(complexType1.BaseType == abstractType);
            //Assert.IsTrue(complexType1.FindAnnotation("annotation") != null);
            //Assert.IsTrue(complexType1.FindAnnotation("annotation", "attr1") != null);

            DomNodeType complexType2 = loader.GetNodeType("test:complexType2");
            Assert.IsTrue(complexType2 != null);
            Assert.IsTrue(!complexType2.IsAbstract);
            AttributeInfo attr1 = complexType2.GetAttributeInfo("attr1");
            Assert.IsTrue(attr1 != null);
            Assert.IsTrue(attr1.DefaultValue.Equals(1));
            //Assert.IsTrue(attr1.FindAnnotation("annotation") != null);
            AttributeInfo attr2 = complexType2.GetAttributeInfo("attr2");
            Assert.IsTrue(attr2 != null);
            Assert.IsTrue(attr2.DefaultValue.Equals(2));

            DomNodeType complexType3 = loader.GetNodeType("test:complexType3");
            Assert.IsTrue(complexType3 != null);
            Assert.IsTrue(!complexType3.IsAbstract);
            Assert.IsTrue(complexType3.BaseType == complexType2);
            AttributeInfo attr3 = complexType3.GetAttributeInfo("attr3");
            Assert.IsTrue(attr3 != null);
            ChildInfo elem1 = complexType3.GetChildInfo("elem1");
            Assert.IsTrue(elem1 != null);
            Assert.IsTrue(elem1.Type == complexType1);
            //Assert.IsTrue(elem1.FindAnnotation("annotation") != null);
            ChildInfo elem2 = complexType3.GetChildInfo("elem2");
            Assert.IsTrue(elem2 != null);
            Assert.IsTrue(elem2.Type == complexType1);
            Assert.IsTrue(MinMaxCheck(elem2, 1, 3));
            ChildInfo elem3 = complexType3.GetChildInfo("elem3");
            Assert.IsTrue(elem3 != null); //because a sequence of simple types becomes a sequence of child DomNodes
            attr3 = complexType3.GetAttributeInfo("elem3");
            Assert.IsTrue(attr3 == null); //because a sequence of simple types becomes a sequence of child DomNodes
            DomNode node3 = new DomNode(complexType3);
            DomNode elem3Child1 = new DomNode(elem3.Type);
            AttributeInfo elem3ValueAttributeInfo = elem3.Type.GetAttributeInfo(string.Empty);
            elem3Child1.SetAttribute(elem3ValueAttributeInfo, 1);
            DomNode elem3Child2 = new DomNode(elem3.Type);
            elem3Child2.SetAttribute(elem3ValueAttributeInfo, 1);
            DomNode elem3Child3 = new DomNode(elem3.Type);
            elem3Child3.SetAttribute(elem3ValueAttributeInfo, 1);
            node3.GetChildList(elem3).Add(elem3Child1);
            node3.GetChildList(elem3).Add(elem3Child2);
            node3.GetChildList(elem3).Add(elem3Child3);

            IList<DomNode> node3Children = node3.GetChildList(elem3);
            Assert.IsTrue((int)node3Children[0].GetAttribute(elem3ValueAttributeInfo) == 1);
            Assert.IsTrue((int)node3Children[1].GetAttribute(elem3ValueAttributeInfo) == 1);
            Assert.IsTrue((int)node3Children[2].GetAttribute(elem3ValueAttributeInfo) == 1);

            // Update on 8/16/2011. DomXmlReader would not be able to handle a sequence of elements
            //  of a simple type like this. When reading, each subsequent element's value would be
            //  used to set the attribute on the DomNode, overwriting the previous one. So, since
            //  this behavior of converting more than one element of a simple type into an attribute
            //  array was broken, I want to change this unit test that I wrote and make sequences of
            //  elements of simple types into a sequence of DomNode children with a value attribute.
            //  (A value attribute means an attribute whose name is "".) --Ron
            //ChildInfo elem3 = complexType3.GetChildInfo("elem3");
            //Assert.IsTrue(elem3 == null); //because a sequence of simple types becomes an attribute
            //attr3 = complexType3.GetAttributeInfo("elem3");
            //Assert.IsTrue(attr3 != null); //because a sequence of simple types becomes an attribute
            //DomNode node3 = new DomNode(complexType3);
            //object attr3Obj = node3.GetAttribute(attr3);
            //Assert.IsTrue(
            //    attr3Obj is int &&
            //    (int)attr3Obj == 0); //the default integer
            //node3.SetAttribute(attr3, 1);
            //attr3Obj = node3.GetAttribute(attr3);
            //Assert.IsTrue(
            //    attr3Obj is int &&
            //    (int)attr3Obj == 1);
            //node3.SetAttribute(attr3, new int[] { 1, 2, 3 });
            //attr3Obj = node3.GetAttribute(attr3);
            //Assert.IsTrue(
            //    attr3Obj is int[] &&
            //    ((int[])attr3Obj)[2]==3);
            
            DomNodeType complexType4 = loader.GetNodeType("test:complexType4");
            Assert.IsTrue(complexType4 != null);
            Assert.IsTrue(!complexType4.IsAbstract);
            attr1 = complexType4.GetAttributeInfo("attr1");
            Assert.IsTrue(attr1 != null);
            elem1 = complexType4.GetChildInfo("elem1");
            Assert.IsTrue(elem1 != null);
            Assert.IsTrue(elem1.Type == complexType3);
            Assert.IsTrue(MinMaxCheck(elem1, 1, 1));

            DomNodeType complexType5 = loader.GetNodeType("test:complexType5");
            Assert.IsTrue(complexType5 != null);
            Assert.IsTrue(!complexType5.IsAbstract);
            elem1 = complexType5.GetChildInfo("elem1");
            Assert.IsTrue(elem1 != null);
            Assert.IsTrue(elem1.Type == abstractType);
            Assert.IsTrue(MinMaxCheck(elem1, 1, Int32.MaxValue));

            DomNode node5 = new DomNode(complexType5);
            elem2 = complexType5.GetChildInfo("elem2");
            DomNode node2 = new DomNode(complexType2);
            node5.SetChild(elem2, node2);
            node5.SetChild(elem2, null);
            node3 = new DomNode(complexType3);
            elem3 = complexType5.GetChildInfo("elem3");
            node5.SetChild(elem3, node3);
            //The following should violate xs:choice, but we don't fully support this checking yet.
            //ExceptionTester.CheckThrow<InvalidOperationException>(delegate { node5.AddChild(elem2, node2); });

            DomNodeType complexType6 = loader.GetNodeType("test:complexType6");
            Assert.IsTrue(complexType6 != null);
            Assert.IsTrue(!complexType6.IsAbstract);
            elem1 = complexType6.GetChildInfo("elem1");
            Assert.IsTrue(elem1 != null);
            Assert.IsTrue(elem1.Type == abstractType);
            Assert.IsTrue(MinMaxCheck(elem1, 1, Int32.MaxValue));
            elem2 = complexType6.GetChildInfo("elem2");
            Assert.IsTrue(elem2 != null);
            Assert.IsTrue(elem2.Type == complexType2);
            Assert.IsTrue(MinMaxCheck(elem2, 1, Int32.MaxValue));

            DomNodeType complexType7 = loader.GetNodeType("test:complexType7");
            Assert.IsTrue(complexType7 != null);
            ChildInfo elemSimpleSequence = complexType7.GetChildInfo("elemSimpleSequence");
            var minMaxOccurs = elemSimpleSequence.Rules.OfType<ChildCountRule>().FirstOrDefault();
            Assert.IsTrue(
                minMaxOccurs != null &&
                minMaxOccurs.Min == 3 &&
                minMaxOccurs.Max == 3);
        }

        //private static bool ArraysEqual(object testObject, float[][] correctSequence)
        //{
        //    var testSequence = testObject as float[][];
        //    if (testSequence == null)
        //        return false;

        //    if (testSequence.Length != correctSequence.Length)
        //        return false;

        //    for (int i = 0; i < testSequence.Length; i++)
        //    {
        //        float[] testArray = testSequence[i];
        //        float[] correctArray = correctSequence[i];
        //        if (testArray.Length != correctArray.Length)
        //            return false;
        //        for (int j = 0; j < testArray.Length; j++)
        //            if (testArray[j] != correctArray[j])
        //                return false;
        //    }
            
        //    return true;
        //}
        
        // Returns <c>True</c> if every ChildCountRule on this ChildInfo matches the given min and max.
        private bool MinMaxCheck(ChildInfo info, int min, int max)
        {
            bool foundOne = false;
            foreach (ChildCountRule countRule in info.Rules.OfType<ChildCountRule>())
            {
                foundOne = true;
                if (countRule.Min != min || countRule.Max != max)
                    return false;
            }
            return foundOne;
        }
    }
}
