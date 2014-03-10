//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Dom;

namespace SimpleDomEditorSample
{
    /// <summary>
    /// DomNodeAdapter enabling DomNodes to be searched</summary>
    public class DomNodeQueryable : DomNodeAdapter, IQueryableContext
    {
        /// <summary>
        /// Event that is raised after the query results change</summary>
        public event EventHandler QueryResultsChanged;

        /// <summary>
        /// Gets an enumeration of strings that represent every domain under which a query can be made.
        /// By convention, it is recommended that you define "all".</summary>
        public IEnumerable<string> Domains
        {
            get
            {
                // There is no division of domains as it applies to searching for properties in a DomNode subtree
                yield break;
            }
        }

        /// <summary>
        /// Gets a list of found objects on the most recent Query() execution</summary>
        public IEnumerable<object> Results
        {
            get
            {
                return m_results;
            }
        }

        /// <summary>
        /// Compiles an enumeration of objects that satisfy the conditions of the search predicates</summary>
        /// <param name="domains">Enumeration of zero or more strings from the set defined by DomNodeQueryable.Domains, which specify the domains in which the query should be made</param>
        /// <param name="predicates">Enumeration of zero or more predicates, which make up all search conditions for the query</param>
        /// <returns>The list of objects satisfying the query</returns>
        public IEnumerable<object> Query(IEnumerable<string> domains, IEnumerable<IPredicateInfo> predicates)
        {
            m_results.Clear();
            // For DomNodeQueryable, there is only one domain, which is entirely under the DomNode managed by this adapter
            foreach (DomNode domNode in DomNode.Subtree)
            {
                bool failed = false;
                Dictionary<IPredicateInfo, object> predicateMatchResults = new Dictionary<IPredicateInfo, object>();

                // The query succeeds for a candidate DomNode if all specified predicates succeed
                foreach (IPredicateInfo predicateInfo in predicates)
                {
                    object matchingPropertiesList;
                    if (predicateInfo.Test(domNode, out matchingPropertiesList) == false)
                    {
                        failed = true;
                        break;
                    }
                    if (matchingPropertiesList != null)
                        predicateMatchResults[predicateInfo] = matchingPropertiesList;
                }
                if (!failed)
                    m_results.Add(new DomNodeQueryMatch(domNode, predicateMatchResults));
            }

            Sce.Atf.Event.Raise(QueryResultsChanged, this, EventArgs.Empty);

            return m_results;
        }

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
                    ITransactionContext newTransaction = domNode != null ? domNode.GetRoot().As<ITransactionContext>() : null;

                    if (newTransaction != currentTransaction)
                    {
                        {
                            if (currentTransaction != null)
                                currentTransaction.End();
                            currentTransaction = newTransaction;
                            if (currentTransaction != null)
                                currentTransaction.Begin(Localizer.Localize("Replace"));
                        }
                    }
                    // Apply replacement to all match items that predicates came up with on last search
                    foreach (IPredicateInfo predicateInfo in match.PredicateMatchResults.Keys)
                        predicateInfo.Replace(domNode, match.PredicateMatchResults[predicateInfo], replaceInfo);
                }
            }
            catch (InvalidTransactionException ex)
            {
                if (currentTransaction != null && currentTransaction.InTransaction)
                    currentTransaction.Cancel();

                if (ex.ReportError)
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
            }
            finally
            {
                if (currentTransaction != null && currentTransaction.InTransaction)
                    currentTransaction.End();
            }

            Sce.Atf.Event.Raise(QueryResultsChanged, this, EventArgs.Empty);
            
            return (IEnumerable<Object>)m_results;
        }

        /// <summary>
        /// Search results list</summary>
        List<object> m_results =  new List<object>();
    }
}
