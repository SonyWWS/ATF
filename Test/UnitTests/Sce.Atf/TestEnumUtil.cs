//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;
using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestEnumUtil
    {
        private enum Enum1
        {
            Zero = 0,
            One = 1,
            Two = 2,
            TwentyTwo = 22
        }

        private enum Enum2
        {
            [DisplayString("twenty-two, busted")]
            TwentyTwo = 22,
            NegativeOne = -1,
            Zero = 0,
            [DisplayString("The One")]
            One = 1,
            Two = 2
        }

        [Test]
        public void TestGetDisplayString()
        {
            string s = EnumUtil.GetDisplayString(typeof(Enum1), Enum1.Zero);
            Assert.IsTrue(s == "Zero");
            s = EnumUtil.GetDisplayString(typeof(Enum1), Enum1.TwentyTwo);
            Assert.IsTrue(s == "TwentyTwo");

            s = EnumUtil.GetDisplayString(typeof(Enum2), Enum2.NegativeOne);
            Assert.IsTrue(s == "NegativeOne");
            s = EnumUtil.GetDisplayString(typeof(Enum2), Enum2.Zero);
            Assert.IsTrue(s == "Zero");
            s = EnumUtil.GetDisplayString(typeof(Enum2), Enum2.TwentyTwo);
            Assert.IsTrue(s == "twenty-two, busted");
            s = EnumUtil.GetDisplayString(typeof(Enum2), Enum2.One);
            Assert.IsTrue(s == "The One");

            s = EnumUtil.GetDisplayString<Enum1>(Enum1.Zero);
            Assert.IsTrue(s == "Zero");
            s = EnumUtil.GetDisplayString<Enum1>(Enum1.TwentyTwo);
            Assert.IsTrue(s == "TwentyTwo");

            s = EnumUtil.GetDisplayString<Enum2>(Enum2.NegativeOne);
            Assert.IsTrue(s == "NegativeOne");
            s = EnumUtil.GetDisplayString<Enum2>(Enum2.Zero);
            Assert.IsTrue(s == "Zero");
            s = EnumUtil.GetDisplayString<Enum2>(Enum2.TwentyTwo);
            Assert.IsTrue(s == "twenty-two, busted");
            s = EnumUtil.GetDisplayString<Enum2>(Enum2.One);
            Assert.IsTrue(s == "The One");
        }

        [Test]
        public void TestParseEnumDefinitions()
        {
            string[] names;
            int[] values;
            
            EnumUtil.ParseEnumDefinitions(
                new [] {"One", "Two", "Three"}, out names, out values);
            Assert.IsTrue(names.Length == 3);
            Assert.IsTrue(names[0] == "One");
            Assert.IsTrue(names[1] == "Two");
            Assert.IsTrue(names[2] == "Three");
            Assert.IsTrue(values.Length == 3);
            Assert.IsTrue(values[0] == 0);
            Assert.IsTrue(values[1] == 1);
            Assert.IsTrue(values[2] == 2);

            string[] displayNames;
            EnumUtil.ParseEnumDefinitions(
                new[] { "One", "Two", "Three" }, out names, out displayNames, out values);
            Assert.IsTrue(names.Length == 3);
            Assert.IsTrue(names[0] == "One");
            Assert.IsTrue(names[1] == "Two");
            Assert.IsTrue(names[2] == "Three");
            Assert.IsTrue(displayNames.Length == 3);
            Assert.IsTrue(displayNames[0] == "One");
            Assert.IsTrue(displayNames[1] == "Two");
            Assert.IsTrue(displayNames[2] == "Three");
            Assert.IsTrue(values.Length == 3);
            Assert.IsTrue(values[0] == 0);
            Assert.IsTrue(values[1] == 1);
            Assert.IsTrue(values[2] == 2);

            EnumUtil.ParseEnumDefinitions(
                new[] { "One=1", "Two=2", "Three=3" }, out names, out displayNames, out values);
            Assert.IsTrue(names.Length == 3);
            Assert.IsTrue(names[0] == "One");
            Assert.IsTrue(names[1] == "Two");
            Assert.IsTrue(names[2] == "Three");
            Assert.IsTrue(displayNames.Length == 3);
            Assert.IsTrue(displayNames[0] == "One");
            Assert.IsTrue(displayNames[1] == "Two");
            Assert.IsTrue(displayNames[2] == "Three");
            Assert.IsTrue(values.Length == 3);
            Assert.IsTrue(values[0] == 1);
            Assert.IsTrue(values[1] == 2);
            Assert.IsTrue(values[2] == 3);

            EnumUtil.ParseEnumDefinitions(
                new[] { "NegativeOne=-1", "Two=2", "Three=3" }, out names, out displayNames, out values);
            Assert.IsTrue(names.Length == 3);
            Assert.IsTrue(names[0] == "NegativeOne");
            Assert.IsTrue(names[1] == "Two");
            Assert.IsTrue(names[2] == "Three");
            Assert.IsTrue(displayNames.Length == 3);
            Assert.IsTrue(displayNames[0] == "NegativeOne");
            Assert.IsTrue(displayNames[1] == "Two");
            Assert.IsTrue(displayNames[2] == "Three");
            Assert.IsTrue(values.Length == 3);
            Assert.IsTrue(values[0] == -1);
            Assert.IsTrue(values[1] == 2);
            Assert.IsTrue(values[2] == 3);

            EnumUtil.ParseEnumDefinitions(
                new[] { "NegativeOne==negative=-1", "Two==two apples=2", "Three==Curly, Moe, and Larry=3" },
                out names, out displayNames, out values);
            Assert.IsTrue(names.Length == 3);
            Assert.IsTrue(names[0] == "NegativeOne");
            Assert.IsTrue(names[1] == "Two");
            Assert.IsTrue(names[2] == "Three");
            Assert.IsTrue(displayNames.Length == 3);
            Assert.IsTrue(displayNames[0] == "negative");
            Assert.IsTrue(displayNames[1] == "two apples");
            Assert.IsTrue(displayNames[2] == "Curly, Moe, and Larry");
            Assert.IsTrue(values.Length == 3);
            Assert.IsTrue(values[0] == -1);
            Assert.IsTrue(values[1] == 2);
            Assert.IsTrue(values[2] == 3);

            EnumUtil.ParseEnumDefinitions(
                new[] { "NegativeOne==negative=-1", "Max==maximum integer!#@$!="+int.MaxValue, "Three==Curly, Moe, and Larry=3" },
                out names, out displayNames, out values);
            Assert.IsTrue(names.Length == 3);
            Assert.IsTrue(names[0] == "NegativeOne");
            Assert.IsTrue(names[1] == "Max");
            Assert.IsTrue(names[2] == "Three");
            Assert.IsTrue(displayNames.Length == 3);
            Assert.IsTrue(displayNames[0] == "negative");
            Assert.IsTrue(displayNames[1] == "maximum integer!#@$!");
            Assert.IsTrue(displayNames[2] == "Curly, Moe, and Larry");
            Assert.IsTrue(values.Length == 3);
            Assert.IsTrue(values[0] == -1);
            Assert.IsTrue(values[1] == int.MaxValue);
            Assert.IsTrue(values[2] == 3);
        }
    }
}
