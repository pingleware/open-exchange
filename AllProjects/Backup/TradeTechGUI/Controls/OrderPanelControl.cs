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

namespace OPEX.SalesGUI
{
    public enum OrderPanelControlMode
    {
        New,
        Sent,
        Idle
    }

    public delegate void AmendButtonPressedEventHandler(object sender, Order newOrder);

    public partial class OrderPanelControl : UserControl
    {
        private readonly int MINPRICE = 0;
        private readonly int MAXPRICE = 400;
        private readonly Color BUYColor = System.Drawing.Color.LightSteelBlue;
        private readonly Color SELLColor = System.Drawing.Color.LightPink;
        private readonly Logger _logger;
        private readonly Control[] ColoredControls;        
        
        private OrderPanelControlMode _mode;
        private Order _shadowOrder;
        private OutgoingOrder _outgoingOrder;
        private long _lastOrderID;
        private OrderSide _side;
        private double _limitPrice;

        public OrderPanelControl()
        {
            InitializeComponent();            
            
            if (IsDesignerHosted)
            {
                return;
            }

            _logger = new Logger("OrderPanelControl");
            _mode = OrderPanelControlMode.Idle;                        
            _shadowOrder = new ShadowOrder();
            UpdateButtons();
            ColoredControls = new Control[] { txtOID, txtRIC, txtLimitPrice, btnSend, btnAmend, spinPrice };
        }

        public OrderPanelControlMode Mode { get { return _mode; } }

        /// <summary>
        /// Gets if the control is in design mode, or if any of its
        /// parents are in design mode.
        /// </summary>
        public bool IsDesignerHosted
        {
            get
            {
                Control ctrl = this;
                while (ctrl != null)
                {
                    if (ctrl.Site == null)
                        return false;
                    if (ctrl.Site.DesignMode == true)
                        return true;
                    ctrl = ctrl.Parent;
                }
                return false;
            }
        }

        public Order Order
        {
            get
            {
                Order order = null;

                if (!IsDesignerHosted)
                {
                    string message = null;
                    if (ValidateControls(ref message))
                    {
                        if (_mode == OrderPanelControlMode.New)
                        {
                            order = _shadowOrder;
                        }
                        else
                        {
                            order = _outgoingOrder;
                        }
                    }
                    else
                    {
                        errorProvider1.SetError(this, message);
                    }
                }

                return order;
            }

            set
            {
                if (IsDesignerHosted)
                {
                    return;
                }
                bool dontUpdatePriceBox = false;
                if (value != null)
                {                    
                    if (value.Status == OrderStatus.NewOrder)
                    {
                        _shadowOrder = value;
                        _mode = OrderPanelControlMode.New;
                        _lastOrderID = _shadowOrder.OrderID;
                    }
                    else if (value is OutgoingOrder)
                    {
                        _outgoingOrder = (OutgoingOrder)value;
                        _mode = OrderPanelControlMode.Sent;
                        _lastOrderID = _outgoingOrder.OrderID;
                    }
                    else
                    {
                        throw new ApplicationException("Invalid order");
                    }

                    
                    if (value.Status == OrderStatus.Rejected || value.Status == OrderStatus.AmendRejected)
                    {
                        dontUpdatePriceBox = true;
                    }
                    else
                    {
                        _limitPrice = value.LimitPrice;
                    }
                    _side = value.Side;
                    _logger.Trace(LogLevel.Debug, "OrderPanelControl.set_Order. limitPrice: {0} side {1} order {2}", _limitPrice, _side, value.ToString());
                }
                else
                {
                    _mode = OrderPanelControlMode.Idle;
                    _shadowOrder = null;
                }

                UpdateButtons();
                UpdateControls(dontUpdatePriceBox);
                UpdateDirection();
            }
        }

        public void Clear()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDelegate(Clear));
            }
            else
            {
                _mode = OrderPanelControlMode.Idle;
                ResetSpinBoxLimits();
                spinPrice.Value = 0;
                txtOID.Text = "0";
                UpdateButtons();
            }
        }        

        private delegate void UpdateDelegate();
        private void UpdateDirection()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDelegate(UpdateDirection));
            }
            else
            {
                Color c = System.Drawing.SystemColors.Control;
                if (_mode != OrderPanelControlMode.Idle)
                {
                    c = (_side == OrderSide.Buy) ? BUYColor : SELLColor;
                }

                foreach (Control ctrl in ColoredControls)
                {
                    if (ctrl == spinPrice && c == System.Drawing.SystemColors.Control)
                    {                        
                        c = System.Drawing.SystemColors.Window;
                    }
                    ctrl.BackColor = c;
                }
            }
        }                
        private void UpdateButtons()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDelegate(UpdateButtons));
            }
            else
            {
                bool viewSend = _mode == OrderPanelControlMode.New;
                bool viewAmend = _mode == OrderPanelControlMode.Sent;

                btnSend.Enabled = viewSend;
                btnSend.Visible = viewSend;

                btnAmend.Enabled = viewAmend;
                btnAmend.Visible = viewAmend;

                spinPrice.Enabled = (_mode != OrderPanelControlMode.Idle);
            }
        }

        private void ResetSpinBoxLimits()
        {
            spinPrice.Minimum = MINPRICE;
            spinPrice.Maximum = MAXPRICE;
        }

        delegate void UpdateControlsDelegate(bool p);
        private void UpdateControls(bool dontUpdatePriceBox)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateControlsDelegate(UpdateControls), dontUpdatePriceBox);
            }
            else
            {
                Order order = null;

                if (_mode == OrderPanelControlMode.Sent)
                {
                    order = _outgoingOrder;
                }
                else if (_mode == OrderPanelControlMode.New)
                {
                    order = _shadowOrder;
                }

                if (order == null)
                {
                    ResetSpinBoxLimits();
                    return;
                }                

                bool isBuy = order.Side == OrderSide.Buy;                
                decimal limit = new decimal(_limitPrice);
                decimal price = new decimal(order.Price);

                _logger.Trace(LogLevel.Debug, "UpdateControls. limit{0} price {1} order: {2}", limit, price, order.ToString());

                txtRIC.Text = order.Instrument;
                txtLimitPrice.Text = order.LimitPrice.ToString();

                if (isBuy)
                {
                    btnSend.Text = "BUY";
                    btnSend.BackColor = BUYColor;                    
                    spinPrice.Maximum = limit;                    
                }
                else
                {
                    btnSend.Text = "SELL";
                    btnSend.BackColor = SELLColor;                    
                    spinPrice.Minimum = limit;
                }

                if (!dontUpdatePriceBox)
                {
                    spinPrice.Value = price;
                    if (isBuy)
                    {
                        spinPrice.Maximum = limit;
                    }
                    else
                    {
                        spinPrice.Minimum = limit;
                    }
                }

                if (_lastOrderID > 0)
                {
                    txtOID.Text = _lastOrderID.ToString();
                }
                else
                {
                    txtOID.Text = string.Empty;
                }
            }
        }

        private bool ValidateControls(ref string message)
        {            
            string ric = txtRIC.Text;
            if (ric == null || ric.Length == 0)
            {
                message = "RIC can't be empty.";
                return false;
            }

            int quantity = 1;
            double price = 0;
            if (!Double.TryParse(spinPrice.Value.ToString(), out price))
            {
                message = "Price has to be a double.";
                return false;
            }

            if (price < 0)
            {
                message = "Price needs to be positive.";
                return false;
            }         

            if (_mode == OrderPanelControlMode.Sent)
            {
                _shadowOrder = new ShadowOrder(_outgoingOrder);
            }

            _shadowOrder.Instrument = ric;
            _shadowOrder.Type = OrderType.Limit;
            _shadowOrder.Side = _side;
            _shadowOrder.Price = price;
            _shadowOrder.LimitPrice = _limitPrice;
            _shadowOrder.Quantity = quantity;
            _shadowOrder.Currency = "GBp";

            string destination = "OPEX";
            string parameters = string.Empty;

            _shadowOrder.Destination = destination;
            _shadowOrder.Parameters = parameters;

            return true;
        }              

        private void EntryKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if ((_mode == OrderPanelControlMode.New  || _mode == OrderPanelControlMode.Sent))
                {
                    OnEnterPressed();
                }
            }
        }

        private void OnEnterPressed()
        {
            if (this.Mode == OrderPanelControlMode.New)
            {
                btnSend_Click(this, null);
            }
            else if (this.Mode == OrderPanelControlMode.Sent)
            {
                btnAmend_Click(this, null);
            }
        }

        public event EventHandler SendButtonPressed;        
        public event AmendButtonPressedEventHandler AmendButtonPressed;

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (SendButtonPressed != null)
            {
                SendButtonPressed(this, null);
            }
        }

        private void btnAmend_Click(object sender, EventArgs e)
        {
            if (AmendButtonPressed != null)
            {
                string m = null;
                if (!ValidateControls(ref m))
                {
                    MessageBox.Show("Error while amending order: " + m);
                }
                else
                {
                    AmendButtonPressed(this, _shadowOrder);
                }                
            }            
        }
    }
}

