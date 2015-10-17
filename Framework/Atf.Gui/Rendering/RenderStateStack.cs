//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// RenderState stack</summary>
    public class RenderStateStack
    {
        /// <summary>
        /// Gets the number of RenderStates</summary>
        public int Count
        {
            get { return m_contents.Count; }
        }

        /// <summary>
        /// Pushes the specified RenderState onto the stack</summary>
        /// <param name="renderState">RenderState to push</param>
        public void Push(RenderState renderState)
        {
            m_contents.Add(renderState);
            ComputeComposedRenderState();
        }

        /// <summary>
        /// Pops the top RenderState off the stack</summary>
        public void Pop()
        {
            m_contents.RemoveAt(m_contents.Count - 1);
            ComputeComposedRenderState();
        }

        /// <summary>
        /// Retrieves the top RenderState from stack</summary>
        /// <returns>Top RenderState in stack</returns>
        public RenderState Peek()
        {
            if (m_contents.Count == 0)
                return null;
            
            RenderState topOfStack = m_contents[m_contents.Count - 1];
            return topOfStack.Clone() as RenderState;
        }

        /// <summary>
        /// Removes the specified RenderState from stack</summary>
        /// <param name="renderState">The RenderState to remove</param>
        public void Remove(RenderState renderState)
        {
            m_contents.Remove(renderState);
            ComputeComposedRenderState();
        }

        /// <summary>
        /// Determines whether stack contains the specified RenderState</summary>
        /// <param name="renderState">Render state</param>
        /// <returns><c>True</c> if stack contains the specified RenderState</returns>
        public bool Contains(RenderState renderState)
        {
            return m_contents.Contains(renderState);
        }

        /// <summary>
        /// Gets the composed RenderState</summary>
        public RenderState ComposedRenderState
        {
            get
            {
                if (m_composedRenderState == null)
                    return null;
                
                return m_composedRenderState.Clone() as RenderState;
            }
        }

        private void ComputeComposedRenderState()
        {
            if (m_contents.Count == 0)
            {
                m_composedRenderState = null;
                return;
            }

            int lastElementIndex = m_contents.Count - 1;
            m_composedRenderState = m_contents[lastElementIndex].Clone() as RenderState;

            for (int i = lastElementIndex - 1; i >= 0; --i)
                m_composedRenderState.ComposeFrom(m_contents[i]);
        }

        private readonly List<RenderState> m_contents = new List<RenderState>();
        private RenderState m_composedRenderState;
    }
}
