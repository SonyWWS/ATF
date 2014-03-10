//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;

using Sce.Atf;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Encapsulate the matching properties of a given DomNode instance.</summary>
    public class DomNodeQueryMatch
    {
        /// <summary>
        /// Constructor (private to prevent default construction).</summary>
        private DomNodeQueryMatch() { }

        /// <summary>
        /// Constructor</summary>
        /// <param name="domNode">The matching DomNode</param>
        /// <param name="predicateMatchResults">The list of matching PropertyDescriptors</param>
		public DomNodeQueryMatch(DomNode domNode, Dictionary<IQueryPredicate, IList<IQueryMatch>> predicateMatchResults)
        {
            m_domNode = domNode;
            m_predicateMatchResults = predicateMatchResults;
        }

        /// <summary>
        /// Gets the DomNode object. No setter.
        /// </summary>
        public DomNode DomNode
        {
            get { return m_domNode; }
        }
		private DomNode m_domNode;

        /// <summary>
        /// Gets a dictionary describing the match results. No setter.
        /// </summary>
        public Dictionary<IQueryPredicate, IList<IQueryMatch>> PredicateMatchResults
        {
            get { return m_predicateMatchResults; }
        }
		private Dictionary<IQueryPredicate, IList<IQueryMatch>> m_predicateMatchResults;
    }
}
