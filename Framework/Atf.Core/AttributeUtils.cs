//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Reflection;

namespace Sce.Atf
{
    /// <summary>
    /// Utilities for handling entity attributes</summary>
    public static class AttributeUtils
    {
        /// <summary>
        /// Obtains the attribute on a property/field/method of a specific type</summary>
        /// <typeparam name="T">Specified type</typeparam>
        /// <param name="info">Member with attribute</param>
        /// <param name="inherit">Whether to search member's inheritance chain to find attribute</param>
        /// <returns>Attribute, if it exists, of specified type</returns>
        public static T GetAttribute<T>(MemberInfo info, bool inherit = false)
            where T : Attribute
        {
            var attr = info.GetCustomAttributes(typeof(T), inherit);
            return (attr.Length > 0) ? (T)attr[0] : default(T);
        }
    }
}
