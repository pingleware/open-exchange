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
using OPEX.StaticData;
using OPEX.DES.Exchange;
using OPEX.DES.OrderManager;

namespace OPEX.DES.ZIP
{
    class ZIPPricer
    {
        #region Consts

        private readonly double Ri_increase_min = 1.0;
        private readonly double Ri_increase_max = 1.05;
        private readonly double Ri_decrease_min = 0.95;
        private readonly double Ri_decrease_max = 1.0;

        private readonly double Ai_increase_min = 0.0;
        private readonly double Ai_increase_max = 0.05;
        private readonly double Ai_decrease_min = -0.05;
        private readonly double Ai_decrease_max = 0.0;

        private readonly double beta_min = 0.1;
        private readonly double beta_max = 0.5;

        private readonly double gamma_min = 0.2;
        private readonly double gamma_max = 0.8;

        private readonly double mu_min = 0.05;
        private readonly double mu_max = 0.35;

        #endregion Consts

        private Random _random;
        private long _step;
        private double _lambda;
        private double _mu;
        private double _p;
        private double _beta;
        private double _gamma;
        private double _GAMMA;
        private double _delta;
        private double _tau;
        private double _targetPrice;

        private string _name;
        private Logger _logger;
        private DESZIPAgent _zipAgent;
        private Instrument _instrument;
        private double _limitPrice;        

        public ZIPPricer(DESZIPAgent agent)
        {
            _zipAgent = agent;            
            _name = agent.Name;
            _logger = new Logger(string.Format("ZIPPricer({0},0)", _name));

            _random = new Random(_name.GetHashCode() ^ Guid.NewGuid().GetHashCode());

            _beta = Uniform(beta_min, beta_max);
            _gamma = Uniform(gamma_min, gamma_max);
            _mu = Uniform(mu_min, mu_max);
            if (MySide == OrderSide.Buy) { _mu = -_mu; }

            _logger.Trace(LogLevel.Method, "beta initialised to {0:F4} [{1:F4}, {2:F4}", _beta, beta_min, beta_max);
            _logger.Trace(LogLevel.Method, "gamma initialised to {0:F4} [{1:F4}, {2:F4}", _gamma, gamma_min, gamma_max);
            _logger.Trace(LogLevel.Method, "mu initialised to {0:F4} [{1:F4}, {2:F4}", _mu, mu_max, mu_max);
        }

        private OrderSide MySide { get { return _zipAgent.CurrentAssignment.Side; } }

        public void ResetLimitPrice()
        {          
            _instrument = StaticDataManager.Instance.InstrumentStaticData[_zipAgent.CurrentAssignment.Ric];
            _lambda = _zipAgent.CurrentAssignment.Price;
            _limitPrice = _zipAgent.CurrentAssignment.Price;
            _p = _lambda * (1 + _mu);           
        }

        public double Price()
        {            
            _p = Math.Round(_p / _instrument.PriceTick) * _instrument.PriceTick;

            if ((MySide == OrderSide.Buy && _p > _limitPrice)
                || (MySide == OrderSide.Sell && _p < _limitPrice))
            {
                _p = _limitPrice;
            }
            return _p;
        }

        public bool AdjustFromInactivity()
        {        
            _logger.Trace(LogLevel.Debug, "AdjustFromInactivity. Inactivity detected.");

            AggregatedDepthSnapshot snapshot = _zipAgent.LastMarketData.LastSnapshot;
            if (snapshot == null)
            {
                _logger.Trace(LogLevel.Info, "AdjustFromInactivity. Last snapshot is null. No action will be taken.");
                return false;
            }

            AggregatedDepthSide side = null;
            if (MySide == OrderSide.Sell)
            {
                side = snapshot.Buy;
            }
            else
            {
                side = snapshot.Sell;
            }
            if (side.Depth == 0)
            {
                _logger.Trace(LogLevel.Info, "AdjustFromInactivity. The opposite depth side is empty. No action will be taken.");
                return false;
            }
            double bestPrice = side[0].Price;

            if (bestPrice == 0)
            {
                _logger.Trace(LogLevel.Info, "AdjustFromInactivity. Null bestPrice. No action will be taken.");
                return false;
            }

            double limitPrice = _zipAgent.CurrentAssignment.Price;
            double desiredPrice = bestPrice;

            _logger.Trace(LogLevel.Info, "AdjustFromInactivity. desiredPrice {0} limitPrice {1} bestPrice {2}", desiredPrice, limitPrice, bestPrice);

            bool increaseProfitMargin;
            if (MySide == OrderSide.Buy)
            {
                if (desiredPrice > limitPrice)
                {
                    _logger.Trace(LogLevel.Info, "AdjustFromInactivity. desiredPrice {0} > limitPrice {1} ---> CLIPPING desiredPrice to limitPrice",
                        desiredPrice, limitPrice);
                    desiredPrice = limitPrice;
                }
                increaseProfitMargin = desiredPrice < _targetPrice;
            }
            else
            {
                if (desiredPrice < limitPrice)
                {
                    _logger.Trace(LogLevel.Info, "AdjustFromInactivity. desiredPrice {0} < limitPrice {1} ---> CLIPPING desiredPrice to limitPrice",
                        desiredPrice, limitPrice);
                    desiredPrice = limitPrice;
                }
                increaseProfitMargin = desiredPrice > _targetPrice;
            }

            if (increaseProfitMargin || _zipAgent.IsActive)
            {
                string s = (increaseProfitMargin) ? "INCREASING" : "REDUCING";
                _logger.Trace(LogLevel.Info, "AdjustFromInactivity. TargetPrice {0} desiredPrice {1}. {2} profit margin.", _targetPrice, desiredPrice, s);
                _targetPrice = desiredPrice;
                ChangeProfitMargin(increaseProfitMargin);
                return true;
            }

            _logger.Trace(LogLevel.Info, "AdjustFromInactivity. Can't reduce profit margin: agent INACTIVE. No action will be taken.");
            return false;           
        }

        public void ReceiveShout(Shout shout)
        {           
            if (shout == null)
            {
                _logger.Trace(LogLevel.Critical, "ReceiveShout. Received a NULL Shout!!!");
                return;
            }

            OrderSide shoutSide = shout.Side;
            double shoutPrice = shout.Price;
            double tradePrice = shout.TradePrice;
            bool accepted = shout.Accepted;
            OrderSide mySide = MySide;

            _targetPrice = shoutPrice;

            if (accepted)
            {
                if (mySide == OrderSide.Sell)
                {
                    if (_p <= tradePrice)
                    {
                        ChangeProfitMargin(true);
                    }

                    if (shoutSide == OrderSide.Buy)
                    {
                        if (_zipAgent.IsActive && _p >= tradePrice)
                        {
                            ChangeProfitMargin(false);
                        }
                    }
                }
                else
                {
                    if (_p >= tradePrice)
                    {
                        ChangeProfitMargin(true);
                    }

                    if (shoutSide == OrderSide.Sell)
                    {
                        if (_zipAgent.IsActive && _p <= tradePrice)
                        {
                            ChangeProfitMargin(false);
                        }
                    }
                }
            }
            else
            {
                if (mySide == OrderSide.Sell)
                {
                    if (shoutSide == OrderSide.Sell)
                    {
                        if (_zipAgent.IsActive && _p >= shoutPrice)
                        {
                            ChangeProfitMargin(false);
                        }
                    }
                }
                else
                {
                    if (shoutSide == OrderSide.Buy)
                    {
                        if (_zipAgent.IsActive && _p <= shoutPrice)
                        {
                            ChangeProfitMargin(false);
                        }
                    }
                }
            }        
        }

        private void ChangeProfitMargin(bool raise)
        {
            bool priceIncrease = (MySide == OrderSide.Buy && !raise) || (MySide == OrderSide.Sell && raise);
            double R = Uniform((priceIncrease) ? Ri_increase_min : Ri_decrease_min, (priceIncrease) ? Ri_increase_max : Ri_decrease_max);
            double A = Uniform((priceIncrease) ? Ai_increase_min : Ai_decrease_min, (priceIncrease) ? Ai_increase_max : Ai_decrease_max);
            double tau = _tau;
            double delta = _delta;
            double GAMMA = _GAMMA;
            double mu = _mu;
            double p = _p;

            _tau = R * _targetPrice + A;
            _delta = _beta * (_tau - _p);
            _GAMMA = _gamma * _GAMMA + (1 - _gamma) * _delta;
            _mu = (_p + _GAMMA) / _lambda - 1;
            _p = _lambda * (1 + _mu);

            _logger.Trace(LogLevel.Debug, "tau(t) = {0:F4}; tau(t+1) = {1:F4}", tau, _tau);
            _logger.Trace(LogLevel.Debug, "delta(t) = {0:F4}; delta(t+1) = {1:F4}", delta, _delta);
            _logger.Trace(LogLevel.Debug, "GAMMA(t) = {0:F4}; GAMMA(t+1) = {1:F4}", GAMMA, _GAMMA);
            _logger.Trace(LogLevel.Debug, "mu(t) = {0:F4}; mu(t+1) = {1:F4}", mu, _mu);
            _logger.Trace(LogLevel.Info, "p(t) = {0:F4}; p(t+1) = {1:F4}", p, _p);

            _step++;
            SetLogTitle();
        }

        private double Uniform(double min, double max)
        {
            return min + _random.NextDouble() * (max - min);
        }

        private void SetLogTitle()
        {
            _logger.LogTitle = string.Format("ZIPPricer({0},{1})", _name, _step);
        }
    }
}
