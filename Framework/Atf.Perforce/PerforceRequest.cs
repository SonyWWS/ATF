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
        /// <param name="doAfterCompleted">Action after request is completed</param>
        public PerforceRequest(FileInfo info, Func<Uri, bool> doRequest, Func<Uri, bool> doAfterCompleted) 
        { 
            Info = info; 
            DoRequest = (u, d) => doRequest(u);
            DoAfterCompleted = doAfterCompleted; 
        }

        public bool Execute()
        { 
            bool result = (DoRequest != null) && DoRequest(Uri, Description); 
            Executed = true;

            //if (result)
            //    DoAfterCompleted(Uri);

            Info.MarkAsDirty();
            return result; 
        }

        public bool Executed { get; private set; }
        public FileInfo Info { get; private set; }
        public Uri Uri { get { return Info.Uri; } }
        public string Description { get; private set; }

        private Func<Uri, string, bool> DoRequest { get; set; }
        private Func<Uri, bool> DoAfterCompleted { get; set; }
    }

 
}
