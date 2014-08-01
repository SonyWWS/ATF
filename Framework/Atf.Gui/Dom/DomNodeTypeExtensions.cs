//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using System.Linq;
using Sce.Atf.Dom;
using PropertyDescriptor = System.ComponentModel.PropertyDescriptor;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Extension methods for working with DomNodeType</summary>
    public static class DomNodeTypeExtensions
    {
        /// <summary>
        /// Register a PropertyDescriptor on a DomNodeType</summary>
        /// <param name="type">The DomNode type</param>
        /// <param name="descriptor">The property descriptor</param>
        public static void RegisterDescriptor(this DomNodeType type, System.ComponentModel.PropertyDescriptor descriptor)
        {
            var collection = type.GetTagLocal<PropertyDescriptorCollection>();
            if (collection == null)
            {
                collection = new PropertyDescriptorCollection(EmptyArray<PropertyDescriptor>.Instance);
                type.SetTag(collection);
            }

            // This is required due to an issue in AttributePropertyDescriptor Equals() implementation
            if (!(descriptor is AttributePropertyDescriptor)
                || !collection.OfType<AttributePropertyDescriptor>().Any(x => Equivalent(x, (AttributePropertyDescriptor)descriptor)))
            {
                collection.Add(descriptor);
            }
        }

        private static bool Equivalent(AttributePropertyDescriptor x, AttributePropertyDescriptor y)
        {
            if (!x.AttributeInfo.Equivalent(y.AttributeInfo))
                return false;

            var descX = x as ChildAttributePropertyDescriptor;
            var descY = y as ChildAttributePropertyDescriptor;

            if (descX == null || descY == null)
                return false;

            var xPath = descX.Path.ToArray();
            var yPath = descY.Path.ToArray();
            if (xPath.Length != yPath.Length)
                return false;

            for (int i = 0; i < xPath.Length; i++)
            {
                if (!yPath[i].IsEquivalent(xPath[i]))
                    return false;
            }
            return true;
        }

    }
}