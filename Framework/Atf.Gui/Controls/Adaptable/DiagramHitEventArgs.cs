//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Event args for events that are raised during picking operations</summary>
    public class DiagramHitEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="hitRecord">Hit record from picking operation</param>
        public DiagramHitEventArgs(DiagramHitRecord hitRecord)
        {
            HitRecord = hitRecord;
        }

        /// <summary>
        /// Hit record from picking operation</summary>
        public readonly DiagramHitRecord HitRecord;
    }
}
