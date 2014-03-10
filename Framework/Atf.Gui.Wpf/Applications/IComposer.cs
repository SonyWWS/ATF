//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for ComposablePart and CompositionContainer, which are fundamental to Managed Extensibility Framework (MEF).
    /// ComposablePart is an abstract base class for composable parts, which import objects and produce exported objects;
    /// ComposablePart is the basic building block for MEF. 
    /// CompositionContainer serves as a repository for ComposablePart objects and provides methods for composing applications.</summary>
    public interface IComposer
    {
        /// <summary>
        /// Gets the CompositionContainer</summary>
        CompositionContainer Container { get; }
    }

    /// <summary>
    /// Extension methods for IComposer</summary>
    public static class IComposerExtensions
    {
        /// <summary>
        /// Creates a ComposablePart from an attributed object and adds it to the composition</summary>
        /// <param name="composer">IComposer</param>
        /// <param name="attributedPart">Attributed object to add to the composition</param>
        /// <returns>ComposablePart from the attributed object</returns>
        /// <remarks>This method creates a ComposablePart from the attributed object, adds it to a CompositionBatch 
        /// and then executes composition on this CompositionBatch.</remarks>
        public static ComposablePart AddPart(this IComposer composer, object attributedPart)
        {
            Requires.NotNull(composer, "composer");
            Requires.NotNull(attributedPart, "attributedPart");

            ComposablePart part = AttributedModelServices.CreatePart(attributedPart);
            var batch = new CompositionBatch(new ComposablePart[]{part}, Enumerable.Empty<ComposablePart>());

            composer.Container.Compose(batch);

            return part;
        }

        /// <summary>
        /// Removes a ComposablePart from the composition</summary>
        /// <param name="composer">IComposer</param>
        /// <param name="composablePart">ComposablePart to remove from the compositoi</param>
        public static void RemovePart(this IComposer composer, ComposablePart composablePart)
        {
            Requires.NotNull(composer, "composer");
            Requires.NotNull(composablePart, "composablePart");

            var batch = new CompositionBatch(Enumerable.Empty<ComposablePart>(), new ComposablePart[] { composablePart });
            composer.Container.Compose(batch);
        }
    }
}
