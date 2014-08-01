//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Slider property editor with a range of allowable values</summary>
    public class RangeSliderValueEditor : ValueEditor
    {
        /// <summary>
        /// Gets the instance of the RangeSliderValueEditor</summary>
        public static RangeSliderValueEditor Instance
        {
            get { return s_instance; }
        }

        /// <summary>
        /// Resource key for the template</summary>
        public static readonly ResourceKey TemplateKey = new ComponentResourceKey(typeof(RangeSliderValueEditor),
                                                                                  "RangeSliderValueEditorTemplate");

        /// <summary>
        /// Gets whether the slider uses a custom context</summary>
        public override bool UsesCustomContext
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a new custom context for the node</summary>
        /// <param name="node">Property node to get the context for</param>
        /// <returns>The new context, or null if the node is null</returns>
        public override object GetCustomContext(PropertyNode node)
        {
            return node != null ? new RangeSliderValueEditorContext(node) : null;
        }

        /// <summary>
        /// Gets the container's template resource as defined by TemplateKey</summary>
        /// <param name="node">unused</param>
        /// <param name="container">The container to get the template from</param>
        /// <returns>The template</returns>
        public override DataTemplate GetTemplate(PropertyNode node, DependencyObject container)
        {
            return FindResource<DataTemplate>(TemplateKey, container);
        }

        private static RangeSliderValueEditor s_instance = new RangeSliderValueEditor();
    }

    /// <summary>
    /// Editing context for RangeSliderValueEditor</summary>
    public class RangeSliderValueEditorContext : SliderValueEditorContext
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="node">The node the context will be editing</param>
        public RangeSliderValueEditorContext(PropertyNode node)
            : base(node)
        {
            RangeStart = Min;
            RangeStop = Max;
            RangeEnabled = true;
        }

        /// <summary>
        /// Commits the range value changes</summary>
        public override void CommitEdit()
        {
            base.CommitEdit();

            if (RangeEnabled)
            {
                OnRangeChanged();
            }
        }

        /// <summary>
        /// Gets and sets whether event notification on range changes is enabled</summary>
        public bool RangeEnabled
        {
            get { return m_rangeEnabled; }
            set
            {
                if (m_rangeEnabled != value)
                {
                    m_rangeEnabled = value;
                    RaisePropertyChanged("RangeEnabled");
                }
            }
        }

        /// <summary>
        /// Gets whether relative mode is enabled</summary>
        public bool RelativeModeEnabled { get; private set; }

        /// <summary>
        /// Gets and sets the start of the allowed range</summary>
        public double RangeStart
        {
            get  { return m_rangeStart; }
            set
            {
                if (!NumericUtil.AreClose(m_rangeStart, value))
                {
                    m_rangeStart = value;
                    RaisePropertyChanged("RangeStart");
                }
            }
        }

        /// <summary>
        /// Gets and sets the end of the allowed range</summary>
        public double RangeStop
        {
            get { return m_rangeStop; }
            set
            {
                if (!NumericUtil.AreClose(m_rangeStop, value))
                {
                    m_rangeStop = value;
                    RaisePropertyChanged("RangeStop");
                }
            }
        }

        /// <summary>
        /// Event fired when the slider range is modified</summary>
        protected virtual void OnRangeChanged()
        {
        }

        private bool m_rangeEnabled;

        private double m_rangeStart;
        private double m_rangeStop;
    }
}
