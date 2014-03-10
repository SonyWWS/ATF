//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// RenderStateGuardian implements a dispatch system for committing RenderState to the device driver layer.
    /// It maintains an associative map from a single RenderMode flag to a platform-specific delegate whose job it is to commit that particular state.</summary>
    public class RenderStateGuardian
    {
        /// <summary>
        /// Callback for Set RenderState handlers</summary>
        /// <param name="newRenderState">New RenderState</param>
        /// <param name="oldRenderState">Old RenderState</param>
        public delegate void RenderStateSetHandler(RenderState newRenderState, RenderState oldRenderState);

        /// <summary>
        /// Resets the RenderStateGuardian's last known good state.  
        /// This forces the RenderStateGuardian delegate targets to do a full and heavy state set
        /// at next Commit() call.</summary>
        public void Reset()
        {
            m_reset = true;
        }

        /// <summary>
        /// Clears all registered RenderStateSetHandlers</summary>
        public void Clear()
        {
            for (int i = 0; i < 32; i++)
                m_renderStateSetters[i] = null;
        }

        /// <summary>
        /// Applies the provided RenderState to the graphics driver.
        /// At exit, the state of the graphics device matches the parameters of the provided RenderState.</summary>
        /// <param name="renderState">The RenderState to set the graphics device state to</param>
        public void Commit(RenderState renderState)
        {
            renderState.CommitAllBitsToGuardian(this);
            m_oldRenderState.Init(renderState);
            m_reset = false;
        }

        /// <summary>
        /// Retrieves a registered RenderStateSetHandler by its bit key</summary>
        /// <param name="renderStateBit">Bit key with which to look up the handler</param>
        /// <returns>RenderStateSetHandler matching bit key</returns>
        public RenderStateSetHandler TryGetHandler(int renderStateBit)
        {
            int index = Sce.Atf.MathUtil.LogBase2(renderStateBit);
            return m_renderStateSetters[index];
        }
        
        /// <summary>
        /// Registers a platform-specific delegate RenderStateSetHandler to the specified render state bit.
        /// This handler is called during SetRenderState calls.</summary>
        /// <param name="renderStateBit">Bit key to register the handler under</param>
        /// <param name="handler">Handler delegate to associate with the bit</param>
        public void RegisterRenderStateHandler(int renderStateBit, RenderStateSetHandler handler)
        {
            if (!Sce.Atf.MathUtil.OnlyOneBitSet(renderStateBit))
                throw new ArgumentException("RenderStateSetHandlers can only be set on keys with only one bit set.");

            if (TryGetHandler(renderStateBit) != null)
                throw new InvalidOperationException("Key " + renderStateBit + " is already associated with a handler.");

            int index = Sce.Atf.MathUtil.LogBase2(renderStateBit);
            m_renderStateSetters[index] = handler;
        }

        /// <summary>
        /// Sets the aspect of the global render state associated with the specified bit.
        /// Consider using SetRenderStateByIndex if you have access to the index.</summary>
        /// <param name="renderStateBit">Render state bit to set</param>
        /// <param name="renderState">Current RenderState to commit to the graphics device driver layer</param>
        public void SetRenderState(int renderStateBit, RenderState renderState)
        {
            RenderStateSetHandler handler = TryGetHandler(renderStateBit);
            if (handler == null)
                return;

            handler(renderState, m_reset==false ? m_oldRenderState : null);
        }

        /// <summary>
        /// Sets the aspect of the global render state associated with the specified bit.
        /// For performance reasons, this is better than SetRenderState().</summary>
        /// <param name="renderStateIndex">Zero-based render state index to set. Is equivalent
        /// to how many times the 1 is shifted left in the render state bit.</param>
        /// <param name="renderState">Current RenderState to commit to the graphics device driver layer</param>
        public void SetRenderStateByIndex(int renderStateIndex, RenderState renderState)
        {
            RenderStateSetHandler handler = m_renderStateSetters[renderStateIndex];
            if (handler == null)
                return;

            handler(renderState, m_reset == false ? m_oldRenderState : null);
        }

        // Assuming that there are very few RenderStateGuardians (e.g., 4 in the Level Editor)
        //  and these look-ups are extremely numerous, so performance is more important than
        //  memory savings here. This used to be a List<>. Max # of handlers is # of bits in an int.
        private readonly RenderStateSetHandler[] m_renderStateSetters = new RenderStateSetHandler[32];
        private bool m_reset = true;//controls m_oldRenderState
        private readonly RenderState m_oldRenderState = new RenderState();//only should be used if m_reset is false
    }
}
