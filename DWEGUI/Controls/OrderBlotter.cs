//////////////////////////////////////////////////////////////////////////////
//
// Attribution-NonCommercial-ShareAlike 3.0 Unported (CC BY-NC-SA 3.0)
// Disclaimer
// 
// This file is part of OpEx.
// 
// You are free:
// 	* to Share : to copy, distribute and transmit the work;
//	* to Remix : to adapt the work.
//		
// Under the following conditions:
// 	* Attribution : You must attribute OpEx to Marco De Luca
//     (marco.de.luca@algotradingconsulting.com);
// 	* Noncommercial : You may not use this work for commercial purposes;
// 	* Share Alike : If you alter, transform, or build upon this work,
//     you may distribute the resulting work only under the same or similar
//     license to this one. 
// 	
// With the understanding that: 
// 	* Waiver : Any of the above conditions can be waived if you get permission
//     from the copyright holder. 
// 	* Public Domain : Where the work or any of its elements is in the public
//     domain under applicable law, that status is in no way affected by the
//     license. 
// 	* Other Rights : In no way are any of the following rights affected by
//     the license: 
//		 . Your fair dealing or fair use rights, or other applicable copyright
//          exceptions and limitations; 
//		 . The author's moral rights; 
//		 . Rights other persons may have either in the work itself or in how
//          the work is used, such as publicity or privacy rights. 
//	* Notice : For any reuse or distribution, you must make clear to others
//     the license terms of this work. The best way to do this is with a link
//     to this web page. 
// 
// You should have received a copy of the Legal Code of the CC BY-NC-SA 3.0
// Licence along with OpEx.
// If not, see <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>.
//
//////////////////////////////////////////////////////////////////////////////


﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OPEX.Common;
using OPEX.OM.Common;
using OPEX.DWEGUI.Data;

namespace OPEX.DWEGUI
{
    public partial class OrderBlotter : UserControl
    {
        private int _nOrders;
        private readonly Logger _logger;

        public OrderBlotter()
        {
            InitializeComponent();

            _nOrders = 0;
            UpdateNumOrdersLabel();

            if (!DesignMode)
            {
                _logger = new Logger("OrderBlotter");
            }
        }

        public void AddOrder(IOrder order)
        {
            InnerAddOrder(order);
        }

        private delegate void InnerAddOrderDelegate(IOrder order);
        private void InnerAddOrder(IOrder order)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new InnerAddOrderDelegate(InnerAddOrder), order);
            }
            else
            {
                DataSetOrders.DataTableOrdersRow tableRow = dataSetOrders.DataTableOrders.FindByClientOrderID(order.ClientOrderID);

                if (tableRow == null)
                {
                    dataSetOrders.DataTableOrders.AddDataTableOrdersRow(
                        order.AveragePriceFilled, order.LastQuantityFilled, order.LastPriceFilled,
                        order.Currency, order.ParentOrderID, order.ClientOrderID, order.Destination,
                        order.Origin, order.Turnover, order.QuantityRemaining, order.QuantityFilled,
                        order.Price, order.Quantity, order.Instrument, order.Side.ToString(),
                        order.Status.ToString(), order.OrderID, order.TimeStamp, order.Message, order.LimitPrice);

                    ++_nOrders;
                    UpdateNumOrdersLabel();
                }
                else
                {
                    tableRow.Destination = order.Destination;
                    tableRow.Side = order.Side.ToString();
                    tableRow.Instrument = order.Instrument;
                    tableRow.Currency = order.Currency;
                    tableRow.ClientOrderID = order.ClientOrderID;
                    tableRow.TimeStamp = order.TimeStamp;
                    tableRow.AveragePriceFilled = order.AveragePriceFilled;
                    tableRow.LastPriceFilled = order.LastPriceFilled;
                    tableRow.LastQuantityFilled = order.LastQuantityFilled;
                    tableRow.OrderID = order.OrderID;
                    tableRow.ParentOrderID = order.ParentOrderID;
                    tableRow.Price = order.Price;
                    tableRow.Quantity = order.Quantity;
                    tableRow.QuantityFilled = order.QuantityFilled;
                    tableRow.QuantityRemaining = order.QuantityRemaining;
                    tableRow.Status = order.Status.ToString();
                    tableRow.Turnover = order.Turnover;
                    tableRow.Message = order.Message;
                    tableRow.LimitPrice = order.LimitPrice;
                    tableRow.Origin = order.Origin;
                }
            }
        }       

        private void UpdateNumOrdersLabel()
        {
            lblNumOrders.Text = string.Format("{0} Order(s)", _nOrders);
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            DataGridViewRow lastRowAdded = null;

            for (int index = e.RowIndex; index < e.RowIndex + e.RowCount; ++index)
            {
                lastRowAdded = this.dataGridView1.Rows[index];
            }

            dataGridView1.Sort(dataGridView1.Columns["timeStampDataGridViewTextBoxColumn"], ListSortDirection.Descending);

            if (lastRowAdded != null)
            {
                dataGridView1.CurrentCell = dataGridView1[0, lastRowAdded.Index];
                dataGridView1.FirstDisplayedScrollingRowIndex = 0;
            }            
        }

        public delegate void OrderSelectedEventHandler(object sender, Order o);
        public event OrderSelectedEventHandler OrderSelected;

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                Order o = RowToOrder(dataGridView1.SelectedRows[0]);
                if (OrderSelected != null)
                {
                    OrderSelected(this, o);
                }               
            }
        }

        private Order RowToOrder(DataGridViewRow dataGridViewRow)
        {
            Order o = null;

            if (dataGridViewRow != null)
            {
                try
                {
                    long clientOrderID = long.Parse(dataGridViewRow.Cells["clientOrderIDDataGridViewTextBoxColumn"].Value.ToString());
                    if (OrderFactory.OutgoingOrders.Contains(clientOrderID))
                    {
                        o = OrderFactory.OutgoingOrders[clientOrderID] as Order;
                    }
                }
                catch { }
            }
            return o;
        }

        private void showToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            RebuildFilter();
        }

        private void newOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            menu.Checked = !menu.Checked;
        }

        private void RebuildFilter()
        {            
            StringBuilder newFilter = new StringBuilder();            
            bool isFirst = true;

            foreach (ToolStripMenuItem toolStripMenuItem in showToolStripMenuItem.DropDownItems)
            {
                bool c = toolStripMenuItem.Checked;
                if (c)
                {
                    continue;
                }
                if (!isFirst)
                {
                    newFilter.Append(" and ");
                }
                newFilter.AppendFormat("Status<>'{0}'", toolStripMenuItem.Text);
                isFirst = false;
            }

            dataTableOrdersBindingSource.Filter = newFilter.ToString();            
        }

        private Color GetColorByRowIndex(int p)
        {
            Color c = Color.LightGoldenrodYellow;

            try
            {
                DataGridViewRow row = dataGridView1.Rows[p];
                DataGridViewCell statusCell = row.Cells[statusDataGridViewTextBoxColumn.Name];
                string statusCellContent = statusCell.Value.ToString();
                OrderStatus status = (OrderStatus)Enum.Parse(typeof(OrderStatus), statusCellContent);
                switch (status)
                {
                    case OrderStatus.NewOrder:
                    case OrderStatus.Accepted:
                    case OrderStatus.AmendAccepted:
                    case OrderStatus.AmendRejected:                        
                        c = Color.Cyan;
                        break;                    
                    case OrderStatus.CancelledByExchange:
                    case OrderStatus.Overfilled:
                    case OrderStatus.Rejected:
                        c = Color.Red;
                        break;
                    case OrderStatus.Filled:
                    case OrderStatus.CompletelyFilled:
                        c = Color.LimeGreen;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "GetColorByRowIndex.Exception: {0} {1}", ex.Message,
                    ex.StackTrace.Replace(Environment.NewLine, " ").Replace("\n", " "));
            }

            return c;
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                Color c = GetColorByRowIndex(e.RowIndex);
                if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
                {
                    e.CellStyle.SelectionBackColor = c;
                    e.CellStyle.SelectionForeColor = SystemColors.ControlText;
                }
                else
                {
                    c = Color.FromArgb(
                        c.R + (int)((255.0 - (double)c.R) * 0.75),
                        c.G + (int)((255.0 - (double)c.G) * 0.75),
                        c.B + (int)((255.0 - (double)c.B) * 0.75));
                    e.CellStyle.BackColor = c;
                }
            }
        }

        public delegate void OrderBlotterRowDoubleClickedEventHandler(object sender, OutgoingOrder order);
        public event OrderBlotterRowDoubleClickedEventHandler RowDoubleClicked;

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {            
            int rowIndex = e.RowIndex;

            if (rowIndex < 0)
            {
                return;
            }

            long clientOrderID = 
                long.Parse(dataGridView1.Rows[rowIndex]
                .Cells["clientOrderIDDataGridViewTextBoxColumn"].Value.ToString());

            if (OrderFactory.OutgoingOrders.Contains(clientOrderID))
            {
                OutgoingOrder o = OrderFactory.OutgoingOrders[clientOrderID] as OutgoingOrder;
                OrderBlotterRowDoubleClickedEventHandler h = RowDoubleClicked;
                if (o != null && h != null)
                {
                    h(this, o);
                }
            }
        }

        private void OrderBlotter_Load(object sender, EventArgs e)
        {
            RebuildFilter();
        }        
    }
}
