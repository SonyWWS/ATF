//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Interop
{
    internal class CommandClientAdapter : Sce.Atf.Applications.ICommandClient

    {
        public CommandClientAdapter(Sce.Atf.Applications.ICommandClient adaptee)
        {
            m_adaptee = adaptee;
        }

        public void AddCommand(ICommandItem command)
        {
            lock (m_commands)
            {
                m_commands.Add(command);
            }
        }

        public ICommandItem RemoveCommand(object commandTag)
        {
            var command = m_commands.FirstOrDefault<ICommandItem>((x) => CommandComparer.TagsEqual(x.CommandTag, commandTag));
            if (command != null)
            {
                lock (m_commands)
                {
                    m_commands.Remove(command);
                }
            }
            return command;
        }

        private HashSet<ICommandItem> m_commands = new HashSet<ICommandItem>();

        public void UpdateCommands()
        {
            ICommandItem[] commands;
            lock (m_commands)
            {
                commands = m_commands.ToArray<ICommandItem>();
            }

            foreach (var command in commands)
            {
                var commandState = new Atf.Applications.CommandState(command.Text, command.IsChecked);

                m_adaptee.UpdateCommand(command.CommandTag, commandState);

                if (commandState.Text != command.Text)
                    command.Text = commandState.Text;
                if (commandState.Check != command.IsChecked)
                    command.IsChecked = commandState.Check;
            }
        }

        #region ICommandClient Members

        public bool CanDoCommand(object command)
        {
            ICommandItem commandItem = command as ICommandItem;
            Requires.NotNull(command, "Object specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
            return m_adaptee.CanDoCommand(commandItem.CommandTag);
        }

        public void DoCommand(object command)
        {
            ICommandItem commandItem = command as ICommandItem;
            Requires.NotNull(command, "Object specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
            m_adaptee.DoCommand(commandItem.CommandTag);
        }

        public void UpdateCommand(object commandTag, Sce.Atf.Applications.CommandState commandState)
        {
            throw new InvalidOperationException("CommandClientAdapter.UpdateCommand() - WPF shouldn't ever be calling this method, and suggests a non-WPF app is erroneously using CommandClientAdapter.");
        }

        #endregion

        private Sce.Atf.Applications.ICommandClient m_adaptee;
    }
}
