//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Controls.Timelines;

#pragma warning disable 0649 // suppress "field never set" warning

namespace TimelineEditorSample.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to an Interval</summary>
    public class Interval : BaseEvent, IInterval
    {
        #region IEvent and IInterval Members

        /// <summary>
        /// Gets and sets the event's name</summary>
        public override string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.intervalType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.intervalType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the event's length (duration)</summary>
        public override float Length
        {
            get { return (float)DomNode.GetAttribute(Schema.intervalType.lengthAttribute); }
            set
            {
                float constrained = Math.Max(value, 1);                 // >= 1
                constrained = (float)MathUtil.Snap(constrained, 1.0);   // snapped to nearest integral frame number
                DomNode.SetAttribute(Schema.intervalType.lengthAttribute, constrained);
            }
        }

        /// <summary>
        /// Gets and sets the event's color</summary>
        public override Color Color
        {
            get { return Color.FromArgb((int)DomNode.GetAttribute(Schema.intervalType.colorAttribute)); }
            set { DomNode.SetAttribute(Schema.intervalType.colorAttribute, value.ToArgb()); }
        }

        /// <summary>
        /// Gets the track containing this interval</summary>
        public ITrack Track
        {
            get { return GetParentAs<Track>(); }
        }

        #endregion
    }
}





