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
            Assert.AreEqual(subRule.Substitutions.Count(), 2, "Wrong number of substitutions for rule.");
            Assert.NotNull(subRule.Substitutions.SingleOrDefault(s => s.Type.Name == "test:containerType"),
                "Missing containerType substitution in rule.");
            Assert.NotNull(subRule.Substitutions.SingleOrDefault(s => s.Type.Name == "test:someOtherType"),
                "Missing someOtherType substitution in rule.");
        }

        [Test]
        public void TestDomXmlWriterPicksCorrectSubstitutions()
        {
            var loader = GetSchemaLoader();
            DomNode node;
            string output;
            string testDoc = @"<root>
									<basic name=""something""/>
									<someOther name=""some-other-thing"" special=""true"" />
									<container name=""some-container"">
										<someOther name=""some-nested-thing"" special=""false"" />
										<someOther name=""some-other-nested-thing"" special=""false"" />
									</container>
								</root>";

            using (Stream s = CreateStreamFromString(testDoc))
            {
                DomXmlReader reader = new DomXmlReader(loader);
                node = reader.Read(s, null);
            }

            Assert.AreEqual(node.Children.Count(), 3);
            var children = node.Children.ToList();
            Assert.AreEqual(children[0].Type.Name, "test:basicType");
            Assert.AreEqual(children[1].Type.Name, "test:someOtherType");
            Assert.AreEqual(children[2].Type.Name, "test:containerType");


            using (MemoryStream s = new MemoryStream())
            {
                DomXmlWriter writer = new DomXmlWriter(loader.GetTypeCollection("test"));
                writer.Write(node, s, null);
                output = Encoding.UTF8.GetString(s.ToArray());
            }

            Assert.AreEqual(output, @"﻿<?xml version=""1.0"" encoding=""utf-8""?>
<root xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns=""test"">
	<basic name=""something"" />
	<someOther name=""some-other-thing"" special=""true"" />
	<container name=""some-container"">
		<someOther name=""some-nested-thing"" />
		<someOther name=""some-other-nested-thing"" />
	</container>
</root>");
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
