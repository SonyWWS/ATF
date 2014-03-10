//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Abstract base class for canvas printing. Inheritors must override methods
    /// to get "page" bounds for the various PrintRange values as well as a method to do
    /// the actual rendering to the page.</summary>
    public abstract class CanvasPrintDocument : PrintDocument
    {
        /// <summary>
        /// Sets the default printer settings. Should be called before
        /// IPrintableDocument.GetPrintDocument is called.</summary>
        protected void SetDefaultPrinterSettings()
        {
            // calculate number of pages for multi-page printing
            m_bounds = GetAllPagesBounds();

            Rectangle bounds = DefaultPageSettings.Bounds;
            Margins margins = DefaultPageSettings.Margins;
            Rectangle marginBounds = new Rectangle(
                margins.Left,
                margins.Top,
                bounds.Width - (margins.Left + margins.Right),
                bounds.Height - (margins.Top + margins.Bottom));

            int columns = (int)Math.Ceiling(m_bounds.Width / marginBounds.Width);
            int rows = (int)Math.Ceiling(m_bounds.Height / marginBounds.Height);
            PrinterSettings.MinimumPage = PrinterSettings.FromPage = 1;
            PrinterSettings.MaximumPage = PrinterSettings.ToPage = columns * rows;
        }

        /// <summary>
        /// Raises the System.Drawing.Printing.PrintDocument.BeginPrint event. It is
        /// called after the System.Drawing.Printing.PrintDocument.Print method is
        /// called and before the first page of the document prints.</summary>
        /// <param name="e">A System.Drawing.Printing.PrintEventArgs that contains the event data</param>
        protected override void OnBeginPrint(PrintEventArgs e)
        {
            switch (PrinterSettings.PrintRange)
            {
                case PrintRange.AllPages:
                case PrintRange.SomePages:
                    m_bounds = GetAllPagesBounds();
                    break;

                case PrintRange.Selection:
                    m_bounds = GetSelectionBounds();
                    PrinterSettings.FromPage = PrinterSettings.ToPage = 1;
                    break;

                case PrintRange.CurrentPage:
                    m_bounds = GetCurrentPageBounds();
                    PrinterSettings.FromPage = PrinterSettings.ToPage = 1;
                    break;
            }

            base.OnBeginPrint(e);
        }

        /// <summary>
        /// Raises the System.Drawing.Printing.PrintDocument.PrintPage event. It is called
        /// before a page prints.</summary>
        /// <param name="e">A System.Drawing.Printing.PrintPageEventArgs that contains the event data</param>
        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            base.OnPrintPage(e);

            PrinterSettings printerSettings = e.PageSettings.PrinterSettings;

            MarginBounds = e.MarginBounds;
            RectangleF destinationBounds = e.MarginBounds;
            RectangleF sourceBounds = m_bounds;

            bool multiPage =
                printerSettings.PrintRange == PrintRange.AllPages ||
                printerSettings.PrintRange == PrintRange.SomePages;

            if (multiPage)
            {
                // set sourceBounds to the next sub-rectangle
                int index = printerSettings.FromPage - 1;
                int columns = (int)Math.Ceiling(m_bounds.Width / destinationBounds.Width);
                int row = 0;
                int column = 0;
                if (columns > 0)
                {
                    row = index / columns;
                    column = index % columns;
                }
                sourceBounds = new RectangleF(
                    m_bounds.Left + destinationBounds.Width * column,
                    m_bounds.Top + destinationBounds.Height * row,
                    destinationBounds.Width,
                    destinationBounds.Height);
            }

            Matrix transform = new Matrix();
            transform.Translate(-sourceBounds.Left, -sourceBounds.Top);

            if (!multiPage)
            {
                float xScale = destinationBounds.Width / sourceBounds.Width;
                float yScale = destinationBounds.Height / sourceBounds.Height;
                if (!AllowNonUniformScale)
                {
                    xScale = yScale = Math.Min(xScale, yScale);
                }

                transform.Scale(xScale, yScale, MatrixOrder.Append);
            }

            transform.Translate(destinationBounds.Left, destinationBounds.Top, MatrixOrder.Append);

            Render(sourceBounds, transform, e.Graphics);

            if (printerSettings.FromPage < printerSettings.ToPage)
            {
                e.HasMorePages = true;
                printerSettings.FromPage++;
            }
        }

        /// <summary>
        /// Gets whether or not non-uniform scaling is allowed when fitting the
        /// image(s) to a page</summary>
        protected virtual bool AllowNonUniformScale
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the rectangular area that represents the portion of the page inside the margins,
        /// measured in hundredths of an inch.</summary>
        protected Rectangle MarginBounds
        {
            get { return m_marginBounds; }
            set { m_marginBounds = value; }
        }

        /// <summary>
        /// Gets the rectangle that bounds the current selection, in world coordinates</summary>
        /// <returns>Selection bounding rectangle</returns>
        protected abstract RectangleF GetSelectionBounds();

        /// <summary>
        /// Gets the rectangle that bounds the entire document, in world coordinates</summary>
        /// <returns>Document bounding rectangle</returns>
        protected abstract RectangleF GetAllPagesBounds();

        /// <summary>
        /// Gets the rectangle that bounds the current visible portion of the document or
        /// the current page if that makes sense for the derived class</summary>
        /// <returns>Rectangle that bounds the current visible portion of the document or page</returns>
        protected abstract RectangleF GetCurrentPageBounds();

        /// <summary>
        /// Renders the given rectangle, in world coordinates, to the given graphics device</summary>
        /// <param name="sourceBounds">Source rectangle, in source (world) coordinates</param>
        /// <param name="transform">World to device transform. Apply to objects to be rendered.</param>
        /// <param name="g">Active graphics device to use to render</param>
        protected abstract void Render(RectangleF sourceBounds, Matrix transform, Graphics g);

        private RectangleF m_bounds;
        private Rectangle m_marginBounds;
    }
}
