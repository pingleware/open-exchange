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

namespace OPEX.DES.Exchange
{    
    public class AggregatedDepthSide : IEnumerator, IEnumerable
    {
        [NonSerialized]
        private static readonly IComparer Comparer;

        [NonSerialized]
        private int _rowPos = -1;
        private ArrayList _quotes;
        private OrderSide _side;

        static AggregatedDepthSide()
        {
            Comparer = new PricePriority();
        }

        public AggregatedDepthSide(OrderSide side)
        {
            _side = side;
            _quotes = ArrayList.Synchronized(new ArrayList());
        }

        public int Depth { get { return _quotes.Count; } }
        public double Best
        {
            get
            {
                double best = 0.0;
                if (_quotes.Count > 0)
                {
                    best = (_quotes[0] as AggregatedQuote).Price;
                }
                return best;
            }
        }

        /// <summary>
        /// Returns the level-th quote in this side.
        /// </summary>
        /// <param name="level">0: best price. 1: second best price. ...</param>
        public AggregatedQuote this[int level]
        {
            get
            {
                return _quotes[level] as AggregatedQuote;
            }
        }

        public bool ContainsPrice(double price)
        {
            foreach (AggregatedQuote quote in _quotes)
            {
                if (quote.Price == price)
                {
                    return true;
                }
            }

            return false;
        }

        public void RemovePrice(double price)
        {
            for (int i = 0; i < _quotes.Count; ++i)
            {
                AggregatedQuote q = _quotes[i] as AggregatedQuote;

                if (q.Price == price)
                {
                    _quotes.RemoveAt(i);
                    break;
                }
            }
        }

        public void ReplacePrice(AggregatedQuote quote)
        {
            for (int i = 0; i < _quotes.Count; ++i)
            {
                AggregatedQuote q = _quotes[i] as AggregatedQuote;

                if (q.Price == quote.Price)
                {
                    _quotes[i] = quote;
                    break;
                }
            }
        }

        public void Add(AggregatedQuote quote, bool clone)
        {
            if (quote == null)
            {
                throw new NullReferenceException("Null quote!");
            }

            if (quote.Side != _side)
            {
                throw new ApplicationException("Wrong side!");
            }

            if (clone)
            {
                _quotes.Add(new AggregatedQuote(quote.Side, quote.Quantity, quote.Price));
            }
            else
            {
                _quotes.Add(quote);
            }
            _quotes.Sort(Comparer);
        }

        #region IEnumerator Members

        public object Current
        {
            get { return _quotes[_rowPos]; }
        }

        public bool MoveNext()
        {
            _rowPos++;

            if (_rowPos < _quotes.Count)
            {
                return true;
            }

            Reset();
            return false;
        }

        public void Reset()
        {
            _rowPos = -1;
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            Reset();
            return this;
        }

        #endregion

        private class PricePriority : IComparer
        {
            public int CompareOrder(AggregatedQuote quoteX, AggregatedQuote quoteY, int sortingOrder)
            {
                int priceComp = quoteX.Price.CompareTo(quoteY.Price);

                return priceComp * sortingOrder;
            }

            #region IComparer Members

            public int Compare(object x, object y)
            {
                AggregatedQuote quoteX = x as AggregatedQuote;
                AggregatedQuote quoteY = y as AggregatedQuote;

                int sortingOrder = 1;

                if (quoteX.Side == OrderSide.Buy)
                {
                    sortingOrder = -1;
                }

                return CompareOrder(quoteX, quoteY, sortingOrder);
            }

            #endregion
        }
    }
}
