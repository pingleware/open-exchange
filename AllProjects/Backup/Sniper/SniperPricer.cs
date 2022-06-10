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
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OPEX.OM.Common;
using OPEX.ShoutService;
using OPEX.Agents.Common;
using OPEX.MDS.Common;
using OPEX.MDS.Client;
using OPEX.StaticData;
using OPEX.Common;
using OPEX.SupplyService.Common;
using OPEX.DWEAS.Client;

namespace OPEX.Agents.Sniper
{    
    internal class SniperPricer
    {
        private readonly double MaxBidSpreadFactor;
        private readonly double MaxAskSpreadFactor;
        private readonly double MinimumProfitFactor;
        
        private double _currentMinTradePrice;
        private double _currentMaxTradePrice;
        private double _lastSessionMinTradePrice;
        private double _lastSessionMaxTradePrice;
        private double _bestBid;
        private double _bestAsk;

        private SniperAgent _agent;
        private OutgoingOrder _currentOrder;
        private Instrument _lastInstrument;
        private Logger _logger;

        public SniperPricer(SniperAgent agent, OutgoingOrder currentOrder)
        {
            _agent = agent;
            _currentOrder = currentOrder;
            _logger = new Logger(string.Format("SniperPricer({0})", _agent.Name));
            MaxAskSpreadFactor = (double)_agent.Parameters.GetValue("MaxAskSpreadFactor", typeof(double));
            MaxBidSpreadFactor = (double)_agent.Parameters.GetValue("MaxBidSpreadFactor", typeof(double));
            MinimumProfitFactor = (double)_agent.Parameters.GetValue("MinimumProfitFactor", typeof(double));
        }        

        public void Init()
        {
            if (_lastInstrument == null || !_lastInstrument.Ric.Equals(_agent.AgentStatus.CurrentInstrument.Ric))
            {
                _lastInstrument = _agent.AgentStatus.CurrentInstrument;
                _lastSessionMinTradePrice = _agent.AgentStatus.CurrentInstrument.MinPrice;
                _lastSessionMaxTradePrice = _agent.AgentStatus.CurrentInstrument.MaxPrice;
                ResetCurrentTradePrices();
            }          
        }
       
        public void ReceiveShout(ShoutStimulus shoutStimulus)
        {            
            if (shoutStimulus.Shout.Accepted)
            {
                LastTradeUpdateMessage trade = shoutStimulus.LastTrade;
                if (trade.Price > _currentMaxTradePrice)
                {
                    _logger.Trace(LogLevel.Debug, "ReceiveShout. CurrentMaxTradePrice updated from {0} to {1}", _currentMaxTradePrice, trade.Price);
                    _currentMaxTradePrice = trade.Price;
                }
                if (trade.Price < _currentMinTradePrice)
                {
                    _logger.Trace(LogLevel.Debug, "ReceiveShout. CurrentMinTradePrice updated from {0} to {1}", _currentMinTradePrice, trade.Price);
                    _currentMinTradePrice = trade.Price;
                }
            }            
        }       

        public void SessionChanged(SessionStimulus sessionStimulus)
        {
            SessionChangedEventArgs sessionInfo = sessionStimulus.SessionInfo;

            if (sessionInfo.SessionState == SessionState.Close)
            {
                _bestBid = _agent.AgentStatus.CurrentInstrument.MinPrice;
                _bestAsk = _agent.AgentStatus.CurrentInstrument.MaxPrice;
                return;
            }

            _logger.Trace(LogLevel.Info, "SessionChanged. Saving min and MAX trade prices: ({0}, {1})", _currentMinTradePrice, _currentMaxTradePrice);

            if (_currentMaxTradePrice == double.NegativeInfinity)
            {
                _lastSessionMaxTradePrice = _agent.AgentStatus.CurrentInstrument.MaxPrice;
            }
            else
            {
                _lastSessionMaxTradePrice = _currentMaxTradePrice;
            }
            if (_currentMinTradePrice == double.PositiveInfinity)
            {
                _lastSessionMinTradePrice = _agent.AgentStatus.CurrentInstrument.MinPrice;
            }
            else
            {
                _lastSessionMinTradePrice = _currentMinTradePrice;
            }
            ResetCurrentTradePrices();
        }

        private void PriceBuy(bool timerExpired, ref double price, ref bool allButLimitPriceCheckIsGood, ref bool doProfitRatioCheck)
        {            
            double limitPrice = _agent.AgentStatus.CurrentAssignmentBucket.Price;

            if (timerExpired)
            {
                allButLimitPriceCheckIsGood = true;
                price = _bestAsk;
                _logger.Trace(LogLevel.Debug, "PriceBuy. TimerExpired. Price = {0} -> will send order if limitPrice check passes", price);
            }
            else
            {                
                if (_bestAsk < _lastSessionMinTradePrice)
                {
                    price = _bestAsk;
                    allButLimitPriceCheckIsGood = true;
                    _logger.Trace(LogLevel.Debug, "PriceBuy. (1) BestAsk is LESS than the MinTradePrice in the previous session. Price = {0} -> will send order if limitPrice check passes", price);
                }
                else if (_bestAsk < _lastSessionMaxTradePrice)
                {
                    double ratio = (_bestAsk - _bestBid) / _bestAsk;
                    _logger.Trace(LogLevel.Debug, "PriceBuy. (2) BestAsk is LESS than the MaxTradePrice in the previous session. Ratio = {0}. MaxSpreadFactor = {1}", ratio, MaxAskSpreadFactor);
                    if (ratio < MaxAskSpreadFactor)
                    {
                        price = _bestAsk;
                        doProfitRatioCheck = true;
                        allButLimitPriceCheckIsGood = true;
                        _logger.Trace(LogLevel.Debug, "PriceBuy. (2) Ratio {0} < MaxSpreadFactor {1} -> will send order if limitPrice & profitRatio checks pass", ratio, MaxAskSpreadFactor);
                    }
                    else
                    {
                        _logger.Trace(LogLevel.Debug, "PriceBuy. (2) Ratio {0} >= MaxSpreadFactor {1} -> will NOT send order", ratio, MaxAskSpreadFactor);
                    }
                }                            
            }
        }

        private void PriceSell(bool timerExpired, ref double price, ref bool allButLimitPriceCheckIsGood, ref bool doProfitRatioCheck)
        {
            double limitPrice = _agent.AgentStatus.CurrentAssignmentBucket.Price;

            if (timerExpired)
            {
                allButLimitPriceCheckIsGood = true;
               price = _bestBid;
               _logger.Trace(LogLevel.Debug, "PriceSell. TimerExpired. Price = {0} -> will send order if limitPrice check passes", price);
            }
            else
            {
                if (_bestBid > _lastSessionMaxTradePrice)
                {
                    price = _bestBid;
                    allButLimitPriceCheckIsGood = true;
                    _logger.Trace(LogLevel.Debug, "PriceSell. (1) BestBid is MORE than the MaxTradePrice in the previous session. Price = {0} -> will send order if limitPrice check passes", price);
                }
                else if (_bestBid > _lastSessionMinTradePrice)
                {
                    double ratio = (_bestAsk - _bestBid) / _bestBid;
                    _logger.Trace(LogLevel.Debug, "PriceSell. (2) BestBid is MORE than the MinTradePrice in the previous session. Ratio = {0}. MaxSpreadFactor = {1}", ratio, MaxBidSpreadFactor);
                    if (ratio < MaxBidSpreadFactor)
                    {
                        price = _bestBid;
                        doProfitRatioCheck = true;
                        allButLimitPriceCheckIsGood = true;
                        _logger.Trace(LogLevel.Debug, "PriceSell. (2) Ratio {0} < MaxSpreadFactor {1} -> will send order if limitPrice & profitRatio checks pass", ratio, MaxBidSpreadFactor);
                    }
                    else
                    {
                        _logger.Trace(LogLevel.Debug, "PriceSell. (2) Ratio {0} >= MaxSpreadFactor {1} -> will NOT send order", ratio, MaxBidSpreadFactor);
                    }
                }
            }        
        }
                                     
        public double Price(bool timerExpired, out bool success)
        {
            double price = 0.0;
            bool doProfitRatioCheck = false;
            bool allButLimitPriceCheckIsGood = false;
            double limitPrice = _agent.AgentStatus.CurrentAssignmentBucket.Price;
            success = false;

            UpdateBest();

            _logger.Trace(LogLevel.Info, "Price. LimitPrice {4}. BestBid {0}. BestAsk {1}. LastMinTrdPrc {2}. LastMaxTrdPrc {3}. TimerExpired {5}.",
                _bestBid, _bestAsk, _lastSessionMinTradePrice, _lastSessionMaxTradePrice, limitPrice, timerExpired);
         
            if (_currentOrder.Side == OrderSide.Buy)
            {
                PriceBuy(timerExpired, ref price, ref allButLimitPriceCheckIsGood, ref doProfitRatioCheck);
            }
            else
            {
                PriceSell(timerExpired, ref price, ref allButLimitPriceCheckIsGood, ref doProfitRatioCheck);
            }                

            if (allButLimitPriceCheckIsGood)
            {                
                double expectedProfit = price - limitPrice;
                if (_currentOrder.Side == OrderSide.Buy)
                {
                    expectedProfit = -expectedProfit;
                }
                _logger.Trace(LogLevel.Debug, "Price. LimitPriceCheck BEGIN. Expected profit {0}", expectedProfit);
                if (expectedProfit >= 0)
                {
                    if (doProfitRatioCheck)
                    {
                        _logger.Trace(LogLevel.Debug, "Price. ProfitRatioCheck BEGIN.");
                        double profitRatio = expectedProfit / limitPrice;
                        if (profitRatio > MinimumProfitFactor)
                        {
                            _logger.Trace(LogLevel.Debug, "Price. ProfitRatio {0} > MinimumProfitFactor {1}. ProfitRatioCheck PASSED successfully.",
                                profitRatio, MinimumProfitFactor);
                            success = true;
                        }
                        else
                        {
                            _logger.Trace(LogLevel.Debug, "Price. ProfitRatio {0} <= MinimumProfitFactor {1}. ProfitRatioCheck FAILED.",
                                profitRatio, MinimumProfitFactor);
                        }
                        _logger.Trace(LogLevel.Debug, "Price. ProfitRatioCheck END.");
                    }
                    else
                    {
                        _logger.Trace(LogLevel.Debug, "Price. LimitPriceCheck PASSED successfully.");
                        success = true;
                    }
                }
                else
                {
                    _logger.Trace(LogLevel.Debug, "Price. LimitPriceCheck FAILED: profit < 0");
                }
                _logger.Trace(LogLevel.Debug, "Price. LimitPriceCheck END.");
            }

            _logger.Trace(LogLevel.Info, "Price. Success {0}. Price {1}.", success, price);
            return price;
        }

        private void UpdateBest()
        {
            AggregatedDepthSnapshot aggregatedDepthSnapshot = _agent.AgentStatus.LastSnapshot;
            double bBid = aggregatedDepthSnapshot[OrderSide.Buy].Best;
            double bAsk = aggregatedDepthSnapshot[OrderSide.Sell].Best;
            double newBestBid = _bestBid;
            double newBestAsk = _bestAsk;

            _logger.Trace(LogLevel.Debug, "UpdateBest. Snapshot.Buy.Best = {0}. Snapshot.Sell.Best = {1}. _bestBid = {2}. _bestAsk = {3}", bBid, bAsk, _bestBid, _bestAsk);

            if (bBid == 0)
            {
                newBestBid = _agent.AgentStatus.CurrentInstrument.MinPrice;
                _logger.Trace(LogLevel.Debug, "UpdateBest. bBid == 0. newBestBid set to {0}", newBestBid);
            }
            else if (bBid > 0)
            {
                newBestBid = bBid;
                _logger.Trace(LogLevel.Debug, "UpdateBest. bBid > 0. newBestBid set to {0}", newBestBid);
            }

            if (bAsk == 0)
            {
                newBestAsk = _agent.AgentStatus.CurrentInstrument.MaxPrice;
                _logger.Trace(LogLevel.Debug, "UpdateBest. bAsk == 0. newBestAsk set to {0}", newBestAsk);
            }
            else if (bAsk > 0)
            {
                newBestAsk = bAsk;
                _logger.Trace(LogLevel.Debug, "UpdateBest. bAsk > 0. newBestAsk set to {0}", newBestAsk);
            }

            _logger.Trace(LogLevel.Debug, "UpdateBest. Old Best Prices ({0}, {1}). New Best Prices ({2}, {3}).", _bestBid, _bestAsk, newBestBid, newBestAsk);

            _bestAsk = newBestAsk;
            _bestBid = newBestBid;
        }

        private void ResetCurrentTradePrices()
        {
            _currentMaxTradePrice = double.NegativeInfinity;
            _currentMinTradePrice = double.PositiveInfinity;
        }
    }
}
