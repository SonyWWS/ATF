using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf;
using Sce.Atf.Applications;

namespace CodeEditor
{
    /// <summary>
    /// Source control context component</summary>
    [Export(typeof(ISourceControlContext))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SourceControlContext : ISourceControlContext, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="documentRegistry">Document registry used to track documents</param>
        /// <param name="contextRegistry">Component that tracks application contexts</param>
        [ImportingConstructor]
        public SourceControlContext(
            IDocumentRegistry documentRegistry,
            IContextRegistry contextRegistry)
        {
            m_documentRegistry = documentRegistry;
            m_contextRegistry = contextRegistry;
        }

        /// <summary>
        /// Gets an enumeration of the open documents</summary>
        public IEnumerable<IResource> Resources
        {
            get
            {
                return m_documentRegistry.ActiveDocument == null
                           ? EmptyEnumerable<IResource>.Instance
                           : new IResource[] { m_documentRegistry.ActiveDocument };
            }
        }

        /// <summary>
        /// Initializes the object</summary>
        void IInitializable.Initialize()
        {
            m_contextRegistry.ActiveContext = this;
        }

        private readonly IDocumentRegistry m_documentRegistry;
        private readonly IContextRegistry m_contextRegistry;
      }
}
