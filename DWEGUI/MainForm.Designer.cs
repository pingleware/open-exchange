namespace OPEX.DWEGUI
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.grpOrders = new System.Windows.Forms.GroupBox();
            this.orderBlotter = new OPEX.DWEGUI.OrderBlotter();
            this.grpTrades = new System.Windows.Forms.GroupBox();
            this.tradeBlotter = new OPEX.DWEGUI.TradeBlotter();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enablePopupsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.grpOrder = new System.Windows.Forms.GroupBox();
            this.assignmentBlotter = new OPEX.DWEGUI.Controls.AssignmentBlotter();
            this.grpDepth = new System.Windows.Forms.GroupBox();
            this.depthPanelControl1 = new OPEX.DWEGUI.DepthPanelControl();
            this.grpInfo = new System.Windows.Forms.GroupBox();
            this.infoPanel1 = new OPEX.DWEGUI.Controls.InfoPanel();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.grpOrders.SuspendLayout();
            this.grpTrades.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.grpOrder.SuspendLayout();
            this.grpDepth.SuspendLayout();
            this.grpInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 214);
            this.splitContainer1.MinimumSize = new System.Drawing.Size(737, 326);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.grpOrders);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.grpTrades);
            this.splitContainer1.Size = new System.Drawing.Size(815, 326);
            this.splitContainer1.SplitterDistance = 172;
            this.splitContainer1.TabIndex = 2;
            // 
            // grpOrders
            // 
            this.grpOrders.Controls.Add(this.orderBlotter);
            this.grpOrders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpOrders.Location = new System.Drawing.Point(0, 0);
            this.grpOrders.Name = "grpOrders";
            this.grpOrders.Size = new System.Drawing.Size(815, 172);
            this.grpOrders.TabIndex = 2;
            this.grpOrders.TabStop = false;
            this.grpOrders.Text = "Orders";
            // 
            // orderBlotter
            // 
            this.orderBlotter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.orderBlotter.Location = new System.Drawing.Point(3, 16);
            this.orderBlotter.Name = "orderBlotter";
            this.orderBlotter.Size = new System.Drawing.Size(809, 153);
            this.orderBlotter.TabIndex = 1;
            // 
            // grpTrades
            // 
            this.grpTrades.Controls.Add(this.tradeBlotter);
            this.grpTrades.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTrades.Location = new System.Drawing.Point(0, 0);
            this.grpTrades.Name = "grpTrades";
            this.grpTrades.Size = new System.Drawing.Size(815, 150);
            this.grpTrades.TabIndex = 1;
            this.grpTrades.TabStop = false;
            this.grpTrades.Text = "Trades";
            // 
            // tradeBlotter
            // 
            this.tradeBlotter.ApplicationName = null;
            this.tradeBlotter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tradeBlotter.Location = new System.Drawing.Point(3, 16);
            this.tradeBlotter.Name = "tradeBlotter";
            this.tradeBlotter.Size = new System.Drawing.Size(809, 131);
            this.tradeBlotter.TabIndex = 0;
            this.tradeBlotter.Load += new System.EventHandler(this.tradeBlotter_Load);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem,
            this.debugToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(815, 24);
            this.menuStrip1.TabIndex = 4;
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
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enablePopupsToolStripMenuItem});
            this.viewToolStripMenuItem.Enabled = false;
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            this.viewToolStripMenuItem.Visible = false;
            // 
            // enablePopupsToolStripMenuItem
            // 
            this.enablePopupsToolStripMenuItem.Checked = true;
            this.enablePopupsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enablePopupsToolStripMenuItem.Name = "enablePopupsToolStripMenuItem";
            this.enablePopupsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.enablePopupsToolStripMenuItem.Text = "Enable popups";
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
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "A&bout";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "&Debug";
            this.debugToolStripMenuItem.Visible = false;
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.testToolStripMenuItem.Text = "Test";
            this.testToolStripMenuItem.Click += new System.EventHandler(this.testToolStripMenuItem_Click);
            // 
            // grpOrder
            // 
            this.grpOrder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpOrder.Controls.Add(this.assignmentBlotter);
            this.grpOrder.Location = new System.Drawing.Point(0, 27);
            this.grpOrder.MinimumSize = new System.Drawing.Size(184, 181);
            this.grpOrder.Name = "grpOrder";
            this.grpOrder.Size = new System.Drawing.Size(371, 181);
            this.grpOrder.TabIndex = 5;
            this.grpOrder.TabStop = false;
            this.grpOrder.Text = "Client Orders";
            // 
            // assignmentBlotter
            // 
            this.assignmentBlotter.Dock = System.Windows.Forms.DockStyle.Top;
            this.assignmentBlotter.Location = new System.Drawing.Point(3, 16);
            this.assignmentBlotter.Name = "assignmentBlotter";
            this.assignmentBlotter.Size = new System.Drawing.Size(365, 109);
            this.assignmentBlotter.TabIndex = 1;
            this.assignmentBlotter.AssignmentBlotterClicked += new OPEX.DWEGUI.Controls.AssignmentBlotterClickedEventHandler(this.assignmentBlotter_AssignmentBlotterClicked);
            // 
            // grpDepth
            // 
            this.grpDepth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.grpDepth.Controls.Add(this.depthPanelControl1);
            this.grpDepth.Location = new System.Drawing.Point(551, 27);
            this.grpDepth.MaximumSize = new System.Drawing.Size(261, 181);
            this.grpDepth.MinimumSize = new System.Drawing.Size(261, 181);
            this.grpDepth.Name = "grpDepth";
            this.grpDepth.Size = new System.Drawing.Size(261, 181);
            this.grpDepth.TabIndex = 6;
            this.grpDepth.TabStop = false;
            this.grpDepth.Text = "OrderBook";
            // 
            // depthPanelControl1
            // 
            this.depthPanelControl1.Instrument = null;
            this.depthPanelControl1.Location = new System.Drawing.Point(6, 11);
            this.depthPanelControl1.Name = "depthPanelControl1";
            this.depthPanelControl1.Size = new System.Drawing.Size(249, 164);
            this.depthPanelControl1.TabIndex = 0;
            // 
            // grpInfo
            // 
            this.grpInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.grpInfo.Controls.Add(this.infoPanel1);
            this.grpInfo.Location = new System.Drawing.Point(377, 27);
            this.grpInfo.MaximumSize = new System.Drawing.Size(168, 181);
            this.grpInfo.MinimumSize = new System.Drawing.Size(168, 181);
            this.grpInfo.Name = "grpInfo";
            this.grpInfo.Size = new System.Drawing.Size(168, 181);
            this.grpInfo.TabIndex = 7;
            this.grpInfo.TabStop = false;
            this.grpInfo.Text = "Info";
            // 
            // infoPanel1
            // 
            this.infoPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoPanel1.Location = new System.Drawing.Point(3, 16);
            this.infoPanel1.Name = "infoPanel1";
            this.infoPanel1.Size = new System.Drawing.Size(162, 162);
            this.infoPanel1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(815, 553);
            this.Controls.Add(this.grpInfo);
            this.Controls.Add(this.grpDepth);
            this.Controls.Add(this.grpOrder);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(745, 580);
            this.Name = "MainForm";
            this.Text = "TradeTech 2011 OpenExchange";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.grpOrders.ResumeLayout(false);
            this.grpTrades.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.grpOrder.ResumeLayout(false);
            this.grpDepth.ResumeLayout(false);
            this.grpInfo.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TradeBlotter tradeBlotter;
        private OrderBlotter orderBlotter;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.GroupBox grpOrder;
        private System.Windows.Forms.GroupBox grpDepth;
        private DepthPanelControl depthPanelControl1;
        private System.Windows.Forms.GroupBox grpOrders;
        private System.Windows.Forms.GroupBox grpTrades;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private OPEX.DWEGUI.Controls.InfoPanel infoPanel1;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enablePopupsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private OPEX.DWEGUI.Controls.AssignmentBlotter assignmentBlotter;
        private System.Windows.Forms.GroupBox grpInfo;
    }
}