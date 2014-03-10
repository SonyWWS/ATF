//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using Sce.Atf.Wpf.Models;
using System.ComponentModel;

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
        private static SliderValueEditor s_instance = new SliderValueEditor();

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
            if(node != null)
                return new SliderValueEditorContext(node);
            return null;
        }

        /// <summary>
        /// Gets the DataTemplate resource for SliderValueEditor</summary>
        /// <param name="node">PropertyNode (unused)</param>
        /// <returns>DataTemplate resource for SliderValueEditor</returns>
        public override DataTemplate GetTemplate(PropertyNode node)
        {
            return Application.Current.FindResource(TemplateKey) as DataTemplate;
        }
       
    }

    /// <summary>
    /// Slider value editor context</summary>
    public class SliderValueEditorContext : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="node">PropertyNode</param>
        public SliderValueEditorContext(PropertyNode node)
        {
            m_node = node;
            m_node.ValueChanged += new EventHandler(Node_ValueChanged);
            Update();

            NumberRangesAttribute numberRange 
                = m_node.Descriptor.Attributes[typeof(NumberRangesAttribute)] as NumberRangesAttribute;

            if (numberRange != null)
            {
                Max = numberRange.Maximum;
                Min = numberRange.Minimum;
            }
            else
            {
                Max = double.MaxValue;
                Min = double.MinValue;
            }

            NumberIncrementsAttribute numberInc
                = m_node.Descriptor.Attributes[typeof(NumberIncrementsAttribute)] as NumberIncrementsAttribute;

            if (numberInc != null)
            {
                SmallChange = numberInc.SmallChange;
                LargeChange = numberInc.LargeChange;
            }
            else
            {
                SmallChange = 1;
                LargeChange = 10;
            }

            NumberFormatAttribute numberFormat
              = m_node.Descriptor.Attributes[typeof(NumberFormatAttribute)] as NumberFormatAttribute;

            if (numberFormat != null)
            {
                FormatString = numberFormat.FormatString;
            }
            else
            {
                FormatString = "{0:0.00}";
            }
        }

        /// <summary>
        /// Gets or sets slider value. This value is bound to the slider control.</summary>
        public double DoubleValue
        {
            get 
            {
                return m_doubleValue;
            }
            set
            {
                if (m_doubleValue != value)
                {
                    m_doubleValue = value;

                    Type type = m_node.Descriptor.PropertyType;
                    if (type == typeof(Int16))
                        m_node.Value = Convert.ToInt16(m_doubleValue);
                    else if (type == typeof(Int32))
                        m_node.Value = Convert.ToInt32(m_doubleValue);
                    else if (type == typeof(Single))
                        m_node.Value = Convert.ToSingle(m_doubleValue);
                    else
                        throw new NotImplementedException("Type conversion not yet supported");
                }
            }
        }

        /// <summary>
        /// Gets slider maximum value</summary>
        public double Max { get; private set; }

        /// <summary>
        /// Gets slider minimum value</summary>
        public double Min { get; private set; }

        /// <summary>
        /// Gets the value to be added to or subtracted from slider value when the slider is moved a small distance</summary>
        public double SmallChange { get; private set; }

        /// <summary>
        /// Gets the value to be added to or subtracted from slider value when the slider is moved a large distance</summary>
        public double LargeChange { get; private set; }

        /// <summary>
        /// Gets format string</summary>
        public string FormatString { get; private set; }

        private void Node_ValueChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            try
            {
                double value = Convert.ToDouble(m_node.Value);
                if (value != m_doubleValue)
                {
                    m_doubleValue = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("DoubleValue"));
                }
            }
            catch (FormatException) { }
            catch (InvalidCastException) { }
            catch (OverflowException) { }
        }

        private readonly PropertyNode m_node;
        private double m_doubleValue;
    }
}
