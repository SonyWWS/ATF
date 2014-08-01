//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Sce.Atf.Wpf.ValueConverters;

namespace Sce.Atf.Wpf.Markup
{
    /// <summary>
    /// XAML markup extension to provide data binding for a resource key</summary>
    public class ResourceKeyBinding : MarkupExtension
    {
        /// <summary>
        /// Gets and sets the data binding</summary>
        public Binding Binding { get; set; }

        /// <summary>
        /// Returns an object that is provided as the value of the target property for this markup extension. </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension</param>
        /// <returns>The object value to set on the property where the extension is applied</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            object value = null;

            if (Binding != null)
            {
                if (Binding.Converter == null)
                {
                    var target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

                    // If the TargetObject is a DependencyObject, then we are binding to a real thing here.
                    // Otherwise it will be a SharedDependencyProperty which is a template (not a real thing).
                    // In that case we return this markup extension so it can provide a future value.
                    if (target.TargetObject is DependencyObject)
                    {
                        var elem = target.TargetObject as FrameworkElement;
                        Binding.Converter = new ResourceLookupConverter(elem);
                        value = Binding.ProvideValue(serviceProvider);
                    }
                    else
                    {
                        value = this;
                    }
                }
                else
                {
                    value = Binding.ProvideValue(serviceProvider);
                }
            }

            return value;
        }
    }
}