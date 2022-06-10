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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OPEX.MDS.Common;
using OPEX.Common;

namespace OPEX.TestMDSClient
{
    public class MarketDataCache
    {
        private static MarketDataCache _theInstance;

        private Hashtable _snapshots;

        public static MarketDataCache Instance
        {
            get
            {
                if (_theInstance == null)
                {
                    _theInstance = new MarketDataCache();
                }

                return _theInstance;
            }
        }

        private MarketDataCache()
        {
            _snapshots = Hashtable.Synchronized(new Hashtable());
        }

        public void ReceiveUpdate(string instrument, AggregatedQuote quote)
        {
            if (!_snapshots.ContainsKey(instrument))
            {
                _snapshots[instrument] = new AggregatedDepthSnapshot(instrument);                
            }

            AggregatedDepthSnapshot s = _snapshots[instrument] as AggregatedDepthSnapshot;
            AggregatedDepthSide side = (quote.Side == OrderSide.Buy) ? s.Buy : s.Sell;

            if (side.ContainsPrice(quote.Price))
            {
                if (quote.Quantity == 0)
                {
                    side.RemovePrice(quote.Price);
                }
                else
                {
                    side.ReplacePrice(quote);
                }
            }
            else
            {
                side.Add(quote,false);
            }
        }

        public AggregatedDepthSnapshot GetSnapshot(string instrument)
        {
            AggregatedDepthSnapshot s = null;

            if (_snapshots.ContainsKey(instrument))
            {
                s = _snapshots[instrument] as AggregatedDepthSnapshot;
            }

            return s;
        }
    }
}
