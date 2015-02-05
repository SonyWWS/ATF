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
        /// Constructor with format string, precision, and scale</summary>
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

        /// <summary>
        /// Constructor with minimum, maximum, and center</summary>
        /// <param name="minimum">Minimum value</param>
        /// <param name="maximum">Maximum value</param>
        /// <param name="center">Center value</param>
        public NumberRangesAttribute(double minimum, double maximum, double center)
        {
            m_minimum = minimum;
            m_maximum = maximum;
            m_center = center;
            m_hardMinimum = double.NaN;
            m_hardMaximum = double.NaN;
        }

        /// <summary>
        /// Constructor with minimum, maximum, center, hard minimum, and hard maximum</summary>
        /// <param name="minimum">Minimum value</param>
        /// <param name="maximum">Maximum value</param>
        /// <param name="center">Center value</param>
        /// <param name="hardMin">Hard minimum value</param>
        /// <param name="hardMax">Hard maximum value</param>
        public NumberRangesAttribute(double minimum, double maximum, double center, double hardMin, double hardMax)
        {
            m_minimum = minimum;
            m_maximum = maximum;
            m_center = center;
            m_hardMinimum = hardMin;
            m_hardMaximum = hardMax;
        }

        /// <summary>
        /// Test if object is equal to instance</summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True iff object equals instance</returns>
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

        /// <summary>
        /// Gets minimum value</summary>
        public double Minimum
        {
            get { return m_minimum; }
        }

        /// <summary>
        /// Gets hard minimum value</summary>
        public double HardMinimum
        {
            get { return m_hardMinimum; }
        }

        /// <summary>
        /// Gets hard maximum value</summary>
        public double HardMaximum
        {
            get { return m_hardMaximum; }
        }

        /// <summary>
        /// Gets center value</summary>
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
        /// Default constructor</summary>
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
        /// <param name="defaultChange">Default change increment</param>
        /// <param name="largeChange">Large change increment</param>
        public NumberIncrementsAttribute(double smallChange, double defaultChange, double largeChange)
        {
            m_smallChange = smallChange;
            m_defaultChange = defaultChange;
            m_largeChange = largeChange;
            m_isLogarithmic = false;
        }

        /// <summary>
        /// Constructor with smallChange, default change, large change, and isLogarithmic</summary>
        /// <param name="smallChange">Small change increment</param>
        /// <param name="defaultChange">Default change increment</param>
        /// <param name="largeChange">Large change increment</param>
        /// <param name="isLogarithmic">Whether scale is logarithmic</param>
        public NumberIncrementsAttribute(double smallChange, double defaultChange, double largeChange, bool isLogarithmic)
        {
            m_smallChange = smallChange;
            m_defaultChange = defaultChange;
            m_largeChange = largeChange;
            m_isLogarithmic = isLogarithmic;
        }

        /// <summary>
        /// Test if object is equal to instance</summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True iff object equals instance</returns>
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

        /// <summary>
        /// Get default change increment</summary>
        public double DefaultChange
        {
            get { return m_defaultChange; }
        }

        /// <summary>
        /// Get large change increment</summary>
        public double LargeChange
        {
            get { return m_largeChange; }
        }

        /// <summary>
        /// Get small change increment</summary>
        public double SmallChange
        {
            get { return m_smallChange; }
        }

        /// <summary>
        /// Gets whether scale is logarithmic</summary>
        public bool IsLogarithimc
            {
            get { return m_isLogarithmic; }
        }

        private readonly double m_largeChange;
        private readonly double m_defaultChange;
        private readonly double m_smallChange;
        private readonly bool m_isLogarithmic;
    }

    /// <summary>
    /// Enum display name attribute</summary>
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
        // couldn't be set in XAML.
        private readonly string m_value;
    }

    /// <summary>
    /// Standard values attribute base for enumerable list of values</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class StandardValuesAttributeBase : Attribute
    {
        /// <summary>
        /// Get values for components</summary>
        /// <param name="components">Enumeration of components</param>
        /// <returns>Values for components</returns>
        public abstract object[] GetValues(IEnumerable components);
    }

    /// <summary>
    /// Standard values attribute for enumerable list of values</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class StandardValuesAttribute : StandardValuesAttributeBase
    {
        /// <summary>
        /// Constructor with object array</summary>
        /// <param name="values">Value array</param>
        public StandardValuesAttribute(object[] values)
        {
            Values = values;
        }

        /// <summary>
        /// Get or set value array</summary>
        public object[] Values { get; set; }

        /// <summary>
        /// Obtain value array</summary>
        /// <param name="components">Unused</param>
        /// <returns>Value array</returns>
        public override object[] GetValues(IEnumerable components)
        {
            return Values;
        }
    }

    /// <summary>
    /// Group enabled attribute</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class GroupEnabledAttribute : Attribute
    {
        /// <summary>
        /// Constructor with array of GroupEnables</summary>
        /// <param name="enables">GroupEnables array</param>
        public GroupEnabledAttribute(GroupEnables[] enables)
        {
            GroupEnables = enables;
        }

        /// <summary>
        ///  Constructor with group name and string values</summary>
        /// <param name="groupName">Group name</param>
        /// <param name="stringValues">String values</param>
        public GroupEnabledAttribute(string groupName, string stringValues)
        {
            string[] values = stringValues.Split(',');
            GroupEnables = new[] { new GroupEnables(groupName, values) };
        }

        /// <summary>
        /// GroupEnables array</summary>
        /// <remarks>This is a work around for the fact that the MemberDescriptor base class
        /// seems to filter out attributes of matching types which prevents multiple
        /// GroupEnabledAttributes from being placed on a PropertyDescriptor</remarks>
        public readonly GroupEnables[] GroupEnables;
    }

    /// <summary>
    /// Dependency attribute for updating a node with dependencies on a property of other nodes</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DependencyAttribute : Attribute
    {
        /// <summary>
        /// Get or set array of dependency descriptor strings</summary>
        public string[] DependencyDescriptors { get; set; }
        /// <summary>
        /// Get or set array of dependency descriptor strings</summary>
        public string[] DependencyDescriptor { get; set; }

        /// <summary>
        /// Constructor with dependency descriptor string</summary>
        /// <param name="dependencyDescriptor">Dependency descriptor string</param>
        public DependencyAttribute(string dependencyDescriptor)
        {
            DependencyDescriptor = new []{ dependencyDescriptor};
        }

        /// <summary>
        /// Constructor with dependency descriptor strings</summary>
        /// <param name="dependencyDescriptors">Dependency descriptor strings</param>
        public DependencyAttribute(string[] dependencyDescriptors)
        {
            DependencyDescriptors = dependencyDescriptors;
        }
    }

    /// <summary>
    /// Class for enabling a node with dependencies on a property of other nodes</summary>
    public class GroupEnables
    {
        /// <summary>
        /// Constructor with group name and string values</summary>
        /// <param name="groupName">Group name</param>
        /// <param name="stringValues">String values</param>
        public GroupEnables(string groupName, string[] stringValues)
        {
            GroupName = groupName;
            StringValues = stringValues; 
        }
        /// <summary>
        /// Get group name</summary>
        public string GroupName { get; private set; }
        /// <summary>
        /// Get string values</summary>
        public string[] StringValues { get; private set; }
        /// <summary>
        /// Get or set cache of typed values</summary>
        public object[] Values { get; set; }
    }

    /// <summary>
    /// Display units attribute</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayUnitsAttribute : Attribute
    {
        /// <summary>
        /// Constructor with units</summary>
        /// <param name="units">Units name</param>
        public DisplayUnitsAttribute(string units)
        {
            Units = units;
        }

        /// <summary>
        /// Get or set units name</summary>
        public string Units { get; set; }
    }

    /// <summary>
    /// String filter attribute</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class StringFilterAttribute : Attribute
    {
        /// <summary>
        /// Constructor with filter string</summary>
        /// <param name="filter">Filter string</param>
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

        /// <summary>
        /// Get or set filter string</summary>
        public IStringValueFilter Filter { get; set; }
    }

    /// <summary>
    /// Image list attribute</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ImageListAttribute : Attribute
    {
        /// <summary>
        /// Constructor with image keys</summary>
        /// <param name="imageKeys">Image keys array</param>
        public ImageListAttribute(object[] imageKeys)
        {
            ImageKeys = imageKeys;
        }

        /// <summary>
        /// Get or set image keys array</summary>
        public object[] ImageKeys { get; set; }
    }

    /// <summary>
    /// Attribute used to tag a property which is to be used as the title/header for an object,
    /// e.g., in property grid or property lists</summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class HeaderPropertyAttribute : Attribute
    {
    }
}
