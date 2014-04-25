//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

namespace TimelineEditorSample.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Track</summary>
    public class Track : DomNodeAdapter, ITrack, ICloneable
    {
        #region ITrack Members

        /// <summary>
        /// Gets or sets the track name</summary>
        public string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.trackType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.trackType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets the group that contains the track</summary>
        public IGroup Group
        {
            get { return GetParentAs<Group>(); }
        }

        /// <summary>
        /// Creates a new interval</summary>
        /// <returns>New interval</returns>
        public IInterval CreateInterval()
        {
            return new DomNode(Schema.intervalType.Type).As<IInterval>();
        }

        /// <summary>
        /// Gets the list of all intervals in the track. Adding or removing intervals in the IList 
        /// modifies the underlying data store. For example, if the DOM is being used, the DOM is
        /// modified when the IList is modified.</summary>
        public IList<IInterval> Intervals
        {
            get { return GetChildList<IInterval>(Schema.trackType.intervalChild); }
        }

        /// <summary>
        /// Creates a new key</summary>
        /// <returns>New key</returns>
        public IKey CreateKey()
        {
            return new DomNode(Schema.keyType.Type).As<IKey>();
        }

        /// <summary>
        /// Gets the list of all keys in the track</summary>
        public IList<IKey> Keys
        {
            get { return GetChildList<IKey>(Schema.trackType.keyChild); }
        }

        #endregion

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
        /// Returns the Name property. Useful for debugging purposes.</summary>
        /// <returns>Name property</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}



