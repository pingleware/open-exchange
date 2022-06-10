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

namespace OPEX.DES.Simulation
{
    public class Assignment
    {
        private string _applicationName;
        private int _id;
        private string _ric;
        private string _currency;
        private OrderSide _side;
        private int _quantity;
        private double _price;
        private int _phase;

        public Assignment(string applicationName, int phase, int id, string ric, string currency, OrderSide side, int quantity, double price)
        {
            _applicationName = applicationName;
            _phase = phase;
            _id = id;
            _ric = ric;
            _currency = currency;
            _side = side;
            _quantity = quantity;
            _price = price;
        }

        public string ApplicationName
        {
            get { return _applicationName; }
        }

        public int Phase
        {
            get { return _phase; }
        }

        public int Id
        {
            get { return _id; }
        }

        public string Ric
        {
            get { return _ric; }
        }

        public string Currency
        {
            get { return _currency; }
        }

        public OrderSide Side
        {
            get { return _side; }
        }

        public int Quantity
        {
            get { return _quantity; }
        }

        public double Price
        {
            get { return _price; }
        }

        public override string ToString()
        {
            return string.Format("ApplicationName {0} Phase {1} Id {2} Ric {3} Currency {4} Side {5} Quantity {6} Price {7}",
                _applicationName, _phase, _id, _ric, _currency, _side.ToString(), _quantity.ToString("N0"), _price.ToString("F4"));
        }
    }
}
