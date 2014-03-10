namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Implementors of IRenderObject should implement this interface if they must be listed
    /// in the RenderObjectList before other IRenderObjects that set the local transform by calling
    /// IRenderAction.PushMatrix in their Traverse methods</summary>
    /// <example>
    /// RenderBoundable (a class for rendering the bounding box of any object implmenting IBoundable)
    /// might construct its own local transform and so must come before classes that
    /// push a matrix onto IRenderAction. RenderBoundable would then implement IBeforeLocalTransform.
    /// 
    /// RenderTransform (a class for a RenderObject for pushing/popping the object's transformation onto the matrix stack)
    /// pushes a matrix by calling IRenderAction.PushMatrix, so it implements
    /// ISetsLocalTransform.
    /// 
    /// DisplayProxy requires the local transform to already be set, so it adds ISetsLocalTransform
    /// to its dependency list in GetDependencies().
    /// 
    /// With everyone's GetDependencies() set appropriately, RenderObjectList sorts these various
    /// IRenderObjects as follows: RenderBoundable, RenderTransform, DisplayProxy.
    /// </example>
    public interface IBeforeLocalTransform
    {
    }
}
