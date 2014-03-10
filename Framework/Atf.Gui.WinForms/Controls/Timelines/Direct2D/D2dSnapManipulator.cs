//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Timelines.Direct2D
{
    /// <summary>
    /// A timeline manipulator intended to be used by other manipulators to allow for snapping
    /// selected objects to non-selected events and give a visual indication of this</summary>
    public class D2dSnapManipulator
    {
        /// <summary>
        /// Constructor that permanently attaches to the given TimelineControl by subscribing to its
        /// events</summary>
        /// <param name="owner">The TimelineControl whose events we permanently listen to</param>
        public D2dSnapManipulator(D2dTimelineControl owner)
        {
            m_owner = owner;
            m_owner.GetSnapOffset = new D2dTimelineControl.SnapOffsetFinder(GetSnapOffset);
            m_owner.DrawingD2d += owner_DrawingD2d;
            m_owner.MouseUp += owner_MouseUp;
            m_owner.MouseDown += owner_MouseDown;
            m_owner.KeyDown += owner_KeyDown;
        }

        /// <summary>
        /// Gets or sets the scrubber manipulator (e.g., a D2dScrubberManipulator) object that is used
        /// with the same TimelineControl as this SnapManipulator. If not null, the scrubber
        /// manipulator is included in snapping calculations.</summary>
        public IEvent Scrubber
        {
            get { return m_scrubber; }
            set { m_scrubber = value; }
        }

        /// <summary>
        /// Gets or sets the snapping tolerance, in pixels</summary>
        [DefaultValue(10.0f)]
        public static float SnapTolerance
        {
            get { return s_snapTolerance; }
            set { s_snapTolerance = value; }
        }

        /// <summary>
        /// Gets or sets the color of the snapping indicator vertical line</summary>
        [DefaultValue(typeof(Color), "DarkRed")]
        public static Color Color
        {
            get { return s_color; }
            set { s_color = value; }
        }

        /// <summary>
        /// Gets or sets the modifier key (Shift, Ctrl, or Alt) or combination of modifier keys
        /// that are required for snapping to occur. Additional modifier keys that are pressed
        /// still allow activation to occur.</summary>
        [DefaultValue(Keys.None)]
        public static Keys ActivatorKeys
        {
            get { return s_activatorKeys; }
            set { s_activatorKeys = value; }
        }

        /// <summary>
        /// Gets or sets the modifier key (Shift, Ctrl, or Alt) or combination of modifier keys
        /// that prevent snapping from occurring. Additional modifier keys that are pressed
        /// still allow activation to occur.</summary>
        [DefaultValue(Keys.Shift)]
        public static Keys DeactivatorKeys
        {
            get { return s_deactivatorKeys; }
            set { s_deactivatorKeys = value; }
        }

        /// <summary>
        /// Causes the results of the last GetSnapOffset to be reset so that the snap-to-line is no
        /// longer drawn</summary>
        public void CancelSnap()
        {
            if (m_snapInfo.Count > 0)
            {
                m_snapInfo.Clear();
                m_owner.Invalidate();
            }
        }

        private class SnapOffsetInfo
        {
            public SnapOffsetInfo(float snapFromPoint)
            {
                m_snapFromPoint = snapFromPoint;
                m_dist = float.MaxValue;
            }

            public void Update(float snapToPoint, IEvent snapToEvent, float tolerance)
            {
                float dist = Math.Abs(snapToPoint - m_snapFromPoint);
                if (dist < tolerance &&
                    dist < m_dist)
                {
                    m_snapToPoint = snapToPoint;
                    m_snapToEvent = snapToEvent;
                    m_dist = dist;
                }
            }

            public static void RemoveInvalid(List<SnapOffsetInfo> infos)
            {
                float smallestDist = float.MaxValue;
                for (int i = 0; i < infos.Count; i++)
                {
                    SnapOffsetInfo info = infos[i];
                    if (info.m_dist < smallestDist)
                        smallestDist = info.m_dist;
                }

                int nextValid = 0;
                for (int i = 0; i < infos.Count; i++)
                {
                    SnapOffsetInfo info = infos[i];
                    
                    // if smallestDist is still MaxValue, then adding the tolerance has no effect
                    if (info.m_dist < smallestDist + 0.0001f)
                    {
                        infos[nextValid++] = info;
                    }
                }

                infos.RemoveRange(nextValid, infos.Count - nextValid);
            }

            //SnapToPoint - SnapFromPoint
            public float Offset { get { return m_snapToPoint - m_snapFromPoint; } }

            public float SnapToPoint { get { return m_snapToPoint; } }

            private readonly float m_snapFromPoint;
            private float m_dist;
            private float m_snapToPoint;
            private IEvent m_snapToEvent;
        }

        /// <summary>
        /// Gets the offset from one of the world snap points to the closest non-selected object's edge</summary>
        /// <param name="movingPoints">The x-coordinates to snap "from", in world coordinates</param>
        /// <param name="options">The options to control the behavior. If null, the defaults are used.</param>
        /// <returns>The value to be added, to GetDragOffset().X, for example. Is in world coordinates.</returns>
        private float GetSnapOffset(IEnumerable<float> movingPoints, D2dTimelineControl.SnapOptions options)
        {
            //we want to recalculate m_snapInfo and reflect changes to the snap-to lines
            m_snapInfo.Clear();
            m_owner.Invalidate();

            if (options == null)
                options = new D2dTimelineControl.SnapOptions();

            // Check for user-forced snapping and no-snapping.
            if (options.CheckModifierKeys)
            {
                Keys modKeys = Control.ModifierKeys;
                if (s_deactivatorKeys != Keys.None &&
                    (modKeys & s_deactivatorKeys) == s_deactivatorKeys)
                    return 0.0f;
                if (s_activatorKeys != Keys.None &&
                    (modKeys & s_activatorKeys) != s_activatorKeys)
                    return 0.0f;
            }

            // Prepare helper object on each moving point.
            foreach (float snapping in movingPoints)
            {
                m_snapInfo.Add(new SnapOffsetInfo(snapping));
            }
            if (m_snapInfo.Count == 0)
                return 0.0f;
            
            // Find the closest IEvent.
            float worldSnapTolerance = GdiUtil.InverseTransformVector(m_owner.Transform, s_snapTolerance);

            List<TimelinePath> events = new List<TimelinePath>(
                D2dTimelineControl.GetObjects<IEvent>(m_owner.Timeline));

            // Allow for snapping to a scrubber manipulator.
            if (m_scrubber != null && options.IncludeScrubber)
                events.Add(new TimelinePath(m_scrubber));

            foreach (TimelinePath path in events)
            {
                if (options.IncludeSelected ||
                    !m_owner.Selection.SelectionContains(path))
                {
                    IEvent snapToEvent = (IEvent)path.Last;

                    if (options.Filter == null ||
                        options.Filter(snapToEvent, options))
                    {
                        Matrix localToWorld = D2dTimelineControl.CalculateLocalToWorld(path);
                        float start, length;
                        GetEventDimensions(snapToEvent, localToWorld, out start, out length);

                        foreach (SnapOffsetInfo info in m_snapInfo)
                        {
                            info.Update(start, snapToEvent, worldSnapTolerance);
                            if (length > 0)
                                info.Update(start + length, snapToEvent, worldSnapTolerance);
                        }
                    }
                }
            }

            // Keep only the shortest distance snap-to points. Could be multiple in case of tie.
            SnapOffsetInfo.RemoveInvalid(m_snapInfo);
            if (m_snapInfo.Count == 0)
                return 0.0f;

            SnapOffsetInfo topInfo = m_snapInfo[0];
            return topInfo.Offset;
        }

        private void GetEventDimensions(IEvent snapToEvent, Matrix localToWorld, out float start, out float length)
        {
            start = GdiUtil.Transform(localToWorld, snapToEvent.Start);
            length = GdiUtil.TransformVector(localToWorld, snapToEvent.Length);
        }

        /// <summary>
        /// Draws the snap-to indicator to give the user a visual cue that snapping is occurring</summary>
        /// <param name="sender">The TimelineControl whose Paint event is being raised</param>
        /// <param name="e">The paint event args</param>
        /// <remarks>Draws a vertical line at the snapping location</remarks>
        private void owner_DrawingD2d(object sender, EventArgs e)
        {
            if (m_snapInfo.Count == 0)
                return;

            D2dGraphics g = m_owner.D2dGraphics;
            Rectangle clipRectangle = m_owner.VisibleClientRectangle;
            try
            {
                g.PushAxisAlignedClip(clipRectangle);
                Matrix worldToView = m_owner.Transform;

                foreach (SnapOffsetInfo info in m_snapInfo)
                {
                    float viewXCoord = GdiUtil.Transform(worldToView, info.SnapToPoint);
                    g.DrawLine(viewXCoord, clipRectangle.Top, viewXCoord, clipRectangle.Bottom, s_color, 3.0f, null);
                }
            }
            finally
            {
                g.PopAxisAlignedClip();
            }
        }

        private void owner_MouseUp(object sender, MouseEventArgs e)
        {
            CancelSnap();
        }

        private void owner_MouseDown(object sender, MouseEventArgs e)
        {
            CancelSnap();
        }

        private void owner_KeyDown(object sender, KeyEventArgs e)
        {
            if (s_deactivatorKeys != Keys.None &&
                (e.Modifiers & s_deactivatorKeys) == s_deactivatorKeys)
                CancelSnap();
        }

        private readonly D2dTimelineControl m_owner;
        private IEvent m_scrubber;
        private readonly List<SnapOffsetInfo> m_snapInfo = new List<SnapOffsetInfo>(2);

        private static float s_snapTolerance = 10.0f;
        private static Color s_color = Color.DarkRed;
        private static Keys s_deactivatorKeys = Keys.Shift;
        private static Keys s_activatorKeys = Keys.None;
    }
}
