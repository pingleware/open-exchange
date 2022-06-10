namespace OPEX.TradingGUI
{
    partial class DepthPanelControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lstDepth = new System.Windows.Forms.ListView();
            this.colHdrBidQty = new System.Windows.Forms.ColumnHeader();
            this.colHdrBidPrice = new System.Windows.Forms.ColumnHeader();
            this.colHdrAskPrice = new System.Windows.Forms.ColumnHeader();
            this.colHdrAskQty = new System.Windows.Forms.ColumnHeader();
            this.lblLastTrd = new System.Windows.Forms.Label();
            this.lblLastTradeQty = new System.Windows.Forms.Label();
            this.lblLastTradePrice = new System.Windows.Forms.Label();
            this.lblAt = new System.Windows.Forms.Label();
            this.lblInstrument = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblBidSpreadValue = new System.Windows.Forms.Label();
            this.lblBidSpread = new System.Windows.Forms.Label();
            this.lblAskSpreadValue = new System.Windows.Forms.Label();
            this.lblAskSpread = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lstDepth
            // 
            this.lstDepth.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstDepth.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colHdrBidQty,
            this.colHdrBidPrice,
            this.colHdrAskPrice,
            this.colHdrAskQty});
            this.lstDepth.GridLines = true;
            this.lstDepth.Location = new System.Drawing.Point(0, 0);
            this.lstDepth.Name = "lstDepth";
            this.lstDepth.Size = new System.Drawing.Size(244, 106);
            this.lstDepth.TabIndex = 0;
            this.lstDepth.UseCompatibleStateImageBehavior = false;
            this.lstDepth.View = System.Windows.Forms.View.Details;
            // 
            // colHdrBidQty
            // 
            this.colHdrBidQty.Text = "BidQty";
            // 
            // colHdrBidPrice
            // 
            this.colHdrBidPrice.Text = "BidPrice";
            // 
            // colHdrAskPrice
            // 
            this.colHdrAskPrice.Text = "AskPrice";
            // 
            // colHdrAskQty
            // 
            this.colHdrAskQty.Text = "AskQty";
            // 
            // lblLastTrd
            // 
            this.lblLastTrd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLastTrd.AutoSize = true;
            this.lblLastTrd.Location = new System.Drawing.Point(-3, 107);
            this.lblLastTrd.Name = "lblLastTrd";
            this.lblLastTrd.Size = new System.Drawing.Size(58, 13);
            this.lblLastTrd.TabIndex = 1;
            this.lblLastTrd.Text = "Last Trade";
            // 
            // lblLastTradeQty
            // 
            this.lblLastTradeQty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLastTradeQty.AutoSize = true;
            this.lblLastTradeQty.Location = new System.Drawing.Point(58, 107);
            this.lblLastTradeQty.Name = "lblLastTradeQty";
            this.lblLastTradeQty.Size = new System.Drawing.Size(40, 13);
            this.lblLastTradeQty.TabIndex = 2;
            this.lblLastTradeQty.Text = "50,000";
            // 
            // lblLastTradePrice
            // 
            this.lblLastTradePrice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLastTradePrice.AutoSize = true;
            this.lblLastTradePrice.Location = new System.Drawing.Point(128, 107);
            this.lblLastTradePrice.Name = "lblLastTradePrice";
            this.lblLastTradePrice.Size = new System.Drawing.Size(52, 13);
            this.lblLastTradePrice.TabIndex = 3;
            this.lblLastTradePrice.Text = "160.1234";
            // 
            // lblAt
            // 
            this.lblAt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAt.AutoSize = true;
            this.lblAt.Location = new System.Drawing.Point(104, 107);
            this.lblAt.Name = "lblAt";
            this.lblAt.Size = new System.Drawing.Size(18, 13);
            this.lblAt.TabIndex = 4;
            this.lblAt.Text = "@";
            // 
            // lblInstrument
            // 
            this.lblInstrument.AutoSize = true;
            this.lblInstrument.Location = new System.Drawing.Point(3, 0);
            this.lblInstrument.Name = "lblInstrument";
            this.lblInstrument.Size = new System.Drawing.Size(0, 13);
            this.lblInstrument.TabIndex = 5;
            // 
            // lblTime
            // 
            this.lblTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(189, 107);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(49, 13);
            this.lblTime.TabIndex = 6;
            this.lblTime.Text = "00:00:00";
            // 
            // lblBidSpreadValue
            // 
            this.lblBidSpreadValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblBidSpreadValue.AutoSize = true;
            this.lblBidSpreadValue.Location = new System.Drawing.Point(58, 120);
            this.lblBidSpreadValue.Name = "lblBidSpreadValue";
            this.lblBidSpreadValue.Size = new System.Drawing.Size(40, 13);
            this.lblBidSpreadValue.TabIndex = 8;
            this.lblBidSpreadValue.Text = "0.2000";
            // 
            // lblBidSpread
            // 
            this.lblBidSpread.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblBidSpread.AutoSize = true;
            this.lblBidSpread.Location = new System.Drawing.Point(-3, 120);
            this.lblBidSpread.Name = "lblBidSpread";
            this.lblBidSpread.Size = new System.Drawing.Size(59, 13);
            this.lblBidSpread.TabIndex = 7;
            this.lblBidSpread.Text = "Bid Spread";
            // 
            // lblAskSpreadValue
            // 
            this.lblAskSpreadValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAskSpreadValue.AutoSize = true;
            this.lblAskSpreadValue.Location = new System.Drawing.Point(189, 120);
            this.lblAskSpreadValue.Name = "lblAskSpreadValue";
            this.lblAskSpreadValue.Size = new System.Drawing.Size(40, 13);
            this.lblAskSpreadValue.TabIndex = 10;
            this.lblAskSpreadValue.Text = "0.1667";
            // 
            // lblAskSpread
            // 
            this.lblAskSpread.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAskSpread.AutoSize = true;
            this.lblAskSpread.Location = new System.Drawing.Point(128, 120);
            this.lblAskSpread.Name = "lblAskSpread";
            this.lblAskSpread.Size = new System.Drawing.Size(62, 13);
            this.lblAskSpread.TabIndex = 9;
            this.lblAskSpread.Text = "Ask Spread";
            // 
            // DepthPanelControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblAskSpreadValue);
            this.Controls.Add(this.lblAskSpread);
            this.Controls.Add(this.lblBidSpreadValue);
            this.Controls.Add(this.lblBidSpread);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.lblInstrument);
            this.Controls.Add(this.lblAt);
            this.Controls.Add(this.lblLastTradePrice);
            this.Controls.Add(this.lblLastTradeQty);
            this.Controls.Add(this.lblLastTrd);
            this.Controls.Add(this.lstDepth);
            this.Name = "DepthPanelControl";
            this.Size = new System.Drawing.Size(244, 135);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lstDepth;
        private System.Windows.Forms.ColumnHeader colHdrBidQty;
        private System.Windows.Forms.ColumnHeader colHdrBidPrice;
        private System.Windows.Forms.ColumnHeader colHdrAskPrice;
        private System.Windows.Forms.ColumnHeader colHdrAskQty;
        private System.Windows.Forms.Label lblLastTrd;
        private System.Windows.Forms.Label lblLastTradeQty;
        private System.Windows.Forms.Label lblLastTradePrice;
        private System.Windows.Forms.Label lblAt;
        private System.Windows.Forms.Label lblInstrument;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label lblBidSpreadValue;
        private System.Windows.Forms.Label lblBidSpread;
        private System.Windows.Forms.Label lblAskSpreadValue;
        private System.Windows.Forms.Label lblAskSpread;

    }
}
