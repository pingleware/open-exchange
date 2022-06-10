namespace OPEX.DWEGUI
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
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.colHdrBidQty = new System.Windows.Forms.ColumnHeader();
            this.colHdrBidPrice = new System.Windows.Forms.ColumnHeader();
            this.colHdrAskPrice = new System.Windows.Forms.ColumnHeader();
            this.colHdrAskQty = new System.Windows.Forms.ColumnHeader();
            this.lblLastTrd = new System.Windows.Forms.Label();
            this.lblLastTradeQty = new System.Windows.Forms.Label();
            this.lblLastTradePrice = new System.Windows.Forms.Label();
            this.lblAt = new System.Windows.Forms.Label();
            this.lblRIC = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lstDepth
            // 
            this.lstDepth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lstDepth.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.colHdrBidQty,
            this.colHdrBidPrice,
            this.colHdrAskPrice,
            this.colHdrAskQty});
            this.lstDepth.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstDepth.GridLines = true;
            this.lstDepth.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstDepth.LabelWrap = false;
            this.lstDepth.Location = new System.Drawing.Point(0, 3);
            this.lstDepth.MaximumSize = new System.Drawing.Size(253, 500);
            this.lstDepth.MinimumSize = new System.Drawing.Size(253, 118);
            this.lstDepth.MultiSelect = false;
            this.lstDepth.Name = "lstDepth";
            this.lstDepth.Size = new System.Drawing.Size(253, 120);
            this.lstDepth.TabIndex = 0;
            this.lstDepth.UseCompatibleStateImageBehavior = false;
            this.lstDepth.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 0;
            // 
            // colHdrBidQty
            // 
            this.colHdrBidQty.Text = "BidQty";
            // 
            // colHdrBidPrice
            // 
            this.colHdrBidPrice.Text = "Bid";
            this.colHdrBidPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colHdrBidPrice.Width = 62;
            // 
            // colHdrAskPrice
            // 
            this.colHdrAskPrice.Text = "Ask";
            this.colHdrAskPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // colHdrAskQty
            // 
            this.colHdrAskQty.Text = "AskQty";
            this.colHdrAskQty.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colHdrAskQty.Width = 63;
            // 
            // lblLastTrd
            // 
            this.lblLastTrd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLastTrd.AutoSize = true;
            this.lblLastTrd.Location = new System.Drawing.Point(66, 124);
            this.lblLastTrd.Name = "lblLastTrd";
            this.lblLastTrd.Size = new System.Drawing.Size(61, 13);
            this.lblLastTrd.TabIndex = 1;
            this.lblLastTrd.Text = "Last Trade:";
            this.lblLastTrd.Visible = false;
            // 
            // lblLastTradeQty
            // 
            this.lblLastTradeQty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLastTradeQty.AutoSize = true;
            this.lblLastTradeQty.Location = new System.Drawing.Point(123, 124);
            this.lblLastTradeQty.Name = "lblLastTradeQty";
            this.lblLastTradeQty.Size = new System.Drawing.Size(0, 13);
            this.lblLastTradeQty.TabIndex = 2;
            this.lblLastTradeQty.Visible = false;
            // 
            // lblLastTradePrice
            // 
            this.lblLastTradePrice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLastTradePrice.AutoSize = true;
            this.lblLastTradePrice.Location = new System.Drawing.Point(190, 124);
            this.lblLastTradePrice.Name = "lblLastTradePrice";
            this.lblLastTradePrice.Size = new System.Drawing.Size(0, 13);
            this.lblLastTradePrice.TabIndex = 3;
            this.lblLastTradePrice.Visible = false;
            // 
            // lblAt
            // 
            this.lblAt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAt.AutoSize = true;
            this.lblAt.Location = new System.Drawing.Point(169, 124);
            this.lblAt.Name = "lblAt";
            this.lblAt.Size = new System.Drawing.Size(18, 13);
            this.lblAt.TabIndex = 4;
            this.lblAt.Text = "@";
            this.lblAt.Visible = false;
            // 
            // lblRIC
            // 
            this.lblRIC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblRIC.AutoSize = true;
            this.lblRIC.Location = new System.Drawing.Point(3, 124);
            this.lblRIC.Name = "lblRIC";
            this.lblRIC.Size = new System.Drawing.Size(0, 13);
            this.lblRIC.TabIndex = 6;
            this.lblRIC.Visible = false;
            // 
            // DepthPanelControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblRIC);
            this.Controls.Add(this.lblAt);
            this.Controls.Add(this.lblLastTradePrice);
            this.Controls.Add(this.lblLastTradeQty);
            this.Controls.Add(this.lblLastTrd);
            this.Controls.Add(this.lstDepth);
            this.DoubleBuffered = true;
            this.Name = "DepthPanelControl";
            this.Size = new System.Drawing.Size(255, 137);
            this.ParentChanged += new System.EventHandler(this.DepthPanelControl_ParentChanged);
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
        private System.Windows.Forms.Label lblRIC;
        private System.Windows.Forms.ColumnHeader columnHeader1;

    }
}
