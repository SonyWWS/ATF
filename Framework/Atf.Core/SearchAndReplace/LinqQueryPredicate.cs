//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Sce.Atf
{
    /// <summary>
    /// Base class for creating search query predicates using Linq expressions</summary>
    public abstract class LinqQueryPredicate : IQueryPredicate
    {
        /// <summary>
        /// Constructor</summary>
        public LinqQueryPredicate()
        {
            m_lambdaExpression = null;
            m_expressionList = new List<Expression>();

            // deriving classes MUST initialize this with some call to Expression.Parameter( <yourtype>, "property"),
            // where <yourtype> is a type implementing IQueryMatch, over which the lambda expression will iterate.
            m_queryableData = null;    
        }

        #region IQueryPredicate elements
        /// <summary>
        /// Tests the predicate on an item</summary>
        /// <param name="searchItem">Item in which searchable elements should be queried</param>
        /// <param name="matchList">Resulting list of matching elements from within searchItem</param>
        /// <returns>True iff the item matches the predicate</returns>
        public bool Test(object searchItem, out IList<IQueryMatch> matchList)
        {
            matchList = null;
            IQueryable query = GetQueryable(searchItem);

            if (query != null)
            {
                // execute the query against domNode's Properties - foreach statement will iterate for each match
                foreach (object queryMatch in query)
                {
                    // The tests below are NOT the match test...each queryMatch *is* a bona-fide match.
                    // Rather, the if statement below confirms a valid 'match pattern' was created, which 
                    // can be used later to perform a replace operation.
                    IQueryMatch candidate = CreatePredicateMatch(searchItem, queryMatch);
                    if (MatchPattern == null || MatchPattern.Matches(candidate))
                    {
                        if (matchList == null)
                            matchList = new List<IQueryMatch>();
                        matchList.Add(candidate);
                    }
                }
            }

            // success based on whether a match list was created or not
            return matchList != null;
        }

        /// <summary>
        /// Replaces the matched elements of item for the specified value</summary>
        /// <param name="matchList">List of elements that matched a query</param>
        /// <param name="replaceValue">Object that designates which data is replaced in each matchList entry</param>
        public void Replace(IList<IQueryMatch> matchList, object replaceValue)
        {
            foreach (IQueryMatch matchCandidate in matchList)
            {
                MatchPattern.Replace(matchCandidate, replaceValue);
            }
        }
        #endregion

        /// <summary>
        /// Method, to be implemented by derived classes, that creates an IQueryable object 
        /// that enumerates through the SEARCHABLE elements within the specified object</summary>
        /// <param name="queryItem">Item in which searchable elements should be enumerated</param>
        /// <returns>IQueryable object that enumerates through SEARCHABLE elements within specified object</returns>
        protected abstract IQueryable GetQueryableElements(object queryItem);

        /// <summary>
        /// Creates an IQueryable object from constructing a Linq query expression. Resulting
        /// IQueryable enumerates through the MATCHING elements of the specified query item.</summary>
        /// <param name="queryItem">Item in which elements are searched, from which the IQueryable object
        /// is produced</param>
        /// <returns>IQueryable object from constructing Linq query expression</returns>
        protected IQueryable GetQueryable(object queryItem)
        {
            IQueryable returnValue = null;
            IQueryable queryableElements = GetQueryableElements(queryItem);
            if (queryableElements != null)
            {
                // 'where' call applies lambda created in the constructor to objects in domNodeProperties
                MethodCallExpression linqExpression = 
                    Expression.Call(typeof(Queryable),
                                     "Where",
                                     new Type[] { queryableElements.ElementType },
                                     queryableElements.Expression,
                                     LambdaExpression);

                // When enumerated on, returnValue will produce only the elements of queryableProperties that 
                // match the search criteria of the LambdaExpression
                returnValue = queryableElements.Provider.CreateQuery(linqExpression);
            }

            return returnValue;
        }

        /// <summary>
        /// Method, to be implemented by derived classes, that returns an encapsulation of the 
        /// specified matching element from the specified search item</summary>
        /// <param name="searchItem">Item for which the matching item was found</param>
        /// <param name="queryMatch">The matching element, which came from searchItem</param>
        /// <returns>Encapsulation of specified matching element from specified search item</returns>
        public abstract IQueryMatch CreatePredicateMatch(object searchItem, object queryMatch);

        #region Linq Query Expression Creators

        /// <summary>
        /// Adds a Linq expression, which is used on the next query</summary>
        /// <param name="expression">Linq expression</param>
        protected void AddExpression(Expression expression)
        {
            m_expressionList.Add(expression);
        }
        private readonly List<Expression> m_expressionList;

        /// <summary>
        /// Creates and gets an expression to reference the value of the queryable data</summary>
        /// <value>Expression that resolves to the QueryMatch.GetValue() method call</value>
        public MethodCallExpression QueryableValue
        {
            get { return Expression.Call(m_queryableData, "GetValue", null, null); }
        }

        #region String-specific Linq query construction methods
        /// <summary>
        /// Creates and gets an expression to reference QueryableValue as a string</summary>
        /// <value>Expression that resolves to the string.ToString() method call</value>
        public MethodCallExpression QueryableValueString
        {
            get { return Expression.Call(QueryableValue, "ToString", null, null); }
        }

        /// <summary>
        /// Gets an expression to test whether a string is null or empty</summary>
        /// <param name="matchString">The string to validate</param>
        /// <returns>Expression to test whether string is null or empty</returns>
        protected BinaryExpression GetNullOrEmptyExpression(string matchString)
        {
            return Expression.Equal(
                Expression.Call(typeof(string), "IsNullOrEmpty", null, new Expression[] { Expression.Constant(matchString) }),
                Expression.Constant(true));
        }

        /// <summary>
        /// Creates an expression to produce a search within the specified string expression</summary>
        /// <param name="sourceStringExpression">Expression referencing the string to search</param>
        /// <param name="matchString">String to match within sourceString</param>
        /// <returns>Expression that resolves to the string.IndexOf() method call</returns>
        public MethodCallExpression GetStringIndexOfExpression(Expression sourceStringExpression, string matchString)
        {
            ConstantExpression ignoreCase = Expression.Constant(StringComparison.InvariantCultureIgnoreCase);
            return Expression.Call(
                sourceStringExpression, "IndexOf", null, new Expression[] { Expression.Constant(matchString), ignoreCase });
        }

        /// <summary>
        /// Creates an expression to produce a regular expression formatted string search</summary>
        /// <param name="matchString">String to match</param>
        /// <param name="searchType">Type of regular expression to format</param>
        /// <returns>Regular expression string to be used in a regular expression string search</returns>
        public string CreateRegularExpressionPattern(string matchString, UInt64 searchType)
        {
            string patternString = "";
            switch (searchType)
            {
                case (UInt64)StringQuery.Matches:
                    patternString = "^" + Regex.Escape(matchString) + "$";
                    break;

                case (UInt64)StringQuery.BeginsWith:
                    patternString = "^" + Regex.Escape(matchString);
                    break;

                case (UInt64)StringQuery.EndsWith:
                    patternString = Regex.Escape(matchString) + "$";
                    break;

                case (UInt64)StringQuery.RegularExpression:
                    {
                        bool patternValid = true;
                        // test that the regex pattern is valid by running a match, and checking for an exception
                        try
                        {
                            Regex.Match(String.Empty, matchString);
                        }
                        catch (ArgumentException)
                        {
                            patternValid = false;
                        }
                        patternString = (patternValid) ? matchString : Regex.Escape(matchString);
                    }
                    break;

                case (UInt64)StringQuery.Contains:
                default:
                    patternString = Regex.Escape(matchString);
                    break;
            }

            return patternString;
        }

        /// <summary>
        /// Adds to the lambda expression a string match test on the queryable value</summary>
        /// <param name="matchString">String to match</param>
        /// <param name="searchType">Regular expression string search type</param>
        /// <param name="isReplacePattern">Whether or not this match should be used in a subsequent replace operation</param>
        public void AddValueStringSearchExpression(string matchString, UInt64 searchType, bool isReplacePattern)
        {
            AddStringSearchExpression(QueryableValueString, matchString, searchType, isReplacePattern);
        }

        /// <summary>
        /// Adds to the lambda expression a string match test on either a property name string or a property value string</summary>
        /// <param name="sourceStringExp">Expression that evaluates to the string on which this search is applied</param>
        /// <param name="matchString">String to match</param>
        /// <param name="searchType">Regular expression string search type</param>
        /// <param name="isReplacePattern">Whether or not this match should be used in a subsequent replace operation</param>
        public void AddStringSearchExpression(Expression sourceStringExp, string matchString, UInt64 searchType, bool isReplacePattern)
        {
            string patternString = CreateRegularExpressionPattern(matchString, searchType);
            Expression patternStringExp = Expression.Constant(patternString);
            if (isReplacePattern)
                MatchPattern = new StringReplaceQueryPattern(patternString);

            Expression optionsExp = Expression.Constant(RegexOptions.IgnoreCase, typeof(RegexOptions));
            MethodCallExpression regexMatchTest = Expression.Call(typeof(Regex), "Match", null, new Expression[3] { sourceStringExp, patternStringExp, optionsExp });
            MemberExpression regexSuccess = Expression.Property(regexMatchTest, "Success");
            AddExpression(
                Expression.OrElse(
                    GetNullOrEmptyExpression(matchString),
                    Expression.Equal(regexSuccess, Expression.Constant(true))));
        }
        #endregion

        #region Number-specific Linq query construction methods
        /// <summary>
        /// Creates an expression that tests whether the queryable value can be converted to a double</summary>
        /// <returns>Expression that resolves to true or false</returns>
        public BinaryExpression GetValueIsConvertibleToDoubleExpression()
        {
            MethodCallExpression isConvertibleCall = Expression.Call(typeof(LinqQueryPredicate), "IsConvertibleToDouble", null,
                new Expression[] { QueryableValue });
            return Expression.Equal(isConvertibleCall, Expression.Constant(true));
        }

        /// <summary>
        /// Creates an expression that converts the queryable value to a double</summary>
        /// <returns>Expression that resolves to the queryable value as a double</returns>
        public MethodCallExpression GetConvertToDoubleExpression()
        {
            return Expression.Call(typeof(Convert), "ToDouble", null, new Expression[] { QueryableValue });
        }

        /// <summary>
        /// Adds to the lambda expression a numerical equality test with the queryable value</summary>
        /// <param name="patternNumber">Number with which to compare queryable value</param>
        /// <param name="isReplacePattern">Whether this match should be used for a subsequent replace operation</param>
        public void AddNumberValueEqualsExpression(Double patternNumber, bool isReplacePattern)
        {
            if (isReplacePattern)
                MatchPattern = new NumberReplaceQueryPattern(patternNumber);

            AddExpression(
                Expression.AndAlso(
                    GetValueIsConvertibleToDoubleExpression(),
                    Expression.Equal(GetConvertToDoubleExpression(), Expression.Constant(patternNumber))));
        }

        /// <summary>
        /// Adds to the lambda expression a numerical "less than" test with the queryable value</summary>
        /// <param name="patternNumber">Number with which to compare queryable value</param>
        /// <param name="isReplacePattern">Whether this match should be used for a subsequent replace operation</param>
        public void AddNumberValueLesserExpression(Double patternNumber, bool isReplacePattern)
        {
            if (isReplacePattern)
                MatchPattern = new NumberReplaceQueryPattern(patternNumber);

            AddExpression(
                Expression.AndAlso(
                    GetValueIsConvertibleToDoubleExpression(),
                    Expression.LessThan(GetConvertToDoubleExpression(), Expression.Constant(patternNumber))));
        }

        /// <summary>
        /// Adds to the lambda expression a numerical "less than or equal" test with the queryable value</summary>
        /// <param name="patternNumber">Number with which to compare queryable value</param>
        /// <param name="isReplacePattern">Whether this match should be used for a subsequent replace operation</param>
        public void AddNumberValueLesserEqualExpression(Double patternNumber, bool isReplacePattern)
        {
            if (isReplacePattern)
                MatchPattern = new NumberReplaceQueryPattern(patternNumber);

            AddExpression(
                Expression.AndAlso(
                    GetValueIsConvertibleToDoubleExpression(),
                    Expression.LessThanOrEqual(GetConvertToDoubleExpression(), Expression.Constant(patternNumber))));
        }

        /// <summary>
        /// Adds to the lambda expression a numerical "greater than or equal" test with the queryable value</summary>
        /// <param name="patternNumber">Number with which to compare queryable value</param>
        /// <param name="isReplacePattern">Whether this match should be used for a subsequent replace operation</param>
        public void AddNumberValueGreaterEqualExpression(Double patternNumber, bool isReplacePattern)
        {
            if (isReplacePattern)
                MatchPattern = new NumberReplaceQueryPattern(patternNumber);

            AddExpression(
                Expression.AndAlso(
                    GetValueIsConvertibleToDoubleExpression(),
                    Expression.GreaterThanOrEqual(GetConvertToDoubleExpression(), Expression.Constant(patternNumber))));
        }

        /// <summary>
        /// Adds to the lambda expression a numerical "greater than" test with the queryable value</summary>
        /// <param name="patternNumber">Number with which to compare queryable value</param>
        /// <param name="isReplacePattern">Whether this match should be used for a subsequent replace operation</param>
        public void AddNumberValueGreaterExpression(Double patternNumber, bool isReplacePattern)
        {
            if (isReplacePattern)
                MatchPattern = new NumberReplaceQueryPattern(patternNumber);

            AddExpression(
                Expression.AndAlso(
                    GetValueIsConvertibleToDoubleExpression(),
                    Expression.GreaterThan(GetConvertToDoubleExpression(), Expression.Constant(patternNumber))));
        }

        /// <summary>
        /// Adds to the lambda expression a numerical "within range" test with the queryable value.
        /// The range numbers don't need to be in numeric order.</summary>
        /// <param name="patternNumber1">Number with which to compare queryable value</param>
        /// <param name="patternNumber2">Number with which to compare queryable value</param>
        /// <param name="isReplacePattern">Whether this match should be used for a subsequent replace operation</param>
        public void AddNumberValueBetweenExpression(Double patternNumber1, Double patternNumber2, bool isReplacePattern)
        {
            if (isReplacePattern)
                MatchPattern = new NumberReplaceQueryPattern(patternNumber1, patternNumber2);

            // Create expression which tests:
            //
            // (PropertyValueConvertibleToDouble() == true) &&
            // (((num1 <= num2) && (num1 <= propertyValue) && (propertyValue <= num2)) || 
            //  ((num2 <= num1) && (num2 <= propertyValue) && (propertyValue <= num1)))
            //
            MethodCallExpression convertToDouble = GetConvertToDoubleExpression();
            ConstantExpression num1Exp = Expression.Constant(patternNumber1);
            ConstantExpression num2Exp = Expression.Constant(patternNumber2);
            AddExpression(
                Expression.AndAlso(
                    GetValueIsConvertibleToDoubleExpression(),
                    Expression.Or(
                        Expression.AndAlso(
                            Expression.LessThanOrEqual(num1Exp, num2Exp),
                            Expression.AndAlso(
                                Expression.LessThanOrEqual(num1Exp, convertToDouble),
                                Expression.LessThanOrEqual(convertToDouble, num2Exp))),
                        Expression.AndAlso(
                            Expression.LessThanOrEqual(num2Exp, num1Exp),
                            Expression.AndAlso(
                                Expression.LessThanOrEqual(num2Exp, convertToDouble),
                                Expression.LessThanOrEqual(convertToDouble, num1Exp))))));

        }

        /// <summary>
        /// Determines whether the specified object is convertible to a double float</summary>
        /// <param name="candidate">Object to test for double-ness</param>
        /// <returns>True iff object is convertible to double float</returns>
        public static bool IsConvertibleToDouble(object candidate)
        {
            if (candidate is string)
            {
                Double doubleResult;
                return Double.TryParse((string)candidate, NumberStyles.Float, CultureInfo.InvariantCulture, out doubleResult);
            }

            // check for ability to convert to double, without throwing an exception
            return (candidate != null &&
                    (candidate is Int16 ||
                     candidate is Int32 ||
                     candidate is Int64 ||
                     candidate is decimal ||
                     candidate is Single ||
                     candidate is Double ||
                     candidate is Boolean));
        }
        #endregion

        /// <summary>
        /// Generates and gets a single lambda expression from the list of expressions
        /// created up to the 'get' call. Lambda can then be applied in a "Where" Linq query</summary>
        protected Expression LambdaExpression
        {
            get
            {
                if (m_queryableData == null)
                    throw new InvalidOperationException("Attempting to construct a Lambda expression when the expression on which to iterate hasn't been created.");

                if (m_lambdaExpression == null)
                {
                    Expression finalExpression = null;
                    foreach (Expression e in m_expressionList)
                    {
                        if (finalExpression == null)
                            finalExpression = e;
                        else
                            finalExpression = Expression.AndAlso(finalExpression, e);
                    }
                    m_lambdaExpression = Expression.Lambda(finalExpression, new[] { m_queryableData });
                }
                return m_lambdaExpression;
            }
        }
        /// <summary>
        /// Single lambda expression from the list of expressions</summary>
        protected LambdaExpression m_lambdaExpression;

        #endregion

        /// <summary>
        /// Gets or sets the "replace template", that is, the search pattern in the Linq query that should be used 
        /// to apply subsequent "replace" operations on the search results.</summary>
        /// <remarks>NOTE: While it does contain part of the search predicate, MatchPattern is NOT involved in the query!
        /// It should not be used to test matches during a search! That is the job of the LambdaExpression.</remarks>
        public IReplacingQueryPattern MatchPattern
        {
            set
            {
                if (m_matchPattern != null)
                    throw new InvalidOperationException("Search predicate has been assigned more than one match pattern");
                m_matchPattern = value;
            }
            get { return m_matchPattern; }
        }
        /// <summary>
        /// Search pattern in the Linq query for replace</summary>
        IReplacingQueryPattern m_matchPattern;

        /// <summary>
        /// Class that defines the replacement template for string search patterns</summary>
        public class StringReplaceQueryPattern : IReplacingQueryPattern
        {
            /// <summary>
            /// Constructor</summary>
            private StringReplaceQueryPattern() : this(null) { }

            /// <summary>
            /// Constructor</summary>
            /// <param name="pattern">String to match &amp; replace in the query data</param>
            public StringReplaceQueryPattern(string pattern) { m_pattern = pattern; }

            #region IReplacingQueryPattern

            /// <summary>
            /// Tests whether the specified query item matches the string replacement pattern</summary>
            /// <param name="itemToMatch">Query item to test</param>
            /// <returns>True iff there was a match</returns>
            public bool Matches(IQueryMatch itemToMatch)
            {
                string value = itemToMatch.GetValue().ToString();
                return Regex.Match(value, m_pattern, RegexOptions.IgnoreCase).Success;
            }

            /// <summary>
            /// Applies replacement on the query item data, using the string replacement pattern</summary>
            /// <param name="itemToReplace">Query item to modify</param>
            /// <param name="replaceWith">String to apply in the query item data, in place of the matching string</param>
            public void Replace(IQueryMatch itemToReplace, object replaceWith)
            {
                string value = itemToReplace.GetValue().ToString();
                itemToReplace.SetValue(Regex.Replace(value, m_pattern, replaceWith.ToString(), RegexOptions.IgnoreCase));
            }

            #endregion

            readonly string m_pattern;
        }

        /// <summary>
        /// Class that defines the replacement template for numerical search patterns</summary>
        public class NumberReplaceQueryPattern : IReplacingQueryPattern
        {
            /// <summary>
            /// Constructor</summary>
            private NumberReplaceQueryPattern() : this(0, 0) { }

            /// <summary>
            /// Constructor with numerical pattern</summary>
            /// <param name="pattern1">Numerical pattern to match against for replacement</param>
            public NumberReplaceQueryPattern(double pattern1) : this(pattern1, 0) { }

            /// <summary>
            /// Constructor with bounds</summary>
            /// <param name="pattern1">Low-value in "between" numerical pattern match</param>
            /// <param name="pattern2">High-value in "between" numerical pattern match</param>
            public NumberReplaceQueryPattern(double pattern1, double pattern2)
            {
                m_pattern1 = pattern1;
                m_pattern2 = pattern2;
            }

            #region IReplacingQueryPattern
            /// <summary>
            /// Tests whether the specified query item matches the numerical replacement pattern</summary>
            /// <param name="itemToMatch">Query item to test</param>
            /// <returns>True iff match successful</returns>
            public bool Matches(IQueryMatch itemToMatch)
            {
                return IsConvertibleToDouble(itemToMatch.GetValue());
            }

            /// <summary>
            /// Applies replacement on the query item data, using numerical replacement pattern</summary>
            /// <param name="itemToMatch">Query item to modify</param>
            /// <param name="replaceWith">Number to apply in the query item data, in place of matching number</param>
            public void Replace(IQueryMatch itemToMatch, object replaceWith)
            {
                // Make sure both itemToMatch and replaceWith are convertible, as attempting a convert that will fail
                // throws an exception, which is time-costly
                if (IsConvertibleToDouble(itemToMatch.GetValue()) && IsConvertibleToDouble(replaceWith))
                {
                    Double newValue = Convert.ToDouble(replaceWith);
                    itemToMatch.SetValue(newValue);
                }
            }
            #endregion

            double m_pattern1;
            double m_pattern2;
        }

        /// <summary>
        /// Named parameter expression on which to iterate</summary>
        protected ParameterExpression m_queryableData;
    }

    /// <summary>
    /// Enum defining masks for which string search types can be selected</summary>
    public enum StringQuery
    {
        /// <summary>No search</summary>
        None=0x00,
        /// <summary>Look for matches</summary>
        Matches = 0x01,
        /// <summary>Contains text</summary>
        Contains = 0x02,
        /// <summary>Begins with text</summary>
        BeginsWith = 0x04,
        /// <summary>Ends with text</summary>
        EndsWith = 0x08,
        /// <summary>Search using regular expression</summary>
        RegularExpression = 0x10,
        /// <summary>Use all string searches</summary>
        All = 0xFF
    }

    /// <summary>
    /// Enum defining masks for which numerical search types can be selected</summary>
    public enum NumericalQuery
    {
        /// <summary>No search</summary>
        None = 0x00,
        /// <summary>Equals</summary>
        Equals = 0x01,
        /// <summary>Less than</summary>
        Lesser = 0x02,
        /// <summary>Less than or equal</summary>
        LesserEqual = 0x04,
        /// <summary>Equals</summary>
        Equal = 0x08,
        /// <summary>Greater than or equal</summary>
        GreaterEqual = 0x10,
        /// <summary>Greater than</summary>
        Greater = 0x20,
        /// <summary>In between values</summary>
        Between = 0x40,
        /// <summary>Use all numerical search types</summary>
        All = 0xFF
    }
}
