//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Command that is a composite of multiple commands</summary>
    public class CompositeCommand : Command
    {
        /// <summary>
        /// Constructor using sub-commands collection</summary>
        /// <param name="commands">Collection of sub-commands</param>
        public CompositeCommand(IEnumerable<Command> commands)
            : this(null, commands)
        {
        }

        /// <summary>
        /// Constructor using description and sub-commands collection</summary>
        /// <param name="description">Command description</param>
        /// <param name="commands">Collection of sub-commands</param>
        public CompositeCommand(string description, IEnumerable<Command> commands)
            : base(description)
        {
            m_commands = new List<Command>(commands);
        }

        /// <summary>
        /// Does/Redoes the command</summary>
        public override void Do()
        {
            for (int i = 0; i < m_commands.Count; ++i)
            {
                Command command = m_commands[i];
                // if a command can't be completed successfully, back up to consistent state
                bool success = false;
                try
                {
                    command.Do();
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        for (--i; i >= 0; --i)
                        {
                            Command done = m_commands[i];
                            done.Undo();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Undoes the command</summary>
        public override void Undo()
        {
            // undo commands in reverse order
            for (int i = m_commands.Count - 1; i >= 0; --i)
                m_commands[i].Undo();
        }

        /// <summary>
        /// Adds a new command to the end of this composite command</summary>
        /// <param name="command">Command to add to the end</param>
        public void Add(Command command)
        {
            CompositeCommand compositeCommand = command as CompositeCommand;
            if (compositeCommand != null)
                m_commands.AddRange(compositeCommand.m_commands);
            else
                m_commands.Add(command);
        }
        
        /// <summary>
        /// Returns a command that is the optimal equivalent of the composite command (null
        /// if composite is empty)</summary>
        /// <returns>Command that is the optimal equivalent of the composite or null if composite empty</returns>
        public Command Optimize()
        {
            // Note that this doesn't optimize out children CompositeCommands
            if (m_commands.Count > 1)
                return this;

            if (m_commands.Count == 0)
                return null;

            // must be a single command, change its description and return it
            Command result = m_commands[0];
            result.Description = Description;
            return result;
        }

        private readonly List<Command> m_commands;
    }
}
