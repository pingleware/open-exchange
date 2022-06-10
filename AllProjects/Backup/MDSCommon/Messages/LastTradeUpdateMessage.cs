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

namespace OPEX.MDS.Common
{
    /// <summary>
    /// Represents a MarketDataMessage that carries information
    /// regarding the last trade.
    /// </summary>
    [Serializable]
    public class LastTradeUpdateMessage : MarketDataMessage
    {        
        private readonly double _price;
        private readonly int _size;

        /// <summary>
        /// Gets the size of the last trade.
        /// </summary>
        public int Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets the price of the last trade.
        /// </summary>
        public double Price
        {
            get { return _price; }
        }

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.MDS.Common.LastTradeUpdateMessage.
        /// </summary>
        /// <param name="dataSource">The data source associated to this LastTradeUdpateMessage.</param>
        /// <param name="instrument">The instrument underlying this LastTradeUdpateMessage.</param>
        /// <param name="size">The size of the trade.</param>
        /// <param name="price">The price of the trade.</param>
        public LastTradeUpdateMessage(string dataSource, string instrument, int size, double price)
            : base(MarketDataMessageType.LastTrade, instrument, dataSource)
        {
            _price = price;
            _size = size;
        }

        /// <summary>
        /// Returns the string representation of this
        /// LastTradeUpdateMessage.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Timestamp {0} Instrument {1} Price {3} Quantity {4} Datasource {2}", 
               this.TimeStamp.ToString("HH:mm:ss.fff"), this._instrument, this._dataSource, this._price, this._size);
        }
    }
}
