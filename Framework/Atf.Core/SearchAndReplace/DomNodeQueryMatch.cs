//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Encapsulates the matching properties of a given DomNode instance</summary>
    public class DomNodeQueryMatch
    {
        /// <summary>
        /// Constructor (private to prevent default construction)</summary>
        private DomNodeQueryMatch() { }

        /// <summary>
        /// Constructor</summary>
        /// <param name="domNode">Matching domNode</param>
        /// <param name="predicateMatchResults">List of matching PropertyDescriptors</param>
        public DomNodeQueryMatch(DomNode domNode, Dictionary<IQueryPredicate, IList<IQueryMatch>> predicateMatchResults)
        {
            m_domNode = domNode;
            m_predicateMatchResults = predicateMatchResults;
        }

        /// <summary>
        /// Gets DomNode</summary>
        public DomNode DomNode
        {
            get { return m_domNode; }
        }
        private readonly DomNode m_domNode;

        /// <summary>
        /// Gets dictionary of predicate match results</summary>
        public Dictionary<IQueryPredicate, IList<IQueryMatch>> PredicateMatchResults
        {
            get { return m_predicateMatchResults; }
        }
        private readonly Dictionary<IQueryPredicate, IList<IQueryMatch>> m_predicateMatchResults;
    }
}
