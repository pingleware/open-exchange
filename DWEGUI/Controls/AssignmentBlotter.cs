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

using OPEX.DWEGUI.Data;
using OPEX.DWEAS.Client;
using OPEX.OM.Common;

namespace OPEX.DWEGUI.Controls
{
    public delegate void AssignmentBlotterClickedEventHandler(object sender, AssignmentBucket pb);

    public partial class AssignmentBlotter : UserControl
    {        
        private SortedDictionary<double, AssignmentBucket> _assignmentBuckets;
        private bool _firstPermit;

        public AssignmentBlotter()
        {
            InitializeComponent();            
            _assignmentBuckets = new SortedDictionary<double, AssignmentBucket>();
            _firstPermit = true;
        }

        public event AssignmentBlotterClickedEventHandler AssignmentBlotterClicked;

        public void Clear()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SafeUpdate(Clear));
            }
            else
            {
                dataSetPermits.DataTablePermits.Clear();
            }            
        }

        internal void UpdatePermits(SortedDictionary<double, AssignmentBucket> assignmentBuckets)
        {
            _assignmentBuckets = assignmentBuckets;

            if (this.InvokeRequired)
            {
                this.Invoke(new SafeUpdate(InnerUpdatePermits));
            }
            else
            {
                InnerUpdatePermits();
            }  
        }

        delegate void SafeUpdate();
        private void InnerUpdatePermits()
        {
            List<double> pricesToRemove = new List<double>();
            foreach (DataSetPermits.DataTablePermitsRow tableRow in dataSetPermits.DataTablePermits.Rows)
            {
                bool remove = false;
                double price = tableRow.LimitPrice;

                if (!_assignmentBuckets.ContainsKey(price))
                {
                    remove = true;
                }
                else
                {
                    AssignmentBucket ab = _assignmentBuckets[price];
                    if (ab.Qty == ab.QtyFilled)
                    {
                        remove = true;
                    }
                }

                if (remove)
                {
                    pricesToRemove.Add(tableRow.LimitPrice);
                }
            }
            foreach (double price in pricesToRemove)
            {
                DataSetPermits.DataTablePermitsRow tableRow =
                   dataSetPermits.DataTablePermits.FindByLimitPrice(price);
                tableRow.Delete();
            }            

            foreach (double price in _assignmentBuckets.Keys)
            {
                AssignmentBucket ab = _assignmentBuckets[price];                
                DataSetPermits.DataTablePermitsRow tableRow =
                    dataSetPermits.DataTablePermits.FindByLimitPrice(price);

                if (_firstPermit)
                {
                    dataGridView1.Sort(dataGridView1.Columns["limitPriceDataGridViewTextBoxColumn"],
                        (ab.Side == OrderSide.Sell) ? ListSortDirection.Ascending
                        : ListSortDirection.Descending);
                    _firstPermit = false;
                }

                if (ab.Qty == ab.QtyFilled)
                {
                    continue;
                }

                if (tableRow == null)
                {
                    AddRow(ab);
                }
                else
                {
                    UpdateRow(tableRow, ab);
                }
            }           
        }

        private void AddRow(AssignmentBucket ab)
        {
            dataSetPermits.DataTablePermits.AddDataTablePermitsRow(
                ab.Price, ab.Qty, ab.QtyOnMkt, ab.QtyRem, ab.Side.ToString(), ab.CCY, ab.RIC, ab.QtyFilled);
        }

        private void UpdateRow(DataSetPermits.DataTablePermitsRow tableRow, AssignmentBucket ab)
        {
            if (ab.Qty > ab.QtyFilled)
            {
                tableRow.Quantity = ab.Qty;
                tableRow.QuantityOnMarket = ab.QtyOnMkt;
                tableRow.RemainingQuantity = ab.QtyRem;
                tableRow.QuantityFilled = ab.QtyFilled;
            }
            else
            {
                tableRow.Delete();
            }
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int rowIndex = e.RowIndex;

            if (rowIndex < 0)
            {
                return;
            }

            double price =
               double.Parse(dataGridView1.Rows[rowIndex]
               .Cells["limitPriceDataGridViewTextBoxColumn"].Value.ToString());
            
            if (!_assignmentBuckets.ContainsKey(price))
            {
                return;
            } 

            AssignmentBlotterClickedEventHandler handler = this.AssignmentBlotterClicked;
            if (handler != null)
            {
                AssignmentBlotterClicked(this, _assignmentBuckets[price]);
            }
        }       
    }
}
