namespace OPEX.SalesGUI
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
            this.orderBlotter = new OPEX.SalesGUI.OrderBlotter();
            this.grpTrades = new System.Windows.Forms.GroupBox();
            this.tradeBlotter = new OPEX.SalesGUI.TradeBlotter();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.grpOrder = new System.Windows.Forms.GroupBox();
            this.orderPanelControl1 = new OPEX.SalesGUI.OrderPanelControl();
            this.grpDepth = new System.Windows.Forms.GroupBox();
            this.depthPanelControl1 = new OPEX.SalesGUI.DepthPanelControl();
            this.grpInfo = new System.Windows.Forms.GroupBox();
            this.infoPanel1 = new OPEX.SalesGUI.Controls.InfoPanel();
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
            this.splitContainer1.Size = new System.Drawing.Size(737, 326);
            this.splitContainer1.SplitterDistance = 172;
            this.splitContainer1.TabIndex = 2;
            // 
            // grpOrders
            // 
            this.grpOrders.Controls.Add(this.orderBlotter);
            this.grpOrders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpOrders.Location = new System.Drawing.Point(0, 0);
            this.grpOrders.Name = "grpOrders";
            this.grpOrders.Size = new System.Drawing.Size(737, 172);
            this.grpOrders.TabIndex = 2;
            this.grpOrders.TabStop = false;
            this.grpOrders.Text = "Orders";
            // 
            // orderBlotter
            // 
            this.orderBlotter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.orderBlotter.Location = new System.Drawing.Point(3, 16);
            this.orderBlotter.Name = "orderBlotter";
            this.orderBlotter.Size = new System.Drawing.Size(731, 153);
            this.orderBlotter.TabIndex = 1;
            // 
            // grpTrades
            // 
            this.grpTrades.Controls.Add(this.tradeBlotter);
            this.grpTrades.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTrades.Location = new System.Drawing.Point(0, 0);
            this.grpTrades.Name = "grpTrades";
            this.grpTrades.Size = new System.Drawing.Size(737, 150);
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
            this.tradeBlotter.Size = new System.Drawing.Size(731, 131);
            this.tradeBlotter.TabIndex = 0;
            this.tradeBlotter.Load += new System.EventHandler(this.tradeBlotter_Load);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem,
            this.debugToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(737, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.aboutToolStripMenuItem.Text = "A&bout";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.expToolStripMenuItem});
            this.debugToolStripMenuItem.Enabled = false;
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.debugToolStripMenuItem.Text = "&Debug";
            this.debugToolStripMenuItem.Visible = false;
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // expToolStripMenuItem
            // 
            this.expToolStripMenuItem.Name = "expToolStripMenuItem";
            this.expToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.expToolStripMenuItem.Text = "Exp";
            this.expToolStripMenuItem.Click += new System.EventHandler(this.expToolStripMenuItem_Click);
            // 
            // grpOrder
            // 
            this.grpOrder.Controls.Add(this.orderPanelControl1);
            this.grpOrder.Location = new System.Drawing.Point(0, 27);
            this.grpOrder.MinimumSize = new System.Drawing.Size(184, 181);
            this.grpOrder.Name = "grpOrder";
            this.grpOrder.Size = new System.Drawing.Size(184, 181);
            this.grpOrder.TabIndex = 5;
            this.grpOrder.TabStop = false;
            this.grpOrder.Text = "Order";
            // 
            // orderPanelControl1
            // 
            this.orderPanelControl1.BackColor = System.Drawing.SystemColors.Control;
            this.orderPanelControl1.Location = new System.Drawing.Point(12, 11);
            this.orderPanelControl1.MaximumSize = new System.Drawing.Size(159, 157);
            this.orderPanelControl1.MinimumSize = new System.Drawing.Size(159, 157);
            this.orderPanelControl1.Name = "orderPanelControl1";
            this.orderPanelControl1.Order = null;
            this.orderPanelControl1.Size = new System.Drawing.Size(159, 157);
            this.orderPanelControl1.TabIndex = 2;
            this.orderPanelControl1.SendButtonPressed += new System.EventHandler(this.orderPanelControl1_SendButtonPressed);
            this.orderPanelControl1.AmendButtonPressed += new OPEX.SalesGUI.AmendButtonPressedEventHandler(this.orderPanelControl1_AmendButtonPressed);
            // 
            // grpDepth
            // 
            this.grpDepth.Controls.Add(this.depthPanelControl1);
            this.grpDepth.Location = new System.Drawing.Point(190, 27);
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
            this.grpInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpInfo.Controls.Add(this.infoPanel1);
            this.grpInfo.Location = new System.Drawing.Point(463, 27);
            this.grpInfo.MinimumSize = new System.Drawing.Size(274, 181);
            this.grpInfo.Name = "grpInfo";
            this.grpInfo.Size = new System.Drawing.Size(274, 181);
            this.grpInfo.TabIndex = 6;
            this.grpInfo.TabStop = false;
            this.grpInfo.Text = "Info";
            // 
            // infoPanel1
            // 
            this.infoPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoPanel1.Location = new System.Drawing.Point(3, 16);
            this.infoPanel1.Name = "infoPanel1";
            this.infoPanel1.Size = new System.Drawing.Size(268, 162);
            this.infoPanel1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(737, 553);
            this.Controls.Add(this.grpDepth);
            this.Controls.Add(this.grpInfo);
            this.Controls.Add(this.grpOrder);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(745, 580);
            this.Name = "MainForm";
            this.Text = "TradeTech 2010 - Sales Trading Simulator";
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
        private System.Windows.Forms.GroupBox grpInfo;
        private DepthPanelControl depthPanelControl1;
        private System.Windows.Forms.GroupBox grpOrders;
        private System.Windows.Forms.GroupBox grpTrades;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private OPEX.SalesGUI.Controls.InfoPanel infoPanel1;
        private OrderPanelControl orderPanelControl1;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expToolStripMenuItem;
    }
}