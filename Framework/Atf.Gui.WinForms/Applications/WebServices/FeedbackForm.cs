//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Reflection;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Sce.Atf.Applications.WebServices
{
    /// <summary>
    /// Form for submitting bugs to the SourceForge bug tracker</summary>
    /// <remarks>
    /// Note that each project has a unique identifier used to
    /// map to the SourceForge project (for example, "com.scea.screamtool").
    /// This identifier can be specified with the ProjectMappingAttribute.
    /// For more info, see http://wiki.ship.scea.com/confluence/display/SUPPORT/Bug+Submit+and+Version+Check+services </remarks>
    public class FeedbackForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TextBox m_userTextBox;
        private System.Windows.Forms.Label m_descLabel;
        private System.Windows.Forms.RichTextBox m_descTextBox;
        private System.Windows.Forms.TextBox m_titleTextBox;
        private System.Windows.Forms.Label m_titleLabel;
        private System.Windows.Forms.Label m_passwordLabel;
        private System.Windows.Forms.Label m_userLabel;
        private System.Windows.Forms.TextBox m_passwordTextBox;
        private System.Windows.Forms.Button m_submitBtn;
        private System.Windows.Forms.Button m_cancelBtn;
        private System.Windows.Forms.StatusBar m_statusBar;
        private Label m_lblEmail;
        private TextBox m_txtEmail;
        private Label m_lblPriority;
        private ComboBox m_cmbPriority;
        /// <summary> 
        /// Required designer variable</summary>
        private readonly System.ComponentModel.Container components = null;

        ////////////////////////////////////////

        /// <summary>
        /// Constructor.
        /// Mapping to SourceForge project is determined from the assembly attributes.</summary>
        /// <param name="anon"><c>True</c> if anonymous login</param>
        public FeedbackForm(bool anon)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            // use ProjectMappingAttribute for mapping
            ProjectMappingAttribute mapAttr = (ProjectMappingAttribute)Attribute.GetCustomAttribute(assembly, typeof(ProjectMappingAttribute));
            if (mapAttr != null && mapAttr.Mapping != null && mapAttr.Mapping.Trim().Length != 0)
                m_mappingName = mapAttr.Mapping.Trim();
            else
                m_mappingName = null;

            InitializeComponent();

            // set the priority to low
            m_cmbPriority.SelectedIndex = 2;

            m_anonymous = anon;
            if (anon)
            {
                m_passwordLabel.Visible = false;
                m_passwordTextBox.Visible = false;
                m_userLabel.Visible = false;
                m_userTextBox.Visible = false;
            }
            else
            {
                m_txtEmail.Visible = false;
                m_lblEmail.Visible = false;
            }
        }

        

        /// <summary>
        /// Gets or sets the exception that caused this bug report. The stack trace is appended
        /// to the description entered by the user.</summary>
        public Exception Exception
        {
            set { m_exception = value; }
            get { return m_exception; }
        }

        /// <summary>
        /// Gets or sets the default title for the bug report</summary>
        public string Title
        {
            set { m_titleTextBox.Text = value; }
            get { return m_titleTextBox.Text; }
        }

        ////////////////////////////////////////
        private bool ValidateData()
        {
            m_lblEmail.ForeColor = Color.Black;
            m_passwordLabel.ForeColor = Color.Black;
            m_userLabel.ForeColor = Color.Black;
            m_titleLabel.ForeColor = Color.Black;

            if (m_mappingName == null)
            {
                MessageBox.Show(this, "Assembly mapping attribute not found.\nCannot proceed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            
            if (m_titleTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show(this, "Please fill in title", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                m_titleLabel.ForeColor = Color.Red;
                m_titleTextBox.Focus();
                return false;
            }

            // if in Anonymouse mode, check for valid email address
            if (m_anonymous)
            {
                // validate email address
                if (!Regex.IsMatch(m_txtEmail.Text.Trim(), @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"))
                {
                    MessageBox.Show(this, "Invalid email address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    m_lblEmail.ForeColor = Color.Red;
                    m_txtEmail.Focus();
                    return false;
                }
            }
            else
            {// check user name and password

                if (m_userTextBox.Text.Trim().Length == 0)
                {
                    MessageBox.Show(this, "User name required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    m_userLabel.ForeColor = Color.Red;
                    m_userTextBox.Focus();
                    return false;
                }

                if(m_passwordTextBox.Text.Trim().Length == 0)
                {
                    MessageBox.Show(this, "Password required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    m_passwordLabel.ForeColor = Color.Red;
                    m_passwordTextBox.Focus();
                    return false;
                }
            }
            return true;
        }

        private void DoSubmit()
        {
            
            try 
            {

                using (new WaitCursor())
                {


                    if (!ValidateData())
                        return;

                    m_statusBar.Text = "Submitting bug...";

                    WebPermission myWebPermission = new WebPermission(PermissionState.Unrestricted);
                    myWebPermission.Demand();

                    string desc = m_descTextBox.Text;

                    // if the exception was set, append the stack trace
                    if (m_exception != null)
                    {
                        desc += "\n\n-----------------------------------\n";
                        desc += "Exception:\n";
                        desc += m_exception.ToString();
                    }

                    // append versions of all modules
                    desc += "\n\n-----------------------------------\n";
                    desc += "Modules:\n";
                    foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
                    {
                        // Get version info
                        FileVersionInfo fileVersionInfo = module.FileVersionInfo;
                        if (fileVersionInfo.FileVersion == "")
                            continue;

                        desc += String.Format("{0} {1}\n", 
                                module.ModuleName, fileVersionInfo.FileVersion);
                    }
                    if (m_anonymous)
                    {
                        string dsc = "Reported by: " + m_txtEmail.Text + "\n" + desc;
                        m_bugService.submitBug(m_mappingName, m_titleTextBox.Text, dsc, m_cmbPriority.SelectedIndex);
                    }
                    else
                    {
                        m_bugService.submitBug(m_mappingName, m_userTextBox.Text, m_passwordTextBox.Text, m_titleTextBox.Text, desc, m_cmbPriority.SelectedIndex);
                    }
                    Close();
                }
            } 
            catch(Exception ex)
            {
                m_statusBar.Text = "Bug not submitted.";
                MessageBox.Show("There were errors while submitting this bug\n" + ex.Message,  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Cleans up any resources being used</summary>
        /// <param name="disposing"><c>True</c> if managed resources should be disposed</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }


        #region private members

        private readonly string m_mappingName; // the name used in the SourceForge mapping
        private Exception m_exception; // exception that triggered this bug report
        private readonly com.scea.ship.submitBug.BugReportingService m_bugService = new com.scea.ship.submitBug.BugReportingService();
        private readonly bool m_anonymous;
      

        #endregion


        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor</summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FeedbackForm));
            this.m_userTextBox = new System.Windows.Forms.TextBox();
            this.m_descLabel = new System.Windows.Forms.Label();
            this.m_descTextBox = new System.Windows.Forms.RichTextBox();
            this.m_titleTextBox = new System.Windows.Forms.TextBox();
            this.m_titleLabel = new System.Windows.Forms.Label();
            this.m_passwordLabel = new System.Windows.Forms.Label();
            this.m_userLabel = new System.Windows.Forms.Label();
            this.m_passwordTextBox = new System.Windows.Forms.TextBox();
            this.m_submitBtn = new System.Windows.Forms.Button();
            this.m_cancelBtn = new System.Windows.Forms.Button();
            this.m_statusBar = new System.Windows.Forms.StatusBar();
            this.m_lblEmail = new System.Windows.Forms.Label();
            this.m_txtEmail = new System.Windows.Forms.TextBox();
            this.m_lblPriority = new System.Windows.Forms.Label();
            this.m_cmbPriority = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // m_userTextBox
            // 
            resources.ApplyResources(this.m_userTextBox, "m_userTextBox");
            this.m_userTextBox.Name = "m_userTextBox";
            // 
            // m_descLabel
            // 
            resources.ApplyResources(this.m_descLabel, "m_descLabel");
            this.m_descLabel.Name = "m_descLabel";
            // 
            // m_descTextBox
            // 
            resources.ApplyResources(this.m_descTextBox, "m_descTextBox");
            this.m_descTextBox.Name = "m_descTextBox";
            // 
            // m_titleTextBox
            // 
            resources.ApplyResources(this.m_titleTextBox, "m_titleTextBox");
            this.m_titleTextBox.Name = "m_titleTextBox";
            // 
            // m_titleLabel
            // 
            resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
            this.m_titleLabel.Name = "m_titleLabel";
            // 
            // m_passwordLabel
            // 
            resources.ApplyResources(this.m_passwordLabel, "m_passwordLabel");
            this.m_passwordLabel.Name = "m_passwordLabel";
            // 
            // m_userLabel
            // 
            resources.ApplyResources(this.m_userLabel, "m_userLabel");
            this.m_userLabel.Name = "m_userLabel";
            // 
            // m_passwordTextBox
            // 
            resources.ApplyResources(this.m_passwordTextBox, "m_passwordTextBox");
            this.m_passwordTextBox.Name = "m_passwordTextBox";
            // 
            // m_submitBtn
            // 
            resources.ApplyResources(this.m_submitBtn, "m_submitBtn");
            this.m_submitBtn.BackColor = System.Drawing.SystemColors.Control;
            this.m_submitBtn.Name = "m_submitBtn";
            this.m_submitBtn.UseVisualStyleBackColor = false;
            this.m_submitBtn.Click += new System.EventHandler(this.m_submitBtn_Click);
            // 
            // m_cancelBtn
            // 
            resources.ApplyResources(this.m_cancelBtn, "m_cancelBtn");
            this.m_cancelBtn.BackColor = System.Drawing.SystemColors.Control;
            this.m_cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_cancelBtn.Name = "m_cancelBtn";
            this.m_cancelBtn.UseVisualStyleBackColor = false;
            this.m_cancelBtn.Click += new System.EventHandler(this.m_cancelBtn_Click);
            // 
            // m_statusBar
            // 
            resources.ApplyResources(this.m_statusBar, "m_statusBar");
            this.m_statusBar.Name = "m_statusBar";
            // 
            // m_lblEmail
            // 
            resources.ApplyResources(this.m_lblEmail, "m_lblEmail");
            this.m_lblEmail.Name = "m_lblEmail";
            // 
            // m_txtEmail
            // 
            resources.ApplyResources(this.m_txtEmail, "m_txtEmail");
            this.m_txtEmail.Name = "m_txtEmail";
            // 
            // m_lblPriority
            // 
            resources.ApplyResources(this.m_lblPriority, "m_lblPriority");
            this.m_lblPriority.Name = "m_lblPriority";
            // 
            // m_cmbPriority
            // 
            resources.ApplyResources(this.m_cmbPriority, "m_cmbPriority");
            this.m_cmbPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbPriority.Items.AddRange(new object[] {
            resources.GetString("m_cmbPriority.Items"),
            resources.GetString("m_cmbPriority.Items1"),
            resources.GetString("m_cmbPriority.Items2"),
            resources.GetString("m_cmbPriority.Items3"),
            resources.GetString("m_cmbPriority.Items4"),
            resources.GetString("m_cmbPriority.Items5")});
            this.m_cmbPriority.Name = "m_cmbPriority";
            // 
            // FeedbackForm
            // 
            this.AcceptButton = this.m_submitBtn;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.m_cancelBtn;
            this.Controls.Add(this.m_cmbPriority);
            this.Controls.Add(this.m_lblPriority);
            this.Controls.Add(this.m_txtEmail);
            this.Controls.Add(this.m_lblEmail);
            this.Controls.Add(this.m_statusBar);
            this.Controls.Add(this.m_cancelBtn);
            this.Controls.Add(this.m_submitBtn);
            this.Controls.Add(this.m_titleTextBox);
            this.Controls.Add(this.m_passwordTextBox);
            this.Controls.Add(this.m_userTextBox);
            this.Controls.Add(this.m_titleLabel);
            this.Controls.Add(this.m_passwordLabel);
            this.Controls.Add(this.m_userLabel);
            this.Controls.Add(this.m_descLabel);
            this.Controls.Add(this.m_descTextBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FeedbackForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void m_cancelBtn_Click(object sender, System.EventArgs e)
        {

            Close();
        }

        private void m_submitBtn_Click(object sender, System.EventArgs e)
        {
            DoSubmit();
        }

        
    }
}
