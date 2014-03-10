//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace Sce.Atf
{
    /// <summary>
    /// MEF (Managed Extensibility Framework) utilities and extension methods</summary>
    public static class MefUtil
    {
        /// <summary>
        /// Instantiates an instance of all objects in the container and additionally initializes all objects
        /// that implement the <see cref="Sce.Atf.IInitializable"/> interface</summary>
        /// <param name="container">MEF composition container</param>
        public static void InitializeAll(this CompositionContainer container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            // ImportDefinition
            // - constraint: An expression that contains a Func<T, TResult> object that defines the conditions
            //      an Export must match to satisfy the ImportDefinition.
            // - contractName: The contract name.
            // - cardinality: One of the enumeration values that indicates the cardinality of the Export objects
            //      required by the ImportDefinition.
            // - ImportCardinality.ZeroOrMore: Zero or more Export objects are required by the ImportDefinition.
            // - isRecomposable: true to specify that the ImportDefinition can be satisfied multiple times
            //      throughout the lifetime of a ComposablePart object; otherwise, false.
            // - isPrerequisite: true to specify that the ImportDefinition must be satisfied before a
            //      ComposablePart can start producing exported objects; otherwise, false.
            var importDef = new ImportDefinition(contraint => true, string.Empty, ImportCardinality.ZeroOrMore, true, true);

            try
            {
                // Create everything. This ensures that all objects are constructed before IInitializable
                //  is used. Note that the order of construction is not deterministic. We have seen the same
                //  code result in different orderings on different computers.
                foreach (Export export in container.GetExports(importDef))
                {
                    // Triggers creation of object (otherwise lazy).
                    // Also, IPartImportsSatisfiedNotification.OnImportsSatisfied() will be called here.
                    object tmp = export.Value;
                }

                // Initialize components that require it. Initialization often can't be done in the constructor,
                //  or even after imports have been satisfied by MEF, since we allow circular dependencies between
                //  components, via the System.Lazy class. IInitializable allows components to defer some operations
                //  until all MEF composition has been completed.
                foreach (IInitializable initializable in container.GetExportedValues<IInitializable>())
                    initializable.Initialize();
            }
            catch (CompositionException ex)
            {
                foreach (var error in ex.Errors)
                    Outputs.WriteLine(OutputMessageType.Error, "MEF CompositionException: {0}", error.Description);

                throw;
            }
        }
    }
}
