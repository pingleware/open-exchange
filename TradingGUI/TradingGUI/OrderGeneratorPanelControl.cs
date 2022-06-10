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

using OPEX.Common;
using OPEX.OM.Common;

namespace OPEX.TradingGUI
{
    public partial class OrderGeneratorPanelControl : UserControl
    {
        private ComplexTimer _timer;

        public event NewOrderGeneratedEventHandler NewOrder;

        public OrderGeneratorPanelControl()
        {
            InitializeComponent();

            InitTIFCombo();
        }

        private void InitTIFCombo()
        {
            cmbTIF.DataSource = Enum.GetValues(typeof(OrderType));
            cmbTIF.SelectedIndex = 1;
        }

        public bool Running
        {
            get { return chkOnOff.Checked; }
            set { if (chkOnOff.Checked != value) chkOnOff.Checked = value; }
        }

        public string RIC
        {
            get { return this.txtRIC.Text; }
            set { this.txtRIC.Text = value; }
        }

        public OrderType TIF
        {
            get { return (OrderType)Enum.Parse(typeof(OrderType), this.cmbTIF.Text); }
            set { this.cmbTIF.Text = value.ToString(); }
        }

        public OrderSide Side
        {
            get { return (optBuy.Checked) ? OrderSide.Buy : OrderSide.Sell; }
            set { optBuy.Checked = (value == OrderSide.Buy); optSell.Checked = (value == OrderSide.Sell); }
        }

        public bool PriceFixed
        {
            get { return optPriceFixed.Checked; }
            set { optPriceFixed.Checked = value; optPriceRandom.Checked = !value; }
        }

        public bool QtyFixed
        {
            get { return optQtyFixed.Checked; }
            set { optQtyFixed.Checked = value; optQtyRandom.Checked = !value; }
        }

        public bool PeriodFixed
        {
            get { return optPeriodFixed.Checked; }
            set { optPeriodFixed.Checked = value; optPeriodRandom.Checked = !value; }
        }

        public double PriceFrom
        {
            get { return (double)spinPriceFrom.Value; }
            set { spinPriceFrom.Value = (decimal)value; }
        }

        public double PriceTo
        {
            get { return (double)spinPriceTo.Value; }
            set { spinPriceTo.Value = (decimal)value; }
        }

        public double PriceStep
        {
            get { return (double)spinPriceStep.Value; }
            set { spinPriceStep.Value = (decimal)value; }
        }

        public int QtyFrom
        {
            get { return (int)spinQtyFrom.Value; }
            set { spinQtyFrom.Value = value; }
        }

        public int QtyTo
        {
            get { return (int)spinQtyTo.Value; }
            set { spinQtyTo.Value = value; }
        }

        public int QtyStep
        {
            get { return (int)spinQtyStep.Value; }
            set { spinQtyStep.Value = value; }
        }

        public int PeriodFrom
        {
            get { return (int)spinPeriodFrom.Value; }
            set { spinPeriodFrom.Value = value; }
        }

        public int PeriodTo
        {
            get { return (int)spinPeriodTo.Value; }
            set { spinPeriodTo.Value = value; }
        }

        private void chkOnOff_CheckedChanged(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new System.EventHandler(chkOnOff_CheckedChanged), sender, e);
            }
            else
            {
                bool start = chkOnOff.Checked;
                
                StartStopUI(start);

                if (start)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }

        private void StartStopUI(bool start)
        {
            if (start)
            {
                ReadValues();
            }

            chkOnOff.Text = (!start) ? "On" : "Off";

            
            foreach (Control control in new Control[] { grpPrice, grpPeriod, grpQty, optBuy, optSell, txtRIC, cmbTIF })
            {
                control.Enabled = !start;
            }
        }

        private void Start()
        {
            bool randomTimer = optPeriodRandom.Checked;

            if (randomTimer)
            {
                _timer = new ComplexTimer((int)Math.Ceiling(spinPeriodFrom.Value), (int)Math.Ceiling(spinPeriodTo.Value));                
            }
            else
            {
                _timer = new ComplexTimer((int)Math.Ceiling(spinPeriodFixed.Value));
            }

            _timer.TimerElapsed += new EventHandler(Timer_TimerElapsed);
            _timer.Start();
        }

        private void Timer_TimerElapsed(object sender, EventArgs e)
        {
            Order order = null;

            double price = CalculateNextPrice();
            int quantity = CalculateNextQty();

            order = OrderFactory.CreateOutgoingOrder(_shadowOrder);
            order.Price = price;
            order.Quantity = quantity;

            if (NewOrder != null)
            {
                NewOrder.BeginInvoke(this, new NewOrderGeneratedEventArgs(order), null, null);
            }
        }

        private bool _isPriceFixed, _isQtyFixed;
        private double _priceFixed, _priceFrom, _priceTo, _priceStep;
        private int _qtyFixed, _qtyFrom, _qtyTo, _qtyStep;
        private Order _shadowOrder;

        private int CalculateNextQty()
        {
            int res = 0;

            if (_isQtyFixed)
            {
                res = _qtyFixed;
            }
            else
            {
                int max = (int)((_qtyTo - _qtyFrom) / _qtyStep) + 1;
                int random = new Random().Next(max);

                res = random * _qtyStep + _qtyFrom;
            }

            return res;

        }

        private double CalculateNextPrice()
        {
            double res = 0.0;

            if (_isPriceFixed)
            {
                res = _priceFixed;
            }
            else
            {
                int max = (int)((_priceTo - _priceFrom) / _priceStep) + 1;
                int random = new Random().Next(max);

                res = (double)random * _priceStep + _priceFrom;
            }

            return res;

        }

        private void ReadValues()
        {
            _isPriceFixed = optPriceFixed.Checked;
            _isQtyFixed = optQtyFixed.Checked;

            _priceFixed = (double)spinPriceFixed.Value;
            _priceFrom = (double)spinPriceFrom.Value;
            _priceTo = (double)spinPriceTo.Value;
            _priceStep = (double)spinPriceStep.Value;

            _qtyFixed = (int)spinQtyFixed.Value;
            _qtyFrom = (int)spinQtyFrom.Value;
            _qtyTo = (int)spinQtyTo.Value;
            _qtyStep = (int)spinQtyStep.Value;

            _shadowOrder = new ShadowOrder();
            _shadowOrder.Instrument = txtRIC.Text;
            _shadowOrder.Type = (OrderType)Enum.Parse(typeof(OrderType), cmbTIF.Text);
            _shadowOrder.Side = (optBuy.Checked) ? OrderSide.Buy : OrderSide.Sell;
            _shadowOrder.Destination = "OPEX";
            _shadowOrder.Currency = "GBp";
        }        

        private void Stop()
        {
            if (_timer == null)
            {
                return;
            }

            _timer.Stop();
            _timer.TimerElapsed -= new EventHandler(Timer_TimerElapsed);
        }
    }

    public delegate void NewOrderGeneratedEventHandler(OrderGeneratorPanelControl sender, NewOrderGeneratedEventArgs args);
    public class NewOrderGeneratedEventArgs : System.EventArgs
    {
        private Order _order;

        public NewOrderGeneratedEventArgs(Order order)
            : base()
        {
            _order = order;
        }

        public Order Order { get { return _order; } }
    }
}
