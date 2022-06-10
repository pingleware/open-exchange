namespace OPEX.SalesGUI
{
    partial class OrderPanelControl
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
            this.components = new System.ComponentModel.Container();
            this.lblRIC = new System.Windows.Forms.Label();
            this.txtRIC = new System.Windows.Forms.TextBox();
            this.optBuy = new System.Windows.Forms.RadioButton();
            this.optSell = new System.Windows.Forms.RadioButton();
            this.lblLimitPrice = new System.Windows.Forms.Label();
            this.lblPrice = new System.Windows.Forms.Label();
            this.spinPrice = new System.Windows.Forms.NumericUpDown();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.btnAmend = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblOID = new System.Windows.Forms.Label();
            this.txtOID = new System.Windows.Forms.TextBox();
            this.txtLimitPrice = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.spinPrice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblRIC
            // 
            this.lblRIC.AutoSize = true;
            this.lblRIC.Location = new System.Drawing.Point(30, 32);
            this.lblRIC.Name = "lblRIC";
            this.lblRIC.Size = new System.Drawing.Size(25, 13);
            this.lblRIC.TabIndex = 210;
            this.lblRIC.Text = "RIC";
            // 
            // txtRIC
            // 
            this.txtRIC.BackColor = System.Drawing.SystemColors.Control;
            this.txtRIC.Location = new System.Drawing.Point(61, 29);
            this.txtRIC.Name = "txtRIC";
            this.txtRIC.ReadOnly = true;
            this.txtRIC.Size = new System.Drawing.Size(94, 20);
            this.txtRIC.TabIndex = 220;
            this.txtRIC.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txtRIC, "The RIC of the instrument to buy or sell");
            this.txtRIC.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EntryKeyDown);
            // 
            // optBuy
            // 
            this.optBuy.AutoSize = true;
            this.optBuy.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.optBuy.Enabled = false;
            this.optBuy.Location = new System.Drawing.Point(71, 210);
            this.optBuy.Name = "optBuy";
            this.optBuy.Size = new System.Drawing.Size(43, 17);
            this.optBuy.TabIndex = 110;
            this.optBuy.Text = "Buy";
            this.optBuy.UseVisualStyleBackColor = true;
            // 
            // optSell
            // 
            this.optSell.AutoSize = true;
            this.optSell.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.optSell.Enabled = false;
            this.optSell.Location = new System.Drawing.Point(120, 210);
            this.optSell.Name = "optSell";
            this.optSell.Size = new System.Drawing.Size(42, 17);
            this.optSell.TabIndex = 120;
            this.optSell.Text = "Sell";
            this.optSell.UseVisualStyleBackColor = true;
            // 
            // lblLimitPrice
            // 
            this.lblLimitPrice.AutoSize = true;
            this.lblLimitPrice.Location = new System.Drawing.Point(0, 58);
            this.lblLimitPrice.Name = "lblLimitPrice";
            this.lblLimitPrice.Size = new System.Drawing.Size(55, 13);
            this.lblLimitPrice.TabIndex = 310;
            this.lblLimitPrice.Text = "Limit Price";
            // 
            // lblPrice
            // 
            this.lblPrice.AutoSize = true;
            this.lblPrice.Location = new System.Drawing.Point(24, 90);
            this.lblPrice.Name = "lblPrice";
            this.lblPrice.Size = new System.Drawing.Size(31, 13);
            this.lblPrice.TabIndex = 410;
            this.lblPrice.Text = "Price";
            // 
            // spinPrice
            // 
            this.spinPrice.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spinPrice.Location = new System.Drawing.Point(61, 81);
            this.spinPrice.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.spinPrice.Name = "spinPrice";
            this.spinPrice.Size = new System.Drawing.Size(94, 29);
            this.spinPrice.TabIndex = 420;
            this.spinPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.spinPrice, "The price of the order");
            this.spinPrice.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EntryKeyDown);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // btnAmend
            // 
            this.btnAmend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAmend.BackColor = System.Drawing.SystemColors.Control;
            this.btnAmend.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAmend.Location = new System.Drawing.Point(5, 116);
            this.btnAmend.Name = "btnAmend";
            this.btnAmend.Size = new System.Drawing.Size(151, 38);
            this.btnAmend.TabIndex = 525;
            this.btnAmend.Text = "AMEND";
            this.toolTip1.SetToolTip(this.btnAmend, "Click here to amend the order");
            this.btnAmend.UseVisualStyleBackColor = false;
            this.btnAmend.Click += new System.EventHandler(this.btnAmend_Click);
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSend.BackColor = System.Drawing.SystemColors.Control;
            this.btnSend.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSend.Location = new System.Drawing.Point(4, 116);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(151, 38);
            this.btnSend.TabIndex = 524;
            this.btnSend.Text = "BUY";
            this.toolTip1.SetToolTip(this.btnSend, "Click here to send the order");
            this.btnSend.UseVisualStyleBackColor = false;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // lblOID
            // 
            this.lblOID.AutoSize = true;
            this.lblOID.Location = new System.Drawing.Point(11, 6);
            this.lblOID.Name = "lblOID";
            this.lblOID.Size = new System.Drawing.Size(44, 13);
            this.lblOID.TabIndex = 527;
            this.lblOID.Text = "OrderID";
            // 
            // txtOID
            // 
            this.txtOID.BackColor = System.Drawing.SystemColors.Control;
            this.txtOID.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOID.Location = new System.Drawing.Point(61, 3);
            this.txtOID.Name = "txtOID";
            this.txtOID.ReadOnly = true;
            this.txtOID.Size = new System.Drawing.Size(94, 20);
            this.txtOID.TabIndex = 528;
            this.txtOID.Text = "0";
            this.txtOID.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txtOID, "The ID of the order. Orders can be viewed in the Order Blotter");
            // 
            // txtLimitPrice
            // 
            this.txtLimitPrice.Location = new System.Drawing.Point(61, 55);
            this.txtLimitPrice.Name = "txtLimitPrice";
            this.txtLimitPrice.ReadOnly = true;
            this.txtLimitPrice.Size = new System.Drawing.Size(94, 20);
            this.txtLimitPrice.TabIndex = 529;
            this.txtLimitPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txtLimitPrice, "The Limit Price of the order, as instructed by the client");
            // 
            // OrderPanelControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.txtLimitPrice);
            this.Controls.Add(this.txtOID);
            this.Controls.Add(this.lblOID);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.spinPrice);
            this.Controls.Add(this.lblPrice);
            this.Controls.Add(this.lblLimitPrice);
            this.Controls.Add(this.optSell);
            this.Controls.Add(this.optBuy);
            this.Controls.Add(this.txtRIC);
            this.Controls.Add(this.lblRIC);
            this.Controls.Add(this.btnAmend);
            this.MaximumSize = new System.Drawing.Size(159, 157);
            this.MinimumSize = new System.Drawing.Size(159, 157);
            this.Name = "OrderPanelControl";
            this.Size = new System.Drawing.Size(159, 157);
            ((System.ComponentModel.ISupportInitialize)(this.spinPrice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblRIC;
        private System.Windows.Forms.TextBox txtRIC;
        private System.Windows.Forms.RadioButton optBuy;
        private System.Windows.Forms.RadioButton optSell;
        private System.Windows.Forms.Label lblLimitPrice;
        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.NumericUpDown spinPrice;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Button btnAmend;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblOID;
        private System.Windows.Forms.TextBox txtOID;
        private System.Windows.Forms.TextBox txtLimitPrice;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
