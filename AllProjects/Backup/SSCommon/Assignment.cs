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

namespace OPEX.SupplyService.Common
{
    [Serializable]
    public class Assignment
    {
        private readonly string _applicationName;
        private readonly int _id;
        private readonly string _ric;
        private readonly string _currency;
        private readonly OrderSide _side;
        private readonly int _quantity;
        private readonly double _price;
        private readonly int _phase;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.SupplyService.Common.Assignment.
        /// </summary>
        /// <param name="applicationName">The name of the application that owns this assignment.</param>
        /// <param name="phase">The trading phase of this assignment.</param>
        /// <param name="id">The assignment ID.</param>
        /// <param name="ric">The RIC of the security.</param>
        /// <param name="currency">The currency of the security.</param>
        /// <param name="side">Indicates whether it's an assignment to Buy or to Sell.</param>
        /// <param name="quantity">The quantity to trade.</param>
        /// <param name="price">The limit price of the assignment: maximum buy price or minimum sell price.</param>
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

        /// <summary>
        /// Gets the name of the application that owns this assignment.
        /// </summary>
        public string ApplicationName
        {
            get { return _applicationName; }
        }

        /// <summary>
        /// Gets the trading phase of this assignment.
        /// </summary>
        public int Phase
        {
            get { return _phase; }
        }

        /// <summary>
        /// Gets the assignment ID.
        /// </summary>
        public int Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets the RIC of the security.
        /// </summary>
        public string Ric
        {
            get { return _ric; }
        }

        /// <summary>
        /// Gets the currency of the security.
        /// </summary>
        public string Currency
        {
            get { return _currency; }
        }

        /// <summary>
        /// Indicates whether it's an assignment to Buy or to Sell.
        /// </summary>
        public OrderSide Side
        {
            get { return _side; }
        }

        /// <summary>
        /// Gets the quantity to trade.
        /// </summary>
        public int Quantity
        {
            get { return _quantity; }
        }

        /// <summary>
        /// Gets the limit price of the assignment:
        /// maximum buy price or minimum sell price.
        /// </summary>
        public double Price
        {
            get { return _price; }
        }

        /// <summary>
        /// Returns the string representation of this Assignment.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ApplicationName {0} Phase {1} Id {2} Ric {3} Currency {4} Side {5} Quantity {6} Price {7}",
                _applicationName, _phase, _id, _ric, _currency, _side.ToString(), _quantity.ToString("N0"), _price.ToString("F4"));
        }
    }
}
