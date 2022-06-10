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

using OPEX.OM.Common;
using OPEX.StaticData;

namespace OPEX.Agents.ZIC
{
    internal class ZICPricer
    {
        private static readonly Random _random = new Random();

        private readonly double _min;        
        private readonly double _step;
        private readonly int _n;
        private readonly ZICAgent _agent;

        public ZICPricer(ZICAgent agent, Order order)
        {
            _agent = agent;
            Instrument instrument = _agent.AgentStatus.CurrentInstrument;

            double max;
            if (order.Side == OrderSide.Buy)
            {
                _min = instrument.MinPrice;
                max = order.Price;
            }
            else
            {
                _min = order.Price;
                max = instrument.MaxPrice;
            }
            _step = instrument.PriceTick;
            _n = (int)((max - _min) / _step) + 1;
        }

        public double Price(out bool success)
        {
            double price = _min + _step * _random.Next(_n);
            double bestPrice;
            success = true;

            if (_agent.NYSESpreadImprovementClientSide && 
                !(_agent.IsPriceValid(price, out bestPrice)))
            {
                success = false;                
            }

            return price;
        }
    }
}