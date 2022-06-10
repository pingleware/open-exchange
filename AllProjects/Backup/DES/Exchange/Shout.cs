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

namespace OPEX.DES.Exchange
{
    public class Shout
    {
        private readonly bool _accepted;
        private readonly double _price;
        private readonly double _tradePrice;
        private readonly string _instrument;
        private readonly string _user;
        private readonly OrderSide _side;
        private readonly TimeStamp _timeStamp;
        
        public Shout(bool accepted, double price, double tradePrice, string instrument, OrderSide side, string user)
        {
            _accepted = accepted;
            _price = price;
            _tradePrice = tradePrice;
            _instrument = instrument;
            _side = side;
            _user = user;
            _timeStamp = TimeManager.CurrentTimeStamp;
        }

        public bool Accepted { get { return _accepted; } }
        public double Price { get { return _price; } }
        public double TradePrice { get { return _tradePrice; } }
        public string Instrument { get { return _instrument; } }
        public string User { get { return _user; } }
        public OrderSide Side { get { return _side; } }
        public TimeStamp TimeStamp { get { return _timeStamp; } }
        public long ID { get { return _timeStamp.ToLong(); } }

        public override string ToString()
        {
            return string.Format("TimeStamp {0} User {1} Accepted {2} Side {3} RIC {4} Price {5} TrdPrc {6}",
                _timeStamp.ToString(), _user, _accepted, _side, _instrument, _price, _tradePrice);
        }
    }
}
