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
using OPEX.DES.OrderManager;

namespace OPEX.DES.AA
{
    class BiddingComponent
    {
        private delegate double PriceDelegate(double tau, out bool success);
        private readonly double MaxSpread;
        private readonly double LambdaRelative;
        private readonly double LambdaAbsolute;
        private readonly double Eta;
        private PriceDelegate InnerPrice;
        private readonly Logger _logger;

        private Instrument _currentInstrument;
        private double _limitPrice;
        private double _bestBid;
        private double _bestAsk;
        private bool _isFirstTradingRound;

        public BiddingComponent(DESAAAgent agent)
        {
            _logger = new Logger(string.Format("BiddingComponent({0})", agent.Name));
            _isFirstTradingRound = true;            

            Eta = (double)agent.Parameters.GetValue("Eta", typeof(double));
            LambdaRelative = (double)agent.Parameters.GetValue("LambdaRelative", typeof(double));
            LambdaAbsolute = (double)agent.Parameters.GetValue("LambdaAbsolute", typeof(double));
            MaxSpread = (double)agent.Parameters.GetValue("MaxSpread", typeof(double));  

            _logger.Trace(LogLevel.Info, "C'tor. Eta: {0} LambdaRelative {1} LambdaAbsolute {2}", Eta, LambdaRelative, LambdaAbsolute);
        }

        public OrderSide Side
        {
            set
            {
                if (value == OrderSide.Buy)
                {
                    InnerPrice = InnerPriceBuyer;
                }
                else
                {
                    InnerPrice = InnerPriceSeller;
                }
            }
        }
        public double LimitPrice { set { _limitPrice = value; } }
        public Instrument Instrument { set { _currentInstrument = value; } }

        public bool IsFirstTradingRound { get { return _isFirstTradingRound; } set { _isFirstTradingRound = value; } }

 

        public void UpdateBestPrices(double bBid, double bAsk)
        {
            if (bBid == 0.0)
            {
                bBid = _currentInstrument.MinPrice;
            }
            if (bAsk == 0.0)
            {
                bAsk = _currentInstrument.MaxPrice;
            }

            _bestBid = bBid;
            _bestAsk = bAsk;

            _logger.Trace(LogLevel.Debug, "UpdateBestPrices. bBid: {0} bAsk: {1} --> bestBid {2} bestAsk {3}", bBid, bAsk, _bestBid, _bestAsk);
        }

        public double Price(double tau, out bool success)
        {
            if (!_isFirstTradingRound && double.IsNaN(tau))
            {
                _logger.Trace(LogLevel.Warning, "Price. tau = NaN. success = false. price = NaN");
                success = false;
                return double.NaN;
            }

            double price = InnerPrice(tau, out success);
            int ticksInPrice = (int)(price / _currentInstrument.PriceTick);
            return (double)ticksInPrice * _currentInstrument.PriceTick;
        }

        private double InnerPriceBuyer(double tau, out bool success)
        {
            double price = 0.0;
            success = false;

            if (_limitPrice <= _bestBid)
            {
                _logger.Trace(LogLevel.Debug, "InnerPriceBuyer. LimitPrice {0} <= BestBid {1}. Will send an order @ limitPrice {0}", _limitPrice, _bestBid);
                success = true;
                price = _limitPrice;
            }
            else if (_limitPrice >= _bestAsk && ((_bestAsk - _bestBid) <= MaxSpread))
            {
                _logger.Trace(LogLevel.Debug, "InnerPriceBuyer. LimitPrice {0} >= BestAsk {1} && spread<=MaxSpread. Will send an order @ BestAsk {1}", _limitPrice, _bestAsk);
                success = true;
                price = _bestAsk;
            }
            else if (_limitPrice > _bestBid)
            {
                _logger.Trace(LogLevel.Debug, "InnerPriceBuyer. LimitPrice {0} > BestBid {1}. Will send an order...", _limitPrice, _bestBid);
                success = true;
                if (_isFirstTradingRound)
                {
                    double bestAskPlus = (1.0 + LambdaRelative) * _bestAsk + LambdaAbsolute;
                    _logger.Trace(LogLevel.Debug,
                       "InnerPriceBuyer. LimitPrice {0} > BestBid {3}. ...(FIRST ROUND) bestAskPlus = (1.0 + LambdaRelative) * _bestAsk + LambdaAbsolute = (1.0 + {2})*{1}+{4}={5} ...",
                       _limitPrice, _bestAsk, LambdaRelative, _bestBid, LambdaAbsolute, bestAskPlus);
                    price = _bestBid + (Math.Min(_limitPrice, bestAskPlus) - _bestBid) / Eta;
                    _logger.Trace(LogLevel.Debug,
                            "InnerPriceBuyer. LimitPrice {0} > BestBid {1}. ...(FIRST ROUND) price = _bestBid + (Math.Min(_limitPrice, bestAskPlus) - _bestBid) / Eta = {1}+(Min({0},{2})-{1})/{3}={4}",
                       _limitPrice, _bestBid, bestAskPlus, Eta, price);
                }
                else
                {
                    if (_bestAsk <= tau)
                    {
                        price = _bestAsk;
                        _logger.Trace(LogLevel.Debug, "InnerPriceBuyer. LimitPrice {0} > BestBid {1}. ...bestAsk {2} <= tau {3} ---> price = bestAsk = {2}",
                            _limitPrice, _bestBid, _bestAsk, tau);
                    }
                    else
                    {
                        price = _bestBid + (tau - _bestBid) / Eta;
                        _logger.Trace(LogLevel.Debug, "InnerPriceBuyer. LimitPrice {0} > BestBid {1}. ...bestAsk {2} > tau {3} ---> price = _bestBid + (tau - _bestBid) / Eta = {1} + ({3}-{1})/{4} = {5}",
                            _limitPrice, _bestBid, _bestAsk, tau, Eta, price);
                    }
                }
            }

            return price;
        }

        private double InnerPriceSeller(double tau, out bool success)
        {
            double price = 0.0;
            success = false;

            if (_limitPrice >= _bestAsk)
            {
                _logger.Trace(LogLevel.Debug, "InnerPriceSeller. LimitPrice {0} >= BestAsk {1}. Will send an order @ limitPrice {0}", _limitPrice, _bestAsk);
                success = true;
                price = _limitPrice;
            }
            else if (_limitPrice <= _bestBid && ((_bestAsk - _bestBid) <= MaxSpread))
            {
                _logger.Trace(LogLevel.Debug, "InnerPriceSeller. LimitPrice {0} <= BestBid {1} && spread<=MaxSpread. Will send an order @ BestBid {1}", _limitPrice, _bestBid);
                success = true;
                price = _bestBid;
            }
            else if (_limitPrice < _bestAsk)
            {
                _logger.Trace(LogLevel.Debug, "InnerPriceSeller. LimitPrice {0} < BestAsk {1}. Will send an order...", _limitPrice, _bestAsk);
                success = true;
                if (_isFirstTradingRound)
                {
                    double bestBidMinus = (1.0 - LambdaRelative) * _bestBid - LambdaAbsolute;
                    _logger.Trace(LogLevel.Debug,
                        "InnerPriceSeller. LimitPrice {0} < BestAsk {1}. ...(FIRST ROUND) bestBidMinus = (1.0 - LambdaRelative) * _bestBid - LambdaAbsolute = (1.0 - {2})*{3}-{4}={5} ...",
                        _limitPrice, _bestAsk, LambdaRelative, _bestBid, LambdaAbsolute, bestBidMinus);
                    price = _bestAsk - (_bestAsk - Math.Max(_limitPrice, bestBidMinus)) / Eta;
                    _logger.Trace(LogLevel.Debug,
                        "InnerPriceSeller. LimitPrice {0} < BestAsk {1}. ...(FIRST ROUND) price = _bestAsk - (_bestAsk - Math.Max(_limitPrice, bestBidMinus)) / Eta = {1}-({1}-Max({0},{2}))/{3}={4}",
                        _limitPrice, _bestAsk, bestBidMinus, Eta, price);
                    _isFirstTradingRound = false;
                }
                else
                {
                    if (_bestBid >= tau)
                    {
                        price = _bestBid;
                        _logger.Trace(LogLevel.Debug, "InnerPriceSeller. LimitPrice {0} < BestAsk {1}. ...bestBid {2} >= tau {3} ---> price = bestBid = {2}",
                            _limitPrice, _bestAsk, _bestBid, tau);
                    }
                    else
                    {
                        price = _bestAsk - (_bestAsk - tau) / Eta;
                        _logger.Trace(LogLevel.Debug, "InnerPriceSeller. LimitPrice {0} < BestAsk {1}. ...bestBid {2} < tau {3} ---> price = _bestAsk - (_bestAsk - tau) / Eta = ({1} - ({1}-{3})/{4} = {5}",
                            _limitPrice, _bestAsk, _bestBid, tau, Eta, price);
                    }
                }
            }

            return price;
        }
    }
}
