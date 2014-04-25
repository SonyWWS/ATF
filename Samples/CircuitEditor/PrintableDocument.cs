//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// DOM adapter to make circuit printable</summary>
    public class PrintableDocument : DomNodeAdapter, IPrintableDocument
    {
        #region IPrintableDocument Implementation

        /// <summary>
        /// Gets a PrintDocument to work with the standard Windows print dialogs</summary>
        /// <returns>PrintDocument to work with the standard Windows print dialogs</returns>
        public PrintDocument GetPrintDocument()
        {
            // static allocation, to remember print settings
            if (s_printDocument == null)
                s_printDocument = new CircuitPrintDocument();

            s_printDocument.SetDocument(
                DomNode.Cast<CircuitEditingContext>(),
                DomNode.Cast<ViewingContext>());
            return s_printDocument;
        }

        private static CircuitPrintDocument s_printDocument;

        private class CircuitPrintDocument : CanvasPrintDocument
        {
            public void SetDocument(CircuitEditingContext editingContext, ViewingContext viewingContext)
            {
                m_editingContext = editingContext;
                m_viewingContext = viewingContext;
                SetDefaultPrinterSettings();
            }

            protected override RectangleF GetSelectionBounds()
            {
                object[] selectedItems = m_editingContext.Selection.GetSnapshot();
                RectangleF result = m_viewingContext.GetBounds(selectedItems);
                result = GdiUtil.InverseTransform(m_viewingContext.Control.As<ITransformAdapter>().Transform, result);
                return result;
            }

            protected override RectangleF GetAllPagesBounds()
            {
                RectangleF result = m_viewingContext.Control.As<ICanvasAdapter>().Bounds;
                return result;
            }

            protected override RectangleF GetCurrentPageBounds()
            {
                RectangleF result = m_viewingContext.Control.As<ICanvasAdapter>().WindowBounds;
                result = GdiUtil.InverseTransform(m_viewingContext.Control.As<ITransformAdapter>().Transform, result);
                return result;
            }

            protected override void Render(RectangleF sourceBounds, Matrix transform, Graphics g)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.Transform = transform;
                sourceBounds.Inflate(1, 1); // allow for pen widths
                g.SetClip(sourceBounds);

                foreach (IPrintingAdapter printingAdapter in m_viewingContext.Control.AsAll<IPrintingAdapter>())
                    printingAdapter.Print(this, g);
            }

            private CircuitEditingContext m_editingContext;
            private ViewingContext m_viewingContext;
        }

        #endregion
    }
}
