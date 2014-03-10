namespace Sce.Atf.Applications.Controls
{
    /// <summary>
    /// Control that displays the rendering performance of a Control</summary>
    partial class PerformanceMonitorControl
    {
        /// <summary> 
        /// Required designer variable</summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor</summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PerformanceMonitorControl));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.FpsLabel = new System.Windows.Forms.Label();
            this.ManagedMemoryLabel = new System.Windows.Forms.Label();
            this.MaxFpsLabel = new System.Windows.Forms.Label();
            this.ResetBtn = new System.Windows.Forms.Button();
            this.ClipboardBtn = new System.Windows.Forms.Button();
            this.controlNameLabel = new System.Windows.Forms.Label();
            this.GarbageCollectionBtn = new System.Windows.Forms.Button();
            this.NumPaintsLabel = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.StressTestBtn = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.UnmanagedMemoryLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // FpsLabel
            // 
            resources.ApplyResources(this.FpsLabel, "FpsLabel");
            this.FpsLabel.Name = "FpsLabel";
            // 
            // ManagedMemoryLabel
            // 
            resources.ApplyResources(this.ManagedMemoryLabel, "ManagedMemoryLabel");
            this.ManagedMemoryLabel.Name = "ManagedMemoryLabel";
            // 
            // MaxFpsLabel
            // 
            resources.ApplyResources(this.MaxFpsLabel, "MaxFpsLabel");
            this.MaxFpsLabel.Name = "MaxFpsLabel";
            // 
            // ResetBtn
            // 
            this.ResetBtn.AutoEllipsis = true;
            resources.ApplyResources(this.ResetBtn, "ResetBtn");
            this.ResetBtn.Name = "ResetBtn";
            this.ResetBtn.UseVisualStyleBackColor = true;
            this.ResetBtn.Click += new System.EventHandler(this.ResetBtn_Click);
            // 
            // ClipboardBtn
            // 
            this.ClipboardBtn.AutoEllipsis = true;
            resources.ApplyResources(this.ClipboardBtn, "ClipboardBtn");
            this.ClipboardBtn.Name = "ClipboardBtn";
            this.ClipboardBtn.UseVisualStyleBackColor = true;
            this.ClipboardBtn.Click += new System.EventHandler(this.ClipboardBtn_Click);
            // 
            // controlNameLabel
            // 
            this.controlNameLabel.AutoEllipsis = true;
            resources.ApplyResources(this.controlNameLabel, "controlNameLabel");
            this.controlNameLabel.Name = "controlNameLabel";
            // 
            // GarbageCollectionBtn
            // 
            resources.ApplyResources(this.GarbageCollectionBtn, "GarbageCollectionBtn");
            this.GarbageCollectionBtn.Name = "GarbageCollectionBtn";
            this.GarbageCollectionBtn.UseVisualStyleBackColor = true;
            this.GarbageCollectionBtn.Click += new System.EventHandler(this.GarbageCollectionBtn_Click);
            // 
            // NumPaintsLabel
            // 
            resources.ApplyResources(this.NumPaintsLabel, "NumPaintsLabel");
            this.NumPaintsLabel.Name = "NumPaintsLabel";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // StressTestBtn
            // 
            resources.ApplyResources(this.StressTestBtn, "StressTestBtn");
            this.StressTestBtn.Name = "StressTestBtn";
            this.StressTestBtn.UseVisualStyleBackColor = true;
            this.StressTestBtn.Click += new System.EventHandler(this.StressTestBtn_Click);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // UnmanagedMemoryLabel
            // 
            resources.ApplyResources(this.UnmanagedMemoryLabel, "UnmanagedMemoryLabel");
            this.UnmanagedMemoryLabel.Name = "UnmanagedMemoryLabel";
            // 
            // PerformanceMonitorControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.UnmanagedMemoryLabel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.StressTestBtn);
            this.Controls.Add(this.NumPaintsLabel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.GarbageCollectionBtn);
            this.Controls.Add(this.controlNameLabel);
            this.Controls.Add(this.ClipboardBtn);
            this.Controls.Add(this.ResetBtn);
            this.Controls.Add(this.MaxFpsLabel);
            this.Controls.Add(this.ManagedMemoryLabel);
            this.Controls.Add(this.FpsLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "PerformanceMonitorControl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label FpsLabel;
        private System.Windows.Forms.Label ManagedMemoryLabel;
        private System.Windows.Forms.Label MaxFpsLabel;
        private System.Windows.Forms.Button ResetBtn;
        private System.Windows.Forms.Button ClipboardBtn;
        private System.Windows.Forms.Label controlNameLabel;
        private System.Windows.Forms.Button GarbageCollectionBtn;
        private System.Windows.Forms.Label NumPaintsLabel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button StressTestBtn;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label UnmanagedMemoryLabel;
    }
}
