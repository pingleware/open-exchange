namespace OPEX.TradingGUI
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
            this.lblQty = new System.Windows.Forms.Label();
            this.lblPrice = new System.Windows.Forms.Label();
            this.lblTIF = new System.Windows.Forms.Label();
            this.spinQty = new System.Windows.Forms.NumericUpDown();
            this.spinPrice = new System.Windows.Forms.NumericUpDown();
            this.cmbTIF = new System.Windows.Forms.ComboBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.cmbAlgo = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.grpAlgoPanel = new System.Windows.Forms.GroupBox();
            this.pnlAlgoPlaceholder = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAmend = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblOID = new System.Windows.Forms.Label();
            this.txtOID = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.spinQty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPrice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.grpAlgoPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblRIC
            // 
            this.lblRIC.AutoSize = true;
            this.lblRIC.Location = new System.Drawing.Point(39, 60);
            this.lblRIC.Name = "lblRIC";
            this.lblRIC.Size = new System.Drawing.Size(25, 13);
            this.lblRIC.TabIndex = 210;
            this.lblRIC.Text = "RIC";
            // 
            // txtRIC
            // 
            this.txtRIC.Location = new System.Drawing.Point(70, 57);
            this.txtRIC.Name = "txtRIC";
            this.txtRIC.Size = new System.Drawing.Size(94, 20);
            this.txtRIC.TabIndex = 220;
            this.txtRIC.Text = "";
            this.txtRIC.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EntryKeyDown);
            // 
            // optBuy
            // 
            this.optBuy.AutoSize = true;
            this.optBuy.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.optBuy.Location = new System.Drawing.Point(39, 8);
            this.optBuy.Name = "optBuy";
            this.optBuy.Size = new System.Drawing.Size(43, 17);
            this.optBuy.TabIndex = 110;
            this.optBuy.Text = "Buy";
            this.optBuy.UseVisualStyleBackColor = true;
            this.optBuy.CheckedChanged += new System.EventHandler(this.optBuy_CheckedChanged);
            // 
            // optSell
            // 
            this.optSell.AutoSize = true;
            this.optSell.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.optSell.Location = new System.Drawing.Point(88, 8);
            this.optSell.Name = "optSell";
            this.optSell.Size = new System.Drawing.Size(42, 17);
            this.optSell.TabIndex = 120;
            this.optSell.Text = "Sell";
            this.optSell.UseVisualStyleBackColor = true;
            this.optSell.CheckedChanged += new System.EventHandler(this.optSell_CheckedChanged);
            // 
            // lblQty
            // 
            this.lblQty.AutoSize = true;
            this.lblQty.Location = new System.Drawing.Point(41, 86);
            this.lblQty.Name = "lblQty";
            this.lblQty.Size = new System.Drawing.Size(23, 13);
            this.lblQty.TabIndex = 310;
            this.lblQty.Text = "Qty";
            // 
            // lblPrice
            // 
            this.lblPrice.AutoSize = true;
            this.lblPrice.Location = new System.Drawing.Point(33, 112);
            this.lblPrice.Name = "lblPrice";
            this.lblPrice.Size = new System.Drawing.Size(31, 13);
            this.lblPrice.TabIndex = 410;
            this.lblPrice.Text = "Price";
            // 
            // lblTIF
            // 
            this.lblTIF.AutoSize = true;
            this.lblTIF.Location = new System.Drawing.Point(41, 138);
            this.lblTIF.Name = "lblTIF";
            this.lblTIF.Size = new System.Drawing.Size(23, 13);
            this.lblTIF.TabIndex = 510;
            this.lblTIF.Text = "TIF";
            // 
            // spinQty
            // 
            this.spinQty.Location = new System.Drawing.Point(70, 83);
            this.spinQty.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.spinQty.Name = "spinQty";
            this.spinQty.Size = new System.Drawing.Size(94, 20);
            this.spinQty.TabIndex = 320;
            this.spinQty.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.spinQty.ValueChanged += new System.EventHandler(this.spinQty_ValueChanged);
            this.spinQty.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EntryKeyDown);
            // 
            // spinPrice
            // 
            this.spinPrice.DecimalPlaces = 2;
            this.spinPrice.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.spinPrice.Location = new System.Drawing.Point(70, 109);
            this.spinPrice.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.spinPrice.Name = "spinPrice";
            this.spinPrice.Size = new System.Drawing.Size(94, 20);
            this.spinPrice.TabIndex = 420;
            this.spinPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.spinPrice.ValueChanged += new System.EventHandler(this.spinPrice_ValueChanged);
            this.spinPrice.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EntryKeyDown);
            // 
            // cmbTIF
            // 
            this.cmbTIF.FormattingEnabled = true;
            this.cmbTIF.Location = new System.Drawing.Point(70, 135);
            this.cmbTIF.Name = "cmbTIF";
            this.cmbTIF.Size = new System.Drawing.Size(94, 21);
            this.cmbTIF.TabIndex = 520;
            this.cmbTIF.SelectedIndexChanged += new System.EventHandler(this.cmbTIF_SelectedIndexChanged);
            this.cmbTIF.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EntryKeyDown);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // cmbAlgo
            // 
            this.cmbAlgo.FormattingEnabled = true;
            this.cmbAlgo.Location = new System.Drawing.Point(201, 30);
            this.cmbAlgo.Name = "cmbAlgo";
            this.cmbAlgo.Size = new System.Drawing.Size(94, 21);
            this.cmbAlgo.TabIndex = 522;
            this.cmbAlgo.SelectedIndexChanged += new System.EventHandler(this.cmbAlgo_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(167, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 13);
            this.label1.TabIndex = 521;
            this.label1.Text = "Algo";
            // 
            // grpAlgoPanel
            // 
            this.grpAlgoPanel.Controls.Add(this.pnlAlgoPlaceholder);
            this.grpAlgoPanel.Location = new System.Drawing.Point(170, 56);
            this.grpAlgoPanel.Name = "grpAlgoPanel";
            this.grpAlgoPanel.Size = new System.Drawing.Size(285, 100);
            this.grpAlgoPanel.TabIndex = 523;
            this.grpAlgoPanel.TabStop = false;
            // 
            // pnlAlgoPlaceholder
            // 
            this.pnlAlgoPlaceholder.Location = new System.Drawing.Point(6, 19);
            this.pnlAlgoPlaceholder.Name = "pnlAlgoPlaceholder";
            this.pnlAlgoPlaceholder.Size = new System.Drawing.Size(273, 75);
            this.pnlAlgoPlaceholder.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(386, 164);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(69, 30);
            this.btnCancel.TabIndex = 526;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Visible = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAmend
            // 
            this.btnAmend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAmend.Enabled = false;
            this.btnAmend.Location = new System.Drawing.Point(188, 164);
            this.btnAmend.Name = "btnAmend";
            this.btnAmend.Size = new System.Drawing.Size(69, 30);
            this.btnAmend.TabIndex = 525;
            this.btnAmend.Text = "A&mend";
            this.btnAmend.UseVisualStyleBackColor = true;
            this.btnAmend.Visible = false;
            this.btnAmend.Click += new System.EventHandler(this.btnAmend_Click);
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(13, 164);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(69, 30);
            this.btnSend.TabIndex = 524;
            this.btnSend.Text = "&Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // lblOID
            // 
            this.lblOID.AutoSize = true;
            this.lblOID.Location = new System.Drawing.Point(20, 34);
            this.lblOID.Name = "lblOID";
            this.lblOID.Size = new System.Drawing.Size(44, 13);
            this.lblOID.TabIndex = 527;
            this.lblOID.Text = "OrderID";
            // 
            // txtOID
            // 
            this.txtOID.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOID.Location = new System.Drawing.Point(70, 31);
            this.txtOID.Name = "txtOID";
            this.txtOID.ReadOnly = true;
            this.txtOID.Size = new System.Drawing.Size(94, 20);
            this.txtOID.TabIndex = 528;
            this.txtOID.Text = "0";
            this.txtOID.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // OrderPanelControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtOID);
            this.Controls.Add(this.lblOID);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAmend);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.grpAlgoPanel);
            this.Controls.Add(this.cmbAlgo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbTIF);
            this.Controls.Add(this.spinPrice);
            this.Controls.Add(this.spinQty);
            this.Controls.Add(this.lblTIF);
            this.Controls.Add(this.lblPrice);
            this.Controls.Add(this.lblQty);
            this.Controls.Add(this.optSell);
            this.Controls.Add(this.optBuy);
            this.Controls.Add(this.txtRIC);
            this.Controls.Add(this.lblRIC);
            this.Name = "OrderPanelControl";
            this.Size = new System.Drawing.Size(462, 201);
            this.Load += new System.EventHandler(this.OrderPanelControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.spinQty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPrice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.grpAlgoPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblRIC;
        private System.Windows.Forms.TextBox txtRIC;
        private System.Windows.Forms.RadioButton optBuy;
        private System.Windows.Forms.RadioButton optSell;
        private System.Windows.Forms.Label lblQty;
        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.Label lblTIF;
        private System.Windows.Forms.NumericUpDown spinQty;
        private System.Windows.Forms.NumericUpDown spinPrice;
        private System.Windows.Forms.ComboBox cmbTIF;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.ComboBox cmbAlgo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpAlgoPanel;
        private System.Windows.Forms.Panel pnlAlgoPlaceholder;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAmend;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblOID;
        private System.Windows.Forms.TextBox txtOID;
    }
}
