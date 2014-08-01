//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Sce.Atf
{
    /// <summary>
    /// Utilities to use strongly typed property changed notifications for INotifyPropertyChanged.
    /// Uses reflection and lambda expressions to create a cached typed set of event args,
    /// rather than hard coding property name string values throughout the code. See
    /// http://compositeextensions.codeplex.com/Thread/View.aspx?ThreadId=53731. </summary>
    public static class ObservableUtil
    {
        /// <summary>
        /// Create PropertyChangedEventArgs for a type and a lambda expression containing a property of the type</summary>
        /// <typeparam name="T">Type whose property changes</typeparam>
        /// <param name="expression">Lambda expression containing the property</param>
        /// <returns>PropertyChangedEventArgs for the type and property in the lambda expression</returns>
        /// <example>Here the lambda expression's variable is an instance of the type and the right side
        /// is that variable with a get for the property, i.e., x.Label:
        /// PropertyChangedEventArgs s_labelArgs = ObservableUtil.CreateArgs&lt;Node&gt;(x =&gt; x.Label);</example>
        public static PropertyChangedEventArgs CreateArgs<T>(Expression<Func<T, object>> expression)
        {
            return new PropertyChangedEventArgs(TypeUtil.GetProperty(expression).Name);
        }

        /// <summary>
        /// Singleton event args for all properties changed
        /// </summary>
        public static readonly PropertyChangedEventArgs AllChangedEventArgs = new PropertyChangedEventArgs(string.Empty);
    }

    /// <summary>
    /// Utilities to use strongly typed property changed notifications for
    /// INotifyPropertyChanged.
    /// Uses reflection and lambda expressions to create a cached typed set of event args,
    /// rather than hard coding property name string values throughout the code. See
    /// http://compositeextensions.codeplex.com/Thread/View.aspx?ThreadId=53731. </summary>
    public static class TypeUtil
    {
        /// <summary>
        /// Gets a PropertyInfo (for access to property metadata) from a type and
        /// a lambda expression containing a property of the type</summary>
        /// <typeparam name="T">Type whose property changes</typeparam>
        /// <param name="propertySelector">Lambda expression containing the property</param>
        /// <returns>PropertyInfo from a type and its property</returns>
        /// <example>Here the lambda expression's variable is an instance of the type and the right side
        /// is that variable with a get for the property, i.e., x.Label:
        /// PropertyInfo s_labelProp = TypeUtil.GetProperty&lt;Node&gt;(x =&gt; x.Label);</example>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static PropertyInfo GetProperty<T>(Expression<Func<T, object>> propertySelector)
        {
            Expression expression = propertySelector.Body;

            // If the Property returns a ValueType then a Convert is required => Remove it
            if (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            // If this isn't a member access expression then the expression isn't valid
            var memberExpression = expression as MemberExpression;
            if (memberExpression == null)
            {
                ThrowExpressionArgumentException("propertySelector");
            }

            expression = memberExpression.Expression;

            // If the Property returns a ValueType then a Convert is required => Remove it
            if (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            // Check if the expression is the parameter itself
            if (expression.NodeType != ExpressionType.Parameter)
            {
                ThrowExpressionArgumentException("propertySelector");
            }

            // Finally retrieve the MemberInfo
            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                ThrowExpressionArgumentException("propertySelector");
            }

            return propertyInfo;
        }

        public static PropertyInfo GetPropertyInfo(Expression<Func<object>> propertySelector)
        {
            PropertyInfo propertyInfo = null;
            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression != null)
            {
                // this is the usual case for when a property has a public getter and setter
                propertyInfo = memberExpression.Member as PropertyInfo;
            }
            else
            {
                // if the setter is private, the expression is a UnaryExpression type for some reason.
                var unaryExpression = propertySelector.Body as UnaryExpression;
                if (unaryExpression != null)
                {
                    memberExpression = unaryExpression.Operand as MemberExpression;
                    if (memberExpression != null)
                        propertyInfo = memberExpression.Member as PropertyInfo;
                }
            }

            if (propertyInfo == null)
                throw new ArgumentException(
                    "lambda expression was not properly formed." +
                    " Should be \"() => myObject.MyProperty\" or" +
                    " \"() => MyClass.MyProperty\"");

            return propertyInfo;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static PropertyDescriptor GetPropertyDescriptor<T>(Expression<Func<T, object>> propertySelector)
        {
            PropertyInfo info = GetProperty(propertySelector);
            return TypeDescriptor.GetProperties(typeof(T))[info.Name];
        }

        private static void ThrowExpressionArgumentException(string argumentName)
        {
            throw new ArgumentException("It's just the simple expression 'x => x.Property' allowed.", argumentName);
        }
    }
}
