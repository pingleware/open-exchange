namespace OPEX.TradingGUI
{
    partial class OrderGeneratorDialog
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
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.orderGeneratorPanelControl1 = new OPEX.TradingGUI.OrderGeneratorPanelControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.orderGeneratorPanelControl2 = new OPEX.TradingGUI.OrderGeneratorPanelControl();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(363, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(87, 43);
            this.button2.TabIndex = 10;
            this.button2.Text = "Switch all OFF";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(246, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(87, 43);
            this.button1.TabIndex = 9;
            this.button1.Text = "Switch all ON";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(8, 61);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(447, 320);
            this.tabControl1.TabIndex = 8;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.orderGeneratorPanelControl1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(439, 294);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "OrderGenerator 1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // orderGeneratorPanelControl1
            // 
            this.orderGeneratorPanelControl1.Location = new System.Drawing.Point(6, 6);
            this.orderGeneratorPanelControl1.Name = "orderGeneratorPanelControl1";
            this.orderGeneratorPanelControl1.PeriodFixed = false;
            this.orderGeneratorPanelControl1.PeriodFrom = 0;
            this.orderGeneratorPanelControl1.PeriodTo = 0;
            this.orderGeneratorPanelControl1.PriceFixed = false;
            this.orderGeneratorPanelControl1.PriceFrom = 0;
            this.orderGeneratorPanelControl1.PriceStep = 0;
            this.orderGeneratorPanelControl1.PriceTo = 0;
            this.orderGeneratorPanelControl1.QtyFixed = false;
            this.orderGeneratorPanelControl1.QtyFrom = 0;
            this.orderGeneratorPanelControl1.QtyStep = 0;
            this.orderGeneratorPanelControl1.QtyTo = 0;
            this.orderGeneratorPanelControl1.RIC = "";
            this.orderGeneratorPanelControl1.Running = false;
            this.orderGeneratorPanelControl1.Side = OPEX.OM.Common.OrderSide.Sell;
            this.orderGeneratorPanelControl1.Size = new System.Drawing.Size(414, 282);
            this.orderGeneratorPanelControl1.TabIndex = 3;
            this.orderGeneratorPanelControl1.TIF = OPEX.OM.Common.OrderType.Limit;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.orderGeneratorPanelControl2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(439, 294);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "OrderGenerator 2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // orderGeneratorPanelControl2
            // 
            this.orderGeneratorPanelControl2.Location = new System.Drawing.Point(4, 6);
            this.orderGeneratorPanelControl2.Name = "orderGeneratorPanelControl2";
            this.orderGeneratorPanelControl2.PeriodFixed = false;
            this.orderGeneratorPanelControl2.PeriodFrom = 0;
            this.orderGeneratorPanelControl2.PeriodTo = 0;
            this.orderGeneratorPanelControl2.PriceFixed = false;
            this.orderGeneratorPanelControl2.PriceFrom = 0;
            this.orderGeneratorPanelControl2.PriceStep = 0;
            this.orderGeneratorPanelControl2.PriceTo = 0;
            this.orderGeneratorPanelControl2.QtyFixed = false;
            this.orderGeneratorPanelControl2.QtyFrom = 0;
            this.orderGeneratorPanelControl2.QtyStep = 0;
            this.orderGeneratorPanelControl2.QtyTo = 0;
            this.orderGeneratorPanelControl2.RIC = "";
            this.orderGeneratorPanelControl2.Running = false;
            this.orderGeneratorPanelControl2.Side = OPEX.OM.Common.OrderSide.Sell;
            this.orderGeneratorPanelControl2.Size = new System.Drawing.Size(414, 282);
            this.orderGeneratorPanelControl2.TabIndex = 4;
            this.orderGeneratorPanelControl2.TIF = OPEX.OM.Common.OrderType.Market;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 12);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(87, 43);
            this.button3.TabIndex = 11;
            this.button3.Text = "Setup";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(129, 12);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(87, 43);
            this.button4.TabIndex = 12;
            this.button4.Text = "Clear";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // OrderGeneratorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 388);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OrderGeneratorDialog";
            this.Text = "Order Generator";
            this.Load += new System.EventHandler(this.OrderGeneratorDialog_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OrderGeneratorDialog_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private OrderGeneratorPanelControl orderGeneratorPanelControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private OrderGeneratorPanelControl orderGeneratorPanelControl2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;

    }
}