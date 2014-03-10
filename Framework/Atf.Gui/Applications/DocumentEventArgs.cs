//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Arguments for document events</summary>
    public class DocumentEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="document">Document</param>
        public DocumentEventArgs(IDocument document)
        {
            Document = document;
            Kind = DocumentEventType.UnKnown;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="document">Document</param>
        /// <param name="kind">Type of document event</param>
        public DocumentEventArgs(IDocument document, DocumentEventType kind)
        {
            Document = document;
            Kind = kind;
        }

        /// <summary>
        /// Document</summary>
        public readonly IDocument Document;

        /// <summary>
        /// Type of document event</summary>
        public readonly DocumentEventType Kind;
    }
}
