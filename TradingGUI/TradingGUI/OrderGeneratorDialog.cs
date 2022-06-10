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

using OPEX.OM.Common;
using OPEX.Configuration.Client;

namespace OPEX.TradingGUI
{
    public partial class OrderGeneratorDialog : Form
    {
        private GraphForm _graphForm;
        private IOrderChangesSink _sink;
        private string DefaultRIC;

        internal IOrderChangesSink OrderChangesSink
        {
            get { return _sink; }
            set { _sink = value; }
        }

        public OrderGeneratorDialog()
        {
            InitializeComponent();

            DefaultRIC = ConfigurationClient.Instance.GetConfigSetting("DefaultRIC", "TTECH.L");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ToggleGenerators(true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ToggleGenerators(false);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //setup
            Setup();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //clear
            Clear();
        }

        private void ToggleGenerators(bool onOff)
        {
            if (onOff)
            {
                if (_graphForm == null)
                {
                    _graphForm = new GraphForm();
                }
                _graphForm.Show();
                _graphForm.Instrument = DefaultRIC;
            }

            foreach (OrderGeneratorPanelControl orderGeneratorPanelControl in new OrderGeneratorPanelControl[] {
                    orderGeneratorPanelControl1, orderGeneratorPanelControl2})
            {
                orderGeneratorPanelControl.Running = onOff;
            }

            if (!onOff && _graphForm != null)
            {
                _graphForm.Instrument = null;
                _graphForm.Close();
                _graphForm = null;
            }
        }

        void Setup()
        {
            OrderGeneratorPanelControl p = orderGeneratorPanelControl1;

            p.RIC = DefaultRIC;
            p.TIF = OrderType.Limit;
            p.Side = OrderSide.Buy;

            p.PriceFixed = false;
            p.PriceFrom = 99.7;
            p.PriceTo = 100.3;
            p.PriceStep = 0.1;

            p.QtyFixed = false;
            p.QtyFrom = 50;
            p.QtyTo = 150;
            p.QtyStep = 1;

            p.PeriodFixed = false;
            p.PeriodFrom = 250;
            p.PeriodTo = 500;

            OrderGeneratorPanelControl p2 = orderGeneratorPanelControl2;

            p2.RIC = DefaultRIC;
            p2.TIF = OrderType.Limit;
            p2.Side = OrderSide.Sell;

            p2.PriceFixed = false;
            p2.PriceFrom = 99.7;
            p2.PriceTo = 100.3;
            p2.PriceStep = 0.1;

            p2.QtyFixed = false;
            p2.QtyFrom = 50;
            p2.QtyTo = 150;
            p2.QtyStep = 1;

            p2.PeriodFixed = false;
            p2.PeriodFrom = 250;
            p2.PeriodTo = 500;
        }

        void Clear()
        {
            OrderGeneratorPanelControl p = orderGeneratorPanelControl1;
            OrderGeneratorPanelControl p2 = orderGeneratorPanelControl2;

            p.RIC = p2.RIC = "";
            p.TIF = p2.TIF = OrderType.Limit;
            p.Side = p2.Side = OrderSide.Buy;

            p.PriceFixed = p2.PriceFixed = true;
            p.PriceFrom = p2.PriceFrom = 0;
            p.PriceTo = p2.PriceTo = 0;
            p.PriceStep = p2.PriceStep = 0.0;

            p.QtyFixed = p2.QtyFixed = true;
            p.QtyFrom = p2.QtyFrom = 0;
            p.QtyTo = p2.QtyTo = 0;
            p.QtyStep = p2.QtyStep = 0;

            p.PeriodFixed = p2.PeriodFixed = false;
            p.PeriodFrom = p2.PeriodFrom = 0;
            p.PeriodTo = p2.PeriodTo = 0;
        }

        private void OrderGeneratorDialog_Load(object sender, EventArgs e)
        {
            orderGeneratorPanelControl1.NewOrder += new NewOrderGeneratedEventHandler(OrderGenerator_NewOrder);
            orderGeneratorPanelControl2.NewOrder += new NewOrderGeneratedEventHandler(OrderGenerator_NewOrder);

        }

        private void OrderGenerator_NewOrder(OrderGeneratorPanelControl sender, NewOrderGeneratedEventArgs args)
        {
            if (sender == null || args == null)
            {
                return;
            }

            OutgoingOrder order = OrderFactory.CreateOutgoingOrder(args.Order);
            if (order == null)
            {
                return;
            }

            HookAndSend(order);
        }

        private void HookAndSend(OutgoingOrder o)
        {
            Hook(o);
            o.Send();
        }

        private void Hook(OutgoingOrder o)
        {
            o.StatusChanged += new OutgoingOrderEventHandler(_sink.OnStatusChanged);
            o.OrderSent += new OutgoingOrderEventHandler(_sink.OnOrderSent);
        }

        private void Unhook(OutgoingOrder o)
        {
            o.StatusChanged -= new OutgoingOrderEventHandler(_sink.OnStatusChanged);
            o.OrderSent -= new OutgoingOrderEventHandler(_sink.OnOrderSent);
        }

        private void UnhookAll()
        {
            foreach (OutgoingOrder o in OrderFactory.OutgoingOrders.Values)
            {
                Unhook(o);
            }
        }

        private void OrderGeneratorDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            orderGeneratorPanelControl1.NewOrder -= new NewOrderGeneratedEventHandler(OrderGenerator_NewOrder);
            orderGeneratorPanelControl2.NewOrder -= new NewOrderGeneratedEventHandler(OrderGenerator_NewOrder);
        }
    }
}
