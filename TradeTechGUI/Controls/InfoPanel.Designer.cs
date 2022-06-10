namespace OPEX.SalesGUI.Controls
{
    partial class InfoPanel
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblStartTime = new System.Windows.Forms.Label();
            this.lblEndTime = new System.Windows.Forms.Label();
            this.lblOrders = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblPNL = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblTimeToEnd = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();            
            this.pnlMarket = new System.Windows.Forms.Panel();
            this.lblMarket = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblOrdersExecuted = new System.Windows.Forms.Label();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblPNLCurrent = new System.Windows.Forms.Label();
            this.pnlMarket.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Current trading period start";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Current trading period end";
            // 
            // lblStartTime
            // 
            this.lblStartTime.AutoSize = true;
            this.lblStartTime.Location = new System.Drawing.Point(140, 56);
            this.lblStartTime.Name = "lblStartTime";
            this.lblStartTime.Size = new System.Drawing.Size(49, 13);
            this.lblStartTime.TabIndex = 2;
            this.lblStartTime.Text = "00:00:00";
            // 
            // lblEndTime
            // 
            this.lblEndTime.AutoSize = true;
            this.lblEndTime.Location = new System.Drawing.Point(140, 42);
            this.lblEndTime.Name = "lblEndTime";
            this.lblEndTime.Size = new System.Drawing.Size(49, 13);
            this.lblEndTime.TabIndex = 3;
            this.lblEndTime.Text = "00:00:00";
            // 
            // lblOrders
            // 
            this.lblOrders.AutoSize = true;
            this.lblOrders.Location = new System.Drawing.Point(120, 82);
            this.lblOrders.Name = "lblOrders";
            this.lblOrders.Size = new System.Drawing.Size(13, 13);
            this.lblOrders.TabIndex = 5;
            this.lblOrders.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(3, 82);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Orders";
            // 
            // lblPNL
            // 
            this.lblPNL.AutoSize = true;
            this.lblPNL.Location = new System.Drawing.Point(212, 95);
            this.lblPNL.Name = "lblPNL";
            this.lblPNL.Size = new System.Drawing.Size(19, 13);
            this.lblPNL.TabIndex = 7;
            this.lblPNL.Text = "£0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(3, 95);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "PNL";
            // 
            // lblTimeToEnd
            // 
            this.lblTimeToEnd.AutoSize = true;
            this.lblTimeToEnd.Location = new System.Drawing.Point(140, 28);
            this.lblTimeToEnd.Name = "lblTimeToEnd";
            this.lblTimeToEnd.Size = new System.Drawing.Size(49, 13);
            this.lblTimeToEnd.TabIndex = 9;
            this.lblTimeToEnd.Text = "00:00:00";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 28);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(107, 13);
            this.label10.TabIndex = 8;
            this.label10.Text = "Time to end of period";
            // 
            // pnlMarket
            // 
            this.pnlMarket.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlMarket.BackColor = System.Drawing.SystemColors.Control;
            this.pnlMarket.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlMarket.Controls.Add(this.lblMarket);
            this.pnlMarket.Controls.Add(this.lblTimeToEnd);
            this.pnlMarket.Controls.Add(this.label1);
            this.pnlMarket.Controls.Add(this.label10);
            this.pnlMarket.Controls.Add(this.lblStartTime);
            this.pnlMarket.Controls.Add(this.lblEndTime);
            this.pnlMarket.Controls.Add(this.label2);
            this.pnlMarket.Location = new System.Drawing.Point(3, 4);
            this.pnlMarket.Name = "pnlMarket";
            this.pnlMarket.Size = new System.Drawing.Size(272, 75);
            this.pnlMarket.TabIndex = 10;
            // 
            // lblMarket
            // 
            this.lblMarket.AutoSize = true;
            this.lblMarket.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMarket.Location = new System.Drawing.Point(3, 3);
            this.lblMarket.Name = "lblMarket";
            this.lblMarket.Size = new System.Drawing.Size(160, 22);
            this.lblMarket.TabIndex = 1;
            this.lblMarket.Text = "Market CLOSED";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(157, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Executed";
            // 
            // lblOrdersExecuted
            // 
            this.lblOrdersExecuted.AutoSize = true;
            this.lblOrdersExecuted.Location = new System.Drawing.Point(218, 82);
            this.lblOrdersExecuted.Name = "lblOrdersExecuted";
            this.lblOrdersExecuted.Size = new System.Drawing.Size(13, 13);
            this.lblOrdersExecuted.TabIndex = 12;
            this.lblOrdersExecuted.Text = "0";
            // 
            // txtMessage
            // 
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.BackColor = System.Drawing.Color.Blue;
            this.txtMessage.Font = new System.Drawing.Font("Lucida Console", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessage.ForeColor = System.Drawing.Color.Gold;
            this.txtMessage.Location = new System.Drawing.Point(3, 111);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.Size = new System.Drawing.Size(272, 48);
            this.txtMessage.TabIndex = 13;
            this.txtMessage.Text = "You have made a new trade!\r\nCheck the Trade Blotter";
            this.txtMessage.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(58, 82);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Remaining";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(58, 95);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Current";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(157, 95);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(31, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Total";
            // 
            // lblPNLCurrent
            // 
            this.lblPNLCurrent.AutoSize = true;
            this.lblPNLCurrent.Location = new System.Drawing.Point(114, 95);
            this.lblPNLCurrent.Name = "lblPNLCurrent";
            this.lblPNLCurrent.Size = new System.Drawing.Size(19, 13);
            this.lblPNLCurrent.TabIndex = 18;
            this.lblPNLCurrent.Text = "£0";
            // 
            // InfoPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblPNLCurrent);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblOrdersExecuted);
            this.Controls.Add(this.lblPNL);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lblOrders);
            this.Controls.Add(this.pnlMarket);
            this.Name = "InfoPanel";
            this.Size = new System.Drawing.Size(278, 162);
            this.pnlMarket.ResumeLayout(false);
            this.pnlMarket.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.Label lblEndTime;
        private System.Windows.Forms.Label lblOrders;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblPNL;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblTimeToEnd;
        private System.Windows.Forms.Label label10;        
        private System.Windows.Forms.Panel pnlMarket;
        private System.Windows.Forms.Label lblMarket;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblOrdersExecuted;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblPNLCurrent;
    }
}
