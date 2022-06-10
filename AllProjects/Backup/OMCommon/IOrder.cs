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

using OPEX.Common;

namespace OPEX.OM.Common
{    
    /// <summary>
    /// Defines a generic order.
    /// </summary>
    public interface IOrder
    {
        /// <summary>
        /// Gets the client-specific unique order ID.
        /// </summary>
        long ClientOrderID { get; }

        /// <summary>
        /// Gets the system-wide order ID.
        /// </summary>
        long OrderID { get; }

        /// <summary>
        /// Gets the system-wide order ID of the parent order (if any).
        /// </summary>
        long ParentOrderID { get; }

        /// <summary>
        /// Gets the origin of this IOrder.
        /// </summary>
        string Origin { get; }

        /// <summary>
        /// Gets and sets the destination of this IOrder.
        /// </summary>
        string Destination { get; set; }

        /// <summary>
        /// Gets the OrderStatus.
        /// </summary>
        OrderStatus Status { get; }

        /// <summary>
        /// Gets and sets the RIC.
        /// </summary>
        string Instrument { get; set; }

        /// <summary>
        /// Gets and sets the OrderSide.
        /// </summary>
        OrderSide Side { get; set; }

        /// <summary>
        /// Gets and sets the currency.
        /// </summary>
        string Currency { get; set; }

        /// <summary>
        /// Gets and sets the OrderType.
        /// </summary>
        OrderType Type { get; set; }
        
        /// <summary>
        /// Gets and sets the quantity.
        /// </summary>
        int Quantity { get; set; }

        /// <summary>
        /// Gets and sets the quantity filled.
        /// </summary>
        int QuantityFilled { get; set; }

        /// <summary>
        /// Gets and sets the last quantity filled.
        /// </summary>
        int LastQuantityFilled { get; set; }

        /// <summary>
        /// Gets and sets the limit price.
        /// </summary>
        double LimitPrice { get; set; }

        /// <summary>
        /// Gets and sets the price.
        /// </summary>
        double Price { get; set; }

        /// <summary>
        /// Gets and sets the last price at which the IOrder was filled.
        /// </summary>
        double LastPriceFilled { get; set; }

        /// <summary>
        /// Gets and sets the average price at which the IOrder was filled.
        /// </summary>
        double AveragePriceFilled { get; set; }

        /// <summary>
        /// Gets the timestamp of the IOrder.
        /// </summary>
        DateTime TimeStamp { get; }

        /// <summary>
        /// Gets the remaining quantity.
        /// </summary>
        int QuantityRemaining { get; }

        /// <summary>
        /// Gets the turnover.
        /// </summary>
        double Turnover { get; }

        /// <summary>
        /// Gets the message associated to this order.
        /// </summary>
        string Message { get; }
    }
}
