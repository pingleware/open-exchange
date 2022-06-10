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
using OPEX.MDS.Common;
using OPEX.MDS.Client;
using OPEX.TDS.Client;
using OPEX.TDS.Common;
using OPEX.Configuration.Client;
using OPEX.SupplyService.Common;
using OPEX.StaticData;
using OPEX.Storage;

namespace OPEX.TradingGUI
{
    public partial class MainForm : Form, IOrderChangesSink
    {
        private string DefaultRIC = "TTECH.L";
        private static readonly string DefaultDestination = "OPEX";
        private MainLogger _log;
        private OutgoingOrderDuplexChannel _outChannel;
        private Dictionary<long, Order> _myOrdersByOrderID;        

        public MainForm()
        {
            _log = new GUILog();
            InitializeComponent();

            _myOrdersByOrderID = new Dictionary<long, Order>();
            orderPanelControl1.SendButtonPressed += new EventHandler(orderPanelControl1_SendButtonPressed);
            orderPanelControl1.CancelButtonPressed += new EventHandler(orderPanelControl1_CancelButtonPressed);
            orderPanelControl1.AmendButtonPressed += new AmendButtonPressedEventHandler(orderPanelControl1_AmendButtonPressed);

            this.DoubleBuffered = true;
        }

        private string _OPEXApplicationName;
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                _outChannel = new OutgoingOrderDuplexChannel(OrderFactory.OMClientName);

                bool purge = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("PurgeQueuesOnStartup", "true"));
                if (purge)
                {
                    _outChannel.Purge();
                }
                OrderFactory.OrderSender = _outChannel;
                _outChannel.Start();

                MarketDataClient.Instance.Start();

                TradeDataClient.Instance.TradeMessageReceived += new TradeMessageReceivedEventHandler(TradeMessageReceived);
                TradeDataClient.Instance.Start();

                string applicationName = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null);
                if (applicationName != null && applicationName.Length > 0)
                {
                    this.Text += string.Format(" ({0})", applicationName);
                }

                DefaultRIC = ConfigurationClient.Instance.GetConfigSetting("DefaultRIC", "TTECH.L");

                StaticDataManager.Instance.Load();
                string[] instruments = StaticDataManager.Instance.InstrumentStaticData.Instruments;
                cmbDepthInstrument.Items.AddRange(instruments);
                cmbDepthInstrument.SelectedIndex = 0;
                _OPEXApplicationName = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", "OPEXApplication");
            }
            orderPanelControl1.EnterPressed += new EventHandler(OrderPanelControl1_EnterPressed);
        }

        void NewAssignmentReceived(object sender, Assignment assignment)
        {
            MessageBox.Show(string.Format("New assignment received: {0}", assignment.ToString()));                
        }

        void OrderPanelControl1_EnterPressed(object sender, EventArgs e)
        {
            SendOrderPanelOrder();
        }

        void orderPanelControl1_CancelButtonPressed(object sender, EventArgs e)
        {
            OutgoingOrder o = (OutgoingOrder) orderPanelControl1.Order;
            o.Cancel();
        }

        void orderPanelControl1_AmendButtonPressed(object sender, Order newOrder)
        {
            OutgoingOrder o = (OutgoingOrder) orderPanelControl1.Order;
            o.Amend(newOrder.Quantity, newOrder.Price, newOrder.Parameters);
        }

        void orderPanelControl1_SendButtonPressed(object sender, EventArgs e)
        {
            SendOrderPanelOrder();
        }

        private void SendOrderPanelOrder()
        {
            OutgoingOrder o = OrderFactory.CreateOutgoingOrder(orderPanelControl1.Order);            
            HookAndSend(o);
            if (o.IsSent)
            {
                orderPanelControl1.Order = o;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseGUI();
        }

        private void HookAndSend(OutgoingOrder o)
        {
            Hook(o);
            o.Send();
        }

        private void Hook(OutgoingOrder o)
        {
            o.StatusChanged += new OutgoingOrderEventHandler(OnStatusChanged);
            o.OrderSent += new OutgoingOrderEventHandler(OnOrderSent);
        }

        public void OnOrderSent(object sender, Order order)
        {
            Order o = sender as Order;
            _log.Trace(LogLevel.Debug, "Order sent {0}", o.ToString());
            orderBlotter1.AddOrder(o);
        }

        private void Unhook(OutgoingOrder o)
        {
            o.StatusChanged -= new OutgoingOrderEventHandler(OnStatusChanged);
            o.OrderSent -= new OutgoingOrderEventHandler(OnOrderSent);
        }

        private void UnhookAll()
        {
            foreach (OutgoingOrder o in OrderFactory.OutgoingOrders.Values)
            {
                Unhook(o);
            }
        }

        public void OnStatusChanged(object sender, Order newOrder)
        {
            OutgoingOrder order = sender as OutgoingOrder;
            _myOrdersByOrderID[order.OrderID] = order;

            _log.Trace(LogLevel.Debug, "Status changed {0}", order.ToString());
            orderBlotter1.AddOrder(order);
        }                  

        private void orderGeneratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OrderGeneratorDialog g = new OrderGeneratorDialog();
            g.OrderChangesSink = this;
            g.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShadowOrder o = new ShadowOrder();

            o.Price = 200;
            o.Quantity = 1;
            o.Instrument = cmbDepthInstrument.SelectedItem.ToString();
            o.Type = OrderType.Limit;
            o.Side = OrderSide.Buy;
            o.Destination = DefaultDestination;
            o.Currency = "GBp";
            o.User = _OPEXApplicationName;            

            orderPanelControl1.Order = o;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CloseGUI()
        {
            orderPanelControl1.EnterPressed -= new EventHandler(OrderPanelControl1_EnterPressed);

            _outChannel.Stop();
            MarketDataClient.Instance.Stop();
            
            TradeDataClient.Instance.Stop();
            TradeDataClient.Instance.TradeMessageReceived -= new TradeMessageReceivedEventHandler(TradeMessageReceived);

            DBConnectionManager.Instance.Disconnect();            

            _log.Dispose();
        }

        bool IsMyOrder(long orderID)
        {
            return _myOrdersByOrderID.ContainsKey(orderID);
        }

        void TradeMessageReceived(object sender, TradeDataMessage tradeDataMessage)
        {
            if (IsMyOrder(tradeDataMessage.OrderID))
            {
                tradeBlotter1.AddTrade(tradeDataMessage);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutTradingGUI about = new AboutTradingGUI();
            about.Show();
        }

        private void orderBlotter1_OrderSelected(object sender, Order o)
        {
            if (o == null)
            {
                tradeBlotter1.ClearFilter();
            }
            else
            {
                tradeBlotter1.ApplyFilter(o.OrderID);
                if (IsMyOrder(o.OrderID))
                {
                    OutgoingOrder outgoingOrder = (OutgoingOrder) _myOrdersByOrderID[o.OrderID];
                    orderPanelControl1.Order = outgoingOrder;
                }
            }
        }

        private void tradeBlotter1_Load(object sender, EventArgs e)
        {
            tradeBlotter1.ApplicationName = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null);
        }

        private void clearDepthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AggregatedDepthSnapshot snapshot = depthPanelControl1.LastSnapshot;

            if (snapshot == null)
            {
                return;
            }

            foreach (AggregatedDepthSide side in new AggregatedDepthSide[] { snapshot.Buy, snapshot.Sell })
            {
                int size = 0;
                OrderSide s = OrderSide.Buy;
                foreach (AggregatedQuote quote in side)
                {
                    s = (quote.Side == OrderSide.Buy) ? OrderSide.Sell : OrderSide.Buy;
                    size += quote.Quantity;
                }
                if (size > 0)
                {
                    OutgoingOrder o = OrderFactory.CreateOutgoingOrder();
                    o.Side = s;
                    o.Quantity = size;
                    o.Instrument = DefaultRIC;
                    o.Type = OrderType.Market;
                    o.Destination = DefaultDestination;
                    o.Currency = "GBp";
                    HookAndSend(o);
                }
            }
        }

        private void setupDepthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[] prices = new double[] { 99.7, 99.8, 99.9, 100.3, 100.2, 100.1 };
            int[] quantities = new int[] { 10000, 5000, 1000, 10000, 5000, 1000 };

            for (int i = 0; i < prices.Length; ++i)
            {
                OutgoingOrder o = OrderFactory.CreateOutgoingOrder();

                o.Price = prices[i];
                o.Quantity = quantities[i];
                o.Instrument = DefaultRIC;
                o.Type = OrderType.Limit;
                o.Side = (o.Price < 100) ? OrderSide.Buy : OrderSide.Sell;
                o.Destination = DefaultDestination;
                o.Currency = "GBp";
                HookAndSend(o);
            }
        }        

        private void cmbDepthInstrument_SelectionChangeCommitted(object sender, EventArgs e)
        {
            depthPanelControl1.Instrument = cmbDepthInstrument.SelectedItem.ToString();
        }

        private void cmbDepthInstrument_SelectedValueChanged(object sender, EventArgs e)
        {
            depthPanelControl1.Instrument = cmbDepthInstrument.SelectedItem.ToString();
        }

        private void cmbDepthInstrument_SelectedIndexChanged(object sender, EventArgs e)
        {
            depthPanelControl1.Instrument = cmbDepthInstrument.SelectedItem.ToString();
        } 
    }
}
