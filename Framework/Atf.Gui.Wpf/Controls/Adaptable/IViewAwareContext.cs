//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Wpf.Controls.Adaptable
{
    /// <summary>
    /// Interface for contexts which are aware of the view bound to them</summary>
    public interface IViewAwareContext
    {
        /// <summary>
        /// View currently bound to the context
        /// </summary>
        IAdaptableControl View { get; set; }
    }
}
