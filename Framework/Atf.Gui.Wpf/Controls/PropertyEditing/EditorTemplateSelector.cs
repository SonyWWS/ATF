//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Provides a way to select a value editor</summary>
    public class EditorTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Get default selector</summary>
        public static EditorTemplateSelector Default
        {
            get { return s_default ?? (s_default = new EditorTemplateSelector(new ObservableCollection<ValueEditor>())); }
        }
        private static EditorTemplateSelector s_default;

        /// <summary>
        /// Get default non-editing selector</summary>
        public static EditorTemplateSelector DefaultNonEdit
        {
            get { return s_defaultNonEdit ?? (s_defaultNonEdit = new EditorTemplateSelector(new ObservableCollection<ValueEditor>()) { SelectNonEditingTemplates = true }); }
        }
        private static EditorTemplateSelector s_defaultNonEdit;

        /// <summary>
        /// Constructor with editors</summary>
        /// <param name="editors">Enumeration of editors</param>
        public EditorTemplateSelector(IEnumerable<ValueEditor> editors)
        {
            Editors = editors;
        }

        /// <summary>
        /// Setting this flag causes selection of special non editing templates if available
        /// This is used for the data grid when cells are not in edit mode
        /// </summary>
        public bool SelectNonEditingTemplates { get; set; }

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

                    if (SelectNonEditingTemplates)
                        return customEditor.GetNonEditingTemplate(node, container);
                    return customEditor.GetTemplate(node, container);
                }

                // If this fails, try to get an editor from the editors list
                if(Editors != null)
                {
                    foreach (ValueEditor editor in Editors)
                    {
                        if (editor.CanEdit(node))
                        {
                            if (SelectNonEditingTemplates)
                                return editor.GetNonEditingTemplate(node, container);

                            if (editor.UsesCustomContext && node.EditorContext == null)
                                node.EditorContext = editor.GetCustomContext(node);
                            else if (!editor.UsesCustomContext)
                                node.EditorContext = null;

                            return editor.GetTemplate(node, container);
                        }
                    }
                }

                // If this fails then use a default template
                return GetDefaultTemplate(node, container);
            }

            return new DataTemplate();
        }

        private DataTemplate GetDefaultTemplate(PropertyNode node, DependencyObject container)
        {
            if (node != null)
            {
                // If node is readonly Or SelectNonEditingTemplates is set then
                // get a readonly template
                bool isWriteable = node.IsWriteable && !SelectNonEditingTemplates;

                object key = GetEditorKey(node, isWriteable, EditorKeyType.Template);
                if (key != null)
                {
                    var fwe = container as FrameworkElement;
                    if (fwe != null)
                        return fwe.FindResource(key) as DataTemplate;
                    return Application.Current.FindResource(key) as DataTemplate;
                }
            }
            return null;
        }

        private static object GetEditorKey(PropertyNode node, bool editable, EditorKeyType keyType)
        {
            Type propertyType = node.PropertyType;

            if (propertyType != null)
            {
                if (propertyType == typeof(bool))
                {
                    if (keyType == EditorKeyType.Style)
                        return PropertyGrid.BoolEditorStyleKey;
                    return PropertyGrid.BoolEditorTemplateKey;
                }
                
                if (editable)
                {
                    if (node.StandardValues != null)
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
