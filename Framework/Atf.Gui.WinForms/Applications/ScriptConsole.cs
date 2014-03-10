//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf.Controls.ConsoleBox;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Python service that provides a dockable command console for entering Python commands
    /// and imports many common .NET and ATF types into the Python namespace</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ScriptConsole))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ScriptConsole : IInitializable, IControlHostClient
    {
        /// <summary>
        /// Constructor</summary>
        [ImportingConstructor]
        public ScriptConsole()
        {
            m_consoleBox = new ConsoleTextBox();
            m_consoleBox.Control.Dock = DockStyle.Fill;
            m_consoleBox.CommandHandler = ProcessCommand;
            m_consoleBox.SuggestionHandler = Suggestions;
        }

        /// <summary>
        /// Constructor for when MEF isn't used. The caller is responsible for registering
        /// the Control with a control host service, if applicable.</summary>
        /// <param name="scriptService">ScriptingService to expose C# objects to a scripting language</param>
        /// <param name="controlHostService">Control host service</param>
        public ScriptConsole(ScriptingService scriptService, IControlHostService controlHostService)
            : this()
        {
            m_controlHostService = controlHostService;
            m_scriptService = scriptService;
        }

        /// <summary>
        /// Constructor for when MEF isn't used. The caller is responsible for registering
        /// the Control with a control host service, if applicable.</summary>
        /// <param name="scriptService">ScriptingService to expose C# objects to a scripting language</param>
        public ScriptConsole(ScriptingService scriptService)
            : this()
        {
            m_scriptService = scriptService;
        }

        /// <summary>
        /// Gets the underlying Control that should be registered with a Control host service</summary>
        public Control Control
        {
            get { return m_consoleBox.Control; }
        }

        /// <summary>
        /// Gets the IConsoleTextBox interface for the console's Control</summary>
        public IConsoleTextBox ConsoleTextBox
        {
            get { return m_consoleBox; }
        }

        /// <summary>
        /// Registers a custom function to be called when collecting auto-complete suggestions for a specific object type.</summary>
        /// <param name="type">The object type</param>
        /// <param name="trigger">The delimiter that marks the end of a variable name.  
        ///                       Note: Currently only '.' and '[' are supported triggers</param>
        /// <param name="func">The function to call that returns a list of suggestions for the associated type</param>
        public void RegisterSuggestor(Type type, string trigger, Func<IEnumerable<string>> func)
        {
            m_registeredSuggestors.Add(new Tuple<Type, string>(type, trigger), func);
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            if (m_scriptService != null && m_controlHostService != null)
            {
                m_controlHostService.RegisterControl(m_consoleBox.Control,
                    m_scriptService.DisplayName,
                    String.Format("Interactive {0} Console", m_scriptService.DisplayName),
                    StandardControlGroup.Bottom,
                    null, this,
                    "https://github.com/SonyWWS/ATF/wiki/Scripting-Applications-with-Python".Localize());
            }
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the control gets focus, or a parent "host" control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        void IControlHostClient.Activate(Control control)
        {

        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another control or "host" control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        void IControlHostClient.Deactivate(Control control)
        {

        }

        /// <summary>
        /// Requests permission to close the client's Control.</summary>
        /// <param name="control">Client control to be closed</param>
        /// <returns>True if the control can close, or false to cancel</returns>
        bool IControlHostClient.Close(Control control)
        {
            return true;
        }

        #endregion

        private void ProcessCommand(string cmd)
        {
            if (StringUtil.IsNullOrEmptyOrWhitespace(cmd))
                return;

            cmd = cmd.Trim();

            // process commands 
            if (cmd == "cls")
            {
                m_consoleBox.Clear();
            }
            else if (cmd.StartsWith("runfile "))
            {
                string file = cmd.Substring("runfile ".Length);
                file = file.Trim();
                string result = m_scriptService.ExecuteFile(file);
                m_consoleBox.Write(result);
            }
            else
            {
                string result = m_scriptService.ExecuteStatement(cmd);
                m_consoleBox.Write(result);
            }

            m_consoleBox.Control.Focus();
        }

        private IEnumerable<string> Suggestions(string obj, string trigger)
        {
            IEnumerable<object> result = null;
            try
            {
                result = m_scriptService.ExecuteSilent(String.Format("dir({0})", obj)) as IEnumerable<object>;

                if (!String.IsNullOrWhiteSpace(obj))
                {
                    Func<IEnumerable<string>> func;
                    var type = m_scriptService.ExecuteSilent(String.Format("{0}.GetType()", obj));
                    if (m_registeredSuggestors.TryGetValue(new Tuple<Type, string>(type, trigger), out func))
                        return func();
                }
            }
            catch (Exception)
            {
            }

            return (result != null)
                    ? result.Cast<string>()
                    : Enumerable.Empty<string>();
        }

        [Import(AllowDefault = true)]
        private IControlHostService m_controlHostService;

        [Import(AllowDefault = true)]
        private ScriptingService m_scriptService;

        private IConsoleTextBox m_consoleBox;
        private readonly Dictionary<Tuple<Type, string>, Func<IEnumerable<string>>> m_registeredSuggestors = new Dictionary<Tuple<Type, string>, Func<IEnumerable<string>>>();
    }
}
