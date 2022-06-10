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
using System.Text;

using OPEX.Common;
using OPEX.OM.Common;

namespace OPEX.ME
{
    /// <summary>
    /// Exposes a method to compare two IOrders.
    /// </summary>
    public class PriceTimePriority : IComparer
    {
        /// <summary>
        /// Compares two IOrders, looking at their price first, and then time.
        /// Buy (Sell) orders with a higher (lower) price come first.
        /// In case of equal price, earlier orders come first.
        /// </summary>
        /// <param name="orderX">The first IOrder to compare.</param>
        /// <param name="orderY">The second IOrder to compare.</param>
        /// <param name="sortingOrder">Positive for Sell orders, negative otherwise.</param>
        /// <returns>-1 if orderX was sent before order Y. 1 if orderX was sent after orderY. 0 otherwise.</returns>
        public int CompareOrder(IOrder orderX, IOrder orderY, int sortingOrder)
        {
            int priceComp = orderX.Price.CompareTo(orderY.Price);

            if (priceComp == 0)
            {
                int timeComp = orderX.TimeStamp.CompareTo(orderY.TimeStamp);
                return timeComp;
            }

            return priceComp * sortingOrder;
        }

        #region IComparer Members

        public int Compare(object x, object y)
        {
            IOrder orderX = x as IOrder;
            IOrder orderY = y as IOrder;

            int sortingOrder = 1;

            if (orderX.Side == OrderSide.Buy)
            {
                sortingOrder = -1;
            }

            return CompareOrder(orderX, orderY, sortingOrder);
        }

        #endregion
    }
}
