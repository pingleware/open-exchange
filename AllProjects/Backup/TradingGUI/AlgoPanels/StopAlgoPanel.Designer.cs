namespace OPEX.TradingGUI.AlgoPanels
{
    partial class StopAlgoPanel
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
            this.spinPrice = new System.Windows.Forms.NumericUpDown();
            this.lblStopPrice = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.spinPrice)).BeginInit();
            this.SuspendLayout();
            // 
            // spinPrice
            // 
            this.spinPrice.DecimalPlaces = 2;
            this.spinPrice.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.spinPrice.Location = new System.Drawing.Point(63, 3);
            this.spinPrice.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.spinPrice.Name = "spinPrice";
            this.spinPrice.Size = new System.Drawing.Size(94, 20);
            this.spinPrice.TabIndex = 422;
            this.spinPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblStopPrice
            // 
            this.lblStopPrice.AutoSize = true;
            this.lblStopPrice.Location = new System.Drawing.Point(4, 5);
            this.lblStopPrice.Name = "lblStopPrice";
            this.lblStopPrice.Size = new System.Drawing.Size(56, 13);
            this.lblStopPrice.TabIndex = 421;
            this.lblStopPrice.Text = "Stop Price";
            // 
            // StopAlgoPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.spinPrice);
            this.Controls.Add(this.lblStopPrice);
            this.Name = "StopAlgoPanel";
            this.Size = new System.Drawing.Size(162, 159);
            ((System.ComponentModel.ISupportInitialize)(this.spinPrice)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown spinPrice;
        private System.Windows.Forms.Label lblStopPrice;
    }
}
