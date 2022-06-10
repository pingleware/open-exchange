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

using OPEX.Messaging;
using OPEX.Common;
using OPEX.Configuration.Client;

namespace OPEX.MDS.Common
{
    /// <summary>
    /// Specifies the type of a MarketDataMessage.
    /// </summary>
    public enum MarketDataMessageType : int
    { 
        /// <summary>
        /// The MarketDataMessage carries information
        /// about the last version of a specific orderbook.
        /// This type of message is broadcast by
        /// MarketDataService.
        /// </summary>
        SnapshotUpdate = 0,

        /// <summary>
        /// The MarketDataMessage carries information
        /// about the last trade.
        /// </summary>
        LastTrade,

        /// <summary>
        /// The MarketDataMessage is either a request
        /// or a response message regarding the latest
        /// version of a specific orderbook.
        /// </summary>
        Snapshot,

        /// <summary>
        /// The MarketDataMessage is either a request
        /// or a response message regarding the status
        /// of the current trading session of an exchange.
        /// </summary>
        Status,

        /// <summary>
        /// Ping message (debug purposes only).
        /// </summary>
        Ping
    }

    /// <summary>
    /// Represents a basic MarketDataMessage, with its
    /// type, creation timestamp, data source and underlying
    /// instrument.
    /// This class is abstract.
    /// </summary>
    [Serializable]
    public abstract class MarketDataMessage : IChannelMessage
    {
        protected static readonly string RequestsOrigin;

        private readonly DateTime _timeStamp;

        protected readonly string _dataSource;
        protected readonly string _instrument;
        protected readonly MarketDataMessageType _type;

        protected string _origin;

        /// <summary>
        /// Gets the MarketDataMessageType of the MarketDataMessage.
        /// </summary>
        public MarketDataMessageType Type { get { return _type; } }

        /// <summary>
        /// Gets the creation timestamp of the MarketDataMessage.
        /// </summary>
        public DateTime TimeStamp { get { return _timeStamp; } }

        /// <summary>
        /// Gets the data source associated to this MarketDataMessage.
        /// </summary>
        public string DataSource { get { return _dataSource; } }

        /// <summary>
        /// Gets the underlying instrument of this MarketDataMessage.
        /// </summary>
        public string Instrument { get { return _instrument; } /* set { _instrument = value; } */ }

        static MarketDataMessage()
        {
            RequestsOrigin = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null);
        }

        /// <summary>
        /// Initialises a new instance of the class OPEX.MDS.Common.MarketDataMessage.
        /// </summary>
        /// <param name="instrument">The underlying instrument of this MarketDataMessage.</param>
        /// <param name="dataSource">The data source associated to this MarketDataMessage.</param>
        public MarketDataMessage(MarketDataMessageType type, string instrument, string dataSource)
            : this(type, instrument, dataSource, RequestsOrigin)
        { 
        }

        /// <summary>
        /// Initialises a new instance of the class OPEX.MDS.Common.MarketDataMessage.
        /// </summary>
        /// <param name="type">The MarketDataMessageType of the MarketDataMessage.</param>
        /// <param name="instrument">The underlying instrument of this MarketDataMessage.</param>
        /// <param name="dataSource">The data source associated to this MarketDataMessage.</param>
        /// <param name="mdRequestsOrigin">The origin of the market data requests associated to this MarketDataMessage.</param>
        public MarketDataMessage(MarketDataMessageType type, string instrument, string dataSource, string mdRequestsOrigin) 
        {
            _type = type;
            _timeStamp = DateTime.Now;
            _dataSource = dataSource;
            _origin = mdRequestsOrigin;
            _instrument = instrument;
        }

        #region IChannelMessage Members

        public virtual string Origin
        {
            get { return _origin; }
            set { _origin = value; }
        }        

        public virtual string Destination
        {
            get { return _dataSource; }
        }

        #endregion
    }
}