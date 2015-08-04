//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using NUnit.Framework;

using Sce.Atf;
using DomGen;

namespace UnitTests.Atf
{
    public class TestSchemaGen
    {
        [Test]
        public void TestGenerate()
        {
            //"usage:   DomGen {schemaPath} {outputPath} {schemaNamespace}  {classNamespace} -a(dapters)"
            //"eg:      DomGen echo.xsd Schema.cs http://sce/audio Sce.Audio -a"
            string inputPath = ".\\Resources\\test_customized.xsd";
            string schemaNamespace = "test";
            string codeNamespace = "testCodeNamespace";
            string className = "TestClass";

            SchemaLoader typeLoader = new SchemaLoader();
            typeLoader.Load(inputPath);

            // Normal test
            string schemaClass = SchemaGen.Generate(typeLoader, schemaNamespace, codeNamespace, className, EmptyArray<string>.Instance);
            Validate(schemaClass, schemaNamespace, codeNamespace, className, false);
            
            // Test with -annotatedOnly option
            string[] args = new string[] { inputPath, "", schemaNamespace, codeNamespace, "-annotatedOnly" };
            schemaClass = SchemaGen.Generate(typeLoader, schemaNamespace, codeNamespace, className, args);
            Validate(schemaClass, schemaNamespace, codeNamespace, className, true);
        }

        private void Validate(string schemaClass, string schemaNamespace, string codeNamespace, string className, bool annotatedOnly)
        {
            string[] lines = schemaClass.Split(Environment.NewLine.ToCharArray());

            string namespaceLine = "namespace " + codeNamespace;
            string classNameLine = "public static class " + className;
            string schemaNamespaceFieldLine = "public const string NS = \"" + schemaNamespace + "\";";
            string complexTypeDeclaration = "public static class complexType";
            string complexType2Declaration = "public static class complexType2";
            string importedDeclaration = "public static class importedComplexType";
            string includeTypeDeclaration = "public static class includeType";
            string excludeTypeDeclaration = "public static class excludeType";
            string elementOfArrayOfSimpleType = "public static ChildInfo TestBoolArrayChild;";
            string attributeOfListType = "public static AttributeInfo TestListAttribute;";
            bool foundNamespace = false;
            bool foundClassName = false;
            bool foundSchemaNamespace = false;
            bool foundComplexType = false;
            bool foundComplexType2 = false;
            bool foundImported = false;
            bool foundIncludeType = false;
            bool foundExcludeType = false;
            bool foundChildInfoOfElementOfArrayOfSimpleType = false;
            bool foundAttributeOfListType = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Equals(namespaceLine))
                    foundNamespace = true;
                else if (line.Equals(classNameLine))
                    foundClassName = true;
                else if (line.Equals(schemaNamespaceFieldLine))
                    foundSchemaNamespace = true;
                else if (line.Equals(complexTypeDeclaration))
                    foundComplexType = true;
                else if (line.Equals(complexType2Declaration))
                    foundComplexType2 = true;
                else if (line.Equals(importedDeclaration))
                    foundImported = true;
                else if (line.Equals(includeTypeDeclaration))
                    foundIncludeType = true;
                else if (line.Equals(excludeTypeDeclaration))
                    foundExcludeType = true;
                else if (line.Equals(elementOfArrayOfSimpleType))
                    foundChildInfoOfElementOfArrayOfSimpleType = true;
                else if (line.Equals(attributeOfListType))
                {
                    Assert.IsFalse(foundAttributeOfListType);
                    foundAttributeOfListType = true;
                }
            }
            Assert.IsTrue(foundNamespace);
            Assert.IsTrue(foundClassName);
            Assert.IsTrue(foundSchemaNamespace);
            if (annotatedOnly)
            {
                // Only types explicitly marked for inclusion are to be included
                Assert.IsFalse(foundComplexType);
                Assert.IsFalse(foundComplexType2);
                Assert.IsFalse(foundImported);
                Assert.IsTrue(foundIncludeType); // only included type
                Assert.IsFalse(foundExcludeType);
            }
            else
            {
                // All types are included except those explicitly marked for exclusion
                Assert.IsTrue(foundComplexType);
                Assert.IsTrue(foundComplexType2);
                Assert.IsTrue(foundImported);
                Assert.IsTrue(foundIncludeType);
                Assert.IsFalse(foundExcludeType); // only excluded type
                Assert.IsTrue(foundChildInfoOfElementOfArrayOfSimpleType);
                Assert.IsTrue(foundAttributeOfListType);
            }
        }
    }
}
