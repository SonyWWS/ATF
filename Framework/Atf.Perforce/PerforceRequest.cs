//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Perforce
{
    /// <summary>
    /// Request for perforce command execution</summary>
    internal class PerforceRequest 
    {    
        /// <summary>
        /// Constructor</summary>
        /// <param name="info"></param>
        /// <param name="doRequest">Requested perforce command</param>
        public PerforceRequest(FileInfo info, Func<Uri, bool> doRequest) 
        { 
            Info = info; 
            DoRequest = doRequest;
        }

        public bool Execute()
        {
            bool result = (DoRequest != null) && DoRequest(Uri);
            Info.MarkAsDirty();
            return result; 
        }

        public FileInfo Info { get; private set; }
        public Uri Uri { get { return Info.Uri; } }

        private Func<Uri, bool> DoRequest { get; set; }
    }
}
