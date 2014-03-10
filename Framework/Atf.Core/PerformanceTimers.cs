//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Sce.Atf.GraphicsApplications
{
    /// <summary>
    /// A utility class that uses a single system stop watch to time any number of distinct
    /// types of events, identified by a supplied name. Can prepare a simple report.</summary>
    public class PerformanceTimers
    {
        /// <summary>
        /// Useful for counting start/stop cycles. Clears all timing data and phase names.
        /// Is not necessary to call explicitly before StartPhase. StartPhase calls
        /// this method if necessary.</summary>
        public void Start()
        {
            m_times.Clear();
            m_stopWatch.Reset();
            m_startTime = m_stopWatch.ElapsedMilliseconds;
            m_stopWatch.Start();
            m_started = true;
        }

        /// <summary>
        /// Increments the start/stop cycle count. The next call to StartPhase or Start clears
        /// all timing data.</summary>
        public void Stop()
        {
            m_stopWatch.Stop();
            long time = m_stopWatch.ElapsedMilliseconds;
            m_cycleTime = time - m_startTime;

            m_startStopCycles++;
            m_started = false;
        }

        /// <summary>
        /// Clears all timing data, phase names, and the number of start/stop cycles except
        /// for the CycleTime</summary>
        public void Clear()
        {
            CheckNotStarted();

            m_startStopCycles = 0;
            m_times.Clear();
        }

        /// <summary>
        /// The number of times Stop has been called since the last Clear or since this
        /// object was created</summary>
        public int StartStopCycles
        {
            get { return m_startStopCycles; }
        }

        /// <summary>
        /// The time, in miliseconds, between the last Start and Stop calls</summary>
        public long CycleTime
        {
            get { return m_cycleTime; }
        }

        /// <summary>
        /// Begins timing a named phase. Call StopPhase when the phase ends.</summary>
        /// <param name="name">Unique name of this phase. Is displayed to the user.</param>
        public void StartPhase(string name)
        {
            CheckStarted();

            long time = m_stopWatch.ElapsedMilliseconds;
            m_times[name] = time;
        }

        /// <summary>
        /// Stops the previously started phase</summary>
        /// <param name="id">Unique ID of this phase. Is displayed to the user.</param>
        public void StopPhase(string id)
        {
            CheckStarted();

            long startTime;
            if (!m_times.TryGetValue(id, out startTime))
                startTime = m_startTime;

            long time = m_stopWatch.ElapsedMilliseconds;
            time = time - startTime;

            m_times[id] = time;
        }

        /// <summary>
        /// Gets the phase time, in miliseconds, for the given phase name</summary>
        /// <param name="id">Phase name</param>
        /// <returns>Phase time, in miliseconds, for the given phase name</returns>
        public long GetPhaseTime(string id)
        {
            long time;
            m_times.TryGetValue(id, out time);
            return time;
        }

        /// <summary>
        /// Formats a string useful for displaying results to the user, with phases sorted
        /// in alphabetical order</summary>
        /// <returns>Timing results of phases, sorted in alphabetical order</returns>
        public string GetDisplayString()
        {
            StringBuilder sb = new StringBuilder();

            List<KeyValuePair<string, long>> times = new List<KeyValuePair<string, long>>(m_times);
            foreach (KeyValuePair<string, long> time in times)
            {
                sb.Append(time.Key);
                sb.Append(": ");
                sb.Append(time.Value.ToString());
                sb.Append(" ms ");
            }

            return sb.ToString();
        }

        private void CheckStarted()
        {
            if (!m_started)
                Start();
        }

        private void CheckNotStarted()
        {
            if (m_started)
                Stop();
        }

        private readonly Stopwatch m_stopWatch = new Stopwatch();

        private readonly SortedDictionary<string, long> m_times = new SortedDictionary<string, long>();

        private long m_startTime;
        private long m_cycleTime;
        private int m_startStopCycles;

        private bool m_started;
    }
}
