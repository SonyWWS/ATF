//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Base class for editing a value with a control</summary>
    public abstract class ValueEditor : DependencyObject
    {
        /// <summary>
        /// Gets whether this editor uses a custom context</summary>
        public virtual bool UsesCustomContext
        {
            get { return false; }
        }

        /// <summary>
        /// Gets custom context for PropertyNode</summary>
        /// <param name="node">PropertyNode</param>
        /// <returns>Custom context for editor</returns>
        public virtual object GetCustomContext(PropertyNode node)
        {
            return null;
        }

        /// <summary>
        /// Can this editor edit a given PropertyNode?</summary>
        /// <param name="node">PropertyNode</param>
        /// <returns><c>True</c> if editor can edit given PropertyNode</returns>
        public virtual bool CanEdit(PropertyNode node)
        {
            return true;
        }

        /// <summary>
        /// Gets style for PropertyNode</summary>
        /// <param name="node">PropertyNode</param>
        /// <param name="container">DependencyObject container</param>
        /// <returns>Style for PropertyNode</returns>
        public virtual Style GetStyle(PropertyNode node, DependencyObject container)
        {
            return null;
        }

        /// <summary>
        /// Get the template to be used for the control</summary>
        /// <param name="node">Typically unused</param>
        /// <param name="container">DependencyObject to query for the template</param>
        /// <returns>The template</returns>
        public abstract DataTemplate GetTemplate(PropertyNode node, DependencyObject container);
        
        /// <summary>
        /// Get the template to use when the control is not in editing mode</summary>
        /// <param name="node">Not used</param>
        /// <param name="container">The DependencyObject to query for the template</param>
        /// <returns>The template</returns>
        public virtual DataTemplate GetNonEditingTemplate(PropertyNode node, DependencyObject container)
        {
            return FindResource<DataTemplate>(PropertyGrid.ReadOnlyTemplateKey, container);
        }

        /// <summary>
        /// Queries the DependencyObject for the requested resource</summary>
        /// <typeparam name="T">Type of resource desired</typeparam>
        /// <param name="key">The requested resource</param>
        /// <param name="container">The DependencyObject to query</param>
        /// <returns>The resource</returns>
        protected static T FindResource<T>(object key, DependencyObject container)
            where T : class
        {
            var fwe = container as FrameworkElement;
            if (fwe != null)
                return fwe.FindResource(key) as T;
            
            return Application.Current.FindResource(key) as T;
        }

    }

    /// <summary>
    /// Enum for specifying direction of binding - source, target or both</summary>
    public enum UpdateBindingType
    {
        Source,
        Target,
        Both
    }

    /// <summary>
    /// Utilities for ValueEditor</summary>
    public static class ValueEditorUtil
    {
        /// <summary>
        /// DependencyProperty for HandlesCommitKeys</summary>
        public static readonly DependencyProperty HandlesCommitKeysProperty = 
            DependencyProperty.RegisterAttached("HandlesCommitKeys",
                typeof(bool), typeof(ValueEditorUtil), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Execute the specified ICommand</summary>
        /// <param name="command">ICommand to execute</param>
        /// <param name="element">Command target</param>
        /// <param name="parameter">Parameter to pass to command</param>
        /// <returns><c>True</c> if the command was executed</returns>
        public static bool ExecuteCommand(ICommand command, IInputElement element, object parameter)
        {
            var routedCommand = command as RoutedCommand;
            if (routedCommand != null)
            {
                if (routedCommand.CanExecute(parameter, element))
                {
                    routedCommand.Execute(parameter, element);
                    return true;
                }
            }
            else if ((command != null) && command.CanExecute(parameter))
            {
                command.Execute(parameter);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Gets the value of the HandlesCommitKeys dependency property</summary>
        /// <param name="dependencyObject">Dependency object to query</param>
        /// <returns>Value of the property</returns>
        public static bool GetHandlesCommitKeys(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(HandlesCommitKeysProperty);
        }

        /// <summary>
        /// Sets the value of the HandlesCommitKeys dependency property</summary>
        /// <param name="dependencyObject">Dependency object to use for setting the value</param>
        /// <param name="value">Value to set</param>
        public static void SetHandlesCommitKeys(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(HandlesCommitKeysProperty, value);
        }

        /// <summary>
        /// Update the binding of a dependency property</summary>
        /// <param name="element">Element to update the binding for</param>
        /// <param name="property">Dependency property to update</param>
        /// <param name="updateType">Direction to update</param>
        public static void UpdateBinding(FrameworkElement element, DependencyProperty property, UpdateBindingType updateType)
        {
            BindingExpression bindingExpression = element.GetBindingExpression(property);
            if (bindingExpression != null)
            {
                if ((updateType == UpdateBindingType.Source) || (updateType == UpdateBindingType.Both))
                {
                    bindingExpression.UpdateSource();
                }
                
                if ((updateType == UpdateBindingType.Target) || (updateType == UpdateBindingType.Both))
                {
                    bindingExpression.UpdateTarget();
                }
            }
        }

        /// <summary>
        /// Update the binding of a dependency property</summary>
        /// <param name="element">Element to update the binding for</param>
        /// <param name="property">Dependency property to update</param>
        /// <param name="updateSource">If true, update source and target. Otherwise update target.</param>
        public static void UpdateBinding(FrameworkElement element, DependencyProperty property, bool updateSource)
        {
            UpdateBinding(element, property, updateSource ? UpdateBindingType.Both : UpdateBindingType.Target);
        }
    }
}
