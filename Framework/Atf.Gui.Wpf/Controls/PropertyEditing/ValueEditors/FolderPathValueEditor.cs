//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Data bound property editor that uses a template to edit a folder path</summary>
    public class FolderPathValueEditor : ValueEditor
    {
        /// <summary>
        /// Resource key for the template</summary>
        public static readonly ResourceKey TemplateKey =
            new ComponentResourceKey(typeof(FilePathValueEditor), "FolderPathValueEditorTemplate");

        /// <summary>
        /// Gets the template resource</summary>
        /// <param name="node">unused</param>
        /// <param name="container">DependencyObject to query for the resource</param>
        /// <returns>The template</returns>
        public override DataTemplate GetTemplate(PropertyNode node, DependencyObject container)
        {
            return FindResource<DataTemplate>(TemplateKey, container);
        }
    }
}
