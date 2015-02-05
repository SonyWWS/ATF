//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Data editing theme, which determines how elements in data editors are rendered</summary>
    public class DataEditorTheme : IDisposable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="font">Diagram font</param>
        public DataEditorTheme(Font font)
        {
            m_font = font;
            m_textBrush = new SolidBrush(SystemColors.WindowText);
            m_readonlyBrush = new SolidBrush(SystemColors.GrayText);
            m_sliderTrackPen = new Pen(Color.Silver);
            m_padding = new Padding(8);
            m__defaultSliderWidth = 100;
            m_fillBrush = new SolidBrush(SystemColors.Window);
            m_solidBrush = new SolidBrush(Color.MediumBlue);
        }

        /// <summary>
        /// Gets or sets the diagram font</summary>
        public Font Font
        {
            get { return m_font; }
            set { SetDisposableField(value, ref m_font); }
        }
        private Font m_font;

        /// <summary>
        /// Gets or sets the brush used to draw data value in text</summary>
        public Brush TextBrush
        {
            get { return m_textBrush; }
            set { SetDisposableField(value, ref m_textBrush); }
        }
        private Brush m_textBrush;

        /// <summary>
        /// Gets or sets the brush used to draw readonly data value in text</summary>
        public Brush ReadonlyBrush
        {
            get { return m_readonlyBrush; }
            set { SetDisposableField(value, ref m_readonlyBrush); }
        }
        private Brush m_readonlyBrush;

        /// <summary>
        /// Gets or sets the brush used to fill cell background</summary>
        public Brush FillBrush
        {
            get { return m_fillBrush; }
            set { SetDisposableField(value, ref m_fillBrush); }
        }
        private Brush m_fillBrush;

        /// <summary>
        /// Gets or sets a solid brush for current drawing</summary>
        public SolidBrush SolidBrush
        {
            get { return m_solidBrush; }
            set { SetDisposableField(value, ref m_solidBrush); }
        }
        private SolidBrush m_solidBrush;

        /// <summary>
        /// Gets or sets the pen used to draw the track of a slider </summary>
        public Pen SliderTrackPen
        {
            get { return m_sliderTrackPen; }
            set { SetDisposableField(value, ref m_sliderTrackPen); }
        }
        private Pen m_sliderTrackPen = new Pen(Color.Silver);

        /// <summary>
        /// Gets or sets the default width of the slider.</summary>
        /// <value>
        /// The default width of the slider.</value>
        public int DefaultSliderWidth
        {
            get { return m__defaultSliderWidth; }
            set { m__defaultSliderWidth = value; }
        }

        private  int m__defaultSliderWidth;

        /// <summary>
        /// Gets or sets the internal spacing, in pixels, between data editing elements</summary>
        public Padding Padding
        {
            get { return m_padding; }
            set { m_padding = value; }
        }
        private Padding m_padding;

        private void SetDisposableField<T>(T value, ref T field)
                  where T : class, IDisposable
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (field != value)
            {
                field.Dispose();
                field = value;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DataEditorTheme"/> class.</summary>
        ~DataEditorTheme()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of unmanaged resources</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of unmanaged resources</summary>
        /// <param name="disposing">Whether or not dispose was called</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            { 
                m_textBrush.Dispose();
                m_readonlyBrush.Dispose();
                m_sliderTrackPen.Dispose();
                m_fillBrush.Dispose();
                m_solidBrush.Dispose();

                m_disposed = true;
            }
        }
        private bool m_disposed;

     }
}
