//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Helper class to select a template for ToolBarItems</summary>
    public class ToolBarItemTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets the static instance</summary>
        public static ToolBarItemTemplateSelector Instance
        {
            get { return s_instance ?? (s_instance = new ToolBarItemTemplateSelector()); }
        }

        /// <summary>
        /// Gets t</summary>
        /// <param name="item">The ICommandItem for this item</param>
        /// <param name="container">The element to get the template for</param>
        /// <returns>If there is a custom template for this type, it is returned. If no custom template is
        /// found but item is non-null, returns the standard ATF ToolBarItemTemplate. Otherwise, returns null.</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (element != null)
            {
                // Try and find a custom template for this type
                var dataTemplate = element.TryFindResource(element.GetType()) as DataTemplate;
                if (dataTemplate != null)
                    return dataTemplate;

                // If this fails then use the default ICommandItem template
                var commandItem = item as ICommandItem;
                if (commandItem != null)
                    return element.FindResource(Resources.ToolBarItemTemplateKey) as DataTemplate;
            }

            return null;
        }

        private static ToolBarItemTemplateSelector s_instance;
    }
}
