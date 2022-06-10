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

namespace OPEX.DES.GDX
{
    class DESGDXPricer
    {
        private readonly DESGDXAgent _agent;
        private readonly Logger _logger;
        private readonly bool _forceImprovePrice;
        private readonly PriceEstimator _estimator;
        private readonly double _gamma;        

        private ShoutHistory _history;
        private ContinuousBeliefFunction _beliefFunction;
        private double _lastPriceComputed;
        private Instrument _instrument;

        public DESGDXPricer(DESGDXAgent agent)
        {
            _agent = agent;
            _logger = new Logger(string.Format("GDXPricer({0})", _agent.Name));
            _forceImprovePrice = (bool)_agent.Parameters.GetValue("ForceImprovePrice", typeof(bool));
            _gamma = (double)_agent.Parameters.GetValue("Gamma", typeof(double));
            _estimator = new PriceEstimator();
        }

        public double LastPriceComputed { get { return _lastPriceComputed; } }

        public void Init()
        {
            if (_instrument == null || !_agent.CurrentAssignment.Ric.Equals(_instrument.Ric))
            {
                double alpha = (double)_agent.Parameters.GetValue("Alpha", typeof(double));
                int windowSize = (int)_agent.Parameters.GetValue("WindowSize", typeof(int));
                double tau = (double)_agent.Parameters.GetValue("Tau", typeof(double));
                double gracePeriodSecs = (double)_agent.Parameters.GetValue("GracePeriodSecs", typeof(double));

                _instrument = StaticDataManager.Instance.InstrumentStaticData[_agent.CurrentAssignment.Ric];
                _history = new ShoutHistory(_instrument.MinPrice, _instrument.MaxPrice, _agent.Name, alpha, windowSize, gracePeriodSecs, tau);
                _beliefFunction = new ContinuousBeliefFunction(_history, _agent.CurrentAssignment.Side, _instrument.MinPrice, _instrument.MaxPrice, _agent.Name);
            }
        }

        public bool Price()
        {
            bool success = true;
            OrderSide side = _agent.CurrentAssignment.Side;
            double bbid = _history.BestBid;
            double bask = _history.BestAsk;
            double limitPrice = _agent.CurrentAssignment.Price;
            double priceStepSize = _instrument.PriceTick;

            _logger.Trace(LogLevel.Debug, "Price. BestBid: {0}. BestAsk: {1}. PriceTick: {2}. LimitPrice: {3}",
                bbid, bask, priceStepSize, limitPrice);

            double searchIntervalMin;
            double searchIntervalMax;
            if (side == OrderSide.Sell)
            {
                searchIntervalMin = limitPrice;
                searchIntervalMax = bask;
                if (_forceImprovePrice)
                {
                    searchIntervalMax -= priceStepSize;
                }
                if (searchIntervalMax - searchIntervalMin < 0)
                {
                    _logger.Trace(LogLevel.Warning, "Price. Cost {0} is greater than the maximum offer allowed {1}. Can't submit an offer.",
                        limitPrice, searchIntervalMax);
                    success = false;
                }
            }
            else
            {
                searchIntervalMin = bbid;
                searchIntervalMax = limitPrice;
                if (_forceImprovePrice)
                {
                    searchIntervalMin += priceStepSize;
                }
                if (searchIntervalMax - searchIntervalMin < 0)
                {
                    _logger.Trace(LogLevel.Warning, "Price. Value {0} is less than the minimum bid allowed {1}. Can't submit a bid.",
                       limitPrice, searchIntervalMin);
                    success = false;
                }
            }

            if (!success)
            {
                return false;
            }

            _logger.Trace(LogLevel.Info, "Price. BestBid: {0} BestAsk: {1} searchMin: {2} searchMax: {3}", bbid, bask, searchIntervalMin, searchIntervalMax);
            
            int M = _agent.AssignmentsLeft;
            int N = TimeLeftToClose;

            _logger.Trace(LogLevel.Info, "Price. M: {0} N: {1} priceStepSize: {2} ~~~GDX~~~", M, N, priceStepSize);
            _logger.Trace(LogLevel.Info, "Price. AssignmentList: {0}  ~~~GDX~~~", _agent.AssignmentListDump);
            _beliefFunction.Build();
            _estimator.Init(M, N);

            double p = _estimator.OptimalPrice(_beliefFunction.Belief, this.ProfitFunction,
                searchIntervalMin, searchIntervalMax, priceStepSize, _gamma,
                _instrument.MinPrice, _instrument.MaxPrice);
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

            _lastPriceComputed = p_adj;
            _logger.Trace(LogLevel.Info, "Price. OPTIMAL PRICE::::::::::::: {0}", _lastPriceComputed);
            return true;
        }

        private double ProfitFunction(double p, int m)
        {
            Assignment a = _agent.Assignments[m - 1];

            double s = a.Price - p;
            if (a.Side == OrderSide.Sell)
            {
                s = -s;
            }

            return Math.Max(s, 0.0);
        }

        internal void NewPhaseStarted()
        {
            _logger.Trace(LogLevel.Info, "NewPhaseStarted. resetting history best prices");
            _history.ResetBestPrices();
        }
      
        public void ReceiveShout(Shout shout)
        {
            _history.Add(shout, _agent.LastMarketData.LastSnapshot);
        }

        private int TimeLeftToClose { get { return TimeManager.MovesPerRound - TimeManager.CurrentTimeStamp.Move; } }
    }
}
