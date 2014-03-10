//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Provides a way to select a value editor</summary>
    public class EditorTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Constructor with editors</summary>
        /// <param name="editors">Enumeration of available editors</param>
        public EditorTemplateSelector(IEnumerable<ValueEditor> editors)
        {
            Editors = editors;
        }

        /// <summary>
        /// Gets the available editors</summary>
        public IEnumerable<ValueEditor> Editors { get; private set; }

        /// <summary>
        /// Selects a DataTemplate appropriate for a PropertyNode</summary>
        /// <param name="item">Item (PropertyNode) to select editor for</param>
        /// <param name="container">Item's container (not used)</param>
        /// <returns>DataTemplate appropriate for the PropertyNode</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var node = item as PropertyNode;
            if (node != null)
            {
                // Try and get custom editor from the node
                ValueEditor customEditor = node.GetCustomEditor();
                if (customEditor != null)
                {
                    if (customEditor.UsesCustomContext && node.EditorContext == null)
                        node.EditorContext = customEditor.GetCustomContext(node);
                    else if (!customEditor.UsesCustomContext)
                        node.EditorContext = null;

                    return customEditor.GetTemplate(node);
                }

                // If this fails, try to get an editor from the editors list
                if(Editors != null)
                {
                    // TODO: need to guard against editors being changed mid-enumeration
                    foreach (ValueEditor editor in Editors)
                    {
                        if (editor.CanEdit(node))
                            return editor.GetTemplate(node);
                    }
                }

                // If this fails then use a default template
                return GetDefaultTemplate(node);
            }

            return null;
        }

        private static DataTemplate GetDefaultTemplate(PropertyNode node)
        {
            if (node != null)
            {
                object key = GetEditorKey(node, node.IsWriteable, EditorKeyType.Template);
                if (key != null)
                    return Application.Current.FindResource(key) as DataTemplate;
            }
            return null;
        }

        private static object GetEditorKey(PropertyNode node, bool editable, EditorKeyType keyType)
        {
            Type propertyType = node.PropertyType;
            object obj2 = node.Value;

            if (editable && propertyType != null)
            {
                if (propertyType == typeof(bool))
                {
                    if (keyType == EditorKeyType.Style)
                        return PropertyGrid.BoolEditorStyleKey;
                    return PropertyGrid.BoolEditorTemplateKey;
                }
                if (((propertyType != null) && node.StandardValues != null))
                {
                    if (keyType == EditorKeyType.Style)
                        return PropertyGrid.ComboEditorStyleKey;
                    return PropertyGrid.ComboEditorTemplateKey;
                }
                if (s_simpleTypes.Contains(propertyType))
                {
                    if (keyType == EditorKeyType.Style)
                        return PropertyGrid.DefaultTextEditorStyleKey;
                    return PropertyGrid.DefaultTextEditorTemplateKey;
                }
            }

            if (keyType == EditorKeyType.Style)
                return PropertyGrid.ReadOnlyStyleKey;
            return PropertyGrid.ReadOnlyTemplateKey;
        }

        private enum EditorKeyType
        {
            Style,
            Template
        }

        private static List<Type> s_simpleTypes = new List<Type>(new Type[] { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(short), typeof(ushort), typeof(byte), typeof(sbyte), typeof(float), typeof(double), typeof(decimal), typeof(string) });
    }
}
