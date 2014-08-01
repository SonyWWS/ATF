//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Numeric format string attribute</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NumberFormatAttribute : Attribute
    {
        /// <summary>
        /// Constructor</summary>
        public NumberFormatAttribute()
        {
            m_formatString = null;
            m_maxPrecision = null;
            m_scale = null;
        }

        /// <summary>
        /// Constructor with format string</summary>
        /// <param name="formatString">Format string</param>
        public NumberFormatAttribute(string formatString)
        {
            m_formatString = formatString;
            m_maxPrecision = null;
            m_scale = null;
        }

        /// <summary>
        /// Constructor with format string</summary>
        /// <param name="formatString">Format string</param>
        /// <param name="maxPrecision">Maximum precision</param>
        /// <param name="scale">Scale</param>
        public NumberFormatAttribute(string formatString, int maxPrecision, double scale)
        {
            m_formatString = formatString;
            m_maxPrecision = maxPrecision;
            m_scale = scale;
        }

        /// <summary>
        /// Tests equality to an object</summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True iff objects are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            var attribute = obj as NumberFormatAttribute;
            if (attribute != null)
            {
                if ((attribute.m_formatString == m_formatString) && (attribute.m_maxPrecision == m_maxPrecision))
                {
                    return (attribute.m_scale == m_scale);
                }
            }

            return false;
        }

        /// <summary>
        /// Gets hash code for instance</summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return (((m_formatString != null ? m_formatString.GetHashCode() : 0) ^ m_maxPrecision.GetHashCode()) ^
                    m_scale.GetHashCode());
        }

        /// <summary>
        /// Gets format string</summary>
        public string FormatString
        {
            get { return m_formatString; }
        }

        /// <summary>
        /// Gets maximum precision</summary>
        public int? MaxPrecision
        {
            get { return m_maxPrecision; }
        }

        /// <summary>
        /// Gets scale</summary>
        public double? Scale
        {
            get { return m_scale; }
        }

        private readonly string m_formatString;
        private readonly int? m_maxPrecision;
        private readonly double? m_scale;
    }

    /// <summary>
    /// Numeric range attribute</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NumberRangesAttribute : Attribute
    {
        /// <summary>
        /// Constructor</summary>
        public NumberRangesAttribute()
        {
            m_minimum = double.MinValue;
            m_maximum = double.MaxValue;
            m_center = double.NaN;
            m_hardMinimum = double.NaN;
            m_hardMaximum = double.NaN;
        }

        /// <summary>
        /// Constructor with range</summary>
        /// <param name="minimum">Minimum value</param>
        /// <param name="maximum">Maximum value</param>
        public NumberRangesAttribute(double minimum, double maximum)
        {
            m_minimum = minimum;
            m_maximum = maximum;
            m_center = minimum + ((maximum - minimum) / 2.0);
            m_hardMinimum = double.NaN;
            m_hardMaximum = double.NaN;
        }

        public NumberRangesAttribute(double minimum, double maximum, double center)
        {
            m_minimum = minimum;
            m_maximum = maximum;
            m_center = center;
            m_hardMinimum = double.NaN;
            m_hardMaximum = double.NaN;
        }

        public NumberRangesAttribute(double minimum, double maximum, double center, double hardMin, double hardMax)
        {
            m_minimum = minimum;
            m_maximum = maximum;
            m_center = center;
            m_hardMinimum = hardMin;
            m_hardMaximum = hardMax;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            
            var attribute = obj as NumberRangesAttribute;
            if (attribute != null)
            {
                if ((attribute.m_minimum == m_minimum) 
                    && (attribute.m_maximum == m_maximum)
                    && (attribute.m_hardMinimum == m_hardMinimum)
                    && (attribute.m_hardMaximum == m_hardMaximum)
                    && (attribute.m_center == m_center))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets hash code for instance</summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return 
                m_minimum.GetHashCode() ^ 
                m_maximum.GetHashCode() ^
                m_hardMinimum.GetHashCode() ^ 
                m_hardMaximum.GetHashCode() ^ 
                m_center.GetHashCode();
        }

        /// <summary>
        /// Gets maximum value</summary>
        public double Maximum
        {
            get { return m_maximum; }
        }

        public double Minimum
        {
            get { return m_minimum; }
        }

        public double HardMinimum
        {
            get { return m_hardMinimum; }
        }

        public double HardMaximum
        {
            get { return m_hardMaximum; }
        }

        public double Center
        {
            get { return m_center; }
        }

        private readonly double m_maximum;
        private readonly double m_minimum;
        private readonly double m_hardMaximum;
        private readonly double m_hardMinimum;
        private readonly double m_center;
    }

    /// <summary>
    /// Numeric small/large increment attribute</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NumberIncrementsAttribute : Attribute
    {
        /// <summary>
        /// Constructor</summary>
        public NumberIncrementsAttribute()
        {
            m_smallChange = 0.1;
            m_defaultChange = 1.0;
            m_largeChange = 10.0;
            m_isLogarithmic = false;
        }

        /// <summary>
        /// Constructor with increments</summary>
        /// <param name="smallChange">Small change increment</param>
        /// <param name="largeChange">Large change increment</param>
        public NumberIncrementsAttribute(double smallChange, double defaultChange, double largeChange)
        {
            m_smallChange = smallChange;
            m_defaultChange = defaultChange;
            m_largeChange = largeChange;
            m_isLogarithmic = false;
        }

        public NumberIncrementsAttribute(double smallChange, double defaultChange, double largeChange, bool isLogarithmic)
        {
            m_smallChange = smallChange;
            m_defaultChange = defaultChange;
            m_largeChange = largeChange;
            m_isLogarithmic = isLogarithmic;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            
            var attribute = obj as NumberIncrementsAttribute;
            if (attribute != null)
            {
                return (attribute.m_smallChange == m_smallChange)
                       && (attribute.m_largeChange == m_largeChange)
                       && (attribute.m_defaultChange == m_defaultChange)
                       && (attribute.m_isLogarithmic == m_isLogarithmic);
                }
            
            return false;
        }

        /// <summary>
        /// Gets hash code for instance</summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return 
                m_smallChange.GetHashCode() ^ 
                m_largeChange.GetHashCode() ^ 
                m_defaultChange.GetHashCode() & 
                m_isLogarithmic.GetHashCode();
        }

        public double DefaultChange
        {
            get { return m_defaultChange; }
        }

        public double LargeChange
        {
            get { return m_largeChange; }
        }

        public double SmallChange
        {
            get { return m_smallChange; }
        }

        public bool IsLogarithimc
            {
            get { return m_isLogarithmic; }
        }

        private readonly double m_largeChange;
        private readonly double m_defaultChange;
        private readonly double m_smallChange;
        private readonly bool m_isLogarithmic;
    }

    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public class EnumDisplayNameAttribute : Attribute
    {
        /// <summary>
        /// Default constructor, necessary to be XAML friendly</summary>
        public EnumDisplayNameAttribute()
        {
        }

        /// <summary>
        /// Constructs a EnumDisplayNameAttribute and initializes it to the input string</summary>
        /// <param name="displayName">The display string</param>
        public EnumDisplayNameAttribute(string displayName)
        {
            m_value = displayName;
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

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class StandardValuesAttributeBase : Attribute
    {
        public abstract object[] GetValues(IEnumerable components);
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class StandardValuesAttribute : StandardValuesAttributeBase
    {
        public StandardValuesAttribute(object[] values)
        {
            Values = values;
        }

        public object[] Values { get; set; }

        public override object[] GetValues(IEnumerable components)
        {
            return Values;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class GroupEnabledAttribute : Attribute
    {
        public GroupEnabledAttribute(GroupEnables[] enables)
        {
            GroupEnables = enables;
        }

        public GroupEnabledAttribute(string groupName, string stringValues)
        {
            string[] values = stringValues.Split(',');
            GroupEnables = new[] { new GroupEnables(groupName, values) };
        }

        // This is a work around for the fact that the MemberDescriptor base class
        // seems to filter out attributes of matching types which prevents multiple
        // GroupEnabledAttributes from being placed on a PropertyDescriptor
        public readonly GroupEnables[] GroupEnables;
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DependencyAttribute : Attribute
    {
        public string[] DependencyDescriptors { get; set; }
        public string[] DependencyDescriptor { get; set; }

        public DependencyAttribute(string dependencyDescriptor)
        {
            DependencyDescriptor = new []{ dependencyDescriptor};
        }

        public DependencyAttribute(string[] dependencyDescriptors)
        {
            DependencyDescriptors = dependencyDescriptors;
        }
    }

    public class GroupEnables
    {
        public GroupEnables(string groupName, string[] stringValues)
        {
            GroupName = groupName;
            StringValues = stringValues; 
        }
        public string GroupName { get; private set; }
        public string[] StringValues { get; private set; }
        public object[] Values { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayUnitsAttribute : Attribute
    {
        public DisplayUnitsAttribute(string units)
        {
            Units = units;
        }

        public string Units { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class StringFilterAttribute : Attribute
    {
        public StringFilterAttribute(string filter)
        {
            // create object from type name
            //Type objectType = Type.GetType(filter);
            //if (objectType == null)
            //    throw new InvalidDataException("Couldn't find type " + filter);

            //// initialize with params
            //object obj = Activator.CreateInstance(objectType);
            //var annotatedObj = obj as IStringValueFilter;
            //if (annotatedObj != null)
            //{
            //}

            //Filter = annotatedObj;
        }

        public IStringValueFilter Filter { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ImageListAttribute : Attribute
    {
        public ImageListAttribute(object[] imageKeys)
        {
            ImageKeys = imageKeys;
        }

        public object[] ImageKeys { get; set; }
    }

    /// <summary>
    /// Attribute used to tag a property which is to be used as the title/header for an object
    /// e.g. in property grid or property lists
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class HeaderPropertyAttribute : Attribute
    {
    }
}
