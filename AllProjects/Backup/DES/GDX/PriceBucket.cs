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

using OPEX.DES.OrderManager;
using OPEX.DES.Exchange;

namespace OPEX.DES.GDX
{
    public class PriceBucket : PriceContainer
    {
        private readonly PriceBucketSide _sell;
        private readonly PriceBucketSide _buy;        

        public PriceBucket(double price, double gracePeriodSecs)
            : base(price)
        {            
            _sell = new PriceBucketSide(price, OrderSide.Sell, gracePeriodSecs);
            _buy = new PriceBucketSide(price, OrderSide.Buy, gracePeriodSecs);            
        }

        public PriceBucketSide this[OrderSide side]
        {
            get
            {
                return (side == OrderSide.Buy) ? _buy : _sell;
            }
        }

        public int Count
        {
            get
            {
                return _buy.Count + _sell.Count;
            }
        }

        public override void Add(Shout s)
        {
            this[s.Side].Add(s);
        }

        public override void Truncate(long id)
        {
            foreach (PriceBucketSide pbs in new PriceBucketSide[] { _buy, _sell })
            {
                pbs.Truncate(id);
            }                
        }

        public string Dump()
        {
            return string.Concat(_buy.Dump(), " ", _sell.Dump());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0} {{", _price);
                        
            foreach (PriceBucketSide pbs in new PriceBucketSide[] { _buy, _sell })
            {
                sb.Append("{ ");
                foreach (Shout s in pbs)
                {
                    string accepted = s.Accepted ? "A" : "R";
                    string buysell = s.Side == OrderSide.Buy ? "B" : "S";
                    sb.AppendFormat("({0} {1} {2} {3}) ", s.TimeStamp.ToString(), accepted, buysell, s.Price);
                }
                sb.Append("} ");
            }

            sb.Append("}");

            return sb.ToString();
        }
    }
}
