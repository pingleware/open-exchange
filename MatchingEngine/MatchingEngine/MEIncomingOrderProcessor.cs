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
using OPEX.StaticData;

namespace OPEX.ME
{
    /// <summary>
    /// Validates incoming orders.
    /// </summary>
    public class MEIncomingOrderProcessor : IncomingOrderProcessor
    {
        /// <summary>
        /// Validates a NewOrderRequest.
        /// </summary>
        /// <param name="order">The Order to validate.</param>
        /// <param name="errorMessage">The errorMessage to write to.</param>
        /// <returns>True if validation was successful. False otherwise.</returns>
        public override bool ValidateNewOrderRequest(Order order, ref string errorMessage)
        {
            if (!base.ValidateNewOrderRequest(order, ref errorMessage))
            {
                return false;
            }

            string m = null;
            bool res = false;
            Instrument instrument = StaticDataManager.Instance.InstrumentStaticData[order.Instrument];

            if (order.Quantity < instrument.MinQty)
            {
                m = string.Format("Invalid quantity. Minimum quantity = {0}", instrument.MinQty);
            }
            else if (order.Quantity > instrument.MaxQty)
            {
                m = string.Format("Invalid quantity. Maximum quantity = {0}", instrument.MaxQty);
            }
            else
            {
                res = true;
            }

            if (!res)
            {
                errorMessage = m;
            }
            return res;
        }
    }
}
