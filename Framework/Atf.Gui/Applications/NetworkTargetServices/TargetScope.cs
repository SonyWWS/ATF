//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Specifies whether the target is persisted for the current application only, the current user, or all users on a machine</summary>
    public enum TargetScope
    {
        /// <summary>
        /// Save target data for the current application. All users on this computer see this target when they run this application. 
        /// Other applications that use Target Manager do not see this target.</summary>
        PerApp,
        /// <summary>
        /// Save target data for the current user. Other applications that use the Target Manager can see this target for this user; 
        /// other users won't see this target in any application.</summary>
        PerUser,
        /// <summary>
        /// Save the target data for all users and applications, so this target is visible for all users in any application 
        /// that uses the Target Manager framework</summary>
        AllUsers
    }
}
