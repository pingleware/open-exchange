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

using OPEX.Common;
using OPEX.OM.Common;

namespace OPEX.TDS.Common
{
    /// <summary>
    /// Collects methods to create TradeDataMessage-s.
    /// </summary>
    public class TradeDataMessageFactory
    {
        private static IUIDGenerator _idGenerator;

        static TradeDataMessageFactory()
        {
            _idGenerator = new IncrementalIDGenerator();
        }

        /// <summary>
        /// Creates a new TradeDataMessage.
        /// </summary>       
        /// <param name="orderID">The order ID of the underlying order.</param>
        /// <param name="quantity">The quantity traded.</param>
        /// <param name="price">The price of the fill.</param>
        /// <param name="limitPrice">The limit price of the underlying order.</param>
        /// <param name="user">The user name of the owner of the underlying order.</param>
        /// <param name="counterparty">The counterparty of the fill.</param>
        /// <param name="instrument">The RIC of the security traded.</param>
        /// <param name="side">The side of the underlying order.</param>
        /// <returns></returns>
        public static TradeDataMessage CreateMessage(long orderID, int quantity, double price, double limitPrice, string user, string counterparty, string instrument, OrderSide side)
        {
            return new TradeDataMessage(_idGenerator.NextID(), orderID, quantity, price, limitPrice, user, counterparty, instrument, side);
        }
    }
}
