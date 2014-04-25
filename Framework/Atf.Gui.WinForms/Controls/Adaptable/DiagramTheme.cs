//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Diagram theme, which determines how elements in diagrams are rendered and picked</summary>
    [Export(typeof(DiagramTheme))]
    public class DiagramTheme : IDisposable
    {
        /// <summary>
        /// Constructor with default font</summary>
        public DiagramTheme()
            : this(new Font("Microsoft Sans Serif", 8))
        {
        }

        /// <summary>
        /// Constructor with font</summary>
        /// <param name="font">Diagram font</param>
        public DiagramTheme(Font font)
        {
            m_font = font;

            m_fillBrush = new SolidBrush(SystemColors.Window);
            m_textBrush = new SolidBrush(SystemColors.WindowText);

            m_outlinePen = new Pen(SystemColors.ControlDark, 1);

            m_highlightPen = new Pen(SystemColors.Highlight, DefaultPenWidth);
            m_highlightPen.DashStyle = DashStyle.Dot;
            m_highlightBrush = new HatchBrush(HatchStyle.ForwardDiagonal, SystemColors.Highlight);

            m_lastHighlightPen = new Pen(SystemColors.Highlight, DefaultPenWidth);
            m_lastHighlightBrush = new SolidBrush(SystemColors.Highlight);

            m_hotPen = new Pen(SystemColors.HotTrack, DefaultPenWidth);
            m_hotPen.Alignment = PenAlignment.Inset;
            m_hotBrush = new SolidBrush(SystemColors.HotTrack);

            m_ghostPen = new Pen(Color.Silver, 1);
            m_ghostBrush = new SolidBrush(Color.White);

            m_hiddenPen = new Pen(Color.LightGray, 1);
            m_hiddenBrush = new SolidBrush(Color.LightGray);

            m_errorPen = new Pen(Color.Tomato, 1);
            m_errorBrush = new SolidBrush(Color.Tomato);

            m_leftFormat.Alignment = StringAlignment.Near;
            m_rightFormat.Alignment = StringAlignment.Far;
            m_centerFormat.Alignment = StringAlignment.Center;
            m_centerFormat.LineAlignment = StringAlignment.Center;
        }

        private const int DefaultPenWidth = 3;

        /// <summary>
        /// Event that is raised after any user property of the theme changes</summary>
        public event EventHandler Redraw;

        /// <summary>
        /// Raises the Redraw event</summary>
        protected virtual void OnRedraw()
        {
            Redraw.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Destructor</summary>
        ~DiagramTheme()
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
        /// <param name="disposing">Whether or not dispose was called; not used</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                m_outlinePen.Dispose();

                m_fillBrush.Dispose();

                m_textBrush.Dispose();

                m_highlightPen.Dispose();
                m_highlightBrush.Dispose();

                m_lastHighlightPen.Dispose();
                m_lastHighlightBrush.Dispose();

                m_ghostPen.Dispose();
                m_ghostBrush.Dispose();

                m_hiddenPen.Dispose();
                m_hiddenBrush.Dispose();

                m_hotPen.Dispose();
                m_hotBrush.Dispose();

                m_errorPen.Dispose();
                m_errorBrush.Dispose();

                foreach (Pen pen in m_pens.Values)
                    pen.Dispose();

                foreach (Brush brush in m_brushes.Values)
                    brush.Dispose();

                m_disposed = true;
            }
        }
        private bool m_disposed;

        /// <summary>
        /// Gets or sets the diagram font</summary>
        public Font Font
        {
            get { return m_font; }
            set { SetDisposableField(value, ref m_font); }
        }
        private Font m_font;

        /// <summary>
        /// Gets or sets the diagram picking tolerance</summary>
        public int PickTolerance
        {
            get { return m_pickTolerance; }
            set { m_pickTolerance = value; }
        }
        private int m_pickTolerance = 3;

        /// <summary>
        /// Gets or sets the pen used to draw diagram outlines</summary>
        public Pen OutlinePen
        {
            get { return m_outlinePen; }
            set { SetDisposableField(value, ref m_outlinePen); }
        }
        private Pen m_outlinePen;

        /// <summary>
        /// Gets or sets the brush used to fill diagram items</summary>
        public Brush FillBrush
        {
            get { return m_fillBrush; }
            set { SetDisposableField(value, ref m_fillBrush); }
        }
        private Brush m_fillBrush;

        /// <summary>
        /// Gets or sets the brush used to draw diagram text</summary>
        public Brush TextBrush
        {
            get { return m_textBrush; }
            set { SetDisposableField(value, ref m_textBrush); }
        }
        private Brush m_textBrush;

        /// <summary>
        /// Gets the theme's pen for the given drawing style</summary>
        /// <param name="style">Drawing style</param>
        /// <returns>Pen for the given drawing style</returns>
        public Pen GetPen(DiagramDrawingStyle style)
        {
            switch (style)
            {
                case DiagramDrawingStyle.Normal:
                    return m_outlinePen;
                case DiagramDrawingStyle.Selected:
                    return m_highlightPen;
                case DiagramDrawingStyle.LastSelected:
                    return m_lastHighlightPen;
                case DiagramDrawingStyle.Hot:
                    return m_hotPen;
                case DiagramDrawingStyle.Ghosted:
                    return m_ghostPen;
                case DiagramDrawingStyle.Hidden:
                    return m_hiddenPen;
                case DiagramDrawingStyle.Error:
                default:
                    return m_errorPen;
            }
        }

        /// <summary>
        /// Gets the theme's brush for the given drawing style</summary>
        /// <param name="style">Drawing style</param>
        /// <returns>Brush for the given drawing style</returns>
        public Brush GetBrush(DiagramDrawingStyle style)
        {
            switch (style)
            {
                case DiagramDrawingStyle.Normal:
                    return m_fillBrush;
                case DiagramDrawingStyle.Selected:
                    return m_highlightBrush;
                case DiagramDrawingStyle.LastSelected:
                    return m_lastHighlightBrush;
                case DiagramDrawingStyle.Hot:
                    return m_hotBrush;
                case DiagramDrawingStyle.Ghosted:
                    return m_ghostBrush;
                case DiagramDrawingStyle.Hidden:
                    return m_hiddenBrush;
                case DiagramDrawingStyle.Error:
                default:
                    return m_errorBrush;
            }
        }

        /// <summary>
        /// Gets or sets the pen used to outline selected diagram items</summary>
        public Pen HighlightPen
        {
            get { return m_highlightPen; }
            set { SetDisposableField(value, ref m_highlightPen); }
        }
        private Pen m_highlightPen;

        /// <summary>
        /// Gets or sets the brush used to fill selected diagram items</summary>
        public Brush HighlightBrush
        {
            get { return m_highlightBrush; }
            set { SetDisposableField(value, ref m_highlightBrush); }
        }
        private Brush m_highlightBrush;

        /// <summary>
        /// Gets or sets the pen used to outline the last selected diagram item</summary>
        public Pen LastHighlightPen
        {
            get { return m_lastHighlightPen; }
            set { SetDisposableField(value, ref m_lastHighlightPen); }
        }
        private Pen m_lastHighlightPen;

        /// <summary>
        /// Gets or sets the brush used to fill the last selected diagram item</summary>
        public Brush LastHighlightBrush
        {
            get { return m_lastHighlightBrush; }
            set { SetDisposableField(value, ref m_lastHighlightBrush); }
        }
        private Brush m_lastHighlightBrush;

        /// <summary>
        /// Gets or sets the pen used to outline ghosted diagram items</summary>
        public Pen GhostPen
        {
            get { return m_ghostPen; }
            set { SetDisposableField(value, ref m_ghostPen); }
        }
        private Pen m_ghostPen;

        /// <summary>
        /// Gets or sets the brush used to fill ghosted diagram items</summary>
        public Brush GhostBrush
        {
            get { return m_ghostBrush; }
            set { SetDisposableField(value, ref m_ghostBrush); }
        }
        private Brush m_ghostBrush;

        /// <summary>
        /// Gets or sets the pen used to outline hidden diagram items</summary>
        public Pen HiddenPen
        {
            get { return m_hiddenPen; }
            set { SetDisposableField(value, ref m_hiddenPen); }
        }
        private Pen m_hiddenPen;

        /// <summary>
        /// Gets or sets the brush used to fill hidden diagram items</summary>
        public Brush HiddenBrush
        {
            get { return m_hiddenBrush; }
            set { SetDisposableField(value, ref m_hiddenBrush); }
        }
        private Brush m_hiddenBrush;

        /// <summary>
        /// Gets or sets the pen used to outline hot-tracked diagram items</summary>
        public Pen HotPen
        {
            get { return m_hotPen; }
            set { SetDisposableField(value, ref m_hotPen); }
        }
        private Pen m_hotPen;

        /// <summary>
        /// Gets or sets the brush used to fill hot-tracked diagram items</summary>
        public Brush HotBrush
        {
            get { return m_hotBrush; }
            set { SetDisposableField(value, ref m_hotBrush); }
        }
        private Brush m_hotBrush;

        /// <summary>
        /// Gets or sets the pen used to outline diagram items with errors</summary>
        public Pen ErrorPen
        {
            get { return m_errorPen; }
            set { SetDisposableField(value, ref m_errorPen); }
        }
        private Pen m_errorPen;

        /// <summary>
        /// Gets or sets the brush used to fill diagram items with errors</summary>
        public Brush ErrorBrush
        {
            get { return m_errorBrush; }
            set { SetDisposableField(value, ref m_errorBrush); }
        }
        private Brush m_errorBrush;

        private void SetDisposableField<T>(T value, ref T field)
            where T : class, IDisposable
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (field != value)
            {
                field.Dispose();
                field = value;
                OnRedraw();
            }
        }

        /// <summary>
        /// Registers a pen with a unique key</summary>
        /// <param name="key">Key to access pen</param>
        /// <param name="pen">Custom pen</param>
        public void RegisterCustomPen(object key, Pen pen)
        {
            Pen oldPen;
            m_pens.TryGetValue(key, out oldPen);
            if (pen != oldPen)
            {
                if (oldPen != null)
                    oldPen.Dispose();

                m_pens[key] = pen;
                OnRedraw();
            }
        }

        /// <summary>
        /// Gets the custom pen corresponding to the key, or null if none</summary>
        /// <param name="key">Key identifying pen</param>
        /// <returns>Custom pen corresponding to the key, or null if none</returns>
        public Pen GetCustomPen(object key)
        {
            Pen pen;
            m_pens.TryGetValue(key, out pen);
            return pen;
        }

        private readonly Dictionary<object, Pen> m_pens = new Dictionary<object, Pen>();

        /// <summary>
        /// Registers a brush with a unique key</summary>
        /// <param name="key">Key to access brush</param>
        /// <param name="brush">Custom brush</param>
        public void RegisterCustomBrush(object key, Brush brush)
        {
            Brush oldBrush;
            m_brushes.TryGetValue(key, out oldBrush);
            if (brush != oldBrush)
            {
                if (oldBrush != null)
                    oldBrush.Dispose();

                m_brushes[key] = brush;
                OnRedraw();
            }
        }

        /// <summary>
        /// Gets the custom brush corresponding to the key, or null if none</summary>
        /// <param name="key">Key identifying brush</param>
        /// <returns>Custom brush corresponding to the key, or null if none</returns>
        public Brush GetCustomBrush(object key)
        {
            Brush brush;
            m_brushes.TryGetValue(key, out brush);
            return brush;
        }

        private readonly Dictionary<object, Brush> m_brushes = new Dictionary<object, Brush>();

        /// <summary>
        /// Gets the left-justified string format for the theme</summary>
        public StringFormat LeftStringFormat
        {
            get { return m_leftFormat; }
        }
        private readonly StringFormat m_leftFormat = new StringFormat();

        /// <summary>
        /// Gets the right-justified string format for the theme</summary>
        public StringFormat RightStringFormat
        {
            get { return m_rightFormat; }
        }
        private readonly StringFormat m_rightFormat = new StringFormat();

        /// <summary>
        /// Gets the centered string format for the theme</summary>
        public StringFormat CenterStringFormat
        {
            get { return m_centerFormat; }
        }
        private readonly StringFormat m_centerFormat = new StringFormat();
    }
}
