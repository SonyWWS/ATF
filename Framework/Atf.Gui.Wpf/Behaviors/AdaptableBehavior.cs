//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Sce.Atf.Adaptation;
using Sce.Atf.Wpf.Controls.Adaptable;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Base adaptable behavior class.
    /// On attached, attempts to get IAdaptableControl interface on AssociatedObject and register itself. 
    /// AssociatedObject is the object to which this behavior is attached.</summary>
    /// <typeparam name="T">DependencyObject, which is an object that participates in the dependency property system</typeparam>
    public abstract class AdaptableBehavior<T> : Behavior<T>
        where T : DependencyObject
    {
        /// <summary>
        /// Raises behavior Attached event and performs custom actions.
        /// Attempts to get IAdaptableControl interface on AssociatedObject and register itself.</summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            var control = AssociatedObject as FrameworkElement;
            if (control != null)
            {
                control.AsAdaptableControl().Attach(this);
            }
        }

        /// <summary>
        /// Raises behavior Detaching event and performs custom actions.
        /// Attempts to get IAdaptableControl interface on AssociatedObject and unregister itself.</summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            var control = AssociatedObject as FrameworkElement;
            if (control != null)
            {
                control.AsAdaptableControl().Detach(this);
            }
        }
    }

    public static class AdaptableControl
    {
        /// <summary>
        /// Returns an IAdaptableControl wrapper for any control
        /// </summary>
        /// <param name="control">Control to adapt</param>
        /// <returns>IAdaptableControl</returns>
        public static IAdaptableControl AsAdaptableControl(this FrameworkElement control)
        {
            var wrapper = GetControlAdapter(control);
            if (wrapper == null)
            {
                wrapper = new AdaptableControlAdapter(control);
                SetControlAdapter(control, wrapper);
            }
            
            return wrapper;
        }

        public static T ControlAs<T>(this FrameworkElement control)
            where T : class
        {
            return control.AsAdaptableControl().As<T>();
        }

        public static IEnumerable<T> ControlAsAll<T>(this FrameworkElement control)
            where T : class
        {
            return control.AsAdaptableControl().AsAll<T>();
        }

        private static readonly DependencyProperty ControlAdapterProperty =
            DependencyProperty.RegisterAttached("ControlAdapter", 
                typeof(AdaptableControlAdapter), typeof(AdaptableControl), 
                    new PropertyMetadata(default(AdaptableControlAdapter)));

        private static void SetControlAdapter(FrameworkElement element, AdaptableControlAdapter value)
        {
            element.SetValue(ControlAdapterProperty, value);
        }

        private static AdaptableControlAdapter GetControlAdapter(FrameworkElement element)
        {
            return (AdaptableControlAdapter)element.GetValue(ControlAdapterProperty);
        }

        /// <summary>
        /// Wraps a normal control and exposes IAdaptableControl interface
        /// </summary>
        private class AdaptableControlAdapter : IAdaptableControl
        {
            private FrameworkElement Control { get; set; }

            public AdaptableControlAdapter(FrameworkElement control)
            {
                Requires.NotNull(control, "control");
                Control = control;
            }

            public void Attach(DependencyObject dependencyObject)
            {
                Requires.NotNull(dependencyObject, "dependencyObject");
                m_adapters.Add(dependencyObject);
            }

            public void Detach(DependencyObject dependencyObject)
            {
                Requires.NotNull(dependencyObject, "dependencyObject");
                m_adapters.Remove(dependencyObject);
            }

            /// <summary>
            /// Gets an adapter of the specified type, or null</summary>
            /// <param name="type">Adapter type</param>
            /// <returns>adapter of the specified type, or null</returns>
            public object GetAdapter(Type type)
            {
                // default is to return this if compatible with requested type
                if (type.IsAssignableFrom(Control.GetType()))
                    return Control;

                // if not, let the adaptee handle it
                return As(type);
            }

            /// <summary>
            /// Gets an adapter of the specified type, or null</summary>
            /// <typeparam name="T">Adapter type</typeparam>
            /// <returns>adapter of the specified type, or null</returns>
            public T As<T>()
                where T : class
            {
                return As(typeof(T)) as T;
            }

            /// <summary>
            /// Gets an adapter of the specified type, or null</summary>
            /// <param name="type">Adapter type</param>
            /// <returns>adapter of the specified type, or null</returns>
            private object As(Type type)
            {
                // try cache
                object adapter;
                if (m_adapterCache.TryGetValue(type, out adapter))
                    return adapter;

                // try adapters (from top to bottom)
                for (int i = m_adapters.Count - 1; i >= 0; i--)
                {
                    adapter = m_adapters[i];
                    if (type.IsAssignableFrom(adapter.GetType()))
                    {
                        m_adapterCache.Add(type, adapter);
                        return adapter;
                    }
                }

                // finally, try the adaptable control itself
                if (type.IsAssignableFrom(Control.GetType()))
                {
                    m_adapterCache.Add(type, Control);
                    return Control;
                }

                return null;
            }

            /// <summary>
            /// Gets all decorators of the specified type, or null</summary>
            /// <typeparam name="T">Decorator type</typeparam>
            /// <returns>Enumeration of objects that are of the specified type</returns>
            public IEnumerable<T> AsAll<T>() where T : class
            {
                return m_adapters.Select(adapter => adapter.As<T>()).Where(t => t != null);
            }

            private readonly List<DependencyObject> m_adapters = new List<DependencyObject>();
            private readonly Dictionary<Type, object> m_adapterCache = new Dictionary<Type, object>();
        }
    }
}
