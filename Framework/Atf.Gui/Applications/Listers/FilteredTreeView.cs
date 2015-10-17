//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Wrapper for an ITreeView which provides filtering
    /// </summary>
    public class BasicFilteredTreeView : ITreeView, IAdaptable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="treeView">Data context of ITreeView to apply a filter</param>
        /// <param name="filterFunc">Callback to determine if an item in the tree is filtered in (return true) or out</param>
        public BasicFilteredTreeView(ITreeView treeView, Predicate<object> filterFunc)
        {
            m_treeView = treeView;
            m_filterFunc = filterFunc;
        }

        /// <summary>
        /// Gets direct access to the wrapped tree view</summary>
        public ITreeView InnerTreeView { get { return m_treeView; } }

        /// <summary>
        /// Indicates whether two ITreeView instances are equal</summary>
        /// <param name="first">First ITreeView to compare</param>
        /// <param name="second">Second ITreeView to compare</param>
        /// <returns><c>True</c> if ITreeView instances are equal</returns>
        public static bool Equals(ITreeView first, ITreeView second)
        {
            BasicFilteredTreeView f1 = first.As<BasicFilteredTreeView>();
            if (f1 != null)
                first = f1.m_treeView;
            BasicFilteredTreeView f2 = second.As<BasicFilteredTreeView>();
            if (f2 != null)
                second = f2.m_treeView;
            return first == second;
        }

        #region ITreeView Members

        /// <summary>
        /// Gets tree root</summary>
        public virtual object Root
        {
            get { return m_treeView.Root; }
        }

        /// <summary>
        /// Gets parent's children</summary>
        /// <param name="parent">Parent</param>
        /// <returns>Children objects</returns>
        public virtual IEnumerable<object> GetChildren(object parent)
        {
            // Return every child that either matches
            // or has a descendant that matches the predicate
            return m_treeView.GetChildren(parent).Where(child => MatchOrDescendantMatch(child));
        }

        #endregion

        #region IAdaptable Members

        /// <summary>
        /// Gets an adapter of the specified type or null</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type or null</returns>
        public virtual object GetAdapter(Type type)
        {
            if (typeof(BasicFilteredTreeView).IsAssignableFrom(type))
                return this;
            return m_treeView;
        }

        #endregion

        /// <summary>
        /// The wrapped tree view</summary>
        protected readonly ITreeView m_treeView;

        private bool MatchOrDescendantMatch(object item)
        {
            return m_filterFunc(item)
                || m_treeView.GetChildren(item).Any(child => MatchOrDescendantMatch(child));
        }

        private readonly Predicate<object> m_filterFunc;
    }
}
