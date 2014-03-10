//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Rendering;
using Sce.Atf.Rendering.Dom;
using Sce.Atf.VectorMath;
using Sce.Atf.Adaptation;

namespace ModelViewerSample.Rendering
{
    /// <summary>
    /// RenderObject for pushing/popping the object's transformation onto the matrix stack.
    /// This class operates on any object that implements IRenderableNode, and
    /// performs view frustum culling in its traverse phase.</summary>
    public class RenderTransform : RenderObject, IRenderThumbnail, ISetsLocalTransform
    {
        /// <summary>
        /// Performs one-time initialization
        /// when this adapter's DomNode property is set.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            m_node = this.Cast<ITransformable>();
        }

        /// <summary>
        /// Traverses the specified graph path.</summary>
        /// <param name="graphPath">The graph path.</param>
        /// <param name="action">The render action.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public override TraverseState Traverse(Stack<SceneNode> graphPath, IRenderAction action, Camera camera, ICollection<TraverseNode> list)
        {
            // Get the "top matrix" before we push a new matrix on to it, just in case we need
            //  it for the bounding box test.
            Matrix4F parentToWorld = action.TopMatrix;

            // Push matrix onto the matrix stack even if we're not visible because this class
            //  implements the marker interface ISetsLocalTransform.
            action.PushMatrix(m_node.Transform, true);

            // If node is invisible then cull
            if (!m_node.Visible)
                return TraverseState.Cull;

            TraverseState dResult = action.TraverseState;
            if (dResult == TraverseState.None)
            {
                // Test if bounding sphere is contained in frustum
                if (s_enableVFCull)
                {
                    // Construct bounding box
                    Box box = new Box();
                    box.Extend(m_node.BoundingBox);

                    // Transform the bounding box into view space
                    Matrix4F localToView = Matrix4F.Multiply(parentToWorld, camera.ViewMatrix);
                    box.Transform(localToView);

                    if (!camera.Frustum.Contains(box))
                        dResult = TraverseState.Cull;
                }
            }
            return dResult;
        }

        /// <summary>
        /// Called after post visiting the SceneNode specified by the graph path</summary>
        /// <param name="graphPath">The graph path.</param>
        /// <param name="action">The render action.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="list">The traverse list.</param>
        /// <returns></returns>
        public override TraverseState PostTraverse(Stack<SceneNode> graphPath, IRenderAction action, Camera camera, ICollection<TraverseNode> list)
        {
            action.PopMatrix();
            return TraverseState.Continue;
        }

        /// <summary>
        /// Gets a collection of other render object interface types this class depends on.
        /// This will determine the order of render objects on the SceneNode.</summary>
        /// <returns>A collection of render object types</returns>
        public override Type[] GetDependencies()
        {
            return new Type[] { typeof(IBeforeLocalTransform) };
        }

        /// <summary>
        /// Enable/disable view frustum culling for all RenderTransform objects</summary>
        public static bool EnableVFCull
        {
            get { return s_enableVFCull; }
            set { s_enableVFCull = value; }
        }

        private ITransformable m_node;
        private static bool s_enableVFCull = true;
    }
}
