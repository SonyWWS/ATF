//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for providers of context menu commands</summary>
    public interface IContextMenuCommandProvider
    {
        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        IEnumerable<object> GetCommands(object context, object target);
    }

    /// <summary>
    /// Extension methods</summary>
    public static class ContextMenuCommandProvider
    {
        /// <summary>
        /// Gets command tags for context menu by querying all IContextMenuCommandProviders with
        /// the given target</summary>
        /// <param name="providers">Context menu item providers</param>
        /// <param name="context">Context of target item</param>
        /// <param name="target">Target item that was right clicked, or null</param>
        /// <returns>Command tags for context menu</returns>
        public static IEnumerable<object> GetCommands(
            this IEnumerable<IContextMenuCommandProvider> providers, object context, object target)
        {
            foreach (IContextMenuCommandProvider provider in providers)
                foreach (object commandTag in provider.GetCommands(context, target))
                    yield return commandTag;
        }

        /// <summary>
        /// Gets command tags for context menu by querying all IContextMenuCommandProviders with
        /// the given target</summary>
        /// <param name="providers">Context menu item providers</param>
        /// <param name="context">Context of target item</param>
        /// <param name="target">Target item that was right clicked, or null</param>
        /// <returns>Command tags for context menu</returns>
        public static IEnumerable<object> GetCommands(
            this IEnumerable<Lazy<IContextMenuCommandProvider>> providers, object context, object target)
        {
            return GetCommands(providers.GetValues(), context, target);
        }
    }
}
