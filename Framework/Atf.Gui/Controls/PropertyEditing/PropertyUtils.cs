//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Input;

using PropertyDescriptor = System.ComponentModel.PropertyDescriptor;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Utilities for working with PropertyDescriptors</summary>
    public static class PropertyUtils
    {
        /// <summary>
        /// Obtains the default properties for an object</summary>
        /// <param name="item">Object whose properties are obtained</param>
        /// <returns>Default properties for an object, as a PropertyDescriptorCollection</returns>
        public static PropertyDescriptorCollection GetDefaultProperties(object item)
        {
            PropertyDescriptorCollection propertyCollection;
            ICustomTypeDescriptor customTypeDescriptor = item.As<ICustomTypeDescriptor>();
            if (customTypeDescriptor != null)
            {
                propertyCollection = customTypeDescriptor.GetProperties();
            }
            else if (!UseCustomTypeDescriptorsOnly)
            {
                propertyCollection = TypeDescriptor.GetProperties(item);
            }
            else
            {
                propertyCollection = new PropertyDescriptorCollection(EmptyArray<PropertyDescriptor>.Instance);
            }

            return propertyCollection;
        }

        /// <summary>
        /// Obtains the default properties for an object</summary>
        /// <param name="owner">Object whose properties are obtained</param>
        /// <returns>Default properties for an object, as an array</returns>
        public static PropertyDescriptor[] GetDefaultProperties2(object owner)
        {
            PropertyDescriptorCollection propertyCollection = GetDefaultProperties(owner);
            PropertyDescriptor[] result = new PropertyDescriptor[propertyCollection.Count];
            propertyCollection.CopyTo(result, 0);
            return result;
        }

        /// <summary>
        /// Obtains the default properties for multiple objects</summary>
        /// <param name="items">Items with properties</param>
        /// <returns>Default properties for multiple objects</returns>
        public static IEnumerable<PropertyDescriptor> GetProperties(IEnumerable<object> items)
        {
            List<PropertyDescriptor> result = new List<PropertyDescriptor>();
            HashSet<PropertyDescriptor> propertySet = new HashSet<PropertyDescriptor>();

            foreach (object item in items)
            {
                PropertyDescriptorCollection properties = GetDefaultProperties(item);
                foreach (PropertyDescriptor property in properties)
                {
                    if (!propertySet.Contains(property))
                    {
                        propertySet.Add(property);
                        result.Add(property);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Obtains the common default properties for multiple objects</summary>
        /// <param name="items">Items with properties</param>
        /// <returns>Default properties all shared by multiple objects</returns>
        /// <remarks>Some of the descriptors may be MultiPropertyDescriptor so that all the items can share them.</remarks>
        public static IEnumerable<PropertyDescriptor> GetSharedProperties(IEnumerable<object> items)
        {
            List<PropertyDescriptor> result = new List<PropertyDescriptor>();
            bool firstTime = true;
            foreach (object item in items)
            {
                IEnumerable<PropertyDescriptor> properties = GetDefaultProperties(item).Cast<PropertyDescriptor>();
                if (firstTime)
                {
                    firstTime = false;
                    result.AddRange(properties);
                }
                else
                {
                    for (int i = 0; i < result.Count; )
                    {
                        string key = GetPropertyDescriptorKey(result[i]);

                        // Check the item for a matching property descriptor
                        // If found: replace normal descriptor with MultiPropertyDescriptor in the result
                        // Otherwise: Remove the property from merged properties
                        PropertyDescriptor matchingDescriptor = FindPropertyDescriptor(item, key);
                        if (matchingDescriptor != null)
                        {
                            var multiDescriptor = result[i] as MultiPropertyDescriptor;
                            if (multiDescriptor == null)
                            {
                                multiDescriptor = new MultiPropertyDescriptor(result[i]);
                                result[i] = multiDescriptor;
                            }
                            i++;
                        }
                        else
                            result.RemoveAt(i);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Obtain the common default properties for multiple objects</summary>
        /// <remarks>Some of the descriptors may be MultiPropertyDescriptor so that all the items can share them.
        /// <para>DAN: the above method has been edited and broken! MultiPropertyDescriptor is specific to DOM.</para></remarks>
        /// <param name="items">Items with properties</param>
        /// <returns>Default properties all shared by multiple objects</returns>
        public static IEnumerable<PropertyDescriptor> GetSharedPropertiesOriginal(IEnumerable<object> items)
        {
            List<PropertyDescriptor> result = new List<PropertyDescriptor>();
            bool firstTime = true;
            foreach (object item in items)
            {
                PropertyDescriptorCollection properties = GetDefaultProperties(item);
                if (firstTime)
                {
                    firstTime = false;
                    foreach (PropertyDescriptor property in properties)
                        result.Add(property);
                }
                else
                {
                    // remove any descriptors in result that this owner doesn't have
                    HashSet<PropertyDescriptor> propertySet = new HashSet<PropertyDescriptor>();
                    foreach (PropertyDescriptor property in properties)
                        propertySet.Add(property);
                    for (int i = 0; i < result.Count; )
                    {
                        if (!propertySet.Contains(result[i]))
                            result.RemoveAt(i);
                        else
                            i++;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a hash code for a property descriptor that allows different PropertyDescriptor
        /// objects to be considered equivalent if their name, category, and property type match</summary>
        /// <param name="propertyDescriptor">Property descriptor to get hash code for</param>
        /// <returns>Hash code for property descriptor</returns>
        /// <remarks>Consider using GetPropertyDescriptorKey() instead, to avoid any chance of
        /// collisions, since it's not really appropriate to compare hash codes alone when
        /// determining equality.</remarks>
        public static int GetPropertyDescriptorHash(this PropertyDescriptor propertyDescriptor)
        {
            // Different PropertyDescriptor objects need to have the same hash code if the Name
            //  and Category and PropertyType match. Note that 'Name' and 'Category' can be equal
            //  to each other, so we can't xor their hash codes together. Note that Category can be null.
            // http://tracker.ship.scea.com/jira/browse/WWSATF-1051
            return
                propertyDescriptor.Name.GetHashCode() ^
                (GetCategoryName(propertyDescriptor).GetHashCode() << 8) ^
                propertyDescriptor.PropertyType.GetHashCode();
        }

        /// <summary>
        /// Gets a string ID or "key" for a property descriptor. If two keys are equal, then the
        /// PropertyDescriptor objects should be considered equivalent.</summary>
        /// <param name="propertyDescriptor">Property descriptor</param>
        /// <returns>Identifier or "key" for property descriptor</returns>
        public static string GetPropertyDescriptorKey(this PropertyDescriptor propertyDescriptor)
        {
            // This is called a lot. Try to reduce unnecessary string copying.
            string name = propertyDescriptor.Name;
            string category = GetCategoryName(propertyDescriptor);
            string typeString = propertyDescriptor.PropertyType.FullName;
            int capacity = name.Length + category.Length + typeString.Length + 2;

            var sb = new StringBuilder(name, 0, name.Length, capacity);
            sb.Append(',');
            sb.Append(category);
            sb.Append(',');
            sb.Append(typeString);

            return sb.ToString();
        }

        /// <summary>
        /// Gets whether two property descriptors should be considered equal, for the purposes of ATF
        /// property editing. The default .NET Equals() method doesn't take into account the Category.</summary>
        /// <param name="a">First PropertyDescriptor to compare</param>
        /// <param name="b">Second PropertyDescriptor to compare</param>
        /// <returns><c>True</c> if PropertyDescriptors equal, i.e., the name, category and type are the same</returns>
        public static bool PropertyDescriptorsEqual(PropertyDescriptor a, PropertyDescriptor b)
        {
            return
                a.Name == b.Name &&
                GetCategoryName(a) == GetCategoryName(b) &&
                a.PropertyType == b.PropertyType;
        }

        /// <summary>
        /// Obtains a property's category name</summary>
        /// <param name="descriptor">Property descriptor</param>
        /// <returns>Property's category name; "Misc" if none is defined by the descriptor</returns>
        public static string GetCategoryName(PropertyDescriptor descriptor)
        {
            string result = descriptor.Category;
            if (result == null)
                result = "Misc".Localize("Miscellaneous category");

            return result;
        }

        /// <summary>
        /// Gets culture formatted text representing the property's value</summary>
        /// <param name="owner">Object whose property text is obtained</param>
        /// <param name="descriptor">Property descriptor</param>
        /// <returns>Culture formatted text representing the property's value</returns>
        public static string GetPropertyText(object owner, PropertyDescriptor descriptor)
        {
            string result = string.Empty;

            // With reflected child property descriptors it is possible to get descriptors
            // that are supposed to operate on a referenced child rather than the object itself
            // Using a PropertyDescriptor on an object type it's not designed for often
            // leads to an exception, which this check safeguards against.
            if (owner != null &&
                !owner.Is<ICustomTypeDescriptor>() &&
                !descriptor.ComponentType.IsAssignableFrom(owner.GetType()))
                return result;

            object value = descriptor.GetValue(owner);
            if (value != null)
            {
                TypeConverter converter = descriptor.Converter;
                if (converter != null && converter.CanConvertTo(typeof(string)))
                {
                    result = converter.ConvertTo(value, typeof(string)) as string;
                }
                if (string.IsNullOrEmpty(result))
                {
                    IFormattable formattable = value as IFormattable;
                    if (formattable != null)
                    {
                        result = formattable.ToString(null, CultureInfo.CurrentUICulture);
                    }
                    else
                    {
                        result = value.ToString();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Event that is raised when a property error occurs</summary>
        /// <remarks>Applications can override the default error MessageBox by setting the Cancel
        /// field to true</remarks>
        public static event EventHandler<PropertyErrorEventArgs> PropertyError;

        /// <summary>
        /// Event that is raised when a property value is edited</summary>
        public static event EventHandler<PropertyEditedEventArgs> PropertyEdited;

        /// <summary>
        /// Tests if a key modifies the text in the edit box</summary>
        /// <param name="keyData">A key, such as from ProcessDialogKey</param>
        /// <returns><c>True</c> if the key modifies text</returns>
        public static bool IsEditKey(Keys keyData)
        {
            if (KeysUtil.IsPrintable(keyData))
                return true;

            // remove any modifiers and check for some non-printable editing keys
            keyData &= Keys.KeyCode;
            return
                keyData == Keys.Back ||
                keyData == Keys.Delete;
        }

        /// <summary>
        /// Sets the property value to the new value</summary>
        /// <param name="owner">Object whose property is set</param>
        /// <param name="descriptor">Property descriptor</param>
        /// <param name="value">New property value</param>
        public static void SetProperty(object owner, PropertyDescriptor descriptor, object value)
        {
            try
            {
                TypeConverter converter = descriptor.Converter;
                if (converter != null &&
                    value != null &&
                    converter.CanConvertFrom(value.GetType()))
                {
                    value = converter.ConvertFrom(value);
                }

                bool notifyChange = false;
                object oldValue = null;
                EventHandler<PropertyEditedEventArgs> handler = PropertyEdited;
                if (handler != null)
                {
                    oldValue = descriptor.GetValue(owner);
                    notifyChange = !PropertyUtils.AreEqual(oldValue, value);

                }
                descriptor.SetValue(owner, value);
                if (notifyChange)
                {
                    PropertyEditedEventArgs eventArgs = new PropertyEditedEventArgs(owner, descriptor, oldValue, value);
                    handler(null, eventArgs);
                }

            }
            catch (InvalidTransactionException)
            {
                // The transaction needs to be cancelled.
                throw;
            }
            catch (Exception ex)
            {
                PropertyErrorEventArgs eventArgs = new PropertyErrorEventArgs(owner, descriptor, ex);

                PropertyError.Raise(null, eventArgs);

                if (!eventArgs.Cancel)
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
            }
        }

        /// <summary>
        /// Returns whether or not the property can be reset</summary>
        /// <param name="owners">Objects whose property is tested</param>
        /// <param name="descriptor">Property to reset</param>
        /// <returns><c>True</c> if the property can be reset on the object</returns>
        public static bool CanResetProperty(IEnumerable<object> owners, PropertyDescriptor descriptor)
        {
            foreach (object owner in owners)
                if (!descriptor.CanResetValue(owner))
                    return false;

            return true;
        }

        /// <summary>
        /// Resets the property on objects</summary>
        /// <param name="owners">Objects whose property is reset</param>
        /// <param name="descriptor">Property to reset</param>
        public static void ResetProperty(IEnumerable<object> owners, PropertyDescriptor descriptor)
        {
            foreach (object owner in owners)
                descriptor.ResetValue(owner);
        }

        /// <summary>
        /// Returns whether the given values are equal</summary>
        /// <param name="value1">First value</param>
        /// <param name="value2">Second value</param>
        /// <returns><c>True</c> if the given values are equal</returns>
        /// <remarks>Does a limited descent into the values, testing array equality</remarks>
        public static bool AreEqual(object value1, object value2)
        {
            if (value1 == null)
                return (value2 == null);

            Array array1 = value1 as Array;
            if (array1 != null)
            {
                Array array2 = value2 as Array;
                if (array2 != null)
                {
                    // compare array shapes
                    int rank = array1.Rank;
                    if (rank != array2.Rank)
                        return false;

                    for (int i = 0; i < rank; i++)
                    {
                        int length = array1.GetLength(i);
                        if (length != array2.GetLength(i))
                            return false;
                    }

                    // compare array elements; lengths must be equal!
                    for (int i = 0; i < array1.Length; i++)
                        if (!AreEqual(array1.GetValue(i), array2.GetValue(i)))
                            return false;

                    return true;
                }
            }

            if (value1 is Single)
            {
                return
                    value2 is Single &&
                    MathUtil.AreApproxEqual((Single)value1, (Single)value2, 0.000001);
            }
            else if (value1 is Double)
            {
                return
                    value2 is Double &&
                    MathUtil.AreApproxEqual((Double)value1, (Double)value2, 0.000001);
            }

            return value1.Equals(value2);
        }


        /// <summary>
        /// Gets or sets a value determining if only properties defined by an ICustomTypeDescriptor 
        /// adapter are used. Otherwise ATF defaults to use all public properties for objects 
        /// without such custom adapters. False by default.</summary>
        public static bool UseCustomTypeDescriptorsOnly { get; set; }


        /// <summary>
        /// Find a property descriptor matching the specified key</summary>
        /// <param name="item">Item with properties; should be (convertable to) a DomNode to take advantage of cache</param>
        /// <param name="key">Identifier string of category, name and type that can be obtained through PropertyUtils.GetPropertyDescriptorKey</param>
        /// <returns>Matching property descriptor if found; null otherwise</returns>
        /// <remarks>If the item is (convertable to) a DomNode, a cached dictionary is used for lookup.
        /// Otherwise the item is scanned every time this method is called.</remarks>
        internal static PropertyDescriptor FindPropertyDescriptor(object item, string key)
        {
            // First, try to cache the results, if the property descriptor will be the same
            //  for all instances of this type of 'item'. This greatly improves results. In
            //  a test case with 112 complex objects in CoreTextEditor, removing the cache
            //  makes showing the properties of the selected objects take over 6 seconds.
            var dynamicTypeDescriptor = item.As<IDynamicTypeDescriptor>();
            if (dynamicTypeDescriptor == null ||
                dynamicTypeDescriptor.CacheableProperties)
            {
                DomNode domNode = item.As<DomNode>();
                if (domNode != null)
                {
                    Dictionary<string, PropertyDescriptor> dict;
                    if (!s_descriptorCache.TryGetValue(domNode.Type, out dict))
                    {
                        dict = new Dictionary<string, PropertyDescriptor>();
                        foreach (PropertyDescriptor desc in GetDefaultProperties(item))
                            dict.Add(desc.GetPropertyDescriptorKey(), desc);
                        s_descriptorCache.Add(domNode.Type, dict);
                    }

                    PropertyDescriptor descriptor;
                    dict.TryGetValue(key, out descriptor);
                    return descriptor;
                }
            }

            foreach (PropertyDescriptor desc in GetDefaultProperties(item))
                if (desc.GetPropertyDescriptorKey() == key)
                    return desc;

            return null;
        }

        // DomNodeType -> { {string of Name, Category, and type} -> PropertyDescriptor }
        private static readonly Dictionary<DomNodeType, Dictionary<string, PropertyDescriptor>> s_descriptorCache
            = new Dictionary<DomNodeType, Dictionary<string, PropertyDescriptor>>();
    }
}