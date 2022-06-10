namespace OPEX.DWEGUI
{
    partial class OrderBlotter
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.timeStampDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.orderIDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sideDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.instrumentDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.currencyDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.priceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LimitPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.quantityDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.quantityFilledDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.quantityRemainingDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.turnoverDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.originDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.destinationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clientOrderIDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.parentOrderIDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastPriceFilledDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastQuantityFilledDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.averagePriceFilledDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newOrderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.acceptedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filledToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.completelyFilledToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rejectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelledByExchangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelledToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataTableOrdersBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dataSetOrders = new OPEX.DWEGUI.Data.DataSetOrders();
            this.lblNumOrders = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataTableOrdersBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetOrders)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.timeStampDataGridViewTextBoxColumn,
            this.orderIDDataGridViewTextBoxColumn,
            this.statusDataGridViewTextBoxColumn,
            this.sideDataGridViewTextBoxColumn,
            this.instrumentDataGridViewTextBoxColumn,
            this.currencyDataGridViewTextBoxColumn,
            this.priceDataGridViewTextBoxColumn,
            this.LimitPrice,
            this.quantityDataGridViewTextBoxColumn,
            this.quantityFilledDataGridViewTextBoxColumn,
            this.quantityRemainingDataGridViewTextBoxColumn,
            this.turnoverDataGridViewTextBoxColumn,
            this.originDataGridViewTextBoxColumn,
            this.destinationDataGridViewTextBoxColumn,
            this.clientOrderIDDataGridViewTextBoxColumn,
            this.parentOrderIDDataGridViewTextBoxColumn,
            this.lastPriceFilledDataGridViewTextBoxColumn,
            this.lastQuantityFilledDataGridViewTextBoxColumn,
            this.averagePriceFilledDataGridViewTextBoxColumn,
            this.Message});
            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip;
            this.dataGridView1.DataSource = this.dataTableOrdersBindingSource;
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle11.NullValue = null;
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle11.SelectionForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle11;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 18;
            this.dataGridView1.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.ShowEditingIcon = false;
            this.dataGridView1.Size = new System.Drawing.Size(577, 120);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.dataGridView1_RowsAdded);
            this.dataGridView1.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridView1_CellPainting);
            this.dataGridView1.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseDoubleClick);
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
            // 
            // timeStampDataGridViewTextBoxColumn
            // 
            this.timeStampDataGridViewTextBoxColumn.DataPropertyName = "TimeStamp";
            dataGridViewCellStyle1.Format = "T";
            dataGridViewCellStyle1.NullValue = null;
            this.timeStampDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this.timeStampDataGridViewTextBoxColumn.HeaderText = "TimeStamp";
            this.timeStampDataGridViewTextBoxColumn.Name = "timeStampDataGridViewTextBoxColumn";
            this.timeStampDataGridViewTextBoxColumn.ReadOnly = true;
            this.timeStampDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.timeStampDataGridViewTextBoxColumn.Width = 85;
            // 
            // orderIDDataGridViewTextBoxColumn
            // 
            this.orderIDDataGridViewTextBoxColumn.DataPropertyName = "OrderID";
            this.orderIDDataGridViewTextBoxColumn.HeaderText = "OrderID";
            this.orderIDDataGridViewTextBoxColumn.Name = "orderIDDataGridViewTextBoxColumn";
            this.orderIDDataGridViewTextBoxColumn.ReadOnly = true;
            this.orderIDDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.orderIDDataGridViewTextBoxColumn.Width = 50;
            // 
            // statusDataGridViewTextBoxColumn
            // 
            this.statusDataGridViewTextBoxColumn.DataPropertyName = "Status";
            this.statusDataGridViewTextBoxColumn.HeaderText = "Status";
            this.statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
            this.statusDataGridViewTextBoxColumn.ReadOnly = true;
            this.statusDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.statusDataGridViewTextBoxColumn.Width = 43;
            // 
            // sideDataGridViewTextBoxColumn
            // 
            this.sideDataGridViewTextBoxColumn.DataPropertyName = "Side";
            this.sideDataGridViewTextBoxColumn.HeaderText = "Side";
            this.sideDataGridViewTextBoxColumn.Name = "sideDataGridViewTextBoxColumn";
            this.sideDataGridViewTextBoxColumn.ReadOnly = true;
            this.sideDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.sideDataGridViewTextBoxColumn.Width = 34;
            // 
            // instrumentDataGridViewTextBoxColumn
            // 
            this.instrumentDataGridViewTextBoxColumn.DataPropertyName = "Instrument";
            this.instrumentDataGridViewTextBoxColumn.HeaderText = "Instrument";
            this.instrumentDataGridViewTextBoxColumn.Name = "instrumentDataGridViewTextBoxColumn";
            this.instrumentDataGridViewTextBoxColumn.ReadOnly = true;
            this.instrumentDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.instrumentDataGridViewTextBoxColumn.Width = 62;
            // 
            // currencyDataGridViewTextBoxColumn
            // 
            this.currencyDataGridViewTextBoxColumn.DataPropertyName = "Currency";
            this.currencyDataGridViewTextBoxColumn.HeaderText = "Currency";
            this.currencyDataGridViewTextBoxColumn.Name = "currencyDataGridViewTextBoxColumn";
            this.currencyDataGridViewTextBoxColumn.ReadOnly = true;
            this.currencyDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.currencyDataGridViewTextBoxColumn.Width = 55;
            // 
            // priceDataGridViewTextBoxColumn
            // 
            this.priceDataGridViewTextBoxColumn.DataPropertyName = "Price";
            dataGridViewCellStyle2.Format = "N4";
            dataGridViewCellStyle2.NullValue = null;
            this.priceDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.priceDataGridViewTextBoxColumn.HeaderText = "Price";
            this.priceDataGridViewTextBoxColumn.Name = "priceDataGridViewTextBoxColumn";
            this.priceDataGridViewTextBoxColumn.ReadOnly = true;
            this.priceDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.priceDataGridViewTextBoxColumn.Width = 37;
            // 
            // LimitPrice
            // 
            this.LimitPrice.DataPropertyName = "LimitPrice";
            dataGridViewCellStyle3.Format = "N4";
            this.LimitPrice.DefaultCellStyle = dataGridViewCellStyle3;
            this.LimitPrice.HeaderText = "LimitPrice";
            this.LimitPrice.Name = "LimitPrice";
            this.LimitPrice.ReadOnly = true;
            this.LimitPrice.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.LimitPrice.Width = 58;
            // 
            // quantityDataGridViewTextBoxColumn
            // 
            this.quantityDataGridViewTextBoxColumn.DataPropertyName = "Quantity";
            dataGridViewCellStyle4.Format = "N0";
            dataGridViewCellStyle4.NullValue = null;
            this.quantityDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle4;
            this.quantityDataGridViewTextBoxColumn.HeaderText = "Quantity";
            this.quantityDataGridViewTextBoxColumn.Name = "quantityDataGridViewTextBoxColumn";
            this.quantityDataGridViewTextBoxColumn.ReadOnly = true;
            this.quantityDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.quantityDataGridViewTextBoxColumn.Width = 52;
            // 
            // quantityFilledDataGridViewTextBoxColumn
            // 
            this.quantityFilledDataGridViewTextBoxColumn.DataPropertyName = "QuantityFilled";
            dataGridViewCellStyle5.Format = "N0";
            dataGridViewCellStyle5.NullValue = null;
            this.quantityFilledDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle5;
            this.quantityFilledDataGridViewTextBoxColumn.HeaderText = "QuantityFilled";
            this.quantityFilledDataGridViewTextBoxColumn.Name = "quantityFilledDataGridViewTextBoxColumn";
            this.quantityFilledDataGridViewTextBoxColumn.ReadOnly = true;
            this.quantityFilledDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.quantityFilledDataGridViewTextBoxColumn.Width = 76;
            // 
            // quantityRemainingDataGridViewTextBoxColumn
            // 
            this.quantityRemainingDataGridViewTextBoxColumn.DataPropertyName = "QuantityRemaining";
            dataGridViewCellStyle6.Format = "N0";
            dataGridViewCellStyle6.NullValue = null;
            this.quantityRemainingDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle6;
            this.quantityRemainingDataGridViewTextBoxColumn.HeaderText = "QuantityRemaining";
            this.quantityRemainingDataGridViewTextBoxColumn.Name = "quantityRemainingDataGridViewTextBoxColumn";
            this.quantityRemainingDataGridViewTextBoxColumn.ReadOnly = true;
            this.quantityRemainingDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.quantityRemainingDataGridViewTextBoxColumn.Width = 102;
            // 
            // turnoverDataGridViewTextBoxColumn
            // 
            this.turnoverDataGridViewTextBoxColumn.DataPropertyName = "Turnover";
            dataGridViewCellStyle7.Format = "N4";
            dataGridViewCellStyle7.NullValue = null;
            this.turnoverDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle7;
            this.turnoverDataGridViewTextBoxColumn.HeaderText = "Turnover";
            this.turnoverDataGridViewTextBoxColumn.Name = "turnoverDataGridViewTextBoxColumn";
            this.turnoverDataGridViewTextBoxColumn.ReadOnly = true;
            this.turnoverDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.turnoverDataGridViewTextBoxColumn.Width = 56;
            // 
            // originDataGridViewTextBoxColumn
            // 
            this.originDataGridViewTextBoxColumn.DataPropertyName = "Origin";
            this.originDataGridViewTextBoxColumn.HeaderText = "Origin";
            this.originDataGridViewTextBoxColumn.Name = "originDataGridViewTextBoxColumn";
            this.originDataGridViewTextBoxColumn.ReadOnly = true;
            this.originDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.originDataGridViewTextBoxColumn.Width = 40;
            // 
            // destinationDataGridViewTextBoxColumn
            // 
            this.destinationDataGridViewTextBoxColumn.DataPropertyName = "Destination";
            this.destinationDataGridViewTextBoxColumn.HeaderText = "Destination";
            this.destinationDataGridViewTextBoxColumn.Name = "destinationDataGridViewTextBoxColumn";
            this.destinationDataGridViewTextBoxColumn.ReadOnly = true;
            this.destinationDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.destinationDataGridViewTextBoxColumn.Width = 66;
            // 
            // clientOrderIDDataGridViewTextBoxColumn
            // 
            this.clientOrderIDDataGridViewTextBoxColumn.DataPropertyName = "ClientOrderID";
            this.clientOrderIDDataGridViewTextBoxColumn.HeaderText = "ClientOrderID";
            this.clientOrderIDDataGridViewTextBoxColumn.Name = "clientOrderIDDataGridViewTextBoxColumn";
            this.clientOrderIDDataGridViewTextBoxColumn.ReadOnly = true;
            this.clientOrderIDDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.clientOrderIDDataGridViewTextBoxColumn.Width = 76;
            // 
            // parentOrderIDDataGridViewTextBoxColumn
            // 
            this.parentOrderIDDataGridViewTextBoxColumn.DataPropertyName = "ParentOrderID";
            this.parentOrderIDDataGridViewTextBoxColumn.HeaderText = "ParentOrderID";
            this.parentOrderIDDataGridViewTextBoxColumn.Name = "parentOrderIDDataGridViewTextBoxColumn";
            this.parentOrderIDDataGridViewTextBoxColumn.ReadOnly = true;
            this.parentOrderIDDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.parentOrderIDDataGridViewTextBoxColumn.Width = 81;
            // 
            // lastPriceFilledDataGridViewTextBoxColumn
            // 
            this.lastPriceFilledDataGridViewTextBoxColumn.DataPropertyName = "LastPriceFilled";
            dataGridViewCellStyle8.Format = "N4";
            dataGridViewCellStyle8.NullValue = null;
            this.lastPriceFilledDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle8;
            this.lastPriceFilledDataGridViewTextBoxColumn.HeaderText = "LastPriceFilled";
            this.lastPriceFilledDataGridViewTextBoxColumn.Name = "lastPriceFilledDataGridViewTextBoxColumn";
            this.lastPriceFilledDataGridViewTextBoxColumn.ReadOnly = true;
            this.lastPriceFilledDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.lastPriceFilledDataGridViewTextBoxColumn.Width = 81;
            // 
            // lastQuantityFilledDataGridViewTextBoxColumn
            // 
            this.lastQuantityFilledDataGridViewTextBoxColumn.DataPropertyName = "LastQuantityFilled";
            dataGridViewCellStyle9.Format = "N0";
            dataGridViewCellStyle9.NullValue = null;
            this.lastQuantityFilledDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle9;
            this.lastQuantityFilledDataGridViewTextBoxColumn.HeaderText = "LastQuantityFilled";
            this.lastQuantityFilledDataGridViewTextBoxColumn.Name = "lastQuantityFilledDataGridViewTextBoxColumn";
            this.lastQuantityFilledDataGridViewTextBoxColumn.ReadOnly = true;
            this.lastQuantityFilledDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.lastQuantityFilledDataGridViewTextBoxColumn.Width = 96;
            // 
            // averagePriceFilledDataGridViewTextBoxColumn
            // 
            this.averagePriceFilledDataGridViewTextBoxColumn.DataPropertyName = "AveragePriceFilled";
            dataGridViewCellStyle10.Format = "N4";
            dataGridViewCellStyle10.NullValue = null;
            this.averagePriceFilledDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle10;
            this.averagePriceFilledDataGridViewTextBoxColumn.HeaderText = "AveragePriceFilled";
            this.averagePriceFilledDataGridViewTextBoxColumn.Name = "averagePriceFilledDataGridViewTextBoxColumn";
            this.averagePriceFilledDataGridViewTextBoxColumn.ReadOnly = true;
            this.averagePriceFilledDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.averagePriceFilledDataGridViewTextBoxColumn.Width = 101;
            // 
            // Message
            // 
            this.Message.DataPropertyName = "Message";
            this.Message.HeaderText = "Message";
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            this.Message.Width = 75;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(104, 26);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newOrderToolStripMenuItem,
            this.acceptedToolStripMenuItem,
            this.filledToolStripMenuItem,
            this.completelyFilledToolStripMenuItem,
            this.rejectedToolStripMenuItem,
            this.cancelledByExchangeToolStripMenuItem,
            this.cancelledToolStripMenuItem});
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.showToolStripMenuItem.Text = "&Show";
            // 
            // newOrderToolStripMenuItem
            // 
            this.newOrderToolStripMenuItem.Checked = true;
            this.newOrderToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.newOrderToolStripMenuItem.Name = "newOrderToolStripMenuItem";
            this.newOrderToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.newOrderToolStripMenuItem.Text = "NewOrder";
            this.newOrderToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showToolStripMenuItem_CheckedChanged);
            this.newOrderToolStripMenuItem.Click += new System.EventHandler(this.newOrderToolStripMenuItem_Click);
            // 
            // acceptedToolStripMenuItem
            // 
            this.acceptedToolStripMenuItem.Checked = true;
            this.acceptedToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.acceptedToolStripMenuItem.Name = "acceptedToolStripMenuItem";
            this.acceptedToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.acceptedToolStripMenuItem.Text = "Accepted";
            this.acceptedToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showToolStripMenuItem_CheckedChanged);
            this.acceptedToolStripMenuItem.Click += new System.EventHandler(this.newOrderToolStripMenuItem_Click);
            // 
            // filledToolStripMenuItem
            // 
            this.filledToolStripMenuItem.Checked = true;
            this.filledToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.filledToolStripMenuItem.Name = "filledToolStripMenuItem";
            this.filledToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.filledToolStripMenuItem.Text = "Filled";
            this.filledToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showToolStripMenuItem_CheckedChanged);
            this.filledToolStripMenuItem.Click += new System.EventHandler(this.newOrderToolStripMenuItem_Click);
            // 
            // completelyFilledToolStripMenuItem
            // 
            this.completelyFilledToolStripMenuItem.Checked = true;
            this.completelyFilledToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.completelyFilledToolStripMenuItem.Name = "completelyFilledToolStripMenuItem";
            this.completelyFilledToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.completelyFilledToolStripMenuItem.Text = "CompletelyFilled";
            this.completelyFilledToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showToolStripMenuItem_CheckedChanged);
            this.completelyFilledToolStripMenuItem.Click += new System.EventHandler(this.newOrderToolStripMenuItem_Click);
            // 
            // rejectedToolStripMenuItem
            // 
            this.rejectedToolStripMenuItem.Name = "rejectedToolStripMenuItem";
            this.rejectedToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.rejectedToolStripMenuItem.Text = "Rejected";
            this.rejectedToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showToolStripMenuItem_CheckedChanged);
            this.rejectedToolStripMenuItem.Click += new System.EventHandler(this.newOrderToolStripMenuItem_Click);
            // 
            // cancelledByExchangeToolStripMenuItem
            // 
            this.cancelledByExchangeToolStripMenuItem.Name = "cancelledByExchangeToolStripMenuItem";
            this.cancelledByExchangeToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.cancelledByExchangeToolStripMenuItem.Text = "CancelledByExchange";
            this.cancelledByExchangeToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showToolStripMenuItem_CheckedChanged);
            this.cancelledByExchangeToolStripMenuItem.Click += new System.EventHandler(this.newOrderToolStripMenuItem_Click);
            // 
            // cancelledToolStripMenuItem
            // 
            this.cancelledToolStripMenuItem.Name = "cancelledToolStripMenuItem";
            this.cancelledToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.cancelledToolStripMenuItem.Text = "Cancelled";
            this.cancelledToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showToolStripMenuItem_CheckedChanged);
            this.cancelledToolStripMenuItem.Click += new System.EventHandler(this.newOrderToolStripMenuItem_Click);
            // 
            // dataTableOrdersBindingSource
            // 
            this.dataTableOrdersBindingSource.DataMember = "DataTableOrders";
            this.dataTableOrdersBindingSource.DataSource = this.dataSetOrders;
            this.dataTableOrdersBindingSource.Sort = "TimeStamp";
            // 
            // dataSetOrders
            // 
            this.dataSetOrders.DataSetName = "DataSetOrders";
            this.dataSetOrders.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // lblNumOrders
            // 
            this.lblNumOrders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblNumOrders.AutoSize = true;
            this.lblNumOrders.Location = new System.Drawing.Point(-3, 123);
            this.lblNumOrders.Name = "lblNumOrders";
            this.lblNumOrders.Size = new System.Drawing.Size(53, 13);
            this.lblNumOrders.TabIndex = 1;
            this.lblNumOrders.Text = "0 Order(s)";
            // 
            // OrderBlotter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.lblNumOrders);
            this.Name = "OrderBlotter";
            this.Size = new System.Drawing.Size(577, 136);
            this.Load += new System.EventHandler(this.OrderBlotter_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataTableOrdersBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetOrders)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource dataTableOrdersBindingSource;
        private OPEX.DWEGUI.Data.DataSetOrders dataSetOrders;
        private System.Windows.Forms.Label lblNumOrders;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newOrderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem acceptedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filledToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem completelyFilledToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rejectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cancelledByExchangeToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn timeStampDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn orderIDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn sideDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn instrumentDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn currencyDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn priceDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn LimitPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn quantityDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn quantityFilledDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn quantityRemainingDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn turnoverDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn originDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn destinationDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn clientOrderIDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn parentOrderIDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastPriceFilledDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastQuantityFilledDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn averagePriceFilledDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
        private System.Windows.Forms.ToolStripMenuItem cancelledToolStripMenuItem;
    }
}
