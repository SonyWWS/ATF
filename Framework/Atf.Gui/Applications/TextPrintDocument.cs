//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Drawing.Printing;
using System.IO;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// PrintDocument class to handle text files</summary>
    public class TextPrintDocument : PrintDocument
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="fileName">Path to file</param>
        public TextPrintDocument(string fileName)
        {
            m_fileName = fileName;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="stream">Text stream</param>
        public TextPrintDocument(Stream stream)
        {
            m_stream = stream;
        }

        /// <summary>
        /// Performs processing when printing begins</summary>
        /// <param name="e">Event args</param>
        protected override void OnBeginPrint(PrintEventArgs e)
        {
            base.OnBeginPrint(e);
            m_font = new Font("Courier", 10);

            if (m_fileName != null)
            {
                try
                {
                    m_streamReader = new StreamReader(m_fileName);
                }
                catch (FileNotFoundException)
                {
                    e.Cancel = true;
                }
            }
            else if (m_stream != null)
            {
                m_streamReader = new StreamReader(m_stream);

            }
            else
                e.Cancel = true;

        }

        /// <summary>
        /// Performs processing after printing ends</summary>
        /// <param name="e">Event args</param>
        protected override void OnEndPrint(PrintEventArgs e)
        {
            base.OnEndPrint(e);
            m_font.Dispose();
            m_streamReader.Close();
            m_streamReader = null;
            if (m_stream != null)
            {
                m_stream.Close();
                m_stream = null;
            }
        }

        /// <summary>
        /// Performs processing when printing a page</summary>
        /// <param name="e">Event args</param>
        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            base.OnPrintPage(e);

            float leftMargin = e.MarginBounds.Left;
            float topMargin = e.MarginBounds.Top;
            float lineHeight = m_font.GetHeight(e.Graphics);
            float linesPerPage = e.MarginBounds.Height / lineHeight;
            int lineCount = 0;
            string lineText = null;

            // Print each line of the file.
            while (lineCount < linesPerPage &&
                  ((lineText = m_streamReader.ReadLine()) != null))
            {
                e.Graphics.DrawString(lineText, m_font, Brushes.Black,
                        leftMargin, (topMargin + (lineCount++ * lineHeight)));
            }

            // If more lines exist, print another page.
            if (lineText != null)
                e.HasMorePages = true;
            else
                e.HasMorePages = false;
        }

        private Font m_font;
        private TextReader m_streamReader;
        private readonly string m_fileName;
        private Stream m_stream;
    }
}
