//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Class for editing a value with multi-line TextBox</summary>
    public class MultiLineTextValueEditor : ValueEditor
    {
        /// <summary>
        /// Class name resource key</summary>
        public static readonly ResourceKey TemplateKey = new ComponentResourceKey(typeof(MultiLineTextValueEditor), "MultiLineTextValueEditorTemplate");

        /// <summary>
        /// Gets the DataTemplate resource for MultiLineTextValueEditor</summary>
        /// <param name="node">PropertyNode (unused)</param>
        /// <param name="container">DependencyObject container of TextBox</param>
        /// <returns>DataTemplate resource for MultiLineTextValueEditor</returns>
        public override DataTemplate GetTemplate(PropertyNode node, DependencyObject container)
        {
            return FindResource<DataTemplate>(TemplateKey, container);
        }
    }
}
