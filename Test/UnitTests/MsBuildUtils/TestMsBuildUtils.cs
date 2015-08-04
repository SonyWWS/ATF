//Copyright © 2015 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MsBuildUtils;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class TestMsBuildUtils
    {
        [Test]
        public void TestGetProjectFilenamesFromProject()
        {
            var projFiles = GetProjectFilenames(Directory.GetCurrentDirectory() + "\\MsBuildUtils\\Cases\\Test.csproj");

            Assert.AreEqual(5, projFiles.Count);
            Assert.IsTrue(projFiles.Contains("Cases/UserControl1.xaml"));
            Assert.IsTrue(projFiles.Contains("Cases/UserControl1.xaml.cs"));
            Assert.IsTrue(projFiles.Contains("Cases/Properties/AssemblyInfo.cs"));
            Assert.IsTrue(projFiles.Contains("Cases/Properties/Resources.Designer.cs"));
            Assert.IsTrue(projFiles.Contains("Cases/Properties/Settings.Designer.cs"));
        }

        [Test]
        public void TestGetProjectFilenamesFromSolution()
        {
            var projFiles = GetProjectFilenames(Directory.GetCurrentDirectory() + "\\MsBuildUtils\\Cases\\Test.sln");

            Assert.AreEqual(5, projFiles.Count);
            Assert.IsTrue(projFiles.Contains("Cases/UserControl1.xaml"));
            Assert.IsTrue(projFiles.Contains("Cases/UserControl1.xaml.cs"));
            Assert.IsTrue(projFiles.Contains("Cases/Properties/AssemblyInfo.cs"));
            Assert.IsTrue(projFiles.Contains("Cases/Properties/Resources.Designer.cs"));
            Assert.IsTrue(projFiles.Contains("Cases/Properties/Settings.Designer.cs"));
        }

        [Test]
        public void TestGetXamlFilenamesFromProject()
        {
            var projFiles = GetXamlFilenames(Directory.GetCurrentDirectory() + "\\MsBuildUtils\\Cases\\Test.csproj");

            Assert.AreEqual(1, projFiles.Count);
            Assert.IsTrue(projFiles.Contains("Cases/UserControl1.xaml"));
        }

        [Test]
        public void TestGetXamlFilenamesFromSolution()
        {
            var projFiles = GetXamlFilenames(Directory.GetCurrentDirectory() + "\\MsBuildUtils\\Cases\\Test.sln");

            Assert.AreEqual(1, projFiles.Count);
            Assert.IsTrue(projFiles.Contains("Cases/UserControl1.xaml"));
        }

        [Test]
        public void TestGetCsFilenamesFromProject()
        {
            var projFiles = GetCsFilenames(Directory.GetCurrentDirectory() + "\\MsBuildUtils\\Cases\\Test.csproj");

            Assert.AreEqual(4, projFiles.Count);
            Assert.IsTrue(projFiles.Contains("Cases/UserControl1.xaml.cs"));
            Assert.IsTrue(projFiles.Contains("Cases/Properties/AssemblyInfo.cs"));
            Assert.IsTrue(projFiles.Contains("Cases/Properties/Resources.Designer.cs"));
            Assert.IsTrue(projFiles.Contains("Cases/Properties/Settings.Designer.cs"));
        }

        [Test]
        public void TestGetCsFilenamesFromSolution()
        {
            var projFiles = GetCsFilenames(Directory.GetCurrentDirectory() + "\\MsBuildUtils\\Cases\\Test.sln");

            Assert.AreEqual(4, projFiles.Count);
            Assert.IsTrue(projFiles.Contains("Cases/UserControl1.xaml.cs"));
            Assert.IsTrue(projFiles.Contains("Cases/Properties/AssemblyInfo.cs"));
            Assert.IsTrue(projFiles.Contains("Cases/Properties/Resources.Designer.cs"));
            Assert.IsTrue(projFiles.Contains("Cases/Properties/Settings.Designer.cs"));
        }

        private List<string> GetProjectFilenames(string pathName)
        {
            var fileNames = MsBuildFile.GetProjectFilenames(pathName);
            return new List<string>(ToRelativePaths(fileNames, Path.GetDirectoryName(pathName)));
        }

        private List<string> GetXamlFilenames(string pathName)
        {
            var fileNames = MsBuildFile.GetXamlFilenames(pathName);
            return new List<string>(ToRelativePaths(fileNames, Path.GetDirectoryName(pathName)));
        }

        private List<string> GetCsFilenames(string pathName)
        {
            var fileNames = MsBuildFile.GetCsharpFilenames(pathName);
            return new List<string>(ToRelativePaths(fileNames, Path.GetDirectoryName(pathName)));
        }

        private IEnumerable<string> ToRelativePaths(IEnumerable<string> fileNames, string rootDir)
        {
            var rootUri = new Uri(rootDir);
            return fileNames
                .Select(fileName => new Uri(fileName, UriKind.Absolute))
                .Select(xamlUri => rootUri.MakeRelativeUri(xamlUri).ToString());
        }
    }
}
