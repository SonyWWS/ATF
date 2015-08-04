//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Renders non-client area of the form</summary>
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")
    ,SuppressUnmanagedCodeSecurity()]
    public class FormNcRenderer : NativeWindow
    {
        #region skinInfo
        /// <summary>
        /// Class for skin information</summary>
        public class SkinInfo
        {
            /// <summary>
            /// Construct instance with default values</summary>
            public SkinInfo()
            {
                ActiveBorderColor = Color.FromArgb(0, 121, 203);
                InactiveBorderColor = Color.FromArgb(70, 70, 75);

                TitleBarBackColor = Color.FromArgb(45, 45, 48);
                TitleBarForeColor = Color.FromArgb(160, 160, 160);
                ActiveTitleBarForeColor = Color.FromArgb(250, 250, 250);

                CaptionButtonHoverColor = Color.FromArgb(70, 70, 75);
            }

            /// <summary>
            /// Get or set color used for 
            /// drawing active border</summary>
            public Color ActiveBorderColor { get; set; }

            /// <summary>
            /// Get or set color used for 
            /// drawing inactive border</summary>
            public Color InactiveBorderColor { get; set; }

            /// <summary>
            /// Get or set color of the title bar and frame.</summary>
            public Color TitleBarBackColor { get; set; }

            /// <summary>
            /// Get or set title bar foreground color.
            /// </summary>
            public Color TitleBarForeColor { get; set; }

            /// <summary>
            /// Get or set title bar foreground color 
            /// for active title bar</summary>
            public Color ActiveTitleBarForeColor { get; set; }

            /// <summary>
            /// Get or set caption button hover color</summary>
            public Color CaptionButtonHoverColor { get; set; }
        }
        #endregion

        /// <summary>
        /// Construct new instance</summary>
        /// <param name="form">Form</param>
        public FormNcRenderer(Form form)
        {
            if (form == null)
                throw new ArgumentNullException();
            m_form = form;
            if (s_captionFormat == null)
            {
                s_captionFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };
            }

            if (s_context == null)
                s_context = new BufferedGraphicsContext();

            if (form.Handle != IntPtr.Zero)
                HanldeCreated();

            m_form.HandleCreated += (sender, e) => HanldeCreated();
            m_form.HandleDestroyed += (sender, e) => ReleaseHandle();
            

            // disable custom painting for parented form.
            m_disabled = form.Parent != null;
            m_form.ParentChanged += (sender, e) =>
            {
                CustomPaintDisabled = m_form == null || m_form.Parent != null;
            };
        }

        /// <summary>
        /// Gets or sets skin used for
        /// drawing title bar and caption buttons</summary>
        public SkinInfo Skin
        {
            get { return m_skin; }
            set
            {
                if (value != null)
                {
                    m_skin = value;
                    PaintTitleBar(m_active);
                }
            }

        }

        /// <summary>
        /// Get or set whether custom painting disabled</summary>
        public bool CustomPaintDisabled
        {
            get 
            {
                return m_disabled || (m_form != null && m_form.FormBorderStyle == FormBorderStyle.None);
            }
            set
            {       
                m_disabled = m_form == null || m_form.Parent != null || value;
                    
                if (m_form != null && !m_form.IsDisposed)
                {                    
                    if (m_disabled)
                    {
                        SetWindowTheme(m_form.Handle, "Explorer", null);
                    }
                    else
                    {
                        m_active = Form.ActiveForm == m_form;
                        SetWindowTheme(m_form.Handle, "", "");
                        CreateCaptionButtons();                       
                    }
                }                
            }

        }

        #region base overrides        
        protected override void WndProc(ref Message m)
        {
            if (CustomPaintDisabled)
            {
                base.WndProc(ref m);
                return;
            }
            bool handled = false;
            switch ((uint)m.Msg)
            {
                case WinMessages.WM_NCPAINT:
                    handled = PaintTitleBar(m_active);
                    if (handled) m.Result = IntPtr.Zero;
                    break;

                case WinMessages.WM_ACTIVATEAPP:
                    m_active = m.WParam != IntPtr.Zero;
                    handled = PaintTitleBar(m_active);
                    if (handled) m.Result = IntPtr.Zero;
                    break;

                case WinMessages.WM_ACTIVATE:
                    int lwp = LowWord(m.WParam.ToInt32());
                    m_active = lwp != 0;
                    handled = PaintTitleBar(m_active);
                    if (handled) m.Result = IntPtr.Zero;

                    // let the window process this message 
                    // so the form can raise activated and deactivate events.
                    handled = false; 
                    break;
                case WinMessages.WM_NCACTIVATE:
                    if (m.WParam != IntPtr.Zero)
                        m.Result = IntPtr.Zero;                        
                    else
                        m.Result = (IntPtr)1;
                    handled = true;                    
                    PaintTitleBar(m.WParam != IntPtr.Zero);
                    break;
                case WinMessages.WM_NCUAHDRAWCAPTION:
                case WinMessages.WM_NCUAHDRAWFRAME:
                    handled = true; // ignore theme related messages.
                    break;
               
                case WinMessages.WM_SIZE:
                    {                       
                        if (m_paintOnResize)
                        {                          
                            m_paintOnResize = false;
                            PaintTitleBar(m_active);
                        }
                    }
                    break;
                case WinMessages.WM_SYSCOMMAND:
                    uint flags = (uint)m.WParam & 0xFFF0;
                    m_paintOnResize =
                        (flags & SYSCOMMANDWPARAM.SC_RESTORE) != 0
                            || (flags & SYSCOMMANDWPARAM.SC_MAXIMIZE) != 0;
                    break;
                case WinMessages.WM_EXITSIZEMOVE:
                    PaintTitleBar(m_active);
                    break;

                case WinMessages.WM_STYLECHANGED:
                    CreateCaptionButtons();                   
                    m.Result = IntPtr.Zero;
                    handled = true;
                    break;

                case WinMessages.WM_SETICON:
                    CreateCaptionButtons();                    
                    break;

                case WinMessages.WM_SETTEXT:
                    base.WndProc(ref m);
                    PaintTitleBar(m_active);
                    handled = true;
                    break;

                case WinMessages.WM_NCHITTEST:
                    handled = OnHitTest(ref m);
                    break;

                case WinMessages.WM_NCMOUSELEAVE:
                    SetHoverState(null);
                    break;
                case WinMessages.WM_NCLBUTTONDOWN:
                    handled = OnNcLButtonDown(ref m);
                    break;

                case WinMessages.WM_NCLBUTTONUP:
                    handled = OnNcLButtonUp(ref m);
                    break;

                case WinMessages.WM_NCMOUSEMOVE:
                    handled = OnNcMouseMove(ref m);
                    break;

            }
            if (!handled)            
                base.WndProc(ref m);
        }        
        #endregion

        #region mouse handling methods

        private bool OnNcLButtonDown(ref Message msg)
        {
            bool handled = false;
            foreach (var b in m_captionButtons)
            {
                b.MouseIn = false;
                b.Pressed = false;
            }
            var btn = FindButtonById(msg.WParam.ToInt32());
            if (btn != null)
            {
                btn.MouseIn = true;
                btn.Pressed = true;
                handled = true;
                msg.Result = IntPtr.Zero;
            }

            uint hit = (uint)msg.WParam;
            if (hit == HitTest.HTSYSMENU)
            {
                SendMessage(m_form.Handle, WinMessages.WM_SYSCOMMAND,
                    (IntPtr)SYSCOMMANDWPARAM.SC_MOUSEMENU, msg.LParam);
                msg.Result = IntPtr.Zero;
                handled = true;
            }

            PaintTitleBar(m_active);
            return handled;
        }

        private bool OnNcMouseMove(ref Message msg)
        {
            if (Control.MouseButtons != MouseButtons.Left)
            {
                var btn = FindButtonById((int)msg.WParam);
                SetHoverState(btn);
            }
            return false;
        }

        private bool OnNcLButtonUp(ref Message msg)
        {
            var pressedButton = GetPressedButton();
            bool handled = false;
            foreach (var b in m_captionButtons)
            {
                b.MouseIn = false;
                b.Pressed = false;
            }

            var btn = FindButtonById(msg.WParam.ToInt32());
            if (btn != null && btn == pressedButton)
            {
                handled = true;
                msg.Result = (IntPtr)0;
                PaintTitleBar(m_active);
                m_paintOnResize = true;
                btn.PerformAction(m_form);                                
            }
            PaintTitleBar(m_active);
            return handled;
        }

        private CustomCaptionButton GetPressedButton()
        {
            foreach (var b in m_captionButtons)
                if (b.Pressed) return b;
            return null;
        }

        private CustomCaptionButton FindButtonById(int id)
        {
            foreach (var btn in m_captionButtons)
                if (btn.Id == id) return btn;
            return null;
        }

        private void SetHoverState(CustomCaptionButton btn)
        {
            bool repaint = false;
            foreach (var button in m_captionButtons)
            {
                if (button == btn)
                {
                    repaint |= !btn.MouseIn;
                    btn.MouseIn = true;
                }
                else
                {
                    repaint |= button.MouseIn;
                    button.MouseIn = false;
                }
            }
            if (repaint)
            {
                PaintTitleBar(m_active);
            }
        }
        
        private bool OnHitTest(ref Message msg)
        {            
            Point scrPt = new Point(msg.LParam.ToInt32());
            Point winPt = PointToWindow(scrPt);
            
            // mouse is in client area.
            if (m_winClientRect.Contains(winPt))
            {                               
                msg.Result = (IntPtr)HitTest.HTCLIENT;
                return true;
            }

            // compute screen bound excluding  border.
            Rectangle winNoBorder = m_winRect;
            winNoBorder.Inflate(-m_borderSize, -m_borderSize);

            // mouse is on border, let window process this case.
            if (!winNoBorder.Contains(winPt)) return false;

            // title bar rect in screen space.
            Rectangle winCapRect = m_winRect;
            winCapRect.Height = m_titleAndBorderSize;

            // mouse is on titlebar area.
            if (winCapRect.Contains(winPt))
            {
                
                // hit test caption buttons                
                foreach (var btn in m_captionButtons)
                {
                    if (btn.Bound.Contains(winPt))
                    {                       
                        msg.Result = (IntPtr)btn.Id;
                        return true;
                    }
                }

                // hit test icon if applicable.
                if (m_showIcon && m_iconRect.Contains(winPt))
                {                   
                    msg.Result = (IntPtr)HitTest.HTSYSMENU;
                    return true;
                }
                
                msg.Result = (IntPtr)HitTest.HTCAPTION;
                return true;
            }

            return false;
        }

        #endregion

        private void CreateCaptionButtons()
        {
            m_captionButtons.Clear();
            m_showIcon = false;
            m_iconRect = Rectangle.Empty;
            FormBorderStyle bstyle = m_form.FormBorderStyle;

            if (bstyle == FormBorderStyle.None || m_form.ControlBox == false)
                return;

            m_showIcon = !(m_form.ControlBox == false
                || m_form.ShowIcon == false
                || bstyle == FormBorderStyle.FixedToolWindow
                || bstyle == FormBorderStyle.SizableToolWindow
                || bstyle == FormBorderStyle.FixedDialog);

            CustomCaptionButton closebtn = new CustomCaptionButton(HitTest.HTCLOSE);
            m_captionButtons.Add(closebtn);

            bool toolWindow = m_form.FormBorderStyle == FormBorderStyle.FixedToolWindow
                || m_form.FormBorderStyle == FormBorderStyle.SizableToolWindow;

            if (!toolWindow)
            {
                if (m_form.MaximizeBox)
                    m_captionButtons.Add(new CustomCaptionButton(HitTest.HTMAXBUTTON));
                if (m_form.MinimizeBox)
                    m_captionButtons.Add(new CustomCaptionButton(HitTest.HTMINBUTTON));
            }
            UpdateBounds();
            UpdateCaptionButtons();
        }
        private Size GetCaptionButtonSize()
        {
            FormBorderStyle bstyle = m_form.FormBorderStyle;

            if (bstyle == FormBorderStyle.FixedToolWindow
                || bstyle == FormBorderStyle.SizableToolWindow)
                return SystemInformation.ToolWindowCaptionButtonSize;
            return SystemInformation.CaptionButtonSize;


        }
        private void UpdateCaptionButtons()
        {
            int capSize = GetCaptionButtonSize().Height;           
            int top = Math.Min(4, m_titleAndBorderSize - (capSize + 1));
            int iconSize = m_titleAndBorderSize - 2;
            int iconTop = 2;
            int iconLeft = 2;

            if (m_form.WindowState == FormWindowState.Maximized)
            {
                top = m_borderSize;
                iconTop = top;
                iconLeft = top;
                iconSize -= top;
            }

            Rectangle btnBound = new Rectangle(m_winRect.Width - (capSize + 3), top, capSize, capSize);
            foreach (var btn in m_captionButtons)
            {
                btn.Bound = btnBound;
                btnBound.X -= (capSize + 1);
            }

            if (m_showIcon)
            {
                m_iconRect = new Rectangle(iconLeft, iconTop, iconSize, iconSize);
            }
        }
        /// <summary>
        /// Transform the give point from screen space to window space.        
        /// </summary>       
        private Point PointToWindow(Point scrPt)
        {
            return new Point(scrPt.X - m_form.Location.X, scrPt.Y - m_form.Location.Y);
        }

        private void HanldeCreated()
        {
            if (m_form.Parent == null)
            {
                SetWindowTheme(m_form.Handle, "", "");               
                CreateCaptionButtons();               
            }
            else
                m_disabled = true;
            AssignHandle(m_form.Handle);
        }

        private bool PaintTitleBar(bool active)
        {
            if (CustomPaintDisabled || m_form == null || !m_form.Visible)
                return false;
           
            UpdateBounds();
            UpdateCaptionButtons();           
            IntPtr hdc = GetDCEx(m_form.Handle, IntPtr.Zero,
                 (uint)(DCXFlags.DCX_CACHE | DCXFlags.DCX_CLIPSIBLINGS | DCXFlags.DCX_WINDOW));
            if (hdc == IntPtr.Zero)
                return false;

            ExcludeClipRect(hdc,
                m_winClientRect.X,
                m_winClientRect.Y,
                m_winClientRect.Right,
                m_winClientRect.Bottom);

            Size maxBuffer = s_context.MaximumBuffer;
            if (m_winRect.Width > maxBuffer.Width || m_winRect.Height > maxBuffer.Height)
            {
                s_context.MaximumBuffer = m_winRect.Size;
            }

            var backbuffer = s_context.Allocate(hdc, m_winRect);
            backbuffer.Graphics.SetClip(m_winClientRect, CombineMode.Exclude);
            PaintTitleBar(backbuffer.Graphics, active);          
            backbuffer.Render();
            backbuffer.Dispose();
            ReleaseDC(m_form.Handle, hdc);
            return true;
        }

        private void PaintTitleBar(Graphics g, bool active)
        {            
            g.Clear(m_skin.TitleBarBackColor);
            
            s_genPen.Color = active ? m_skin.ActiveBorderColor
                : m_skin.InactiveBorderColor;

            Rectangle borderRect = m_winRect;
            borderRect.X = 1;
            borderRect.Y = 1;
            borderRect.Width -= 2;
            borderRect.Height -= 2;

            s_genPen.Width = 2.0f;
            g.DrawRectangle(s_genPen, borderRect);

            
            // draw caption buttoon.
            foreach (var button in m_captionButtons)
                button.Draw(g, m_skin, m_form, active);

            if (m_showIcon)
                g.DrawIcon(m_form.Icon, m_iconRect);

            string captionStr = m_form.Text;
            if (!string.IsNullOrWhiteSpace(captionStr))
            {
                int w = m_iconRect.Width > 0 ? -(m_iconRect.Width + 1) : -6;
                if (m_captionButtons.Count > 0)
                    w += m_captionButtons[m_captionButtons.Count - 1].Bound.X;
                else
                    w += m_winRect.Width;

                int x = m_iconRect.Right > 0 ? m_iconRect.Right : 6;
                int top = 0;
                int height = m_titleAndBorderSize;
                if (m_form.WindowState == FormWindowState.Maximized)
                {
                    top = m_borderSize;
                    height = m_titleSize;
                }


                s_genBrush.Color = active ? m_skin.ActiveTitleBarForeColor
               : m_skin.TitleBarForeColor;

                Rectangle strRect = new Rectangle(x, top, w, height);
                g.DrawString(captionStr,
                    SystemFonts.CaptionFont,
                    s_genBrush,
                    strRect,
                    s_captionFormat);
            }
        }

        private void UpdateBounds()
        {           
            var scrRect = new User32.RECT();
            GetWindowRect(m_form.Handle, ref scrRect);
            int w = scrRect.Width;
            int h = scrRect.Height;            
            m_winRect = new Rectangle(0, 0, w, h);

            var clRect = new User32.RECT();
            GetClientRect(m_form.Handle, ref clRect);
            int cw = clRect.Width;
            int ch = clRect.Height;
          
            m_borderSize = (w - cw) / 2;
            m_titleSize = (h - ch) - 2 * m_borderSize;
            m_titleAndBorderSize = m_borderSize + m_titleSize;
            m_winClientRect = new Rectangle(m_borderSize, m_titleAndBorderSize,
                    cw, ch);
        }

        private bool m_active; // is m_form active.
        private bool m_paintOnResize;
        private bool m_disabled;
        private SkinInfo m_skin = new SkinInfo();

        private static Pen s_genPen = new Pen(Color.White);
        private static SolidBrush s_genBrush = new SolidBrush(Color.White);

        // s_context was previously disposed of during the Application.ApplicationExit event.
        // There is no need to dispose of this resource when the application is exiting.
        //  Also, for some apps with multiple GUI threads, this event could be fired while the
        //  app is still running. In this case, disposing the resource will crash the application.
        //  For example, the Metrics splash screen causes this event to fire when metrics starts.
        private static BufferedGraphicsContext s_context;

        private static StringFormat s_captionFormat;
        private bool m_showIcon;
        private Rectangle m_iconRect; // icon rect in window space.        
        private Rectangle m_winRect; // Form rectangle.
        private Rectangle m_winClientRect; // Form client rect in window space.
        private int m_borderSize;  // the size of border in pixels
        private int m_titleSize; // the size of title bar in pixels.
        private int m_titleAndBorderSize; // the size of title and border in pixels.

        private Form m_form;

        public int HiWord(int val)
        {
            return (val >> 16) & 0xFFFF;
        }

        public int LowWord(int val)
        {
            return val & 0xFFFF;
        }

        private List<CustomCaptionButton> m_captionButtons = new List<CustomCaptionButton>();
        private class CustomCaptionButton
        {
            public CustomCaptionButton(int id)
            {
                Id = id;
            }

            public void PerformAction(Form form)
            {
                if (Id == HitTest.HTCLOSE)
                {
                    form.Close();
                }
                else if (Id == HitTest.HTMAXBUTTON)
                {
                    if (form.WindowState == FormWindowState.Normal)
                        form.WindowState = FormWindowState.Maximized;
                    else
                        form.WindowState = FormWindowState.Normal;
                }
                else if (Id == HitTest.HTMINBUTTON)
                {
                    form.WindowState = FormWindowState.Minimized; ;
                }
            }

            public void Draw(Graphics g, SkinInfo skin, Form form, bool active)
            {
                if (MouseIn)
                {
                    s_genBrush.Color = skin.CaptionButtonHoverColor;
                    g.FillRectangle(s_genBrush, Bound);
                }
                else
                {
                    s_genBrush.Color = skin.TitleBarBackColor;
                }

                s_genPen.Color = active ? skin.ActiveTitleBarForeColor
                    : skin.TitleBarForeColor;

                int margin = (int)(Bound.Height * 0.3f);
                Rectangle rect = Bound;
                rect.Inflate(-margin, -margin);


                if (Id == HitTest.HTCLOSE)
                {
                    var smoothMode = g.SmoothingMode;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    s_genPen.Width = 2.0f;
                    g.DrawLine(s_genPen, rect.X, rect.Y, rect.Right, rect.Bottom);
                    g.DrawLine(s_genPen, rect.Right, rect.Y, rect.X, rect.Bottom);
                    g.SmoothingMode = smoothMode;
                }
                else if (Id == HitTest.HTMAXBUTTON)
                {
                    s_genPen.Width = 1.0f;
                    if (form.WindowState == FormWindowState.Normal)
                    {
                        g.DrawRectangle(s_genPen, rect);
                        g.DrawLine(s_genPen, rect.X, rect.Y + 1, rect.Right, rect.Y + 1);
                    }
                    else
                    {
                        Rectangle rect2 = rect;

                        rect2.X += margin / 2;
                        rect2.Y -= margin / 2;

                        g.DrawRectangle(s_genPen, rect2);
                        g.DrawLine(s_genPen, rect2.X, rect2.Y + 1, rect2.Right, rect2.Y + 1);

                        rect2.X -= margin / 2;
                        rect2.Y += margin / 2;

                        g.FillRectangle(s_genBrush, rect2);
                        g.DrawRectangle(s_genPen, rect2);
                        g.DrawLine(s_genPen, rect2.X, rect2.Y + 1, rect2.Right, rect2.Y + 1);
                    }

                }
                else if (Id == HitTest.HTMINBUTTON)
                {
                    s_genPen.Width = 2;
                    g.DrawLine(s_genPen, rect.X, rect.Bottom, rect.Right, rect.Bottom);
                }
            }

            private static Pen s_genPen = new Pen(Color.White);
            private static SolidBrush s_genBrush = new SolidBrush(Color.White);

            public readonly int Id;
            // in window space.
            public Rectangle Bound;
            public bool Pressed;
            public bool MouseIn;
        }

        #region PInvoke

        private static class HitTest
        {
            public const int HTBORDER = 18;
            public const int HTBOTTOM = 15;
            public const int HTBOTTOMLEFT = 16;
            public const int HTBOTTOMRIGHT = 17;
            public const int HTCAPTION = 2;
            public const int HTCLIENT = 1;
            public const int HTCLOSE = 20;
            public const int HTERROR = -2;
            public const int HTGROWBOX = 4;
            public const int HTSIZE = 4;
            public const int HTHELP = 21;
            public const int HTHSCROLL = 6;
            public const int HTLEFT = 10;
            public const int HTMENU = 5;
            public const int HTMAXBUTTON = 9;
            public const int HTZOOM = 9;

            public const int HTMINBUTTON = 8;
            public const int HTREDUCE = 8; // minimize button.

            public const int HTNOWHERE = 0;

            public const int HTRIGHT = 11;

            public const int HTSYSMENU = 3;
            public const int HTTOP = 12;
            public const int HTTOPLEFT = 13;
            public const int HTTOPRIGHT = 14;
            public const int HTTRANSPARENT = -1;
            public const int HTVSCROLL = 7;

        }

        private static class WM_ACTIVATEState
        {
            public const int WA_INACTIVE = 0;
            public const int WA_ACTIVE = 1;
            public const int WA_CLICKACTIVE = 2;
        }

        private static class WinMessages
        {
            // icon
            public const uint WM_SETICON = 0x0080;

            // text
            public const uint WM_SETTEXT = 0x000C;
            public const uint WM_GETTEXT = 0x000d;

            // style related messages
            public const uint WM_STYLECHANGED = 0x007D;

            public const uint WM_ACTIVATE = 0x0006;
            public const uint WM_ACTIVATEAPP = 0x001C;

            // size related messages
            public const uint WM_EXITSIZEMOVE = 0x0232;
            public const uint WM_SIZE = 0x0005;
            public const uint WM_SIZING = 0x0214;
            public const uint WM_GETMINMAXINFO = 0x0024;
            public const uint WM_WINDOWPOSCHANGING = 0x0046;
            public const uint WM_WINDOWPOSCHANGED = 0x0047;

            // non cleint area related messages.
            public const uint WM_NCHITTEST = 0x0084;
            public const uint WM_NCPAINT = 0x0085;
            public const uint WM_NCLBUTTONDOWN = 0x00A1;
            public const uint WM_NCMOUSEMOVE = 0x00A0;
            public const uint WM_NCMOUSELEAVE = 0x02A2;
            public const uint WM_NCLBUTTONUP = 0x00A2;
            public const uint WM_NCCALCSIZE = 0x0083;
            public const uint WM_NCACTIVATE = 0x0086;
            public const uint WM_MOUSEHOVER = 0x02A1;

            public const uint WM_SHOWWINDOW = 0x0018;

            // Undocumented theme related messages.
            public const uint WM_NCUAHDRAWCAPTION = 0x00AE;
            public const uint WM_NCUAHDRAWFRAME = 0x00AF;

            // sys-command
            public const uint WM_SYSCOMMAND = 0x0112;

            // cursor.
            public const uint WM_SETCURSOR = 0x0020;

            public const uint WM_ERASEBKGND = 0x0014;
        }

        // note: bitwisse and the flag with 0xFFF0
        /// <summary>
        /// WM_SYSCOMMAND message flags</summary>
        /// <remarks>For details, see http://msdn.microsoft.com/en-us/library/windows/desktop/ms646360%28v=vs.85%29.aspx. </remarks>
        public static class SYSCOMMANDWPARAM
        {
            public const uint SC_MAXIMIZE = 0xF030;
            public const uint SC_MINIMIZE = 0xF020;
            public const uint SC_RESTORE = 0xF120;
            public const uint SC_MOUSEMENU = 0xF090; // system menu.
            public const uint SC_CLOSE = 0xF060;

        }

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hwnd, String pszSubAppName, String pszSubIdList);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern void DisableProcessWindowsGhosting();

        public static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLong32(hWnd, nIndex);
            }
            return GetWindowLongPtr64(hWnd, nIndex);
        }
        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern Int32 SetWindowLong(IntPtr hWnd, Int32 Offset, Int32 newLong);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref User32.RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hWnd, ref User32.RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDCEx(IntPtr hwnd, IntPtr hrgnclip, uint fdwOptions);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hDC);

        [DllImport("User32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern IntPtr GetActiveWindow();


        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRectRgn(int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect);

        [DllImport("gdi32.dll")]
        public static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);


        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteObject(IntPtr hObject);


        [DllImport("gdi32.dll")]
        static extern int ExcludeClipRect(
            IntPtr hdc,
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect);

        /// <summary>
        /// Rendering flags</summary>
        [Flags]
        public enum DCXFlags : uint
        {
            DCX_CACHE = 0x2,
            DCX_CLIPCHILDREN = 0x8,
            DCX_CLIPSIBLINGS = 0x10,
            DCX_EXCLUDERGN = 0x40,
            DCX_EXCLUDEUPDATE = 0x100,
            DCX_INTERSECTRGN = 0x80,
            DCX_INTERSECTUPDATE = 0x200,
            DCX_LOCKWINDOWUPDATE = 0x400,
            DCX_NORECOMPUTE = 0x100000,
            DCX_NORESETATTRS = 0x4,
            DCX_PARENTCLIP = 0x20,
            DCX_VALIDATE = 0x200000,
            DCX_WINDOW = 0x1,
            DCX_UNDOCUMENTED = 0x10000
        }

        /// <summary>
        /// Window positioning flags</summary>
        /// <remarks>For details, see http://msdn.microsoft.com/en-us/library/windows/desktop/ms632612%28v=vs.85%29.aspx. </remarks>
        [Flags]
        public enum SWPFlags
        {
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0x0002,
            SWP_NOZORDER = 0x0004,
            SWP_NOREDRAW = 0x0008,
            SWP_NOACTIVATE = 0x0010,
            SWP_FRAMECHANGED = 0x0020,
        }

        /// <summary>
        /// GetWindowLong() value indexes</summary>
        public enum GetWindowLongIndex : int
        {
            GWL_STYLE = -16,
            GWL_EXSTYLE = -20,
            // see GetWindowLong  Docs for more indices.
        }

        /// <summary>
        /// Enumeration for window styles</summary>
        public enum WindowStyles : ulong
        {
            WS_CAPTION = 0x00C00000,
            WS_BORDER = 0x00800000,
            WS_MAXIMIZEBOX = 0x00010000,
            WS_MINIMIZEBOX = 0x00020000,
            WS_SYSMENU = 0x00080000,
            WS_POPUP = 0x80000000,
        }

        /// <summary>
        /// Structure for point</summary>
        [StructLayout(LayoutKind.Sequential)]
        [Obsolete("Please use User32.POINT instead")]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// Structure for rectangle</summary>
        [StructLayout(LayoutKind.Sequential)]
        [Obsolete("Please use User32.RECT instead")]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public int Width
            {
                get { return Right - Left; }
            }
            public int Height
            {
                get { return Bottom - Top; }
            }
        }

        /// <summary>
        /// Information about a window's maximized size and position and its minimum and maximum tracking size.
        /// </summary>
        /// <remarks>For details, see http://msdn.microsoft.com/en-us/library/windows/desktop/ms632605%28v=vs.85%29.aspx. </remarks>
        [StructLayout(LayoutKind.Sequential)]
        [Obsolete("Please use User32.MINMAXINFO instead")]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        /// <summary>
        /// Information that an application can use while processing the WM_NCCALCSIZE message to calculate 
        /// the size, position, and valid contents of the client area of a window</summary>
        /// <remarks>For details, see http://msdn.microsoft.com/en-us/library/windows/desktop/ms632606%28v=vs.85%29.aspx. </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct NCCALCSIZE_PARAMS
        {
            public User32.RECT rect0;
            public User32.RECT rect1;
            public User32.RECT rect2;
            public IntPtr lppos; //a pointer to a WINDOWPOS
        };

        /// <summary>
        /// Structure with information about the size and position of a window</summary>
        /// <remarks>For details, see http://msdn.microsoft.com/en-us/library/windows/desktop/ms632612%28v=vs.85%29.aspx. </remarks>
        [StructLayout(LayoutKind.Sequential)]
        [Obsolete("Please use User32.WINDOWPOS instead")]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        #endregion
    }
}
