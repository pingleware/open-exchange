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

using OPEX.ShoutService;
using OPEX.Common;
using OPEX.StaticData;
using OPEX.OM.Common;
using OPEX.Agents.GD.Calculus;
using OPEX.Agents.GDX.Calculus;
using OPEX.MDS.Client;
using OPEX.SupplyService.Common;
using OPEX.Agents.Common;
using OPEX.DWEAS.Client;

namespace OPEX.Agents.GDX
{
    public class GDXPricer
    {
        private readonly Logger _logger;
        private readonly GDXAgent _agent;
        private readonly double _gamma;        
        private readonly int _primaryTimerPeriodMsec;
        private readonly PriceEstimator _estimator;
        private readonly bool _forceImprovePrice;

        private ContinuousBeliefFunction _beliefFunction;        
        private ShoutHistory _history;
        private Instrument _lastInstrument;
        private double _lastPrice = double.NaN;
        private DateTime _nextClosingTime;        

        public GDXPricer(GDXAgent agent)
        {
            _agent = agent;
            _logger = new Logger(string.Format("GDXPricer({0})", _agent.Name));
            _gamma = (double)_agent.Parameters.GetValue("Gamma", typeof(double));            
            _forceImprovePrice = (bool)_agent.Parameters.GetValue("ForceImprovePrice", typeof(bool));
            AgentInfo info = StaticDataManager.Instance.AgentsStaticData.Agents[_agent.Name];
            _primaryTimerPeriodMsec = info.SleepTimeMsec;
            _estimator = new PriceEstimator();
        }

        public double LastPriceComputed { get { return _lastPrice; } }

        public void ReceiveShout(Shout shout)
        {
            _history.Add(shout, _agent.AgentStatus.LastSnapshot);
        }

        public void Init()
        {
            if (_lastInstrument == null || !_lastInstrument.Ric.Equals(_agent.AgentStatus.CurrentInstrument.Ric))
            {                
                double alpha = (double)_agent.Parameters.GetValue("Alpha", typeof(double));
                int windowSize = (int)_agent.Parameters.GetValue("WindowSize", typeof(int));
                double tau = (double)_agent.Parameters.GetValue("Tau", typeof(double));
                double gracePeriodSecs = (double)_agent.Parameters.GetValue("GracePeriodSecs", typeof(double));

                _lastInstrument = _agent.AgentStatus.CurrentInstrument;
                _history = new ShoutHistory(_lastInstrument.MinPrice, _lastInstrument.MaxPrice, _agent.Name, alpha, windowSize, gracePeriodSecs, tau);
                _beliefFunction = new ContinuousBeliefFunction(_history, _agent.AgentStatus.CurrentAssignmentBucket.Side, _lastInstrument.MinPrice, _lastInstrument.MaxPrice, _agent.Name);
            }
        }

        public bool Price() 
        {            
            OrderSide side = _agent.AgentStatus.CurrentAssignmentBucket.Side;
            double bbid = _history.BestBid;
            double bask = _history.BestAsk;
            double limitPrice = _agent.AgentStatus.CurrentAssignmentBucket.Price;
            double priceStepSize = _lastInstrument.PriceTick;

            _logger.Trace(LogLevel.Debug, "Price. BestBid: {0}. BestAsk: {1}. PriceTick: {2}. LimitPrice: {3}",
                bbid, bask, priceStepSize, limitPrice);          

            double searchIntervalMin;
            double searchIntervalMax;
            if (side == OrderSide.Sell)
            {
                searchIntervalMin = limitPrice;
                searchIntervalMax = _agent.AgentStatus.CurrentInstrument.MaxPrice;                
            }
            else
            {                
                searchIntervalMax = limitPrice;
                searchIntervalMin = _agent.AgentStatus.CurrentInstrument.MinPrice;                
            }           

            _logger.Trace(LogLevel.Info, "Price. BestBid: {0} BestAsk: {1} searchMin: {2} searchMax: {3}", 
                bbid, bask, searchIntervalMin, searchIntervalMax);

            double T = TimeLeftToClose;
            int M = _agent.AssignmentController.QtyRem + _agent.AssignmentController.QtyOnMkt;
            int N = (int)Math.Floor(T * 1000.0 / (double)_primaryTimerPeriodMsec);

            _logger.Trace(LogLevel.Info, "Price. M: {0} N: {1} TmLftToCls: {2} TABLE: {3} ~~~GDX~~~",
                M, N, (int)T, _agent.AssignmentController.ToString());

            if (M == 0)
            {
                _logger.Trace(LogLevel.Warning, "Price. M=0: no assignments left to process.");
                return false;
            }

            if (N == 0)
            {
                _logger.Trace(LogLevel.Warning, "Price. N=0: no time left to send an order.");
                return false;
            }            

            _beliefFunction.Build();

            double xmin = _beliefFunction.Function.XMin;
            double xmax = _beliefFunction.Function.XMax;

            _logger.Trace(LogLevel.Warning, "Price. beliefFunction [{0} {1}].",
                xmin, xmax);

            if (searchIntervalMax < xmin || searchIntervalMin > xmax)
            {
                _logger.Trace(LogLevel.Warning, "Price. searchInterval [{0} {1}] not contained in beliefFunction [{2} {3}]. Returning false.",
                    searchIntervalMin, searchIntervalMax, xmin, xmax);                
                return false;
            }

            if (side == OrderSide.Buy)
            {
                searchIntervalMin = xmin;                
            }
            else
            {
                searchIntervalMax = xmax;
            }

            _estimator.Init(M, N);

            double p = _estimator.OptimalPrice(_beliefFunction.Belief, this.ProfitFunction, 
                searchIntervalMin, searchIntervalMax, priceStepSize, _gamma);           

            double p_adj = p;
            
            if (p < searchIntervalMin || p > searchIntervalMax)
            {                
                if (side == OrderSide.Buy)
                {
                    p_adj = searchIntervalMin;
                }
                else
                {
                    p_adj = searchIntervalMax;
                }
                _logger.Trace(LogLevel.Error, "Price. THIS REALLY SHOULDN'T HAPPEN. An adjustment was made to the result of the optimisation: p = {0} p_adj = {1}", p, p_adj);                
            }

            _lastPrice = p_adj;
            _logger.Trace(LogLevel.Info, "Price. OPTIMAL PRICE::::::::::::: {0}", _lastPrice);
            return true;
        }

        public void NewSessionStarted(SessionChangedEventArgs sessionChangedEventArgs)
        {           
            _nextClosingTime = sessionChangedEventArgs.EndTime;
            _logger.Trace(LogLevel.Info, "NewSessionStarted. ##################### nextClosingTime: {0}", _nextClosingTime);
            _history.ResetBestPrices();
        }

        private readonly double DefaultSessionLengthSec = 60 * 30;
        private double TimeLeftToClose
        {
            get 
            {
                DateTime now = DateTime.Now;
                DateTime nextClosingTime = _agent.AgentStatus.LastSessionInfo.EndTime;
                double T = nextClosingTime.Subtract(now).TotalSeconds;

                _logger.Trace(LogLevel.Debug, "TimeLeftToClose: nextClose {0} now {1} timeLeftSec {2}", 
                    nextClosingTime.ToString(), now, T);             
                
                if (T < 0)
                {
                    _logger.Trace(LogLevel.Info, "TimeLeftToClose: result NEGATIVE -> will be capped to 0 seconds");
                    T = 0; 
                }

                return T;
            } 
        }

        private AssignmentBucket M2AssignmentBucket(int m)
        {
            AssignmentBucket ab = null;
            foreach (AssignmentBucket a in _agent.AssignmentBuckets.Values)
            {
                if (m > a.Qty - a.QtyFilled)
                {
                    m -= a.Qty - a.QtyFilled;
                    continue;
                }

                ab = a;
                break;
            }

            return ab;
        }

        private double ProfitFunction(double p, int m)
        {            
            AssignmentBucket ab = M2AssignmentBucket(m);            
            
            if (ab == null)
            {
                _logger.Trace(LogLevel.Error,
                    "ProfitFunction. COULDN'T FIND assignment # {0}. TABLE: {1}",
                    m, _agent.AssignmentController.ToString());

                return Double.NaN;
            }            

            double profit = ab.Price - p;
            if (ab.Side == OrderSide.Sell)
            {
                profit = -profit;
            }            
            
            profit = Math.Max(profit, 0.0);
            return profit;
        }
    }
}
