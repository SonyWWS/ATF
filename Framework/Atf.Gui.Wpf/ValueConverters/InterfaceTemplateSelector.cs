//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.ValueConverters
{
    /// <summary>
    /// WPF does not provide automatic data template selection based on interfaces (only based on concrete types).
    /// This is due to problems with multiple inheritance of interfaces.
    /// This selector ignores these problems by searching for a data template for each interface implemented on
    /// a type until one is found and then stopping.
    /// In other words, if data templates exist for multiple interfaces on a type then it will be random which is chosen!
    /// Therefore make sure that any types used with this selector only implement a single interface for which a template
    /// is available in application resources.
    /// NOTE: also uses slow reflection, so should NOT be used for large collections or performance critical areas.
    /// </summary>
    public class InterfaceTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Get InterfaceTemplateSelector</summary>
        public static InterfaceTemplateSelector Instance
        {
            get 
            {
                if (s_instance == null)
                    s_instance = new InterfaceTemplateSelector();
                return s_instance;
            }
        }
        private static InterfaceTemplateSelector s_instance;

        /// <summary>
        /// When overridden in a derived class, returns a System.Windows.DataTemplate based on custom logic</summary>
        /// <param name="item">The data object for which to select the template</param>
        /// <param name="container">The data-bound object</param>
        /// <returns>Returns a System.Windows.DataTemplate or null. The default value is null.</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // Use reflection to get interfaces
            var element = container as FrameworkElement;

            if (element != null)
            {
                foreach (var type in item.GetType().FindInterfaces(TypeFilter, null))
                {
                    var dataTemplate = element.TryFindResource(type) as DataTemplate;
                    if (dataTemplate != null)
                        return dataTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }

        private bool TypeFilter(Type m, object filterCriteria)
        {
            return true;
        }
    }
}
