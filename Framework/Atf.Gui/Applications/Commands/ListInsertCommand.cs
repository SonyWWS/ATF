//Sony Computer Entertainment Confidential

using System.Collections.Generic;

namespace Sce.Atf.Applications
{
	/// <summary>
	/// Command to insert an element into an IList</summary>
    /// <typeparam name="T">List element type</typeparam>
	public class ListInsertCommand<T> : Command
	{
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandName">Command name</param>
        /// <param name="list">Target list</param>
        /// <param name="element">Inserted element</param>
        /// <param name="index">Index of insertion</param>
        public ListInsertCommand(string commandName, IList<T> list, T element, int index)
			: base(commandName)
		{
			m_list = list;
			m_element = element;
			m_index = index;
		}

        /// <summary>
        /// Does / redoes the command</summary>
        public override void Do()
		{
			m_list.Insert(m_index, m_element);
		}

        /// <summary>
        /// Undoes the command</summary>
        public override void Undo()
		{
			m_list.RemoveAt(m_index);
		}

		private IList<T> m_list;
        private T m_element;
		private int m_index;
	}
}
