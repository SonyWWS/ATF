//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Numeric format string attribute</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NumberFormatAttribute : Attribute
    {
        private string formatString;
        private int? maxPrecision;
        private double? scale;

        /// <summary>
        /// Constructor</summary>
        public NumberFormatAttribute()
        {
            this.formatString = null;
            this.maxPrecision = null;
            this.scale = null;
        }

        /// <summary>
        /// Constructor with format string</summary>
        /// <param name="formatString">Format string</param>
        public NumberFormatAttribute(string formatString)
        {
            this.formatString = formatString;
            this.maxPrecision = null;
            this.scale = null;
        }

        /// <summary>
        /// Constructor with format string</summary>
        /// <param name="formatString">Format string</param>
        /// <param name="maxPrecision">Maximum precision</param>
        /// <param name="scale">Scale</param>
        public NumberFormatAttribute(string formatString, int maxPrecision, double scale)
        {
            this.formatString = formatString;
            this.maxPrecision = maxPrecision;
            this.scale = scale;
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
            NumberFormatAttribute attribute = obj as NumberFormatAttribute;
            if (attribute != null)
            {
                if ((attribute.formatString == this.formatString) && (attribute.maxPrecision == this.maxPrecision))
                {
                    return (attribute.scale == this.scale);
                }
            }
            return false;
        }

        /// <summary>
        /// Gets hash code for instance</summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return ((((this.formatString != null) ? this.formatString.GetHashCode() : 0) ^ this.maxPrecision.GetHashCode()) ^ this.scale.GetHashCode());
        }

        /// <summary>
        /// Gets format string</summary>
        public string FormatString
        {
            get
            {
                return this.formatString;
            }
        }

        /// <summary>
        /// Gets maximum precision</summary>
        public int? MaxPrecision
        {
            get
            {
                return this.maxPrecision;
            }
        }

        /// <summary>
        /// Gets scale</summary>
        public double? Scale
        {
            get
            {
                return this.scale;
            }
        }
    }

    /// <summary>
    /// Numeric range attribute</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NumberRangesAttribute : Attribute
    {
        private double maximum;
        private double minimum;

        /// <summary>
        /// Constructor</summary>
        public NumberRangesAttribute()
        {
            this.minimum = double.MinValue;
            this.maximum = double.MaxValue;
        }

        /// <summary>
        /// Constructor with range</summary>
        /// <param name="minimum">Minimum value</param>
        /// <param name="maximum">Maximum value</param>
        public NumberRangesAttribute(double minimum, double maximum)
        {
            this.minimum = minimum;
            this.maximum = maximum;
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
            NumberRangesAttribute attribute = obj as NumberRangesAttribute;
            if (attribute != null)
            {
                if (((attribute.minimum == this.minimum)) && ((attribute.maximum == this.maximum)))
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
            return ((((this.minimum.GetHashCode()) ^ this.maximum.GetHashCode())));
        }

        /// <summary>
        /// Gets maximum value</summary>
        public double Maximum
        {
            get
            {
                return this.maximum;
            }
        }

        /// <summary>
        /// Gets minimum value</summary>
        public double Minimum
        {
            get
            {
                return this.minimum;
            }
        }
    }

    /// <summary>
    /// Numeric small/large increment attribute</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NumberIncrementsAttribute : Attribute
    {
        private double largeChange;
        private double smallChange;

        /// <summary>
        /// Constructor</summary>
        public NumberIncrementsAttribute()
        {
            this.smallChange = 1;
            this.largeChange = 10;
        }

        /// <summary>
        /// Constructor with increments</summary>
        /// <param name="smallChange">Small change increment</param>
        /// <param name="largeChange">Large change increment</param>
        public NumberIncrementsAttribute(double smallChange, double largeChange)
        {
            this.smallChange = smallChange;
            this.largeChange = largeChange;
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
            NumberIncrementsAttribute attribute = obj as NumberIncrementsAttribute;
            if (attribute != null)
            {
                if ((attribute.smallChange == this.smallChange))
                {
                    return (attribute.largeChange == this.largeChange);
                }
            }
            return false;
        }

        /// <summary>
        /// Gets hash code for instance</summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return (this.smallChange.GetHashCode() ^ this.largeChange.GetHashCode());
        }

        /// <summary>
        /// Gets or sets the value to be added to or subtracted from value for a large change</summary>
        public double LargeChange
        {
            get
            {
                return this.largeChange;
            }
        }

        /// <summary>
        /// Gets or sets the value to be added to or subtracted from value for a small change</summary>
        public double SmallChange
        {
            get
            {
                return this.smallChange;
            }
        }
    }
}
