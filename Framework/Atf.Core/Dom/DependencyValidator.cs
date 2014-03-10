//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Adapter that adds a dependency system to the DomNode's subtree and
    /// maintains constraints through transactions. The dependencies are updated at
    /// the end of every transaction that occurs in the DOM sub-tree, so are not part
    /// of transactions themselves.</summary>
    public abstract class DependencyValidator : Validator
    {
        /// <summary>
        /// Gets all DOM nodes that the given node depends on</summary>
        /// <param name="dependent">Dependent node</param>
        /// <returns>All DOM nodes that the given node depends on</returns>
        protected abstract IEnumerable<DomNode> GetDependencies(DomNode dependent);

        /// <summary>
        /// Invalidates the DOM node in the dependency system</summary>
        /// <param name="domNode">Dependency, should have been returned at some
        /// point by GetDependencies()</param>
        protected void Invalidate(DomNode domNode)
        {
            m_dependencySystem.Invalidate(domNode);
        }

        /// <summary>
        /// Performs updating operations for the dependent, which has become invalid due
        /// to the dependencies</summary>
        /// <param name="dependent">Dependent node</param>
        /// <param name="dependencies">Nodes that dependent node depends on</param>
        protected abstract void Update(DomNode dependent, IEnumerable<DomNode> dependencies);

        /// <summary>
        /// Performs custom actions after validation finished</summary>
        /// <param name="sender">Sender DOM node</param>
        /// <param name="e">Event args</param>
        protected override void OnEnded(object sender, EventArgs e)
        {
            if (m_dependencySystem.NeedsUpdate)
            {
                foreach (DependencySystem<DomNode>.InvalidDependent invalid in m_dependencySystem.GetInvalidDependents())
                    Update(invalid.Dependent, invalid.Dependencies);
            }
        }

        /// <summary>
        /// Performs custom actions after validation cancelled</summary>
        /// <param name="sender">Performs custom actions after validation finished</param>
        /// <param name="e">Event args</param>
        protected override void OnCancelled(object sender, EventArgs e)
        {
            m_dependencySystem.Cancel();
        }

        /// <summary>
        /// Performs custom actions for a node that has been added to the DOM subtree</summary>
        /// <param name="node">Added node</param>
        protected override void AddNode(DomNode node)
        {
            foreach (DomNode dependency in GetDependencies(node))
                m_dependencySystem.AddDependency(node, dependency);

            base.AddNode(node);
        }

        /// <summary>
        /// Performs custom actions for a node that has been removed from the DOM subtree</summary>
        /// <param name="node">Removed node</param>
        protected override void RemoveNode(DomNode node)
        {
            foreach (DomNode dependency in GetDependencies(node))
                m_dependencySystem.RemoveDependency(node, dependency);

            base.RemoveNode(node);
        }

        private readonly DependencySystem<DomNode> m_dependencySystem = new DependencySystem<DomNode>();
    }
}

