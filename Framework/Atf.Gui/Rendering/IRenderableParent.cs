//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Interface for determining if a child object reference is renderable or not. This is
    /// used when building a scene graph to give the parent or owning object a chance to not allow a
    /// child or referenced object to be added to the scene graph.</summary>
    public interface IRenderableParent : IAdaptable
    {
        /// <summary>
        /// Gets whether or not the given referenced or child object is renderable</summary>
        /// <param name="child">The child object</param>
        /// <returns><c>True</c> if the object is renderable</returns>
        bool IsRenderableChild(object child);
    }
}
