using System;
using System.Windows.Forms;
using System.Drawing;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Lightweight button that is drawn on the client
    /// area of the parent control.    
    /// </summary>
    public class OverlayButton : IDisposable
    {
        /// <summary>
        /// Construct new instance for the given parent Control.</summary>
        /// <param name="parent">Parent Control</param>
        public OverlayButton(Control parent)
        {
            Parent = parent;           
            m_bound = new Rectangle(0,0,16,16);
            Visible = true;
        }

        /// <summary>
        /// Event that is raised when the button is left-clicked</summary>
        public event EventHandler Click = delegate { };


        #region IDisposable implementation
        /// <summary>        
        /// Gets whether the object has been disposed of</summary>
        public bool IsDisposed
        {
            get { return m_disposed; }
        }
        private bool m_disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources</summary>
        public void Dispose()
        {
            if (m_disposed) return;
            Dispose(true);
            GC.SuppressFinalize(this);            
        }

        /// <summary>
        /// Disposes resources</summary>
        /// <param name="disposing">If true, then Dispose() called this method and managed resources should
        /// be released in addition to unmanaged resources. If false, then the finalizer called this method
        /// and no managed objects should be called and only unmanaged resources should be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (m_backgroundImage != null) m_backgroundImage.Dispose();
                    if (m_pressedImage != null) m_pressedImage.Dispose();
                    if (m_hoverImage != null) m_hoverImage.Dispose();
                    m_backgroundImage = null;
                    m_pressedImage = null;
                    m_hoverImage = null;
                    Parent = null;
                }
            }
            finally
            {
                m_disposed = true;
            }                        
        }

        /// <summary>
        /// Destructor</summary>
        ~OverlayButton()
        {
            Dispose(false);
        }        

        #endregion

        #region Properties

        /// <summary>
        /// Gets and sets tooltip text.
        /// </summary>
        public string ToolTipText
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets ToolTip.</summary>
        public ToolTip ToolTip
        {
            get;
            set;
        }
        /// <summary>
        /// Gets and sets parent control</summary>
        public Control Parent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether the button is visible</summary>
        public bool Visible
        {
            get { return m_visible; }
            set
            {
                if (!value)
                {
                    HideToolTip();
                }
                m_visible = value;
            }
        }
        private bool m_visible;
        
        /// <summary>
        /// Gets and sets Text property of the button</summary>
        public string Text
        {
            get;
            set;
        }
        /// <summary>
        /// Gets and sets Top of coordinate of the button.</summary>
        public int Top
        {
            get { return m_bound.Y; }
            set { m_bound.Y = value; }
        }

        /// <summary>
        /// Gets and sets Left coordinate of the button</summary>
        public int Left
        {
            get { return m_bound.X; }
            set { m_bound.X = value; }
        }

        /// <summary>
        /// Gets and sets width in pixels</summary>
        public int Width
        {
            get { return m_bound.Width; }
            set { m_bound.Width = value; }
        }

        /// <summary>
        /// Gets and sets Height</summary>
        public int Height
        {
            get{ return m_bound.Height;}
            set{ m_bound.Height = value;}
        }

        /// <summary>
        /// Gets and sets image that is showen 
        /// when the button is pressed</summary>
        public Image PressedImage
        {
            get { return m_pressedImage; }
            set
            {
                SetImage(value, ref m_pressedImage);              
            }
        }

        /// <summary>
        /// Gets and sets image that is shown when the mouse
        /// is over the button</summary>
        public Image HoverImage
        {
            get { return m_hoverImage; }
            set
            {
                SetImage(value, ref m_hoverImage);              
            }
        }

        /// <summary>
        /// Gets and sets background image</summary>
        public  Image BackgroundImage
        {
            get { return m_backgroundImage; }
            set
            {
                SetImage(value, ref m_backgroundImage);              
            }
        }

        #endregion

        #region mouse handling and drawing  methods.
        
        /// <summary>
        /// When the left mouse button is pressed, show pressed image if it is not null</summary>
        /// <param name="e">MouseEventArgs describing event</param>
        /// <returns>True iff the mouse event is handled</returns>        
        public bool MouseDown(MouseEventArgs e)
        {
            if (!Visible) return false;

            m_pressed = false;
            if ( m_bound.Contains(e.Location) && e.Button == MouseButtons.Left)
            {
                m_pressed = true;
                Click(this, EventArgs.Empty);
            }
            return m_pressed;
        }

        private bool m_tooltipActive;
        /// <summary>
        /// When mouse enters show hover image unless it is pressed</summary>
        /// <param name="e">MouseEventArgs describing event</param>
        /// <returns>True iff the mouse event is handled</returns>        
        public bool MouseMove(MouseEventArgs e)
        {
            if (!Visible) return false;
            bool oldval = m_mouseIn;
            m_mouseIn = m_bound.Contains(e.Location);
            if (m_mouseIn != oldval && Parent != null)
            {
                Parent.Invalidate();
            }

            if (m_mouseIn)
            {
                ShowToolTip(e.X, e.Y + m_bound.Height);                
            }
            else
            {
                HideToolTip();                
            }               
            return m_mouseIn;
        }

        /// <summary>
        /// When mouse button is released, show hover image</summary>     
        /// <param name="e">MouseEventArgs describing event</param>
        /// <returns>True iff the mouse event is handled</returns>        
        public bool MouseUp(MouseEventArgs e)
        {
            m_pressed = false;
            if (!Visible) return false;            
            return m_bound.Contains(e.Location);            
        }

        /// <summary>
        /// Draws button depending on its state</summary>
        /// <param name="g">Current GDI+ Graphics object</param>
        public void Draw(Graphics g)
        {
            if (Parent == null || !Visible)
                return;

            if (m_backgroundImage == null)
            {
                g.DrawLine(Pens.Red, m_bound.Left, m_bound.Top, m_bound.Width, m_bound.Height);
                g.DrawLine(Pens.Red, m_bound.Right, m_bound.Top,m_bound.Left, m_bound.Height);
                return;
            }

            if (m_pressedImage == null)
            {
                m_pressedImage = CreateNewBitmap(m_backgroundImage, 0.0f, -0.1f);
            }

            if (m_hoverImage == null)
            {
                m_hoverImage = CreateNewBitmap(m_backgroundImage, 0.8f, 0.1f);
            }

            Image img;
            if (m_pressed)
            {
                img = m_pressedImage;
            }
            else if (m_mouseIn)
            {
                img = m_hoverImage;

            }
            else
            {
                img = m_backgroundImage;
            }

            g.DrawImage(img, m_bound);

            if (!string.IsNullOrWhiteSpace(Text))
            {
                using (Brush brush = new SolidBrush(Parent.ForeColor))
                {
                    SizeF size = g.MeasureString(Text, Parent.Font);
                    g.DrawString(Text, Parent.Font, brush,
                       m_bound.Left + (m_bound.Width - size.Width) / 2.0f,
                       m_bound.Top + (m_bound.Height - size.Height) / 2.0f);
                }
            }            
        }

        #endregion

        #region private members
        private void ShowToolTip(int x, int y)
        {
            if (Parent != null && ToolTip != null && !string.IsNullOrWhiteSpace(ToolTipText))
            {
                if (!m_tooltipActive)
                {
                    m_tooltipActive = true;
                    ToolTip.Show(ToolTipText, Parent, x, y);
                }
            }
                
        }
        private void HideToolTip()
        {
            if (m_tooltipActive)
            {
                m_tooltipActive = false;
                ToolTip.RemoveAll();
            }
        }

        private void SetImage(Image img, ref Bitmap target)
        {
            if (target != null)
            {
                target.Dispose();
                target = null;
            }
            if (img != null)
            {
                target = new Bitmap(img);
            }
        }

        /// <summary>
        /// Create new bitmap with the new adjusted Saturation and
        /// Brightness</summary>        
        private Bitmap CreateNewBitmap(Bitmap src, float saturation, float brightness)
        {
            Bitmap result = (Bitmap)src.Clone();
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    Color srcColor = src.GetPixel(x, y);
                    float s = MathUtil.Clamp(srcColor.GetSaturation() + saturation, 0.0f, 1.0f);
                    float b = MathUtil.Clamp(srcColor.GetBrightness() + brightness, 0.0f, 1.0f);

                    Color destColor = ColorUtil.FromAhsb(srcColor.A, srcColor.GetHue(), s, b);
                    result.SetPixel(x, y, destColor);
                }
            }
            return result;
        }

        private bool m_pressed;
        private bool m_mouseIn;
        private Bitmap m_backgroundImage;
        private Bitmap m_pressedImage;
        private Bitmap m_hoverImage;
        private Rectangle m_bound; // button bounds in parent space.        
        #endregion        
    }
}
