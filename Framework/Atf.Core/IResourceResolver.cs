//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// MEF component able to resolve a specified URI into an IResource object</summary>
    /// <remarks>You can register multiple resolvers with MEF. The ResourceService (default implementation)
    /// goes through all registered resolvers until one returns a non-null IResource.
    /// Don't assume any order in which resolvers are used. If your application requires ordering, you can 
    /// create a master resolver and handle dispatch and ordering yourself.</remarks>
    public interface IResourceResolver
    {
        /// <summary>
        /// Attempts to resolve (e.g., load from a file) the resource associated with the given URI</summary>
        /// <param name="uri">Resource URI</param>
        /// <returns>The resolved resource or null if there was a failure of some kind</returns>
        IResource Resolve(Uri uri);
    }
}
