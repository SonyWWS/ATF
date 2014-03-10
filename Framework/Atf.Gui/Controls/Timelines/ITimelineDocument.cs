//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Interface for editable timeline documents</summary>
    public interface ITimelineDocument : IDocument, IObservableContext, IAdaptable
    {
        /// <summary>
        /// Gets the timeline</summary>
        ITimeline Timeline { get; }
    }
}
