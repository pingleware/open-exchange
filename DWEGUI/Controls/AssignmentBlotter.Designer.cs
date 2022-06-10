namespace OPEX.DWEGUI.Controls
{
    partial class AssignmentBlotter
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dataSetPermitsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dataSetPermits = new OPEX.DWEGUI.Data.DataSetPermits();
            this.Side = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.quantityDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RIC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.limitPriceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CCY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.QuantityFilled = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.quantityOnMarketDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.remainingQuantityDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetPermitsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetPermits)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Side,
            this.quantityDataGridViewTextBoxColumn,
            this.RIC,
            this.limitPriceDataGridViewTextBoxColumn,
            this.CCY,
            this.QuantityFilled,
            this.quantityOnMarketDataGridViewTextBoxColumn,
            this.remainingQuantityDataGridViewTextBoxColumn});
            this.dataGridView1.DataSource = this.dataSetPermitsBindingSource;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(461, 109);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseDoubleClick);
            // 
            // dataSetPermitsBindingSource
            // 
            this.dataSetPermitsBindingSource.DataMember = "DataTablePermits";
            this.dataSetPermitsBindingSource.DataSource = this.dataSetPermits;
            // 
            // dataSetPermits
            // 
            this.dataSetPermits.DataSetName = "DataSetPermits";
            this.dataSetPermits.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // Side
            // 
            this.Side.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Side.DataPropertyName = "Side";
            this.Side.HeaderText = "Side";
            this.Side.MinimumWidth = 40;
            this.Side.Name = "Side";
            this.Side.ReadOnly = true;
            this.Side.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Side.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // quantityDataGridViewTextBoxColumn
            // 
            this.quantityDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.quantityDataGridViewTextBoxColumn.DataPropertyName = "Quantity";
            this.quantityDataGridViewTextBoxColumn.HeaderText = "Qty";
            this.quantityDataGridViewTextBoxColumn.MinimumWidth = 35;
            this.quantityDataGridViewTextBoxColumn.Name = "quantityDataGridViewTextBoxColumn";
            this.quantityDataGridViewTextBoxColumn.ReadOnly = true;
            this.quantityDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.quantityDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // RIC
            // 
            this.RIC.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.RIC.DataPropertyName = "RIC";
            this.RIC.HeaderText = "RIC";
            this.RIC.MinimumWidth = 35;
            this.RIC.Name = "RIC";
            this.RIC.ReadOnly = true;
            this.RIC.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.RIC.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // limitPriceDataGridViewTextBoxColumn
            // 
            this.limitPriceDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.limitPriceDataGridViewTextBoxColumn.DataPropertyName = "LimitPrice";
            this.limitPriceDataGridViewTextBoxColumn.HeaderText = "LmtPrc";
            this.limitPriceDataGridViewTextBoxColumn.MinimumWidth = 50;
            this.limitPriceDataGridViewTextBoxColumn.Name = "limitPriceDataGridViewTextBoxColumn";
            this.limitPriceDataGridViewTextBoxColumn.ReadOnly = true;
            this.limitPriceDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.limitPriceDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // CCY
            // 
            this.CCY.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.CCY.DataPropertyName = "CCY";
            this.CCY.HeaderText = "CCY";
            this.CCY.MinimumWidth = 50;
            this.CCY.Name = "CCY";
            this.CCY.ReadOnly = true;
            this.CCY.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.CCY.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // QuantityFilled
            // 
            this.QuantityFilled.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.QuantityFilled.DataPropertyName = "QuantityFilled";
            this.QuantityFilled.HeaderText = "QtyFilled";
            this.QuantityFilled.MinimumWidth = 50;
            this.QuantityFilled.Name = "QuantityFilled";
            this.QuantityFilled.ReadOnly = true;
            this.QuantityFilled.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.QuantityFilled.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // quantityOnMarketDataGridViewTextBoxColumn
            // 
            this.quantityOnMarketDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.quantityOnMarketDataGridViewTextBoxColumn.DataPropertyName = "QuantityOnMarket";
            this.quantityOnMarketDataGridViewTextBoxColumn.HeaderText = "QtyOnMkt";
            this.quantityOnMarketDataGridViewTextBoxColumn.MinimumWidth = 50;
            this.quantityOnMarketDataGridViewTextBoxColumn.Name = "quantityOnMarketDataGridViewTextBoxColumn";
            this.quantityOnMarketDataGridViewTextBoxColumn.ReadOnly = true;
            this.quantityOnMarketDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.quantityOnMarketDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // remainingQuantityDataGridViewTextBoxColumn
            // 
            this.remainingQuantityDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.remainingQuantityDataGridViewTextBoxColumn.DataPropertyName = "RemainingQuantity";
            this.remainingQuantityDataGridViewTextBoxColumn.HeaderText = "QtyRem";
            this.remainingQuantityDataGridViewTextBoxColumn.MinimumWidth = 50;
            this.remainingQuantityDataGridViewTextBoxColumn.Name = "remainingQuantityDataGridViewTextBoxColumn";
            this.remainingQuantityDataGridViewTextBoxColumn.ReadOnly = true;
            this.remainingQuantityDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.remainingQuantityDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // AssignmentBlotter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridView1);
            this.Name = "AssignmentBlotter";
            this.Size = new System.Drawing.Size(461, 109);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetPermitsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetPermits)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource dataSetPermitsBindingSource;
        private OPEX.DWEGUI.Data.DataSetPermits dataSetPermits;
        private System.Windows.Forms.DataGridViewTextBoxColumn Side;
        private System.Windows.Forms.DataGridViewTextBoxColumn quantityDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn RIC;
        private System.Windows.Forms.DataGridViewTextBoxColumn limitPriceDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn CCY;
        private System.Windows.Forms.DataGridViewTextBoxColumn QuantityFilled;
        private System.Windows.Forms.DataGridViewTextBoxColumn quantityOnMarketDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn remainingQuantityDataGridViewTextBoxColumn;
    }
}
