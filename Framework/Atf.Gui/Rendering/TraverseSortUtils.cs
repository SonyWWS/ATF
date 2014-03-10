//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Helper functions for render state sorting</summary>
    public static class TraverseSortUtils
    {
        /// <summary>
        /// Sorts the provided list by OpenGL texture name. Minimizes calls to glBindTexture().</summary>
        /// <param name="list">List of textured TraverseNodes to sort</param>
        public static void SortByTextureName(List<TraverseNode> list)
        {
            list.Sort(new TextureNameComparer());
        }

        private class TextureNameComparer : IComparer<TraverseNode>
        {
            public int Compare(TraverseNode x, TraverseNode y)
            {
                if (x.RenderState.TextureName < y.RenderState.TextureName)
                    return -1;

                if (x.RenderState.TextureName > y.RenderState.TextureName)
                    return 1;

                return 0;
            }
        }

        /// <summary>
        /// Sorts the provided list by camera space z distance.  
        /// Assumes a right-handed coordinate system, so the farthest nodes have the most negative values.</summary>
        /// <param name="list">List of alpha TraverseNodes to sort</param>
        /// <param name="viewMatrix">Current view matrix to transform bounding box centroids by</param>
        public static void SortByCameraSpaceDepth(List<TraverseNode> list, Matrix4F viewMatrix)
        {
            KeyValuePair<float, TraverseNode>[] camSpaceDistances =
                new KeyValuePair<float, TraverseNode>[list.Count];

            for (int i = 0; i < list.Count; ++i)
            {
                TraverseNode node = list[i];
                Vec3F worldSpaceCentroid = node.WorldSpaceBoundingBox.Centroid;

                Vec3F centerPointInCameraSpace;
                viewMatrix.Transform(worldSpaceCentroid, out centerPointInCameraSpace);

                camSpaceDistances[i] = new KeyValuePair<float, TraverseNode>(centerPointInCameraSpace.Z, node);
            }

            Array.Sort(camSpaceDistances, new CamSpaceDistanceComparer());

            list.Clear();

            foreach (KeyValuePair<float, TraverseNode> entry in camSpaceDistances)
                list.Add(entry.Value);
        }

        private class CamSpaceDistanceComparer : IComparer<KeyValuePair<float, TraverseNode>>
        {
            public int Compare(KeyValuePair<float, TraverseNode> x, KeyValuePair<float, TraverseNode> y)
            {
                if (x.Key < y.Key)
                    return -1;

                if (x.Key > y.Key)
                    return 1;

                return 0;
            }
        }

        /// <summary>
        /// Sorts the provided list by render mode. This minimizes OpenGl state changes.
        /// Should be lowest priority after sorting alpha'd objects by distance and textured
        /// objects by texture.</summary>
        /// <param name="list">List of textured TraverseNodes to sort</param>
        public static void SortByRenderMode(List<TraverseNode> list)
        {
            list.Sort(new RenderModeComparer());
        }

        private class RenderModeComparer : IComparer<TraverseNode>
        {
            public int Compare(TraverseNode x, TraverseNode y)
            {
                if (x.RenderState.RenderMode < y.RenderState.RenderMode)
                    return -1;

                if (x.RenderState.RenderMode > y.RenderState.RenderMode)
                    return -1;

                return 0;
            }
        }

    }
}
