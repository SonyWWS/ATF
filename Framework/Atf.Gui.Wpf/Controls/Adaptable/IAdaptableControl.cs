//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Wpf.Controls.Adaptable
{
    /// <summary>
    /// Interface for types that can provide adapters to other types and attach to DependencyObjects</summary>
    public interface IAdaptableControl : IAdaptable
    {
        /// <summary>
        /// Attach an object, such as a Behavior</summary>
        /// <param name="dependencyObject">Dependency object to attach</param>
        void Attach(DependencyObject dependencyObject);
        /// <summary>
        /// Detach an attached object, such as a Behavior</summary>
        /// <param name="dependencyObject">Dependency object to detach</param>
        void Detach(DependencyObject dependencyObject);

        /// <summary>
        /// Converts a reference to the given type by first trying a CLR cast, and then
        /// trying to get an adapter</summary>
        /// <typeparam name="T">Desired type, must be ref type</typeparam>
        /// <returns>Converted reference for the given object or null</returns>
        T As<T>() where T : class;
        IEnumerable<T> AsAll<T>() where T : class;
    }
}
