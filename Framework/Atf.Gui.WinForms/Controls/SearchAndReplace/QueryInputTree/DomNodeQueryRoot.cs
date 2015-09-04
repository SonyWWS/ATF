//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Applications;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Class for specifying the root of a query tree for searches on DOM nodes</summary>
    public class DomNodeQueryRoot : QueryRoot
    {
        /// <summary>
        /// Parses query tree to build list of predicates, with which the search is made</summary>
        /// <returns>Search predicates</returns>
        public override IQueryPredicate GetPredicate()
        {
            DomNodePropertyPredicate predicate = new DomNodePropertyPredicate();
            BuildPredicate(predicate);
            return predicate;
        }
    }

    /// <summary>
    /// DomNode-specific extension methods for QueryNode and core inheriting classes</summary>
    public static class QueryTreeDomNode
    {
#pragma warning disable 1587 // XML comment is not placed on a valid language element.
        /// <summary>
        /// Adds a QueryTree allowing user to search by DomNode name</summary>
        /// <param name="parentNode">Node to receive child</param>
        /// <param name="isReplacePattern">Whether or not this match should be used in a subsequent replace operation</param>
        /// <returns>The root of the new DomNode name query subtree</returns>
        public static QueryNode AddDomNodeNameQuery(this QueryNode parentNode, bool isReplacePattern)
        {
            return new QueryDomNodeName(parentNode, StringQuery.All, isReplacePattern);
        }

        /// <summary>
        /// Adds a QueryTree allowing user to search by DomNode property name and value (both string and numerical searches)</summary>
        /// <param name="parentNode">Node to receive child</param>
        /// <returns>The root of the new DomNode property query subtree</returns>
        public static QueryNode AddDomNodePropertyQuery(this QueryNode parentNode)
        {
            return new QueryDomNodeProperty(parentNode);
        }
#pragma warning restore 1587 // XML comment is not placed on a valid language element.
    }

    /// <summary>
    /// Enumerates the types of element properties that can be searched for in DomNode properties</summary>
    public static class DomNodeQuery
    {
        /// <summary>
        /// Enum for target of property search</summary>
        public enum PropertySearchTarget
        {
            /// <summary>
            /// Search on property names</summary>
            Name=0,
            /// <summary>
            /// Search on property values</summary>
            Value
        }
    }

    /// <summary>
    /// QueryStringInput tree specifically for searching DomNode property names OR property values, as strings</summary>
    public class QueryStringPropertyInput : QueryStringInput
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="parentNode">Node to receive child</param>
        /// <param name="stringQueryOptions">Bitfield defining which string search types can be selected</param>
        /// <param name="isReplacePattern">Whether or not this match should be used in a subsequent replace operation</param>
        public QueryStringPropertyInput(QueryNode parentNode, StringQuery stringQueryOptions, bool isReplacePattern) :
            base(parentNode, stringQueryOptions) 
        {
            m_isReplacePattern = isReplacePattern;
        }

        /// <summary>
        /// Adds a search for DomNode property names or values with a matching string</summary>
        /// <param name="predicate">Predicate to which the property search expression is added</param>
        /// <param name="target">Whether search is on DomNode property names or property values</param>
        /// <exception cref="InvalidOperationException">Unhandled property search target type</exception>
        protected void BuildStringPredicate(DomNodePropertyPredicate predicate, DomNodeQuery.PropertySearchTarget target)
        {
            switch(target)
            {
                case DomNodeQuery.PropertySearchTarget.Name:
                    predicate.AddNameStringSearchExpression(TextInput, SelectedItem.Tag, m_isReplacePattern);
                    break;

                case DomNodeQuery.PropertySearchTarget.Value:
                    predicate.AddValueStringSearchExpression(TextInput, SelectedItem.Tag, m_isReplacePattern);
                    break;
                    
                default:
                    throw new InvalidOperationException("Unhandled property search target type");
            }
        }

        private readonly bool m_isReplacePattern;
    }

    /// <summary>
    /// QueryStringInput tree specifically for searching DomNode property names</summary>
    public class QueryPropertyNameInput : QueryStringPropertyInput
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="parentNode">Parent node to receive child</param>
        /// <param name="stringQueryOptions">Bitfield defining which string search types can be selected</param>
        /// <param name="isReplacePattern">Whether or not this match should be used in a subsequent replace operation</param>
        public QueryPropertyNameInput(QueryNode parentNode, StringQuery stringQueryOptions, bool isReplacePattern) :
            base(parentNode, stringQueryOptions, isReplacePattern) { }

        /// <summary>
        /// Adds a search for DomNode property names or values with a matching string</summary>
        /// <param name="predicate">Predicate to which the property search expression is added</param>
        /// <exception cref="ArgumentException">DomNode-specific query tree has been passed an unhandled type of predicate info</exception>
        public override void BuildPredicate(IQueryPredicate predicate)
        {
            DomNodePropertyPredicate domNodePropertyPredicate = (DomNodePropertyPredicate)predicate;
            if (domNodePropertyPredicate == null)
                throw new ArgumentException("DomNode-specific query tree has been passed an unhandled type of predicate info");

            BuildStringPredicate(domNodePropertyPredicate, DomNodeQuery.PropertySearchTarget.Name);
        }
    }

    /// <summary>
    /// QueryStringInput tree specifically for searching DomNode property values as strings</summary>
    public class QueryPropertyValueAsStringInput : QueryStringPropertyInput
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="parentNode">Node to receive child</param>
        /// <param name="stringQueryOptions">Bitfield defining which string search types can be selected</param>
        /// <param name="isReplacePattern">Whether or not this match should be used in a subsequent replace operation</param>
        public QueryPropertyValueAsStringInput(QueryNode parentNode, StringQuery stringQueryOptions, bool isReplacePattern) :
            base(parentNode, stringQueryOptions, isReplacePattern) { }

        /// <summary>
        /// Adds a search for DomNode property values with a matching string</summary>
        /// <param name="predicate">Predicate to which the property search expression is added</param>
        /// <exception cref="ArgumentException">DomNode-specific query tree has been passed an unhandled type of predicate info</exception>
        public override void BuildPredicate(IQueryPredicate predicate)
        {
            DomNodePropertyPredicate domNodePropertyPredicate = (DomNodePropertyPredicate)predicate;
            if (domNodePropertyPredicate == null)
                throw new ArgumentException("DomNode-specific query tree has been passed an unhandled type of predicate info");

            BuildStringPredicate(domNodePropertyPredicate, DomNodeQuery.PropertySearchTarget.Value);
        }
    }

    /// <summary>
    /// QueryStringInput tree specifically for searching DomNode property values as numbers</summary>
    public class QueryPropertyValueAsNumberInput : QueryNumericalInput
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="parentNode">Parent node to receive child</param>
        /// <param name="numericalQueryOptions">Bitfield defining which numerical search types can be selected</param>
        /// <param name="isReplacePattern">Whether or not this match should be used in a subsequent replace operation</param>
        public QueryPropertyValueAsNumberInput(QueryNode parentNode, NumericalQuery numericalQueryOptions, bool isReplacePattern) :
            base(parentNode, numericalQueryOptions) { m_isReplacePattern = isReplacePattern; }

        /// <summary>
        /// Adds a search for DomNode property values with a matching number</summary>
        /// <param name="predicate">Predicate to which the property search expression is added</param>
        /// <exception cref="ArgumentException">DomNode-specific query tree has been passed an unhandled type of predicate info</exception>
        public override void BuildPredicate(IQueryPredicate predicate)
        {
            // No predicate added if the text input can't be parsed as a number
            Double num;
            if (Double.TryParse(TextInput1, out num) == false)
                return;

            DomNodePropertyPredicate domNodePredicate = (DomNodePropertyPredicate)predicate;
            if (domNodePredicate == null)
                throw new ArgumentException("DomNode-specific query tree has been passed an unhandled type of predicate info");

            // Add appropriate search expression to predicate
            switch (SelectedItem.Tag)
            {
                case (UInt64)NumericalQuery.Equals:
                    domNodePredicate.AddNumberValueEqualsExpression(num, m_isReplacePattern);
                    break;

                case (UInt64)NumericalQuery.Lesser:
                    domNodePredicate.AddNumberValueLesserExpression(num, m_isReplacePattern);
                    break;

                case (UInt64)NumericalQuery.LesserEqual:
                    domNodePredicate.AddNumberValueLesserEqualExpression(num, m_isReplacePattern);
                    break;

                case (UInt64)NumericalQuery.GreaterEqual:
                    domNodePredicate.AddNumberValueGreaterEqualExpression(num, m_isReplacePattern);
                    break;

                case (UInt64)NumericalQuery.Greater:
                    domNodePredicate.AddNumberValueGreaterExpression(num, m_isReplacePattern);
                    break;

                case (UInt64)NumericalQuery.Between:
                    {
                        Double num2;
                        if (Double.TryParse(TextInput2, out num2))
                            domNodePredicate.AddNumberValueBetweenExpression(num, num2, m_isReplacePattern);
                        break;
                    }

                default:
                    // throw exception...?
                    break;
            }
        }

        readonly bool m_isReplacePattern;
    }

    /// <summary>
    /// QueryStringInput tree specifically for searching DomNode names</summary>
    public class QueryDomNodeName : QueryPropertyValueAsStringInput
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="parentNode">Parent node to receive child</param>
        /// <param name="stringQueryOptions">Bitfield defining which string search types can be selected</param>
        /// <param name="isReplacePattern">Whether or not this match should be used in a subsequent replace operation</param>
        public QueryDomNodeName(QueryNode parentNode, StringQuery stringQueryOptions, bool isReplacePattern) :
            base(parentNode, stringQueryOptions, isReplacePattern) { }

        /// <summary>
        /// Adds a search for DomNode properties with name "Name", and with a matching string value</summary>
        /// <param name="predicate">Predicate to which the property search expression is added</param>
        /// <exception cref="ArgumentException">DomNode-specific query tree has been passed an unhandled type of predicate info</exception>
        public override void BuildPredicate(IQueryPredicate predicate)
        {
            DomNodePropertyPredicate domNodePredicate = (DomNodePropertyPredicate)predicate;
            if (domNodePredicate == null)
                throw new ArgumentException("DomNode-specific query tree has been passed an unhandled type of predicate info");

            domNodePredicate.AddPropertyNameExpression("Name");
            base.BuildPredicate(domNodePredicate);
        }
    }

    /// <summary>
    /// QueryStringInput tree specifically for searching DomNode properties, using search patterns for property name and value</summary>
    public class QueryDomNodeProperty : QueryNode
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="parentNode">Node to receive child</param>
        public QueryDomNodeProperty(QueryNode parentNode)
        {
            parentNode.Add(this);

            this.AddLabel("whose name");
            new QueryPropertyNameInput(this, StringQuery.Matches, false);
            this.AddLabel("and whose");
            QueryOption stringOrNumberOption = this.AddOption();
            new QueryPropertyValueAsStringInput(stringOrNumberOption.AddOptionItem("string value", 0), StringQuery.All, true);
            new QueryPropertyValueAsNumberInput(stringOrNumberOption.AddOptionItem("numerical value", 0), NumericalQuery.All, true);
        }
    }
}
