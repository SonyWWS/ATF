//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Markup;
using Sce.Atf.Wpf.Applications;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Markup
{
    [ContentProperty("Command")]
    public class CommandServiceExtension : MarkupExtension
    {
        public CommandServiceExtension()
        {
        }

        public CommandServiceExtension(object command)
        {
            Command = command;
        }

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

        public object Command { get; set; }

    }
}
