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

namespace OPEX.DES.AA
{
    class EquilibriumEstimator
    {
        private readonly double InitialPriceGuessPercentageAroundMid = 0.25;

        private readonly int _windowSize;
        private readonly double _rho;
        private readonly Cache<double> _prices;
        private readonly Logger _logger;
        private readonly DESAAAgent _agent;
        private double _estimatedPrice;

        public EquilibriumEstimator(DESAAAgent agent)
        {
            _logger = new Logger(string.Format("EquilibriumEstimator({0})", agent.Name));
            _agent = agent;
            _windowSize = (int)agent.Parameters.GetValue("WindowSize", typeof(int));
            _rho = (double)agent.Parameters.GetValue("Rho", typeof(double));
            _prices = new Cache<double>(_windowSize);
            _estimatedPrice = double.NaN;
            _logger.Trace(LogLevel.Info, "C'tor. WindowSize {0} Rho {1} estimatedPrice {2}", _windowSize, _rho, _estimatedPrice);
        }

        public double EstimatedPrice { get { return _estimatedPrice; } }
        public Cache<double> Prices { get { return _prices; } }

        public void AddNewTransactionPrice(double price)
        {
            _logger.Trace(LogLevel.Debug, "AddNewTransactionPrice. New price added: {0}", price);

            _prices.Add(price);

            double w = 1.0;
            double wSum = 0.0;
            double priceSum = 0.0;
            foreach (double p in _prices)
            {
                priceSum += p * w;
                wSum += w;
                w *= _rho;
            }
            _estimatedPrice = priceSum / wSum;

            _logger.Trace(LogLevel.Debug, "AddNewTransactionPrice. Prices in cache: {0}", _prices.ToString());
            _logger.Trace(LogLevel.Info, "AddNewTransactionPrice. UPDATED p* = {0}", _estimatedPrice);
        }

        public void Reset()
        {
            _prices.Clear();

            double a = _agent.CurrentInstrument.MinPrice;
            double b = _agent.CurrentInstrument.MaxPrice;
            double pMin = a + 0.5 * (b - a) * (1.0 - InitialPriceGuessPercentageAroundMid);
            Random r = new Random();
            _estimatedPrice = pMin + r.NextDouble() * (b - a) * InitialPriceGuessPercentageAroundMid;

            int totalTicks = (int)(_estimatedPrice / _agent.CurrentInstrument.PriceTick);
            _estimatedPrice = (double)totalTicks * _agent.CurrentInstrument.PriceTick;

            _logger.Trace(LogLevel.Info,
                "Reset. estimatedPrice set to {4}. minPrice {0} maxPrice {1} pMin {2} guess% {3}",
                a, b, pMin, InitialPriceGuessPercentageAroundMid, _estimatedPrice);
        }
    }
}
