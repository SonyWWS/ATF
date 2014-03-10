//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Interface for control adapters</summary>
    public interface IControlAdapter
    {
        /// <summary>
        /// Gets the adapted control</summary>
        AdaptableControl AdaptedControl
        {
            get;
        }

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        /// <remarks>By convention, the bottom-most layer should appear first in ControlAdapter's
        /// Adapt() method, and so the Paint event should be subscribed to in Bind().</remarks>
        void Bind(AdaptableControl control);

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the reverse order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        /// <remarks>By convention, the bottom-most layer should appear first in ControlAdapter's
        /// Adapt() method, and so mouse events should be subscribed to in BindReverse().</remarks>
        void BindReverse(AdaptableControl control);

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        void Unbind(AdaptableControl control);
    }
}
