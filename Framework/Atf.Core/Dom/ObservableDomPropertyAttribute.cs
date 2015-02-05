//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Attribute used on properties of DomNodeAdapters derived from ObservableDomNodeAdapter</summary>
    /// <remarks>
    /// When placed on a property, the ObservableDomNodeAdapter will raise NotifyPropertyChanged events
    /// for this property when the corresponding attribute changes</remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ObservableDomPropertyAttribute : Attribute
    {
        /// <summary>
        /// Constructor with attribute name</summary>
        /// <param name="attributeName">Name of property to observe</param>
        public ObservableDomPropertyAttribute(string attributeName)
        {
            AttributeName = attributeName;
        }

        /// <summary>
        /// Get or set name of property to observe</summary>
        public string AttributeName { get; set; }
    }

    /// <summary>
    /// Attribute for properties of DomNodeAdapters derived from ObservableDomNodeAdapter
    /// </summary>
    /// <remarks>
    /// Similar to ObservableDomPropertyAttribute but for use with single (non-list) Dom child properties 
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ObservableDomChildAttribute : Attribute
    {
        /// <summary>
        /// Constructor with child name</summary>
        /// <param name="childName">Child name</param>
        public ObservableDomChildAttribute(string childName)
        {
            ChildName = childName;
        }

        /// <summary>
        /// Child name</summary>
        public string ChildName { get; set; }
    }
}
