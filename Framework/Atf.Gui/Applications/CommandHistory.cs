//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// A Command history for undo/redo</summary>
    public class CommandHistory
    {
        /// <summary>
        /// Constructor</summary>
        public CommandHistory()
        {
            m_commands = new List<Command>();
            m_commandCount = new CommandCount();
            m_commandCount.DirtyChanged += commandCount_DirtyChanged;
        }


        /// <summary>
        /// Gets index of the current command.</summary>
        public int Current
        {
            get { return m_commandCount.Current; }
        }

        /// <summary>
        /// Gets command count.</summary>
        public int Count 
        {
            get { return m_commands.Count; }
        }

        /// <summary>
        /// Gets command at the given index</summary>        
        public Command this[int index]
        {
            get { return m_commands[index];}
        }

        /// <summary>
        /// Clear the command history</summary>
        public void Clear()
        {
            m_commandCount.Reset();
            m_commands.Clear();
        }

        /// <summary>
        /// Gets whether there is a command that can be undone</summary>
        public bool CanUndo
        {
            get { return m_commandCount.CanDecrement; }
        }

        /// <summary>
        /// Gets if there is a command that can be redone</summary>
        public bool CanRedo
        {
            get { return m_commandCount.Current < m_commands.Count; }
        }

        /// <summary>
        /// Gets and sets whether the command history is at its "clean" point</summary>
        /// <remarks>Setting this property to false sets the "clean" point. Setting
        /// it to true forces a "dirty" state until the next time Dirty is set to false.</remarks>
        public bool Dirty
        {
            get { return m_commandCount.Dirty; }
            set { m_commandCount.Dirty = value; }
        }

        /// <summary>
        /// Event that is raised after a command is added or redone</summary>
        public event EventHandler CommandDone;

        /// <summary>
        /// Event that is raised after a command is undone</summary>
        public event EventHandler CommandUndone;

        /// <summary>
        /// Event that is raised when the Dirty state changes</summary>
        public event EventHandler DirtyChanged;

        /// <summary>
        /// Gets the description of the next undoable command</summary>
        public string UndoDescription
        {
            get
            {
                string name = "";
                if (CanUndo)
                {
                    Command command = m_commands[m_commandCount.Current - 1];
                    name = command.Description;
                }
                return name;
            }
        }

        /// <summary>
        /// Gets the name of the next redoable command</summary>
        public string RedoDescription
        {
            get
            {
                string name = "";
                if (CanRedo)
                {
                    Command command = m_commands[m_commandCount.Current];
                    name = command.Description;
                }
                return name;
            }
        }

        /// <summary>
        /// Gets the last successfully completed command. This is the command that is
        /// undone if the user presses Ctrl+z.</summary>
        public Command LastDone
        {
            get
            {
                int i = m_commandCount.Current - 1;
                return i >= 0 ? m_commands[i] : null;
            }
        }

        /// <summary>
        /// Adds a command to the history</summary>
        /// <param name="command">Command to add</param>
        /// <remarks>Use when the command has already been done.</remarks>
        public void Add(Command command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            // remove any un-done commands
            ClearUndoneCommands();

            Command last = LastDone;
            m_commands.Add(command);
            m_commandCount.Increment();

            OnCommandDone();
        }

        /// <summary>
        /// Undo the last "done" command</summary>
        public Command Undo()
        {
            if (!CanUndo)
                throw new InvalidOperationException("Can't undo");

            Command command = m_commands[m_commandCount.Current - 1];
            m_commandCount.Decrement();

            command.Undo();

            OnCommandUndone();

            return command;
        }

        /// <summary>
        /// Redo the last "undone" command</summary>
        public Command Redo()
        {
            if (!CanRedo)
                throw new InvalidOperationException("Can't redo");

            Command command = m_commands[m_commandCount.Current];
            m_commandCount.Increment();

            command.Do();

            OnCommandDone();

            return command;
        }

        /// <summary>
        /// Undoes commands until the history is not dirty, or there are no more
        /// commands</summary>
        public void Revert()
        {
            while (Dirty && CanUndo)
                Undo();
        }

        /// <summary>
        /// Returns array of all "done" commands and clears the history</summary>
        /// <returns>Array of all done commands</returns>
        public Command[] Collapse()
        {
            // remove any un-done commands
            ClearUndoneCommands();
            Command[] result = new Command[m_commands.Count];
            m_commands.CopyTo(result);
            Clear();
            return result;
        }

        private void ClearUndoneCommands()
        {
            int i = m_commandCount.Current - 1;
            int numUndone = m_commands.Count - (i + 1);
            if (numUndone > 0)
                m_commands.RemoveRange(i + 1, numUndone);
        }

        private void commandCount_DirtyChanged(object sender, EventArgs e)
        {
            DirtyChanged.Raise(this, e);
        }

        private void OnCommandDone()
        {
            CommandDone.Raise(this, EventArgs.Empty);
        }

        private void OnCommandUndone()
        {
            CommandUndone.Raise(this, EventArgs.Empty);
        }

        IEnumerable<Command> Commands
        {
            get
            {
                return m_commands;
            }
        }
        private readonly List<Command> m_commands;
        private readonly CommandCount m_commandCount;                
    }
}
