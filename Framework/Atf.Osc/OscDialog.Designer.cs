namespace Sce.Atf.Applications
{
    partial class OscDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OscDialog));
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.m_listView = new System.Windows.Forms.ListView();
            this.oscAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.propertyName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.propertyType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.className = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.m_okButton = new System.Windows.Forms.Button();
            this.m_cancelButton = new System.Windows.Forms.Button();
            this.m_toClipboardButton = new System.Windows.Forms.Button();
            this.m_statusTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.m_destinationPortNumber = new System.Windows.Forms.TextBox();
            this.m_destinationIPAddress = new System.Windows.Forms.TextBox();
            this.m_noDestinationButton = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.m_hostName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.m_receivingPortNumber = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.m_receivingIPAddresses = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label3.Name = "label3";
            // 
            // m_listView
            // 
            resources.ApplyResources(this.m_listView, "m_listView");
            this.m_listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.oscAddress,
            this.propertyName,
            this.propertyType,
            this.className});
            this.m_listView.Name = "m_listView";
            this.m_listView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.m_listView.UseCompatibleStateImageBehavior = false;
            this.m_listView.View = System.Windows.Forms.View.Details;
            // 
            // oscAddress
            // 
            resources.ApplyResources(this.oscAddress, "oscAddress");
            // 
            // propertyName
            // 
            resources.ApplyResources(this.propertyName, "propertyName");
            // 
            // propertyType
            // 
            resources.ApplyResources(this.propertyType, "propertyType");
            // 
            // className
            // 
            resources.ApplyResources(this.className, "className");
            // 
            // m_okButton
            // 
            resources.ApplyResources(this.m_okButton, "m_okButton");
            this.m_okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_okButton.Name = "m_okButton";
            this.m_okButton.UseVisualStyleBackColor = true;
            this.m_okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // m_cancelButton
            // 
            resources.ApplyResources(this.m_cancelButton, "m_cancelButton");
            this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_cancelButton.Name = "m_cancelButton";
            this.m_cancelButton.UseVisualStyleBackColor = true;
            // 
            // m_toClipboardButton
            // 
            resources.ApplyResources(this.m_toClipboardButton, "m_toClipboardButton");
            this.m_toClipboardButton.Name = "m_toClipboardButton";
            this.m_toClipboardButton.UseVisualStyleBackColor = true;
            this.m_toClipboardButton.Click += new System.EventHandler(this.toClipboardButton_Click);
            // 
            // m_statusTextBox
            // 
            resources.ApplyResources(this.m_statusTextBox, "m_statusTextBox");
            this.m_statusTextBox.Name = "m_statusTextBox";
            this.m_statusTextBox.ReadOnly = true;
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.tableLayoutPanel2);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.label8, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label12, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.m_destinationPortNumber, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.m_destinationIPAddress, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.m_noDestinationButton, 1, 2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            // 
            // m_destinationPortNumber
            // 
            resources.ApplyResources(this.m_destinationPortNumber, "m_destinationPortNumber");
            this.m_destinationPortNumber.Name = "m_destinationPortNumber";
            // 
            // m_destinationIPAddress
            // 
            resources.ApplyResources(this.m_destinationIPAddress, "m_destinationIPAddress");
            this.m_destinationIPAddress.Name = "m_destinationIPAddress";
            // 
            // m_noDestinationButton
            // 
            resources.ApplyResources(this.m_noDestinationButton, "m_noDestinationButton");
            this.m_noDestinationButton.Name = "m_noDestinationButton";
            this.m_noDestinationButton.UseVisualStyleBackColor = true;
            this.m_noDestinationButton.Click += new System.EventHandler(this.disableButton_Click);
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.tableLayoutPanel1);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.m_hostName, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.m_receivingPortNumber, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.m_receivingIPAddresses, 1, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // m_hostName
            // 
            this.m_hostName.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.m_hostName, "m_hostName");
            this.m_hostName.Name = "m_hostName";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // m_receivingPortNumber
            // 
            resources.ApplyResources(this.m_receivingPortNumber, "m_receivingPortNumber");
            this.m_receivingPortNumber.Name = "m_receivingPortNumber";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // m_receivingIPAddresses
            // 
            this.m_receivingIPAddresses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_receivingIPAddresses.FormattingEnabled = true;
            resources.ApplyResources(this.m_receivingIPAddresses, "m_receivingIPAddresses");
            this.m_receivingIPAddresses.Name = "m_receivingIPAddresses";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // OscDialog
            // 
            this.AcceptButton = this.m_okButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_cancelButton;
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.m_statusTextBox);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.m_toClipboardButton);
            this.Controls.Add(this.m_cancelButton);
            this.Controls.Add(this.m_okButton);
            this.Controls.Add(this.m_listView);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Name = "OscDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView m_listView;
        private System.Windows.Forms.ColumnHeader oscAddress;
        private System.Windows.Forms.ColumnHeader className;
        private System.Windows.Forms.ColumnHeader propertyName;
        private System.Windows.Forms.ColumnHeader propertyType;
        private System.Windows.Forms.Button m_okButton;
        private System.Windows.Forms.Button m_cancelButton;
        private System.Windows.Forms.Button m_toClipboardButton;
        private System.Windows.Forms.TextBox m_statusTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TextBox m_destinationIPAddress;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox m_destinationPortNumber;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label m_hostName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox m_receivingPortNumber;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button m_noDestinationButton;
        private System.Windows.Forms.ComboBox m_receivingIPAddresses;
    }
}