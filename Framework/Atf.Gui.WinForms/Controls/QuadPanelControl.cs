//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Control holding 4 panels with 2-way splitter</summary>
    public class QuadPanelControl : Control
    {
        /// <summary>
        /// Constructor</summary>
        public QuadPanelControl()
        {
        }

        /// <summary>
        /// Constructor with context</summary>
        /// <param name="context">Context associated with this control</param>
        public QuadPanelControl(object context)
        {
            m_context = context;
        }

        /// <summary>
        /// Event that is raised when the splitter position changes due to a drag</summary>
        public event EventHandler<EventArgs> SplitterDragged;

        /// <summary>
        /// Gets the context associated with this control</summary>
        public object Context { get { return m_context; } }

        /// <summary>
        /// Gets or sets the top left control</summary>
        /// <value>Top left control</value>
        public Control TopLeft
        {
            get { return m_topLeft; }
            set
            {
                if (m_topLeft != null)
                {
                    Controls.Remove(m_topLeft);
                    m_topLeft.GotFocus -= ControlGotFocus;
                }

                m_topLeft = value;

                if (m_topLeft != null)
                {
                    Controls.Add(m_topLeft);
                    m_topLeft.GotFocus += ControlGotFocus;
                    SizeTopLeft();
                }
            }
        }

        /// <summary>
        /// Gets or sets the top right control</summary>
        /// <value>Top right control</value>
        public Control TopRight
        {
            get { return m_topRight; }
            set
            {
                if (m_topRight != null)
                {
                    Controls.Remove(m_topRight);
                    m_topRight.GotFocus -= ControlGotFocus;
                }

                m_topRight = value;

                if (m_topRight != null)
                {
                    Controls.Add(m_topRight);
                    m_topRight.GotFocus += ControlGotFocus;
                    SizeTopRight();
                }
            }
        }

        /// <summary>
        /// Gets or sets the bottom left control</summary>
        /// <value>Bottom left control</value>
        public Control BottomLeft
        {
            get { return m_bottomLeft; }
            set
            {
                if (m_bottomLeft != null)
                {
                    Controls.Remove(m_bottomLeft);
                    m_bottomLeft.GotFocus -= ControlGotFocus;
                }

                m_bottomLeft = value;

                if (m_bottomLeft != null)
                {
                    Controls.Add(m_bottomLeft);
                    m_bottomLeft.GotFocus += ControlGotFocus;
                    SizeBottomLeft();
                }
            }
        }

        /// <summary>
        /// Gets or sets the bottom right control</summary>
        /// <value>Bottom right control</value>
        public Control BottomRight
        {
            get { return m_bottomRight; }
            set
            {
                if (m_bottomRight != null)
                {
                    Controls.Remove(m_bottomRight);
                    m_bottomRight.GotFocus -= ControlGotFocus;
                }

                m_bottomRight = value;

                if (m_bottomRight != null)
                {
                    Controls.Add(m_bottomRight);
                    m_bottomRight.GotFocus += ControlGotFocus;
                    SizeBottomRight();
                }
            }
        }

        /// <summary>
        /// Gets or sets the active control</summary>
        public Control ActiveControl
        {
            get
            {
                return
                    (m_activeControl == null) ? m_topLeft : m_activeControl;
            }
            set
            {
                if (value != m_topLeft &&
                    value != m_topRight &&
                    value != m_bottomLeft &&
                    value != m_bottomRight)
                    throw new InvalidOperationException("Invalid control");

                m_activeControl = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the splitter x fraction of the total width of this Control (i.e., in the range [0,1]).
        /// Set to 1.0f and set EnableX to false to make the right two panels disappear.</summary>
        public float SplitX
        {
            get { return m_splitX; }
            set
            {
                if (m_splitX != value)
                {
                    SetSplitX(value);
                    SizeAll();
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets the splitter y fraction of the total height of this Control (i.e., in the range [0,1]).
        /// Set to 1.0f and set EnableY to false to make the bottom two panels disappear.</summary>
        public float SplitY
        {
            get { return m_splitY; }
            set
            {
                if (m_splitY != value)
                {
                    SetSplitY(value);
                    SizeAll();
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether splitter can move horizontally</summary>
        public bool EnableX
        {
            get { return m_enableX; }
            set { m_enableX = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether splitter can move vertically</summary>
        public bool EnableY
        {
            get { return m_enableY; }
            set { m_enableY = value; }
        }

        /// <summary>
        /// Gets or sets the splitter thickness</summary>
        public int SplitterThickness
        {
            get { return m_splitterThickness; }
            set
            {
                value = Math.Min(Math.Max(0, value), 20);
                if (m_splitterThickness != value)
                {
                    m_splitterThickness = value;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets the picking tolerance</summary>
        public int Tolerance
        {
            get { return m_tolerance; }
            set { m_tolerance = value; }
        }

        /// <summary>
        /// Gets or sets persisted settings as XML data</summary>
        public string PersistedSettings
        {
            get
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                XmlElement root = xmlDoc.CreateElement(SettingsElementName);
                xmlDoc.AppendChild(root);

                try
                {
                    root.SetAttribute(SettingsSplitXAttributeName, SplitX.ToString());
                    root.SetAttribute(SettingsSplitYAttributeName, SplitY.ToString());
                    root.SetAttribute(SettingsEnableXAttributeName, EnableX.ToString());
                    root.SetAttribute(SettingsEnableYAttributeName, EnableY.ToString());

                    if (xmlDoc.DocumentElement == null)
                        xmlDoc.RemoveAll();
                }
                catch (Exception ex)
                {
                    Outputs.WriteLine(
                        OutputMessageType.Error,
                        "{0}: Exception saving quad panel persisted settings: {1}", this, ex.Message);

                    xmlDoc.RemoveAll();
                }

                return xmlDoc.InnerXml.Trim();
            }

            set
            {
                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(value);

                    if (xmlDoc.DocumentElement == null)
                        return;

                    var root = xmlDoc.DocumentElement;

                    SplitX = float.Parse(root.GetAttribute(SettingsSplitXAttributeName));
                    SplitY = float.Parse(root.GetAttribute(SettingsSplitYAttributeName));
                    EnableX = bool.Parse(root.GetAttribute(SettingsEnableXAttributeName));
                    EnableY = bool.Parse(root.GetAttribute(SettingsEnableYAttributeName));
                }
                catch (Exception ex)
                {
                    Outputs.WriteLine(
                        OutputMessageType.Error,
                        "{0}: Exception loading quad panel persisted settings: {1}", this, ex.Message);
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:Sce.Atf.Controls.QuadPanelControl.SplitterDragged"/> event</summary>
        /// <param name="e">Event args containing the event data</param>
        protected virtual void OnSplitterDragged(EventArgs e)
        {
            EventHandler<EventArgs> handler = SplitterDragged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.GotFocus"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            SizeAll();
            Refresh();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (IsOverSplitX(e.X))
            {
                m_draggingY = true;
                DrawXSplitter();
            }
            if (IsOverSplitY(e.Y))
            {
                m_draggingX = true;
                DrawYSplitter();
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                if (m_draggingY)
                {
                    DrawXSplitter();
                    SetSplitX(e.X / (float)Width);
                    DrawXSplitter();
                }
                if (m_draggingX)
                {
                    DrawYSplitter();
                    SetSplitY(e.Y / (float)Height);
                    DrawYSplitter();
                }
            }

            bool overX = IsOverSplitX(e.X);
            bool overY = IsOverSplitY(e.Y);
            if (overX && overY)
                Cursor = s_xyCursor;
            else if (overX)
                Cursor = s_xCursor;
            else if (overY)
                Cursor = s_yCursor;

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (m_draggingX || m_draggingY)
            {
                m_draggingY = m_draggingX = false;

                OnSplitterDragged(EventArgs.Empty);

                SizeAll();
                Refresh();
            }

            base.OnMouseUp(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseLeave"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            Cursor = Cursors.Arrow;

            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Resize"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SizeAll();
            Refresh();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int x = GetSplitX();
            int y = GetSplitY();

            if (m_enableX)
            {
                ControlPaint.DrawBorder3D(
                    e.Graphics,
                    x - m_splitterThickness / 2,
                    -1,
                    m_splitterThickness,
                    Height + 2);
            }

            if (m_enableY)
            {
                ControlPaint.DrawBorder3D(
                    e.Graphics,
                    -1,
                    y - m_splitterThickness / 2,
                    Width + 2,
                    m_splitterThickness);
            }

            if (MultiPanelMode)
            {
                ControlPaint.DrawBorder3D(
                    e.Graphics,
                    x - m_splitterThickness / 2,
                    y - m_splitterThickness / 2,
                    m_splitterThickness,
                    m_splitterThickness,
                    Border3DStyle.Flat,
                    Border3DSide.Middle);

                Control activeControl = ActiveControl;
                if (activeControl != null)
                {
                    Rectangle rect = activeControl.Bounds;
                    rect.Inflate(2, 2);
                    ControlPaint.DrawBorder(
                        e.Graphics,
                        rect,
                        Color.Blue,
                        2,
                        ButtonBorderStyle.Solid,
                        Color.Blue,
                        2,
                        ButtonBorderStyle.Solid,
                        Color.Blue,
                        2,
                        ButtonBorderStyle.Solid,
                        Color.Blue,
                        2,
                        ButtonBorderStyle.Solid);
                }
            }
        }

        private bool MultiPanelMode
        {
            get { return m_enableX || m_enableY || m_splitX < 1.0f || m_splitY < 1.0f; }
        }

        private void ControlGotFocus(Object sender, EventArgs e)
        {
            m_activeControl = sender as Control;
            Refresh();
        }

        private void SizeAll()
        {
            SizeTopLeft();
            SizeTopRight();
            SizeBottomLeft();
            SizeBottomRight();
        }

        private void SizeTopLeft()
        {
            if (m_topLeft != null)
            {
                int x = (int)(m_splitX * Width) - m_splitterThickness / 2;
                int y = (int)(m_splitY * Height) - m_splitterThickness / 2;
                m_topLeft.Bounds = new Rectangle(1, 1, x - 2, y - 2);
            }
        }

        private void SizeTopRight()
        {
            if (m_topRight != null)
            {
                int x = (int)(m_splitX * Width) - m_splitterThickness / 2 + m_splitterThickness;
                int y = (int)(m_splitY * Height) - m_splitterThickness / 2;
                m_topRight.Bounds = new Rectangle(x, 1, Width - x - 1, y - 2);
            }
        }

        private void SizeBottomLeft()
        {
            if (m_bottomLeft != null)
            {
                int x = (int)(m_splitX * Width) - m_splitterThickness / 2;
                int y = (int)(m_splitY * Height) - m_splitterThickness / 2 + m_splitterThickness;
                m_bottomLeft.Bounds = new Rectangle(1, y, x - 2, Height - y - 1);
            }
        }

        private void SizeBottomRight()
        {
            if (m_bottomRight != null)
            {
                int x = (int)(m_splitX * Width) - m_splitterThickness / 2 + m_splitterThickness;
                int y = (int)(m_splitY * Height) - m_splitterThickness / 2 + m_splitterThickness;
                m_bottomRight.Bounds = new Rectangle(x, y, Width - x - 1, Height - y - 1);
            }
        }

        private void DrawXSplitter()
        {
            int x = GetSplitX();
            Point p1 = PointToScreen(new Point(x, 0));
            Point p2 = PointToScreen(new Point(x, Height));
            ControlPaint.DrawReversibleLine(p1, p2, BackColor);
        }

        private void DrawYSplitter()
        {
            int y = GetSplitY();
            Point p1 = PointToScreen(new Point(0, y));
            Point p2 = PointToScreen(new Point(Width, y));
            ControlPaint.DrawReversibleLine(p1, p2, BackColor);
        }

        private bool IsOverSplitX(int x)
        {
            if (!m_enableX)
                return false;
            return Math.Abs(GetSplitX() - x) < m_splitterThickness / 2 + m_tolerance;
        }

        private bool IsOverSplitY(int y)
        {
            if (!m_enableY)
                return false;
            return Math.Abs(GetSplitY() - y) < m_splitterThickness / 2 + m_tolerance;
        }

        private int GetSplitX()
        {
            return (int)(m_splitX * Width);
        }

        private int GetSplitY()
        {
            return (int)(m_splitY * Height);
        }

        private void SetSplitX(float x)
        {
            m_splitX = Math.Min(Math.Max(0, x), 1);
        }

        private void SetSplitY(float y)
        {
            m_splitY = Math.Min(Math.Max(0, y), 1);
        }

        private readonly object m_context;

        private Control m_topLeft;
        private Control m_topRight;
        private Control m_bottomLeft;
        private Control m_bottomRight;
        private Control m_activeControl;

        private float m_splitX = 0.5f;
        private float m_splitY = 0.5f;
        private bool m_draggingX;
        private bool m_draggingY;
        private int m_splitterThickness = 8;
        private int m_tolerance = 2;
        private bool m_enableX = true;
        private bool m_enableY = true;

        private static readonly Cursor s_xyCursor = ResourceUtil.GetCursor(Resources.FourWayCursor);
        private static readonly Cursor s_xCursor = ResourceUtil.GetCursor(Resources.VerticalSizeCursor);
        private static readonly Cursor s_yCursor = ResourceUtil.GetCursor(Resources.HorizSizeCursor);

        private const string SettingsElementName = "QuadPanelControlSettings";
        private const string SettingsSplitXAttributeName = "SplitX";
        private const string SettingsSplitYAttributeName = "SplitY";
        private const string SettingsEnableXAttributeName = "EnableX";
        private const string SettingsEnableYAttributeName = "EnableY";
    }
}
