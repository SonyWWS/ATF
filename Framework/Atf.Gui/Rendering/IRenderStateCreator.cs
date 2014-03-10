//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for RenderState creators</summary>
    public interface IRenderStateCreator
    {
        /// <summary>
        /// Creates RenderState for the object</summary>
        /// <returns>RenderState for the object</returns>
        RenderState CreateRenderState();
    }
}
