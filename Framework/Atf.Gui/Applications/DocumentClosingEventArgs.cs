//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Arguments passed when a document is closing, allows subscribers to cancel the close operation</summary>
    public class DocumentClosingEventArgs : DocumentEventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="document">Closing document</param>
        public DocumentClosingEventArgs(IDocument document)
            : base(document)
        {
        }

        /// <summary>
        /// Gets or sets whether to cancel the close operation; set true to cancel</summary>
        public bool Cancel { get; set; }
    }
}
