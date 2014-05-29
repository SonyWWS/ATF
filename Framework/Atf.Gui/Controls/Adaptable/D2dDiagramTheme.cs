//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;

using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Diagram rendering theme class</summary>
    public class D2dDiagramTheme : D2dResource
    {
        /// <summary>
        /// Constructor with no parameters</summary>
        public D2dDiagramTheme()
            : this("Microsoft Sans Serif", 10)
        {

        }
        
        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="fontFamilyName">Font family name for theme</param>
        /// <param name="fontSize">Font size</param>
        public D2dDiagramTheme(string fontFamilyName, float fontSize)
        {            
            m_d2dTextFormat = D2dFactory.CreateTextFormat(fontFamilyName, fontSize);            
            m_fillBrush = D2dFactory.CreateSolidBrush(SystemColors.Window);
            m_textBrush = D2dFactory.CreateSolidBrush(SystemColors.WindowText);
            m_outlineBrush = D2dFactory.CreateSolidBrush(SystemColors.ControlDark);
            m_highlightBrush = D2dFactory.CreateSolidBrush(SystemColors.Highlight);                                   
            m_lastHighlightBrush = D2dFactory.CreateSolidBrush(SystemColors.Highlight);
            m_textHighlightBrush = D2dFactory.CreateSolidBrush(SystemColors.Highlight);
            m_hotBrush = D2dFactory.CreateSolidBrush(SystemColors.HotTrack);
            m_dragSourceBrush = D2dFactory.CreateSolidBrush(Color.SlateBlue);
            m_dropTargetBrush = D2dFactory.CreateSolidBrush(Color.Chartreuse);
            m_ghostBrush = D2dFactory.CreateSolidBrush(Color.White);            
            m_hiddenBrush = D2dFactory.CreateSolidBrush(Color.LightGray);
            m_templatedInstance = D2dFactory.CreateSolidBrush(Color.Yellow);
            m_copyInstance = D2dFactory.CreateSolidBrush(Color.Green);     
            m_errorBrush = D2dFactory.CreateSolidBrush(Color.Tomato);
            m_infoBrush = D2dFactory.CreateSolidBrush(SystemColors.Info);            
            m_hoverBorderBrush = D2dFactory.CreateSolidBrush(SystemColors.ControlDarkDark);

            int fontHeight = (int)TextFormat.FontHeight;
            m_rowSpacing = fontHeight + PinMargin;
            m_pinOffset = (fontHeight - m_pinSize) / 2;
        
            D2dGradientStop[] gradstops = 
            { 
                new D2dGradientStop(Color.White, 0),
                new D2dGradientStop(Color.LightSteelBlue, 1.0f),
            };
            m_fillLinearGradientBrush = D2dFactory.CreateLinearGradientBrush(gradstops);
            StrokeWidth = 2;
        }
       
        /// <summary>
        /// Gets row spacing in pixels between pins on element</summary>
        public virtual int RowSpacing { get { return m_rowSpacing; } }
        /// <summary>
        /// Gets pin offset in pixels from pin location</summary>
        public virtual int PinOffset { get { return m_pinOffset; } }
        /// <summary>
        /// Gets pin size in pixels</summary>
        public int PinSize { get { return m_pinSize; } }
        /// <summary>
        /// Gets margin in pixels around pins between pin and other markings, such as labels</summary>
        public int PinMargin { get { return m_pinMargin; } }

        private int m_rowSpacing;
        private int m_pinOffset;
        private int m_pinSize = 8;
        private int m_pinMargin = 2;
       
        /// <summary>
        /// Registers a brush (pen) with a unique key</summary>
        /// <param name="key">Key to access brush</param>
        /// <param name="brush">Custom brush</param>
        public void RegisterCustomBrush(object key, D2dBrush brush)
        {
            D2dBrush oldBrush;
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
        /// Gets the custom brush (pen) corresponding to the key, or null</summary>
        /// <param name="key">Key identifying brush</param>
        /// <returns>Custom brush corresponding to the key, or null</returns>
        public D2dBrush GetCustomBrush(object key)
        {
            D2dBrush pen;
            m_brushes.TryGetValue(key, out pen);
            return pen;
        }
       
        /// <summary>
        /// Registers a bitmap with a unique key</summary>
        /// <param name="key">Key to access bitmap</param>
        /// <param name="image">Custom bitmap</param>
        public void RegisterBitmap(object key, Image image)
        {
             var bitmap = D2dFactory.CreateBitmap(image);
             m_bitmaps[key] = bitmap;
        }

        /// <summary>
        /// Gets the custom bitmap corresponding to the key, or null</summary>
        /// <param name="key">Key identifying bitmap</param>
        /// <returns>Bitmap corresponding to the key, or null</returns>
        public D2dBitmap GetBitmap(object key)
        {
            D2dBitmap bitmap;
            if (m_bitmaps.TryGetValue(key, out bitmap))
                return bitmap;
            return null;
        }

        /// <summary>
        /// Gets or sets default Stroke width used for drawing outline</summary>        
        public float StrokeWidth
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the diagram text format</summary>
        public D2dTextFormat TextFormat
        {
            get { return m_d2dTextFormat; }
            set { SetDisposableField(value, ref m_d2dTextFormat); }
        }

        /// <summary>
        /// Gets or sets the brush (pen) used to draw diagram outlines</summary>
        public D2dBrush OutlineBrush
        {
            get { return m_outlineBrush; }
            set { SetDisposableField(value, ref m_outlineBrush); }
        }
       
        /// <summary>
        /// Gets or sets the gradient brush (pen) used to fill diagram items</summary>
        public D2dLinearGradientBrush FillGradientBrush
        {
            get { return m_fillLinearGradientBrush; }
            set { SetDisposableField(value, ref m_fillLinearGradientBrush); }
        }

        /// <summary>
        /// Gets or sets the brush (pen) used to fill diagram items</summary>
        public D2dBrush FillBrush
        {
            get { return m_fillBrush; }
            set { SetDisposableField(value, ref m_fillBrush); }
        }

        /// <summary>
        /// Gets or sets the brush (pen) used to draw diagram text</summary>
        public D2dBrush TextBrush
        {
            get { return m_textBrush; }
            set { SetDisposableField(value, ref m_textBrush); }
        }

        /// <summary>
        /// Gets or sets the brush (pen) used to outline highighted (selected) diagram items</summary>
        public D2dBrush HighlightBrush
        {
            get { return m_highlightBrush; }
            set { SetDisposableField(value, ref m_highlightBrush); }
        }

        /// <summary>
        /// Gets or sets the brush (pen) used to outline the last highighted (selected) diagram item</summary>
        public D2dBrush LastHighlightBrush
        {
            get { return m_lastHighlightBrush; }
            set { SetDisposableField(value, ref m_lastHighlightBrush); }
        }

        /// <summary>
        /// Gets or sets the brush that paints the background of selected text</summary>
        public D2dBrush TextHighlightBrush
        {
            get { return m_textHighlightBrush; }
            set { SetDisposableField(value, ref m_textHighlightBrush); }
        }

        /// <summary>
        /// Gets or sets the brush (pen) used to outline ghosted diagram items</summary>
        public D2dBrush GhostBrush
        {
            get { return m_ghostBrush; }
            set { SetDisposableField(value, ref m_ghostBrush); }
        }
       
        /// <summary>
        /// Gets or sets the brush (pen) used to outline hidden diagram items</summary>
        public D2dBrush HiddenBrush
        {
            get { return m_hiddenBrush; }
            set { SetDisposableField(value, ref m_hiddenBrush); }
        }

        /// <summary>
        /// Gets or sets the brush (pen) used to outline templated subgroup diagram items</summary>
        public D2dBrush TemplatedInstance
        {
            get { return m_templatedInstance; }
            set { SetDisposableField(value, ref m_templatedInstance); }
        }

        /// <summary>
        /// Gets or sets the brush (pen) used to outline copy instance subgroup diagram items</summary>
        public D2dBrush CopyInstance
        {
            get { return m_copyInstance; }
            set { SetDisposableField(value, ref m_copyInstance); }
        }
               
        /// <summary>
        /// Gets or sets the brush (pen) used to fill hot-tracked diagram items</summary>
        public D2dBrush HotBrush
        {
            get { return m_hotBrush; }
            set { SetDisposableField(value, ref m_hotBrush); }
        }

        /// <summary>
        /// Gets or sets the brush (pen) used to fill hot-tracked diagram sub-items</summary>
        public D2dBrush DragSourceBrush
        {
            get { return m_dragSourceBrush; }
            set { SetDisposableField(value, ref m_dragSourceBrush); }
        }

        /// <summary>
        /// Gets or sets the brush (pen) used to fill hot-tracked diagram sub-items</summary>
        public D2dBrush DropTargetBrush
        {
            get { return m_dropTargetBrush; }
            set { SetDisposableField(value, ref m_dropTargetBrush); }
        }

        /// <summary>
        /// Gets or sets the brush (pen) used to outline diagram items with errors</summary>
        public D2dBrush ErrorBrush
        {
            get { return m_errorBrush; }
            set { SetDisposableField(value, ref m_errorBrush); }
        }

        /// <summary>
        /// Gets or sets the brush (pen) used to draw the info/tool tip background</summary>
        public D2dBrush InfoBrush
        {
            get { return m_infoBrush; }
            set { SetDisposableField(value, ref m_infoBrush); }
        }
              
        /// <summary>
        /// Gets or sets the brush (pen) used to hover border</summary>
        public D2dBrush HoverBorderBrush
        {
            get { return m_hoverBorderBrush; }
            set { SetDisposableField(value, ref m_hoverBorderBrush); }
        }

        /// <summary>
        /// Returns the custom brush (pen) corresponding to the key, or null</summary>
        /// <param name="key">Key identifying brush</param>
        /// <returns>Custom brush corresponding to the key, or null</returns>
        public D2dBrush GetCustomOrDefaultBrush(object key)
        {
            // try to use custom brush (pen) if registered
            D2dBrush brush;
            m_brushes.TryGetValue(key, out brush);
            if (brush == null)  // use a default brush
                brush = m_fillLinearGradientBrush;
            return brush;
        }

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
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                m_fillBrush.Dispose();
                m_textBrush.Dispose();
                m_d2dTextFormat.Dispose();
                m_highlightBrush.Dispose();
                m_lastHighlightBrush.Dispose();
                m_ghostBrush.Dispose();
                m_hiddenBrush.Dispose();
                m_templatedInstance.Dispose();
                m_copyInstance.Dispose();
                m_hotBrush.Dispose();
                m_errorBrush.Dispose();
                m_highlightBrush.Dispose();

                foreach (D2dBrush brush in m_brushes.Values)
                    brush.Dispose();
                m_brushes.Clear();

                foreach (D2dBitmap bitmap in m_bitmaps.Values)
                    bitmap.Dispose();
                m_bitmaps.Clear();                                    
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets or sets the diagram picking tolerance</summary>
        public int PickTolerance
        {
            get { return m_pickTolerance; }
            set { m_pickTolerance = value; }
        }
  
        /// <summary>
        /// Gets the theme's brush (pen) for the given drawing style</summary>
        /// <param name="style">Drawing style</param>
        /// <returns>Brush for the given drawing style</returns>
        public D2dBrush GetOutLineBrush(DiagramDrawingStyle style)
        {
            switch (style)
            {
                case DiagramDrawingStyle.Normal:
                    return m_outlineBrush;
                case DiagramDrawingStyle.Selected:
                    return m_highlightBrush;
                case DiagramDrawingStyle.LastSelected:
                    return m_lastHighlightBrush;
                case DiagramDrawingStyle.Hot:
                    return m_hotBrush;
                case DiagramDrawingStyle.DragSource:
                    return m_dragSourceBrush;
                case DiagramDrawingStyle.DropTarget:
                    return m_dropTargetBrush;
                case DiagramDrawingStyle.Ghosted:
                    return m_ghostBrush;
                case DiagramDrawingStyle.Hidden:
                    return m_hiddenBrush;
                case DiagramDrawingStyle.TemplatedInstance:
                    return m_templatedInstance;
                case DiagramDrawingStyle.CopyInstance:
                    return m_copyInstance;
                case DiagramDrawingStyle.Error:
                default:
                    return m_errorBrush;
            }
        }

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
         
        private readonly Dictionary<object, D2dBrush> m_brushes = new Dictionary<object, D2dBrush>();
        private readonly Dictionary<object, D2dBitmap> m_bitmaps = new Dictionary<object, D2dBitmap>();        
        private D2dTextFormat m_d2dTextFormat;
        private D2dBrush m_fillBrush;
        private D2dBrush m_textBrush;        
        private D2dBrush m_highlightBrush;
        private D2dBrush m_textHighlightBrush;
        private D2dBrush m_lastHighlightBrush;
        private D2dBrush m_ghostBrush;
        private D2dBrush m_hiddenBrush;
        private D2dBrush m_templatedInstance;
        private D2dBrush m_copyInstance;
        private D2dBrush m_hotBrush;
        private D2dBrush m_dropTargetBrush;
        private D2dBrush m_dragSourceBrush;
        private D2dBrush m_errorBrush;        
        private D2dBrush m_infoBrush;        
        private D2dBrush m_outlineBrush;                         
        private D2dBrush m_hoverBorderBrush;        
        private D2dLinearGradientBrush m_fillLinearGradientBrush;
        private int m_pickTolerance = 3;
    }
    
}
