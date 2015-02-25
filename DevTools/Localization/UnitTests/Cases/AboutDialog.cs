//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// A "Help About..." dialog, with Sony Logo and assembly list</summary>
    public class AboutDialog : System.Windows.Forms.Form
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="title">Optional dialog title. If null, the title is "About"</param>
        /// <param name="url">Optional URL of tool's home page. If null, no URL Control is shown</param>
        /// <param name="clientControl">Optional Control describing project</param>
        public AboutDialog(
            string title,
            string url,
            Control clientControl)
            : this(title, url, clientControl, null, null, false)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="title">Optional dialog title. If null, the title is "About"</param>
        /// <param name="url">Optional URL of tool's home page. If null, no URL Control is shown</param>
        /// <param name="clientControl">Optional Control describing project</param>
        /// <param name="logo">Custom logo image. If null, the SCEA logo is used.</param>
        /// <remarks>Image size should be 200 x 200</remarks>
        /// <param name="credits">Optional application credits list. Can be null.</param>
        public AboutDialog(
            string title,
            string url,
            Control clientControl,
            Image logo,
            IList<string> credits)
            : this(title, url, clientControl, logo, credits, false)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="title">Optional dialog title. If null, the title is "About"</param>
        /// <param name="url">Optional URL of tool's home page. If null, no URL Control is shown</param>
        /// <param name="clientControl">Optional Control to be added. If this is a RichTextBox, URLs
        /// will launch the user's browser if the user clicks on them.</param>
        /// <param name="logo">Custom logo image. If null, the SCEA logo is used.</param>
        /// <remarks>Image size should be 200 x 200</remarks>
        /// <param name="credits">Optional application credits list. Can be null.</param>
        /// <param name="addAtfInfo">Whether ATF version and credits should be added to end of credits</param>
        public AboutDialog(
            string title,
            string url,
            Control clientControl,
            Image logo,
            IList<string> credits,
            bool addAtfInfo)
        {
            // Required for Windows Form Designer support
            InitializeComponent();

            if (title == null)
                title = "About".Localize();
            Text = title;

            if (logo == null)
                logo = Sce.Atf.ResourceUtil.GetImage(Resources.AtfLogoImage);

            if (credits == null)
                credits = new List<string>();

            if (addAtfInfo)
            {
                Version v = AtfVersion.GetVersion();
                credits.Add(string.Format("Authoring Tools Framework (ATF {0}), by Ron Little, Jianhua Shen," +
                    " Julianne Harrington, Alan Beckus, Matt Mahony, Pat O'Leary, Paul Skibitzke, and Max Elliott." +
                    " Copyright © 2014 Sony Computer Entertainment America LLC".Localize("{0} is the version number"), v));

                // Test a commented-out version of Localize().
                //credits.Add(string.Format("Authoring Tools Framework (ATF {0}), by Ron Little, Jianhua Shen," +
                //    " Julianne Harrington, Alan Beckus, Matt Mahony, Pat O'Leary, Paul Skibitzke, and Max Elliott." +
                //    " Copyright © 2014 Sony Computer Entertainment America LLC".Localize("{0} is the version number"), v));
            }

            pictureBox.Size = logo.Size;
            pictureBox.Image = logo;

            if (clientControl != null)
            {
                clientControl.Dock = DockStyle.Fill;
                clientPanel.Controls.Add(clientControl);

                m_richTextBox = clientControl as RichTextBox;
                if (m_richTextBox != null)
                    m_richTextBox.LinkClicked += RichTextBoxOnLinkClicked;
            }

            // link to project home page
            if (url != null)
            {
                // To-do: this seems like a useful utility method that surely must exist somewhere,
                //  to return how many characters can fit in a given amount of space for a particular font.
                //float fontSize = linkLabel.Font.SizeInPoints;
                //int numVisibleChars = (int)((linkLabel.ClientRectangle.Width / fontSize) * GdiUtil.DpiFactor * (96.0f / 72.0f));

                m_url = url;
                //int numCharsForUrl = numVisibleChars - linkLabel.Text.Length;
                //url = PathUtil.GetCompactedPath(url, numCharsForUrl);//to-do: this throws a System.ExecutionEngineException!
                linkLabel.Links.Add(linkLabel.Text.Length, url.Length, url);
                linkLabel.Text += url;
                linkLabel.MouseDown += linkLabel_MouseDown;
                linkLabel.Visible = true;
            }
            else
            {
                linkLabel.Visible = false;
            }

            if (credits.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string credit in credits)
                {
                    sb.Append(credit);
                    sb.Append(Environment.NewLine);
                }
                creditsTextBox.Text = sb.ToString();
            }
            else
            {
                Controls.Remove(creditsTextBox);
            }
        }

        private void RichTextBoxOnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void linkLabel_MouseDown(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Process.Start(m_url);
            linkLabel.LinkVisited = true;
        }

        /// <summary>
        /// Cleans up any resources being used</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; 
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
                if (m_richTextBox != null)
                {
                    m_richTextBox.LinkClicked -= RichTextBoxOnLinkClicked;
                    m_richTextBox = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor</summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutDialog));
            this.linkLabel = new System.Windows.Forms.LinkLabel();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.clientPanel = new System.Windows.Forms.Panel();
            this.creditsTextBox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.sysInfoButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // linkLabel
            // 
            resources.ApplyResources(this.linkLabel, "linkLabel");
            this.linkLabel.Name = "linkLabel";
            this.linkLabel.TabStop = true;
            // 
            // pictureBox
            // 
            resources.ApplyResources(this.pictureBox, "pictureBox");
            this.pictureBox.BackColor = System.Drawing.SystemColors.Window;
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.TabStop = false;
            // 
            // clientPanel
            // 
            resources.ApplyResources(this.clientPanel, "clientPanel");
            this.clientPanel.Name = "clientPanel";
            // 
            // creditsTextBox
            // 
            resources.ApplyResources(this.creditsTextBox, "creditsTextBox");
            this.creditsTextBox.Name = "creditsTextBox";
            this.creditsTextBox.ReadOnly = true;
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // sysInfoButton
            // 
            resources.ApplyResources(this.sysInfoButton, "sysInfoButton");
            this.sysInfoButton.Name = "sysInfoButton";
            this.sysInfoButton.UseVisualStyleBackColor = true;
            this.sysInfoButton.Click += new System.EventHandler(this.sysInfoButton_Click);
            // 
            // AboutDialog
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.sysInfoButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.creditsTextBox);
            this.Controls.Add(this.clientPanel);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.linkLabel);
            this.Name = "AboutDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.AboutDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
            if (s_sysInfoDialog != null)
                s_sysInfoDialog.Close();
        }

        private void sysInfoButton_Click(object sender, EventArgs e)
        {
            if (s_sysInfoDialog == null)
            {
                s_sysInfoDialog = new AboutSysInfoDialog();
                s_sysInfoDialog.FormClosed += sysInfoDialog_FormClosed;
            }
            s_sysInfoDialog.BringToFront();
            s_sysInfoDialog.Show();
        }

        private static void sysInfoDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            s_sysInfoDialog = null;
        }
        
        private void AboutDialog_Load(object sender, EventArgs e)
        {

        }
        
        private LinkLabel linkLabel;
        private PictureBox pictureBox;
        private Panel clientPanel;
        private TextBox creditsTextBox;
        private Button okButton;
        private Button sysInfoButton;
        private RichTextBox m_richTextBox;
        // Required designer variable.
        private readonly System.ComponentModel.Container components = null;
        private readonly string m_url;
        private static AboutSysInfoDialog s_sysInfoDialog;
    }
}
