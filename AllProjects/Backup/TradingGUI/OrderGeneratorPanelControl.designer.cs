namespace OPEX.TradingGUI
{
    partial class OrderGeneratorPanelControl
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
            this.cmbTIF = new System.Windows.Forms.ComboBox();
            this.lblTIF = new System.Windows.Forms.Label();
            this.optSell = new System.Windows.Forms.RadioButton();
            this.optBuy = new System.Windows.Forms.RadioButton();
            this.txtRIC = new System.Windows.Forms.TextBox();
            this.lblRIC = new System.Windows.Forms.Label();
            this.grpPrice = new System.Windows.Forms.GroupBox();
            this.spinPriceFixed = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.spinPriceStep = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.spinPriceTo = new System.Windows.Forms.NumericUpDown();
            this.lblRandomPriceFrom = new System.Windows.Forms.Label();
            this.spinPriceFrom = new System.Windows.Forms.NumericUpDown();
            this.optPriceRandom = new System.Windows.Forms.RadioButton();
            this.optPriceFixed = new System.Windows.Forms.RadioButton();
            this.grpQty = new System.Windows.Forms.GroupBox();
            this.spinQtyFixed = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.spinQtyStep = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.spinQtyTo = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.spinQtyFrom = new System.Windows.Forms.NumericUpDown();
            this.optQtyRandom = new System.Windows.Forms.RadioButton();
            this.optQtyFixed = new System.Windows.Forms.RadioButton();
            this.grpPeriod = new System.Windows.Forms.GroupBox();
            this.spinPeriodFixed = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.spinPeriodTo = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.spinPeriodFrom = new System.Windows.Forms.NumericUpDown();
            this.optPeriodRandom = new System.Windows.Forms.RadioButton();
            this.optPeriodFixed = new System.Windows.Forms.RadioButton();
            this.chkOnOff = new System.Windows.Forms.CheckBox();
            this.grpPrice.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinPriceFixed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPriceStep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPriceTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPriceFrom)).BeginInit();
            this.grpQty.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinQtyFixed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinQtyStep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinQtyTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinQtyFrom)).BeginInit();
            this.grpPeriod.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinPeriodFixed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPeriodTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPeriodFrom)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbTIF
            // 
            this.cmbTIF.FormattingEnabled = true;
            this.cmbTIF.Location = new System.Drawing.Point(163, 7);
            this.cmbTIF.Name = "cmbTIF";
            this.cmbTIF.Size = new System.Drawing.Size(94, 21);
            this.cmbTIF.TabIndex = 526;
            // 
            // lblTIF
            // 
            this.lblTIF.AutoSize = true;
            this.lblTIF.Location = new System.Drawing.Point(134, 10);
            this.lblTIF.Name = "lblTIF";
            this.lblTIF.Size = new System.Drawing.Size(23, 13);
            this.lblTIF.TabIndex = 525;
            this.lblTIF.Text = "TIF";
            // 
            // optSell
            // 
            this.optSell.AutoSize = true;
            this.optSell.Location = new System.Drawing.Point(359, 8);
            this.optSell.Name = "optSell";
            this.optSell.Size = new System.Drawing.Size(42, 17);
            this.optSell.TabIndex = 522;
            this.optSell.Text = "Sell";
            this.optSell.UseVisualStyleBackColor = true;
            // 
            // optBuy
            // 
            this.optBuy.AutoSize = true;
            this.optBuy.Location = new System.Drawing.Point(307, 8);
            this.optBuy.Name = "optBuy";
            this.optBuy.Size = new System.Drawing.Size(43, 17);
            this.optBuy.TabIndex = 521;
            this.optBuy.Text = "Buy";
            this.optBuy.UseVisualStyleBackColor = true;
            // 
            // txtRIC
            // 
            this.txtRIC.Location = new System.Drawing.Point(34, 7);
            this.txtRIC.Name = "txtRIC";
            this.txtRIC.Size = new System.Drawing.Size(94, 20);
            this.txtRIC.TabIndex = 524;
            this.txtRIC.Text = "";
            // 
            // lblRIC
            // 
            this.lblRIC.AutoSize = true;
            this.lblRIC.Location = new System.Drawing.Point(3, 10);
            this.lblRIC.Name = "lblRIC";
            this.lblRIC.Size = new System.Drawing.Size(25, 13);
            this.lblRIC.TabIndex = 523;
            this.lblRIC.Text = "RIC";
            // 
            // grpPrice
            // 
            this.grpPrice.Controls.Add(this.spinPriceFixed);
            this.grpPrice.Controls.Add(this.label2);
            this.grpPrice.Controls.Add(this.spinPriceStep);
            this.grpPrice.Controls.Add(this.label1);
            this.grpPrice.Controls.Add(this.spinPriceTo);
            this.grpPrice.Controls.Add(this.lblRandomPriceFrom);
            this.grpPrice.Controls.Add(this.spinPriceFrom);
            this.grpPrice.Controls.Add(this.optPriceRandom);
            this.grpPrice.Controls.Add(this.optPriceFixed);
            this.grpPrice.Location = new System.Drawing.Point(6, 34);
            this.grpPrice.Name = "grpPrice";
            this.grpPrice.Size = new System.Drawing.Size(408, 69);
            this.grpPrice.TabIndex = 527;
            this.grpPrice.TabStop = false;
            this.grpPrice.Text = "Price";
            // 
            // spinPriceFixed
            // 
            this.spinPriceFixed.DecimalPlaces = 2;
            this.spinPriceFixed.Increment = new decimal(new int[] {
            10,
            0,
            0,
            131072});
            this.spinPriceFixed.Location = new System.Drawing.Point(118, 16);
            this.spinPriceFixed.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.spinPriceFixed.Name = "spinPriceFixed";
            this.spinPriceFixed.Size = new System.Drawing.Size(68, 20);
            this.spinPriceFixed.TabIndex = 8;
            this.spinPriceFixed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.spinPriceFixed.ThousandsSeparator = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(299, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "step";
            // 
            // spinPriceStep
            // 
            this.spinPriceStep.DecimalPlaces = 2;
            this.spinPriceStep.Increment = new decimal(new int[] {
            10,
            0,
            0,
            131072});
            this.spinPriceStep.Location = new System.Drawing.Point(332, 39);
            this.spinPriceStep.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.spinPriceStep.Name = "spinPriceStep";
            this.spinPriceStep.Size = new System.Drawing.Size(68, 20);
            this.spinPriceStep.TabIndex = 6;
            this.spinPriceStep.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.spinPriceStep.ThousandsSeparator = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(192, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(16, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "to";
            // 
            // spinPriceTo
            // 
            this.spinPriceTo.DecimalPlaces = 2;
            this.spinPriceTo.Increment = new decimal(new int[] {
            10,
            0,
            0,
            131072});
            this.spinPriceTo.Location = new System.Drawing.Point(225, 39);
            this.spinPriceTo.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.spinPriceTo.Name = "spinPriceTo";
            this.spinPriceTo.Size = new System.Drawing.Size(68, 20);
            this.spinPriceTo.TabIndex = 4;
            this.spinPriceTo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.spinPriceTo.ThousandsSeparator = true;
            // 
            // lblRandomPriceFrom
            // 
            this.lblRandomPriceFrom.AutoSize = true;
            this.lblRandomPriceFrom.Location = new System.Drawing.Point(85, 44);
            this.lblRandomPriceFrom.Name = "lblRandomPriceFrom";
            this.lblRandomPriceFrom.Size = new System.Drawing.Size(27, 13);
            this.lblRandomPriceFrom.TabIndex = 3;
            this.lblRandomPriceFrom.Text = "from";
            // 
            // spinPriceFrom
            // 
            this.spinPriceFrom.DecimalPlaces = 2;
            this.spinPriceFrom.Increment = new decimal(new int[] {
            10,
            0,
            0,
            131072});
            this.spinPriceFrom.Location = new System.Drawing.Point(118, 39);
            this.spinPriceFrom.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.spinPriceFrom.Name = "spinPriceFrom";
            this.spinPriceFrom.Size = new System.Drawing.Size(68, 20);
            this.spinPriceFrom.TabIndex = 2;
            this.spinPriceFrom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.spinPriceFrom.ThousandsSeparator = true;
            // 
            // optPriceRandom
            // 
            this.optPriceRandom.AutoSize = true;
            this.optPriceRandom.Location = new System.Drawing.Point(6, 42);
            this.optPriceRandom.Name = "optPriceRandom";
            this.optPriceRandom.Size = new System.Drawing.Size(65, 17);
            this.optPriceRandom.TabIndex = 1;
            this.optPriceRandom.TabStop = true;
            this.optPriceRandom.Text = "Random";
            this.optPriceRandom.UseVisualStyleBackColor = true;
            // 
            // optPriceFixed
            // 
            this.optPriceFixed.AutoSize = true;
            this.optPriceFixed.Location = new System.Drawing.Point(6, 19);
            this.optPriceFixed.Name = "optPriceFixed";
            this.optPriceFixed.Size = new System.Drawing.Size(50, 17);
            this.optPriceFixed.TabIndex = 0;
            this.optPriceFixed.TabStop = true;
            this.optPriceFixed.Text = "Fixed";
            this.optPriceFixed.UseVisualStyleBackColor = true;
            // 
            // grpQty
            // 
            this.grpQty.Controls.Add(this.spinQtyFixed);
            this.grpQty.Controls.Add(this.label3);
            this.grpQty.Controls.Add(this.spinQtyStep);
            this.grpQty.Controls.Add(this.label4);
            this.grpQty.Controls.Add(this.spinQtyTo);
            this.grpQty.Controls.Add(this.label5);
            this.grpQty.Controls.Add(this.spinQtyFrom);
            this.grpQty.Controls.Add(this.optQtyRandom);
            this.grpQty.Controls.Add(this.optQtyFixed);
            this.grpQty.Location = new System.Drawing.Point(6, 109);
            this.grpQty.Name = "grpQty";
            this.grpQty.Size = new System.Drawing.Size(408, 69);
            this.grpQty.TabIndex = 528;
            this.grpQty.TabStop = false;
            this.grpQty.Text = "Quantity";
            // 
            // spinQtyFixed
            // 
            this.spinQtyFixed.Location = new System.Drawing.Point(118, 16);
            this.spinQtyFixed.Maximum = new decimal(new int[] {
            -1539607552,
            11,
            0,
            0});
            this.spinQtyFixed.Name = "spinQtyFixed";
            this.spinQtyFixed.Size = new System.Drawing.Size(68, 20);
            this.spinQtyFixed.TabIndex = 8;
            this.spinQtyFixed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(299, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "step";
            // 
            // spinQtyStep
            // 
            this.spinQtyStep.Location = new System.Drawing.Point(332, 39);
            this.spinQtyStep.Maximum = new decimal(new int[] {
            -1539607552,
            11,
            0,
            0});
            this.spinQtyStep.Name = "spinQtyStep";
            this.spinQtyStep.Size = new System.Drawing.Size(68, 20);
            this.spinQtyStep.TabIndex = 6;
            this.spinQtyStep.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(192, 44);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(16, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "to";
            // 
            // spinQtyTo
            // 
            this.spinQtyTo.Location = new System.Drawing.Point(225, 39);
            this.spinQtyTo.Maximum = new decimal(new int[] {
            -1539607552,
            11,
            0,
            0});
            this.spinQtyTo.Name = "spinQtyTo";
            this.spinQtyTo.Size = new System.Drawing.Size(68, 20);
            this.spinQtyTo.TabIndex = 4;
            this.spinQtyTo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(85, 44);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(27, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "from";
            // 
            // spinQtyFrom
            // 
            this.spinQtyFrom.Location = new System.Drawing.Point(118, 39);
            this.spinQtyFrom.Maximum = new decimal(new int[] {
            -1539607552,
            11,
            0,
            0});
            this.spinQtyFrom.Name = "spinQtyFrom";
            this.spinQtyFrom.Size = new System.Drawing.Size(68, 20);
            this.spinQtyFrom.TabIndex = 2;
            this.spinQtyFrom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // optQtyRandom
            // 
            this.optQtyRandom.AutoSize = true;
            this.optQtyRandom.Location = new System.Drawing.Point(6, 42);
            this.optQtyRandom.Name = "optQtyRandom";
            this.optQtyRandom.Size = new System.Drawing.Size(65, 17);
            this.optQtyRandom.TabIndex = 1;
            this.optQtyRandom.TabStop = true;
            this.optQtyRandom.Text = "Random";
            this.optQtyRandom.UseVisualStyleBackColor = true;
            // 
            // optQtyFixed
            // 
            this.optQtyFixed.AutoSize = true;
            this.optQtyFixed.Location = new System.Drawing.Point(6, 19);
            this.optQtyFixed.Name = "optQtyFixed";
            this.optQtyFixed.Size = new System.Drawing.Size(50, 17);
            this.optQtyFixed.TabIndex = 0;
            this.optQtyFixed.TabStop = true;
            this.optQtyFixed.Text = "Fixed";
            this.optQtyFixed.UseVisualStyleBackColor = true;
            // 
            // grpPeriod
            // 
            this.grpPeriod.Controls.Add(this.spinPeriodFixed);
            this.grpPeriod.Controls.Add(this.label7);
            this.grpPeriod.Controls.Add(this.spinPeriodTo);
            this.grpPeriod.Controls.Add(this.label8);
            this.grpPeriod.Controls.Add(this.spinPeriodFrom);
            this.grpPeriod.Controls.Add(this.optPeriodRandom);
            this.grpPeriod.Controls.Add(this.optPeriodFixed);
            this.grpPeriod.Location = new System.Drawing.Point(6, 184);
            this.grpPeriod.Name = "grpPeriod";
            this.grpPeriod.Size = new System.Drawing.Size(408, 69);
            this.grpPeriod.TabIndex = 528;
            this.grpPeriod.TabStop = false;
            this.grpPeriod.Text = "Period";
            // 
            // spinPeriodFixed
            // 
            this.spinPeriodFixed.Location = new System.Drawing.Point(118, 16);
            this.spinPeriodFixed.Maximum = new decimal(new int[] {
            -1539607552,
            11,
            0,
            0});
            this.spinPeriodFixed.Name = "spinPeriodFixed";
            this.spinPeriodFixed.Size = new System.Drawing.Size(68, 20);
            this.spinPeriodFixed.TabIndex = 8;
            this.spinPeriodFixed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(192, 44);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(16, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "to";
            // 
            // spinPeriodTo
            // 
            this.spinPeriodTo.Location = new System.Drawing.Point(225, 39);
            this.spinPeriodTo.Maximum = new decimal(new int[] {
            -1539607552,
            11,
            0,
            0});
            this.spinPeriodTo.Name = "spinPeriodTo";
            this.spinPeriodTo.Size = new System.Drawing.Size(68, 20);
            this.spinPeriodTo.TabIndex = 4;
            this.spinPeriodTo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(85, 44);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(27, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "from";
            // 
            // spinPeriodFrom
            // 
            this.spinPeriodFrom.Location = new System.Drawing.Point(118, 39);
            this.spinPeriodFrom.Maximum = new decimal(new int[] {
            -1539607552,
            11,
            0,
            0});
            this.spinPeriodFrom.Name = "spinPeriodFrom";
            this.spinPeriodFrom.Size = new System.Drawing.Size(68, 20);
            this.spinPeriodFrom.TabIndex = 2;
            this.spinPeriodFrom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // optPeriodRandom
            // 
            this.optPeriodRandom.AutoSize = true;
            this.optPeriodRandom.Location = new System.Drawing.Point(6, 42);
            this.optPeriodRandom.Name = "optPeriodRandom";
            this.optPeriodRandom.Size = new System.Drawing.Size(65, 17);
            this.optPeriodRandom.TabIndex = 1;
            this.optPeriodRandom.TabStop = true;
            this.optPeriodRandom.Text = "Random";
            this.optPeriodRandom.UseVisualStyleBackColor = true;
            // 
            // optPeriodFixed
            // 
            this.optPeriodFixed.AutoSize = true;
            this.optPeriodFixed.Location = new System.Drawing.Point(6, 19);
            this.optPeriodFixed.Name = "optPeriodFixed";
            this.optPeriodFixed.Size = new System.Drawing.Size(50, 17);
            this.optPeriodFixed.TabIndex = 0;
            this.optPeriodFixed.TabStop = true;
            this.optPeriodFixed.Text = "Fixed";
            this.optPeriodFixed.UseVisualStyleBackColor = true;
            // 
            // chkOnOff
            // 
            this.chkOnOff.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkOnOff.AutoSize = true;
            this.chkOnOff.Location = new System.Drawing.Point(6, 259);
            this.chkOnOff.Name = "chkOnOff";
            this.chkOnOff.Size = new System.Drawing.Size(31, 23);
            this.chkOnOff.TabIndex = 529;
            this.chkOnOff.Text = "Off";
            this.chkOnOff.UseVisualStyleBackColor = true;
            this.chkOnOff.CheckedChanged += new System.EventHandler(this.chkOnOff_CheckedChanged);
            // 
            // OrderGeneratorPanelControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkOnOff);
            this.Controls.Add(this.grpPeriod);
            this.Controls.Add(this.grpQty);
            this.Controls.Add(this.grpPrice);
            this.Controls.Add(this.cmbTIF);
            this.Controls.Add(this.lblTIF);
            this.Controls.Add(this.optSell);
            this.Controls.Add(this.optBuy);
            this.Controls.Add(this.txtRIC);
            this.Controls.Add(this.lblRIC);
            this.Name = "OrderGeneratorPanelControl";
            this.Size = new System.Drawing.Size(420, 291);
            this.grpPrice.ResumeLayout(false);
            this.grpPrice.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinPriceFixed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPriceStep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPriceTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPriceFrom)).EndInit();
            this.grpQty.ResumeLayout(false);
            this.grpQty.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinQtyFixed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinQtyStep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinQtyTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinQtyFrom)).EndInit();
            this.grpPeriod.ResumeLayout(false);
            this.grpPeriod.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinPeriodFixed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPeriodTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPeriodFrom)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbTIF;
        private System.Windows.Forms.Label lblTIF;
        private System.Windows.Forms.RadioButton optSell;
        private System.Windows.Forms.RadioButton optBuy;
        private System.Windows.Forms.TextBox txtRIC;
        private System.Windows.Forms.Label lblRIC;
        private System.Windows.Forms.GroupBox grpPrice;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown spinPriceTo;
        private System.Windows.Forms.Label lblRandomPriceFrom;
        private System.Windows.Forms.NumericUpDown spinPriceFrom;
        private System.Windows.Forms.RadioButton optPriceRandom;
        private System.Windows.Forms.RadioButton optPriceFixed;
        private System.Windows.Forms.NumericUpDown spinPriceFixed;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown spinPriceStep;
        private System.Windows.Forms.GroupBox grpQty;
        private System.Windows.Forms.NumericUpDown spinQtyFixed;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown spinQtyStep;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown spinQtyTo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown spinQtyFrom;
        private System.Windows.Forms.RadioButton optQtyRandom;
        private System.Windows.Forms.RadioButton optQtyFixed;
        private System.Windows.Forms.GroupBox grpPeriod;
        private System.Windows.Forms.NumericUpDown spinPeriodFixed;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown spinPeriodTo;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown spinPeriodFrom;
        private System.Windows.Forms.RadioButton optPeriodRandom;
        private System.Windows.Forms.RadioButton optPeriodFixed;
        private System.Windows.Forms.CheckBox chkOnOff;
    }
}
