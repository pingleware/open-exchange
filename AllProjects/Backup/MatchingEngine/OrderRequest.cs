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

namespace OPEX.ME
{
    /// <summary>
    /// Specifies the type of the OrderRequest.
    /// </summary>
    public enum OrderRequestType
    {
        /// <summary>
        /// A request to create a new order.
        /// </summary>
        NewOrder,

        /// <summary>
        /// A request to cancel an existing order.
        /// </summary>
        Cancellation,

        /// <summary>
        /// A request to amend an existing order.
        /// </summary>
        Amendment,

        /// <summary>
        /// A request to pull an existing order from the exchange.
        /// </summary>
        CancelByExchange
    }

    /// <summary>
    /// Represents an order request.
    /// </summary>
    public class OrderRequest
    {
        private OrderRequestType _requestType;
        private IncomingOrder _incomingOrder;
        private Order _order;
        private string _message;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.ME.OrderRequest.
        /// </summary>
        /// <param name="requestType">The type of the OrderRequest.</param>
        /// <param name="incomingOrder">The IncomingOrder to associate this OrderRequest to.</param>
        /// <param name="order">The Order to associate this OrderRequest to.</param>
        /// <param name="message">The message of this OrderRequest.</param>
        public OrderRequest(OrderRequestType requestType, IncomingOrder incomingOrder, Order order, string message)
        {
            _requestType = requestType;
            _incomingOrder = incomingOrder;
            _order = order;
            _message = message;
        }

        /// <summary>
        /// Gets the type of this OrderRequest.
        /// </summary>
        public OrderRequestType Type { get { return _requestType; } }

        /// <summary>
        /// Gets the IncomingOrder associated to this OrderRequest.
        /// </summary>
        public IncomingOrder IncomingOrder { get { return _incomingOrder; } }

        /// <summary>
        /// Gets the Order associated to this OrderRequest.
        /// </summary>
        public Order Order { get { return _order; } }
        
        /// <summary>
        /// Gets the message associated to this OrderRequest.
        /// </summary>
        public string Message { get { return _message; } }
    }
}
