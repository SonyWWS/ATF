//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using NUnit.Framework;

using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace UnitTests.Atf
{
    /// <summary>
    /// Tests PropertyUtils static class</summary>
    [TestFixture]
    public class TestPropertyUtils
    {
        [Test]
        public void TestGetPropertyDescriptorHash()
        {
            // First use AttributePropertyDescriptor and MultiPropertyDescriptor
            {
                // Make sure that different property descriptor objects with the same Name, Category and Type
                //  have the same hash code, using PropertyUtils.GetPropertyDescriptorHash().
                var attrPropertyDescriptor = new AttributePropertyDescriptor(
                    "TestName", new AttributeInfo("TestAttrName", new AttributeType("TestAttrName", typeof(string))),
                    "TestCategory", "Test description", false);
                var multiPropertyDescriptor = new MultiPropertyDescriptor(attrPropertyDescriptor);
                Assert.IsTrue(IsSameHashCode(attrPropertyDescriptor, multiPropertyDescriptor));

                // Make sure that null Category names work, too.
                attrPropertyDescriptor = new AttributePropertyDescriptor(
                    "TestName", new AttributeInfo("TestAttrName", new AttributeType("TestAttrName", typeof(string))),
                    null, "Test description", false);
                multiPropertyDescriptor = new MultiPropertyDescriptor(attrPropertyDescriptor);
                Assert.IsTrue(IsSameHashCode(attrPropertyDescriptor, multiPropertyDescriptor));

                // Make sure that if the Name and Category and identical, that it works with other property
                //  descriptors who also have identical Name and Category strings. This makes sure that we
                //  are not using 'xor' between the Name and Category.
                attrPropertyDescriptor = new AttributePropertyDescriptor(
                    "SameName", new AttributeInfo("TestAttrName", new AttributeType("TestAttrName", typeof(string))),
                    "SameName", "Name and Category are the same!", false);
                multiPropertyDescriptor = new MultiPropertyDescriptor(attrPropertyDescriptor);
                Assert.IsTrue(IsSameHashCode(attrPropertyDescriptor, multiPropertyDescriptor));

                var d2 = new AttributePropertyDescriptor(
                    "AnotherSameName", new AttributeInfo("TestAttrName", new AttributeType("TestAttrName", typeof(string))),
                    "AnotherSameName", "Name and Category are the same!", false);
                var m2 = new MultiPropertyDescriptor(d2);
                Assert.IsTrue(IsSameHashCode(d2, m2));

                Assert.IsTrue(!IsSameHashCode(attrPropertyDescriptor, d2));

                // Make sure that if the Name and Category are swapped, that they yield different codes.
                attrPropertyDescriptor = new AttributePropertyDescriptor(
                    "Name1", new AttributeInfo("TestAttrName", new AttributeType("TestAttrName", typeof(string))),
                    "Name2", "Test description", false);
                d2 = new AttributePropertyDescriptor(
                    "Name2", new AttributeInfo("TestAttrName", new AttributeType("TestAttrName", typeof(string))),
                    "Name1", "Test description", false);
                Assert.IsTrue(!IsSameHashCode(attrPropertyDescriptor, d2));
            }

            // Now use a stub property descriptor and MultiPropertyDescriptor
            {
                // Make sure that different property descriptor objects with the same Name, Category and Type
                //  have the same hash code, using PropertyUtils.GetPropertyDescriptorHash().
                var stubPropertyDescriptor = new MyPropertyDescriptor("TestName", "TestCategory", typeof(string));
                var multiPropertyDescriptor = new MultiPropertyDescriptor(stubPropertyDescriptor);
                Assert.IsTrue(IsSameHashCode(stubPropertyDescriptor, multiPropertyDescriptor));

                // Make sure that null Category names work, too.
                stubPropertyDescriptor = new MyPropertyDescriptor("TestName", null, typeof(string));
                multiPropertyDescriptor = new MultiPropertyDescriptor(stubPropertyDescriptor);
                Assert.IsTrue(IsSameHashCode(stubPropertyDescriptor, multiPropertyDescriptor));

                // Make sure that if the Name and Category and identical, that it works with other property
                //  descriptors who also have identical Name and Category strings. This makes sure that we
                //  are not using 'xor' between the Name and Category.
                stubPropertyDescriptor = new MyPropertyDescriptor("SameName", "SameName", typeof(string));
                multiPropertyDescriptor = new MultiPropertyDescriptor(stubPropertyDescriptor);
                Assert.IsTrue(IsSameHashCode(stubPropertyDescriptor, multiPropertyDescriptor));

                var d2 = new MyPropertyDescriptor("AnotherSameName", "AnotherSameName", typeof(string));
                var m2 = new MultiPropertyDescriptor(d2);
                Assert.IsTrue(IsSameHashCode(d2, m2));

                Assert.IsTrue(!IsSameHashCode(stubPropertyDescriptor, d2));

                // Make sure that if the Name and Category are swapped, that they yield different codes.
                stubPropertyDescriptor = new MyPropertyDescriptor("Name1", "Name2", typeof(string));
                d2 = new MyPropertyDescriptor("Name2", "Name1", typeof(string));
                Assert.IsTrue(!IsSameHashCode(stubPropertyDescriptor, d2));
            }
        }

        [Test]
        public void TestGetPropertyDescriptorKey()
        {
            // First use AttributePropertyDescriptor and MultiPropertyDescriptor
            {
                // Make sure that different property descriptor objects with the same Name, Category and Type
                //  have the same hash code, using PropertyUtils.GetPropertyDescriptorHash().
                var attrPropertyDescriptor = new AttributePropertyDescriptor(
                    "TestName", new AttributeInfo("TestAttrName", new AttributeType("TestAttrName", typeof(string))),
                    "TestCategory", "Test description", false);
                var multiPropertyDescriptor = new MultiPropertyDescriptor(attrPropertyDescriptor);
                Assert.IsTrue(IsSameHashKey(attrPropertyDescriptor, multiPropertyDescriptor));

                // Make sure that null Category names work, too.
                attrPropertyDescriptor = new AttributePropertyDescriptor(
                    "TestName", new AttributeInfo("TestAttrName", new AttributeType("TestAttrName", typeof(string))),
                    null, "Test description", false);
                multiPropertyDescriptor = new MultiPropertyDescriptor(attrPropertyDescriptor);
                Assert.IsTrue(IsSameHashKey(attrPropertyDescriptor, multiPropertyDescriptor));

                // Make sure that if the Name and Category and identical, that it works with other property
                //  descriptors who also have identical Name and Category strings. This makes sure that we
                //  are not using 'xor' between the Name and Category.
                attrPropertyDescriptor = new AttributePropertyDescriptor(
                    "SameName", new AttributeInfo("TestAttrName", new AttributeType("TestAttrName", typeof(string))),
                    "SameName", "Name and Category are the same!", false);
                multiPropertyDescriptor = new MultiPropertyDescriptor(attrPropertyDescriptor);
                Assert.IsTrue(IsSameHashKey(attrPropertyDescriptor, multiPropertyDescriptor));

                var d2 = new AttributePropertyDescriptor(
                    "AnotherSameName", new AttributeInfo("TestAttrName", new AttributeType("TestAttrName", typeof(string))),
                    "AnotherSameName", "Name and Category are the same!", false);
                var m2 = new MultiPropertyDescriptor(d2);
                Assert.IsTrue(IsSameHashKey(d2, m2));

                Assert.IsTrue(!IsSameHashKey(attrPropertyDescriptor, d2));

                // Make sure that if the Name and Category are swapped, that they yield different codes.
                attrPropertyDescriptor = new AttributePropertyDescriptor(
                    "Name1", new AttributeInfo("TestAttrName", new AttributeType("TestAttrName", typeof(string))),
                    "Name2", "Test description", false);
                d2 = new AttributePropertyDescriptor(
                    "Name2", new AttributeInfo("TestAttrName", new AttributeType("TestAttrName", typeof(string))),
                    "Name1", "Test description", false);
                Assert.IsTrue(!IsSameHashKey(attrPropertyDescriptor, d2));
            }

            // Now use a stub property descriptor and MultiPropertyDescriptor
            {
                // Make sure that different property descriptor objects with the same Name, Category and Type
                //  have the same hash code, using PropertyUtils.GetPropertyDescriptorHash().
                var stubPropertyDescriptor = new MyPropertyDescriptor("TestName", "TestCategory", typeof(string));
                var multiPropertyDescriptor = new MultiPropertyDescriptor(stubPropertyDescriptor);
                Assert.IsTrue(IsSameHashKey(stubPropertyDescriptor, multiPropertyDescriptor));

                // Make sure that null Category names work, too.
                stubPropertyDescriptor = new MyPropertyDescriptor("TestName", null, typeof(string));
                multiPropertyDescriptor = new MultiPropertyDescriptor(stubPropertyDescriptor);
                Assert.IsTrue(IsSameHashKey(stubPropertyDescriptor, multiPropertyDescriptor));

                // Make sure that if the Name and Category and identical, that it works with other property
                //  descriptors who also have identical Name and Category strings. This makes sure that we
                //  are not using 'xor' between the Name and Category.
                stubPropertyDescriptor = new MyPropertyDescriptor("SameName", "SameName", typeof(string));
                multiPropertyDescriptor = new MultiPropertyDescriptor(stubPropertyDescriptor);
                Assert.IsTrue(IsSameHashKey(stubPropertyDescriptor, multiPropertyDescriptor));

                var d2 = new MyPropertyDescriptor("AnotherSameName", "AnotherSameName", typeof(string));
                var m2 = new MultiPropertyDescriptor(d2);
                Assert.IsTrue(IsSameHashKey(d2, m2));

                Assert.IsTrue(!IsSameHashKey(stubPropertyDescriptor, d2));

                // Make sure that if the Name and Category are swapped, that they yield different codes.
                stubPropertyDescriptor = new MyPropertyDescriptor("Name1", "Name2", typeof(string));
                d2 = new MyPropertyDescriptor("Name2", "Name1", typeof(string));
                Assert.IsTrue(!IsSameHashKey(stubPropertyDescriptor, d2));
            }
        }

        private bool IsSameHashCode(System.ComponentModel.PropertyDescriptor p1, System.ComponentModel.PropertyDescriptor p2)
        {
            int hash1 = p1.GetPropertyDescriptorHash();
            int hash2 = p2.GetPropertyDescriptorHash();
            return hash1 == hash2;
        }

        private bool IsSameHashKey(System.ComponentModel.PropertyDescriptor p1, System.ComponentModel.PropertyDescriptor p2)
        {
            string id1 = p1.GetPropertyDescriptorKey();
            string id2 = p2.GetPropertyDescriptorKey();
            return id1 == id2;
        }

        private class MyPropertyDescriptor : System.ComponentModel.PropertyDescriptor
        {
            public MyPropertyDescriptor(string name, string category, Type type)
                : base(name, null)
            {
                m_category = category;
                m_type = type;
            }

            /// <summary>
            /// Gets the name of the category to which the member belongs, as specified in the <see cref="T:System.ComponentModel.CategoryAttribute"></see></summary>
            public override string Category
            {
                get { return m_category; }
            }

            /// <summary>
            /// Gets the description of the member, as specified in the <see cref="T:System.ComponentModel.DescriptionAttribute"></see></summary>
            public override string Description
            {
                get { return ""; }
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type PropertyType
            {
                get { return m_type; }
            }

            public override Type ComponentType
            {
                get { return typeof(DomNode); }
            }

            public override bool ShouldSerializeValue(object component)
            {
                return
                    CanResetValue(component);
            }

            public override bool CanResetValue(object component)
            {
                return true;
            }

            public override void ResetValue(object component)
            {
            }

            public override void SetValue(object component, object value)
            {
            }
            
            public override object GetValue(object component)
            {
                return null;
            }

            private string m_category;
            private Type m_type;
        }
    }
}
