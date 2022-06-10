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

using OPEX.OM.Common;
using OPEX.Agents.Common;
using OPEX.MDS.Common;
using OPEX.Common;

namespace OPEX.Agents.AA.Components
{
    class AdaptiveComponent
    {
        private readonly double InitialThetaGuessPercentageAroundMid = 0.5;
        private readonly double Gamma = 2.0;
        private readonly double ThetaMin = -8.0;
        private readonly double ThetaMax = 2.0;

        private readonly double LambdaRelative;
        private readonly double LambdaAbsolute;

        private readonly double Beta1;
        private readonly double Beta2;                
        private readonly OrderSide Side;
        private readonly Cache<double> _transactionPrices;        
        private readonly Logger _logger;
        private readonly AAAgent _agent;

        private double _limitPrice;
        private double _aggressiveness;     
        private double _theta;
        private double _alphaMin;
        private double _alphaMax;
        private AggressivenessModel _aggressivenessModel;

        public double Theta { get { return _theta; } }
        public double Aggressiveness { get { return _aggressiveness; } }

        public AdaptiveComponent(AAAgent agent, OrderSide side, double limitPrice, AggressivenessModel aggressivenessModel)
        {
            _logger = new Logger(string.Format("AdaptiveComponent({0})", agent.Name));

            _agent = agent;
            _limitPrice = limitPrice;
            Side = side;

            Beta1 = (double)agent.Parameters.GetValue("Beta1", typeof(double));
            Beta2 = (double)agent.Parameters.GetValue("Beta2", typeof(double));            
            LambdaRelative = (double)agent.Parameters.GetValue("LambdaRelative", typeof(double));
            LambdaAbsolute = (double)agent.Parameters.GetValue("LambdaAbsolute", typeof(double));
            int windowSize = (int)agent.Parameters.GetValue("WindowSize", typeof(int));

            _alphaMax = double.NegativeInfinity;
            _alphaMin = double.PositiveInfinity;

            _transactionPrices = new Cache<double>(windowSize);
            _aggressivenessModel = aggressivenessModel;

            Initialise();
        }       

        public void UpdateLimitPrice(double limitPrice)
        {
            _logger.Trace(LogLevel.Debug, "UpdateLimitPrice. Limit price updated to {0}", limitPrice);
            _limitPrice = limitPrice;
        }

        public void InitialiseAggressiveness(double aggressiveness)
        {
            _logger.Trace(LogLevel.Debug, "InitialiseAggressiveness. r = {0}", aggressiveness);
            _aggressiveness = aggressiveness;
        }

        public void UpdateLongTerm(double transactionPrice, double estimatedPrice)
        {
            _transactionPrices.Add(transactionPrice);
            int N = _transactionPrices.Count;
            _logger.Trace(LogLevel.Debug, "UpdateLongTerm. transactionPrice {0} added to buffer. Prices in buffer now: {1}. Buffer: {2}", 
                transactionPrice, N, _transactionPrices.ToString());

            double sum = 0.0;
            foreach (double price in _transactionPrices)
            {
                double d = price - estimatedPrice;
                sum += d * d;
            }
            double alpha = Math.Sqrt(sum / (double)N) / estimatedPrice;
            if (alpha < _alphaMin)
            {
                _alphaMin = alpha;
            }
            if (alpha > _alphaMax)
            {
                _alphaMax = alpha;
            }
            _logger.Trace(LogLevel.Debug, "UpdateLongTerm. alpha {0} alphaMin {1} alphaMax {2}", alpha, _alphaMin, _alphaMax);

            double a = 0.5;
            if (_alphaMax != _alphaMin)
            {
                a = (alpha - _alphaMin) / (_alphaMax - _alphaMin);
            }
            double thetaStar = (ThetaMax - ThetaMin) * (1.0 - a) * Math.Exp(Gamma * (a - 1.0)) + ThetaMin;
            double newTheta = _theta + Beta2 * (thetaStar - _theta);

            _logger.Trace(LogLevel.Debug,
                "UpdateLongTerm. theta(n+1) = theta(n) + Beta2 * (ThetaStar(alpha) - theta(n)) = {0} + {1} * ({2} - {0}) = {3}",
                _theta, Beta2, thetaStar, newTheta);

            _theta = newTheta;
        }

        public bool UpdateShortTermFromInactivity(double estimatedPrice)
        {
            _logger.Trace(LogLevel.Debug, "UpdateShortTermFromInactivity.");

            AggregatedDepthSnapshot snapshot = _agent.AgentStatus.LastSnapshot;            
            if (snapshot == null)
            {
                _logger.Trace(LogLevel.Info, "UpdateShortTermFromInactivity. Last snapshot is null. No action will be taken.");
                return false;
            }

            AggregatedDepthSide side = null;
            if (Side == OrderSide.Sell)
            {
                side = snapshot.Buy;
            }
            else
            {
                side = snapshot.Sell;
            }
            if (side.Depth == 0)
            {
                _logger.Trace(LogLevel.Info, "UpdateShortTermFromInactivity. The opposite depth side is empty. No action will be taken.");
                return false;
            }
            double bestPrice = side[0].Price;

            if (bestPrice == 0)
            {
                _logger.Trace(LogLevel.Info, "UpdateShortTermFromInactivity. Null bestPrice. No action will be taken.");
                return false;
            }
            
            double desiredPrice = bestPrice;
            double currentTargetPrice = _aggressivenessModel.ComputeTau(_theta, _aggressiveness, estimatedPrice);
            _logger.Trace(LogLevel.Info, "UpdateShortTermFromInactivity. desiredPrice {0} limitPrice {1} bestPrice {2} currentTargetPrice {3}", 
                desiredPrice, _limitPrice, bestPrice, currentTargetPrice);

            bool increaseProfitMargin;
            
            if (Side == OrderSide.Buy)
            {
                if (desiredPrice > _limitPrice)
                {                    
                    _logger.Trace(LogLevel.Info, "UpdateShortTermFromInactivity. desiredPrice {0} > limitPrice {1} ---> CLIPPING desiredPrice to limitPrice",
                        desiredPrice, _limitPrice);
                    desiredPrice = _limitPrice;
                }
                increaseProfitMargin = desiredPrice < currentTargetPrice;
            }
            else
            {
                if (desiredPrice < _limitPrice)
                {                    
                    _logger.Trace(LogLevel.Info, "UpdateShortTermFromInactivity. desiredPrice {0} < limitPrice {1} ---> CLIPPING desiredPrice to limitPrice",
                        desiredPrice, _limitPrice);
                    desiredPrice = _limitPrice;
                }
                increaseProfitMargin = desiredPrice > currentTargetPrice;
            }

            if (increaseProfitMargin || _agent.IsActive)
            {
                string s = (increaseProfitMargin) ? "INCREASING" : "REDUCING";
                _logger.Trace(LogLevel.Info, "UpdateShortTermFromInactivity. CurrentTargetPrice {0} desiredPrice {1}. {2} profit margin.", currentTargetPrice, desiredPrice, s);
                UpdateAggressiveness(increaseProfitMargin, estimatedPrice, desiredPrice);
                return true;
            }
            
            _logger.Trace(LogLevel.Info, "UpdateShortTermFromInactivity. Can't reduce profit margin: agent INACTIVE. No action will be taken.");
            return false;            
        }

        public void UpdateShortTerm(ShoutStimulus shoutStimulus, double estimatedPrice)
        {
            double desiredTargetPrice = double.NaN;
            double currentTargetPrice = _aggressivenessModel.ComputeTau(_theta, _aggressiveness, estimatedPrice);

            _logger.Trace(LogLevel.Debug, "UpdateShortTerm. Estimated price: {0} currentTargetPrice {1}.",
                estimatedPrice, currentTargetPrice);

            bool changeAggressiveness = false;
            bool increaseAggressiveness = false;            
            if (shoutStimulus.Shout.Accepted)
            {
                _logger.Trace(LogLevel.Debug, "UpdateShortTerm. Last shout was ACCEPTED. Updating aggressiveness.");
                changeAggressiveness = true;
                double pT = shoutStimulus.LastTrade.Price;
                if (Side == OrderSide.Buy)
                {
                    if (currentTargetPrice < pT)
                    {
                        _logger.Trace(LogLevel.Info, "UpdateShortTerm. Agent is a BUYER. pT {0} > currentTargetPrice {1}. INCREASING aggressiveness.", pT, currentTargetPrice);
                        increaseAggressiveness = true;
                    }
                    else
                    {
                        _logger.Trace(LogLevel.Info, "UpdateShortTerm. Agent is a BUYER. pT {0} <= currentTargetPrice {1}. DECREASING aggressiveness.", pT, currentTargetPrice);
                    }
                }
                else if (Side == OrderSide.Sell)
                {
                    if (currentTargetPrice > pT)
                    {
                        _logger.Trace(LogLevel.Info, "UpdateShortTerm. Agent is a SELLER. pT {0} < currentTargetPrice {1}. INCREASING aggressiveness.", pT, currentTargetPrice);
                        increaseAggressiveness = true;
                    }
                    else
                    {
                        _logger.Trace(LogLevel.Info, "UpdateShortTerm. Agent is a SELLER. pT {0} >= currentTargetPrice {1}. DECREASING aggressiveness.", pT, currentTargetPrice);
                    }
                }
                desiredTargetPrice = pT;
            }
            else if (Side == OrderSide.Buy && shoutStimulus.Shout.Side == OrderSide.Buy)
            {
                double bid = shoutStimulus.Shout.Price;
                if (currentTargetPrice <= bid)
                {
                    _logger.Trace(LogLevel.Info, "UpdateShortTerm. Last shout was a REJECTED BID at {0}. Agent is a BUYER. currentTargetPrice {1} <= bid {0}. INCREASING aggressiveness.", bid, currentTargetPrice);
                    changeAggressiveness = true;
                    increaseAggressiveness = true;
                    desiredTargetPrice = bid;
                }
                else
                {
                    _logger.Trace(LogLevel.Info, "UpdateShortTerm. Last shout was a REJECTED BID at {0}. Agent is a BUYER. currentTargetPrice {1} > bid {0}. NOT UPDATING aggressiveness.", bid, currentTargetPrice);
                }
            }
            else if (Side == OrderSide.Sell && shoutStimulus.Shout.Side == OrderSide.Sell)
            {
                double ask = shoutStimulus.Shout.Price;
                if (currentTargetPrice >= ask)
                {
                    _logger.Trace(LogLevel.Info, "UpdateShortTerm. Last shout was a REJECTED OFFER at {0}. Agent is a SELLER. currentTargetPrice {1} >= ask {0}. INCREASING aggressiveness.", ask, currentTargetPrice);
                    changeAggressiveness = true;
                    increaseAggressiveness = true;
                    desiredTargetPrice = ask;
                }
                else
                {
                    _logger.Trace(LogLevel.Info, "UpdateShortTerm. Last shout was a REJECTED OFFER at {0}. Agent is a SELLER. currentTargetPrice {1} < ask {0}. NOT UPDATING aggressiveness.", ask, currentTargetPrice);
                }
            }

            if (changeAggressiveness)
            {
                UpdateAggressiveness(increaseAggressiveness, estimatedPrice, desiredTargetPrice);
            }
        }

        private void UpdateAggressiveness(bool increaseAggressiveness, double estimatedPrice, double desiredTargetPrice)
        {
            double delta;
            _logger.Trace(LogLevel.Debug, "UpdateAggressiveness. increaseAggressiveness: {0} estimatedPrice {1} desiredTargetPrice {2}", increaseAggressiveness, estimatedPrice, desiredTargetPrice);

            double rShout = _aggressivenessModel.ComputeRShout(_theta, estimatedPrice, desiredTargetPrice);
            _logger.Trace(LogLevel.Debug, "UpdateAggressiveness. rShout(theta={0},p*={1},pi={2}): {3}", _theta, estimatedPrice, desiredTargetPrice, rShout);

            if (increaseAggressiveness)
            {
                delta = (1.0 + LambdaRelative) * rShout + LambdaAbsolute;
                _logger.Trace(LogLevel.Debug,
                    "UpdateAggressiveness. delta = (1.0 + LambdaRelative) * rShout + LambdaAbsolute = (1.0 + {0}) * {1} + {2} = {3}",
                    LambdaRelative, rShout, LambdaAbsolute, delta);
            }
            else
            {
                delta = (1.0 - LambdaRelative) * rShout - LambdaAbsolute;
                _logger.Trace(LogLevel.Debug,
                    "UpdateAggressiveness. delta = (1.0 - LambdaRelative) * rShout - LambdaAbsolute = (1.0 - {0}) * {1} - {2} = {3}",
                    LambdaRelative, rShout, LambdaAbsolute, delta);
            }

            double newAggressiveness = _aggressiveness + Beta1 * (delta - _aggressiveness);
            _logger.Trace(LogLevel.Debug,
                   "UpdateAggressiveness. r(n+1) = r(n) + Beta1 * (delta - r(n)) = {0} + {1} * ({2} - {0}) = {3}",
                   _aggressiveness, Beta1, delta, newAggressiveness);
            _aggressiveness = newAggressiveness;
        }        

        private void Initialise()
        {
            Random r = new Random();
            double thetaRange = ThetaMax - ThetaMin;
            double pMin = ThetaMin + 0.5 * thetaRange * (1.0 - InitialThetaGuessPercentageAroundMid);
            _theta = pMin + r.NextDouble() * InitialThetaGuessPercentageAroundMid * thetaRange;
            _logger.Trace(LogLevel.Info,
                "Initialise. theta set to {4}. ThetaMin {0} ThetaMax {1} thetaRange {2} guess% {3}",
                ThetaMin, ThetaMax, thetaRange, InitialThetaGuessPercentageAroundMid, _theta);
        }
    }
}
