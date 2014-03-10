//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf
{
    /// <summary>
    /// Interface for objects that are initializable. This concept is useful for objects
    /// that can't be fully initialized in their constructor, e.g. objects in Dependency
    /// Injection containers, or objects created by a Factory.</summary>
    /// <remarks>This interface is special to ATF's MEF support. See Sce.Atf.MefUtil.</remarks>
    public interface IInitializable
    {
        /// <summary>
        /// Initializes the object</summary>
        void Initialize();
    }
}
