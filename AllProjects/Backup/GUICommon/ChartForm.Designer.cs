namespace OPEX.GUICommon
{
    partial class ChartForm
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
            this.chartPanel1 = new OPEX.GUICommon.ChartPanel();
            this.SuspendLayout();
            // 
            // chartPanel1
            // 
            this.chartPanel1.BackColor = System.Drawing.Color.Red;
            this.chartPanel1.ChartTitle = "Title";
            this.chartPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartPanel1.Location = new System.Drawing.Point(0, 0);
            this.chartPanel1.Name = "chartPanel1";
            this.chartPanel1.Size = new System.Drawing.Size(462, 288);
            this.chartPanel1.TabIndex = 0;
            this.chartPanel1.XAxisTitle = "X Axis";
            this.chartPanel1.XMax = 180;
            this.chartPanel1.YAxisTitle = "Y Axis";
            this.chartPanel1.YMax = 400;
            // 
            // ChartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(462, 288);
            this.Controls.Add(this.chartPanel1);
            this.Name = "ChartForm";
            this.Text = "ChartForm";
            this.ResumeLayout(false);

        }

        #endregion

        private ChartPanel chartPanel1;
    }
}