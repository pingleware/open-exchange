namespace OPEX.TradingGUI
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
            this.grpOrder = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.orderPanelControl1 = new OPEX.TradingGUI.OrderPanelControl();
            this.grpDepth = new System.Windows.Forms.GroupBox();
            this.cmbDepthInstrument = new System.Windows.Forms.ComboBox();
            this.lblInstrument = new System.Windows.Forms.Label();
            this.depthPanelControl1 = new OPEX.TradingGUI.DepthPanelControl();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearDepthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setupDepthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.orderGeneratorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.orderBlotter1 = new OPEX.TradingGUI.OrderBlotter();
            this.tradeBlotter1 = new OPEX.TradingGUI.TradeBlotter();
            this.grpOrder.SuspendLayout();
            this.grpDepth.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpOrder
            // 
            this.grpOrder.Controls.Add(this.button1);
            this.grpOrder.Controls.Add(this.orderPanelControl1);
            this.grpOrder.Location = new System.Drawing.Point(0, 27);
            this.grpOrder.Name = "grpOrder";
            this.grpOrder.Size = new System.Drawing.Size(470, 225);
            this.grpOrder.TabIndex = 1;
            this.grpOrder.TabStop = false;
            this.grpOrder.Text = "Order";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(385, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(69, 30);
            this.button1.TabIndex = 2;
            this.button1.Text = "Set&up";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // orderPanelControl1
            // 
            this.orderPanelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.orderPanelControl1.Location = new System.Drawing.Point(3, 16);
            this.orderPanelControl1.Name = "orderPanelControl1";
            this.orderPanelControl1.Order = null;
            this.orderPanelControl1.Size = new System.Drawing.Size(464, 206);
            this.orderPanelControl1.TabIndex = 0;
            // 
            // grpDepth
            // 
            this.grpDepth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.grpDepth.Controls.Add(this.cmbDepthInstrument);
            this.grpDepth.Controls.Add(this.lblInstrument);
            this.grpDepth.Controls.Add(this.depthPanelControl1);
            this.grpDepth.Location = new System.Drawing.Point(476, 27);
            this.grpDepth.MaximumSize = new System.Drawing.Size(257, 225);
            this.grpDepth.MinimumSize = new System.Drawing.Size(257, 225);
            this.grpDepth.Name = "grpDepth";
            this.grpDepth.Size = new System.Drawing.Size(257, 225);
            this.grpDepth.TabIndex = 2;
            this.grpDepth.TabStop = false;
            this.grpDepth.Text = "Depth";
            // 
            // cmbDepthInstrument
            // 
            this.cmbDepthInstrument.FormattingEnabled = true;
            this.cmbDepthInstrument.Location = new System.Drawing.Point(68, 16);
            this.cmbDepthInstrument.Name = "cmbDepthInstrument";
            this.cmbDepthInstrument.Size = new System.Drawing.Size(84, 21);
            this.cmbDepthInstrument.TabIndex = 2;
            this.cmbDepthInstrument.SelectionChangeCommitted += new System.EventHandler(this.cmbDepthInstrument_SelectionChangeCommitted);
            this.cmbDepthInstrument.SelectedIndexChanged += new System.EventHandler(this.cmbDepthInstrument_SelectedIndexChanged);
            this.cmbDepthInstrument.SelectedValueChanged += new System.EventHandler(this.cmbDepthInstrument_SelectedValueChanged);
            // 
            // lblInstrument
            // 
            this.lblInstrument.AutoSize = true;
            this.lblInstrument.Location = new System.Drawing.Point(6, 19);
            this.lblInstrument.Name = "lblInstrument";
            this.lblInstrument.Size = new System.Drawing.Size(56, 13);
            this.lblInstrument.TabIndex = 1;
            this.lblInstrument.Text = "Instrument";
            // 
            // depthPanelControl1
            // 
            this.depthPanelControl1.Instrument = null;
            this.depthPanelControl1.Location = new System.Drawing.Point(5, 43);
            this.depthPanelControl1.Name = "depthPanelControl1";
            this.depthPanelControl1.Size = new System.Drawing.Size(244, 179);
            this.depthPanelControl1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(737, 24);
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
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearDepthToolStripMenuItem,
            this.setupDepthToolStripMenuItem,
            this.toolStripSeparator1,
            this.orderGeneratorToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // clearDepthToolStripMenuItem
            // 
            this.clearDepthToolStripMenuItem.Name = "clearDepthToolStripMenuItem";
            this.clearDepthToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.clearDepthToolStripMenuItem.Text = "&Clear Depth";
            this.clearDepthToolStripMenuItem.Click += new System.EventHandler(this.clearDepthToolStripMenuItem_Click);
            // 
            // setupDepthToolStripMenuItem
            // 
            this.setupDepthToolStripMenuItem.Name = "setupDepthToolStripMenuItem";
            this.setupDepthToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.setupDepthToolStripMenuItem.Text = "Setup &Depth";
            this.setupDepthToolStripMenuItem.Click += new System.EventHandler(this.setupDepthToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(165, 6);
            // 
            // orderGeneratorToolStripMenuItem
            // 
            this.orderGeneratorToolStripMenuItem.Name = "orderGeneratorToolStripMenuItem";
            this.orderGeneratorToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.orderGeneratorToolStripMenuItem.Text = "Order &Generator...";
            this.orderGeneratorToolStripMenuItem.Click += new System.EventHandler(this.orderGeneratorToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.aboutToolStripMenuItem.Text = "A&bout...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 258);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.orderBlotter1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tradeBlotter1);
            this.splitContainer1.Size = new System.Drawing.Size(733, 300);
            this.splitContainer1.SplitterDistance = 158;
            this.splitContainer1.TabIndex = 14;
            // 
            // orderBlotter1
            // 
            this.orderBlotter1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.orderBlotter1.Location = new System.Drawing.Point(0, 0);
            this.orderBlotter1.Name = "orderBlotter1";
            this.orderBlotter1.Size = new System.Drawing.Size(733, 158);
            this.orderBlotter1.TabIndex = 13;
            this.orderBlotter1.OrderSelected += new OPEX.TradingGUI.OrderBlotter.OrderSelectedEventHandler(this.orderBlotter1_OrderSelected);
            // 
            // tradeBlotter1
            // 
            this.tradeBlotter1.ApplicationName = null;
            this.tradeBlotter1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tradeBlotter1.Location = new System.Drawing.Point(0, 0);
            this.tradeBlotter1.Name = "tradeBlotter1";
            this.tradeBlotter1.Size = new System.Drawing.Size(733, 138);
            this.tradeBlotter1.TabIndex = 0;
            this.tradeBlotter1.Load += new System.EventHandler(this.tradeBlotter1_Load);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(737, 553);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.grpDepth);
            this.Controls.Add(this.grpOrder);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "OPen EXchange - Trading GUI";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.grpOrder.ResumeLayout(false);
            this.grpDepth.ResumeLayout(false);
            this.grpDepth.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OrderPanelControl orderPanelControl1;
        private System.Windows.Forms.GroupBox grpOrder;
        private System.Windows.Forms.GroupBox grpDepth;
        private DepthPanelControl depthPanelControl1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem orderGeneratorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private OrderBlotter orderBlotter1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private TradeBlotter tradeBlotter1;
        private System.Windows.Forms.ToolStripMenuItem clearDepthToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setupDepthToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ComboBox cmbDepthInstrument;
        private System.Windows.Forms.Label lblInstrument;
    }
}

