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

using OPEX.OM.Common;

namespace OPEX.DWEAS
{
    public class ScheduleEntry
    {
        private readonly int _step;
        private readonly double _waitTime;
        private readonly string _userName;
        private readonly double _price;
        private readonly OrderSide _side;
        private readonly string _ric;
        private readonly string _ccy;
        private readonly int _qty;

        public ScheduleEntry(int step, double waitTime, string userName, double price, OrderSide side, string ric, string ccy, int qty)
        {
            _step = step;
            _waitTime = waitTime;
            _userName = userName;
            _price = price;
            _side = side;
            _ric = ric;
            _ccy = ccy;
            _qty = qty;
        }

        public int Step { get { return _step; } }
        public double Time { get { return _waitTime; } }
        public string UserName { get { return _userName; } }
        public double Price { get { return _price; } }
        public OrderSide Side { get { return _side; } }
        public string RIC { get { return _ric; } }
        public string CCY { get { return _ccy; } }
        public int Qty { get { return _qty; } }

        public override string ToString()
        {
            return string.Format("Step {7}. WaitTime {0}. UserName {1}. Price {2}. Side {3}. RIC {4}. CCY {5}. Qty {6}.",
                _waitTime, _userName, _price, _side.ToString(), _ric, _ccy, _qty, _step);
        }
    }
}
