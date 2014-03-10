//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Attribute to tag a field with a display string.
    /// Currently only used by EnumUtil for providing display strings for enum values.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DisplayStringAttribute : Attribute
    {
        /// <summary>
        /// Default constructor, necessary to be XAML friendly</summary>
        public DisplayStringAttribute()
        {
        }
        
        /// <summary>
        /// Constructs a DisplayStringAttribute and initializes it to the input string</summary>
        /// <param name="v">The display string</param>
        public DisplayStringAttribute(string v)
        {
            m_value = v;
        }
        
        /// <summary>
        /// Gets the display string</summary>
        public string Value
        {
            get { return m_value; }
        }

        // Note this can't be implemented as public readonly because then it
        // couldn't be set in xaml.
        private readonly string m_value;
    }
}
