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

using OPEX.OM.Common;

namespace OPEX.TradingGUI
{
    public partial class OrderBlotter : UserControl
    {
        private int _nOrders;

        public OrderBlotter()
        {
            InitializeComponent();

            _nOrders = 0;
            UpdateNumOrdersLabel();

            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            
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
                        order.Status.ToString(), order.OrderID, order.TimeStamp, order.Message);

                    ++_nOrders;
                    UpdateNumOrdersLabel();
                }
                else
                {
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
                }
            }
        }

        private void ChangeRowColor(DataGridViewRow row, bool selected)
        {
            string status = row.Cells["statusDataGridViewTextBoxColumn"].Value.ToString();
            Color color = Color.White;            
            switch (status)
            {
                case "NewOrder":
                    color = Color.Ivory;
                    break;
                case "Accepted":
                    color = Color.LightYellow;
                    break;
                case "Filled":
                    color = Color.GreenYellow;
                    break;
                case "CompletelyFilled":
                    color = Color.LimeGreen;
                    break;
                case "CancelledByExchange":
                    color = Color.LightCoral;
                    break;
                case "Rejected":
                    color = Color.Beige;
                    break;
                case "Overfilled":
                    color = Color.DarkGreen;
                    break;
                default:
                    break;
            }
            if (!selected)
            {
                row.DefaultCellStyle.BackColor = color;
                row.DefaultCellStyle.ForeColor = Color.Black;
            }
            else
            {
                row.DefaultCellStyle.SelectionBackColor = color;
                row.DefaultCellStyle.SelectionForeColor = Color.Black;
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

                ChangeRowColor(lastRowAdded, false);
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
    }
}
