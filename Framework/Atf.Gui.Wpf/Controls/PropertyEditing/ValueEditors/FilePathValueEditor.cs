//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Custom property editor for editing file paths</summary>
    public class FilePathValueEditor : ValueEditor
    {
        /// <summary>
        /// Resource key for the template to use for the FilePathValueEditor</summary>
        public static readonly ResourceKey TemplateKey = 
            new ComponentResourceKey(typeof(FilePathValueEditor), "FilePathValueEditorTemplate");

        /// <summary>
        /// Gets whether the editor uses a custom context</summary>
        public override bool UsesCustomContext { get { return true; } }

        /// <summary>
        /// Gets a new custom context for the node</summary>
        /// <param name="node">Property node to get the context for</param>
        /// <returns>The new context, or null if the node is null</returns>
        public override object GetCustomContext(PropertyNode node)
        {
            return node != null ? new FilePathValueEditorContext(node, this) : null;
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

        /// <summary>
        /// Gets and sets the filter for the file path</summary>
        public string Filter { get; set; }

        /// <summary>
        /// Gets and sets the default extension for the file path</summary>
        public string DefaultExtension { get; set; }
    }

    /// <summary>
    /// Editing context for the FilePathValueEditor</summary>
    public class FilePathValueEditorContext : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="node">The node the context will be editing</param>
        /// <param name="editor">The editor, which provides the Filter and DefaultExtension information</param>
        public FilePathValueEditorContext(PropertyNode node, FilePathValueEditor editor)
        {
            m_node = node;
            
            Filter = editor.Filter;
            DefaultExtension = editor.DefaultExtension;
        }

        /// <summary>
        /// Gets and sets the filter for the file path</summary>
        public string Filter { get; set; }

        /// <summary>
        /// Gets and sets the default extension for the file path</summary>
        public string DefaultExtension { get; set; }

        /// <summary>
        /// Gets and sets the value of the property</summary>
        public object Value
        {
            get { return m_node.Value; }
            set { m_node.Value = value; }
        }

        private readonly PropertyNode m_node;
    }
}
