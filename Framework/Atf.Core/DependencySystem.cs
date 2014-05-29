//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Class that contains a dependency relation between objects. Dependencies can
    /// be invalidated, and a list of invalidated dependents can be retrieved to be
    /// updated by the client.</summary>
    /// <typeparam name="T">Type of object with dependencies</typeparam>
    public class DependencySystem<T>
    {
        /// <summary>
        /// Adds a dependency of one object on another to the system</summary>
        /// <param name="dependent">Dependent object</param>
        /// <param name="dependency">Object that dependent depends on</param>
        public void AddDependency(T dependent, T dependency)
        {
            List<T> dependents;
            if (!m_dependenciesToDependents.TryGetValue(dependency, out dependents))
            {
                dependents = new List<T>();
                m_dependenciesToDependents.Add(dependency, dependents);
            }

            dependents.Add(dependent);
        }

        /// <summary>
        /// Removes a dependency of one object on another from the system</summary>
        /// <param name="dependent">Dependent object</param>
        /// <param name="dependency">Object that dependent depends on</param>
        public void RemoveDependency(T dependent, T dependency)
        {
            List<T> dependents;
            if (m_dependenciesToDependents.TryGetValue(dependency, out dependents))
            {
                dependents.Remove(dependent);
                if (dependents.Count == 0)
                    m_dependenciesToDependents.Remove(dependency);
            }
        }

        /// <summary>
        /// Invalidates a dependency object if it is in the system. Only dependencies known
        /// to the system are invalidated.</summary>
        /// <param name="dependency">Dependency object, added by call to AddDependency</param>
        public void Invalidate(T dependency)
        {
            if (m_dependenciesToDependents.ContainsKey(dependency))
                m_invalidDependencies.Add(dependency);
        }

        /// <summary>
        /// Cancels all pending invalidations</summary>
        public void Cancel()
        {
            m_invalidDependencies.Clear();
        }

        /// <summary>
        /// Gets a value indicating if there are invalidated dependencies in the system</summary>
        public bool NeedsUpdate
        {
            get { return m_invalidDependencies.Count > 0; }
        }

        /// <summary>
        /// Structure holding a dependent that has become invalidated, and the dependencies
        /// that caused it</summary>
        public struct InvalidDependent
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="dependent">Dependent object that has become invalidated</param>
            /// <param name="dependencies">Enumeration of dependencies invalidating object</param>
            public InvalidDependent(T dependent, IEnumerable<T> dependencies)
            {
                Dependent = dependent;
                Dependencies = dependencies;
            }
            /// <summary>
            /// Dependent object that has become invalidated</summary>
            public readonly T Dependent;
            /// <summary>
            /// Enumeration of dependencies invalidating object</summary>
            public readonly IEnumerable<T> Dependencies;
        }

        /// <summary>
        /// Gets an enumeration of all invalidated dependents and the dependencies
        /// that caused them to be invalid</summary>
        /// <returns>Enumerator for all invalidated dependents</returns>
        /// <remarks>Circular dependencies can't be handled in a single step</remarks>
        public IEnumerable<InvalidDependent> GetInvalidDependents()
        {
            // use invalidated dependencies to build a map from invalidated dependents
            //  to lists of invalid depedencies for that dependent.
            Dictionary<T, List<T>> dependentsToDependencies = new Dictionary<T, List<T>>();
            Queue<T> queue = new Queue<T>(m_invalidDependencies);
            HashSet<T> visited = new HashSet<T>(m_invalidDependencies);
            m_invalidDependencies.Clear();
            while (queue.Count > 0)
            {
                T dependency = queue.Dequeue();
                List<T> dependents;
                if (m_dependenciesToDependents.TryGetValue(dependency, out dependents))
                {
                    foreach (T dependent in dependents)
                    {
                        List<T> dependencies;
                        if (!dependentsToDependencies.TryGetValue(dependent, out dependencies))
                        {
                            dependencies = new List<T>();
                            dependentsToDependencies.Add(dependent, dependencies);
                        }
                        dependencies.Add(dependency);

                        // add dependent to queue in case it's a dependency for other objects
                        if (!visited.Contains(dependent))
                        {
                            queue.Enqueue(dependent);
                            visited.Add(dependent);
                        }
                    }
                }
            }

            // sort dependents topologically, if possible, using a depth first traversal
            //  of the dependency graph
            Stack<T> stack = new Stack<T>();
            visited.Clear();
            foreach (KeyValuePair<T, List<T>> kvp in dependentsToDependencies)
            {
                T dependent = kvp.Key;
                if (visited.Contains(dependent))
                    continue;

                stack.Push(dependent);
                visited.Add(dependent);
                while (stack.Count > 0)
                {
                    dependent = stack.Peek();
                    bool canUpdate = true;
                    List<T> dependencies;
                    dependentsToDependencies.TryGetValue(dependent, out dependencies);
                    foreach (T dependency in dependencies)
                    {
                        if (!visited.Contains(dependency) &&
                            dependentsToDependencies.ContainsKey(dependency))
                        {
                            canUpdate = false;
                            stack.Push(dependency);
                            visited.Add(dependency);
                        }
                    }

                    if (canUpdate)
                    {
                        stack.Pop();
                        yield return new InvalidDependent(dependent, dependencies);
                    }
                }
            }
        }
        
        private readonly Dictionary<T, List<T>> m_dependenciesToDependents = // map dependencies to dependents
            new Dictionary<T, List<T>>();
        private readonly HashSet<T> m_invalidDependencies = new HashSet<T>();
    }
}

