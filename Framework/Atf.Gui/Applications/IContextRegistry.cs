//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for component that tracks application contexts</summary>
    public interface IContextRegistry
    {
        /// <summary>
        /// Gets or sets the active context</summary>
        object ActiveContext
        {
            get;
            set;
        }

        /// <summary>
        /// Obtains the active context as the given type</summary>
        /// <typeparam name="T">Desired context type</typeparam>
        /// <returns>Active context as the given type, or null</returns>
        T GetActiveContext<T>()
            where T : class;

        /// <summary>
        /// Gets the most recently active context of the given type; this may not be the
        /// same as the ActiveContext</summary>
        /// <typeparam name="T">Desired context type</typeparam>
        /// <returns>Most recently active context of the given type, or null</returns>
        T GetMostRecentContext<T>()
            where T : class;

        /// <summary>
        /// Event that is raised before the active context changes</summary>
        event EventHandler ActiveContextChanging;

        /// <summary>
        /// Event that is raised after the active context changes</summary>
        event EventHandler ActiveContextChanged;

        /// <summary>
        /// Gets enumeration of the open contexts, in order of least-recently-active to the active context</summary>
        IEnumerable<object> Contexts
        {
            get;
        }

        /// <summary>
        /// Event that is raised after a context is added; it becomes the active context</summary>
        event EventHandler<ItemInsertedEventArgs<object>> ContextAdded;

        /// <summary>
        /// Event that is raised after a context is removed</summary>
        event EventHandler<ItemRemovedEventArgs<object>> ContextRemoved;

        /// <summary>
        /// Removes the given context if it is open</summary>
        /// <param name="context">Context to remove</param>
        /// <returns><c>True</c> if the context was removed</returns>
        bool RemoveContext(object context);
    }

    /// <summary>
    /// Extension methods for IContextService</summary>
    public static class ContextRegistries
    {
        /// <summary>
        /// Gets the command target for a menu command. First, if the active context has a
        /// selection, the last selected object of the requested type is adapted. If that is
        /// null, the context is adapted. Returns null if no target can be adapted.</summary>
        /// <typeparam name="T">Target object type</typeparam>
        /// <param name="contextRegistry">Context registry</param>
        /// <returns>The command target for a menu command</returns>
        public static T GetCommandTarget<T>(this IContextRegistry contextRegistry) where T : class
        {
            T target = null;

            var selectionContext = contextRegistry.GetActiveContext<ISelectionContext>();
            if (selectionContext != null)
                target = selectionContext.GetLastSelected<T>();

            if (target == null)
                target = contextRegistry.GetActiveContext<T>();

            return target;
        }
    }
}
