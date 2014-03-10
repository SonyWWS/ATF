namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// An interface for IRenderObjects that sets the local transform matrix (the transform from the
    /// parent to this render object) by calling IRenderAction.PushMatrix in the Traverse method.
    /// Other IRenderObjects that need this local transform can add this interface to their
    /// dependency list, obtainable by IRenderObject.GetDependences(), which controls the order that IRenderObjects
    /// appear in RenderObjectList.</summary>
    /// <example>
    /// RenderBoundable (a class for rendering the bounding box of any object implmenting IBoundable)
    /// might construct its own local transform and so must come before classes that
    /// push a matrix onto IRenderAction. RenderBoundable would then implement IBeforeLocalTransform.
    /// 
    /// RenderTransform (a class for a RenderObject for pushing/popping the object's transformation onto the matrix stack)
    /// pushes a matrix by calling IRenderAction.PushMatrix(), so it implements
    /// ISetsLocalTransform.
    /// 
    /// DisplayProxy requires the local transform to already be set, so it adds ISetsLocalTransform
    /// to its dependency list in GetDependencies().
    /// 
    /// With everyone's GetDependencies() set appropriately, RenderObjectList sorts these various
    /// IRenderObjects as follows: RenderBoundable, RenderTransform, DisplayProxy.
    /// </example>
    public interface ISetsLocalTransform
    {
    }
}
