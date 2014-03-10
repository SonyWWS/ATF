//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// DomNodeAdapter enabling DomNodes to be searched, to supply the search results, and to have those results
    /// replaced with other data</summary>
    public class DomNodeQueryable : DomNodeAdapter, IQueryableContext, IQueryableResultContext, IQueryableReplaceContext
    {
        #region IQueryableContext members
        /// <summary>
        /// Compiles an enumeration of DomNode properties (as objects) that matched the conditions of the search predicates</summary>
        /// <param name="predicate">Specifies the test conditions for a query</param>
        /// <returns>The enumeration of DomNode properties (e.g., DomNodeQueryMatch) satisfying the query</returns>
        public IEnumerable<object> Query(IQueryPredicate predicate)
        {
            m_results.Clear();
            // Iterate over all dom nodes under this adapter
            foreach (DomNode domNode in DomNode.Subtree)
            {
                // The results of one DomNode query associate each predicate with matching dom node properties
                Dictionary<IQueryPredicate, IList<IQueryMatch>> predicateMatchResults 
                    = new Dictionary<IQueryPredicate, IList<IQueryMatch>>();

                // For each queryable item (ie a DomNode) there may be 0 to many "query matches" 
                // (ie a DomNode property).  On success, predicate.Test() will supply one 
                // IQueryMatch per DomNode property that matched.
                IList<IQueryMatch> matchingPropertiesList;
                if (predicate.Test(domNode, out matchingPropertiesList))
                {
                    if (matchingPropertiesList != null)
                        predicateMatchResults[predicate] = matchingPropertiesList;

                    // For this queryable, a match is the DomNode that passed the predicate test,
                    // paired with all its properties that allowed it to satisfy the test
                    m_results.Add(new DomNodeQueryMatch(domNode, predicateMatchResults));
                }
            }

            // Announce that the search results have changed
            ResultsChanged.Raise(this, EventArgs.Empty);

            return m_results;
        }
        #endregion

        #region IQueryableResultContext members
        /// <summary>
        /// Event that is raised after the Results property has changed</summary>
        public event EventHandler ResultsChanged;

        /// <summary>
        /// Gets the list of found objects on the most recent Query() execution</summary>
        public IEnumerable<object> Results { get { return m_results; } }
        #endregion

        #region IQueryableReplaceContext members
        /// <summary>
        /// Applies a replacement on the results of the last Query</summary>
        /// <returns>The list of objects on which we just performed a replacement</returns>
        public IEnumerable<object> Replace(object replaceInfo)
        {
            ITransactionContext currentTransaction = null;
            try
            {
                foreach (DomNodeQueryMatch match in m_results)
                {
                    DomNode domNode = match.DomNode;

                    // Set up undo/redo for the replacement operation
                    ITransactionContext newTransaction = domNode != null ? domNode.GetRoot().As<ITransactionContext>() : null;
                    if (newTransaction != currentTransaction)
                    {
                        {
                            if (currentTransaction != null)
                                currentTransaction.End();
                            currentTransaction = newTransaction;
                            if (currentTransaction != null)
                                currentTransaction.Begin("Replace".Localize());
                        }
                    }

                    // Apply replacement to all matching items that were found on last search
                    foreach (IQueryPredicate predicateInfo in match.PredicateMatchResults.Keys)
                        predicateInfo.Replace(match.PredicateMatchResults[predicateInfo], replaceInfo);
                }
            }
            catch (InvalidTransactionException ex)
            {
                // cancel the replacement transaction in the undo/redo queue
                if (currentTransaction != null && currentTransaction.InTransaction)
                    currentTransaction.Cancel();

                if (ex.ReportError)
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
            }
            finally
            {
                // finish the replacement transaction for the undo/redo queue
                if (currentTransaction != null && currentTransaction.InTransaction)
                    currentTransaction.End();
            }

            ResultsChanged.Raise(this, EventArgs.Empty);
            
            return m_results;
        }
        #endregion

        private readonly List<object> m_results =  new List<object>();
    }
}
