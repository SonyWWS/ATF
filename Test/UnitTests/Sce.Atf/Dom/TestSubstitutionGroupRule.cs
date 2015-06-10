// See License.txt.

using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Sce.Atf;
using Sce.Atf.Dom;

namespace UnitTests.Atf.Dom
{
    [TestFixture]
    public class TestSubstitutionGroupRule
    {
        [Test]
        public void TestCorrectSubstitutionsExist()
        {
            var loader = GetSchemaLoader();

            DomNodeType containerType = loader.GetNodeType("test:containerType");
            var subRule =
                containerType.GetChildInfo("basic").Rules.OfType<SubstitutionGroupChildRule>().SingleOrDefault();

            Assert.NotNull(subRule, "No SubstitutionGroupChildRule was generated.");
            Assert.AreEqual(3, subRule.Substitutions.Count(), "Wrong number of substitutions for rule.");
            Assert.NotNull(subRule.Substitutions.SingleOrDefault(s => s.Type.Name == "test:containerType"), "Missing containerType substitution in rule.");
            Assert.NotNull(subRule.Substitutions.SingleOrDefault(s => s.Type.Name == "test:middleType"), "Missing middleType substitution in rule.");
            Assert.NotNull(subRule.Substitutions.SingleOrDefault(s => s.Type.Name == "test:descendantType"), "Missing descendantType substitution in rule.");
        }

        [Test]
        public void TestDomXmlWriterPicksCorrectSubstitutions()
        {
            var loader = GetSchemaLoader();
            DomNode node;
            string output;
            const string testDoc = @"﻿<?xml version=""1.0"" encoding=""utf-8""?>
<root xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns=""test"">
	<basic name=""something"" />
	<middle name=""some-other-thing"" special=""true"" />
	<descendant name=""leaf-thing"" special=""true"" extra=""important"" />
	<container name=""some-container"">
		<middle name=""some-nested-thing"" />
		<middle name=""some-other-nested-thing"" special=""true"" />
	</container>
</root>";
            using (Stream s = CreateStreamFromString(testDoc))
            {
                DomXmlReader reader = new DomXmlReader(loader);
                node = reader.Read(s, null);
            }

            Assert.AreEqual(4, node.Children.Count());
            var children = node.Children.ToList();
            Assert.AreEqual("test:basicType", children[0].Type.Name);
            Assert.AreEqual("test:middleType", children[1].Type.Name);
            Assert.AreEqual("test:descendantType", children[2].Type.Name);
            Assert.AreEqual("test:containerType", children[3].Type.Name);


            using (MemoryStream s = new MemoryStream())
            {
                DomXmlWriter writer = new DomXmlWriter(loader.GetTypeCollection("test"));
                writer.Write(node, s, null);
                output = Encoding.UTF8.GetString(s.ToArray());
            }

            Assert.AreEqual(testDoc, output);
        }

        private XmlSchemaTypeLoader GetSchemaLoader()
        {
            XmlSchemaTypeLoader loader = new XmlSchemaTypeLoader();
            loader.SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(),
                "UnitTests.Atf/Resources");
            loader.Load("testSubstitutionGroups.xsd");

            return loader;
        }

        private Stream CreateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
