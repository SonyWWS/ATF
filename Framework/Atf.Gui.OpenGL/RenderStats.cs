//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// A class for accumulating and displaying rendering statistics, to the 
    /// Design View, for example. Util3D.RenderStats property is an instance of this class.</summary>
    public class RenderStats
    {
        /// <summary>
        /// Gets or sets whether the rendering statistics are displayed</summary>
        public bool Enabled
        {
            get { return m_enabled; }
            set { m_enabled = value; }
        }

        /// <summary>
        /// Resets the counts for rendering the current frame. Increments the frame counter.</summary>
        public virtual void ResetFrame()
        {
            m_nPrimitives = 0;
            m_nVertices = 0;
            m_traverseNodeCount = 0;
            m_renderStateChanges = 0;
            m_timeForConstraints = 0;
            m_timeForTraverse = 0;
            m_timeForDispatchTraverseList = 0;

            // update the queue of the # of frames we've drawn recently
            DateTime now = DateTime.Now;
            while (m_updateTimes.Count > 0)
            {
                DateTime oldest = m_updateTimes.Peek();
                TimeSpan age = now - oldest;
                if (age.TotalMilliseconds > AverageFrameRateTime * 1000)
                    m_updateTimes.Dequeue();
                else
                    break;
            }
            m_updateTimes.Enqueue(now);

            m_frameCount++;
        }

        /// <summary>
        /// Gets or sets the number of primitives (triangles, quads, etc.) rendered this frame</summary>
        public int PrimCount
        {
            get { return m_nPrimitives; }
            set { m_nPrimitives = value; }
        }

        /// <summary>
        /// Gets or sets the number of vertices rendered this frame</summary>
        public int VertexCount
        {
            get { return m_nVertices; }
            set { m_nVertices = value; }
        }

        /// <summary>
        /// Gets or sets the number of miliseconds spent calling ConstraintManager.Tick</summary>
        public long TimeForConstraints
        {
            get { return m_timeForConstraints; }
            set { m_timeForConstraints = value; }
        }
        
        /// <summary>
        /// Gets or sets the number of miliseconds spent calling RenderAction.BuildTraverseList</summary>
        public long TimeForTraverse
        {
            get { return m_timeForTraverse; }
            set { m_timeForTraverse = value; }
        }
        
        /// <summary>
        /// Gets or sets the number of miliseconds spent calling RenderAction.RenderPass</summary>
        public long TimeForDispatchTraverseList
        {
            get { return m_timeForDispatchTraverseList; }
            set { m_timeForDispatchTraverseList = value; }
        }
        
        /// <summary>
        /// Gets the number of miliseconds spent preparing, traversing, and rendering the scene graph.
        /// In other words, this is the total time required to render one frame buffer. This
        /// is the sum of TimeForContraints, TimeForTraverse, and TimeForDispatchTraverseList.</summary>
        public long TimeForFrame
        {
            get { return m_timeForConstraints + m_timeForTraverse + m_timeForDispatchTraverseList; }
        }

        /// <summary>
        /// Gets or sets the number of TraverseNode objects that came out of RenderAction.BuildTraverseList
        /// and that took TimeForTraverse miliseconds to prepare</summary>
        public int TraverseNodeCount
        {
            get { return m_traverseNodeCount; }
            set { m_traverseNodeCount = value; }
        }

        /// <summary>
        /// Gets or sets the number of OpenGL render state changes for rendering one frame</summary>
        public int RenderStateChanges
        {
            get { return m_renderStateChanges; }
            set { m_renderStateChanges = value; }
        }

        /// <summary>
        /// Gets the number of times a frame has been rendered. Is the number of times that ResetFrame
        /// has been called.</summary>
        public int FrameCount
        {
            get { return m_frameCount; }
        }

        /// <summary>
        /// Gets the average number of frames drawn per second over the last 5 seconds (or whatever
        /// is specified by AverageFrameRateTime)</summary>
        public float AverageFrameRate
        {
            get { return (float)m_updateTimes.Count / (float)AverageFrameRateTime; }
        }

        /// <summary>
        /// Number of seconds used to calculate the running average frame rate. Must be > 0.</summary>
        public int AverageFrameRateTime = 5;

        /// <summary>
        /// Gets or sets custom text to add to the display string, which is useful for temporary
        /// measurements without having to modify this RenderStats class</summary>
        public string CustomText
        {
            get { return m_customText; }
            set { m_customText = value; }
        }

        /// <summary>
        /// Prepares a string containing the results to be displayed</summary>
        /// <returns>Results string</returns>
        public virtual string GetDisplayString()
        {
            return string.Format("{0} {1}; {2}ms {3:F2}fps {4}",
                "polys".Localize("abbreviation of 'polygons'"), m_nPrimitives,
                //"frame", m_frameCount % 1000,
                //"nodes", m_traverseNodeCount,
                //Localization.StateChanges, m_renderStateChanges,
                //"states", m_renderStateChanges,
                TimeForFrame,
                AverageFrameRate,
                m_customText);
        }

        private bool m_enabled = true;
        private int m_nPrimitives;
        private int m_nVertices;
        private int m_traverseNodeCount;
        private int m_renderStateChanges;
        private int m_frameCount;
        private long m_timeForConstraints, m_timeForTraverse, m_timeForDispatchTraverseList;
        private string m_customText = string.Empty;
        private readonly Queue<DateTime> m_updateTimes = new Queue<DateTime>();
    }
}
