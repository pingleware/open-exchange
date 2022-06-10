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

using OPEX.DES.OrderManager;
using OPEX.Common;

namespace OPEX.DES.Exchange
{
    public class AggregatedDepth
    {
        private readonly object _root = new object();
        private readonly Logger _logger;

        private string _instrumentName;
        private Hashtable _buyQuotes;
        private Hashtable _sellQuotes;

        public AggregatedDepth(string instrumentName)
        {
            if (instrumentName == null)
            {
                throw new ArgumentException("Null instrument!");
            }

            _logger = new Logger(string.Format("AggregatedDepth({0})", instrumentName));

            _instrumentName = instrumentName;

            _logger.Trace(LogLevel.Debug, "Creating AggregatedDepth.");

            _buyQuotes = Hashtable.Synchronized(new Hashtable());
            _sellQuotes = Hashtable.Synchronized(new Hashtable());

            _logger.Trace(LogLevel.Debug, "AggregatedDepth created.");
        }

        public AggregatedDepthSnapshot Snapshot
        {
            get
            {
                AggregatedDepthSnapshot snapshot = new AggregatedDepthSnapshot(_instrumentName);
                lock (_root)
                {
                    foreach (AggregatedQuote quote in _buyQuotes.Values)
                    {
                        snapshot.Buy.Add(quote, true);
                    }
                    foreach (AggregatedQuote quote in _sellQuotes.Values)
                    {
                        snapshot.Sell.Add(quote, true);
                    }
                }
                return snapshot;
            }
        }

        public void Add(AggregatedQuote quote)
        {
            string key = quote.Price.ToString();
            Hashtable side = this[quote.Side];

            lock (_root)
            {
                if (!side.ContainsKey(key))
                {
                    side[key] = quote;
                }
                else
                {
                    AggregatedQuote q = side[key] as AggregatedQuote;
                    AggregatedQuote newQuote = new AggregatedQuote(q.Side, q.Quantity + quote.Quantity, q.Price);
                    side[key] = newQuote;
                }
            }
        }

        public void Subtract(AggregatedQuote quote)
        {
            string key = quote.Price.ToString();
            Hashtable side = this[quote.Side];

            lock (_root)
            {
                if (!side.ContainsKey(key))
                {
                    return;
                }

                AggregatedQuote q = side[key] as AggregatedQuote;

                if (q.Quantity < quote.Quantity)
                {
                    return;
                }
                else if (q.Quantity == quote.Quantity)
                {
                    side.Remove(key);
                }
                else
                {
                    side[key] = new AggregatedQuote(q.Side, q.Quantity - quote.Quantity, q.Price);
                }
            }
        }

        public void Clear()
        {
            lock (_root)
            {
                _buyQuotes.Clear();
                _sellQuotes.Clear();
            }
        }

        private Hashtable this[OrderSide side] { get { return (side == OrderSide.Buy) ? _buyQuotes : _sellQuotes; } }
    }
}
