//Sony Computer Entertainment Confidential

using System.Collections.Generic;

namespace Sce.Atf.Applications
{
	/// <summary>
	/// Command to change an element in an IList</summary>
    /// <typeparam name="T">List element type</typeparam>
	public class ListChangeCommand<T> : Command
	{
		/// <summary>
		/// Constructor</summary>
		/// <param name="commandName">Name of command</param>
		/// <param name="list">Target list</param>
		/// <param name="oldElement">Old element</param>
		/// <param name="newElement">New element</param>
		/// <param name="index">Position of insertion</param>
		public ListChangeCommand(
            string commandName,
            IList<T> list, 
            T oldElement,
            T newElement,
            int index)

			: base(commandName)
		{
			m_list = list;
			m_oldElement = oldElement;
			m_newElement = newElement;
			m_index = index;
		}

		/// <summary>
		/// Does / Redoes the command  </summary>
		public override void Do()
		{
			m_list[m_index] = m_newElement;
		}

		/// <summary>
		/// Undoes the command </summary>
		public override void Undo()
		{
			m_list[m_index] = m_oldElement;
		}

        private IList<T> m_list;
		private T m_oldElement;
		private T m_newElement;
		private int m_index;
	}
}

