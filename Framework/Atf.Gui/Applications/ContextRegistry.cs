//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component that tracks application contexts. Contexts correspond to user views of
    /// data.</summary>
    [Export(typeof(IContextRegistry))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ContextRegistry : IContextRegistry
    {
        /// <summary>
        /// Constructor</summary>
        public ContextRegistry()
        {
            m_contexts = new AdaptableActiveCollection<object>();
            m_contexts.ActiveItemChanged += contexts_ActiveItemChanged;
            m_contexts.ActiveItemChanging += contexts_ActiveItemChanging;
            m_contexts.ItemAdded += contexts_ItemAdded;
            m_contexts.ItemRemoved += contexts_ItemRemoved;
        }

        /// <summary>
        /// Gets the collection of active contexts</summary>
        public AdaptableActiveCollection<object> Contexts
        {
            get { return m_contexts; }
        }

        #region IContextRegistry Members

        /// <summary>
        /// Gets or sets the active context</summary>
        public object ActiveContext
        {
            get { return m_contexts.ActiveItem; }
            set { m_contexts.ActiveItem = value; }
        }

        /// <summary>
        /// Gets the active context as the given type</summary>
        /// <typeparam name="T">Desired context type</typeparam>
        /// <returns>Active context as the given type, or null</returns>
        public T GetActiveContext<T>() where T : class
        {
            return m_contexts.ActiveItem.As<T>();
        }

        /// <summary>
        /// Gets the most recently active context of the given type; this may not be the
        /// same as the ActiveContext</summary>
        /// <typeparam name="T">Desired context type</typeparam>
        /// <returns>Most recently active context of the given type, or null</returns>
        public T GetMostRecentContext<T>() where T : class
        {
            return m_contexts.GetActiveItem<T>();
        }

        /// <summary>
        /// Event that is raised before the active context changes</summary>
        public event EventHandler ActiveContextChanging;

        /// <summary>
        /// Event that is raised after the active context changes</summary>
        public event EventHandler ActiveContextChanged;

        /// <summary>
        /// Gets the open contexts, in order of least-recently-active to the active context</summary>
        IEnumerable<object> IContextRegistry.Contexts
        {
            get { return m_contexts; }
        }

        /// <summary>
        /// Event that is raised after a context is added; it will be the active context</summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ContextAdded;

        /// <summary>
        /// Event that is raised after a context is removed</summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ContextRemoved;

        /// <summary>
        /// Removes the given context if it is open</summary>
        /// <param name="context">Context to remove</param>
        /// <returns><c>True</c> if the context was removed</returns>
        public bool RemoveContext(object context)
        {
            return m_contexts.Remove(context);
        }

        #endregion

        private void contexts_ActiveItemChanging(object sender, EventArgs e)
        {
            ActiveContextChanging.Raise(this, EventArgs.Empty);
        }

        private void contexts_ActiveItemChanged(object sender, EventArgs e)
        {
            ActiveContextChanged.Raise(this, EventArgs.Empty);
        }

        private void contexts_ItemAdded(object sender, ItemInsertedEventArgs<object> e)
        {
            ContextAdded.Raise(this, e);
        }

        private void contexts_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            ContextRemoved.Raise(this, e);
        }

        private readonly AdaptableActiveCollection<object> m_contexts;
    }
}
