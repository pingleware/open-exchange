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
using System.Linq;
using System.Text;

using OPEX.Common;
using OPEX.DES.Simulation;
using OPEX.DES.OrderManager;
using OPEX.DES.Exchange;
using OPEX.StaticData;

namespace OPEX.DES
{
    public abstract class DESAgent : IDESAgent
    {
        protected readonly Logger _logger;
        protected readonly string _agentName;
        protected readonly DESAgentWakeupMode _agentWakeupMode;        
        protected readonly  int _sleepTimeMsec;
        protected readonly List<Assignment> _assignments;
        protected readonly GlobalOrderBook _gob;
        protected readonly ParameterBag _parameters;
        
        protected IOutgoingOrder _currentOrder;            
        private MarketData _marketData;

        public DESAgent(string agentName, DESAgentWakeupMode wakeupMode, int sleepTimeMsec, int inactivitySleepTimeMsec, ParameterBag parameters, GlobalOrderBook gob)
        {
             _logger = new Logger(agentName);

            _agentName = agentName;
            _agentWakeupMode = wakeupMode;
            _sleepTimeMsec = sleepTimeMsec;
            _parameters = parameters;            
            _assignments = new List<Assignment>();
            _marketData = new MarketData(null, null);
            _gob = gob;

            LoadSettings();
        }

        public bool IsActive { get { return _currentOrder != null && _currentOrder.QuantityRemaining > 0; } }
        public string Name { get { return _agentName; } }
        public Assignment[] Assignments { get { return _assignments.ToArray(); } }
        public int AssignmentsLeft { get { return _assignments.Count; } }
        public Assignment CurrentAssignment { get { return (_assignments.Count == 0) ? null : _assignments[_assignments.Count - 1]; } }        
        public MarketData LastMarketData { get { return _marketData; } }
        public ParameterBag Parameters { get { return _parameters; } }
        public Instrument CurrentInstrument
        {
            get
            {
                Instrument res = null;
                if (CurrentAssignment != null)
                {
                    res = StaticDataManager.Instance.InstrumentStaticData[CurrentAssignment.Ric];
                }
                return res;
            }
        }

        public abstract string Type { get; }
        public virtual void Init() { }

        protected IOrder CurrentOrder { get { return _currentOrder; } }
        private bool CurrentOrderIsComplete { get { return _currentOrder != null && _currentOrder.QuantityRemaining == 0; } }

        protected abstract void OnPlay(out bool newShout, out bool newDepth);
        protected virtual void LoadSettings() { }
        protected virtual void OnNewShout() { }
        protected virtual void OnNewOrder() { }
        protected virtual void OnNewPhaseStarted() { }
        protected virtual void OnNewSimulationStarted() { }
        protected virtual void OnOrderComplete() { }
        protected virtual void OnInactivity() { } 

        public void LoadAssignments(SimulationPhase phase, bool newSimulation)
        {
            _logger.Trace(LogLevel.Method, "LoadAssignments. Loading new assignments for this round.");
            _assignments.Clear();
            foreach (Assignment a in phase)
            {
                if (a.ApplicationName == _agentName)
                {
                    _assignments.Insert(0, a);
                }
            }
            if (newSimulation)
            {
                OnNewSimulationStarted();
            }
            OnNewPhaseStarted();
            LoadNextAssignment();            
        }

        private void LoadNextAssignment()
        {
            _currentOrder = null;
            if (_assignments.Count > 0)
            {
                _currentOrder = OrderFactory.CreateOrder(CurrentAssignment.Ric, CurrentAssignment.ApplicationName, CurrentAssignment.Side, 0.0, CurrentAssignment.Price, CurrentAssignment.Quantity);
                _logger.Trace(LogLevel.Info, "LoadNextAssignment. New order reated for currenta assignment {0}", CurrentAssignment);
                OnNewOrder();
            }
            else
            {
                _logger.Trace(LogLevel.Warning, "LoadNextAssignment. No Assignments left to process.");
            }
        }

        private void ManageCompleteOrder()
        {
            if (CurrentOrderIsComplete)
            {
                _logger.Trace(LogLevel.Info, "ManageCompleteOrder. Current order is complete.");
                OnOrderComplete();
                CompleteAssignment();
            }  
        }

        public void Play(out bool newShout, out bool newDepth)
        {            
            OnPlay(out newShout, out newDepth);
            ManageCompleteOrder();         
        }

        public void NotifyMarketData(bool newShout, bool newDepth)
        {
            if (newDepth)            
            {
                _marketData = _gob[CurrentAssignment.Ric];
            }

            if (newShout)
            {
                ManageCompleteOrder();
                OnNewShout();
            }

            if (!newShout && !newDepth)
            {
                OnInactivity();
            }
        }

        public string AssignmentListDump
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < _assignments.Count; ++i)
                {
                    sb.AppendFormat(" m={0}: {1};", i + 1, _assignments[i]);
                }

                return sb.ToString();
            }
        }

        protected bool SendOrAmendCurrentOrder(double price)
        {
            if (_currentOrder == null || !CanSendOrder(_currentOrder, price))
            {
                _logger.Trace(LogLevel.Info, "SendOrAmendCurrentOrder. Cannot send or amend order at price {0}", price);
                return false;
            }

            if (!_currentOrder.IsSent)
            {
                _currentOrder.Price = price;
                _logger.Trace(LogLevel.Info, "SendOrAmendCurrentOrder. Order will be SENT at price {0}", price);
                return _currentOrder.Send();
            }
            else
            {
                _logger.Trace(LogLevel.Info, "SendOrAmendCurrentOrder. Order will be AMENDED at price {0} [old price {1}]", price, _currentOrder.Price);
                return _currentOrder.Amend(price);
            }
        }

        private bool CanSendOrder(IOrder order, double price)
        {
            bool canSend = false;
            double best = 0.0;

            if (_marketData.LastSnapshot != null)
            {
                best = _marketData.LastSnapshot[order.Side].Best;
            }

            if ((order.Side == OrderSide.Buy && price <= order.LimitPrice)
                || (order.Side == OrderSide.Sell && price >= order.LimitPrice))
            {
                if (best == 0.0)
                {
                    canSend = true;
                }
                else
                {
                    if (order.Side == OrderSide.Buy && price > best)
                    {
                        canSend = true;
                    }
                    else if (order.Side == OrderSide.Sell && price < best)
                    {
                        canSend = true;
                    }
                    else
                    {
                        _logger.Trace(LogLevel.Info, "CanSendOrder. Order is a {0} and price {1} is worse than the best {0} price {2}. Order CANNOT be sent.",
                    order.Side, price, best);
                    }
                }
            }
            else
            {
                _logger.Trace(LogLevel.Info, "CanSendOrder. Order is a {0} and price {1} exceeds limit price {2}. Order CANNOT be sent.",
                    order.Side, price, order.LimitPrice);
            }

            return canSend;
        }             

        private void CompleteAssignment()
        {            
            if (_assignments.Count > 0)
            {
                _assignments.RemoveAt(_assignments.Count - 1);                
            }
            _logger.Trace(LogLevel.Debug, "CompleteAssignment. {0} Assignment(s) left to process.", _assignments.Count);
           LoadNextAssignment();
        }
    }

    [Flags]
    public enum DESAgentWakeupMode : int
    {
        AlwaysSleep = 0,
        WakeUpOnTimer = 1,
        WakeUpOnTrades = 2,
        WakeUpOnOrders = 4
    }
}
