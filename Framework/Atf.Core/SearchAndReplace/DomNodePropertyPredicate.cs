//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Linq-query-based searching over DomNode properties</summary>
    public class DomNodePropertyPredicate : LinqQueryPredicate
    {
        /// <summary>
        /// Constructor</summary>
        public DomNodePropertyPredicate()
        {
            m_domNode = null;

            // 'queryableData' is the parameter in the lambda expression on which all queries
            // will be made.  It will be referenced everywhere in the Expression tree, and each 
            // reference (some of which are retrieved from method calls) must be the same 
            // parameter expression instance.
            m_queryableData = Expression.Parameter(typeof(DomNodePropertyMatch), "queryableData");
        }

        #region Linq Query Expresion Creators

        /// <summary>
        /// Creates and gets an expression to reference the 'Name' of the queryable data</summary>
        public MemberExpression QueryableName
        {
            get { return Expression.PropertyOrField(m_queryableData, "Name"); }
        }

        /// <summary>
        /// Add to the lambda expression a query on the name of a property</summary>
        /// <param name="matchString">Property name to find</param>
        public void AddPropertyNameExpression(string matchString)
        {
            MethodCallExpression stringInPropertyName = GetStringIndexOfExpression(QueryableName, matchString);
            AddExpression(
                Expression.OrElse(
                    GetNullOrEmptyExpression(matchString),
                    Expression.NotEqual(stringInPropertyName, Expression.Constant(-1))));
        }

        /// <summary>
        /// Add to the lambda expression a string match test on the queryable name</summary>
        /// <param name="matchString">String to match</param>
        /// <param name="searchType">Regular expression string search type</param>
        /// <param name="isReplacePattern">Whether or not this match should be used in a subsequent replace operation</param>
        public void AddNameStringSearchExpression(string matchString, UInt64 searchType, bool isReplacePattern)
        {
            AddStringSearchExpression(QueryableName, matchString, searchType, isReplacePattern);
        }

        /// <summary>
        /// Gets the DomNode instance on whose properties we will make a Linq query.
        /// This member variable is only asssigned and used in the GetQueryableElements method. So for all intents &amp; purposes, 
        /// it *should* really be a local variable in that method. But DomNode is referenced in the lambda expression
        /// created in that method. There is no way to reference a local variable in an Expression without 
        /// it being treated as a constant. So the member variable allows us to abstractly reference "the current DomNode"
        /// being queried by the lambda expression</summary>
        protected DomNode DomNode { get { return m_domNode; } }
        private DomNode m_domNode;

        #endregion

        /// <summary>
        /// Instantiates an object implementing IQueryMatch, which tracks the match of a single item from a Linq query</summary>
        /// <param name="searchItem">The item containing the query match</param>
        /// <param name="queryMatch">The object, owned by searchItem, that matched in the query</param>
        /// <returns>The object implementing IQueryMatch</returns>
        public override IQueryMatch CreatePredicateMatch(object searchItem, object queryMatch)
        {
            // argument queryMatch is of type DomNodePropertyMatch, which implements IQueryMatch.  So just return it.
            return (DomNodePropertyMatch)queryMatch;
        }

        /// <summary>
        /// Produces an object that, given the specified object, can enumerate over queryable elements owned by that object.
        /// For this implementation, the specified object must be a DomNode.</summary>
        /// <param name="item">The object containing the queryable elements</param>
        /// <returns>An object that implements IQueryable on elements of the specified object</returns>
        protected override IQueryable GetQueryableElements(object item)
        {
            IQueryable returnValue = null;
            DomNode domNode = item as DomNode;
            if (domNode != null)
            {
                // see summary of "DomNode" property for an explanation on why we don't simply use local variable domNode
                m_domNode = domNode;

                // Get dom node properties, encapsulate into PropertyDescriptorQueryable
                ICustomTypeDescriptor iCustomTypeDescriptor = m_domNode.GetAdapter(typeof(ICustomTypeDescriptor)) as ICustomTypeDescriptor;
                if (iCustomTypeDescriptor != null)
                {
                    PropertyDescriptorCollection domNodeProperties = iCustomTypeDescriptor.GetProperties();
                    returnValue = new PropertyDescriptorQueryable(domNodeProperties, m_domNode).AsQueryable();
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Wrapper class for IEnumerable&lt;PropertyDescriptor&gt;, exposing as an IEnumerable&lt;DomNodePropertyMatch&gt;</summary>
        private class PropertyDescriptorQueryable : IEnumerable<DomNodePropertyMatch>
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="propertyDescriptorCollection">An enumeration of PropertyDescriptors</param>
            /// <param name="domNode">The DomNode instance, whose properties specifically will be queried</param>
            public PropertyDescriptorQueryable(PropertyDescriptorCollection propertyDescriptorCollection, DomNode domNode)
            {
                m_propertyDescriptorCollection = propertyDescriptorCollection;
                m_domNode = domNode;
            }

            #region IEnumerable<DomNodePropertyMatch> elements

            /// <summary>
            /// Implementation of IEnumerable</summary>
            /// <returns>Enumerator for DomNodePropertyMatch instances</returns>
            public IEnumerator<DomNodePropertyMatch> GetEnumerator()
            {
                if (m_propertyDescriptorCollection != null)
                {
                    foreach (PropertyDescriptor pd in m_propertyDescriptorCollection)
                        yield return new DomNodePropertyMatch(pd, m_domNode);
                }
            }

            /// <summary>
            /// Implementation of IEnumerable&lt;DomNodePropertyMatch&gt;</summary>
            /// <returns>IEnumerator for DomNodePropertyMatch instances</returns>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

            #endregion

            private readonly PropertyDescriptorCollection m_propertyDescriptorCollection;
            private readonly DomNode m_domNode;
        }
    }
}
