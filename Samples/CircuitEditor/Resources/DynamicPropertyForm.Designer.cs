//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace CircuitEditorSample
{
    partial class DynamicPropertyForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DynamicPropertyForm));
            this.PropertyName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.VectorType = new System.Windows.Forms.RadioButton();
            this.BooleanType = new System.Windows.Forms.RadioButton();
            this.IntegerType = new System.Windows.Forms.RadioButton();
            this.FloatingPointType = new System.Windows.Forms.RadioButton();
            this.StringType = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.Description = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Category = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PropertyName
            // 
            resources.ApplyResources(this.PropertyName, "PropertyName");
            this.PropertyName.Name = "PropertyName";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.VectorType);
            this.panel1.Controls.Add(this.BooleanType);
            this.panel1.Controls.Add(this.IntegerType);
            this.panel1.Controls.Add(this.FloatingPointType);
            this.panel1.Controls.Add(this.StringType);
            this.panel1.Name = "panel1";
            // 
            // VectorType
            // 
            resources.ApplyResources(this.VectorType, "VectorType");
            this.VectorType.Name = "VectorType";
            this.VectorType.TabStop = true;
            this.VectorType.UseVisualStyleBackColor = true;
            // 
            // BooleanType
            // 
            resources.ApplyResources(this.BooleanType, "BooleanType");
            this.BooleanType.Name = "BooleanType";
            this.BooleanType.TabStop = true;
            this.BooleanType.UseVisualStyleBackColor = true;
            // 
            // IntegerType
            // 
            resources.ApplyResources(this.IntegerType, "IntegerType");
            this.IntegerType.Checked = true;
            this.IntegerType.Name = "IntegerType";
            this.IntegerType.TabStop = true;
            this.IntegerType.UseVisualStyleBackColor = true;
            // 
            // FloatingPointType
            // 
            resources.ApplyResources(this.FloatingPointType, "FloatingPointType");
            this.FloatingPointType.Name = "FloatingPointType";
            this.FloatingPointType.TabStop = true;
            this.FloatingPointType.UseVisualStyleBackColor = true;
            // 
            // StringType
            // 
            resources.ApplyResources(this.StringType, "StringType");
            this.StringType.Name = "StringType";
            this.StringType.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // Cancel
            // 
            resources.ApplyResources(this.Cancel, "Cancel");
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Name = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            resources.ApplyResources(this.Ok, "Ok");
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Name = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // Description
            // 
            resources.ApplyResources(this.Description, "Description");
            this.Description.Name = "Description";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // Category
            // 
            resources.ApplyResources(this.Category, "Category");
            this.Category.Name = "Category";
            // 
            // DynamicPropertyForm
            // 
            this.AcceptButton = this.Ok;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Category);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Description);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PropertyName);
            this.Name = "DynamicPropertyForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        public System.Windows.Forms.TextBox PropertyName;
        public System.Windows.Forms.RadioButton FloatingPointType;
        public System.Windows.Forms.RadioButton StringType;
        public System.Windows.Forms.RadioButton BooleanType;
        public System.Windows.Forms.RadioButton IntegerType;
        public System.Windows.Forms.RadioButton VectorType;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox Description;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox Category;
    }
}