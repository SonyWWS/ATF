//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Controls.Adaptable;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Arguments for hover events</summary>
    /// <typeparam name="TObject">Object under hover</typeparam>
    /// <typeparam name="TPart">Object part under hover</typeparam>
    public class HoverEventArgs<TObject, TPart> : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="obj">Object under hover</param>
        /// <param name="part">Object part under hover</param>
        public HoverEventArgs(TObject obj, TPart part)
        {
            Object = obj;
            Part = part;
            AdaptableControl = null;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="obj">Object under hover</param>
        /// <param name="part">Object part under hover</param>
        /// <param name="adaptableControl">AdaptableControl under hover</param>
        public HoverEventArgs(TObject obj, TPart part, AdaptableControl adaptableControl)
        {
            Object = obj;
            Part = part;
            AdaptableControl = adaptableControl;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="obj">Object under hover</param>
        /// <param name="part">Object part under hover</param>
        /// <param name="subobj">Subobject under hover</param>
        /// <param name="subpart">SubObject subpart under hover</param>
        /// <param name="adaptableControl">AdaptableControl under hover</param>
        public HoverEventArgs(TObject obj, TPart part, TObject subobj, TPart subpart, AdaptableControl adaptableControl)
        {
            Object = obj;
            Part = part;
            SubObject = subobj;
            SubPart = subpart;
            AdaptableControl = adaptableControl;
        }

        /// <summary>
        /// Object under hover</summary>
        public readonly TObject Object;

        /// <summary>
        /// Object part under hover</summary>
        public readonly TPart Part;

        /// <summary>
        /// Sub-Object under hover</summary>
        public readonly TObject SubObject;

        /// <summary>
        /// Sub-part pf sub-object under hover</summary>
        public readonly TPart SubPart;

        /// <summary>
        /// AdaptableControl under hover</summary>
        public readonly AdaptableControl AdaptableControl;
    }
}
