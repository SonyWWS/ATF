//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Adaptation
{
    /// <summary>
    /// Exception that is thrown when an object can't be adapted to some required type</summary>
    public class AdaptationException : Exception
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="message">Message explaining why this object couldn't be adapted</param>
        public AdaptationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="message">Message explaining why this object couldn't be adapted</param>
        /// <param name="innerException">The exception that prevented adaptation. Will become the
        /// InnerException property.</param>
        public AdaptationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
