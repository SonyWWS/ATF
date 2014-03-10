//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Perforce.P4;

using Sce.Atf.Applications;

namespace Sce.Atf.Perforce
{
    /// <summary>
    /// Encapsulates a cached P4 file record, exposes select properties of that record.
    /// Last cache update is timestamped, allowing queries of whether it is time to refresh</summary>
    internal class FileInfo
    {
        internal FileInfo(Uri uri) { Uri = uri; }

        internal readonly Uri Uri;

        private List<PerforceRequest> m_requests = new List<PerforceRequest>();

        internal void RegisterRequest(PerforceRequest request)
        {
            if (m_requests.Contains(request))
                throw new ArgumentException("Request has already been registered");
            m_requests.Add(request);
        }
        internal void UnregisterRequest(PerforceRequest request)
        {
            if (!m_requests.Contains(request))
                throw new ArgumentException("Request wasn't registered with this instance");
            m_requests.Remove(request);
        }

        internal SourceControlStatus Status
        {
            get
            {
                if (m_cachedStatus != SourceControlStatus.Unknown)
                    return m_cachedStatus;

                if (m_requests.Count > 0)
                    return SourceControlStatus.Unknown;

                var result = SourceControlStatus.Unknown;

                  if (NotControlled)
                {
                    result = SourceControlStatus.NotControlled;
                }
                else if (NotInClientView)
                {
                    //just return SourceControlStatus.Unknown
                }
                else if (m_cachedRecord != null)
                {
                    if (m_cachedRecord["dummy"] != null)
                    {
                        result = SourceControlStatus.NotControlled;
                    }
                    else
                    {
                        string action = m_cachedRecord["action"];

                        // CachedRecord.Record["action"] is null if the file is in Perforce and is not checked-out by anyone.
                        if (action != null)
                        {
                            switch (action)
                            {
                                case "edit": result = SourceControlStatus.CheckedOut; break;
                                case "add": result = SourceControlStatus.Added; break;
                                case "delete": result = SourceControlStatus.Deleted; break;
                            }
                        }
                        else
                        {
                            // The file could have been previously deleted from Perforce.
                            action = m_cachedRecord["headAction"];//TOFIX
                            result = action == "delete" ? SourceControlStatus.NotControlled : SourceControlStatus.CheckedIn;
                        }
                    }
                }

                m_cachedStatus = result;
                return result;
            }
        }

        internal int HeadRevision
        {
            get
            {
                if (m_cachedRecord == null)
                    return 0;
                string headRev =  m_cachedRecord["headRev"];
                return (headRev != null && headRev != "none") ? Convert.ToInt32(headRev) : 0;
            }
        }

        internal int Revision
        {
            get
            {
                if (m_cachedRecord == null)
                    return 0;
                string haveRev = m_cachedRecord["haveRev"];
                return (haveRev != null && haveRev != "none") ? Convert.ToInt32(haveRev) : 0;
            }
        }

        internal bool IsLocked
        {
            get
            {
                if (m_cachedRecord == null)
                    return false;
                return (m_cachedRecord.OurLock || m_cachedRecord["otherLock"] != null);
            }
        }

        internal string[] OtherUsers // other users have the file checked out
        {
            get
            {
                var otherUsers = new List<string>();
                if (m_cachedRecord != null)
                {

                    string otherOpen = m_cachedRecord["otherOpen"]; //the number of other users who have the file open
                    int numOtherOpen;
                    if (int.TryParse(otherOpen, out numOtherOpen))
                    {
                        if (numOtherOpen > 0)
                        {
                            for (int i = 0; i < numOtherOpen; ++i)
                            {
                                string otherUser = m_cachedRecord["otherOpen" + Convert.ToString(i)];
                                otherUsers.Add(otherUser);
                            }
                        }
                    }
                }

                return otherUsers.ToArray();
            }
        }

        internal string OtherLock // if another user has the file locked
        {
            get
            {
                if (m_cachedRecord != null)
                {
                   return m_cachedRecord["otherLock0"] ?? string.Empty;
                }
                return string.Empty;
            }
        }

        internal bool RequiresRefresh(double expireTime)
        {
            // Can't refresh until all requests are processed
            if (m_requests.Count > 0)
                return false;

            // No record forces refresh, or one marked as dirty
            if (m_cachedRecord == null || m_dirty)
                return true;

            // Refresh if record timestamp is stale and there's no pending requests on this FileInfo
            var deadline = m_cachedRecord.Timestamp + TimeSpan.FromSeconds(expireTime);
            return deadline < DateTime.UtcNow;
        }

        internal void UpdateRecord(TaggedObject record)
        {
            m_cachedRecord = new CachedP4Record(record);
            m_cachedStatus = SourceControlStatus.Unknown; // invalidate cache status
            NotControlled = false;
            NotInClientView = false;
            m_dirty = false;
        }

        internal void ClearRecord()
        {
            m_cachedRecord = null;
            m_cachedStatus = SourceControlStatus.Unknown;
        }

        // two special cases that have no cache record
        private bool m_notControlled = false;
        internal bool NotControlled
        {
            get { return m_notControlled; }
            set { m_notControlled = value; ResetTimestamp(); } 
        }

        private bool m_notInClientView = false;
        internal bool NotInClientView
        {
            get { return m_notInClientView; }
            set { m_notInClientView = value; ResetTimestamp(); } 
        }

        private void ResetTimestamp()
        {
            if (m_cachedRecord != null)
                m_cachedRecord.ResetTimestamp();
        }
       
        internal void MarkAsDirty() { m_dirty = true; }

        private class CachedP4Record
        {
            public CachedP4Record(TaggedObject record)
            {
                Record = record;
                ResetTimestamp();
            }

            internal string this[string key]
            {
                get
                {
                    if (Record != null && Record.ContainsKey(key))
                        return Record[key];
                    return null;
                }
            }

            /// <summary>
            /// If the current user has the file locked
            /// </summary>
            internal bool OurLock
            {
                get
                {
                    if (Record != null && Record.ContainsKey("ourLock"))
                        return true;
                    return false;
                }
            }

            private TaggedObject Record { get;  set; }

            internal DateTime Timestamp { get; private set; }

            internal void ResetTimestamp() { Timestamp = DateTime.UtcNow; }
        }

        private CachedP4Record m_cachedRecord { get; set; }
        private SourceControlStatus m_cachedStatus = SourceControlStatus.Unknown;
        private bool m_dirty;
    }
}
