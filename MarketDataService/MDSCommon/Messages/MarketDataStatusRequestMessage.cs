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

namespace OPEX.MDS.Common
{
    /// <summary>
    /// Represents a MarketDataMessage that carries
    /// information to request the status of the current
    /// exchange session.
    /// </summary>
    [Serializable]
    public class MarketDataStatusRequestMessage : MarketDataMessage
    {
        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.MDS.Common.MarketDataStatusRequestMessage.
        /// </summary>
        /// <param name="instrument">The underlying instrument.</param>
        /// <param name="dataSource">The data source associated to this MarketDataStatusRequestMessage.</param>
        public MarketDataStatusRequestMessage(string instrument, string dataSource)
            : base(MarketDataMessageType.Status, instrument, dataSource)
        {
        }

        /// <summary>
        /// Creates a MarketDataSessionMessage that responds to
        /// this MarketDataStatusRequestMessage.
        /// </summary>
        /// <param name="currentState">The current exchange session state.</param>
        /// <param name="currentStartTime">The current exchange session start time.</param>
        /// <param name="currentEndTime">The current exchange session end time.</param>
        /// <param name="dataSource">The data source associated to this MarketDataStatusRequestMessage.</param>
        /// <returns>A MarketDataSessionMessage that responds to
        /// this MarketDataStatusRequestMessage.</returns>
        public MarketDataSessionMessage CreateStatusResponseMessage(SessionState currentState, DateTime currentStartTime, DateTime currentEndTime, string dataSource)
        {
            return new MarketDataSessionMessage(currentState, currentStartTime, currentEndTime, dataSource, _origin);
        }
    }
}
