//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Core
{
    /// <summary>
    /// Exception thrown when the ArgParser can't parse arguments</summary>
    public class ArgParserException : Exception
    {
        /// <summary>
        /// Exception thrown when the ArgParser can't parse arguments</summary>
        /// <param name="message">The message to report</param>
        public ArgParserException(string message) : base(message) { }
    }

    /// <summary>
    /// Parser for command-line arguments</summary>
    /// <remarks>
    /// Most options are specified like "-name value" or "/name value", except for 
    /// boolean options, which are specified like "-name" or "/name". 
    /// Any command-line arguments that aren't part of an option are "extras".
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // For example:
    /// //  foo.exe extra1.txt -speed 3.4 -verbose extra2.txt extra3.txt
    ///
    ///    ArgParser parser = new ArgParser();
    ///    parser.AddOption("speed", typeof(float), 1.0);
    ///    parser.AddOption("verbose", typeof(bool), false);
    ///
    ///    bool result = parser.Parse();
    ///    if (result)
    ///    {
    ///       Console.WriteLine("speed:   {0}", parser["speed"]);
    ///       Console.WriteLine("verbose: {0}", parser["verbose"]);
    ///
    ///       string[] extras = parser.ExtraArgs;
    ///       Console.WriteLine("{0} extra args:", extras.Length);
    ///       foreach (string arg in extras)
    ///          Console.WriteLine("   {0}", arg);
    ///    }
    ///    else
    ///    {
    ///       Console.WriteLine("couldn't parse!");
    ///    }
    /// </code>
    /// </example>
    public class ArgParser
    {
        /// <summary>Constructor</summary>
        /// <remarks>The command line is determined from Environment.GetCommandLineArgs()</remarks>
        public ArgParser()
        {
            string[] args = Environment.GetCommandLineArgs();
            // strip out first element, which is the executable
            _args = new string[args.Length - 1];
            Array.Copy(args, 1, _args, 0, _args.Length);
        }

        /// <summary>Constructor</summary>
        /// <param name="args">The command-line arguments to parse.  This is assumed
        /// not to contain the executable as its first argument; that is, it is 
        /// assumed that the array is the argument to a Main() function, rather than
        /// from Environment.GetCommandLineArgs().</param>
        public ArgParser(string[] args)
        {
            _args = args;
        }

        /// <summary>Add a command-line option to look for on the command line</summary>
        /// <param name="name">The name of the option</param>
        /// <param name="type">The type to be parsed</param>
        /// <param name="defaultValue">The default value for the option</param>
        public void AddOption(string name, Type type, object defaultValue)
        {
            _options[name] = NewOption(type, defaultValue);
        }

        /// <summary>Add an "alias" to an existing option; this alias can be used
        /// instead of the option name.</summary>
        public void AddAlias(string aliasName, string optionName)
        {
            System.Diagnostics.Debug.Assert(_options.ContainsKey(optionName));
            _options[aliasName] = _options[optionName];
        }

        /// <summary>Parse the command line</summary>
        /// <remarks>Throws an exception if the command line could not be parsed</remarks>
        public void Parse()
        {
            List<string> extras = new List<string>();
            for (int i = 0; i < _args.Length; ++i)
            {
                string arg = _args[i];
                if (arg[0] == '-' || arg[0] == '/') // it's an option
                {
                    if (arg.Length == 1)
                        throw new ArgParserException(String.Format("Couldn't parse argument \"{0}\"", arg));
                    IOption opt = (IOption)_options[arg.Substring(1)];
                    if (opt == null)
                        throw new ArgParserException(String.Format("Couldn't parse argument \"{0}\"", arg));
                    i = opt.Set(_args, i);
                }
                else
                {
                    extras.Add(arg);
                }
            }

            _extraArgs = extras;
        }

        /// <summary>The value of the named option parsed from the command
        /// line, or null if none was parsed.</summary>
        public object this[string name]
        {
            get
            {
                IOption opt = (IOption)_options[name];
                if (opt == null)
                    return null;
                return opt.Value;
            }
        }

        /// <summary>An array containing the arguments that followed the last option</summary>
        public IList<string> ExtraArgs
        {
            get { return _extraArgs; }
        }

        #region Option classes

        private interface IOption
        {
            /// <summary>
            /// Set this option from the command line
            /// </summary>
            /// <param name="args">Args data</param>
            /// <param name="i">The index of the option</param>
            /// <returns>The index of the last argument consumed</returns>
            /// <remarks>Throws an exception if the argument could not be parsed</remarks>
            int Set(string[] args, int i);

            /// <summary>The value parsed for this option, or null if not set</summary>
            object Value { get; }
        }

        /// <summary>Parses an option of the form "-name value"</summary>
        private class GenericOption : IOption
        {
            public GenericOption(Type type, object defaultValue)
            {
                _type = type;
                _value = defaultValue;
            }

            public int Set(string[] args, int i)
            {
                if (i >= args.Length - 1)
                    throw new ArgParserException(String.Format("\"{0}\" argument requires a value", args[0]));

                string value = args[i + 1];
                TypeConverter converter = TypeDescriptor.GetConverter(_type);
                try
                {
                    _value = converter.ConvertFromString(value);
                }

                catch (Exception ex)
                {
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                    Outputs.WriteLine(OutputMessageType.Info, ex.StackTrace);
                    throw new ArgParserException(String.Format("Couldn't parse value \"{0}\" for argument \"{1}\"", value, args[0]));
                }

                return i + 1;
            }


            public object Value
            {
                get { return _value; }
            }

            private Type _type;
            private object _value;
        }

        /// <summary>Parses a boolean option of the form "-name"</summary>
        private class BoolOption : IOption
        {
            public BoolOption(object defaultValue)
            {
                _value = defaultValue;
            }

            public int Set(string[] args, int i)
            {
                _value = true;
                return i;
            }

            public object Value
            {
                get { return _value; }
            }

            private object _value;
        }

        #endregion

        private string[] _args;
        private IList<string> _extraArgs;
        private Hashtable _options = new Hashtable();

        private IOption NewOption(Type type, object defaultValue)
        {
            if (type == typeof(bool))
                return new BoolOption(defaultValue);
            else
                return new GenericOption(type, defaultValue);
        }
    }
}
