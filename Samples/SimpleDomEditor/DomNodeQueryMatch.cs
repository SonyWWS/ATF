//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Dom;

namespace SimpleDomEditorSample
{
    /// <summary>
    /// Encapsulates the conditions for which a DomNode was a query match</summary>
    public class DomNodeQueryMatch
    {
        /// <summary>
        /// Constructor (private to prevent default construction)</summary>
        private DomNodeQueryMatch() { }

        /// <summary>
        /// Constructor</summary>
        /// <param name="domNode">The matching DomNode</param>
        /// <param name="predicateResultList">Dictionary of matching PropertyDescriptors as predicate/matching object</param>
        public DomNodeQueryMatch(DomNode domNode, Dictionary<IPredicateInfo, object> predicateMatchResults)
        {
            m_domNode = domNode;
            m_predicateMatchResults = predicateMatchResults;
        }

        /// <summary>
        /// Gets the matching DomNode</summary>
        public DomNode DomNode
        {
            get { return m_domNode; }
        }

        /// <summary>
        /// Gets the Dictionary of matching PropertyDescriptors as predicate/matching object</summary>
        public Dictionary<IPredicateInfo, object> PredicateMatchResults
        {
            get { return m_predicateMatchResults; }
        }

        private DomNode m_domNode;
        private Dictionary<IPredicateInfo, object> m_predicateMatchResults;
    }
}
