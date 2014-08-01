//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Property editor for an enumerable list of values</summary>
    public class StandardValuesEditor : ValueEditor
    {
        /// <summary>
        /// Static instance of the StandardValuesEditor</summary>
        public static StandardValuesEditor Instance
        {
            get { return s_instance; }
        }

        /// <summary>
        /// Gets whether the editor uses a custom context. Always returns true.</summary>
        public override bool UsesCustomContext { get { return true; } }

        /// <summary>
        /// Gets the custom context</summary>
        /// <param name="node">Node to edit</param>
        /// <returns>A new StandardValuesEditorContext for the node</returns>
        public override object GetCustomContext(PropertyNode node) 
        {
            return (node == null) ? null : new StandardValuesEditorContext(node);
        }

        /// <summary>
        /// Gets the template used for the specified object</summary>
        /// <param name="node">The node being edited</param>
        /// <param name="container">The dependency object</param>
        /// <returns>The item's template</returns>
        public override DataTemplate GetTemplate(PropertyNode node, DependencyObject container)
        {
            return FindResource<DataTemplate>(PropertyGrid.StandardValuesEditorTemplateKey, container);
        }

        private readonly static StandardValuesEditor s_instance = new StandardValuesEditor();
    }

    /// <summary>
    /// Editing context for an enumerable list of values</summary>
    public class StandardValuesEditorContext : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="node">The node to edit</param>
        public StandardValuesEditorContext(PropertyNode node)
        {
            SetNode(node);
            m_attribute = m_node.Descriptor.Attributes[typeof(StandardValuesAttributeBase)] as StandardValuesAttributeBase;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="node">The node to edit</param>
        /// <param name="standardValues">The list of values</param>
        public StandardValuesEditorContext(PropertyNode node, IEnumerable<object> standardValues)
        {
            SetNode(node);
            m_standardValues = standardValues;
        }

        /// <summary>
        /// Gets the node being edited</summary>
        public PropertyNode Node { get { return m_node; } }

        /// <summary>
        /// Gets the list of values</summary>
        public IEnumerable<object> StandardValues
        {
            get { return m_attribute == null ? m_standardValues : m_attribute.GetValues(m_node.Instances); }
        }

        /// <summary>
        /// Gets the image list</summary>
        public IEnumerable<object> ImageList { get { return m_imageList; } }

        private void SetNode(PropertyNode node)
        {
            m_node = node;
            var attribute = m_node.Descriptor.Attributes[typeof(ImageListAttribute)] as ImageListAttribute;
            if (attribute != null)
            {
                m_imageList = attribute.ImageKeys;
            }
        }

        private PropertyNode m_node;
        private IEnumerable<object> m_standardValues;
        private IEnumerable<object> m_imageList;
        private StandardValuesAttributeBase m_attribute;
    }
}
