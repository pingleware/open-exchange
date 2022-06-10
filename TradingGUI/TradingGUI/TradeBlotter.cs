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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OPEX.TDS.Common;
using OPEX.Common;

namespace OPEX.TradingGUI
{
    public partial class TradeBlotter : UserControl
    {
        private int _nTrades;

        public TradeBlotter()
        {
            InitializeComponent();

            _nTrades = 0;
            UpdateNumTradesLabel();
        }

        private string _applicationName;
        public string ApplicationName { get { return _applicationName; } set { _applicationName = value; } }
        
        public void AddTrade(TradeDataMessage tradeData)
        {
            InnerAddTrade(tradeData);
        }

        public void ClearFilter()
        {
            RebuildFilter(-1);
        }

        public void ApplyFilter(long orderID)
        {
            RebuildFilter(orderID);
        }

        private delegate void InnerAddTradeDelegate(TradeDataMessage tradeDataMessage);
        private void InnerAddTrade(TradeDataMessage tradeData)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new InnerAddTradeDelegate(InnerAddTrade), tradeData);
            }
            else
            {
                dataSetTrades.Trades.AddTradesRow(
                    tradeData.Counterparty,
                    tradeData.Price,
                    tradeData.Quantity,
                    tradeData.OrderID,
                    tradeData.TimeStamp,
                    tradeData.FillID);

                _nTrades++;
                UpdateNumTradesLabel();
            }
        }

        private void UpdateNumTradesLabel()
        {
            this.lblNumTrades.Text = string.Format("{0} Trade(s)", _nTrades);
        }        

        private void RebuildFilter(long orderID)
        {
            StringBuilder newFilter = new StringBuilder();

            newFilter.AppendFormat("Counterparty<>'{0}'", _applicationName);

            if (orderID > 0)
            {
                newFilter.AppendFormat("and OrderID={0}", orderID.ToString());
            }

            tradesBindingSource.Filter = newFilter.ToString();            
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            dataGridView1.Sort(timeDataGridViewTextBoxColumn, ListSortDirection.Ascending);
            dataGridView1.FirstDisplayedScrollingRowIndex = 0;
        }        
    }
}
