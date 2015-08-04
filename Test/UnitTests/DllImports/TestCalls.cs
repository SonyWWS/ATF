//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Runtime.InteropServices;
using NUnit.Framework;
using Sce.Atf;

namespace UnitTests.Atf.DllImports
{
    [TestFixture]
    public class TestCalls
    {
        [Test]
        public void TestSHGetFileInfo()
        {
            var shellFileInfo = new Shell32.SHFILEINFO();
            var cbFileInfo = (uint)Marshal.SizeOf(shellFileInfo);
            Shell32.SHGetFileInfo(".", Shell32.FILE_ATTRIBUTE_DIRECTORY, ref shellFileInfo,
                cbFileInfo, Shell32.SHGFI_DISPLAYNAME);
            Assert.AreEqual("tests", shellFileInfo.szDisplayName);
        }
    }
}
