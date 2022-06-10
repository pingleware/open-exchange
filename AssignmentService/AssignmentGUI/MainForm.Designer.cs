namespace OPEX.AssignmentGUI
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblTimeStart = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel5 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblTimeEnd = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel7 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblTimeLeft = new System.Windows.Forms.ToolStripStatusLabel();
            this.exchangeStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.grpControl = new System.Windows.Forms.GroupBox();
            this.controlPanel1 = new OPEX.AssignmentGUI.ControlPanel();
            this.grpExplorer = new System.Windows.Forms.GroupBox();
            this.jobExplorerPanel1 = new OPEX.AssignmentGUI.JobExplorerPanel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1.SuspendLayout();
            this.grpControl.SuspendLayout();
            this.grpExplorer.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.toolStripStatusLabel3,
            this.lblTimeStart,
            this.toolStripStatusLabel5,
            this.lblTimeEnd,
            this.toolStripStatusLabel7,
            this.lblTimeLeft,
            this.exchangeStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 522);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(729, 22);
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(238, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.Text = "Connecting to services...";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(73, 17);
            this.toolStripStatusLabel3.Text = "Session Start";
            // 
            // lblTimeStart
            // 
            this.lblTimeStart.Name = "lblTimeStart";
            this.lblTimeStart.Size = new System.Drawing.Size(49, 17);
            this.lblTimeStart.Text = "00:00:00";
            // 
            // toolStripStatusLabel5
            // 
            this.toolStripStatusLabel5.Name = "toolStripStatusLabel5";
            this.toolStripStatusLabel5.Size = new System.Drawing.Size(69, 17);
            this.toolStripStatusLabel5.Text = "Session End";
            // 
            // lblTimeEnd
            // 
            this.lblTimeEnd.Name = "lblTimeEnd";
            this.lblTimeEnd.Size = new System.Drawing.Size(49, 17);
            this.lblTimeEnd.Text = "00:00:00";
            // 
            // toolStripStatusLabel7
            // 
            this.toolStripStatusLabel7.Name = "toolStripStatusLabel7";
            this.toolStripStatusLabel7.Size = new System.Drawing.Size(57, 17);
            this.toolStripStatusLabel7.Text = "Time Left";
            // 
            // lblTimeLeft
            // 
            this.lblTimeLeft.Name = "lblTimeLeft";
            this.lblTimeLeft.Size = new System.Drawing.Size(49, 17);
            this.lblTimeLeft.Text = "00:00:00";
            // 
            // exchangeStatusLabel
            // 
            this.exchangeStatusLabel.AutoSize = false;
            this.exchangeStatusLabel.BackColor = System.Drawing.Color.Yellow;
            this.exchangeStatusLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exchangeStatusLabel.MergeIndex = 0;
            this.exchangeStatusLabel.Name = "exchangeStatusLabel";
            this.exchangeStatusLabel.Size = new System.Drawing.Size(130, 17);
            this.exchangeStatusLabel.Text = "Exchange Unavailable";
            this.exchangeStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // grpControl
            // 
            this.grpControl.Controls.Add(this.controlPanel1);
            this.grpControl.Enabled = false;
            this.grpControl.Location = new System.Drawing.Point(12, 27);
            this.grpControl.Name = "grpControl";
            this.grpControl.Size = new System.Drawing.Size(713, 154);
            this.grpControl.TabIndex = 10;
            this.grpControl.TabStop = false;
            this.grpControl.Text = "Control Panel";
            // 
            // controlPanel1
            // 
            this.controlPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.controlPanel1.Location = new System.Drawing.Point(5, 15);
            this.controlPanel1.Name = "controlPanel1";
            this.controlPanel1.Size = new System.Drawing.Size(702, 133);
            this.controlPanel1.TabIndex = 0;
            // 
            // grpExplorer
            // 
            this.grpExplorer.Controls.Add(this.jobExplorerPanel1);
            this.grpExplorer.Enabled = false;
            this.grpExplorer.Location = new System.Drawing.Point(12, 187);
            this.grpExplorer.Name = "grpExplorer";
            this.grpExplorer.Size = new System.Drawing.Size(713, 341);
            this.grpExplorer.TabIndex = 11;
            this.grpExplorer.TabStop = false;
            this.grpExplorer.Text = "Job Explorer";
            // 
            // jobExplorerPanel1
            // 
            this.jobExplorerPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.jobExplorerPanel1.Location = new System.Drawing.Point(6, 19);
            this.jobExplorerPanel1.Name = "jobExplorerPanel1";
            this.jobExplorerPanel1.Size = new System.Drawing.Size(701, 316);
            this.jobExplorerPanel1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(729, 24);
            this.menuStrip1.TabIndex = 12;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(729, 544);
            this.Controls.Add(this.grpExplorer);
            this.Controls.Add(this.grpControl);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(745, 582);
            this.MinimumSize = new System.Drawing.Size(745, 582);
            this.Name = "MainForm";
            this.Text = "Admin GUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.grpControl.ResumeLayout(false);
            this.grpExplorer.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel exchangeStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel lblTimeStart;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel5;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripStatusLabel lblTimeEnd;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel7;
        private System.Windows.Forms.ToolStripStatusLabel lblTimeLeft;
        private System.Windows.Forms.GroupBox grpControl;
        private System.Windows.Forms.GroupBox grpExplorer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private ControlPanel controlPanel1;
        private JobExplorerPanel jobExplorerPanel1;
    }
}