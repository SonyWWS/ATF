//Sony Computer Entertainment Confidential

using System.Collections.Generic;

namespace Sce.Atf.Applications
{
	/// <summary>
	/// Command to remove an element from an IList</summary>
    /// <typeparam name="T">List element type</typeparam>
	public class ListRemoveCommand<T> : Command
	{
		/// <summary>
		/// Constructor</summary>
		/// <param name="commandName">Name of command</param>
		/// <param name="list">Target list</param>
		/// <param name="element">Element to remove</param>
		/// <param name="index">Position of removal</param>
        public ListRemoveCommand(string commandName, IList<T> list, T element, int index)
			: base(commandName)
		{
			m_list = list;
			m_element = element;
			m_index = index;
		}

		/// <summary>
        /// Does / Redoes the command  </summary>
		public override void Do()
		{
			m_list.RemoveAt(m_index);
		}

		/// <summary>
        /// Undoes the command </summary>
		public override void Undo()
		{
			m_list.Insert(m_index, m_element);
		}

        private IList<T> m_list;
        private T m_element;
		private int m_index;
	}
}
