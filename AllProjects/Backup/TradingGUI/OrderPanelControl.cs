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

using OPEX.TradingGUI.AlgoPanels;

namespace OPEX.TradingGUI
{
    enum OrderPanelControlMode
    {
        New,
        Sent
    }

    public delegate void AmendButtonPressedEventHandler(object sender, Order newOrder);

    public partial class OrderPanelControl : UserControl
    {
        private Logger _logger;        
        private string _lastAlgo;
        private Dictionary<string, VanillaAlgoPanel> _panels;
        private VanillaAlgoPanel _currentAlgoPanel;
        private ScrollableControl _currentPanel;
        private OrderPanelControlMode _mode;

        private Order _shadowOrder;
        private OutgoingOrder _outgoingOrder;

        public OrderPanelControl()
        {
            InitializeComponent();

            _logger = new Logger("OrderPanelControl");

            _panels = new Dictionary<string, VanillaAlgoPanel>();
            _panels.Add("Vanilla", new VanillaAlgoPanel());
            _panels.Add("StopLoss", new StopAlgoPanel());
            _currentPanel = pnlAlgoPlaceholder;
            _mode = OrderPanelControlMode.New;

            InitTIFCombo();
            InitAlgoCombo();
            UpdateButtons();

            if (!DesignMode)
            {
                _shadowOrder = new ShadowOrder();
            }
        }

        private void UpdateButtons()
        {
            bool isNew = _mode == OrderPanelControlMode.New;

            btnSend.Enabled = isNew;
            btnSend.Visible = isNew;

            btnAmend.Enabled = !isNew;
            btnCancel.Enabled = !isNew;
            btnAmend.Visible = !isNew;
            btnCancel.Visible = !isNew;
        }

        public Order Order
        {
            get
            {
                Order order = null;

                if (!this.DesignMode)
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
                if (this.DesignMode)
                {
                    return;
                }

                if (value != null)
                {
                    long newOrderId = -1;
                    if (value.Status == OrderStatus.NewOrder)
                    {
                        _shadowOrder = value;
                        _mode = OrderPanelControlMode.New;
                        newOrderId = _shadowOrder.OrderID;
                    }
                    else if (value is OutgoingOrder)
                    {
                        _outgoingOrder = (OutgoingOrder)value;
                        _mode = OrderPanelControlMode.Sent;
                        newOrderId = _outgoingOrder.OrderID;
                    }
                    else
                    {
                        throw new ApplicationException("Invalid order");
                    }

                    if (newOrderId >= 0)
                    {
                        txtOID.Text = newOrderId.ToString();
                    }
                }

                UpdateButtons();
                UpdateControls();
            }
        }

        private bool ValidateControls(ref string message)
        {
            OrderSide side = OrderSide.Buy;
            
            if (!(optBuy.Checked ^ optSell.Checked))
            {
                message = "One between Buy and Sell has to be checked.";
                return false;
            }
            if (optSell.Checked)
            {
                side = OrderSide.Sell;
            }

            string ric = txtRIC.Text;
            if (ric == null || ric.Length == 0)
            {
                message = "RIC can't be empty.";
                return false;
            }

            int quantity = 0;
            if (!Int32.TryParse(spinQty.Value.ToString(), out quantity))
            {
                message = "Quantity has to be an integer.";
                return false;
            }

            if (quantity < 1)
            {
                message = "Quantity needs to be greater than 1.";
                return false;
            }

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

            OrderType type;
            try
            {
                type = (OrderType) Enum.Parse(typeof(OrderType), cmbTIF.Text);
            }
            catch(Exception)
            {
                message = "Invalid TIF.";
                return false;
            }

            if (_mode == OrderPanelControlMode.Sent)
            {
                _shadowOrder = new ShadowOrder(_outgoingOrder);
            }

            _shadowOrder.Instrument = ric;
            _shadowOrder.Type = type;
            _shadowOrder.Side = side;
            _shadowOrder.Price = price;
            _shadowOrder.Quantity = quantity;
            _shadowOrder.Currency = "GBp";

            string destination = "OPEX";
            string parameters = string.Empty;
            if (_currentAlgoPanel != null)
            {
                destination = _currentAlgoPanel.Destination;
                parameters = _currentAlgoPanel.Parameters;
            }
            _shadowOrder.Destination = destination;
            _shadowOrder.Parameters = parameters;

            return true;
        }

        private void UpdateControls()
        {
            Order order = (_mode == OrderPanelControlMode.Sent) ? _outgoingOrder : _shadowOrder;

            if (order == null)
            {
                return;
            }

            bool isOpen = (_mode == OrderPanelControlMode.Sent) && (OrderStateMachine.IsActiveStatus(_outgoingOrder.Status));            
            bool isBuy = order.Side == OrderSide.Buy;

            optBuy.Checked = isBuy;
            optSell.Checked = !isBuy;           
            txtRIC.Text = order.Instrument;
            spinQty.Value = order.Quantity;
            spinPrice.Value = new decimal(order.Price);
            cmbTIF.Text = order.Type.ToString();

            if (isOpen && order.Parameters != null)
            {
                _currentAlgoPanel.Parameters = _shadowOrder.Parameters;
            }
        }

        private void InitTIFCombo()
        {
            cmbTIF.DataSource = Enum.GetValues(typeof(OrderType));
            cmbTIF.SelectedIndex = 1;
        }

        private void InitAlgoCombo()
        {
            cmbAlgo.Items.Add("Vanilla");
            cmbAlgo.Items.Add("StopLoss");
            cmbAlgo.SelectedIndex = 0;
        }

        private void optBuy_CheckedChanged(object sender, EventArgs e)
        {
            optSell.Checked = !optBuy.Checked;
        }

        private void optSell_CheckedChanged(object sender, EventArgs e)
        {
            optBuy.Checked = !optSell.Checked;
        }

        private void spinQty_ValueChanged(object sender, EventArgs e)
        {
            errorProvider1.Clear();
        }

        private void spinPrice_ValueChanged(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            SetStopPrice();
        }

        private void cmbTIF_SelectedIndexChanged(object sender, EventArgs e)
        {
            errorProvider1.Clear();
        }

        public event EventHandler EnterPressed;

        private void EntryKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (_mode == OrderPanelControlMode.New && EnterPressed != null)
                {
                    EnterPressed(this, null);
                }
            }
        }

        private void Swap(string newAlgoKey)
        {
            if (DesignMode)
            {
                return;
            }

            ScrollableControl oldControl = _currentPanel;
            VanillaAlgoPanel newControl = _panels[newAlgoKey];
            
            this.grpAlgoPanel.SuspendLayout();
            this.SuspendLayout();
            
            this.grpAlgoPanel.Controls.Remove(oldControl);
            this.grpAlgoPanel.Controls.Add(newControl);
            // 
            // pnlAlgoPlaceholder
            // 
            newControl.Location = new System.Drawing.Point(6, 19);
            newControl.Size = new System.Drawing.Size(273, 75);            

            this.grpAlgoPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

            grpAlgoPanel.Text = newAlgoKey;
            _currentPanel = newControl;
            _currentAlgoPanel = newControl;
        }

        private void cmbAlgo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string currentAlgo = cmbAlgo.Text;
            if (currentAlgo.Equals(_lastAlgo))
            {
                return;
            }

            switch (currentAlgo)
            {
                case "Vanilla":
                case "StopLoss":
                    Swap(currentAlgo);
                    _lastAlgo = currentAlgo;
                    if (currentAlgo.Equals("StopLoss"))
                    {
                        SetStopPrice();
                    }
                    break;
                default:
                    return;
            }
        }

        private void SetStopPrice()
        {
            string currentAlgo = cmbAlgo.Text;
            if (!currentAlgo.Equals("StopLoss"))
            {
                return;
            }
            AlgoPanels.StopAlgoPanel s = _panels["StopLoss"] as AlgoPanels.StopAlgoPanel;            
            double newPrice = (double)spinPrice.Value;
            s.StopPrice = newPrice;
        }

        public event EventHandler SendButtonPressed;
        public event EventHandler CancelButtonPressed;
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (CancelButtonPressed != null)
            {
                CancelButtonPressed(this, null);
            }

        }

        private void OrderPanelControl_Load(object sender, EventArgs e)
        {

        }
    }
}

