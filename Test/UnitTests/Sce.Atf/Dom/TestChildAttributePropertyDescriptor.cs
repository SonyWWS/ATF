//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;

using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestChildAttributePropertyDescriptor : DomTest
    {
        [Test]
        public void TestEquality()
        {
            var attrType1 = new AttributeType("xkcd", typeof(string));
            var attrInfo1 = new AttributeInfo("xkcd", attrType1);
            var domNodeType = new DomNodeType("WebComic", DomNodeType.BaseOfAllTypes);
            var childInfo1 = new ChildInfo("xkcd", domNodeType);
            attrInfo1.DefaultValue = "Firefly";
            var desc1 = new ChildAttributePropertyDescriptor(
                "xkcd", attrInfo1, childInfo1, "Category 1", "A commonly used word or phrase in the xkcd comic", true);
            int originalHashCode = desc1.GetHashCode();

            // test if two identically created property descriptors compare as being equal
            var desc2 = new ChildAttributePropertyDescriptor(
                "xkcd", attrInfo1, childInfo1, "Category 1", "A commonly used word or phrase in the xkcd comic", true);
            Assert.AreEqual(desc1, desc2);
            Assert.AreEqual(desc1.GetHashCode(), desc2.GetHashCode());

            // test category being different; oddly, although I think they should not be considered equal,
            //  the .NET PropertyDescriptor ignores the difference in category name. For our purposes, we
            //  want to allow for two different property editors to appear with the same name but different
            //  categories. This is how our PropertyUtils.PropertyDescriptorsEqual() and GetPropertyDescriptorHash()
            //  work.
            var desc3 = new ChildAttributePropertyDescriptor(
                "xkcd", attrInfo1, childInfo1, "Category 2", "A commonly used word or phrase in the xkcd comic", true);
            Assert.AreNotEqual(desc1, desc3);
            Assert.AreNotEqual(desc1.GetHashCode(), desc3.GetHashCode());

            // test description being different; similarly here, the .Net PropertyDescriptor doesn't care.
            var desc4 = new ChildAttributePropertyDescriptor(
                "xkcd", attrInfo1, childInfo1, "Category 1", "slightly different description", true);
            Assert.AreEqual(desc1, desc4);
            Assert.AreEqual(desc1.GetHashCode(), desc4.GetHashCode());

            // test readOnly being different; ditto for read-only flag!
            var desc5 = new ChildAttributePropertyDescriptor(
                "xkcd", attrInfo1, childInfo1, "Category 1", "A commonly used word or phrase in the xkcd comic", false);
            Assert.AreEqual(desc1, desc5);
            Assert.AreEqual(desc1.GetHashCode(), desc5.GetHashCode());

            // test that the hash code hasn't changed after using the AttributeInfo
            var attrInfo2 = new AttributeInfo("xkcd", attrType1);
            domNodeType.Define(attrInfo2);
            Assert.AreEqual(desc1.GetHashCode(), originalHashCode);

            // test that the hash code hasn't changed after creating a derived DomNodeType
            var derivedDomNodeType = new DomNodeType("ScientificWebComic", domNodeType);
            var derivedAttrInfo = new AttributeInfo("xkcd", attrType1);
            var derivedChildInfo = new ChildInfo("xkcd", derivedDomNodeType);
            derivedDomNodeType.Define(derivedAttrInfo);
            Assert.AreEqual(desc1.GetHashCode(), originalHashCode);

            // test that an AttributeInfo used in a derived DomNodeType doesn't change equality or hash code
            var desc6 = new ChildAttributePropertyDescriptor(
                "xkcd", derivedAttrInfo, derivedChildInfo, "Category 1", "A commonly used word or phrase in the xkcd comic", true);
            Assert.AreEqual(desc1, desc6);
            Assert.AreEqual(desc1.GetHashCode(), desc6.GetHashCode());

            // test that a different name counts as a different property descriptor
            var desc7 = new ChildAttributePropertyDescriptor(
                "xkcd2", attrInfo1, childInfo1, "Category 1", "different name", true);
            Assert.AreNotEqual(desc1, desc7);
            Assert.AreNotEqual(desc1.GetHashCode(), desc7.GetHashCode());
        }
    }
}
