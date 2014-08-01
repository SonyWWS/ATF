//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Wpf.ValueConverters
{
    /// <summary>
    /// DataTemplateSelector for DomNodes which checks the type metadata for a data template tag
    /// Note: the node will most likely need to be adaptable to the BindingAdapter type for this
    /// selector to be useful!
    /// </summary>
    public class DomDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var node = item.As<DomNode>();
            if (node != null)
            {
                DataTemplate template = node.Type.GetTag<DataTemplate>();
                if (template != null)
                    return template;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
