namespace OPEX.GUICommon
{
    partial class ChartPanel
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
            this.zgc1 = new ZedGraph.ZedGraphControl();
            this.SuspendLayout();
            // 
            // zgc1
            // 
            this.zgc1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.zgc1.Location = new System.Drawing.Point(3, 3);
            this.zgc1.Name = "zgc1";
            this.zgc1.ScrollGrace = 0;
            this.zgc1.ScrollMaxX = 0;
            this.zgc1.ScrollMaxY = 0;
            this.zgc1.ScrollMaxY2 = 0;
            this.zgc1.ScrollMinX = 0;
            this.zgc1.ScrollMinY = 0;
            this.zgc1.ScrollMinY2 = 0;
            this.zgc1.Size = new System.Drawing.Size(406, 243);
            this.zgc1.TabIndex = 0;
            // 
            // ChartPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Red;
            this.Controls.Add(this.zgc1);
            this.Name = "ChartPanel";
            this.Size = new System.Drawing.Size(412, 249);
            this.Load += new System.EventHandler(this.ChartPanel_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ZedGraph.ZedGraphControl zgc1;
    }
}
