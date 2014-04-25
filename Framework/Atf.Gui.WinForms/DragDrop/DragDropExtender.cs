//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace Sce.Atf.DragDrop
{
    /// <summary>
    /// A class to extend Drag Drop functionality, allowing the attachment of an image and description.
    /// 
    /// For example:
    ///     private void label1_MouseDown(object sender, MouseEventArgs e)
    ///     {
    ///         using (var extender = new DragDropExtender(label1)
    ///         {
    ///            extender.DoDragDrop("data", DragDropEffects.Copy, dragImage, new Point(5, 5));
    ///        }
    ///     }
    /// </summary>
    public class DragDropExtender
    {
        /// <summary>
        /// Construct a DragDropExtender</summary>
        /// <param name="owner">The control that starts the drag event.</param>
        public DragDropExtender(Control owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            m_owner = owner;
        }

        /// <summary>
        /// Begins a drag-and-drop operation, with an image attached.
        /// </summary>
        /// <param name="data">The data to drag.</param>
        /// <param name="allowedEffects">One of the DragDropEffects values.</param>
        /// <param name="dragImage">The image to attach to the cursor</param>
        /// <param name="cursorOffset">The offset within the image that the cursor attaches to.</param>
        /// <returns>A value from the DragDropEffects enumeration that represents the final effect that was performed during the drag-and-drop operation.</returns>
        public DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects, Bitmap dragImage, Point cursorOffset)
        {
            var adviseConnection = 0;
            try
            {
                DragDropHelper.SetFlags(1);

                // Create IDataObject
                m_dragData = new DragDropDataObject();

                // attach image if we have one.
                if (dragImage != null)
                {
                    DragDropHelper.InitializeFromBitmap(m_dragData, dragImage, cursorOffset);
                }

                // We need to listen for drop description changes. If a drop target
                // changes the drop description, we shouldn't provide a default one.
                var formatEtc = OleConverter.CreateFormat("DropDescription");
                var hr = m_dragData.DAdvise(ref formatEtc, 0, new AdviseSink(m_dragData), out adviseConnection);
                if (hr != 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                // associate data with it.
                m_dragData.SetData("DragDropExtender", this);
                m_dragData.SetData(data);

                m_owner.GiveFeedback += OnGiveFeedback;
                m_owner.QueryContinueDrag += OnQueryContinueDrag;
                return m_owner.DoDragDrop(m_dragData, allowedEffects);
            }
            finally
            {
                m_owner.GiveFeedback -= OnGiveFeedback;
                m_owner.QueryContinueDrag -= OnQueryContinueDrag;

                if (m_dragData != null)
                {
                    // Stop listening to drop description changes
                    m_dragData.DUnadvise(adviseConnection);
                    m_dragData.Dispose();
                    m_dragData = null;
                }
            }
        }

        private void OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (m_dragData != null)
            {
                DropDescriptionHelper.DefaultGiveFeedback(m_dragData, e);
            }
        }

        private void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
            }
        }

        #region -- AdviseSink class -------------------------------------------

        /// <summary>
        /// Provides an advisory sink for the COM IDataObject implementation.
        /// </summary>
        private class AdviseSink : IAdviseSink
        {
            // The associated data object
            private readonly IDataObject m_data;

            /// <summary>
            /// Creates an AdviseSink associated to the specified data object.
            /// </summary>
            /// <param name="data">The data object.</param>
            public AdviseSink(IDataObject data)
            {
                m_data = data;
            }

            /// <summary>
            /// Handles DataChanged events from a COM IDataObject.
            /// </summary>
            /// <param name="format">The data format that had a change.</param>
            /// <param name="stgmedium">The data value.</param>
            public void OnDataChange(ref FORMATETC format, ref STGMEDIUM stgmedium)
            {
                // We listen to DropDescription changes, so that we can unset the IsDefault
                // drop description flag.
                var odd = DropDescriptionHelper.GetDropDescription(m_data);
                if (odd != null)
                {
                    DropDescriptionHelper.SetDropDescriptionIsDefault(m_data, false);
                }
            }

            #region Unsupported callbacks

            public void OnClose()
            {
                throw new NotImplementedException();
            }

            public void OnRename(IMoniker moniker)
            {
                throw new NotImplementedException();
            }

            public void OnSave()
            {
                throw new NotImplementedException();
            }

            public void OnViewChange(int aspect, int index)
            {
                throw new NotImplementedException();
            }

            #endregion // Unsupported callbacks
        }

        #endregion // AdviseSink class

        private readonly Control m_owner;
        private DragDropDataObject m_dragData;
    }
}
