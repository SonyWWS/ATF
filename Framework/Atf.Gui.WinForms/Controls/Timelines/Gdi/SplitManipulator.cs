//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// OBSOLETE. Please use the much faster D2dSplitManipulator in the Sce.Atf.Controls.Timelines.Direct2D namespace.
    /// A timeline manipulator for splitting intervals into two. When hovering the mouse over an
    /// interval, the user will hold down a modifier key (Alt by default), the cursor will change
    /// to indicate where a split will take place, and on the MouseDown event, the interval will be
    /// split.</summary>
    /// <remarks>In the next release of ATF, we are planning on marking this with the Obsolete attribute.</remarks>
    public class SplitManipulator
    {
        /// <summary>
        /// Constructor that permanently attaches to the given TimelineControl by subscribing to its
        /// events</summary>
        /// <param name="owner">The TimelineControl whose events we permanently listen to</param>
        public SplitManipulator(TimelineControl owner)
        {
            m_owner = owner;
            m_owner.MouseDownPicked += owner_MouseDownPicked;
            m_owner.MouseMovePicked += owner_MouseMovePicked;
            m_owner.KeyDown += owner_KeyDown;
        }

        /// <summary>
        /// Gets and sets whether the splitting mode is active</summary>
        public bool Active
        {
            get { return m_active; }
            set
            {
                if (m_active != value)
                {
                    m_active = value;
                    if (value)
                    {   //false -> true
                        SetCursor();
                    }
                    else
                    {   //true -> false
                        ClearCursor();
                    }
                }
            }
        }

        private void owner_MouseMovePicked(object sender, HitEventArgs e)
        {
            string toolTipText = null;

            if (m_active)
            {
                e.Handled = true;

                if (e.HitRecord.Type == HitType.Interval &&
                    e.MouseEvent.Button == MouseButtons.None &&
                    !m_owner.IsUsingMouse &&
                    m_owner.IsEditable(e.HitRecord.HitPath))
                {
                    SetCursor();
                    TimelinePath hitPath = e.HitRecord.HitPath;
                    IInterval hitObject = (IInterval)e.HitRecord.HitTimelineObject;
                    float worldX = GdiUtil.InverseTransform(m_owner.Transform, e.MouseEvent.Location.X);

                    //Make sure the snap-to indicator line is drawn.
                    float delta = m_owner.GetSnapOffset(new[] { worldX }, s_snapOptions);

                    worldX += delta;
                    worldX = m_owner.ConstrainFrameOffset(worldX);

                    Matrix localToWorld = TimelineControl.CalculateLocalToWorld(hitPath);

                    if (worldX <= GdiUtil.Transform(localToWorld, hitObject.Start) ||
                        worldX >= GdiUtil.Transform(localToWorld, hitObject.Start + hitObject.Length))
                    {
                        // Clear the results since a split is impossible.
                        m_owner.GetSnapOffset(new float[] { }, s_snapOptions);
                    }
                    else
                    {
                        toolTipText = worldX.ToString();
                    }
                }
            }

            if (toolTipText != null)
                m_toolTip.Show(toolTipText, m_owner, e.MouseEvent.Location);
            else
                m_toolTip.Hide(m_owner);
        }

        private void owner_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Active = false;
        }

        private void owner_MouseDownPicked(object sender, HitEventArgs e)
        {
            TimelinePath hitPath = e.HitRecord.HitPath;
            IInterval hitInterval = e.HitRecord.HitTimelineObject as IInterval;

            if (m_active &&
                e.MouseEvent.Button == MouseButtons.Left &&
                !m_owner.IsUsingMouse &&
                m_owner.IsEditable(hitPath))
            {
                if (hitInterval != null &&
                    e.HitRecord.Type == HitType.Interval)
                {
                    float worldX =
                        Sce.Atf.GdiUtil.InverseTransform(m_owner.Transform, e.MouseEvent.Location.X);

                    worldX += m_owner.GetSnapOffset(new[] { worldX }, s_snapOptions);

                    Matrix localToWorld = TimelineControl.CalculateLocalToWorld(hitPath);

                    float fraction =
                        (worldX - GdiUtil.Transform(localToWorld, hitInterval.Start)) /
                        GdiUtil.TransformVector(localToWorld, hitInterval.Length);

                    if (m_owner.Selection.SelectionContains(hitInterval))
                        SplitSelectedIntervals(fraction);
                    else
                        SplitUnselectedInterval(hitInterval, fraction);
                }
                Active = false;
                e.Handled = true; //don't let subsequent listeners get this event
            }
        }

        private void SplitUnselectedInterval(IInterval interval, float fraction)
        {
            m_owner.TransactionContext.DoTransaction(delegate
                {
                    DoSplit(interval, fraction);
                },
                "Split Interval");
        }

        private void SplitSelectedIntervals(float fraction)
        {
            m_owner.TransactionContext.DoTransaction(delegate
                {
                    List<IInterval> newSelection = new List<IInterval>(m_owner.Selection.SelectionCount * 2);
                    newSelection.AddRange(m_owner.Selection.GetSelection<IInterval>());
                    foreach (IInterval interval in m_owner.Selection.GetSelection<IInterval>())
                    {
                        IInterval rightSide = DoSplit(interval, fraction);
                        if (rightSide != null)
                            newSelection.Add(rightSide);
                    }
                    m_owner.Selection.SetRange(newSelection);
                },
                "Split Interval");
        }

        private IInterval DoSplit(IInterval interval, float fraction)
        {
            IInterval rightSide = null;
            float originalStart = interval.Start;
            float originalLength = interval.Length;
            float worldX = originalStart + originalLength * fraction;

            worldX = m_owner.ConstrainFrameOffset(worldX);
            
            if (worldX > originalStart &&
                worldX < originalStart + originalLength)
            {
                // 'interval' becomes the left side and we create a new interval for the right side.
                rightSide = m_owner.Create(interval);
                rightSide.Start = worldX;
                rightSide.Length = originalLength - (worldX - originalStart);
                interval.Length = worldX - originalStart;
                interval.Track.Intervals.Add(rightSide);
            }
            return rightSide;
        }

        private void SetCursor()
        {
            // Other event handlers or the TimelineControl itself may have set the cursor even though
            //  'm_splitCursor' is already true. So, let's always set it.
            m_splitCursor = true;
            m_owner.Cursor = Cursors.VSplit;
        }

        private void ClearCursor()
        {
            if (m_splitCursor)
            {
                m_owner.GetSnapOffset(new float[] { }, null);

                m_splitCursor = false;
                m_owner.Cursor = Cursors.Default;
                m_toolTip.Hide(m_owner);
            }
        }

        static SplitManipulator()
        {
            s_snapOptions = new TimelineControl.SnapOptions();
            s_snapOptions.IncludeSelected = true;
        }

        private readonly TimelineControl m_owner;
        
        private bool m_active; //is the splitting mode active?
        private bool m_splitCursor; //if we've set the cursor, we need to know to restore it
        private ToolTip m_toolTip = new ToolTip();

        private static readonly TimelineControl.SnapOptions s_snapOptions;
    }
}
