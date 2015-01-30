//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;
using Sce.Atf.Controls.PropertyEditing;

namespace UnitTests.Atf
{
    /// <summary>
    /// Tests Editors PropertyEditing</summary>
    [TestFixture]
    public class TestBoundPropertyDescriptor
    {
        /// <summary>
        /// Simple class to test model/propertyowner interactions</summary>
        private class MyClass
        {
            public MyClass(string property, int readOnlyProperty)
            {
                Property = property;
                ReadOnlyProperty = readOnlyProperty;
            }

            public string Property { get; set; }

            public int ReadOnlyProperty { get; private set; }

            //public string Field = "unset";
        }

        [Test]
        public void TestInstanceWithStringLiterals()
        {
            MyClass myClass = new MyClass("myClass", 5);
            BoundPropertyDescriptor test;

            test = new BoundPropertyDescriptor(myClass, "Property", "Property", "", "");
            Assert.IsFalse(test.IsReadOnly);
            Assert.IsTrue(test.GetValue(null).Equals("myClass"));
            test.SetValue(null, "xxx");
            Assert.IsTrue(test.GetValue(null).Equals("xxx"));
            Assert.IsTrue(myClass.Property == "xxx");

            test = new BoundPropertyDescriptor(myClass, "ReadOnlyProperty", "ReadOnlyProperty", "", "");
            Assert.IsTrue(test.IsReadOnly);
            Assert.IsTrue(test.GetValue(null).Equals(5));
            test.SetValue(null, 10);
            Assert.IsTrue(test.GetValue(null).Equals(10));
            Assert.IsTrue(myClass.ReadOnlyProperty == 10);
        }

        [Test]
        public void TestInstanceWithLambdas()
        {
            MyClass myClass = new MyClass("myClass", 5);
            BoundPropertyDescriptor test;
            
            test = new BoundPropertyDescriptor(myClass, () => myClass.Property, "Property", "", "");
            Assert.IsFalse(test.IsReadOnly);
            Assert.IsTrue(test.GetValue(null).Equals("myClass"));
            test.SetValue(null, "xxx");
            Assert.IsTrue(test.GetValue(null).Equals("xxx"));
            Assert.IsTrue(myClass.Property == "xxx");

            test = new BoundPropertyDescriptor(myClass, () => myClass.ReadOnlyProperty, "ReadOnlyProperty", "", "");
            Assert.IsTrue(test.IsReadOnly);
            Assert.IsTrue(test.GetValue(null).Equals(5));
            test.SetValue(null, 10);
            Assert.IsTrue(test.GetValue(null).Equals(10));
            Assert.IsTrue(myClass.ReadOnlyProperty == 10);
        }

        private static class MyStaticClass
        {
            static MyStaticClass()
            {
                Reset();
            }

            public static void Reset()
            {
                StaticStringProperty = "uninitialized";
                StaticIntProperty = 5;
                StaticStringAsymmetricalAccessProperty = "uninitialized";
                StaticIntAsymmetricalProperty = 5;
            }

            public static string StaticStringProperty
            {
                get;
                set;
            }

            public static int StaticIntProperty
            {
                get;
                set;
            }

            public static string StaticStringAsymmetricalAccessProperty
            {
                get;
                private set;
            }

            public static int StaticIntAsymmetricalProperty
            {
                get;
                private set;
            }
        }

        [Test]
        public void TestStaticTypeWithStringLiterals()
        {
            MyStaticClass.Reset();
            BoundPropertyDescriptor test;

            test = new BoundPropertyDescriptor(typeof(MyStaticClass), "StaticStringProperty", "StaticStringProperty", "", "");
            Assert.IsFalse(test.IsReadOnly);
            Assert.IsTrue(test.GetValue(null).Equals("uninitialized"));
            test.SetValue(null, "xxx");
            Assert.IsTrue(test.GetValue(null).Equals("xxx"));
            Assert.IsTrue(MyStaticClass.StaticStringProperty == "xxx");

            test = new BoundPropertyDescriptor(typeof(MyStaticClass), "StaticIntProperty", "StaticIntProperty", "", "");
            Assert.IsFalse(test.IsReadOnly);
            Assert.IsTrue(test.GetValue(null).Equals(5));
            test.SetValue(null, 10);
            Assert.IsTrue(test.GetValue(null).Equals(10));
            Assert.IsTrue(MyStaticClass.StaticIntProperty == 10);

            test = new BoundPropertyDescriptor(typeof(MyStaticClass), "StaticStringAsymmetricalAccessProperty", "StaticStringAsymmetricalAccessProperty", "", "");
            Assert.IsTrue(test.IsReadOnly);
            Assert.IsTrue(test.GetValue(null).Equals("uninitialized"));
            test.SetValue(null, "xxx");
            Assert.IsTrue(test.GetValue(null).Equals("xxx"));
            Assert.IsTrue(MyStaticClass.StaticStringAsymmetricalAccessProperty == "xxx");

            test = new BoundPropertyDescriptor(typeof(MyStaticClass), "StaticIntAsymmetricalProperty", "StaticIntAsymmetricalProperty", "", "");
            Assert.IsTrue(test.IsReadOnly);
            Assert.IsTrue(test.GetValue(null).Equals(5));
            test.SetValue(null, 10);
            Assert.IsTrue(test.GetValue(null).Equals(10));
            Assert.IsTrue(MyStaticClass.StaticIntAsymmetricalProperty == 10);
        }

        [Test]
        public void TestStaticTypeWithLambdas()
        {
            MyStaticClass.Reset();
            BoundPropertyDescriptor test;

            test = new BoundPropertyDescriptor(typeof(MyStaticClass), () => MyStaticClass.StaticStringProperty, "StaticStringProperty", "", "");
            Assert.IsFalse(test.IsReadOnly);
            Assert.IsTrue(test.GetValue(null).Equals("uninitialized"));
            test.SetValue(null, "xxx");
            Assert.IsTrue(test.GetValue(null).Equals("xxx"));
            Assert.IsTrue(MyStaticClass.StaticStringProperty == "xxx");

            test = new BoundPropertyDescriptor(typeof(MyStaticClass),  () => MyStaticClass.StaticIntProperty, "StaticIntProperty", "", "");
            Assert.IsFalse(test.IsReadOnly);
            Assert.IsTrue(test.GetValue(null).Equals(5));
            test.SetValue(null, 10);
            Assert.IsTrue(test.GetValue(null).Equals(10));
            Assert.IsTrue(MyStaticClass.StaticIntProperty == 10);

            test = new BoundPropertyDescriptor(typeof(MyStaticClass), () => MyStaticClass.StaticStringAsymmetricalAccessProperty, "StaticStringAsymmetricalAccessProperty", "", "");
            Assert.IsTrue(test.IsReadOnly);
            Assert.IsTrue(test.GetValue(null).Equals("uninitialized"));
            test.SetValue(null, "xxx");
            Assert.IsTrue(test.GetValue(null).Equals("xxx"));
            Assert.IsTrue(MyStaticClass.StaticStringAsymmetricalAccessProperty == "xxx");

            test = new BoundPropertyDescriptor(typeof(MyStaticClass), () => MyStaticClass.StaticIntAsymmetricalProperty, "StaticIntAsymmetricalProperty", "", "");
            Assert.IsTrue(test.IsReadOnly);
            Assert.IsTrue(test.GetValue(null).Equals(5));
            test.SetValue(null, 10);
            Assert.IsTrue(test.GetValue(null).Equals(10));
            Assert.IsTrue(MyStaticClass.StaticIntAsymmetricalProperty == 10);
        }
    }
}
