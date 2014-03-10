//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Text;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// An abstract base class for services (e.g., MEF components) that can expose
    /// C# objects to a scripting language</summary>
    public abstract class ScriptingService
    {
        /// <summary>
        /// Gets the language display name</summary>
        public string DisplayName
        {
            get { return m_engine.Setup.DisplayName; }
        }
        /// <summary>
        /// Loads assembly into script domain</summary>
        /// <param name="assembly">Assembly</param>
        public virtual void LoadAssembly(Assembly assembly)
        {
            m_engine.Runtime.LoadAssembly(assembly);
        }

        /// <summary>
        /// Imports all the types from the given namespace</summary>
        /// <param name="nmspace">Namespace</param>
        public abstract void ImportAllTypes(string nmspace);

        /// <summary>
        /// Imports given type from given namespace</summary>
        /// <param name="nmspace">Namespace</param>
        /// <param name="typename">Imported type</param>
        public abstract void ImportType(string nmspace, string typename);

        /// <summary>
        /// Try to gets variable from scripting domain</summary>
        /// <typeparam name="T">Type of the variable</typeparam>
        /// <param name="name">Variable name</param>
        /// <param name="var">Variable </param>
        /// <returns>True iff the variable found</returns>
        public bool TryGetVariable<T>(string name, out T var)
        {
            return m_scope.TryGetVariable<T>(name, out var);
        }

        /// <summary>
        /// Exposes C# variable to script</summary>
        /// <param name="name">Variable name used in script</param>
        /// <param name="var">The object to be exposed</param>
        public void SetVariable(string name, object var)
        {
            m_scope.SetVariable(name, var);
        }

        /// <summary>
        /// Removes C# variable from script domain</summary>
        /// <param name="name">Name of the variable</param>
        public void RemoveVariable(string name)
        {
            m_scope.RemoveVariable(name);
        }

        /// <summary>
        /// Executes a single script statement. The statement does not need a carriage return.</summary>
        /// <param name="statement">Script statement</param>
        public string ExecuteStatement(string statement)
        {
            return ExecuteStatement(statement, false);
        }

        /// <summary>
        /// Executes multiple script statements</summary>
        /// <param name="statements">Script statements</param>
        public string ExecuteStatements(string statements)
        {
            return ExecuteStatement(statements, true);
        }

        /// <summary>
        /// Executes a statement directly on the script engine, returning the actual object the statement returns</summary>
        /// <param name="statement"></param>
        /// <returns>A dynamic type, whatever the type the statement returns</returns>
        public dynamic ExecuteSilent(string statement)
        {
            return m_engine.Execute(statement, m_scope);
        }

        private string ExecuteStatement(string statement, bool multiStatements)
        {
            string result = string.Empty;

            if (string.IsNullOrEmpty(statement))
                return result;

            try
            {
                SourceCodeKind sourceKind = multiStatements ? SourceCodeKind.Statements
                    : SourceCodeKind.SingleStatement;
                ScriptSource source =
                    m_engine.CreateScriptSourceFromString(statement, sourceKind);
                
                // JAL - 7/18/2012 - 
                // 
                //  If you are running from the debugger and see the following:
                //
                //         An exception of type 'IronPython.Runtime.Exceptions.ImportException' occurred in IronPython.Modules.dll 
                //         and wasn't handled before a managed/native boundary
                //
                //         Additional information: not a Zip file
                //
                //  and if this exception occurs during application initialization (trying to import python modules, e.g. from system import *
                //  then here are some notes that may help:
                //
                //      - you may safely continue through these exceptions, and possibly disable them with a debugger setting.
                //      - this behavior only occurs when running from the debugger
                //      - behavior started when upgrading from IronPython 2.6 to 2.7.3
                //      - the exception is part of normal Python flow control, and is related to loading modules from zip files
                //      - In VS2010: Tools/Options -> Debugging/General --> set a check on option "just my code"
                //           - note: this alters the appearance of "Debug/Exceptions" menu
                //           - note: unfortunately didn't work for me, but worked for everyone else.
                //           - will update this comment when resolved.
                //
                //  UPDATE:  Ron disabled the zipimport feature as a more reliable workaround (see BasicPythonService.cs)
                //   
                source.Execute(m_scope);
                result = m_stream.Text;
            }
            catch (Exception ex)
            {
                ExceptionOperations eo
                    = m_engine.GetService<ExceptionOperations>();
                result = m_stream.Text + eo.FormatException(ex);
            }

            m_stream.Reset();
            return result;
        }

        /// <summary>
        /// Executes the given file</summary>
        /// <param name="fileName">Full file path</param>
        /// <returns>Script output text</returns>
        public string ExecuteFile(string fileName)
        {
            string result = string.Empty;
            try
            {
                FileInfo finfo = new FileInfo(fileName);
                if (!finfo.Exists)
                    throw new FileNotFoundException(finfo.FullName);

                ScriptSource source =
                    m_engine.CreateScriptSourceFromFile(finfo.FullName);

                source.Execute(m_scope);
                result = m_stream.Text;
            }
            catch (Exception ex)
            {
                ExceptionOperations eo
                    = m_engine.GetService<ExceptionOperations>();
                result = m_stream.Text + eo.FormatException(ex);
            }
            m_stream.Reset();
            return result;
        }

        /// <summary>
        /// Sets scripting engine</summary>
        /// <param name="engine">Scripting engine</param>
        protected void SetEngine(ScriptEngine engine)
        {
            if (m_engine != null)
                throw new InvalidOperationException("engine is already set");

            if (engine == null)
                throw new ArgumentNullException("engine");

            m_engine = engine;
            m_stream = new ScriptOutStream();
            m_engine.Runtime.IO.SetOutput(m_stream, Encoding.ASCII);
            m_scope = m_engine.CreateScope();

            LoadDefaultAssemblies(m_engine.Runtime);
        }

        /// <summary>
        /// Loads the initial assemblies into the given ScriptRuntime</summary>
        /// <param name="runtime">ScriptRuntime</param>
        protected virtual void LoadDefaultAssemblies(ScriptRuntime runtime)
        {
            //BEGIN: for backwards compatibility with ATF 3.1
            runtime.LoadAssembly(typeof(string).Assembly);
            runtime.LoadAssembly(typeof(System.Drawing.Point).Assembly);
            runtime.LoadAssembly(typeof(System.Uri).Assembly);
            runtime.LoadAssembly(typeof(System.Xml.XmlReader).Assembly);
            runtime.LoadAssembly(typeof(Sce.Atf.Dom.DomNode).Assembly);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;
                if (name.Equals("System.Windows.Forms") ||
                    name.Equals("Atf.Gui.WinForms") ||
                    name.Equals("Atf.Gui"))
                {
                    runtime.LoadAssembly(assembly);
                }
            }
            //END: for backwards compatibility with ATF 3.1
        }

        // Custom stream class for capturing script output
        private class ScriptOutStream : Stream
        {
            public void Reset()
            {
                m_output = string.Empty;
            }
            public string Text { get { return m_output; } }

            public override bool CanRead { get { return false; } }

            public override bool CanSeek { get { return false; } }

            public override bool CanWrite { get { return true; } }

            public override long Length { get { return 0; } }

            public override long Position { get { return 0; } set { } }


            public override void Flush()
            {

            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return 0;
            }

            public override void SetLength(long value)
            {

            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                m_output += Encoding.UTF8.GetString(buffer, offset, count);
            }

            private string m_output = string.Empty;
        }

        private ScriptEngine m_engine;
        private ScriptScope m_scope;
        private ScriptOutStream m_stream;
    }
}
