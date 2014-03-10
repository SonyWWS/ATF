//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

#pragma warning disable 0649 // suppress "field never set" warning
#pragma warning disable 0169 // suppress "field never used" warning

namespace TimelineEditorSample.DomNodeAdapters
{
    /// <summary>
    /// Class that adapts a DomNode to an event; a base class for adapters for Intervals, Markers, and Keys</summary>
    public class BaseEvent : DomNodeAdapter, IEvent, ICloneable
    {
        #region IEvent Members

        /// <summary>
        /// Gets and sets the event's name</summary>
        public virtual string Name
        {
            get { return string.Empty; }
            set { }
        }

        /// <summary>
        /// Gets and sets the event's start time</summary>
        public float Start
        {
            get { return (float)DomNode.GetAttribute(Schema.eventType.startAttribute); }
            set
            {
                float constrained = Math.Max(value, 0);                 // >= 0
                constrained = (float)MathUtil.Snap(constrained, 1.0);   // snapped to nearest integral frame number
                DomNode.SetAttribute(Schema.eventType.startAttribute, constrained);
            }
        }

        /// <summary>
        /// Gets and sets the event's length (duration)</summary>
        public virtual float Length
        { 
            get { return 0.0f; }
            set { }
        }

        /// <summary>
        /// Gets and sets the event's color</summary>
        public virtual Color Color
        {
            get { return Color.LimeGreen; }
            set { }
        }

        #endregion

        /// <summary>
        /// Gets and sets the event's user-readable description. If not empty, it's used in ToString().</summary>
        public string Description
        {
            get { return (string)DomNode.GetAttribute(Schema.eventType.descriptionAttribute); }
            set { DomNode.SetAttribute(Schema.eventType.descriptionAttribute, value); }
        }

        #region ICloneable Members

        /// <summary>
        /// Copies this timeline object, returning a new timeline object that is not in any timeline-related
        /// container. If the copy can't be done, null is returned.</summary>
        /// <returns>A copy of this timeline object or null if copy fails</returns>
        public virtual object Clone()
        {
            DomNode domCopy = DomNode.Copy(new DomNode[] { DomNode })[0];
            return domCopy.As<ITimelineObject>();
        }

        #endregion

        /// <summary>
        /// Returns a string that represents the event</summary>
        /// <returns>String that represents the event</returns>
        /// <remarks>Implemented for tooltip support in TimelineControl</remarks>
        public override string ToString()
        {
            string result = DomNode.GetAttribute(Schema.eventType.descriptionAttribute).ToString();
            if (result == string.Empty)
                result = Name;
            return result;
        }
    }
}



