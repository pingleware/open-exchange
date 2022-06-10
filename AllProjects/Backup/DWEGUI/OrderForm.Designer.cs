namespace OPEX.DWEGUI
{
    partial class OrderForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrderForm));
            this.orderPanel = new OPEX.DWEGUI.OrderPanelControl();
            this.chkCloseOnSend = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // orderPanel
            // 
            this.orderPanel.BackColor = System.Drawing.SystemColors.Control;
            this.orderPanel.Location = new System.Drawing.Point(5, 6);
            this.orderPanel.MinimumSize = new System.Drawing.Size(159, 196);
            this.orderPanel.Name = "orderPanel";
            this.orderPanel.Order = null;
            this.orderPanel.Size = new System.Drawing.Size(159, 196);
            this.orderPanel.TabIndex = 0;
            this.orderPanel.SendButtonPressed += new System.EventHandler(this.orderPanel_SendButtonPressed);
            this.orderPanel.AmendButtonPressed += new OPEX.DWEGUI.AmendButtonPressedEventHandler(this.orderPanel_AmendButtonPressed);
            this.orderPanel.CancelButtonPressed += new OPEX.DWEGUI.AmendButtonPressedEventHandler(this.orderPanel_CancelButtonPressed);
            // 
            // chkCloseOnSend
            // 
            this.chkCloseOnSend.AutoSize = true;
            this.chkCloseOnSend.Checked = true;
            this.chkCloseOnSend.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCloseOnSend.Location = new System.Drawing.Point(12, 214);
            this.chkCloseOnSend.Name = "chkCloseOnSend";
            this.chkCloseOnSend.Size = new System.Drawing.Size(136, 17);
            this.chkCloseOnSend.TabIndex = 1;
            this.chkCloseOnSend.Text = "Close after order is sent";
            this.chkCloseOnSend.UseVisualStyleBackColor = true;
            this.chkCloseOnSend.CheckedChanged += new System.EventHandler(this.chkCloseOnSend_CheckedChanged);
            // 
            // OrderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(169, 243);
            this.Controls.Add(this.chkCloseOnSend);
            this.Controls.Add(this.orderPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OrderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "New Order";
            this.Load += new System.EventHandler(this.OrderForm_Load);
            this.Shown += new System.EventHandler(this.OrderForm_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OrderForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OrderPanelControl orderPanel;
        private System.Windows.Forms.CheckBox chkCloseOnSend;
    }
}