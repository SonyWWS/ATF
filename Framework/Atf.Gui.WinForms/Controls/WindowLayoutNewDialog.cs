//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Atf.Applications;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// New window layout dialog</summary>
    public partial class WindowLayoutNewDialog : Form
    {
        /// <summary>
        /// Constructor</summary>
        public WindowLayoutNewDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the user entered layout name</summary>
        public string LayoutName
        {
            get { return m_txtLayout.Text.Trim(); }
            set { m_txtLayout.Text = value; }
        }

        /// <summary>
        /// Sets the internal list of existing layout names</summary>
        /// <remarks>If this value is set, the user-entered layout
        /// name cannot match any items in this list</remarks>
        public IEnumerable<string> ExistingLayoutNames
        {
            set
            {
                m_existingItems.Clear();
                m_existingItems.AddRange(value);
            }
        }

        private void BtnOkClick(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.None;

            string layoutName = LayoutName;
            if (string.IsNullOrEmpty(layoutName))
            {
                m_toolTip.Show(m_txtLayout, null, ToolTipIcon.Warning, "Layout name cannot be empty!");
                return;
            }

            if (!WindowLayoutService.IsValidLayoutName(layoutName))
            {
                m_toolTip.Show(m_txtLayout, null, ToolTipIcon.Warning, "Invalid layout name!");
                return;
            }

            if (m_existingItems.Contains(layoutName))
            {
                m_toolTip.Show(m_txtLayout, null, ToolTipIcon.Warning, "A layout with this name already exists!");
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private readonly List<string> m_existingItems =
            new List<string>();

        private readonly BalloonToolTipHelper m_toolTip =
            new BalloonToolTipHelper();
    }

    /// <summary>
    /// Helper class for displaying balloon tool tips on controls</summary>
    /// <remarks>Before making this public, consider removing 'args' parameter and 'title' parameter
    /// and renaming 'format' to be 'message', in the constructors below.</remarks>
    internal class BalloonToolTipHelper
    {
        /// <summary>
        /// Constructor</summary>
        public BalloonToolTipHelper()
        {
            m_toolTip =
                new ToolTip
                    {
                        IsBalloon = true,
                        ReshowDelay = 9999,
                        ShowAlways = true,
                        UseFading = true
                    };
        }

        /// <summary>
        /// Shows a balloon tool tip on a control</summary>
        /// <param name="control">Control</param>
        /// <param name="title">Tool tip title, or null</param>
        /// <param name="icon">Tool tip icon</param>
        /// <param name="format">Message to the user, with optional formatting arguments in 'args'</param>
        /// <param name="args">Optional arguments for 'format'</param>
        public void Show(Control control, string title, ToolTipIcon icon, string format, params object[] args)
        {
            Show(control, title, icon, DefaultToolTipTimeoutMsec, format, args);
        }

        /// <summary>
        /// Shows a balloon tool tip on a control</summary>
        /// <param name="control">Control</param>
        /// <param name="point">Offset to upper left corner of tool tip window, in pixels, relative to Control's position.</param>
        /// <param name="title">Tool tip title, or null</param>
        /// <param name="icon">Tool tip icon</param>
        /// <param name="format">Message to the user, with optional formatting arguments in 'args'</param>
        /// <param name="args">Optional arguments for 'format'</param>
        public void Show(Control control, System.Drawing.Point point, string title, ToolTipIcon icon, string format, params object[] args)
        {
            Show(control, point, title, icon, DefaultToolTipTimeoutMsec, format, args);
        }

        /// <summary>
        /// Shows a balloon tool tip on a control</summary>
        /// <param name="control">Control</param>
        /// <param name="title">Tool tip title, or null</param>
        /// <param name="icon">Tool tip icon</param>
        /// <param name="timeoutMsec">Time in milliseconds to show tool tip</param>
        /// <param name="format">Message to the user, with optional formatting arguments in 'args'</param>
        /// <param name="args">Optional arguments for 'format'</param>
        public void Show(Control control, string title, ToolTipIcon icon, int timeoutMsec, string format, params object[] args)
        {
            Show(control, new System.Drawing.Point(), title, icon, timeoutMsec, format, args);
        }

        /// <summary>
        /// Shows a balloon tool tip on a control</summary>
        /// <param name="control">Control</param>
        /// <param name="point">Offset to upper left corner of tool tip window, in pixels, relative to Control's position</param>
        /// <param name="title">Tool tip title, or null</param>
        /// <param name="icon">Tool tip icon</param>
        /// <param name="timeoutMsec">Time in milliseconds to show tool tip</param>
        /// <param name="format">Message to the user, with optional formatting arguments in 'args'</param>
        /// <param name="args">Optional arguments for 'format'</param>
        public void Show(Control control, System.Drawing.Point point, string title, ToolTipIcon icon, int timeoutMsec, string format, params object[] args)
        {
            if (m_control != null)
            {
                m_toolTip.Hide(m_control);
                m_toolTip.SetToolTip(m_control, string.Empty);
            }

            m_control = control;

            m_toolTip.ToolTipIcon = icon;

            if (!string.IsNullOrEmpty(title))
                m_toolTip.ToolTipTitle = title;

            {
                // This is a hack to get the ToolTip to show up at the
                // right position when using "IsBalloon = true"
                m_toolTip.Show(string.Empty, control, 0);
            }

            var text = string.Format(format, args);

            if (point.IsEmpty)
                m_toolTip.Show(text, control, timeoutMsec);
            else
                m_toolTip.Show(text, control, point, timeoutMsec);
        }

        private Control m_control;
        private readonly ToolTip m_toolTip;

        /// <summary>
        /// Default tool tip time displayed in milliseconds</summary>
        public const int DefaultToolTipTimeoutMsec = 2000;
    }
}
