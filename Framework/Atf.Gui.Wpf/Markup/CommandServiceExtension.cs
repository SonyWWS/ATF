//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Markup;
using Sce.Atf.Wpf.Applications;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Markup
{
    /// <summary>
    /// Command service markup extension</summary>
    [ContentProperty("Command")]
    public class CommandServiceExtension : MarkupExtension
    {
        /// <summary>
        /// Default constructor</summary>
        public CommandServiceExtension()
        {
        }

        /// <summary>
        /// Constructor with command</summary>
        /// <param name="command">Command object</param>
        public CommandServiceExtension(object command)
        {
            Command = command;
        }

        /// <summary>
        /// Return object set as value of target property for this markup extension</summary>
        /// <param name="serviceProvider">Object that provides custom support</param>
        /// <returns>ICommandItem representing a command</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ICommand result = null;
            var composer = Composer.Current;
            
            if (composer != null && Command != null)
            {
                var commandService = composer.Container.GetExportedValue<CommandService>();
                if (commandService != null)
                {
                    result = commandService.GetCommand(Command);
                }
            }

            System.Diagnostics.Debug.WriteLineIf(result == null, "Error binding to command service command item " + Command);

            return result;
        }

        /// <summary>
        /// Get or set command</summary>
        public object Command { get; set; }

    }
}
