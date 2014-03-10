//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Runtime.Serialization;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Exception to throw when annotation is ill-formed</summary>
    public class AnnotationException : Exception
    {
        /// <summary>
        /// Constructor</summary>
        public AnnotationException()
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="message">Exception message</param>
        public AnnotationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public AnnotationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Constructor (needed for serialization)</summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected AnnotationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
