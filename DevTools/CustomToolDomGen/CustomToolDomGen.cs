using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.CustomTool;
using Microsoft.Win32;

namespace DomGen
{
    [Guid("9F877959-E457-4824-B58F-110FF320F0D1")]
    [ComVisible(true)]
    public class CustomToolDomGen : BaseCodeGeneratorWithSite
    {
        // Called every time the attached XML file is saved within visual studio.
        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            // class name should always the same as output file name
            string className = Path.GetFileNameWithoutExtension(inputFileName);

            SchemaLoader typeLoader = new SchemaLoader();
            typeLoader.Load(inputFileName);

            string[] fakeArgs = {"CustomToolDomGen", inputFileName, Path.GetFileNameWithoutExtension(inputFileName) + ".cs", className, FileNameSpace};

            return System.Text.Encoding.ASCII.GetBytes(SchemaGen.Generate(typeLoader, "", FileNameSpace, className, fakeArgs));
        }

        #region Registration

        private const string CustomToolName = "DomSchemaGen";
        private const string CustomToolDescription = "DOM Schema Class Generator";
        private static Guid CustomToolGuid = new Guid("{9F877959-E457-4824-B58F-110FF320F0D1}");    // generated, but must match the class attribute

        // Registry Categories to tell Visual Studio where to find the tool
        private static Guid CSharpCategory = new Guid("{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}");
        private static Guid VBCategory =     new Guid("{164B10B9-B200-11D0-8C61-00A0C91E29D5}"); 

        private const string KeyFormat = @"SOFTWARE\Microsoft\VisualStudio\{0}\Generators\{1}\{2}";

        protected static void Register(Version vsVersion, Guid categoryGuid)
        {
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(
                String.Format(KeyFormat, vsVersion, categoryGuid.ToString("B"), CustomToolName)))
            {
                key.SetValue("", CustomToolDescription);
                key.SetValue("CLSID", CustomToolGuid.ToString("B"));
                key.SetValue("GeneratesDesignTimeSource", 1);
            }
        }
        
        protected static void Unregister(Version vsVersion, Guid categoryGuid)
        {
            Registry.LocalMachine.DeleteSubKey(
                String.Format(KeyFormat, vsVersion, categoryGuid.ToString("B"), CustomToolName), false);
        }

        [ComRegisterFunction]
        public static void RegisterClass(Type t)
        {
            // Register for VS.NET 2002, 2003, 2005, 2008, 2010 (C#)
            Register(new Version(7, 0), CSharpCategory);
            Register(new Version(7, 1), CSharpCategory);
            Register(new Version(8, 0), CSharpCategory);
            Register(new Version(9, 0), CSharpCategory);
            Register(new Version(10, 0), CSharpCategory);

            // Register for VS.NET 2002, 2003, 2005, 2008, 2010 (VB)
            Register(new Version(7, 0), VBCategory);
            Register(new Version(7, 1), VBCategory);
            Register(new Version(8, 0), VBCategory);
            Register(new Version(9, 0), VBCategory);
            Register(new Version(10, 0), VBCategory);
        }

        [ComUnregisterFunction]
        public static void UnregisterClass(Type t)
        {
            // Unregister for VS.NET 2002, 2003, 2005, 2008, 2010 (C#)
            Unregister(new Version(7, 0), CSharpCategory);
            Unregister(new Version(7, 1), CSharpCategory);
            Unregister(new Version(8, 0), CSharpCategory);
            Unregister(new Version(9, 0), CSharpCategory);
            Unregister(new Version(10, 0), CSharpCategory);

            // Unregister for VS.NET 2002, 2003, 2005, 2008, 2010 (VB)
            Unregister(new Version(7, 0), VBCategory);
            Unregister(new Version(7, 1), VBCategory);
            Unregister(new Version(8, 0), VBCategory);
            Unregister(new Version(9, 0), VBCategory);
            Unregister(new Version(10, 0), VBCategory);
        }

        #endregion
    }
}
