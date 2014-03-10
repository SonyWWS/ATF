//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for context with history</summary>
    public interface IHistoryContext
    {
        /// <summary>
        /// Gets whether there is a command that can be undone</summary>
        bool CanUndo
        {
            get;
        }

        /// <summary>
        /// Gets whether there is a command that can be redone</summary>
        bool CanRedo
        {
            get;
        }

        /// <summary>
        /// Gets the description of the next undoable command</summary>
        string UndoDescription
        {
            get;
        }

        /// <summary>
        /// Gets the name of the next redoable command</summary>
        string RedoDescription
        {
            get;
        }

        /// <summary>
        /// Undoes the last "done" command</summary>
        void Undo();

        /// <summary>
        /// Redoes the last "undone" command</summary>
        void Redo();

        /// <summary>
        /// Gets and sets a value indicating whether the command history is at its "clean" point</summary>
        /// <remarks>Setting this property to false sets the "clean" point. Setting
        /// it to true forces a "dirty" state until the next time Dirty is set to false.</remarks>
        bool Dirty
        {
            get;
            set;
        }

        /// <summary>
        /// Event that is raised when the Dirty state changes</summary>
        event EventHandler DirtyChanged;
    }
}
