//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Perforce
{
    /// <summary>
    /// Wraps a PerforceRequest Queue in order to reverse-link the FileInfo contained in each enqueued/dequeued request</summary>
    internal class PerforceRequestQueue
    {
        internal int Count { get { return m_requestQueue.Count; } }

        internal void Enqueue(PerforceRequest request)
        {
            if (m_requestQueue.Contains(request))
                throw new ArgumentException("Cannot add the same request twice");
            m_requestQueue.Enqueue(request);
            request.Info.RegisterRequest(request);
        }

        internal PerforceRequest Peek()
        {
            return m_requestQueue.Peek();
        }

        internal PerforceRequest Dequeue()
        {
            var request = m_requestQueue.Dequeue();
            request.Info.UnregisterRequest(request);
            return request;
        }

        internal void Clear()
        {
            while (Count > 0)
                Dequeue();
        }

        private Queue<PerforceRequest> m_requestQueue = new Queue<PerforceRequest>();
    }
}
