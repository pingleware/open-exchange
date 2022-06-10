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
    /// Represents a MarketDataMessage that requests information
    /// about the latest version of an orderbook.
    /// </summary>
    [Serializable]
    public class MarketDataSnapshotRequestMessage : MarketDataMessage
    {
        /// <summary>
        /// Initialises a new instance of the lcass
        /// OPEX.MDS.Common.MarketDataSnapshotRequestMessage.
        /// </summary>
        /// <param name="instrument">The instrument underlying this MarketDataSnapshotMessage.</param>
        /// <param name="dataSource">The data source underlying this MarketDataSnapshotMessage.</param>
        public MarketDataSnapshotRequestMessage(string dataSource, string instrument)
            : base(MarketDataMessageType.Status, instrument, dataSource)
        {
        }

        /// <summary>
        /// Create a MarketDataSnapshotResponseMessage to this
        /// MarketDataSnapshotRequestMessage.
        /// </summary>
        /// <param name="snapshot">The AggregatedDepthSnapshot to attach
        /// as a response to this MarketDataSnapshotRequestMessage.</param>
        /// <returns>A MarketDataSnapshotResponseMessage that responds to this
        /// MarketDataSnapshotRequestMessage.</returns>
        public MarketDataSnapshotResponseMessage CreateResponseMessage(AggregatedDepthSnapshot snapshot)
        {
            return new MarketDataSnapshotResponseMessage(_instrument, _dataSource, _origin, snapshot);
        }
    }
}
