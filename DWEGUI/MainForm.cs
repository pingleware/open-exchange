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
using System.Threading;

using OPEX.Common;
using OPEX.OM.Common;
using OPEX.MDS.Common;
using OPEX.MDS.Client;
using OPEX.TDS.Client;
using OPEX.TDS.Common;
using OPEX.Configuration.Client;
using OPEX.SupplyService.Common;
using OPEX.DWEAS.Client;
using OPEX.AS.Service;

using Timer = System.Threading.Timer;
using NewAssignmentBatchReceivedEventHandler = OPEX.DWEAS.Client.NewAssignmentBatchReceivedEventHandler;

namespace OPEX.DWEGUI
{
    public partial class MainForm : Form
    {
        private static readonly string DefaultRic = "TTECH.L";
        private readonly string MyUser;

        private readonly Logger _log;
        private readonly Services _services;        
        private readonly Dictionary<long, Order> _myOrdersByOrderID;
        private readonly Dictionary<long, Assignment> _assignmentsByOID;        
        private readonly Blinking _blinkOnNewOrder;        
        private readonly BlinkingScheduler _blinkingScheduler;
        private readonly SpeechSynthesizer speech = new SpeechSynthesizer();
        private readonly AssignmentController _assignmentController;
        private readonly bool _speak;

        private double _pnlCurrent = 0;
        private System.Threading.Timer _initialRequestTimer;
        private System.Threading.Timer _countdownTimer;
        private bool _connecting = true;
        private SessionChangedEventArgs _currentSessionState;
        

        public MainForm()
        {
            InitializeComponent();

            if (DesignMode)
            {
                return;                
            }

            _log = new Logger("TradingGUI");

            _services = Services.Instance;

            _myOrdersByOrderID = new Dictionary<long, Order>();
            _assignmentsByOID = new Dictionary<long, Assignment>();

            MyUser = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", "OPEXApplication");

            _assignmentController = new AssignmentController(MyUser);

            _speak = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("SpeakSessionState", "false"));

            _blinkOnNewOrder = new Blinking(infoPanel1.MessageTextBox, grpOrder, Color.Blue, SystemColors.Control);            
            _blinkingScheduler = new BlinkingScheduler();
            _blinkingScheduler.Start();            
            this.DoubleBuffered = true;
            _countdownTimer = new Timer(
                new TimerCallback(UpdateTimerText),
                null,
                Timeout.Infinite,
                Timeout.Infinite);
        }
                
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }
            
            _services.TradeMessageReceived += new TradeMessageReceivedEventHandler(TradeMessageReceived);
            _services.AssignmentBatchReceived += new NewAssignmentBatchReceivedEventHandler(AssignmentBatchReceived);
            _services.Start();

            MarketDataClient.Instance.SessionChanged += new SessionChangedEventHandler(SessionChanged);

            OrderMonitor.Instance.OrderAccepted += new OutgoingOrderEventHandler(OrderAccepted);
            OrderMonitor.Instance.OrderSwitchedToActiveState += new OutgoingOrderEventHandler(OnOrderSwitchedToActiveState);
            OrderMonitor.Instance.OrderSwitchedToClosedState += new OutgoingOrderEventHandler(OnOrderSwitchedToClosedState);
            OrderMonitor.Instance.OrderFilled += new OutgoingOrderEventHandler(OnOrderFilled);
            OrderMonitor.Instance.OrderRejected += new OutgoingOrderEventHandler(OnOrderRejected);

            string applicationName = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null);
            if (applicationName != null && applicationName.Length > 0)
            {
                this.Text += string.Format(" ({0})", applicationName);
            }

            orderBlotter.RowDoubleClicked += new OrderBlotter.OrderBlotterRowDoubleClickedEventHandler(orderBlotter_RowDoubleClicked);
            depthPanelControl1.Instrument = DefaultRic;
            grpDepth.Text = string.Format("Orderbook({0})", DefaultRic);
            _initialRequestTimer = 
                new System.Threading.Timer(new System.Threading.TimerCallback(InitialStatusRequest), null, 1000, System.Threading.Timeout.Infinite); 
        }

        public void OrderAccepted(object sender, Order order)
        {
            Order o = sender as Order;
            orderBlotter.AddOrder(o);

            _assignmentController.Allocate(o.LimitPrice, o.Quantity);
            UpdatePermitBlotter();
        }

        private void OnOrderSwitchedToActiveState(object sender, Order order)
        {
            Order o = sender as Order;
            orderBlotter.AddOrder(o);

            // Note: no quantity amendments => no need 
            // for _assignmentController.Allocate
        }

        private void OnOrderSwitchedToClosedState(object sender, Order order)
        {
            Order o = sender as Order;
            orderBlotter.AddOrder(o);

            if (o.QuantityRemaining > 0)
            {
                _assignmentController.Rel(o.LimitPrice, o.QuantityRemaining);
                UpdatePermitBlotter();
            }
        }

        private void OnOrderFilled(object sender, Order order)
        {
            Order o = sender as Order;
            orderBlotter.AddOrder(o);

            double partialPNL = (o.LimitPrice - o.LastPriceFilled) * o.LastQuantityFilled;
            if (o.Side == OrderSide.Sell)
            {
                partialPNL = -partialPNL;
            }
            _pnlCurrent += partialPNL;

            _assignmentController.Fll(o.LimitPrice, o.LastQuantityFilled);

            UpdateInfoPanel();
            UpdatePermitBlotter();
        }

        private void OnOrderRejected(object sender, Order order)
        {
            Order o = sender as Order;
            orderBlotter.AddOrder(o);
        }

        void AssignmentBatchReceived(object sender, OPEX.AS.Service.AssignmentBatch assignmentBatch, bool newSimulationStarted)
        {
            _log.Trace(LogLevel.Debug, "AssignmentBatchReceived. assignmentBatch: {0} newSimulationStarted {1}", 
                (assignmentBatch != null) ? assignmentBatch.ToString() : "(null)", newSimulationStarted);

            if (newSimulationStarted)
            {
                _log.Trace(LogLevel.Method, "AssignmentBatchReceived. NEW SIMULATION STARTED. Resetting PNL and updating info panel.");

                _pnlCurrent = 0;                
                _assignmentController.Clear();
                UpdatePermitBlotter();
                UpdateInfoPanel();
            }

            if (assignmentBatch != null)
            {
                _log.Trace(LogLevel.Debug, "AssignmentBatchReceived. New assignment batch received {0}", assignmentBatch.ToString());
                foreach (Assignment a in assignmentBatch.Assignments)
                {
                    _assignmentController.AddAssignment(a);
                    _log.Trace(LogLevel.Debug, "AssignmentBatchReceived. Assignment enqueued");
                }
            }

            UpdatePermitBlotter();                   
        }

        private void UpdatePermitBlotter()
        {
            assignmentBlotter.UpdatePermits(_assignmentController.AssignmentBuckets);
        }

        void InitialStatusRequest(object state)
        {
            _log.Trace(LogLevel.Info, "InitialStatusRequest. Requesting MARKET STATUS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");            
            MarketDataClient.Instance.RequestStatus(MarketDataClient.DefaultDataSource);
        }

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
            }

            if (args.SessionState == SessionState.Close)
            {
                _pnlCurrent = 0;
                _blinkingScheduler.Enabled = false;
                _blinkingScheduler.Flush();
                _assignmentController.Clear();
                ToggleTimer(false);
                infoPanel1.TimeLeft = TimeSpan.Zero;
                UpdatePermitBlotter();
                return;
            }
            else if (args.SessionState == SessionState.Open)
            {
                _nextSessionEnd = args.EndTime;
                infoPanel1.TimeLeft = TimeSpan.Zero;
                ToggleTimer(true);                
            }

            _blinkingScheduler.Enabled = true;            
        }

        private DateTime _nextSessionEnd;

        private void ToggleTimer(bool start)
        {
            if (start)
            {
                _countdownTimer.Change(
                    TimeSpan.FromMilliseconds(1000), 
                    TimeSpan.FromMilliseconds(1000));                
            }
            else
            {
                _countdownTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        void UpdateTimerText(object state)
        {
            infoPanel1.TimeLeft = _nextSessionEnd.Subtract(DateTime.Now);
            if (_nextSessionEnd.Subtract(DateTime.Now).CompareTo(TimeSpan.FromSeconds(1)) < 1)
            {
                ToggleTimer(false);
            }
        }

        void orderPanelControl1_AmendButtonPressed(object sender, Order newOrder)
        {
            _log.Trace(LogLevel.Debug, "orderPanelControl1_AmendButtonPressed. Amsing order.");
        }

        bool IsMyOrder(TradeDataMessage tradeDataMessage)
        {
            return (tradeDataMessage.User.Equals(MyUser) 
                && !tradeDataMessage.Counterparty.Equals(MyUser));
        }

        void TradeMessageReceived(object sender, TradeDataMessage tradeDataMessage)
        {
            if (IsMyOrder(tradeDataMessage))
            {
                tradeBlotter.AddTrade(tradeDataMessage);
                tradeBlotter.BeginFlash();
                depthPanelControl1.BeginFlash();           
            }
        }     

        private void UpdateInfoPanel()
        {                
            infoPanel1.PNLCurrent = _pnlCurrent;
        }        

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseGUI();
        }

        private void CloseGUI()
        {
            _countdownTimer.Change(Timeout.Infinite, Timeout.Infinite);
            orderBlotter.RowDoubleClicked -= new OrderBlotter.OrderBlotterRowDoubleClickedEventHandler(orderBlotter_RowDoubleClicked);

            OrderMonitor.Instance.OrderAccepted -= new OutgoingOrderEventHandler(OrderAccepted);
            OrderMonitor.Instance.OrderSwitchedToActiveState -= new OutgoingOrderEventHandler(OnOrderSwitchedToActiveState);
            OrderMonitor.Instance.OrderSwitchedToClosedState -= new OutgoingOrderEventHandler(OnOrderSwitchedToClosedState);
            OrderMonitor.Instance.OrderFilled -= new OutgoingOrderEventHandler(OnOrderFilled);
            OrderMonitor.Instance.OrderRejected -= new OutgoingOrderEventHandler(OnOrderRejected);

            _blinkingScheduler.Stop();
            _services.TradeMessageReceived -= new TradeMessageReceivedEventHandler(TradeMessageReceived);
            _services.AssignmentBatchReceived -= new NewAssignmentBatchReceivedEventHandler(AssignmentBatchReceived);
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

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _assignmentController.AddAssignment(
                new Assignment("SELLER1", 0, 0, "TTECH.L", "GBp", OrderSide.Sell, 3, 5));

            _assignmentController.AddAssignment(
                new Assignment("SELLER1", 0, 0, "TTECH.L", "GBp", OrderSide.Sell, 3, 105));

            _assignmentController.AddAssignment(
                new Assignment("SELLER1", 0, 0, "TTECH.L", "GBp", OrderSide.Sell, 3, 125));

            UpdatePermitBlotter();
        }

        private void orderBlotter_RowDoubleClicked(object sender, OutgoingOrder order)
        {
            if (!OrderStateMachine.IsActiveStatus(order.Status))
            {
                return;
            }

            OrderForm f = new OrderForm(MyUser, order, CalculateOrderFormPosition());
            f.ShowDialog(this);
        }

        private void assignmentBlotter_AssignmentBlotterClicked(object sender, AssignmentBucket ab)
        {
            if (ab.QtyRem == 0)
            {
                return;
            }

            OrderForm f = new OrderForm(MyUser, ab, CalculateOrderFormPosition());
            f.ShowDialog(this);
        }

        private Point CalculateOrderFormPosition()
        {
            return new Point(
                grpInfo.Location.X + this.Location.X,
                grpInfo.Location.Y + this.Location.Y);
        }
    }    
}
