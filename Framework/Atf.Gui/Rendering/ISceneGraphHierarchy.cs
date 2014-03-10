using System.Collections.Generic;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Optional interface used for guiding SceneGraphBuilder when traversing a 3D model.
    /// Since each 3D model format has a different structure, 
    /// the default traversal does not work for all model formats.
    /// For example, the default traversal does not correct model 
    /// hierarchy for Collada models</summary>
    public interface ISceneGraphHierarchy
    {
        /// <summary>
        /// Gets enumeration of the children of the object</summary>
        /// <returns>Enumeration of the children of the object</returns>
        IEnumerable<object> GetChildren();
    }    
}
