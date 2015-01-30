//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;

using NUnit.Framework;

using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestAttributeType : DomTest
    {
        [Test]
        public void TestScalarConstructor()
        {
            // fully test one type
            {
                AttributeType test = new AttributeType("test", typeof(SByte));
                Assert.AreEqual(test.Name, "test");
                Assert.AreEqual(test.ClrType, typeof(SByte));
                Assert.AreEqual(test.Type, AttributeTypes.Int8);
                Assert.AreEqual(test.Length, 1);
                Assert.False(test.IsArray);

                Assert.AreEqual((SByte)test.GetDefault(), (SByte)0);
                // test validation of type, without any custom rules
                Assert.True(test.Validate((SByte)1, null));
                Assert.False(test.Validate((Int32)1, null));

                Assert.AreEqual(test.GetDefault(), (SByte)0);
            }

            // test an unrecognized type
            {
                AttributeType test = new AttributeType("test", typeof(AttributeType[])); // unrecognized type is treated as string
                Assert.AreEqual(test.ClrType, typeof(String));
                Assert.AreEqual(test.Type, AttributeTypes.String);
                Assert.AreEqual(test.Length, 1);
                Assert.False(test.IsArray);
            }

            // test all other types
            TestScalar<Byte>(AttributeTypes.UInt8);
            TestScalar<Int16>(AttributeTypes.Int16);
            TestScalar<UInt16>(AttributeTypes.UInt16);
            TestScalar<Int32>(AttributeTypes.Int32);
            TestScalar<UInt32>(AttributeTypes.UInt32);
            TestScalar<Int64>(AttributeTypes.Int64);
            TestScalar<UInt64>(AttributeTypes.UInt64);
            TestScalar<Single>(AttributeTypes.Single);
            TestScalar<Double>(AttributeTypes.Double);
            TestScalar<Decimal>(AttributeTypes.Decimal);
            TestScalar<Boolean>(AttributeTypes.Boolean);
            TestScalar<DateTime>(AttributeTypes.DateTime);

            TestScalar<String>(AttributeTypes.String);
            Assert.AreEqual(new AttributeType("test", typeof(string)).GetDefault(), string.Empty);

            TestScalar<Uri>(AttributeTypes.Uri);
            Assert.Null(new AttributeType("test", typeof(Uri)).GetDefault());

            TestScalar<DomNode>(AttributeTypes.Reference);
            Assert.Null(new AttributeType("test", typeof(DomNode)).GetDefault());

            TestArray<SByte>(AttributeTypes.Int8Array);
            TestArray<Byte>(AttributeTypes.UInt8Array);
            TestArray<Int16>(AttributeTypes.Int16Array);
            TestArray<UInt16>(AttributeTypes.UInt16Array);
            TestArray<Int32>(AttributeTypes.Int32Array);
            TestArray<UInt32>(AttributeTypes.UInt32Array);
            TestArray<Int64>(AttributeTypes.Int64Array);
            TestArray<UInt64>(AttributeTypes.UInt64Array);
            TestArray<Single>(AttributeTypes.SingleArray);
            TestArray<Double>(AttributeTypes.DoubleArray);
            TestArray<Decimal>(AttributeTypes.DecimalArray);
            TestArray<Boolean>(AttributeTypes.BooleanArray);
            TestArray<String>(AttributeTypes.StringArray);
        }

        private void TestScalar<T>(AttributeTypes type)
        {
            AttributeType test = new AttributeType("test", typeof(T));
            Assert.AreEqual(test.ClrType, typeof(T));
            Assert.AreEqual(test.Type, type);

            // test default for value types
            object typeDefault = default(T);
            if (typeDefault != null)
            {
                Assert.AreEqual(test.GetDefault(), typeDefault);
            }
        }

        private void TestArray<T>(AttributeTypes type)
        {
            AttributeType test = new AttributeType("test", typeof(T[]), 2);
            Assert.AreEqual(test.ClrType, typeof(T[]));
            Assert.AreEqual(test.Type, type);
            Assert.AreEqual(test.Length, 2);
            Assert.True(test.IsArray);
            //since length < int.MaxValue, default has 'length' elements
            Assert.AreEqual(test.GetDefault(), new T[] { default(T), default(T) });

            test = new AttributeType("testUnbounded", typeof(T[]), Int32.MaxValue);
            Assert.AreEqual(test.ClrType, typeof(T[]));
            Assert.AreEqual(test.Type, type);
            Assert.AreEqual(test.Length, Int32.MaxValue);
            Assert.True(test.IsArray);
            //since length == int.MaxValue, default has zero elements
            Assert.AreEqual(test.GetDefault(), new T[] { });

            test = new AttributeType("testDefaultLength", typeof(T[])); //default is length of 1
            Assert.AreEqual(test.ClrType, typeof(T[]));
            Assert.AreEqual(test.Type, type);
            Assert.AreEqual(test.Length, 1);
            Assert.True(test.IsArray);
            //since length == 1, default is T[] with 1 element
            Assert.AreEqual(test.GetDefault(), new T[] { default(T) });
        }

        [Test]
        public void TestStringArray()
        {
            AttributeType stringArrayType;

            // Test a string array attribute of unspecified length
            stringArrayType = new AttributeType("sentences", typeof (string[]), Int32.MaxValue);
            TestStringArray(stringArrayType);

            // Test a string array attribute of fixed length
            stringArrayType = new AttributeType("sentences", typeof(string[]), 4);
            TestStringArray(stringArrayType);
        }

        // Test that arrays of strings are converted to and from the 'object' type correctly.
        private void TestStringArray(AttributeType stringArrayType)
        {
            // Test without spaces, like in ATF 3.1 when spaces were not handled correctly.
            {
                string[] array = {
                                     "e1",
                                     "e2",
                                     "e3",
                                     "e4"
                                 };
                string toString = stringArrayType.Convert(array);
                object toObject = stringArrayType.Convert(toString);
                Assert.IsTrue(stringArrayType.AreEqual(array, toObject));
            }

            // Test elements that have spaces and quotes ('"') and other special characters.
            {
                string[] array = {
                                     "first",
                                     "second with spaces",
                                     "third with \\@##\\\\$#%%!\"~~~ possible escape __ characters\n",
                                     "\"fourth\""
                                 };
                string toString = stringArrayType.Convert(array);
                object toObject = stringArrayType.Convert(toString);
                Assert.IsTrue(stringArrayType.AreEqual(array, toObject));
            }

            // Test that empty strings and null strings are encoded correctly.
            {
                string[] array = {
                                     "first element",
                                     "",
                                     "\"",
                                     ""
                                 };
                string toString = stringArrayType.Convert(array);
                object toObject = stringArrayType.Convert(toString);
                Assert.IsTrue(stringArrayType.AreEqual(array, toObject));
            }
        }

        [Test]
        public void TestRules()
        {
            {
                AttributeType test = new AttributeType("test", typeof(SByte));
                CollectionAssert.IsEmpty(test.Rules);

                SimpleAttributeRule rule = new SimpleAttributeRule();
                test.AddRule(rule);

                CollectionAssert.Contains(test.Rules, rule);
                Assert.False(rule.Validated);
                test.Validate(null, null);
                Assert.True(rule.Validated);
            }
        }

        [Test]
        public void TestAreEqual()
        {
            TestAreEqual<SByte>(0, 1);
            TestAreEqual<Byte>(0, 1);
            TestAreEqual<Int16>(0, 1);
            TestAreEqual<UInt16>(0, 1);
            TestAreEqual<Int32>(0, 1);
            TestAreEqual<UInt32>(0, 1);
            TestAreEqual<Int64>(0, 1);
            TestAreEqual<UInt64>(0, 1);
            TestAreEqual<Single>(0, 1);
            TestAreEqual<Double>(0, 1);
            TestAreEqual<Decimal>(0, 1);
            TestAreEqual<Boolean>(true, false);
            TestAreEqual<String>("foo", "bar");

            TestAreEqualScalar<DateTime>(DateTime.Now, new DateTime());

            TestAreEqualScalar<Uri>(new Uri("foo", UriKind.Relative), new Uri("bar", UriKind.Relative));

            DomNodeType nodeType = new DomNodeType("foo");
            TestAreEqualScalar<DomNode>(new DomNode(nodeType), new DomNode(nodeType));
        }

        private void TestAreEqualScalar<T>(T value, T unequalValue)
        {
            // test scalar comparison
            AttributeType scalarTest = new AttributeType("test", typeof(T));
            Assert.True(scalarTest.AreEqual(value, value));
            Assert.True(scalarTest.AreEqual(unequalValue, unequalValue));
            Assert.False(scalarTest.AreEqual(value, unequalValue));
        }

        private void TestAreEqual<T>(T value, T unequalValue)
        {
            TestAreEqualScalar<T>(value, unequalValue);

            // test array comparison
            AttributeType arrayTest = new AttributeType("test", typeof(T[]), 2);
            Assert.True(arrayTest.AreEqual(new T[] { value, unequalValue }, new T[] { value, unequalValue }));
            Assert.False(arrayTest.AreEqual(new T[] { value }, new T[] { value, unequalValue }));
            Assert.False(arrayTest.AreEqual(new T[] { value, value }, new T[] { value, unequalValue }));
        }

        [Test]
        public void TestClone()
        {
            TestClone<SByte>(1);
            TestClone<Byte>(1);
            TestClone<Int16>(1);
            TestClone<UInt16>(1);
            TestClone<Int32>(1);
            TestClone<UInt32>(1);
            TestClone<Int64>(1);
            TestClone<UInt64>(1);
            TestClone<Single>(1);
            TestClone<Double>(1);
            TestClone<Decimal>(1);
            TestClone<Boolean>(true);
            TestClone<String>("foo");

            TestCloneScalar<DateTime>(DateTime.Now);

            TestCloneScalar<Uri>(new Uri("foo", UriKind.Relative));

            DomNodeType nodeType = new DomNodeType("foo");
            TestCloneScalar<DomNode>(new DomNode(nodeType));
        }

        private void TestCloneScalar<T>(T value)
        {
            // test scalar clone
            AttributeType scalarTest = new AttributeType("test", typeof(T));
            object original = value;
            object clone = scalarTest.Clone(original);
            Assert.AreEqual(original, clone);
            Assert.AreSame(original, clone);
        }

        private void TestClone<T>(T value)
        {
            TestCloneScalar<T>(value);

            // test array clone
            AttributeType arrayTest = new AttributeType("test", typeof(T[]), 2);
            object original = new T[] { value, value };
            object clone = arrayTest.Clone(original);
            Assert.AreEqual(original, clone);
            Assert.AreNotSame(original, clone);
        }

        [Test]
        public void TestConvertToString()
        {
            TestConvertToString<SByte>(0, 1);
            TestConvertToString<Byte>(0, 1);
            TestConvertToString<Int16>(0, 1);
            TestConvertToString<UInt16>(0, 1);
            TestConvertToString<Int32>(0, 1);
            TestConvertToString<UInt32>(0, 1);
            TestConvertToString<Int64>(0, 1);
            TestConvertToString<UInt64>(0, 1);
            TestConvertToString<Single>(0, 1);
            TestConvertToString<Double>(0, 1);
            TestConvertToString<Decimal>(0, 1);
        }

        private void TestConvertToStringScalar<T>(T value)
            where T : IFormattable
        {
            AttributeType test = new AttributeType("test", typeof(T));
            string expected = value.ToString(null, CultureInfo.InvariantCulture);
            string actual = test.Convert(value);
            Assert.AreEqual(expected, actual);
        }

        private void TestConvertToString<T>(T value1, T value2)
            where T : IFormattable
        {
            TestConvertToStringScalar<T>(value1);

            AttributeType arrayTest = new AttributeType("test", typeof(T[]), 2);
            string valueString1 = value1.ToString(null, CultureInfo.InvariantCulture);
            string valueString2 = value2.ToString(null, CultureInfo.InvariantCulture);
            string expected = valueString1 + " " + valueString2;
            string actual = arrayTest.Convert(new T[] { value1, value2 });
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestConvertStringToString()
        {
            AttributeType test = new AttributeType("test", typeof(string));
            Assert.AreEqual("foo", test.Convert((object)"foo"));
        }

        [Test]
        public void TestConvertStringArrayToString()
        {
            AttributeType test = new AttributeType("test", typeof(string[]), 2);
            Assert.AreEqual("foo bar", test.Convert(new string[] { "foo", "bar" }));
        }

        [Test]
        public void TestConvertBoolToString()
        {
            AttributeType test = new AttributeType("test", typeof(Boolean));
            Assert.AreEqual("true", test.Convert(true));
            Assert.AreEqual("false", test.Convert(false));
        }

        [Test]
        public void TestConvertBoolArrayToString()
        {
            AttributeType test = new AttributeType("test", typeof(Boolean[]), 2);
            Assert.AreEqual("true false", test.Convert(new Boolean[] { true, false }));
        }

        [Test]
        public void TestConvertUriToString()
        {
            AttributeType test = new AttributeType("test", typeof(Uri));
            Uri uri = new Uri("foo", UriKind.Relative);
            Assert.AreEqual(uri.ToString(), test.Convert(uri));
        }

        [Test]
        public void TestConvertRelativeUriContainingSpacesToString()
        {
            var test = new AttributeType("test", typeof(Uri));

            var baseUri = new Uri("C:\\base\\path");
            var relPath = "rel\\path\\with spaces\\in it\\file.txt";

            var absUri = new Uri(baseUri, relPath);
            var relUri = baseUri.MakeRelativeUri(absUri);

            var uriToString = relUri.ToString();
            var uriConverted = test.Convert(relUri);

            Assert.AreEqual(uriToString, uriConverted);
        }
        [Test]
        public void TestConvertFromString()
        {
            TestConvertFromString<SByte>(0, 1);
            TestConvertFromString<Byte>(0, 1);
            TestConvertFromString<Int16>(0, 1);
            TestConvertFromString<UInt16>(0, 1);
            TestConvertFromString<Int32>(0, 1);
            TestConvertFromString<UInt32>(0, 1);
            TestConvertFromString<Int64>(0, 1);
            TestConvertFromString<UInt64>(0, 1);
            TestConvertFromString<Single>(0, 1);
            TestConvertFromString<Double>(0, 1);
            TestConvertFromString<Decimal>(0, 1);
        }

        private void TestConvertFromStringScalar<T>(T value)
            where T : IFormattable
        {
            AttributeType test = new AttributeType("test", typeof(T));
            object converted = test.Convert(value.ToString(null, CultureInfo.InvariantCulture));
            Assert.AreEqual(converted, value);
        }

        private void TestConvertFromString<T>(T value1, T value2)
            where T : IFormattable
        {
            TestConvertFromStringScalar<T>(value1);

            AttributeType arrayTest = new AttributeType("test", typeof(T[]), 2);
            string valueString1 = value1.ToString(null, CultureInfo.InvariantCulture);
            string valueString2 = value2.ToString(null, CultureInfo.InvariantCulture);
            object converted = arrayTest.Convert(valueString1 + " " + valueString2);
            Assert.AreEqual(converted, new T[] { value1, value2 });
        }
    }
}
