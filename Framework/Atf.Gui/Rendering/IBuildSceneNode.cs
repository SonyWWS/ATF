//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Interface that enables scene graph building for a DOM object. A DOM object may
    /// have multiple DOM object interfaces that implement this IBuildSceneNode interface. Each
    /// instance of this interface on a DOM object causes the interface builder to try to
    /// evaluate this instance as one DomConstraint and then as one IRenderObject. All of these
    /// DomConstraint and IRenderObject instances are added to the SceneNode.</summary>
    public interface IBuildSceneNode : IAdaptable
    {
        /// <summary>
        /// Gets a value indicating whether this object should generate a DomConstraint and a
        /// IRenderObject for the SceneNode that was created by SceneGraphBuilder. If this
        /// property is false, the children are still evaluated separately.</summary>
        bool CreateByGraphBuilder
        {
            get;
        }

        /// <summary>
        /// Called by SceneGraphBuilder upon building the node, if CreateByGraphBuilder is
        /// true and after the DomConstraint and IRenderObject have been attached to the
        /// SceneNode.</summary>
        /// <param name="node">Scene node that was built</param>
        void OnBuildNode(SceneNode node);
    }
}
