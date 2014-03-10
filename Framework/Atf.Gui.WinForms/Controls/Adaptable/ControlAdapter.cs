//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Base class for control adapters</summary>
    public abstract class ControlAdapter : IControlAdapter
    {
        #region IControlAdapter Members

        /// <summary>
        /// Gets the adapted control</summary>
        public AdaptableControl AdaptedControl
        {
            get { return m_control; }
        }

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        /// <remarks>By convention, the bottom-most layer should appear first in ControlAdapter's
        /// Adapt() method, and so the Paint event should be subscribed to in Bind().</remarks>
        void IControlAdapter.Bind(AdaptableControl control)
        {
            m_control = control;
            Bind(control);
        }

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the reverse order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        /// <remarks>By convention, the bottom-most layer should appear first in ControlAdapter's
        /// Adapt() method, and so mouse events should be subscribed to in BindReverse().</remarks>
        void IControlAdapter.BindReverse(AdaptableControl control)
        {
            BindReverse(control);
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        void IControlAdapter.Unbind(AdaptableControl control)
        {
            Unbind(control);
            m_control = null;
        }

        #endregion

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected virtual void Bind(AdaptableControl control)
        {
        }

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the reverse order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected virtual void BindReverse(AdaptableControl control)
        {
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected virtual void Unbind(AdaptableControl control)
        {
        }

        private AdaptableControl m_control;
    }
}
