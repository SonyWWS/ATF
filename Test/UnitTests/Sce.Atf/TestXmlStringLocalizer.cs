//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestXmlStringLocalizer
    {
        [Test]
        public void TestBasics()
        {
            var localizer = new XmlStringLocalizerTester();

            var doc = new TranslationXmlDocument();
            doc.AddTranslation("A", "ContextY", "Y");
            doc.AddTranslation("B", "", "XB");
            doc.AddTranslation("C", "", "XC");
            localizer.AddLocalizedStrings(doc);

            doc = new TranslationXmlDocument();
            doc.AddTranslation("A", "ContextY", "Y"); //duplicate original and context and translation
            doc.AddTranslation("B", "ContextZ", "Z"); //duplicate original, but different context
            doc.AddTranslation("D", "", "XD");
            localizer.AddLocalizedStrings(doc);

            Assert.AreEqual(localizer.Localize("A", "ContextY"), "Y");
            Assert.AreEqual(localizer.Localize("A", ""), "Y"); // Omit context
            Assert.AreEqual(localizer.Localize("B", ""), "XB");
            Assert.AreEqual(localizer.Localize("C", ""), "XC");
            Assert.AreEqual(localizer.Localize("B", "ContextZ"), "Z");
            Assert.AreEqual(localizer.Localize("D", ""), "XD");
            Assert.AreEqual(localizer.Localize("?", ""), "?"); //no translation available
        }

        [Test]
        public void TestAsteriskTranslation()
        {
            var doc = new TranslationXmlDocument();
            doc.AddTranslation("A", "ContextY", "Y");
            doc.AddTranslation("A", "", "*"); //'*' means no translation available

            var localizer = new XmlStringLocalizerTester();
            localizer.AddLocalizedStrings(doc);

            Assert.AreEqual(localizer.Localize("A", "ContextY"), "Y");
            Assert.AreEqual(localizer.Localize("A", ""), "A");
        }

        [Test]
        public void TestConflictingTranslation()
        {
            var doc = new TranslationXmlDocument();
            doc.AddTranslation("A", "ContextY", "Y");
            doc.AddTranslation("A", "ContextY", "Z"); //Duplicate original and duplicate context, but different translation.
            doc.AddTranslation("B", "", "Y");
            doc.AddTranslation("B", "", "Z"); //Duplicate original and duplicate context, but different translation.

            var localizer = new XmlStringLocalizerTester();
            Assert.Throws<InvalidOperationException>(() => localizer.AddLocalizedStrings(doc));
        }

        private class XmlStringLocalizerTester : XmlStringLocalizer
        {
            public new void AddLocalizedStrings(XmlDocument document)
            {
                base.AddLocalizedStrings(document);
            }
        }

        private class TranslationXmlDocument : XmlDocument
        {
            public TranslationXmlDocument()
            {
                m_root = CreateElement("StringLocalizationTable");
                AppendChild(m_root);
            }

            public void AddTranslation(string original, string context, string translation)
            {
                var row = CreateElement("StringItem");
                row.SetAttribute("id", original);
                row.SetAttribute("context", context);
                row.SetAttribute("translation", translation);
                m_root.AppendChild(row);
            }

            private XmlElement m_root;
        }
    }
}
