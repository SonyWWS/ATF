//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Arguments for event handlers related to picking</summary>
    public class HitEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs a hit event argument</summary>
        /// <param name="hitRecord">Hit record. Can be null if raising a Picking event.</param>
        /// <param name="pickRectangle">Picking rectangle in Windows client coordinates</param>
        /// <param name="mouseEvent">The mouse event, if any, associated with this pick action</param>
        public HitEventArgs(HitRecord hitRecord, RectangleF pickRectangle, MouseEventArgs mouseEvent)
        {
            m_hitRecord = hitRecord;
            PickRectangle = pickRectangle;
            MouseEvent = mouseEvent;
        }

        /// <summary>
        /// Gets or sets the HitRecord associated with a pick or "hit" that has already occurred. Is null if
        /// a picking test is taking place. Should be set by an event handler if a picking event
        /// has been considered handled.</summary>
        public HitRecord HitRecord
        {
            get { return m_hitRecord; }
            set
            {
                m_hitRecord = value;
                Handled = true;
            }
        }

        /// <summary>
        /// Whether or not this event has been handled and so the remaining event
        /// handlers should not be called</summary>
        public bool Handled;

        /// <summary>
        /// Picking rectangle in Windows client coordinates</summary>
        public readonly RectangleF PickRectangle;

        /// <summary>
        /// The mouse event associated with this picking operation or null if this HitEventArgs
        /// is for a programmatic request for an object at a certain location</summary>
        public readonly MouseEventArgs MouseEvent;

        private HitRecord m_hitRecord;
    }
}
