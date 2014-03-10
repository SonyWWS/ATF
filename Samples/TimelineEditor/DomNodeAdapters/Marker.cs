//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

using Sce.Atf.Controls.Timelines;

#pragma warning disable 0649 // suppress "field never set" warning

namespace TimelineEditorSample.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Marker</summary>
    public class Marker : BaseEvent, IMarker
    {
        /// <summary>
        /// Performs custom processing after adapter successfully attaches to the Marker's DOM object</summary>
        protected override void OnNodeSet()
        {
            // initialize defaulted attributes
            DomNode.SetAttributeIfDefault(Schema.markerType.nameAttribute, "Marker");
            DomNode.SetAttributeIfDefault(Schema.markerType.colorAttribute, Color.MediumBlue.ToArgb());
        }

        #region IEvent and IMarker Members

        /// <summary>
        /// Gets and sets the Marker's name</summary>
        public override string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.markerType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.markerType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the Marker's color</summary>
        public override Color Color
        {
            get { return Color.FromArgb((int)DomNode.GetAttribute(Schema.markerType.colorAttribute)); }
            set { DomNode.SetAttribute(Schema.markerType.colorAttribute, value.ToArgb()); }
        }

        /// <summary>
        /// Gets the timeline that contains the Marker</summary>
        public ITimeline Timeline
        {
            get { return GetParentAs<Timeline>(); }
        }

        #endregion
    }
}


