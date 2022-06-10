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
using System.Text;
using System.Threading;

using OPEX.Common;
using OPEX.OM.Common;
using OPEX.SupplyService.Common;
using OPEX.MDS.Common;
using OPEX.MDS.Client;
using OPEX.Configuration.Client;
using OPEX.StaticData;
using OPEX.ShoutService;
using OPEX.AS.Service;
using OPEX.DWEAS.Client;

namespace OPEX.Agents.Common
{
    public abstract partial class Agent
    {
        protected static readonly bool NYSESpreadImprovement = false;
        private static readonly string DefaultDestination = "OPEX";
        private static readonly int MaxRefreshMarketDataRetry = 10;

        static Agent()
        {
            try
            {
                NYSESpreadImprovement = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("NYSESpreadImprovement", "false"));
            }
            finally { }
        }

        private bool _wakeup = false;         
        
        private readonly object _root = new object();
        private readonly Thread _mainThread;
        private readonly ManualResetEvent _mainThreadReset;
        private readonly AutoResetEvent _wakeupEvent;
        private readonly Semaphore _awake;
        private readonly AssignmentController _assignmentController;        
        private readonly int _inactivityTimerCycleMsec;
                        
        protected readonly Logger _logger;
        protected readonly AgentState _state;
        protected readonly string _agentName;
        protected readonly AgentWakeupMode _agentWakeupMode;        
        protected readonly  int _sleepTimeMsec;
        protected readonly AgentStimulusCollector _stimulusCollector;
        protected readonly ParameterBag _parameters;

        private bool _isActive;
        private bool _running;
        private bool _replacementOrderCreated;
        private SynchronousOrderSender _orderSender;
        private InstrumentWatcher _watcher;
        private bool _newSimulationStarted = false;

        public Agent(string agentName, AgentWakeupMode wakeupMode, int sleepTimeMsec, int inactivitySleepTimeMsec, ParameterBag parameters)
        {            
            _logger = new Logger(agentName);

            _agentName = agentName;
            _agentWakeupMode = wakeupMode;
            _sleepTimeMsec = sleepTimeMsec;
            _mainThread = new Thread(new ThreadStart(AgentMainThread));
            _mainThreadReset = new ManualResetEvent(false);
            _stimulusCollector = new AgentStimulusCollector(_agentName, _sleepTimeMsec, inactivitySleepTimeMsec);            
            _awake = new Semaphore(1, 1);            
            _assignmentController = new AssignmentController(agentName);
            _state = new AgentState();            
            _wakeupEvent = new AutoResetEvent(false);
            _parameters = parameters;
            _isActive = false;
            _inactivityTimerCycleMsec = inactivitySleepTimeMsec;

            LoadSettings();
        }
                
        public abstract string Type { get; }
        public string Name { get { return _agentName; } }
        public AgentState AgentStatus { get { return _state; } }
        public ParameterBag Parameters { get { return _parameters; } }
        public bool NYSESpreadImprovementClientSide { get { return NYSESpreadImprovement; } }
        public bool IsActive { get { return _isActive; } }
        public bool WakeUp { get { return _wakeup; } }
        public SortedDictionary<double, AssignmentBucket> AssignmentBuckets { get { return _assignmentController.AssignmentBuckets; } }
        public AssignmentController AssignmentController { get { return _assignmentController; } }

        public void Start()
        {
            lock (_root)
            {
                _logger.Trace(LogLevel.Method, "Starting agent");

                if (_running)
                {
                    _logger.Trace(LogLevel.Critical, "Cannot start agent: agent already running");
                    return;
                }

                Init();

                _mainThreadReset.Reset();
                _mainThread.Start();                

                _logger.Trace(LogLevel.Method, "Agent started");
            }
        }

        public void Stop()
        {
            lock (_root)
            {
                _logger.Trace(LogLevel.Method, "Stopping agent");

                if (!_running)
                {
                    _logger.Trace(LogLevel.Critical, "Cannot stop agent: agent not running");
                    return;
                }

                Shutdown();

                _mainThreadReset.Set();
                _wakeupEvent.Set();
                _mainThread.Join(1000);                
    
                _logger.Trace(LogLevel.Method, "Agent stopped");
            }
        }

        protected abstract void OnWakeUp(Stimulus stimulus);
        protected abstract void OnNewOrder();

        protected virtual void OnSession(SessionStimulus sessionStimulus) { }
        protected virtual void OnAllAssignmentFinished() { }
        protected virtual void OnOfflineShout(ShoutStimulus stimulus) { }
        protected virtual void OnNewSimulationStarted() { }
        protected virtual void LoadSettings() { }

        private void DoWakeUp()
        {
            try
            {
                bool freshOrder = false;
                bool replacementOrder = false;
                if (_state.CurrentOrder == null)
                {
                    if (_assignmentController.OpnBckts == 0)
                    {
                        if (_state.CurrentStimulus.Type == StimulusType.Shout)
                        {
                            ShoutStimulus ss = _state.CurrentStimulus as ShoutStimulus;
                            if (ss == null)
                            {
                                _logger.Trace(LogLevel.Error, "DoWakeUp. Received a NULL offline shout stimulus. Skipping call to OnOfflineShout.");
                            }
                            else if (ss.Shout == null)
                            {
                                _logger.Trace(LogLevel.Error, "DoWakeUp. Received a NULL offline shout. Skipping call to OnOfflineShout.");
                            }
                            else
                            {
                                OnOfflineShout(ss);
                            }
                        }
                        return;
                    }
                    
                    _state.CurrentAssignmentBucket = _assignmentController.BestAvailableAssignmentBucket;
                    CreateNewOrder();                    
                    freshOrder = true;

                    if (_newSimulationStarted)
                    {
                        _logger.Trace(LogLevel.Method, "DoWakeUp. NEW SIMULATION STARTED! Propagating stimulus to agent logic...");
                        OnNewSimulationStarted();
                        _newSimulationStarted = false;
                    }
                }
                else if (_replacementOrderCreated && !_state.CurrentOrder.IsSent) // DWE _replacementOrderCreated is set within InnerOrderStatus
                {
                    replacementOrder = true;
                    _replacementOrderCreated = false;                    
                }

                if (replacementOrder || freshOrder)
                {
                    _logger.Trace(LogLevel.Info, "DoWakeUp. replacementOrder {0} freshOrder {1}", replacementOrder, freshOrder);
                    _stimulusCollector.ShoutQueue.ListenToInstrument(_state.CurrentAssignmentBucket.RIC);
                    _isActive = true;

                    OnNewOrder();

                    if (freshOrder)
                    {
                        return;
                    }
                }

                switch (_state.CurrentStimulus.Type)
                {
                    case StimulusType.Shout:                                               
                    case StimulusType.Timer:
                        OnWakeUp(_state.CurrentStimulus);                  
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "DoWakeUp. Exception: {0} {1}", ex.Message, ex.StackTrace.Replace(Environment.NewLine, " "));
            }
        }
    
        private void AgentMainThread()
        {
            try
            {
                bool thereHasBeenATrade = false;
                bool thereHasBeenAShout = false;
                _running = true;
                _logger.Trace(LogLevel.Method, "AgentMainThread started");

                WaitHandle[] EventArray = new WaitHandle[] {
                    _mainThreadReset,
                    _newItemEvent
                };

                if (_sleepTimeMsec > 0)
                {
                    _stimulusCollector.TimerQueue.ToggleTimer(true);
                }

                while (WaitHandle.WaitAny(EventArray) != 0)
                {
                    bool restartTimer = false;
                    while (_stimulusQueue.Count > 0)                        
                    {                            
                        Stimulus currentStimulus = _stimulusQueue.Dequeue();

                        _state.CurrentStimulus = currentStimulus;

                        switch (_state.CurrentStimulus.Type)
                        {
                            case StimulusType.NewOrder:
                                InnerNewOrder(_state.CurrentStimulus as NewOrderStimulus);
                                break;
                            case StimulusType.OrderStatus:
                                InnerOrderStatus(_state.CurrentStimulus as OrderStatusStimulus);
                                break;
                            case StimulusType.Session:
                                InnerSession(_state.CurrentStimulus as SessionStimulus);
                                break;
                            case StimulusType.Shout:
                                ShoutStimulus ss = _state.CurrentStimulus as ShoutStimulus;
                                if (ss == null)
                                {
                                    _logger.Trace(LogLevel.Error, "OnWakeUp. shoutStimulus == null. Skip.");
                                    continue;
                                }
                                else if (ss.Shout == null)
                                {
                                    _logger.Trace(LogLevel.Error, "OnWakeUp. shoutStimulus.Shout == null. Skip.");
                                    continue;
                                }
                                
                                if (!_agentName.Equals(ss.Shout.User))
                                {
                                    thereHasBeenAShout = true;
                                    thereHasBeenATrade |= ss.Shout.Accepted;
                                }
                                break;
                            case StimulusType.Timer:
                                TimerStimulus ts = _state.CurrentStimulus as TimerStimulus;
                                if (ts.Primary)
                                {
                                    restartTimer = true;

                                    if ((_agentWakeupMode & AgentWakeupMode.WakeUpOnTrades) != 0)
                                    {
                                        _wakeup = thereHasBeenATrade;
                                    }
                                    if ((_agentWakeupMode & AgentWakeupMode.WakeUpOnOrders) != 0)
                                    {
                                        _wakeup = thereHasBeenAShout;
                                    }
                                }

                                break;
                            default:
                                _logger.Trace(LogLevel.Critical, "AgentMainThread. Unknown stimulus type {0}. Ignoring stimulus.", _state.CurrentStimulus.Type);
                                break;
                        }

                        DoWakeUp();

                        if (_wakeup)
                        {
                            thereHasBeenAShout = false;
                            thereHasBeenATrade = false;
                            _wakeup = false;
                        }

                    }                        
                    
                    if (restartTimer && _sleepTimeMsec > 0)
                    {
                        _stimulusCollector.TimerQueue.ToggleTimer(true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "AgentMainThread. Exception: {0} {1}", ex.Message, ex.StackTrace.Replace(Environment.NewLine, " ").Replace("\n", " "));
            }
            finally
            {
                _logger.Trace(LogLevel.Method, "AgentMainThread ended");
                _running = false;
            }
        }

        private void InnerNewOrder(NewOrderStimulus newOrderStimulus)
        {            
            AssignmentBatch assignmentBatch = newOrderStimulus.AssignmentBatch;

            if (newOrderStimulus.NewSimulationStarted)
            {
                _logger.Trace(LogLevel.Method, "InnerNewOrder. New SIMULATION started");
                _newSimulationStarted = true;                
            }

            if (assignmentBatch != null)
            {
                _logger.Trace(LogLevel.Debug, "InnerNewOrder. New assignment batch received {0}", assignmentBatch.ToString());
                foreach (Assignment a in assignmentBatch.Assignments)
                {
                    _assignmentController.AddAssignment(a);
                    _logger.Trace(LogLevel.Debug, "InnerNewOrder. Assignment enqueued");
                }
            }

            if (_state.CurrentOrder == null)
            {
                return;
            }

            // if there's an open order...       
            double currentLimitPrice = _state.CurrentOrder.LimitPrice;                
            string error = "";
            AssignmentBucket ab = _assignmentController.BestAvailableAssignmentBucket;

            if (ab == null)
            {
                return;
            }

            if ((ab.Side == OrderSide.Buy && ab.Price > currentLimitPrice)
                || (ab.Side == OrderSide.Sell && ab.Price < currentLimitPrice))
            {
                _logger.Trace(LogLevel.Method,
                    "InnerNewOrder. =-DWE-= A BETTER Priced Bucket has arrived: currentLimitPrice: {0} bestBucketPrice: {1}. Cancelling current order",
                    currentLimitPrice, ab.Price);

                if (_state.CurrentOrder.IsSent)
                {
                    CancelCurrentOrder();
                }
                else
                {
                    _state.CurrentOrder = null;
                    _stimulusCollector.OrderStatusQueue.StopReceiving();
                    _state.CurrentAssignmentBucket = null;
                }
            }

            if (ab.Price == currentLimitPrice)
            {                               
                if (_state.CurrentOrder.IsSent)
                {
                    _logger.Trace(LogLevel.Method,
                   "InnerNewOrder. =-DWE-= More units ({0}) at the same limit price as the currently open order {1} have arrived. Amending current order.",
                   ab.QtyRem, ab.Price);

                    if (!_orderSender.AmendSynch(_state.CurrentOrder.Price,
                        _state.CurrentOrder.Quantity + ab.QtyRem,
                        SendAmendTimeoutMsec, ref error))
                    {
                        _logger.Trace(LogLevel.Warning, "InnerNewOrder. Couldn't amend order: {0}", error);
                    }
                    else
                    {
                        _assignmentController.Allocate(ab.Price, ab.QtyRem);
                    }
                }
                else
                {
                    if (ab.QtyRem > _state.CurrentOrder.Quantity)
                    {
                        _logger.Trace(LogLevel.Method,
                       "InnerNewOrder. =-DWE-= More units ({0}) at the same limit price as the currently open order {1} have arrived. Amending current order.",
                       ab.QtyRem - _state.CurrentOrder.Quantity, ab.Price);

                        _state.CurrentOrder.Quantity = ab.QtyRem;
                    }                   
                }                         
            }
        }
        
        private readonly AutoResetEvent _newItemEvent = new AutoResetEvent(false);
        private PriorityQueue<Stimulus> _stimulusQueue = new PriorityQueue<Stimulus>();        
        private void StimulusCollector_NewStimulus(object sender, StimulusEventArgs args)
        {
            Stimulus s = args.Stimulus;
            
            _stimulusQueue.Enqueue(s.Priority, s);
            _newItemEvent.Set();
        }
        
        private bool _closeSessionProcessingToBePerformed = false;
        private void InnerSession(SessionStimulus sessionStimulus)
        {            
            _logger.Trace(LogLevel.Method, "### InnerSession. New Session: {0}", sessionStimulus.SessionInfo);            

            _state.LastSessionInfo = sessionStimulus.SessionInfo;

            if (sessionStimulus.SessionInfo.SessionState == SessionState.Close)
            {
                if (_state.CurrentOrder != null)
                {
                    FinishProcessingCurrentOrder(true);
                }
            }
            else
            {
                _closeSessionProcessingToBePerformed = true;
            }

            OnSession(sessionStimulus);
        }

        private void InnerOrderStatus(OrderStatusStimulus orderStatusStimulus)
        {
            if (_state.CurrentOrder == null)
            {
                _logger.Trace(LogLevel.Warning, "InnerOrderStatus. IGNORING update, as _state.CurrentOrder == null");
                return;
            }

            if (orderStatusStimulus.ClientOrderID != _state.CurrentOrder.ClientOrderID)
            {
                _logger.Trace(LogLevel.Warning, "InnerOrderStatus. IGNORING update, as it was received for an order different from current order.");
                return;
            }
            
            if (orderStatusStimulus.Status == OrderStatus.Rejected)
            {
                _stimulusCollector.OrderStatusQueue.StopReceiving();                
                _logger.Trace(LogLevel.Info, "InnerOrderStatus. The order was REJECTED ({0}). A new replacement order will be created", orderStatusStimulus.Message);
                CreateNewOrder();
                _replacementOrderCreated = true;
            }
            else if (orderStatusStimulus.Status == OrderStatus.Filled
                || orderStatusStimulus.Status == OrderStatus.CompletelyFilled)
            {
                _assignmentController.Fll(_state.CurrentOrder.LimitPrice, _state.CurrentOrder.LastQuantityFilled);
            }

            if (!OrderStateMachine.IsActiveStatus(orderStatusStimulus.Status))
            {
                _logger.Trace(LogLevel.Info, "InnerOrderStatus. Order finished: status {0}", orderStatusStimulus.Status, orderStatusStimulus.Message);
                bool sessionEnded = (orderStatusStimulus.Status == OrderStatus.CancelledByExchange);
                if (sessionEnded && _closeSessionProcessingToBePerformed)
                {
                    _logger.Trace(LogLevel.Info, "InnerOrderStatus. SESSION END sensed.");
                }
                FinishProcessingCurrentOrder(sessionEnded);
            }     
        }

        private void FinishProcessingCurrentOrder(bool sessionEnded)
        {
            if (sessionEnded && !_closeSessionProcessingToBePerformed)
            {
                _logger.Trace(LogLevel.Debug, "FinishProcessingCurrentOrder. Close session processing already performed. Skipping.");
                return;
            }

            _logger.Trace(LogLevel.Method, "FinishProcessingCurrentOrder. Current order processing finished: {0}", _state.CurrentOrder.ToString());

            if (_state.CurrentOrder.QuantityRemaining > 0)
            {
                _assignmentController.Rel(_state.CurrentOrder.LimitPrice, _state.CurrentOrder.QuantityRemaining);
            }

            _stimulusCollector.OrderStatusQueue.StopReceiving();                        
            _state.CurrentAssignmentBucket = null;

            if (sessionEnded)
            {
                _logger.Trace(LogLevel.Info, "FinishProcessingCurrentOrder. SESSION ENDED. Clearing remaining {0} open assignments buckets.",
                    _assignmentController.OpnBckts);
                _assignmentController.Clear();
                _closeSessionProcessingToBePerformed = false;
            }

            if (_assignmentController.OpnBckts == 0)
            {
                _isActive = false;
                OnAllAssignmentFinished();
            }

            _state.CurrentOrder = null;
        }

        private readonly int RefreshMarketDataTimeoutMsec = 5000;
        protected void CreateNewOrder()
        {
            OutgoingOrder o = OrderFactory.CreateOutgoingOrder();

            _logger.Trace(LogLevel.Debug, "CreateNewOrder. New order created");

            o.Side = _state.CurrentAssignmentBucket.Side;
            o.Quantity = _state.CurrentAssignmentBucket.QtyRem;
            o.LimitPrice = _state.CurrentAssignmentBucket.Price;
            o.Price = _state.CurrentAssignmentBucket.Price;
            o.Instrument = _state.CurrentAssignmentBucket.RIC;
            o.Currency = _state.CurrentAssignmentBucket.CCY;
            o.Type = OrderType.Limit;
            o.Destination = DefaultDestination;
            o.User = _agentName;                        

            _state.CurrentOrder = o;
            _state.CurrentInstrument = StaticDataManager.Instance.InstrumentStaticData[o.Instrument];
            _orderSender = new SynchronousOrderSender(o);
            _stimulusCollector.OrderStatusQueue.ListenToOrder(o);
            
            HookMarketDataChanged(o.Instrument, true);
            _logger.Trace(LogLevel.Debug, "Getting first market data snapshot for instrument {0}", _state.CurrentInstrument.ToString());
            RefreshMarketData(RefreshMarketDataTimeoutMsec, true);

            _logger.Trace(LogLevel.Method, "CreateNewOrder. Current order: {0}", o.ToString());
        }

        protected void CancelCurrentOrder()
        {
            if (!_state.CurrentOrder.IsSent)
            {
                _logger.Trace(LogLevel.Warning, "CancelCurrentOrder. Cannot cancel current order. Order hasn't been sent yet.");                
            }
            else
            {
                string error = null;
                _logger.Trace(LogLevel.Info, "CancelCurrentOrder. CANCELLING order");                
                if (!_orderSender.CancelSynch(500, ref error))
                {
                    _logger.Trace(LogLevel.Error, "CancelCurrentOrder. Order wasn't cancelled: {0}", error);
                }
            }

            FinishProcessingCurrentOrder(false);          
        }

        private readonly int SendAmendTimeoutMsec = 1500;
        protected void SendOrAmendCurrentOrder(double price)
        {
            bool res = false;
            string error = null;
            double bestPrice = 0;
            double limitPrice = _state.CurrentAssignmentBucket.Price;

            if ((_state.CurrentAssignmentBucket.Side == OrderSide.Sell && price < limitPrice) ||
                (_state.CurrentAssignmentBucket.Side == OrderSide.Buy && price > limitPrice))
            {
                _logger.Trace(LogLevel.Critical, "SendOrAmendCurrentOrder. Pricer generated {0:F4}, limit price {1:F4}. Price EXCEEDS LIMIT PRICE. Skipping.", price, limitPrice); 
            }
            if (NYSESpreadImprovement && !(IsPriceValid(price, out bestPrice)))
            {
                _logger.Trace(LogLevel.Debug, "SendOrAmendCurrentOrder. Pricer generated {0:F4}, best price {1:F4} limit price {2:F4}. Price invalid. Skipping.", price, bestPrice, limitPrice);            
            }
            else if (_state.CurrentOrder.IsSent && _state.CurrentOrder.Price == price)
            {
                _logger.Trace(LogLevel.Debug, "SendOrAmendCurrentOrder. Pricer generated same price as before {0:F4}. Skipping.", price);
            }
            else if (!_state.CurrentOrder.IsSent)
            {
                _logger.Trace(LogLevel.Info, "SendOrAmendCurrentOrder. SENDING order: price {0:F4} limit price {1:F4}", price, limitPrice);
                _state.CurrentOrder.Price = price;
                res = _orderSender.SendSynch(SendAmendTimeoutMsec, ref error);
                if (!res)
                {
                    _logger.Trace(LogLevel.Warning, "SendOrAmendCurrentOrder. Order wasn't sent: {0}", error);
                }
                _assignmentController.Allocate(_state.CurrentOrder.LimitPrice, _state.CurrentOrder.Quantity);
            }
            else
            {
                _logger.Trace(LogLevel.Info, "SendOrAmendCurrentOrder. AMENDING order: old price {0:F4} new price {1:F4} limit price {2:F4}", _state.CurrentOrder.Price, price, limitPrice);
                res = _orderSender.AmendSynch(price, SendAmendTimeoutMsec, ref error);
                if (!res)
                {
                    _logger.Trace(LogLevel.Warning, "SendOrAmendCurrentOrder. Order wasn't amended: {0}", error);
                }
            }           
        }

        protected void RefreshMarketData(int maxWaitMsec)
        {
            RefreshMarketData(maxWaitMsec, false);
        }

        protected void RefreshMarketData(int maxWaitMsec, bool forceRetry)
        {
            int attempts = 0;
            bool success = false;
            do
            {
                _logger.Trace(LogLevel.Debug, "RefreshMarketData. Attempt #{2} of {3}. maxWaitMsec {0} forceRetry {1}",
                    maxWaitMsec, forceRetry, attempts + 1, MaxRefreshMarketDataRetry);
                if (!_watcher.GetLastSnapshot(maxWaitMsec))
                {
                    _logger.Trace(LogLevel.Warning, "RefreshMarketData. New market data hasn't arrived");
                }
                else
                {
                    success = true;
                    _state.LastSnapshot = _watcher.LastSnapshot;
                }
            }
            while (!success && forceRetry && (++attempts < MaxRefreshMarketDataRetry));
        }

        public bool IsPriceValid(double price, out double bestPrice)
        {
            bestPrice = 0;            

            if (_state.LastSnapshot != null)
            {
                OrderSide orderSide = _state.CurrentAssignmentBucket.Side;
                AggregatedDepthSide side = _state.LastSnapshot[orderSide];
                bestPrice = side.Best;
                if (bestPrice != 0)
                {
                    if (orderSide == OrderSide.Buy)
                    {
                        return price > bestPrice;
                    }
                    else
                    {
                        return price < bestPrice;
                    }
                }
            }

            return true;
        }
      
        private void MarketDataChanged(object sender, MarketDataEventArgs args)
        {
            MarketDataEventType type = args.Type;
            bool trade = (type == MarketDataEventType.DepthChangedWithNewTrade);
            bool shout = trade || (type == MarketDataEventType.DepthChangedWithNewShout);

            AggregatedDepthSnapshot lastSnapshot = _watcher.LastSnapshot;
            Shout lastShout = _watcher.LastShout;
            LastTradeUpdateMessage lastTrade = _watcher.LastTrade;

            _state.LastSnapshot = lastSnapshot;
            string tradeLog = null;
            if (trade)
            {                
                _state.LastTrade = lastTrade;
                tradeLog = string.Format(" LastTrade @ {0}", lastTrade.Price);
            }
        }

        private void Init()
        {
            _stimulusCollector.NewStimulus += new StimulusEventHandler(StimulusCollector_NewStimulus);
            _stimulusCollector.Start(true, true);
        }

        private void Shutdown()
        {
            _stimulusCollector.Stop(true, true);
            _stimulusCollector.NewStimulus -= new StimulusEventHandler(StimulusCollector_NewStimulus);
            if (_watcher != null)
            {
                HookMarketDataChanged(_watcher.Instrument, false);
            }
        }

        private void HookMarketDataChanged(string instrument, bool hook)
        {            
            bool unhookOld = (_watcher != null) && (hook ^ instrument.Equals(_watcher.Instrument));
            bool hookNew = hook && ((_watcher == null) || !instrument.Equals(_watcher.Instrument));

            if (unhookOld)
            {
                string oldInstrument = _watcher.Instrument;
                _logger.Trace(LogLevel.Debug, "HookMarketDataChanged. Unhooking MarketDataChanged for instrument {0}", oldInstrument);
                _watcher.MarketDataChanged -= new EventHandler<MarketDataEventArgs>(MarketDataChanged);
                _watcher.Dispose();
                _watcher = null;
            }

            if (hookNew)
            {
                _logger.Trace(LogLevel.Debug, "HookMarketDataChanged. Hooking MarketDataChanged for instrument {0}", instrument);
                _watcher = MarketDataClient.Instance.CreateInstrumentWatcher(instrument);
                _watcher.MarketDataChanged += new EventHandler<MarketDataEventArgs>(MarketDataChanged);
            }
        }
    }

    [Flags]
    public enum AgentWakeupMode : int
    {
        AlwaysSleep = 0,
        WakeUpOnTimer = 1,
        WakeUpOnTrades = 2,
        WakeUpOnOrders = 4
    }
}
