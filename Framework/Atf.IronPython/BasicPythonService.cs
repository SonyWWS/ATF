//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Text;
using IronPython;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Basic Python service that provides the Python scripting engine and imports many common
    /// .NET and ATF types into the Python namespace. Consider using ScriptConsole and
    /// AtfScriptVariables as additional MEF components.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ScriptingService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BasicPythonService : ScriptingService, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        [ImportingConstructor]
        public BasicPythonService()
        {
            ScriptEngine engine = CreateEngine();
            SetEngine(engine);
            Initialize();
        }

        /// <summary>
        /// Creates the Python engine with some default options and common assembly imports.
        /// Called by the constructor.</summary>
        protected virtual ScriptEngine CreateEngine()
        {
            // create and init scripting engine. 
            Dictionary<String, Object> options = new Dictionary<string, object>();
            options["DivisionOptions"] = PythonDivisionOptions.New;
            options["PrivateBinding"] = true;

            ScriptEngine engine = Python.CreateEngine(options);
            
            // Set system variables that need to be different from the default.
            // The zipimporter module is on the path_hooks list by default and it throws exceptions and/or
            //  prevents the "import *" command from working on some computers. We don't need it, so let's
            //  remove it.  If you need to re-add it, see detailed comment in ScriptingServices.cs
            ScriptScope sysScope = engine.GetSysModule();
            sysScope.SetVariable("path_hooks", new List());
            
            return engine;
        }

        /// <summary>
        /// Initializes the current Python engine with some common assembly imports.
        /// Called by the constructor after CreateEngine.</summary>
        protected virtual void Initialize()
        {
            StringBuilder importStatement = new StringBuilder();
            importStatement.AppendLine("import clr");
            importStatement.AppendLine("import System");

            // These namespaces are guaranteed to exist because Atf.IronPython depends on assemblies
            //  that contain these namespaces.
            importStatement.AppendLine("from System import *");
            importStatement.AppendLine("from System.Drawing import *");
            importStatement.AppendLine("from System.Collections.Generic import *");
            importStatement.AppendLine("from System.Collections.ObjectModel import *");
            importStatement.AppendLine("from System.Windows.Forms import *");
            importStatement.AppendLine("from System.Text import *");
            importStatement.AppendLine("from System.IO import *");
            importStatement.AppendLine("from System.Xml.Schema import *");
            importStatement.AppendLine("from System.Xml.XPath import *");
            importStatement.AppendLine("from System.Xml.Serialization import *");
            importStatement.AppendLine("from Sce.Atf import *");
            importStatement.AppendLine("from Sce.Atf.Applications import *");
            importStatement.AppendLine("from Sce.Atf.VectorMath import *");
            importStatement.AppendLine("from Sce.Atf.Adaptation import *");
            importStatement.AppendLine("from Sce.Atf.Dom import *");

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.FullName.StartsWith("Atf.") ||
                    assembly.FullName.StartsWith("Scea."))
                {
                    LoadAssembly(assembly);
                }

                if (assembly.FullName.StartsWith("Atf.Gui.WinForms"))
                {
                    importStatement.AppendLine("from Sce.Atf.Controls import *");
                    importStatement.AppendLine("from Sce.Atf.Controls.Adaptable import *");
                }
                else if (assembly.FullName.StartsWith("Scea.Core"))
                {
                    importStatement.AppendLine("from Scea.Editors.Host.Internal import *");
                }
                else if (assembly.FullName.StartsWith("Scea.Dom"))
                {
                    importStatement.AppendLine("from Scea.Dom import *");
                }
            }

            //Console.WriteLine("BasicPythonService loading:\n" + importStatement);
            ExecuteStatements(importStatement.ToString());
        }

        /// <summary>
        /// Imports all types from a given namespace</summary>
        /// <param name="nmspace">Namespace</param>
        public override void ImportAllTypes(string nmspace)
        {
            ExecuteStatement(string.Format("from {0} import *", nmspace));            
        }

        /// <summary>
        /// Imports a given type from a specified namespace</summary>
        /// <param name="nmspace">Namespace to import from</param>
        /// <param name="typename">Type to import</param>
        public override void ImportType(string nmspace, string typename)
        {            
            ExecuteStatement(string.Format("from {0} import {1}", nmspace, typename));
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {           
        }

        #endregion
    }   
}
