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

using OPEX.Common;
using OPEX.OM.Common;

namespace OPEX.ShoutService
{
    /// <summary>
    /// Collects methods to create Shouts.
    /// </summary>
    public class ShoutFactory
    {
        static readonly IncrementalIDGenerator IDGenerator;

        static ShoutFactory()
        {
            IDGenerator = new IncrementalIDGenerator();
        }

        /// <summary>
        /// Creates a new shout.
        /// </summary>
        /// <param name="accepted">Identifies whether this Shouts matches some previously sent order in the market.</param>
        /// <param name="price">The price of this Shout.</param>
        /// <param name="instrument">The instrument to buy or sell.</param>
        /// <param name="side">The side of the trade, i.e. buy or sell.</param>
        /// <param name="user">The user id of the market participant who wants to buy or sell.</param>
        /// <returns></returns>
        public static Shout CreateShout(bool accepted, double price, string instrument, OrderSide side, string user)
        {
            return new Shout(IDGenerator.NextID(), accepted, price, instrument, side, user);
        }
    }
}
