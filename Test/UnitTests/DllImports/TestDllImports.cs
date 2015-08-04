//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;

namespace UnitTests.Atf.DllImports
{
    /// <summary>
    /// Tests Platform Invoke (DllImportAttribute) usage, to enforce some rules</summary>
    [TestFixture]
    public class TestDllImports
    {
        [Test]
        public void CheckDllImports()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // We don't want to test the .NET Framework assemblies.
                // It's good to test our 3rd party libraries, though.
                if (!assembly.GlobalAssemblyCache)
                {
                    foreach (Type t in assembly.GetTypes())
                    {
                        CheckDllImports(t);
                    }
                }
            }
        }

        // Look for methods on this type that are DllImports.
        private void CheckDllImports(Type type)
        {
            foreach (var m in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                foreach(object attr in m.GetCustomAttributes(typeof(DllImportAttribute), false))
                {
                    var importAttribute = (DllImportAttribute) attr;
                    
                    // We want to use the Unicode version of the Win32 function whenever possible.
                    // Unfortunately, the default in C# is to use ANSI strings. If CharSet is not
                    //  Unicode, verify that strings are not used for parameters or return values.
                    // https://msdn.microsoft.com/en-us/library/vstudio/system.runtime.interopservices.charset(v=vs.100).aspx
                    if (importAttribute.CharSet != CharSet.Unicode)
                        AssertNoStrings(m);

                    // Check that all structs have the correct StructLayout attribute -- either
                    //  explicit or sequential.
                    // Todo: make this work with struct references, too.
                    // Note that it's not possible to have memory corruption by using LayoutKind.Auto;
                    //  a run-time exception will be thrown in this case.
                    // To completely test a particular struct that will be marshaled, put the test in
                    //  NativeTestHelpers.
                    AssertLayoutKindOnParams(m);
                }
            }

            foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
                CheckDllImports(nestedType);
        }

        private void AssertLayoutKindOnParams(MethodInfo importedMethod)
        {
            Type returnType = importedMethod.ReturnParameter.ParameterType;
            AssertLayoutKind(returnType);

            foreach (ParameterInfo parameter in importedMethod.GetParameters())
            {
                AssertLayoutKind(parameter.ParameterType);
            }
        }

        private void AssertLayoutKind(Type type)
        {
            if (IsStruct(type))
            {
                Assert.IsTrue(type.IsLayoutSequential || type.IsExplicitLayout,
                    "This type is a struct that is used on a DllImport, but it does not have" +
                    " the StructLayout attribute with either LayoutKind.Sequential or" +
                    " LayoutKind.Explicit.");
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic))
                {
                    AssertLayoutKind(field.FieldType);
                }
            }
        }

        private static void AssertNoStrings(MethodInfo importedMethod)
        {
            Type returnType = importedMethod.ReturnParameter.ParameterType;
            if (ContainsStringType(returnType))
            {
                Assert.Fail("This method needs its DllImport attribute to specify CharSet.Unicode" +
                            " because the return type is a string or contains a string: " + importedMethod.Name);
            }

            foreach (ParameterInfo parameter in importedMethod.GetParameters())
            {
                if (ContainsStringType(parameter.ParameterType))
                    Assert.Fail("This method needs its DllImport attribute to specify CharSet.Unicode" +
                                " because it has a parameter that is a string or contains a string: " + importedMethod.Name);
            }
        }

        private static bool ContainsStringType(Type type)
        {
            if (IsStruct(type))
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (ContainsStringType(field.FieldType))
                        return true;
                }
            }
            else
            {
                return
                    typeof(string).IsAssignableFrom(type) ||
                    typeof(StringBuilder).IsAssignableFrom(type);
            }
            return false;
        }

        private static bool IsStruct(Type type)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum &&
                !type.IsArray;
        }
    }
}
