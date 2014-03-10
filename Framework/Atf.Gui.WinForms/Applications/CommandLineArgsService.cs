//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Command line arguments parsing service that validates the arguments and facilitates the option access. 
    /// It assumes that arguments without the option prefix ( '-' and '/') are file names, and tries to 
    /// auto-load them at application startup.</summary>
    /// <remarks>
    /// Most options are specified as "-name value" or "/name value", except for 
    /// boolean options, which are specified as "-name" or "/name". 
    /// Any command-line arguments that aren't part of an option are "Parameters".
    /// 
    /// AutoDocumentService is used to automatically open last opened documents at application startup,  
    /// but CommandLineArgsService takes precedence to open the files specified from command line,  
    /// if both services are instantiated.</remarks>
    [Export(typeof (IInitializable))]
    [Export(typeof (CommandLineArgsService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CommandLineArgsService : IInitializable, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="documentRegistry">Document registry used to get and set the active document and to
        /// know which documents are open when the main form closes</param>
        /// <param name="documentService">Document service used to open previously opened documents and to
        /// close the auto-generated new document</param>
        [ImportingConstructor]
        public CommandLineArgsService(
            IDocumentRegistry documentRegistry,
            IDocumentService documentService)
        {
            m_documentRegistry = documentRegistry;
            m_documentService = documentService;
        }

        /// <summary>Gets command line arguments that are not options</summary>
        public IList<string> Parameters
        {
            get { return m_parameters; }
        }

        /// <summary>
        /// Gets the value of the named option parsed from the command
        /// line, or null if none was parsed</summary>
        public object this[string name]
        {
            get
            {
                IOption opt = (IOption) m_options[name];
                if (opt == null)
                    return null;
                return opt.Value;
            }
        }

        /// <summary>
        /// Adds a command-line option to look for on the command line</summary>
        /// <param name="name">The name of the option</param>
        /// <param name="type">The type to be parsed</param>
        /// <param name="defaultValue">The default value for the option</param>
        public void AddOption(string name, Type type, object defaultValue)
        {
            m_options[name] = NewOption(type, defaultValue);
        }

        /// <summary>
        /// Adds an "alias" to an existing option; this alias can be used
        /// instead of the option name</summary>
        public void AddAlias(string aliasName, string optionName)
        {
            Debug.Assert(m_options.ContainsKey(optionName));
            m_options[aliasName] = m_options[optionName];
        }

        /// <summary>
        /// Exception thrown when the ArgParser can't parse arguments</summary>
        public class ArgParserException : Exception
        {
            public ArgParserException(string message) : base(message)
            {
            }
        }

        #region IPartImportsSatisfiedNotification Members

        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            if (m_mainWindow == null &&
                m_mainForm != null)
            {
                m_mainWindow = new MainFormAdapter(m_mainForm);
            }

            if (m_mainWindow == null)
                throw new InvalidOperationException("Can't get main window");

            m_mainWindow.Loading += mainWindow_Loaded;
        }

        #endregion

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            string[] args = Environment.GetCommandLineArgs();
            // strip out first element, which is the executable
            var realArgs = new string[args.Length - 1];
            Array.Copy(args, 1, realArgs, 0, realArgs.Length);
            Parse(realArgs);
        }

        #endregion

        #region Option classes

        private interface IOption
        {
            /// <summary>
            /// Sets this option from the command line</summary>
            /// <param name="args">Args data</param>
            /// <param name="i">The index of the option</param>
            /// <returns>The index of the last argument consumed</returns>
            /// <remarks>Throws an exception if the argument could not be parsed</remarks>
            int Set(string[] args, int i);

            /// <summary>
            /// Gets the value parsed for this option, or null if not set</summary>
            object Value { get; }
        }

        /// <summary>
        /// Parses an option of the form "-name value"</summary>
        private class GenericOption : IOption
        {
            public GenericOption(Type type, object defaultValue)
            {
                m_type = type;
                m_value = defaultValue;
            }

            public int Set(string[] args, int i)
            {
                if (i >= args.Length - 1)
                    throw new ArgParserException(String.Format("\"{0}\" argument requires a value", args[0]));

                string value = args[i + 1];
                TypeConverter converter = TypeDescriptor.GetConverter(m_type);
                try
                {
                    m_value = converter.ConvertFromString(value);
                }

                catch (Exception ex)
                {
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                    Outputs.WriteLine(OutputMessageType.Info, ex.StackTrace);
                    throw new ArgParserException(String.Format("Couldn't parse value \"{0}\" for argument \"{1}\"",
                                                               value, args[0]));
                }

                return i + 1;
            }


            public object Value
            {
                get { return m_value; }
            }

            private Type m_type;
            private object m_value;
        }

        /// <summary>
        /// Parses a boolean option of the form "-name"</summary>
        private class BoolOption : IOption
        {
            public BoolOption(object defaultValue)
            {
                m_value = defaultValue;
            }

            public int Set(string[] args, int i)
            {
                m_value = true;
                return i;
            }

            public object Value
            {
                get { return m_value; }
            }

            private object m_value;
        }

        #endregion

        static private bool IsArgOption(string arg)
        {
            return  arg[0] == '-' || arg[0] == '/';
        }

    /// <summary>
        /// Parses the command line</summary>
        /// <remarks>Throws an exception if the command line could not be parsed</remarks>
        public void Parse(string[] args)
        {
            List<string> extras = new List<string>();
            for (int i = 0; i < args.Length; ++i)
            {
                string arg = args[i];
                if (IsArgOption(arg)) // it's an option
                {
                    if (arg.Length == 1)
                        throw new ArgParserException(String.Format("Couldn't parse argument \"{0}\"", arg));
                    string argName = arg.Substring(1);
                    if (i < args.Length-1 && !IsArgOption(args[i+1]) )
                        AddOption(argName, typeof(string), string.Empty);
                    else
                        AddOption(argName, typeof(bool), true);

                    IOption opt = (IOption)m_options[argName];
                    if (opt == null)
                        throw new ArgParserException(String.Format("Couldn't parse argument \"{0}\"", arg));
                    i = opt.Set(args, i);
                }
                else
                {
                    extras.Add(arg);
                }
            }

            m_parameters = extras;
        }

        private IOption NewOption(Type type, object defaultValue)
        {
            if (type == typeof(bool))
                return new BoolOption(defaultValue);
            else
                return new GenericOption(type, defaultValue);
        }


        private void mainWindow_Loaded(object sender, EventArgs e)
        {
             bool documentsOpen = m_documentRegistry.ActiveDocument != null;

            // auto-load documents only if there are none open yet
            if (Parameters.Count > 0  && !documentsOpen)
            {
                // assume Parameters are filenames (try to open them)
                foreach (string uriString in Parameters)
                {
                    Uri uri;
                    if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
                    {
                        foreach (IDocumentClient client in m_documentClients.GetValues())
                        {
                            if (client.CanOpen(uri))
                            {
                                m_documentService.OpenExistingDocument(client, uri);
                                break;
                            }
                        }
                    }
                }
            }        
        }

        private readonly IDocumentRegistry m_documentRegistry;
        private readonly IDocumentService m_documentService;

        private IList<string> m_parameters;
        private Hashtable m_options = new Hashtable();

        [Import(AllowDefault = true)]
        private IMainWindow m_mainWindow;

        [Import(AllowDefault = true)]
        private Form m_mainForm;

  
        [ImportMany]
        private IEnumerable<Lazy<IDocumentClient>> m_documentClients;

    }
}
