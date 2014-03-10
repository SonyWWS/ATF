//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Abstract base class for Commands, which encapsulate some undoable/redoable action</summary>
    public abstract class Command
    {
        /// <summary>
        /// Constructor</summary>
        public Command()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Constructor with description</summary>
        /// <param name="description">Command description that appears to the user</param>
        public Command(string description)
        {
            m_description = description;
        }

        /// <summary>
        /// Gets or sets description of command</summary>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        /// <summary>
        /// Do or Redo the command</summary>
        public abstract void Do();

        /// <summary>
        /// Undo the command</summary>
        public abstract void Undo();

        private string m_description;
    }
}

