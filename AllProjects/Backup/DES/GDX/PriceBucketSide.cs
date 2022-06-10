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
using OPEX.DES.Exchange;

namespace OPEX.DES.GDX
{
    public class PriceBucketSide : PriceContainer, IEnumerator, IEnumerable
    {
        private readonly TimeSpan GracePeriod;

        private OrderSide _side;        
        private SortedDictionary<long, Shout> _shouts;
        private IEnumerator _shoutsEnumerator;        

        public PriceBucketSide(double price, OrderSide side, double gracePeriodSecs)
            : base(price)
        {            
            _side = side;
            _shouts = new SortedDictionary<long, Shout>();
            GracePeriod = TimeSpan.FromSeconds(gracePeriodSecs);
        }

        public OrderSide Side { get { return _side; } }
        public int Count { get { return InnerCount(true, true); } }
        public int CountAccepted { get { return InnerCount(true, false); } }
        public int CountRejected { get { return InnerCount(false, true); } }        
        
        public override void Add(Shout s)
        {
            if (s.Side != _side)
            {
                throw new ArgumentException("PriceBucketSide.Add. Invalid side");
            }
            else if (s.Price != _price)
            {
                throw new ArgumentException("PriceBucketSide.Add. Invalid price");
            }

            _shouts.Add(s.ID, s);
        }

        public override void Truncate(long id)
        {
            SortedDictionary<long, Shout> truncatedShouts = new SortedDictionary<long, Shout>();

            foreach (Shout s in _shouts.Values)
            {
                if (s.ID > id)
                {
                    truncatedShouts.Add(s.ID, s);
                }
            }

            _shouts = truncatedShouts;
        }

        public string Dump()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0} [", _price);

            foreach (Shout s in _shouts.Values)
            {               
                string accepted = s.Accepted ? "A" : "R";
                string buysell = s.Side == OrderSide.Buy ? "B" : "S";
                sb.AppendFormat("({0} {1} {2} {3}) ", s.TimeStamp.ToString(), accepted, buysell, s.Price);          
            }

            sb.Append("]");

            return sb.ToString();
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            Reset();
            return this;
        }

        #endregion

        #region IEnumerator Members

        public object Current
        {
            get { return _shoutsEnumerator.Current; }
        }

        public bool MoveNext()
        {            
            while (_shoutsEnumerator.MoveNext())
            {
                return true;
            }

            Reset();
            return false;
        }

        public void Reset()
        {
            _shoutsEnumerator = _shouts.Values.GetEnumerator();
            _shoutsEnumerator.Reset();
        }

        #endregion
        
        private bool HasGracePeriodExpired(Shout s)
        {
            return TimeManager.CurrentTimeStamp.Subtract(s.TimeStamp) > GracePeriod.Seconds;
        }

        private int InnerCount(bool countAccepted, bool countRejected)
        {
            int accepted = 0;
            int rejected = 0;
            int res = 0;

            foreach (Shout s in this)
            {
                if (s.Accepted && !HasGracePeriodExpired(s))
                {
                    accepted++;
                }
                else
                {
                    rejected++;
                }
            }
            if (countAccepted)
            {
                res = accepted;
            }
            if (countRejected)
            {
                res += rejected;
            }
            return res;
        }
    }
}
