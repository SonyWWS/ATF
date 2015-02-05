//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

#pragma warning disable 0649 // suppress "field never set" warning

namespace TimelineEditorSample.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a timeline reference, which allows a referenced timeline document
    /// to appear within the owning document, positioned to start at a particular location in the
    /// owning document</summary>
    public class TimelineReference : DomNodeAdapter, ITimelineReference, ICloneable
    {
        /// <summary>
        /// Gets and sets the event's name</summary>
        public string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.timelineRefType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.timelineRefType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the event's start time</summary>
        public float Start
        {
            get { return (float)DomNode.GetAttribute(Schema.timelineRefType.startAttribute); }
            set { DomNode.SetAttribute(Schema.timelineRefType.startAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the event's length (duration)</summary>
        public virtual float Length
        {
            get { return 0; }
            set { }
        }

        /// <summary>
        /// Gets and sets the event's color</summary>
        public Color Color
        {
            get { return Color.FromArgb((int)DomNode.GetAttribute(Schema.timelineRefType.colorAttribute)); }
            set { DomNode.SetAttribute(Schema.timelineRefType.colorAttribute, value.ToArgb()); }
        }

        /// <summary>
        /// Gets the referenced ITimeline object or null if it doesn't exist or if the physical file
        /// could not be found, etc.</summary>
        public IHierarchicalTimeline Target
        {
            get
            {
                IHierarchicalTimeline target = null;
                var doc = (TimelineDocument)TimelineEditor.TimelineDocumentRegistry.GetDocument(Uri);
                if (doc != null)
                    target = doc.Timeline as IHierarchicalTimeline;
                return target;
            }
        }

        /// <summary>
        /// Gets the timeline that contains this reference</summary>
        public IHierarchicalTimeline Parent
        {
            get { return GetParentAs<IHierarchicalTimeline>(); }
        }

        /// <summary>
        /// Gets an object that contains properties that affect the user interface and behavior of this
        /// timeline reference</summary>
        /// <remarks>There is no 'set' because changes to the object persist, and so that client code
        /// can provide a derived class.</remarks>
        public TimelineReferenceOptions Options
        {
            get { return m_options; }
        }

        /// <summary>
        /// Gets or sets the URI of the referenced IHierarchicalTimeline</summary>
        public Uri Uri
        {
            get { return DomNode.GetAttribute(Schema.timelineRefType.refAttribute) as Uri; }
            set { DomNode.SetAttribute(Schema.timelineRefType.refAttribute, value); }
        }

        /// <summary>
        /// Returns a string that represents the event</summary>
        /// <returns>String that represents the event</returns>
        /// <remarks>Implemented for tooltip support in TimelineControl</remarks>
        public override string ToString()
        {
            return DomNode.GetAttribute(Schema.timelineRefType.descriptionAttribute).ToString();
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

        private TimelineReferenceOptions m_options = new TimelineReferenceOptions();
    }
}
