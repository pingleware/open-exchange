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
using System.Speech.Synthesis;

using OPEX.Common;
using OPEX.OM.Common;
using OPEX.MDS.Common;
using OPEX.MDS.Client;
using OPEX.TDS.Client;
using OPEX.TDS.Common;
using OPEX.Configuration.Client;
using OPEX.SupplyService.Common;

namespace OPEX.SalesGUI
{
    public partial class MainForm : Form
    {
        private static readonly string DefaultDestination = "OPEX";
        private static readonly string DefaultRic = "TTECH.L";
        private readonly string MyUser;

        private readonly Logger _log;
        private readonly Services _services;        
        private readonly Dictionary<long, Order> _myOrdersByOrderID;
        private readonly Dictionary<long, Assignment> _assignmentsByOID;
        private readonly AssignmentManager _assignmentManager;        
        private readonly Blinking _blinkOnNewOrder;        
        private readonly BlinkingScheduler _blinkingScheduler;
        private readonly SpeechSynthesizer speech = new SpeechSynthesizer();
        private readonly bool _speak;

        public MainForm()
        {
            InitializeComponent();

            if (DesignMode)
            {
                return;                
            }

            _log = new Logger("TradingGUI");

            _assignmentManager = new AssignmentManager();
            _assignmentManager.NewAssignmentsAvailable += new NewAssignmentsAvailableEventHandler(OnNewAssignmentAvailable);             

            _services = Services.Instance;

            _myOrdersByOrderID = new Dictionary<long, Order>();
            _assignmentsByOID = new Dictionary<long, Assignment>();

            MyUser = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", "OPEXApplication");

            _speak = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("SpeakSessionState", "false"));

            _blinkOnNewOrder = new Blinking(infoPanel1.MessageTextBox, grpOrder, Color.Blue, SystemColors.Control);            
            _blinkingScheduler = new BlinkingScheduler();
            _blinkingScheduler.Start();            
            this.DoubleBuffered = true;
        }
      

        private System.Threading.Timer _initialRequestTimer;
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }
            
            _services.TradeMessageReceived += new TradeMessageReceivedEventHandler(TradeMessageReceived);
            _services.Start();

            MarketDataClient.Instance.SessionChanged += new SessionChangedEventHandler(SessionChanged);            

            string applicationName = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null);
            if (applicationName != null && applicationName.Length > 0)
            {
                this.Text += string.Format(" ({0})", applicationName);
            }

            orderPanelControl1.Order = null;
            depthPanelControl1.Instrument = DefaultRic;
            grpDepth.Text = string.Format("Orderbook({0})", DefaultRic);
            _initialRequestTimer = 
                new System.Threading.Timer(new System.Threading.TimerCallback(InitialStatusRequest), null, 1000, System.Threading.Timeout.Infinite); 
        }

        void InitialStatusRequest(object state)
        {
            _log.Trace(LogLevel.Info, "InitialStatusRequest. Requesting MARKET STATUS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");            
            MarketDataClient.Instance.RequestStatus(MarketDataClient.DefaultDataSource);
        }

        private bool _connecting = true;
        private SessionChangedEventArgs _currentSessionState;
        void SessionChanged(object sender, SessionChangedEventArgs args)
        {
            _log.Trace(LogLevel.Info, "SessionChanged. {0} ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~", args.ToString());

            if (_speak)
            {
                speech.SpeakAsync(string.Format("{0}! {1}!", args.SessionState.ToString(), Environment.MachineName));
            }

            _currentSessionState = args;

            if (!args.ServerAlive)
            {
                _log.Trace(LogLevel.Warning, "SessionChanged. EXCHANGE UNAVAILABLE. Trying to RECONNECT... ##############################################");
                MarketDataClient.Instance.RequestStatus(MarketDataClient.DefaultDataSource);
                return;
            }

            if (_connecting)
            {
                _log.Trace(LogLevel.Info, "SessionChanged. The GUI is connecting: calling WakeUp() ~~~~~~~~~~");
                Wakeup();
            }

            if (args.SessionState == SessionState.Close)
            {
                orderPanelControl1.Order = null;                                
                _pnlCurrent = 0;
                orderPanelControl1.Clear();
                _blinkingScheduler.Enabled = false;
                _blinkingScheduler.Flush();
                _assignmentManager.Flush();                
                return;
            }

            _ordersExecuted = 0;
            _blinkingScheduler.Enabled = true;
            UpdateInfoPanelTimes(args.StartTime, args.EndTime);
        }

        delegate void UpdateDelegate();
        private void Wakeup()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDelegate(Wakeup));
            }
            else
            {                
                _log.Trace(LogLevel.Method, "Wakeup. ##############################################");
                infoPanel1.StartTime = _currentSessionState.StartTime;
                infoPanel1.EndTime = _currentSessionState.EndTime;
                _connecting = false;
            }
        }

        private delegate void UpdateInfoPanelTimesDelegate(DateTime start, DateTime end);
        void UpdateInfoPanelTimes(DateTime start, DateTime end)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateInfoPanelTimesDelegate(UpdateInfoPanelTimes), start, end);
            }
            else
            {
                infoPanel1.OrdersRemaining = 0;
                infoPanel1.StartTime = start;
                infoPanel1.EndTime = end;
            }
        }

        private readonly Dictionary<long, bool> _sentOrders = new Dictionary<long, bool>();      
        void orderPanelControl1_SendButtonPressed(object sender, EventArgs e)
        {
            SendOrderPanelOrder();
        }

        private void SendOrderPanelOrder()
        {
            OutgoingOrder o = OrderFactory.CreateOutgoingOrder(orderPanelControl1.Order);
            o.User = MyUser;
            _sentOrders[o.ClientOrderID] = false;
            HookAndSend(o);
            if (o.IsSent)
            {
                orderPanelControl1.Order = o;
            }
        }

        void orderPanelControl1_AmendButtonPressed(object sender, Order newOrder)
        {
            _log.Trace(LogLevel.Debug, "orderPanelControl1_AmendButtonPressed. Amsing order.");
            OutgoingOrder o = (OutgoingOrder)orderPanelControl1.Order;
            o.Amend(newOrder.Quantity, newOrder.Price, newOrder.Parameters);
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
        
        public void OnStatusChanged(object sender, Order newOrder)
        {
            OutgoingOrder order = sender as OutgoingOrder;
            bool active = OrderStateMachine.IsActiveStatus(order.Status);

            if (!active)
            {
                order.StatusChanged -= new OutgoingOrderEventHandler(OnStatusChanged);
            }

            if (_sentOrders.ContainsKey(order.ClientOrderID))
            {
                if (!_sentOrders[order.ClientOrderID])
                {
                    _sentOrders[order.ClientOrderID] = true;
                    orderPanelControl1.Order = order;
                }
                else if (!active)
                {
                    orderPanelControl1.Order = null;
                }
            }

            _myOrdersByOrderID[order.OrderID] = order;

            if (!_assignmentsByOID.ContainsKey(order.OrderID))
            {
                Assignment currentAssignment = _assignmentManager.CurrentAssignment;
                _assignmentsByOID[order.OrderID] = currentAssignment;
                _log.Trace(LogLevel.Info, "Order id {0} was associated to assignment {1}", order.OrderID, currentAssignment.ToString());
            }

            _log.Trace(LogLevel.Debug, "Status changed {0}", order.ToString());
            orderBlotter.AddOrder(order);
           
            switch (order.Status)
            {
                case OrderStatus.Accepted:
                case OrderStatus.AmendAccepted:
                case OrderStatus.AmendRejected:                
                case OrderStatus.CancelRejected:
                case OrderStatus.Filled:                
                    _log.Trace(LogLevel.Info, 
                        "Order went into open state {0}. Passing the order back to the orderpanel so it can be amended", 
                        order.Status.ToString());
                    orderPanelControl1.Order = order;
                    break;
                case OrderStatus.CancelledByExchange:
                case OrderStatus.Cancelled:
                case OrderStatus.Overfilled:
                    SwitchToNextAssignment();
                    break;
                case OrderStatus.CompletelyFilled:
                    _log.Trace(LogLevel.Info, 
                        "Order went into final state {0}. Switching to new assignment (if any)", order.Status.ToString());
                    double partialPNL = (order.LimitPrice - order.LastPriceFilled);
                    if (order.Side == OrderSide.Sell)
                    {
                        partialPNL = -partialPNL;
                    }
                    _pnl += partialPNL;
                    _pnlCurrent += partialPNL;
                    UpdateInfoPanel();
                    SwitchToNextAssignment();
                    break;                
                case OrderStatus.Rejected:
                    _log.Trace(LogLevel.Info, "Order rejected. Resetting order panel so that the order can be re-sent.");
                    orderPanelControl1.Clear();
                    SwitchToNextAssignment(false);
                    break;

                case OrderStatus.NewOrder:
                default:
                    _log.TraceAndThrow("Should never get here! Status = {0}", order.Status.ToString());
                    break;
            }
        }

        private void SwitchToNextAssignment()
        {
            SwitchToNextAssignment(true);
        }

        private void SwitchToNextAssignment(bool completeCurrentAssignment)
        {
            if (completeCurrentAssignment)
            {
                _assignmentManager.CompleteCurrentAssignment();
                _ordersExecuted++;
            }

            Assignment a = _assignmentManager.CurrentAssignment;
            if (a != null)
            {
                if (infoPanel1.RemainingTime.CompareTo(TimeSpan.FromSeconds(3))>0)
                {
                    _log.Trace(LogLevel.Info, "There is a new assignment - setting up order panel");
                    if (completeCurrentAssignment)
                    {
                        SetupNewOrder(a);
                    }
                    else
                    {
                        SetupNewOrder(a, "Your order was REJECTED. Check the Order Blotter.");
                    }
                }
                else
                {
                    _log.Trace(LogLevel.Info, "There is a new assignment, but the time is over. Clearing order panel");
                    orderPanelControl1.Clear();
                }
            }
            else
            {
                _log.Trace(LogLevel.Info, "There are no more assignments left - clearing order panel");
                orderPanelControl1.Clear();
            }

            UpdateInfoPanel();
        }

        public void OnOrderSent(object sender, Order order)
        {
            Order o = sender as Order;
            _log.Trace(LogLevel.Debug, "Order sent {0}", o.ToString());
            orderBlotter.AddOrder(o);
        }

        bool IsMyOrder(TradeDataMessage tradeDataMessage)
        {
            return (tradeDataMessage.User.Equals(MyUser) 
                && !tradeDataMessage.Counterparty.Equals(MyUser));
        }

        private double _pnlCurrent = 0;
        private double _pnl = 0;
        private int _blinkingID = 0;
        void TradeMessageReceived(object sender, TradeDataMessage tradeDataMessage)
        {
            if (IsMyOrder(tradeDataMessage))
            {
                tradeBlotter.AddTrade(tradeDataMessage);
                tradeBlotter.BeginFlash();
                depthPanelControl1.BeginFlash();           
            }
        }     

        void OnNewAssignmentAvailable(object sender, bool newSimulationStarted)
        {
            if (newSimulationStarted)
            {
                _log.Trace(LogLevel.Method, "OnNewAssignmentAvailable. Resetting PNL and updating info panel.");
                _pnl = _pnlCurrent = 0;
                UpdateInfoPanel();
            }

            _log.Trace(LogLevel.Debug, "OnNewAssignmentAvailable. CurrentAssignment: {0}", _assignmentManager.CurrentAssignment.ToString());
            UpdateInfoPanel();
            if (orderPanelControl1.Mode == OrderPanelControlMode.Idle)
            {
                _log.Trace(LogLevel.Info, "OnNewAssignmentAvailable. New assignment arrived + orderpanelcontrol in idle mode -> setting up new order");
                SetupNewOrder(_assignmentManager.CurrentAssignment);
            }            
        }

        private int _ordersExecuted = 0;        
        private void UpdateInfoPanel()
        {        
            infoPanel1.OrderExecuted = _ordersExecuted;
            infoPanel1.OrdersRemaining = _assignmentManager.Assignments.Length;
            infoPanel1.PNL = _pnl;
            infoPanel1.PNLCurrent = _pnlCurrent;
        }

        void SetupNewOrder(Assignment assignment)
        {
            SetupNewOrder(assignment, "A NEW ORDER is ready. Check the Order panel");
        }

        private delegate void SetupNewOrderDelegate(Assignment assignment, string message);
        void SetupNewOrder(Assignment assignment, string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetupNewOrderDelegate(SetupNewOrder), assignment, message);
            }
            else
            {
                ShadowOrder o = new ShadowOrder();

                o.Price = assignment.Price;
                o.LimitPrice = assignment.Price;
                o.Quantity = assignment.Quantity;
                o.Instrument = assignment.Ric;
                o.Type = OrderType.Limit;
                o.Side = (OrderSide)Enum.Parse(typeof(OrderSide), assignment.Side.ToString());
                o.Destination = DefaultDestination;
                o.Currency = assignment.Currency;

                orderPanelControl1.Order = o;

                _log.Trace(LogLevel.Method, "About to schedule a NEW ORDER BLINKING");
                _blinkingScheduler.Schedule(_blinkOnNewOrder, _blinkingID, message);
                ++_blinkingID;
            }
        }      

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseGUI();
        }

        private void CloseGUI()
        {
            infoPanel1.Stop();
            _blinkingScheduler.Stop();
            _services.TradeMessageReceived -= new TradeMessageReceivedEventHandler(TradeMessageReceived);
            _services.Stop();
            MarketDataClient.Instance.SessionChanged -= new SessionChangedEventHandler(SessionChanged);
        }

        private void tradeBlotter_Load(object sender, EventArgs e)
        {
            tradeBlotter.ApplicationName = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox a = new AboutBox();
            a.Show();
        }

        int _phase = 0;
        int _id = 0;
        double _price = StartPrice;
        const double StartPrice = 50.0;
        const double PriceStep = 25;
        const int DefaultQuantity = 1;
        const OrderSide DefaultSide = OrderSide.Sell;        
        const string DefaultCCY = "GBp";
        const int SwitchPhaseEvery = 5;       
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // make up a new assignment and send it to the assignment manager     
            Assignment a = new Assignment(
                ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null),
                _phase,
                _id,
                DefaultRic,
                DefaultCCY,
                DefaultSide,
                DefaultQuantity,
                _price);

            _assignmentManager.ForceReceiveNewAssignment(a);

            _price -= (DefaultSide == OrderSide.Buy) ? PriceStep : -PriceStep;
            if (++_id % SwitchPhaseEvery == 0)
            {
                ++_phase;
                _price = StartPrice;
            }
        }

        private void expToolStripMenuItem_Click(object sender, EventArgs e)
        {
            orderPanelControl1.Clear();      
        }        
    }    
}
