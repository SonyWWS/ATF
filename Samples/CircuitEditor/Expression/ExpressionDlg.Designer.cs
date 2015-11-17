namespace CircuitEditorSample
{
    partial class ExpressionDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.m_tabControl = new System.Windows.Forms.TabControl();
            this.m_objectPage = new System.Windows.Forms.TabPage();
            this.m_copyBtn = new System.Windows.Forms.Button();
            this.m_objectAttribute = new System.Windows.Forms.Label();
            this.m_propList = new System.Windows.Forms.ListBox();
            this.m_objectList = new System.Windows.Forms.ListBox();
            this.m_exprPage = new System.Windows.Forms.TabPage();
            this.m_exprList = new System.Windows.Forms.ListBox();
            this.m_exprTxt = new System.Windows.Forms.TextBox();
            this.m_applyBtn = new System.Windows.Forms.Button();
            this.m_cancelBtn = new System.Windows.Forms.Button();
            this.m_okBtn = new System.Windows.Forms.Button();
            this.m_exprId = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.m_exprLabelTxt = new System.Windows.Forms.TextBox();
            this.m_exprIdTxt = new System.Windows.Forms.TextBox();
            this.m_tabControl.SuspendLayout();
            this.m_objectPage.SuspendLayout();
            this.m_exprPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_tabControl
            // 
            this.m_tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_tabControl.Controls.Add(this.m_objectPage);
            this.m_tabControl.Controls.Add(this.m_exprPage);
            this.m_tabControl.Location = new System.Drawing.Point(12, 12);
            this.m_tabControl.Name = "m_tabControl";
            this.m_tabControl.SelectedIndex = 0;
            this.m_tabControl.Size = new System.Drawing.Size(621, 237);
            this.m_tabControl.TabIndex = 0;
            // 
            // m_objectPage
            // 
            this.m_objectPage.Controls.Add(this.m_copyBtn);
            this.m_objectPage.Controls.Add(this.m_objectAttribute);
            this.m_objectPage.Controls.Add(this.m_propList);
            this.m_objectPage.Controls.Add(this.m_objectList);
            this.m_objectPage.Location = new System.Drawing.Point(4, 22);
            this.m_objectPage.Name = "m_objectPage";
            this.m_objectPage.Padding = new System.Windows.Forms.Padding(3);
            this.m_objectPage.Size = new System.Drawing.Size(613, 211);
            this.m_objectPage.TabIndex = 0;
            this.m_objectPage.Text = "Objects";
            this.m_objectPage.UseVisualStyleBackColor = true;
            // 
            // m_copyBtn
            // 
            this.m_copyBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_copyBtn.Location = new System.Drawing.Point(6, 184);
            this.m_copyBtn.Name = "m_copyBtn";
            this.m_copyBtn.Size = new System.Drawing.Size(75, 23);
            this.m_copyBtn.TabIndex = 3;
            this.m_copyBtn.Text = "Copy";
            this.m_copyBtn.UseVisualStyleBackColor = true;
            // 
            // m_objectAttribute
            // 
            this.m_objectAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_objectAttribute.Location = new System.Drawing.Point(86, 182);
            this.m_objectAttribute.Name = "m_objectAttribute";
            this.m_objectAttribute.Size = new System.Drawing.Size(521, 23);
            this.m_objectAttribute.TabIndex = 2;
            this.m_objectAttribute.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // m_propList
            // 
            this.m_propList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_propList.FormattingEnabled = true;
            this.m_propList.Location = new System.Drawing.Point(342, 6);
            this.m_propList.Name = "m_propList";
            this.m_propList.Size = new System.Drawing.Size(265, 173);
            this.m_propList.TabIndex = 1;
            // 
            // m_objectList
            // 
            this.m_objectList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_objectList.FormattingEnabled = true;
            this.m_objectList.Location = new System.Drawing.Point(6, 6);
            this.m_objectList.Name = "m_objectList";
            this.m_objectList.Size = new System.Drawing.Size(330, 173);
            this.m_objectList.TabIndex = 0;
            // 
            // m_exprPage
            // 
            this.m_exprPage.Controls.Add(this.m_exprList);
            this.m_exprPage.Location = new System.Drawing.Point(4, 22);
            this.m_exprPage.Name = "m_exprPage";
            this.m_exprPage.Padding = new System.Windows.Forms.Padding(3);
            this.m_exprPage.Size = new System.Drawing.Size(613, 211);
            this.m_exprPage.TabIndex = 1;
            this.m_exprPage.Text = "Expressions";
            this.m_exprPage.UseVisualStyleBackColor = true;
            // 
            // m_exprList
            // 
            this.m_exprList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_exprList.FormattingEnabled = true;
            this.m_exprList.Location = new System.Drawing.Point(6, 6);
            this.m_exprList.Name = "m_exprList";
            this.m_exprList.Size = new System.Drawing.Size(601, 199);
            this.m_exprList.TabIndex = 0;
            // 
            // m_exprTxt
            // 
            this.m_exprTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_exprTxt.Location = new System.Drawing.Point(16, 310);
            this.m_exprTxt.Multiline = true;
            this.m_exprTxt.Name = "m_exprTxt";
            this.m_exprTxt.Size = new System.Drawing.Size(613, 125);
            this.m_exprTxt.TabIndex = 1;
            // 
            // m_applyBtn
            // 
            this.m_applyBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_applyBtn.Location = new System.Drawing.Point(554, 447);
            this.m_applyBtn.Name = "m_applyBtn";
            this.m_applyBtn.Size = new System.Drawing.Size(75, 23);
            this.m_applyBtn.TabIndex = 2;
            this.m_applyBtn.Text = "Apply";
            this.m_applyBtn.UseVisualStyleBackColor = true;
            // 
            // m_cancelBtn
            // 
            this.m_cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_cancelBtn.Location = new System.Drawing.Point(473, 447);
            this.m_cancelBtn.Name = "m_cancelBtn";
            this.m_cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.m_cancelBtn.TabIndex = 3;
            this.m_cancelBtn.Text = "Cancel";
            this.m_cancelBtn.UseVisualStyleBackColor = true;
            // 
            // m_okBtn
            // 
            this.m_okBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_okBtn.Location = new System.Drawing.Point(392, 447);
            this.m_okBtn.Name = "m_okBtn";
            this.m_okBtn.Size = new System.Drawing.Size(75, 23);
            this.m_okBtn.TabIndex = 4;
            this.m_okBtn.Text = "OK";
            this.m_okBtn.UseVisualStyleBackColor = true;
            // 
            // m_exprId
            // 
            this.m_exprId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_exprId.Location = new System.Drawing.Point(13, 252);
            this.m_exprId.Name = "m_exprId";
            this.m_exprId.Size = new System.Drawing.Size(94, 22);
            this.m_exprId.TabIndex = 5;
            this.m_exprId.Text = "Expression Label:";
            this.m_exprId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Location = new System.Drawing.Point(13, 276);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 22);
            this.label1.TabIndex = 6;
            this.label1.Text = "Expression  Id: ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // m_exprLabelTxt
            // 
            this.m_exprLabelTxt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_exprLabelTxt.Location = new System.Drawing.Point(112, 254);
            this.m_exprLabelTxt.Name = "m_exprLabelTxt";
            this.m_exprLabelTxt.Size = new System.Drawing.Size(240, 20);
            this.m_exprLabelTxt.TabIndex = 7;
            // 
            // m_exprIdTxt
            // 
            this.m_exprIdTxt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_exprIdTxt.Location = new System.Drawing.Point(112, 278);
            this.m_exprIdTxt.Name = "m_exprIdTxt";
            this.m_exprIdTxt.ReadOnly = true;
            this.m_exprIdTxt.Size = new System.Drawing.Size(240, 20);
            this.m_exprIdTxt.TabIndex = 8;
            // 
            // ExpressionDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(645, 482);
            this.Controls.Add(this.m_exprIdTxt);
            this.Controls.Add(this.m_exprLabelTxt);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_exprId);
            this.Controls.Add(this.m_okBtn);
            this.Controls.Add(this.m_cancelBtn);
            this.Controls.Add(this.m_applyBtn);
            this.Controls.Add(this.m_exprTxt);
            this.Controls.Add(this.m_tabControl);
            this.Name = "ExpressionDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Expression Editor";
            this.m_tabControl.ResumeLayout(false);
            this.m_objectPage.ResumeLayout(false);
            this.m_exprPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl m_tabControl;
        private System.Windows.Forms.TabPage m_objectPage;
        private System.Windows.Forms.TabPage m_exprPage;
        private System.Windows.Forms.TextBox m_exprTxt;
        private System.Windows.Forms.Button m_applyBtn;
        private System.Windows.Forms.Button m_cancelBtn;
        private System.Windows.Forms.Button m_okBtn;
        private System.Windows.Forms.ListBox m_propList;
        private System.Windows.Forms.ListBox m_objectList;
        private System.Windows.Forms.Button m_copyBtn;
        private System.Windows.Forms.Label m_objectAttribute;
        private System.Windows.Forms.ListBox m_exprList;
        private System.Windows.Forms.Label m_exprId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox m_exprLabelTxt;
        private System.Windows.Forms.TextBox m_exprIdTxt;
    }
}