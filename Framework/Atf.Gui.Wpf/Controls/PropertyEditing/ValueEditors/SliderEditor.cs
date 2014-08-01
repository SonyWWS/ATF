//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Input;
using Sce.Atf.Wpf.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Class for editing a value with a slider control</summary>
    public class SliderValueEditor : ValueEditor
    {
        /// <summary>
        /// Gets this instance</summary>
        public static SliderValueEditor Instance
        {
            get { return s_instance; }
        }

        /// <summary>
        /// Resource key used in XAML files for the SliderValueEditorTemplate class</summary>
        public static readonly ResourceKey TemplateKey = new ComponentResourceKey(typeof(SliderValueEditor), "SliderValueEditorTemplate");

        /// <summary>
        /// Gets whether this editor uses a custom context</summary>
        public override bool UsesCustomContext { get { return true; } }

        /// <summary>
        /// Gets custom context for PropertyNode</summary>
        /// <param name="node">PropertyNode </param>
        /// <returns>New custom context for PropertyNode</returns>
        public override object GetCustomContext(PropertyNode node) 
        {
            return node != null ? new SliderValueEditorContext(node) : null;
        }

        /// <summary>
        /// Gets the DataTemplate resource for SliderValueEditor</summary>
        /// <param name="node">PropertyNode (unused)</param>
        /// <returns>DataTemplate resource for SliderValueEditor</returns>
        public override DataTemplate GetTemplate(PropertyNode node, DependencyObject container)
        {
            return FindResource<DataTemplate>(TemplateKey, container);
        }

        private static SliderValueEditor s_instance = new SliderValueEditor();
    }

    /// <summary>
    /// Slider value editor context</summary>
    public class SliderValueEditorContext : NotifyPropertyChangedBase, IDisposable
    {
        /// <summary>
        /// Gets and sets the property node</summary>
        public PropertyNode Node { get; private set; }

        /// <summary>
        /// Constructor</summary>
        /// <param name="node">Property node</param>
        public SliderValueEditorContext(PropertyNode node)
        {
            Node = node;
            Node.ValueChanged += OnNodeOnValueChanged;

            var numberRange = Node.Descriptor.Attributes[typeof(NumberRangesAttribute)] as NumberRangesAttribute;
            if (numberRange != null)
            {
                m_max = numberRange.Maximum;
                m_min = numberRange.Minimum;
                m_center = numberRange.Center;
                m_hardMin = numberRange.HardMinimum;
                m_hardMax = numberRange.HardMaximum;
            }
            else
            {
                var dataRange = Node.Descriptor.Attributes[typeof(RangeAttribute)] as RangeAttribute;
                if (dataRange != null)
                {
                    m_max = Convert.ToDouble(dataRange.Minimum);
                    m_min = Convert.ToDouble(dataRange.Maximum);
                    m_center = (m_max - m_min) / 2.0;
                }
            }

            var defaultValue = Node.Descriptor.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;
            if (defaultValue != null)
            {
                if (defaultValue.Value.GetType().IsValueType)
                    m_default = Convert.ToDouble(defaultValue.Value);
            }

            var numberInc = Node.Descriptor.Attributes[typeof(NumberIncrementsAttribute)] as NumberIncrementsAttribute;
            if (numberInc != null)
            {
                m_smallChange = numberInc.SmallChange;
                m_defaultChange = numberInc.DefaultChange;
                m_largeChange = numberInc.LargeChange;
                m_isLogarithmic = numberInc.IsLogarithimc;
            }

            var units = Node.Descriptor.Attributes[typeof(DisplayUnitsAttribute)] as DisplayUnitsAttribute;
            if (units != null)
            {
                m_units = units.Units;
            }

            var numberFormat = Node.Descriptor.Attributes[typeof(NumberFormatAttribute)] as NumberFormatAttribute;
            if (numberFormat != null)
            {
                m_scale = numberFormat.Scale ?? 1.0;
                m_formatString = numberFormat.FormatString;
            }

            Update();
        }

        /// <summary>
        /// Gets the ICommand to commit the edit in progress</summary>
        public ICommand CommitEditCommand
        {
            get { return m_commitEditCommand ?? (m_commitEditCommand = new DelegateCommand(CommitEdit)); }
        }

        /// <summary>
        /// Commit the edit in progress</summary>
        public virtual void CommitEdit()
        {
            if (!NumericUtil.AreClose(m_doubleValue, m_cachedValue))
                {
                    m_doubleValue = m_cachedValue;

                    Type type = Node.Descriptor.PropertyType;
                    if (type == typeof(Int16))
                        Node.Value = Convert.ToInt16(m_doubleValue);
                    else if (type == typeof(UInt16))
                        Node.Value = Convert.ToUInt16(m_doubleValue);
                    else if (type == typeof(Int32))
                        Node.Value = Convert.ToInt32(m_doubleValue);
                    else if (type == typeof(UInt32))
                        Node.Value = Convert.ToUInt32(m_doubleValue);
                    else if (type == typeof(Int64))
                        Node.Value = Convert.ToInt64(m_doubleValue);
                    else if (type == typeof(UInt64))
                        Node.Value = Convert.ToUInt64(m_doubleValue);
                    else if (type == typeof(Single))
                        Node.Value = Convert.ToSingle(m_doubleValue);
                    else if (type == typeof(Double))
                        Node.Value = m_doubleValue;
                    else
                        throw new NotImplementedException("Type conversion not yet supported");
                }
        }

        /// <summary>
        /// Gets the ICommand to cancel the edit in progress</summary>
        public ICommand CancelEditCommand
        {
            get { return m_cancelEditCommand ?? (m_cancelEditCommand = new DelegateCommand(CancelEdit)); }
        }

        /// <summary>
        /// Does nothing</summary>
        public virtual void CancelEdit()
        {
        }

        /// <summary>
        /// Gets and sets the slider's value as a double</summary>
        public double DoubleValue
        {
            get { return m_cachedValue; }
            set { m_cachedValue = value; }
        }

        /// <summary>
        /// Gets the slider maximum value</summary>
        public double Max
        {
            get { return m_max; }
        }

        /// <summary>
        /// Gets the slider minimum value</summary>
        public double Min
        {
            get { return m_min; }
        }

        /// <summary>
        /// Gets the default value for the slider</summary>
        public double Default
        {
            get { return m_default; }
        }

        /// <summary>
        /// Gets the hard minimum for the slider</summary>
        public double HardMin
        {
            get { return m_hardMin; }
        }

        /// <summary>
        /// Gets the hard maximum for the slider</summary>
        public double HardMax
        {
            get { return m_hardMax; }
        }

        /// <summary>
        /// Gets the center value for the slider</summary>
        public double Center
        {
            get { return m_center; }
        }

        /// <summary>
        /// Gets the scale factor for the slider</summary>
        public double Scale
        {
            get { return m_scale; }
        }

        /// <summary>
        /// Gets whether the slider values are logarithmic</summary>
        public bool IsLogarithmic
        {
            get { return m_isLogarithmic; }
        }

        /// <summary>
        /// Gets the small change value for the slider</summary>
        public double SmallChange
        {
            get { return m_smallChange; }
        }

        /// <summary>
        /// Gets the default change value for the slider</summary>
        public double DefaultChange
        {
            get { return m_defaultChange; }
        }

        /// <summary>
        /// Gets the large change value for the slider</summary>
        public double LargeChange
        {
            get { return m_largeChange; }
        }

        /// <summary>
        /// Gets the format string to use in displaying the slider labels</summary>
        public string FormatString
        {
            get { return m_formatString; }
        }

        /// <summary>
        /// Gets the units for the slider</summary>
        public string Units
        {
            get { return m_units; }
        }

        /// <summary>
        /// Gets and sets whether to show the slider</summary>
        public bool ShowSlider
        {
            get { return m_showSlider; }
            set
            {
                if (m_showSlider != value)
                {
                    m_showSlider = value;
                    RaisePropertyChanged("ShowSlider");
                }
            }
        }

        #region IDisposable members

        /// <summary>
        /// Dispose of system resources</summary>
        /// <param name="disposing">not used</param>
        public virtual void Dispose(bool disposing)
        {
            if (Node != null)
                Node.ValueChanged -= OnNodeOnValueChanged;
        }

        /// <summary>
        /// Dispose of system resources</summary>
        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        #endregion

        /// <summary>
        /// Update the value and slider state</summary>
        protected virtual void Update()
        {
            try
            {
                bool showSlider = true;

                if (Node.Value == null)
                {
                    showSlider = false;
                }
                else
                {
                    double value = Convert.ToDouble(Node.Value);
                    if (!NumericUtil.AreClose(m_doubleValue, value))
                    {
                        m_doubleValue = value;
                        m_cachedValue = value;
                        RaisePropertyChanged("DoubleValue");
                    }
                }

                ShowSlider = showSlider;
            }
            catch (FormatException) { }
            catch (InvalidCastException) { }
            catch (OverflowException) { }
        }

        private void OnNodeOnValueChanged(object s, EventArgs e)
        {
            Update();
        }

        private double m_cachedValue;
        private double m_doubleValue;
        private ICommand m_commitEditCommand;
        private ICommand m_cancelEditCommand;
        private readonly double m_max = double.MaxValue;
        private readonly double m_min = double.MinValue;
        private readonly double m_center = double.NaN;
        private readonly double m_default = double.NaN;
        private readonly double m_hardMin = double.NaN;
        private readonly double m_hardMax = double.NaN;
        private readonly double m_scale = 1.0;
        private readonly bool m_isLogarithmic;
        private readonly double m_smallChange = 0.1;
        private readonly double m_defaultChange = 1.0;
        private readonly double m_largeChange = 10.0;
        private readonly string m_formatString = "0.#";
        private readonly string m_units;
        private bool m_showSlider;

    }
}
