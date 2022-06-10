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

namespace OPEX.MDS.Common
{
    /// <summary>
    /// Represents an aggregated quote, i.e.
    /// a triplet constituted by a price, a quantity and a side.
    /// </summary>
    [Serializable]
    public class AggregatedQuote
    {
        private OrderSide _side;
        private int _quantity;
        private double _price;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.MDS.Common.AggregatedQuote.
        /// </summary>
        /// <param name="side">The side of the AggregatedQuote.</param>
        /// <param name="quantity">The quantity of the AggregatedQuote.</param>
        /// <param name="price">The price of the AggregatedQuote.</param>
        public AggregatedQuote(OrderSide side, int quantity, double price)
        {
            _side = side;
            _quantity = quantity;
            _price = price;
        }

        /// <summary>
        /// Gets the side of the AggregatedQuote.
        /// </summary>
        public OrderSide Side { get { return _side; } }


        /// <summary>
        /// Gets the quantity of the AggregatedQuote.
        /// </summary>
        public int Quantity { get { return _quantity; } }


        /// <summary>
        /// Gets the price of the AggregatedQuote.
        /// </summary>
        public double Price { get { return _price; } }

        /// <summary>
        /// Returns the string representation of the AggregatedQuote.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} {1:N0} @ {2:F4}", _side, _quantity, _price);
        }
    }
}
