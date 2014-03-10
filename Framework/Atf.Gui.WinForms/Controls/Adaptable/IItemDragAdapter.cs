//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Interface for control adapters that can drag items. This interface provides
    /// a mechanism for any control adapter that can drag items to initiate drags
    /// of items in other layers at the same time. In this way, all selected items
    /// may be dragged together even if they are managed by different adapter layers.</summary>
    public interface IItemDragAdapter
    {
        /// <summary>
        /// Begins dragging any selected items managed by the adapter. May be called
        /// by another adapter when it begins dragging.</summary>
        /// <param name="initiator">Control adapter that is initiating the drag</param>
        void BeginDrag(ControlAdapter initiator);

        /// <summary>
        /// Called before a transaction is initiated and before EndDrag() is called, to
        /// indicate that the drag will be ending. The implementor can use this opportunity
        /// to prepare for being included in the caller's transaction.</summary>
        /// <remarks>If the location of the dragged objects is backed by the DOM, use this
        /// opportunity to relocate the selected objects to their original positions when
        /// the drag began. Then, EndDrag() can move the objects to their final location
        /// and the transaction will record the correct before and after positions.</remarks>
        void EndingDrag();

        /// <summary>
        /// Ends dragging any selected items managed by the adapter. May be called
        /// by another adapter when it ends dragging. May be called from within a transaction.</summary>
        void EndDrag();
    }
}
