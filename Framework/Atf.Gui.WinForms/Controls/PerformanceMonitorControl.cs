//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Sce.Atf.Applications.Controls
{
    /// <summary>
    /// Control that displays the rendering performance of a Control</summary>
    public partial class PerformanceMonitorControl : UserControl
    {
        /// <summary>
        /// Constructor</summary>
        public PerformanceMonitorControl()
        {
            InitializeComponent();

            m_stopWatch.Start();
            m_lastIntervalStart = m_stopWatch.Elapsed;
            m_timer.Interval = Interval;
            m_timer.Tick += TimerTick;
        }

        /// <summary>
        /// Raises the System.Windows.Forms.Control.VisibleChanged event</summary>
        /// <param name="e">A System.EventArgs that contains the event data</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            m_timer.Enabled = Visible;
        }

        /// <summary>
        /// Sets the given target Control as the Control whose performance is monitored by
        /// listening to its Paint event</summary>
        /// <param name="target">Target Control, or null to remove existing binding</param>
        /// <param name="name">The readable name of the Control, to indicate to the user
        /// which Control has the focus</param>
        public void Bind(Control target, string name)
        {
            var binding = new ControlAdapter(target);
            Bind(binding, name);
        }

        /// <summary>
        /// Sets the given target using a generic binding info object</summary>
        /// <param name="target">The binding info for the target object, or null to remove existing
        /// binding</param>
        /// <param name="name">The readable name of the Control, to indicate to the user
        /// which Control has the focus. Can't be null</param>
        /// <remarks>Use this for special cases, such as when the Paint event isn't raised on a Control</remarks>
        public void Bind(IPerformanceTarget target, string name)
        {
            if (m_target == target &&
                m_targetName.Equals(name))
                return;

            if (m_target != null)
                m_target.EventOccurred -= TargetEventHandler;

            m_target = target;
            m_targetName = name;

            if (m_target != null)
                m_target.EventOccurred += TargetEventHandler;

            controlNameLabel.Text = m_targetName;
        }

        private void TargetEventHandler(object sender, EventArgs e)
        {
            if (m_timer.Enabled)
            {
                m_intervalFrameCount++;
                m_frameCountSinceReset++;
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            // The desired interval of time from m_timer can be far off from the real interval
            //  time due to delays in windows messages when the app is under high CPU load.
            TimeSpan now = m_stopWatch.Elapsed;
            TimeSpan actualInterval = now - m_lastIntervalStart;
            m_lastIntervalStart = now;

            m_fps = (float)(m_intervalFrameCount * 1000 / actualInterval.TotalMilliseconds);
            m_intervalFrameCount = 0;

            if (m_fps > m_maxFps)
                m_maxFps = m_fps;
            
            m_managedBytes = GC.GetTotalMemory(false);

            Process currentProcess = Process.GetCurrentProcess();
            m_unmanagedBytes = currentProcess.WorkingSet64;//the physical RAM used by this Process

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            FpsLabel.Text = m_fps.ToString();
            MaxFpsLabel.Text = m_maxFps.ToString();
            NumPaintsLabel.Text = m_frameCountSinceReset.ToString();
            ManagedMemoryLabel.Text = (m_managedBytes / 1024).ToString("N0") + " K";
            UnmanagedMemoryLabel.Text = (m_unmanagedBytes / 1024).ToString("N0") + " K";

            // Under a high load situation, it may take too long to wait for our paint message.
            // Update the display of the stats immediately.
            Refresh();
        }

        private void ResetBtn_Click(object sender, EventArgs e)
        {
            // Reset everything but m_intervalFrameCount since that is tied to the TimerTick event
            //  and needs to be correct over Interval miliseconds.
            m_fps = 0;
            m_maxFps = 0;
            m_frameCountSinceReset = 0;
            UpdateDisplay();
        }

        private void ClipboardBtn_Click(object sender, EventArgs e)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Control's Name: {0}".Localize(), controlNameLabel.Text);
            builder.AppendLine();
            builder.AppendFormat("Max frames per second: {0}".Localize(), m_maxFps);
            builder.AppendLine();
            builder.AppendFormat("Total frame count: {0}".Localize(), m_frameCountSinceReset);
            builder.AppendLine();
            builder.AppendFormat("Managed memory (KB): {0}".Localize(), (m_managedBytes / 1024));
            builder.AppendLine();
            builder.AppendFormat("Unmanaged memory (KB): {0}".Localize(), (m_unmanagedBytes / 1024));
            builder.AppendLine();
            Clipboard.SetText(builder.ToString());
        }

        private void GarbageCollectionBtn_Click(object sender, EventArgs e)
        {
            GC.Collect();
            UpdateDisplay();
        }

        private void StressTestBtn_Click(object sender, EventArgs e)
        {
            // Reset everything and then do as many refreshes as possible in N seconds
            ResetBtn_Click(sender, e);
            m_target.EventOccurred -= TargetEventHandler;
            string originalStressText = StressTestBtn.Text;
            StressTestBtn.Enabled = false;
            m_timer.Tick -= TimerTick;
            TimeSpan lastIntervalStart = m_stopWatch.Elapsed;
            var allFrames = new List<long>(1000);

            try
            {
                while (true)
                {
                    long startTick = m_stopWatch.ElapsedTicks;

                    m_target.DoEvent();

                    long endTick = m_stopWatch.ElapsedTicks;
                    allFrames.Add(endTick - startTick);

                    // Update the frame count
                    TargetEventHandler(sender, e);

                    // Has the stress test duration elapsed?
                    TimeSpan now = m_stopWatch.Elapsed;
                    TimeSpan actualInterval = now - lastIntervalStart;
                    if (actualInterval.TotalMilliseconds >= StressTestDuration)
                        break;

                    // Update the countdown timer
                    int secondsRemaining = (StressTestDuration / 1000) - (int)actualInterval.TotalSeconds;
                    string newCountdown = secondsRemaining.ToString();
                    if (!StressTestBtn.Text.Equals(newCountdown))
                    {
                        StressTestBtn.Text = newCountdown;
                        StressTestBtn.Refresh();
                    }
                }
            }
            finally
            {
                StressTestBtn.Text = originalStressText;
                StressTestBtn.Enabled = true;
                m_target.EventOccurred += TargetEventHandler;
                m_timer.Interval = Interval;
                m_timer.Tick += TimerTick;
                TimerTick(sender, e);

                if (allFrames.Count > 0)
                {
                    allFrames.Sort(); //sort from lowest to highest
                    long meanTicks = allFrames[allFrames.Count / 2];
                    long fastestTicks = allFrames[0];
                    long slowestTicks = allFrames[allFrames.Count - 1];
                    long meanMs = (meanTicks * 1000) / Stopwatch.Frequency;
                    long fastestMs = (fastestTicks * 1000) / Stopwatch.Frequency;
                    long slowesttMs = (slowestTicks * 1000) / Stopwatch.Frequency;
                    var report = new StringBuilder();
                    report.AppendFormat("Target: {0}".Localize("'target' is the Windows Control that is being analyzed"), m_targetName);
                    report.AppendLine();
                    report.AppendFormat("Number of rendered frames: {0}".Localize(), allFrames.Count);
                    report.AppendLine();
                    report.AppendFormat("Mean rendering time: {0}ms or {1} ticks".Localize(), meanMs, meanTicks);
                    report.AppendLine();
                    report.AppendFormat("Fastest rendering time: {0}ms or {1} ticks".Localize(), fastestMs, fastestTicks);
                    report.AppendLine();
                    report.AppendFormat("Slowest rendering time: {0}ms or {1} ticks".Localize(), slowesttMs, slowestTicks);
                    report.AppendLine();
                    Clipboard.SetText(report.ToString()); 
                    MessageBox.Show(report.ToString(), "The performance report is in the clipboard".Localize());
                }
            }
        }

        /// <summary> 
        /// Cleans up any resources being used</summary>
        /// <param name="disposing">True iff managed resources should be disposed</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
                m_timer.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// A convenience class for adapting a Control to the IPerformanceTarget interface, and
        /// mapping the Control's Paint event to the EventOccurred event</summary>
        public class ControlAdapter : IPerformanceTarget
        {
            public ControlAdapter(Control control)
            {
                m_control = control;
            }

            #region IPerformanceTarget

            /// <summary>
            /// Does the event whose duration is measured and then returns. Raising the
            /// EventOccurred event is optional.</summary>
            /// <example>For example, if we are monitoring the time it takes a window to redraw,
            /// this event would be the equivalent of Refresh() on a control.</example>
            public void DoEvent()
            {
                m_control.Refresh();
            }

            /// <summary>
            /// Event that is raised on the object being measured to indicate that
            /// another timing sample should be taken, so that the number of events per second
            /// can be calculated.</summary>
            /// <remarks>For example, if we are monitoring the time it takes a window to redraw,
            /// this event might be raised for every Paint event on the control.</remarks>
            public event EventHandler EventOccurred
            {
                add
                {
                    if (m_eventOccurred == null)
                        m_control.Paint += PaintEventHandler;
                    m_eventOccurred += value;
                }
                remove
                {
                    m_eventOccurred -= value;
                    if (m_eventOccurred == null)
                        m_control.Paint -= PaintEventHandler;
                }
            }

            #endregion

            private event EventHandler m_eventOccurred;

            private void PaintEventHandler(object sender, PaintEventArgs e)
            {
                m_eventOccurred.Raise(this, EventArgs.Empty);
            }

            private readonly Control m_control;
        }

        private IPerformanceTarget m_target;
        private string m_targetName = string.Empty;
        private int m_intervalFrameCount;
        private int m_frameCountSinceReset;
        private TimeSpan m_lastIntervalStart;
        private float m_fps, m_maxFps;
        private long m_managedBytes;
        private long m_unmanagedBytes;

        private readonly Stopwatch m_stopWatch = new Stopwatch();
        private readonly Timer m_timer = new Timer();
        
        //# of miliseconds between updates of framerate, etc.
        private const int Interval = 2500;
        private const int StressTestDuration = 5000;
    }
}
