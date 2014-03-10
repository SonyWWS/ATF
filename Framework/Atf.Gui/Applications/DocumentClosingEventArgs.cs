//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Arguments passed when a document is closing, allows subscribers to cancel the close operation</summary>
    public class DocumentClosingEventArgs : DocumentEventArgs
    {
        public DocumentClosingEventArgs(IDocument document)
            : base(document)
        {
        }

        /// <summary>
        /// Set to true in order to cancel the close operation</summary>
        public bool Cancel { get; set; }
    }
}
