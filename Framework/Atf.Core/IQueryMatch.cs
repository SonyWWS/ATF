//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
	/// <summary>
	/// Interface to the actual data that matched a query</summary>
	public interface IQueryMatch
	{
		/// <summary>
		/// Get the value of the matching data</summary>
		object GetValue();

		/// <summary>
		/// Replace the value of the matching data</summary>
		void SetValue(object value);
	}
}
