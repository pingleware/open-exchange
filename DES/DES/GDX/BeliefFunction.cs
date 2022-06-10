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
using OPEX.DES.OrderManager;
using OPEX.DES.Exchange;

namespace OPEX.DES.GDX
{
    public class BeliefFunction
    {
        protected readonly Logger _logger;
        protected readonly ShoutHistory _history;
        protected readonly OrderSide _side;
        protected readonly double PMIN;
        protected readonly double PMAX;

        protected TabulatedFunction _function;
        protected BuildFunctionDelegate BuildFunction;

        public BeliefFunction(ShoutHistory history, OrderSide side, double pmin, double pmax, string name) 
        {
            _logger = new Logger(string.Format("BeliefFunction({0})", name));
            _history = history;
            _side = side;
            _function = new TabulatedFunction();
            PMIN = pmin;
            PMAX = pmax;
            if (side == OrderSide.Sell)
            {
                BuildFunction = BuildSellersFunction;
            }
            else
            {
                BuildFunction = BuildBuyersFunction;
            }
        }

        public TabulatedFunction Function { get { return _function; } }
        public double OA { get { return _history.BestAsk; } }
        public double OB { get { return _history.BestBid; } }
        
        public virtual void Build()
        {            
            _function = new TabulatedFunction();

            _logger.Trace(LogLevel.Debug, "Build. History: {0}", _history.ToString());
            _logger.Trace(LogLevel.Debug, "Build. History DUMP: {0}", _history.Dump());
            BuildFunction();
            _logger.Trace(LogLevel.Debug, "Build. Function built: {0}", _function.ToString());
            RemoveZeroes();
            _logger.Trace(LogLevel.Debug, "Build. Function with zeroes removed: {0}", _function.ToString());
        }                      

        protected delegate void BuildFunctionDelegate();

        protected void BuildSellersFunction()
        {
            _function.Add(PMIN, 1.0);
            _function.Add(PMAX, 0.0);

            foreach (double a in _history.Domain)
            {
                double TAG = _history.FindExp(a, ShoutFilter.Accepted, OrderSide.Sell, true);
                double BG = _history.FindExp(a, ShoutFilter.All, OrderSide.Buy, true);
                double RAL = _history.FindExp(a, ShoutFilter.Rejected, OrderSide.Sell, false);

                _logger.Trace(LogLevel.Debug, "BuildSellersFunction. price {3} TAG {0} BG {1} RAL {2}", TAG, BG, RAL, a);

                double den = TAG + BG + RAL;
                if (den != 0)
                {
                    double p = (TAG + BG) / den;
                    _function.Add(a, p);
                    _logger.Trace(LogLevel.Debug, "BuildSellersFunction. Point [{0}, {1}] added to the function", a, p);
                    _logger.Trace(LogLevel.Debug, "BuildSellersFunction. Function so far: {0}", _function.ToString());
                }
            }
        }

        protected void BuildBuyersFunction()
        {
            _function.Add(PMIN, 0.0);
            _function.Add(PMAX, 1.0);            

            foreach (double b in _history.Domain)
            {
                double TBL = _history.FindExp(b, ShoutFilter.Accepted, OrderSide.Buy, false);
                double AL = _history.FindExp(b, ShoutFilter.All, OrderSide.Sell, false);
                double RBG = _history.FindExp(b, ShoutFilter.Rejected, OrderSide.Buy, true);

                _logger.Trace(LogLevel.Debug, "BuildBuyersFunction. price {3} TBL {0} AL {1} RBG {2}", TBL, AL, RBG, b);

                double den = TBL + AL + RBG;
                if (den != 0)
                {
                    double q = (TBL + AL) / den;
                    _function.Add(b, q);
                    _logger.Trace(LogLevel.Debug, "BuildBuyersFunction. Point [{0}, {1}] added to the function", b, q);
                    _logger.Trace(LogLevel.Debug, "BuildBuyersFunction. Function so far: {0}", _function.ToString());
                }
            }
        }

        protected void RemoveZeroes()
        {
            if (_side == OrderSide.Sell)
            {
                _function.RemoveTrailingZeroes();                
            }
            else
            {
                _function.RemoveLeadingZeroes();
            }
        }      
    }
}
