//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

using Scea.UnitTesting;

namespace UnitTestAtgi
{
    class Program
    {
        static int Main(string[] args)
        {
            string assemblyName = Assembly.GetEntryAssembly().Location;
            return Scea.UnitTesting.UnitTestRunner.RunAllTests(assemblyName);            
        }
    }
}