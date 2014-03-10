//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using Sce.Atf.Controls.Timelines;

#pragma warning disable 0649 // suppress "field never set" warning

namespace TimelineEditorSample.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Key</summary>
    public class Key : BaseEvent, IKey
    {
        #region IKey Members

        /// <summary>
        /// Gets the track that contains the key</summary>
        public ITrack Track
        {
            get { return GetParentAs<ITrack>(); }
        }

        #endregion
    }
}




