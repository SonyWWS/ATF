//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Sce.Atf
{
    /// <summary>
    /// Assembly utilities</summary>
    public static class AssemblyUtil
    {
        /// <summary>
        /// Gets an array of loaded assemblies to the current application domain</summary>
        /// <returns>Array of loaded assemblies</returns>
        public static Assembly[] GetLoadedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// Gets the assembly name for the specified file without loading the assembly.
        /// For unmanaged assemblies, returns null.</summary>
        /// <param name="fileName">Assembly file name</param>
        /// <returns>AssemblyName or null</returns>
        public static AssemblyName GetAssemblyName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("file");
            if (!File.Exists(fileName))
                throw new ArgumentException(fileName + " does not exist");

            AssemblyName result = null;
            try
            {
                result = AssemblyName.GetAssemblyName(fileName);
            }
            catch (Exception ex)
            {
                Outputs.Write(OutputMessageType.Error, fileName + ": ");
                Outputs.WriteLine(OutputMessageType.Error, ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Gets a dictionary of [fullName and assembly] pairs of loaded assemblies in the current
        /// application domain</summary>
        /// <returns>Dictionary of [fullName and assembly] pairs of loaded assemblies in the current
        /// application domain</returns>
        public static Dictionary<string, Assembly> GetDictionaryOfLoadedAssemblies()
        {
            Dictionary<string, Assembly> result = new Dictionary<string, Assembly>();
            Assembly[] assems = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assem in assems)
            {
                if (result.ContainsKey(assem.FullName))
                {
                    Outputs.WriteLine(OutputMessageType.Error, assem.FullName + " has been loaded twice");
                    Outputs.WriteLine(OutputMessageType.Error, "\t" + assem.Location);
                    Outputs.WriteLine(OutputMessageType.Error, "\t" + result[assem.FullName].Location);
                }
                else
                {
                    result.Add(assem.FullName, assem);
                }
            }
            return result;
        }
    }
}
