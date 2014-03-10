//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Utilities to use strongly typed property changed notifications for
    /// INotifyPropertyChanged.
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
            return new PropertyChangedEventArgs(TypeUtil.GetProperty<T>(expression).Name);
        }
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
            MemberExpression memberExpression = expression as MemberExpression;
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
            PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                ThrowExpressionArgumentException("propertySelector");
            }

            return propertyInfo;
        }

        /// <summary>
        /// Gets a PropertyDescriptor from a type and a lambda expression containing a property of the type</summary>
        /// <typeparam name="T">Type whose property changes</typeparam>
        /// <param name="propertySelector">Lambda expression containing the property</param>
        /// <returns>PropertyDescriptor from a type and its property</returns>
        /// <example>Here the lambda expression's variable is an instance of the type and the right side
        /// is that variable with a get for the property, i.e., x.Label:
        /// PropertyDescriptor s_labelPropDesc = TypeUtil.GetPropertyDescriptor&lt;Node&gt;(x =&gt; x.Label);</example>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static PropertyDescriptor GetPropertyDescriptor<T>(Expression<Func<T, object>> propertySelector)
        {
            PropertyInfo info = GetProperty<T>(propertySelector);
            return TypeDescriptor.GetProperties(typeof(T))[info.Name];
        }

        private static void ThrowExpressionArgumentException(string argumentName)
        {
            throw new ArgumentException("It's just the simple expression 'x => x.Property' allowed.", argumentName);
        }
    }
}
