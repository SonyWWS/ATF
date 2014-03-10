//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Wpf.Controls.Adaptable
{
    /// <summary>
    /// Interface for auto-translate adapters, which track the mouse when enabled
    /// and adjust any ITransformAdapter</summary>
    public interface IAutoTranslateAdapter
    {
        /// <summary>
        /// Gets or sets whether auto-translation is enabled</summary>
        bool Enabled
        {
            get;
            set;
        }
    }
}
