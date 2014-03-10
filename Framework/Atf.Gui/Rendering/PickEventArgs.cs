//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Event args for pick events</summary>
    public class PickEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="hits">HitRecords for event</param>
        /// <param name="multiSelect">Whether user selecting multiple objects</param>
        public PickEventArgs(HitRecord[] hits, bool multiSelect)
        {
            HitArray = hits;
            MultiSelect = multiSelect;
        }

        /// <summary>
        /// HitRecords for event. Includes all the HitRecords underneath the
        /// cursor, sorted nearest to farthest. Use the MultiSelect property to determine if the
        /// user was intending to select one or multiple objects.</summary>
        public readonly HitRecord[] HitArray;

        /// <summary>
        /// Was the user intending to select multiple objects by doing a rubber band selection?</summary>
        public readonly bool MultiSelect;
    }
}
