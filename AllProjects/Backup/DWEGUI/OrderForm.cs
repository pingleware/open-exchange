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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OPEX.Common;
using OPEX.OM.Common;
using OPEX.DWEAS.Client;

namespace OPEX.DWEGUI
{
    public partial class OrderForm : Form
    {
        private static bool CloseOnSendNew = true;
        private static bool CloseOnSendAmend = true;

        public enum Mode
        {
            Design,
            NewOrder,
            AmendOrder
        };

        private static readonly string DefaultDestination = "OPEX";

        private Logger _log;        
        private readonly string _myUser;
        private Mode _mode;
        private AssignmentBucket _assignmentBucket;
        private OutgoingOrder _outgoingOrder;
        private readonly Point _location;

        public OrderForm()
        {
            InitializeComponent();

            _mode = Mode.Design;
            _myUser = null;
            _location = Point.Empty;
        }      

        public OrderForm(string myUser, AssignmentBucket ab, Point location)
        {
            InitializeComponent();

            _myUser = myUser;
            _mode = Mode.NewOrder;
            _assignmentBucket = ab;
            _location = location;

            SetupOrder();

            SetFormTitle();
            SetLocation();
            _log.Trace(LogLevel.Method, "C'tor. Form OPENING.");
        }

        public OrderForm(string myUser, OutgoingOrder order, Point location)
        {
            InitializeComponent();

            _myUser = myUser;
            _outgoingOrder = order;
            _mode = Mode.AmendOrder;
            _location = location;

            orderPanel.Order = order;

            SetFormTitle();
            SetLocation();
            _log = new Logger(string.Format("OrderForm ({0}:{1})", _mode.ToString(), order.ClientOrderID));
            _log.Trace(LogLevel.Method, "C'tor. Form OPENING.");
        }

        private void SetLocation()
        {
            if (_location != Point.Empty)
            {
                this.Location = _location;                    
            }
        }

        private void orderPanel_SendButtonPressed(object sender, EventArgs e)
        {
            OutgoingOrder o = OrderFactory.CreateOutgoingOrder(orderPanel.Order);
            o.User = _myUser;
            
            OrderMonitor.Instance.Hook(o);
            OrderMonitor.Instance.Send(o);
        }

        private void orderPanel_AmendButtonPressed(object sender, Order newOrder)
        {
            OutgoingOrder o = (OutgoingOrder)orderPanel.Order;
            o.Amend(newOrder.Quantity, newOrder.Price, newOrder.Parameters);
        }

        private void orderPanel_CancelButtonPressed(object sender, Order newOrder)
        {
            OutgoingOrder o = (OutgoingOrder)orderPanel.Order;
            o.Cancel();
        }

        public void OnOrderAccepted(object sender, Order order)
        {            
            OutgoingOrder o = sender as OutgoingOrder;

            if (o == null || CloseOnSendNew)
            {
                SafeClose();
                return;
            }

            _log.Trace(LogLevel.Method, "OnOrderSent. order: {0}", o.ToString());

            _outgoingOrder = o;                
            orderPanel.Order = _outgoingOrder;            
            SwitchMode();
        }

        private void OnOrderSwitchedToActiveState(object sender, Order order)
        {
            OutgoingOrder o = sender as OutgoingOrder;

            if (o == null)
            {
                SafeClose();
                return;
            }

            if (CloseOnSendAmend 
                && (o.Status == OrderStatus.AmendAccepted)
                && (_mode == Mode.AmendOrder))
            {
                SafeClose();
                return;
            }

            if (_mode == Mode.AmendOrder)
            {
                orderPanel.EnableDisableButton(true, ButtonType.Amend);
            }
            else if (_mode == Mode.NewOrder)
            {
                orderPanel.EnableDisableButton(true, ButtonType.Send);
            }
                
            _log.Trace(LogLevel.Method, "OnOrderSwitchedToActiveState. order: {0}", o.ToString());
        }

        private void OnOrderSwitchedToClosedState(object sender, Order order)
        {
            OutgoingOrder o = sender as OutgoingOrder;

            if (o != null)
            {
                _log.Trace(LogLevel.Method, "OnOrderSwitchedToClosedState. order: {0}", o.ToString());
                OrderMonitor.Instance.Unhook(o);
            }            

            SafeClose();
        }

        private void OnOrderFilled(object sender, Order order)
        {
            OutgoingOrder o = sender as OutgoingOrder;

            if (o != null)
            {
                _log.Trace(LogLevel.Method, "OnOrderFilled. order: {0}", o.ToString());

                if (o.QuantityRemaining == 0)
                {
                    OrderMonitor.Instance.Unhook(o);
                    SafeClose();
                }
                else
                {
                    if (_mode == Mode.AmendOrder)
                    {
                        orderPanel.EnableDisableButton(true, ButtonType.Amend);
                    }
                    else if (_mode == Mode.NewOrder)
                    {
                        orderPanel.EnableDisableButton(true, ButtonType.Send);
                    }
                }
            }              
        }

        private void OnOrderRejected(object sender, Order order)
        {
            OutgoingOrder o = sender as OutgoingOrder;

            if (o == null)
            {
                SafeClose();
                return;
            }

            OrderMonitor.Instance.Unhook(o);

            MessageBox.Show("Your order was rejected: " + o.Message);

            SetupOrder();
        }

        private void SafeClose()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SafeUpdateDelegate(SafeClose));
            }
            else
            {
                this.Close();
            }
        }

        private void SwitchMode()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SafeUpdateDelegate(SwitchMode));
            }
            else
            {
                if (_mode != Mode.NewOrder)
                {
                    return;
                }

                _mode = Mode.AmendOrder;
                SetFormTitle();
            }
        }

        delegate void SafeUpdateDelegate();
        private void SetFormTitle()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SafeUpdateDelegate(SetFormTitle));
            }
            else
            {
                string title = "OrderForm";

                if (_mode == Mode.AmendOrder)
                {
                    title = "Amend Order";
                }
                else if (_mode == Mode.NewOrder)
                {
                    title = "New Order";
                }

                this.Text = title;
            }
        }

        private void OrderForm_Load(object sender, EventArgs e)
        {
            OrderMonitor.Instance.OrderAccepted += new OutgoingOrderEventHandler(OnOrderAccepted);
            OrderMonitor.Instance.OrderSwitchedToActiveState += new OutgoingOrderEventHandler(OnOrderSwitchedToActiveState);
            OrderMonitor.Instance.OrderSwitchedToClosedState += new OutgoingOrderEventHandler(OnOrderSwitchedToClosedState);
            OrderMonitor.Instance.OrderFilled += new OutgoingOrderEventHandler(OnOrderFilled);
            OrderMonitor.Instance.OrderRejected += new OutgoingOrderEventHandler(OnOrderRejected);

            if (_mode == Mode.AmendOrder)
            {
                chkCloseOnSend.Checked = CloseOnSendAmend;
            }
            else if (_mode == Mode.NewOrder)
            {
                chkCloseOnSend.Checked = CloseOnSendNew;
            }
            else
            {
                chkCloseOnSend.Enabled = false;                
            }
        }

        private void OrderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseOrderForm();
        }

        private void CloseOrderForm()
        {
            _log.Trace(LogLevel.Method, "CloseOrderForm. Form CLOSING.");

            OrderMonitor.Instance.OrderAccepted -= new OutgoingOrderEventHandler(OnOrderAccepted);
            OrderMonitor.Instance.OrderSwitchedToActiveState -= new OutgoingOrderEventHandler(OnOrderSwitchedToActiveState);
            OrderMonitor.Instance.OrderSwitchedToClosedState -= new OutgoingOrderEventHandler(OnOrderSwitchedToClosedState);
            OrderMonitor.Instance.OrderFilled -= new OutgoingOrderEventHandler(OnOrderFilled);
            OrderMonitor.Instance.OrderRejected -= new OutgoingOrderEventHandler(OnOrderRejected);
        }

        private void SetupOrder()
        {
            ShadowOrder o = new ShadowOrder();
            string ric = _assignmentBucket.RIC;
            OrderSide side = _assignmentBucket.Side;
            string ccy = _assignmentBucket.CCY;
            o.Price = _assignmentBucket.Price;
            o.LimitPrice = _assignmentBucket.Price;
            o.Quantity = _assignmentBucket.QtyRem;
            o.Instrument = ric;
            o.Type = OrderType.Limit;
            o.Side = side;
            o.Destination = DefaultDestination;
            o.Currency = ccy;
            orderPanel.Order = o;
            orderPanel.SetQtyLimits(1, _assignmentBucket.QtyRem);

            if (_log == null)
            {
                _log = new Logger(string.Format("OrderForm ({0}:{1})", _mode.ToString(), o.ClientOrderID));
            }
            else
            {
                _log.LogTitle = string.Format("OrderForm ({0}:{1})", _mode.ToString(), o.ClientOrderID);
            }
        }

        private void OrderForm_Shown(object sender, EventArgs e)
        {
            orderPanel.HighlightAndFocus();
        }

        private void chkCloseOnSend_CheckedChanged(object sender, EventArgs e)
        {
            if (this._mode == Mode.AmendOrder)
            {
                CloseOnSendAmend = chkCloseOnSend.Checked;
            }
            else if (this._mode == Mode.NewOrder)
            {
                CloseOnSendNew = chkCloseOnSend.Checked;
            }
        }
    }
}
